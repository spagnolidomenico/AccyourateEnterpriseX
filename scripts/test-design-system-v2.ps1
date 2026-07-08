Write-Host "Accyourate Enterprise X - Design System v2 smoke test" -ForegroundColor Cyan
$project = ".\src\Accyourate.App\Accyourate.App.csproj"

dotnet build $project

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build fallita." -ForegroundColor Red
    exit $LASTEXITCODE
}

$required = @(
    ".\src\Accyourate.App\UIFramework\DesignSystem\AxKpiCard.cs",
    ".\src\Accyourate.App\UIFramework\DesignSystem\AxToolbar.cs",
    ".\src\Accyourate.App\UIFramework\DesignSystem\AxSearchBox.cs",
    ".\src\Accyourate.App\UIFramework\DesignSystem\AxStatusBadge.cs",
    ".\src\Accyourate.App\UIFramework\DesignSystem\AxInfoPanel.cs",
    ".\src\Accyourate.App\UIFramework\DesignSystem\AxTimeline.cs",
    ".\src\Accyourate.App\UIFramework\DesignSystem\AxDashboardWidget.cs"
)

foreach ($file in $required) {
    if (-not (Test-Path $file)) {
        Write-Host "File mancante: $file" -ForegroundColor Red
        exit 1
    }
}

Write-Host "Design System v2 compilabile e file principali presenti." -ForegroundColor Green
