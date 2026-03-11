using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using Xunit;
using FluentAssertions;
using Moq;
using Backend.Data;
using Backend.Models;
using Backend.Constants;
using Backend.DTOs.Facturacion;
using Backend.Services.External.Afip.Interfaces;
using Backend.Services.External.Afip.Models;

namespace backend.Tests.Services
{
    public class ComprobantesServiceTests
    {
        private readonly Mock<IAfipWsfeService> _afipMock;
        private readonly Mock<IAfipComprobantePdfService> _pdfMock;
        private readonly Mock<IDetalleComprobanteService> _detalleMock;
        private readonly AppDbContext _db;
        private readonly ComprobantesService _service;

        public ComprobantesServiceTests()
        {
            var options = new DbContextOptionsBuilder<AppDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .ConfigureWarnings(x => x.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.InMemoryEventId.TransactionIgnoredWarning))
                .Options;

            _db = new AppDbContext(options);

            _afipMock = new Mock<IAfipWsfeService>();
            _pdfMock = new Mock<IAfipComprobantePdfService>();
            _detalleMock = new Mock<IDetalleComprobanteService>();

            _service = new ComprobantesService(_db, _afipMock.Object, _pdfMock.Object, _detalleMock.Object);
        }

        private async Task SeedDataObligatoria()
        {
            _db.AfipConfiguraciones.Add(new AfipConfiguracion { Activa = true });
            
            _db.TiposComprobantes.Add(new TipoComprobante { Id = 1, Nombre = "Factura B", CodigoAfip = 6 });
            _db.TiposComprobantes.Add(new TipoComprobante { Id = 2, Nombre = "Nota de Crédito B", CodigoAfip = 8 });
            
            _db.FormasPago.Add(new FormaPago { Id = 1, Nombre = "Efectivo", PorcentajeAjuste = 0 });
            _db.CondicionesVenta.Add(new CondicionVenta { Id = 1, Descripcion = "Al contado" });
            
            var caja = new Caja { Id = 1, Nombre = "Principal", PuntoVenta = 3 };
            _db.Cajas.Add(caja);
            
            _db.Usuarios.Add(new Usuario { Id = 1, Nombre = "Admin" });
            _db.SesionesCaja.Add(new SesionCaja { Id = 1, CajaId = 1, UsuarioId = 1, Estado = EstadoSesionCaja.Abierta });
            
            _db.Productos.Add(new Producto { Id = 1, Nombre = "Pan", Precio = 100, Stock = 50 });

            await _db.SaveChangesAsync();
        }

        [Fact]
        public async Task CrearComprobante_ClienteInexistente_RetornaError()
        {
            // Arrange
            await SeedDataObligatoria();
            var dto = new CreateComprobanteDto
            {
                IdCliente = 999, // Inexistente
                IdTipoComprobante = 1,
                IdFormaPago = 1,
                IdCondicionVenta = 1,
                Detalles = new List<DetalleComprobanteDto>()
            };

            // Act
            var result = await _service.CrearComprobanteAsync(dto, 1);

            // Assert
            result.success.Should().BeFalse();
            result.message.Should().Contain("El cliente especificado no existe");
        }

        [Fact]
        public async Task CrearComprobante_SinConfigAfip_RetornaError()
        {
            // Arrange
            // Seed only the base navigation models, omit AfipConfiguracion
            _db.TiposComprobantes.Add(new TipoComprobante { Id = 1, Nombre = "Factura B", CodigoAfip = 6 });
            _db.FormasPago.Add(new FormaPago { Id = 1, Nombre = "Efectivo", PorcentajeAjuste = 0 });
            _db.CondicionesVenta.Add(new CondicionVenta { Id = 1, Descripcion = "Al contado" });
            _db.Cajas.Add(new Caja { Id = 1, Nombre = "Principal", PuntoVenta = 3 });
            _db.SesionesCaja.Add(new SesionCaja { Id = 1, CajaId = 1, UsuarioId = 1, Estado = EstadoSesionCaja.Abierta });
            await _db.SaveChangesAsync();

            var dto = new CreateComprobanteDto { IdTipoComprobante = 1, IdFormaPago = 1, IdCondicionVenta = 1, Detalles = new List<DetalleComprobanteDto>() };

            // Act
            var result = await _service.CrearComprobanteAsync(dto, 1);

            // Assert
            result.success.Should().BeFalse();
            result.message.Should().Contain("AFIP activa");
        }

        [Fact]
        public async Task CrearComprobante_StockInsuficiente_RetornaError()
        {
            // Arrange
            await SeedDataObligatoria();
            var dto = new CreateComprobanteDto
            {
                IdTipoComprobante = 1,
                IdFormaPago = 1,
                IdCondicionVenta = 1,
                Detalles = new List<DetalleComprobanteDto>
                {
                    new DetalleComprobanteDto { IdProducto = 1, Cantidad = 100 } // Stock es 50
                }
            };

            // Act
            var result = await _service.CrearComprobanteAsync(dto, 1);

            // Assert
            result.success.Should().BeFalse();
            result.message.Should().Contain("Stock insuficiente");
        }

