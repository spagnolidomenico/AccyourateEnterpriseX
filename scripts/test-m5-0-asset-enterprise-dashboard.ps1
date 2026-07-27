$ErrorActionPreference = "Stop"

Write-Host "[M5.0] Verifica struttura Asset Enterprise Dashboard..."

$view = "src/Accyourate.App/AssetManagement/AssetManagementView.cs"
if (-not (Test-Path $view)) { throw "File non trovato: $view" }

$content = Get-Content $view -Raw
$required = @(
    "BuildEnterpriseOverview",
    "Stato del patrimonio",
    "Scadenze e avvisi",
    "Attività recente",
    "ProgressRow"
)

foreach ($token in $required) {
    if ($content -notmatch [regex]::Escape($token)) {
        throw "Controllo fallito: elemento mancante '$token'"
    }
}

Write-Host "[M5.0] Controlli statici completati con successo." -ForegroundColor Green
