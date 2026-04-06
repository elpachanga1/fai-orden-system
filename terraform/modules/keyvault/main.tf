# ---------------------------------------------------------------
# Identidad del cliente Terraform
# Obtiene el tenant_id y object_id de quien esta ejecutando
# "terraform apply" — necesario para otorgar acceso al Key Vault.
# ---------------------------------------------------------------
data "azurerm_client_config" "current" {}

# ---------------------------------------------------------------
# Sufijo aleatorio para el nombre del Key Vault
# Los nombres de Key Vault son globalmente unicos en Azure.
# Con soft-delete activo, el nombre queda reservado hasta 90 dias
# despues de eliminarlo — el sufijo previene colisiones.
# ---------------------------------------------------------------
resource "random_string" "kv_suffix" {
  length  = 6
  special = false
  upper   = false
  numeric = true
}

# ---------------------------------------------------------------
# Key Vault
# Almacena los secretos de la aplicacion:
#   - Connection string de PostgreSQL
#   - JWT secret key
#   - Application Insights connection string
#
# enable_rbac_authorization = true:
#   Usa roles de Azure (RBAC) en vez del modelo de Access Policies
#   legacy. RBAC es mas granular, auditable y consistente con el
#   resto del ecosistema Azure.
#
# network_acls default_action = "Allow":
#   En dev, se permite acceso desde cualquier IP para que
#   Terraform pueda crear secretos desde la maquina local.
#   En produccion, cambiar a "Deny" y usar solo Private Endpoint.
# ---------------------------------------------------------------
resource "azurerm_key_vault" "main" {
  name                = "kv-${var.prefix}-${var.environment}-${random_string.kv_suffix.result}"
  resource_group_name = var.resource_group_name
  location            = var.location
  sku_name            = "standard"
  tenant_id           = data.azurerm_client_config.current.tenant_id

  rbac_authorization_enabled = true
  soft_delete_retention_days = var.soft_delete_retention_days
  purge_protection_enabled   = var.purge_protection_enabled

  # En dev: Allow para poder crear secretos desde la maquina local durante terraform apply.
  # En prod: cambiar default_action a "Deny" — solo el Private Endpoint tendra acceso.
  network_acls {
    default_action = "Allow"
    bypass         = "AzureServices"
  }

  tags = var.tags
}

# ---------------------------------------------------------------
# RBAC: Key Vault Secrets Officer para Terraform
# Otorga al SP/identidad que ejecuta Terraform permiso para
# crear, leer y actualizar secretos durante el apply.
# Sin esto, "terraform apply" falla al intentar crear los secretos.
# ---------------------------------------------------------------
resource "azurerm_role_assignment" "terraform_secrets_officer" {
  scope                = azurerm_key_vault.main.id
  role_definition_name = "Key Vault Secrets Officer"
  principal_id         = data.azurerm_client_config.current.object_id
}

# ---------------------------------------------------------------
# Secretos
# Los valores de postgresql-connection-string y jwt-secret-key
# son placeholders — se actualizaran en fases posteriores
# cuando los recursos reales esten creados.
# app-insights-connection-string se toma del modulo monitoring.
# ---------------------------------------------------------------

resource "azurerm_key_vault_secret" "postgresql_connection_string" {
  name         = "postgresql-connection-string"
  value        = var.postgresql_connection_string
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [azurerm_role_assignment.terraform_secrets_officer]
}

resource "azurerm_key_vault_secret" "jwt_secret_key" {
  name         = "jwt-secret-key"
  value        = var.jwt_secret_key
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [azurerm_role_assignment.terraform_secrets_officer]
}

resource "azurerm_key_vault_secret" "app_insights_connection_string" {
  name         = "app-insights-connection-string"
  value        = var.app_insights_connection_string
  key_vault_id = azurerm_key_vault.main.id

  depends_on = [azurerm_role_assignment.terraform_secrets_officer]
}

# ---------------------------------------------------------------
# Private Endpoint del Key Vault
# ---------------------------------------------------------------
resource "azurerm_private_endpoint" "keyvault" {
  name                = "pe-kv-${var.prefix}-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location
  subnet_id           = var.private_endpoint_subnet_id
  tags                = var.tags

  private_service_connection {
    name                           = "psc-kv-${var.prefix}-${var.environment}"
    private_connection_resource_id = azurerm_key_vault.main.id
    subresource_names              = ["vault"]
    is_manual_connection           = false
  }

  private_dns_zone_group {
    name                 = "dns-group-kv"
    private_dns_zone_ids = [var.private_dns_zone_id]
  }
}
