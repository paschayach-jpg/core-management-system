# Authority Excel Import Implementation Plan

> **For agentic workers:** REQUIRED SUB-SKILL: Use superpowers:subagent-driven-development (recommended) or superpowers:executing-plans to implement this plan task-by-task. Steps use checkbox (`- [ ]`) syntax for tracking.

**Goal:** Add an admin-only form that imports the authority matrix Excel template into position-based `FolderAuthorityConfig` rows, with an opt-in update of `UserAccount.LevelCode`.

**Architecture:** `AuthorityImportForm` (WinForms) → `AuthorityImportController` → `AuthorityImportService` (validate / plan / commit / audit) which uses `AuthorityExcelParser` (NPOI) and the existing `IFolderAuthorityService`, `IUserAccountRepository`, `IHrDepartmentRepository`, `IHrDivisionRepository`, `IAuditService`. The form is opened from a new button on `AuthorityConfigForm`.

**Tech Stack:** C# 5 / .NET Framework 4.5, WinForms, NPOI 2.5.6 (already referenced by `CMS.Services`), ADO.NET.

**Spec:** `docs/superpowers/specs/2026-10-01-authority-excel-import-design.md`

## Deviations from the spec (decided while reading the code)

1. **No automated tests.** User decision: branch `Pitakpong` has no `CMS.Tests` project (removed in commit 05a581d). Verification is MSBuild plus a PowerShell parser check (Task 2) plus a manual run of the form (Task 6). `CLAUDE.md` describes the `main` layout, not this branch.
2. **No new SQL script / SP.** There is no `CMS.Data/Scripts` schema folder on this branch. `UserAccountRepository.Update` already uses inline SQL, so `UpdateLevelCode` does the same.
3. **No cross-repository transaction.** Each repository opens its own connection and the authority SPs are not in the repo. Commit uses **snapshot + compensation**: on any failure, already-applied changes are undone in reverse order and the user gets an error. Compensation is best-effort (an undo can itself fail; the message says so).
4. **No `.resx` for the new form** (not needed; `AuthorityConfigEditDialog` has none either).
5. DTOs live in a single file `AuthorityImportDtos.cs` to keep the `.csproj` edits small.

## Global Constraints

- C# 5 style only: no string interpolation, `?.`, `nameof`, expression-bodied members, `out var`. Explicit null checks with `throw new ArgumentNullException("x")`.
- Private fields `_camelCase`. DTOs named `<Action><Entity>Request` / `<Entity>Result` where applicable.
- Old-style `.csproj`: every new `.cs` file needs a `<Compile Include=...>` entry in the owning project.
- Form rule: all `new Control()`, control fields, `Controls.Add`, `SuspendLayout`, layout code live in `*.Designer.cs`; code-behind holds logic and handlers only.
- Admin only = `UserContext.AppRole == 4`, enforced in the service.
- Audit log must not contain names (PDPA); employee code only.
- Do not stage `bin/`, `obj/`, `.vs/`. Do not copy the DB credentials in `app.config`.
- Per the user's CLAUDE.md: **ask before every `git commit`**; commit messages need a body listing all changes (check `git diff --staged`) and end with the attribution line given in the session. Commit steps below are therefore "propose and wait for approval".
- Build command (run from repo root, PowerShell):
  `& "C:\Program Files\Microsoft Visual Studio\2022\Professional\MSBuild\Current\Bin\MSBuild.exe" CMS.sln /p:Configuration=Debug /nologo /v:minimal`

## Review Focus

Failure modes the spec implies but nothing else pins down (each is checked in the task named):

1. Trailing/extra whitespace in names (the sample has `รับประกัน 1 ` with a trailing space in the department column) must not create separate positions or fail org lookup → Task 3 trims everywhere; Task 2 check prints trimmed values.
2. Employee code stored as a numeric cell loses leading zeros (`01111` → `1111`) → Task 2 emits `NUMERIC_EMPLOYEE_CODE` warning.
3. Workbook open in Excel (file locked) or not a real `.xlsx` → Task 2 opens with `FileShare.ReadWrite` and turns failures into a readable message.
4. Empty sheet / wrong template → `TEMPLATE_MISMATCH` Error, no rows, Commit disabled → Task 2/6.
5. A new position label that collides with an existing non-position config label (e.g. a department-named config) would be wiped by `Delete(label)` → Task 3 `LABEL_COLLISION` Error.

---

## File Structure

| File | Responsibility |
|---|---|
| `CMS.Services/DTOs/AuthorityImportDtos.cs` (new) | Row, issue, plan, preview, result types |
| `CMS.Services/AuthorityExcelParser.cs` (new) | NPOI read + template check → `AuthorityImportRow` list |
| `CMS.Services/Interfaces/IAuthorityImportService.cs` (new) | Service contract |
| `CMS.Services/AuthorityImportService.cs` (new) | Validation, planning, commit with compensation, audit |
| `CMS.Data/Interfaces/IUserAccountRepository.cs` (mod) | `UpdateLevelCode` |
| `CMS.Data/Repositories/UserAccountRepository.cs` (mod) | `UpdateLevelCode` inline SQL |
| `CMS.Controllers/DMS/Document/Interfaces/IAuthorityImportController.cs` (new) | Controller contract |
| `CMS.Controllers/DMS/Document/AuthorityImportController.cs` (new) | Pass-through |
| `CMS.Presentation/Modules/DMS/AuthorityImportForm.cs` / `.Designer.cs` (new) | UI |
| `CMS.Presentation/Modules/DMS/AuthorityConfigForm.cs` / `.Designer.cs` (mod) | "นำเข้าจาก Excel" button |
| `CMS.Presentation/MDI_Container.cs`, `Program.cs` (mod) | Wiring |

---

### Task 1: DTOs, service contract, and `UpdateLevelCode`

**Files:**
- Create: `CMS.Services/DTOs/AuthorityImportDtos.cs`
- Create: `CMS.Services/Interfaces/IAuthorityImportService.cs`
- Modify: `CMS.Services/CMS.Services.csproj` (add two `<Compile Include>`)
- Modify: `CMS.Data/Interfaces/IUserAccountRepository.cs`
- Modify: `CMS.Data/Repositories/UserAccountRepository.cs` (add method after `Update`, ~line 170)

**Interfaces:**
- Produces: all types below, `IAuthorityImportService`, `IUserAccountRepository.UpdateLevelCode(int userId, string levelCode)`.

- [ ] **Step 1: Create DTOs**

```csharp
using System.Collections.Generic;
using System.Linq;

namespace CMS.Services.DTOs
{
    public enum AuthorityImportSeverity { Warning = 1, Error = 2 }

    public enum AuthorityImportStatus { New = 1, Updated = 2, Unchanged = 3, NotFound = 4 }

    public class AuthorityImportIssue
    {
        /// <summary>1-based Excel row number; 0 = file level.</summary>
        public int RowNumber { get; set; }
        public AuthorityImportSeverity Severity { get; set; }
        public string Code { get; set; }
        public string Message { get; set; }
    }

    /// <summary>One employee row. Permissions index = levelIndex*4 + action; levelIndex 0=HC,1=SC,2=NC,3=PC; action 0=View,1=Add,2=Edit,3=Approve.</summary>
    public class AuthorityImportRow
    {
        public AuthorityImportRow() { Permissions = new bool[16]; }

        public int RowNumber { get; set; }
        public string EmployeeCode { get; set; }
        public string Level { get; set; }
        public string Position { get; set; }
        public string Department { get; set; }
        public string Division { get; set; }
        public string Remark { get; set; }
        public bool[] Permissions { get; set; }
    }

    public class AuthorityImportPositionPlan
    {
        public string Label { get; set; }
        public string PositionName { get; set; }
        public string DepartmentCode { get; set; }
        public string DivisionCode { get; set; }
        public string DepartmentText { get; set; }
        public string DivisionText { get; set; }
        public bool[] Permissions { get; set; }
        public AuthorityImportStatus Status { get; set; }
    }

    public class AuthorityImportLevelPlan
    {
        public string EmployeeCode { get; set; }
        public int UserId { get; set; }
        public string OldLevel { get; set; }
        public string NewLevel { get; set; }
        /// <summary>Updated | Unchanged | NotFound</summary>
        public AuthorityImportStatus Status { get; set; }
    }

    public class AuthorityImportPreview
    {
        public AuthorityImportPreview()
        {
            Rows = new List<AuthorityImportRow>();
            Issues = new List<AuthorityImportIssue>();
            PositionPlans = new List<AuthorityImportPositionPlan>();
            LevelPlans = new List<AuthorityImportLevelPlan>();
        }

        public string FilePath { get; set; }
        public string SheetName { get; set; }
        public bool UpdateUserLevel { get; set; }
        public List<AuthorityImportRow> Rows { get; set; }
        public List<AuthorityImportIssue> Issues { get; set; }
        public List<AuthorityImportPositionPlan> PositionPlans { get; set; }
        public List<AuthorityImportLevelPlan> LevelPlans { get; set; }

        public bool HasErrors { get { return Issues.Any(i => i.Severity == AuthorityImportSeverity.Error); } }
        public int NewCount { get { return PositionPlans.Count(p => p.Status == AuthorityImportStatus.New); } }
        public int UpdatedCount { get { return PositionPlans.Count(p => p.Status == AuthorityImportStatus.Updated); } }
        public int UnchangedCount { get { return PositionPlans.Count(p => p.Status == AuthorityImportStatus.Unchanged); } }
        public int LevelChangeCount { get { return LevelPlans.Count(p => p.Status == AuthorityImportStatus.Updated); } }
        public bool HasChanges { get { return NewCount + UpdatedCount + LevelChangeCount > 0; } }
    }

    public class AuthorityImportResult
    {
        public int ConfigsCreated { get; set; }
        public int ConfigsUpdated { get; set; }
        public int LevelsChanged { get; set; }
    }
}
```

