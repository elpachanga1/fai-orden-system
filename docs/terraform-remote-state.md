# Remote State de Terraform — Qué es, cómo funciona y por qué se implementó así

## Por qué `terraform plan` tardó tanto la primera vez

Hubo dos causas independientes:

### 1. Re-descarga de providers (~90MB)

Antes del `terraform init` real con el backend remoto, corrimos `terraform init -backend=false` para validar. Eso descargó los providers a `.terraform/`. Luego borramos esa carpeta para hacer un init limpio, y el segundo `init` tuvo que descargar `azurerm` (~90MB) y `random` de nuevo desde el Terraform Registry.

**A partir de ahora** los providers están cacheados en `.terraform/` — el próximo `terraform init` los reutiliza sin descargar nada.

### 2. `terraform plan` autentica y llama a Azure Resource Manager

Aunque no haya ningún recurso creado aún, `plan` hace:
1. Adquiere el lock del state (escribe un lease en el Azure Blob)
2. Descarga el estado actual (vacío = ~1KB en este caso)
3. Por cada recurso a crear, llama a la API de Azure para confirmar que no existe
4. Libera el lock

Las llamadas a la API de Azure cuestan entre 50-200ms cada una. Con 27 recursos en el plan, eso suma.

### ¿Siempre tardará lo mismo?

| Operación | Primera vez | Subsiguientes |
|---|---|---|
| `terraform init` | ~2-3 min (descarga providers) | ~5 seg (usa caché local) |
| `terraform plan` (infra vacía) | ~30-60 seg | ~15-30 seg |
| `terraform plan` (infra existente) | — | ~20-40 seg (refresh de estado) |
| `terraform apply` | proporcional a recursos creados | proporcional a diff |

El `.tfstate` con 27 recursos pesa aproximadamente 200KB — la descarga/subida al Blob Storage tarda menos de 1 segundo.

---

## Qué es el Remote State

Por defecto, Terraform guarda el estado de la infraestructura en un archivo local llamado `terraform.tfstate`. Ese archivo es la fuente de verdad de Terraform: le dice qué recursos existen, cuáles son sus IDs en Azure, y qué valores tienen sus atributos.

### El problema con el estado local

```
terraform.tfstate  ← contiene esto en texto plano:
  - Host: kv-carrito-dev-abc123.vault.azure.net
  - connection_string: Host=prod-db.postgres...;Password=secreto_real
  - instrumentation_key: a1b2c3d4-...
```

Si el `.tfstate` está en el repositorio:
- Cualquiera con acceso al repo ve todos los secretos
- Si dos personas hacen `terraform apply` al mismo tiempo con estado local, pueden corromper la infraestructura (apply concurrente sin coordinación)
- El historial de git acumula versiones del estado con secretos

### La solución: Remote State en Azure Blob Storage

El estado se guarda en un Storage Account privado en Azure con dos características clave:

**State Locking** — cuando Terraform inicia un `plan` o `apply`, escribe un lease exclusivo en el blob. Si otro proceso intenta correr al mismo tiempo, recibe el error:

```
Error acquiring the state lock:
  Lock info: ID=xxx, Who=user@machine, Operation=plan
```

Esto hace que los applies concurrentes sean imposibles — el segundo proceso espera o falla.

**Cifrado en reposo** — Azure Storage cifra todos los blobs automáticamente con AES-256. Los secretos del estado no están expuestos aunque alguien acceda al Storage Account sin autorización por RBAC.

---

## Cómo funciona el script `scripts/bootstrap-remote-state.ps1`

El script se ejecuta **una sola vez** antes del primer `terraform init`. Crea la infraestructura que Terraform necesita para guardar su propio estado.

### Paso 1: Seleccionar Subscription

```powershell
az account set --subscription $SubscriptionId
```

Establece la subscription activa para todos los comandos `az` subsiguientes. Sin esto, el CLI usa la subscription default (que puede ser otra).

### Paso 2: Registrar Resource Providers

```powershell
foreach ($provider in $providers) {
    $state = az provider show --namespace $provider ...
    if ($state -ne "Registered") {
        az provider register --namespace $provider ...
    }
}
```

Las subscriptions nuevas de Azure no tienen los Resource Providers registrados. Sin este paso, `az storage account create` falla con el error confuso `SubscriptionNotFound` — aunque el ID de subscription sea correcto. El mensaje real debería ser "Microsoft.Storage no está habilitado en esta subscription".

El script registra los 9 providers que necesita todo el proyecto, no solo el del Storage. Esto evita que Terraform fracase provider por provider en cada fase.

