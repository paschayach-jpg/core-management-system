# CMS Project Structure & Architecture
> อัปเดตล่าสุด: พฤษภาคม 2569 | เพิ่ม WFE v2.0 Architecture

## Project Overview

This is a comprehensive **Content Management System (CMS)** with integrated **Document Management System (DMS)** and **Knowledge Management System (KMS)** modules. Built as a Windows Forms desktop application in C#/.NET Framework, it provides enterprise-level document and knowledge management capabilities with Active Directory integration, approval workflows, and comprehensive access control.

---

## Architecture Overview

The project follows a **Layered/N-Tier Architecture** with clear separation of concerns:

```
┌─────────────────────────────────────────────────────────┐
│          CMS.Presentation (UI Layer)                    │
│  - Windows Forms (DMS, KMS Modules)                     │
│  - LoginForm, MainForm, Dialog-based UIs                │
└───────────────────────┬─────────────────────────────────┘
                        │
┌───────────────────────▼─────────────────────────────────┐
│       CMS.Controllers (Controller Layer)                │
│  - Request handling & orchestration                     │
│  - DMS & KMS controllers                                │
└───────────────────────┬─────────────────────────────────┘
                        │
┌───────────────────────▼─────────────────────────────────┐
│        CMS.Services (Business Logic Layer)              │
│  - Core business logic & rules                          │
│  - 25+ service classes for various operations           │
└───────────────────────┬─────────────────────────────────┘
                        │
┌───────────────────────▼─────────────────────────────────┐
│    CMS.Domain / CMS.Data (Data Access Layer)            │
│  - Domain models & entities                             │
│  - Repositories & database access                       │
└───────────────────────┬─────────────────────────────────┘
                        │
┌───────────────────────▼─────────────────────────────────┐
│      CMS.Infrastructure (Cross-Cutting Concerns)        │
│  - Config management, Logging, Security helpers         │
└─────────────────────────────────────────────────────────┘
```

---

## Project Structure Details

### 1. **CMS.Domain** (50 C# files)
**Purpose:** Domain models and business entities

**Key Components:**
- **Models:** Core domain entities
  - `FileDocument.cs` - Document/file representation
  - `Folder.cs` - Folder structure
  - `FilePermission.cs`, `FolderPermission.cs` - Access control
  - `ApprovalRequest.cs`, `ApprovalStep.cs` - Approval workflow
  - `ExternalShare.cs` - External sharing mechanism
  - `KBArticle.cs`, `KBCategory.cs` - Knowledge base articles
  - `AuditLog.cs`, `AuditEntry.cs` - Audit trail
  - `HrDepartment.cs`, `HrDivision.cs`, `HrPosition.cs` - HR hierarchy
  - `UserAccount.cs` - User information
  - `FolderTemplate.cs`, `FolderTemplateItem.cs` - Folder templates

- **Enums:** Business logic constants
  - `ApprovalStatus`, `PermissionType`, `DocumentStatus`
  - User roles, file types, etc.

**Responsibility:** Pure data models with minimal logic; represents the business domain

---

### 2. **CMS.Data** (59 C# files)
**Purpose:** Data access and persistence layer

**Key Components:**
- **Repositories:** Database access patterns
  - `IRepository<T>` interface pattern for all entities
  - Repositories for Files, Folders, Permissions, Approvals, KBArticles, etc.
  
- **Models:** Data layer models/DTOs

- **Interfaces:** Repository contracts defining data operations

- **Scripts:** Database initialization and migration scripts

**Responsibility:** Encapsulates all database operations; prevents direct SQL in upper layers

---

### 3. **CMS.Services** (63 C# files)
**Purpose:** Business logic and core application logic

**Key Services (25+):**

