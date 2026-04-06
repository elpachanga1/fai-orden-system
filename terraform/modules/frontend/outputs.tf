output "static_web_app_id" {
  description = "Resource ID del Static Web App."
  value       = azurerm_static_web_app.main.id
}

output "static_web_app_name" {
  description = "Nombre del Static Web App."
  value       = azurerm_static_web_app.main.name
}

output "default_host_name" {
  description = "URL publica del Static Web App (ej: gentle-sea-123.azurestaticapps.net)."
  value       = azurerm_static_web_app.main.default_host_name
}

output "api_key" {
  description = "API key para deploy desde CI/CD (GitHub Actions). Se guarda como GitHub Actions secret AZURE_STATIC_WEB_APPS_API_TOKEN."
  value       = azurerm_static_web_app.main.api_key
  sensitive   = true
}
