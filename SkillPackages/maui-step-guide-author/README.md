# MAUI Step Guide Author Skill Package

This package provides the **`maui-step-guide-author`** skill, bringing the acclaimed pedagogical methodology of **Leomaris Reyes** (creator of AskXammy.com and Telerik .NET MAUI guides) into .NET MAUI student projects.

## Key Features

1. **The Block Strategy**: Deconstructs mobile designs into named, color-coded functional blocks before writing code.
2. **Modular XAML with Placeholder Anchors**: Step-by-step layout assembly that avoids dumping large blocks of code at once.
3. **HTML/Browser Visual Mockups**: Creates responsive HTML/CSS mobile mockups captured via browser agents to produce authentic visual progression diagrams.
4. **Didactic Concept Callouts ("✍️ פינת העמקה")**: Explains tricky properties (e.g. negative margins, gradients, `CanExecute`) with official Microsoft Learn references.
5. **K-12 Hebrew Standard**: All teaching and explanations written in fluent Hebrew, keeping code identifiers in clean English.

## Installation

### Into Current Project

**PowerShell:**
```powershell
.\SkillPackages\maui-step-guide-author\scripts\Install-Skill.ps1 -Target Project
```

**Bash:**
```bash
./SkillPackages/maui-step-guide-author/scripts/Install-Skill.sh --target Project
```

### Global Host Installation

**Claude Code:**
```powershell
.\SkillPackages\maui-step-guide-author\scripts\Install-Skill.ps1 -Target ClaudeCode
```

**Codex:**
```powershell
.\SkillPackages\maui-step-guide-author\scripts\Install-Skill.ps1 -Target Codex
```
