<#
.SYNOPSIS
    Crea los GitHub Actions secrets leyendo los outputs de Terraform.

.DESCRIPTION
    Usa un helper .NET temporal (Sodium.Core) para cifrar con libsodium.
    Requiere .NET SDK 8+ y haber corrido "terraform apply" antes.

.PARAMETER GithubToken
    PAT de GitHub con permisos "Secrets: Read and write" en el repo.

.PARAMETER Repo
    Repositorio en formato "org/repo". Default: elpachanga1/fai-orden-system

.PARAMETER TfStateStorageAccount
    Nombre del Storage Account del remote state. Si se omite, se lee automaticamente
    de terraform/backend.conf (campo storage_account_name).

.EXAMPLE
    cd C:\Code\fai-orden-system
    .\scripts\set-github-secrets.ps1 -GithubToken "github_pat_11A..."

.EXAMPLE
    .\scripts\set-github-secrets.ps1 -GithubToken "github_pat_11A..." -TfStateStorageAccount "sttfstatek1pnqer3"
#>

param(
    [Parameter(Mandatory = $true)]
    [string]$GithubToken,

    [Parameter(Mandatory = $false)]
    [string]$Repo = "elpachanga1/fai-orden-system",

    [Parameter(Mandatory = $false)]
    [string]$TfStateStorageAccount = ""
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

# ---------------------------------------------------------------
# 1. Leer outputs de Terraform
# ---------------------------------------------------------------
Write-Host "Leyendo outputs de Terraform..." -ForegroundColor Cyan

$tfDir = Resolve-Path (Join-Path $PSScriptRoot "..\terraform")
Push-Location $tfDir
$tfOutputRaw = terraform output -json
$tfExit = $LASTEXITCODE
Pop-Location
if ($tfExit -ne 0) {
    Write-Error "terraform output fallo. Asegurate de haber corrido apply primero."
}

$tf = ($tfOutputRaw -join "`n") | ConvertFrom-Json

$azureClientId       = $tf.oidc_client_id.value
$azureTenantId       = $tf.oidc_tenant_id.value
$azureSubscriptionId = $tf.oidc_subscription_id.value
$staticWebAppToken   = $tf.frontend_api_key.value
$containerAppName    = $tf.backend.value.container_app_name
$acrLoginServer      = $tf.backend.value.acr_login_server
$resourceGroup       = $tf.resource_group_name.value
$backendFqdn         = $tf.backend.value.fqdn

Write-Host ""
Write-Host "Valores leidos de Terraform:" -ForegroundColor Cyan
Write-Host "  AZURE_CLIENT_ID          = $azureClientId"
Write-Host "  AZURE_TENANT_ID          = $azureTenantId"
Write-Host "  AZURE_SUBSCRIPTION_ID    = $azureSubscriptionId"
Write-Host "  AZURE_CONTAINER_APP_NAME = $containerAppName"
Write-Host "  AZURE_ACR_LOGIN_SERVER   = $acrLoginServer"
Write-Host "  AZURE_RESOURCE_GROUP     = $resourceGroup"
Write-Host "  REACT_APP_API_URL        = https://$backendFqdn"
Write-Host ""

# Auto-leer storage account desde backend.conf si no se paso como parametro
if ([string]::IsNullOrWhiteSpace($TfStateStorageAccount)) {
    $backendConfPath = Join-Path $tfDir "backend.conf"
    if (Test-Path $backendConfPath) {
        $match = Select-String -Path $backendConfPath -Pattern 'storage_account_name\s*=\s*"([^"]+)"'
        if ($match) {
            $TfStateStorageAccount = $match.Matches[0].Groups[1].Value
            Write-Host "  TF_STATE_STORAGE_ACCOUNT = $TfStateStorageAccount (leido de backend.conf)" -ForegroundColor DarkGray
        }
    }
}
if ([string]::IsNullOrWhiteSpace($TfStateStorageAccount)) {
    $TfStateStorageAccount = Read-Host "Nombre del Storage Account del remote state (ej: sttfstatek1pnqer3)"
}

# ---------------------------------------------------------------
# 2. Obtener la public key del repo
# ---------------------------------------------------------------
Write-Host ""
Write-Host "Obteniendo public key de $Repo..." -ForegroundColor Cyan

$apiHeaders = @{
    Authorization          = "Bearer $GithubToken"
    Accept                 = "application/vnd.github+json"
    "X-GitHub-Api-Version" = "2022-11-28"
    "User-Agent"           = "set-github-secrets-ps"
}

$pubKey = Invoke-RestMethod `
    -Uri "https://api.github.com/repos/$Repo/actions/secrets/public-key" `
    -Headers $apiHeaders

Write-Host "Public key obtenida. key_id: $($pubKey.key_id)" -ForegroundColor Green

# ---------------------------------------------------------------
# 3. Crear helper .NET temporal con Sodium.Core
#    Escrito linea por linea para evitar problemas de encoding con heredocs.
# ---------------------------------------------------------------
Write-Host ""
Write-Host "Preparando helper de cifrado (Sodium.Core)..." -ForegroundColor Cyan

$helperDir = Join-Path $env:TEMP "gh-secrets-helper-$(Get-Random)"
New-Item -ItemType Directory -Path $helperDir | Out-Null

$programLines = @(
    'using System;',
    'using System.IO;',
    'using System.Net.Http;',
    'using System.Text;',
    'using System.Text.Json;',
    'using System.Threading.Tasks;',
    'using Sodium;',
    '',
    'var raw   = File.ReadAllText(args[0], System.Text.Encoding.UTF8);',
    'var input = JsonDocument.Parse(raw).RootElement;',
    '',
    'var token     = input.GetProperty("token").GetString()!;',
    'var repo      = input.GetProperty("repo").GetString()!;',
    'var keyId     = input.GetProperty("key_id").GetString()!;',
    'var publicKey = Convert.FromBase64String(input.GetProperty("public_key").GetString()!);',
    'var secrets   = input.GetProperty("secrets");',
    '',
    'using var http = new HttpClient();',
    'http.DefaultRequestHeaders.Add("Authorization", $"Bearer {token}");',
    'http.DefaultRequestHeaders.Add("Accept", "application/vnd.github+json");',
    'http.DefaultRequestHeaders.Add("X-GitHub-Api-Version", "2022-11-28");',
    'http.DefaultRequestHeaders.Add("User-Agent", "set-github-secrets-ps");',
    '',
    'foreach (var secret in secrets.EnumerateObject())',
    '{',
    '    var name      = secret.Name;',
    '    var value     = secret.Value.GetString() ?? "";',
    '    var encrypted = SealedPublicKeyBox.Create(Encoding.UTF8.GetBytes(value), publicKey);',
    '    var body      = JsonSerializer.Serialize(new { encrypted_value = Convert.ToBase64String(encrypted), key_id = keyId });',
    '    var response  = await http.PutAsync(',
    '        $"https://api.github.com/repos/{repo}/actions/secrets/{name}",',
    '        new StringContent(body, Encoding.UTF8, "application/json")',
    '    );',
    '    var status = (int)response.StatusCode;',
    '    var ok     = status == 201 || status == 204;',
    '    Console.WriteLine($"{(ok ? "OK  " : "FAIL")} {name} ({status})");',
    '}'
)

$csprojLines = @(
    '<Project Sdk="Microsoft.NET.Sdk">',
    '  <PropertyGroup>',
    '    <OutputType>Exe</OutputType>',
    '    <TargetFramework>net8.0</TargetFramework>',
    '    <Nullable>enable</Nullable>',
    '    <ImplicitUsings>disable</ImplicitUsings>',
    '  </PropertyGroup>',
    '  <ItemGroup>',
    '    <PackageReference Include="Sodium.Core" Version="1.3.4" />',
    '  </ItemGroup>',
    '</Project>'
)

Set-Content -Path (Join-Path $helperDir "Program.cs")    -Value $programLines -Encoding UTF8
Set-Content -Path (Join-Path $helperDir "Helper.csproj") -Value $csprojLines  -Encoding UTF8

Write-Host "Restaurando paquetes (Sodium.Core)..." -ForegroundColor Gray
& dotnet restore (Join-Path $helperDir "Helper.csproj") --nologo -q
if ($LASTEXITCODE -ne 0) { Write-Error "dotnet restore fallo." }

# ---------------------------------------------------------------
# 4. Filtrar valores vacios y construir payload
# ---------------------------------------------------------------
$allSecrets = @{
    AZURE_CLIENT_ID                 = $azureClientId
    AZURE_TENANT_ID                 = $azureTenantId
    AZURE_SUBSCRIPTION_ID           = $azureSubscriptionId
    AZURE_STATIC_WEB_APPS_API_TOKEN = $staticWebAppToken
    REACT_APP_API_URL               = "https://$backendFqdn"
    AZURE_ACR_LOGIN_SERVER          = $acrLoginServer
    AZURE_CONTAINER_APP_NAME        = $containerAppName
    AZURE_RESOURCE_GROUP            = $resourceGroup
    TF_STATE_STORAGE_ACCOUNT        = $TfStateStorageAccount
}

$filteredSecrets = @{}
foreach ($pair in $allSecrets.GetEnumerator()) {
    $val = [string]$pair.Value
    if ([string]::IsNullOrWhiteSpace($val) -or $val -eq "https://") {
        Write-Host "  SKIP $($pair.Key) - valor vacio, secret no modificado" -ForegroundColor Yellow
    } else {
        $filteredSecrets[$pair.Key] = $val
    }
}

if ($filteredSecrets.Count -eq 0) {
    Write-Error "Todos los valores estan vacios. Verificar que terraform apply haya terminado."
}

$payload = [ordered]@{
    token      = $GithubToken
    repo       = $Repo
    key_id     = $pubKey.key_id
    public_key = $pubKey.key
    secrets    = $filteredSecrets
} | ConvertTo-Json -Depth 3

# ---------------------------------------------------------------
# 5. Ejecutar el helper
# ---------------------------------------------------------------
Write-Host ""
Write-Host "Creando secrets en $Repo..." -ForegroundColor Cyan

$payloadFile = Join-Path $helperDir "payload.json"
[System.IO.File]::WriteAllText($payloadFile, $payload, [System.Text.UTF8Encoding]::new($false))
& dotnet run --project (Join-Path $helperDir "Helper.csproj") --no-restore -- $payloadFile 2>&1 |
    ForEach-Object {
        if ($_ -match "^OK") {
            Write-Host "  $_" -ForegroundColor Green
        } elseif ($_ -match "^FAIL") {
            Write-Host "  $_" -ForegroundColor Red
        } else {
            Write-Host "  $_"
        }
    }

# ---------------------------------------------------------------
# 6. Limpiar helper temporal
# ---------------------------------------------------------------
Remove-Item -Recurse -Force $helperDir

Write-Host ""
Write-Host "Listo. Verificar en: https://github.com/$Repo/settings/secrets/actions" -ForegroundColor Green
Write-Host ""
Write-Host "Secrets manuales que todavia necesitas crear:" -ForegroundColor Yellow
Write-Host "  TF_POSTGRESQL_PASSWORD  = (tu password de PostgreSQL)"
Write-Host "  TF_JWT_SECRET_KEY       = (tu JWT secret key)"
