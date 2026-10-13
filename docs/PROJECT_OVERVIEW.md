# Enterprise CMS — ภาพรวมโปรเจ็ค

อัปเดต 2026-10-12 · ฉบับออนไลน์: https://claude.ai/code/artifact/16a234e0-21d1-45c4-a072-9b821280d38f

## ภาพรวม

Enterprise CMS เป็นแอป Desktop แบบ Windows Forms (MDI) สำหรับจัดการเอกสารองค์กร เขียนด้วย C# บน .NET Framework 4.5 และใช้ SQL Server เป็นฐานข้อมูล ผู้ใช้คือพนักงานภายในองค์กรที่ยืนยันตัวตนผ่าน Active Directory

ระบบแบ่งเป็น 4 โมดูลหลักภายใต้หน้าต่าง MDI เดียวกัน:

| โมดูล | ย่อมาจาก | หน้าที่ |
| --- | --- | --- |
| DMS | Document Management | จัดการโฟลเดอร์/ไฟล์ที่ผูกกับหน่วยงาน, สิทธิ์, การแชร์, quota, audit, PDPA |
| KMS | Knowledge Management | คลังบทความความรู้ (KB), หมวดหมู่, เวอร์ชัน, import จาก Excel, dashboard |
| WFE | Workflow Engine | ออกแบบ template workflow, อนุมัติตามขั้นตอน, มอบหมายแทน, SLA และรายงาน |
| RGP | Project / Progress | โครงการ, state, task, สมาชิก/บทบาท, ความคืบหน้า และเอกสารแนบผ่าน DMS |

ไฟล์จริงเก็บบน network drive ส่วน metadata สิทธิ์ และ log อยู่ใน SQL Server

## ฟีเจอร์หลัก

แกนของระบบคือ DMS: ทุกโฟลเดอร์ผูกกับหน่วยงาน (OrgUnit) และทุกการเปลี่ยนแปลงต้องผ่านการตรวจสิทธิ์และบันทึก audit

| ฟีเจอร์ | สิ่งที่ทำ | Service หลัก |
| --- | --- | --- |
| โฟลเดอร์/ไฟล์ | สร้าง/ย้าย/คัดลอก/อัปโหลด, folder template, ลึกสูงสุด 10 ชั้น, recycle bin (soft delete) | `FolderService`, `FileService` |
| สิทธิ์ (RBAC) | ACL ระดับโฟลเดอร์และไฟล์, deny-first, สืบทอดจากโฟลเดอร์แม่ | `PermissionService`, `FilePermissionService` |
| ระดับชั้นความลับ | HC=1, SC=2, NC=3, PC=4 บนโฟลเดอร์/ไฟล์ | `ConfidentialityLevel` enum |
| อำนาจอนุมัติ | กำหนดผู้อนุมัติตามหน่วยงานและระดับตำแหน่ง, import จาก Excel | `FolderAuthorityService`, `ApproverResolver`, `AuthorityImportService` |
| คำขออนุมัติ | คำขอดาวน์โหลด, แชร์, เปลี่ยนสิทธิ์, ขยาย quota ผ่าน Approval Inbox | `ApprovalService` |
| นโยบายดาวน์โหลด | ดาวน์โหลดตรงตามระดับ/ตำแหน่ง/ผู้ใช้ หรือต้องขออนุมัติก่อน | `DownloadPolicyService`, `DownloadApproverConfigService` |
| การแชร์ | แชร์ภายใน (รายผู้รับ, มีวันหมดอายุ) และภายนอก (token), Share Management, Shared With Me | `ShareService`, `ShareTokenService`, `ShareAccessService` |
| Quota | กำหนดพื้นที่ต่อโฟลเดอร์, ขอเพิ่ม, รายงานการใช้งาน | `FolderQuotaService` |
| Audit log | บันทึกทุก mutation แบบ append-only | `AuditService` |
| สแกนไวรัส | สแกนไฟล์ก่อนจัดเก็บด้วย ClamAV | `VirusScanService`, `ClamAVAdapter` |
| PDPA | บังคับยินยอมก่อนใช้งานครั้งแรก, รายงาน PDPA | `PdpaConsentDialog` |
| โครงสร้างองค์กร | Org Master, ซิงค์พนักงาน/หน่วยงานจาก Employee API | `OrgStructureService`, `OrgMasterService`, `EmployeeApiService` |
| แจ้งเตือน | popup และรายการแจ้งเตือนในแอป | `NotificationService` |

