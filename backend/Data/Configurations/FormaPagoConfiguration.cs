using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations
{
    public class FormaPagoConfiguration : IEntityTypeConfiguration<FormaPago>
    {
        public void Configure(EntityTypeBuilder<FormaPago> builder)
        {
            builder.HasQueryFilter(f => f.Eliminado_at == null);

            builder.HasOne(f => f.Creado_por)
                .WithMany()
                .HasForeignKey(f => f.IdCreado_por)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.Eliminado_por)
                .WithMany()
                .HasForeignKey(f => f.IdEliminado_por)
                .OnDelete(DeleteBehavior.Restrict);
        }
    }
}
