# AGENTS.md

Operational guidance for Codex and other coding agents working in `C:\ProjectBui\core-management-system`.

Last reviewed: 2026-10-01 against branch `popnahaff`. If this file disagrees with the code, trust the code and fix this file.

## Mission

- Make the smallest safe change that fully solves the user's request.
- Preserve the existing architecture: `Presentation -> Controllers -> Services -> Data -> Domain`, with `Infrastructure` supporting cross-cutting concerns.
- Keep WinForms UI code thin, business rules in Services, and SQL access in Data.
- Favor fixes that are easy for the human team to maintain in this .NET 4.5 solution.

## Communication

- Reply to the user in **concise Thai** (the team works in Thai). Code, identifiers, SQL and log text stay as they are.
- User-facing text (labels, messages, exception messages that reach the user) is Thai. Match the surrounding file; comments follow the file (Thai or English).
- Commit or push only when the user asks. Commit messages in this repo are short Thai summaries.
- Minimize output tokens. End with: what changed, what was verified, what was not.

## Project Stack and Build

- C# / .NET Framework 4.5, WinForms with MDI, SQL Server, ADO.NET. No ORM, no DI container.
- `CMS.sln` has 6 projects: `CMS.Domain`, `CMS.Infrastructure`, `CMS.Data`, `CMS.Services`, `CMS.Controllers`, `CMS.Presentation` (the exe). There is **no automated test project**.
- Build with the Visual Studio MSBuild, for example `MSBuild.exe CMS.sln /t:Build /p:Configuration=Debug /m`. If the `packages\` folder is incomplete (typical symptom: `CS8137` / missing `System.ValueTuple`), run `nuget restore CMS.sln` first.
- **Old-style csproj (ToolsVersion 14): files are NOT globbed.** Every new `.cs` file needs a `<Compile Include="...">` entry in its project, and every deleted file needs its entry removed. A file missing from the csproj compiles nowhere and nothing warns you (several `I*View.cs` files lived like that unnoticed until they were deleted on 2026-10-01).
- **Encoding:** all `.cs` files use CRLF line endings. About 100 files (mostly Forms and `*.Designer.cs`) have a UTF-8 BOM, the rest do not. Preserve both exactly when you edit or generate files, otherwise git shows whole-file diffs. There is no `.gitattributes`.
- `package.json` / `node_modules` at the root are tooling only; ignore them. `Law.xlsx` / `NonLaw.xlsx` at the root may exist as untracked sample import files.

## Expected Agent Workflow

1. Read the relevant files before editing anything.
2. State the bug or change mechanism to yourself in one sentence. If it is still fuzzy, keep investigating.
3. Trace the impact through the layer cascade before changing code:
   `Domain -> Data -> Services -> Controllers -> Presentation (Program.cs / MDI_Container.cs)`
4. Make the smallest coherent patch that keeps responsibilities in the correct layer.
5. Build, then validate the changed behavior and adjacent flows (see **Verification**).
6. Summarize what changed, why, and anything not verified.

## Core Architecture Rules

### Layer responsibilities

| Layer | Responsibility | Must not contain |
|---|---|---|
| `CMS.Presentation` | Forms, control wiring, event handlers, view implementation, composition root (`Program.cs`, `MDI_Container.cs`); WFE presenters live here too | SQL, business rules |
| `CMS.Controllers` | Presenter / controller logic, coordination between view contract and services | SQL, direct control manipulation outside the view contract |
| `CMS.Services` | Business rules, validation, orchestration, Excel/file helpers | WinForms UI calls, `MessageBox`, raw control references |
| `CMS.Data` | Repositories, SQL, mapping, persistence | UI logic, business workflow decisions |
| `CMS.Domain` | POCOs, enums, small pure static rule/lookup helpers (e.g. `KBImportLayoutTypeCatalog`) | I/O, repository or service logic |
| `CMS.Infrastructure` | Logging, config (`IConfigManager`), AD, network drive, cross-cutting helpers | Feature-specific UI or SQL workflow logic |

New Forms and Controllers talk to Services only; do not add `using CMS.Data...` to them. Repositories are used by Services and constructed in `Program.cs`. (Legacy exceptions exist: about 30 DMS lookup controllers/presenters, WFE forms/presenters and `FolderForm` reference repositories directly. KMS does not. Do not extend that to new code.)

### Composition rules (who constructs what)

- **`Program.cs`** is the composition root for repositories and shared services. It builds them once and passes them into `new MDI_Container(...)`.
- **`MDI_Container.cs`** builds the per-form object graph on demand in `Open…` / `Create…Form` methods: the controller/presenter, feature helpers (for example `KBExcelReader`, `KBImportProcessor`, `LawFileDownloader`), then wires the form (KMS: `form.AttachPresenter(controller)`) and opens it with `OpenChildForm`.
- **Forms never construct repositories, services, controllers or presenters.**
- Adding a dependency to a factory means: build it in `Program.cs` -> add a constructor parameter + field on `MDI_Container` -> use it in the factory. Do the whole chain in the same change.
- Module forms open as MDI children of `MDI_Container`. Dialogs (`*Dialog`) may be modal. Some forms are embedded in a host panel (`KBImportWizardForm` inside `KBMainForm`).

### View / controller pattern differs per module. Follow the module you are in

| Module | Presenter / controller | View contract | Notes |
|---|---|---|---|
| **KMS** | `CMS.Controllers/KMS/KB/<Feature>Controller.cs` (concrete class, no interface) | `I<Feature>Controller` in `CMS.Controllers/KMS/KB/Interfaces`, **implemented by the Form** | Despite the name, `I<Feature>Controller` here *is the view contract* (e.g. `KBImportWizardForm : IKBImportWizardController`). There are no `I*View` interfaces in KMS. Do not create them. |
| **DMS** | `CMS.Controllers/DMS/Document`: `*Presenter` classes plus many `*Controller` classes with `I*Controller` | `I<Feature>View`, implemented by the Form | Here `I*Controller` is a real controller contract (command/lookup controllers). |
| **WFE** | Presenters in `CMS.Presentation/Modules/WFE/Presenters`; controllers in `CMS.Controllers/WFE` | `I<Feature>View` in `Presenters/Interfaces` | |
| **RGP** | `CMS.Controllers/RGP/*Controller` | none; Forms take concrete controllers | Legacy `Frm_*` naming, `RgpSession`. |

For a new KMS screen copy a small existing one (`KBImportTemplateConfigForm` + `KBImportTemplateConfigController`).

## Editing Rules

- Prefer surgical changes over broad refactors unless the user asks for restructuring.
- **`*.Designer.cs`:** create new controls through the Visual Designer. Do not hand-edit a Designer file unless the user explicitly asks (or the change cannot be done otherwise; then ask first). When you do edit one: change only the exact blocks involved, never regenerate or reformat (these files often already contain pending Visual Studio changes), keep BOM and CRLF, and tell the user to reload the file in Visual Studio.
- Do not `new` WinForms controls in code-behind in new code. (Building `DataGridViewColumn`s for a data grid in code is fine; some older DMS/WFE forms create controls dynamically, so do not treat that as precedent.)
- Keep naming consistent with the existing conventions in the solution.
- Do not move logic upward in the stack just to make a change faster.
- Avoid introducing new dependencies or frameworks unless clearly required.
- Delete dead code only after checking references **and** csproj membership.

## Layer Cascade Rules

When one layer changes, propagate the contract updates all the way upward.

| If you change | Also review and update |
|---|---|
| Domain model property or enum | Repository mapping, service logic, controller/view usage, form binding, SQL script if a column changes |
| Repository interface | Repository implementation, service constructor and calls, `Program.cs` wiring |
| Service interface | Service implementation, controller constructor and calls, `Program.cs` / `MDI_Container.cs` wiring |
| View contract or controller | Controller, form constructor, form interface implementation, `MDI_Container` factory |
| Form field bindings | Controller contract and any dependent service inputs |
| Any added / removed file | The owning `.csproj` |

- If you add a member to an interface, implement it everywhere.
- If you rename a contract member, update all callers consistently.
- If a change breaks the dependency graph, fix the composition root before stopping.

## New Feature / New Form Checklist

Follow this order and do not skip layers (register every new file in its `.csproj`):

1. `CMS.Domain/Models/<Entity>.cs` (and `Enums/` when needed)
2. `CMS.Data/Interfaces/I<Entity>Repository.cs`, then `CMS.Data/Repositories/<Entity>Repository.cs`
3. `MOD-<nnn>/NN_<Module>_<What>.sql` when the schema changes (see **SQL scripts**)
4. `CMS.Services/Interfaces/I<Feature>Service.cs`, then `CMS.Services/<Feature>Service.cs`; optional helper `CMS.Services/<Feature>Processor.cs`
5. View contract + controller following the module pattern above
6. `CMS.Presentation/Modules/<Module>/<Feature>Form.cs` (+ `.Designer.cs` through Visual Studio)
7. `CMS.Presentation/Program.cs` (repositories/services) and `CMS.Presentation/MDI_Container.cs` (factory, menu entry)

### Form rules

Allowed in forms:

```csharp
private void btnSave_Click(object sender, EventArgs e) => _controller.Save();
public string InputText => txtInput.Text.Trim();
```

Not allowed in forms: opening a `SqlConnection`, business validation (`if (txtName.Text.Length < 3) ...`), constructing services/controllers.

## Data Access Rules

- Use ADO.NET only: `SqlConnection`, `SqlCommand`, `SqlDataReader`; connection string from `IConfigManager.Get<string>("ConnectionString")`.
- Wrap every connection and command in `using`. Parameterize every value.
- Keep repositories focused on SQL and mapping only.
- Do not hide failures by returning misleading success values; let Services decide business handling.
- Soft delete: `KMS.t_KBArticle` uses `IsDelete`. Check each query's filter; `ExistsByCode` does **not** filter `IsDelete` while `GetByCode` does.

### SQL scripts (`MOD-006/NN_*.sql`)

- Schema changes ship as a new numbered script (next free `NN`), named `NN_<Module>_<What>.sql`, with a Thai header comment saying what it does and whether it is destructive.
- Make them idempotent and non-destructive (`IF NOT EXISTS` / `OBJECT_ID` guards, `GO` between batches). `03` and `04` are DROP+CREATE bootstrap scripts; do not copy that style for changes to live tables.
- Do not run scripts against a database yourself; hand them to the user. Code must not depend on a column that no script creates.
- `MOD-006/CMS_DataDictionary_Full2.md` describes tables but can lag behind the scripts.

## Service Rules

- Put validation and business rules in Services.
- Throw meaningful exceptions (Thai messages are fine) instead of silently returning `null` or `false` for exceptional conditions; Controllers turn them into `_view.ShowError(...)`.
- Keep UI messaging decisions (dialogs, colors, layout) out of Services.
- Audit-worthy actions go through `IAuditService.Log(new AuditEntry { ... })`.

## Controller / Presenter Rules

- Controllers coordinate between the view contract and services; Forms call controller methods on user actions.
- Controllers depend on interfaces (view contract, service interfaces), not on concrete form internals.

## KMS Import (MOD-006) quick facts

- A **Template** (`KMS.m_KBImportTemplate` + `m_KBImportTemplateColumn`) is a list of mappings `TargetColumnName -> ExcelColumnRef` (column letter, header text, or a `{A}`-style expression). Fixed values live in mapping rows: `ArticleType`, and the virtual `LayoutType` (`Law` / `NonLaw`) and `Attachment` (NonLaw) rows. **CategoryID is not stored in a Template.**
- There are two templates in practice: one Law, one NonLaw. A file is either Law or NonLaw, never mixed. The wizard checks the file kind (from the ArticleCode) against the Template's `LayoutType` before anything else.
- **ArticleCode** is 27 chars. Law: ends with `00000`, first 3 chars = law `CategoryCode` under `LW`. NonLaw: does not end with `00000`, char 23 (`1`-`6`) = `AN/OR/SO/RG/CI/MR`, first 22 chars = parent law section. `KBImportCategoryResolver` resolves CategoryID per row from these.
- **NonLaw: 1 ArticleCode = 1 row.** A cell holding several codes (one per line) is expanded by `KBExcelReader` into several `KBImportRow`s (`CodeIndex/CodeCount`); each passes, is skipped or fails on its own.
- **Re-uploading is safe.** An ArticleCode already in the DB is never inserted twice or overwritten. The wizard only fills what is missing (attachment in `t_KBArticleAttachment`, file link `t_KBArticleLink` EntityType=1/EntityID=FileID, parent-law links both ways). Complete -> `Skip`; missing source file -> `Skip` for that code only.
- Gotchas: `KBArticleService.SaveArticleAttachments` **deletes all existing attachments first**; only call it for an article with none. DMS `UploadFiles` with `OverwriteExisting=false` renames duplicates to `name (1).pdf`, so upload once and reuse the FileID. `LawFileDownloader` still reads a hard-coded local folder (known debt).

## Verification

- Always build the solution after changes (0 errors; the existing `CS0169/CS0414` warnings are pre-existing).
- There are no automated tests. For logic changes write a throwaway harness **outside the repo** (stubs for repository interfaces, synthetic Excel files via NPOI, in-memory state). WinForms screens can be instantiated headlessly and rendered with `DrawToBitmap` to check layout.
- You cannot reach the real database or the DMS file server; say so explicitly and give the user a short manual test list.

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
| Form / dialog | `<Feature>Form`, `<Feature>Dialog` (RGP legacy: `Frm_*`) | `KBImportWizardForm` |
| View contract, KMS | `I<Feature>Controller` (implemented by the Form) | `IKBImportWizardController` |
| View contract, DMS / WFE | `I<Feature>View` | `IFileUploadView` |
| Controller / presenter | `<Feature>Controller`, `<Feature>Presenter` | `KBImportWizardController` |
| Controller interface (DMS) | `I<Feature>Controller` | `IFileUploadController` |
| Service interface / impl | `I<Entity>Service` / `<Entity>Service` | `IKBArticleService` |
| Repository interface / impl | `I<Entity>Repository` / `<Entity>Repository` | `KBArticleRepository` |
| Domain model | `<EntityName>` | `KBArticle` |
| Helper | `<Feature><Role>` | `KBImportProcessor`, `KBExcelReader` |

## File Map

```text
CMS.Domain/            Models/  Enums/
CMS.Infrastructure/    ConfigManager, LoggingService, ActiveDirectoryHelper, network drive helpers, Interfaces/
CMS.Data/              Interfaces/  Repositories/
CMS.Services/          Interfaces/  DTOs/  <Feature>Service.cs  <Feature>Processor.cs  (+ Integrations/, Notifications/, ...)
CMS.Controllers/
  DMS/Document/        presenters + controllers, Interfaces/
  KMS/KB/              controllers, Interfaces/ (view contracts)
  RGP/  WFE/
CMS.Presentation/
  Program.cs  MDI_Container.cs
  Modules/{DMS,KMS,RGP,WFE}/   <Feature>Form.cs + <Feature>Form.Designer.cs   (WFE/Presenters/)
MOD-006/               numbered SQL scripts, SRS / BRD / data dictionary, HTML mockups
```

## Definition Of Done For Agents

- The change follows the intended layer boundaries and the module's existing pattern.
- The solution builds with 0 errors; new/deleted files are reflected in the `.csproj`.
- Dependency wiring still compiles from `Program.cs` through `MDI_Container.cs`.
- Changed interfaces, constructors, and bindings are updated consistently; BOM and CRLF are preserved.
- A schema change comes with a numbered, idempotent SQL script.
- The user-facing behavior requested by the task is addressed, with Thai user-facing text.
- The final response (in Thai) states the verification performed and any remaining uncertainty.
