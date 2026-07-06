Write-Host "Accyourate Enterprise X - Workspace Enhancements smoke test" -ForegroundColor Cyan
$project = ".\src\Accyourate.App\Accyourate.App.csproj"

dotnet build $project

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build fallita." -ForegroundColor Red
    exit $LASTEXITCODE
}

$required = @(
    ".\src\Accyourate.App\UIFramework\WorkspaceTabs\WorkspaceTabManager.cs",
    ".\src\Accyourate.App\UIFramework\WorkspaceTabs\WorkspaceHost.cs",
    ".\src\Accyourate.App\EnterpriseWorkspaceWindow.cs"
)

foreach ($file in $required) {
    if (-not (Test-Path $file)) {
        Write-Host "File mancante: $file" -ForegroundColor Red
        exit 1
    }
}

Write-Host "Workspace Enhancements compilabili e file principali presenti." -ForegroundColor Green
