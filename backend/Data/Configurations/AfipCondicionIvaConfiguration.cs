using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations
{
    public class AfipCondicionIvaConfiguration : IEntityTypeConfiguration<AfipCondicionIva>
    {
        public void Configure(EntityTypeBuilder<AfipCondicionIva> builder)
        {
            builder.HasKey(c => c.Id);
            
            builder.Property(c => c.Id)
                .ValueGeneratedNever(); // El Id es el código de AFIP, no es autoincremental
            
            builder.Property(c => c.Descripcion)
                .IsRequired()
                .HasMaxLength(100);
        }
    }
}
