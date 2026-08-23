param(
    [string]$Title,
    [string]$WhatChanged,
    [string]$Why,
    [string]$Concept,
    [string]$Verification = "dotnet build; dotnet test",
    [string]$MicrosoftLearn = ""
)

$root = Split-Path -Parent $PSScriptRoot
$learningDir = Join-Path $root "Learning"
$book = Join-Path $learningDir "MAUI-Learning-Book.md"

New-Item -ItemType Directory -Path $learningDir -Force | Out-Null

if (-not (Test-Path $book)) {
@"
# MAUI Learning Book

This is the student's living e-learning book.

"@ | Set-Content $book -Encoding UTF8
}

$existing = Get-Content $book -Raw
$lessonCount = ([regex]::Matches($existing, '(?m)^# Lesson \d+')).Count
$number = $lessonCount + 1
$date = Get-Date -Format "yyyy-MM-dd HH:mm"

@"

# Lesson $number — $Title

_Date: $date_

## What changed?
$WhatChanged

## Why did we change it?
$Why

## What MAUI/C# concept did we learn?
$Concept

## How to verify

```text
$Verification
```

## Microsoft Learn
$MicrosoftLearn

## Check yourself

1. Can you explain the change without reading the code?
2. Can you explain where the state and logic belong?

"@ | Add-Content $book -Encoding UTF8

Write-Host "Learning lesson $number added." -ForegroundColor Green
