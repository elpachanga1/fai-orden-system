output "key_vault_id" {
  description = "Resource ID del Key Vault."
  value       = azurerm_key_vault.main.id
}

output "key_vault_uri" {
  description = "URI del Key Vault. Usado para construir referencias @Microsoft.KeyVault(SecretUri=...) en App Service."
  value       = azurerm_key_vault.main.vault_uri
}

output "key_vault_name" {
  description = "Nombre del Key Vault."
  value       = azurerm_key_vault.main.name
}
