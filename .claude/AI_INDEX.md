# CMS Project — AI Index (สารบัญสำหรับ AI)
> อัปเดตล่าสุด: พฤษภาคม 2569
> ใช้ไฟล์นี้เป็นจุดเริ่มต้นก่อนทำงานในทุก session

---

## 1. Project Identity

| ข้อมูล | รายละเอียด |
|--------|-----------|
| ชื่อโครงการ | Core Management System (CMS) — โครงการ 2511AL01 |
| Stack | C# .NET Framework 4.5 + WinForms + SQL Server 2019 |
| Pattern | MVP (Presenter/View) + Repository (ADO.NET) |
| DB | SQL Server 2019, Collation: Thai_CI_AS |
| Auth | Active Directory / LDAP |
| File Storage | UNC Path: `\\bui-filecenter\Core Management` |

---

## 2. Skill Files (ใช้ `/skill-name`)

| Skill | เมื่อไหร่ใช้ | ไฟล์ |
|-------|------------|------|
| `/wfe-engine` | implement/แก้ไข WFE Service, Repository, Domain Model, Plugin | `.claude/skills/wfe-engine/SKILL.md` |
| `/wfe-form-ui` | สร้าง/แก้ไข WinForm ใน WFE module | `.claude/commands/wfe-form-ui.md` |
| `/menu-action-standard` | เพิ่ม menu item ใหม่ หรือ wire action key | `.claude/skills/menu-action-standard/SKILL.md` |
| `/menu-action-debug` | debug ปัญหา menu action ไม่ทำงาน | `.claude/skills/menu-action-debug/SKILL.md` |

---

## 3. เอกสาร SRS/SDD ที่มี

### WFE (MOD-008) — เอกสารครบชุด v2.0
| ไฟล์ | เนื้อหา | ใช้เมื่อ |
|------|---------|---------|
| `SRS-WFE-v2.0.md` | Requirements, Business Rules, Use Cases, Sequence Diagrams | เข้าใจ requirements + BR |
| `SDD-WFE-v2.0.md` | Architecture, Class Design, Service Layer, Plugin System, OCC | implement services/repos |
| `WFE-Database.md` | DDL, Stored Procedures, Functions, Sample Data | สร้าง/แก้ DB schema |
| `WFE-Wireframe.md` | UI Wireframe ทุก form (WFE-01 ถึง WFE-10) | ออกแบบ/implement UI |

### เอกสาร CMS หลัก (อยู่ใน project root)
| ไฟล์ | เนื้อหา |
|------|---------|
| `CLAUDE.md` | Build commands, project naming, architecture overview |
| `.claude/CLAUDE.md` | Coding standards, RBAC, Audit rules, Business rules |
| `.claude/PATTERNS.md` | Project structure + WFE v2.0 architecture summary |

---

## 4. Project Structure (ชื่อจริง vs ชื่อในเอกสาร)

| ชื่อในเอกสาร | ชื่อ csproj จริง | หน้าที่ |
|-------------|----------------|---------|
| `CMS.Models` | **`CMS.Domain`** | Domain models + enums |
| `CMS.Common` | **`CMS.Infrastructure`** | AD, ClamAV, log4net, ConfigManager |
| `CMS.DataAccess` | **`CMS.Data`** | Repository interfaces + ADO.NET |
| `CMS.Controllers` | `CMS.Controllers` | MVP Presenters |
| `CMS.Services` | `CMS.Services` | Business logic + DTOs |
| `CMS.Presentation` | `CMS.Presentation` | WinForms UI |
| `CMS.Tests` | `CMS.Tests` | NUnit 3 |

**Dependency flow:** `Presentation → Controllers → Services → Data → Domain`

---

## 5. Module Map (MOD Reference)