บทบาทผู้ใช้ในแอป (AppRole) มี 4 ระดับ: Officer=1, SrOfficer=2, Manager=3, Admin=4 ผู้ใช้ใหม่ถูกลงทะเบียนอัตโนมัติเป็น Officer

## สถาปัตยกรรมและโครงสร้างโปรเจ็ค

ระบบเป็น layered architecture: Presentation → Controllers → Services → Data → SQL Server โดยไม่มี DI container — `Program.cs` สร้างทุก object เองด้วย constructor injection

```
CMS.Presentation (WinForms MDI)  ──────────────┐
  MDI_Container + Modules: DMS, KMS, WFE, RGP  │
  Program.cs = composition root                ▼
        │                              CMS.Infrastructure
        ▼                                ActiveDirectoryHelper
CMS.Controllers                          ClamAVAdapter
  used by DMS, KMS, RGP screens          NetworkDriveHelper
  (WFE presenters call Services)         ConfigManager
        │                                LoggingService (log4net)
        ▼                                      │
CMS.Services (business logic) ────────────────┘
  UserContext + IPermissionService                    │
  IAuditService on every change                       ▼
        │                              External systems
        ▼                                Active Directory, ClamAV server,
CMS.Data (Repositories, ADO.NET + SPs)   Network drive, Employee API,
        │                                cms_config.json
        ▼
   SQL Server

CMS.Domain: models and enums, no dependencies, referenced by every layer
```

Services คือจุดเดียวที่มี business logic ตรวจสิทธิ์ และเขียน audit; Infrastructure ต่อกับระบบภายนอก และถูกใช้ทั้งจาก Services และ Presentation

## Data layer และฐานข้อมูล

`CMS.Data` คุยกับ SQL Server ผ่าน ADO.NET เรียก stored procedure เท่านั้น ไม่มี ORM

- **Repositories** — มีประมาณ 55 คลาสใน `CMS.Data/Repositories/` แต่ละคลาสมี interface คู่กันใน `Interfaces/` ครอบคลุมทั้ง DMS (Folder, FileDocument, Permission, Share, Approval, Audit), KMS (KBArticle, KBCategory, KBVersion), WFE (Workflow, WorkflowStep, WorkflowTemplate, Delegation) และ RGP (Project, ProjectTask, ProjectState, ProjectMember)
- **Domain models** — อยู่ใน `CMS.Domain/Models` และ `Enums` ไม่พึ่งพา project อื่น
- **SQL scripts** — `CMS.Data/Scripts/NNN_Description.sql` รันด้วยมือตามลำดับ เพิ่มไฟล์ใหม่ด้วยเลขถัดไป ห้ามใช้เลขซ้ำหรือเรียงใหม่ ตอนนี้ใน repo เหลือเพียง `071_Share_Batch_Request.sql` และ `072_Share_Permission_Change_Request.sql` พร้อม script แก้ไข/ตรวจสอบไม่มีเลข ส่วน 001–070 ถูกลบออกใน commit "clean solution" (ดูย้อนได้จาก git history)
- **Script แก้ข้อมูล** — ควร preview และ rollback เป็นค่าเริ่มต้น จะ commit ก็ต่อเมื่อตั้ง `@Commit = 1`
- **Audit** — บันทึกอยู่ในตาราง `audit.AuditLog` ใช้พิสูจน์ได้ว่า action ใน UI ทำอะไรจริง (เช่น MoveFolder)

ส่วนไฟล์เอกสารจริงเก็บบน network drive ผ่าน `NetworkDriveHelper` / `NetworkShareConnector` ใน `CMS.Infrastructure`

## ระบบสิทธิ์และความปลอดภัย

