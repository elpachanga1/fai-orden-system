# Backend — Carrito Compras

API REST construida con **.NET 8** y **PostgreSQL**, organizada en múltiples proyectos dentro de esta carpeta.

---

## Versiones y tecnologías

| Componente | Versión |
|---|---|
| .NET SDK | **8.0** |
| Entity Framework Core | 8.0.2 |
| Npgsql EF PostgreSQL | 8.0.2 |
| Swashbuckle (Swagger) | 6.5.0 |
| Application Insights | 2.22.0 |
| PostgreSQL (servidor) | 14 o superior recomendado |

---

## Estructura de proyectos

```
backend-dotnet/
├── CarritoComprasBackend/   # Proyecto principal (API, controladores, configuración)
├── BusinessRules/           # Reglas de negocio y cálculo de precios
├── DataRepository/          # Acceso a datos, modelos EF Core y migraciones
├── Services/                # Capa de servicios y AutoMapper
├── ValidationFactory/       # Fábrica de cadenas de validación
└── Validations/             # Implementaciones concretas de validaciones
```

---

## Modelos de datos

La aplicación tiene 5 entidades principales. Entender su relación es clave para saber qué datos necesitás en BD.

### `User` — Usuarios
Representa a las personas que usan la aplicación. Tienen `UserName` (login), `Name` (nombre visible) y `Password` (almacenada como hash **SHA-256**). Es el punto de entrada: sin un usuario no se puede autenticar ni operar el carrito.

> **Requiere seed manual** — la app no tiene registro público, los usuarios deben existir previamente en BD.

### `Session` — Sesiones de autenticación
Se crea automáticamente cada vez que un usuario hace login exitoso. Almacena `UserId`, `SessionStart` y `SessionEnd`. La API devuelve un **token JWT** asociado a la sesión.

> **No requiere seed** — se genera sola al autenticar.

### `Product` — Catálogo de productos
Contiene los artículos disponibles en la tienda: `Sku`, `Name`, `Description`, `AvailableUnits`, `UnitPrice` e `Image`. Es consultado por los endpoints de `/Product/` y es el núcleo del catálogo.

> **Requiere seed** — sin productos la tienda aparece vacía. Los CSVs de `Resources/` cubren esto.

### `ShoppingCart` — Carrito de compras
Representa un carrito abierto por un usuario (`IdUser`). Tiene fechas de creación, actualización y finalización, más el flag `IsCompleted` que se activa al confirmar la compra (`CompleteCartTransaction`).

> **No requiere seed** — se crea automáticamente al agregar el primer producto al carrito.

### `Item` — Ítems dentro del carrito
Es la línea de detalle: relaciona un `ShoppingCart` con un `Product` a través de `IdShoppingCart` e `IdProduct`. Guarda `Quantity`, `TotalPrice` y el flag `IsDeleted` para borrado lógico.

> **No requiere seed** — se crea automáticamente al agregar productos al carrito.

---

## Prerrequisitos

