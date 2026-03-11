using Xunit;
using FluentAssertions;
using Backend.Models;
using System;
using System.Linq;

namespace backend.Tests.Models
{
    public class ComprobanteModelTests
    {
        private readonly Producto _productoPrueba;

        public ComprobanteModelTests()
        {
            _productoPrueba = new Producto
            {
                Id = 1,
                Nombre = "Producto Test",
                Codigo = "TEST-01",
                Precio = 100m,
                Stock = 50
            };
        }

        [Fact]
        public void AgregarProducto_CalculaSubtotalCorrecto()
        {
            // Arrange
            var comprobante = new Comprobante(1, 1, 1, 1, 1);

            // Act
            comprobante.AgregarProducto(_productoPrueba, 2, 0m);

            // Assert
            comprobante.Detalles.Should().HaveCount(1);
            comprobante.Detalles.First().Subtotal.Should().Be(200m); // 2 * 100
            comprobante.Subtotal.Should().Be(200m);
            comprobante.Total.Should().Be(200m);
        }

        [Fact]
        public void AgregarProducto_ConPorcentajeFormaPago_AplicaRecargo()
        {
            // Arrange
            var comprobante = new Comprobante(1, 1, 1, 1, 1);

            // Act
            // Recargo del 10%
            comprobante.AgregarProducto(_productoPrueba, 2, 10m);

            // Assert
            var detalle = comprobante.Detalles.First();
            detalle.PrecioUnitario.Should().Be(110m); // 100 + 10%
            detalle.Subtotal.Should().Be(220m); // 2 * 110
            comprobante.Total.Should().Be(220m);
        }

        [Fact]
        public void AgregarProducto_MultiplesProductos_SumaTotalesCorrectamente()
        {
            // Arrange
            var comprobante = new Comprobante(1, 1, 1, 1, 1);
            var producto2 = new Producto { Id = 2, Codigo = "TEST-02", Nombre = "Prod 2", Precio = 50m };

            // Act
            comprobante.AgregarProducto(_productoPrueba, 1, 0m); // 1 * 100 = 100
            comprobante.AgregarProducto(producto2, 3, 0m); // 3 * 50 = 150

            // Assert
            comprobante.Detalles.Should().HaveCount(2);
            comprobante.Subtotal.Should().Be(250m);
            comprobante.Total.Should().Be(250m);
            comprobante.ImporteNetoGravado.Should().Be(250m);
        }

        [Fact]
        public void AgregarProducto_ProductoNulo_LanzaArgumentNullException()
        {
            // Arrange
            var comprobante = new Comprobante(1, 1, 1, 1, 1);

            // Act & Assert
            Action act = () => comprobante.AgregarProducto(null!, 1, 0m);
            act.Should().Throw<ArgumentNullException>();
        }

        [Fact]
        public void AgregarProducto_CantidadInvalida_LanzaArgumentException()
        {
            // Arrange
            var comprobante = new Comprobante(1, 1, 1, 1, 1);

            // Act & Assert
            Action actCero = () => comprobante.AgregarProducto(_productoPrueba, 0, 0m);
            actCero.Should().Throw<ArgumentException>();

            Action actNegativa = () => comprobante.AgregarProducto(_productoPrueba, -5, 0m);
            actNegativa.Should().Throw<ArgumentException>();
        }

        [Fact]
        public void ConfirmarCAE_AsignaValoresCorrectos()
        {
            // Arrange
            var comprobante = new Comprobante(1, 1, 1, 1, 1);
            var cae = "12345678901234";
            var vto = new DateTime(2025, 12, 31);
            var numero = 1500L;
            var ptoVenta = 3;

            // Act
            comprobante.ConfirmarCAE(cae, vto, numero, ptoVenta);

            // Assert
            comprobante.CAE.Should().Be(cae);
            comprobante.CAEVencimiento.Should().Be(vto);
            comprobante.NumeroComprobante.Should().Be(numero);
            comprobante.PuntoVenta.Should().Be(ptoVenta);
        }
    }
}
