param(
    [ValidateSet("student","code")]
    [string]$Mode = "student",
    [string]$Root = (Split-Path -Parent $PSScriptRoot)
)

$configPath = Join-Path $Root ".agents\MAUI-Agent-Mode.json"

if ($Mode -eq "student") {
    $value = $true
    $label = "MAXIMIZE STUDENT CAPABILITY"
} else {
    $value = $false
    $label = "MAXIMIZE CODE GENERATED"
}

$config = @{
    studentCapabilityMode = $value
    description = @{
        true = "MAXIMIZE STUDENT CAPABILITY"
        false = "MAXIMIZE CODE GENERATED"
    }
} | ConvertTo-Json -Depth 4

$config | Set-Content $configPath -Encoding UTF8

Write-Host "Agent mode set to: $label" -ForegroundColor Green
