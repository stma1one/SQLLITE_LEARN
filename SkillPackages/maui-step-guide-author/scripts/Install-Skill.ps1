param(
    [Parameter(Mandatory = $true)]
    [ValidateSet("Project", "Codex", "ClaudeCode")]
    [string] $Target,

    [string] $ProjectPath = "."
)

$ErrorActionPreference = "Stop"

$scriptDir = Split-Path -Parent $MyInvocation.MyCommand.Path
$packageRoot = Split-Path -Parent $scriptDir
$skillSource = Join-Path $packageRoot "skills\maui-step-guide-author"

if (-not (Test-Path $skillSource)) {
    throw "Cannot find skill source: $skillSource"
}

function Copy-Directory($Source, $Destination) {
    New-Item -ItemType Directory -Force -Path $Destination | Out-Null
    Copy-Item -Path (Join-Path $Source "*") -Destination $Destination -Recurse -Force
}

if ($Target -eq "Project") {
    $resolvedProject = Resolve-Path $ProjectPath

    $projectAgents = Join-Path $resolvedProject ".agents"
    $projectSkillDest = Join-Path $projectAgents "skills\maui-step-guide-author"
    $projectLearning = Join-Path $resolvedProject "Learning"

    # Copy skill files
    Copy-Directory $skillSource $projectSkillDest

    # Create expected Learning subdirectories for guides, mockups, and images
    New-Item -ItemType Directory -Force -Path (Join-Path $projectLearning "Guides") | Out-Null
    New-Item -ItemType Directory -Force -Path (Join-Path $projectLearning "Mockups") | Out-Null
    New-Item -ItemType Directory -Force -Path (Join-Path $projectLearning "Images") | Out-Null

    Write-Host "Installed MAUI Step Guide Author skill into project: $resolvedProject"
    return
}

if ($Target -eq "Codex") {
    $dest = Join-Path $env:USERPROFILE ".codex\skills\maui-step-guide-author"
    Copy-Directory $skillSource $dest
    Write-Host "Installed Codex skill: $dest"
    return
}

if ($Target -eq "ClaudeCode") {
    $dest = Join-Path $env:USERPROFILE ".claude\skills\maui-step-guide-author"
    Copy-Directory $skillSource $dest
    Write-Host "Installed Claude Code skill: $dest"
    return
}
