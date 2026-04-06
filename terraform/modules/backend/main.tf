# ---------------------------------------------------------------
# User Assigned Managed Identity
#
# Se usa User Assigned (no System Assigned) porque:
# - Tiene ciclo de vida independiente del App Service
# - Se puede pre-autorizar en Key Vault antes de que el App Service exista
# - Si se destruye y recrea el App Service, la identidad (y sus permisos) persisten
# ---------------------------------------------------------------
resource "azurerm_user_assigned_identity" "main" {
  name                = "id-${var.prefix}-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  tags                = var.tags
}

# ---------------------------------------------------------------
# App Service Plan
# Linux, SKU B1 (dev). En produccion: S2 o P1v3.
# P1v3 tiene mejor rendimiento de VNet Integration (routing dedicado).
# ---------------------------------------------------------------
resource "azurerm_service_plan" "main" {
  name                = "asp-${var.prefix}-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  os_type             = "Linux"
  sku_name            = var.sku_name
  tags                = var.tags
}

# ---------------------------------------------------------------
# App Service (.NET 8 API)
#
# VNet Integration (virtual_network_subnet_id):
#   El trafico SALIENTE del App Service va por snet-appservice.
#   Esto permite que el App Service alcance la DB, Blob y KV
#   via sus Private Endpoints (IPs privadas en la VNet).
#
# key_vault_reference_identity_id:
#   Indica a Azure qué Managed Identity usar para resolver las
#   referencias @Microsoft.KeyVault(...) en app_settings.
#   Necesario cuando se usa User Assigned Identity.
#
# app_settings con @Microsoft.KeyVault(SecretUri=...):
#   Azure App Service resuelve estas referencias en tiempo de arranque,
#   inyectando el valor del secreto como variable de entorno.
#   El codigo .NET los lee via IConfiguration igual que si estuvieran
#   en appsettings.json — sin cambios en el codigo.
# ---------------------------------------------------------------
resource "azurerm_linux_web_app" "main" {
  name                = "app-${var.prefix}-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  service_plan_id     = azurerm_service_plan.main.id

  # VNet Integration: trafico saliente por snet-appservice
  virtual_network_subnet_id = var.appservice_subnet_id

  # Resolucion de referencias Key Vault con la Managed Identity
  key_vault_reference_identity_id = azurerm_user_assigned_identity.main.id

  identity {
    type         = "UserAssigned"
    identity_ids = [azurerm_user_assigned_identity.main.id]
  }

  site_config {
    always_on = true # Disponible en B1+. Evita cold starts por inactividad.

    application_stack {
      dotnet_version = "8.0"
    }
  }

  app_settings = {
    # CORS: el frontend en produccion estara en staticwebapp.azure.net
    # En dev se puede dejar vacio y configurar via appsettings.json local
    "ASPNETCORE_ENVIRONMENT" = var.environment == "prod" ? "Production" : "Development"

    # Application Insights — el SDK ApplicationInsights.AspNetCore ya esta
    # instalado en el proyecto. Solo necesita esta variable para saber a donde enviar.
    "APPLICATIONINSIGHTS_CONNECTION_STRING" = "@Microsoft.KeyVault(SecretUri=${var.key_vault_uri}secrets/app-insights-connection-string/)"

    # Connection string de PostgreSQL — EF Core y Npgsql la leen desde
    # IConfiguration["ConnectionStrings:ConnectionString"] (appsettings.json)
    # El doble guion bajo (__) es el separador de secciones en .NET con env vars.
    "ConnectionStrings__ConnectionString" = "@Microsoft.KeyVault(SecretUri=${var.key_vault_uri}secrets/postgresql-connection-string/)"

    # JWT secret key — leida desde IConfiguration["Auth:secretKey"]
    "Auth__secretKey" = "@Microsoft.KeyVault(SecretUri=${var.key_vault_uri}secrets/jwt-secret-key/)"
  }

  tags = var.tags
}

# ---------------------------------------------------------------
# RBAC: Key Vault Secrets User
# Permite a la Managed Identity LEER secretos del Key Vault.
# (No puede crearlos ni eliminarlos — minimo privilegio)
# Sin este rol, las referencias @Microsoft.KeyVault(...) fallan
# con "Access denied" al arrancar el App Service.
# ---------------------------------------------------------------
resource "azurerm_role_assignment" "kv_secrets_user" {
  scope                = var.key_vault_id
  role_definition_name = "Key Vault Secrets User"
  principal_id         = azurerm_user_assigned_identity.main.principal_id
}

# ---------------------------------------------------------------
# RBAC: Storage Blob Data Contributor
# Permite a la Managed Identity leer y escribir blobs.
# El App Service sube imagenes al container product-images
# y las sirve via URL firmada (SAS) o directamente via SDK.
# ---------------------------------------------------------------
resource "azurerm_role_assignment" "storage_blob_contributor" {
  scope                = var.storage_account_id
  role_definition_name = "Storage Blob Data Contributor"
  principal_id         = azurerm_user_assigned_identity.main.principal_id
}