- [ ] **Step 2: Create the service contract**

```csharp
using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Services.Interfaces
{
    public interface IAuthorityImportService
    {
        /// <summary>Sheet names of the workbook. Admin only.</summary>
        IList<string> GetSheetNames(string filePath, UserContext user);

        /// <summary>Parse + validate + diff against current data. Writes nothing.</summary>
        AuthorityImportPreview BuildPreview(string filePath, string sheetName, bool updateUserLevel, UserContext user);

        /// <summary>Apply a preview that has no Errors. Compensates on failure. Admin only.</summary>
        AuthorityImportResult Commit(AuthorityImportPreview preview, UserContext user);
    }
}
```

- [ ] **Step 3: Add `UpdateLevelCode` to the interface and repository**

In `IUserAccountRepository` add:

```csharp
        /// <summary>อัพเดท LevelCode ของ user (null = ล้างค่า)</summary>
        void UpdateLevelCode(int userId, string levelCode);
```

In `UserAccountRepository` add after `Update`:

```csharp
        public void UpdateLevelCode(int userId, string levelCode)
        {
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    "UPDATE EMS.UserAccount SET LevelCode = @LevelCode, UpdatedAt = GETUTCDATE() WHERE UserID = @UserID", conn))
                {
                    cmd.Parameters.AddWithValue("@UserID",    userId);
                    cmd.Parameters.AddWithValue("@LevelCode", (object)levelCode ?? DBNull.Value);
                    cmd.ExecuteNonQuery();
                }
            }
        }
```

- [ ] **Step 4: Register in `CMS.Services.csproj`** next to the `KBExcelReader.cs` line:

```xml
    <Compile Include="DTOs\AuthorityImportDtos.cs" />
    <Compile Include="Interfaces\IAuthorityImportService.cs" />
```

- [ ] **Step 5: Build.** Run the build command. Expected: `CMS.Data` and `CMS.Services` compile; any other class implementing `IUserAccountRepository` (search with Grep `: IUserAccountRepository`) must also get the method — fix if the build reports CS0535.

- [ ] **Step 6: Commit** — propose message "feat(authority-import): add DTOs, service contract and UpdateLevelCode" with a body listing each file, then wait for approval.

---

### Task 2: `AuthorityExcelParser` + PowerShell verification

**Files:**
- Create: `CMS.Services/AuthorityExcelParser.cs`
- Modify: `CMS.Services/CMS.Services.csproj`

**Interfaces:**
- Consumes: `AuthorityImportRow`, `AuthorityImportIssue`, `AuthorityImportSeverity` (Task 1).
- Produces: `public class AuthorityExcelParser { IList<string> GetSheetNames(string filePath); IList<AuthorityImportRow> Parse(string filePath, string sheetName, IList<AuthorityImportIssue> issues); }`

- [ ] **Step 1: Write the parser**

```csharp
using System;
using System.Collections.Generic;
using System.IO;
using CMS.Services.DTOs;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace CMS.Services
{
    /// <summary>
    /// Reads the authority matrix template (header rows 1-2, data from row 3):
    /// B=employee code, E=level, F=position, G=department, H=division,
    /// I-L=HC, M-P=SC, Q-T=NC, U-X=PC (view/add/edit/approve), Y=remark.
    /// </summary>
    public class AuthorityExcelParser
    {
        private const int ColCode = 1, ColLevel = 4, ColPosition = 5, ColDept = 6, ColDiv = 7, ColRemark = 24;
        private const int FirstDataRowIndex = 2;
        private static readonly int[] GroupStart = { 8, 12, 16, 20 };
        private static readonly string[] GroupName = { "HC", "SC", "NC", "PC" };
        private static readonly string[] ActionName = { "ดู", "เพิ่ม", "แก้ไข", "อนุมัติ" };

        public IList<string> GetSheetNames(string filePath)
        {
            var wb = Open(filePath);
            var names = new List<string>();
            for (int i = 0; i < wb.NumberOfSheets; i++) names.Add(wb.GetSheetName(i));
            return names;
        }

        public IList<AuthorityImportRow> Parse(string filePath, string sheetName, IList<AuthorityImportIssue> issues)
        {
            if (issues == null) throw new ArgumentNullException("issues");
            var rows = new List<AuthorityImportRow>();
            var wb = Open(filePath);
            var sheet = string.IsNullOrEmpty(sheetName) ? wb.GetSheetAt(0) : wb.GetSheet(sheetName);
            if (sheet == null)
            {
                AddIssue(issues, 0, AuthorityImportSeverity.Error, "SHEET_NOT_FOUND", "ไม่พบ sheet ที่เลือก");
                return rows;
            }

            string mismatch = CheckTemplate(sheet);
            if (mismatch != null)
            {
                AddIssue(issues, 0, AuthorityImportSeverity.Error, "TEMPLATE_MISMATCH",
                    "ไม่ใช่ template ที่รองรับ: " + mismatch);
                return rows;
            }

            var fmt = new DataFormatter();
            for (int i = FirstDataRowIndex; i <= sheet.LastRowNum; i++)
            {
                var r = sheet.GetRow(i);
                if (r == null) continue;

                string code = Text(fmt, r, ColCode);
                string position = Text(fmt, r, ColPosition);
                bool anyMark = false;
                var perms = new bool[16];
                for (int g = 0; g < 4; g++)
                    for (int a = 0; a < 4; a++)
                    {
                        string t = Text(fmt, r, GroupStart[g] + a);
                        if (t.Length == 0) continue;
                        if (t.Equals("x", StringComparison.OrdinalIgnoreCase)) { perms[g * 4 + a] = true; anyMark = true; }
                        else AddIssue(issues, i + 1, AuthorityImportSeverity.Warning, "UNEXPECTED_CELL",
                            "ค่าในช่องสิทธิ์ไม่ใช่ X (\"" + t + "\") ถือเป็นไม่มีสิทธิ์");
                    }

                if (code.Length == 0 && position.Length == 0 && !anyMark) continue;
                if (code.Length == 0)
                {
                    AddIssue(issues, i + 1, AuthorityImportSeverity.Warning, "SKIPPED_ROW", "ไม่มีรหัสพนักงาน ข้ามแถวนี้");
                    continue;
                }

                var codeCell = r.GetCell(ColCode);
                if (codeCell != null && codeCell.CellType == CellType.Numeric)
                    AddIssue(issues, i + 1, AuthorityImportSeverity.Warning, "NUMERIC_EMPLOYEE_CODE",
                        "รหัสพนักงานเป็นตัวเลข เลข 0 นำหน้าอาจหายไป");

                rows.Add(new AuthorityImportRow
                {
                    RowNumber = i + 1,
                    EmployeeCode = code,
                    Level = Text(fmt, r, ColLevel).ToUpperInvariant(),
                    Position = position,
                    Department = Text(fmt, r, ColDept),
                    Division = Text(fmt, r, ColDiv),
                    Remark = Text(fmt, r, ColRemark),
                    Permissions = perms
                });
            }

            if (rows.Count == 0)
                AddIssue(issues, 0, AuthorityImportSeverity.Error, "NO_DATA", "ไม่พบข้อมูลพนักงานใน sheet นี้");
            return rows;
        }

        private static string CheckTemplate(ISheet sheet)
        {
            var fmt = new DataFormatter();
            var h1 = sheet.GetRow(0);
            var h2 = sheet.GetRow(1);
            if (h1 == null || h2 == null) return "ไม่พบแถวหัวตาราง";
            if (!Text(fmt, h1, ColCode).Contains("รหัสพนักงาน")) return "คอลัมน์ B ต้องเป็น \"รหัสพนักงาน\"";
            if (!Text(fmt, h1, ColPosition).Contains("ตำแหน่ง")) return "คอลัมน์ F ต้องเป็น \"ตำแหน่ง\"";
            for (int g = 0; g < 4; g++)
            {
                if (Text(fmt, h1, GroupStart[g]).IndexOf(GroupName[g], StringComparison.OrdinalIgnoreCase) < 0)
                    return "ไม่พบกลุ่มสิทธิ์ " + GroupName[g];
                for (int a = 0; a < 4; a++)
                    if (Text(fmt, h2, GroupStart[g] + a) != ActionName[a])
                        return "หัวคอลัมน์ย่อยของ " + GroupName[g] + " ต้องเป็น ดู/เพิ่ม/แก้ไข/อนุมัติ";
            }
            return null;
        }

        private static string Text(DataFormatter fmt, IRow row, int col)
        {
            var c = row.GetCell(col);
            return c == null ? "" : (fmt.FormatCellValue(c) ?? "").Trim();
        }

        private static IWorkbook Open(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
            if (!File.Exists(filePath)) throw new InvalidOperationException("ไม่พบไฟล์: " + filePath);
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    return new XSSFWorkbook(fs);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("เปิดไฟล์ไม่ได้ (ต้องเป็น .xlsx ที่ไม่เสียหาย): " + ex.Message, ex);
            }
        }

        private static void AddIssue(IList<AuthorityImportIssue> issues, int row, AuthorityImportSeverity sev, string code, string msg)
        {
            issues.Add(new AuthorityImportIssue { RowNumber = row, Severity = sev, Code = code, Message = msg });
        }
    }
}
```

