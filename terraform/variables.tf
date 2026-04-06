variable "location" {
  description = "Region de Azure donde se desplegaran todos los recursos."
  type        = string
  default     = "eastus2"
}

variable "prefix" {
  description = "Prefijo corto usado en los nombres de todos los recursos (ej: 'carrito')."
  type        = string
  default     = "carrito"

  validation {
    condition     = length(var.prefix) <= 10 && can(regex("^[a-z0-9]+$", var.prefix))
    error_message = "El prefix debe tener maximo 10 caracteres, minusculas y numeros unicamente."
  }
}

variable "environment" {
  description = "Entorno de despliegue."
  type        = string
  default     = "dev"

  validation {
    condition     = contains(["dev", "staging", "prod"], var.environment)
    error_message = "El environment debe ser 'dev', 'staging' o 'prod'."
  }
}

variable "tags" {
  description = "Tags adicionales que se aplicaran a todos los recursos."
  type        = map(string)
  default     = {}
}
