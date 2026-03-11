using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations
{
    public class PresupuestoEstadoConfiguration : IEntityTypeConfiguration<PresupuestoEstado>
    {
        public void Configure(EntityTypeBuilder<PresupuestoEstado> builder)
        {
            builder.ToTable("PresupuestoEstados");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(e => e.Descripcion)
                .HasMaxLength(255);
        }
    }
}
