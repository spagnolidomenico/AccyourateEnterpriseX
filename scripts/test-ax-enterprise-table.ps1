Write-Host "Accyourate Enterprise X - AxEnterpriseTable smoke test" -ForegroundColor Cyan
$project = ".\src\Accyourate.App\Accyourate.App.csproj"

dotnet build $project

if ($LASTEXITCODE -ne 0) {
    Write-Host "Build fallita." -ForegroundColor Red
    exit $LASTEXITCODE
}

$required = @(
    ".\src\Accyourate.App\UIFramework\EnterpriseTable\AxColumnAlignment.cs",
    ".\src\Accyourate.App\UIFramework\EnterpriseTable\AxEnterpriseColumn.cs",
    ".\src\Accyourate.App\UIFramework\EnterpriseTable\AxEnterpriseTable.cs"
)

foreach ($file in $required) {
    if (-not (Test-Path $file)) {
        Write-Host "File mancante: $file" -ForegroundColor Red
        exit 1
    }
}

Write-Host "AxEnterpriseTable compilabile." -ForegroundColor Green
