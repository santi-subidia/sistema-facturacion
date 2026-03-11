# 🧾 FacturaPro — Contexto del Proyecto

## Visión General

**Sistema de facturación electrónica argentino** empaquetado como aplicación de escritorio.
Todo corre 100% en la PC del cliente — **sin servidores remotos ni costos de hosting**.
La única conexión externa es hacia los web services de AFIP para autorización de comprobantes fiscales.

---

## Arquitectura — "Server-on-Desktop"

```
┌─────────────────────────────────────────┐
│              Electron (app/)            │  ← Wrapper Desktop (ventana nativa)
│  ┌───────────────────────────────────┐  │
│  │   React SPA (frontend/)          │  │  ← UI servida desde localhost:5000
│  │   Vite • React 18 • Tailwind CDN │  │
│  └───────────┬───────────────────────┘  │
│              │ HTTP (fetch)             │
│  ┌───────────▼───────────────────────┐  │
│  │   .NET 9 API (backend/)          │  │  ← API REST local (self-contained)
│  │   ASP.NET Core • EF Core • SQLite│  │
│  └───────────┬───────────────────────┘  │
│              │                          │
│  ┌───────────▼───────────────────────┐  │
│  │   SQLite (facturacion.db)        │  │  ← BD embebida, archivo local
│  └───────────────────────────────────┘  │
└─────────────────────────────────────────┘
         │ HTTPS (solo para facturación)
         ▼
   AFIP Web Services
   (WSAA + WSFEV1)
```

### Flujo de Arranque

1. Electron abre una **splash screen** (`app/splash.html`)
2. Spawna el **backend .NET** como proceso hijo (puerto `5000`)
3. Espera el health check (`/health`) con hasta 40 reintentos
4. Crea la **ventana principal** apuntando a `http://localhost:5000`
5. El backend sirve el **frontend estático** desde `wwwroot/` y la **API REST**
6. Al cerrar, Electron mata el proceso backend con `taskkill /f /t`

### Implicaciones Clave (Sin Servidor)

- **Base de datos local**: SQLite vive en el directorio del ejecutable (`facturacion.db`)
- **Backups automáticos**: `DatabaseBackupService` crea copias cada 24h en `backups/`
- **JWT auto-generado**: Si la clave es la default, `Program.cs` genera una clave segura al primer arranque
- **Todas las rutas usan `AppContext.BaseDirectory`** para garantizar portabilidad (el CWD de Electron es impredecible)
- **Self-contained deployment**: El publish incluye el runtime .NET completo (`win-x64`)
- **No requiere permisos de Admin**: El instalador NSIS instala per-user

---

## Stack Tecnológico

| Capa | Tecnología | Versión |
|------|-----------|---------|
| Desktop | Electron | 26.x |
| Frontend | React + Vite | React 18.2 • Vite 5 |
| Estilos | Tailwind CSS (CDN) | — |
| Iconos | Lucide React | 0.577 |
| Gráficos | Recharts | 3.7 |
| Routing | React Router DOM | 7.13 |
| Fechas | date-fns | 4.1 |
| Backend | ASP.NET Core | .NET 9 |
| ORM | Entity Framework Core | 9.0 |
| BD | SQLite | embebida |
| Auth | JWT Bearer | 8.2 |
| Validación | FluentValidation | 11.3 |
| PDFs | QuestPDF (Community) | 2024.12 |
| QR | QRCoder | 1.6 |
| Logging | Serilog (Console + File) | 10.0 |
| Excel | ClosedXML | 0.105 |
| CSV | CsvHelper | 33.1 |
| Encriptación | ASP.NET DataProtection | — |
| Hashing | BCrypt.Net | 4.0 |
| Build Desktop | electron-builder (NSIS) | 24.x |

---

## Estructura del Proyecto

