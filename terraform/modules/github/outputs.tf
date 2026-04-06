output "variables_created" {
  description = "Lista de nombres de variables creadas en el repositorio de GitHub."
  value       = [for v in github_actions_variable.vars : v.variable_name]
}