- [ ] **Step 2: Register** `<Compile Include="AuthorityExcelParser.cs" />` in `CMS.Services.csproj`, then build (expect success).

- [ ] **Step 3: Verify against the sample file** with a throwaway script (scratchpad dir, not the repo). Save as `verify-parser.ps1`:

```powershell
$root = "C:\Users\pitakpong\Documents\core-management-system"
$bin  = Join-Path $root "CMS.Services\bin\Debug"
Get-ChildItem $bin -Filter *.dll | ForEach-Object { try { [Reflection.Assembly]::LoadFrom($_.FullName) | Out-Null } catch {} }
$file = Get-ChildItem (Join-Path $root "Docdmc") -Filter "*IT_test.xlsx" | Select-Object -First 1
$p = New-Object CMS.Services.AuthorityExcelParser
$issues = New-Object 'System.Collections.Generic.List[CMS.Services.DTOs.AuthorityImportIssue]'
$rows = $p.Parse($file.FullName, $null, $issues)
"rows: " + $rows.Count
$rows | ForEach-Object { "{0} | {1} | {2} | [{3}] | [{4}] | {5}" -f $_.RowNumber, $_.EmployeeCode, $_.Level, $_.Department, $_.Position, (($_.Permissions | ForEach-Object { if ($_) {1} else {0} }) -join "") }
"issues: " + $issues.Count
$issues | ForEach-Object { "{0} {1} {2}" -f $_.RowNumber, $_.Severity, $_.Code }
```

Run: `powershell -File <scratchpad>\verify-parser.ps1`
Expected: `rows: 9`; first row `3 | 01111 | 4A | [รับประกัน 1] | ...` with permission bits `1000100010110000`... precisely HC `1000`, SC `1000`, NC `1101`, PC `1100` → `1000100011011100`; second row (`01671`, `3B`) bits `1000100011111100`; department shown **trimmed** (no trailing space); `issues: 0` (the parser emits no issue for the sample; the remark warning is produced later by the service in Task 3). If assembly loading fails because NPOI is not in `CMS.Services\bin\Debug`, add `$root\packages\NPOI.2.5.6\lib\net45` to the load loop.

- [ ] **Step 4: Negative check.** Run the script on a copy of the file with sheet cell B1 changed (or a different .xlsx such as any file in the repo `Docdmc` folder that is not this template). Expected: `rows: 0`, issues contain `TEMPLATE_MISMATCH`.

- [ ] **Step 5: Commit** — propose "feat(authority-import): add Excel template parser", wait for approval.

---

### Task 3: `AuthorityImportService`

**Files:**
- Create: `CMS.Services/AuthorityImportService.cs`
- Modify: `CMS.Services/CMS.Services.csproj`

**Interfaces:**
- Consumes: Task 1 DTOs and contract; `AuthorityExcelParser` (Task 2); `IFolderAuthorityService.GetAll()/Save/Delete`; `IUserAccountRepository.GetAll()/UpdateLevelCode`; `IHrDepartmentRepository.GetAll()`; `IHrDivisionRepository.GetAll()`; `IAuditService.Log(AuditEntry)`.
- Produces: `public class AuthorityImportService : IAuthorityImportService` with ctor `(IFolderAuthorityService, IUserAccountRepository, IHrDepartmentRepository, IHrDivisionRepository, IAuditService, AuthorityExcelParser parser = null)`.

- [ ] **Step 1: Write the service**