```
sistema_facturacion/
├── app/                          # Electron — wrapper desktop
│   ├── main.js                   #   Proceso principal (spawn backend, crear ventana)
│   ├── splash.html               #   Pantalla de carga animada
│   ├── electron-builder.yml      #   Config del instalador (NSIS, win-x64)
│   └── build/                    #   Recursos de build (iconos)
│
├── backend/                      # .NET 9 API REST
│   ├── Program.cs                #   Entry point — DI, middleware, startup
│   ├── Controllers/              #   17 controllers (REST endpoints)
│   ├── Services/
│   │   ├── Business/             #   19 servicios de negocio
│   │   ├── External/Afip/        #   Integración AFIP (WSAA, WSFEV1, PDF fiscal)
│   │   └── Interfaces/           #   16 interfaces de servicios
│   ├── Models/                   #   25 entidades del dominio
│   ├── DTOs/                     #   Data Transfer Objects por módulo
│   ├── Data/
│   │   ├── AppDbContext.cs       #   DbContext principal
│   │   ├── Configurations/       #   23 configuraciones Fluent API
│   │   ├── DataSeeder.cs         #   Seed inicial de datos
│   │   └── Seeds/                #   11 archivos de seed
│   ├── Validators/               #   16 validadores FluentValidation
│   ├── Filters/                  #   Filtros de acción (AfipConfiguracionFilter)
│   ├── Middleware/               #   SecurityHeaders + JsonError middleware
│   ├── Migrations/               #   53 migraciones EF Core
│   ├── Constants/                #   4 archivos de constantes
│   ├── appsettings.json          #   Config principal (BD, JWT, AFIP, Backups)
│   └── wwwroot/                  #   Frontend compilado (build de producción)
│
├── frontend/                     # React 18 + Vite 5 SPA
│   ├── src/
│   │   ├── App.jsx               #   Componente raíz
│   │   ├── main.jsx              #   Entry point React
│   │   ├── routes.jsx            #   Definición de rutas (code-splitting con lazy)
│   │   ├── config.js             #   URL base de la API
│   │   ├── index.css             #   Estilos globales
│   │   ├── context/              #   React Context (Auth)
│   │   ├── hooks/                #   15 custom hooks (lógica de negocio)
│   │   ├── components/           #   Componentes organizados por dominio
│   │   ├── utils/                #   Utilidades compartidas
│   │   └── pages/                #   Páginas especiales (NotFound)
│   │   │
│   │   ├── Dashboard.jsx         #   Panel principal con métricas
│   │   ├── Comprobantes.jsx      #   Crear comprobante fiscal (factura/NC/ND)
│   │   ├── ComprobantesLista.jsx #   Listado de comprobantes
│   │   ├── NotaCredito.jsx       #   Emisión de notas de crédito
│   │   ├── Presupuestos.jsx      #   Crear presupuesto
│   │   ├── PresupuestosLista.jsx #   Listado de presupuestos
│   │   ├── Clientes.jsx          #   ABM de clientes
│   │   ├── Productos.jsx         #   ABM de productos (con importación CSV/Excel)
│   │   ├── Usuarios.jsx          #   ABM de usuarios (solo admin)
│   │   ├── AfipConfiguracion.jsx #   Config AFIP (certificados, CUIT, etc.)
│   │   ├── CajaSesion.jsx        #   Sesión de caja activa
│   │   ├── CajaAdministrar.jsx   #   Administrar cajas (solo admin)
│   │   ├── CajaHistorial.jsx     #   Historial de sesiones de caja
│   │   └── Perfil.jsx            #   Perfil del usuario
│   └── vite.config.js
│
├── backend.Tests/                # Tests unitarios (xUnit)
├── sistema_facturacion.sln       # Solution .NET
└── README.md
```

---

## Módulos Funcionales

### 1. Autenticación y Autorización
- **JWT Bearer** con access token + refresh token
- Auto-generación de clave JWT al primer arranque
- Rate limiting en login (5 intentos/minuto)
- Roles: `Admin` y `Usuario`
- Rutas protegidas (`ProtectedRoute`) con chequeo de `requireAdmin` y `requireAfipConfig`
- `TokenCleanupBackgroundService` limpia tokens expirados

### 2. Facturación Electrónica (AFIP)
- Integración completa con **WSAA** (autenticación) y **WSFEV1** (facturación)
- Tipos de comprobante: Factura A/B/C, Nota de Crédito A/B/C, Nota de Débito A/B/C
- Certificados digitales almacenados localmente (`.p12`)
- Cache de tokens AFIP (`afip_token_cache.json`)
- Rate limiting configurable para requests a AFIP
- Generación de **PDFs fiscales** con QuestPDF (con código QR)
- Entornos: Homologación (testing) y Producción
- Wizard de **onboarding** para primera configuración AFIP

### 3. Presupuestos
- CRUD completo con detalles (líneas de productos)
- Estados: Pendiente → Aprobado → Facturado / Rechazado / Vencido
- Conversión de presupuesto a comprobante fiscal
- Generación de PDF de presupuesto
- Envío por email

### 4. Productos
- CRUD con soft-delete
- Código de producto, descripción, precio, unidad de medida, alícuota IVA
- Importación masiva desde **CSV** y **Excel** (ClosedXML)
- Paginación y búsqueda

