output "client_id" {
  description = "Client ID (Application ID) del App Registration. Va al secret AZURE_CLIENT_ID en GitHub."
  value       = azuread_application.github_actions.client_id
}

output "tenant_id" {
  description = "Tenant ID de Azure AD. Va al secret AZURE_TENANT_ID en GitHub."
  value       = data.azurerm_client_config.current.tenant_id
}

output "subscription_id" {
  description = "Subscription ID. Va al secret AZURE_SUBSCRIPTION_ID en GitHub."
  value       = data.azurerm_client_config.current.subscription_id
}

output "service_principal_object_id" {
  description = "Object ID del Service Principal (Enterprise Application). Usado para futuras asignaciones RBAC."
  value       = azuread_service_principal.github_actions.object_id
}
