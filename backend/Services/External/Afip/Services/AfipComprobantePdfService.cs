using Backend.Data;
using Backend.Models;
using Microsoft.EntityFrameworkCore;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;
using System.Text;
using System.Text.Json;
using QRCoder;
using Backend.Services.External.Afip.Interfaces;

namespace Backend.Services.External.Afip.Services
{
    public class AfipComprobantePdfService : IAfipComprobantePdfService
    {
        private readonly AppDbContext _db;

        public AfipComprobantePdfService(AppDbContext db)
        {
            _db = db;
        }

        public async Task<byte[]> GenerarPdfComprobanteAsync(int idComprobante)
        {
            var comprobante = await _db.Comprobantes
                .Include(f => f.Cliente)
                .Include(f => f.Cliente!.AfipCondicionIva)
                .Include(f => f.TipoComprobante)
                .Include(f => f.FormaPago)
                .Include(f => f.Creado_por)
                .Include(f => f.CondicionVenta)
                .Include(f => f.AfipTipoDocumento)
                .Include(f => f.FacturaAsociada)
                .FirstOrDefaultAsync(f => f.Id == idComprobante);

            if (comprobante == null)
                throw new Exception("Comprobante no encontrado");

            var detalles = await _db.DetallesComprobante
                .Include(d => d.Producto)
                .Where(d => d.IdComprobante == idComprobante)
                .ToListAsync();

            var configAfip = await _db.AfipConfiguraciones
                .Include(c => c.AfipCondicionIva)
                .FirstOrDefaultAsync(c => c.Activa);

            if (configAfip == null)
                throw new Exception("No hay configuración AFIP activa");

            var qrData = GenerarDatosQR(comprobante, configAfip);
            var qrBytes = GenerarCodigoQR(qrData);

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(PageSizes.A4);
                    page.Margin(20);
                    page.Header().Element(c => DibujarEncabezado(c, comprobante, configAfip));
                    
                    page.Content().Column(col => 
                    {
                        col.Item().Element(c => DibujarContenido(c, comprobante, detalles, configAfip));
                        
                        col.Item().EnsureSpace(100).Element(c => DibujarCierreComprobante(c, comprobante, qrBytes));
                    });

                    page.Footer().Element(c => DibujarNumeracion(c));
                });
            });

            return document.GeneratePdf();
        }

        private void DibujarEncabezado(IContainer container, Comprobante comprobante, AfipConfiguracion config)
        {
            container.BorderBottom(1).BorderColor(Colors.Black)
                .Row(row =>
            {
                row.RelativeItem(1)
                    .PaddingTop(10).PaddingLeft(10).PaddingBottom(10).PaddingRight(60)
                    .Column(column =>
                {
                    column.Item().AlignCenter().Text(config.NombreFantasia ?? config.RazonSocial ?? " - ")
                        .Bold().FontSize(20);
                    
                    column.Item().PaddingTop(10).Text(text =>
                    {
                        text.Span("Razón social: ").Bold().FontSize(10);
                        text.Span(config.RazonSocial).FontSize(10);
                    });
                    
                    column.Item().Text(text =>
                    {
                        text.Span("Domicilio Comercial: ").Bold().FontSize(10);
                        text.Span(config.DireccionFiscal ?? "").FontSize(10);
                    });
                    
                    column.Item().Text(text =>
                    {
                        text.Span("Condición Frente al IVA: ").Bold().FontSize(10);
                        text.Span(config.AfipCondicionIva?.Descripcion ?? "").FontSize(10);
                    });
                });

                row.ConstantItem(60).AlignCenter().AlignMiddle().Column(column =>
                {
                    var letra = ObtenerLetraComprobante(comprobante.TipoComprobante?.CodigoAfip ?? 0);
                    
                    column.Item().Border(1).BorderColor(Colors.Black)
                        .Width(60).Height(50)
                        .AlignCenter().AlignMiddle()
                        .Text(letra).Bold().FontSize(40);
                    
                    column.Item().PaddingTop(5).AlignCenter()
                        .Text($"COD. {comprobante.TipoComprobante?.CodigoAfip:D3}")
                        .FontSize(8);
                });

                row.RelativeItem(1)
                    .PaddingTop(10).PaddingLeft(60).PaddingRight(10).PaddingBottom(10)
                    .Column(column =>
                {
                    string titulo = comprobante.TipoComprobante?.Nombre?.ToUpper() ?? "COMPROBANTE";
                    if (titulo.Length > 2 && titulo[^2] == ' ' && char.IsLetter(titulo[^1]))
                    {
                        titulo = titulo[..^2].TrimEnd();
                    }

                    column.Item().Text(titulo)
                        .FontSize(20).Bold();
                    
                    column.Item().PaddingTop(5).Row(r =>
                    {
                        r.RelativeItem().Text(text =>
                        {
                            text.Span("Punto de Venta: ").Bold().FontSize(10);
                            text.Span($"{comprobante.PuntoVenta:D4}").FontSize(10);
                        });
                        
                        r.RelativeItem().Text(text =>
                        {
                            text.Span("Comp. Nro: ").Bold().FontSize(10);
                            text.Span($"{comprobante.NumeroComprobante:D8}").FontSize(10);
                        });
                    });
                    
                    column.Item().Text(text =>
                    {
                        text.Span("Fecha de Emisión: ").Bold().FontSize(10);
                        text.Span($"{comprobante.Fecha:dd/MM/yyyy}").FontSize(10);
                    });
                    
                    column.Item().Text(text =>
                    {
                        text.Span("CUIT: ").Bold().FontSize(10);
                        text.Span(config.Cuit).FontSize(10);
                    });
                    
                    if (!string.IsNullOrEmpty(config.IngresosBrutosNumero))
                    {
                        column.Item().Text(text =>
                        {
                            text.Span("Ingresos Brutos: ").Bold().FontSize(10);
                            text.Span(config.IngresosBrutosNumero).FontSize(10);
                        });
                    }
                    
                    column.Item().Text(text =>
                    {
                        text.Span("Fecha de Inicio de Actividades: ").Bold().FontSize(10);
                        text.Span($"{config.InicioActividades:dd/MM/yyyy}").FontSize(10);
                    });
                });
            });
        }

        private void DibujarContenido(IContainer container, Comprobante comprobante, List<DetalleComprobante> detalles, AfipConfiguracion config)
        {
            container.Column(column =>
            {
                column.Item().PaddingTop(5)
                    .BorderBottom(1).BorderColor(Colors.Black)
                    .Padding(10)
                    .Column(clienteCol =>
                    {
                        var nombreCompleto = !string.IsNullOrEmpty(comprobante.ClienteApellido)
                            ? $"{comprobante.ClienteApellido}, {comprobante.ClienteNombre}"
                            : comprobante.ClienteNombre ?? "";

                        clienteCol.Item().Row(r =>
                        {
                            r.RelativeItem(33).Text(text =>
                            {
                                text.Span("CUIL/CUIT: ").Bold().FontSize(10);
                                text.Span(comprobante.ClienteDocumento ?? " - ").FontSize(10);
                            });
                            r.RelativeItem(67).Text(text =>
                            {
                                text.Span("Apellido y Nombre / Razón social: ").Bold().FontSize(10);
                                text.Span(nombreCompleto ?? " - ").FontSize(10);
                            });
                        });

                        clienteCol.Item().PaddingTop(5).Row(r =>
                        {
                            r.RelativeItem().Text(text =>
                            {
                                text.Span("Condición Frente al IVA: ").Bold().FontSize(10);
                                text.Span(comprobante.Cliente?.AfipCondicionIva?.Descripcion ?? " - ").FontSize(10);
                            });
                            r.RelativeItem().Text(text =>
                            {
                                text.Span("Domicilio: ").Bold().FontSize(10);
                                text.Span(comprobante.ClienteDireccion ?? " - ").FontSize(10);
                            });
                        });

                        clienteCol.Item().PaddingTop(5).Text(text =>
                        {
                            text.Span("Condicion de venta: ").Bold().FontSize(10);
                            text.Span(comprobante?.CondicionVenta?.Descripcion ?? " - ").FontSize(10);
                        });

                        if (comprobante?.FacturaAsociada != null)
                        {
                            clienteCol.Item().PaddingTop(10).Text(text =>
                            {
                                text.Span("Comprobantes Asociados: ").Bold().FontSize(10);
                                text.Span($"Factura Nro: {comprobante.FacturaAsociada.PuntoVenta:D4}-{comprobante.FacturaAsociada.NumeroComprobante:D8} - Fecha: {comprobante.FacturaAsociada.Fecha:dd/MM/yyyy}").FontSize(10);
                            });
                            clienteCol.Item().PaddingTop(2).LineHorizontal(1).LineColor(Colors.Grey.Lighten2);
                        }
                    });

                column.Item().PaddingTop(5).BorderBottom(1).BorderColor(Colors.Black)
                    .Table(table =>
                {
                    table.ColumnsDefinition(columns =>
                    {
                        columns.RelativeColumn(1.5f);   // Código
                        columns.RelativeColumn(3);      // Descripción
                        columns.RelativeColumn(1);      // Cantidad
                        columns.RelativeColumn(1.2f);   // U. Medida
                        columns.RelativeColumn(1.5f);   // Precio Unit.
                        columns.RelativeColumn(1);      // % Bonif
                        columns.RelativeColumn(1.5f);   // Imp. Bonif
                        columns.RelativeColumn(1.5f);   // Subtotal
                    });

                    table.Header(header =>
                    {
                        header.Cell().Background("#c0c0c0").BorderBottom(1).BorderColor(Colors.Black).Padding(5)
                            .AlignCenter().Text("Código").Bold().FontSize(10);
                        header.Cell().Background("#c0c0c0").BorderBottom(1).BorderColor(Colors.Black).Padding(5)
                            .AlignCenter().Text("Producto / Servicio").Bold().FontSize(10);
                        header.Cell().Background("#c0c0c0").BorderBottom(1).BorderColor(Colors.Black).Padding(5)
                            .AlignCenter().Text("Cantidad").Bold().FontSize(10);
                        header.Cell().Background("#c0c0c0").BorderBottom(1).BorderColor(Colors.Black).Padding(5)
                            .AlignCenter().Text("U. Medida").Bold().FontSize(10);
                        header.Cell().Background("#c0c0c0").BorderBottom(1).BorderColor(Colors.Black).Padding(5)
                            .AlignCenter().Text("Precio Unit.").Bold().FontSize(10);
                        header.Cell().Background("#c0c0c0").BorderBottom(1).BorderColor(Colors.Black).Padding(5)
                            .AlignCenter().Text("% Bonif.").Bold().FontSize(10);
                        header.Cell().Background("#c0c0c0").BorderBottom(1).BorderColor(Colors.Black).Padding(5)
                            .AlignCenter().Text("Imp. Bonif.").Bold().FontSize(10);
                        header.Cell().Background("#c0c0c0").BorderBottom(1).BorderColor(Colors.Black).Padding(5)
                            .AlignCenter().Text("Subtotal").Bold().FontSize(10);
                    });

                    foreach (var detalle in detalles)
                    {
                        table.Cell().BorderTop(1).BorderColor("#c0c0c0").Padding(5)
                            .Text(detalle.ProductoCodigo ?? "-").FontSize(10);
                        table.Cell().BorderTop(1).BorderColor("#c0c0c0").Padding(5)
                            .Text(detalle.ProductoNombre ?? "-").FontSize(10);
                        table.Cell().BorderTop(1).BorderColor("#c0c0c0").Padding(5)
                            .Text(detalle.Cantidad.ToString()).FontSize(10);
                        table.Cell().BorderTop(1).BorderColor("#c0c0c0").Padding(5)
                            .Text("Unidad").FontSize(10);
                        table.Cell().BorderTop(1).BorderColor("#c0c0c0").Padding(5)
                            .Text($"{detalle.PrecioUnitario:N2}").FontSize(10);
                        table.Cell().BorderTop(1).BorderColor("#c0c0c0").Padding(5)
                            .Text("0,00").FontSize(10);
                        table.Cell().BorderTop(1).BorderColor("#c0c0c0").Padding(5)
                            .Text("0,00").FontSize(10);
                        table.Cell().BorderTop(1).BorderColor("#c0c0c0").Padding(5)
                            .Text($"{detalle.Subtotal:N2}").FontSize(10);
                    }
                });
            });
        }

        private void DibujarCierreComprobante(IContainer container, Comprobante comprobante, byte[] qrBytes)
        {
            container.ShowEntire().Column(col => 
            {
                col.Item().PaddingTop(10)
                    .BorderTop(1).BorderBottom(1).BorderColor(Colors.Black)
                    .Padding(10).AlignRight().Column(totalesCol =>
                {
                    totalesCol.Item().Row(r =>
                    {
                        r.RelativeItem(83).AlignRight().Text("Subtotal: $").Bold().FontSize(10);
                        r.RelativeItem(17).AlignRight().Text($"{comprobante.Subtotal:N2}").Bold().FontSize(10);
                    });

                    totalesCol.Item().Row(r =>
                    {
                        r.RelativeItem(83).AlignRight().Text("Importe Otros Tributos: $").Bold().FontSize(10);
                        r.RelativeItem(17).AlignRight().Text("0,00").Bold().FontSize(10);
                    });

                    totalesCol.Item().Row(r =>
                    {
                        r.RelativeItem(83).AlignRight().Text("Importe total: $").Bold().FontSize(10);
                        r.RelativeItem(17).AlignRight().Text($"{comprobante.Total:N2}").Bold().FontSize(10);
                    });
                });

                col.Item().PaddingTop(10)
                    .BorderBottom(1).BorderColor(Colors.Black)
                    .Row(row =>
                {
                    row.RelativeItem()
                        .Padding(10).Column(qrCol =>
                    {
                        qrCol.Item().Width(120).Height(120).Image(qrBytes);
                    });

                    row.RelativeItem().Padding(10).AlignRight()
                        .Column(caeCol =>
                    {
                        caeCol.Item().PaddingBottom(10).Text(text =>
                        {
                            text.Span("CAE Nº: ").Bold().FontSize(10);
                            text.Span(comprobante.CAE ?? "").FontSize(10);
                        });

                        caeCol.Item().Text(text =>
                        {
                            text.Span("Fecha de Vto. de CAE: ").Bold().FontSize(10);
                            text.Span($"{comprobante.CAEVencimiento:dd/MM/yyyy}").FontSize(10);
                        });
                    });
                });
            });
        }
        
        private void DibujarNumeracion(IContainer container)
        {
            container.PaddingTop(10).AlignRight().Text(x =>
            {
                x.Span("Página ");
                x.CurrentPageNumber();
                x.Span(" de ");
                x.TotalPages();
            });
        }

        private string ObtenerLetraComprobante(int codigoAfip)
        {
            return codigoAfip switch
            {
                1 or 2 or 3 => "A",
                6 or 7 or 8 => "B",
                11 or 12 or 13 or 15 => "C",
                201 or 202 or 203 => "C",
                _ => "X"
            };
        }

        private string GenerarDatosQR(Comprobante comprobante, AfipConfiguracion config)
        {
            var qrData = new
            {
                ver = 1,
                fecha = comprobante.Fecha.ToString("yyyy-MM-dd"),
                cuit = long.TryParse(config.Cuit.Replace("-", ""), out long cuitVal) ? cuitVal : 0,
                ptoVta = comprobante.PuntoVenta,
                tipoCmp = comprobante.TipoComprobante?.CodigoAfip ?? 0,
                nroCmp = comprobante.NumeroComprobante,
                importe = comprobante.Total,
                moneda = comprobante.CodigoMoneda ?? "PES",
                ctz = comprobante.CotizacionMoneda,
                tipoDocRec = comprobante.IdAfipTipoDocumento,
                nroDocRec = ObtenerNumeroDocumentoLimpio(comprobante.ClienteDocumento),
                tipoCodAut = "E",
                codAut = long.Parse(comprobante.CAE ?? "0")
            };

            var json = JsonSerializer.Serialize(qrData);
            var base64 = Convert.ToBase64String(Encoding.UTF8.GetBytes(json));
            
            return $"https://www.afip.gob.ar/fe/qr/?p={base64}";
        }

        private long ObtenerNumeroDocumentoLimpio(string? documento)
        {
            if (string.IsNullOrEmpty(documento))
                return 0;

            var limpio = new string(documento.Where(char.IsDigit).ToArray());
            return long.TryParse(limpio, out var numero) ? numero : 0;
        }

        private byte[] GenerarCodigoQR(string data)
        {
            using var qrGenerator = new QRCodeGenerator();
            using var qrCodeData = qrGenerator.CreateQrCode(data, QRCodeGenerator.ECCLevel.Q);
            using var qrCode = new PngByteQRCode(qrCodeData);
            return qrCode.GetGraphic(5);
        }
    }
}
