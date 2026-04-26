using Backend.Data;
using Backend.Models;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using QuestPDF.Fluent;
using QuestPDF.Helpers;
using QuestPDF.Infrastructure;

namespace Backend.Services.Business
{
    public class PresupuestoPdfService : IPresupuestoPdfService
    {
        private readonly AppDbContext _db;
        private readonly ILogger<PresupuestoPdfService> _logger;

        public PresupuestoPdfService(AppDbContext db, ILogger<PresupuestoPdfService> logger)
        {
            _db = db;
            _logger = logger;
        }

        public async Task<byte[]> GenerarPdfPresupuestoAsync(int idPresupuesto)
        {
            var presupuesto = await _db.Presupuestos
                .Include(p => p.Cliente)
                .Include(p => p.FormaPago)
                .Include(p => p.CondicionVenta)
                .Include(p => p.Creado_por)
                .FirstOrDefaultAsync(p => p.Id == idPresupuesto);

            if (presupuesto == null)
                throw new Exception("Presupuesto no encontrado");

            var detalles = await _db.DetallesPresupuesto
                .Include(d => d.Producto)
                .Where(d => d.IdPresupuesto == idPresupuesto)
                .ToListAsync();

            var configAfip = await _db.AfipConfiguraciones
                .Include(c => c.AfipCondicionIva)
                .FirstOrDefaultAsync(c => c.Activa);

            if (configAfip == null)
                throw new Exception("No hay configuración AFIP activa");

            var document = Document.Create(container =>
            {
                container.Page(page =>
                {
                    page.Size(5.5f, 8.26f, Unit.Inch);
                    page.Margin(15);
                    
                    page.Header().Element(c => DibujarEncabezado(c, presupuesto, configAfip));
                    
                    page.Content().Column(col =>
                    {
                        col.Item().Element(c => DibujarDatosCliente(c, presupuesto));
                        col.Item().PaddingTop(5).Element(c => DibujarTablaProductos(c, detalles));
                        col.Item().PaddingTop(5).Element(c => DibujarTotales(c, presupuesto));
                    });

                    page.Footer().AlignRight().Text(text =>
                    {
                        text.DefaultTextStyle(x => x.FontSize(8));
                        text.Span("Pág. ");
                        text.CurrentPageNumber();
                        text.Span(" de ");
                        text.TotalPages();
                    });
                });
            });

            return document.GeneratePdf();
        }

        private void DibujarEncabezado(IContainer container, Presupuesto presupuesto, AfipConfiguracion config)
        {
            container.BorderBottom(1).BorderColor(Colors.Black)
                .Padding(8)
                .Row(row =>
                {
                    if (!string.IsNullOrEmpty(config.Logo_Url))
                    {
                        try
                        {
                            // Resolver el logo relativo a AppContext.BaseDirectory para portabilidad en Electron
                            // Logo_Url viene como "/images/Logos/Logo_1.png"
                            var pathParts = config.Logo_Url.Split('/', StringSplitOptions.RemoveEmptyEntries);
                            var fullLogoPath = Path.Combine(AppContext.BaseDirectory, Path.Combine(pathParts));
                            
                            if (File.Exists(fullLogoPath))
                            {
                                row.ConstantItem(60)
                                    .AlignMiddle()
                                    .Image(fullLogoPath)
                                    .FitArea();
                            }
                        }
                        catch (Exception ex)
                        {
                            _logger.LogWarning(ex, "No se pudo cargar el logo del presupuesto desde {LogoUrl}", config.Logo_Url);
                        }
                    }

                    // Información de la empresa
                    row.RelativeItem()
                        .PaddingLeft(10)
                        .Column(column =>
                        {
                            column.Item().Text(config.NombreFantasia ?? config.RazonSocial ?? "")
                                .Bold().FontSize(14);

                            column.Item().Text(text =>
                            {
                                text.Span("CUIT: ").Bold().FontSize(9);
                                text.Span(config.Cuit).FontSize(9);
                            });

                            if (!string.IsNullOrEmpty(config.DireccionFiscal))
                            {
                                column.Item().Text(text =>
                                {
                                    text.Span("Dirección: ").Bold().FontSize(8);
                                    text.Span(config.DireccionFiscal).FontSize(8);
                                });
                            }

                            if (!string.IsNullOrEmpty(config.EmailContacto))
                            {
                                column.Item().Text(text =>
                                {
                                    text.Span("Email: ").Bold().FontSize(8);
                                    text.Span(config.EmailContacto).FontSize(8);
                                });
                            }
                        });

                    // Información del presupuesto
                    row.RelativeItem()
                        .AlignRight()
                        .Column(column =>
                        {
                            column.Item().Text("PRESUPUESTO")
                                .Bold().FontSize(14);

                            column.Item().Text(text =>
                            {
                                text.Span("Nº: ").Bold().FontSize(10);
                                text.Span($"{presupuesto.NumeroPresupuesto:D6}").FontSize(10);
                            });

                            column.Item().Text(text =>
                            {
                                text.Span("Fecha: ").Bold().FontSize(9);
                                text.Span($"{presupuesto.Fecha:dd/MM/yyyy}").FontSize(9);
                            });

                            if (presupuesto.FechaVencimiento.HasValue)
                            {
                                column.Item().Text(text =>
                                {
                                    text.Span("Vence: ").Bold().FontSize(9);
                                    text.Span($"{presupuesto.FechaVencimiento:dd/MM/yyyy}").FontSize(9);
                                });
                            }
                        });
                });
        }

