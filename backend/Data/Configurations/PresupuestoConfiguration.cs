using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Backend.Models;

namespace Backend.Data.Configurations
{
    public class PresupuestoConfiguration : IEntityTypeConfiguration<Presupuesto>
    {
        public void Configure(EntityTypeBuilder<Presupuesto> builder)
        {
            // Configurar la colección de detalles con el backing field
            builder.HasMany(p => p.Detalles)
                .WithOne(d => d.Presupuesto)
                .HasForeignKey(d => d.IdPresupuesto)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Metadata
                .FindNavigation(nameof(Presupuesto.Detalles))!
                .SetPropertyAccessMode(PropertyAccessMode.Field);

            // Relación con Cliente (opcional)
            builder.HasOne(p => p.Cliente)
                .WithMany()
                .HasForeignKey(p => p.IdCliente)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con FormaPago
            builder.HasOne(p => p.FormaPago)
                .WithMany()
                .HasForeignKey(p => p.IdFormaPago)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con CondicionVenta
            builder.HasOne(p => p.CondicionVenta)
                .WithMany()
                .HasForeignKey(p => p.IdCondicionVenta)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con Usuario que creó
            builder.HasOne(p => p.Creado_por)
                .WithMany()
                .HasForeignKey(p => p.IdCreado_por)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con Usuario que eliminó (opcional)
            builder.HasOne(p => p.Eliminado_por)
                .WithMany()
                .HasForeignKey(p => p.IdEliminado_por)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con Usuario que descontó stock (opcional)
            builder.HasOne(p => p.UsuarioDescontoStock)
                .WithMany()
                .HasForeignKey(p => p.IdUsuarioDescontoStock)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con Factura generada (opcional)
            builder.HasOne(p => p.ComprobanteGenerado)
                .WithMany()
                .HasForeignKey(p => p.IdComprobanteGenerado)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.Restrict);

            // Relación con SesionCaja (opcional)
            builder.HasOne(p => p.SesionCaja)
                .WithMany(s => s.Presupuestos)
                .HasForeignKey(p => p.SesionCajaId)
                .IsRequired(false)
                .OnDelete(DeleteBehavior.SetNull);

            // Configurar propiedades decimales
            builder.Property(p => p.Subtotal).HasPrecision(18, 2);
            builder.Property(p => p.PorcentajeAjuste).HasPrecision(5, 2);
            builder.Property(p => p.Total).HasPrecision(18, 2);

            // Configurar propiedades de texto
            builder.Property(p => p.ClienteDocumento).HasMaxLength(20);
            builder.Property(p => p.ClienteNombre).HasMaxLength(100);
            builder.Property(p => p.ClienteApellido).HasMaxLength(100);
            builder.Property(p => p.ClienteTelefono).HasMaxLength(20);
            builder.Property(p => p.ClienteCorreo).HasMaxLength(100);
            builder.Property(p => p.ClienteDireccion).HasMaxLength(200);

            // Relación con PresupuestoEstado
            builder.HasOne(p => p.PresupuestoEstado)
                .WithMany()
                .HasForeignKey(p => p.IdPresupuestoEstado)
                .IsRequired()
                .OnDelete(DeleteBehavior.Restrict);

            // Query filter para soft delete
            builder.HasQueryFilter(p => p.Eliminado_at == null);
        }
    }
}
