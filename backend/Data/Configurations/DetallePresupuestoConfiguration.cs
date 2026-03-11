using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations
{
    public class DetallePresupuestoConfiguration : IEntityTypeConfiguration<DetallePresupuesto>
    {
        public void Configure(EntityTypeBuilder<DetallePresupuesto> builder)
        {
            builder.Ignore(d => d.Subtotal);
            builder.Ignore(d => d.PrecioUnitario);

            builder.HasQueryFilter(dp => dp.Presupuesto!.Eliminado_at == null);

            // Configurar la relación con Presupuesto usando IdPresupuesto como FK
            builder.HasOne(dp => dp.Presupuesto)
                .WithMany(p => p.Detalles)
                .HasForeignKey(dp => dp.IdPresupuesto)
                .IsRequired()
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasOne(dp => dp.Producto)
                .WithMany()
                .HasForeignKey(dp => dp.IdProducto)
                .IsRequired(false) 
                .OnDelete(DeleteBehavior.Restrict); 

            builder.Property(d => d.IdPresupuesto).UsePropertyAccessMode(PropertyAccessMode.Property);
            builder.Property(d => d.IdProducto).UsePropertyAccessMode(PropertyAccessMode.Property);
            builder.Property(d => d.ProductoNombre).UsePropertyAccessMode(PropertyAccessMode.Property);
            builder.Property(d => d.ProductoCodigo).UsePropertyAccessMode(PropertyAccessMode.Property);
            builder.Property(d => d.Cantidad).UsePropertyAccessMode(PropertyAccessMode.Property);
            builder.Property(d => d.Precio).UsePropertyAccessMode(PropertyAccessMode.Property);

            builder.Property(d => d.Precio).HasPrecision(18, 2);
            builder.Property(d => d.Cantidad).HasPrecision(18, 2);

            builder.Property(d => d.ProductoNombre).HasMaxLength(100).IsRequired();
            builder.Property(d => d.ProductoCodigo).HasMaxLength(50);
        }
    }
}
