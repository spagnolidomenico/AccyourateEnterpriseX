$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
$file = Join-Path $root "src/Accyourate.App/Platform/Relations/EmployeeRelationsService.cs"

if (-not (Test-Path $file)) {
    throw "KO - File non trovato: $file"
}

$required = @(
    "Load(string? employeeId, string? employeeName)",
    "var safeEmployeeId = employeeId ?? string.Empty;",
    "var safeEmployeeName = employeeName ?? string.Empty;",
    "Assets = LoadAssets(safeEmployeeId, safeEmployeeName)",
    "Documents = LoadDocuments(safeEmployeeId, safeEmployeeName)",
    "DeliveryReports = LoadDeliveryReports(safeEmployeeId, safeEmployeeName)"
)

foreach ($pattern in $required) {
    if (-not (Select-String -Path $file -Pattern $pattern -SimpleMatch -Quiet)) {
        throw "KO - Correzione mancante: $pattern"
    }
}

Write-Host "OK - Fix nullable EmployeeRelationsService presente" -ForegroundColor Green
Write-Host "Ora eseguire: dotnet build" -ForegroundColor Cyan
