# MAUI Specialist Guardrails

Read this reference during `EXECUTION_AGENT` or `VERIFICATION_AUDITOR` when the current slice touches architecture, XAML, Shell navigation, tests, style resources, audit, or Hebrew learning documentation.

## Architecture And State

- Keep the project student-readable. Avoid enterprise abstractions unless they remove real complexity.
- Use clear MVVM boundaries:
  - View: layout, controls, visual states, bindings, accessibility.
  - ViewModel: UI state, commands, validation, service coordination.
  - Service: data access, APIs, persistence, platform operations.
  - Model: data representation.
- Prefer constructor dependency injection.
- Do not instantiate services inside ViewModels.
- Do not perform network, database, or file work in constructors.
- Avoid `.Result` and `.Wait()`.
- Use `async Task` for async work; reserve `async void` for required event handlers.
- Do not use `Page.IsBusy` in new code. Use ViewModel `IsLoading` plus `ActivityIndicator` or another current loading UX.
- Start students with explicit property setters. Do not use `SetProperty(ref _field, value)` until the student demonstrates `ref`, backing fields, equality checks, and `OnPropertyChanged`.
- Prefer `ICommand.CanExecute` over explicit `IsEnabled` bindings in XAML buttons and interactive controls. Use `CanExecute` with meaningful condition and refresh it via `CanExecuteChanged` when relevant state changes.

## Targeted Documentation Verification (High-Friction Triggers)

Do not perform redundant web searches for routine C# or MVVM code. 
However, you MUST actively query official documentation (Microsoft Learn for MAUI, Firebase for Google services) when encountering any of these **High-Friction Triggers**:

1. **Native & Storage NuGet Packages**:
   - When adding or choosing packages for SQLite, local storage, or platform bindings.
   - Purpose: Ensure zero package conflicts and correct platform provider bundles.
2. **Platform & Device-Specific Features**:
   - When using `WebView`, `MediaPicker`, `Geolocation`, `Permissions`, or native hardware controls.
   - Purpose: Ensure correct MAUI-specific XAML controls and lifecycle methods are used (not Web or standard desktop idioms).
3. **Cloud & Third-Party SDK Integration**:
   - When integrating Firebase, Firestore, Authentication providers, or Azure services in a MAUI client.
   - Purpose: Verify current recommended mobile/.NET SDK architecture, initialization hooks, and credentials handling.
4. **Platform Crash / Native Diagnostics**:
   - When encountering native symbol collisions, P/Invoke failures, or platform sandbox exceptions.


## XAML UI

- For Hebrew-first apps, set `FlowDirection="RightToLeft"` at Shell or page level. Do not force RTL onto URLs, emails, code, or identifiers.
- Prefer compiled bindings with `x:DataType` when the binding context is known.
- Always type `DataTemplate` contexts.
- Do not use untyped bindings to silence warnings.
- Use the simplest suitable layout: `Grid`, `VerticalStackLayout`, `HorizontalStackLayout`, `FlexLayout`, or `CollectionView`.
- Avoid arbitrary fixed page dimensions.
- Important controls need meaningful labels, readable states, and accessible touch targets.
- Prefer ViewModel validation state plus binding, `DataTrigger`, or `VisualStateManager`.
- Every `StaticResource`, `DynamicResource`, `BasedOn`, and `x:Key` reference must resolve.
- Put shared styles in `Resources/Styles/Styles.xaml`, shared colors in `Resources/Styles/Colors.xaml`, and page-specific styles in page resources.

## Shell Navigation

- Use Shell as the primary navigation system for Shell-based student projects.
- Root hierarchy belongs in `AppShell.xaml`.
- Detail pages normally use registered routes.
- Prefer route names based on `nameof(PageType)` when practical.
- Navigate with `Shell.Current.GoToAsync(...)`; back navigation uses `".."`.
- Pass small parameters through `Dictionary<string, object>`.
- Prefer passing IDs over large objects when the destination can load data itself.
- Use `IQueryAttributable` or another current documented Shell mechanism when receiving parameters.
- Register Pages and ViewModels in DI when Shell or constructors need to resolve them.
- Do not create an `INavigationService` wrapper unless tests or project requirements justify it.

## Tests

- Use xUnit for ViewModel and Service behavior.
- Use Arrange, Act, Assert.
- Prefer descriptive names such as `SaveCommand_WhenFormInvalid_CannotExecute`.
- Async tests use `async Task`.
- Test behavior, not XAML visual trees.
- Cover initial state, success, failure, validation, loading state, error state, collection updates, command behavior, and `CanExecute` where relevant.
- Use simple manual fakes when mocking libraries are not allowed.
- Reject weak tests such as `Assert.NotNull(vm)` when they cannot fail for the behavior under test.

## Verification Audit

Before declaring PASS, verify with evidence:

```text
1. Target framework/version
2. Current MAUI compatibility
3. Build
4. Architecture boundaries
5. DI
6. Navigation
7. Bindings
8. Styles/resources
9. State/commands
10. Async/error handling
11. Tests
12. Learning documentation
```

Run available checks:

**Windows (PowerShell):**
```powershell
dotnet build
dotnet test
.\Scripts\Validate-Maui-Version.ps1
.\Scripts\Validate-Maui-ObsoleteApis.ps1
.\Scripts\Validate-Maui-XamlResources.ps1
.\Scripts\Audit-DI.ps1
```

**macOS / Linux (Bash):**
```bash
dotnet build
dotnet test
./Scripts/Validate-Maui-Version.sh
./Scripts/Validate-Maui-ObsoleteApis.sh
./Scripts/Validate-Maui-XamlResources.sh
./Scripts/Audit-DI.sh
```

Never claim PASS without evidence. If a check cannot run, state the limitation.

In Student Capability Mode, also verify:
- the plan was approved or planning decisions were documented
- concept-gate evidence exists
- ownership split was chosen before execution
- agent-owned and student-owned tasks are recorded
- `Learning/Student-Mastery.md` reflects demonstrated understanding, not agent-generated code

## Learning Documentation

Maintain `Learning/MAUI-Learning-Book.md` as append-only.

When the student works in Hebrew:
- write explanations in Hebrew
- keep C#, XAML, and .NET identifiers in English

For each meaningful code change, document:
- current phase
- baton workflow stage
- planning decision or concept gate
- ownership split
- what the student did
- what the agent changed
- why the architecture was chosen
- how to verify
- Do not write generic textbook lessons. Teach the actual change.

### Step-by-Step UI Guides (Leomaris Reyes Block Strategy)

When documenting a screen, page, or UI feature, use the `maui-step-guide-author` methodology:
- Follow the 6-part structure: Hook & Design Goal -> Meta-Frame -> Namespaces -> Block Strategy (Visual mockups, Grid backbone, individual blocks) -> Deep-Dive Callouts -> MVVM Wiring.
- Do not dump monolithic XAML files. Provide modular snippets with placeholders (`<!-- Next block goes here -->`).
- Create an HTML mobile mockup at `Learning/Mockups/<ScreenName>.html` and capture `Learning/Images/<ScreenName>-overview.png`.
- Highlight visual patterns: negative margins for floating cards, gradient brushes, rounded borders, shadows, and accessible layouts.
- Keep explanations in Hebrew; keep XAML elements, C# identifiers, and commands in English.
- Save each screen guide at `Learning/Guides/<ScreenName>-Guide.md` and link it in `Learning/MAUI-Learning-Book.md`.

## Student Review Tone

Use supportive review:

```text
What is good
What to improve
Why it matters
Smallest useful fix
```

Do not help disguise generated work. Help the student understand, simplify, and explain it.
