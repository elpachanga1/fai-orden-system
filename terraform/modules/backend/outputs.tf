output "web_app_name" {
  description = "Nombre del App Service."
  value       = azurerm_linux_web_app.main.name
}

output "web_app_default_hostname" {
  description = "Hostname por defecto del App Service (ej: app-carrito-dev.azurewebsites.net)."
  value       = azurerm_linux_web_app.main.default_hostname
}

output "web_app_id" {
  description = "Resource ID del App Service."
  value       = azurerm_linux_web_app.main.id
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
  description = "Client ID de la Managed Identity. Usado en referencias de Key Vault si hay multiples identidades."
  value       = azurerm_user_assigned_identity.main.client_id
}
