locals {
  common_tags = merge(var.tags, {
    project     = "carrito-compras"
    environment = var.environment
    managed_by  = "terraform"
  })
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
# Modulo: Networking
# VNet, subnets, NSGs y Private DNS Zones.
# Este modulo no depende de monitoring — se puede crear en paralelo.
# Sus outputs (subnet IDs, DNS zone IDs) son prerequisito para
# los modulos de keyvault, database, storage y backend.
# ---------------------------------------------------------------
module "networking" {
  source = "./modules/networking"

  resource_group_name = azurerm_resource_group.main.name
  location            = azurerm_resource_group.main.location
  prefix              = var.prefix
  environment         = var.environment
  tags                = local.common_tags
}

# ---------------------------------------------------------------
# Modulo: Key Vault
# Almacena todos los secretos de la aplicacion.
# Depende de monitoring (para guardar la connection string de AI)
# y de networking (para el Private Endpoint y la DNS zone).
# ---------------------------------------------------------------
module "keyvault" {
  source = "./modules/keyvault"

  resource_group_name            = azurerm_resource_group.main.name
  location                       = azurerm_resource_group.main.location
  prefix                         = var.prefix
  environment                    = var.environment
  tags                           = local.common_tags
  private_endpoint_subnet_id     = module.networking.private_endpoints_subnet_id
  private_dns_zone_id            = module.networking.keyvault_private_dns_zone_id
  app_insights_connection_string = module.monitoring.application_insights_connection_string
}
