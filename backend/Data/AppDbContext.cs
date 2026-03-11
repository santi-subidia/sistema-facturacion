using Microsoft.EntityFrameworkCore;
using Backend.Models;

namespace Backend.Data
{
    public class AppDbContext : DbContext
    {
        public DbSet<Producto> Productos { get; set; }
        public DbSet<Usuario> Usuarios { get; set; }
        public DbSet<Rol> Roles { get; set; }
        public DbSet<Cliente> Clientes { get; set; }
        public DbSet<TipoComprobante> TiposComprobantes { get; set; }
        public DbSet<Comprobante> Comprobantes { get; set; }
        public DbSet<DetalleComprobante> DetallesComprobante { get; set; }
        public DbSet<FormaPago> FormasPago { get; set; }
        public DbSet<CondicionVenta> CondicionesVenta { get; set; }
        public DbSet<EstadoComprobante> EstadosComprobantes { get; set; }
        
        // Presupuestos
        public DbSet<Presupuesto> Presupuestos { get; set; }
        public DbSet<DetallePresupuesto> DetallesPresupuesto { get; set; }
        public DbSet<PresupuestoEstado> PresupuestoEstados { get; set; }

        // Caja
        public DbSet<Caja> Cajas { get; set; }
        public DbSet<SesionCaja> SesionesCaja { get; set; }
        public DbSet<MovimientoCaja> MovimientosCaja { get; set; }
        
        // Auth
        public DbSet<RefreshToken> RefreshTokens { get; set; }
        
        // Email
        public DbSet<EmailQueue> EmailQueues { get; set; }
        
        // Parámetros de AFIP
        public DbSet<AfipTipoDocumento> AfipTiposDocumentos { get; set; }
        public DbSet<AfipCondicionIva> AfipCondicionesIva { get; set; }
        public DbSet<AfipTipoIva> AfipTiposIva { get; set; }
        
        // Configuración AFIP (catálogo)
        public DbSet<AfipConfiguracion> AfipConfiguraciones { get; set; }
        public DbSet<AfipTipoComprobanteHabilitado> AfipTiposComprobantesHabilitados { get; set; }
        public DbSet<AfipPuntoVenta> AfipPuntosVenta { get; set; }

        public AppDbContext(DbContextOptions<AppDbContext> options)
            : base(options)
        {
        }

        protected override void OnModelCreating(ModelBuilder modelBuilder)
        {
            base.OnModelCreating(modelBuilder);

            modelBuilder.Ignore<DocumentoComercial>();

            // Aplicar todas las configuraciones del ensamblado actual
            modelBuilder.ApplyConfigurationsFromAssembly(typeof(AppDbContext).Assembly);
        }

        public override int SaveChanges()
        {
            HashPasswords();
            return base.SaveChanges();
        }

        public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
        {
            HashPasswords();
            return await base.SaveChangesAsync(cancellationToken);
        }

        private void HashPasswords()
        {
            var usuariosEntries = ChangeTracker.Entries<Usuario>()
                .Where(e => e.State == EntityState.Added || e.State == EntityState.Modified);

            foreach (var entry in usuariosEntries)
            {
                var usuario = entry.Entity;
                
                if (entry.State == EntityState.Added)
                {
                    usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(usuario.PasswordHash);
                }
                else if (entry.State == EntityState.Modified)
                {
                    var originalPassword = entry.OriginalValues.GetValue<string>(nameof(Usuario.PasswordHash));
                    var currentPassword = usuario.PasswordHash;
                    
                    if (originalPassword != currentPassword && !currentPassword.StartsWith("$2"))
                    {
                        usuario.PasswordHash = BCrypt.Net.BCrypt.HashPassword(currentPassword);
                    }
                }
            }
        }
    }
}
