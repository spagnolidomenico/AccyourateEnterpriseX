param(
    [string]$OutputDirectory = ".\artifacts\responsive-ui-audit",
    [switch]$FailOnHigh
)

$ErrorActionPreference = "Stop"
$projectRoot = Split-Path -Parent $PSScriptRoot
$sourceRoot = Join-Path $projectRoot "src\Accyourate.App"
$resolvedOutput = if ([System.IO.Path]::IsPathRooted($OutputDirectory)) { $OutputDirectory } else { Join-Path $projectRoot $OutputDirectory }
New-Item -ItemType Directory -Force -Path $resolvedOutput | Out-Null

$findings = New-Object System.Collections.Generic.List[object]

function Add-Finding {
    param([string]$Severity, [string]$Rule, [string]$File, [int]$Line, [string]$Detail, [string]$Recommendation)
    $relative = $File.Substring($projectRoot.Length).TrimStart('\')
    $findings.Add([pscustomobject]@{
        Severity = $Severity
        Rule = $Rule
        File = $relative
        Line = $Line
        Detail = $Detail
        Recommendation = $Recommendation
    })
}

$files = Get-ChildItem -Path $sourceRoot -Filter "*.cs" -Recurse -File | Where-Object { $_.FullName -notmatch "\\(bin|obj)\\" }
foreach ($file in $files) {
    $lines = Get-Content -LiteralPath $file.FullName
    $fileText = $lines -join "`n"
    $hasWideRows = $false

    for ($index = 0; $index -lt $lines.Count; $index++) {
        $line = $lines[$index]
        $lineNumber = $index + 1

        foreach ($match in [regex]::Matches($line, 'MinWidth\s*=\s*(\d+)')) {
            $width = [int]$match.Groups[1].Value
            if ($width -ge 900) {
                $hasWideRows = $true
                Add-Finding "High" "LargeMinWidth" $file.FullName $lineNumber "MinWidth impostata a $width px." "Rimuovere il vincolo o migrare il contenuto a WrapPanel/schede responsive."
            }
        }

        foreach ($match in [regex]::Matches($line, 'ColumnDefinitions\s*=\s*new ColumnDefinitions\("([^"]+)"\)')) {
            $definition = $match.Groups[1].Value
            $numbers = [regex]::Matches($definition, '(?<![A-Za-z*])(\d+)(?![A-Za-z])') | ForEach-Object { [int]$_.Groups[1].Value }
            $sum = ($numbers | Measure-Object -Sum).Sum
            if ($numbers.Count -ge 6 -and $sum -ge 900) {
                $hasWideRows = $true
                Add-Finding "High" "RigidWideGrid" $file.FullName $lineNumber "Griglia con $($numbers.Count) colonne fisse e almeno $sum px." "Usare AxResponsiveRecordCard oppure isolare una vera tabella in uno scorrimento dedicato."
            }
        }

        if ($line -match 'ColumnDefinitions\s*=\s*new ColumnDefinitions\("\*,Auto"\)') {
            Add-Finding "Medium" "LegacyPageHeader" $file.FullName $lineNumber "Intestazione potenzialmente basata su colonne titolo/comandi." "Usare AxResponsivePageHeader per separare titolo e barra comandi."
        }

        if ($line -match 'StackPanel\s*\{[^\r\n]*Orientation\s*=\s*Orientation\.Horizontal') {
            Add-Finding "Low" "HorizontalStackPanel" $file.FullName $lineNumber "StackPanel orizzontale che non supporta il ritorno a capo." "Se contiene filtri o comandi, sostituire con WrapPanel o un componente responsive Ax*."
        }
    }

    if ($fileText -match 'HorizontalScrollBarVisibility\s*=\s*Avalonia\.Controls\.Primitives\.ScrollBarVisibility\.Auto') {
        $first = ($lines | Select-String -Pattern 'HorizontalScrollBarVisibility\s*=.*ScrollBarVisibility\.Auto' | Select-Object -First 1)
        $severity = if ($hasWideRows) { "High" } else { "Medium" }
        Add-Finding $severity "HorizontalAutoScroll" $file.FullName $first.LineNumber "Scorrimento orizzontale automatico presente." "Verificare che sia limitato a una tabella reale e non all'intera pagina."
    }
}

$order = @{ High = 0; Medium = 1; Low = 2 }
$sorted = $findings | Sort-Object @{ Expression = { $order[$_.Severity] } }, File, Line, Rule
$csvPath = Join-Path $resolvedOutput "responsive-ui-findings.csv"
$sorted | Export-Csv -Path $csvPath -NoTypeInformation -Encoding UTF8

$high = @($sorted | Where-Object Severity -eq "High").Count
$medium = @($sorted | Where-Object Severity -eq "Medium").Count
$low = @($sorted | Where-Object Severity -eq "Low").Count
$filesAffected = @($sorted | Select-Object -ExpandProperty File -Unique).Count
$generated = Get-Date -Format "yyyy-MM-dd HH:mm:ss"

$markdown = New-Object System.Collections.Generic.List[string]
$markdown.Add("# Responsive UI Audit")
$markdown.Add("")
$markdown.Add("Generato: $generated")
$markdown.Add("")
$markdown.Add("- File C# analizzati: $($files.Count)")
$markdown.Add("- File con segnalazioni: $filesAffected")
$markdown.Add("- High: $high")
$markdown.Add("- Medium: $medium")
$markdown.Add("- Low: $low")
$markdown.Add("")
$markdown.Add("## Priorita High")
$markdown.Add("")
if ($high -eq 0) {
    $markdown.Add("Nessuna criticita ad alta priorita rilevata.")
} else {
    $markdown.Add("| File | Riga | Regola | Dettaglio | Correzione consigliata |")
    $markdown.Add("|---|---:|---|---|---|")
    foreach ($item in $sorted | Where-Object Severity -eq "High") {
        $detail = $item.Detail.Replace("|", "\\|")
        $recommendation = $item.Recommendation.Replace("|", "\\|")
        $markdown.Add("| $($item.File) | $($item.Line) | $($item.Rule) | $detail | $recommendation |")
    }
}
$markdown.Add("")
$markdown.Add("## Come interpretare il report")
$markdown.Add("")
$markdown.Add("- High: probabile contenuto nascosto o finestra non ridimensionabile.")
$markdown.Add("- Medium: layout da verificare visivamente prima della migrazione.")
$markdown.Add("- Low: schema non responsive che puo essere corretto quando contiene comandi o filtri.")
$markdown.Add("")
$markdown.Add("Il CSV contiene tutte le segnalazioni, comprese Medium e Low.")

$reportPath = Join-Path $resolvedOutput "responsive-ui-audit.md"
$markdown | Set-Content -Path $reportPath -Encoding UTF8

Write-Host "Responsive UI Audit completato."
Write-Host "File analizzati: $($files.Count)"
Write-Host "High: $high | Medium: $medium | Low: $low"
Write-Host "Report: $reportPath"
Write-Host "CSV: $csvPath"

if ($FailOnHigh -and $high -gt 0) { exit 2 }
