using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations
{
    public class CondicionVentaConfiguration : IEntityTypeConfiguration<CondicionVenta>
    {
        public void Configure(EntityTypeBuilder<CondicionVenta> builder)
        {
            builder.HasQueryFilter(c => c.Eliminado_at == null);

            builder.HasOne(c => c.Creado_por)
                .WithMany()
                .HasForeignKey(c => c.IdCreado_por)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Eliminado_por)
                .WithMany()
                .HasForeignKey(c => c.IdEliminado_por)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
