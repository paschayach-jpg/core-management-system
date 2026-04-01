# Project Structure

## Solution Layout

```
CMS.sln
├── CMS.Domain/          # Domain models and enums — no dependencies on other projects
├── CMS.Infrastructure/  # Cross-cutting concerns (logging, config, AD, network drive, ClamAV)
├── CMS.Data/            # Data access layer (repositories, interfaces, SQL scripts)
├── CMS.Services/        # Business logic layer (services, DTOs)
└── CMS.Presentation/    # WinForms UI layer (forms, dialogs, presenters)
```

## Layer Dependencies

```
Presentation → Services → Data → Domain
                    ↓
              Infrastructure (used by Services and Presentation)
```

Domain has zero dependencies. Infrastructure depends only on Domain.

## Key Folders Per Project

### CMS.Domain
- `Models/` — entity classes (Folder, FileDocument, OrgUnit, Permission, etc.)
- `Enums/` — PermissionType, ConfidentialityLevel, UserRole, ApprovalStatus, etc.

### CMS.Infrastructure
- `Interfaces/` — ILoggingService, INetworkDriveHelper, IConfigService, etc.
- Implementations live directly in the project root

### CMS.Data
- `Interfaces/` — repository interfaces (IFolderRepository, IFileDocumentRepository, etc.)
- `Repositories/` — ADO.NET implementations calling stored procedures
- `Scripts/` — numbered SQL migration scripts (`NNN_Description.sql`)
- `Models/` — query/result models used only by the data layer (AuditQuery, PagedResult)

### CMS.Services
- `Interfaces/` — service interfaces (IFolderService, IFileService, etc.)
- `DTOs/` — request/response objects crossing layer boundaries
- Service implementations in project root (FolderService.cs, FileService.cs, etc.)

### CMS.Presentation
- `Presenters/` — MVP presenters + view interfaces (IFolderView, IPermissionView, etc.)
- Forms and dialogs in project root (`*Form.cs`, `*Dialog.cs`)
- `MDI_Container.cs` — main application shell
- `Program.cs` — entry point and manual DI composition root

## Architecture Patterns

- **Repository pattern** — all DB access behind interfaces in CMS.Data
- **Service layer** — all business logic in CMS.Services; services validate, check permissions, then call repositories
- **MVP (Model-View-Presenter)** — WinForms views implement view interfaces; presenters hold logic
- **Constructor injection** — dependencies injected manually in `Program.cs`; no DI container
- **UserContext** — passed into every service method for permission checks and audit logging
- **Soft delete** — `IsDeleted` flag + `DeletedDate`; use `SoftDelete()` not `PermanentDelete()` unless explicitly required
- **Audit trail** — every mutating operation must call `IAuditService`; audit log is append-only

## Naming Conventions

- Interfaces: `I` prefix — `IFolderRepository`, `IFolderService`
- Private fields: `_camelCase` — `_folderRepository`
- Classes/methods/properties: PascalCase
- DTOs: named as `<Action><Entity>Request` or `<Entity>Result` — `CreateFolderRequest`, `FileDownloadResult`
- SQL scripts: `NNN_PascalCaseDescription.sql` (sequential number, never reuse or renumber)

## Domain Model Notes

- Models include property aliases for compatibility (e.g. `FolderId` ↔ `FolderID`)
- Transient/UI-only properties (e.g. `OrgUnitName`, `UploadedByName`) are populated by the service layer, not the DB
- Confidentiality levels map to int in DB; use the `ConfidentialityLevelEnum` property in code
- Max folder depth is 10 (`MaxFolderDepth` constant in `FolderService`)
