# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

CMS_DB — internal document management (DMS), knowledge base (KMS), and workflow engine (WFE) system for an insurance-regulator org. .NET Framework 4.5, WinForms desktop client (MDI shell), SQL Server backend accessed via ADO.NET + stored procedures. No web frontend exists yet in code — see "Migration specs" below.

## Build / run

Visual Studio / MSBuild only (old-style `.csproj`, `packages.config`, no `dotnet` CLI support — TargetFrameworkVersion v4.5).

```
msbuild CMS.sln /p:Configuration=Debug
```

Startup project is `CMS.Presentation` (`OutputType=WinExe`, entry point `Program.cs`). Run via Visual Studio (F5) — it's a Windows desktop app, not a service.

**No automated test suite is wired into the solution.** `CMS.Tests` exists on disk but is not referenced by `CMS.sln` and has no source files checked in (only stale `bin`/`obj`). Treat this codebase as untested — manual verification in the running WinForms app is the only check available.

## Architecture

Strict layered dependency chain, enforced by project references (no circular refs, no skipping layers):

```
CMS.Domain          → enums + POCO models, zero dependencies
CMS.Infrastructure   → cross-cutting: ConfigManager, LoggingService (log4net), ActiveDirectoryHelper, NetworkShareConnector/NetworkDriveHelper, ClamAVAdapter
CMS.Data             → repositories (ADO.NET, System.Data.SqlClient). Depends on Domain + Infrastructure.
CMS.Services         → business logic / DTOs / validators. Depends on Domain + Data + Infrastructure.
CMS.Controllers      → MVP presenters/controllers (one interface + one class per screen). Depends on all of the above.
CMS.Presentation     → WinForms views (Forms/Designer.cs) + Program.cs composition root. Depends on all of the above.
```

**No DI container.** Every repository/service/controller is `new`'d up by hand in `CMS.Presentation/Program.cs` and passed down through constructors. When adding a new repository or service, wire it into `Program.cs` in the same style (grouped by feature, passed to the services/controllers that need it).

**Data access pattern** (`CMS.Data/Repositories/*.cs`): mutations go through stored procedures (`CommandType.StoredProcedure`, e.g. `WFE.sp_CreateWFInstance`); reads commonly use inline SQL for flexibility. Connection string comes from `IConfigManager.Get<string>("ConnectionString")` (backed by `app.config`).

**MVP pattern in `CMS.Controllers`**: each screen has an `I*View`/`I*Controller` interface plus a `*Presenter`/`*Controller` implementation that the WinForms `Form` in `CMS.Presentation` implements/consumes. Organized by module folder (`DMS/Document`, `KMS/KB`, `WFE`), mirroring `CMS.Presentation/Modules/{DMS,KMS,WFE}`.

### Three functional modules

- **DMS** (Document Management) — folders, files, permissions (folder + file-level ACLs), org units/HR structure, approvals, external/internal sharing, quotas, recycle bin, audit log. Largest module (~87 forms).
- **KMS** (Knowledge Base) — KB categories/articles/versions, Excel import (NPOI-based `KBExcelReader`/`KBImportProcessor`), dashboard.
- **WFE** (Workflow Engine) — two coexisting engines in `CMS.Services`:
  - **v1.0**: `WorkflowService`, `WorkflowTemplateService`, `StepResolverService`, `ConditionEvaluatorService`, `SLACalculator`, `WorkflowDelegationService` (flat files directly under `CMS.Services/`).
  - **v2.0**: everything under `CMS.Services/Workflow/` — plugin-based engine (`IWorkflowPlugin` + `PluginRegistry`, built-ins: `SendNotificationPlugin`, `CallRiscServerPlugin`, `CalcPremiumPlugin`, `SendOicReportPlugin`), `WorkflowOrchestrator`, `DefinitionParser`/`DefinitionValidator`/`StepGraphCache`, `ConditionEvaluator`. **The v1.0 and v2.0 engines are intentionally kept separate — do not mix their types** (explicit comment in `CMS.Services.csproj`: "WFE v2.0 Engine — DO NOT mix with v1.0 WorkflowService").
  - WFE DB objects live in a `WFE` SQL schema (stored procs prefixed `WFE.sp_*`, tables `WFE.t_WF*`).

### Cross-cutting integration points