```csharp
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.RegularExpressions;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    public class AuthorityImportService : IAuthorityImportService
    {
        private const int AdminRole = 4;
        private static readonly string[] LevelCodes = { "1HC", "2SC", "3NC", "4PC" };
        private static readonly Regex LevelPattern = new Regex(@"^[1-9][A-Z]$", RegexOptions.Compiled);

        private readonly IFolderAuthorityService _authority;
        private readonly IUserAccountRepository _users;
        private readonly IHrDepartmentRepository _departments;
        private readonly IHrDivisionRepository _divisions;
        private readonly IAuditService _audit;
        private readonly AuthorityExcelParser _parser;

        public AuthorityImportService(IFolderAuthorityService authority, IUserAccountRepository users,
            IHrDepartmentRepository departments, IHrDivisionRepository divisions, IAuditService audit,
            AuthorityExcelParser parser = null)
        {
            if (authority == null) throw new ArgumentNullException("authority");
            if (users == null) throw new ArgumentNullException("users");
            if (departments == null) throw new ArgumentNullException("departments");
            if (divisions == null) throw new ArgumentNullException("divisions");
            if (audit == null) throw new ArgumentNullException("audit");
            _authority = authority;
            _users = users;
            _departments = departments;
            _divisions = divisions;
            _audit = audit;
            _parser = parser ?? new AuthorityExcelParser();
        }

        public IList<string> GetSheetNames(string filePath, UserContext user)
        {
            RequireAdmin(user);
            return _parser.GetSheetNames(filePath);
        }

        public AuthorityImportPreview BuildPreview(string filePath, string sheetName, bool updateUserLevel, UserContext user)
        {
            RequireAdmin(user);
            var preview = new AuthorityImportPreview
            {
                FilePath = filePath, SheetName = sheetName, UpdateUserLevel = updateUserLevel
            };

            preview.Rows.AddRange(_parser.Parse(filePath, sheetName, preview.Issues));
            if (preview.HasErrors) return preview;

            ValidateRows(preview, updateUserLevel);
            BuildPositionPlans(preview);
            if (updateUserLevel) BuildLevelPlans(preview);
            return preview;
        }

        // ── Validation ──────────────────────────────────────────────────────

        private static void ValidateRows(AuthorityImportPreview p, bool checkLevel)
        {
            var seen = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var r in p.Rows)
            {
                if (string.IsNullOrEmpty(r.Position))
                    Add(p, r.RowNumber, AuthorityImportSeverity.Error, "MISSING_POSITION", "ไม่มีตำแหน่ง");
                if (!seen.Add(r.EmployeeCode))
                    Add(p, r.RowNumber, AuthorityImportSeverity.Error, "DUPLICATE_EMPLOYEE", "รหัสพนักงานซ้ำในไฟล์: " + r.EmployeeCode);
                if (checkLevel && !LevelPattern.IsMatch(r.Level ?? ""))
                    Add(p, r.RowNumber, AuthorityImportSeverity.Error, "INVALID_LEVEL", "รูปแบบ Level ไม่ถูกต้อง: \"" + r.Level + "\"");
                for (int g = 0; g < 4; g++)
                    if (r.Permissions[g * 4 + 3] && !r.Permissions[g * 4])
                        Add(p, r.RowNumber, AuthorityImportSeverity.Warning, "APPROVE_WITHOUT_VIEW",
                            "มีสิทธิ์อนุมัติแต่ไม่มีสิทธิ์ดู (" + LevelCodes[g] + ")");
                if (!string.IsNullOrEmpty(r.Remark))
                    Add(p, r.RowNumber, AuthorityImportSeverity.Warning, "REMARK", "หมายเหตุ: " + r.Remark);
            }
        }

        // ── Position plans ──────────────────────────────────────────────────

        private void BuildPositionPlans(AuthorityImportPreview p)
        {
            var deptByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var d in _departments.GetAll())
            {
                if (string.IsNullOrEmpty(d.DepartmentCode)) continue;
                foreach (var n in new[] { d.DepartmentNameTh, d.DepartmentName, d.DepartmentCode })
                    if (!string.IsNullOrWhiteSpace(n) && !deptByName.ContainsKey(n.Trim())) deptByName[n.Trim()] = d.DepartmentCode;
            }
            var divByName = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            foreach (var d in _divisions.GetAll())
            {
                if (string.IsNullOrEmpty(d.DivisionCode)) continue;
                foreach (var n in new[] { d.DivisionName, d.DivisionCode })
                    if (!string.IsNullOrWhiteSpace(n) && !divByName.ContainsKey(n.Trim())) divByName[n.Trim()] = d.DivisionCode;
            }

            var existing = _authority.GetAll().GroupBy(c => c.ConfigLabel ?? "", StringComparer.OrdinalIgnoreCase)
                                     .ToDictionary(g => g.Key, g => g.ToList(), StringComparer.OrdinalIgnoreCase);

            foreach (var grp in p.Rows.Where(r => !string.IsNullOrEmpty(r.Position))
                                      .GroupBy(r => r.Position.Trim(), StringComparer.OrdinalIgnoreCase))
            {
                var first = grp.First();
                bool conflict = false;
                foreach (var r in grp.Skip(1))
                    if (!r.Permissions.SequenceEqual(first.Permissions))
                    {
                        conflict = true;
                        Add(p, r.RowNumber, AuthorityImportSeverity.Error, "POSITION_CONFLICT",
                            "ตำแหน่ง \"" + grp.Key + "\" มีสิทธิ์ไม่ตรงกับแถว " + first.RowNumber);
                    }
                if (conflict) continue;

                var plan = new AuthorityImportPositionPlan
                {
                    Label = grp.Key, PositionName = grp.Key, Permissions = first.Permissions,
                    DepartmentText = first.Department, DivisionText = first.Division
                };

                string code;
                if (!string.IsNullOrEmpty(first.Department))
                {
                    if (deptByName.TryGetValue(first.Department, out code)) plan.DepartmentCode = code;
                    else Add(p, first.RowNumber, AuthorityImportSeverity.Warning, "ORG_UNRESOLVED",
                        "ไม่พบแผนก \"" + first.Department + "\" ใน HR master จะไม่กำหนดเงื่อนไขแผนก");
                }
                if (!string.IsNullOrEmpty(first.Division))
                {
                    if (divByName.TryGetValue(first.Division, out code)) plan.DivisionCode = code;
                    else Add(p, first.RowNumber, AuthorityImportSeverity.Warning, "ORG_UNRESOLVED",
                        "ไม่พบฝ่าย \"" + first.Division + "\" ใน HR master จะไม่กำหนดเงื่อนไขฝ่าย");
                }

                List<FolderAuthorityConfig> rows;
                if (!existing.TryGetValue(plan.Label, out rows) || rows.Count == 0)
                    plan.Status = AuthorityImportStatus.New;
                else if (!string.Equals((rows[0].PositionName ?? "").Trim(), plan.PositionName, StringComparison.OrdinalIgnoreCase))
                {
                    Add(p, first.RowNumber, AuthorityImportSeverity.Error, "LABEL_COLLISION",
                        "มี config ชื่อ \"" + plan.Label + "\" อยู่แล้วแต่ไม่ใช่ config ของตำแหน่งนี้ การนำเข้าจะลบทับ");
                    continue;
                }
                else
                    plan.Status = SameAsExisting(plan, rows) ? AuthorityImportStatus.Unchanged : AuthorityImportStatus.Updated;

                p.PositionPlans.Add(plan);
            }
        }

        private static bool SameAsExisting(AuthorityImportPositionPlan plan, List<FolderAuthorityConfig> rows)
        {
            if (!string.Equals(plan.DepartmentCode ?? "", rows[0].DepartmentCode ?? "", StringComparison.OrdinalIgnoreCase)) return false;
            if (!string.Equals(plan.DivisionCode ?? "", rows[0].DivisionCode ?? "", StringComparison.OrdinalIgnoreCase)) return false;
            for (int g = 0; g < 4; g++)
            {
                var row = rows.FirstOrDefault(r => r.ConfidentialityLevel == g + 1);
                bool v = row != null && row.CanView, a = row != null && row.CanAdd,
                     e = row != null && row.CanEdit, ap = row != null && row.CanApprove;
                if (v != plan.Permissions[g * 4] || a != plan.Permissions[g * 4 + 1] ||
                    e != plan.Permissions[g * 4 + 2] || ap != plan.Permissions[g * 4 + 3]) return false;
            }
            return true;
        }

        // ── Level plans ─────────────────────────────────────────────────────

        private void BuildLevelPlans(AuthorityImportPreview p)
        {
            var byCode = new Dictionary<string, UserAccount>(StringComparer.OrdinalIgnoreCase);
            foreach (var u in _users.GetAll())
                if (!string.IsNullOrWhiteSpace(u.EmployeeCode) && !byCode.ContainsKey(u.EmployeeCode.Trim()))
                    byCode[u.EmployeeCode.Trim()] = u;

            foreach (var r in p.Rows)
            {
                UserAccount u;
                if (!byCode.TryGetValue(r.EmployeeCode, out u))
                {
                    p.LevelPlans.Add(new AuthorityImportLevelPlan
                    {
                        EmployeeCode = r.EmployeeCode, NewLevel = r.Level, Status = AuthorityImportStatus.NotFound
                    });
                    Add(p, r.RowNumber, AuthorityImportSeverity.Warning, "USER_NOT_FOUND",
                        "ไม่พบผู้ใช้รหัส " + r.EmployeeCode + " ในระบบ (อาจยังไม่เคย login) ข้ามการอัปเดต Level");
                    continue;
                }
                string old = (u.LevelCode ?? "").Trim().ToUpperInvariant();
                p.LevelPlans.Add(new AuthorityImportLevelPlan
                {
                    EmployeeCode = r.EmployeeCode, UserId = u.UserID, OldLevel = old, NewLevel = r.Level,
                    Status = old == r.Level ? AuthorityImportStatus.Unchanged : AuthorityImportStatus.Updated
                });
            }
        }

        // ── Commit ──────────────────────────────────────────────────────────

        public AuthorityImportResult Commit(AuthorityImportPreview preview, UserContext user)
        {
            RequireAdmin(user);
            if (preview == null) throw new ArgumentNullException("preview");
            if (preview.HasErrors) throw new InvalidOperationException("ไม่สามารถนำเข้าได้ เพราะยังมี Error");

            var result = new AuthorityImportResult();
            var existing = _authority.GetAll().ToList();
            var undo = new Stack<Action>();

            try
            {
                foreach (var plan in preview.PositionPlans.Where(x => x.Status != AuthorityImportStatus.Unchanged))
                {
                    var snapshot = existing
                        .Where(c => string.Equals(c.ConfigLabel, plan.Label, StringComparison.OrdinalIgnoreCase))
                        .Select(Clone).ToList();
                    var label = plan.Label;
                    var uid = user.UserID;

                    if (plan.Status == AuthorityImportStatus.Updated) _authority.Delete(label, uid);
                    undo.Push(() => Restore(label, snapshot, uid));
                    for (int g = 0; g < 4; g++)
                        _authority.Save(new FolderAuthorityConfig
                        {
                            ConfigLabel = plan.Label, PositionName = plan.PositionName,
                            DepartmentCode = plan.DepartmentCode, DivisionCode = plan.DivisionCode,
                            ConfidentialityLevel = g + 1, LevelCode = LevelCodes[g],
                            CanView = plan.Permissions[g * 4], CanAdd = plan.Permissions[g * 4 + 1],
                            CanEdit = plan.Permissions[g * 4 + 2], CanApprove = plan.Permissions[g * 4 + 3]
                        }, uid);

                    if (plan.Status == AuthorityImportStatus.New) result.ConfigsCreated++; else result.ConfigsUpdated++;
                }

                if (preview.UpdateUserLevel)
                    foreach (var lp in preview.LevelPlans.Where(x => x.Status == AuthorityImportStatus.Updated))
                    {
                        int userId = lp.UserId;
                        string old = string.IsNullOrEmpty(lp.OldLevel) ? null : lp.OldLevel;
                        _users.UpdateLevelCode(userId, lp.NewLevel);
                        undo.Push(() => _users.UpdateLevelCode(userId, old));
                        result.LevelsChanged++;
                    }
            }
            catch (Exception ex)
            {
                int failedUndo = 0;
                while (undo.Count > 0)
                {
                    try { undo.Pop()(); } catch { failedUndo++; }
                }
                throw new InvalidOperationException(
                    "การนำเข้าล้มเหลวและพยายามย้อนกลับแล้ว"
                    + (failedUndo > 0 ? " (ย้อนกลับไม่สำเร็จ " + failedUndo + " ขั้นตอน กรุณาตรวจสอบข้อมูลด้วยตนเอง)" : "")
                    + ": " + ex.Message, ex);
            }

            WriteAudit(preview, result, user);
            return result;
        }

        private void Restore(string label, List<FolderAuthorityConfig> snapshot, int userId)
        {
            _authority.Delete(label, userId);
            foreach (var c in snapshot)
            {
                c.ConfigID = 0;
                _authority.Save(c, userId);
            }
        }

        private static FolderAuthorityConfig Clone(FolderAuthorityConfig c)
        {
            return new FolderAuthorityConfig
            {
                ConfigID = c.ConfigID, ConfigLabel = c.ConfigLabel, PositionName = c.PositionName,
                DepartmentCode = c.DepartmentCode, DivisionCode = c.DivisionCode,
                ConfidentialityLevel = c.ConfidentialityLevel, LevelCode = c.LevelCode,
                CanView = c.CanView, CanAdd = c.CanAdd, CanEdit = c.CanEdit, CanApprove = c.CanApprove
            };
        }

        private void WriteAudit(AuthorityImportPreview preview, AuthorityImportResult result, UserContext user)
        {
            string file = System.IO.Path.GetFileName(preview.FilePath ?? "");
            _audit.Log(NewEntry(user, "AuthorityImport", "FolderAuthorityConfig", null, null,
                string.Format("file={0}; sheet={1}; created={2}; updated={3}; levelChanges={4}",
                    file, preview.SheetName, result.ConfigsCreated, result.ConfigsUpdated, result.LevelsChanged)));

            if (!preview.UpdateUserLevel) return;
            foreach (var lp in preview.LevelPlans.Where(x => x.Status == AuthorityImportStatus.Updated))
                _audit.Log(NewEntry(user, "AuthorityImportUserLevel", "UserAccount", lp.UserId,
                    "emp=" + lp.EmployeeCode + "; LevelCode=" + lp.OldLevel, "LevelCode=" + lp.NewLevel));
        }

        private static AuditEntry NewEntry(UserContext user, string action, string table, int? recordId, string oldValue, string newValue)
        {
            return new AuditEntry
            {
                Module = "DMS", Action = action, UserID = user.UserID, UserName = user.ADUserName,
                UserRole = user.AppRole, TableName = table, RecordID = recordId,
                OldValue = oldValue, NewValue = newValue, Result = 1,
                IPAddress = user.IPAddress, MachineName = user.MachineName, SessionID = user.SessionID
            };
        }

        // ── Helpers ─────────────────────────────────────────────────────────

        private static void RequireAdmin(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            if (user.AppRole != AdminRole)
                throw new UnauthorizedAccessException("เฉพาะผู้ดูแลระบบ (Admin) เท่านั้นที่นำเข้าสิทธิ์ได้");
        }

        private static void Add(AuthorityImportPreview p, int row, AuthorityImportSeverity sev, string code, string msg)
        {
            p.Issues.Add(new AuthorityImportIssue { RowNumber = row, Severity = sev, Code = code, Message = msg });
        }
    }
}
```

