$ErrorActionPreference = "Stop"
$file = Join-Path $PSScriptRoot "..\src\Accyourate.App\AssetManagement\AssetManagementView.cs"
$content = Get-Content $file -Raw
$checks = @(
    @{ Name = "Vista Lista"; Pattern = 'SetViewMode\(false\)' },
    @{ Name = "Vista Card"; Pattern = 'SetViewMode\(true\)' },
    @{ Name = "Card asset"; Pattern = 'BuildAssetCard' },
    @{ Name = "KPI cliccabili"; Pattern = 'ApplyKpiFilter' },
    @{ Name = "Filtro KPI"; Pattern = 'MatchesKpiFilter' }
)
foreach ($check in $checks) {
    if ($content -notmatch $check.Pattern) {
        Write-Host "ERRORE - $($check.Name) non trovato" -ForegroundColor Red
        exit 1
    }
    Write-Host "OK - $($check.Name)" -ForegroundColor Green
}
Write-Host "M4.3 Asset Management 2.0: controlli statici superati." -ForegroundColor Cyan
