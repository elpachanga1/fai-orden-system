# ---------------------------------------------------------------
# PostgreSQL Flexible Server
#
# Se usa Flexible Server (no Single Server, deprecated en 2025) porque:
# - Soporta despliegue en subnet delegada (trafico privado dentro de la VNet)
# - Soporta Private DNS Zone para resolucion interna
# - La subnet snet-database esta delegada a Microsoft.DBforPostgreSQL/flexibleServers
#   por el modulo de networking — esta delegacion es obligatoria
#
# NOTA: En produccion considerar:
# - sku_name: "GP_Standard_D2s_v3" (General Purpose — SLA y IOPS garantizados)
# - high_availability.mode: "ZoneRedundant" (requiere zona primaria y standby)
# - geo_redundant_backup_enabled: true
# ---------------------------------------------------------------
resource "azurerm_postgresql_flexible_server" "main" {
  name                = "psql-${var.prefix}-${var.environment}"
  resource_group_name = var.resource_group_name
  location            = var.location

  version    = var.postgresql_version
  sku_name   = var.sku_name
  storage_mb = var.storage_mb

  administrator_login    = var.administrator_login
  administrator_password = var.administrator_password

  # La subnet DEBE estar delegada a Microsoft.DBforPostgreSQL/flexibleServers
  delegated_subnet_id = var.database_subnet_id

  # La Private DNS Zone resuelve el FQDN del servidor dentro de la VNet
  private_dns_zone_id = var.private_dns_zone_id

  # Con VNet integration activa el acceso publico debe deshabilitarse
  public_network_access_enabled = false

  # En dev no necesitamos backup retenido por mucho tiempo
  backup_retention_days        = 7
  geo_redundant_backup_enabled = false

  tags = var.tags

  # Azure asigna automaticamente una zona de disponibilidad al crear el servidor.
  # Ignorar cambios en zone para evitar el error:
  # "zone can only be changed when exchanged with standby_availability_zone"
  lifecycle {
    ignore_changes = [zone]
  }
}

# ---------------------------------------------------------------
# Base de datos: carrito_compras
# El servidor es el contenedor; la base de datos es el schema
# que EF Core usa para las migraciones y las tablas.
# ---------------------------------------------------------------
resource "azurerm_postgresql_flexible_server_database" "main" {
  name      = var.database_name
  server_id = azurerm_postgresql_flexible_server.main.id
  charset   = "UTF8"
  collation = "en_US.utf8"
}

# Permite conexiones desde servicios de Azure (App Service, GitHub Actions, etc.).
# 0.0.0.0 -> 0.0.0.0 es la convencion de Azure para "Allow all Azure services".
resource "azurerm_postgresql_flexible_server_firewall_rule" "azure_services" {
  name             = "AllowAzureServices"
  server_id        = azurerm_postgresql_flexible_server.main.id
  start_ip_address = "0.0.0.0"
  end_ip_address   = "0.0.0.0"
}
