Write-Host "Accyourate Enterprise X - Delivery Report smoke test" -ForegroundColor Cyan

$project = ".\src\Accyourate.App\Accyourate.App.csproj"

if (-not (Test-Path $project)) {
    Write-Host "Progetto App non trovato. Eseguire dalla root del repository." -ForegroundColor Red
    exit 1
}

dotnet build $project

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build fallita." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host "Build completata. Delivery Report Foundation compilabile." -ForegroundColor Green
