using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations
{
    public class SesionCajaConfiguration : IEntityTypeConfiguration<SesionCaja>
    {
        public void Configure(EntityTypeBuilder<SesionCaja> builder)
        {
            builder.ToTable("SesionesCaja");
            
            builder.HasKey(s => s.Id);

            builder.Property(s => s.MontoApertura)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(s => s.MontoCierreReal)
                .HasColumnType("decimal(18,2)");

            builder.Property(s => s.MontoCierreSistema)
                .HasColumnType("decimal(18,2)");

            builder.Property(s => s.Estado)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(s => s.FechaApertura)
                .IsRequired();

            builder.HasOne(s => s.Usuario)
                .WithMany()
                .HasForeignKey(s => s.UsuarioId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasMany(s => s.Movimientos)
                .WithOne(m => m.SesionCaja)
                .HasForeignKey(m => m.SesionCajaId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.HasMany(s => s.Comprobantes)
                .WithOne(c => c.SesionCaja)
                .HasForeignKey(c => c.SesionCajaId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasMany(s => s.Presupuestos)
                .WithOne(p => p.SesionCaja)
                .HasForeignKey(p => p.SesionCajaId)
                .OnDelete(DeleteBehavior.SetNull);
        }
    }
}
