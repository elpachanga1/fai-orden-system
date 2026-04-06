output "storage_account_id" {
  description = "Resource ID del Storage Account."
  value       = azurerm_storage_account.main.id
}

output "storage_account_name" {
  description = "Nombre del Storage Account."
  value       = azurerm_storage_account.main.name
}

output "primary_blob_endpoint" {
  description = "Endpoint publico del blob (URL base para acceder a los containers via SDK)."
  value       = azurerm_storage_account.main.primary_blob_endpoint
}

output "container_name" {
  description = "Nombre del container de imagenes."
  value       = azurerm_storage_container.product_images.name
}
