# Carrito de Compras

Sistema de e-commerce full-stack desarrollado como trabajo práctico para la materia **Diseño de Software Flexible y Reusable**.

Incluye una API REST en .NET 8 y un frontend en React + TypeScript, integrados mediante JWT y desplegables de forma independiente.

---

## Stack tecnológico

| Capa | Tecnología |
|---|---|
| Backend | .NET 8, ASP.NET Core, Entity Framework Core 8 |
| Base de datos | PostgreSQL 14+ |
| Frontend | React 18, TypeScript, React Router v6, Bootstrap 5 |
| Autenticación | JWT |
| CI/CD | GitHub Actions (detección de cambios por paths) |

---

## Estructura del repositorio

```
├── backend-dotnet/          # API REST (.NET 8)
│   ├── src/
│   │   ├── CarritoComprasBackend/   # Proyecto principal — controladores, configuración
│   │   ├── BusinessRules/           # Reglas de precio (descuento, normal, por peso)
│   │   ├── DataRepository/          # Acceso a datos, modelos EF Core, migraciones
│   │   ├── Services/                # Capa de servicios, AutoMapper
│   │   └── Validations/             # Cadena de responsabilidad para validaciones
│   └── tests/                       # Proyectos de tests unitarios por capa
├── frontend/
│   └── react/carrito-compras-frontend/  # SPA en React + TypeScript
├── bd.sql                   # Backup de PostgreSQL (pg_dump)
└── .github/workflows/       # Pipelines de CI (detección automática de cambios)
```

---

## Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [Node.js 18+](https://nodejs.org/) y npm
- [PostgreSQL 14+](https://www.postgresql.org/download/)

---

## Inicio rápido

### Base de datos

```bash
# Crear la base de datos
psql -U postgres -c "CREATE DATABASE carrito_compras;"

# Restaurar el backup
pg_restore -U postgres -d carrito_compras bd.sql

# (Opcional) Cargar productos desde los CSVs
cd backend-dotnet/src/CarritoComprasBackend/Resources
cp .env.example .env   # completar con tus credenciales
bash seed.sh
```

### Backend

```bash
cd backend-dotnet

# Configurar la cadena de conexión
# Editar src/CarritoComprasBackend/appsettings.json → ConnectionStrings.WebApiDatabase

dotnet restore
dotnet run --project src/CarritoComprasBackend/ShoppingCartBackEnd.csproj
# API disponible en https://localhost:7xxx — Swagger en /swagger
```

### Frontend

```bash
cd frontend/react/carrito-compras-frontend
npm install
npm start
# App disponible en http://localhost:3000
```

---

## Testing

```bash
# Backend
dotnet test backend-dotnet/CarritoComprasBackend.sln

# Frontend
cd frontend/react/carrito-compras-frontend
npm test -- --watchAll=false
```

---

## Infraestructura en Azure

La infraestructura se gestiona con Terraform y se despliega automáticamente via GitHub Actions.

Para configurar el entorno por primera vez (remote state, secrets, primer apply) ver:
**[docs/terraform-bootstrap.md](docs/terraform-bootstrap.md)**

Para entender las decisiones de arquitectura (por qué App Service y no Functions, por qué no VNet en dev, etc.) ver:
**[docs/terraform-architecture.md](docs/terraform-architecture.md)**

---

## CI/CD

El pipeline en GitHub Actions detecta automáticamente qué parte del código cambió y ejecuta solo el workflow correspondiente:

- Cambios en `backend-dotnet/**` → lint, build y test del backend (.NET)
- Cambios en `frontend/react/**` → lint, build y test del frontend (React)

Ver [`.github/workflows/`](.github/workflows/) para la configuración completa.
