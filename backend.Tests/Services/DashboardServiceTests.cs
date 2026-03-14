using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Data;
using Backend.Models;
using Backend.Services.Business;
using backend.Tests.Helpers;
using FluentAssertions;
using Xunit;

namespace backend.Tests.Services
{
    public class DashboardServiceTests
    {
        private async Task SeedDashboardData(AppDbContext db)
        {
            var hoy = DateTime.UtcNow;
            
            // Productos
            var p1 = new Producto { Nombre = "Prod 1 (Bajo Stock)", Stock = 3, Precio = 1500 }; // Stock bajo <= 5
            var p2 = new Producto { Nombre = "Prod 2 (Stock OK)", Stock = 10, Precio = 500 };
            var p3 = new Producto { Nombre = "Prod 3", Stock = 100, Precio = 1000 };
            var p4 = new Producto { Nombre = "Prod 4", Stock = 100, Precio = 9999 };
            db.Productos.AddRange(p1, p2, p3, p4);

            // Condicion Venta & Forma Pago
            var cv = new CondicionVenta { Descripcion = "Contado" };
            var fp = new FormaPago { Nombre = "Efectivo" };
            db.CondicionesVenta.Add(cv);
            db.FormasPago.Add(fp);
            
            var usuario = new Usuario { Nombre = "Admin" };
            db.Usuarios.Add(usuario);

            await db.SaveChangesAsync();

            // Comprobantes
            var c1 = new Comprobante(1, fp.Id, cv.Id, 3, usuario.Id) { Fecha = hoy, IdEstadoComprobante = 1 };
            c1.AgregarProducto(p1, 1, 0); // Total 1500

            var c2 = new Comprobante(1, fp.Id, cv.Id, 3, usuario.Id) { Fecha = hoy.AddDays(-2), IdEstadoComprobante = 1 };
            c2.AgregarProducto(p2, 1, 0); // Total 500

            var c3 = new Comprobante(1, fp.Id, cv.Id, 3, usuario.Id) { Fecha = hoy.AddMonths(-1), IdEstadoComprobante = 1 };
            c3.AgregarProducto(p3, 1, 0); // Total 1000

            var c4 = new Comprobante(1, fp.Id, cv.Id, 3, usuario.Id) { Fecha = hoy, IdEstadoComprobante = 3 };
            c4.AgregarProducto(p4, 1, 0); // Total 9999
            
            db.Comprobantes.AddRange(c1, c2, c3, c4);

            // Presupuestos
            var pre1 = new Presupuesto(fp.Id, cv.Id, usuario.Id) { Fecha = hoy, NumeroPresupuesto = 1 };
            // Estado por def Borrador = 1
            
            var pre2 = new Presupuesto(fp.Id, cv.Id, usuario.Id) { Fecha = hoy, NumeroPresupuesto = 2 };
            pre2.CambiarEstado(3); // Aprobado
            
            db.Presupuestos.AddRange(pre1, pre2);

            // Caja
            var caja = new Caja { Nombre = "Caja 1", PuntoVenta = 1 };
            db.Cajas.Add(caja);
            await db.SaveChangesAsync();

            db.SesionesCaja.Add(new SesionCaja 
            { 
                CajaId = caja.Id, 
                UsuarioId = usuario.Id,
                Estado = "Abierta", 
                MontoApertura = 1000,
                Movimientos = new List<MovimientoCaja>
                {
                    new MovimientoCaja { Tipo = "Ingreso", Monto = 500 },
                    new MovimientoCaja { Tipo = "Egreso", Monto = 200 }
                }
            });

            await db.SaveChangesAsync();
        }

        [Fact]
        public async Task ObtenerResumenAsync_SinDatos_RetornaValoresEnCero()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new DashboardService(db);

            var result = await service.ObtenerResumenAsync();

            result.Should().NotBeNull();
            result.VentasMes.Should().Be(0);
            result.VentasMesAnterior.Should().Be(0);
            result.ComprobantesEmitidos.Should().Be(0);
            result.PresupuestosPendientes.Should().Be(0);
            result.ProductosStockBajo.Should().Be(0);
            result.CajaSesionActual.Should().BeNull();
            result.Alertas.Should().BeEmpty();
            result.DistribucionCondicionVenta.Should().BeEmpty();
            result.ActividadReciente.Should().BeEmpty();
        }

        [Fact]
        public async Task ObtenerResumenAsync_ConComprobantes_CalculaVentasMes()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            await SeedDashboardData(db);
            var service = new DashboardService(db);

            var result = await service.ObtenerResumenAsync();

            // Ventas mes: 1500 + 500 = 2000
            result.VentasMes.Should().Be(2000);
            // Comprobantes emitidos este mes: solo los aprobados (2)
            result.ComprobantesEmitidos.Should().Be(2); 
            // Ventas mes anterior: 1000
            result.VentasMesAnterior.Should().Be(1000);
        }

        [Fact]
        public async Task ObtenerResumenAsync_ExcluyeRechazados_DeVentasMes()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            await SeedDashboardData(db);
            var service = new DashboardService(db);

            var result = await service.ObtenerResumenAsync();

            // El comprobante 4 (9999, estado 3 Rechazado) no se debe contar
            result.VentasMes.Should().NotBe(11999);
            result.VentasMes.Should().Be(2000);
        }

        [Fact]
        public async Task ObtenerResumenAsync_ProductosStockBajo_CuentaCorrectamente()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            await SeedDashboardData(db);
            var service = new DashboardService(db);

            var result = await service.ObtenerResumenAsync();

            // 1 producto con stock = 3 (<= 5)
            result.ProductosStockBajo.Should().Be(1);
        }

        [Fact]
        public async Task ObtenerResumenAsync_VentasPorMes_Retorna6Meses()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new DashboardService(db);

            var result = await service.ObtenerResumenAsync();

            result.VentasPorMes.Should().NotBeNull();
            result.VentasPorMes.Count().Should().Be(6); // Siempre 6 meses, aunque estén en 0
        }

        [Fact]
        public async Task ObtenerResumenAsync_ConCajaAbierta_RetornaDatosSesion()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            await SeedDashboardData(db);
            var service = new DashboardService(db);

            var result = await service.ObtenerResumenAsync();

            result.CajaSesionActual.Should().NotBeNull();
            result.CajaSesionActual!.MontoApertura.Should().Be(1000);
            
            // Saldo = 1000 (apertura) + 500 (ingreso) - 200 (egreso) = 1300
            result.CajaSesionActual.SaldoActual.Should().Be(1300);
            result.CajaSesionActual.MovimientosHoy.Should().Be(2);
        }

        [Fact]
        public async Task ObtenerResumenAsync_Alertas_GeneraWarnings()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            await SeedDashboardData(db);
            var service = new DashboardService(db);

            var result = await service.ObtenerResumenAsync();

            // Debe haber 1 alerta de stock y 1 de presupuestos (idPresupuesto = 1 estado Borrador)
            result.Alertas.Count().Should().Be(2);
            result.Alertas.Any(a => a.Tipo == "warning" && a.Mensaje.Contains("stock")).Should().BeTrue();
            result.Alertas.Any(a => a.Tipo == "info" && a.Mensaje.Contains("presupuestos")).Should().BeTrue();
        }
    }
}
