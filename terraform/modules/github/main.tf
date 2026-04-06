data "github_repository" "main" {
  full_name = "${var.github_org}/${var.github_repo}"
}

# ---------------------------------------------------------------
# GitHub Actions Secrets
#
# Estos secrets estan cifrados con la public key del repositorio
# antes de enviarse via la API de GitHub — nunca viajan en texto plano.
#
# El provider github usa TF_VAR_github_token (o var.github_token
# heredado del root) para autenticarse en la API de GitHub.
#
# AZURE_CLIENT_ID, AZURE_TENANT_ID, AZURE_SUBSCRIPTION_ID:
#   Son los 3 valores necesarios para el paso azure/login@v2 con OIDC.
#   NO hay client_secret — la autenticacion es federada via JWT de GitHub.
#
# AZURE_STATIC_WEB_APPS_API_TOKEN:
#   Usado por el paso azure/static-web-apps-deploy@v1 para publicar
#   el build de React al Static Web App.
#
# REACT_APP_API_URL:
#   Variable de entorno que CRA inyecta en el bundle del frontend
#   durante el build (process.env.REACT_APP_API_URL).
#   Apunta al hostname del App Service.
# ---------------------------------------------------------------
locals {
  secrets = {
    # OIDC auth para azure/login@v2 y azure/webapps-deploy@v3
    "AZURE_CLIENT_ID"                   = var.azure_client_id
    "AZURE_TENANT_ID"                   = var.azure_tenant_id
    "AZURE_SUBSCRIPTION_ID"             = var.azure_subscription_id

    # Deploy del React al Static Web App
    "AZURE_STATIC_WEB_APPS_API_TOKEN"   = var.static_web_app_api_key

    # URL del backend — inyectada en el bundle de React durante el build
    "REACT_APP_API_URL"                 = "https://${var.backend_hostname}"

    # Nombre del Storage Account del remote state — usado en terraform-cd.yml
    # para generar backend.conf en el runner sin guardarlo en el repo
    "TF_STATE_STORAGE_ACCOUNT"          = var.tf_state_storage_account
  }
}

resource "github_actions_secret" "secrets" {
  for_each = local.secrets

  repository      = data.github_repository.main.name
  secret_name     = each.key
  plaintext_value = each.value
}
