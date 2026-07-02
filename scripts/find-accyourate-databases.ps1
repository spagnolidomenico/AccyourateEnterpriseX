Write-Host "Ricerca database Accyourate..." -ForegroundColor Cyan

$locations = @(
    "$env:APPDATA\AccyourateEnterpriseX",
    "$env:LOCALAPPDATA\AccyourateEnterpriseX",
    (Get-Location).Path
)

foreach ($location in $locations) {
    Write-Host ""
    Write-Host "Controllo: $location" -ForegroundColor DarkCyan

    if (Test-Path $location) {
        Get-ChildItem $location -Recurse -Include *.db,*.sqlite,*.db3 -ErrorAction SilentlyContinue |
            Select-Object FullName, Length, LastWriteTime
    }
    else {
        Write-Host "Percorso non trovato."
    }
}
