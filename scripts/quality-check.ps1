Write-Host "Accyourate Enterprise X - RC Quality Check" -ForegroundColor Cyan

$project = ".\src\Accyourate.App\Accyourate.App.csproj"
$errors = 0

function Check-File($path) {
    if (-not (Test-Path $path)) {
        Write-Host "MISSING: $path" -ForegroundColor Red
        $script:errors++
    } else {
        Write-Host "OK: $path" -ForegroundColor Green
    }
}

Check-File ".\VERSION"
Check-File ".\CHANGELOG.md"
Check-File ".\RELEASE_NOTES.md"
Check-File $project

if ($errors -gt 0) {
    Write-Host "Quality check fallito: file richiesti mancanti." -ForegroundColor Red
    exit 1
}

Write-Host "Build progetto..." -ForegroundColor Cyan
dotnet build $project

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build fallita." -ForegroundColor Red
    exit $LASTEXITCODE
}

if (Test-Path ".\scripts\test-smoke.ps1") {
    Write-Host "Esecuzione smoke test..." -ForegroundColor Cyan
    powershell -ExecutionPolicy Bypass -File ".\scripts\test-smoke.ps1"

    if ($LASTEXITCODE -ne 0) {
        Write-Host "Smoke test fallito." -ForegroundColor Red
        exit $LASTEXITCODE
    }
}

Write-Host "Quality check completato con successo." -ForegroundColor Green
