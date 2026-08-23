param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"

$program = Get-ChildItem $ProjectRoot -Filter "MauiProgram.cs" -Recurse -File |
    Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\" } |
    Select-Object -First 1

if (-not $program) {
    Write-Host "MauiProgram.cs not found." -ForegroundColor Red
    exit 2
}

$text = Get-Content $program.FullName -Raw
$files = Get-ChildItem $ProjectRoot -Filter "*.cs" -Recurse -File |
    Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\|\\Tests\\" }

foreach ($file in $files | Where-Object {
    $_.FullName -match "\\Services\\" -or $_.FullName -match "\\ViewModels\\"
}) {
    $name = [IO.Path]::GetFileNameWithoutExtension($file.Name)

    if ($name -match "BaseViewModel") {
        continue
    }

    if ($text -match [regex]::Escape($name)) {
        Write-Host "[OK] $name appears in MauiProgram.cs" -ForegroundColor Green
    }
    else {
        Write-Host "[CHECK] $name does not appear in MauiProgram.cs (may be intentional)." -ForegroundColor Yellow
    }
}

Write-Host "Conservative audit: verify actual DI resolution paths." -ForegroundColor Gray
exit 0
