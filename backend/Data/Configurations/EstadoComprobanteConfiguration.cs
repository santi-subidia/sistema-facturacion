using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations
{
    public class EstadoComprobanteConfiguration : IEntityTypeConfiguration<EstadoComprobante>
    {
        public void Configure(EntityTypeBuilder<EstadoComprobante> builder)
        {
            builder.ToTable("EstadosComprobantes");

            builder.HasKey(e => e.Id);

            builder.Property(e => e.Nombre)
                .IsRequired()
                .HasMaxLength(50);
                
            builder.Property(e => e.Descripcion)
                .HasMaxLength(255);
        }
    }
}
