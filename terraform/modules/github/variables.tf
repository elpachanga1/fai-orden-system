variable "github_org" {
  description = "Nombre del usuario u organizacion de GitHub."
  type        = string
}

variable "github_repo" {
  description = "Nombre del repositorio de GitHub."
  type        = string
}

variable "azure_client_id" {
  description = "Client ID del App Registration creado por el modulo oidc."
  type        = string
  sensitive   = true
}

variable "azure_tenant_id" {
  description = "Tenant ID de Azure AD."
  type        = string
  sensitive   = true
}

variable "azure_subscription_id" {
  description = "Subscription ID de Azure."
  type        = string
  sensitive   = true
}

variable "static_web_app_api_key" {
  description = "API key del Static Web App para deploy desde GitHub Actions."
  type        = string
  sensitive   = true
}

variable "backend_hostname" {
  description = "FQDN del Container App (no sensible — se usa como variable de entorno en el frontend)."
  type        = string
}

variable "acr_login_server" {
  description = "Login server del Azure Container Registry (ej: acrcarritodev.azurecr.io). Usado por CI para docker push."
  type        = string
}

variable "container_app_name" {
  description = "Nombre del Container App. Usado por CI para az containerapp update."
  type        = string
}

variable "resource_group" {
  description = "Nombre del resource group principal. Usado por CI para az containerapp update."
  type        = string
}

variable "tf_state_storage_account" {
  description = "Nombre del Storage Account del remote state. Se guarda como secret TF_STATE_STORAGE_ACCOUNT en GitHub para que terraform-cd.yml pueda generar el backend.conf."
  type        = string
}
