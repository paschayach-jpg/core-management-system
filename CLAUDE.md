# CLAUDE.md

This file provides guidance to Claude Code (claude.ai/code) when working with code in this repository.

> Detailed coding standards, RBAC rules, audit patterns, and business rules are in [.claude/CLAUDE.md](.claude/CLAUDE.md).

---

## Build & Test Commands

```powershell
# Build full solution
msbuild CMS.sln /p:Configuration=Debug /p:Platform="Any CPU"
msbuild CMS.sln /p:Configuration=Release /p:Platform="Any CPU"

# Run all tests (CMS.Tests targets .NET 9.0 SDK-style)
dotnet test CMS.Tests

# Run a single test class or method
dotnet test CMS.Tests --filter "FullyQualifiedName~FolderServiceTests"
dotnet test CMS.Tests --filter "FullyQualifiedName~CreateSubFolder_ExceedsMaxDepth"

# Verbose test output
dotnet test CMS.Tests --logger "console;verbosity=detailed"
```

Test framework: **NUnit 3.14** + **NSubstitute** (mocks) + **FsCheck** (property-based). Use `[Test]` attribute, not `[TestMethod]`.

---

## Actual Project Names

The `.claude/CLAUDE.md` uses planning names. Actual csproj names differ:

| Planning Name | Actual Project | Purpose |
|--------------|----------------|---------|
| `CMS.Models` | **`CMS.Domain`** | Domain models + enums |
| `CMS.Common` | **`CMS.Infrastructure`** | AD auth, ClamAV, log4net, NetworkDrive helpers |
| `CMS.DataAccess` | **`CMS.Data`** | Repository interfaces + ADO.NET implementations |
| `CMS.Controllers` | `CMS.Controllers` | MVP Presenters + View interfaces |
| `CMS.Services` | `CMS.Services` | Business logic, DTOs, validators, background jobs |
| `CMS.Presentation` | `CMS.Presentation` | WinForms UI (70+ forms) |
| `CMS.Tests` | `CMS.Tests` | NUnit test suite |

Dependency flow: `Presentation → Controllers → Services → Data → Domain`  
`Infrastructure` is referenced by `Services` and `Data` (cross-cutting: logging, AD, virus scan, file I/O).

---

## Architecture: MVP Pattern (WinForms)

Each module uses **Model–View–Presenter**:

1. **View Interface** (`CMS.Controllers`) — e.g., `IFolderView` declares properties/events the Form exposes
2. **Presenter** (`CMS.Controllers`) — e.g., `FolderPresenter` holds the logic, calls Services
3. **Form** (`CMS.Presentation`) — implements the View interface, wires events to Presenter

For complex modules (WFE, KMS), there are also dedicated **Lookup Controllers** (read-only queries) and **Command Controllers** (write operations) split from the main Presenter.

---

## Architecture: Menu Action System (DMS)

New menu items require touching three files in `CMS.Presentation/Modules/DMS/MenuActions/`:

| File | Role |
|------|------|
| `MenuActionKeyCatalog.cs` | Declare the action key constant (e.g., `public const string OPEN_FOLDER = "OPEN_FOLDER"`) |
| `MenuActionRegistry.cs` | Register handler: map action key → lambda/method |
| `MDI_Container.cs` | Route action from MDI toolbar/menu through `MenuActionContext` |

The `MenuManagementForm` lets Admin assign action keys to menu nodes stored in the DB (`m_MenuItem`). `MenuService` loads the tree at login.

---

## Architecture: Workflow Engine (WFE)

The WFE (MOD-008) has its own sub-architecture inside `CMS.Services`:

- **`WorkflowService`** — instance lifecycle (start, complete step, reject, cancel)
- **`StepResolverService`** — determines next step(s) from current state + connections
- **`ConditionEvaluatorService`** — evaluates branching expressions on workflow data
- **`SLACalculator`** — computes elapsed/remaining time respecting business hours
- **`WFESLABackgroundJob`** — polls every 5 min (configurable) for overdue steps, triggers escalation
- **`WorkflowIntegrationService`** — bridges WFE events to other modules (Audit, Notification, Task)

WFE forms follow the same MVP pattern; `WorkflowDashboardFormNew` is the canonical reference implementation — see `/wfe-form-ui` skill for its UI conventions.

---

## Key Configuration

`CMS.Presentation/app.config` holds:

| Key | Value |
|-----|-------|
| `CmsDb` connection string | `Data Source=192.168.250.101;Initial Catalog=CMS_DB;...` |
| `NetworkDrivePath` | UNC root for file storage (read at runtime via `ConfigManager`) |
| `EmployeeApiUrl` | `http://150.1.8.28:13001/api` |
| `AdAuthApiUrl` | `http://150.1.8.28:7653` |
| WFE SLA / background job interval | `appSettings` section |

Never hardcode these values — always read via `CMS.Infrastructure.ConfigManager`.

---

## Database Scripts

76 incremental SQL scripts live in `CMS.Data/Scripts/` (named `001_*.sql` … `076_*.sql`). Run them in numeric order when setting up a new DB instance. Scripts are idempotent where possible.
