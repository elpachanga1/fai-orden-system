variable "resource_group_name" {
  description = "Nombre del resource group donde se creara el Key Vault."
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

variable "private_endpoint_subnet_id" {
  description = "ID de la subnet donde se creara el Private Endpoint del Key Vault."
  type        = string
}

variable "private_dns_zone_id" {
  description = "ID de la Private DNS Zone 'privatelink.vaultcore.azure.net'."
  type        = string
}

variable "app_insights_connection_string" {
  description = "Connection string de Application Insights. Se guarda como secreto en Key Vault."
  type        = string
  sensitive   = true
}

variable "soft_delete_retention_days" {
  description = "Dias de retencion de soft delete. Minimo 7, maximo 90."
  type        = number
  default     = 7
}

variable "purge_protection_enabled" {
  description = "Habilita purge protection. En produccion debe ser true para prevenir borrado accidental."
  type        = bool
  default     = false
}
