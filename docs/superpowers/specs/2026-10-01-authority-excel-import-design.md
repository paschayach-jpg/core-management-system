# Authority Excel Import — Design

Date: 2026-10-01
Status: Draft, pending user review

## 1. Purpose

Admins configure folder access authority (`FolderAuthorityConfig`) today one entry at a time in `AuthorityConfigForm`. HR/Underwriting provides authority as an Excel matrix (template: `Docdmc/สิทธิ์ รับประกัน 1 ส่ง IT_test.xlsx`). This feature adds an **import form** that reads that template, shows a preview, and on confirmation writes position-based authority configs and (optionally) updates user `LevelCode`.

### Decisions already made (user-confirmed)
- **Model A**: the import produces **position-based** `FolderAuthorityConfig` rows (existing match key: Position > Department > Division). No per-employee authority table.
- **Level update is opt-in**: a checkbox "อัปเดต Level ของผู้ใช้" controls whether `UserAccount.LevelCode` is updated.

### Assumptions (not yet explicitly confirmed)
- All files use this same template; one file per department.
- `X`/`x` = true, empty = false.
- Preview before any DB write; every import is audited.

## 2. Template analysis

Sheet `ชื่อพนักงาน`, range A1:Y(n). Header spans rows 1–2; data starts row 3.

| Col | Content |
|---|---|
| A | No |
| B | Employee code (รหัสพนักงาน) |
| C | Name TH |
| D | Name EN |
| E | Level (e.g. `4A`, `3B`) |
| F | Position (ตำแหน่ง) |
| G | Department (แผนก) |
| H | Division (ฝ่าย) |
| I–L | HC (1, สูงสุด): ดู / เพิ่ม / แก้ไข / อนุมัติ |
| M–P | SC (2, ลับมาก): same four |
| Q–T | NC (3, ปกติ): same four |
| U–X | PC (4, ทั่วไป): same four |
| Y | Remark (e.g. "ย้ายไปการตลาด") |

Row 2 also carries a department summary row (no employee code, e.g. "รับประกัน 1 / จำนวน 9 คน") which must be skipped. Header groups are merged cells (I1:L1, M1:P1, Q1:T1, U1:X1).

## 3. Existing system touchpoints

- `FolderAuthorityConfig`: one row per (label × ConfidentialityLevel 1–4) with `CanView/CanAdd/CanEdit/CanApprove`; match keys `PositionName`, `DepartmentCode`, `DivisionCode`.
- Save path: `AuthorityConfigController.Save` → `FolderAuthorityService.Save` → `FolderAuthorityRepository.Insert/Update` (`EMS.sp_Insert/UpdateFolderAuthorityConfig`). `ConfigLabel` is required.
- `UserAccount` has `EmployeeCode`, `PositionName`, `DepartmentCode`, `DivisionCode`, `LevelCode`. Login-time sync (`Program.cs` `SyncEmployeeInfoFromAuthResult` / `SyncEmployeeInfoFromApi`) updates everything **except** `LevelCode`; `PositionName` is overwritten from the Employee API.
- NPOI 2.5.6 is already referenced by `CMS.Services` (`KBExcelReaderService`) — reuse it; no new dependency.
- Menu wiring pattern: `MenuActionRegistry` / `MDI_Container`. The new form opens from a button on `AuthorityConfigForm`, so no new menu row is required.

## 4. Architecture

```
AuthorityImportForm (Presentation, .cs/.Designer.cs/.resx)
        │  (via controller, same pattern as AuthorityConfigController)
AuthorityImportController (CMS.Controllers/DMS/Document)
        │
IAuthorityImportService / AuthorityImportService (CMS.Services)
        ├─ Parse   : NPOI → List<AuthorityImportRow>
        ├─ Validate: → issues (Error/Warning), conflict detection
        ├─ Preview : diff vs existing configs / users
        └─ Commit  : IFolderAuthorityService.Save, IUserAccountRepository.UpdateLevelCode, IAuditService
```

Wiring added to `Program.cs` (composition root) and the `.csproj` files. New service files are added to `CMS.Tests.csproj`.

