# Guía — set-github-secrets.ps1

Este script crea el secret `AZURE_STATIC_WEB_APPS_API_TOKEN` en GitHub Actions.
Es el único secret que no puede ser una Variable pública porque es un token de deploy real.

Los otros 8 valores de infraestructura (`AZURE_CLIENT_ID`, `REACT_APP_API_URL`, etc.)
los crea automáticamente el módulo Terraform `github` como **GitHub Actions Variables**
durante el `terraform apply`.

---

## Cuándo ejecutarlo

| Situación | ¿Ejecutar? |
|---|---|
| Primer deploy — `terraform apply` terminó OK | **Sí, siempre** |
| Re-deploy — cambió la URL o el token del Static Web App | Sí |
| El pipeline falla con `AZURE_STATIC_WEB_APPS_API_TOKEN not found` | Sí |
| Solo cambiaron archivos de código (backend/frontend) | No necesario |

> Las 8 Variables de infraestructura las gestiona Terraform automáticamente.
> Este script solo existe para el token de deploy del Static Web App.

---

## Requisitos previos

| Herramienta | Versión mínima | Cómo verificar |
|---|---|---|
| .NET SDK | 8+ | `dotnet --version` |
| Terraform CLI | 1.10+ | `terraform --version` |
| PowerShell | 5.1+ | `$PSVersionTable.PSVersion` |
| Terraform apply corrido | — | `terraform output -json` devuelve datos |

### GitHub PAT necesario

Crear un **Fine-grained token** en: **GitHub → Settings → Developer settings → Fine-grained tokens**

| Campo | Valor |
|---|---|
| Repository access | Solo `fai-orden-system` |
| Permissions → Secrets | Read and write |

> Este PAT solo necesita permisos de **Secrets**. Los permisos de Variables
> son para el PAT `TF_GITHUB_TOKEN` que usa Terraform.

---

## Uso

```powershell
cd C:\Code\fai-orden-system
.\scripts\set-github-secrets.ps1 -GithubToken "github_pat_11A..."
```

---

## Paso a paso — qué hace el script

### Paso 1 — Lee `frontend_api_key` de Terraform

```powershell
terraform output -json
```

Extrae solo `frontend_api_key.value` — el API key del Static Web App.

### Paso 2 — Obtiene la public key del repo

```
GET https://api.github.com/repos/{repo}/actions/secrets/public-key
```

GitHub exige que los secrets se cifren con la public key del repo antes de
enviarse. La respuesta incluye `key_id` y `key` (Base64).

### Paso 3 — Construye un helper .NET temporal

