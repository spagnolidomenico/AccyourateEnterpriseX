Write-Host "Accyourate Enterprise X - Asset Management Polish smoke test" -ForegroundColor Cyan
$project = ".\src\Accyourate.App\Accyourate.App.csproj"

dotnet build $project

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build fallita." -ForegroundColor Red
    exit $LASTEXITCODE
}

$required = @(
    ".\src\Accyourate.App\AssetManagement\AssetManagementView.cs",
    ".\src\Accyourate.App\UIFramework\Controls\EnterpriseKpiCard.cs"
)

foreach ($file in $required) {
    if (-not (Test-Path $file)) {
        Write-Host "File mancante: $file" -ForegroundColor Red
        exit 1
    }
}

Write-Host "Asset Management Polish compilabile." -ForegroundColor Green