        private void DibujarDatosCliente(IContainer container, Presupuesto presupuesto)
        {
            container.BorderBottom(1).BorderColor(Colors.Grey.Lighten2)
                .Padding(8)
                .Column(column =>
                {
                    column.Item().Text("DATOS DEL CLIENTE")
                        .Bold().FontSize(10);

                    var nombreCompleto = !string.IsNullOrEmpty(presupuesto.ClienteApellido)
                        ? $"{presupuesto.ClienteApellido}, {presupuesto.ClienteNombre}"
                        : presupuesto.ClienteNombre ?? "Cliente sin nombre";

                    column.Item().PaddingTop(3).Row(r =>
                    {
                        r.RelativeItem().Text(text =>
                        {
                            text.Span("Nombre: ").Bold().FontSize(9);
                            text.Span(nombreCompleto).FontSize(9);
                        });

                        if (!string.IsNullOrEmpty(presupuesto.ClienteDocumento))
                        {
                            r.RelativeItem().Text(text =>
                            {
                                text.Span("Documento: ").Bold().FontSize(9);
                                text.Span(presupuesto.ClienteDocumento).FontSize(9);
                            });
                        }
                    });

                    if (!string.IsNullOrEmpty(presupuesto.ClienteDireccion))
                    {
                        column.Item().Text(text =>
                        {
                            text.Span("Dirección: ").Bold().FontSize(9);
                            text.Span(presupuesto.ClienteDireccion).FontSize(9);
                        });
                    }

                    column.Item().Row(r =>
                    {
                        if (!string.IsNullOrEmpty(presupuesto.ClienteTelefono))
                        {
                            r.RelativeItem().Text(text =>
                            {
                                text.Span("Teléfono: ").Bold().FontSize(9);
                                text.Span(presupuesto.ClienteTelefono).FontSize(9);
                            });
                        }

                        if (!string.IsNullOrEmpty(presupuesto.ClienteCorreo))
                        {
                            r.RelativeItem().Text(text =>
                            {
                                text.Span("Email: ").Bold().FontSize(9);
                                text.Span(presupuesto.ClienteCorreo).FontSize(9);
                            });
                        }
                    });

                    column.Item().PaddingTop(3).Row(r =>
                    {
                        r.RelativeItem().Text(text =>
                        {
                            text.Span("Forma de Pago: ").Bold().FontSize(9);
                            text.Span(presupuesto.FormaPago?.Nombre ?? "-").FontSize(9);
                        });

                        r.RelativeItem().Text(text =>
                        {
                            text.Span("Condición de Venta: ").Bold().FontSize(9);
                            text.Span(presupuesto.CondicionVenta?.Descripcion ?? "-").FontSize(9);
                        });
                    });
                });
        }

        private void DibujarTablaProductos(IContainer container, List<DetallePresupuesto> detalles)
        {
            container.Table(table =>
            {
                table.ColumnsDefinition(columns =>
                {
                    columns.RelativeColumn(1);      // Código
                    columns.RelativeColumn(3);      // Descripción
                    columns.RelativeColumn(1);      // Cantidad
                    columns.RelativeColumn(1.5f);   // Precio Unit.
                    columns.RelativeColumn(1.5f);   // Subtotal
                });

                table.Header(header =>
                {
                    header.Cell().Background("#e0e0e0").BorderBottom(1).BorderColor(Colors.Black).Padding(4)
                        .AlignCenter().Text("Código").Bold().FontSize(9);
                    header.Cell().Background("#e0e0e0").BorderBottom(1).BorderColor(Colors.Black).Padding(4)
                        .AlignCenter().Text("Descripción").Bold().FontSize(9);
                    header.Cell().Background("#e0e0e0").BorderBottom(1).BorderColor(Colors.Black).Padding(4)
                        .AlignCenter().Text("Cant.").Bold().FontSize(9);
                    header.Cell().Background("#e0e0e0").BorderBottom(1).BorderColor(Colors.Black).Padding(4)
                        .AlignCenter().Text("Precio Unit.").Bold().FontSize(9);
                    header.Cell().Background("#e0e0e0").BorderBottom(1).BorderColor(Colors.Black).Padding(4)
                        .AlignCenter().Text("Subtotal").Bold().FontSize(9);
                });

                foreach (var detalle in detalles)
                {
                    table.Cell().BorderBottom(1).BorderColor("#f0f0f0").Padding(4)
                        .Text(detalle.ProductoCodigo ?? "-").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor("#f0f0f0").Padding(4)
                        .Text(detalle.ProductoNombre ?? "-").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor("#f0f0f0").Padding(4)
                        .AlignCenter().Text(detalle.Cantidad.ToString()).FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor("#f0f0f0").Padding(4)
                        .AlignRight().Text($"${detalle.PrecioUnitario:N2}").FontSize(8);
                    table.Cell().BorderBottom(1).BorderColor("#f0f0f0").Padding(4)
                        .AlignRight().Text($"${detalle.Subtotal:N2}").FontSize(8);
                }
            });
        }

        private void DibujarTotales(IContainer container, Presupuesto presupuesto)
        {
            container.BorderTop(2).BorderColor(Colors.Black)
                .Padding(8)
                .AlignRight()
                .Column(column =>
                {
                    column.Item().Row(r =>
                    {
                        r.RelativeItem(70).AlignRight().Text("TOTAL: ").Bold().FontSize(12);
                        r.RelativeItem(30).AlignRight().Text($"${presupuesto.Total:N2}").Bold().FontSize(12);
                    });
                });
        }
    }
}
