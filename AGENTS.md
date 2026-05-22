# AGENTS.md

Operational guidance for Codex and other coding agents working in `C:\ProjectBui\cms`.

## Mission

- Make the smallest safe change that fully solves the user's request.
- Preserve the existing architecture: `Presentation -> Controllers -> Services -> Data -> Domain`, with `Infrastructure` supporting cross-cutting concerns.
- Keep WinForms UI code thin, business rules in Services, and SQL access in Data.
- Favor fixes that are easy for the human team to maintain in this .NET 4.5 solution.

## Project Stack

- C# / .NET Framework 4.5
- WinForms with MDI
- SQL Server
- ADO.NET
- MVP-style presentation flow

## Expected Agent Workflow

1. Read the relevant files before editing anything.
2. State the bug or change mechanism to yourself in one sentence. If it is still fuzzy, keep investigating.
3. Trace the impact through the layer cascade before changing code:
   `Domain -> Data -> Services -> Controllers -> Presentation -> Program.cs`
4. Make the smallest coherent patch that keeps responsibilities in the correct layer.
5. Validate the changed behavior and check for regressions in adjacent flows.
6. Summarize what changed, why, and anything not verified.

## Core Architecture Rules

### Layer responsibilities

| Layer | Responsibility | Must not contain |
|---|---|---|
| `CMS.Presentation` | Forms, control wiring, event handlers, view implementation | SQL, business rules, service construction |
| `CMS.Controllers` | Presenter / controller logic, coordination with view and services | SQL, direct control manipulation outside the view contract |
| `CMS.Services` | Business rules, validation, orchestration | WinForms UI calls, `MessageBox`, raw control references |
| `CMS.Data` | Repositories, SQL, mapping, persistence | UI logic, business workflow decisions |
| `CMS.Domain` | POCOs, enums, simple computed read-only properties | Repository logic, service logic |
| `CMS.Infrastructure` | Logging, config, AD, cross-cutting helpers | Feature-specific UI or SQL workflow logic |

### Composition rules

- Construct repositories and services in `Program.cs` only.
- Do not instantiate repositories or services inside Forms.
- Update constructor wiring in `Program.cs` immediately after any constructor signature change.
- All module forms must open as MDI children of `MDI_Container`.

## Editing Rules

- Prefer surgical changes over broad refactors unless the user asks for restructuring.
- Never manually edit `*.Designer.cs` unless the user explicitly requests designer-file changes and there is no safer alternative.
- Create WinForms controls through the Visual Designer only; do not instantiate UI controls in form code-behind.
- Keep naming consistent with the existing conventions in the solution.
- Do not move logic upward in the stack just to make a change faster.
- Avoid introducing new dependencies or frameworks unless clearly required.

## Layer Cascade Rules

When one layer changes, propagate the contract updates all the way upward.

| If you change | Also review and update |
|---|---|
| Domain model property or enum | Repository mapping, service logic, controller/view usage, form binding |
| Repository interface | Repository implementation, service constructor and calls, `Program.cs` wiring |
| Service interface | Service implementation, controller constructor and calls, `Program.cs` wiring |
| View or controller interface | Controller, form constructor, and form interface implementation |
| Form field bindings | Controller contract and any dependent service inputs |

- If you add a member to an interface, implement it everywhere.
- If you rename a contract member, update all callers consistently.
- If a change breaks the dependency graph, fix the composition root before stopping.

## New Feature / New Form Checklist

Follow this order and do not skip layers:

1. `CMS.Domain/Models/<Entity>.cs`
2. `CMS.Domain/Enums/<Feature>Enum.cs` when needed
3. `CMS.Data/Interfaces/I<Entity>Repository.cs`
4. `CMS.Data/Repositories/<Entity>Repository.cs`
5. `CMS.Services/Interfaces/I<Feature>Service.cs`
6. `CMS.Services/<Feature>Service.cs`
7. Optional helper: `CMS.Services/<Feature>Processor.cs`
8. `CMS.Controllers/<Module>/Interfaces/I<Feature>View.cs`
9. `CMS.Controllers/<Module>/Interfaces/I<Feature>Controller.cs`
10. `CMS.Controllers/<Module>/<Feature>Controller.cs`
11. `CMS.Presentation/Modules/<Module>/<Feature>Form.cs`
12. `CMS.Presentation/Program.cs`
13. `CMS.Presentation/MDI_Container.cs`