Crea un proyecto C# mínimo en `$TEMP\gh-secrets-helper-*` con la dependencia
[Sodium.Core](https://github.com/tabrath/libsodium-core) (binding de libsodium).
El helper cifra el valor con `SealedPublicKeyBox.Create` (X25519 + XSalsa20-Poly1305)
y llama a la API de GitHub.

> Se usa un helper .NET porque PowerShell 5.1 no tiene bindings nativos de
> libsodium y la API de GitHub requiere ese cifrado específico.

### Paso 4 — Escribe el payload y ejecuta el helper

El JSON se escribe en disco como UTF-8 sin BOM y se pasa al helper como
argumento de archivo (evita el BOM que PowerShell 5.1 añade al piping a procesos nativos).

### Paso 5 — Limpia el helper temporal

El directorio `$TEMP\gh-secrets-helper-*` se elimina al finalizar.

---

## Secret que crea este script

| Secret | Descripción |
|---|---|
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | Token de deploy del Static Web App — cualquiera con este token puede publicar al sitio |

## Variables que crea Terraform (NO este script)

| Variable | Output de Terraform |
|---|---|
| `AZURE_CLIENT_ID` | `oidc_client_id` |
| `AZURE_TENANT_ID` | `oidc_tenant_id` |
| `AZURE_SUBSCRIPTION_ID` | `oidc_subscription_id` |
| `REACT_APP_API_URL` | `backend.fqdn` |
| `AZURE_ACR_LOGIN_SERVER` | `backend.acr_login_server` |
| `AZURE_CONTAINER_APP_NAME` | `backend.container_app_name` |
| `AZURE_RESOURCE_GROUP` | `resource_group_name` |
| `TF_STATE_STORAGE_ACCOUNT` | `var.tf_state_storage_account` |

Verificar en: **GitHub → Settings → Variables → Actions**

## Secrets que debés crear manualmente

| Secret | Cómo obtenerlo |
|---|---|
| `TF_STATE_STORAGE_KEY` | `terraform/backend.conf` → campo `access_key` |
| `TF_POSTGRESQL_PASSWORD` | el password elegido en `terraform.tfvars` |
| `TF_JWT_SECRET_KEY` | el JWT secret elegido en `terraform.tfvars` |
| `TF_GITHUB_TOKEN` | PAT Fine-grained con **Variables: Read and write** |

Ir a: **GitHub → Settings → Secrets and variables → Actions → New repository secret**

---

## ¿Por qué no está integrado en el workflow de CI/CD?

Dependencia circular inevitable: el workflow necesita los secrets para correr,
pero el script que los crea requiere un PAT que también vive como secret.

**Flujo correcto de bootstrap:**

```
bootstrap-remote-state.ps1   →   terraform apply   →   set-github-secrets.ps1   →   pipeline funciona
                                      ↓
                              (crea 8 Variables automáticamente)
```

Este script sincroniza los outputs de Terraform como GitHub Actions secrets.
Se ejecuta **una vez localmente** después del primer `terraform apply`, o cuando
necesitás actualizar los secrets sin re-ejecutar todo el apply.

---

## Cuándo ejecutarlo

| Situación | ¿Ejecutar? |
|---|---|
| Primer deploy — `terraform apply` terminó OK | **Sí, siempre** |
| Re-deploy — cambiaron outputs (ej. nueva URL del Container App) | Sí |
| El pipeline falla con `secret not found` | Sí |
| Solo cambiaron archivos de código (backend/frontend) | No necesario |

> **Este script es el mecanismo oficial** para crear los secrets en GitHub.
> El módulo Terraform `github` fue removido del proyecto (`terraform/main.tf` línea 162).
> Terraform solo crea la infraestructura Azure y el App Registration OIDC;
> los secrets de GitHub Actions los gestiona exclusivamente este script.

---

## Requisitos previos

| Herramienta | Versión mínima | Cómo verificar |
|---|---|---|
| .NET SDK | 8+ | `dotnet --version` |
| Terraform CLI | 1.10+ | `terraform --version` |
| PowerShell | 5.1+ | `$PSVersionTable.PSVersion` |
| Terraform apply corrido | — | `terraform output -json` devuelve datos |

### GitHub PAT necesario

Crear un **Fine-grained token** en: **GitHub → Settings → Developer settings → Fine-grained tokens**

| Campo | Valor |
|---|---|
| Repository access | Solo `fai-orden-system` |
| Permissions → Secrets | Read and write |

---

## Uso

```powershell
cd C:\Code\fai-orden-system

# Forma estándar — el Storage Account se lee solo desde terraform/backend.conf
.\scripts\set-github-secrets.ps1 -GithubToken "github_pat_11A..."

# Forma explícita — útil si backend.conf no está disponible
.\scripts\set-github-secrets.ps1 `
    -GithubToken "github_pat_11A..." `
    -TfStateStorageAccount "sttfstatek1pnqer3"
```

---

## Paso a paso — qué hace el script

### Paso 1 — Lee los outputs de Terraform

```powershell
terraform output -json
```

Extrae los valores que Azure generó durante el `apply`:

| Variable interna | Output de Terraform |
|---|---|
| `$azureClientId` | `oidc_client_id` |
| `$azureTenantId` | `oidc_tenant_id` |
| `$azureSubscriptionId` | `oidc_subscription_id` |
| `$staticWebAppToken` | `frontend_api_key` |
| `$containerAppName` | `backend.container_app_name` |
| `$acrLoginServer` | `backend.acr_login_server` |
| `$resourceGroup` | `resource_group_name` |
| `$backendFqdn` | `backend.fqdn` |

### Paso 2 — Resuelve el nombre del Storage Account del remote state

Busca `storage_account_name` en `terraform/backend.conf` (generado por
`bootstrap-remote-state.ps1`). Si no lo encuentra, lo pide interactivamente.
También se puede pasar explícitamente con `-TfStateStorageAccount`.

### Paso 3 — Obtiene la public key del repo

```
GET https://api.github.com/repos/{repo}/actions/secrets/public-key
```

GitHub exige que los secrets se cifren con la public key del repo antes de
enviarse. La respuesta incluye `key_id` y `key` (Base64).

### Paso 4 — Construye un helper .NET temporal

Crea un proyecto C# mínimo en `$TEMP\gh-secrets-helper-*` con la dependencia
[Sodium.Core](https://github.com/tabrath/libsodium-core) (binding de libsodium).
El helper recibe un JSON con todos los valores, cifra cada secret con
`SealedPublicKeyBox.Create` (X25519 + XSalsa20-Poly1305) y llama a la API de
GitHub por cada uno.

> Se usa un helper .NET porque PowerShell 5.1 no tiene bindings nativos de
> libsodium y la API de GitHub requiere ese cifrado específico.

### Paso 5 — Filtra valores vacíos

Cualquier secret cuyo valor esté vacío (ej. `frontend_api_key` si no se
desplegó Static Web App) se omite — el secret existente en GitHub no se
sobreescribe.

### Paso 6 — Escribe el payload y ejecuta el helper

El JSON se escribe en disco como UTF-8 sin BOM (requerimiento del parser .NET
en PowerShell 5.1) y se pasa al helper como argumento de archivo.

### Paso 7 — Limpia el helper temporal

El directorio `$TEMP\gh-secrets-helper-*` se elimina al finalizar.

---

## Secrets que crea este script

| Secret | Descripción |
|---|---|
| `AZURE_CLIENT_ID` | Client ID de la App Registration (OIDC) |
| `AZURE_TENANT_ID` | Tenant ID de Entra ID |
| `AZURE_SUBSCRIPTION_ID` | Subscription ID de Azure |
| `AZURE_STATIC_WEB_APPS_API_TOKEN` | Token de deploy del Static Web App |
| `REACT_APP_API_URL` | URL pública del Container App backend |
| `AZURE_ACR_LOGIN_SERVER` | Login server del Azure Container Registry |
| `AZURE_CONTAINER_APP_NAME` | Nombre del Container App |
| `AZURE_RESOURCE_GROUP` | Nombre del Resource Group principal |
| `TF_STATE_STORAGE_ACCOUNT` | Nombre del Storage Account del remote state |

## Secrets que debés crear manualmente

El script **no** puede leer estos desde Terraform porque son inputs, no outputs:

| Secret | Cómo generarlo |
|---|---|
| `TF_POSTGRESQL_PASSWORD` | El password que pusiste en `terraform.tfvars` |
| `TF_JWT_SECRET_KEY` | El JWT secret que pusiste en `terraform.tfvars` |
| `TF_STATE_STORAGE_KEY` | Access key del Storage Account — está en `terraform/backend.conf` → campo `access_key` |
| `TF_GITHUB_TOKEN` | El mismo PAT Fine-grained que usaste con `-GithubToken` |

Ir a: **GitHub → Settings → Secrets and variables → Actions → New repository secret**

---

## ¿Por qué no está integrado en el workflow de CI/CD?

El módulo Terraform `github` fue removido del proyecto para simplificar la configuración
del provider y eliminar la dependencia del `github_token` en el `apply`.
Este script corre **una sola vez localmente** como paso post-`apply`.

No se integra en el pipeline por una dependencia circular inevitable:
el workflow necesita los secrets para correr, pero el script que los crea
requiere un PAT que también debería vivir como secret.

**Flujo correcto de bootstrap:**

```
bootstrap-remote-state.ps1   →   terraform apply   →   set-github-secrets.ps1   →   pipeline funciona
```
