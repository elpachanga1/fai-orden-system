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
Write-Host "`n[1/6] Seleccionando subscription: $SubscriptionId" -ForegroundColor Cyan
az account set --subscription $SubscriptionId
if ($LASTEXITCODE -ne 0) { throw "No se pudo seleccionar la subscription." }

# ---------- 2. Registrar Resource Providers ----------
# Los providers no se registran automaticamente en subscriptions nuevas.
# Sin este paso, az storage account create falla con el error
# confuso "SubscriptionNotFound" aunque el ID sea correcto.
Write-Host "[2/6] Registrando resource providers necesarios..." -ForegroundColor Cyan

$providers = @(
    "Microsoft.Storage",          # Storage Account del estado + Blob del proyecto
    "Microsoft.KeyVault",         # Key Vault
    "Microsoft.Web",              # App Service
    "Microsoft.DBforPostgreSQL",  # PostgreSQL Flexible Server
    "Microsoft.Network",          # VNet, subnets, NSGs, Private Endpoints
    "Microsoft.Insights",         # Application Insights
    "Microsoft.OperationalInsights", # Log Analytics Workspace
    "Microsoft.ApiManagement",    # API Management
    "Microsoft.ManagedIdentity"   # User Assigned Identity
)

foreach ($provider in $providers) {
    $state = az provider show --namespace $provider --subscription $SubscriptionId --query "registrationState" -o tsv 2>$null
    if ($state -ne "Registered") {
        Write-Host "  Registrando $provider..." -ForegroundColor DarkCyan
        az provider register --namespace $provider --subscription $SubscriptionId --output none
        if ($LASTEXITCODE -ne 0) { throw "Error al registrar provider $provider." }
    } else {
        Write-Host "  $provider ya registrado." -ForegroundColor DarkGray
    }
}

# Los providers se registran de forma asincrona — esperar hasta que
# Microsoft.Storage este en Registered antes de continuar.
Write-Host "  Esperando confirmacion de Microsoft.Storage..." -ForegroundColor DarkCyan
$attempts = 0
do {
    Start-Sleep -Seconds 5
    $state = az provider show --namespace Microsoft.Storage --subscription $SubscriptionId --query "registrationState" -o tsv 2>$null
    $attempts++
    if ($attempts -gt 24) { throw "Timeout esperando registro de Microsoft.Storage (>2 min)." }
} while ($state -ne "Registered")
Write-Host "  Microsoft.Storage: Registered" -ForegroundColor DarkGray

# ---------- 3. Crear Resource Group ----------
Write-Host "[3/6] Creando resource group '$ResourceGroupName' en '$Location'..." -ForegroundColor Cyan
az group create `
    --name $ResourceGroupName `
    --location $Location `
    --subscription $SubscriptionId `
    --tags "managed_by=manual" "purpose=terraform-state" `
    --output none

# ---------- 4. Generar nombre unico para Storage Account ----------
# Storage Account names: 3-24 chars, lowercase alphanumeric only, globally unique
$suffix = -join ((48..57) + (97..122) | Get-Random -Count 8 | ForEach-Object { [char]$_ })
$StorageAccountName = "sttfstate$suffix"
Write-Host "[4/6] Creando storage account '$StorageAccountName'..." -ForegroundColor Cyan

az storage account create `
    --name $StorageAccountName `
    --resource-group $ResourceGroupName `
    --location $Location `
    --subscription $SubscriptionId `
    --sku Standard_LRS `
    --kind StorageV2 `
    --allow-blob-public-access false `
    --min-tls-version TLS1_2 `
    --tags "managed_by=manual" "purpose=terraform-state" `
    --output none

# ---------- 5. Crear container ----------
Write-Host "[5/6] Creando container '$ContainerName'..." -ForegroundColor Cyan
az storage container create `
    --name $ContainerName `
    --account-name $StorageAccountName `
    --subscription $SubscriptionId `
    --auth-mode login `
    --output none

# ---------- 6. Imprimir backend config ----------
Write-Host "[6/6] Generando archivo backend.conf..." -ForegroundColor Cyan

$backendConf = @"
resource_group_name  = "$ResourceGroupName"
storage_account_name = "$StorageAccountName"
container_name       = "$ContainerName"
key                  = "carrito-compras.tfstate"
"@

$backendConfPath = Join-Path $PSScriptRoot "..\terraform\backend.conf"
# UTF8NoBOM: PowerShell 5 escribe BOM con Out-File -Encoding UTF8.
# Terraform no puede parsear archivos con BOM y falla con "Too many arguments".
[System.IO.File]::WriteAllText($backendConfPath, $backendConf, [System.Text.UTF8Encoding]::new($false))

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
