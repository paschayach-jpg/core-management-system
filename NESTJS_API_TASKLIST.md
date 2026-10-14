# NestJS API Migration — Tasklist แยกตาม Module (DMS / KMS / RGP / WFE)

> วิเคราะห์จาก source ของ `core-management-system` (branch `popnahaff`) เมื่อ 2026-10-14 · WinForms .NET 4.5 + ADO.NET + SQL Server
> เป้าหมาย: ระบุว่าแต่ละ module **CRUD ตารางอะไรบ้าง** เพื่อออกแบบ API ใน NestJS — นับเฉพาะโค้ดที่ถูกเรียกใช้จริง (ตัด dead code ออก)
> หมายเหตุ: ในโค้ดสะกดโมดูลว่า **RGP** (Project / Regulatory-Project) ไม่ใช่ RPG — เอกสารนี้ใช้ RGP ตามโค้ด

## 0. วิธีอ่านเอกสาร / วิธีวิเคราะห์

- [x] อ่านเฉพาะไฟล์ที่ **อยู่ใน `.csproj` (Compile Include)** — ไฟล์ที่ไม่ถูก compile (30 ไฟล์) ไม่นับ (ดูภาคผนวก A)
- [x] สร้าง call graph เริ่มจาก `Program.Main` → `MDI_Container` → Form/Controller/Service → Repository แล้วนับเฉพาะ **Repository method ที่ถูกเรียกถึงจริง** (ที่เหลือ = dead code ดูภาคผนวก A)
- [x] แยก module จากตำแหน่งต้นทางของการเรียก: `CMS.Presentation/Modules/<M>` + `CMS.Controllers/<M>` (ไม่ผ่านตัวกลาง `MDI_Container`) — ตารางที่ module เดียวใช้ = เป็นของ module นั้น, ใช้ ≥2 module = เป็นของ schema เจ้าของ หรือ **Shared** (EMS.* / audit.*)
- [x] ดึงตาราง + การกระทำ (C/R/U/D) จาก SQL ใน `CMS.Data/Repositories/*` (`INSERT/SELECT/UPDATE/DELETE/MERGE`) และจาก Stored Procedure
- ⚠ **Stored Procedure:** repo ไม่มีสคริปต์ของ SP (เฉพาะ `MOD-006/*.sql` ของ KMS) → ตารางที่ได้จาก SP **อนุมานจากชื่อ SP** (ระบุ SP ไว้ในแต่ละ method) ต้องให้ DBA export SP definition มายืนยัน
- ⚠ call graph เป็น static analysis (ชื่อ method + ชนิดของตัวแปร) ไม่ได้รันโปรแกรม — ควรไล่ตรวจกับเมนูจริงอีกรอบ โดยเฉพาะ method ที่ระบุเป็น dead

**สัญลักษณ์:** `C` = INSERT · `R` = SELECT · `U` = UPDATE · `D` = DELETE จริง · `d` / `soft` = soft delete (UPDATE `IsDeleted=1` / `IsActive=0` หรือ SP `SoftDelete*`)
**Verb ที่แนะนำ:** GET=อ่าน · POST=สร้าง/restore/activate · PUT=แก้ไข · DELETE=ลบ (ชื่อ route/DTO ให้ทีมกำหนดเอง)

## 1. สรุปภาพรวม

| Module | ตารางที่เป็นเจ้าของ | Repository ที่ใช้จริง | Public repo method ที่ใช้จริง |
|---|---:|---:|---:|
| DMS | 20 | 18 | 114 |
| KMS | 9 | 8 | 56 |
| RGP | 20 | 15 | 89 |
| WFE | 6 | 6 | 30 |
| Shared (ใช้ร่วมหลาย module) | 7 | 8 | 46 |
| **รวม** | **62** | **53** | **335** |

- Repository method (public) ทั้งหมดในโค้ด 412 → **ใช้จริง 338** · dead 74 (ตัดออก)
- ตารางที่พบจากการใช้งานจริง: **62** ตาราง (รวมที่อนุมานจาก SP)
- แหล่งข้อมูลหลายฐาน: ส่วนใหญ่ใช้ `ConnectionString` (CMS_DB) — RGP ใช้เพิ่ม `ProjectDbConnectionString`, `BisDbConnectionString`, `CommonDbConnectionString`, `CmsLinkDbConnectionString` และอ้างตาราง cross-db `BUI_RG.dbo.*` → NestJS ต้องตั้งหลาย DataSource (ดูภาคผนวก C)

## 1.1 ลำดับงานที่แนะนำ

- [ ] **Phase 0** — โครง Nest: multi-DataSource (ภาคผนวก C), config module (SP `sp_GetSystemConfig`/`sp_UpsertSystemConfig`), auth (AD/LDAP → `EMS.UserAccount`)
- [ ] **Phase 1 — Shared** (section 6): `EMS.UserAccount`, `EMS.OrgUnit`, `EMS.m_*`, `audit.AuditLog`, `EMS.Notification` — ทุก module พึ่งพา
- [ ] **Phase 2 — DMS** (section 2): Folder/File/Permission ก่อน แล้วค่อย Approval/Share/Quota/Admin (KMS และ RGP พึ่ง `storage.*`, `security.*`)
- [ ] **Phase 3 — KMS** (section 3): Category → Article/Version/Link/Attachment → Import Template/Display Profile → Dashboard (ทำ query จริง)
- [ ] **Phase 4 — RGP** (section 4): Template → Project/State/Task/Progress → Doc workflow/Member/Attachment
- [ ] **Phase 5 — WFE** (section 5): Definition/Step/Connection → Instance/StepInstance → Delegation → SLA report

## 2. DMS — Document Management System

- Document Management — Folder/File/Permission/Approval/Share/Quota + งาน admin (User, Org, HR master, Menu, Audit)
- ไฟล์จริงเก็บบน network drive/file server (ผ่าน `NetworkDriveHelper`) ไม่ใช่ใน DB — API ต้องรับ-ส่ง stream และเขียนลงที่เดียวกัน (ตารางเก็บเฉพาะ metadata)
- ⚠ ชื่อ repo กับตารางสลับกัน: `HrPositionRepository` → `EMS.m_Role`, `HrPositionLevelRepository` → `EMS.m_Position` (ตามโค้ดจริง)
- ส่วนใหญ่เรียกผ่าน **Stored Procedure** (`storage.sp_*`, `security.sp_*`, `workflow.sp_*`, `EMS.sp_*`) → ใช้ `query('EXEC ...')`/`request.execute()` ใน Nest ได้ตรง ๆ

**สรุป CRUD ต่อตาราง**

| ตาราง | C | R | U | D | Repository | ใช้โดย |
|---|:-:|:-:|:-:|:-:|---|---|
| `EMS.FolderAuthorityConfig` | ✅ | ✅ | ✅ | ✅ | FolderAuthority | DMS |
| `EMS.MenuItem` | ✅ | ✅ | ✅ | ✅ | MenuItem | DMS |
| `EMS.OrgUnitRelationship` | — | ✅ | — | — | FolderQuotaConfig, OrgUnit | DMS |
| `EMS.m_Position` | ✅ | ✅ | ✅ | ✅ | HrPositionLevel | DMS |
| `EMS.m_PositionMapping` | ✅ | ✅ | ✅ | ✅ | HrPositionMapping | DMS |
| `EMS.m_Role` | ✅ | ✅ | ✅ | ✅ | HrPosition, HrPositionMapping | DMS |
| `dms.FolderQuotaChangeRequest` | ✅ | ✅ | ✅ | — | FolderQuotaChangeRequest | DMS |
| `security.FilePermission` | ✅ | ✅ | ✅ | — | FilePermission | DMS, KMS, RGP |
| `security.FolderPermission` | ✅ | ✅ | ✅ | ✅ | Permission | DMS, KMS, RGP |
| `security.UserOrgVisibility` | ✅ | ✅ | ✅ | ✅ | UserOrgVisibility | DMS |
| `storage.ExternalShare` | ✅ | ✅ | ✅ | — | ExternalShare | DMS |
| `storage.FileDocument` | ✅ | ✅ | ✅ | ✅ (+soft) | FileDocument, FolderQuotaConfig, ProjectAttachment | DMS, KMS, RGP |
| `storage.Folder` | ✅ | ✅ | ✅ | ✅ (+soft) | FileDocument, Folder, FolderQuotaConfig, OrgUnit, ProjectAttachment | DMS, KMS, RGP |
| `storage.FolderTemplate` | ✅ | ✅ | ✅ | ✅ | FolderTemplate | DMS |
| `storage.FolderTemplateItem` | ✅ | ✅ | ✅ | ✅ | FolderTemplate | DMS |
| `storage.QuickAccess` | ✅ | ✅ | — | ✅ | QuickAccess | DMS |
| `storage.ShareAccessLog` | ✅ | ✅ | — | — | ShareAccessLog | DMS |
| `storage.ShareToken` | ✅ | ✅ | ✅ | — | ShareToken | DMS |
| `workflow.ApprovalRequest` | ✅ | ✅ | ✅ | ✅ | Approval | DMS |
| `workflow.ApprovalStep` | ✅ | ✅ | ✅ | ✅ | Approval | DMS |

**Tasklist ต่อตาราง**

- [ ] **`EMS.FolderAuthorityConfig`** — `C R U D` · ใช้โดย: DMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetAll()` → `IEnumerable<FolderAuthorityConfig>` · op `R` — SP: `EMS.sp_GetAllFolderAuthorityConfigs`
  - [ ] **GET** `Resolve(string positionName, string departmentCode, string divisionCode, string levelCode = null)` → `IEnumerable<FolderAuthorityConfig>` · op `R` — SP: `EMS.sp_ResolveFolderAuthority`
  - [ ] **POST** `Insert(FolderAuthorityConfig c, int createdBy)` → `int` · op `C` — SP: `EMS.sp_InsertFolderAuthorityConfig`
  - [ ] **PUT** `Update(FolderAuthorityConfig c, int updatedBy)` · op `U` — SP: `EMS.sp_UpdateFolderAuthorityConfig`
  - [ ] **DELETE** `Delete(string configLabel, int deletedBy)` · op `D` — SP: `EMS.sp_DeleteFolderAuthorityConfig`
- [ ] **`EMS.MenuItem`** — `C R U D` · ใช้โดย: DMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetAllMenuItems()` → `List<MenuItem>` · op `R`
  - [ ] **GET** `GetRootMenuItems()` → `List<MenuItem>` · op `R`
  - [ ] **GET** `GetChildMenuItems(int parentMenuItemID)` → `List<MenuItem>` · op `R`
  - [ ] **GET** `GetMenuItemByID(int menuItemID)` → `MenuItem` · op `R`
  - [ ] **GET** `GetMenuItemByKey(string menuKey)` → `MenuItem` · op `R`
  - [ ] **POST** `CreateMenuItem(MenuItem menuItem)` → `int` · op `C` — SP: `EMS.sp_CreateMenuItem`
  - [ ] **PUT** `UpdateMenuItem(MenuItem menuItem)` · op `U` — SP: `EMS.sp_UpdateMenuItem`
  - [ ] **DELETE** `DeleteMenuItem(int menuItemID)` · op `D` — SP: `EMS.sp_DeleteMenuItem`
  - [ ] **GET** `GetMenuHierarchy()` → `List<MenuItem>` · op `R`
- [ ] **`EMS.OrgUnitRelationship`** — `· R · ·` · ใช้โดย: DMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetAllActiveRelationships()` → `IEnumerable<OrgUnitRelationship>` · op `R`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `FolderQuotaConfig.GetAllFileStats`
- [ ] **`EMS.m_Position`** — `C R U D` · ใช้โดย: DMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetAll()` → `IEnumerable<HrPositionLevel>` · op `R`
  - [ ] **POST** `Save(HrPositionLevel level)` → `bool` · op `C`
  - [ ] **DELETE** `Delete(string levelCode)` → `bool` · op `D`
- [ ] **`EMS.m_PositionMapping`** — `C R U D` · ใช้โดย: DMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetAll()` → `IEnumerable<HrPositionMapping>` · op `R` — join/อ่าน: `EMS.m_Department`, `EMS.m_Division`, `EMS.m_Role`
  - [ ] **GET** `GetByDepartmentId(int departmentId)` → `IEnumerable<HrPositionMapping>` · op `R` — join/อ่าน: `EMS.m_Department`, `EMS.m_Division`, `EMS.m_Role`
  - [ ] **POST** `Save(HrPositionMapping mapping)` → `int` · op `C`
  - [ ] **DELETE** `Delete(int mappingId)` → `bool` · op `D`
- [ ] **`EMS.m_Role`** — `C R U D` · ใช้โดย: DMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetAll()` → `IEnumerable<HrPosition>` · op `R`
  - [ ] **POST** `Save(HrPosition position)` → `int` · op `C`
  - [ ] **DELETE** `Delete(int positionId)` → `bool` · op `D`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `HrPositionMapping.GetAll`, `HrPositionMapping.GetByDepartmentId`
