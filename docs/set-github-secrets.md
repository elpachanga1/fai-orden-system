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
Agregar el federated credential para environment:production (ver abajo, solo la primera vez)
        ↓
Pipeline funciona
```

---

## Federated credential para `environment:production` (una sola vez)

> **Por qué es necesario:** Terraform crea automáticamente dos federated credentials OIDC
> (`ref:refs/heads/main` y `pull_request`). Sin embargo, los jobs de GitHub Actions que
> declaran `environment: production` emiten un subject diferente:
> `repo:<org>/<repo>:environment:production`. Azure rechaza el login con `AADSTS700213`
> si no existe un credential que coincida exactamente con ese subject.
>
> **Cuándo correrlo:** Solo la primera vez, como parte del bootstrap. A partir del primer
> `terraform apply` exitoso, el credential queda en el state y Terraform lo gestiona solo.

```powershell
# 1. Obtener el object ID del App Registration creado por Terraform
$appId = az ad app list --display-name "sp-*-github-actions" --query "[0].id" -o tsv

# 2. Agregar el federated credential para environment:production
az ad app federated-credential create --id $appId --parameters '{
  "name": "github-environment-production",
  "issuer": "https://token.actions.githubusercontent.com",
  "subject": "repo:elpachanga1/fai-orden-system:environment:production",
  "audiences": ["api://AzureADTokenExchange"]
}'
```

Verificar en: **Azure Portal → App Registrations → sp-\*-github-actions → Certificates & secrets → Federated credentials**

Después de esto, el pipeline de GitHub Actions (`dotnet` deploy job y `terraform` apply job)
podrá autenticarse via OIDC sin errores.

---

## Cuándo volver a correr `terraform apply`

| Situación | Acción |
|---|---|
| Cambió el Container App FQDN o nombre | `terraform apply` actualiza las Variables automáticamente |
| Rotó el API key del Static Web App | `terraform apply` actualiza el secret automáticamente |
| Primer deploy | Seguir el flujo de bootstrap completo (ver `terraform-bootstrap.md`) |
