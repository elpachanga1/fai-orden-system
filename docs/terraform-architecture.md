# Decisiones de Arquitectura — Infraestructura Azure

Este documento explica el **por qué** detrás de cada decisión técnica en la infraestructura Terraform de este proyecto. El objetivo es que cualquier persona que lea el `terraform/` entienda la razón de cada recurso, no solo el qué.

> **Estado actual**: VNet restaurada. Backend migrado de App Service a Container Apps por cuota de VMs = 0 en la suscripción. Ver sección [Backend](#backend-container-apps-después-de-app-service) para el razonamiento.

---

## Análisis de Costos: App Service vs Container Apps

La migración a Container Apps no solo resolvió el problema de quota — también reduce el costo operativo en todos los escenarios.

### Por qué App Service cobra aunque nadie use la app

App Service reserva una VM dedicada que corre **24/7** independientemente del tráfico. Pagás por el tiempo encendido, no por el uso real. En una suscripción sin quota esto era irrelevante porque directamente no se podía crear — pero en una suscripción normal los costos serían:

| Tier | Precio | Nota |
|---|---|---|
| F1 Free | $0/mes | 60 min CPU/día, sin SLA, sin always_on |
| B1 Basic | ~$13/mes | always_on, mínimo viable para staging |
| S1 Standard | ~$73/mes | auto-scaling, deployment slots |

### Cómo cobra Container Apps

Dos componentes:

**ACR Basic**: $5/mes fijo — almacena las imágenes Docker.

**Container App**: consumo puro con free tier mensual incluido:
- vCPU: $0.000024 / vCPU-segundo activo
- Memoria: $0.000003 / GiB-segundo activo
- Free tier: 180,000 vCPU-segundos + 360,000 GiB-segundos por mes

Con `min_replicas = 0`, cuando no hay tráfico el Container App baja a cero réplicas → **$0 de cómputo**.

### Simulación para dev (uso esporádico, ~4 horas/día)

```
0.25 vCPU × 4hs × 3600s × 30 días = 108,000 vCPU-segundos
Free tier mensual:                   180,000 vCPU-segundos
                                     ─────────────────────
Cómputo adicional:                   $0 (dentro del free tier)

Total dev: ~$5/mes (solo ACR)
```

### Simulación para prod (24/7 con tráfico constante)

```
vCPU:    0.25 × 86,400s × 30 días = 648,000 vCPU-seg
         Free tier:                  180,000 vCPU-seg
         Exceso: 468,000 × $0.000024 = ~$11/mes

Memoria: 0.5Gi × 86,400s × 30 días = 1,296,000 GiB-seg
         Free tier:                    360,000 GiB-seg
         Exceso: 936,000 × $0.000003 = ~$2.8/mes

Total prod 24/7: ~$19/mes  (ACR $5 + cómputo $14)
```

### Comparativa final

| Escenario | App Service S1 | Container Apps |
|---|---|---|
| Dev uso esporádico | $73/mes (si hubiera quota) | **~$5/mes** |
| Prod 24/7 | $73/mes | **~$19/mes** |
| Sin tráfico (noche/finde) | $73/mes igual | **$0 cómputo** |
| Quota requerida | Standard VMs (= 0 en esta suscripción) | Ninguna |

Container Apps es más barato en todos los escenarios. La única adición fija es el ACR Basic ($5/mes), que también sería necesario si en algún momento se usara App Service con contenedores.

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

## Backend: Container Apps (después de App Service)

### Por qué empezamos con App Service

El código del backend tiene dos características que son **incompatibles** con Azure Functions en modo Consumption:

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

El pool de conexiones vive en memoria del proceso. En Functions Consumption el proceso arranca desde cero en cada cold start.

App Service era la alternativa natural: proceso persistente, pool de conexiones estable, compatible con el singleton.

---

### Por qué App Service no funcionó: quota de VMs = 0

Durante el primer `terraform apply` se descubrió que **ningún tier de App Service tiene cuota en esta suscripción**:

```
Error: creating App Service Plan...
Current Limit (Free VMs): 0        ← F1 fallaba
Current Limit (Basic VMs): 0       ← B1 fallaba
Current Limit (Standard VMs): 0    ← S1 fallaba
Current Limit (PremiumV2 VMs): 0   ← P1v2 fallaba
Current Limit (PremiumV3 VMs): 0   ← P1v3 fallaba
```

Esto ocurre en suscripciones de tipo **Free Trial, Student o CSP** — Azure no asigna cuota de VMs automáticamente. El proceso para solicitarlo tarda entre 30 minutos y 2 días hábiles, lo que bloquea el desarrollo.

Lo curioso del mensaje de error: dice _"Amount required: 0"_ porque Azure usa un template de mensaje que no resuelve el parámetro correctamente, pero el fallo real es `Current Limit = 0`.

---

### Por qué se eligió Container Apps y no solicitar la cuota

**Azure Container Apps** usa un modelo de consumo serverless diferente al de App Service: no reserva VMs dedicadas, no consume de ninguna cuota de `*VMs`, y está disponible en cualquier tipo de suscripción.

Además resuelve los dos problemas originales de Functions:

| Problema | Functions Consumption | App Service | Container Apps |
|---|---|---|---|
| Singleton en memoria | ❌ proceso destruido | ✅ proceso persistente | ✅ `min_replicas = 0` pero el proceso persiste mientras hay tráfico |
| Connection pool EF Core | ❌ cold start reinicia pool | ✅ pool estable | ✅ pool estable |
| Cuota de VMs | N/A | ❌ quota = 0 | ✅ sin cuota de VMs |
| Precio dev (0 tráfico) | ~$0 | ~$0 (F1) o ~$13+ (B1+) | ~$0 (scale to 0) |

**`min_replicas = 0`**: Container Apps puede bajar a cero réplicas cuando no hay tráfico (igual que Functions), pero cuando escala a 1 réplica el proceso es continuo — el singleton y el pool no se pierden entre requests.

---

### Qué cambió arquitectónicamente

**Antes (App Service)**:
- `azurerm_service_plan` (SKU configurable) + `azurerm_linux_web_app`
- Secretos inyectados como `@Microsoft.KeyVault(SecretUri=...)` en `app_settings`
- VNet Integration vía `snet-appservice` (delegación `Microsoft.Web/serverFarms`, `/24`)
- Deploy: `dotnet publish` → artefacto ZIP → `azure/webapps-deploy@v3`

**Ahora (Container Apps)**:
- `azurerm_container_registry` (Basic) + `azurerm_container_app_environment` + `azurerm_container_app`
- Secretos inyectados como `secret { key_vault_secret_id = ... }` bloques nativos de Container Apps
- VNet Integration vía `snet-containerapp` (delegación `Microsoft.App/environments`, `/23` mínimo)
- Deploy: `az acr build` (imagen Docker) → `az containerapp update` (nueva revisión)

**Nuevos recursos creados por Terraform**:
- ACR `acrcarritodev<suffix>` — registry de imágenes Docker
- Container Apps Environment `cae-carrito-dev` — entorno compartido (Log Analytics integrado)
- Container App `ca-carrito-dev` — la API, con `cpu=0.25`, `memory=0.5Gi`, `min_replicas=0`
- RBAC `AcrPull` para la Managed Identity del Container App
- RBAC `AcrPush` para el SP de GitHub Actions OIDC

**Nuevos secrets en GitHub** (creados automáticamente por el módulo `github`):
- `AZURE_ACR_LOGIN_SERVER` — para `az acr build`
- `AZURE_CONTAINER_APP_NAME` — para `az containerapp update`
- `AZURE_RESOURCE_GROUP` — contexto del resource group

---

### Imagen inicial (placeholder)

Terraform crea el Container App con `mcr.microsoft.com/dotnet/samples:aspnetapp` como imagen inicial. Esta imagen de ejemplo responde en el puerto 8080 y sirve como smoke test de que el entorno funciona antes del primer build real. El primer `git push` a `main` la reemplaza con la imagen del proyecto.

---

## Red: VNet con subnets (dev y prod)

**Historial**: la VNet se quitó temporalmente cuando se intentó simplificar el entorno dev para evitar el error de quota de App Service. Una vez migrado a Container Apps (que no tiene restricciones de quota), se restauró la VNet porque Container Apps la soporta nativamente y la integración de red es necesaria para que PostgreSQL funcione en modo privado.

**Arquitectura de subnets actual**:

| Subnet | CIDR | Delegación | Propósito |
|---|---|---|---|
| `snet-apim` | `/24` | — | Reservada para API Management futuro |
| `snet-containerapp` | `/23` | `Microsoft.App/environments` | Container Apps Environment — requiere `/23` mínimo (512 IPs para infraestructura interna de Azure) |
| `snet-private-endpoints` | `/24` | — | Private Endpoints de Key Vault y Blob Storage |
| `snet-database` | `/24` | `Microsoft.DBforPostgreSQL/flexibleServers` | PostgreSQL Flexible Server |

**¿Por qué `/23` para Container Apps?**

Azure reserva un bloque grande de IPs para la infraestructura interna del Container Apps Environment (nodos de control, proxies, DNS interno). Si la subnet es menor a `/23`, el apply falla con "subnet too small".

**Tabla de seguridad por entorno**:

| Recurso | Dev (actual) | Prod (a implementar) |
|---|---|---|
| Container App | Ingress externo habilitado | Ingress externo habilitado |
| PostgreSQL acceso | Público (firewall Azure services) + subnet delegada | Solo subnet delegada |
| Key Vault acceso | Público (`network_acls Allow`) + Private Endpoint | Solo Private Endpoint |
| Blob Storage acceso | Público + Private Endpoint | Solo Private Endpoint |
| NSGs | Sí (APIM + private endpoints) | Sí |
| Private DNS Zones | Sí | Sí |

---

## Private Endpoints (referencia para prod)

**¿Por qué PostgreSQL, Blob Storage y Key Vault no deberían ser accesibles desde internet en producción?**

Sin Private Endpoints, estos recursos tienen un endpoint público. Aunque Azure tiene controles de autenticación, la superficie de ataque es mayor: cualquier atacante en internet puede intentar autenticarse contra el endpoint.

Con Private Endpoints:
1. El recurso obtiene una IP privada dentro de tu VNet
2. El endpoint público puede deshabilitarse completamente
3. El tráfico nunca sale de la red de Azure — va de tu App Service al recurso por la red interna

---

## Private DNS Zones (referencia para prod)

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
3. Azure rota las credenciales automáticamente
4. Cada acceso queda registrado en el audit log del Key Vault

---

## Key Vault

**¿Por qué centralizar secretos en Key Vault?**

Un secreto en el código fuente o en `appsettings.json` es un secreto comprometido desde el momento en que alguien hace `git clone`. Key Vault resuelve:

- **Centralización**: un solo lugar para todos los secretos (`jwt-secret-key`, `postgresql-connection-string`, `app-insights-connection-string`)
- **Auditoría**: log completo de quién accedió a qué secreto y cuándo
- **Rotación**: cambiar el secreto en Key Vault actualiza automáticamente todos los consumidores (via referencia `@Microsoft.KeyVault(SecretUri=...)` en App Service)
- **Acceso con RBAC**: roles de Azure controlan quién puede leer/escribir secretos, no passwords compartidos

---

## OIDC / Federated Credentials para GitHub Actions

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

**Bootstrap catch-22**: los secrets de OIDC (`AZURE_CLIENT_ID`, etc.) los crea el módulo `github` en el primer `terraform apply`. Por eso el primer apply debe correrse localmente. Solo a partir del segundo push el pipeline puede autenticarse con OIDC por sí solo.

---

## Remote State en Azure Blob Storage

**¿Por qué no commitear el `.tfstate` al repositorio?**

El archivo `terraform.tfstate` contiene **en texto plano**:
- Connection strings de la base de datos
- Claves de acceso
- Todos los valores de outputs `sensitive`

Commitear el `.tfstate` equivale a commitear todos tus secretos. Además, si dos personas corren `terraform apply` al mismo tiempo con estado local, pueden corromper la infraestructura.

**Azure Blob Storage con state locking** resuelve ambos problemas:
- El estado está cifrado en reposo (Azure Storage encryption)
- El locking previene applies concurrentes (usando Azure Blob Leases)
- El estado nunca pasa por el repositorio git

**Autenticación del backend**: el pipeline usa `access_key` del Storage Account (secret `TF_STATE_STORAGE_KEY`) para autenticar `terraform init`. Esto es independiente de OIDC, lo que permite que el pipeline funcione antes de que exista el App Registration.

---

## API Management (APIM) — no implementado en dev

Sin APIM, el frontend llama directamente al App Service. Para dev esto es suficiente. APIM agrega valor en producción cuando necesitás rate limiting, API versioning, políticas centralizadas de auth o routing entre múltiples backends.

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
