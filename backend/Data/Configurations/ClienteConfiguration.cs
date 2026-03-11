using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations
{
    public class ClienteConfiguration : IEntityTypeConfiguration<Cliente>
    {
        public void Configure(EntityTypeBuilder<Cliente> builder)
        {
            builder.HasQueryFilter(c => c.Eliminado_at == null);

            builder.HasOne(c => c.Creado_por)
                .WithMany()
                .HasForeignKey(c => c.IdCreado_por)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.Eliminado_por)
                .WithMany()
                .HasForeignKey(c => c.IdEliminado_por)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(c => c.AfipCondicionIva)
                .WithMany()
                .HasForeignKey(c => c.IdAfipCondicionIva)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