- [ ] **`dms.FolderQuotaChangeRequest`** — `C R U ·` · ใช้โดย: DMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **POST** `Insert(FolderQuotaChangeRequest req)` → `int` · op `C`
  - [ ] **GET** `GetById(int requestId)` → `FolderQuotaChangeRequest` · op `R` — join/อ่าน: `EMS.OrgUnit`, `EMS.UserAccount`
  - [ ] **GET** `GetPending()` → `IEnumerable<FolderQuotaChangeRequest>` · op `R` — join/อ่าน: `EMS.OrgUnit`, `EMS.UserAccount`
  - [ ] **GET** `GetAll()` → `IEnumerable<FolderQuotaChangeRequest>` · op `R` — join/อ่าน: `EMS.OrgUnit`, `EMS.UserAccount`
  - [ ] **PUT** `UpdateStatus(int requestId, QuotaRequestStatus status, int reviewedBy, string remark)` · op `U`
- [ ] **`security.FilePermission`** — `C R U ·` · ใช้โดย: DMS, KMS, RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **POST** `Insert(FilePermission permission)` → `int` · op `C` — SP: `security.sp_InsertFilePermission`
  - [ ] **GET** `GetById(int filePermissionId)` → `FilePermission` · op `R`
  - [ ] **GET** `GetByFile(int fileId)` → `IEnumerable<FilePermission>` · op `R` — SP: `security.sp_GetFilePermissionsByFile`
  - [ ] **GET** `GetByGrantedBy(int userId)` → `IEnumerable<FilePermission>` · op `R`
  - [ ] **GET** `GetAll()` → `IEnumerable<FilePermission>` · op `R`
  - [ ] **GET** `GetByPrincipal(int principalType, int principalId)` → `IEnumerable<FilePermission>` · op `R`
  - [ ] **PUT** `Revoke(int filePermissionId, int revokedBy, string revokeReason)` · op `U` — SP: `security.sp_RevokeFilePermission`
- [ ] **`security.FolderPermission`** — `C R U D` · ใช้โดย: DMS, KMS, RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **POST** `Insert(FolderPermission permission)` → `int` · op `C` — SP: `security.sp_InsertFolderPermission`
  - [ ] **GET** `GetByFolder(int folderId)` → `IEnumerable<FolderPermission>` · op `R` — SP: `security.sp_GetPermissionsByFolder`
  - [ ] **GET** `GetAllByFolder(int folderId)` → `IEnumerable<FolderPermission>` · op `R` — SP: `security.sp_GetAllPermissionsByFolder`
  - [ ] **GET** `GetByPrincipal(int principalType, int principalId)` → `IEnumerable<FolderPermission>` · op `R` — SP: `security.sp_GetPermissionsByPrincipal`
  - [ ] **PUT** `Revoke(int permissionId, int revokedBy, string revokeReason)` · op `U` — SP: `security.sp_RevokePermission`
  - [ ] **DELETE** `DeleteByFolder(int folderId)` · op `D`
  - [ ] **GET** `GetAclEntries(int folderId)` → `IEnumerable<FolderPermission>` · op `R` — SP: `security.sp_GetPermissionsByFolder`
  - [ ] **GET** `GetAllForUser(int userId, int roleId, int? deptId, int? divId)` → `IEnumerable<FolderPermission>` · op `R`
- [ ] **`security.UserOrgVisibility`** — `C R U D` · ใช้โดย: DMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetByUser(int userId)` → `IEnumerable<UserOrgVisibility>` · op `R`
  - [ ] **POST** `Grant(UserOrgVisibility record)` · op `C`
  - [ ] **DELETE** `Revoke(int userId, int orgUnitId)` · op `D`
- [ ] **`storage.ExternalShare`** — `C R U ·` · ใช้โดย: DMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **POST** `Insert(ExternalShare externalShare)` → `int` · op `C`
  - [ ] **GET** `GetById(int externalShareId)` → `ExternalShare` · op `R`
  - [ ] **GET** `GetBySharer(int sharerUserId)` → `IEnumerable<ExternalShare>` · op `R`
  - [ ] **PUT** `Update(ExternalShare externalShare)` · op `U`
  - [ ] **GET** `GetAll()` → `IEnumerable<ExternalShare>` · op `R`
- [ ] **`storage.FileDocument`** — `C R U D` · ใช้โดย: DMS, KMS, RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **POST** `Insert(FileDocument document)` → `int` · op `C` — SP: `storage.sp_InsertFileDocument`
  - [ ] **GET** `GetById(int fileId)` → `FileDocument` · op `R` — SP: `storage.sp_GetFileDocumentByID`
  - [ ] **GET** `GetByFolder(int folderId)` → `IEnumerable<FileDocument>` · op `R` — SP: `storage.sp_GetFileDocumentsByFolder`
  - [ ] **PUT** `Update(FileDocument document)` · op `U` — SP: `storage.sp_UpdateFileDocument`
  - [ ] **DELETE** `SoftDelete(int fileId, int deletedBy)` · op `d` — SP: `storage.sp_SoftDeleteFileDocument`
  - [ ] **DELETE** `PermanentDelete(int fileId)` · op `D`
  - [ ] **GET** `GetAllGroupedByFolder()` → `IEnumerable<FileDocument>` · op `R` — SP: `storage.sp_GetAllFileDocumentsGroupedByFolder`
  - [ ] **GET** `GetByOrgUnit(int orgUnitId)` → `IEnumerable<FileDocument>` · op `R` — SP: `storage.sp_GetFileDocumentsByOrgUnit`
  - [ ] **DELETE** `DeleteAllByFolder(int folderId)` · op `D`
  - [ ] **GET** `ExistsWithName(string fileName, int folderId)` → `bool` · op `R`
  - [ ] **GET** `GetByOriginalName(string fileName, int folderId)` → `FileDocument` · op `R`
  - [ ] **GET** `CountByNamePrefix(string namePrefix, int folderId)` → `int` · op `R`
  - [ ] **PUT** `RenameFile(int fileId, string newName)` · op `U` — SP: `storage.sp_RenameFileDocument`
  - [ ] **GET** `GetDeletedFiles(int? orgUnitId)` → `IEnumerable<FileDocument>` · op `R` — join/อ่าน: `EMS.OrgUnit`, `storage.Folder`
  - [ ] **POST** `RestoreFile(int fileId)` · op `U`
  - [ ] **PUT** `UpdateVersion(int fileId, int version)` · op `U`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `FolderQuotaConfig.GetAllFileStats`, `ProjectAttachment.GetByProject`
- [ ] **`storage.Folder`** — `C R U D` · ใช้โดย: DMS, KMS, RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **DELETE** `ForceDeleteWithDependents(int orgUnitId, int deletedBy)` · op `d` — SP: `EMS.sp_SoftDeleteOrgUnit`; เขียนตารางอื่น: `d:EMS.OrgUnit`
  - [ ] **POST** `Insert(Folder folder)` → `int` · op `C` — SP: `storage.sp_InsertFolder`
  - [ ] **PUT** `Update(Folder folder)` · op `U` — SP: `storage.sp_UpdateFolder`
  - [ ] **PUT** `UpdateFileCount(int folderId)` · op `U` — SP: `storage.sp_UpdateFolderFileCount`
  - [ ] **GET** `GetById(int folderId)` → `Folder` · op `R` — SP: `storage.sp_GetFolderByID`
  - [ ] **GET** `GetChildren(int parentFolderId)` → `IEnumerable<Folder>` · op `R` — SP: `storage.sp_GetFolderChildren`
  - [ ] **GET** `GetDescendants(int folderId)` → `IEnumerable<Folder>` · op `R` — SP: `storage.sp_GetFolderDescendants`
  - [ ] **GET** `GetAllRootFolders()` → `IEnumerable<Folder>` · op `R`
  - [ ] **GET** `GetAllDeletedFolders()` → `IEnumerable<Folder>` · op `R`
  - [ ] **GET** `GetRootFoldersByOrgUnit(int orgUnitId)` → `IEnumerable<Folder>` · op `R` — SP: `storage.sp_GetRootFoldersByOrgUnit`
  - [ ] **GET** `GetDeletedByOrgUnit(int orgUnitId)` → `IEnumerable<Folder>` · op `R` — SP: `storage.sp_GetDeletedFoldersByOrgUnit`
  - [ ] **GET** `GetByOrgUnit(int orgUnitId)` → `IEnumerable<Folder>` · op `R` — SP: `storage.sp_GetFoldersByOrgUnit`
  - [ ] **GET** `GetAll()` → `IEnumerable<Folder>` · op `R` — SP: `storage.sp_GetAllFolders`
  - [ ] **GET** `GetSiblings(int? parentFolderId, int orgUnitId)` → `IEnumerable<Folder>` · op `R` — SP: `storage.sp_GetFolderSiblings`
  - [ ] **DELETE** `SoftDelete(int folderId, int deletedBy)` · op `d` — SP: `storage.sp_SoftDeleteFolder`
  - [ ] **POST** `Restore(int folderId)` · op `U` — SP: `storage.sp_RestoreFolder`
  - [ ] **DELETE** `PermanentDelete(int folderId)` · op `D`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `FileDocument.GetDeletedFiles`, `FolderQuotaConfig.GetAllFileStats`, `ProjectAttachment.GetByProject`
- [ ] **`storage.FolderTemplate`** — `C R U D` · ใช้โดย: DMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **POST** `Insert(FolderTemplate template)` → `int` · op `C` — SP: `storage.sp_InsertFolderTemplate`
  - [ ] **GET** `GetAll()` → `IEnumerable<FolderTemplate>` · op `R` — SP: `storage.sp_GetAllFolderTemplates`
  - [ ] **PUT** `Update(FolderTemplate template)` · op `U` — SP: `storage.sp_UpdateFolderTemplate`
  - [ ] **DELETE** `Delete(int templateId)` · op `D` — SP: `storage.sp_DeleteFolderTemplate`
- [ ] **`storage.FolderTemplateItem`** — `C R U D` · ใช้โดย: DMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetItems(int templateId)` → `IEnumerable<FolderTemplateItem>` · op `R` — SP: `storage.sp_GetFolderTemplateItems`
  - [ ] **POST** `InsertItem(FolderTemplateItem item)` → `int` · op `C` — SP: `storage.sp_InsertFolderTemplateItem`
  - [ ] **PUT** `UpdateItem(FolderTemplateItem item)` · op `U` — SP: `storage.sp_UpdateFolderTemplateItem`
  - [ ] **DELETE** `DeleteItem(int itemId)` · op `D` — SP: `storage.sp_DeleteFolderTemplateItem`
