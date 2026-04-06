locals {
  common_tags = merge(var.tags, {
    project     = "carrito-compras"
    environment = var.environment
    managed_by  = "terraform"
  })

  # Connection string en formato Npgsql — coincide con el formato que ya usa
  # la app en appsettings.json. SSL Mode=Require es obligatorio en Azure
  # PostgreSQL Flexible Server (TLS forzado).
  postgresql_connection_string = "Host=${module.database.server_fqdn};Port=5432;Database=${module.database.database_name};Username=${var.postgresql_admin_username};Password=${var.postgresql_admin_password};SSL Mode=Require;Minimum Pool Size=1;Maximum Pool Size=100;"
}

# ---------------------------------------------------------------
# Resource Group principal
# Contiene todos los recursos de la aplicacion.
# ---------------------------------------------------------------
resource "azurerm_resource_group" "main" {
  name     = "rg-${var.prefix}-${var.environment}"
  location = var.location
  tags     = local.common_tags
}

# ---------------------------------------------------------------
# Modulo: Monitoring
# Log Analytics Workspace + Application Insights.
# Se despliega primero porque otros modulos (keyvault) necesitan
# la connection string de App Insights como input.
# ---------------------------------------------------------------
module "monitoring" {
  source = "./modules/monitoring"

  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  prefix              = var.prefix
  environment         = var.environment
  tags                = local.common_tags
}

# ---------------------------------------------------------------
# Modulo: Key Vault
# Almacena todos los secretos de la aplicacion.
# Acceso publico (dev). En prod: agregar private endpoint + network_acls.
# ---------------------------------------------------------------
module "keyvault" {
  source = "./modules/keyvault"

  resource_group_name            = azurerm_resource_group.main.name
  location                       = azurerm_resource_group.main.location
  prefix                         = var.prefix
  environment                    = var.environment
  tags                           = local.common_tags
  app_insights_connection_string = module.monitoring.application_insights_connection_string
  postgresql_connection_string   = local.postgresql_connection_string
  jwt_secret_key                 = var.jwt_secret_key
}

# ---------------------------------------------------------------
# Modulo: Database
# PostgreSQL Flexible Server v16 con acceso publico (dev).
# En prod: agregar delegated_subnet_id + private_dns_zone_id.
# ---------------------------------------------------------------
module "database" {
  source = "./modules/database"

  resource_group_name    = azurerm_resource_group.main.name
  location               = azurerm_resource_group.main.location
  prefix                 = var.prefix
  environment            = var.environment
  tags                   = local.common_tags
  administrator_login    = var.postgresql_admin_username
  administrator_password = var.postgresql_admin_password
}

# ---------------------------------------------------------------
# Modulo: Storage
# Blob Storage para imagenes de productos.
# Acceso via Managed Identity del App Service (no account keys).
# ---------------------------------------------------------------
module "storage" {
  source = "./modules/storage"

  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  prefix              = var.prefix
  environment         = var.environment
  tags                = local.common_tags
}

# ---------------------------------------------------------------
# Modulo: Backend
# App Service Plan F1 (free) + App Service .NET 8.
# Secretos inyectados via Key Vault references.
# ---------------------------------------------------------------
module "backend" {
  source = "./modules/backend"

  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  prefix              = var.prefix
  environment         = var.environment
  tags                = local.common_tags
  key_vault_id        = module.keyvault.key_vault_id
  key_vault_uri       = module.keyvault.key_vault_uri
  storage_account_id  = module.storage.storage_account_id
}

# ---------------------------------------------------------------
# Modulo: Frontend (Fase 8)
# Static Web App (Free tier) para el React 18 + TypeScript.
# El deploy del bundle se hace via GitHub Actions con la api_key.
# ---------------------------------------------------------------
module "frontend" {
  source = "./modules/frontend"

  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  prefix              = var.prefix
  environment         = var.environment
  tags                = local.common_tags
}

# ---------------------------------------------------------------
# Modulo: OIDC (Fase 9)
# App Registration + Federated Identity Credential para GitHub Actions.
# Sin secrets que rotar: GitHub Actions se autentica con JWT firmado.
# Depende de: resource group (scope del rol Contributor).
# ---------------------------------------------------------------
module "oidc" {
  source = "./modules/oidc"

  resource_group_id = azurerm_resource_group.main.id
  prefix            = var.prefix
  environment       = var.environment
  github_org        = var.github_org
  github_repo       = var.github_repo
  github_branch     = var.github_branch
}

# ---------------------------------------------------------------
# Modulo: GitHub (Fase 10)
# Crea los 5 secrets que necesitan los workflows de GitHub Actions.
# Depende de: oidc (client_id), frontend (api_key), backend (hostname).
# ---------------------------------------------------------------
module "github" {
  source = "./modules/github"

  github_org               = var.github_org
  github_repo              = var.github_repo
  azure_client_id          = module.oidc.client_id
  azure_tenant_id          = module.oidc.tenant_id
  azure_subscription_id    = module.oidc.subscription_id
  static_web_app_api_key   = module.frontend.api_key
  backend_hostname         = module.backend.web_app_default_hostname
  tf_state_storage_account = var.tf_state_storage_account
}
