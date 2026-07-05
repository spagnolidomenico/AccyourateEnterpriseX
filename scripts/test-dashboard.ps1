Write-Host "Accyourate Enterprise X - Dashboard smoke test" -ForegroundColor Cyan
$project = ".\src\Accyourate.App\Accyourate.App.csproj"
dotnet build $project
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }
Write-Host "Build completata. Enterprise Dashboard compilabile." -ForegroundColor Green
