param(
    [string]$Version = "dev"
)

Write-Host "=== Accyourate Enterprise X - Release Check $Version ===" -ForegroundColor Cyan

& .\scripts\test-smoke.ps1
if ($LASTEXITCODE -ne 0) { exit $LASTEXITCODE }

Write-Host ""
Write-Host "Controlli manuali consigliati:" -ForegroundColor Yellow
Write-Host "1. Avvio applicazione"
Write-Host "2. Login"
Write-Host "3. Enterprise Workspace"
Write-Host "4. Digital Twin"
Write-Host "5. AI Assistant"
Write-Host "6. Action Engine"
Write-Host "7. Universal Command Bar"
Write-Host ""
Write-Host "Se tutto è OK, fai commit e push su GitHub Desktop." -ForegroundColor Green
