using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations
{
    public class AfipConfiguracionConfiguration : IEntityTypeConfiguration<AfipConfiguracion>
    {
        public void Configure(EntityTypeBuilder<AfipConfiguracion> builder)
        {
            builder.HasKey(c => c.Id);
            
            builder.Property(c => c.LimiteMontoConsumidorFinal)
                .HasPrecision(18, 2);
            
            builder.HasOne(c => c.AfipCondicionIva)
                .WithMany()
                .HasForeignKey(c => c.IdAfipCondicionIva)
                .OnDelete(DeleteBehavior.Restrict);
            
            builder.HasIndex(c => c.Cuit)
                .IsUnique();
        }
    }
}
