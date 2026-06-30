Write-Host "=== Accyourate Enterprise X - Smoke Test ===" -ForegroundColor Cyan

& .\scripts\build.ps1
if ($LASTEXITCODE -ne 0) {
    Write-Host "Smoke test fallito: build non riuscita." -ForegroundColor Red
    exit $LASTEXITCODE
}

Write-Host ""
Write-Host "Smoke test build OK." -ForegroundColor Green
Write-Host "Ora puoi avviare l'app con:" -ForegroundColor Cyan
Write-Host ".\scripts\run.ps1" -ForegroundColor White