- [ ] **Step 2: Register** `<Compile Include="AuthorityImportService.cs" />` in `CMS.Services.csproj`. Confirm `CMS.Services` already references `CMS.Data` (other services use repositories, so it does).

- [ ] **Step 3: Build.** Expected: success. Fix any C# 5 incompatibilities the compiler reports (e.g. if `HashSet`/LINQ usings are missing).

- [ ] **Step 4: Commit** — propose "feat(authority-import): add import service with validation and compensating commit".

---

### Task 4: Controller layer

**Files:**
- Create: `CMS.Controllers/DMS/Document/Interfaces/IAuthorityImportController.cs`
- Create: `CMS.Controllers/DMS/Document/AuthorityImportController.cs`
- Modify: `CMS.Controllers/CMS.Controllers.csproj` (next to the `AuthorityConfigController` entries at lines 73 and 119)

**Interfaces:**
- Consumes: `IAuthorityImportService` (Task 1).
- Produces: `IAuthorityImportController { IList<string> GetSheetNames(string filePath, UserContext user); AuthorityImportPreview BuildPreview(string filePath, string sheetName, bool updateUserLevel, UserContext user); AuthorityImportResult Commit(AuthorityImportPreview preview, UserContext user); }`, class `AuthorityImportController(IAuthorityImportService service)`.

- [ ] **Step 1: Interface**

```csharp
using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IAuthorityImportController
    {
        IList<string> GetSheetNames(string filePath, UserContext user);
        AuthorityImportPreview BuildPreview(string filePath, string sheetName, bool updateUserLevel, UserContext user);
        AuthorityImportResult Commit(AuthorityImportPreview preview, UserContext user);
    }
}
```

- [ ] **Step 2: Implementation**

```csharp
using System;
using System.Collections.Generic;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class AuthorityImportController : IAuthorityImportController
    {
        private readonly IAuthorityImportService _service;

        public AuthorityImportController(IAuthorityImportService service)
        {
            if (service == null) throw new ArgumentNullException("service");
            _service = service;
        }

        public IList<string> GetSheetNames(string filePath, UserContext user)
        {
            return _service.GetSheetNames(filePath, user);
        }

        public AuthorityImportPreview BuildPreview(string filePath, string sheetName, bool updateUserLevel, UserContext user)
        {
            return _service.BuildPreview(filePath, sheetName, updateUserLevel, user);
        }

        public AuthorityImportResult Commit(AuthorityImportPreview preview, UserContext user)
        {
            return _service.Commit(preview, user);
        }
    }
}
```

- [ ] **Step 3: Register both files** in `CMS.Controllers.csproj`:

```xml
    <Compile Include="DMS\Document\Interfaces\IAuthorityImportController.cs" />
    <Compile Include="DMS\Document\AuthorityImportController.cs" />
```

- [ ] **Step 4: Build** (expect success) and **commit** — propose "feat(authority-import): add import controller".

---

### Task 5: `AuthorityImportForm` (Designer + code-behind)

**Files:**
- Create: `CMS.Presentation/Modules/DMS/AuthorityImportForm.Designer.cs`
- Create: `CMS.Presentation/Modules/DMS/AuthorityImportForm.cs`
- Modify: `CMS.Presentation/CMS.Presentation.csproj` (add both `<Compile>` entries; `.cs` first, Designer with `<DependentUpon>AuthorityImportForm.cs</DependentUpon>`, mirroring lines 224-229)

