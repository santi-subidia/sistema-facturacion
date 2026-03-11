using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations
{
    public class AfipTipoIvaConfiguration : IEntityTypeConfiguration<AfipTipoIva>
    {
        public void Configure(EntityTypeBuilder<AfipTipoIva> builder)
        {
            builder.HasKey(t => t.Id);
            
            builder.Property(t => t.Id)
                .ValueGeneratedNever(); // El Id es el código de AFIP, no es autoincremental
            
            builder.Property(t => t.Descripcion)
                .IsRequired()
                .HasMaxLength(100);
            
            builder.Property(t => t.Porcentaje)
                .HasPrecision(5, 2);
        }
    }
}
