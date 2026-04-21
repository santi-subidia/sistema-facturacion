using Backend.Data;
using Backend.Services.External.Afip.Services;
using Backend.Services.External.Afip.Interfaces;
using Backend.Services.Business;
using Backend.Services.Interfaces;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using QuestPDF.Infrastructure;
using FluentValidation;
using FluentValidation.AspNetCore;
using System.Reflection;
using Microsoft.Extensions.FileProviders;
using Backend.Filters;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.Extensions.DependencyInjection;
using Serilog;
using Microsoft.AspNetCore.DataProtection;

// Configurar licencia de QuestPDF (Community para proyectos open source)
QuestPDF.Settings.License = LicenseType.Community;

var builder = WebApplication.CreateBuilder(args);

// Auto-generar JWT Key si tiene el valor por defecto
var appSettingsPath = Path.Combine(AppContext.BaseDirectory, "appsettings.json");
if (File.Exists(appSettingsPath))
{
    var appSettingsJson = File.ReadAllText(appSettingsPath);
    if (appSettingsJson.Contains("CAMBIAR-POR-TU-CLAVE-SECRETA-JWT-MINIMO-32-CARACTERES"))
    {
        var secureKeyBytes = new byte[64];
        using (var rng = System.Security.Cryptography.RandomNumberGenerator.Create())
        {
            rng.GetBytes(secureKeyBytes);
        }
        var newSecureKey = Convert.ToBase64String(secureKeyBytes);
        
        appSettingsJson = appSettingsJson.Replace(
            "CAMBIAR-POR-TU-CLAVE-SECRETA-JWT-MINIMO-32-CARACTERES", 
            newSecureKey);
            
        File.WriteAllText(appSettingsPath, appSettingsJson);
        
        // Recargar configuración en memoria
        builder.Configuration.AddJsonFile(appSettingsPath, optional: false, reloadOnChange: true);
    }
}

// Configurar Serilog
var logsPath = Path.Combine(AppContext.BaseDirectory, "logs", "log-.txt");
Log.Logger = new LoggerConfiguration()
    .ReadFrom.Configuration(builder.Configuration)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(logsPath, rollingInterval: RollingInterval.Day, retainedFileCountLimit: 30)
    .CreateLogger();

builder.Host.UseSerilog();

// Construir ruta absoluta del archivo .db basada en AppContext.BaseDirectory
// para garantizar portabilidad en Electron (el CWD puede ser distinto al dir del .exe)
var configuredConnStr = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Falta ConnectionStrings:DefaultConnection en appsettings.json");
// Si la connection string usa una ruta relativa como "Data Source=facturacion.db",
// la resolvemos contra AppContext.BaseDirectory
var sqliteConnectionString = System.Text.RegularExpressions.Regex.Replace(
    configuredConnStr,
    @"Data Source=([^;]+)",
    match =>
    {
        var dataSource = match.Groups[1].Value;
        var absolutePath = Path.IsPathRooted(dataSource)
            ? dataSource
            : Path.Combine(AppContext.BaseDirectory, dataSource);
        return $"Data Source={absolutePath}";
    },
    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlite(sqliteConnectionString));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

// Caché en memoria para idempotencia y datos estáticos
builder.Services.AddMemoryCache();
builder.Services.AddScoped<IdempotencyFilter>();

builder.Services.AddControllers(options => 
    {
        options.Filters.Add<AfipConfiguracionFilter>();
    })
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddHttpClient<AfipAuthService>(client => 
{
    client.Timeout = TimeSpan.FromSeconds(30);
});

builder.Services.AddHttpClient<AfipWsfeService>(client => 
{
    client.Timeout = TimeSpan.FromSeconds(60);
});
builder.Services.AddScoped<IAfipWsfeService, AfipWsfeService>();

