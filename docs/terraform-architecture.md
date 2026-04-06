# Decisiones de Arquitectura — Infraestructura Azure

Este documento explica el **por qué** detrás de cada decisión técnica en la infraestructura Terraform de este proyecto. El objetivo es que cualquier persona que lea el `terraform/` entienda la razón de cada recurso, no solo el qué.

---

## Base de Datos: Azure Database for PostgreSQL Flexible Server

**¿Por qué no CosmosDB for PostgreSQL?**

CosmosDB for PostgreSQL (anteriormente Citus) es una extensión de sharding horizontal para PostgreSQL. Está diseñada para cargas masivas que requieren distribuir datos en múltiples nodos. Este proyecto usa:

- Driver `Npgsql.EFCore.PostgreSQL` — driver nativo de PostgreSQL estándar
- EF Core con `AppDbContext` y tablas definidas (`Products`, `ShoppingCarts`, `Items`, `Users`, `Sessions`)
- Un `pg_dump` estándar de PostgreSQL como volcado inicial

**PostgreSQL Flexible Server es 100% compatible con `Npgsql` sin ningún cambio en el código.** CosmosDB for PostgreSQL requeriría adaptar queries y el modelo de datos para sharding — trabajo innecesario para una aplicación de este tamaño.

**¿Cuándo tiene sentido CosmosDB for PostgreSQL?** Cuando tienes decenas de millones de filas y necesitas escalar horizontalmente escribiendo en múltiples nodos simultáneamente.

---

## Backend: App Service sobre Azure Functions

**¿Por qué App Service y no Functions (serverless)?**

El código del backend tiene dos características que son **incompatibles** con Functions en modo Consumption:

**1. `InMemoryRequestRepository` como singleton**

```csharp
// Registrado como Singleton en el DI container
services.AddSingleton<IRequestRepository, InMemoryRequestRepository>();
```

Functions Consumption destruye el proceso entre invocaciones cuando no hay tráfico. El singleton se pierde junto con el proceso. Cada invocación arrancaría con el repositorio en blanco.

**2. EF Core Connection Pooling**

```json
// appsettings.json
"ConnectionString": "...Minimum Pool Size=1;Maximum Pool Size=100;"
```

El pool de conexiones de EF Core vive en la memoria del proceso. En Functions Consumption el proceso arranca desde cero en cada cold start, el pool se reinicializa, y la primera query de cada invocación paga el costo de establecer la conexión.

**¿Cuándo tiene sentido Functions?** Para lógica stateless, event-driven (procesar mensajes de una queue, responder a eventos de storage, timers). Si el backend se refactorizara eliminando el singleton y el pool gestionado manualmente, podría migrar a **Functions Premium** (que mantiene instancias calientes).

**Tier elegido: Basic B1 (dev), Standard S2 o Premium P1v3 (producción)**

| Tier | Precio aprox. | Cuándo usarlo |
|---|---|---|
| Basic B1 | ~$13/mes | Dev/staging sin auto-scaling |
| Standard S2 | ~$97/mes | Producción con auto-scaling y deployment slots |
| Premium P1v3 | ~$119/mes | Producción con mejor rendimiento de VNet Integration |

---

## Red: 4 Subnets Separadas

**¿Por qué no una sola subnet para todo?**

La segmentación de red aplica el principio de **menor privilegio a nivel de red**. Con una sola subnet, si alguien compromete el API Gateway, tiene acceso directo a la base de datos. Con subnets separadas:

| Subnet | Propósito | Qué contiene |
|---|---|---|
| `snet-apim` `10.0.0.0/24` | Capa pública | API Management — único punto de entrada desde internet |
| `snet-appservice` `10.0.1.0/24` | Capa de aplicación | App Service con VNet Integration — solo recibe tráfico de APIM |
| `snet-private-endpoints` `10.0.2.0/24` | Capa de conectividad privada | Private Endpoints de Key Vault, Blob Storage |
| `snet-database` `10.0.3.0/24` | Capa de datos | PostgreSQL Flexible Server — delegada, nadie accede desde fuera |

Los **NSGs** (Network Security Groups) en cada subnet son las reglas que hacen cumplir este aislamiento.

---

## Network Security Groups (NSGs)

**¿Por qué son necesarios?**

Una VNet sin NSGs es como una red física sin firewalls: tienes segmentos de red pero ninguna regla que controle quién puede hablar con quién. Los NSGs definen reglas de `Allow`/`Deny` por subnet:

- `nsg-apim`: Permite tráfico HTTPS (443) desde Internet + puerto 3443 desde el servicio `ApiManagement` (requerido por Azure para gestionar APIM)
- `nsg-appservice`: Permite HTTPS solo desde la subnet de APIM, deniega todo lo demás
- `nsg-pe`: Permite tráfico interno de la VNet hacia los private endpoints

---

## Private Endpoints

**¿Por qué PostgreSQL, Blob Storage y Key Vault no son accesibles desde internet?**

Sin Private Endpoints, estos recursos tienen un endpoint público. Aunque Azure tiene controles de autenticación, la superficie de ataque es mayor: cualquier atacante en internet puede intentar autenticarse contra el endpoint.

Con Private Endpoints:
1. El recurso obtiene una IP privada dentro de tu VNet
2. El endpoint público puede deshabilitarse completamente
3. El tráfico nunca sale de la red de Azure — va de tu App Service al recurso por la red interna

**Consecuencia práctica**: Después del `terraform apply`, no podrás conectarte directamente a la base de datos desde tu laptop a menos que estés dentro de la VNet o uses Azure Bastion / VPN.