### DTOs
- `ParseAuthorityImportRequest { FilePath, SheetName }`
- `AuthorityImportRow { RowNumber, EmployeeCode, Level, Position, Department, Division, Remark, bool[4][4] Permissions }`
- `AuthorityImportIssue { RowNumber, Severity (Error|Warning), Code, Message }`
- `AuthorityImportResult { Rows, PositionPlans, LevelPlans, Issues, Counts }`
- `CommitAuthorityImportRequest { ParseResult, UpdateUserLevel }`

## 5. Behaviour

### 5.1 Form
Fields: file path + browse, sheet dropdown, checkbox "อัปเดต Level ของผู้ใช้", Preview button. Tabs: *สิทธิ์ตามตำแหน่ง*, *Level ผู้ใช้* (visible/enabled only when the checkbox is ticked), *ข้อผิดพลาด/คำเตือน*. Summary line: new / updated / unchanged / errors. **Commit button is enabled only when a preview exists and there are zero Errors.** Changing file, sheet, or checkbox invalidates the preview.

### 5.2 Parse
1. Verify template: header row labels and the four merged groups at the expected columns; otherwise Error `TEMPLATE_MISMATCH`.
2. Read rows from 3; skip empty rows and summary rows (no employee code).
3. Cell → bool: `X`/`x` true; empty false; anything else → Warning `UNEXPECTED_CELL`, treated as false.

### 5.3 Validate

| Condition | Severity |
|---|---|
| Missing position | Error |
| Duplicate employee code in file | Error |
| Same position, different permission sets | Error `POSITION_CONFLICT` (user fixes the file) |
| Level not matching `^[1-9][A-Z]$` | Error |
| Approve without View in same level | Warning |
| Remark present (e.g. transferred) | Warning |
| Employee code not found in `UserAccount` | Warning (Level tab only) |
| Department/Division name not resolvable to a code | Warning; that match key is left null |

### 5.4 Mapping to configs
Per distinct position: 4 `FolderAuthorityConfig` (levels 1–4). `ConfigLabel = PositionName` so re-import upserts. Department/Division codes are resolved from the HR master by name (best effort, see Warning above).

### 5.5 Level update (opt-in)
Match `EmployeeCode` ↔ `UserAccount.EmployeeCode`. Preview shows `old → new`, `unchanged`, or `not found`. Commit calls new `IUserAccountRepository.UpdateLevelCode(userId, levelCode)` backed by a new SP. `PositionName` is **not** imported into `UserAccount` (Employee API owns it). Users without an account (never logged in) are skipped and reported; accounts are not pre-created.

### 5.6 Commit
- Admin only (AppRole 4), enforced in the service, not just the UI.
- Single transaction across all writes; any failure rolls back everything and reports the cause. (Whether existing repositories can share a transaction is to be verified during planning; if not, add a transactional overload.)
- Audit via `IAuditService`: file name, counts of created/updated configs, each Level change (old/new, keyed by employee code). **No names are written to the audit log** (PDPA).
- Idempotent: re-importing the same file yields no changes.

## 6. Database
New numbered script in `CMS.Data/Scripts/` (next free number, chosen at plan time) adding the Level-update SP; applied manually per project convention. No table changes.

## 7. Testing (NUnit + NSubstitute + FsCheck)
- Parse: `X` handling, summary-row skipping, merged-header detection, template mismatch.
- Validate: each row in the table of 5.3, especially `POSITION_CONFLICT`.
- Mapping: one position → exactly 4 configs with correct flags.
- Level plan: change / unchanged / not found; checkbox off → no Level writes.
- Authorization: non-admin rejected.
- Property: import is idempotent; parse∘serialize round-trips permission matrices.
- Commit failure rolls back (mocked repo throws mid-way).

## 8. Out of scope (YAGNI)
Per-employee authority, custom column mapping, legacy `.xls`, creating `UserAccount` rows, syncing `PositionName`, a menu row for the import form.

## 9. Open items for planning
- Transaction support across `FolderAuthorityRepository` and `UserAccountRepository`.
- Whether `FolderAuthorityService.Save` upserts by `ConfigLabel`+level or only inserts.
- How to resolve Department/Division names to codes (HR master repositories exist: `HrDepartmentRepository`, `HrDivisionRepository`).
- Source of truth for `LevelCode` if Employee API sync is later extended.