| Service | Purpose |
|---------|---------|
| `AdAuthService` | Active Directory authentication |
| `FileService` | File upload, deletion, versioning |
| `FolderService` | Folder CRUD operations |
| `ApprovalService` | Workflow & approval management |
| `PermissionService` | Permission calculation & validation |
| `ShareAccessService` | Sharing & access delegation |
| `ShareService` | Share link management |
| `ShareTokenService` | Token-based access |
| `KBArticleService` | KB article management |
| `KBCategoryService` | KB category management |
| `MenuService` | Application menu configuration |
| `OrgStructureService` | Organizational structure |
| `EmployeeApiService` | HR employee data integration |
| `AuditService` | Audit logging |
| `FolderAuthorityService` | Folder authority config |
| `FilePermissionService` | File-level permissions |
| `NotificationService` | User notifications |
| `VirusScanService` | File scanning integration |
| `UploadCheckpointService` | Upload session management |

- **DTOs:** Data Transfer Objects for API/Controller contracts
- **Interfaces:** Service contracts in `Interfaces/` folder

**Responsibility:** Contains all business rules, calculations, and workflows; orchestrates repositories

---

### 4. **CMS.Controllers** (106 C# files)
**Purpose:** Request handling and orchestration between UI and Services

**Structure:**
```
CMS.Controllers/
├── DMS/
│   └── Document/
│       ├── Interfaces/
│       │   ├── ILoginAuthController
│       │   ├── ILoginAccountController
│       │   └── (Other DMS controllers)
│       ├── LoginAuthController
│       ├── LoginAccountController
│       ├── FileController
│       ├── FolderController
│       ├── ApprovalController
│       ├── ShareController
│       ├── PermissionController
│       ├── ReportController
│       └── (Other DMS controllers)
├── KMS/
│   └── Interfaces/
│       └── (KMS controller interfaces)
└── Properties/
    └── AssemblyInfo.cs
```

**Key Controllers:**
- **DMS Controllers** (Document Management)
  - `LoginAuthController` - AD authentication
  - `LoginAccountController` - User account management
  - `FileController` - File operations
  - `FolderController` - Folder operations
  - `ApprovalController` - Approval workflows
  - `ShareController` - Sharing management
  - `PermissionController` - Permission control
  - `ReportController` - Reporting features

- **KMS Controllers** (Knowledge Management)
  - KB article management controllers

**Responsibility:** Translates UI requests to service calls; handles errors and response mapping

---

### 5. **CMS.Presentation** (97 C# files)
**Purpose:** Windows Forms UI implementation

**Structure:**
```
CMS.Presentation/
├── Modules/
│   ├── DMS/
│   │   ├── LoginForm.cs - User authentication
│   │   ├── MainForm.cs - Main application window
│   │   ├── FolderForm.cs - Folder management
│   │   ├── FileListForm.cs - File listing
│   │   ├── ApprovalInboxForm.cs - Approval workflows
│   │   ├── ApplyTemplateDialog.cs - Template application
│   │   ├── AuthorityConfigForm.cs - Authority configuration
│   │   ├── ConfigAdminForm.cs - Admin configuration
│   │   ├── CreateOrgUnitDialog.cs - Org unit creation
│   │   ├── EmployeeSearchDialog.cs - Employee lookup
│   │   ├── ShareAccessForm.cs - Share management
│   │   ├── ReportForm.cs - Reporting UI
│   │   ├── AuditLogForm.cs - Audit trail viewing
│   │   ├── DuplicateFileChoiceDialog.cs - Duplicate handling
│   │   └── (Many more forms & dialogs)
│   └── KMS/
│       └── (Knowledge Management UI)
├── Resources/
│   ├── Images (login.png, icons, etc.)
│   └── Localized resources
├── Program.cs - Application entry point
├── App.config - Configuration
└── Properties/
    └── AssemblyInfo.cs
```

**Key Forms:**
1. **LoginForm** - Initial authentication screen with AD integration
2. **MainForm** - Primary application window (navigation hub)
3. **FolderForm** - Folder browser & management
4. **FileListForm** - File listing with operations
5. **ApprovalInboxForm** - Approval task management
6. **AuthorityConfigForm** - Authority/role configuration
7. **ShareAccessForm** - External sharing interface
8. **ReportForm** - Report generation
9. **AuditLogForm** - Audit trail viewer

**UI Features:**
- Rounded corners on forms & controls
- Draggable windows
- Drop shadow effects
- Custom styled buttons & text boxes
- Modal dialogs for operations