---

## Private DNS Zones

**¿Por qué son necesarias si ya tengo Private Endpoints?**

Cuando tu App Service hace una petición a `kv-carrito-dev-abc123.vault.azure.net`, el DNS resuelve a una IP **pública** por defecto. El Private Endpoint tiene una IP privada (`10.0.2.x`), pero nadie le dice al DNS que use esa IP.

Las Private DNS Zones sobreescriben la resolución DNS dentro de tu VNet:

```
kv-carrito-dev-abc123.vault.azure.net
  → sin Private DNS Zone: 52.x.x.x (IP pública, bloqueada)
  → con Private DNS Zone: 10.0.2.5 (IP privada del Private Endpoint)
```

Cada zona cubre un tipo de recurso:
- `privatelink.vaultcore.azure.net` → Key Vault
- `privatelink.postgres.database.azure.com` → PostgreSQL
- `privatelink.blob.core.windows.net` → Blob Storage

---

## Managed Identity

**¿Por qué no usar un Service Principal con password?**

El `appsettings.json` actual tiene el `secretKey` JWT hardcodeado. Eso implica:
- Cualquiera con acceso al repo (o al binario) conoce el secreto
- Si se filtra, hay que rotar manualmente en todos los entornos
- No hay auditoría de quién usó el secreto

Con **Managed Identity**:
1. El App Service tiene una identidad asignada por Azure
2. Se le otorga el rol `Key Vault Secrets User` sobre el Key Vault
3. El código usa `DefaultAzureCredential` — sin secretos en código ni en configuración
4. Azure rota las credenciales automáticamente
5. Cada acceso queda registrado en el audit log del Key Vault

---

## Key Vault

**¿Por qué centralizar secretos en Key Vault?**

Un secreto en el código fuente o en `appsettings.json` es un secreto comprometido desde el momento en que alguien hace `git clone`. Key Vault resuelve:

- **Centralización**: un solo lugar para todos los secretos (`jwt-secret-key`, `postgresql-connection-string`, `app-insights-connection-string`)
- **Auditoría**: log completo de quién accedió a qué secreto y cuándo
- **Rotación**: cambiar el secreto en Key Vault actualiza automáticamente todos los consumidores (via referencia `@Microsoft.KeyVault(SecretUri=...)` en App Service)
- **Acceso con RBAC**: roles de Azure controlan quién puede leer/escribir secretos, no passwords compartidos

---

## OIDC / Federated Credentials para GitHub Actions (fase futura)

**¿Por qué no usar un Service Principal con client_secret en GitHub Secrets?**

Los SP secrets:
- Tienen fecha de expiración (máximo 2 años en Azure AD)
- Si se filtran en logs de CI/CD, son credenciales de larga vida
- Requieren rotación manual

Con **OIDC Federated Credentials**:
1. GitHub Actions solicita un token JWT efímero a GitHub
2. Azure verifica que el token viene del repo y branch correcto (sin contraseña)
3. Se emite un access token de Azure que dura **~1 hora**
4. No existe ninguna credencial persistente que pueda filtrarse

---

## Remote State en Azure Blob Storage

**¿Por qué no commitear el `.tfstate` al repositorio?**

El archivo `terraform.tfstate` contiene **en texto plano**:
- Connection strings de la base de datos
- Claves de acceso
- Todos los valores de outputs `sensitive`

Commitear el `.tfstate` equivale a commitear todos tus secretos. Además:
- Si dos personas corren `terraform apply` al mismo tiempo con estado local, pueden corromper la infraestructura
- El historial de git no es el lugar adecuado para el estado de infraestructura

**Azure Blob Storage con state locking** resuelve ambos problemas:
- El estado está cifrado en reposo (Azure Storage encryption)
- El locking previene applies concurrentes (usando Azure Blob Leases)
- El estado nunca pasa por el repositorio git

---

## API Management (APIM)

**¿Por qué no conectar el frontend directamente al App Service?**

Sin APIM, el frontend llama directamente a `https://app-carrito-dev.azurewebsites.net`. Esto implica:
- El App Service debe estar expuesto a internet
- No hay rate limiting — un atacante puede hacer flood de requests
- Si cambias el backend (App Service → Container Apps, por ejemplo), el frontend necesita cambiar la URL
- No hay un lugar central para aplicar políticas de auth, logging o versioning de API

APIM actúa como fachada:
- El frontend siempre llama a `https://apim-carrito-dev.azure-api.net` — URL estable
- APIM puede validar JWT antes de que el request llegue al backend
- Rate limiting por IP o por usuario sin cambiar código del backend
- El App Service puede estar en una subnet privada — APIM es el único que lo llama

**Tier Developer vs Standard**: Developer no tiene SLA y no escala horizontalmente — solo para desarrollo y testing. En producción usar Standard o Premium.

---

## Application Insights + Log Analytics Workspace

**¿Por qué el proyecto ya tiene el SDK pero necesita el recurso en Azure?**

El backend tiene `ApplicationInsights.AspNetCore` instalado. El SDK sabe cómo enviar telemetría, pero necesita saber **a dónde enviarla**: el `connection_string` del recurso de Application Insights en Azure.

Sin el recurso creado, el SDK no envía nada — simplemente ignora la telemetría. Con el recurso creado, la `connection_string` va al Key Vault y desde ahí al App Service como variable de entorno.

**Log Analytics Workspace** es el backend de almacenamiento de Application Insights en su versión moderna (workspace-based). Permite correlacionar logs del App Service, la base de datos y la aplicación en un solo lugar con KQL.
