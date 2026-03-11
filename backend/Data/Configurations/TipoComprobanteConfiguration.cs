using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations
{
    public class TipoComprobanteConfiguration : IEntityTypeConfiguration<TipoComprobante>
    {
        public void Configure(EntityTypeBuilder<TipoComprobante> builder)
        {
            builder.ToTable("TiposComprobantes");

            builder.HasKey(t => t.Id);

            builder.Property(t => t.Nombre)
                .IsRequired()
                .HasMaxLength(50);
        }
    }
}
