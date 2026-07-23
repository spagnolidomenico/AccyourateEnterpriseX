$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$view = Join-Path $root "src/Accyourate.App/AssetManagement/AssetManagementView.cs"
$relations = Join-Path $root "src/Accyourate.App/Platform/Relations/EmployeeRelationsService.cs"

$checks = @(
    @{ Name = "KPI garanzie"; File = $view; Pattern = "Garanzie in scadenza" },
    @{ Name = "Conteggio risultati"; File = $view; Pattern = "asset visualizzati" },
    @{ Name = "Reset filtri"; File = $view; Pattern = "ResetFilters" },
    @{ Name = "Colonna assegnatario"; File = $view; Pattern = 'Id = "assigned-to"' },
    @{ Name = "Fix assegnazione async"; File = $view; Pattern = "_ = OpenAssignAsset" },
    @{ Name = "Fix nullable relazioni"; File = $relations; Pattern = "safeEmployeeId" }
)

foreach ($check in $checks) {
    if (-not (Select-String -Path $check.File -Pattern $check.Pattern -SimpleMatch -Quiet)) {
        throw "KO - $($check.Name)"
    }
    Write-Host "OK - $($check.Name)" -ForegroundColor Green
}

Write-Host "M3.5 Asset Management Professional: controlli statici completati." -ForegroundColor Cyan
