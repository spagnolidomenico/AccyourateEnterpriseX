$ErrorActionPreference = 'Stop'
$main = Join-Path $PSScriptRoot '..\src\Accyourate.App\MainWindow.cs'
$text = Get-Content $main -Raw

if ($text -notmatch 'private void OpenModuleInWorkspace\(Window moduleWindow, string breadcrumb\)') {
    throw 'ERRORE - helper OpenModuleInWorkspace non trovato.'
}

$forbidden = @(
    'new EmployeesWindow\(_database, _user\)\.Show\(\)',
    'new MedicalDevicesWindow\(_database, _user\)\.Show\(\)',
    'new InfrastructureWindow\(_database, _user\)\.Show\(\)',
    'new DocumentManagementWindow\(_database, _user\)\.Show\(\)',
    'new EnterpriseAiAssistantWindow\(_database, _user\)\.Show\(\)',
    'new UsersWindow\(_database, _user\)\.Show\(\)'
)

foreach ($pattern in $forbidden) {
    if ($text -match $pattern) {
        throw "ERRORE - navigazione esterna ancora presente: $pattern"
    }
}

Write-Host 'OK - Persone, Medical, Infrastruttura, Documenti, AI e Amministrazione usano il workspace principale.' -ForegroundColor Green
