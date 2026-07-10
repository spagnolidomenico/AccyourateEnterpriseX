Write-Host "Accyourate Enterprise X - KPI Layout Fix smoke test" -ForegroundColor Cyan
$project = ".\src\Accyourate.App\Accyourate.App.csproj"

dotnet build $project
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build fallita." -ForegroundColor Red
    exit $LASTEXITCODE
}

$required = @(
    ".\src\Accyourate.App\UIFramework\DesignSystem\AxKpiCard.cs",
    ".\src\Accyourate.App\Platform\Dashboard\EnterpriseDashboardView.cs"
)

foreach ($file in $required) {
    if (-not (Test-Path $file)) {
        Write-Host "File mancante: $file" -ForegroundColor Red
        exit 1
    }
}

Write-Host "KPI Layout Fix compilabile." -ForegroundColor Green