**Responsibility:** All user-facing UI; translates user actions to controller calls

---

### 6. **CMS.Infrastructure** (Not detailed, but likely contains)
**Purpose:** Cross-cutting concerns and utilities

**Likely Components:**
- `ConfigManager` - Configuration file handling
- `LoggingService` - Application logging
- `ActiveDirectoryHelper` - AD integration
- `EncryptionHelper` - Password/data encryption
- Database connection management
- File storage helpers
- Virus scan integration

**Responsibility:** Provides shared utilities used across all layers

---

### 7. **CMS.Tests**
**Purpose:** Unit & integration tests

**Responsibility:** Test coverage for services, controllers, and logic

---

## Data Flow Example: File Upload

```
User clicks "Upload" in FileListForm (Presentation)
        ↓
FileController.UploadFile() (Controllers)
        ↓
FileService.SaveFile() (Services)
        ↓
FileRepository.Add() & Commit (Data)
        ↓
Database stores FileDocument record
        ↓
ApprovalService.CreateApprovalRequest() (if needed)
        ↓
AuditService.LogAction() (Audit trail)
        ↓
Return success message to UI
```

---

## Key Technologies & Libraries

- **Framework:** .NET Framework (Windows Forms)
- **Language:** C# (.NET)
- **Database:** SQL Server (inferred)
- **Authentication:** Active Directory (LDAP)
- **Architecture Pattern:** Repository Pattern, MVC-like Controller pattern
- **UI Pattern:** Windows Forms with custom rendering (GDI+)
- **Data Access:** ADO.NET or Entity Framework (inferred)

---

## File Statistics

| Project | Files | Purpose |
|---------|-------|---------|
| CMS.Domain | 50 | Domain models |
| CMS.Services | 63 | Business logic |
| CMS.Controllers | 106 | Request handling |
| CMS.Data | 59 | Data access |
| CMS.Presentation | 97 | UI/Forms |
| CMS.Tests | ? | Unit tests |
| **Total** | **~375+** | Complete application |

---

## Key Features Identified

1. **Document Management**
   - File upload/download with versioning
   - Folder hierarchy management
   - File templates

2. **Access Control**
   - Role-based access (via Authority Config)
   - Permission inheritance
   - Effective permission calculation
   - External sharing with tokens

3. **Approval Workflows**
   - Multi-step approval process
   - Approval inbox & task management
   - Status tracking

4. **Knowledge Base**
   - Articles & categories
   - Dashboard views
   - Searchable content

5. **Organizational Structure**
   - Department/Division hierarchy
   - Position management
   - Employee mapping

6. **Security & Compliance**
   - Audit logging
   - Active Directory integration
   - Login/IP tracking
   - Virus scanning support

7. **Administrative Functions**
   - User account management
   - Configuration management
   - Report generation
   - Menu customization

---

## Development Workflow

1. **UI Changes:** Modify `.Designer.cs` and `.cs` forms in `CMS.Presentation/Modules/`
2. **Business Logic:** Add/modify services in `CMS.Services/`
3. **Data Operations:** Update repositories in `CMS.Data/Repositories/`
4. **Models:** Update domain models in `CMS.Domain/Models/`
5. **Control Flow:** Add/modify controllers in `CMS.Controllers/`
6. **Infrastructure:** Utilities in `CMS.Infrastructure/`

---

## Notable Observations

- Large presentation layer (97 files) suggests complex UI with many forms/dialogs
- Service layer is substantial (63 files) indicating complex business logic
- Architecture promotes separation of concerns with clear layer boundaries
- Heavy use of controllers as orchestrators between UI and services
- Repository pattern for data access abstraction
- Custom GDI+ rendering for UI enhancements
- Active Directory integration for enterprise security

---

## Next Steps for Development

1. Review individual service classes for business rule documentation
2. Map database schema to domain models
3. Document approval workflow state machines
4. Create API documentation for key controllers
5. Establish data migration strategies
6. Performance optimization for large file operations
7. Add comprehensive error handling & logging

---

## WFE v2.0 Architecture (MOD-008 Workflow Engine)