`PermissionService` ตัดสินสิทธิ์แบบ deny-first และไล่ขึ้นไปที่โฟลเดอร์แม่ ถ้าไม่มีรายการใดตรงเลยจะปฏิเสธ (default deny)

ลำดับการตรวจสอบที่โฟลเดอร์หนึ่ง:

1. Manager (3) และ Admin (4) ผ่านทันที เว้นแต่มี DENY ที่ระบุตรง
2. คัด ACL ที่ตรงกับผู้ใช้: ตาม User, Role (AppRole), Department หรือ Division ของผู้ใช้
3. มี DENY ที่ตรงกับสิทธิ์ที่ขอ → ปฏิเสธทันที
4. มี ALLOW → อนุญาต (FullControl ครอบทุกสิทธิ์)
5. ไม่มีทั้งสอง และไม่ได้ตัด inheritance → ตรวจที่โฟลเดอร์แม่ซ้ำ
6. ถึงรากหรือถูกตัด inheritance → ปฏิเสธ

ประเภทสิทธิ์ (`PermissionType`): Read=1, Write=2, Delete=3, Modify=4, FullControl=5 ผลลัพธ์ถูก cache ต่อ user+folder และต้อง invalidate เมื่อ ACL หรือข้อมูลผู้ใช้เปลี่ยน การตั้งหรือ propagate สิทธิ์โฟลเดอร์ต้องเป็น Manager ขึ้นไป และทุกครั้งเขียน audit (`GRANT` / `DENY`)

ไฟล์แต่ละไฟล์มีสิทธิ์เฉพาะได้ผ่าน `FilePermissionService` ส่วนการดาวน์โหลดมีชั้นนโยบายแยกอีกชั้นหนึ่ง (`DownloadPolicyService` + `FileDownloadGate`)

การยืนยันตัวตนใช้ Active Directory (`AdAuthService`) เปิดแอปได้ 3 แบบ: ไม่มี argument (แสดง LoginForm), `/u: /p:` (credentials จากโปรแกรมอื่น) หรือ `/token:` (token จากโปรแกรมอื่น)

## Presentation layer และกฎโครงสร้าง Form

`CMS.Presentation` เป็น WinForms MDI มี `MDI_Container` เป็นหน้าต่างหลัก และเมนูโหลดตามสิทธิ์ผู้ใช้ (`MenuActionRegistry`, `MenuTreeViewManager`) หน้าจอแบ่งตามโมดูลใน `Modules/` รวมประมาณ 150 ไฟล์

| โฟลเดอร์ | ตัวอย่างหน้าจอ |
| --- | --- |
| `Modules/DMS` | FolderForm, FileUploadForm, ShareManagementForm, SharedWithMeForm, ApprovalInboxForm, AuditLogForm, RecycleBinForm, OrgStructureForm, PdpaConsentDialog |
| `Modules/KMS` | KBMainForm, KBEditorDialog, ArticleViewerForm, KBCategoryForm, KBImportWizardForm, KBDashboardForm |
| `Modules/WFE` | WorkflowDesignerForm, WorkflowCreateTemplateWizard, WorkflowDashboardForm, WorkflowMonitorForm, SLAReportForm, WorkflowDelegationForm |
| `Modules/RGP` | Frm_Project_List, Frm_Project_Detail, Frm_Project_Task_List, Frm_Task_Dashboard, Frm_Project_Progress_Update |

หน้าใหญ่ใช้ MVP (เช่น `Modules/WFE/Presenters/` คู่กับ interface `I*View`) หน้าเล็กเป็น code-behind โมดูล DMS, KMS และ RGP เรียก logic ผ่าน `CMS.Controllers` อีกชั้นหนึ่ง

กฎไฟล์ Form (บังคับใช้เคร่งครัด):

- ทุก Form ต้องมี `*.Designer.cs` แยก คู่กับ `.resx`
- ห้าม `new Control()`, ห้ามประกาศ field ของ control, ห้าม `Controls.Add()` / `SuspendLayout()` / `BeginInit()` ใน code-behind
- code-behind เก็บเฉพาะ logic และ event handler
- ฟอร์มเก่าที่ยังไม่มี Designer (`RecycleBinForm`, `UserAccessForm`, `UserManagementForm`, `FilePreviewPanel`) อย่าใช้เป็นแบบ และแยก Designer เมื่อแก้ไขมาก

