# CLAUDE.md — Core Management System (CMS)
## โครงการ 2511AL01 | .NET 4.5 WinForms | SQL Server 2019

---

## 🎯 Project Context

ระบบ **Core Management System (CMS)** สำหรับบริษัทประกันภัย  
เป็น Enterprise Document & Knowledge Management System แบบ On-Premise  
พัฒนาด้วย **C# .NET Framework 4.5 WinForms** + **SQL Server 2019**

> อ่าน SRS ครบก่อน Implement: `SRS-Master.md` และ `SRS-MOD001-004.md` (และไฟล์ MOD อื่นๆ ที่เกี่ยวข้อง)

---

## 🏗️ Solution Structure

```
CMS.sln
├── CMS.Presentation      (.NET 4.5 WinForms — UI Layer)
├── CMS.Controllers       (Controller Layer — MVC Pattern)
├── CMS.Services          (Business Logic + Validation)
├── CMS.DataAccess        (Repository Layer — Raw ADO.NET)
├── CMS.Models            (Domain Models)
├── CMS.Common            (Shared Utilities, Constants, Enums)
└── CMS.Tests             (Unit Tests — MSTest หรือ NUnit)
```

**กฎสำคัญ:** ห้าม reference Project ข้ามชั้น เช่น Presentation → DataAccess โดยตรง  
ต้องผ่าน: Presentation → Controllers → Services → DataAccess

---

## ⚙️ Tech Stack & Constraints

| ส่วน | Technology | หมายเหตุ |
|------|-----------|----------|
| UI | C# .NET 4.5 WinForms | ใช้ Visual Designer + Code-behind |
| ORM | **ไม่ใช้ ORM** | Raw ADO.NET เท่านั้น (Performance + Compatibility) |
| Database | SQL Server 2019 | Collation: **Thai_CI_AS** |
| Auth | Active Directory / LDAP | `System.DirectoryServices` |
| File Storage | Network Drive (UNC Path) | `\\bui-filecenter\Core Management` |
| Pattern | MVC + Repository | ดูรายละเอียดใน SRS-Master §3.1 |

---

## 📋 Coding Standards

### Naming Convention

```csharp
// Classes & Methods: PascalCase
public class FolderService { }
public void CreateSubFolder() { }

// Variables & Parameters: camelCase
var folderName = "รายงาน";
int parentFolderId = 42;

// Constants: ALL_CAPS
const int MAX_FILE_SIZE_MB = 100;
const int MAX_FOLDER_DEPTH = 10;

// Private fields: _camelCase
private readonly IFolderRepository _folderRepository;
```

### Database Naming (ตาม SRS §3.3)

| Prefix | ประเภท | ตัวอย่าง |
|--------|--------|---------|
| `m_` | Master | `m_OrgUnit`, `m_UserAccount` |
| `s_` | Setup/Config | `s_FolderPermission`, `s_WorkflowDefinition` |
| `t_` | Transaction | `t_FileDocument`, `t_TaskItem` |
| `l_` | Log | `l_AuditLog`, `l_ShareAccessLog` |
| `r_` | Reference | `r_FileType`, `r_Status` |
| `c_` | Configuration | `c_SystemConfig`, `c_EmailTemplate` |

### ADO.NET — กฎบังคับ

```csharp
// ✅ ถูกต้อง — ใช้ Parameterized Query เสมอ
var cmd = new SqlCommand(
    "SELECT * FROM t_FileDocument WHERE FolderID = @FolderID", conn);
cmd.Parameters.AddWithValue("@FolderID", folderId);

// ❌ ห้ามเด็ดขาด — SQL Injection
var cmd = new SqlCommand(
    $"SELECT * FROM t_FileDocument WHERE FolderID = {folderId}", conn);
```

---

## 🔐 Audit Stamp — กฎบังคับ

**ทุก Record ที่สร้าง/แก้ไข ต้องใช้ ADUserName (string snapshot) ไม่ใช่ UserID (FK)**

```csharp
// ✅ ถูกต้อง
record.CreatedBy   = SessionContext.Current.ADUserName; // "domain\\john.doe"
record.CreatedDate = DateTime.Now;
record.UpdatedBy   = SessionContext.Current.ADUserName;
record.UpdatedDate = DateTime.Now;

// ❌ ผิด — ห้ามใช้ UserID เป็น FK สำหรับ Audit Stamp
record.CreatedByUserID = SessionContext.Current.UserID;
```

**l_AuditLog — Append-Only เท่านั้น**
- GRANT เฉพาะ INSERT, SELECT
- ห้าม UPDATE / DELETE แม้แต่ DBA

```csharp
// Pattern การ Write Audit Log
auditRepo.WriteLog(new AuditLog {
    TableName  = "t_FileDocument",
    RecordID   = fileId,
    Action     = AuditAction.FILE_UPLOAD,
    ActionBy   = SessionContext.Current.ADUserName,
    IPAddress  = SessionContext.Current.IPAddress,
    ModuleCode = "MOD-003"
});
```

---

## 🏢 RBAC & Permission

### Roles (ห้าม Hard-code ค่าอื่น)

```csharp
public enum UserRole
{
    Officer    = 1,
    SrOfficer  = 2,
    Manager    = 3,
    Admin      = 4
}
```

### Confidentiality Levels (ห้ามเปลี่ยนค่า)

```csharp
public enum ConfidentialityLevel
{
    HC1_HighConfidential  = 1,  // สีแดง — Restricted
    SC2_SemiConfidential  = 2,  // สีส้ม — Internal
    NC3_NormalConfidential = 3, // สีน้ำเงิน — Standard
    PC4_PublicConfidential = 4  // สีเขียว — Open
}
```

### Permission Model

