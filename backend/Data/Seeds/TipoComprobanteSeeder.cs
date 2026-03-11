using Backend.Models;

namespace Backend.Data.Seeds
{
    public class TipoComprobanteSeeder
    {
        public static void Seed(AppDbContext context)
        {
            if (!context.TiposComprobantes.Any())
            {
                var tipos = new List<TipoComprobante>
                {
                    new TipoComprobante { Nombre = "Factura A", CodigoAfip = 1 },
                    new TipoComprobante { Nombre = "Nota de Débito A", CodigoAfip = 2 },
                    new TipoComprobante { Nombre = "Nota de Crédito A", CodigoAfip = 3 },
                    new TipoComprobante { Nombre = "Recibo A", CodigoAfip = 4 },
                    new TipoComprobante { Nombre = "Nota de Venta al Contado A", CodigoAfip = 5 },
                    new TipoComprobante { Nombre = "Factura B", CodigoAfip = 6 },
                    new TipoComprobante { Nombre = "Nota de Débito B", CodigoAfip = 7 },
                    new TipoComprobante { Nombre = "Nota de Crédito B", CodigoAfip = 8 },
                    new TipoComprobante { Nombre = "Recibo B", CodigoAfip = 9 },
                    new TipoComprobante { Nombre = "Nota de Venta al Contado B", CodigoAfip = 10 },
                    new TipoComprobante { Nombre = "Factura C", CodigoAfip = 11 },
                    new TipoComprobante { Nombre = "Nota de Débito C", CodigoAfip = 12 },
                    new TipoComprobante { Nombre = "Nota de Crédito C", CodigoAfip = 13 },
                    new TipoComprobante { Nombre = "Recibo C", CodigoAfip = 15 }
                };

                context.TiposComprobantes.AddRange(tipos);
                context.SaveChanges();
            }
        }
    }
}
