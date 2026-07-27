$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$file = Join-Path $root "src/Accyourate.App/AssetManagement/AssetManagementView.cs"

if (!(Test-Path $file)) { throw "File AssetManagementView.cs non trovato." }
$content = Get-Content $file -Raw

$checks = @(
    "BuildCollapsibleEnterpriseOverview",
    "ToggleEnterpriseOverview",
    "RefreshEnterpriseOverview",
    "Dashboard operativa",
    "Mostra ▼",
    "Nascondi ▲"
)

foreach ($check in $checks) {
    if ($content -notmatch [regex]::Escape($check)) {
        throw "Controllo non superato: $check"
    }
}

Write-Host "M5.1 Asset Focus Layout: controlli statici superati." -ForegroundColor Green
