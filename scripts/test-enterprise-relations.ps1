Write-Host "Accyourate Enterprise X - Enterprise Relations smoke test" -ForegroundColor Cyan
$project = ".\src\Accyourate.App\Accyourate.App.csproj"
dotnet build $project
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
$required = @(
    ".\src\Accyourate.App\Platform\Relations\EmployeeRelationsService.cs",
    ".\src\Accyourate.App\Platform\Relations\EmployeeRelationsSnapshot.cs",
    ".\src\Accyourate.App\Platform\Relations\EnterpriseRelationItem.cs"
)
foreach ($file in $required) { if (-not (Test-Path $file)) { Write-Host "File mancante: $file" -ForegroundColor Red; exit 1 } }
Write-Host "Enterprise Relations compilabile." -ForegroundColor Green
