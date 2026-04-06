variable "resource_group_name" {
  description = "Nombre del resource group donde se crearan los recursos de red."
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

variable "vnet_address_space" {
  description = "Espacio de direcciones de la VNet."
  type        = list(string)
  default     = ["10.0.0.0/16"]
}

variable "subnet_apim_prefix" {
  description = "CIDR de la subnet de APIM."
  type        = string
  default     = "10.0.0.0/24"
}

variable "subnet_containerapp_prefix" {
  description = "CIDR de la subnet del Container Apps Environment. Minimo /23 requerido por Azure."
  type        = string
  default     = "10.0.4.0/23"
}

variable "subnet_private_endpoints_prefix" {
  description = "CIDR de la subnet de Private Endpoints."
  type        = string
  default     = "10.0.2.0/24"
}

variable "subnet_database_prefix" {
  description = "CIDR de la subnet de la base de datos (delegada a PostgreSQL Flexible Server)."
  type        = string
  default     = "10.0.3.0/24"
}
