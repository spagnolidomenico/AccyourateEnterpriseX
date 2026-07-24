$ErrorActionPreference = "Stop"

$root = Split-Path -Parent $PSScriptRoot
$view = Join-Path $root "src\Accyourate.App\AssetManagement\AssetManagementView.cs"
$table = Join-Path $root "src\Accyourate.App\UIFramework\EnterpriseTable\AxEnterpriseTable.cs"

$checks = @(
    @{ Name = "Lista a tutta larghezza"; File = $view; Pattern = "!_detailsVisible" },
    @{ Name = "Pannello dettagli richiudibile"; File = $view; Pattern = "Mostra dettagli" },
    @{ Name = "Colonna Asset compatta"; File = $view; Pattern = "AssetIdentityCell" },
    @{ Name = "KPI compatti"; File = $view; Pattern = "CompactKpi" },
    @{ Name = "Righe compatte"; File = $table; Pattern = "CompactRows" },
    @{ Name = "Righe alternate"; File = $table; Pattern = "AlternatingRows" },
    @{ Name = "Intestazione fissa"; File = $table; Pattern = "_rowsScroll" }
)

foreach ($check in $checks) {
    if (-not (Test-Path $check.File) -or -not (Select-String -Path $check.File -Pattern $check.Pattern -Quiet)) {
        Write-Host "KO - $($check.Name)" -ForegroundColor Red
        throw "KO - $($check.Name)"
    }
    Write-Host "OK - $($check.Name)" -ForegroundColor Green
}

Write-Host "M3.6.1 Asset List Usability: controlli statici completati." -ForegroundColor Cyan
