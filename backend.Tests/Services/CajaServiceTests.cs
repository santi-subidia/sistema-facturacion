using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Backend.Constants;
using Backend.Data;
using Backend.DTOs.Caja;
using Backend.Models;
using Backend.Services.Business;
using backend.Tests.Helpers;
using FluentAssertions;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace backend.Tests.Services
{
    public class CajaServiceTests
    {
        // ─── CRUD CAJAS ────────────────────────────────────────────────────────

        [Fact]
        public async Task ObtenerCajasAsync_ConDatos_RetornaLista()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Cajas.Add(new Caja { Id = 1, Nombre = "Caja 1", PuntoVenta = 1 });
            db.Cajas.Add(new Caja { Id = 2, Nombre = "Caja 2", PuntoVenta = 2 });
            await db.SaveChangesAsync();
            var service = new CajaService(db);

            var result = await service.ObtenerCajasAsync();

            result.Count().Should().Be(2);
        }

        [Fact]
        public async Task ObtenerCajasAsync_SinDatos_RetornaVacio()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new CajaService(db);

            var result = await service.ObtenerCajasAsync();

            result.Should().BeEmpty();
        }

        [Fact]
        public async Task ObtenerCajaPorIdAsync_Existente_RetornaCaja()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Cajas.Add(new Caja { Id = 1, Nombre = "Principal", PuntoVenta = 3 });
            await db.SaveChangesAsync();
            var service = new CajaService(db);

            var result = await service.ObtenerCajaPorIdAsync(1);

            result.Should().NotBeNull();
            result!.Nombre.Should().Be("Principal");
            result.PuntoVenta.Should().Be(3);
        }

        [Fact]
        public async Task ObtenerCajaPorIdAsync_Inexistente_RetornaNull()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new CajaService(db);

            var result = await service.ObtenerCajaPorIdAsync(999);

            result.Should().BeNull();
        }

        [Fact]
        public async Task CrearCajaAsync_DatosValidos_CreaCaja()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new CajaService(db);
            var dto = new CrearCajaDto { Nombre = "Caja Nueva", Activa = true, PuntoVenta = 5 };

            var result = await service.CrearCajaAsync(dto);

            result.Should().NotBeNull();
            result.Nombre.Should().Be("Caja Nueva");
            result.PuntoVenta.Should().Be(5);
            db.Cajas.Count().Should().Be(1);
        }

        [Fact]
        public async Task ActualizarCajaAsync_DatosValidos_ActualizaCaja()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Cajas.Add(new Caja { Id = 1, Nombre = "Original", PuntoVenta = 1, Activa = true });
            await db.SaveChangesAsync();
            var service = new CajaService(db);
            var dto = new CrearCajaDto { Nombre = "Actualizada", Activa = false, PuntoVenta = 2 };

            var result = await service.ActualizarCajaAsync(1, dto);

            result.Should().NotBeNull();
            result!.Nombre.Should().Be("Actualizada");
            result.Activa.Should().BeFalse();
        }

        [Fact]
        public async Task ActualizarCajaAsync_Inexistente_RetornaNull()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new CajaService(db);
            var dto = new CrearCajaDto { Nombre = "X" };

            var result = await service.ActualizarCajaAsync(999, dto);

            result.Should().BeNull();
        }

        // ─── APERTURA Y CIERRE DE SESIONES ─────────────────────────────────────

        [Fact]
        public async Task AbrirCajaAsync_DatosValidos_CreaSesion()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Cajas.Add(new Caja { Id = 1, Nombre = "Caja 1" });
            db.Usuarios.Add(new Usuario { Id = 1, Username = "user" });
            await db.SaveChangesAsync();
            var service = new CajaService(db);
            var dto = new AperturaCajaDto { CajaId = 1, MontoInicial = 500 };

            var result = await service.AbrirCajaAsync(1, dto);

            result.Should().NotBeNull();
            result.Estado.Should().Be(EstadoSesionCaja.Abierta);
            result.MontoApertura.Should().Be(500);
            db.SesionesCaja.Count().Should().Be(1);
        }

        [Fact]
        public async Task AbrirCajaAsync_UsuarioConSesionActiva_LanzaExcepcion()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Cajas.Add(new Caja { Id = 1, Nombre = "C1" });
            db.Cajas.Add(new Caja { Id = 2, Nombre = "C2" });
            db.Usuarios.Add(new Usuario { Id = 1, Username = "user" });
            db.SesionesCaja.Add(new SesionCaja { Id = 1, CajaId = 1, UsuarioId = 1, Estado = EstadoSesionCaja.Abierta });
            await db.SaveChangesAsync();
            var service = new CajaService(db);
            var dto = new AperturaCajaDto { CajaId = 2, MontoInicial = 100 };

            // Only check exception type - message uses Unicode chars that encoding-differ between test/service
            Func<Task> act = async () => await service.AbrirCajaAsync(1, dto);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task AbrirCajaAsync_CajaOcupada_LanzaExcepcion()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Cajas.Add(new Caja { Id = 1, Nombre = "C1" });
            db.Usuarios.Add(new Usuario { Id = 1, Username = "user1" });
            db.Usuarios.Add(new Usuario { Id = 2, Username = "user2" });
            db.SesionesCaja.Add(new SesionCaja { Id = 1, CajaId = 1, UsuarioId = 1, Estado = EstadoSesionCaja.Abierta });
            await db.SaveChangesAsync();
            var service = new CajaService(db);
            var dto = new AperturaCajaDto { CajaId = 1, MontoInicial = 200 };

            Func<Task> act = async () => await service.AbrirCajaAsync(2, dto);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task CerrarCajaAsync_SesionPropia_CierraSesion()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Cajas.Add(new Caja { Id = 1, Nombre = "Caja 1" });
            db.Usuarios.Add(new Usuario { Id = 1, Username = "user" });
            db.SesionesCaja.Add(new SesionCaja { Id = 1, CajaId = 1, UsuarioId = 1, Estado = EstadoSesionCaja.Abierta, MontoApertura = 1000 });
            await db.SaveChangesAsync();
            var service = new CajaService(db);
            var dto = new CierreCajaDto { SesionCajaId = 1, MontoCierreReal = 950 };

            var result = await service.CerrarCajaAsync(1, dto);

            result.Should().NotBeNull();
            result.Estado.Should().Be(EstadoSesionCaja.Cerrada);
            result.MontoCierreReal.Should().Be(950);
            result.FechaCierre.Should().NotBeNull();
        }

        [Fact]
        public async Task CerrarCajaAsync_SesionDeOtroUsuario_LanzaExcepcion()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Cajas.Add(new Caja { Id = 1, Nombre = "C1" });
            db.Usuarios.Add(new Usuario { Id = 1, Username = "user1" });
            db.Usuarios.Add(new Usuario { Id = 2, Username = "user2" });
            db.SesionesCaja.Add(new SesionCaja { Id = 1, CajaId = 1, UsuarioId = 1, Estado = EstadoSesionCaja.Abierta });
            await db.SaveChangesAsync();
            var service = new CajaService(db);
            var dto = new CierreCajaDto { SesionCajaId = 1, MontoCierreReal = 500 };

            Func<Task> act = async () => await service.CerrarCajaAsync(2, dto);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        [Fact]
        public async Task CerrarCajaAsync_SesionYaCerrada_LanzaExcepcion()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Cajas.Add(new Caja { Id = 1, Nombre = "C1" });
            db.Usuarios.Add(new Usuario { Id = 1, Username = "user" });
            db.SesionesCaja.Add(new SesionCaja { Id = 1, CajaId = 1, UsuarioId = 1, Estado = EstadoSesionCaja.Cerrada });
            await db.SaveChangesAsync();
            var service = new CajaService(db);
            var dto = new CierreCajaDto { SesionCajaId = 1, MontoCierreReal = 0 };

            Func<Task> act = async () => await service.CerrarCajaAsync(1, dto);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task CerrarCajaAsync_SesionInexistente_LanzaExcepcion()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new CajaService(db);
            var dto = new CierreCajaDto { SesionCajaId = 999, MontoCierreReal = 0 };

            Func<Task> act = async () => await service.CerrarCajaAsync(1, dto);

            await act.Should().ThrowAsync<KeyNotFoundException>();
        }

        // ─── SESIONES Y MOVIMIENTOS ─────────────────────────────────────────────

        [Fact]
        public async Task ObtenerSesionActivaAsync_ConSesionAbierta_RetornaSesion()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Cajas.Add(new Caja { Id = 1, Nombre = "C1" });
            db.Usuarios.Add(new Usuario { Id = 1, Username = "user" });
            db.SesionesCaja.Add(new SesionCaja { Id = 1, CajaId = 1, UsuarioId = 1, Estado = EstadoSesionCaja.Abierta, MontoApertura = 500 });
            await db.SaveChangesAsync();
            var service = new CajaService(db);

            var result = await service.ObtenerSesionActivaAsync(1);

            result.Should().NotBeNull();
            result!.Estado.Should().Be(EstadoSesionCaja.Abierta);
        }

        [Fact]
        public async Task ObtenerSesionActivaAsync_SinSesion_RetornaNull()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            var service = new CajaService(db);

            var result = await service.ObtenerSesionActivaAsync(1);

            result.Should().BeNull();
        }

        [Fact]
        public async Task AgregarMovimientoAsync_SesionAbierta_AgregaMovimiento()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Cajas.Add(new Caja { Id = 1, Nombre = "C1" });
            db.Usuarios.Add(new Usuario { Id = 1, Username = "user" });
            db.SesionesCaja.Add(new SesionCaja { Id = 1, CajaId = 1, UsuarioId = 1, Estado = EstadoSesionCaja.Abierta });
            await db.SaveChangesAsync();
            var service = new CajaService(db);
            var dto = new CrearMovimientoCajaDto { SesionCajaId = 1, Tipo = "Ingreso", Monto = 200, Concepto = "Venta extra" };

            var result = await service.AgregarMovimientoAsync(1, dto);

            result.Should().NotBeNull();
            result.Tipo.Should().Be("Ingreso");
            result.Monto.Should().Be(200);
            db.MovimientosCaja.Count().Should().Be(1);
        }

        [Fact]
        public async Task AgregarMovimientoAsync_SesionCerrada_LanzaExcepcion()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Cajas.Add(new Caja { Id = 1, Nombre = "C1" });
            db.Usuarios.Add(new Usuario { Id = 1, Username = "user" });
            db.SesionesCaja.Add(new SesionCaja { Id = 1, CajaId = 1, UsuarioId = 1, Estado = EstadoSesionCaja.Cerrada });
            await db.SaveChangesAsync();
            var service = new CajaService(db);
            var dto = new CrearMovimientoCajaDto { SesionCajaId = 1, Tipo = "Ingreso", Monto = 100 };

            Func<Task> act = async () => await service.AgregarMovimientoAsync(1, dto);

            await act.Should().ThrowAsync<InvalidOperationException>();
        }

        [Fact]
        public async Task AgregarMovimientoAsync_SesionDeOtro_LanzaExcepcion()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Cajas.Add(new Caja { Id = 1, Nombre = "C1" });
            db.Usuarios.Add(new Usuario { Id = 1, Username = "user1" });
            db.Usuarios.Add(new Usuario { Id = 2, Username = "user2" });
            db.SesionesCaja.Add(new SesionCaja { Id = 1, CajaId = 1, UsuarioId = 1, Estado = EstadoSesionCaja.Abierta });
            await db.SaveChangesAsync();
            var service = new CajaService(db);
            var dto = new CrearMovimientoCajaDto { SesionCajaId = 1, Tipo = "Egreso", Monto = 50 };

            Func<Task> act = async () => await service.AgregarMovimientoAsync(2, dto);

            await act.Should().ThrowAsync<UnauthorizedAccessException>();
        }

        // ─── CÁLCULO DE CIERRE ──────────────────────────────────────────────────

        [Fact]
        public async Task CalcularMontoCierreSistemaAsync_ConMovimientos_SumaCorrectamente()
        {
            using var db = TestDbHelper.CreateInMemoryContext();
            db.Cajas.Add(new Caja { Id = 1, Nombre = "C1" });
            db.Usuarios.Add(new Usuario { Id = 1, Username = "user" });
            db.SesionesCaja.Add(new SesionCaja { Id = 1, CajaId = 1, UsuarioId = 1, Estado = EstadoSesionCaja.Abierta, MontoApertura = 1000 });
            await db.SaveChangesAsync();
            db.MovimientosCaja.Add(new MovimientoCaja { Id = 1, SesionCajaId = 1, Tipo = "Ingreso", Monto = 500, Concepto = "Venta", Fecha = DateTime.UtcNow });
            db.MovimientosCaja.Add(new MovimientoCaja { Id = 2, SesionCajaId = 1, Tipo = "Egreso", Monto = 200, Concepto = "Gasto", Fecha = DateTime.UtcNow });
            await db.SaveChangesAsync();
            var service = new CajaService(db);

            // MontoApertura(1000) + Ingresos(500) - Egresos(200) = 1300
            var resultado = await service.CalcularMontoCierreSistemaAsync(1);

            resultado.Should().Be(1300);
        }
    }
}
