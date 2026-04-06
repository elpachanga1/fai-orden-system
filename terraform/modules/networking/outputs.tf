output "vnet_id" {
  description = "Resource ID de la Virtual Network."
  value       = azurerm_virtual_network.main.id
}

output "vnet_name" {
  description = "Nombre de la Virtual Network."
  value       = azurerm_virtual_network.main.name
}

output "apim_subnet_id" {
  description = "ID de la subnet reservada para API Management."
  value       = azurerm_subnet.apim.id
}

output "containerapp_subnet_id" {
  description = "ID de la subnet delegada al Container Apps Environment."
  value       = azurerm_subnet.containerapp.id
}

output "private_endpoints_subnet_id" {
  description = "ID de la subnet donde se crean los Private Endpoints."
  value       = azurerm_subnet.private_endpoints.id
}

output "database_subnet_id" {
  description = "ID de la subnet delegada a PostgreSQL Flexible Server."
  value       = azurerm_subnet.database.id
}

output "postgres_private_dns_zone_id" {
  description = "ID de la Private DNS Zone para PostgreSQL Flexible Server."
  value       = azurerm_private_dns_zone.postgres.id
}

output "blob_private_dns_zone_id" {
  description = "ID de la Private DNS Zone para Azure Blob Storage."
  value       = azurerm_private_dns_zone.blob.id
}

output "keyvault_private_dns_zone_id" {
  description = "ID de la Private DNS Zone para Azure Key Vault."
  value       = azurerm_private_dns_zone.keyvault.id
}
