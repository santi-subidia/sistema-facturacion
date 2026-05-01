# 🧾 FacturaPro - Sistema de Facturación

Sistema de facturación completo con arquitectura fullstack: API REST en .NET 9, interfaz moderna en React, y wrapper desktop.

---

## 📁 Estructura del Proyecto

```
sistema_facturacion/
├── backend/          # API REST con .NET 9 + Entity Framework + SQLite
├── frontend/         # Interfaz de usuario con React 18 + Vite + Tailwind CSS
├── app/              # Aplicación de escritorio con Electron
└── README.md         # Este archivo
```

---

## 🚀 Tecnologías

### Backend
- .NET 9
- Entity Framework Core 9 (Migrations)
- SQLite
- Autenticación JWT
- Integración AFIP (WSAA / WSFEv1)
- QuestPDF y QRCoder (Generación de Comprobantes PDF)
- ClosedXML y CsvHelper (Generación de Reportes Múltiples)

### Frontend
- React 18
- Vite 5
- Tailwind CSS 4
- React Router DOM
- Recharts (Gráficos Dashboard)
- Lucide React (Íconos)
- Tipografías: Outfit & Plus Jakarta Sans

### Desktop
- Electron

---

## ⚙️ Instalación y Configuración

### Requisitos previos
- [.NET 9 SDK](https://dotnet.microsoft.com/download/dotnet/9.0)
- [Node.js](https://nodejs.org/) (v18 o superior)
- Git

### 1️⃣ Clonar el repositorio
```bash
git clone <tu-repo-url>
cd sistema_facturacion
```

### 2️⃣ Backend
```bash
cd backend
dotnet restore
dotnet run                   # Corre en http://localhost:5000 y aplica EnsureCreated
```
*(Ver `backend/README.md` para más información sobre AFIP y configuración de `appsettings.json`)*

### 3️⃣ Frontend (Desarrollo)
```bash
cd frontend
npm install
npm run dev                  # Corre en http://localhost:5173
```

### 4️⃣ Frontend (Producción - para Electron)
```bash
cd frontend
npm install
npm run build
# Copiar el contenido generado en dist/ al wwwroot del backend
```

### 5️⃣ Electron Desktop
```bash
cd app
npm install
npm start                    # Inicia aplicación desktop
```

---

## 📝 Características Implementadas

### Módulo Principal y Facturación
- ✅ Autenticación segura con JWT (Login/Logout)
- ✅ Dashboard interactivo con Recharts (métricas, estados, métodos de pago)
- ✅ CRUD completo de Productos y stock
- ✅ CRUD de Clientes
- ✅ Módulo de Presupuestos (Creación, estados, conversión a factura)
- ✅ Emisión de Facturas y Notas de Crédito (A, B y C)
- ✅ Integración real con AFIP (Homologación y Producción)
- ✅ Generación de PDFs con Códigos QR (Formato AFIP)
- ✅ Paginación y Filtrado avanzado en todos los listados
- ✅ Soft delete (eliminación lógica) a lo largo del sistema

### Usuarios y Roles
- ✅ CRUD de Usuarios del sistema
- ✅ Sistema de roles (Administrador, Vendedor, etc.)
- ✅ Protección de Rutas

### Reportes
- ✅ Exportación de listados a Excel (ClosedXML)
- ✅ Exportación de listados a CSV (CsvHelper)

---

## 👤 Autor

**Subidia Hector Santiago**

---

## 📄 Licencia

Este proyecto es privado.
