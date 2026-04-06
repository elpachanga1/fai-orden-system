variable "resource_group_name" {
  description = "Nombre del resource group donde se crearan los recursos de monitoring."
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

variable "log_retention_days" {
  description = "Dias de retencion de logs en Log Analytics Workspace."
  type        = number
  default     = 30
}
