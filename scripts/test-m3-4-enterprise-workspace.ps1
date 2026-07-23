$ErrorActionPreference = "Stop"

$main = Join-Path $PSScriptRoot "..\src\Accyourate.App\MainWindow.cs"
$content = Get-Content $main -Raw

$checks = @(
    @{ Name = "Ctrl+K Command Palette"; Pattern = "KeyModifiers.Control" },
    @{ Name = "Sidebar persistente"; Pattern = "workspace.sidebar" },
    @{ Name = "Theme manager"; Pattern = "AxThemeManager.Current.Toggle" },
    @{ Name = "Quick action Asset"; Pattern = "Nuovo asset" },
    @{ Name = "Release M3.4"; Pattern = "M3.4 • Enterprise Workspace" }
)

foreach ($check in $checks) {
    if ($content -notmatch [regex]::Escape($check.Pattern)) {
        throw "Controllo non superato: $($check.Name)"
    }
    Write-Host "OK - $($check.Name)" -ForegroundColor Green
}

Write-Host "M3.4 Enterprise Workspace: controlli statici completati." -ForegroundColor Cyan
