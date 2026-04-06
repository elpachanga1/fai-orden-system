# ---------------------------------------------------------------
# Sufijo aleatorio para el nombre del Storage Account
# Los nombres son globalmente unicos (3-24 chars, solo alfanumerico)
# ---------------------------------------------------------------
resource "random_string" "storage_suffix" {
  length  = 6
  special = false
  upper   = false
  numeric = true
}

# ---------------------------------------------------------------
# Storage Account
# Almacena imagenes de productos subidas por los administradores.
#
# allow_nested_items_to_be_public = false:
#   Ninguna imagen es accesible públicamente sin un SAS token.
#   El App Service accede via Managed Identity (Storage Blob Data Contributor).
#
# En produccion considerar:
# - account_replication_type = "GRS" (geo-redundante)
# - Un CDN frente al blob para servir imagenes al frontend
# ---------------------------------------------------------------
resource "azurerm_storage_account" "main" {
  name                = "st${var.prefix}${var.environment}${random_string.storage_suffix.result}"
  resource_group_name = var.resource_group_name
  location            = var.location

  account_tier             = "Standard"
  account_replication_type = "LRS"
  account_kind             = "StorageV2"

  # Ningún blob es accesible publicamente — el acceso es via Managed Identity + RBAC
  allow_nested_items_to_be_public = false
  min_tls_version                 = "TLS1_2"

  tags = var.tags
}

# ---------------------------------------------------------------
# Container para imagenes de productos
# access_type = "private": requiere autenticacion para acceder.
# El App Service usara su Managed Identity con rol
# Storage Blob Data Contributor (asignado en el modulo backend).
# ---------------------------------------------------------------
resource "azurerm_storage_container" "product_images" {
  name                  = var.container_name
  storage_account_id    = azurerm_storage_account.main.id
  container_access_type = "private"
}


