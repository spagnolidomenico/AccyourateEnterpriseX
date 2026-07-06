Write-Host "Accyourate Enterprise X - Warning Cleanup smoke test" -ForegroundColor Cyan
$project = ".\src\Accyourate.App\Accyourate.App.csproj"
dotnet build $project
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "Build completata. Warning cleanup compilabile." -ForegroundColor Green