**Interfaces:**
- Consumes: `IAuthorityImportController` (Task 4), `UserContext`, DTOs.
- Produces: `public partial class AuthorityImportForm : Form` with ctor `(IAuthorityImportController controller, UserContext user)` and `public bool ImportCompleted { get; }`.

- [ ] **Step 1: Designer file** (all controls, fields, layout; grid columns are created by a helper in this file)

```csharp
namespace CMS.Presentation
{
    partial class AuthorityImportForm
    {
        private System.ComponentModel.IContainer components = null;

        protected override void Dispose(bool disposing)
        {
            if (disposing && (components != null)) components.Dispose();
            base.Dispose(disposing);
        }

        private static void AddColumns(System.Windows.Forms.DataGridView grid, params string[] headers)
        {
            for (int i = 0; i < headers.Length; i++)
                grid.Columns.Add(new System.Windows.Forms.DataGridViewTextBoxColumn
                {
                    Name = grid.Name + "Col" + i,
                    HeaderText = headers[i],
                    AutoSizeMode = System.Windows.Forms.DataGridViewAutoSizeColumnMode.AllCells
                });
        }

        private void InitializeComponent()
        {
            this.pnlTop = new System.Windows.Forms.Panel();
            this.lblFile = new System.Windows.Forms.Label();
            this.txtFile = new System.Windows.Forms.TextBox();
            this.btnBrowse = new System.Windows.Forms.Button();
            this.lblSheet = new System.Windows.Forms.Label();
            this.cboSheet = new System.Windows.Forms.ComboBox();
            this.chkUpdateLevel = new System.Windows.Forms.CheckBox();
            this.btnPreview = new System.Windows.Forms.Button();
            this.tabs = new System.Windows.Forms.TabControl();
            this.tabPlans = new System.Windows.Forms.TabPage();
            this.tabLevels = new System.Windows.Forms.TabPage();
            this.tabIssues = new System.Windows.Forms.TabPage();
            this.dgvPlans = new System.Windows.Forms.DataGridView();
            this.dgvLevels = new System.Windows.Forms.DataGridView();
            this.dgvIssues = new System.Windows.Forms.DataGridView();
            this.pnlBottom = new System.Windows.Forms.Panel();
            this.lblSummary = new System.Windows.Forms.Label();
            this.btnCommit = new System.Windows.Forms.Button();
            this.btnClose = new System.Windows.Forms.Button();

            // pnlTop
            this.pnlTop.Dock = System.Windows.Forms.DockStyle.Top;
            this.pnlTop.Height = 78;
            this.pnlTop.Controls.Add(this.lblFile);
            this.pnlTop.Controls.Add(this.txtFile);
            this.pnlTop.Controls.Add(this.btnBrowse);
            this.pnlTop.Controls.Add(this.lblSheet);
            this.pnlTop.Controls.Add(this.cboSheet);
            this.pnlTop.Controls.Add(this.chkUpdateLevel);
            this.pnlTop.Controls.Add(this.btnPreview);

            this.lblFile.Text = "ไฟล์:";
            this.lblFile.Location = new System.Drawing.Point(12, 14);
            this.lblFile.AutoSize = true;

            this.txtFile.Location = new System.Drawing.Point(50, 10);
            this.txtFile.Size = new System.Drawing.Size(560, 23);
            this.txtFile.ReadOnly = true;
            this.txtFile.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Left | System.Windows.Forms.AnchorStyles.Right;

            this.btnBrowse.Text = "เลือกไฟล์…";
            this.btnBrowse.Location = new System.Drawing.Point(620, 9);
            this.btnBrowse.Size = new System.Drawing.Size(100, 26);
            this.btnBrowse.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnBrowse.Click += new System.EventHandler(this.btnBrowse_Click);

            this.lblSheet.Text = "Sheet:";
            this.lblSheet.Location = new System.Drawing.Point(12, 46);
            this.lblSheet.AutoSize = true;

            this.cboSheet.Location = new System.Drawing.Point(60, 42);
            this.cboSheet.Size = new System.Drawing.Size(200, 23);
            this.cboSheet.DropDownStyle = System.Windows.Forms.ComboBoxStyle.DropDownList;
            this.cboSheet.SelectedIndexChanged += new System.EventHandler(this.InputChanged);

            this.chkUpdateLevel.Text = "อัปเดต Level ของผู้ใช้ (LevelCode)";
            this.chkUpdateLevel.Location = new System.Drawing.Point(280, 44);
            this.chkUpdateLevel.AutoSize = true;
            this.chkUpdateLevel.CheckedChanged += new System.EventHandler(this.InputChanged);

            this.btnPreview.Text = "ตรวจสอบ (Preview)";
            this.btnPreview.Location = new System.Drawing.Point(560, 40);
            this.btnPreview.Size = new System.Drawing.Size(160, 28);
            this.btnPreview.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnPreview.Enabled = false;
            this.btnPreview.Click += new System.EventHandler(this.btnPreview_Click);

            // tabs
            this.tabs.Dock = System.Windows.Forms.DockStyle.Fill;
            this.tabs.Controls.Add(this.tabPlans);
            this.tabs.Controls.Add(this.tabLevels);
            this.tabs.Controls.Add(this.tabIssues);

            this.tabPlans.Text = "สิทธิ์ตามตำแหน่ง";
            this.tabPlans.Controls.Add(this.dgvPlans);
            this.tabLevels.Text = "Level ผู้ใช้";
            this.tabLevels.Controls.Add(this.dgvLevels);
            this.tabIssues.Text = "ข้อผิดพลาด/คำเตือน";
            this.tabIssues.Controls.Add(this.dgvIssues);

            ConfigureGrid(this.dgvPlans, "dgvPlans");
            AddColumns(this.dgvPlans, "สถานะ", "ตำแหน่ง", "แผนก", "ฝ่าย", "1.HC", "2.SC", "3.NC", "4.PC");
            ConfigureGrid(this.dgvLevels, "dgvLevels");
            AddColumns(this.dgvLevels, "สถานะ", "รหัสพนักงาน", "Level เดิม", "Level ใหม่");
            ConfigureGrid(this.dgvIssues, "dgvIssues");
            AddColumns(this.dgvIssues, "ระดับ", "แถว", "รหัส", "รายละเอียด");

            // pnlBottom
            this.pnlBottom.Dock = System.Windows.Forms.DockStyle.Bottom;
            this.pnlBottom.Height = 48;
            this.pnlBottom.Controls.Add(this.lblSummary);
            this.pnlBottom.Controls.Add(this.btnCommit);
            this.pnlBottom.Controls.Add(this.btnClose);

            this.lblSummary.Location = new System.Drawing.Point(12, 16);
            this.lblSummary.AutoSize = true;
            this.lblSummary.Text = "เลือกไฟล์ Excel แล้วกด ตรวจสอบ";

            this.btnCommit.Text = "นำเข้า";
            this.btnCommit.Size = new System.Drawing.Size(110, 30);
            this.btnCommit.Location = new System.Drawing.Point(500, 9);
            this.btnCommit.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnCommit.Enabled = false;
            this.btnCommit.Click += new System.EventHandler(this.btnCommit_Click);

            this.btnClose.Text = "ปิด";
            this.btnClose.Size = new System.Drawing.Size(100, 30);
            this.btnClose.Location = new System.Drawing.Point(620, 9);
            this.btnClose.Anchor = System.Windows.Forms.AnchorStyles.Top | System.Windows.Forms.AnchorStyles.Right;
            this.btnClose.Click += new System.EventHandler(this.btnClose_Click);

            // form
            this.AutoScaleDimensions = new System.Drawing.SizeF(7F, 15F);
            this.AutoScaleMode = System.Windows.Forms.AutoScaleMode.Font;
            this.ClientSize = new System.Drawing.Size(740, 520);
            this.Controls.Add(this.tabs);
            this.Controls.Add(this.pnlTop);
            this.Controls.Add(this.pnlBottom);
            this.MinimumSize = new System.Drawing.Size(700, 420);
            this.Name = "AuthorityImportForm";
            this.StartPosition = System.Windows.Forms.FormStartPosition.CenterParent;
            this.Text = "นำเข้าสิทธิ์จาก Excel";
        }

        private static void ConfigureGrid(System.Windows.Forms.DataGridView g, string name)
        {
            g.Name = name;
            g.Dock = System.Windows.Forms.DockStyle.Fill;
            g.ReadOnly = true;
            g.AllowUserToAddRows = false;
            g.AllowUserToDeleteRows = false;
            g.RowHeadersVisible = false;
            g.SelectionMode = System.Windows.Forms.DataGridViewSelectionMode.FullRowSelect;
            g.BackgroundColor = System.Drawing.Color.White;
        }

        private System.Windows.Forms.Panel pnlTop;
        private System.Windows.Forms.Label lblFile;
        private System.Windows.Forms.TextBox txtFile;
        private System.Windows.Forms.Button btnBrowse;
        private System.Windows.Forms.Label lblSheet;
        private System.Windows.Forms.ComboBox cboSheet;
        private System.Windows.Forms.CheckBox chkUpdateLevel;
        private System.Windows.Forms.Button btnPreview;
        private System.Windows.Forms.TabControl tabs;
        private System.Windows.Forms.TabPage tabPlans;
        private System.Windows.Forms.TabPage tabLevels;
        private System.Windows.Forms.TabPage tabIssues;
        private System.Windows.Forms.DataGridView dgvPlans;
        private System.Windows.Forms.DataGridView dgvLevels;
        private System.Windows.Forms.DataGridView dgvIssues;
        private System.Windows.Forms.Panel pnlBottom;
        private System.Windows.Forms.Label lblSummary;
        private System.Windows.Forms.Button btnCommit;
        private System.Windows.Forms.Button btnClose;
    }
}
```

