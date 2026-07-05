Write-Host "Accyourate Enterprise X - Document Center smoke test" -ForegroundColor Cyan
$project = ".\src\Accyourate.App\Accyourate.App.csproj"
dotnet build $project
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "Build completata. Document Center compilabile." -ForegroundColor Green
