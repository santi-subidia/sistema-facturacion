using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations
{
    public class DetalleComprobanteConfiguration : IEntityTypeConfiguration<DetalleComprobante>
    {
        public void Configure(EntityTypeBuilder<DetalleComprobante> builder)
        {
            builder.Property(d => d.Cantidad).UsePropertyAccessMode(PropertyAccessMode.Property);
            builder.Property(d => d.Precio).UsePropertyAccessMode(PropertyAccessMode.Property);

            builder.Property(d => d.Precio).HasPrecision(18, 2);
            builder.Property(d => d.Cantidad).HasPrecision(18, 2);

            builder.Property(d => d.ProductoNombre).HasMaxLength(100).IsRequired();
            builder.Property(d => d.ProductoCodigo).HasMaxLength(50);

            builder.HasOne(d => d.Comprobante)
                .WithMany(c => c.Detalles)
                .HasForeignKey(d => d.IdComprobante)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(d => d.Producto)
                .WithMany()
                .HasForeignKey(d => d.IdProducto)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}