# ---------------------------------------------------------------
# User Assigned Managed Identity
#
# Se usa User Assigned (no System Assigned) porque:
# - Tiene ciclo de vida independiente del Container App
# - Se puede pre-autorizar en Key Vault y ACR antes de que el
#   Container App exista
# - Si se destruye y recrea el Container App, los permisos persisten
# ---------------------------------------------------------------
resource "azurerm_user_assigned_identity" "main" {
  name                = "id-${var.prefix}-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  tags                = var.tags
}

# ---------------------------------------------------------------
# Azure Container Registry (Basic SKU)
# Almacena las imagenes Docker del backend.
# El CI construye con 'az acr build' y actualiza el Container App.
# Sufijo aleatorio para nombre globalmente unico (solo alfanumerico).
# ---------------------------------------------------------------
resource "random_string" "acr_suffix" {
  length  = 6
  special = false
  upper   = false
}

resource "azurerm_container_registry" "main" {
  name                = "acr${replace(var.prefix, "-", "")}${var.environment}${random_string.acr_suffix.result}"
  resource_group_name = var.resource_group_name
  location            = var.location
  sku                 = "Basic"

  # La Managed Identity del Container App necesita AcrPull para obtener imagenes.
  # Se permite admin para el primer deploy via 'az acr build' desde CI.
  admin_enabled = false

  tags = var.tags
}

# RBAC: AcrPull para que el Container App pueda descargar imagenes
resource "azurerm_role_assignment" "acr_pull" {
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPull"
  principal_id         = azurerm_user_assigned_identity.main.principal_id
}

# RBAC: AcrPush para que el SP de OIDC (GitHub Actions) pueda subir imagenes
# El SP hereda Contributor en el RG desde el modulo OIDC, pero AcrPush
# es mas limpio semanticamente para este modulo.
resource "azurerm_role_assignment" "acr_push" {
  scope                = azurerm_container_registry.main.id
  role_definition_name = "AcrPush"
  principal_id         = var.oidc_principal_id
}

# ---------------------------------------------------------------
# Container Apps Environment
# Entorno de ejecucion compartido para todos los Container Apps.
# infrastructure_subnet_id: VNet Integration via snet-containerapp (/23).
# Necesita /23 minimo para la infraestructura interna de Azure.
# ---------------------------------------------------------------
resource "azurerm_container_app_environment" "main" {
  name                           = "cae-${var.prefix}-${var.environment}"
  resource_group_name            = var.resource_group_name
  location                       = var.location
  log_analytics_workspace_id     = var.log_analytics_workspace_id
  infrastructure_subnet_id       = var.containerapp_subnet_id
  internal_load_balancer_enabled = false
  tags                           = var.tags
}

# ---------------------------------------------------------------
# Container App (.NET 8 API)
#
# secret blocks con key_vault_secret_id:
#   Container Apps resuelve los secretos directamente desde Key Vault
#   usando la Managed Identity. Los valores se inyectan como env vars
#   en el contenedor — el codigo .NET los lee via IConfiguration igual
#   que si estuvieran en appsettings.json.
#
# registry block con identity:
#   Container Apps usa AcrPull de la Managed Identity para descargar
#   la imagen. No se necesita username/password ni admin account.
#
# image placeholder:
#   'mcr.microsoft.com/dotnet/samples:aspnetapp' es la imagen inicial.
#   El CI la reemplaza con la imagen real via 'az containerapp update'.
# ---------------------------------------------------------------
resource "azurerm_container_app" "main" {
  name                         = "ca-${var.prefix}-${var.environment}"
  container_app_environment_id = azurerm_container_app_environment.main.id
  resource_group_name          = var.resource_group_name
  revision_mode                = "Single"

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.main.id]
  }

  registry {
    server   = azurerm_container_registry.main.login_server
    identity = azurerm_user_assigned_identity.main.id
  }

  # Secretos resueltos desde Key Vault usando la Managed Identity
  secret {
    name                = "postgresql-connection-string"
    key_vault_secret_id = "${var.key_vault_uri}secrets/postgresql-connection-string"
    identity            = azurerm_user_assigned_identity.main.id
  }

  secret {
    name                = "jwt-secret-key"
    key_vault_secret_id = "${var.key_vault_uri}secrets/jwt-secret-key"
    identity            = azurerm_user_assigned_identity.main.id
  }

  secret {
    name                = "app-insights-connection-string"
    key_vault_secret_id = "${var.key_vault_uri}secrets/app-insights-connection-string"
    identity            = azurerm_user_assigned_identity.main.id
  }

  template {
    container {
      name  = "backend"
      image = "mcr.microsoft.com/dotnet/samples:aspnetapp"
      cpu    = 0.25
      memory = "0.5Gi"

      env {
        name  = "ASPNETCORE_ENVIRONMENT"
        value = var.environment == "prod" ? "Production" : "Development"
      }

      env {
        name        = "APPLICATIONINSIGHTS_CONNECTION_STRING"
        secret_name = "app-insights-connection-string"
      }

      env {
        name        = "ConnectionStrings__ConnectionString"
        secret_name = "postgresql-connection-string"
      }

      env {
        name        = "Auth__secretKey"
        secret_name = "jwt-secret-key"
      }
    }

    min_replicas = 0
    max_replicas = 2
  }

  ingress {
    external_enabled = true
    target_port      = 8080
    transport        = "http"

    traffic_weight {
      latest_revision = true
      percentage      = 100
    }
  }

  tags = var.tags

  depends_on = [
    azurerm_role_assignment.kv_secrets_user,
    azurerm_role_assignment.acr_pull,
  ]
}

# ---------------------------------------------------------------
# RBAC: Key Vault Secrets User
# Permite a la Managed Identity LEER secretos del Key Vault.
# Necesario para resolver los secret blocks del Container App.
# ---------------------------------------------------------------
resource "azurerm_role_assignment" "kv_secrets_user" {
  scope                = var.key_vault_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.main.principal_id
}

# ---------------------------------------------------------------
# RBAC: Storage Blob Data Contributor
# Permite a la Managed Identity leer y escribir blobs.
# El backend sube imagenes al container product-images.
# ---------------------------------------------------------------
resource "azurerm_role_assignment" "storage_blob_contributor" {
  scope                = var.storage_account_id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.main.principal_id
}
