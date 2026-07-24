$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$main = Join-Path $root "src/Accyourate.App/MainWindow.cs"
$view = Join-Path $root "src/Accyourate.App/AssetManagement/AssetManagementView.cs"

$mainText = Get-Content $main -Raw
$viewText = Get-Content $view -Raw

if ($mainText -notmatch 'OpenAssetWorkspace\("Notebook"') { throw "Filtro Notebook non collegato" }
if ($mainText -notmatch 'OpenAssetWorkspace\("Smartphone"') { throw "Filtro Smartphone non collegato" }
if ($mainText -notmatch '_workspaceContent.Content = new AssetManagementView\(category\)') { throw "Asset workspace non integrato" }
if ($viewText -notmatch 'ApplyInitialCategoryFilter') { throw "Filtro iniziale non implementato" }

Write-Host "OK - Navigazione Asset contestuale corretta" -ForegroundColor Green
