param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"

if (-not (Test-Path $ProjectRoot)) {
    Write-Host "Project root not found." -ForegroundColor Red
    exit 2
}

$xamlFiles = Get-ChildItem $ProjectRoot -Filter "*.xaml" -Recurse -File |
    Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\" }

if (-not $xamlFiles.Count) {
    Write-Host "No XAML files found." -ForegroundColor Red
    exit 2
}

$defined = @{}
$duplicates = @()

foreach ($file in $xamlFiles) {
    $text = Get-Content $file.FullName -Raw

    foreach ($match in [regex]::Matches($text, 'x:Key\s*=\s*"([^"]+)"')) {
        $key = $match.Groups[1].Value.Trim()

        if ($defined.ContainsKey($key)) {
            $duplicates += [PSCustomObject]@{
                Key = $key
                First = $defined[$key]
                Duplicate = $file.FullName
            }
        }
        else {
            $defined[$key] = $file.FullName
        }
    }
}

$references = @()
$patterns = @(
    '\{StaticResource\s+([^\s\}]+)\}',
    '\{DynamicResource\s+([^\s\}]+)\}',
    'BasedOn\s*=\s*"\{StaticResource\s+([^\s\}]+)\}"',
    'BasedOn\s*=\s*"\{DynamicResource\s+([^\s\}]+)\}"'
)

foreach ($file in $xamlFiles) {
    $text = Get-Content $file.FullName -Raw

    foreach ($pattern in $patterns) {
        foreach ($match in [regex]::Matches($text, $pattern)) {
            $references += [PSCustomObject]@{
                Key = $match.Groups[1].Value.Trim()
                File = $file.FullName
            }
        }
    }
}

$missing = $references |
    Where-Object { -not $defined.ContainsKey($_.Key) } |
    Sort-Object Key, File -Unique

Write-Host "==============================================" -ForegroundColor Cyan
Write-Host " .NET MAUI XAML STYLE/RESOURCE AUDIT" -ForegroundColor Cyan
Write-Host "==============================================" -ForegroundColor Cyan
Write-Host "XAML files scanned: $($xamlFiles.Count)"
Write-Host "Resource keys found: $($defined.Count)"
Write-Host "References found: $($references.Count)"
Write-Host ""

if ($missing.Count) {
    Write-Host "MISSING RESOURCE REFERENCES" -ForegroundColor Red

    foreach ($item in $missing) {
        Write-Host "[$($item.Key)]" -ForegroundColor Red
        Write-Host "    $($item.File)"
    }

    Write-Host ""
}

if ($duplicates.Count) {
    Write-Host "POSSIBLE DUPLICATE RESOURCE KEYS" -ForegroundColor Yellow

    foreach ($item in $duplicates) {
        Write-Host "[$($item.Key)]" -ForegroundColor Yellow
        Write-Host "    first:     $($item.First)"
        Write-Host "    duplicate: $($item.Duplicate)"
    }

    Write-Host ""
}

if (-not $missing.Count -and -not $duplicates.Count) {
    Write-Host "RESULT: PASS" -ForegroundColor Green
    exit 0
}

Write-Host "RESULT: CHECK REQUIRED" -ForegroundColor Yellow
exit 1
