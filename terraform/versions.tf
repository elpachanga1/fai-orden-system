terraform {
  required_version = ">= 1.7.0"

  required_providers {
    azurerm = {
      source  = "hashicorp/azurerm"
      version = "~> 4.0"
    }
    random = {
      source  = "hashicorp/random"
      version = "~> 3.0"
    }
    azuread = {
      source  = "hashicorp/azuread"
      version = "~> 3.0"
    }
    github = {
      source  = "integrations/github"
      version = "~> 6.0"
    }
  }

  # Backend configurado via: terraform init -backend-config=backend.conf
  # Generar backend.conf ejecutando: scripts/bootstrap-remote-state.ps1
  backend "azurerm" {
    container_name = "tfstate"
    key            = "carrito-compras.tfstate"
    # resource_group_name y storage_account_name se pasan en backend.conf
  }
}

provider "azurerm" {
  features {
    key_vault {
      purge_soft_delete_on_destroy    = true
      recover_soft_deleted_key_vaults = true
    }
  }
}

provider "azuread" {}

provider "github" {
  token = var.github_token
  owner = var.github_org
}

