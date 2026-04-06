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

# ---------------------------------------------------------------
# Variables sensibles — pasar siempre como TF_VAR_* o en
# terraform.tfvars (que esta en .gitignore, nunca se commitea)
# ---------------------------------------------------------------

variable "postgresql_admin_username" {
  description = "Usuario administrador de PostgreSQL. No puede ser 'postgres' (reservado por Azure Flexible Server)."
  type        = string
  default     = "admincarrito"
}

variable "postgresql_admin_password" {
  description = "Contrasena del administrador de PostgreSQL. Minimo 8 caracteres con mayusculas, minusculas, numeros y simbolos."
  type        = string
  sensitive   = true
}

variable "jwt_secret_key" {
  description = "Clave secreta para firmar tokens JWT. Minimo 32 caracteres. Generar con: openssl rand -base64 32"
  type        = string
  sensitive   = true
}

variable "github_org" {
  description = "Nombre del usuario u organizacion de GitHub (ej: elpachanga1)."
  type        = string
  default     = "elpachanga1"
}

variable "github_repo" {
  description = "Nombre del repositorio de GitHub (ej: fai-orden-system)."
  type        = string
  default     = "fai-orden-system"
}

variable "github_branch" {
  description = "Rama principal del proyecto. Solo esta rama puede hacer terraform apply via OIDC."
  type        = string
  default     = "main"
}

variable "tf_state_storage_account" {
  description = "Nombre del Storage Account donde vive el remote state (creado por bootstrap-remote-state.ps1). Se usa para generar el secret TF_STATE_STORAGE_ACCOUNT en GitHub Actions."
  type        = string
}