> เอกสารอ้างอิง: `SRS-WFE-v2.0.md`, `SDD-WFE-v2.0.md`, `WFE-Database.md`, `WFE-Wireframe.md`
> Skill guide: `/wfe-engine` (engine) | `/wfe-form-ui` (UI pattern)

### Design Principles ของ WFE v2.0

| Principle | รายละเอียด |
|-----------|-----------|
| Workflow as Data | Flow = JSON definition ใน DB ไม่ต้อง redeploy เมื่อเพิ่ม workflow |
| FrozenDefJSON | Snapshot DefinitionJSON ตอน StartInstance → Instance ไม่ถูก affect เมื่อ Definition เปลี่ยน |
| Polymorphic Reference | `RefType` (VARCHAR) + `RefId` (INT) แทน FK 3 ตัว (LinkedFileID/TaskID/FolderID) |
| Plugin Architecture | AutoTask → `IWorkflowPlugin.ExecuteAsync()` ผ่าน `PluginRegistry` |
| OCC Concurrency | `RowVer` (rowversion) ใน t_WorkflowInstance ป้องกัน race condition |
| Audit HashChain | SHA256 chain ทุก row ใน l_AuditLog ตาม OIC Circular 2566 |

### Layer Structure ของ WFE

```
Presentation (WFE Forms)
    ↓
Controllers/WFE/ (WorkflowController, WorkflowDashboardController, ...)
    ↓
Services/Workflow/ (IWorkflowService → WorkflowOrchestrator)
    ├── Engine/ (WorkflowOrchestrator, ConcurrencyGuard)
    ├── Definition/ (DefinitionService, DefinitionParser, StepGraphCache)
    ├── Rules/ (RuleEngine, ConditionEvaluator/NCalc, SlaCalculator, EscalationChecker)
    └── Plugins/ (IWorkflowPlugin, PluginRegistry, built-in plugins)
        ↓
Data/Repositories/Workflow* (WFERepository, ADO.NET + Stored Procedures)
    ↓
SQL Server 2019 (WFE Schema: m_WorkflowDefinition, t_WorkflowInstance, t_WorkflowStepInstance, ...)
```

### GAP: Current v1.0 vs Target v2.0

| Component | v1.0 (มีอยู่แล้ว) | v2.0 (ต้องสร้าง/แก้) |
|-----------|-------------------|----------------------|
| `IWorkflowService` | Synchronous + UserContext | Async + StartWorkflowDto |
| `WorkflowInstance` | `TemplateVersion`, `CurrentStepId` | + `FrozenDefJSON`, `RefType`, `RefId`, `RowVer`, `TenantId` |
| Condition Engine | ไม่มี | `ConditionEvaluator` (NCalc) |
| AutoTask | ไม่มี plugin | `IWorkflowPlugin` + `PluginRegistry` |
| Concurrency | ไม่มี | `ConcurrencyGuard` (OCC rowversion) |
| AuditLog | ADUserName snapshot | + `HashChain` SHA256 |
| Plugin Registry UI | ไม่มี | `PluginRegistryForm.cs` |

### WFE Forms (Wireframe ↔ Form)

| Wireframe | Form File | สถานะ |
|-----------|-----------|-------|
| WFE-01 Workflow Designer | `WorkflowDesignerForm.cs` | มี |
| WFE-02 Instance Detail | `WorkflowInstanceDetailForm.cs` | มี |
| WFE-03 My Tasks | `WorkflowDashboardFormNew.cs` (master ref) | มี |
| WFE-04 Complete Step | `Dialogs/WorkflowStepActionForm.cs` | มี |
| WFE-05 Monitor Dashboard | `WorkflowMonitorForm.cs` | มี |
| WFE-06 SLA Report | `SLAReportForm.cs` | มี |
| WFE-07 Plugin Registry | **ยังไม่มี** | ต้องสร้าง |
| WFE-08 Start Workflow | `Dialogs/WorkflowStartDialog.cs` | มี |
| WFE-09 Definition List | `WorkflowTemplateListForm.cs` | มี |
| WFE-10 Delegate | `Dialogs/DelegationDialog.cs` | มี |