# Guía de Bootstrap — Infraestructura Azure con Terraform

Esta guía explica cómo desplegar la infraestructura por primera vez. El proceso tiene dos etapas: preparar el remote state (una sola vez) y correr Terraform.

---

## Requisitos previos

| Herramienta | Versión mínima | Cómo verificar |
|---|---|---|
| [Azure CLI](https://learn.microsoft.com/cli/azure/install-azure-cli) | 2.50+ | `az --version` |
| [Terraform](https://developer.hashicorp.com/terraform/install) | 1.10+ | `terraform --version` |
| [Git](https://git-scm.com/) | cualquiera | `git --version` |
| Permisos Azure | Contributor en la subscription | — |
| Permisos GitHub | Dueño del repo | — |

Autenticarse en Azure antes de empezar:

```powershell
az login
az account show   # verificar que está la subscription correcta
```

---

## Paso 1 — Remote state (UNA SOLA VEZ)

Terraform necesita un Storage Account en Azure para guardar el estado de la infraestructura. Este script lo crea:

```powershell
.\scripts\bootstrap-remote-state.ps1 -SubscriptionId "2754c055-9b6b-486b-9f14-bc18efbb40e6"
```

**Qué hace el script:**

| Paso | Qué crea |
|---|---|
| 1 | Selecciona la subscription |
| 2 | Registra 9 resource providers (Storage, KeyVault, Web, PostgreSQL, etc.) |
| 3 | Crea el resource group `rg-tfstate` en `eastus2` |
| 4 | Crea el Storage Account (nombre único con sufijo aleatorio, ej. `sttfstatek1pnqer3`) |
| 5 | Crea el container `tfstate` dentro del Storage Account |
| 6 | Recupera la access key y genera `terraform/backend.conf` |

Al final el script imprime en pantalla dos secrets que debés crear manualmente en GitHub.

> **Parámetros opcionales:**
> ```powershell
> .\scripts\bootstrap-remote-state.ps1 `
>     -SubscriptionId "xxxx" `
>     -Location "eastus2" `        # región del storage (default: eastus2)
>     -ResourceGroupName "rg-tfstate" `  # default: rg-tfstate
>     -ContainerName "tfstate"           # default: tfstate
> ```

---

## Paso 2 — Secrets manuales en GitHub

El pipeline necesita dos secrets para poder conectarse al remote state desde el primer push. Los otros secrets los crea Terraform automáticamente en el Paso 5.

Ir a: **GitHub → Settings → Secrets and variables → Actions → New repository secret**

| Secret | Valor | Cómo obtenerlo |
|---|---|---|
| `TF_STATE_STORAGE_ACCOUNT` | nombre del storage account | lo muestra el script al finalizar |
| `TF_STATE_STORAGE_KEY` | access key del storage account | está en `terraform/backend.conf` → campo `access_key` |

---

## Paso 3 — Crear `terraform.tfvars`

```powershell
cd terraform
Copy-Item terraform.tfvars.example terraform.tfvars
```

Editar `terraform.tfvars` con los valores reales:

```hcl
location    = "eastus2"
prefix      = "carrito"
environment = "dev"

tags = {
  owner   = "tu-usuario-github"
  project = "carrito-compras"
  repo    = "tu-org/fai-orden-system"
}

postgresql_admin_username = "admincarrito"
postgresql_admin_password = "UnaContrasenaSegura2026!"   # mínimo 8 chars, mayúscula, número, especial

jwt_secret_key = "una-clave-aleatoria-de-al-menos-32-caracteres"
# Generar en PowerShell: -join ((1..40) | ForEach-Object { [char](Get-Random -Min 65 -Max 90) })

# PAT de GitHub — Fine-grained token con permiso Secrets: Read and write
# Crear en: https://github.com/settings/tokens → Fine-grained tokens
# Scope: solo el repo fai-orden-system
github_token  = "github_pat_11A..."
github_org    = "tu-org"
github_repo   = "fai-orden-system"
github_branch = "main"

# Nombre del Storage Account generado en el Paso 1
tf_state_storage_account = "sttfstatek1pnqer3"
```

> `terraform.tfvars` está en `.gitignore` — nunca lo commitees. Contiene contraseñas y tokens.

---

## Paso 4 — Inicializar Terraform

```powershell
cd terraform
terraform init -reconfigure "-backend-config=backend.conf"
```

`-reconfigure` es necesario si ya corriste un `init` anterior con una configuración de backend diferente.

Verificar que la configuración es válida:

```powershell
terraform validate
```

---

## Paso 5 — Plan y Apply (primer deploy)

El módulo `github` de Terraform necesita autenticarse con la API de GitHub para crear
las Variables de Actions. Antes de correr el plan, setear la variable de entorno:

```powershell
$env:GITHUB_TOKEN = "github_pat_11A..."   # tu TF_GITHUB_TOKEN
```

Para no tener que setearlo en cada nueva terminal, agregarlo al perfil de PowerShell:

```powershell
Add-Content $PROFILE "`n`$env:GITHUB_TOKEN = `"github_pat_11A...`""
```

> El token necesita permisos **Secrets: Read and write** y **Variables: Read and write** en el repo.
> Crearlo en: GitHub → Settings → Developer settings → Fine-grained tokens

```powershell
terraform plan -out=tfplan
```

El plan tarda entre 5 y 15 minutos en el primer run. Al terminar, revisá que no haya nada inesperado.

```powershell
terraform apply tfplan
```

El apply crea en orden:
1. Resource group `rg-carrito-dev`
2. Log Analytics + Application Insights
3. Key Vault con los 3 secretos (connection string, JWT key, App Insights key)
4. PostgreSQL Flexible Server + base de datos `carrito_compras`
5. Storage Account + container `product-images`
6. Container App + Azure Container Registry
7. Static Web App (Free)
8. App Registration en Entra ID + Federated Credentials para GitHub Actions (OIDC)
9. **4 Secrets en GitHub** (AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID, AZURE_STATIC_WEB_APPS_API_TOKEN)
10. **5 Variables en GitHub** (REACT_APP_API_URL, AZURE_ACR_LOGIN_SERVER, AZURE_CONTAINER_APP_NAME, AZURE_RESOURCE_GROUP, TF_STATE_STORAGE_ACCOUNT)

Al finalizar el apply, GitHub ya tiene todo lo que necesitan los pipelines de dotnet y react.

---

## Paso 6 — Crear los secrets manuales y verificar en GitHub

Antes de hacer el primer push, crear estos 4 secrets manualmente en
**GitHub → Settings → Secrets and variables → Actions → New repository secret**:

| Secret | Cómo obtenerlo |
|---|---|
| `TF_STATE_STORAGE_KEY` | Azure Portal → Storage Account → Access keys → key1 |
| `TF_POSTGRESQL_PASSWORD` | El password elegido en `terraform.tfvars` |
| `TF_JWT_SECRET_KEY` | El JWT secret elegido en `terraform.tfvars` |
| `TF_GITHUB_TOKEN` | El mismo PAT usado localmente (con Secrets + Variables: Read and write) |

Verificar que el `terraform apply` del Paso 5 creó los automáticos:

**Secrets creados por Terraform:**
`AZURE_CLIENT_ID`, `AZURE_TENANT_ID`, `AZURE_SUBSCRIPTION_ID`, `AZURE_STATIC_WEB_APPS_API_TOKEN`

**Variables creadas por Terraform** (pestaña Variables):
`REACT_APP_API_URL`, `AZURE_ACR_LOGIN_SERVER`, `AZURE_CONTAINER_APP_NAME`, `AZURE_RESOURCE_GROUP`, `TF_STATE_STORAGE_ACCOUNT`

Ver: [set-github-secrets.md](set-github-secrets.md) para más detalles sobre la distribución.

---

## Paso 7 — Primer push

```powershell
git add .
git commit -m "feat: infraestructura inicial"
git push
```

El pipeline `CI / CD` va a correr. En el primer push a `main`:
- `terraform` job: corre `plan` (OIDC ya funciona gracias al Paso 5)
- `dotnet` job: build + test + deploy al App Service
- `react` job: build + deploy al Static Web App

El `terraform apply` en CI requiere aprobación manual del environment `production` (configurar en GitHub → Settings → Environments → production).

---

## Solución de problemas frecuentes

**`Error acquiring the state lock — state blob is already locked`**

El pipeline falló y dejó el state bloqueado. Desbloquear con:

```powershell
terraform force-unlock <ID-del-lock>
```

El Lock ID aparece en el mensaje de error.

---

**`Backend configuration changed`**

El `backend.conf` cambió desde el último `init`. Usar:

```powershell
terraform init -reconfigure "-backend-config=backend.conf"
```

---

**`ARM_CLIENT_ID empty — a Tenant ID must be configured`**

El pipeline corrió antes de que existiera el App Registration. Causa: los secrets de OIDC los crea el `terraform apply` local (Paso 5). Solución: asegurarse de haber completado el Paso 5 antes del primer push.

---

**PostgreSQL: no podés conectarte desde tu laptop**

El firewall de PostgreSQL solo permite servicios de Azure (`0.0.0.0 → 0.0.0.0`). Para conectarte desde DBeaver/pgAdmin, agregar tu IP en el portal:

**Azure Portal → PostgreSQL → Networking → Add current client IP address → Save**

---

## Destruir la infraestructura

Para eliminar todos los recursos de Azure y evitar costos:

```powershell
cd terraform
terraform destroy
```

> El Storage Account del remote state (`rg-tfstate`) **no** lo destruye este comando porque fue creado por el bootstrap script, no por Terraform. Para borrarlo: `az group delete --name rg-tfstate`.
