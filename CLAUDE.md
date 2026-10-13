# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Enterprise Content Management System (CMS) — a Windows Forms MDI desktop app (C#, .NET Framework 4.5) for folder/file management mapped to org units, with RBAC, confidentiality levels (HC/SC/NC/PC), approval workflows, sharing, audit logging, ClamAV scanning, and PDPA consent. Backed by SQL Server (stored procedures, ADO.NET, no ORM). Screens are grouped into four modules under `CMS.Presentation/Modules/`: **DMS** (documents/folders, permissions, sharing, quota, audit), **KMS** (knowledge-base articles), **WFE** (workflow engine, SLA), **RGP** (projects/tasks/progress).

Docs: a BRD is at `CMS.Presentation/Resources/BRD2.md`; feature specs/plans go in `docs/superpowers/{specs,plans}/`; a Thai project overview is at `docs/PROJECT_OVERVIEW.md`. There is no README at the repo root.

## Build & test

Windows only. MSBuild is not on PATH; use the VS 2022 one:

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" CMS.sln /p:Configuration=Debug /nologo /v:minimal
nuget restore CMS.sln          # if packages are missing
```

Run MSBuild from PowerShell (Git Bash mangles `/p:` switches). If the app is running, `bin\Debug\CMS.Presentation.exe` is locked; compile-check with `/p:OutDir=$env:TEMP\<dir>\` instead.

The solution has six projects, all .NET Framework 4.5: Domain, Infrastructure, Data, Services, Controllers, Presentation.

**There are no automated tests.** The former `CMS.Tests` project (NUnit + NSubstitute + FsCheck, net9.0) was removed in commit `05a581d`; recover it from git history if tests are needed again. Verify changes by building and running the app.

## Architecture

Layering: `Presentation → Controllers → Services → Data → Domain`, with `Infrastructure` (logging, config, AD, network drive, ClamAV) used by Services and Presentation. Domain has no dependencies.

- **No DI container.** `CMS.Presentation/Program.cs` is the manual composition root: it builds every repository/service by constructor injection, authenticates against AD (`AdAuthService`), auto-registers unknown users (AppRole 1 = Officer), enforces PDPA consent, builds the `UserContext`, then launches `MDI_Container`. Adding a service/repository means wiring it there.
- **Login modes** (`StartupArgs`): no args → `LoginForm`; `/u:<user> /p:<pass>` → credentials passed by another program; `/token:<jwt>` → token passed by another program.
- **Controllers** (`CMS.Controllers/{DMS,KMS,RGP,WFE}`): a thin layer between forms and services. DMS, KMS and RGP screens mostly go through controllers; WFE presenters call services directly. RGP wiring lives in `CMS.Controllers/RGP/RgpComposition.cs`.
- **Data layer**: `CMS.Data/Repositories/*` call stored procedures via ADO.NET; interfaces in `Interfaces/`. Schema and SPs are numbered scripts in `CMS.Data/Scripts/NNN_Description.sql`, applied **manually** in order. Add new ones with the next number; never reuse or renumber. Scripts 001–070 were removed from the working tree in commit "clean solution" (`ee7783f`) and live only in git history; currently only `071`/`072` plus unnumbered fix/diagnostic scripts remain. The latest number in use is 072.
- **Data-fix scripts**: deliver as SQL scripts that preview and roll back by default and commit only when `@Commit = 1`. Audit rows live in `audit.AuditLog`, which is useful for proving what a UI action actually did.
- **Services**: all business logic. Every method takes a `UserContext`; services validate, check permissions via `IPermissionService`, call repositories, and must write to `IAuditService` for every mutation (audit log is append-only).
- **Permissions** (`PermissionService`): deny-first evaluation with parent-folder inheritance and default deny. Manager (3) and Admin (4) bypass ACLs unless an explicit DENY matches. Applicable ACL entries match by User, Role (AppRole), Department or Division; setting/propagating folder permissions requires AppRole ≥ 3. `PrincipalType` enum: 1=User, 2=Role, 3=Department, 4=Division, 5=Group, 6=OrgUnit (the doc comment in `PermissionService.cs` listing 1–4 is stale). `PermissionType`: 1=Read, 2=Write, 3=Delete, 4=Modify, 5=FullControl. Files can additionally carry their own permissions (`FilePermissionService`); downloads go through a separate policy layer (`DownloadPolicyService`, `FileDownloadGate`).
- **Presentation**: WinForms MDI; forms live in `CMS.Presentation/Modules/{DMS,KMS,RGP,WFE}`. MVP is used for the larger WFE screens (`Modules/WFE/Presenters/` holds presenters + `Interfaces/I*View`); smaller forms/dialogs are code-behind. Forms have `*.Designer.cs` + `.resx` pairs. Menus are loaded per user (`MenuActions/MenuActionRegistry`, `MenuTreeViewManager`).
- **Config**: `ConfigManager` does **not** read `app.config`. It loads a JSON file from the `CMS_CONFIG_PATH` environment variable, falling back to the central `cms_config.json` on the `CMS_Central_Configs` network share (path hard-coded in `ConfigManager.cs`). The connection string is `ConnectionStrings.CmsDb`; `Get<T>(key)` resolves `AppSettings.<key>` from the JSON, then falls back to the DB `SystemConfig` table. The config holds real credentials — don't copy them elsewhere.

## Form file structure (strictly enforced)

- ❌ Never `new Control()` in `Frm{Name}.cs`.
- ❌ Never declare control fields (e.g. `private TextEdit txt...`) in `Frm{Name}.cs`.
- ❌ Never call `Controls.Add()`, `SuspendLayout()`, or `BeginInit()` in `Frm{Name}.cs`.
- ❌ Never create a Form without a separate `Frm{Name}.Designer.cs`.

All control creation, field declarations, and layout code belong in the Designer file; the code-behind holds only logic and event handlers. Note: existing forms here are named `{Name}Form`/`{Name}Dialog`, and a few (`RecycleBinForm`, `UserAccessForm`, `UserManagementForm`, `FilePreviewPanel`) have no Designer file yet — these predate the rule; don't copy them for new work, and split them out when you touch them substantially.

## Conventions

- Soft delete (`IsDeleted` + `DeletedDate`); use `SoftDelete()` rather than `PermanentDelete()` unless explicitly required.
- Domain models carry property aliases (e.g. `FolderId` ↔ `FolderID`) and transient UI-only properties (`OrgUnitName`, `UploadedByName`) filled by services, not the DB.
- Confidentiality is an int in the DB; use the `ConfidentialityLevelEnum` property in code.
- Max folder depth is 10 (`MaxFolderDepth` in `FolderService`).
- DTOs: `<Action><Entity>Request` / `<Entity>Result`. Private fields `_camelCase`. Code is C# 5-era style (explicit null checks, `throw new ArgumentNullException("x")`) because of the .NET 4.5 target — avoid newer-only APIs in product projects.
- `bin/`, `obj/`, and `.vs/` are in `.gitignore` but some were committed earlier; don't stage build output.
