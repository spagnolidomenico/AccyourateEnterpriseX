Write-Host "=== Accyourate Enterprise X - Run ===" -ForegroundColor Cyan

$project = "src\Accyourate.App\Accyourate.App.csproj"

if (!(Test-Path $project)) {
    Write-Host "ERRORE: progetto non trovato. Esegui questo script dalla cartella principale del repository." -ForegroundColor Red
    exit 1
}

dotnet run --project $project
