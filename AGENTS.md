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
| `CMS.Domain` | POCOs, enums, small pure static rule/lookup helpers (e.g. `KBImportLayoutTypeCatalog`, `KBCategoryTree`) | I/O, repository or service logic |
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
- KMS Forms start with a header comment: allowed usings `CMS.Controllers`, `CMS.Domain`; forbidden `CMS.Services`, `CMS.Data`. Keep it, and put any shared pure helper a Form needs in `CMS.Domain`.
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
- `KMS.m_KBCategory.CategoryID` is IDENTITY in script `03` but a plain `INT NOT NULL` in `CMS_DB`; `KBCategoryRepository.Insert` detects which one it is (not IDENTITY -> `MAX(CategoryID)+1` inside a serializable transaction). Do not assume IDENTITY on other KMS tables either; the real DB may have been created differently from the scripts.

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
- **ArticleCode (Code รวม) is 29 chars** (script `12`; was 27). Law: ends with `00000`, first 5 chars = law `CategoryCode` under `LW` (e.g. `PVI00`; `PVI00` and `PVI01` are different laws), chars 6-9 = section number; the stored `ArticleCode` of a Law article is the first 24 chars (`KBImportLayoutTypeCatalog.ParentCodeLength`). NonLaw: does not end with `00000`, char 25 (`1`-`9`) = the document type (`KMS.m_KBCategory.ImportTypeDigit` of the type category, e.g. `2` -> `OR00`), first 24 chars = parent law section (equals the Law's stored `ArticleCode`); stored as the full 29 chars. All positions/lengths are constants in `KBImportLayoutTypeCatalog` — do not hard-code numbers. Only 29-char files are accepted (no 27-char compatibility). `KBImportCategoryResolver` resolves CategoryID per row from these.
- **Law year categories.** A law that exists in several years under one code (e.g. `PLI00` in 2535 and 2540) is split into child categories `<prefix><พ.ศ. 4 digits>` directly under the law category: `LW > PLI00 > PLI002535`. The year comes from the ArticleCode: digits 10-12 with `2` in front (`535` -> `2535`, see `KBImportLayoutTypeCatalog.GetLawYear`). `KBImportCategoryResolver.TryResolveLawArticle` picks the category: a law that has such children must resolve to one of them (missing year -> row Error, never silently the parent); a law without them keeps using the law category itself. The hierarchy is read from `m_KBCategory` only (the Excel file has none). `MOD-006/08_KMS_MoveLawArticlesToYearCategory.sql` moves articles imported before a split (re-uploading does not move existing codes: an update never touches `CategoryID`).
- **NonLaw year categories.** A NonLaw article lands in a *year* category under its type category: type from char 25 (the category whose `ImportTypeDigit` equals it, e.g. `OR00`), then `<type code><พ.ศ.>` with the year from digits 10-12 (`OR002535`, a direct child of `OR00`; `KBImportCategoryResolver.TryResolveNonLawArticle`). Unknown type digit -> row Error (no `MissingCategory`: there is no code to suggest); missing year category -> row Error carrying `KBImportRow.MissingCategory`; the wizard collects them into one "create these categories" dialog (`BuildMissingCategoryNotice`, also used for missing Law year categories). The year is the parent law's, so two laws of the same year share `OR002535`. The digit -> type category table is a master in the DB (`m_KBCategory.ImportTypeDigit`, script `11`; unique, 1-9, never on `LW` or below it; `KBNonLawTypeMap` builds the lookup, `KBCategoryService` validates it): adding a document type = create the category and set its "รหัสประเภทเอกสาร" on the category page, no code change. Only the format rule (29 chars, not `00000`, digit at position 25) stays in `KBImportLayoutTypeCatalog`. Type labels / cross-reference folder names are the category names. `MOD-006/09_KMS_MoveNonLawArticlesToYearCategory.sql` moves articles imported before the year categories existed. `MOD-006/12_KMS_ExtendCodesTo5CharPrefix.sql` converts existing `CategoryCode` (`PVI`->`PVI00`, `OR`->`OR00`, `PLI2535`->`PLI002535`; `LW` unchanged) and `ArticleCode` (insert `00` after the 3-char law code) with backup tables `KMS.bak_KBCodeExtend_*`; 08/09 assume the new format. Cross-reference folders in the KB tree count a type's sub-categories as that type.
- **Move articles (ย้ายบทความ).** `KBArticleMoveForm` (header button `btnMoveArticles` in `KBMainForm`) -> `KBArticleMoveController` -> `IKBArticleMoveService` -> `IKBArticleRepository.MoveToCategory`: changes only `CategoryID` + `UpdatedBy/UpdatedDate` (no `t_KBVersion` row, one `MOVE_KB_ARTICLE` audit row per article); no role check (everyone can move). Destination must be an existing active category; Law<->NonLaw, parent-with-year-children, `LW` root and Display Profile changes are warnings the user confirms, not blocks. Excel import still never touches `CategoryID`, so re-uploading does not move an article back.
- **Category hierarchy has no depth limit** (`ParentCategoryID`). Use `KBCategoryTree` (Domain) for tree order, indentation, descendants and paths instead of hand-written 2-level loops. The KB tree, category management, the article editor dropdown and the roll-up all use it. In `KBMainForm`, clicking a category node lists its sub-categories' articles too; clicking a title node (e.g. `มาตรา 29` of one year) does not.
- **NonLaw: 1 ArticleCode = 1 row.** A cell holding several codes (one per line) is expanded by `KBExcelReader` into several `KBImportRow`s (`CodeIndex/CodeCount`); each passes, is skipped or fails on its own.
- **Re-uploading updates existing codes.** An ArticleCode already in the DB is never inserted twice; the row is UPDATED (`KBImportProcessor.UpdateExistingLawArticle` / `UpdateExistingNonLawArticle` -> `IKBArticleRepository.UpdateFromImport`). No field comparison: every column the Template supplies a **non-blank** Excel value for is rewritten (Title, Tags (Law), TitleCategory, ContentHTML, Summary, PublishedDate, EffectiveDate), plus `ArticleType`; a blank cell keeps the DB value. **`ArticleCode` and `CategoryID` are never updated** (the category is still resolved during validation, so a missing category is still a row Error). `UpdatedBy='system'`, `UpdatedDate=GETDATE()` are always set. Import never touches Status, ConfidentialityLevel, IsOfficialPolicy, CurrentVersion (except the version bump), ExpiryDate, ReviewDueDate, ViewCount or Created*. The column whitelist lives in `KBArticleRepository.ImportUpdatableColumns`; add a column there before updating it. Law: a new `t_KBVersion` row (MAX+1, `CurrentVersion` bumped in the same transaction) only when the new ContentHTML differs from the stored one (ordinal compare). NonLaw has no version rows. A soft-deleted (`IsDelete`) code is a row Error. Files: if the article already has `t_KBArticleAttachment` rows nothing is downloaded or uploaded (only missing file links EntityType=1/EntityID=FileID and parent-law links both ways are repaired); if it has none, the source file must download first - missing/failed -> `Skip` for that code and nothing is updated.
- **Text fidelity.** Title / TitleCategory / Summary / ContentHTML read through a Template mapping are never `Trim`med (`KBExcelReader.ResolveTemplateExpression(..., preserveWhitespace: true)`; codes, dates and file names still are). ContentHTML goes through `KBImportProcessor.ConvertPlainTextToHtml` (HTML-encode, line break -> `<br>`, tab / indent / runs of spaces -> `&nbsp;`, because the viewer renders it as HTML). Title / TitleCategory / Summary are plain text shown in Label / TextBox / DataGridView, so they are stored exactly as typed and only line breaks become CRLF (`ToPlainText`; a multiline TextBox does not break on a lone LF). Never HTML-encode them. A Law article's Title is derived (`มาตรา N`), not copied from Excel.
- **Law / Decree attachments (`Attachment` mapping on a Law Template, optional).** Not mapped = no file step at all (`KBImportRow.AttachmentFileName == null`). Mapped = a new article must have a file name; many sections (Excel rows) with the **same file name share one file** (one FileID): `KBImportProcessor.SharedFileCache` caches by name for the whole batch (case-insensitive), uses the file already in the destination DMS folder first (`IFileService.GetFilesInFolder`, name = `OriginalFileName`, hidden files ignored; no re-upload, no local file needed, content in DMS is never replaced - DMS "replace" would create a new FileID and soft-delete the old one), otherwise reads the local folder and uploads once. A DMS listing failure is a row Error (never falls back to uploading, which would create `copy(N)` duplicates). NonLaw keeps the per-Excel-row `RowFileCache`. `AttachUploadedFile` must never `DeleteFile` a shared file (`RowFile.ReusedExisting` / `BoundCount > 0`). An existing article without attachments gets the file on re-upload (no file -> row `Skip`); with attachments only the missing file links are repaired (`SyncExistingArticleFiles`).
- Gotchas: `KBArticleService.SaveArticleAttachments` **deletes all existing attachments first**; only call it for an article with none. DMS `UploadFiles` with `OverwriteExisting=false` renames duplicates to `name copy(N).pdf`, so upload once and reuse the FileID. **Attachment source folder:** when the selected Template maps an `Attachment` column the wizard (Step 2, next to `btnImport`) makes the user browse to the local folder that holds the PDFs; `btnImport` stays disabled until one is chosen (`IKBImportWizardController.AttachmentFolderPath` / `SetAttachmentFolderRequired`, passed to `IKBImportProcessorService.InsertArticles(..., attachmentFolder, ...)`). `LawFileDownloader.Download(folder, url, name)` matches the cell's file name against that folder (exact, then case-insensitive; extension included; no sub-folders; a name with a path is rejected); the hyperlink URL is still read but unused. The DMS destination `AttachmentFolderId = 437` in `KBImportProcessor` is still hard-coded (the article editor uses folder 1).

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
