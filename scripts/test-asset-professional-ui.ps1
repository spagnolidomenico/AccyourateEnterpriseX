Write-Host "Accyourate Enterprise X - Asset Professional UI smoke test" -ForegroundColor Cyan
$project = ".\src\Accyourate.App\Accyourate.App.csproj"

dotnet build $project
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build fallita." -ForegroundColor Red
    exit $LASTEXITCODE
}

$view = ".\src\Accyourate.App\AssetManagement\AssetManagementView.cs"
if (-not (Test-Path $view)) {
    Write-Host "File mancante: $view" -ForegroundColor Red
    exit 1
}

$content = Get-Content $view -Raw
$required = @(
    'AssetTableColumns',
    'ColumnDefinitions = new ColumnDefinitions("*,18,440")',
    'Width = 146',
    'MinWidth = 150',
    'RowDefinitions = new RowDefinitions("Auto,Auto")'
)

foreach ($token in $required) {
    if (-not $content.Contains($token)) {
        Write-Host "Controllo UI mancante: $token" -ForegroundColor Red
        exit 1
    }
}

Write-Host "Asset Management Professional UI compilabile e controlli presenti." -ForegroundColor Green