| Module | ID | หน้าที่ | Key Forms |
|--------|----|---------|-----------|
| Org Structure | MOD-001 | แผนก/ตำแหน่ง | OrgUnitForm |
| Folder Mgmt | MOD-002 | Folder hierarchy, permission | FolderForm |
| File Upload | MOD-003 | Upload, download, versioning | FileListForm |
| Security | MOD-004 | RBAC, permission | PermissionForm |
| Sharing | MOD-005 | Internal/External share | ShareAccessForm |
| Knowledge Base | MOD-006 | KMS articles | KBArticleForm |
| Announcement | MOD-007 | Notification | NotificationForm |
| **Workflow Engine** | **MOD-008** | **WFE v2.0** | **WorkflowDashboardFormNew** (master ref) |
| Task Mgmt | MOD-009 | t_TaskItem | TaskForm |
| Todo | MOD-010 | Personal todo | TodoForm |
| Notes | MOD-011 | Personal notes | NotesForm |
| Work Instructions | MOD-012 | SOP docs | WorkInstructionForm |
| User/Role | MOD-020 | User accounts, roles | UserManagementForm |
| Audit Trail | MOD-021 | l_AuditLog viewer | AuditLogForm |
| Config | MOD-022 | c_SystemConfig | ConfigAdminForm |
| Integration | MOD-023 | External system bridge | (Phase 2) |

---

## 6. WFE v2.0 — Key Facts for AI

### สิ่งที่ต้องสร้างใหม่ (ยังไม่มีใน codebase)
- [ ] `CMS.Services/Workflow/Engine/WorkflowOrchestrator.cs`
- [ ] `CMS.Services/Workflow/Rules/ConditionEvaluator.cs` (NCalc)
- [ ] `CMS.Services/Workflow/Plugins/IWorkflowPlugin.cs` + `PluginRegistry.cs`
- [ ] `CMS.Services/Workflow/Plugins/SendNotificationPlugin.cs` (+ 3 อื่น)
- [ ] `CMS.Services/Workflow/Engine/ConcurrencyGuard.cs`
- [ ] `CMS.Services/Workflow/Definition/StepGraphCache.cs`
- [ ] `CMS.Domain/Models/Workflow/StepGraph.cs` + `StepNode.cs` + `TransitionRule.cs`
- [ ] `CMS.Presentation/Modules/WFE/PluginRegistryForm.cs` (เดียวที่ UI ขาด)
- [ ] DB Migration script — เพิ่ม columns + สร้าง s_WorkflowPlugin, s_WFDelegation
- [ ] NuGet: `NCalc2` (สำหรับ ConditionEvaluator ใน .NET 4.5)

### สิ่งที่มีอยู่แล้ว (อย่าสร้างซ้ำ)
- `WorkflowService.cs`, `IWorkflowService.cs` — มีแต่ต้องอัปเกรด signature
- `WorkflowTemplateService.cs`, `WorkflowDelegationService.cs`
- `WFESLABackgroundJob.cs`, `OptimizedWFESLABackgroundJob.cs`
- `WorkflowTemplateCacheImpl.cs` (refactor เป็น StepGraphCache)
- Forms ทั้งหมดใน `CMS.Presentation/Modules/WFE/` ยกเว้น PluginRegistryForm

### WFE Step Types
`START` | `HumanTask` | `AutoTask` | `DECISION` | `PARALLEL` | `MERGE` | `END`

### WFE Instance Status
`1=Running` | `2=Completed` | `3=Cancelled` | `4=Faulted`

### WFE StepInstance Status
`1=Pending` | `2=InProgress` | `3=Completed` | `4=Skipped` | `5=Overdue`

---

## 7. Coding Rules — Critical

### Audit Stamp (ห้ามผิด)
```csharp
// ✅ ถูก — ADUserName string snapshot
record.CreatedBy = SessionContext.Current.ADUserName;
// ❌ ผิด — ห้ามใช้ UserID FK
record.CreatedByUserID = SessionContext.Current.UserID;
```

### SQL (ห้ามผิด)
```csharp
// ✅ ถูก — Parameterized Query เสมอ
cmd.Parameters.AddWithValue("@FolderID", folderId);
// ❌ ผิด — SQL Injection
$"WHERE FolderID = {folderId}"
```

### Soft Delete (ห้ามลบจริง)
```csharp
// ✅ ถูก — Soft Delete
record.IsDeleted = true; record.DeletedBy = adUser;
// ❌ ผิด — Hard Delete
DELETE FROM t_FileDocument WHERE ...
```

