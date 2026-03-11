using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations
{
    public class ProductoConfiguration : IEntityTypeConfiguration<Producto>
    {
        public void Configure(EntityTypeBuilder<Producto> builder)
        {
            builder.HasQueryFilter(p => p.Eliminado_at == null);

            builder.Ignore(p => p.StockTotal);

            builder.Property(p => p.Stock).HasPrecision(18, 2);
            builder.Property(p => p.StockNegro).HasPrecision(18, 2);

            builder.HasOne(p => p.Creado_por)
                .WithMany()
                .HasForeignKey(p => p.IdCreado_por)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(p => p.Eliminado_por)
                .WithMany()
                .HasForeignKey(p => p.IdEliminado_por)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
