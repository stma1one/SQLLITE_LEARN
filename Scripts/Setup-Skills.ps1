param([string]$Root = (Split-Path -Parent $PSScriptRoot))
$ErrorActionPreference = "Stop"

$skills = @(
    "maui-k12-baton-workflow"
)

foreach ($skill in $skills) {
    $path = Join-Path $Root ".agents\skills\$skill"
    New-Item -ItemType Directory -Path $path -Force | Out-Null
}

New-Item -ItemType Directory -Path (Join-Path $Root "Learning") -Force | Out-Null
New-Item -ItemType Directory -Path (Join-Path $Root ".agents") -Force | Out-Null

$configPath = Join-Path $Root ".agents\MAUI-Agent-Mode.json"

if (-not (Test-Path $configPath)) {
@'
{
  "studentCapabilityMode": true,
  "description": {
    "true": "MAXIMIZE STUDENT CAPABILITY",
    "false": "MAXIMIZE CODE GENERATED"
  }
}
'@ | Set-Content $configPath -Encoding UTF8
}

$currentPhase = Join-Path $Root "Learning\Current-Phase.md"
if (-not (Test-Path $currentPhase)) {
@'
# Current Phase

Status: NOT_STARTED

Mode is controlled by `.agents/MAUI-Agent-Mode.json`.

The orchestrator creates phase details when the first feature begins.
'@ | Set-Content $currentPhase -Encoding UTF8
}

$mastery = Join-Path $Root "Learning\Student-Mastery.md"
if (-not (Test-Path $mastery)) {
@'
# Student Mastery

Use:
- ⚪ Not introduced
- 🟡 Practicing
- 🟢 Understands
- 🔵 Can work independently
'@ | Set-Content $mastery -Encoding UTF8
}

$learningBook = Join-Path $Root "Learning\MAUI-Learning-Book.md"
if (-not (Test-Path $learningBook)) {
@'
# MAUI Learning Book

This is the student's append-only learning book.
'@ | Set-Content $learningBook -Encoding UTF8
}

Write-Host "MAUI K12 skill structure and learning state are ready." -ForegroundColor Green
Write-Host "Default mode: MAXIMIZE STUDENT CAPABILITY" -ForegroundColor Cyan