## Build, test และ configuration

Build ได้บน Windows ด้วย MSBuild ของ Visual Studio 2022 (ไม่ได้อยู่ใน PATH) ให้รันจาก PowerShell:

```powershell
& "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" CMS.sln /p:Configuration=Debug /nologo /v:minimal
nuget restore CMS.sln   # ถ้า package หาย
```

- ถ้าแอปเปิดอยู่ `CMS.Presentation.exe` จะถูก lock — compile-check ด้วย `/p:OutDir=$env:TEMP\<dir>\` แทน
- Solution มี 6 project: Domain, Infrastructure, Data, Services, Controllers, Presentation ทั้งหมด target .NET Framework 4.5
- Library หลัก: log4net, Newtonsoft.Json, NPOI (Excel), iTextSharp (PDF), BouncyCastle, BCrypt.Net-Next, SharpZipLib

**Test** — เดิมมี `CMS.Tests` (NUnit + NSubstitute + FsCheck, net9.0) แต่ถูกลบใน commit `05a581d remove test` ตอนนี้จึงไม่มี automated test ใน solution

**Configuration** — `ConfigManager` (`CMS.Infrastructure`) อ่านค่าจากไฟล์กลาง `cms_config.json` บน file share (`CMS_Central_Configs`) ไม่ใช่จาก `app.config` ค่าสำคัญคือ connection string `CmsDb`, path ของ network drive ที่เก็บไฟล์ และ URL/token ของ Employee API อย่าคัดลอก credential ไปที่อื่น

**Startup** — `Program.cs` เป็น composition root: สร้าง repository/service ทุกตัว → login ผ่าน AD → ลงทะเบียนผู้ใช้ใหม่ → ขอความยินยอม PDPA → สร้าง `UserContext` → เปิด `MDI_Container`

## Conventions และเอกสารอ้างอิง

- **Soft delete** — ใช้ `IsDeleted` + `DeletedDate` ผ่าน `SoftDelete()` เว้นแต่ต้องลบจริง
- **Audit** — ทุก mutation ใน Service ต้องเขียน `IAuditService` และ log เป็น append-only
- **UserContext** — ทุก method ใน Service รับ `UserContext` และตรวจสิทธิ์ผ่าน `IPermissionService`
- **Domain model** — มี property alias (`FolderId` ↔ `FolderID`) และ property สำหรับ UI (`OrgUnitName`, `UploadedByName`) ที่ Service เติมให้
- **Confidentiality** — เก็บเป็น int ใน DB ในโค้ดใช้ property `ConfidentialityLevelEnum`
- **ตั้งชื่อ** — DTO เป็น `<Action><Entity>Request` / `<Entity>Result`, private field เป็น `_camelCase`
- **สไตล์โค้ด** — C# 5 ตาม .NET 4.5 (null check แบบเต็ม, `throw new ArgumentNullException("x")`) ห้ามใช้ API ใหม่กว่านั้น
- **เพิ่ม service/repository** — ต้อง wire เองใน `Program.cs` เพราะไม่มี DI container
- **Git** — อย่า stage `bin/`, `obj/`, `.vs/` ทำงานบน branch `Pitakpong` และ merge เข้า `cms_develop` / `main`

เอกสารอ้างอิงใน repo:

| เอกสาร | เนื้อหา |
| --- | --- |
| `CLAUDE.md` | คู่มือสถาปัตยกรรม, build และ convention |
| `CMS.Presentation/Resources/BRD2.md` | Business Requirements Document |
| `docs/superpowers/specs/2026-10-01-authority-excel-import-design.md` | ดีไซน์การ import อำนาจอนุมัติจาก Excel |
| `docs/superpowers/plans/2026-10-01-authority-excel-import.md` | แผนงานของฟีเจอร์เดียวกัน |

ค่า `PrincipalType` ในโค้ดคือ User=1, Role=2, Department=3, Division=4, Group=5, OrgUnit=6