El registro es **asíncrono** — el script espera hasta que `Microsoft.Storage` esté en estado `Registered` antes de continuar, usando polling cada 5 segundos con timeout de 2 minutos.

### Paso 3: Crear el Resource Group

```powershell
az group create --name rg-tfstate --location eastus2 --subscription $SubscriptionId
```

Resource Group separado del proyecto principal (`rg-carrito-dev`). La razón: si algún día destruyes toda la infraestructura del proyecto con `terraform destroy`, el state del terraform no se destruye con ella. El RG `rg-tfstate` es independiente y permanente.

### Paso 4: Crear el Storage Account con nombre único

```powershell
$suffix = -join ((48..57) + (97..122) | Get-Random -Count 8 ...)
$StorageAccountName = "sttfstate$suffix"
az storage account create --name $StorageAccountName ...
```

Los nombres de Storage Account son **globalmente únicos** en todo Azure (no solo en tu subscription). El sufijo aleatorio de 8 caracteres alfanuméricos previene colisiones con accounts de otras personas.

Configuración de seguridad:
- `--allow-blob-public-access false` — ningún blob es accesible públicamente sin autenticación
- `--min-tls-version TLS1_2` — TLS 1.0 y 1.1 deshabilitados (están retirados desde Feb 2026)
- `--sku Standard_LRS` — replicación local, suficiente para estado de Terraform

### Paso 5: Crear el Container

```powershell
az storage container create --name tfstate --auth-mode login
```

`--auth-mode login` usa el token de `az login` para autenticar contra el Storage, en vez de usar la account key (más seguro — no necesita regenerar ni guardar claves).

### Paso 6: Generar `terraform/backend.conf`

```powershell
[System.IO.File]::WriteAllText($path, $content, [System.Text.UTF8Encoding]::new($false))
```

**Importante**: Se usa `[System.Text.UTF8Encoding]::new($false)` en lugar de `Out-File -Encoding UTF8`.

PowerShell 5 (Windows) escribe UTF-8 **con BOM** (Byte Order Mark: `0xEF 0xBB 0xBF`) cuando se usa `Out-File`. Terraform no puede parsear archivos con BOM y falla con el error confuso `"Too many command line arguments"`, haciendo parecer que el argumento `-backend-config=backend.conf` está mal formado.

El archivo generado tiene este formato:

```hcl
resource_group_name  = "rg-tfstate"
storage_account_name = "sttfstatek1pnqer3"
container_name       = "tfstate"
key                  = "carrito-compras.tfstate"
```

El campo `key` es la ruta dentro del container donde se guarda el `.tfstate`. Si tuvieras múltiples proyectos usando el mismo Storage Account, cada uno usaría un `key` diferente.

---

## Cómo usar el backend configurado

```powershell
# Solo la primera vez, o cuando cambia backend.conf:
terraform init "-backend-config=backend.conf"
# Las comillas alrededor del argumento son necesarias en PowerShell
# para que no divida el string en dos argumentos al pasar al ejecutable

# Todos los comandos subsiguientes no necesitan especificar el backend:
terraform plan -out=tfplan
terraform apply tfplan
```

### Por qué `backend.conf` está en `.gitignore`

El archivo contiene el nombre del Storage Account (`sttfstatek1pnqer3`). Aunque no es un secreto crítico, exponer el nombre exacto del recurso que guarda el estado de toda la infraestructura añade superficie de ataque innecesaria. Cada desarrollador genera su propio `backend.conf` corriendo el script de bootstrap.

### Qué pasa si pierdes `backend.conf`

```powershell
# Puedes regenerarlo manualmente:
az storage account list --resource-group rg-tfstate --subscription TU_SUB_ID --query "[0].name" -o tsv
# Luego creas backend.conf con ese nombre
```

O corres el script de bootstrap de nuevo — detecta que el Storage Account ya existe y solo regenera el archivo.

---

## Flujo completo de un `terraform apply` con remote state

```
Tu máquina                    Azure Blob Storage              Azure (recursos reales)
     │                               │                               │
     │── terraform apply ──►         │                               │
     │                    Lock ─────►│ (lease exclusivo)             │
     │◄─── lock OK ───────           │                               │
     │                               │                               │
     │── descarga state ────────────►│                               │
     │◄─── state.json (200KB) ──────│                               │
     │                               │                               │
     │── llama ARM API ─────────────────────────────────────────────►│
     │◄─── estado actual ───────────────────────────────────────────│
     │                               │                               │
     │── crea recursos ─────────────────────────────────────────────►│
     │◄─── confirmación ────────────────────────────────────────────│
     │                               │                               │
     │── sube nuevo state ──────────►│                               │
     │── libera lock ───────────────►│                               │
     │                               │                               │
```