- **Active Directory**: login/auth via `AdAuthService` (`CMS.Services`) hitting an external Auth API (`AdAuthApiBaseUrl`/`AdAuthLoginPath` in `app.config`), plus `ActiveDirectoryHelper`/`LdapUserSearchService` for direct LDAP lookups.
- **External APIs** (all configured in `CMS.Presentation/app.config` `appSettings`): Employee directory API, AD Auth API, Menu API (`userinforequest`), Notification server.
- **File storage**: not local disk — files live on a network share (`NetworkDrivePath`/`NetworkShareTarget`), accessed via `NetworkShareConnector`/`NetworkDriveHelper` (pre-authenticated SMB session using `NetworkShareUser`/`NetworkShareDomain`/`NetworkSharePassword`).
- **Virus scanning**: uploads go through `ClamAVAdapter`/`VirusScanService` before being persisted.
- **Menu system**: dynamic, role-driven menu built from the Menu API; `MenuActionKeyCatalog` + `MenuActionKeyAliases.*` config keys map menu action keys to forms.
- Exports: Excel via NPOI, PDF via iTextSharp.

### app.config caution

`CMS.Presentation/app.config` currently has **live SQL Server credentials, network share password, and API keys committed in plaintext**. Be careful never to echo/paste these into logs, commits, PRs, or external tools — treat any edit to this file as touching a secret. `RG_Project/App.config` (see below) has the same issue across five separate connection strings.

## Second executable: RG_Project (RGP — internal Project Management module)

`CMS.sln` builds **two** WinForms MDI apps. `RG_Project` (`OutputType=WinExe`, its own `Program.cs` composition root, own `LoginFrmNew`/`MenuControl` login flow) is a separate entry point for an internal project-tracking module (project templates, tasks, states, progress, member roles, document templates). It targets `v4.7.2` (newer than CMS.Presentation's v4.5) and references `CMS.Domain`/`CMS.Infrastructure`/`CMS.Data`/`CMS.Services`/`CMS.Controllers` as project references, so it shares the same layered stack rather than duplicating it.

- Proper business logic lives in `CMS.Controllers/RGP/*`, DI'd through `CMS.Controllers/RGP/RgpComposition.cs` — an internal lazy-singleton composition root used as a fallback wherever a form `new`'s up a controller ad-hoc mid-method instead of receiving it via constructor from the MDI container.
- **Caution**: `RG_Project/Project/Controllers/*.cs` contains a parallel, older set of controllers with the *same class names* (e.g. `ProjectController`) that hit the DB directly via `ConectionDB`/inline SQL instead of going through `CMS.Services`. These are legacy duplicates, not the ones `RgpComposition` wires up — check which controller a given form actually calls before assuming the layered architecture applies.
- Talks to four databases via its own connection strings in `RG_Project/App.config`: `ConnectionString` (CMS_DB, same DB as the main app), `ProjectDbConnectionString` (BUI_RG), and `BisDbConnectionString`/`CommonDbConnectionString`/`CmsLinkDbConnectionString` (BISDB/BUI_COMMON/CMS_DB on a different server, for cross-system lookups).

## Migration specs (design docs, not implemented)

Two root-level Thai-language spec docs describe a planned WFE migration off WinForms — **no corresponding code exists yet**:

- `wfe_all_pages_web_specification.md` — target: ASP.NET Core Razor Pages, sidebar+content shell, page-by-page mapping from existing `*FormNew.cs` WinForms screens to `.cshtml`/`.cshtml.cs` pages, with SQL/repository call references per page.
- `wfe_database_and_sp_specification.md` — full inventory of WFE stored procedures (`WFE.sp_*`), grouped by concern (instance/step management, templates, delegation, SLA/reporting), each with parameters and calling repository/method.
- `.agents/blazor_interactive_auth.md` — Blazor Web App (.NET 8/9) patterns for interactive render mode + session-restore race conditions, for a *different* (Blazor Server) migration path than the Razor Pages spec above. Treat these two migration docs as alternative explorations, not a settled decision — check with the user which target applies before doing migration work.

When doing WFE migration/spec work, treat these `.md` files as the source of truth for target page structure and DB contracts, and cross-reference against the current WinForms implementation in `CMS.Presentation/Modules/WFE` + `CMS.Controllers/WFE` + `CMS.Services`/`CMS.Services/Workflow`.

## Coding Guidelines

### ❌ ข้อห้ามด้าน Form File Structure (Strictly Enforced)
- ❌ ห้าม `new Control()` ใน `Frm{Name}.cs` เด็ดขาด
- ❌ ห้าม declare control fields (`private TextEdit txt...`) ใน `Frm{Name}.cs`
- ❌ ห้าม `Controls.Add()`, `SuspendLayout()`, `BeginInit()` ใน `Frm{Name}.cs`
- ❌ ห้ามสร้าง Form โดยไม่มีไฟล์ `Frm{Name}.Designer.cs` แยกต่างหาก
