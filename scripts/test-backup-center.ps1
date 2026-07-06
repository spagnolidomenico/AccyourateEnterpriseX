Write-Host "Accyourate Enterprise X - Backup Center smoke test" -ForegroundColor Cyan
$project = ".\src\Accyourate.App\Accyourate.App.csproj"
dotnet build $project
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "Build completata. Backup Center compilabile." -ForegroundColor Green
