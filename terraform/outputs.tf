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