- [ ] **`storage.QuickAccess`** — `C R · D` · ใช้โดย: DMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetByUser(int userId)` → `IEnumerable<QuickAccessFolder>` · op `R` — SP: `storage.sp_GetQuickAccessByUser`
  - [ ] **POST** `Add(int userId, int folderId)` · op `C` — SP: `storage.sp_AddQuickAccess`
  - [ ] **DELETE** `Remove(int userId, int folderId)` · op `D` — SP: `storage.sp_RemoveQuickAccess`
- [ ] **`storage.ShareAccessLog`** — `C R · ·` · ใช้โดย: DMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **POST** `Insert(ShareAccessLog accessLog)` → `long` · op `C`
  - [ ] **GET** `GetByExternalShare(int externalShareId)` → `IEnumerable<ShareAccessLog>` · op `R`
- [ ] **`storage.ShareToken`** — `C R U ·` · ใช้โดย: DMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **POST** `Insert(ShareToken shareToken)` → `int` · op `C`
  - [ ] **PUT** `Update(ShareToken shareToken)` · op `U`
  - [ ] **GET** `GetByExternalShare(int externalShareId)` → `IEnumerable<ShareToken>` · op `R`
- [ ] **`workflow.ApprovalRequest`** — `C R U D` · ใช้โดย: DMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **POST** `InsertRequest(ApprovalRequest request)` → `int` · op `C`
  - [ ] **GET** `GetRequest(int requestId)` → `ApprovalRequest` · op `R` — SP: `workflow.sp_GetApprovalRequestByID`
  - [ ] **GET** `GetAllRequestsByFolder(int folderId)` → `IEnumerable<ApprovalRequest>` · op `R`
  - [ ] **GET** `GetPendingRequests()` → `IEnumerable<ApprovalRequest>` · op `R`
  - [ ] **GET** `GetExpiredPendingRequests()` → `IEnumerable<ApprovalRequest>` · op `R` — SP: `workflow.sp_GetExpiredPendingRequests`
  - [ ] **PUT** `UpdateRequestStatus(int requestId, int requestStatus, int resolvedBy, string resolveRemark)` · op `U` — SP: `workflow.sp_UpdateApprovalRequestStatus`
  - [ ] **GET** `GetApprovedDownloadRequest(int fileId, int userId)` → `ApprovalRequest` · op `R`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `Approval.DeleteByFolder`
- [ ] **`workflow.ApprovalStep`** — `C R U D` · ใช้โดย: DMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **POST** `InsertStep(ApprovalStep step)` → `int` · op `C`
  - [ ] **GET** `GetPendingSteps(int requestId)` → `IEnumerable<ApprovalStep>` · op `R`
  - [ ] **GET** `GetAllSteps(int requestId)` → `IEnumerable<ApprovalStep>` · op `R` — SP: `workflow.sp_GetAllApprovalSteps`
  - [ ] **PUT** `UpdateStepStatus(int stepId, int stepStatus, string actionRemark)` · op `U` — SP: `workflow.sp_UpdateApprovalStepStatus`
  - [ ] **DELETE** `DeleteByFolder(int folderId)` · op `D` — เขียนตารางอื่น: `D:workflow.ApprovalRequest`; join/อ่าน: `workflow.ApprovalRequest`

**ตารางของ module อื่น/Shared ที่ DMS เรียกใช้** (รายละเอียด method อยู่ใน section ของเจ้าของตาราง)

| ตาราง | เจ้าของ | การกระทำที่ DMS ใช้ | Repository.Method ที่ถูกเรียก |
|---|---|---|---|
| `EMS.Notification` | SHARED | `C · · ·` | `Notification.Insert` |
| `EMS.OrgUnit` | SHARED | `C R U d` | `OrgUnit.Insert`, `OrgUnit.Update`, `OrgUnit.GetById`, `OrgUnit.ExistsByCode`, `OrgUnit.ExistsByNameUnderParent`, `OrgUnit.GetHierarchy`, `OrgUnit.GetByConfidentialityLevel`, `OrgUnit.HasActiveDependents`, `OrgUnit.SoftDelete`, `OrgUnit.Deactivate` … (+7) |
| `EMS.UserAccount` | SHARED | `C R U d` | `UserAccount.GetByADUserName`, `UserAccount.GetById`, `UserAccount.GetByRole`, `UserAccount.GetAll`, `UserAccount.Create`, `UserAccount.Update`, `UserAccount.UpdateLevelCode`, `UserAccount.UpdateLastLoginWithIP`, `UserAccount.RegisterOrUpdate`, `UserAccount.UpsertFromLdap` … (+6) |
| `EMS.m_Department` | SHARED | `C R U D` | `HrDepartment.GetAll`, `HrDepartment.Save`, `HrDepartment.Delete`, `HrPositionMapping.GetAll`, `HrPositionMapping.GetByDepartmentId`, `FolderQuotaConfig.GetByOrgUnit`, `FolderQuotaConfig.GetByMasterCode`, `FolderQuotaConfig.GetByEmsOrgUnitId`, `FolderQuotaConfig.GetAll`, `FolderQuotaConfig.Save` … (+2) |
| `EMS.m_Division` | SHARED | `C R U D` | `HrDivision.GetAll`, `HrDivision.Save`, `HrDivision.Delete`, `HrPositionMapping.GetAll`, `HrPositionMapping.GetByDepartmentId`, `FolderQuotaConfig.GetByOrgUnit`, `FolderQuotaConfig.GetByMasterCode`, `FolderQuotaConfig.GetByEmsOrgUnitId`, `FolderQuotaConfig.GetAll`, `FolderQuotaConfig.Save` … (+2) |
| `EMS.m_SubCommittee` | SHARED | `C R U ·` | `OrgUnit.InsertSubCommitteeRecord`, `FolderQuotaConfig.GetByOrgUnit`, `FolderQuotaConfig.GetByMasterCode`, `FolderQuotaConfig.GetByEmsOrgUnitId`, `FolderQuotaConfig.GetAll`, `FolderQuotaConfig.Save`, `FolderQuotaConfig.Delete`, `FolderQuotaConfig.GetAllFileStats` |
| `audit.AuditLog` | SHARED | `C R · ·` | `Audit.Insert`, `Audit.Query` |

## 3. KMS — Knowledge Management System

- Knowledge Management — Category / Article / Version / Link / Attachment / Import Template / Display Profile
- ตารางทั้งหมดมี DDL ใน `MOD-006/03..15_*.sql` (ใช้เป็นต้นแบบ entity ได้ตรง ๆ)
- **KB Dashboard เป็น mock** — `KBDashboardRepository` ทั้ง 3 method คืนค่าคงที่ ไม่แตะ DB (คอมเมนต์ TODO จะ query `KMS.t_KBArticle`) → API ต้องออกแบบ query จริงใหม่
- Attachment ของบทความอัปโหลดผ่าน DMS (`storage.FileDocument`) — KMS จึงพึ่งตาราง DMS (ดูตาราง "ของ module อื่น")

**สรุป CRUD ต่อตาราง**

| ตาราง | C | R | U | D | Repository | ใช้โดย |
|---|:-:|:-:|:-:|:-:|---|---|
| `KMS.m_KBArticleDisplayProfile` | ✅ | ✅ | ✅ | soft | KBArticleDisplayProfile | KMS |
| `KMS.m_KBArticleDisplayProfileColumn` | ✅ | ✅ | — | ✅ | KBArticleDisplayProfile | KMS |
| `KMS.m_KBCategory` | ✅ | ✅ | ✅ | soft | KBArticle, KBCategory, Kms | KMS, RGP |
| `KMS.m_KBImportTemplate` | ✅ | ✅ | ✅ | soft | KBImportTemplate | KMS |
| `KMS.m_KBImportTemplateColumn` | ✅ | ✅ | — | ✅ | KBImportTemplate | KMS |
| `KMS.t_KBArticle` | ✅ | ✅ | ✅ | soft | KBArticle | KMS |
| `KMS.t_KBArticleAttachment` | ✅ | ✅ | — | ✅ | KBArticleAttachment | KMS |
| `KMS.t_KBArticleLink` | ✅ | ✅ | — | ✅ | KBArticle, KBArticleLink | KMS |
| `KMS.t_KBVersion` | ✅ | ✅ | — | — | KBArticle, KBVersion | KMS |

**Tasklist ต่อตาราง**

- [ ] **`KMS.m_KBArticleDisplayProfile`** — `C R U d` · ใช้โดย: KMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetAll()` → `IEnumerable<KBArticleDisplayProfile>` · op `R`
  - [ ] **GET** `GetById(int profileId)` → `KBArticleDisplayProfile` · op `R`
  - [ ] **GET** `ExistsByName(string profileName, int excludeProfileId = 0)` → `bool` · op `R`
  - [ ] **POST** `SaveProfileWithColumns(KBArticleDisplayProfile profile, IList<KBArticleDisplayProfileColumn> columns)` → `int` · op `C` — เขียนตารางอื่น: `C:KMS.m_KBArticleDisplayProfileColumn`, `D:KMS.m_KBArticleDisplayProfileColumn`
  - [ ] **DELETE** `Deactivate(int profileId)` · op `d`
  - [ ] **POST** `Activate(int profileId)` · op `U`
- [ ] **`KMS.m_KBArticleDisplayProfileColumn`** — `C R · D` · ใช้โดย: KMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetColumnsByProfileId(int profileId)` → `IList<KBArticleDisplayProfileColumn>` · op `R`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `KBArticleDisplayProfile.SaveProfileWithColumns`
- [ ] **`KMS.m_KBCategory`** — `C R U d` · ใช้โดย: KMS, RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetAll()` → `IEnumerable<KBCategory>` · op `R`
  - [ ] **GET** `GetAllActive()` → `IEnumerable<KBCategory>` · op `R`
  - [ ] **GET** `GetById(int categoryId)` → `KBCategory` · op `R`
  - [ ] **POST** `Insert(KBCategory category)` → `int` · op `C`
  - [ ] **PUT** `Update(KBCategory category)` · op `U`
  - [ ] **DELETE** `SoftDelete(int categoryId)` · op `d`
  - [ ] **GET** `GetLawTypes()` → `DataTable` · op `R` — conn: `CmsLinkDbConnectionString`
  - [ ] **GET** `GetCategoriesByParent(int parentCategoryId)` → `DataTable` · op `R` — conn: `CmsLinkDbConnectionString`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `KBArticle.GetAll`, `KBArticle.GetByCategory`, `KBArticle.GetById`, `KBArticle.GetMoveCandidates`, `KBArticle.GetMoveCandidatesByIds`, `KBArticle.Search`
- [ ] **`KMS.m_KBImportTemplate`** — `C R U d` · ใช้โดย: KMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetAll()` → `IEnumerable<KBImportTemplate>` · op `R`
  - [ ] **GET** `GetById(int templateId)` → `KBImportTemplate` · op `R`
  - [ ] **GET** `ExistsByName(string templateName, int excludeTemplateId = 0)` → `bool` · op `R`
  - [ ] **POST** `SaveTemplateWithMappings(KBImportTemplate template, IList<KBImportTemplateColumnMapping> mappings)` → `int` · op `C` — เขียนตารางอื่น: `C:KMS.m_KBImportTemplateColumn`, `D:KMS.m_KBImportTemplateColumn`
  - [ ] **DELETE** `Deactivate(int templateId, string updatedBy)` · op `d`
  - [ ] **POST** `Activate(int templateId, string updatedBy)` · op `U`
- [ ] **`KMS.m_KBImportTemplateColumn`** — `C R · D` · ใช้โดย: KMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetMappingsByTemplateId(int templateId)` → `IList<KBImportTemplateColumnMapping>` · op `R`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `KBImportTemplate.SaveTemplateWithMappings`
- [ ] **`KMS.t_KBArticle`** — `C R U d` · ใช้โดย: KMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetById(int articleId)` → `KBArticle` · op `R` — join/อ่าน: `KMS.m_KBCategory`
  - [ ] **POST** `Insert(KBArticle article)` → `int` · op `C`
  - [ ] **POST** `InsertWithVersion(KBArticle article, KBVersion version)` → `int` · op `C` — เขียนตารางอื่น: `C:KMS.t_KBVersion`
  - [ ] **PUT** `Update(KBArticle article, KBVersion newVersion, string updatedBy)` · op `U` — เขียนตารางอื่น: `C:KMS.t_KBVersion`; join/อ่าน: `KMS.t_KBVersion`
  - [ ] **PUT** `UpdateFromImport(int articleId, KBArticle values, IEnumerable<string> columns, string updatedBy, KBVersion newVersion)` · op `U` — เขียนตารางอื่น: `C:KMS.t_KBVersion`; join/อ่าน: `KMS.t_KBVersion`
  - [ ] **PUT** `UpdateStatus(int articleId, byte status, string reviewedBy)` · op `U`
  - [ ] **PUT** `IncrementViewCount(int articleId)` · op `U`
  - [ ] **PUT** `IncrementLikeCount(int articleId)` · op `U`
  - [ ] **GET** `GetNextCodeSequence(int year)` → `int` · op `R`
  - [ ] **GET** `GetImportableTargetColumns()` → `IList<string>` · op `R`
  - [ ] **GET** `ExistsByCode(string articleCode)` → `bool` · op `R`
  - [ ] **GET** `GetByCode(string articleCode)` → `KBArticle` · op `R`
  - [ ] **GET** `GetMoveCandidates(IEnumerable<int> categoryIds)` → `IList<KBArticle>` · op `R` — join/อ่าน: `KMS.m_KBCategory`
  - [ ] **GET** `GetMoveCandidatesByIds(IEnumerable<int> articleIds)` → `IList<KBArticle>` · op `R` — join/อ่าน: `KMS.m_KBCategory`
  - [ ] **PUT** `MoveToCategory(IEnumerable<int> articleIds, int toCategoryId, string updatedBy)` → `IList<KeyValuePair<int, int>>` · op `U`
  - [ ] **DELETE** `SoftDelete(int articleId)` · op `d`
  - [ ] **PUT** `UpdateExpiryDate(int articleId, DateTime? expiryDate)` · op `U`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `KBArticle.GetAll`, `KBArticle.GetByCategory`, `KBArticle.Search`
- [ ] **`KMS.t_KBArticleAttachment`** — `C R · D` · ใช้โดย: KMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetAttachmentsForArticle(int articleId)` → `IEnumerable<KBArticleAttachment>` · op `R`
  - [ ] **POST** `Insert(KBArticleAttachment attachment)` → `int` · op `C`
  - [ ] **DELETE** `Delete(int attachmentId)` · op `D`
  - [ ] **DELETE** `DeleteAllForArticle(int articleId)` · op `D`
  - [ ] **DELETE** `DeleteByArticleAndFile(int articleId, int fileId)` · op `D`
- [ ] **`KMS.t_KBArticleLink`** — `C R · D` · ใช้โดย: KMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetAll()` → `IEnumerable<KBArticle>` · op `R` — join/อ่าน: `EMS.m_Department`, `EMS.m_Division`, `EMS.m_SubCommittee`, `KMS.m_KBCategory`, `KMS.t_KBArticle`
  - [ ] **GET** `GetByCategory(int categoryId)` → `IEnumerable<KBArticle>` · op `R` — join/อ่าน: `EMS.m_Department`, `EMS.m_Division`, `EMS.m_SubCommittee`, `KMS.m_KBCategory`, `KMS.t_KBArticle`
  - [ ] **GET** `Search(int? categoryId, byte? status, byte? articleType)` → `IEnumerable<KBArticle>` · op `R` — join/อ่าน: `EMS.m_Department`, `EMS.m_Division`, `EMS.m_SubCommittee`, `KMS.m_KBCategory`, `KMS.t_KBArticle`
  - [ ] **GET** `GetLinksForArticle(int articleId, byte? entityType = null)` → `IEnumerable<KBArticleLink>` · op `R`
  - [ ] **GET** `GetAllByEntityType(byte entityType)` → `IEnumerable<KBArticleLink>` · op `R`
  - [ ] **POST** `Insert(KBArticleLink link)` → `int` · op `C`
  - [ ] **GET** `Exists(int articleId, byte entityType, string entityId)` → `bool` · op `R`
  - [ ] **DELETE** `Delete(int linkId)` · op `D`
  - [ ] **DELETE** `DeleteAllForArticle(int articleId, byte? entityType = null)` · op `D`
- [ ] **`KMS.t_KBVersion`** — `C R · ·` · ใช้โดย: KMS
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetByArticle(int articleId)` → `IEnumerable<KBVersion>` · op `R`
  - [ ] **GET** `GetLatestByArticle(int articleId)` → `KBVersion` · op `R`
  - [ ] **POST** `Insert(KBVersion version)` → `int` · op `C`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `KBArticle.InsertWithVersion`, `KBArticle.Update`, `KBArticle.UpdateFromImport`

