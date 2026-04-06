data "github_repository" "main" {
  full_name = "${var.github_org}/${var.github_repo}"
}

# ---------------------------------------------------------------
# GitHub Actions Secrets (cifrados por el provider)
#
# El provider integrations/github obtiene la public key del repo
# automáticamente y cifra con libsodium antes de enviar a la API.
#
# AZURE_CLIENT_ID / TENANT_ID / SUBSCRIPTION_ID:
#   Necesarios para azure/login@v2 con OIDC. No hay client_secret.
#
# AZURE_STATIC_WEB_APPS_API_TOKEN:
#   Token de deploy del Static Web App para azure/static-web-apps-deploy@v1.
# ---------------------------------------------------------------
locals {
  secrets = {
    "AZURE_CLIENT_ID"                 = var.azure_client_id
    "AZURE_TENANT_ID"                 = var.azure_tenant_id
    "AZURE_SUBSCRIPTION_ID"           = var.azure_subscription_id
    "AZURE_STATIC_WEB_APPS_API_TOKEN" = var.static_web_app_api_key
  }

  # Valores no sensibles — texto plano, visibles en la UI de GitHub.
  variables = {
    # URL del backend inyectada en el bundle de React durante el build
    "REACT_APP_API_URL"        = "https://${var.backend_hostname}"

    # Container Registry y Container App para el pipeline de CI del backend
    "AZURE_ACR_LOGIN_SERVER"   = var.acr_login_server
    "AZURE_CONTAINER_APP_NAME" = var.container_app_name
    "AZURE_RESOURCE_GROUP"     = var.resource_group

    # Nombre del Storage Account del remote state
    "TF_STATE_STORAGE_ACCOUNT" = var.tf_state_storage_account
  }
}

resource "github_actions_secret" "secrets" {
  for_each = local.secrets

  repository      = data.github_repository.main.name
  secret_name     = each.key
  plaintext_value = each.value
}

resource "github_actions_variable" "vars" {
  for_each = local.variables

  repository    = data.github_repository.main.name
  variable_name = each.key
  value         = each.value
}