### WFE Concurrency (ต้องทำ)
```csharp
// UPDATE t_WorkflowInstance ต้องมี RowVer check เสมอ
WHERE InstanceID = @id AND RowVer = @expectedRowVer AND TenantId = @tenant
// ถ้า @@ROWCOUNT = 0 → throw ConcurrencyException
```

---

## 8. DB Naming Convention

| Prefix | ประเภท | ตัวอย่าง |
|--------|--------|---------|
| `m_` | Master | `m_WorkflowDefinition`, `m_OrgUnit` |
| `s_` | Setup/Config | `s_WorkflowPlugin`, `s_WFDelegation`, `s_SystemCalendar` |
| `t_` | Transaction | `t_WorkflowInstance`, `t_WorkflowStepInstance`, `t_FileDocument` |
| `l_` | Log (Append-only) | `l_AuditLog` |
| `r_` | Reference | `r_FileType`, `r_Status` |
| `c_` | Configuration | `c_SystemConfig` |

---

## 9. RBAC — Roles & Confidentiality

```csharp
public enum UserRole { Officer=1, SrOfficer=2, Manager=3, Admin=4 }
public enum ConfidentialityLevel {
    HC1_HighConfidential=1,   // สีแดง
    SC2_SemiConfidential=2,   // สีส้ม
    NC3_NormalConfidential=3, // สีน้ำเงิน
    PC4_PublicConfidential=4  // สีเขียว
}
```

**กฎ Permission:** Explicit Deny > Allow | 1HC ห้าม External Share | Sub-folder ไม่ลด Confidentiality

---

## 10. File Locations — หาไฟล์สำคัญ

| ต้องการ | ไปที่ |
|---------|------|
| WFE Forms | `CMS.Presentation/Modules/WFE/` |
| WFE Dialogs | `CMS.Presentation/Modules/WFE/Dialogs/` |
| WFE Presenters | `CMS.Presentation/Modules/WFE/Presenters/` |
| WFE Services | `CMS.Services/Workflow*/` |
| WFE Controllers | `CMS.Controllers/WFE/` |
| WFE Repos | `CMS.Data/Repositories/Workflow*` |
| WFE Domain Models | `CMS.Domain/Models/Workflow*.cs` |
| WFE DTOs | `CMS.Services/DTOs/Workflow*` |
| WFE Background Jobs | `CMS.Services/BackgroundJobs/` |
| Menu Actions | `CMS.Presentation/Modules/DMS/MenuActions/` |
| DB Scripts | `CMS.Data/Scripts/` (001-076 ปัจจุบัน) |
| App Config | `CMS.Presentation/app.config` |
| Master reference form | `CMS.Presentation/Modules/WFE/WorkflowDashboardFormNew.cs` |

---

## 11. App Config Keys ที่ใช้บ่อย

```xml
CmsDb              → connection string → SQL Server 192.168.250.101
NetworkDrivePath   → UNC root file storage
EmployeeApiUrl     → http://150.1.8.28:13001/api
AdAuthApiUrl       → http://150.1.8.28:7653
WFE.StepGraphCacheMinutes       → 30
WFE.EscalationPollIntervalMinutes → 5
WFE.WorkingHoursStart           → 08:30
WFE.WorkingHoursEnd             → 17:30
```

---

## 12. สิ่งที่ไม่ต้องทำซ้ำ (อย่า duplicate)

- ไม่ต้องสร้าง ORM — ใช้ ADO.NET raw SQL + Parameterized Query เท่านั้น
- ไม่ต้อง hardcode connection string — อ่านจาก `ConfigManager` เสมอ
- ไม่ต้อง hardcode UNC path — อ่านจาก `c_SystemConfig.NetworkDriveRoot`
- ไม่ต้องเพิ่ม error handling ที่ "เป็นไปไม่ได้" — validate เฉพาะ system boundary
- ไม่ต้องสร้าง abstract class เพิ่มถ้าไม่จำเป็น — simple is better
- Designer.cs ห้ามมี `const`, local variable, method call ที่ไม่ใช่ `new` + assignment