### 5. Clientes
- CRUD con datos fiscales (CUIT/CUIL/DNI, condición IVA)
- Tipos de documento AFIP
- Asociación automática al emitir comprobantes

### 6. Caja
- Sesiones de caja (apertura/cierre con monto inicial)
- Movimientos: ingresos, egresos, ventas
- Formas de pago parametrizables
- Historial de sesiones con totales

### 7. Usuarios
- ABM de usuarios (solo admin)
- Cambio de contraseña (perfil propio)
- Hashing con BCrypt
- Gestión de roles

### 8. Dashboard
- Métricas de ventas
- Gráficos con Recharts

### 9. Backups Automáticos
- `DatabaseBackupService`: backup del `.db` cada 24h
- Retención configurable (default: 30 días)
- Endpoint para descarga/restauración manual (`BackupController`)

### 10. Email
- `EmailService` para envío de comprobantes/presupuestos por correo
- `EmailBackgroundService`: cola de envío en background
- Modelo `EmailQueue` para persistir pendientes

---

## Rutas del Frontend

| Ruta | Componente | Acceso |
|------|-----------|--------|
| `/login` | Login | Público |
| `/onboarding` | AfipOnboarding | Autenticado (sin AFIP requerido) |
| `/` | Dashboard | Autenticado |
| `/comprobantes` | Comprobantes | Autenticado |
| `/comprobantes/lista` | ComprobantesLista | Autenticado |
| `/comprobantes/nota-credito` | NotaCredito | Autenticado |
| `/presupuestos` | Presupuestos | Autenticado |
| `/presupuestos/lista` | PresupuestosLista | Autenticado |
| `/caja` | CajaSesion | Autenticado |
| `/caja/administrar` | CajaAdministrar | **Solo Admin** |
| `/caja/historial` | CajaHistorial | Autenticado |
| `/clientes` | Clientes | Autenticado |
| `/productos` | Productos | Autenticado |
| `/usuarios` | Usuarios | **Solo Admin** |
| `/afip-config` | AfipConfiguracion | **Solo Admin** |
| `/perfil` | Perfil | Autenticado |

---

## API REST — Endpoints Principales

| Controlador | Base Path | Descripción |
|------------|-----------|-------------|
| AuthController | `/api/auth` | Login, refresh token |
| ComprobantesController | `/api/comprobantes` | CRUD + emitir contra AFIP |
| PresupuestoController | `/api/presupuestos` | CRUD + PDF + envío email |
| ProductosController | `/api/productos` | CRUD + importación CSV/Excel |
| ClienteController | `/api/clientes` | CRUD clientes |
| UsuarioController | `/api/usuarios` | ABM usuarios |
| CajaController | `/api/caja` | Sesiones + movimientos |
| AfipConfiguracionController | `/api/afip-configuracion` | Config AFIP + certificados |
| DashboardController | `/api/dashboard` | Métricas y estadísticas |
| BackupController | `/api/backup` | Backup/restore de BD |
| FormaPagoController | `/api/formas-pago` | Formas de pago |
| CondicionVentaController | `/api/condicion-venta` | Condiciones de venta |
| TipoComprobanteController | `/api/tipo-comprobante` | Tipos de comprobante |
| RolController | `/api/roles` | Roles del sistema |
| HealthController | `/health` | Health check |
| VersionController | `/api/version` | Info de versión |

---

## Modelo de Datos (Entidades Principales)

```
Usuario ──┬── Rol
           └── RefreshToken

Cliente ──── Comprobante ──┬── DetalleComprobante ──── Producto
                           ├── FormaPago
                           └── TipoComprobante

Cliente ──── Presupuesto ──┬── DetallePresupuesto ──── Producto
                           └── CondicionVenta

SesionCaja ──── MovimientoCaja ──── Caja

AfipConfiguracion ──┬── AfipPuntoVenta
                    ├── AfipTipoComprobanteHabilitado
                    └── AfipCondicionIva / AfipTipoDocumento / AfipTipoIva

EmailQueue          # Cola de emails pendientes
DocumentoComercial  # Clase base/compartida
```

---

## Configuración (`appsettings.json`)

```jsonc
{
  "ConnectionStrings": {
    "DefaultConnection": "Data Source=facturacion.db"  // SQLite local
  },
  "Jwt": {
    "Key": "...",           // Auto-generada al primer arranque
    "Issuer": "sistema-facturacion",
    "Audience": "sistema-facturacion-client"
  },
  "Afip": {
    "WsaaUrl": "https://wsaahomo.afip.gov.ar/...",  // Homologación o Producción
    "WsfeUrl": "https://wswhomo.afip.gov.ar/...",
    "MaxRequestsPerWindow": 5,
    "WindowSeconds": 60
  },
  "BackupConfig": {
    "Enabled": true,
    "IntervalHours": 24,
    "RetentionDays": 30,
    "BackupFolder": "backups"
  }
}
```