builder.Services.AddScoped<IAfipComprobantePdfService, AfipComprobantePdfService>();
builder.Services.AddScoped<IComprobantesService, ComprobantesService>();
builder.Services.AddScoped<IDetalleComprobanteService, DetalleComprobanteService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IProductoService, ProductoService>();
builder.Services.AddScoped<IClienteService, ClienteService>();
builder.Services.AddScoped<IUsuarioService, UsuarioService>();
builder.Services.AddScoped<ICondicionVentaService, CondicionVentaService>();
builder.Services.AddScoped<IFormaPagoService, FormaPagoService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IAfipConfiguracionService, AfipConfiguracionService>();
builder.Services.AddScoped<IAfipParametrosService, AfipParametrosService>();
builder.Services.AddScoped<IAfipComprobantesHabilitadosService, AfipComprobantesHabilitadosService>();
builder.Services.AddScoped<IAfipAuthService, AfipAuthService>();
builder.Services.AddScoped<IRolService, RolService>();
builder.Services.AddScoped<IPresupuestoService, PresupuestoService>();
builder.Services.AddScoped<IPresupuestoPdfService, PresupuestoPdfService>();
builder.Services.AddScoped<ICajaService, CajaService>();
builder.Services.AddScoped<IDashboardService, DashboardService>();
builder.Services.AddHostedService<EmailBackgroundService>();
builder.Services.AddHostedService<DatabaseBackupService>();
builder.Services.AddHostedService<TokenCleanupBackgroundService>();
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Falta Jwt:Key en appsettings.json. Debe tener mínimo 32 caracteres.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "sistema-facturacion";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "sistema-facturacion-client";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

builder.Services.AddAuthorization();

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowFrontend", policy =>
    {
        policy.WithOrigins(
                "http://localhost:5173",  // Vite dev
                "http://localhost:5174",  // Vite dev fallback
                "http://localhost:5175",  // Vite dev fallback
                "http://localhost:3000",  // Alt dev
                "http://localhost:5000",  // Backend (Electron)
                "file://"                 // Electron local files
            )
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddFluentValidationAutoValidation();
builder.Services.AddValidatorsFromAssembly(Assembly.GetExecutingAssembly());

builder.Services.AddRateLimiter(options =>
{
    options.AddFixedWindowLimiter("login", opt =>
    {
        opt.PermitLimit = 5;
        opt.Window = TimeSpan.FromMinutes(1);
        opt.QueueLimit = 0;
    });
    options.RejectionStatusCode = 429;
});

// Data Protection y Encriptación
var keysDirectory = new DirectoryInfo(Path.Combine(AppContext.BaseDirectory, "keys"));
builder.Services.AddDataProtection()
    .PersistKeysToFileSystem(keysDirectory)
    .SetApplicationName("sistema-facturacion");
builder.Services.AddScoped<IEncryptionService, EncryptionService>();

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    var comprobantesHabilitadosService = scope.ServiceProvider.GetRequiredService<IAfipComprobantesHabilitadosService>();
    
    db.Database.Migrate();
    db.Database.ExecuteSqlRaw("PRAGMA journal_mode=WAL;");
    db.Database.ExecuteSqlRaw("PRAGMA busy_timeout = 5000;");
    var seederLogger = scope.ServiceProvider.GetRequiredService<ILoggerFactory>()
        .CreateLogger("DataSeeder");
    await DataSeeder.InitializeAsync(db, comprobantesHabilitadosService, seederLogger);
}

app.UseCors("AllowFrontend");

// Security Headers
app.UseMiddleware<Backend.Middleware.SecurityHeadersMiddleware>();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseDefaultFiles();
app.UseStaticFiles();

// Usar AppContext.BaseDirectory para garantizar portabilidad en Electron
var imagesPath = Path.Combine(AppContext.BaseDirectory, "Images");
if (!Directory.Exists(imagesPath))
{
    Directory.CreateDirectory(imagesPath);
}

app.UseStaticFiles(new StaticFileOptions
{
    FileProvider = new PhysicalFileProvider(imagesPath),
    RequestPath = "/images"
});

app.UseRateLimiter();
app.UseAuthentication();
app.UseAuthorization();
app.UseMiddleware<Backend.Middleware.JsonErrorMiddleware>();

app.MapHealthChecks("/health");
app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
