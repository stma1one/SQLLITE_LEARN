# Generate-Learning-Book-Pdf.ps1
# Runs the PDF generator script to compile MAUI-Learning-Book.md and all guides in Learning/Guides/

param(
    [string]$ProjectRoot = (Split-Path -Parent $PSScriptRoot),
    [string]$AppName = "",
    [string]$OutputPdf = ""
)

$ErrorActionPreference = "Stop"

$scriptPath = Join-Path $PSScriptRoot "Generate-Learning-Book-Pdf.py"
if (-not (Test-Path $scriptPath)) {
    # Fallback to skill script if not found locally
    $scriptPath = Join-Path $ProjectRoot ".agents\skills\maui-step-guide-author\scripts\generate_book_pdf.py"
}

if (-not (Test-Path $scriptPath)) {
    Write-Error "Could not find generate_book_pdf.py script."
    exit 1
}

$argsList = @("--project-root", $ProjectRoot)
if ($AppName) {
    $argsList += @("--app-name", $AppName)
}
if ($OutputPdf) {
    $argsList += @("--output-pdf", $OutputPdf)
}

Write-Host "Running PDF generation via Python..." -ForegroundColor Cyan
python $scriptPath @argsList

if ($LASTEXITCODE -eq 0) {
    Write-Host "PDF compilation completed successfully." -ForegroundColor Green
} else {
    Write-Error "PDF compilation failed with exit code $LASTEXITCODE"
}
