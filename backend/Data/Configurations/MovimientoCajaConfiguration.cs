using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations
{
    public class MovimientoCajaConfiguration : IEntityTypeConfiguration<MovimientoCaja>
    {
        public void Configure(EntityTypeBuilder<MovimientoCaja> builder)
        {
            builder.ToTable("MovimientosCaja");
            
            builder.HasKey(m => m.Id);

            builder.Property(m => m.Tipo)
                .IsRequired()
                .HasMaxLength(50);

            builder.Property(m => m.Monto)
                .HasColumnType("decimal(18,2)")
                .IsRequired();

            builder.Property(m => m.Concepto)
                .HasMaxLength(255);
                
            builder.Property(m => m.Fecha)
                .IsRequired();
                
            builder.HasOne(m => m.SesionCaja)
                .WithMany(s => s.Movimientos)
                .HasForeignKey(m => m.SesionCajaId)
                .OnDelete(DeleteBehavior.Cascade);
        }
    }
}
