$ErrorActionPreference = "Stop"

$checks = @(
    @{ Name = "Ordinamento tabella"; File = "src/Accyourate.App/UIFramework/EnterpriseTable/AxEnterpriseTable.cs"; Pattern = "SortRequested" },
    @{ Name = "Indicatori ordinamento"; File = "src/Accyourate.App/UIFramework/EnterpriseTable/AxEnterpriseTable.cs"; Pattern = "sortIndicator" },
    @{ Name = "Colonne asset ordinabili"; File = "src/Accyourate.App/AssetManagement/AssetManagementView.cs"; Pattern = "IsSortable = true" },
    @{ Name = "Duplicazione asset"; File = "src/Accyourate.App/AssetManagement/AssetManagementView.cs"; Pattern = "DuplicateAsset" },
    @{ Name = "Codice copia univoco"; File = "src/Accyourate.App/AssetManagement/AssetManagementView.cs"; Pattern = "BuildDuplicateAssetCode" }
)

foreach ($check in $checks) {
    if (-not (Test-Path $check.File)) {
        throw "KO - File mancante: $($check.File)"
    }

    if (-not (Select-String -Path $check.File -Pattern $check.Pattern -Quiet)) {
        throw "KO - $($check.Name)"
    }

    Write-Host "OK - $($check.Name)" -ForegroundColor Green
}

Write-Host "M3.6 Asset Details & Enterprise Grid: controlli statici completati." -ForegroundColor Cyan