### Form rules

Allowed in forms:

```csharp
private void btnSave_Click(object sender, EventArgs e) => _controller.Save();
public string InputText => txtInput.Text.Trim();
```

Not allowed in forms:

```csharp
private void btnSave_Click(object sender, EventArgs e)
{
    var conn = new SqlConnection(...); // No DB access in forms
    if (txtName.Text.Length < 3) { }   // No business validation in forms
}
```

- Use controls declared by the Visual Designer only.
- Do not `new` `Button`, `TextBox`, `Panel`, `Label`, or any other WinForms control inside form code-behind.
- If the UI needs a new control, add it through the Visual Designer so the generated code stays in the designer file.

## Data Access Rules

- Use ADO.NET only: `SqlConnection`, `SqlCommand`, `SqlDataReader`, and related types already used by the project.
- Wrap every connection and command in `using`.
- Keep repositories focused on SQL and mapping only.
- Do not hide failures by returning misleading success values; let Services decide business handling.

## Service Rules

- Put validation and business rules in Services.
- Throw meaningful exceptions instead of silently returning `null` or `false` for exceptional conditions.
- Keep UI messaging decisions out of Services.

## Controller / Presenter Rules

- Controllers coordinate between the view contract and services.
- Forms should call controller methods on user actions.
- Controllers should depend on interfaces, not concrete form internals.

## Troubleshooting Mindset

Before writing a fix:

1. State the mechanism in one sentence.
2. Map runtime constraints first, especially for COM, browser engines, native APIs, timers, focus handling, or WinForms threading.
3. List possible side effects and invariants that the change could break.

During diagnosis:

- If one specific value fails while others pass, assume a structural or runtime constraint first.
- If the same failure returns after multiple patches, stop treating symptoms and identify the actual constraint.
- If a fix introduces a new regression, rethink the approach instead of stacking more patches on top.
- If two attempts fail in the same way, step back and choose a different strategy.

Before declaring success, check:

- The originally failing case now works.
- Previously working cases still work.
- Repeated use does not freeze, deadlock, or corrupt UI state.
- Focus, selection, and MDI behavior still feel consistent.

## Naming Conventions

| Artifact | Convention | Example |
|---|---|---|
| Form | `<Feature>Form` | `KBImportWizardForm` |
| View interface | `I<Feature>View` | `IKBImportWizardView` |
| Controller interface | `I<Feature>Controller` | `IKBImportWizardController` |
| Controller / presenter | `<Feature>Controller` | `KBImportWizardController` |
| Service interface | `I<Entity>Service` | `IKBArticleService` |
| Repository interface | `I<Entity>Repository` | `IKBArticleRepository` |
| Repository implementation | `<Entity>Repository` | `KBArticleRepository` |
| Domain model | `<EntityName>` | `KBArticle` |
| Helper | `<Feature><Role>` | `KBImportProcessor` |

## File Map

```text
CMS.Domain/
  Enums/
  Models/

CMS.Data/
  Interfaces/
  Repositories/

CMS.Services/
  Interfaces/
  <Feature>Service.cs
  <Feature>Processor.cs

CMS.Controllers/<Module>/
  Interfaces/
  <Feature>Controller.cs

CMS.Presentation/
  Modules/<Module>/
    <Feature>Form.cs
    <Feature>Form.Designer.cs
  Program.cs
  MDI_Container.cs
```

## Definition Of Done For Agents

- The change follows the intended layer boundaries.
- Dependency wiring still compiles conceptually from `Program.cs` downward.
- Any changed interfaces, constructors, and bindings are updated consistently.
- The user-facing behavior requested by the task is addressed.
- The final response mentions verification performed and any remaining uncertainty.