- [.NET 8 SDK](https://dotnet.microsoft.com/download/dotnet/8)
- [PostgreSQL 14+](https://www.postgresql.org/download/)
- (Opcional) [dotnet-ef CLI](https://learn.microsoft.com/en-us/ef/core/cli/dotnet) para gestionar migraciones

Verificá que tenés el SDK instalado:

```bash
dotnet --version
# Debe mostrar 8.x.x
```

---

## Instalación y configuración de la base de datos

### 1. Crear la base de datos en PostgreSQL

Conectate a PostgreSQL con tu cliente (psql, pgAdmin, DBeaver, etc.) y ejecutá:

```sql
CREATE DATABASE carrito_compras;
```

### 2. Configurar la cadena de conexión

Abrí `CarritoComprasBackend/appsettings.json` y ajustá los valores de `ConnectionStrings.WebApiDatabase` según tu entorno:

```json
"ConnectionStrings": {
  "WebApiDatabase": "Host=localhost;Port=5432;Database=carrito_compras;Username=postgres;Password=postgres;Pooling=true;MinPoolSize=1;MaxPoolSize=100;"
}
```

> Para no exponer credenciales en el repositorio, podés sobrescribir la cadena de conexión creando
> `CarritoComprasBackend/appsettings.Development.json` con la clave `ConnectionStrings.WebApiDatabase`.

### 3. Crear el esquema — migraciones EF Core

Desde la **raíz del workspace** (`fai-orden-system/`) ejecutá:

```bash
dotnet ef database update \
  --project backend-dotnet/DataRepository/DataRepository.csproj \
  --startup-project backend-dotnet/CarritoComprasBackend/ShoppingCartBackEnd.csproj
```

Esto aplica la migración `InitialCreate` y crea todas las tablas:

| Tabla | Descripción |
|---|---|
| `Products` | Catálogo de productos |
| `ShoppingCarts` | Carritos de compra |
| `Items` | Ítems dentro de un carrito |
| `Users` | Usuarios registrados |
| `Sessions` | Sesiones de autenticación |

### 4. Cargar datos iniciales — CSVs de productos

En `CarritoComprasBackend/Resources/` hay tres archivos CSV con productos de ejemplo listos para importar:

| Archivo | Contenido |
|---|---|
| `productsEA.csv` | Productos de almacén (lácteos, panadería, etc.) |
| `productsSP.csv` | Productos especiales (café, mermeladas, etc.) |
| `productsWE.csv` | Productos por peso (frutas, carnes, etc.) |

Todos tienen el mismo formato de columnas: `Sku, Name, Description, AvailableUnits, UnitPrice, Image`.

Para importarlos ejecutá el script de seed desde la raíz del workspace:

```bash
bash backend-dotnet/CarritoComprasBackend/Resources/seed.sh
```

El script acepta parámetros opcionales `[host] [port] [user]` si tu PostgreSQL usa una configuración diferente a la de por defecto:

```bash
bash backend-dotnet/CarritoComprasBackend/Resources/seed.sh 192.168.1.10 5433 miusuario
```

> Si usás pgAdmin o DBeaver, podés importar los CSV directamente con la opción **Import/Export** sobre la tabla `Products`.

### 5. Cargar usuario inicial

La app **no tiene registro público**. El mismo script `seed.sh` del paso anterior también crea el usuario administrador:

| Campo | Valor |
|---|---|
| `UserName` | `admin` |
| `Password` | `admin123` |

> La contraseña se almacena como hash SHA-256, igual que lo hace la API internamente. Cambiala después del primer acceso.

---

## Cómo correr el backend

### Opción A — Visual Studio Code (recomendado)

1. Abrí el workspace en VS Code.
2. Presioná `F5` (o **Run > Start Debugging**).
3. Se ejecuta la tarea `build` automáticamente y luego lanza el perfil **"Launch API"**.
4. El navegador abre `https://localhost:5001/swagger` con la documentación interactiva.

### Opción B — Terminal (dotnet CLI)

Desde la carpeta `backend-dotnet/CarritoComprasBackend/`:

```bash
# Restaurar dependencias (solo la primera vez o al agregar paquetes)
dotnet restore

# Compilar
dotnet build

# Ejecutar
dotnet run
```

La API queda disponible en:

- `https://localhost:5001` — HTTPS
- `http://localhost:5000` — HTTP

La UI de Swagger estará en: `https://localhost:5001/swagger`

### Opción C — Modo watch (recarga automática en desarrollo)

```bash
dotnet watch run --project backend-dotnet/CarritoComprasBackend/ShoppingCartBackEnd.csproj
```

---

## Variables de configuración relevantes

| Clave | Ubicación | Descripción |
|---|---|---|
| `ConnectionStrings.WebApiDatabase` | `appsettings.json` | Cadena de conexión a PostgreSQL |
| `auth.secretKey` | `appsettings.json` | Clave secreta para firmar tokens JWT (cambiar en producción) |
| `auth.authActivityTime` | `appsettings.json` | Tiempo de expiración de sesión (minutos) |
| `cache.cacheActivityTime` | `appsettings.json` | Tiempo de vida del caché (minutos) |
| `ASPNETCORE_ENVIRONMENT` | Variable de entorno | `Development` activa Swagger y logs detallados |

> **Importante:** Cambiá `auth.secretKey` por una cadena larga y aleatoria antes de desplegar a producción.

---

## Endpoints principales

| Método | Ruta | Descripción |
|---|---|---|
| `POST` | `/api/auth/...` | Autenticación y sesiones |
| `GET/POST/...` | `/api/store/...` | Gestión del catálogo y tienda |
| `GET/POST/...` | `/api/process/...` | Procesamiento del carrito |

La documentación completa de cada endpoint está disponible en Swagger al correr el proyecto.

---

## Notas adicionales

- El frontend React (cuando esté disponible) espera la API en `http://localhost:3000` → CORS ya está configurado para ese origen.
- Si necesitás crear una nueva migración después de modificar los modelos:
  ```bash
  dotnet ef migrations add NombreDeLaMigracion \
    --project backend-dotnet/DataRepository/DataRepository.csproj \
    --startup-project backend-dotnet/CarritoComprasBackend/ShoppingCartBackEnd.csproj
  ```
