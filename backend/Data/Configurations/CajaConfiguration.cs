using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations
{
    public class CajaConfiguration : IEntityTypeConfiguration<Caja>
    {
        public void Configure(EntityTypeBuilder<Caja> builder)
        {
            builder.ToTable("Cajas");
            
            builder.HasKey(c => c.Id);
            
            builder.Property(c => c.Nombre)
                .IsRequired()
                .HasMaxLength(150);

            builder.Property(c => c.Activa)
                .IsRequired()
                .HasDefaultValue(true);

            builder.Property(c => c.PuntoVenta)
                .IsRequired();

            builder.HasOne(c => c.AfipPuntoVentaObj)
                .WithMany()
                .HasForeignKey(c => c.PuntoVenta)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(c => c.Sesiones)
                .WithOne(s => s.Caja)
                .HasForeignKey(s => s.CajaId)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
