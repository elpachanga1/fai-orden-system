output "resource_group_name" {
  description = "Nombre del resource group principal."
  value       = azurerm_resource_group.main.name
}

output "resource_group_id" {
  description = "ID del resource group principal."
  value       = azurerm_resource_group.main.id
}

output "resource_group_location" {
  description = "Region del resource group principal."
  value       = azurerm_resource_group.main.location
}

output "networking" {
  description = "Outputs del modulo de networking (IDs de subnets, VNet, DNS zones)."
  value = {
    vnet_id                      = module.networking.vnet_id
    vnet_name                    = module.networking.vnet_name
    apim_subnet_id               = module.networking.apim_subnet_id
    appservice_subnet_id         = module.networking.appservice_subnet_id
    private_endpoints_subnet_id  = module.networking.private_endpoints_subnet_id
    database_subnet_id           = module.networking.database_subnet_id
    postgres_private_dns_zone_id = module.networking.postgres_private_dns_zone_id
    blob_private_dns_zone_id     = module.networking.blob_private_dns_zone_id
    keyvault_private_dns_zone_id = module.networking.keyvault_private_dns_zone_id
  }
}

output "key_vault_name" {
  description = "Nombre del Key Vault."
  value       = module.keyvault.key_vault_name
}

output "key_vault_uri" {
  description = "URI del Key Vault (usado para referenciar secretos desde App Service)."
  value       = module.keyvault.key_vault_uri
}

output "key_vault_id" {
  description = "Resource ID del Key Vault."
  value       = module.keyvault.key_vault_id
}

output "application_insights_connection_string" {
  description = "Connection string de Application Insights (ya guardado en Key Vault)."
  value       = module.monitoring.application_insights_connection_string
  sensitive   = true
}

output "database" {
  description = "Outputs del servidor PostgreSQL."
  value = {
    server_name   = module.database.server_name
    server_fqdn   = module.database.server_fqdn
    database_name = module.database.database_name
  }
}

output "storage" {
  description = "Outputs del Storage Account."
  value = {
    account_name          = module.storage.storage_account_name
    container_name        = module.storage.container_name
    primary_blob_endpoint = module.storage.primary_blob_endpoint
  }
}

output "backend" {
  description = "Outputs del App Service."
  value = {
    web_app_name          = module.backend.web_app_name
    hostname              = module.backend.web_app_default_hostname
    managed_identity_id   = module.backend.managed_identity_id
    managed_identity_client_id = module.backend.managed_identity_client_id
  }
}

output "frontend" {
  description = "Outputs del Static Web App."
  value = {
    name              = module.frontend.static_web_app_name
    default_host_name = module.frontend.default_host_name
  }
}

output "frontend_api_key" {
  description = "API key del Static Web App para GitHub Actions (AZURE_STATIC_WEB_APPS_API_TOKEN)."
  value       = module.frontend.api_key
  sensitive   = true
}

output "github_actions_secrets" {
  description = "Nombres de los secrets creados en el repositorio de GitHub."
  value       = module.github.secrets_created
}

output "oidc_client_id" {
  description = "Client ID del App Registration de GitHub Actions. Tambien disponible como secret AZURE_CLIENT_ID en el repo."
  value       = module.oidc.client_id
}

