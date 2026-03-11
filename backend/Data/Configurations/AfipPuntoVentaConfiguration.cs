using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations
{
    public class AfipPuntoVentaConfiguration : IEntityTypeConfiguration<AfipPuntoVenta>
    {
        public void Configure(EntityTypeBuilder<AfipPuntoVenta> builder)
        {
            builder.ToTable("AfipPuntosVenta");

            builder.HasKey(p => p.Numero);
            
            // Indicar explícitamente que el ID (Numero) no se autogenera
            builder.Property(p => p.Numero)
                .ValueGeneratedNever();

            builder.Property(p => p.EmisionTipo)
                .HasMaxLength(50);

            builder.Property(p => p.Bloqueado)
                .HasMaxLength(10);

            builder.Property(p => p.FechaBaja)
                .HasMaxLength(50);
        }
    }
}
