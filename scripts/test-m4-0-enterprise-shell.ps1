$ErrorActionPreference = 'Stop'

$main = Join-Path $PSScriptRoot '..\src\Accyourate.App\MainWindow.cs'
$content = Get-Content $main -Raw

$checks = @(
    @{ Name = 'Release M4.0'; Pattern = 'M4.0 Enterprise UX' },
    @{ Name = 'Sidebar a tutta altezza'; Pattern = 'Grid.SetRowSpan\(menu, 2\)' },
    @{ Name = 'Header nel workspace'; Pattern = 'Grid.SetColumn\(header, 1\)' },
    @{ Name = 'Ricerca globale compatta'; Pattern = 'Cerca nel gestionale o premi Ctrl\+K' },
    @{ Name = 'Brand nella sidebar'; Pattern = 'Text = "Accyourate"' },
    @{ Name = 'Command Palette header'; Pattern = 'MakeLightHeaderAction\("⌘K"' },
    @{ Name = 'Header chiaro'; Pattern = 'Background = Brush.Parse\("#FFFFFF"\)' }
)

foreach ($check in $checks) {
    if ($content -match $check.Pattern) {
        Write-Host "OK - $($check.Name)" -ForegroundColor Green
    } else {
        Write-Host "KO - $($check.Name)" -ForegroundColor Red
        throw "KO - $($check.Name)"
    }
}

Write-Host 'M4.0 Enterprise Shell: controlli statici completati.' -ForegroundColor Cyan
Write-Host 'Ora eseguire: dotnet build' -ForegroundColor Cyan
