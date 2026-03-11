using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations
{
    public class ComprobanteConfiguration : IEntityTypeConfiguration<Comprobante>
    {
        public void Configure(EntityTypeBuilder<Comprobante> builder)
        {
            builder.ToTable("Comprobantes");

            builder.HasKey(f => f.Id);

            builder.Property(f => f.Total).HasColumnType("decimal(18,2)");
            builder.Property(f => f.Subtotal).HasColumnType("decimal(18,2)");
            
            builder.Property(f => f.ImporteNetoGravado).HasPrecision(18, 2);
            builder.Property(f => f.ImporteIVA).HasPrecision(18, 2);
            builder.Property(f => f.ImporteExento).HasPrecision(18, 2);
            builder.Property(f => f.ImporteTributos).HasPrecision(18, 2);
            builder.Property(f => f.CotizacionMoneda).HasPrecision(18, 6);
            builder.Property(f => f.PorcentajeAjuste).HasPrecision(18, 2);

            builder.Property(f => f.Subtotal).UsePropertyAccessMode(PropertyAccessMode.Property);
            builder.Property(f => f.Total).UsePropertyAccessMode(PropertyAccessMode.Property);
            builder.Property(f => f.ImporteNetoGravado).UsePropertyAccessMode(PropertyAccessMode.Property);
            builder.Property(f => f.ImporteIVA).UsePropertyAccessMode(PropertyAccessMode.Property);
            builder.Property(f => f.ImporteExento).UsePropertyAccessMode(PropertyAccessMode.Property);
            builder.Property(f => f.ImporteTributos).UsePropertyAccessMode(PropertyAccessMode.Property);
            
            builder.Metadata.FindNavigation(nameof(Comprobante.Detalles))
                   !.SetPropertyAccessMode(PropertyAccessMode.Field);

            builder.HasMany(f => f.Detalles)
                   .WithOne(d => d.Comprobante)
                   .HasForeignKey(d => d.IdComprobante)
                   .OnDelete(DeleteBehavior.Cascade); 

            builder.HasQueryFilter(f => f.Eliminado_at == null);

            builder.HasOne(f => f.Creado_por)
                .WithMany()
                .HasForeignKey(f => f.IdCreado_por)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.Eliminado_por)
                .WithMany()
                .HasForeignKey(f => f.IdEliminado_por)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.Cliente)
                .WithMany()
                .HasForeignKey(f => f.IdCliente)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.TipoComprobante)
                .WithMany()
                .HasForeignKey(f => f.IdTipoComprobante)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.AfipPuntoVentaObj)
                .WithMany()
                .HasForeignKey(f => f.PuntoVenta)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.FormaPago)
                .WithMany()
                .HasForeignKey(f => f.IdFormaPago)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.CondicionVenta)
                .WithMany()
                .HasForeignKey(f => f.IdCondicionVenta)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.AfipTipoDocumento)
                .WithMany()
                .HasForeignKey(f => f.IdAfipTipoDocumento)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.FacturaAsociada)
                .WithMany()
                .HasForeignKey(f => f.IdFacturaAsociada)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.EstadoComprobante)
                .WithMany()
                .HasForeignKey(f => f.IdEstadoComprobante)
                .OnDelete(DeleteBehavior.Restrict);

            builder.HasOne(f => f.SesionCaja)
                .WithMany(s => s.Comprobantes)
                .HasForeignKey(f => f.SesionCajaId)
                .OnDelete(DeleteBehavior.SetNull);

            builder.HasIndex(f => new { f.PuntoVenta, f.NumeroComprobante, f.IdTipoComprobante })
                .IsUnique()
                .HasFilter("[NumeroComprobante] IS NOT NULL");

            builder.HasIndex(f => f.CAE)
                .HasFilter("[CAE] IS NOT NULL");
        }
    }
}