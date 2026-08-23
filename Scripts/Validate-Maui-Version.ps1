param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"

$whatNew = "https://learn.microsoft.com/en-us/dotnet/maui/whats-new/?view=net-maui-10.0"
$dotnet10 = "https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-10?view=net-maui-10.0"
$dotnet11 = "https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-11?view=net-maui-11.0"

Write-Host "Checking Microsoft Learn MAUI documentation..." -ForegroundColor Cyan

$reachable = $true
foreach ($url in @($whatNew, $dotnet10, $dotnet11)) {
    try {
        Invoke-WebRequest -Uri $url -UseBasicParsing -TimeoutSec 15 | Out-Null
    }
    catch {
        $reachable = $false
        Write-Host "WARNING: Could not retrieve $url" -ForegroundColor Yellow
    }
}

$csproj = Get-ChildItem $ProjectRoot -Filter "*.csproj" -Recurse -File |
    Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\" } |
    Select-Object -First 1

if (-not $csproj) {
    Write-Host "No .csproj found." -ForegroundColor Red
    exit 2
}

$text = Get-Content $csproj.FullName -Raw
$match = [regex]::Match($text, '<TargetFrameworks?>\s*([^<]+)')
$tfm = $match.Groups[1].Value

Write-Host "Project: $($csproj.FullName)"
Write-Host "Target framework: $tfm"

if ($tfm -match 'net11') {
    Write-Host "NOTICE: .NET 11 MAUI is currently preview. Do not use preview-only APIs unless explicitly required." -ForegroundColor Yellow
}
elseif ($tfm -match 'net10') {
    Write-Host "Target appears to be .NET 10." -ForegroundColor Green
}
else {
    Write-Host "Target is not clearly net10/net11. Verify the instructor's target version against Microsoft Learn." -ForegroundColor Yellow
}

if (-not $reachable) {
    Write-Host "RESULT: CHECK REQUIRED - live documentation could not be fully retrieved." -ForegroundColor Yellow
    exit 1
}

Write-Host "RESULT: PASS" -ForegroundColor Green
exit 0
