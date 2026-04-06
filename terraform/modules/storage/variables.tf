variable "resource_group_name" {
  description = "Nombre del resource group donde se creara el Storage Account."
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

variable "container_name" {
  description = "Nombre del container de blob donde se guardan las imagenes de productos."
  type        = string
  default     = "product-images"
}