**ตารางของ module อื่น/Shared ที่ KMS เรียกใช้** (รายละเอียด method อยู่ใน section ของเจ้าของตาราง)

| ตาราง | เจ้าของ | การกระทำที่ KMS ใช้ | Repository.Method ที่ถูกเรียก |
|---|---|---|---|
| `security.FilePermission` | DMS | `C R U ·` | `FilePermission.Insert`, `FilePermission.GetByFile`, `FilePermission.Revoke` |
| `security.FolderPermission` | DMS | `· R · ·` | `Permission.GetByFolder`, `Permission.GetAclEntries` |
| `storage.FileDocument` | DMS | `C R U d` | `FileDocument.Insert`, `FileDocument.GetById`, `FileDocument.GetByFolder`, `FileDocument.SoftDelete`, `FileDocument.ExistsWithName`, `FileDocument.GetByOriginalName`, `FileDocument.CountByNamePrefix`, `FileDocument.UpdateVersion` |
| `storage.Folder` | DMS | `· R U ·` | `Folder.Update`, `Folder.UpdateFileCount`, `Folder.GetById`, `Folder.GetDescendants`, `Folder.GetRootFoldersByOrgUnit`, `Folder.GetByOrgUnit` |
| `EMS.Notification` | SHARED | `C · · ·` | `Notification.Insert` |
| `EMS.OrgUnit` | SHARED | `· R · ·` | `OrgUnit.GetById`, `FolderQuotaConfig.GetByEmsOrgUnitId` |
| `EMS.UserAccount` | SHARED | `· R · ·` | `UserAccount.GetById` |
| `EMS.m_Department` | SHARED | `· R · ·` | `KBArticle.GetAll`, `KBArticle.GetByCategory`, `KBArticle.Search`, `FolderQuotaConfig.GetByOrgUnit`, `FolderQuotaConfig.GetByMasterCode`, `FolderQuotaConfig.GetByEmsOrgUnitId` |
| `EMS.m_Division` | SHARED | `· R · ·` | `KBArticle.GetAll`, `KBArticle.GetByCategory`, `KBArticle.Search`, `FolderQuotaConfig.GetByOrgUnit`, `FolderQuotaConfig.GetByMasterCode`, `FolderQuotaConfig.GetByEmsOrgUnitId` |
| `EMS.m_SubCommittee` | SHARED | `· R · ·` | `KBArticle.GetAll`, `KBArticle.GetByCategory`, `KBArticle.Search`, `FolderQuotaConfig.GetByOrgUnit`, `FolderQuotaConfig.GetByMasterCode`, `FolderQuotaConfig.GetByEmsOrgUnitId` |
| `audit.AuditLog` | SHARED | `C · · ·` | `Audit.Insert` |

## 4. RGP — Project (ในโค้ดเรียก RGP)

- Project tracking — Project / State / Task / Progress / Template / Member / Doc workflow
- ใช้ **หลาย connection string** (Project / Bis / Common / CmsLink) + ตาราง cross-db `BUI_RG.dbo.*` และ `RunningNumber` (ยังอยู่ BUI_RG เดิม)
- ส่วน **E-Filing** (`EFilingRepository` — `RGP.t_project_efiling_request`) ตรวจแล้วไม่ถูกเรียกจากหน้าจอใด = dead code จึงไม่นับ
- `RgpComposition` (static factory) ไม่ถูกเรียก — ไม่นับ

**สรุป CRUD ต่อตาราง**

| ตาราง | C | R | U | D | Repository | ใช้โดย |
|---|:-:|:-:|:-:|:-:|---|---|
| `BUI_RG.dbo.m_document` | — | ✅ | — | — | TemplateDoc | RGP |
| `BUI_RG.dbo.m_document_type` | — | ✅ | — | — | TemplateDoc | RGP |
| `EMS.m_Employee` | — | ✅ | — | — | Employee | RGP |
| `RGP.m_project_member_role` | ✅ | ✅ | ✅ | ✅ | ProjectMemberRole | RGP |
| `RGP.m_project_template` | ✅ | ✅ | ✅ | ✅ | Project, ProjectTemplate, TemplateDoc | RGP |
| `RGP.m_template_doc` | ✅ | ✅ | ✅ | ✅ | ProjectTemplate, TemplateDoc | RGP |
| `RGP.m_template_state` | ✅ | ✅ | ✅ | ✅ | ProjectState, ProjectTemplate, TemplateDoc | RGP |
| `RGP.t_project` | ✅ | ✅ | ✅ | — | Project, ProjectProgress, ProjectState, ProjectTemplate | RGP |
| `RGP.t_project_audit_log` | ✅ | — | — | — | ProjectAudit | RGP |
| `RGP.t_project_doc_workflow` | ✅ | ✅ | ✅ | ✅ | ProjectDocument | RGP |
| `RGP.t_project_member` | ✅ | ✅ | — | ✅ | ProjectMember | RGP |
| `RGP.t_project_notify_log` | ✅ | ✅ | — | — | ProjectNotifyLog, ProjectStateMember | RGP |
| `RGP.t_project_progress` | ✅ | ✅ | — | — | Project, ProjectProgress, ProjectState | RGP |
| `RGP.t_project_state` | ✅ | ✅ | ✅ | ✅ | Project, ProjectDocument, ProjectProgress, ProjectState, ProjectTask | RGP |
| `RGP.t_project_state_member` | ✅ | ✅ | — | ✅ | ProjectStateMember | RGP |
| `RGP.t_project_task` | ✅ | ✅ | ✅ | — | ProjectTask | RGP |
| `RGP.t_project_task_comment` | ✅ | ✅ | ✅ | — | ProjectTask | RGP |
| `RGP.t_project_task_status_log` | ✅ | — | — | — | ProjectTaskStatusLog | RGP |
| `RunningNumber` | ✅ | ✅ | ✅ | — | Project, ProjectTask | RGP |
| `storage.t_project_attachment` | ✅ | ✅ | — | ✅ | ProjectAttachment | RGP |

**Tasklist ต่อตาราง**

- [ ] **`BUI_RG.dbo.m_document`** — `· R · ·` · ใช้โดย: RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetMasterDocuments()` → `DataTable` · op `R`
- [ ] **`BUI_RG.dbo.m_document_type`** — `· R · ·` · ใช้โดย: RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetDocumentTypes()` → `DataTable` · op `R`
- [ ] **`EMS.m_Employee`** — `· R · ·` · ใช้โดย: RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetActiveADUsers()` → `DataTable` · op `R`
- [ ] **`RGP.m_project_member_role`** — `C R U D` · ใช้โดย: RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetAll()` → `DataTable` · op `R`
  - [ ] **GET** `GetActive()` → `DataTable` · op `R`
  - [ ] **POST** `Insert(string roleName, string userName)` → `bool` · op `C`
  - [ ] **PUT** `Update(int roleId, string roleName, string fActive, string userName)` → `bool` · op `U`
  - [ ] **DELETE** `Delete(int roleId)` → `bool` · op `D`
- [ ] **`RGP.m_project_template`** — `C R U D` · ใช้โดย: RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetActiveTemplates()` → `DataTable` · op `R`
  - [ ] **POST** `Insert(string code, string name, string desc, string user, bool isLegalDict = false)` → `int` · op `C`
  - [ ] **PUT** `Update(int id, string code, string name, string desc, string active, string user, bool isLegalDict = false)` → `bool` · op `U`
  - [ ] **GET** `IsLegalDict(int templateId)` → `bool` · op `R`
  - [ ] **GET** `GetAllActive()` → `DataTable` · op `R`
  - [ ] **GET** `IsCodeDuplicate(string code, int excludeId = -1)` → `bool` · op `R`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `Project.GetProjectById`, `Project.SearchProjects`, `ProjectTemplate.Delete`, `ProjectTemplate.GetAll`, `TemplateDoc.GetDocsByState`
- [ ] **`RGP.m_template_doc`** — `C R U D` · ใช้โดย: RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **DELETE** `Delete(int id, out string error)` → `bool` · op `D` — เขียนตารางอื่น: `D:RGP.m_template_state`, `D:RGP.m_project_template`; join/อ่าน: `RGP.t_project`
  - [ ] **GET** `GetStates(int templateId)` → `DataTable` · op `R` — join/อ่าน: `RGP.m_template_state`
  - [ ] **GET** `GetDocsByState(int i_state_id)` → `DataTable` · op `R` — join/อ่าน: `RGP.m_project_template`, `RGP.m_template_state`
  - [ ] **GET** `GetDocsByTemplate(int i_template_id)` → `DataTable` · op `R` — join/อ่าน: `RGP.m_template_state`
  - [ ] **POST** `InsertTemplateDoc(TemplateDocModel model)` → `int` · op `C`
  - [ ] **PUT** `UpdateTemplateDoc(TemplateDocModel model)` → `bool` · op `U`
  - [ ] **DELETE** `DeleteTemplateDoc(int i_template_doc_id)` → `bool` · op `D`
  - [ ] **GET** `IsDuplicate(int i_state_id, int i_doc_id, int excludeId = -1)` → `bool` · op `R`
  - [ ] **GET** `GetNextSeqNo(int i_state_id)` → `int` · op `R`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `ProjectTemplate.CopyStatesFrom`, `ProjectTemplate.DeleteState`
- [ ] **`RGP.m_template_state`** — `C R U D` · ใช้โดย: RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetTemplateStates(int i_template_id)` → `DataTable` · op `R`
  - [ ] **GET** `GetAll()` → `DataTable` · op `R` — join/อ่าน: `RGP.m_project_template`, `RGP.t_project`
  - [ ] **POST** `CopyStatesFrom(int sourceTemplateId, int targetTemplateId, string user)` → `int` · op `C` — เขียนตารางอื่น: `D:RGP.m_template_doc`
  - [ ] **POST** `InsertState(int templateId, string name, string desc, bool required, int seqNo, int? estimatedDays, string user)` → `int` · op `C`
  - [ ] **PUT** `UpdateState(int stateId, string name, string desc, bool required, int seqNo, int? estimatedDays, string user)` → `bool` · op `U`
  - [ ] **DELETE** `DeleteState(int stateId, out string error)` → `bool` · op `D` — join/อ่าน: `RGP.m_template_doc`
  - [ ] **GET** `GetNextStateSeqNo(int templateId)` → `int` · op `R`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `ProjectTemplate.Delete`, `ProjectTemplate.GetStates`, `TemplateDoc.GetDocsByState`, `TemplateDoc.GetDocsByTemplate`
- [ ] **`RGP.t_project`** — `C R U ·` · ใช้โดย: RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `SearchProjects(string c_project_no, string n_project_name, string c_status, string i_template_id, string d_from, string d_to)` → `DataTable` · op `R` — join/อ่าน: `RGP.m_project_template`, `RGP.t_project_progress`, `RGP.t_project_state`
  - [ ] **GET** `GetProjectById(int i_project_id)` → `DataTable` · op `R` — join/อ่าน: `RGP.m_project_template`
  - [ ] **POST** `InsertProject(ProjectModel model)` → `int` · op `C`
  - [ ] **PUT** `UpdateProject(ProjectModel model)` → `bool` · op `U`
  - [ ] **PUT** `UpdateLegalDictJson(int i_project_id, string json, string i_user)` → `bool` · op `U`
  - [ ] **PUT** `DeleteProject(int i_project_id, string i_user)` → `bool` · op `U`
  - [ ] **PUT** `UpdateProjectStatus(int i_project_id, string c_status, string i_user)` → `bool` · op `U`
  - [ ] **GET** `IsProjectNoExists(string c_project_no)` → `bool` · op `R`
  - [ ] **GET** `GetActiveProjectsForRef(int i_exclude_project_id)` → `DataTable` · op `R` — join/อ่าน: `RGP.t_project_progress`, `RGP.t_project_state`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `ProjectProgress.GetLatestProgressByProject`, `ProjectState.GetStatesByProject`, `ProjectTemplate.Delete`, `ProjectTemplate.GetAll`
- [ ] **`RGP.t_project_audit_log`** — `C · · ·` · ใช้โดย: RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **POST** `Insert(ProjectAuditEntry entry)` · op `C`
- [ ] **`RGP.t_project_doc_workflow`** — `C R U D` · ใช้โดย: RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetDocsByState(int i_proj_state_id)` → `DataTable` · op `R` — join/อ่าน: `RGP.t_project_state`
  - [ ] **GET** `GetDocsByProject(int i_project_id)` → `DataTable` · op `R` — join/อ่าน: `RGP.t_project_state`
  - [ ] **POST** `InsertDocWorkflow(ProjectDocumentWorkflowModel model)` → `int` · op `C`
  - [ ] **PUT** `UpdateDocWorkflow(ProjectDocumentWorkflowModel model)` → `bool` · op `U`
  - [ ] **DELETE** `DeleteDocWorkflow(int i_doc_wf_id)` → `bool` · op `D`
  - [ ] **PUT** `UpdateDocStatus(int i_doc_wf_id, string c_doc_status, string i_user)` → `bool` · op `U`
  - [ ] **GET** `GetNextSeqNo(int i_proj_state_id)` → `int` · op `R`
- [ ] **`RGP.t_project_member`** — `C R · D` · ใช้โดย: RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetByProject(int i_project_id)` → `DataTable` · op `R`
  - [ ] **POST** `Insert(ProjectMemberModel m)` → `bool` · op `C`
  - [ ] **DELETE** `Delete(int i_member_id)` → `bool` · op `D`
