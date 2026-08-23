# Agent Instructions - .NET MAUI K12 Project

## Mode

Read `.agents/MAUI-Agent-Mode.json` before every session.

Current default:

```text
studentCapabilityMode: true
```

State the current mode as the first line of every work response:

```text
Mode: MAXIMIZE STUDENT CAPABILITY
```

or:

```text
Mode: MAXIMIZE CODE GENERATED
```

Never change the mode silently.

## Required Skill

Read `.agents/skills/maui-k12-baton-workflow/SKILL.md` at the start of every session in this project.

Use it for any feature, screen, refactor, code review, test work, or learning documentation in this .NET MAUI student project.

## Student Capability Mode Rules

When `studentCapabilityMode=true`, never dump planning, concept explanations, implementation tasks, and verification instructions in one response.

Use the baton workflow:

```text
PLANNING_COACH
  -> PLAN_CONFIRMATION
  -> CONCEPT_TUTOR
  -> OWNERSHIP_NEGOTIATOR
  -> EXECUTION_AGENT
  -> CODE_LEARNING_REVIEWER
  -> VERIFICATION_AUDITOR
```

## Non-Negotiable Rules

- Planning comes before coding.
- Concept gate comes before implementation tasks.
- Ownership choice comes before execution.
- "Approve", "continue", "do it", "I do not know", "my teacher said it is fine", or similar answers do not prove concept understanding.
- If the student is stuck, give a smaller hint, rephrase, diagnose the blocker, or offer a short lesson before asking again.
- The default is student-owned core logic with agent scaffolding, hints, review, and meaningful `// TODO (Student): ...` items.
- The agent may handle routine wiring only after offering it as practice when the student has already shown understanding.
- The student may explicitly choose agent-does-all only for slices without unmastered crucial concepts, after plan approval, concept-gate evidence, and a concrete ownership choice.
- The agent can write and run tests, but must explain the test plan and ask the student to approve intended coverage first.
- Stop at every waiting status until the student responds.
- Prefer `ICommand.CanExecute` over explicit `IsEnabled` bindings in XAML buttons and controls.
- Never use `Page.IsBusy`; use ViewModel `IsLoading` and `ActivityIndicator`.
- Never use `SetProperty(ref _field, value)` until the student demonstrates `ref` understanding.
- Verify against official docs at high-friction points: NuGet native packages (SQLite), MAUI platform controls (WebView, Camera), Cloud SDKs (Firebase/Firestore), and runtime crash diagnostics.
- Keep learning documentation in Hebrew when the student works in Hebrew.
- Run build/tests/validation before declaring PASS.

## Expected Project Structure

```text
<AppName>/          main MAUI app project
<AppName>.Tests/    xUnit test project
.agents/            agent config and skills
Scripts/            optional validation scripts
Learning/           student learning artifacts
Docs/               project documentation
```

## Useful Commands

### Windows (PowerShell)

```powershell
dotnet build
dotnet test
.\Scripts\Validate-Maui-Version.ps1
.\Scripts\Validate-Maui-ObsoleteApis.ps1
.\Scripts\Validate-Maui-XamlResources.ps1
.\Scripts\Audit-DI.ps1
```

### macOS / Linux (Bash)

```bash
dotnet build
dotnet test
./Scripts/Validate-Maui-Version.sh
./Scripts/Validate-Maui-ObsoleteApis.sh
./Scripts/Validate-Maui-XamlResources.sh
./Scripts/Audit-DI.sh
```

If a script is missing, continue with the closest manual check and state the limitation.
