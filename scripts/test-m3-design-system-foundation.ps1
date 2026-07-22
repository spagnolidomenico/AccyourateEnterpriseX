param(
    [switch]$SkipBuild
)

$ErrorActionPreference = "Stop"
$root = Split-Path -Parent $PSScriptRoot
Set-Location $root

Write-Host "Accyourate Enterprise X - M3.1 Design System Foundation" -ForegroundColor Cyan

$requiredFiles = @(
    "src/Accyourate.App/UIFramework/Foundation/AxColorTokens.cs",
    "src/Accyourate.App/UIFramework/Foundation/AxSemanticTokens.cs",
    "src/Accyourate.App/UIFramework/Foundation/AxLayoutTokens.cs",
    "src/Accyourate.App/UIFramework/Foundation/AxTypographyTokens.cs",
    "src/Accyourate.App/UIFramework/Foundation/AxThemePalette.cs",
    "src/Accyourate.App/UIFramework/Tokens/UiTokens.cs",
    "src/Accyourate.App/DesignSystem/AccyourateDesignTokens.cs"
)

foreach ($file in $requiredFiles) {
    if (-not (Test-Path $file)) {
        throw "File M3.1 mancante: $file"
    }
}

$uiTokens = Get-Content "src/Accyourate.App/UIFramework/Tokens/UiTokens.cs" -Raw
if ($uiTokens -notmatch "AxSemanticTokens.BrandPrimary") {
    throw "UiTokens non delega alla foundation canonica."
}

$legacyTokens = Get-Content "src/Accyourate.App/DesignSystem/AccyourateDesignTokens.cs" -Raw
if ($legacyTokens -notmatch "Obsolete" -or $legacyTokens -notmatch "AxLayoutTokens") {
    throw "Il layer legacy non e stato convertito in compatibility facade."
}

if (-not $SkipBuild) {
    dotnet build "src/Accyourate.App/Accyourate.App.csproj"
    if ($LASTEXITCODE -ne 0) {
        throw "Build M3.1 fallita."
    }
}

Write-Host "M3.1 foundation verificata con successo." -ForegroundColor Green
