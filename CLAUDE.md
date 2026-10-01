# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

## What this is

Enterprise Content Management System (CMS) — a Windows Forms MDI desktop app (C#, .NET Framework 4.5) for folder/file management mapped to org units, with RBAC, confidentiality levels (HC/SC/NC/PC), approval workflows, audit logging, ClamAV scanning, and PDPA consent. Backed by SQL Server (stored procedures, ADO.NET, no ORM). Specs and steering docs live in `.kiro/` (`steering/*.md`, `specs/*/{requirements,design,tasks}.md`); a BRD is at `CMS.Presentation/Resources/BRD2.md`. The root `README.md` is the GitLab template and has no useful content.

## Build & test

Windows only. MSBuild is not on PATH; use the VS 2022 one:

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" CMS.sln /p:Configuration=Debug /nologo /v:minimal
nuget restore CMS.sln          # if packages are missing
```

Tests (`CMS.Tests`) are NUnit + NSubstitute + FsCheck (property-based), targeting **net9.0** while the product is .NET Framework 4.5. The test project does not reference the other projects; it compiles selected source files directly via `<Compile Include="..\CMS.Services\XxxService.cs" />`. **When a new service/model/interface file needs test coverage, add it to `CMS.Tests.csproj`** (interfaces, DTOs, Domain models/enums are already included via wildcards).

```bash
dotnet test CMS.Tests/CMS.Tests.csproj
dotnet test CMS.Tests/CMS.Tests.csproj --filter "FullyQualifiedName~PermissionServiceTests"
dotnet test CMS.Tests/CMS.Tests.csproj --filter "Name=SomeTestName"
```

## Architecture

Layering: `Presentation → Services → Data → Domain`, with `Infrastructure` (logging, config, AD, network drive, ClamAV) used by Services and Presentation. Domain has no dependencies.

- **No DI container.** `CMS.Presentation/Program.cs` is the manual composition root: it builds every repository/service by constructor injection, resolves the Windows/AD identity, auto-registers unknown users (AppRole 1 = Officer), enforces PDPA consent, builds the `UserContext`, then launches `MDI_Container`. Adding a service/repository means wiring it there.
- **Data layer**: `CMS.Data/Repositories/*` call stored procedures via ADO.NET; interfaces in `Interfaces/`. Schema and SPs are numbered scripts in `CMS.Data/Scripts/NNN_Description.sql`, applied **manually** in order. Add new ones with the next number; never reuse or renumber. Many existing scripts are one-off cleanup/debug/sample-data scripts, not schema — check before assuming a script is part of the schema.
- **Services**: all business logic. Every method takes a `UserContext`; services validate, check permissions via `IPermissionService`, call repositories, and must write to `IAuditService` for every mutation (audit log is append-only; see `AuditServiceImmutabilityTests`).
- **Permissions** (`PermissionService`): deny-first evaluation with parent-folder inheritance. `PrincipalType`: 1=User, 2=Role, 3=OrgUnit, 4=Group. Files can additionally carry their own permissions (`FilePermissionService`).
- **Presentation**: WinForms with MVP for the larger screens (`Presenters/` holds presenters + `I*View` interfaces); smaller forms/dialogs are code-behind. Forms have `*.Designer.cs` + `.resx` pairs.
- **Config**: `ConfigManager` reads `CMS.Presentation/app.config` (connection string `CmsDb`, `NetworkDrivePath` for file storage, `EmployeeApiBaseUrl`/`EmployeeApiToken` for employee/org sync). The committed app.config contains real-looking dev DB credentials — don't copy them elsewhere.

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