- [ ] **Step 2: Code-behind** (logic and handlers only)

```csharp
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Presentation
{
    public partial class AuthorityImportForm : Form
    {
        private static readonly string[] ActionText = { "ดู", "เพิ่ม", "แก้ไข", "อนุมัติ" };

        private readonly IAuthorityImportController _controller;
        private readonly UserContext _user;
        private AuthorityImportPreview _preview;

        public bool ImportCompleted { get; private set; }

        public AuthorityImportForm()
        {
            InitializeComponent();
        }

        public AuthorityImportForm(IAuthorityImportController controller, UserContext user)
        {
            if (controller == null) throw new ArgumentNullException("controller");
            if (user == null) throw new ArgumentNullException("user");
            _controller = controller;
            _user = user;
            InitializeComponent();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog { Filter = "Excel (*.xlsx)|*.xlsx", Title = "เลือกไฟล์สิทธิ์" })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    var sheets = _controller.GetSheetNames(dlg.FileName, _user);
                    txtFile.Text = dlg.FileName;
                    cboSheet.Items.Clear();
                    foreach (var s in sheets) cboSheet.Items.Add(s);
                    int idx = sheets.IndexOf("ชื่อพนักงาน");
                    cboSheet.SelectedIndex = idx >= 0 ? idx : (cboSheet.Items.Count > 0 ? 0 : -1);
                    btnPreview.Enabled = cboSheet.SelectedIndex >= 0;
                    ClearPreview();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "ผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void InputChanged(object sender, EventArgs e)
        {
            ClearPreview();
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                _preview = _controller.BuildPreview(txtFile.Text, cboSheet.Text, chkUpdateLevel.Checked, _user);
                BindPreview(_preview);
            }
            catch (Exception ex)
            {
                ClearPreview();
                MessageBox.Show(this, ex.Message, "ผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnCommit_Click(object sender, EventArgs e)
        {
            if (_preview == null || _preview.HasErrors || !_preview.HasChanges) return;
            string msg = string.Format("ยืนยันการนำเข้า?\n\nตำแหน่งใหม่ {0} · อัปเดต {1}{2}",
                _preview.NewCount, _preview.UpdatedCount,
                _preview.UpdateUserLevel ? string.Format(" · เปลี่ยน Level ผู้ใช้ {0} คน", _preview.LevelChangeCount) : "");
            if (MessageBox.Show(this, msg, "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                var result = _controller.Commit(_preview, _user);
                ImportCompleted = true;
                MessageBox.Show(this,
                    string.Format("นำเข้าสำเร็จ\nสร้างใหม่ {0} · อัปเดต {1} · เปลี่ยน Level {2}",
                        result.ConfigsCreated, result.ConfigsUpdated, result.LevelsChanged),
                    "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "ผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ClearPreview()
        {
            _preview = null;
            dgvPlans.Rows.Clear();
            dgvLevels.Rows.Clear();
            dgvIssues.Rows.Clear();
            btnCommit.Enabled = false;
            lblSummary.Text = string.IsNullOrEmpty(txtFile.Text) ? "เลือกไฟล์ Excel แล้วกด ตรวจสอบ" : "กด ตรวจสอบ เพื่อดูตัวอย่างก่อนนำเข้า";
        }

        private void BindPreview(AuthorityImportPreview p)
        {
            dgvPlans.Rows.Clear();
            dgvLevels.Rows.Clear();
            dgvIssues.Rows.Clear();

            foreach (var plan in p.PositionPlans)
                dgvPlans.Rows.Add(StatusText(plan.Status), plan.PositionName,
                    plan.DepartmentText, plan.DivisionText,
                    PermText(plan.Permissions, 0), PermText(plan.Permissions, 1),
                    PermText(plan.Permissions, 2), PermText(plan.Permissions, 3));

            foreach (var lp in p.LevelPlans)
                dgvLevels.Rows.Add(StatusText(lp.Status), lp.EmployeeCode, lp.OldLevel, lp.NewLevel);

            foreach (var issue in p.Issues.OrderByDescending(i => i.Severity).ThenBy(i => i.RowNumber))
            {
                int idx = dgvIssues.Rows.Add(issue.Severity == AuthorityImportSeverity.Error ? "Error" : "Warning",
                    issue.RowNumber == 0 ? "" : issue.RowNumber.ToString(), issue.Code, issue.Message);
                if (issue.Severity == AuthorityImportSeverity.Error)
                    dgvIssues.Rows[idx].DefaultCellStyle.ForeColor = Color.Firebrick;
            }

            int errors = p.Issues.Count(i => i.Severity == AuthorityImportSeverity.Error);
            int warnings = p.Issues.Count - errors;
            lblSummary.Text = string.Format("ใหม่ {0} · อัปเดต {1} · ไม่เปลี่ยน {2}{3} · Error {4} · Warning {5}",
                p.NewCount, p.UpdatedCount, p.UnchangedCount,
                p.UpdateUserLevel ? string.Format(" · เปลี่ยน Level {0}", p.LevelChangeCount) : "",
                errors, warnings);
            btnCommit.Enabled = !p.HasErrors && p.HasChanges;
            if (errors > 0) tabs.SelectedTab = tabIssues;
        }

        private static string PermText(bool[] perms, int levelIndex)
        {
            var on = new List<string>();
            for (int a = 0; a < 4; a++)
                if (perms[levelIndex * 4 + a]) on.Add(ActionText[a]);
            return on.Count == 0 ? "-" : string.Join(", ", on.ToArray());
        }

        private static string StatusText(AuthorityImportStatus s)
        {
            switch (s)
            {
                case AuthorityImportStatus.New: return "ใหม่";
                case AuthorityImportStatus.Updated: return "อัปเดต";
                case AuthorityImportStatus.Unchanged: return "ไม่เปลี่ยน";
                default: return "ไม่พบผู้ใช้";
            }
        }
    }
}
```

- [ ] **Step 3: Register in `CMS.Presentation.csproj`**:

```xml
    <Compile Include="Modules\DMS\AuthorityImportForm.cs">
      <SubType>Form</SubType>
    </Compile>
    <Compile Include="Modules\DMS\AuthorityImportForm.Designer.cs">
      <DependentUpon>AuthorityImportForm.cs</DependentUpon>
    </Compile>
```

(Match the exact `SubType` pattern used by the neighbouring `AuthorityConfigForm.cs` entry at line 224.)

- [ ] **Step 4: Build.** Expected: success. Check the form-structure rules: `AuthorityImportForm.cs` contains no `new Control`-type construction (`OpenFileDialog` is a component, not a control), no `Controls.Add`.

- [ ] **Step 5: Commit** — propose "feat(authority-import): add import form".

---