- [ ] **`RGP.t_project_notify_log`** — `C R · ·` · ใช้โดย: RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetExistingTaskId(int i_proj_state_id, int i_state_member_id)` → `int?` · op `R`
  - [ ] **POST** `Insert(ProjectNotifyLogModel m)` → `bool` · op `C`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `ProjectStateMember.GetByState`
- [ ] **`RGP.t_project_progress`** — `C R · ·` · ใช้โดย: RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `HasProgressRecords(int i_proj_state_id)` → `bool` · op `R`
  - [ ] **POST** `InsertProgress(ProjectProgressModel model)` → `int` · op `C`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `Project.SearchProjects`, `ProjectProgress.CalculateOverallProgress`, `ProjectProgress.GetLatestProgressByProject`, `ProjectState.GetActiveProjectsForRef`, `ProjectState.GetStatesByProject`
- [ ] **`RGP.t_project_state`** — `C R U D` · ใช้โดย: RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `HasActiveProgress(int i_project_id)` → `bool` · op `R`
  - [ ] **GET** `AllRequiredStatesCompleted(int i_project_id)` → `bool` · op `R`
  - [ ] **GET** `GetStatesByProject(int i_project_id)` → `DataTable` · op `R` — join/อ่าน: `RGP.t_project`, `RGP.t_project_progress`
  - [ ] **POST** `InsertState(ProjectStateModel model)` → `int` · op `C`
  - [ ] **PUT** `UpdateState(ProjectStateModel model)` → `bool` · op `U`
  - [ ] **DELETE** `DeleteState(int i_proj_state_id)` → `bool` · op `D`
  - [ ] **PUT** `UpdateStateStatus(int i_proj_state_id, string c_status, string i_user)` → `bool` · op `U`
  - [ ] **PUT** `ReorderStates(List<Tuple<int, int>> orderList)` → `bool` · op `U`
  - [ ] **GET** `GetNextSeqNo(int i_project_id)` → `int` · op `R`
  - [ ] **GET** `GetTotalEstimatedDays(int i_project_id, int excludeStateId = 0)` → `int` · op `R`
  - [ ] **GET** `GetStatesForCombo(int i_project_id)` → `DataTable` · op `R`
  - [ ] **GET** `GetLatestProgressByProject(int i_project_id)` → `DataTable` · op `R` — join/อ่าน: `RGP.t_project`, `RGP.t_project_progress`
  - [ ] **GET** `CalculateOverallProgress(int i_project_id)` → `int` · op `R` — join/อ่าน: `RGP.t_project_progress`
  - [ ] **GET** `HasAnyProgress(int i_project_id)` → `bool` · op `R`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `Project.SearchProjects`, `ProjectDocument.GetDocsByProject`, `ProjectDocument.GetDocsByState`, `ProjectState.GetActiveProjectsForRef`, `ProjectTask.GetAllTasksForProject`, `ProjectTask.GetByProject`, `ProjectTask.GetByState`
- [ ] **`RGP.t_project_state_member`** — `C R · D` · ใช้โดย: RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetByState(int i_proj_state_id)` → `DataTable` · op `R` — join/อ่าน: `RGP.t_project_notify_log`
  - [ ] **POST** `Insert(ProjectStateMemberModel m)` → `bool` · op `C`
  - [ ] **DELETE** `Delete(int i_state_member_id)` → `bool` · op `D`
- [ ] **`RGP.t_project_task`** — `C R U ·` · ใช้โดย: RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetByState(int i_proj_state_id, bool includeDeleted = false)` → `DataTable` · op `R` — join/อ่าน: `RGP.t_project_state`
  - [ ] **GET** `GetByProject(int i_project_id)` → `DataTable` · op `R` — join/อ่าน: `RGP.t_project_state`
  - [ ] **GET** `GetAllTasksForProject(int projectId)` → `DataTable` · op `R` — join/อ่าน: `RGP.t_project_state`
  - [ ] **GET** `GetById(int i_task_id)` → `ProjectTaskModel` · op `R`
  - [ ] **POST** `InsertTask(ProjectTaskModel m)` → `int` · op `C`
  - [ ] **PUT** `UpdateTask(ProjectTaskModel m)` → `bool` · op `U`
  - [ ] **PUT** `UpdateStatus(int i_task_id, string c_status, int i_pct_complete, string i_user)` → `bool` · op `U`
  - [ ] **PUT** `SoftDelete(int i_task_id, string i_user)` → `bool` · op `U`
  - [ ] **GET** `CalcStatePctFromTasks(int i_proj_state_id)` → `int` · op `R`
- [ ] **`RGP.t_project_task_comment`** — `C R U ·` · ใช้โดย: RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetComments(int i_task_id)` → `DataTable` · op `R`
  - [ ] **POST** `InsertComment(ProjectTaskCommentModel m)` → `int` · op `C`
  - [ ] **PUT** `SoftDeleteComment(int i_comment_id)` → `bool` · op `U`
- [ ] **`RGP.t_project_task_status_log`** — `C · · ·` · ใช้โดย: RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **POST** `Insert(ProjectTaskStatusLogModel m)` → `bool` · op `C`
- [ ] **`RunningNumber`** — `C R U ·` · ใช้โดย: RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **PUT** `GenerateProjectNo()` → `string` · op `U` — conn: `ProjectDbConnectionString`
  - [ ] **POST** `GenerateTaskCode()` → `string` · op `C` — conn: `ProjectDbConnectionString`
