$ErrorActionPreference = "Stop"
$main = Join-Path $PSScriptRoot "..\src\Accyourate.App\MainWindow.cs"
$content = Get-Content $main -Raw
$checks = @(
    @{ Name = "Layout a tre colonne"; Pattern = 'ColumnDefinitions = new ColumnDefinitions\(\$"\{\(_sidebarCollapsed.*220,\*"\)' },
    @{ Name = "Sidebar contestuale"; Pattern = 'BuildContextSidebar\(\)' },
    @{ Name = "Macro area Asset"; Pattern = 'AddAreaButton\(stack, "💻", "Asset", "Asset"\)' },
    @{ Name = "Macro area Persone"; Pattern = 'AddAreaButton\(stack, "👥", "Persone", "Persone"\)' },
    @{ Name = "Macro area Medical"; Pattern = 'AddAreaButton\(stack, "✚", "Medical", "Medical"\)' },
    @{ Name = "Navigazione contestuale"; Pattern = 'SetContextArea\(string area\)' },
    @{ Name = "Release M4.1"; Pattern = 'M4\.1 • Adaptive Workspace' }
)
foreach ($check in $checks) {
    if ($content -match $check.Pattern) { Write-Host "OK - $($check.Name)" -ForegroundColor Green }
    else { Write-Host "KO - $($check.Name)" -ForegroundColor Red; throw "KO - $($check.Name)" }
}
Write-Host "M4.1 Adaptive Workspace: controlli statici completati." -ForegroundColor Cyan
