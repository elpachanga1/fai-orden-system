variable "resource_group_name" {
  description = "Nombre del resource group donde se creara el Static Web App."
  type        = string
}

variable "location" {
  description = "Region de Azure. Static Web App Free tier solo esta disponible en ciertas regiones."
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

variable "sku_tier" {
  description = "Tier del Static Web App. Free incluye CDN global. Standard agrega autenticacion personalizada y SLA."
  type        = string
  default     = "Free"

  validation {
    condition     = contains(["Free", "Standard"], var.sku_tier)
    error_message = "El sku_tier debe ser 'Free' o 'Standard'."
  }
}
