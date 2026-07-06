Write-Host "Accyourate Enterprise X - Design System smoke test" -ForegroundColor Cyan
$project = ".\src\Accyourate.App\Accyourate.App.csproj"

dotnet build $project

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build fallita." -ForegroundColor Red
    exit $LASTEXITCODE
}

$required = @(
    ".\src\Accyourate.App\UIFramework\DesignSystem\AxSpacing.cs",
    ".\src\Accyourate.App\UIFramework\DesignSystem\AxTypography.cs",
    ".\src\Accyourate.App\UIFramework\DesignSystem\AxButton.cs",
    ".\src\Accyourate.App\UIFramework\DesignSystem\AxCard.cs",
    ".\src\Accyourate.App\UIFramework\DesignSystem\AxPageHeader.cs"
)

foreach ($file in $required) {
    if (-not (Test-Path $file)) {
        Write-Host "File mancante: $file" -ForegroundColor Red
        exit 1
    }
}

Write-Host "Design System compilabile e file principali presenti." -ForegroundColor Green
