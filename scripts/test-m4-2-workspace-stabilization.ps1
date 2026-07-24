$ErrorActionPreference = 'Stop'

$root = Split-Path -Parent $PSScriptRoot
$mainWindow = Join-Path $root 'src/Accyourate.App/MainWindow.cs'

if (-not (Test-Path $mainWindow)) {
    throw "MainWindow.cs non trovato: $mainWindow"
}

$content = Get-Content $mainWindow -Raw
$checks = @(
    @{ Name = 'Area buttons registry'; Pattern = '_areaButtons' },
    @{ Name = 'Context buttons registry'; Pattern = '_contextButtons' },
    @{ Name = 'Active area styling'; Pattern = 'ApplyAreaButtonState' },
    @{ Name = 'Active context styling'; Pattern = 'ApplyContextButtonState' },
    @{ Name = 'Duplicate workspace guard'; Pattern = '_currentWorkspaceKey' },
    @{ Name = 'M4.2 release'; Pattern = 'M4.2 Workspace Stabilization' }
)

foreach ($check in $checks) {
    if ($content -notmatch [regex]::Escape($check.Pattern)) {
        throw "KO - $($check.Name) non presente"
    }
    Write-Host "OK - $($check.Name)"
}

Write-Host 'OK - Controlli statici M4.2 completati'
