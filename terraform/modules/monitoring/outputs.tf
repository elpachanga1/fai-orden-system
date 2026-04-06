output "workspace_id" {
  description = "Resource ID del Log Analytics Workspace."
  value       = azurerm_log_analytics_workspace.main.id
}

output "workspace_name" {
  description = "Nombre del Log Analytics Workspace."
  value       = azurerm_log_analytics_workspace.main.name
}

output "application_insights_id" {
  description = "Resource ID del componente de Application Insights."
  value       = azurerm_application_insights.main.id
}

output "application_insights_name" {
  description = "Nombre del recurso de Application Insights."
  value       = azurerm_application_insights.main.name
}

output "application_insights_connection_string" {
  description = "Connection string de Application Insights. Sensible — se almacena en Key Vault."
  value       = azurerm_application_insights.main.connection_string
  sensitive   = true
}

output "instrumentation_key" {
  description = "Instrumentation key de Application Insights (legacy, preferir connection_string)."
  value       = azurerm_application_insights.main.instrumentation_key
  sensitive   = true
}