### Task 6: Wire-up, button, and manual verification

**Files:**
- Modify: `CMS.Presentation/Modules/DMS/AuthorityConfigForm.cs` (ctor ~line 29, add handler near `btnClose_Click` ~line 605)
- Modify: `CMS.Presentation/Modules/DMS/AuthorityConfigForm.Designer.cs` (field list ~line 600, init ~line 19, toolbar ~line 99, new button block before `btnClose`)
- Modify: `CMS.Presentation/MDI_Container.cs` (field near line 74, ctor param after line 213, assignment near line 295, creation at line 2291)
- Modify: `CMS.Presentation/Program.cs` (create service/controller near line 496; pass at line ~640)

**Interfaces:**
- Consumes: `AuthorityImportService`, `AuthorityImportController`, `AuthorityImportForm`.
- Produces: button `btnImport` on `AuthorityConfigForm`; optional ctor parameter `IAuthorityImportController authorityImportController = null` on `AuthorityConfigForm` and `MDI_Container`.

- [ ] **Step 1: `AuthorityConfigForm.Designer.cs`.** Declare the field with the others, create it with the other buttons (line ~22), add `this.pnlToolbar.Controls.Add(this.btnImport);`, then add the block (placed at X=378; **move `btnClose` to X=502**):

```csharp
            // 
            // btnImport
            // 
            this.btnImport.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(0)))), ((int)(((byte)(105)))), ((int)(((byte)(148)))));
            this.btnImport.FlatAppearance.BorderSize = 0;
            this.btnImport.FlatStyle = System.Windows.Forms.FlatStyle.Flat;
            this.btnImport.Font = new System.Drawing.Font("Segoe UI", 8.5F);
            this.btnImport.ForeColor = System.Drawing.Color.White;
            this.btnImport.Location = new System.Drawing.Point(378, 6);
            this.btnImport.Name = "btnImport";
            this.btnImport.Size = new System.Drawing.Size(118, 32);
            this.btnImport.TabIndex = 3;
            this.btnImport.Text = "📥 นำเข้าจาก Excel";
            this.btnImport.UseVisualStyleBackColor = false;
            this.btnImport.Click += new System.EventHandler(this.btnImport_Click);
```

and change `btnClose.Location` to `new System.Drawing.Point(502, 6)` and `btnClose.TabIndex` to 4. Field: `private System.Windows.Forms.Button        btnImport;`.

- [ ] **Step 2: `AuthorityConfigForm.cs`.** Add field and ctor parameter, plus handler:

```csharp
        private readonly IAuthorityImportController _authorityImportController;
```

Change the ctor signature to
`..., IAuthorityOrgLookupController authorityOrgLookupController = null, IAuthorityImportController authorityImportController = null)` and set `_authorityImportController = authorityImportController;`. After `InitializeComponent();` in the ctor add `btnImport.Visible = _authorityImportController != null && _user.AppRole == 4;`. Handler:

```csharp
        private void btnImport_Click(object sender, EventArgs e)
        {
            if (_authorityImportController == null) return;
            using (var dlg = new AuthorityImportForm(_authorityImportController, _user))
            {
                dlg.ShowDialog(this);
                if (dlg.ImportCompleted) LoadData();
            }
        }
```

Add `using CMS.Controllers.DMS.Document.Interfaces;` is already present (it hosts `IAuthorityConfigController`).

- [ ] **Step 3: `MDI_Container.cs`.** Add `private readonly IAuthorityImportController _authorityImportController;` beside line 74; add ctor parameter `IAuthorityImportController authorityImportController = null,` after `IAuthorityConfigController authorityConfigController = null,` (line 213); assign `_authorityImportController = authorityImportController;` after line 295; in the `new AuthorityConfigForm(...)` call (line 2291) add `authorityImportController: _authorityImportController`. Because the new parameter has a default and is placed mid-list, **verify every call site passes arguments by name** (`Grep "new MDI_Container("`); if any passes positionally, move the new parameter to the end of the list instead.

- [ ] **Step 4: `Program.cs`.** After line 496 add:

```csharp
                var authorityImportService = new AuthorityImportService(
                    folderAuthorityService, userAccountRepo, hrDepartmentRepository, hrDivisionRepository, auditService);
                var authorityImportController = new AuthorityImportController(authorityImportService);
```

and pass `authorityImportController: authorityImportController,` next to `authorityConfigController: authorityConfigController,` (line ~640). Add `using` directives only if the build asks for them.

- [ ] **Step 5: Build.** Expected: success, 0 errors.

- [ ] **Step 6: Manual verification** (the replacement for automated tests). Run the app as an Admin against the dev DB, open the authority config screen, click **นำเข้าจาก Excel**, choose `Docdmc/สิทธิ์ รับประกัน 1 ส่ง IT_test.xlsx`, then check:
  1. Sheet defaults to `ชื่อพนักงาน`; **ตรวจสอบ** shows 9 rows in *สิทธิ์ตามตำแหน่ง* (all `ใหม่` on a clean DB) with `Error 0`; Warning includes `REMARK` for the row of employee `01111`; commit button enabled.
  2. Row for `01111`: HC `ดู`, SC `ดู`, NC `ดู, เพิ่ม, อนุมัติ`, PC `ดู, เพิ่ม`.
  3. Tick *อัปเดต Level* → preview clears; press **ตรวจสอบ** → *Level ผู้ใช้* tab lists users as `อัปเดต` / `ไม่เปลี่ยน` / `ไม่พบผู้ใช้` as expected for the dev data.
  4. **นำเข้า** → confirm → success dialog; the `AuthorityConfigForm` grid reloads with the new positions; an audit row `AuthorityImport` exists (query `EMS.AuditLog`, no names in `NewValue`).
  5. Re-run the same import: every plan shows `ไม่เปลี่ยน`, commit is disabled (idempotent).
  6. Log in (or simulate) as a non-admin: the button is hidden; calling the service directly throws `UnauthorizedAccessException`.
  7. Open a non-template `.xlsx`: preview shows `TEMPLATE_MISMATCH`, commit disabled. Open the sample while it is open in Excel: preview still works (file shared read).
  8. Compensation check (optional, dev DB only): temporarily make `_authority.Save` throw on the 3rd call, run an import, confirm the DB is back to its prior state and the error mentions the rollback; revert the temporary change.

- [ ] **Step 7: Commit** — propose "feat(authority-import): wire import form into authority config screen", then ask the user whether to push; do not push without approval.

---

## Self-Review

- **Spec coverage:** form + preview/commit gating (Task 5/6); template parse, skip summary row, `X` handling, template check (Task 2); all validation rows of spec §5.3 — missing position, duplicate code, conflict, level pattern, approve-without-view, remark, user-not-found, org-unresolved (Task 3) — plus `LABEL_COLLISION` from Review Focus; mapping 1 position → 4 configs, label = position (Task 3); opt-in Level update with old→new and `UpdateLevelCode` (Task 1/3/5); admin-only in service + button hidden (Task 3/6); audit with employee code only (Task 3); idempotent re-import (Unchanged status, Task 3/6 step 5); out-of-scope items untouched. Transaction requirement is replaced by compensation (documented deviation 3). Spec §6 SP replaced by inline SQL (deviation 2). Spec §7 tests replaced by Task 2/6 verification (deviation 1).
- **Placeholders:** none; every code step is complete.
- **Type consistency:** `AuthorityImportPreview/Result/Status/Severity`, `GetSheetNames/BuildPreview/Commit` signatures are identical in the DTOs, service, controller and form. `AuthorityImportStatus.NotFound` is only used for level plans.
- **Spec open items resolved:** transaction → compensation; `Save` upsert semantics → mirror the existing `Delete(label)` + insert pattern from `AuthorityConfigForm.SaveConfig`; Dept/Division → name→code lookup via `IHrDepartmentRepository/IHrDivisionRepository` with a warning when unresolved; `LevelCode` source of truth → import writes it, login sync does not touch it (unchanged).
- **Known risks to watch during execution:** `sp_ResolveFolderAuthority` is not in the repo, so how it combines `PositionName` with `DepartmentCode`/`DivisionCode` is unverified — check on the dev DB in Task 6 step 4 by logging in as an imported user and confirming `ResolveForUser` returns the 4 levels; `FolderAuthorityRepository.GetAll` may exclude soft-deleted rows (assumed).