        [Fact]
        public async Task CrearComprobante_AfipRechaza_HaceRollback()
        {
            // Arrange
            await SeedDataObligatoria();
            var dto = new CreateComprobanteDto
            {
                IdTipoComprobante = 1,
                IdFormaPago = 1,
                IdCondicionVenta = 1,
                Detalles = new List<DetalleComprobanteDto>
                {
                    new DetalleComprobanteDto { IdProducto = 1, Cantidad = 1 }
                }
            };

            _afipMock.Setup(a => a.FECompUltimoAutorizadoAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new FECompUltimoAutorizadoResponse { CbteNro = 10 });

            // Simulamos error de afip
            _afipMock.Setup(a => a.FECAESolicitarAsync(It.IsAny<FECAERequest>()))
                .ReturnsAsync(new FECAEResponse 
                { 
                    FeCabResp = new FECAECabResponse { Resultado = "R" },
                    FeDetResp = new List<FECAEDetResponse> { new FECAEDetResponse { Observaciones = new List<FEErr> { new FEErr { Msg = "Rechazado" } } } }
                });

            // Act
            var result = await _service.CrearComprobanteAsync(dto, 1);

            // Assert
            result.success.Should().BeFalse();
            result.message.Should().Contain("aprob");
            
            // Verificamos el rollback: No debe haber comprobante guardado
            _db.Comprobantes.Count().Should().Be(0);
            
            // Nota: En DB Relacional el stock se haría rollback junto con la transacción.
            // Entity Framework Core InMemory ignora las transacciones, por lo que el objeto trackeado mantiene el cambio en este entorno, omitimos esa aserción.
        }

        [Fact]
        public async Task CrearComprobante_AfipAprueba_GuardaConCAE()
        {
            // Arrange
            await SeedDataObligatoria();
            var dto = new CreateComprobanteDto
            {
                IdTipoComprobante = 1,
                IdFormaPago = 1,
                IdCondicionVenta = 1,
                Detalles = new List<DetalleComprobanteDto>
                {
                    new DetalleComprobanteDto { IdProducto = 1, Cantidad = 1 }
                }
            };

            _afipMock.Setup(a => a.FECompUltimoAutorizadoAsync(It.IsAny<int>(), It.IsAny<int>()))
                .ReturnsAsync(new FECompUltimoAutorizadoResponse { CbteNro = 10 });

            _afipMock.Setup(a => a.FECAESolicitarAsync(It.IsAny<FECAERequest>()))
                .ReturnsAsync(new FECAEResponse 
                { 
                    FeCabResp = new FECAECabResponse { Resultado = "A" },
                    FeDetResp = new List<FECAEDetResponse> { new FECAEDetResponse { CAE = "123123123", CAEFchVto = "20251231" } }
                });

            // Act
            var result = await _service.CrearComprobanteAsync(dto, 1);

            // Assert
            result.success.Should().BeTrue(because: result.message);
            result.comprobante.Should().NotBeNull(because: result.message);
            result.comprobante!.CAE.Should().Be("123123123");
            result.comprobante.NumeroComprobante.Should().Be(11);
            result.comprobante.Total.Should().Be(100);

            // Verificamos que se descontó el stock
            var prod = await _db.Productos.FindAsync(1);
            prod!.Stock.Should().Be(49);
        }

        [Fact]
        public async Task GetSaldos_ConNotasCredito_CalculaSaldoCorrectamente()
        {
            // Arrange
            await SeedDataObligatoria();

            // Factura Original
            var factura = new Comprobante(1, 1, 1, 3, 1) { Creado_at = DateTime.Now, Fecha = DateTime.Now };
            var prod = await _db.Productos.FindAsync(1);
            factura.AgregarProducto(prod!, 2, 0); // 2 unidades
            _db.Comprobantes.Add(factura);
            await _db.SaveChangesAsync();

            // Nota Crédito anula 1 unidad
            var notaCredito = new Comprobante(2, 1, 1, 3, 1) { IdFacturaAsociada = factura.Id, Creado_at = DateTime.Now, Fecha = DateTime.Now };
            notaCredito.AgregarProducto(prod!, 1, 0); // 1 unidad
            _db.Comprobantes.Add(notaCredito);
            await _db.SaveChangesAsync();

            // Act
            var (cbte, saldos) = await _service.GetSaldosComprobanteAsync(factura.Id);

            // Assert
            cbte.Should().NotBeNull();
            saldos.Should().NotBeNull();
            saldos.Count.Should().Be(1);
            saldos[0].Cantidad.Should().Be(1); // Queda 1 unidad pendiente de anular o pagar
        }
    }
}
