Write-Host "Accyourate Enterprise X - Human Resources Enterprise smoke test" -ForegroundColor Cyan
$project = ".\src\Accyourate.App\Accyourate.App.csproj"

dotnet build $project

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build fallita." -ForegroundColor Red
    exit $LASTEXITCODE
}

$required = @(
    ".\src\Accyourate.App\HumanResources\Enterprise\HumanResourcesEnterpriseView.cs",
    ".\src\Accyourate.App\HumanResources\Enterprise\HumanResourcesEnterpriseService.cs",
    ".\src\Accyourate.App\HumanResources\Enterprise\HumanResourcesEnterpriseSnapshot.cs"
)

foreach ($file in $required) {
    if (-not (Test-Path $file)) {
        Write-Host "File mancante: $file" -ForegroundColor Red
        exit 1
    }
}

Write-Host "Human Resources Enterprise compilabile." -ForegroundColor Green
