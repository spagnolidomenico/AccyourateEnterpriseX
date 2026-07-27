$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$assetView = Join-Path $root "src\Accyourate.App\AssetManagement\AssetManagementView.cs"
$table = Join-Path $root "src\Accyourate.App\UIFramework\EnterpriseTable\AxEnterpriseTable.cs"

$checks = @(
    @{ File = $assetView; Pattern = 'ToolbarButton\("✎", "Modifica"' },
    @{ File = $assetView; Pattern = 'Doppio clic per aprire' },
    @{ File = $assetView; Pattern = 'Cerca asset, seriale, modello' },
    @{ File = $table; Pattern = 'PointerEntered' },
    @{ File = $table; Pattern = 'PremiumHover' }
)

foreach ($check in $checks) {
    if (-not (Select-String -Path $check.File -Pattern $check.Pattern -Quiet)) {
        throw "Controllo non superato: $($check.Pattern)"
    }
}

Write-Host "M6.0 Enterprise UX Foundation: controlli statici superati." -ForegroundColor Green