---

## Comandos de Desarrollo

### Backend
```bash
cd backend
dotnet restore              # Restaurar dependencias
dotnet ef database update   # Aplicar migraciones
dotnet run                  # Correr en http://localhost:5000
dotnet test                 # Correr tests (desde raíz: dotnet test)
```

### Frontend
```bash
cd frontend
npm install
npm run dev                 # Dev server en http://localhost:5173
npm run build               # Build de producción → dist/
```

### Electron (Desktop)
```bash
cd app
npm install
npm start                   # Ejecutar app desktop (dev)
npm run dist                # Generar instalador (.exe con NSIS)
npm run build:full          # Build completo: frontend + backend + instalador
```

### Build de Producción Completo
```bash
cd app
npm run build:full
# Esto hace:
# 1. cd ../frontend && npm run build        (genera dist/)
# 2. dotnet publish backend (win-x64, self-contained) → resources/backend/
# 3. electron-builder genera el .exe instalador en app/dist/
```

---

## Convenciones del Proyecto

### Backend (.NET)
- **Patrón Repository/Service**: Controllers delegan a Services via interfaces (`I*Service`)
- **DTOs**: Nunca exponer modelos directamente — siempre usar DTOs por módulo
- **FluentValidation**: Validadores separados en `/Validators/`
- **Soft delete**: Entidades con campo `Activo` (borrado lógico)
- **Migraciones**: Nunca editar migraciones existentes, crear nuevas
- **Logging**: Usar Serilog inyectado (`ILogger<T>`)
- **Rutas absolutas**: Siempre usar `AppContext.BaseDirectory` (portabilidad Electron)

### Frontend (React)
- **Custom Hooks**: Toda la lógica de negocio vive en `hooks/` (separación de concerns)
- **Code Splitting**: Todos los componentes de página con `lazy()` import
- **Componentes por dominio**: Organizados en carpetas dentro de `components/`
- **Sin state management global**: Solo React Context para Auth
- **API calls**: Fetch directo usando `config.js` para la URL base
- **Tailwind CDN**: Estilos con utilidades de Tailwind (cargado via CDN en `index.html`)

### General
- **Idioma del código**: Español para nombres de negocio, inglés para patrones técnicos
- **Versionado semántico**: `Backend.csproj` → `<Version>1.0.0</Version>`
- **Git**: `.gitignore` excluye `node_modules/`, `bin/`, `obj/`, `dist/`, archivos `.db`

---

## Seguridad

- **SecurityHeadersMiddleware**: Headers de seguridad HTTP (CSP, X-Frame-Options, etc.)
- **CORS**: Restringido a orígenes locales (localhost:5173, :5000, file://)
- **AllowedHosts**: Solo `localhost`
- **Rate Limiting**: Login limitado a 5 intentos por minuto
- **Encriptación**: Passwords de certificados AFIP encriptados con ASP.NET DataProtection
- **BCrypt**: Hashing de contraseñas de usuario
- **JWT**: Tokens con validación de issuer, audience, lifetime y signing key
- **Sandbox**: Electron con `contextIsolation: true`, `sandbox: true`, sin `nodeIntegration`

---

## Background Services

| Servicio | Función | Frecuencia |
|----------|---------|------------|
| `DatabaseBackupService` | Backup automático del `.db` | Cada 24h |
| `EmailBackgroundService` | Procesa cola de emails pendientes | Continuo |
| `TokenCleanupBackgroundService` | Limpia refresh tokens expirados | Periódico |

---

## Consideraciones Importantes

> **⚠️ Desktop-first**: No hay servidor remoto. Todo cambio debe funcionar offline excepto AFIP.

> **⚠️ SQLite**: No soporta queries concurrentes pesadas. Ya configurado con WAL mode y busy_timeout.

> **⚠️ Portabilidad**: Usar siempre `AppContext.BaseDirectory` para paths, nunca paths relativos al CWD.

> **⚠️ Publish**: El backend se publica como self-contained (`win-x64`), NO se puede usar trimming porque rompe la reflection de EF Core.

> **⚠️ AFIP Homologación**: Actualmente apunta a entorno de testing (wsaahomo/wswhomo). Cambiar en `appsettings.json` para producción.
