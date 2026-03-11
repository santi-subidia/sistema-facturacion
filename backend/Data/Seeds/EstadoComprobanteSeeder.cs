using Backend.Models;

namespace Backend.Data.Seeds
{
    public static class EstadoComprobanteSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (!context.EstadosComprobantes.Any())
            {
                context.EstadosComprobantes.AddRange(
                    new EstadoComprobante { Nombre = "Parcialmente Anulada", Descripcion = "La factura tiene una Nota de Crédito asociada por una cantidad parcial." },
                    new EstadoComprobante { Nombre = "Anulada", Descripcion = "La factura ha sido anulada en su totalidad con una Nota de Crédito." }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}
