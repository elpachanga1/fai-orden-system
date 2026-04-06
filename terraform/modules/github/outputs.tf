output "secrets_created" {
  description = "Lista de nombres de secrets creados en el repositorio de GitHub."
  value       = [for s in github_actions_secret.secrets : s.secret_name]
}
