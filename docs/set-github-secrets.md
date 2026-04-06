# Guía — Secrets y Variables de GitHub Actions

La infraestructura gestiona automáticamente los secrets y variables de GitHub Actions
mediante el módulo Terraform `github`. No hay script manual.

---

## Qué crea `terraform apply`

### Secrets (cifrados por el provider de Terraform)

| Secret | Origen |
|---|---|
| `AZURE_CLIENT_ID` | `module.oidc.client_id` |
| `AZURE_TENANT_ID` | `module.oidc.tenant_id` |
| `AZURE_SUBSCRIPTION_ID` | `module.oidc.subscription_id` |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | `module.frontend.api_key` |

### Variables (texto plano, visibles en GitHub UI)

| Variable | Origen |
|---|---|
| `REACT_APP_API_URL` | `module.backend.container_app_fqdn` |
| `AZURE_ACR_LOGIN_SERVER` | `module.backend.acr_login_server` |
| `AZURE_CONTAINER_APP_NAME` | `module.backend.container_app_name` |
| `AZURE_RESOURCE_GROUP` | `azurerm_resource_group.main.name` |
| `TF_STATE_STORAGE_ACCOUNT` | `var.tf_state_storage_account` |

Verificar en: **GitHub → Settings → Secrets and variables → Actions**

---

## Secrets que debés crear manualmente (una sola vez)

| Secret | Cómo obtenerlo |
|---|---|
| `TF_STATE_STORAGE_KEY` | Azure Portal → Storage Account → Access keys → key1 |
| `TF_POSTGRESQL_PASSWORD` | El password elegido en `terraform.tfvars` |
| `TF_JWT_SECRET_KEY` | El JWT secret elegido en `terraform.tfvars` |
| `TF_GITHUB_TOKEN` | PAT Fine-grained con **Variables: Read and write** (ver abajo) |

Ir a: **GitHub → Settings → Secrets and variables → Actions → New repository secret**

---

## PAT para `TF_GITHUB_TOKEN`

El provider `integrations/github` usa este token para autenticarse contra la API de GitHub
y crear secrets/variables durante el `terraform apply`.

Crear en: **GitHub → Settings → Developer settings → Fine-grained tokens**

| Campo | Valor |
|---|---|
| Repository access | Solo `fai-orden-system` |
| Permissions → Secrets | Read and write |
| Permissions → Variables | Read and write |

El mismo PAT se usa localmente vía la variable de entorno `GITHUB_TOKEN`:

```powershell
# Setear en el perfil de PowerShell para no repetirlo en cada terminal
Add-Content $PROFILE "`n`$env:GITHUB_TOKEN = `"github_pat_11A...`""
```

---

## Flujo de bootstrap

```
bootstrap-remote-state.ps1
        ↓
terraform init -backend-config=backend.conf
        ↓
terraform plan -out=tfplan
        ↓
terraform apply tfplan          ← crea 4 secrets + 5 variables en GitHub automáticamente
        ↓
Crear manualmente los 4 secrets manuales listados arriba
        ↓
Pipeline funciona
```

---

## Cuándo volver a correr `terraform apply`

| Situación | Acción |
|---|---|
| Cambió el Container App FQDN o nombre | `terraform apply` actualiza las Variables automáticamente |
| Rotó el API key del Static Web App | `terraform apply` actualiza el secret automáticamente |
| Primer deploy | Seguir el flujo de bootstrap completo (ver `terraform-bootstrap.md`) |
