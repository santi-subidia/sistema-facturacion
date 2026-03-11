using Backend.Models;

namespace Backend.Data.Seeds
{
    public static class PresupuestoEstadoSeeder
    {
        public static async Task SeedAsync(AppDbContext context)
        {
            if (!context.PresupuestoEstados.Any())
            {
                context.PresupuestoEstados.AddRange(
                    new PresupuestoEstado { Id = 1, Nombre = "Borrador", Descripcion = "El presupuesto está en estado borrador." },
                    new PresupuestoEstado { Id = 2, Nombre = "Enviado", Descripcion = "El presupuesto fue enviado al cliente." },
                    new PresupuestoEstado { Id = 3, Nombre = "Aceptado", Descripcion = "El presupuesto fue aceptado por el cliente." },
                    new PresupuestoEstado { Id = 4, Nombre = "Rechazado", Descripcion = "El presupuesto fue rechazado por el cliente." },
                    new PresupuestoEstado { Id = 5, Nombre = "Venta en Negro", Descripcion = "El presupuesto fue vendido sin facturar." },
                    new PresupuestoEstado { Id = 6, Nombre = "Facturado", Descripcion = "El presupuesto fue facturado." },
                    new PresupuestoEstado { Id = 7, Nombre = "Vencido", Descripcion = "El presupuesto venció sin ser aceptado." },
                    new PresupuestoEstado { Id = 8, Nombre = "Cancelado", Descripcion = "El presupuesto fue cancelado." }
                );

                await context.SaveChangesAsync();
            }
        }
    }
}
