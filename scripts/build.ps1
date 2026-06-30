param(
    [string]$Configuration = "Debug"
)

Write-Host "=== Accyourate Enterprise X - Build ===" -ForegroundColor Cyan

$solution = "AccyourateEnterpriseX.sln"

if (!(Test-Path $solution)) {
    Write-Host "ERRORE: soluzione non trovata. Esegui questo script dalla cartella principale del repository." -ForegroundColor Red
    exit 1
}

Write-Host "Pulizia progetto..." -ForegroundColor Yellow
dotnet clean $solution
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Ripristino pacchetti..." -ForegroundColor Yellow
dotnet restore $solution
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "Compilazione $Configuration..." -ForegroundColor Yellow
dotnet build $solution --configuration $Configuration
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host "BUILD COMPLETATA CON SUCCESSO" -ForegroundColor Green
