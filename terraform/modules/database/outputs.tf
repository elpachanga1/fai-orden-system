output "server_id" {
  description = "Resource ID del servidor PostgreSQL Flexible Server."
  value       = azurerm_postgresql_flexible_server.main.id
}

output "server_name" {
  description = "Nombre del servidor PostgreSQL."
  value       = azurerm_postgresql_flexible_server.main.name
}

output "server_fqdn" {
  description = "FQDN del servidor (ej: srv-carrito-dev.postgres.database.azure.com). Usado para construir la connection string."
  value       = azurerm_postgresql_flexible_server.main.fqdn
}

output "database_name" {
  description = "Nombre de la base de datos creada."
  value       = azurerm_postgresql_flexible_server_database.main.name
}
