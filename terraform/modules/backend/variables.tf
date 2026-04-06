variable "resource_group_name" {
  description = "Nombre del resource group donde se creara el App Service."
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

variable "appservice_subnet_id" {
  description = "ID de la subnet snet-appservice para VNet Integration."
  type        = string
}

variable "key_vault_id" {
  description = "Resource ID del Key Vault. Usado para asignar el rol Key Vault Secrets User a la Managed Identity."
  type        = string
}

variable "key_vault_uri" {
  description = "URI del Key Vault (ej: https://kv-name.vault.azure.net/). Usado para construir las referencias @Microsoft.KeyVault(...) en app_settings."
  type        = string
}

variable "storage_account_id" {
  description = "Resource ID del Storage Account. Usado para asignar el rol Storage Blob Data Contributor."
  type        = string
}

variable "sku_name" {
  description = "SKU del App Service Plan. B1 para dev, S2 o P1v3 para produccion."
  type        = string
  default     = "B1"
}
