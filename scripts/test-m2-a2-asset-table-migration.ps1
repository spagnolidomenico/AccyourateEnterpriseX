Write-Host "Accyourate Enterprise X - M2-A2 Asset Management migration test" -ForegroundColor Cyan

$project = ".\src\Accyourate.App\Accyourate.App.csproj"
$view = ".\src\Accyourate.App\AssetManagement\AssetManagementView.cs"
$table = ".\src\Accyourate.App\UIFramework\EnterpriseTable\AxEnterpriseTable.cs"

dotnet build $project
if ($LASTEXITCODE -ne 0) {
    Write-Host "Build fallita." -ForegroundColor Red
    exit $LASTEXITCODE
}

foreach ($file in @($view, $table)) {
    if (-not (Test-Path $file)) {
        Write-Host "File mancante: $file" -ForegroundColor Red
        exit 1
    }
}

$viewContent = Get-Content $view -Raw
$tableContent = Get-Content $table -Raw

if ($viewContent -notmatch 'AxEnterpriseTable<Asset>') {
    Write-Host "Asset Management non usa AxEnterpriseTable<Asset>." -ForegroundColor Red
    exit 1
}

if ($viewContent -match 'private Button Row\(Asset asset\)') {
    Write-Host "La vecchia implementazione Row(Asset) è ancora presente." -ForegroundColor Red
    exit 1
}

if ($tableContent -notmatch 'ItemActivated') {
    Write-Host "Il supporto all’attivazione riga non è presente." -ForegroundColor Red
    exit 1
}

Write-Host "M2-A2 compilato e migrazione strutturale verificata." -ForegroundColor Green
