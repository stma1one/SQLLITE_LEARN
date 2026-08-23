param([string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"

$files = Get-ChildItem $ProjectRoot -Include "*.cs","*.xaml" -Recurse -File |
    Where-Object { $_.FullName -notmatch "\\bin\\|\\obj\\" }

if (-not $files.Count) {
    Write-Host "No source files found." -ForegroundColor Red
    exit 2
}

# High-value guardrails for current MAUI versions.
# The script is intentionally not presented as a complete replacement for compiler/analyzer warnings.
$rules = @(
    @{
        Pattern = '\bPage\.IsBusy\b'
        Name = 'Page.IsBusy'
        Replacement = 'ViewModel IsLoading + ActivityIndicator/other current loading UX'
        Source = 'https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-10?view=net-maui-10.0'
    },
    @{
        Pattern = '\bListView\b'
        Name = 'ListView'
        Replacement = 'Review current CollectionView guidance and project requirements'
        Source = 'https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-10?view=net-maui-10.0'
    },
    @{
        Pattern = '\bMessagingCenter\b'
        Name = 'MessagingCenter'
        Replacement = 'Review the current MAUI messaging guidance for the target version'
        Source = 'https://learn.microsoft.com/en-us/dotnet/maui/whats-new/dotnet-10?view=net-maui-10.0'
    }
)

$found = $false

foreach ($rule in $rules) {
    foreach ($file in $files) {
        foreach ($match in Select-String -Path $file.FullName -Pattern $rule.Pattern) {
            $found = $true

            Write-Host "[REVIEW] $($rule.Name) -> $($file.FullName):$($match.LineNumber)" -ForegroundColor Yellow
            Write-Host "  Replacement/review: $($rule.Replacement)"
            Write-Host "  Microsoft Learn: $($rule.Source)"
        }
    }
}

Write-Host ""
Write-Host "This script is a guardrail. Compiler warnings, analyzers, and current Microsoft Learn pages remain authoritative." -ForegroundColor Gray

if ($found) {
    Write-Host "RESULT: CHECK REQUIRED" -ForegroundColor Yellow
    exit 1
}

Write-Host "RESULT: PASS" -ForegroundColor Green
exit 0
