variable "resource_group_name" {
  description = "Nombre del resource group donde se creara el servidor PostgreSQL."
  type        = string
}

variable "location" {
  description = "Region de Azure."
  type        = string
}

variable "prefix" {
  description = "Prefijo para los nombres de los recursos."
  type        = string
}

variable "environment" {
  description = "Entorno (dev, staging, prod)."
  type        = string
}

variable "tags" {
  description = "Tags aplicados a todos los recursos del modulo."
  type        = map(string)
  default     = {}
}

variable "database_subnet_id" {
  description = "ID de la subnet delegada a Microsoft.DBforPostgreSQL/flexibleServers."
  type        = string
}

variable "private_dns_zone_id" {
  description = "ID de la Private DNS Zone 'privatelink.postgres.database.azure.com'."
  type        = string
}

variable "administrator_login" {
  description = "Nombre de usuario administrador del servidor PostgreSQL. No puede ser 'postgres' (reservado por Azure)."
  type        = string
  default     = "admincarrito"
}

variable "administrator_password" {
  description = "Contrasena del administrador. Minimo 8 caracteres, mayusculas, minusculas, numeros y simbolos."
  type        = string
  sensitive   = true
}

variable "postgresql_version" {
  description = "Version de PostgreSQL."
  type        = string
  default     = "16"
}

variable "sku_name" {
  description = "SKU del servidor. B_Standard_B1ms = Burstable (dev). D2s_v3 = General Purpose (prod)."
  type        = string
  default     = "B_Standard_B1ms"
}

variable "storage_mb" {
  description = "Almacenamiento en MB. Minimo 32768 (32GB)."
  type        = number
  default     = 32768
}

variable "database_name" {
  description = "Nombre de la base de datos a crear dentro del servidor."
  type        = string
  default     = "carrito_compras"
}
