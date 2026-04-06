variable "resource_group_id" {
  description = "Resource ID del resource group principal. Se usa como scope del rol Contributor para el SP."
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

variable "github_org" {
  description = "Nombre del usuario u organizacion de GitHub (ej: elpachanga1)."
  type        = string
}

variable "github_repo" {
  description = "Nombre del repositorio de GitHub (ej: fai-orden-system)."
  type        = string
}

variable "github_branch" {
  description = "Rama desde la que se permite hacer terraform apply. Solo commits en esta rama pueden asumir el rol."
  type        = string
  default     = "main"
}