- [ ] **`storage.t_project_attachment`** — `C R · D` · ใช้โดย: RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetByProject(int projectId)` → `IList<ProjectAttachmentModel>` · op `R` — join/อ่าน: `storage.FileDocument`, `storage.Folder`
  - [ ] **POST** `Insert(int projectId, int fileId, string user)` → `int` · op `C`
  - [ ] **DELETE** `DeleteByProjectAndFile(int projectId, int fileId)` → `bool` · op `D`

**ตารางของ module อื่น/Shared ที่ RGP เรียกใช้** (รายละเอียด method อยู่ใน section ของเจ้าของตาราง)

| ตาราง | เจ้าของ | การกระทำที่ RGP ใช้ | Repository.Method ที่ถูกเรียก |
|---|---|---|---|
| `security.FilePermission` | DMS | `C R U ·` | `FilePermission.Insert`, `FilePermission.GetByFile`, `FilePermission.Revoke` |
| `security.FolderPermission` | DMS | `· R · ·` | `Permission.GetByFolder`, `Permission.GetAclEntries` |
| `storage.FileDocument` | DMS | `C R U d` | `FileDocument.Insert`, `FileDocument.GetById`, `FileDocument.SoftDelete`, `FileDocument.ExistsWithName`, `FileDocument.GetByOriginalName`, `FileDocument.CountByNamePrefix`, `FileDocument.UpdateVersion`, `ProjectAttachment.GetByProject` |
| `storage.Folder` | DMS | `· R U ·` | `Folder.Update`, `Folder.UpdateFileCount`, `Folder.GetById`, `Folder.GetDescendants`, `Folder.GetRootFoldersByOrgUnit`, `Folder.GetByOrgUnit`, `ProjectAttachment.GetByProject` |
| `KMS.m_KBCategory` | KMS | `· R · ·` | `Kms.GetLawTypes`, `Kms.GetCategoriesByParent` |
| `EMS.Notification` | SHARED | `C · · ·` | `Notification.Insert` |
| `EMS.OrgUnit` | SHARED | `· R · ·` | `OrgUnit.GetById`, `FolderQuotaConfig.GetByEmsOrgUnitId` |
| `EMS.UserAccount` | SHARED | `· R · ·` | `UserAccount.GetById` |
| `EMS.m_Department` | SHARED | `· R · ·` | `FolderQuotaConfig.GetByOrgUnit`, `FolderQuotaConfig.GetByMasterCode`, `FolderQuotaConfig.GetByEmsOrgUnitId`, `ProjectMemberRole.GetDepartments` |
| `EMS.m_Division` | SHARED | `· R · ·` | `FolderQuotaConfig.GetByOrgUnit`, `FolderQuotaConfig.GetByMasterCode`, `FolderQuotaConfig.GetByEmsOrgUnitId`, `ProjectMemberRole.GetDivisions` |
| `EMS.m_SubCommittee` | SHARED | `· R · ·` | `FolderQuotaConfig.GetByOrgUnit`, `FolderQuotaConfig.GetByMasterCode`, `FolderQuotaConfig.GetByEmsOrgUnitId`, `ProjectMemberRole.GetSubCommittees` |
| `audit.AuditLog` | SHARED | `C · · ·` | `Audit.Insert` |

## 5. WFE — Workflow Engine

- Workflow Engine — Template (Definition/Step/Connection), Instance, Step Instance, Delegation, SLA report
- ตารางหลักอยู่ schema `WFE.*` + `dbo.WFDelegation`; รายงาน SLA เรียก SP `dbo.sp_WF_*`
- `WorkflowCreateTemplateForm.LoadStepsAndApproversFromCopiedTemplate` เขียน SQL ตรงใน Form (อ่าน `WFE.s_WFStep`, `WFE.s_WFConnection` ผ่าน connection `CmsDb` ใน app.config) เป็น fallback ตอน copy template → ใน API ให้รวมเข้า endpoint อ่าน step/connection ของ template
- ค้นหาพนักงาน/ผู้รับมอบหมายใช้ `EmployeeApiService` (HTTP ภายนอก) + `LdapUserSearchService` (LDAP) ไม่ผ่านตาราง DB — ถ้า API ใหม่ต้องแทนที่ ให้ใช้ `EMS.UserAccount` ใน Shared
- Workflow Engine V2 (`WorkflowServiceV2`, `WorkflowOrchestrator`, `Plugins/*`), `WFESLABackgroundJob` และ compliance/PDPA service ไม่ถูกเรียก = dead code ไม่นับ

**สรุป CRUD ต่อตาราง**

| ตาราง | C | R | U | D | Repository | ใช้โดย |
|---|:-:|:-:|:-:|:-:|---|---|
| `WFE.s_WFConnection` | ✅ | ✅ | — | — | WorkflowStep | WFE |
| `WFE.s_WFDefinition` | ✅ | ✅ | ✅ | soft | WFReport, WorkflowTemplate | WFE |
| `WFE.s_WFStep` | ✅ | ✅ | — | — | WorkflowStep, WorkflowStepInstance, WorkflowTemplate | WFE |
| `WFE.t_WFInstance` | ✅ | ✅ | ✅ | — | WFReport, Workflow, WorkflowTemplate | WFE |
| `WFE.t_WFStepInstance` | ✅ | ✅ | ✅ | — | Audit, WFReport, WorkflowStepInstance | WFE |
| `dbo.WFDelegation` | ✅ | ✅ | ✅ | — | Delegation | WFE |

**Tasklist ต่อตาราง**

- [ ] **`WFE.s_WFConnection`** — `C R · ·` · ใช้โดย: WFE
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetConnectionsByTemplate(int workflowDefinitionId)` → `IList<WorkflowConnection>` · op `R`
  - [ ] **GET** `GetConnectionsFromStep(int fromStepId)` → `IList<WorkflowConnection>` · op `R`
  - [ ] **POST** `CreateConnection(WorkflowConnection connection)` → `int` · op `C`
- [ ] **`WFE.s_WFDefinition`** — `C R U d` · ใช้โดย: WFE
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **POST** `CreateTemplate(WorkflowDefinition template)` → `int` · op `C`
  - [ ] **GET** `GetTemplate(int workflowDefinitionId)` → `WorkflowDefinition` · op `R`
  - [ ] **GET** `GetTemplateVersions(int workflowDefinitionId)` → `IList<WorkflowDefinition>` · op `R`
  - [ ] **PUT** `UpdateTemplate(WorkflowDefinition template)` · op `U`
  - [ ] **DELETE** `SoftDeleteTemplate(int workflowDefinitionId, string deletedBy)` · op `d`
  - [ ] **GET** `GetSLAByTemplate(WorkflowReportFilter filter)` → `IList<SLATemplateMetricRow>` · op `R` — join/อ่าน: `WFE.t_WFInstance`, `WFE.t_WFStepInstance`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `WorkflowTemplate.GetTemplates`
- [ ] **`WFE.s_WFStep`** — `C R · ·` · ใช้โดย: WFE
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetTemplates(WorkflowTemplateFilter filter)` → `PagedResult<WorkflowDefinition>` · op `R` — join/อ่าน: `WFE.s_WFDefinition`, `WFE.t_WFInstance`
  - [ ] **GET** `GetStep(int workflowStepId)` → `WorkflowStep` · op `R`
  - [ ] **GET** `GetStepsByTemplate(int workflowDefinitionId)` → `IList<WorkflowStep>` · op `R`
  - [ ] **POST** `CreateStep(WorkflowStep step)` → `int` · op `C`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `WorkflowStepInstance.GetStepInstancesByWorkflow`
- [ ] **`WFE.t_WFInstance`** — `C R U ·` · ใช้โดย: WFE
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **POST** `CreateWorkflowInstance(WorkflowInstance instance)` → `int` · op `C` — SP: `WFE.sp_CreateWFInstance`
  - [ ] **GET** `GetWorkflowInstance(int workflowInstanceId)` → `WorkflowInstance` · op `R`
  - [ ] **GET** `GetWorkflowInstances(WorkflowInstanceFilter filter)` → `PagedResult<WorkflowInstance>` · op `R`
  - [ ] **PUT** `UpdateWorkflowInstance(WorkflowInstance instance)` · op `U`
  - [ ] **GET** `GetSLAComplianceMetrics(WorkflowReportFilter filter)` → `SLAComplianceMetrics` · op `R` — SP: `dbo.sp_WF_GetSLAComplianceMetrics`; join/อ่าน: `WFE.t_WFStepInstance`
  - [ ] **GET** `GetTaskDistribution(WorkflowReportFilter filter)` → `PagedResult<TaskDistributionRow>` · op `R` — SP: `dbo.sp_WF_GetTaskDistribution`; join/อ่าน: `WFE.t_WFStepInstance`
  - [ ] **GET** `GetCycleTimeDistribution(WorkflowReportFilter filter)` → `IList<CycleTimeRow>` · op `R`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `WFReport.GetSLAByTemplate`, `WorkflowTemplate.GetTemplates`
- [ ] **`WFE.t_WFStepInstance`** — `C R U ·` · ใช้โดย: WFE
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **POST** `CreateStepInstance(WorkflowStepInstance instance)` → `int` · op `C` — SP: `WFE.sp_CreateWFStepInstance`
  - [ ] **GET** `GetStepInstance(int workflowStepInstanceId)` → `WorkflowStepInstance` · op `R`
  - [ ] **GET** `GetStepInstancesByWorkflow(int workflowInstanceId)` → `IList<WorkflowStepInstance>` · op `R` — join/อ่าน: `WFE.s_WFStep`
  - [ ] **GET** `GetStepInstancesByUser(string adUserName)` → `IList<WorkflowStepInstance>` · op `R`
  - [ ] **PUT** `UpdateStepInstance(WorkflowStepInstance instance)` · op `U`
  - [ ] **GET** `GetSLAWeeklyTrend(int weeks)` → `IList<SLATrendRow>` · op `R`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `Audit.GetAuditLogsByWorkflowInstance`, `WFReport.GetSLAByTemplate`, `WFReport.GetSLAComplianceMetrics`, `WFReport.GetTaskDistribution`
- [ ] **`dbo.WFDelegation`** — `C R U ·` · ใช้โดย: WFE
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **POST** `CreateDelegation(WFDelegation delegation, int maxDelegationDepth)` → `int` · op `C` — SP: `dbo.sp_WF_CreateDelegation`
  - [ ] **GET** `GetDelegation(int wfDelegationId)` → `WFDelegation` · op `R`
  - [ ] **GET** `GetDelegationsByStepInstance(int workflowStepInstanceId)` → `IList<WFDelegation>` · op `R` — SP: `dbo.sp_WF_GetDelegationHistory`
  - [ ] **PUT** `UpdateDelegation(WFDelegation delegation)` · op `U`

**ตารางของ module อื่น/Shared ที่ WFE เรียกใช้** (รายละเอียด method อยู่ใน section ของเจ้าของตาราง)

| ตาราง | เจ้าของ | การกระทำที่ WFE ใช้ | Repository.Method ที่ถูกเรียก |
|---|---|---|---|
| `audit.AuditLog` | SHARED | `C R · ·` | `Audit.Insert`, `Audit.GetAuditLogsByWorkflowInstance` |

## 6. Shared — ตารางที่ใช้ร่วมหลาย module (Auth / Org / HR master / Audit / Notification)

ควรทำเป็น Nest module กลาง (`auth`, `org`, `audit`, `notification`) แล้วให้ DMS/KMS/RGP/WFE import ไปใช้

**สรุป CRUD ต่อตาราง**

| ตาราง | C | R | U | D | Repository | ใช้โดย |
|---|:-:|:-:|:-:|:-:|---|---|
| `EMS.Notification` | ✅ | — | — | — | Notification | DMS, KMS, RGP |
| `EMS.OrgUnit` | ✅ | ✅ | ✅ | soft | FileDocument, FolderQuotaChangeRequest, FolderQuotaConfig, OrgUnit | DMS, KMS, RGP |
| `EMS.UserAccount` | ✅ | ✅ | ✅ | soft | FolderQuotaChangeRequest, UserAccount | DMS, KMS, RGP |
| `EMS.m_Department` | ✅ | ✅ | ✅ | ✅ | FolderQuotaConfig, HrDepartment, HrPositionMapping, KBArticle, ProjectMemberRole | DMS, KMS, RGP |
| `EMS.m_Division` | ✅ | ✅ | ✅ | ✅ | FolderQuotaConfig, HrDivision, HrPositionMapping, KBArticle, ProjectMemberRole | DMS, KMS, RGP |
| `EMS.m_SubCommittee` | ✅ | ✅ | ✅ | — | FolderQuotaConfig, KBArticle, OrgUnit, ProjectMemberRole | DMS, KMS, RGP |
| `audit.AuditLog` | ✅ | ✅ | — | — | Audit | DMS, KMS, RGP, WFE |

**Tasklist ต่อตาราง**

- [ ] **`EMS.Notification`** — `C · · ·` · ใช้โดย: DMS, KMS, RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **POST** `Insert(Notification notification)` → `int` · op `C` — SP: `EMS.sp_InsertNotification`
- [ ] **`EMS.OrgUnit`** — `C R U d` · ใช้โดย: DMS, KMS, RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **POST** `Insert(OrgUnit orgUnit)` → `int` · op `C` — SP: `EMS.sp_InsertOrgUnit`
  - [ ] **PUT** `Update(OrgUnit orgUnit)` · op `U` — SP: `EMS.sp_UpdateOrgUnit`
  - [ ] **GET** `GetById(int orgUnitId)` → `OrgUnit` · op `R` — SP: `EMS.sp_GetOrgUnitByID`
  - [ ] **GET** `ExistsByCode(string orgUnitCode)` → `bool` · op `R`
  - [ ] **GET** `ExistsByNameUnderParent(string nameTH, int? parentId, int? excludeOrgUnitId = null)` → `bool` · op `R`
  - [ ] **GET** `GetHierarchy()` → `IEnumerable<OrgUnit>` · op `R` — SP: `EMS.sp_GetOrgUnitHierarchy`
  - [ ] **GET** `GetByConfidentialityLevel(ConfidentialityLevel level)` → `IEnumerable<OrgUnit>` · op `R` — SP: `EMS.sp_GetOrgUnitByConfidentialityLevel`
  - [ ] **GET** `HasActiveDependents(int orgUnitId)` → `bool` · op `R` — SP: `EMS.sp_HasActiveDependents`
  - [ ] **DELETE** `SoftDelete(int orgUnitId, int deletedBy)` · op `d` — SP: `EMS.sp_SoftDeleteOrgUnit`
  - [ ] **DELETE** `Deactivate(int orgUnitId)` · op `d` — SP: `EMS.sp_SoftDeleteOrgUnit`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `FileDocument.GetDeletedFiles`, `FolderQuotaChangeRequest.GetAll`, `FolderQuotaChangeRequest.GetById`, `FolderQuotaChangeRequest.GetPending`, `FolderQuotaConfig.GetAllFileStats`, `FolderQuotaConfig.GetByEmsOrgUnitId`, `OrgUnit.ForceDeleteWithDependents`
- [ ] **`EMS.UserAccount`** — `C R U d` · ใช้โดย: DMS, KMS, RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetByADUserName(string adUserName)` → `UserAccount` · op `R` — SP: `EMS.sp_GetUserAccountByADUserName`
  - [ ] **GET** `GetById(int userId)` → `UserAccount` · op `R` — SP: `EMS.sp_GetUserAccountByID`
  - [ ] **GET** `GetByRole(int appRole)` → `IEnumerable<UserAccount>` · op `R` — SP: `EMS.sp_GetUserAccountsByRole`
  - [ ] **GET** `GetAll()` → `IEnumerable<UserAccount>` · op `R` — SP: `EMS.sp_GetAllUserAccounts`
  - [ ] **POST** `Create(UserAccount account)` → `int` · op `C` — SP: `EMS.sp_InsertUserAccount`
  - [ ] **PUT** `Update(UserAccount account)` · op `U`
  - [ ] **PUT** `UpdateLevelCode(int userId, string levelCode)` · op `U`
  - [ ] **PUT** `UpdateLastLogin(int userId)` · op `U` — SP: `EMS.sp_UpdateLastLogin`
  - [ ] **PUT** `UpdateLastLoginWithIP(int userId, string ipAddress)` · op `U`
  - [ ] **POST** `RegisterOrUpdate(UserAccount account, string ipAddress, out bool isNew)` → `int` · op `C`
  - [ ] **POST** `UpsertFromLdap(string adUserName, string displayName, string email, bool isActive, string positionName, string departmentName, int? createdBy, out bool isNew)` → `int` · op `C`
  - [ ] **PUT** `UpdateConsent(int userId, string email, bool consentGiven)` · op `U` — SP: `EMS.sp_UpdateUserConsent`
  - [ ] **DELETE** `Deactivate(int userId)` · op `d` — SP: `EMS.sp_DeactivateUserAccount`
  - [ ] **GET** `GetPositionsByDepartmentCode(string departmentCode)` → `IEnumerable<string>` · op `R`
  - [ ] **PUT** `UpdateEmployeeInfo(int userId, string employeeCode, string titleNameEn, string firstNameEn, string lastNameEn, string titleNameTh, string firstNameTh, string lastNameTh, int? departmentId, string departmentCode, string departmentNameEn, string departmentNameTh, int? divisionId, string divisionCode, string divisionNameEn, string divisionNameTh, string positionName)` · op `U`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `FolderQuotaChangeRequest.GetAll`, `FolderQuotaChangeRequest.GetById`, `FolderQuotaChangeRequest.GetPending`
- [ ] **`EMS.m_Department`** — `C R U D` · ใช้โดย: DMS, KMS, RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetAll()` → `IEnumerable<HrDepartment>` · op `R`
  - [ ] **POST** `Save(HrDepartment department)` → `int` · op `C`
  - [ ] **DELETE** `Delete(int departmentId)` → `bool` · op `D`
  - [ ] **GET** `GetDepartments()` → `DataTable` · op `R` — conn: `BisDbConnectionString`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `FolderQuotaConfig.Delete`, `FolderQuotaConfig.GetAll`, `FolderQuotaConfig.GetAllFileStats`, `FolderQuotaConfig.GetByEmsOrgUnitId`, `FolderQuotaConfig.GetByMasterCode`, `FolderQuotaConfig.GetByOrgUnit`, `FolderQuotaConfig.Save`, `HrPositionMapping.GetAll`, `HrPositionMapping.GetByDepartmentId`, `KBArticle.GetAll`, `KBArticle.GetByCategory`, `KBArticle.Search`
- [ ] **`EMS.m_Division`** — `C R U D` · ใช้โดย: DMS, KMS, RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **GET** `GetAll()` → `IEnumerable<HrDivision>` · op `R`
  - [ ] **POST** `Save(HrDivision division)` → `int` · op `C`
  - [ ] **DELETE** `Delete(int divisionId)` → `bool` · op `D`
  - [ ] **GET** `GetDivisions()` → `DataTable` · op `R` — conn: `BisDbConnectionString`
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `FolderQuotaConfig.Delete`, `FolderQuotaConfig.GetAll`, `FolderQuotaConfig.GetAllFileStats`, `FolderQuotaConfig.GetByEmsOrgUnitId`, `FolderQuotaConfig.GetByMasterCode`, `FolderQuotaConfig.GetByOrgUnit`, `FolderQuotaConfig.Save`, `HrPositionMapping.GetAll`, `HrPositionMapping.GetByDepartmentId`, `KBArticle.GetAll`, `KBArticle.GetByCategory`, `KBArticle.Search`
- [ ] **`EMS.m_SubCommittee`** — `C R U ·` · ใช้โดย: DMS, KMS, RGP
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **POST** `InsertSubCommitteeRecord(int orgUnitId, string code, string nameTH, string nameEN, string createdByADUser)` · op `C` — SP: `dbo.sp_InsertSubCommittee`
  - [ ] **GET** `GetByOrgUnit(int orgUnitId, int? parentOrgUnitId = null)` → `FolderQuotaConfig` · op `R` — join/อ่าน: `EMS.m_Department`, `EMS.m_Division`
  - [ ] **GET** `GetByMasterCode(int orgUnitType, string orgUnitCode)` → `FolderQuotaConfig` · op `R` — join/อ่าน: `EMS.m_Department`, `EMS.m_Division`
  - [ ] **GET** `GetByEmsOrgUnitId(int emsOrgUnitId, int orgUnitType)` → `FolderQuotaConfig` · op `R` — join/อ่าน: `EMS.OrgUnit`, `EMS.m_Department`, `EMS.m_Division`
  - [ ] **GET** `GetAll()` → `IEnumerable<FolderQuotaConfig>` · op `R` — join/อ่าน: `EMS.m_Department`, `EMS.m_Division`
  - [ ] **PUT** `Save(FolderQuotaConfig config)` · op `U` — เขียนตารางอื่น: `U:EMS.m_Division`, `U:EMS.m_Department`
  - [ ] **PUT** `Delete(int quotaConfigId, string orgUnitTypeName)` · op `U` — เขียนตารางอื่น: `U:EMS.m_Division`, `U:EMS.m_Department`
  - [ ] **GET** `GetAllFileStats()` → `Dictionary<int, OrgUnitFileStat>` · op `R` — join/อ่าน: `EMS.OrgUnit`, `EMS.OrgUnitRelationship`, `EMS.m_Department`, `EMS.m_Division`, `storage.FileDocument`, `storage.Folder`
  - [ ] **GET** `GetSubCommittees()` → `DataTable` · op `R` — conn: `BisDbConnectionString`
  - ℹ️ `FolderQuotaConfig.*` ทำงานกับ **3 ตารางพร้อมกัน** (UNION `m_SubCommittee` / `m_Division` / `m_Department`) โดยอ่าน/แก้คอลัมน์ `quota_size` — ควรทำเป็น endpoint `quota` เดียว
  - ↳ ถูกเข้าถึงทางอ้อมจาก: `KBArticle.GetAll`, `KBArticle.GetByCategory`, `KBArticle.Search`
- [ ] **`audit.AuditLog`** — `C R · ·` · ใช้โดย: DMS, KMS, RGP, WFE
  - [ ] สร้าง Nest module / entity (หรือ query) / DTO / controller / service สำหรับตารางนี้
  - [ ] **POST** `Insert(AuditLog entry)` → `long` · op `C` — SP: `audit.sp_InsertAuditLog`
  - [ ] **GET** `Query(AuditQuery query)` → `PagedResult<AuditLog>` · op `R` — SP: `audit.sp_QueryAuditLog`
  - [ ] **GET** `GetAuditLogsByWorkflowInstance(int workflowInstanceId)` → `List<AuditLog>` · op `R` — join/อ่าน: `WFE.t_WFStepInstance`

**Module ที่เรียกใช้ตาราง Shared**

| ตาราง Shared | DMS | KMS | RGP | WFE |
|---|---|---|---|---|
| `EMS.Notification` | `C · · ·` | `C · · ·` | `C · · ·` | — |
| `EMS.OrgUnit` | `C R U d` | `· R · ·` | `· R · ·` | — |
| `EMS.UserAccount` | `C R U d` | `· R · ·` | `· R · ·` | — |
| `EMS.m_Department` | `C R U D` | `· R · ·` | `· R · ·` | — |
| `EMS.m_Division` | `C R U D` | `· R · ·` | `· R · ·` | — |
| `EMS.m_SubCommittee` | `C R U ·` | `· R · ·` | `· R · ·` | — |
| `audit.AuditLog` | `C R · ·` | `C · · ·` | `C · · ·` | `C R · ·` |

### 6.1 การเข้าถึง DB นอก Repository

- [ ] **System config** — `CMS.Infrastructure/ConfigManager` เรียก SP `sp_GetSystemConfig(@Key)` และ `sp_UpsertSystemConfig(@Key,@Value,@UpdatedByUserId)` (ไม่ทราบชื่อตารางจริง — ขอ SP definition) → ทำ `config` module (GET/PUT key-value)
- [ ] **WFE template copy** — SQL ตรงใน `WorkflowCreateTemplateForm` (ดู section WFE) → ย้ายเข้า API
- [ ] **KB Dashboard (mock)** — ยังไม่มี query จริง → นิยาม endpoint stats/monthly/top-articles เอง

## ภาคผนวก A — Dead code ที่ตัดออก (ไม่ต้องทำ API)

### A.1 ไฟล์ `.cs` ที่ไม่อยู่ใน `.csproj` (ไม่ถูก compile)

- `CMS.Controllers/WFE/WorkflowPluginController.cs`
- `CMS.Data/Repositories/PermissionGroupRepository.cs`
- `CMS.Infrastructure/DatabaseConnectionTester.cs`
- `CMS.Presentation/MenuManagementForm.Designer.cs`
- `CMS.Presentation/MenuManagementForm.cs`
- `CMS.Presentation/Modules/WFE/PluginRegistryForm.Designer.cs`
- `CMS.Presentation/Modules/WFE/PluginRegistryForm.cs`
- `CMS.Presentation/Modules/WFE/Presenters/Interfaces/IPluginRegistryView.cs`
- `CMS.Presentation/Modules/WFE/Presenters/PluginRegistryPresenter.cs`
- `CMS.Presentation/Modules/WFE/WorkflowCreateTemplateFormTest.cs`
- `CMS.Services/BackgroundJobs/OptimizedWFESLABackgroundJob.cs`
- `CMS.Services/Caching/IWorkflowTemplateCache.cs`
- `CMS.Services/Caching/WorkflowTemplateCacheImpl.cs`
- `CMS.Services/DTOs/MenuDto.cs`
- `CMS.Services/Interfaces/IWorkflowComplianceService.cs`
- `CMS.Services/Interfaces/IWorkflowDataEncryptionService.cs`
- `CMS.Services/Interfaces/IWorkflowDataRetentionService.cs`
- `CMS.Services/Interfaces/IWorkflowPDPAComplianceService.cs`
- `CMS.Services/Interfaces/IWorkflowPermissionService.cs`
- `CMS.Services/Notifications/AsyncNotificationQueueImpl.cs`
- `CMS.Services/Notifications/IAsyncNotificationQueue.cs`
- `CMS.Services/Performance/PerformanceMetrics.cs`
- `CMS.Services/SLA/IOptimizedSLACalculator.cs`
- `CMS.Services/SLA/OptimizedSLACalculatorImpl.cs`
- `CMS.Services/WorkflowComplianceService.cs`
- `CMS.Services/WorkflowConfiguration.cs`
- `CMS.Services/WorkflowDataEncryptionService.cs`
- `CMS.Services/WorkflowDataRetentionService.cs`
- `CMS.Services/WorkflowPDPAComplianceService.cs`
- `CMS.Services/WorkflowPermissionService.cs`

> รวม `PermissionGroupRepository` (SP `security.sp_*PermissionGroup`, `sp_AddUserToGroup` ฯลฯ) และ `PluginRegistry*` — ไม่อยู่ใน build จึงไม่ต้องทำ

### A.2 Class ที่ compile แต่ไม่ถูกเรียกจาก entry point ใด

- **CMS.Controllers/DMS**: `KBController`, `OrgStructureLookupController`, `ProjectPresenter`
- **CMS.Controllers/RGP**: `RgpComposition`
- **CMS.Presentation/**: `MenuTreeViewManager`
- **CMS.Presentation/Modules**: `AccessShareForm`, `FolderDetailForm`, `TextBoxPlaceholderCompat`, `ListViewColumnSorter`, `CalendarCell`, `CalendarEditingControl`
- **CMS.Services/**: `SessionContext`, `WorkflowIntegrationService`
- **CMS.Services/BackgroundJobs**: `WFESLABackgroundJob`
- **CMS.Services/Integrations**: `ApprovalWorkflowAdapter`, `AuditAdapter`, `NotificationAdapter`, `OrgStructureAdapter`, `PermissionAdapter`
- **CMS.Services/Validators**: `ConditionExpressionValidator`, `CreateTemplateValidator`, `CreateWorkflowInstanceValidator`, `DelegationValidator`, `WorkflowStepValidator`
- **CMS.Services/Workflow**: `ServiceResult`, `DefinitionValidator`, `DefinitionService`, `ConcurrencyGuard`, `WorkflowOrchestrator`, `CalcPremiumPlugin`, `CallRiscServerPlugin`, `SendNotificationPlugin`, `SendOicReportPlugin`, `PluginContext`, `PluginResult`, `PluginRegistry`, `ConditionEvaluator`, `WorkflowServiceV2`

### A.3 Repository method ที่ไม่มีใครเรียก (dead)

| Repository | Method (dead) | ตารางที่เกี่ยวข้อง |
|---|---|---|
| SubCommitteeRepository | `GetAll` | EMS.m_SubCommittee |
| UserAccountRepository | `GetByDepartmentId`, `GetByDivisionId` | EMS.UserAccount |
| OrgUnitRepository | `GetByCode`, `LinkDivisionToSubCommittee`, `IsDivisionLinkedToSubCommittee`, `SoftDeleteSubCommitteeRecord` | EMS.OrgUnit, EMS.OrgUnitRelationship, EMS.m_SubCommittee |
| FolderRepository | `GetActiveFolders`, `GetByIdAsync`, `GetByOrgUnitAsync`, `GetAllAsync`, `GetActiveFoldersAsync` | storage.Folder |
| FileDocumentRepository | `UpdateVirusScanStatus`, `GetByChecksum` | storage.FileDocument, storage.sp_UpdateVirusScanStatus |
| ApprovalRepository | `DeletePendingHCRequestsForOrgUnit` | workflow.ApprovalRequest, workflow.ApprovalStep |
| NotificationRepository | `GetUnreadByUser`, `MarkRead` | EMS.Notification |
| FilePermissionRepository | `GetAllByFile`, `DeleteByFile` | security.FilePermission |
| FolderTemplateRepository | `GetById`, `DeleteItemsByTemplate` | storage.FolderTemplate, storage.FolderTemplateItem |
| UserOrgVisibilityRepository | `GetByOrgUnit` | security.UserOrgVisibility |
| HrDepartmentRepository | `GetByDivisionID` | EMS.m_Department |
| HrPositionMappingRepository | `GetByDivisionId` | EMS.m_Department, EMS.m_Division, EMS.m_PositionMapping, EMS.m_Role |
| ExternalShareRepository | `GetByToken`, `GetByStatus` | storage.ExternalShare |
| ShareAccessLogRepository | `GetByToken`, `GetByDateRange` | storage.ShareAccessLog |
| ShareTokenRepository | `GetById`, `GetByTokenValue` | storage.ShareToken |
| KBCategoryRepository | `GetByCode` | KMS.m_KBCategory |
| KBArticleRepository | `FindByNormalizedTitleCategory` | KMS.t_KBArticle |
| KBVersionRepository | `GetById`, `GetNextVersionNumber` | KMS.t_KBVersion |
| MenuItemRepository | `GetMenuItemsByRole`, `UpdateMenuItemOrder`, `MenuKeyExists` | EMS.MenuItem |
| WorkflowRepository | `GetActiveWorkflowInstances`, `SoftDeleteWorkflowInstance` | WFE.t_WFInstance |
| WorkflowTemplateRepository | `GetTemplateVersion`, `HasActiveInstances` | WFE.s_WFDefinition, WFE.t_WFInstance |
| WorkflowStepRepository | `UpdateStep`, `SoftDeleteStep`, `UpdateConnection`, `SoftDeleteConnection` | WFE.s_WFConnection, WFE.s_WFStep |
| WorkflowStepInstanceRepository | `GetPendingStepInstances`, `GetByExternalReference`, `SoftDeleteStepInstance` | WFE.t_WFStepInstance, dbo.sp_WF_GetPendingSteps |
| DelegationRepository | `GetActiveDelegation`, `GetDelegationDepth`, `DeactivateDelegations` | dbo.WFDelegation |
| WFReportRepository | `GetBottleneckAnalysis`, `GetEscalationStatistics` | WFE.t_WFInstance, WFE.t_WFStepInstance, dbo.WFSLARule, dbo.sp_WF_GetBottleneckAnalysis |
| FolderQuotaChangeRequestRepository | `GetByOrgUnit` | EMS.OrgUnit, EMS.UserAccount, dms.FolderQuotaChangeRequest |
| ProjectRepository | `CloseProject` | RGP.t_project |
| ProjectStateRepository | `InsertStatesFromTemplate` | RGP.m_template_state, RGP.t_project_state |
| ProjectTaskRepository | `GetSubtasks`, `GetSubtasksForDashboard`, `Restore`, `HasActiveTasks`, `EditComment` | RGP.t_project_state, RGP.t_project_task, RGP.t_project_task_comment |
| ProjectProgressRepository | `GetProgressHistory` | RGP.t_project_progress, RGP.t_project_state |
| ProjectDocumentRepository | `InsertDocWorkflowFromTemplate` | RGP.m_template_doc, RGP.t_project_doc_workflow, RGP.t_project_state |
| ProjectAttachmentRepository | `DeleteByProject` | storage.t_project_attachment |
| EFilingRepository | `SearchEFilingDoc`, `CheckEFilingRefExists`, `InsertEFilingRequest`, `UpdateEFilingRequestStatus`, `GetEFilingRequestsByProject`, `GetEFilingRequestsByState` | RGP.t_project_efiling_request, RGP.t_project_state |
| EmployeeRepository | `GetActiveEmployees` | dbo.Employees |
| KmsRepository | `GetCategories`, `SearchArticles`, `GetArticleById` | KMS.m_KBCategory, KMS.t_KBArticle |

**ตาราง/SP ที่ปรากฏเฉพาะใน dead code (ไม่ต้องทำ):** `RGP.t_project_efiling_request`, `dbo.Employees`, `dbo.WFSLARule`, `dbo.sp_WF_GetBottleneckAnalysis`, `dbo.sp_WF_GetPendingSteps`, `storage.sp_UpdateVirusScanStatus`

## ภาคผนวก B — Stored Procedure ที่ต้องเรียก/พอร์ตจาก Nest (เฉพาะที่ใช้งานจริง)

| Stored Procedure | Repository.Method | ตาราง (อนุมาน) | Module |
|---|---|---|---|
| `EMS.sp_GetUserAccountByADUserName` | `UserAccount.GetByADUserName` | EMS.UserAccount | DMS |
| `EMS.sp_GetUserAccountByID` | `UserAccount.GetById` | EMS.UserAccount | DMS, KMS, RGP |
| `EMS.sp_GetUserAccountsByRole` | `UserAccount.GetByRole` | EMS.UserAccount | DMS |
| `EMS.sp_GetAllUserAccounts` | `UserAccount.GetAll` | EMS.UserAccount | DMS |
| `EMS.sp_InsertUserAccount` | `UserAccount.Create` | EMS.UserAccount | DMS |
| `EMS.sp_UpdateLastLogin` | `UserAccount.UpdateLastLogin` | EMS.UserAccount | Core |
| `EMS.sp_UpdateUserConsent` | `UserAccount.UpdateConsent` | EMS.UserAccount | Core |
| `EMS.sp_DeactivateUserAccount` | `UserAccount.Deactivate` | EMS.UserAccount | DMS |
| `EMS.sp_InsertOrgUnit` | `OrgUnit.Insert` | EMS.OrgUnit | DMS |
| `EMS.sp_UpdateOrgUnit` | `OrgUnit.Update` | EMS.OrgUnit | DMS |
| `EMS.sp_GetOrgUnitByID` | `OrgUnit.GetById` | EMS.OrgUnit | DMS, KMS, RGP |
| `EMS.sp_GetOrgUnitHierarchy` | `OrgUnit.GetHierarchy` | EMS.OrgUnit | DMS |
| `EMS.sp_GetOrgUnitByConfidentialityLevel` | `OrgUnit.GetByConfidentialityLevel` | EMS.OrgUnit | DMS |
| `EMS.sp_HasActiveDependents` | `OrgUnit.HasActiveDependents` | EMS.OrgUnit | DMS |
| `EMS.sp_SoftDeleteOrgUnit` | `OrgUnit.SoftDelete` | EMS.OrgUnit | DMS |
| `EMS.sp_SoftDeleteOrgUnit` | `OrgUnit.Deactivate` | EMS.OrgUnit | DMS |
| `EMS.sp_SoftDeleteOrgUnit` | `OrgUnit.ForceDeleteWithDependents` | EMS.OrgUnit, storage.Folder | DMS |
| `dbo.sp_InsertSubCommittee` | `OrgUnit.InsertSubCommitteeRecord` | EMS.m_SubCommittee | DMS |
| `storage.sp_InsertFolder` | `Folder.Insert` | storage.Folder | DMS |
| `storage.sp_UpdateFolder` | `Folder.Update` | storage.Folder | DMS, KMS, RGP |
| `storage.sp_UpdateFolderFileCount` | `Folder.UpdateFileCount` | storage.Folder | DMS, KMS, RGP |
| `storage.sp_GetFolderByID` | `Folder.GetById` | storage.Folder | DMS, KMS, RGP |
| `storage.sp_GetFolderChildren` | `Folder.GetChildren` | storage.Folder | DMS |
| `storage.sp_GetFolderDescendants` | `Folder.GetDescendants` | storage.Folder | DMS, KMS, RGP |
| `storage.sp_GetRootFoldersByOrgUnit` | `Folder.GetRootFoldersByOrgUnit` | storage.Folder | DMS, KMS, RGP |
| `storage.sp_GetDeletedFoldersByOrgUnit` | `Folder.GetDeletedByOrgUnit` | storage.Folder | DMS |
| `storage.sp_GetFoldersByOrgUnit` | `Folder.GetByOrgUnit` | storage.Folder | DMS, KMS, RGP |
| `storage.sp_GetAllFolders` | `Folder.GetAll` | storage.Folder | DMS |
| `storage.sp_GetFolderSiblings` | `Folder.GetSiblings` | storage.Folder | DMS |
| `storage.sp_SoftDeleteFolder` | `Folder.SoftDelete` | storage.Folder | DMS |
| `storage.sp_RestoreFolder` | `Folder.Restore` | storage.Folder | DMS |
| `storage.sp_InsertFileDocument` | `FileDocument.Insert` | storage.FileDocument | DMS, KMS, RGP |
| `storage.sp_GetFileDocumentByID` | `FileDocument.GetById` | storage.FileDocument | DMS, KMS, RGP |
| `storage.sp_GetFileDocumentsByFolder` | `FileDocument.GetByFolder` | storage.FileDocument | DMS, KMS |
| `storage.sp_UpdateFileDocument` | `FileDocument.Update` | storage.FileDocument | DMS |
| `storage.sp_SoftDeleteFileDocument` | `FileDocument.SoftDelete` | storage.FileDocument | DMS, KMS, RGP |
| `storage.sp_GetAllFileDocumentsGroupedByFolder` | `FileDocument.GetAllGroupedByFolder` | storage.FileDocument | DMS |
| `storage.sp_GetFileDocumentsByOrgUnit` | `FileDocument.GetByOrgUnit` | storage.FileDocument | DMS |
| `storage.sp_RenameFileDocument` | `FileDocument.RenameFile` | storage.FileDocument | DMS |
| `security.sp_InsertFolderPermission` | `Permission.Insert` | security.FolderPermission | DMS |
| `security.sp_GetPermissionsByFolder` | `Permission.GetByFolder` | security.FolderPermission | DMS, KMS, RGP |
| `security.sp_GetAllPermissionsByFolder` | `Permission.GetAllByFolder` | security.FolderPermission | DMS |
| `security.sp_GetPermissionsByPrincipal` | `Permission.GetByPrincipal` | security.FolderPermission | DMS |
| `security.sp_RevokePermission` | `Permission.Revoke` | security.FolderPermission | DMS |
| `security.sp_GetPermissionsByFolder` | `Permission.GetAclEntries` | security.FolderPermission | DMS, KMS, RGP |
| `workflow.sp_GetApprovalRequestByID` | `Approval.GetRequest` | workflow.ApprovalRequest | DMS |
| `workflow.sp_GetAllApprovalSteps` | `Approval.GetAllSteps` | workflow.ApprovalStep | DMS |
| `workflow.sp_GetExpiredPendingRequests` | `Approval.GetExpiredPendingRequests` | workflow.ApprovalRequest | DMS |
| `workflow.sp_UpdateApprovalStepStatus` | `Approval.UpdateStepStatus` | workflow.ApprovalStep | DMS |
| `workflow.sp_UpdateApprovalRequestStatus` | `Approval.UpdateRequestStatus` | workflow.ApprovalRequest | DMS |
| `audit.sp_InsertAuditLog` | `Audit.Insert` | audit.AuditLog | DMS, KMS, RGP, WFE |
| `audit.sp_QueryAuditLog` | `Audit.Query` | audit.AuditLog | DMS |
| `EMS.sp_InsertNotification` | `Notification.Insert` | EMS.Notification | DMS, KMS, RGP |
| `security.sp_InsertFilePermission` | `FilePermission.Insert` | security.FilePermission | DMS, KMS, RGP |
| `security.sp_GetFilePermissionsByFile` | `FilePermission.GetByFile` | security.FilePermission | DMS, KMS, RGP |
| `security.sp_RevokeFilePermission` | `FilePermission.Revoke` | security.FilePermission | DMS, KMS, RGP |
| `storage.sp_InsertFolderTemplate` | `FolderTemplate.Insert` | storage.FolderTemplate | DMS |
| `storage.sp_GetAllFolderTemplates` | `FolderTemplate.GetAll` | storage.FolderTemplate | DMS |
| `storage.sp_GetFolderTemplateItems` | `FolderTemplate.GetItems` | storage.FolderTemplateItem | DMS |
| `storage.sp_InsertFolderTemplateItem` | `FolderTemplate.InsertItem` | storage.FolderTemplateItem | DMS |
| `storage.sp_UpdateFolderTemplate` | `FolderTemplate.Update` | storage.FolderTemplate | DMS |
| `storage.sp_DeleteFolderTemplate` | `FolderTemplate.Delete` | storage.FolderTemplate | DMS |
| `storage.sp_UpdateFolderTemplateItem` | `FolderTemplate.UpdateItem` | storage.FolderTemplateItem | DMS |
| `storage.sp_DeleteFolderTemplateItem` | `FolderTemplate.DeleteItem` | storage.FolderTemplateItem | DMS |
| `storage.sp_GetQuickAccessByUser` | `QuickAccess.GetByUser` | storage.QuickAccess | DMS |
| `storage.sp_AddQuickAccess` | `QuickAccess.Add` | storage.QuickAccess | DMS |
| `storage.sp_RemoveQuickAccess` | `QuickAccess.Remove` | storage.QuickAccess | DMS |
| `EMS.sp_GetAllFolderAuthorityConfigs` | `FolderAuthority.GetAll` | EMS.FolderAuthorityConfig | DMS |
| `EMS.sp_ResolveFolderAuthority` | `FolderAuthority.Resolve` | EMS.FolderAuthorityConfig | DMS |
| `EMS.sp_InsertFolderAuthorityConfig` | `FolderAuthority.Insert` | EMS.FolderAuthorityConfig | DMS |
| `EMS.sp_UpdateFolderAuthorityConfig` | `FolderAuthority.Update` | EMS.FolderAuthorityConfig | DMS |
| `EMS.sp_DeleteFolderAuthorityConfig` | `FolderAuthority.Delete` | EMS.FolderAuthorityConfig | DMS |
| `EMS.sp_CreateMenuItem` | `MenuItem.CreateMenuItem` | EMS.MenuItem | DMS |
| `EMS.sp_UpdateMenuItem` | `MenuItem.UpdateMenuItem` | EMS.MenuItem | DMS |
| `EMS.sp_DeleteMenuItem` | `MenuItem.DeleteMenuItem` | EMS.MenuItem | DMS |
| `WFE.sp_CreateWFInstance` | `Workflow.CreateWorkflowInstance` | WFE.t_WFInstance | WFE |
| `WFE.sp_CreateWFStepInstance` | `WorkflowStepInstance.CreateStepInstance` | WFE.t_WFStepInstance | WFE |
| `dbo.sp_WF_CreateDelegation` | `Delegation.CreateDelegation` | dbo.WFDelegation | WFE |
| `dbo.sp_WF_GetDelegationHistory` | `Delegation.GetDelegationsByStepInstance` | dbo.WFDelegation | WFE |
| `dbo.sp_WF_GetSLAComplianceMetrics` | `WFReport.GetSLAComplianceMetrics` | WFE.t_WFInstance, WFE.t_WFStepInstance | WFE |
| `dbo.sp_WF_GetTaskDistribution` | `WFReport.GetTaskDistribution` | WFE.t_WFInstance, WFE.t_WFStepInstance | WFE |

## ภาคผนวก C — Connection string / ฐานข้อมูลที่ต้องตั้ง DataSource

| Key | ใช้ใน Repository | หมายเหตุ |
|---|---|---|
| `ConnectionString` | Approval, Audit, Delegation, Employee, ExternalShare, FileDocument, FilePermission, Folder, FolderAuthority, FolderQuota | CMS_DB (หลัก — DMS/KMS/WFE/RGP ส่วนใหญ่) |
| `ProjectDbConnectionString` | Project, ProjectTask | RunningNumber (สร้างเลข Project/Task) — ฐานเดิม BUI_RG |
| `CmsLinkDbConnectionString` | Kms | RGP อ่านหมวด/บทความ KMS |
| `BisDbConnectionString` | ProjectMemberRole | ดึง SubCommittee/Division/Department สำหรับ Project member role |

- `ProjectMemberRoleRepository.GetSubCommittees/GetDivisions/GetDepartments` อ่าน `EMS.m_SubCommittee/m_Division/m_Department` จาก `BisDbConnectionString` (คนละฐานกับ CMS_DB ที่ DMS ใช้ แม้ชื่อตารางเหมือนกัน — ต้องยืนยันว่าเป็นฐานเดียวกันหรือไม่)
- `TemplateDocRepository` อ่าน `BUI_RG.dbo.m_document` และ `BUI_RG.dbo.m_document_type` (cross-database บน server เดียวกับ `ConnectionString`)
- `WorkflowCreateTemplateForm` ใช้ `ConnectionStrings["CmsDb"]` จาก app.config

## ภาคผนวก D — ที่ไม่ใช่ตารางแต่ API ต้องดูแล

- [ ] ไฟล์เอกสาร DMS บน network drive (`NetworkDriveHelper`) + virus scan (`VirusScanService`) + PDF security (`PdfSecurityHelper`)
- [ ] Active Directory / LDAP login (`AdAuthService`, `LdapUserSearchService`) และ API ภายนอกที่ client เรียกอยู่: `EmployeeApiService`, `MenuApiService`, `NotificationClient`/`NotificationManager` (server URL ตั้งใน config)
- [ ] Excel import ของ KMS (`KBExcelReader`, `KBImportProcessor`) — logic ฝั่ง Service ต้องพอร์ตหรือเรียกเป็น job

