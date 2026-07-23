# CMS Architecture Reference

> เอกสารนี้สรุปสถาปัตยกรรมของโปรแกรม **CMS** (`CMS.sln`) ทั้งระบบ โดยอ้างอิงจากโค้ดจริง เพื่อใช้เป็น "ต้นแบบ" pattern สำหรับการอ้างอิงเมื่อจะออกแบบ/ปรับโครงสร้างโมดูลอื่น ๆ ให้เป็นแนวทางเดียวกัน
>
> ขอบเขตของเอกสาร: **อธิบายเฉพาะสถาปัตยกรรมและ pattern ของ CMS เท่านั้น** ไม่มีการเปรียบเทียบ/แผนปรับปรุงกับโปรเจกต์อื่นในเอกสารนี้

---

## สารบัญ

1. [ภาพรวมสถาปัตยกรรม](#1-ภาพรวมสถาปัตยกรรม)
2. [CMS.Domain](#2-cmsdomain)
3. [CMS.Infrastructure](#3-cmsinfrastructure)
4. [CMS.Data (Repository Layer)](#4-cmsdata-repository-layer)
5. [CMS.Services (Service Layer)](#5-cmsservices-service-layer)
6. [CMS.Controllers (Controller/Presenter Layer)](#6-cmscontrollers-controllerpresenter-layer)
7. [CMS.Presentation (WinForms Layer)](#7-cmspresentation-winforms-layer)
8. [ตัวอย่างสายงานเต็มรูปแบบ #1 — โมดูล DMS: อัปโหลดไฟล์เข้าโฟลเดอร์](#8-ตัวอย่างสายงานเต็มรูปแบบ-1--โมดูล-dms-อัปโหลดไฟล์เข้าโฟลเดอร์)
9. [ตัวอย่างสายงานเต็มรูปแบบ #2 — โมดูล KMS: สร้าง/แก้ไขบทความ + นำเข้า Excel](#9-ตัวอย่างสายงานเต็มรูปแบบ-2--โมดูล-kms-สร้างแก้ไขบทความ--นำเข้า-excel)
10. [Cross-Cutting Concerns](#10-cross-cutting-concerns)
11. [Pattern Checklist (สรุปย่อ)](#11-pattern-checklist-สรุปย่อ)

---

## 1. ภาพรวมสถาปัตยกรรม

CMS แบ่งเป็น 6 โปรเจกต์ (.csproj แยกกัน) ภายใต้ `CMS.sln` โดยมีทิศทางการอ้างอิง (ProjectReference) เป็นชั้น ๆ ดังนี้:

```
CMS.Domain            (ไม่ reference โปรเจกต์อื่นเลย — POCO/Enum ล้วน)
   ▲
CMS.Infrastructure    (ref: Domain)                              — cross-cutting: config, log, AD, ClamAV
   ▲
CMS.Data              (ref: Domain, Infrastructure)               — repositories + interfaces
   ▲
CMS.Services          (ref: Domain, Data, Infrastructure)         — business logic, DTOs, validators
   ▲
CMS.Controllers       (ref: Domain, Data, Services, Infrastructure) — controllers/presenters
   ▲
CMS.Presentation      (ref: Domain, Data, Infrastructure, Services, Controllers) — WinForms UI
```

**ข้อสังเกตสำคัญ:** การแบ่งเลเยอร์นี้ไม่ใช่ "clean architecture" แบบเข้มงวดที่บังคับให้ชั้นบนคุยกับชั้นล่างสุดผ่านชั้นถัดไปเท่านั้น ในทางปฏิบัติ **`CMS.Controllers` และ `CMS.Presentation` สามารถ reference `CMS.Data` ได้โดยตรง** ไม่จำเป็นต้องผ่าน `CMS.Services` เสมอไป — เช่น controller ที่ทำหน้าที่ query/lookup ล้วน ๆ (read-only) มักรับ repository interface เข้ามาตรง ๆ แทนที่จะพันผ่าน service ที่ไม่มี business rule อะไรเพิ่ม การแบ่งชั้นในระบบนี้จึงเป็น "แนวทางที่ยึดถือเป็นหลัก" มากกว่า "กฎที่บังคับด้วยโครงสร้างโปรเจกต์"

---

## 2. CMS.Domain

- ไม่มี ProjectReference ไปยังโปรเจกต์อื่นเลย เป็นชั้นล่างสุดของระบบ
- ประกอบด้วย 2 โฟลเดอร์หลัก:
  - `Models/` — POCO ล้วน ๆ แทนข้อมูลที่ไหลผ่านทุกเลเยอร์ (เช่น `FileDocument.cs`, `Folder.cs`, `KBArticle.cs`, `UserContext.cs`)
  - `Enums/` — enum และ helper ของ enum (เช่น `PermissionType.cs`, `KBCategoryIconHelper.cs`)
- ไม่มี logic ทางธุรกิจอยู่ในชั้นนี้ (ยกเว้น helper แปลงค่า enum เล็กน้อย เช่น `KBCategoryIconHelper.ToSymbol(...)`)
- โมเดลบางตัวมี "alias property" เพื่อความเข้ากันได้ย้อนหลัง เช่น `FileDocument.FileDocumentId` เป็น alias ของ `FileID`, `IsDeleted` คำนวณจาก `DeletedDate != null`

---

## 3. CMS.Infrastructure

ชั้นนี้รวม cross-cutting concern ที่ไม่ผูกกับ business logic โดยตรง:

| ไฟล์ | หน้าที่ |
|---|---|
| `ConfigManager.cs` (impl `IConfigManager`) | อ่านค่า config แบบ 3-tier fallback: (1) in-memory cache → (2) `app.config` appSettings → (3) ตาราง `SystemConfig` ใน DB (ผ่าน `sp_GetSystemConfig`). การ **เขียนค่า** (`Set`) ต้องเช็ค `UserContext.AppRole == 4` (Admin) เท่านั้น และเขียนลง DB เสมอผ่าน `sp_UpsertSystemConfig` |
| `ActiveDirectoryHelper.cs` | ผูกกับ Active Directory |
| `LoggingService.cs` (impl `ILoggingService`) | log4net wrapper |
| `ClamAVAdapter.cs` / `ClamAVResult.cs` | เชื่อมต่อ ClamAV สำหรับ virus scan |
| `NetworkDriveHelper.cs` / `NetworkShareConnector.cs` | อ่าน/เขียนไฟล์บน network share (physical storage ของเอกสาร) |
| `DatabaseConnectionTester.cs` | ทดสอบการเชื่อมต่อฐานข้อมูล |

**หลักการสำคัญ:** ทุก repository ในชั้น `CMS.Data` รับ `IConfigManager` เข้ามาทาง constructor เพื่อดึง connection string — ไม่มี repository ไหนอ่าน `ConfigurationManager.AppSettings` ตรง ๆ เอง

---

## 4. CMS.Data (Repository Layer)

### Pattern การเข้าถึงข้อมูล

- ใช้ **ADO.NET ล้วน ๆ** (`SqlConnection`/`SqlCommand`) — **ไม่มี ORM หรือ Dapper**
- เกือบทั้งหมด (~34 repository) เรียกผ่าน **stored procedure** (`CommandType.StoredProcedure`) ไม่ใช้ inline SQL (ยกเว้นบางจุดเฉพาะกิจ)
- **ไม่มี base repository class** — แต่ละ repository เปิด/ปิด `SqlConnection` ของตัวเองทุกครั้งที่เรียก (`using` block ต่อ method)
- แต่ละ repository มี interface คู่กันเสมอ ใน `CMS.Data/Interfaces/IXxxRepository.cs` (1:1 กับ `CMS.Data/Repositories/XxxRepository.cs`)
- Mapping จาก `SqlDataReader` ไป domain model ทำด้วยมือผ่าน static helper method (เช่น `MapFolder(reader)`) ไม่มี auto-mapper

### ตัวอย่างโค้ด (`FolderRepository.cs`)

```csharp
public class FolderRepository : IFolderRepository
{
    private readonly string _connectionString;
    public FolderRepository(IConfigManager configManager)
    {
        _connectionString = configManager.Get<string>("ConnectionString");
    }

    public int Insert(Folder folder)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand("storage.sp_InsertFolder", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FolderName", folder.FolderName);
                // ... parameter อื่น ๆ
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}
```

### ข้อยกเว้นที่ควรรู้: KMS ใช้ inline parameterized SQL แทน stored procedure

ต่างจาก DMS ที่ใช้ stored procedure ล้วน ๆ, repository ของโมดูล KMS (เช่น `KBArticleRepository.cs`) เขียน SQL แบบ inline พร้อม parameter (`AddWithValue`, ป้องกัน SQL injection ด้วย parameterized query เหมือนกัน) แทนการเรียก stored procedure:

```csharp
public int Insert(KBArticle article)
{
    using (var conn = new SqlConnection(_connectionString))
    {
        conn.Open();
        const string sql = @"INSERT INTO KMS.t_KBArticle (...) VALUES (...); SELECT SCOPE_IDENTITY();";
        using (var cmd = new SqlCommand(sql, conn))
        {
            AddParams(cmd, article);
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}
```

สรุป: **ทั้งสองแบบ (stored procedure และ inline parameterized SQL) ถือเป็น pattern ที่ยอมรับได้ในระบบนี้** เลือกใช้ตามความเหมาะสมของโมดูล ไม่มีกฎตายตัวว่าต้องใช้แบบใดแบบหนึ่งเท่านั้น แต่ **ต้องเป็น parameterized เสมอ ห้าม concat string เข้า SQL ตรง ๆ**

### โมเดลข้อมูลสนับสนุน

- `CMS.Data/Models/PagedResult.cs` — ห่อผลลัพธ์แบบแบ่งหน้า
- `CMS.Data/Models/AuditQuery.cs` — พารามิเตอร์ query สำหรับ audit log

---

## 5. CMS.Services (Service Layer)

### Dependency Injection แบบ manual (ไม่มี container)

Service รับ dependency ผ่าน **constructor injection ด้วย interface type** เท่านั้น (ไม่มี Unity/Autofac/Ninject/SimpleInjector หรือ container ใด ๆ ในระบบทั้งหมด) dependency ที่ไม่จำเป็นเสมอไปจะใช้ **default-parameter (`= null`)** แทนการทำ constructor overload หลายตัว:

```csharp
public FolderService(
    IFolderRepository folderRepository,
    IFileDocumentRepository fileDocumentRepository,
    IPermissionRepository permissionRepository,
    IPermissionService permissionService,
    INetworkDriveHelper networkDriveHelper,
    IAuditService auditService,
    IFolderTemplateRepository folderTemplateRepository = null,
    IApprovalRepository approvalRepository = null,
    IUserOrgVisibilityService visibilityService = null,
    IUserAccountRepository userAccountRepository = null)
```

**Composition root อยู่ที่จุดเดียว:** `CMS.Presentation/Program.cs::Main` เป็นบล็อกยาวราว ~150 บรรทัดที่ `new` repository ทุกตัว → `new` service ทุกตัว (ส่ง repository เข้าไป) → `new` controller ทุกตัว (ส่ง service/repository เข้าไป) → สุดท้าย `new MDI_Container(...)` รับ 40+ named parameter ไม่มี factory/module registration ใด ๆ — ทุกการต่อสายอยู่ที่ไฟล์เดียวนี้

### การประกอบ business rule เข้าไว้ใน service

Service คือที่รวม business logic ทั้งหมด (ไม่ใช่ controller หรือ repository) ตัวอย่างจาก `FileService.UploadFiles`:

1. ตรวจสอบสถานะ folder (ห้าม deleted/archived)
2. ตรวจสอบสิทธิ์แบบ **dual-gate**: `user.AppRole >= 3` (privileged) **หรือ** `user.CanAddToFolder(confidentialityLevel)` (org-authority) **หรือ** `IPermissionService.CheckPermission(...)` (ACL) — ถ้าไม่ผ่านทั้งคู่ throw `UnauthorizedAccessException`
3. ตรวจสอบ quota ระดับ org ก่อนเริ่ม batch (`IFolderQuotaService.GetUsage`)
4. ต่อไฟล์: ตรวจนามสกุล/ขนาด → จัดการชื่อซ้ำ/versioning → ตรวจ quota ระดับโฟลเดอร์ → คำนวณ checksum (SHA-256) → virus scan ผ่าน `IVirusScanService` → เขียนไฟล์จริงผ่าน `INetworkDriveHelper` → insert record ผ่าน repository → propagate permission จาก folder ไปยัง file → อัปเดตขนาด/จำนวนไฟล์ของโฟลเดอร์ → **audit log**
5. หากเกิด exception ระหว่างทาง: **rollback** (ลบไฟล์จริงที่เขียนไปแล้ว + soft-delete record ใน DB) แล้วบันทึก audit log ผลลัพธ์ล้มเหลว

รูปแบบนี้ (auth dual-gate → quota → checksum/scan → เขียนจริง → เขียน DB → propagate permission → audit → rollback เมื่อ error) คือ **pattern มาตรฐานของ service ที่ทำงานกับไฟล์/ทรัพยากรภายนอกร่วมกับ DB** ในระบบนี้

### State ระดับ session: GlobalData / SessionContext / UserContext

สามคลาสนี้มีบทบาทต่างกันชัดเจน อย่าสับสน:

| คลาส | ตำแหน่ง | บทบาท |
|---|---|---|
| `GlobalData` (static class) | `CMS.Services` | เก็บ state ระดับ UI/session แบบ global bag: `Token`, `CombinedList`, `MenuActions` (`List<MenuAction>`), `UserMenuModel` — ตั้งค่าครั้งเดียวหลัง login เพื่อคุมสิทธิ์เมนู |
| `SessionContext` (singleton `.Current`) | `CMS.Services` | เก็บ auth token, `UserID`, `ADUserName`, refresh token และ **persist/read token ผ่าน Windows Registry** (`HKLM\Software\BIS`) |
| `UserContext` (domain model) | `CMS.Domain` | โมเดลข้อมูล identity ต่อ request/ต่อ session ที่แท้จริง มี `UserID`, `AppRole`, `DepartmentId`, `Authority`, `SessionAcl` (preload ตอน login) — **ไหลผ่านเป็น parameter ชัดเจนในทุก method ของ service/repository ที่ต้องใช้สิทธิ์** เช่น `GetFolder(int folderId, UserContext user)` ไม่ใช่ ambient/thread-static context |

หลักการ: **ห้ามอ่านสิทธิ์ผู้ใช้ปัจจุบันจาก global/static state ในชั้น service** — ต้องรับ `UserContext` เข้ามาทาง parameter เสมอ เพื่อให้ testable และชัดเจนว่า method ไหนต้องใช้สิทธิ์อะไร

---

## 6. CMS.Controllers (Controller/Presenter Layer)

เมื่อมี `CMS.Services` ทำ business logic อยู่แล้ว บทบาทของ `CMS.Controllers` คือ **ชั้นเชื่อมบาง ๆ ระหว่าง Presentation กับ Services/Data** ซึ่งพบ 2 รูปแบบในระบบนี้:

### 6.1 Facade controller (thin wrapper)

ห่อ service เดียว 1:1 แทบไม่มี logic เพิ่ม เช่น `KBController` ห่อ `IKBCategoryService` + `IKBArticleService` พร้อม null-guard คืนค่า empty list ถ้า service ที่ควรมีไม่ได้ถูกต่อสายไว้

### 6.2 Presenter-style controller (MVP)

ตัวอย่าง: `FilePresenter`, `FolderPresenter`, `PermissionPresenter`, `AuditLogPresenter`, `OrgStructurePresenter` — คลาสเหล่านี้ถือ reference ไปยัง `IXxxView` (ที่ WinForms form เป็นคน implement) แล้ว orchestrate การเรียก service และส่งผลกลับเข้า view (`_view.ShowFolderTree(...)`, `_view.ShowError(...)`) — เป็น pattern **Model-View-Presenter (MVP)** classic

### 6.3 การแตกความรับผิดชอบแบบ CQRS-ish

Controller หลายตัวถูกแยกตามความรับผิดชอบแทนที่จะเป็น controller อ้วนตัวเดียว เช่นสำหรับ Folder:

- `FolderCommandController` — เขียน (create/update/delete)
- `FolderReadLookupController` — อ่านสำหรับ dropdown/lookup
- `FolderSettingsController` — การตั้งค่า (บางตัวรับ repository ตรง ๆ ไม่ผ่าน service เพราะเป็น read-only ล้วน)
- `FolderPermissionLookupController`, `FolderTemplateController`

นี่คือเหตุผลที่ `CMS.Controllers` ต้อง reference `CMS.Data` ตรง — controller ประเภท read-only lookup บางตัวคุยกับ repository ตรง ๆ โดยไม่ผ่าน service ที่ไม่มี business rule อะไรเพิ่มให้ต้องมี

---

## 7. CMS.Presentation (WinForms Layer)

| ส่วนประกอบ | หน้าที่ |
|---|---|
| `Program.cs` | Composition root: parse CLI args (`/u:`/`/p:`/`/token:` — 3 โหมด login) → ประกอบ dependency graph ทั้งหมดด้วยมือ → auth ผ่าน `AdAuthService` → auto-register user ใหม่ → แสดง PDPA consent dialog ถ้ายังไม่ยินยอม → สร้าง `UserContext` (preload ACL/authority) → เปิด `MDI_Container` ผ่าน `Application.Run` |
| `MDI_Container` | MDI parent form ตัวเดียวของทั้งโปรแกรม รับ dependency 40+ ตัวผ่าน constructor (named parameter) เก็บเป็น `private readonly` field + expose บาง property (เช่น `WorkflowService`, `CurrentUser`) ให้ child form เรียกใช้ |
| `MenuControl` | แปลง `UserMenuModel` (โครงสร้างเมนู/action แบบ nested จาก API) เป็น `List<MenuAction>` แบบ flat เก็บใน `GlobalData.MenuActions` — เป็นตัวกำหนดว่า user คนนี้เห็น/ทำ action ไหนได้บ้าง |
| `MenuActions/MenuActionRegistry` | คลาส static ภายใน ใช้ `switch` บน action key string (เช่น `"ShareDialog"`, `"OrgStructure"`, `"UserManagement"`) เพื่อเปิดฟอร์มที่ตรงกัน โดยรับ `MenuActionContext` (DTO ที่พก user ปัจจุบัน + resource ที่เลือกอยู่ + reference ของทุก controller/service ที่ switch อาจต้องใช้) และใช้ `MenuActionKeyCatalog` สำหรับ alias resolution (alias กำหนดผ่าน `app.config`) |
| Forms (`Modules/DMS`, `Modules/KMS`, `Modules/WFE`) | **ไม่มี base Form class ร่วมกัน** แต่ละ form implement `IXxxView` interface ของตัวเอง (MVP) รับ presenter/controller ผ่าน constructor โดยตรง form ถูก `new` โดย `MenuActionRegistry` หรือ `Program.cs`/form อื่น ไม่ใช่ผ่าน factory กลาง |

**กฎการอ้างอิงที่พบใน KMS (comment บังคับไว้ในโค้ด):**

```csharp
// ✅ Allowed usings: CMS.Controllers, CMS.Domain
// ❌ Forbidden usings: CMS.Services, CMS.Data
```

Form ห้าม `using CMS.Services;` หรือ `using CMS.Data;` ตรง ๆ — ต้องคุยผ่าน Controller/Presenter เท่านั้น (แม้ว่า project reference ทางเทคนิคจะอนุญาตให้ทำได้ก็ตาม — นี่คือวินัยของทีมที่บังคับด้วย comment/convention ไม่ใช่ compiler)

**WFE เป็นโมดูลที่วาง MVP เข้มงวดที่สุด:** มีโฟลเดอร์ `Presenters/` แยกต่างหากพร้อม `IXxxView` ของแต่ละหน้าจอ (`WorkflowDashboardPresenter`, `WorkflowMonitorPresenter` ฯลฯ) — ถือเป็นตัวอย่างอ้างอิงที่ดีที่สุดถ้าต้องการทำ MVP แบบเต็มรูปแบบ

---

## 8. ตัวอย่างสายงานเต็มรูปแบบ #1 — โมดูล DMS: อัปโหลดไฟล์เข้าโฟลเดอร์

```
FolderForm (คลิก "Upload")
   → new FileUploadForm(_fileUploadController, _user, folder, ...) .ShowDialog()
        → FileUploadForm implements IFileUploadView, ถือ IFileUploadController ผ่าน constructor
        → เลือกไฟล์/ลาก drop → สร้าง FileUploadRequest DTO ต่อไฟล์
        → StartUpload() รันบน BackgroundWorker
             → _fileUploadController.UploadFiles(view, files, folderId, user)
                  → FileUploadController.UploadFiles(...)
                       → new FilePresenter(fileService, folderService, view, user)
                       → presenter.UploadFiles(files, folderId)
                            → toggle view state (disable ปุ่ม ฯลฯ)
                            → _fileService.UploadFiles(files, folderId, user)
                                 (ดู business rule เต็มใน §5)
                            → ป้อนผลลัพธ์ทีละไฟล์กลับ view (progress %, success/error)
                            → เช็ค quota หลังอัปโหลดเสร็จ → เตือนถ้า usage ≥ 90%
             ← ผลลัพธ์ (UploadResult ต่อไฟล์) กลับผ่าน IFileUploadView callback
               (SetFileStatus, SetFileResult, SetProgress, ShowQuotaWarning, ShowError)
               ทุก callback marshal กลับ UI thread ผ่าน Invoke()
```

**Repository:**

```csharp
public int Insert(FileDocument document)
{
    using (var conn = new SqlConnection(_connectionString))
    {
        conn.Open();
        using (var cmd = new SqlCommand("storage.sp_InsertFileDocument", conn))
        {
            cmd.CommandType = CommandType.StoredProcedure;
            cmd.Parameters.AddWithValue("@FolderID", document.FolderID);
            // ...
            return Convert.ToInt32(cmd.ExecuteScalar());
        }
    }
}
```

**Domain model ที่เกี่ยวข้อง:** `FileDocument` (FileID, FileGUID, FolderID, OriginalFileName, StoredFileName, FileExtension, MimeType, FileSizeBytes, PhysicalPath, Checksum, VirusScanStatus, FileStatus, UploadedBy/Date, Version, ...), `Folder` (FolderID, ParentFolderID, OrgUnitID, ConfidentialityLevel, PhysicalPath, FolderSizeBytes, QuotaBytes, IsArchived/IsDeleted, ...)

**DTO สนับสนุน:** `FileUploadRequest` (FileName, FileStream, FileSizeBytes, MimeType, OverwriteExisting), `UploadResult` (Success, OriginalFileName, ActualFileName, IsDuplicate, FileDocumentId, ErrorMessage)

**สรุป pattern สายงาน:**

```
Form (implements IXxxView, รับ IXxxController ผ่าน DI)
  → Controller (thin, สร้าง Presenter ต่อการเรียกใช้งาน)
    → Presenter (MVP mediator แปล service exception เป็น view call)
      → Service (business rule: auth, quota, checksum, rollback, audit)
        → Repository (stored procedure, เปิด connection ต่อครั้งเรียก)
          → Domain model (POCO พร้อม alias property เพื่อ backward compat)
```

pattern เดียวกันนี้ใช้ซ้ำกับฟีเจอร์ "เรียกดู folder tree" ผ่าน `FolderForm` → `FolderCommandController`/`FolderReadLookupController` → `FolderService`/`IFolderService` → `FolderRepository` → `Folder`

---

## 9. ตัวอย่างสายงานเต็มรูปแบบ #2 — โมดูล KMS: สร้าง/แก้ไขบทความ + นำเข้า Excel

### 9.1 สร้าง/แก้ไข KB Article

```
KBEditorDialog : Form, IKBEditorController   (view implement ตาม view-interface ของ presenter)
   ├─ Load → _presenter.Initialize()  (โหลด dropdown, gen code ใหม่ถ้าเป็นโหมดสร้าง)
   ├─ BtnSaveDraft_Click → validate ฝั่ง view → _presenter.Save(changeDesc)
   │      → วน related-article rows → _presenter.AddRelatedArticleLink(id)
   ├─ BtnSendReview_Click → confirm → _presenter.SendReview() / _presenter.Publish()
   └─ BtnArchive_Click → confirm → _presenter.Archive()

KBEditorController (CMS.Controllers/KMS/KB)
   constructor: IKBEditorController view, IKBCategoryService, IKBArticleService, UserContext user, KBArticle? editingArticle
   ├─ Initialize()   → _categoryService.GetAllActive() + _articleService.GenerateNextArticleCode()
   ├─ Save(desc)      → ประกอบ KBArticle จาก view getter → _articleService.Create(...) / Update(...)
   ├─ SendReview()    → บังคับ Status = 2 → create/update → _view.CloseOk()
   └─ Publish()/Archive() → _articleService.ChangeStatus(articleId, 3/4, user.AdIdentity)

KBArticleService (CMS.Services), constructor รับ:
   IKBArticleRepository, IKBVersionRepository, IKBArticleLinkRepository, IAuditService
   ├─ Create(article)
   │     validate Title/CategoryID → gen ArticleCode ("KB-{year}-{seq:D3}")
   │     → Status=1, CurrentVersion=1, ReviewDueDate=+1 ปี
   │     → repo.Insert(article)
   │     → **insert KBVersion snapshot แรก (VersionNumber=1)**   ← versioning เกิดในชั้น service
   │     → auditService.Log(AuditEntry{ Action="CREATE_KB_ARTICLE", ... })
   ├─ Update(article, changeDescription)
   │     → nextVer = versionRepo.GetNextVersionNumber(articleId)
   │     → **insert KBVersion snapshot ใหม่ก่อน** update ตัว article เอง
   │     → auditService.Log("UPDATE_KB_ARTICLE")
   └─ ChangeStatus(articleId, newStatus, actionBy)
         บังคับ lifecycle: Draft → Review → Published → Archived (ห้ามออกจาก Archived)
         → repo.UpdateStatus(...) → auditService.Log(...)

KBArticleRepository (CMS.Data) — inline parameterized SQL (ไม่ใช่ stored procedure, ดู §4)
   → domain model KBArticle / KBCategory / KBVersion
```

**Domain model:**

- `KBArticle`: ArticleID, ArticleCode, Title, ContentHTML, Summary, CategoryID, ArticleType, Status, CurrentVersion, PublishedDate, EffectiveDate, ExpiryDate, ReviewDueDate, ViewCount, LikeCount, IsOfficialPolicy, Tags, ConfidentialityLevel, CreatedBy/Date, ReviewedBy + computed (CategoryName, LinkedEntities, StatusName)
- `KBCategory`: CategoryID, ParentCategoryID, CategoryCode, CategoryName, Icon, Description, SortOrder, IsActive + computed IconSymbol (ผ่าน `KBCategoryIconHelper`)
- `KBVersion`: VersionID, ArticleID, VersionNumber, **ContentSnapshot** (เก็บเนื้อหาทั้งฉบับต่อเวอร์ชัน), ChangeDescription, CreatedBy/Date

**หลักการ versioning ที่ควรทำซ้ำ:** ทุกครั้งที่ข้อมูลหลักถูกแก้ไข ให้ insert snapshot ฉบับเต็มลงตาราง version แยก **ก่อน** ทำการ update จริง เพื่อให้ history สมบูรณ์ ย้อนดูของเก่าได้เสมอ

### 9.2 Pipeline นำเข้า Excel (Bulk Import)

ใช้ไลบรารี **NPOI 2.5.6** (`NPOI.SS.UserModel`, `NPOI.XSSF.UserModel.XSSFWorkbook`) อ่านไฟล์ `.xlsx`

```
KBExcelReader (IKBExcelReaderService)
   → ReadRows(filePath): เปิดไฟล์แบบ read-share, อ่าน sheet แรกเริ่มแถวที่ 4 (index 3, แถว 0-2 คือ header)
     map 20+ คอลัมน์ดิบ → KBImportRow, แปลงวันที่แบบไทย (พ.ศ. → ค.ศ. ลบ 543 ปีถ้า Year > 2400)
     แถวเสีย → mark Status="Error" รายแถว (ไม่ทำให้ทั้งไฟล์ล้ม), ไฟล์เสียทั้งไฟล์ → throw KBExcelReadException

KBImportProcessor (IKBImportProcessorService) — orchestrate 3 phase:
   1. ValidateRows       — เช็ค required field/รูปแบบตัวเลข (ไม่แตะ DB) → Status="Valid"/"Error"
   2. ResolveCategories  — map โค้ดคอลัมน์ A → ชื่อ category ผ่าน AppSettings["KBCategories"]
                            → upsert parent/sub category ผ่าน repository (มี in-memory cache กันซ้ำ)
   3. InsertArticles     — resolve แผนก/คณะย่อยที่เกี่ยวข้อง → สร้าง KBArticle (ArticleType=5, Status=2)
                            → insert + KBVersion snapshot + KBArticleLink ต่อแถว
                            → try/catch รายแถว (successCount/errorCount แยก)
                            → callback Action<int,int> onProgress สำหรับ progress bar บน UI

KBImportValidationResult (CMS.Domain.Models) — รวมผล TotalRecords/ValidRecords/WarningRecords/
   ErrorRecords/Rows/SkippedRows/ImportFailedRows ส่งขึ้นไปแสดงใน KBImportWizardForm
```

**หลักการที่ควรทำซ้ำ:** แยก 3 phase ชัดเจน (validate ล้วน ๆ ไม่แตะ DB → resolve reference data → insert จริง) และให้แต่ละแถวล้มเหลวแยกจากกัน (ไม่ทำให้ทั้ง batch abort) พร้อม progress callback กลับ UI

---

## 10. Cross-Cutting Concerns

### 10.1 Permission Model (2 แกน อิสระต่อกัน)

| แกน | Service หลัก | หลักการ |
|---|---|---|
| **ACL แบบ deny-first + inheritance** | `PermissionService` | ตรวจสิทธิ์ระดับ folder/file ตาม `PrincipalType` (1=User, 2=Role, 3=OrgUnit, 4=Group) โดยไล่ตามลำดับชั้นโฟลเดอร์ (inheritance) และ **deny ชนะ allow เสมอ** มี in-memory cache หลายชุด (`_permCache`, `_aclCache`, `_folderCache`, `_userAccountCache`) กันคำนวณซ้ำ |
| **Org/Position authority** | `FolderAuthorityService` | resolve ระดับความลับ (confidentiality level) ที่ตำแหน่ง/แผนก/ฝ่ายของ user คนนั้นได้รับอนุญาต ผ่าน `FolderAuthorityRepository.Resolve(positionName, departmentCode, divisionCode, levelCode)` — เป็นเมทริกซ์สิทธิ์ตามโครงสร้างองค์กร แยกจาก ACL โดยสิ้นเชิง |

ทั้งสองแกนถูก **preload เข้า `UserContext` ครั้งเดียวตอน login** (`SessionAcl`, `Authority`) เพื่อไม่ต้อง round-trip DB ซ้ำระหว่าง session — ดู `Program.cs` เมธอด `TryPreloadSessionAcl`

`FilePermissionService` เป็นคู่แฝดของ `PermissionService` แต่ scope เฉพาะไฟล์ (มี repository/ตารางแยก) ใช้ตอนต้องแชร์ไฟล์เดี่ยว ๆ โดยไม่อิงสิทธิ์ของทั้งโฟลเดอร์

### 10.2 Audit Logging

ทุก service ที่ทำ business operation สำคัญ (Create/Update/Delete/ChangeStatus ฯลฯ) inject `IAuditService` แล้วเรียก:

```csharp
_auditService.Log(new AuditEntry
{
    UserID = user.UserID,
    Module = "...",
    Action = "CREATE_KB_ARTICLE",     // ชื่อ action เป็น UPPER_SNAKE_CASE
    TableName = "KMS.t_KBArticle",
    RecordID = articleId,
    Result = 1,                        // 1 = success, 2 = fail
    // MachineName, IPAddress, SessionID ฯลฯ
});
```

ทั้ง success และ failure ต้องถูกบันทึก (ดูตัวอย่าง rollback ใน `FileService.UploadFiles` §5 ที่ log ผลล้มเหลวด้วย `Result = 2`)

### 10.3 Config Convention

- Connection string หลักอ่านจาก **`appSettings["ConnectionString"]`** (ใช้โดย repository ทุกตัวผ่าน `IConfigManager`)
- `connectionStrings["CmsDb"]` สำรองไว้เฉพาะ path ที่ `ConfigManager` fallback ไปอ่านตาราง `SystemConfig` ใน DB
- Endpoint/credential ของระบบภายนอกทั้งหมดเป็น flat key ใน `appSettings`: `NotificationServerUrl/AppCode/ApiKey`, `NetworkDrivePath` + SMB credential, `EmployeeApiBaseUrl`, `AdAuthApiBaseUrl/LoginPath/ValidatePath`, `MenuApiUrl/ProgramId`, LDAP settings, ค่าที่ปรับได้ของ Workflow engine (SLA hours, job interval)
- Menu-action key สามารถ remap ผ่าน config ได้โดยไม่ต้องแก้โค้ด: `MenuActionKeys`, `MenuActionKeys.<Module>`, `MenuActionKeyAliases`, `MenuActionKeyAliases.<Module>` — อ่านผ่าน `MenuActionKeyCatalog`
- `log4net` section ตั้งค่า logger แยกตาม class (ส่วนใหญ่เป็นคลาสของ Workflow engine) ด้วย `RollingFileAppender` + `ConsoleAppender`
- **ข้อควรระวังที่มีอยู่จริงในระบบ:** รหัสผ่าน DB/SMB/LDAP เก็บเป็น plaintext ใน `appSettings` (มี comment ในโค้ดที่ยอมรับจุดอ่อนนี้ไว้แล้วสำหรับ LdapPassword) — เป็นสิ่งที่ควรพึงระวังถ้าจะทำซ้ำ pattern นี้ ควรพิจารณาใช้ secret vault หรือ DPAPI แทน

---

## 11. Pattern Checklist (สรุปย่อ)

เมื่อจะสร้างโมดูลใหม่ตาม pattern เดียวกับ DMS/KMS ให้ตรวจสอบทีละข้อ:

- [ ] **Domain**: สร้าง POCO ล้วน ๆ ใน `CMS.Domain/Models`, enum ใน `CMS.Domain/Enums` — ห้ามมี logic
- [ ] **Data**: สร้าง `IXxxRepository` + `XxxRepository` คู่กัน, รับ `IConfigManager` ทาง constructor, เปิด/ปิด `SqlConnection` ต่อ method, ใช้ parameterized SQL เสมอ (stored procedure หรือ inline parameterized ก็ได้), เขียน mapping มือผ่าน static helper
- [ ] **Services**: constructor injection ด้วย interface, dependency ที่ไม่จำเป็นเสมอไปใช้ `= null`, รวม business rule ทั้งหมดไว้ที่นี่ (auth dual-gate, quota, validation, versioning, rollback), รับ `UserContext user` เป็น parameter ทุกจุดที่ต้องใช้สิทธิ์ (ห้ามอ่านจาก static/global), เรียก `IAuditService.Log(...)` ทุก operation สำคัญทั้ง success/fail
- [ ] **Controllers**: เลือกระหว่าง facade (wrap service 1:1) กับ presenter (`IXxxView` + orchestrate) ตามความซับซ้อนของหน้าจอ, แยก command/read-lookup ถ้า controller เริ่มอ้วน
- [ ] **Presentation**: form implement `IXxxView` ของตัวเอง รับ presenter/controller ผ่าน constructor, ห้าม `using CMS.Services;`/`using CMS.Data;` ตรงจาก form, ต่อสายทั้งหมดที่ composition root เดียว (`Program.cs`), ถ้าต้องเปิดจากเมนูให้ลงทะเบียน action key ใน `MenuActionRegistry`
- [ ] **Config**: connection string/endpoint/credential เป็น `appSettings` flat key, เพิ่ม audit action name แบบ `UPPER_SNAKE_CASE`
