data "azurerm_client_config" "current" {}

# ---------------------------------------------------------------
# App Registration en Azure AD
#
# Este es el "identity" de GitHub Actions frente a Azure.
# No tiene client secret — usa OIDC (Federated Identity Credential)
# para autenticarse sin secretos que rotar ni expiran.
#
# OIDC funciona asi:
#  1. GitHub Actions genera un JWT firmado por GitHub
#  2. Azure AD verifica que el JWT venga del repo/branch correcto
#  3. Azure AD emite un access token con los permisos del SP
#
# El workflow en GitHub Actions usara:
#   uses: azure/login@v2
#   with:
#     client-id: ${{ secrets.AZURE_CLIENT_ID }}
#     tenant-id: ${{ secrets.AZURE_TENANT_ID }}
#     subscription-id: ${{ secrets.AZURE_SUBSCRIPTION_ID }}
# ---------------------------------------------------------------
resource "azuread_application" "github_actions" {
  display_name = "sp-${var.prefix}-${var.environment}-github-actions"
}

resource "azuread_service_principal" "github_actions" {
  client_id = azuread_application.github_actions.client_id
}

# ---------------------------------------------------------------
# Federated Identity Credential — rama main (apply)
#
# Subject claim: repo:<org>/<repo>:ref:refs/heads/<branch>
# Solo los workflows que corren desde esta rama
# pueden obtener un token con este SP.
#
# Para pull requests (plan) se podria agregar otro credential con:
#   subject = "repo:${var.github_org}/${var.github_repo}:pull_request"
# ---------------------------------------------------------------
resource "azuread_application_federated_identity_credential" "main_branch" {
  application_id = azuread_application.github_actions.id
  display_name   = "github-${var.github_branch}-branch"
  description    = "Permite a GitHub Actions en ${var.github_org}/${var.github_repo} (branch: ${var.github_branch}) autenticarse en Azure via OIDC."

  audiences = ["api://AzureADTokenExchange"]
  issuer    = "https://token.actions.githubusercontent.com"
  subject   = "repo:${var.github_org}/${var.github_repo}:ref:refs/heads/${var.github_branch}"
}

# Federated credential adicional para pull_request (terraform plan en PRs)
resource "azuread_application_federated_identity_credential" "pull_request" {
  application_id = azuread_application.github_actions.id
  display_name   = "github-pull-request"
  description    = "Permite a GitHub Actions en PRs de ${var.github_org}/${var.github_repo} correr terraform plan."

  audiences = ["api://AzureADTokenExchange"]
  issuer    = "https://token.actions.githubusercontent.com"
  subject   = "repo:${var.github_org}/${var.github_repo}:pull_request"
}

# ---------------------------------------------------------------
# RBAC: Contributor en el resource group principal
#
# El SP solo tiene Contributor sobre el RG del proyecto (no sobre
# toda la subscription). Esto es minimo privilegio:
# - Puede crear/modificar recursos dentro del RG
# - No puede crear RGs, asignar roles a nivel subscription,
#   ni tocar recursos fuera de este RG
#
# EXCEPCION: Los role assignments (azurerm_role_assignment) dentro
# del RG requieren User Access Administrator ademas de Contributor.
# Si terraform apply falla con "AuthorizationFailed" al crear
# los RBAC del modulo backend, agregar este segundo rol:
#
#   resource "azurerm_role_assignment" "uaa" {
#     scope                = var.resource_group_id
#     role_definition_name = "User Access Administrator"
#     principal_id         = azuread_service_principal.github_actions.object_id
#   }
# ---------------------------------------------------------------
resource "azurerm_role_assignment" "contributor" {
  scope                = var.resource_group_id
  role_definition_name = "Contributor"
  principal_id         = azuread_service_principal.github_actions.object_id
}

# Necesario para que el SP pueda asignar roles (modulo backend crea
# azurerm_role_assignment para la Managed Identity del App Service)
resource "azurerm_role_assignment" "user_access_administrator" {
  scope                = var.resource_group_id
  role_definition_name = "User Access Administrator"
  principal_id         = azuread_service_principal.github_actions.object_id
}
