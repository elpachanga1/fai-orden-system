# ============================================================
# Bootstrap: Remote State Storage para Terraform
# ============================================================
# Ejecutar UNA SOLA VEZ antes del primer "terraform init".
# Crea el Storage Account que almacenara el estado de Terraform.
#
# Uso:
#   .\scripts\bootstrap-remote-state.ps1 -SubscriptionId "xxxx-xxxx" -Location "eastus2"
#
# Requisitos:
#   - Azure CLI instalado y autenticado (az login)
#   - Permisos: Contributor en la subscription
# ============================================================

param(
    [Parameter(Mandatory = $true)]
    [string]$SubscriptionId,

    [Parameter(Mandatory = $false)]
    [string]$Location = "eastus2",

    [Parameter(Mandatory = $false)]
    [string]$ResourceGroupName = "rg-tfstate",

    [Parameter(Mandatory = $false)]
    [string]$ContainerName = "tfstate"
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ---------- 1. Seleccionar Subscription ----------
Write-Host "`n[1/5] Seleccionando subscription: $SubscriptionId" -ForegroundColor Cyan
az account set --subscription $SubscriptionId
if ($LASTEXITCODE -ne 0) { throw "No se pudo seleccionar la subscription." }

# ---------- 2. Crear Resource Group ----------
Write-Host "[2/5] Creando resource group '$ResourceGroupName' en '$Location'..." -ForegroundColor Cyan
az group create `
    --name $ResourceGroupName `
    --location $Location `
    --tags "managed_by=manual" "purpose=terraform-state" `
    --output none

# ---------- 3. Generar nombre unico para Storage Account ----------
# Storage Account names: 3-24 chars, lowercase alphanumeric only, globally unique
$suffix = -join ((48..57) + (97..122) | Get-Random -Count 8 | ForEach-Object { [char]$_ })
$StorageAccountName = "sttfstate$suffix"
Write-Host "[3/5] Creando storage account '$StorageAccountName'..." -ForegroundColor Cyan

az storage account create `
    --name $StorageAccountName `
    --resource-group $ResourceGroupName `
    --location $Location `
    --sku Standard_LRS `
    --kind StorageV2 `
    --allow-blob-public-access false `
    --min-tls-version TLS1_2 `
    --tags "managed_by=manual" "purpose=terraform-state" `
    --output none

# ---------- 4. Crear container ----------
Write-Host "[4/5] Creando container '$ContainerName'..." -ForegroundColor Cyan
az storage container create `
    --name $ContainerName `
    --account-name $StorageAccountName `
    --auth-mode login `
    --output none

# ---------- 5. Imprimir backend config ----------
Write-Host "[5/5] Generando archivo backend.conf..." -ForegroundColor Cyan

$backendConf = @"
resource_group_name  = "$ResourceGroupName"
storage_account_name = "$StorageAccountName"
container_name       = "$ContainerName"
key                  = "carrito-compras.tfstate"
"@

$backendConfPath = Join-Path $PSScriptRoot "..\terraform\backend.conf"
$backendConf | Out-File -FilePath $backendConfPath -Encoding UTF8

Write-Host @"

============================================================
 Bootstrap completado exitosamente
============================================================
 Resource Group    : $ResourceGroupName
 Storage Account   : $StorageAccountName
 Container         : $ContainerName
 Archivo generado  : terraform/backend.conf

 Siguiente paso — inicializar Terraform con el backend remoto:

   cd terraform
   terraform init -backend-config=backend.conf

 IMPORTANTE: backend.conf esta en .gitignore.
 No lo commitees — contiene el nombre del storage account.
============================================================
"@ -ForegroundColor Green
