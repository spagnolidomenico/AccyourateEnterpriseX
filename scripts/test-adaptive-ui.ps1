Write-Host "Accyourate Enterprise X - Adaptive UI smoke test" -ForegroundColor Cyan
$project = ".\src\Accyourate.App\Accyourate.App.csproj"
dotnet build $project
if ($LASTEXITCODE -ne 0) { Write-Host "Build fallita." -ForegroundColor Red; exit $LASTEXITCODE }
$required = @(
 ".\src\Accyourate.App\UIFramework\Layout\EnterpriseAdaptiveLayout.cs",
 ".\src\Accyourate.App\AssetManagement\AssetManagementView.cs"
)
foreach ($file in $required) { if (-not (Test-Path $file)) { Write-Host "File mancante: $file" -ForegroundColor Red; exit 1 } }
Write-Host "Adaptive UI compilabile." -ForegroundColor Green
