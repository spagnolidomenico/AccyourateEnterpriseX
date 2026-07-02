param(
    [string]$AppFolder = "$env:APPDATA\AccyourateEnterpriseX"
)

Write-Host "Accyourate Enterprise X - SQLite Schema Inspector" -ForegroundColor Cyan
Write-Host "Cartella dati: $AppFolder"
Write-Host ""

if (-not (Test-Path $AppFolder)) {
    Write-Host "Cartella dati non trovata." -ForegroundColor Yellow
    exit 0
}

$dbFiles = Get-ChildItem $AppFolder -Recurse -Include *.db,*.sqlite,*.db3 -ErrorAction SilentlyContinue

if (-not $dbFiles -or $dbFiles.Count -eq 0) {
    Write-Host "Nessun database SQLite trovato." -ForegroundColor Yellow
    exit 0
}

foreach ($db in $dbFiles) {
    Write-Host "========================================" -ForegroundColor DarkGray
    Write-Host "DATABASE: $($db.FullName)" -ForegroundColor Green
    Write-Host "Dimensione: $([Math]::Round($db.Length / 1KB, 2)) KB"
    Write-Host "Ultima modifica: $($db.LastWriteTime)"
    Write-Host ""

    $query = @"
SELECT name, sql
FROM sqlite_master
WHERE type='table'
ORDER BY name;
"@

    try {
        $result = dotnet tool run sqlite3 "$($db.FullName)" "$query" 2>$null
        if ($LASTEXITCODE -ne 0 -or [string]::IsNullOrWhiteSpace($result)) {
            Write-Host "Nota: sqlite3 non disponibile tramite dotnet tool. Uso fallback con PowerShell non invasivo." -ForegroundColor Yellow
            Write-Host "File individuato, schema non letto automaticamente." -ForegroundColor Yellow
        }
        else {
            Write-Host $result
        }
    }
    catch {
        Write-Host "Schema non letto automaticamente. Installa sqlite3 o invia il file .db per analisi." -ForegroundColor Yellow
    }

    Write-Host ""
}

Write-Host "Ispezione completata." -ForegroundColor Cyan
