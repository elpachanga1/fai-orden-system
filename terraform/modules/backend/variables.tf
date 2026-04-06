variable "resource_group_name" {
  description = "Nombre del resource group donde se crearan los recursos del backend."
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

variable "containerapp_subnet_id" {
  description = "ID de la subnet snet-containerapp (/23) para el Container Apps Environment."
  type        = string
}

variable "log_analytics_workspace_id" {
  description = "Resource ID del Log Analytics Workspace. Requerido por el Container Apps Environment."
  type        = string
}

variable "key_vault_id" {
  description = "Resource ID del Key Vault. Usado para asignar el rol Key Vault Secrets User a la Managed Identity."
  type        = string
}

variable "key_vault_uri" {
  description = "URI del Key Vault (ej: https://kv-name.vault.azure.net/). Usado para construir los key_vault_secret_id de los Container App secrets."
  type        = string
}

variable "storage_account_id" {
  description = "Resource ID del Storage Account. Usado para asignar el rol Storage Blob Data Contributor."
  type        = string
}

variable "oidc_principal_id" {
  description = "Object ID del App Registration de GitHub Actions OIDC. Necesita AcrPush para subir imagenes desde CI."
  type        = string
}
