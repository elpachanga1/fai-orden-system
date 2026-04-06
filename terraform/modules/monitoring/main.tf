# ---------------------------------------------------------------
# Log Analytics Workspace
# Prerequisito de Application Insights en modo workspace-based.
# Centraliza logs del App Service, PostgreSQL y la aplicacion.
# ---------------------------------------------------------------
resource "azurerm_log_analytics_workspace" "main" {
  name                = "law-${var.prefix}-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  sku                 = "PerGB2018"
  retention_in_days   = var.log_retention_days
  tags                = var.tags
}

# ---------------------------------------------------------------
# Application Insights (workspace-based)
# El backend ya tiene ApplicationInsights.AspNetCore instalado.
# Este recurso provee el endpoint al que el SDK envia telemetria.
# La connection_string se guarda en Key Vault y se inyecta
# al App Service como variable de entorno en la Fase 7.
# ---------------------------------------------------------------
resource "azurerm_application_insights" "main" {
  name                = "ai-${var.prefix}-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  workspace_id        = azurerm_log_analytics_workspace.main.id
  application_type    = "web"
  tags                = var.tags
}
