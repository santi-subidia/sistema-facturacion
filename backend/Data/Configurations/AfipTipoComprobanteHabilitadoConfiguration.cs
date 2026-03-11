using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations
{
    public class AfipTipoComprobanteHabilitadoConfiguration : IEntityTypeConfiguration<AfipTipoComprobanteHabilitado>
    {
        public void Configure(EntityTypeBuilder<AfipTipoComprobanteHabilitado> builder)
        {
            builder.HasKey(t => t.Id);
            
            builder.HasOne(t => t.AfipConfiguracion)
                .WithMany()
                .HasForeignKey(t => t.IdAfipConfiguracion)
                .OnDelete(DeleteBehavior.Cascade);
            
            builder.HasOne(t => t.TipoComprobante)
                .WithMany()
                .HasForeignKey(t => t.IdTipoComprobante)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasIndex(t => new { t.IdAfipConfiguracion, t.IdTipoComprobante })
                .IsUnique();
        }
    }
}