- **Explicit Deny Overrides Allow** เสมอ
- Sub-Folder inherit Confidentiality จาก Parent (ไม่ลดลงได้)
- 1HC Folder: ห้าม External Share ไม่ว่าในกรณีใด
- Break Inheritance: Admin เท่านั้น

---

## 📁 File Storage Convention

```
Network Drive Root: \\bui-filecenter\Core Management
  (อ่านจาก c_SystemConfig.NetworkDriveRoot — ห้าม Hard-code)

Path Pattern:
  {OrgUnitCode}\{ConfidentialityCode}\{FolderID:D8}\{FileID:D10}_{OriginalName}

ตัวอย่าง:
  \\bui-filecenter\Core Management\DEPT-001\3NC\00000042\0000000123_รายงาน.pdf

Credentials:
  pmc1 / Bui@12345  — Read-Write-Modify
  pmc2 / Bui@12345  — Read Only
```

**กฎ File Operations:**
- ลบไฟล์ = Soft-Delete (Status=3) เท่านั้น ห้ามลบ Physical File ทันที
- ทุกไฟล์ต้องผ่าน Virus Scan ก่อน Write
- Duplicate Filename → Auto-rename ด้วย Timestamp suffix

---

## 🔒 Security Rules

```csharp
// Session Timeout: 30 นาที (Inactivity)
// Account Lockout: 5 ครั้งผิด → Lock 30 นาที
// Concurrent Session: สูงสุด 3 Sessions/User
// TLS 1.2+ สำหรับทุก Network Connection
// AES-256 สำหรับ Sensitive Documents (1HC, 2SC)
```

---

## 🗄️ Database Rules

```sql
-- Collation บังคับ: Thai_CI_AS
-- ใน Column NVARCHAR ที่เก็บภาษาไทย

-- ตัวอย่าง
OrgUnitNameTH NVARCHAR(200) NOT NULL COLLATE Thai_CI_AS

-- Soft-Delete Pattern (ห้าม Hard-Delete)
IsDeleted BIT NOT NULL DEFAULT 0
DeletedByADUser NVARCHAR(100) NULL
DeletedDate DATETIME NULL
```

---

## ✅ Business Rules Summary (Critical)

| Rule | รายละเอียด |
|------|-----------|
| BR-001-06 | ทุก Department ต้องมี 4 Conf-Root Folder อัตโนมัติ (1HC, 2SC, 3NC, 4PC) |
| BR-001-07 | Conf-Root Folder ถูก Lock — เปลี่ยนชื่อ/ลบไม่ได้ |
| BR-002-02 | Folder Depth สูงสุด 10 ระดับจาก Conf-Root |
| BR-002-07 | Sub-Folder ไม่สามารถตั้ง Confidentiality ต่ำกว่า Parent |
| BR-003-01 | ขนาดไฟล์สูงสุด 100 MB |
| BR-003-02 | File Type Whitelist: PDF, DOC, DOCX, XLS, XLSX, PPT, PPTX, JPG, PNG, GIF, ZIP |
| BR-003-05 | Upload Quota/Dept = 40 GB (Default, config ได้) |
| BR-004-04 | Explicit Deny Overrides Allow |
| BR-004-08 | 1HC Folder ห้าม External Share เด็ดขาด |

---

## 🧪 Testing Guidelines

```csharp
// Unit Test ทุก Service Method ที่มี Business Logic
// ตั้งชื่อ Test: MethodName_Scenario_ExpectedResult
[TestMethod]
public void CreateSubFolder_ExceedsMaxDepth_ThrowsBusinessException() { }

[TestMethod]
public void UploadFile_ExceedsQuota_ReturnsErrorCode_FILE005() { }

[TestMethod]
public void ChangePermission_DenyOverridesAllow_UserCannotAccess() { }
```

---

## 📝 Error Handling Pattern

```csharp
// ใช้ Custom Exception สำหรับ Business Rule Violation
public class CmsBusinessException : Exception
{
    public string ErrorCode { get; }
    public CmsBusinessException(string errorCode, string message)
        : base(message) => ErrorCode = errorCode;
}

// ตัวอย่างการโยน
throw new CmsBusinessException("FILE-001", 
    $"ไฟล์มีขนาด {fileSizeMB:F1} MB เกินขีดจำกัด {MAX_FILE_SIZE_MB} MB");

// UI Layer รับ Exception
catch (CmsBusinessException ex)
{
    MessageBox.Show(ex.Message, "ไม่สามารถดำเนินการได้", 
        MessageBoxButtons.OK, MessageBoxIcon.Warning);
    Logger.Warn($"[{ex.ErrorCode}] {ex.Message}");
}
```

---

## 📌 Module Reference

| ไฟล์ SRS | Modules |
|---------|---------|
| `SRS-Master.md` | NFR, PDPA, Audit Trail, Security, Architecture |
| `SRS-MOD001-004.md` | MOD-001 Org Structure, MOD-002 Folder Mgmt, MOD-003 File Upload, MOD-004 Security |
| `SRS-MOD005-007.md` | MOD-005 Sharing, MOD-006 Knowledge Base, MOD-007 Announcement |
| `SRS-MOD008-012.md` | MOD-008 Workflow, MOD-009 Task, MOD-010 Todo, MOD-011 Notes, MOD-012 Work Instructions |
| `SRS-MOD013-019.md` | MOD-013-019 Project, Resource, Timeline, Employee, Programs, IT Landscape, Data Dictionary |
| `SRS-MOD020-023.md` | MOD-020 User/Role, MOD-021 Audit, MOD-022 Config, MOD-023 Integration |

---

*อัปเดตล่าสุด: มีนาคม 2569 | Version 1.0 | โครงการ 2511AL01*
