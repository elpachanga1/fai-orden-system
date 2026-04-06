# ---------------------------------------------------------------
# Static Web App
#
# El frontend React (CRA con React 18 + TypeScript) se despliega aqui.
#
# Por que Static Web App y no un Storage Account con hosting estatico:
# - CDN global incluido sin configuracion adicional
# - HTTPS automatico con certificado gestionado por Azure
# - Preview environments por branch (Standard tier)
# - API integrada (Azure Functions) si se necesita en el futuro
#
# Free tier incluye:
# - CDN global
# - 100GB/mes de transferencia
# - Dominio personalizado con SSL
# - CI/CD via GitHub Actions (usando api_key)
#
# El deploy del build de React se hace separado via GitHub Actions
# usando la api_key (output de este modulo → GitHub Actions secret).
# ---------------------------------------------------------------
resource "azurerm_static_web_app" "main" {
  name                = "swa-${var.prefix}-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  sku_tier            = var.sku_tier
  sku_size            = var.sku_tier # En azurerm, sku_size debe coincidir con sku_tier
  tags                = var.tags
}
