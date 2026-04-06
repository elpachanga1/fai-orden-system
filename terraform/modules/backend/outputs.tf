output "container_app_name" {
  description = "Nombre del Container App."
  value       = azurerm_container_app.main.name
}

output "container_app_fqdn" {
  description = "FQDN publico del Container App (ej: ca-carrito-dev.eastus2.azurecontainerapps.io)."
  value       = azurerm_container_app.main.latest_revision_fqdn
}

output "container_app_id" {
  description = "Resource ID del Container App."
  value       = azurerm_container_app.main.id
}

output "acr_login_server" {
  description = "Login server del Azure Container Registry (ej: acrcarritodev.azurecr.io)."
  value       = azurerm_container_registry.main.login_server
}

output "acr_name" {
  description = "Nombre del Azure Container Registry."
  value       = azurerm_container_registry.main.name
}

output "managed_identity_id" {
  description = "Resource ID de la User Assigned Managed Identity."
  value       = azurerm_user_assigned_identity.main.id
}

output "managed_identity_principal_id" {
  description = "Object ID (Principal ID) de la Managed Identity en Azure AD. Usado para RBAC."
  value       = azurerm_user_assigned_identity.main.principal_id
}

output "managed_identity_client_id" {
  description = "Client ID de la Managed Identity."
  value       = azurerm_user_assigned_identity.main.client_id
}
