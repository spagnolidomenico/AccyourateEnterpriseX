Write-Host "Accyourate Enterprise X - Enterprise Home 2.0 smoke test" -ForegroundColor Cyan
$project = ".\src\Accyourate.App\Accyourate.App.csproj"

dotnet build $project

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build fallita." -ForegroundColor Red
    exit $LASTEXITCODE
}

$required = @(
    ".\src\Accyourate.App\Platform\Home\EnterpriseHomeView.cs",
    ".\src\Accyourate.App\Platform\Home\EnterpriseHomeService.cs",
    ".\src\Accyourate.App\Platform\Home\EnterpriseHomeSnapshot.cs"
)

foreach ($file in $required) {
    if (-not (Test-Path $file)) {
        Write-Host "File mancante: $file" -ForegroundColor Red
        exit 1
    }
}

Write-Host "Enterprise Home 2.0 compilabile." -ForegroundColor Green
