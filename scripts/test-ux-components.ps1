Write-Host "Accyourate Enterprise X - UX Components smoke test" -ForegroundColor Cyan
$project = ".\src\Accyourate.App\Accyourate.App.csproj"

dotnet build $project

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build fallita." -ForegroundColor Red
    exit $LASTEXITCODE
}

$required = @(
    ".\src\Accyourate.App\UIFramework\UX\AxDialogService.cs",
    ".\src\Accyourate.App\UIFramework\UX\AxStatusBanner.cs",
    ".\src\Accyourate.App\UIFramework\UX\AxLoadingOverlay.cs",
    ".\src\Accyourate.App\UIFramework\UX\AxSnackbar.cs"
)

foreach ($file in $required) {
    if (-not (Test-Path $file)) {
        Write-Host "File mancante: $file" -ForegroundColor Red
        exit 1
    }
}

Write-Host "UX Components compilabili e file principali presenti." -ForegroundColor Green
