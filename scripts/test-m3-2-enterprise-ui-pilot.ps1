$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$mainWindow = Join-Path $root "src/Accyourate.App/MainWindow.cs"

if (-not (Test-Path $mainWindow)) {
    throw "MainWindow.cs non trovato: $mainWindow"
}

$content = Get-Content $mainWindow -Raw
$required = @(
    "AxSemanticTokens.Background",
    "Buongiorno,",
    "Stato operativo",
    "Azioni rapide",
    "M3 • Design System Foundation"
)

foreach ($marker in $required) {
    if (-not $content.Contains($marker)) {
        throw "Marker M3.2 mancante: $marker"
    }
}

Write-Host "Controlli statici M3.2 superati." -ForegroundColor Green
Write-Host "Esecuzione dotnet build..." -ForegroundColor Cyan
Push-Location $root
try {
    dotnet build
}
finally {
    Pop-Location
}
