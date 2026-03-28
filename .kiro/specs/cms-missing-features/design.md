# Design Document — cms-missing-features

## Overview

Feature นี้เพิ่มและแก้ไขฟีเจอร์ที่ขาดอยู่ใน CMS WinForms C# .NET โดยแบ่งเป็น 3 กลุ่ม:

1. **Validation & UI พื้นฐาน** — Folder name validation (Req 1), duplicate check (Req 2), tree search (Req 3), column sort (Req 4), delete protection (Req 5)
2. **Filter & Quick Access** — File list filter (Req 6), Quick Access panel (Req 7)
3. **ฟีเจอร์ขั้นสูง** — Copy folder structure (Req 8), File preview (Req 9), Resume upload (Req 10)

ทุก feature ยึดสถาปัตยกรรม MVP + Repository pattern ที่มีอยู่เดิม ไม่เปลี่ยน layer boundary

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  CMS.Presentation (WinForms)                                │
│  FolderForm  FileUploadForm  FilePreviewPanel               │
│       │              │                                      │
│  FolderPresenter  FilePresenter                             │
└──────────────┬──────────────────────────────────────────────┘
               │ IFolderService / IFileService
┌──────────────▼──────────────────────────────────────────────┐
│  CMS.Services                                               │
│  FolderService (+CopyFolderStructure)                       │
│  QuickAccessService (new)                                   │
│  UploadCheckpointService (new)                              │
└──────────────┬──────────────────────────────────────────────┘
               │ IFolderRepository / IQuickAccessRepository
┌──────────────▼──────────────────────────────────────────────┐
│  CMS.Data                                                   │
│  FolderRepository (+GetSiblings)                            │
│  QuickAccessRepository (new)                                │
│  SQL: 019_QuickAccess_Table.sql                             │
└─────────────────────────────────────────────────────────────┘
```

### Design Decisions

- **Folder name validation** อยู่ใน `FolderService` (service layer) ไม่ใช่ UI เพื่อให้ testable และ reusable
- **Duplicate check** ใช้ `IFolderRepository.GetSiblings()` method ใหม่แทนการ load ทั้ง tree เพื่อ performance
- **Tree search** ทำ client-side filter บน `TreeNode` ที่โหลดไว้แล้ว ไม่ query DB ใหม่
- **Column sort** ใช้ `ListViewItemSorter` pattern มาตรฐานของ WinForms
- **Quick Access** เก็บใน DB (ไม่ใช่ local file) เพื่อ sync ข้าม machine
- **Upload checkpoint** เก็บใน `IsolatedStorage` เพราะเป็น per-user per-machine state ไม่ต้องการ DB
- **File preview** ใช้ `PictureBox` สำหรับ image, `WebBrowser` สำหรับ PDF — ไม่ใช้ third-party library เพิ่มเติม

---

## Components and Interfaces

### 1. FolderService — เพิ่ม validation และ method ใหม่

```csharp
// เพิ่มใน FolderService.cs
private static readonly char[] InvalidFolderNameChars = { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };

private static void ValidateFolderName(string name)
{
    var found = name.Where(c => InvalidFolderNameChars.Contains(c)).Distinct().ToArray();
    if (found.Length > 0)
        throw new ArgumentException(
            string.Format("ชื่อ Folder ไม่อนุญาตให้ใช้อักขระ: {0}", string.Join(" ", found)));
}

// เพิ่มใน IFolderService
void CopyFolderStructure(int sourceFolderId, int destinationParentFolderId, UserContext user);
```

### 2. IFolderRepository — เพิ่ม method

```csharp
// เพิ่มใน IFolderRepository
IEnumerable<Folder> GetSiblings(int? parentFolderId, int orgUnitId);
```

### 3. QuickAccessService (ใหม่)

```csharp
public interface IQuickAccessService
{
    IEnumerable<QuickAccessFolder> GetByUser(int userId);
    void Add(int userId, int folderId);
    void Remove(int userId, int folderId);
}
```

### 4. UploadCheckpointService (ใหม่)

```csharp
public interface IUploadCheckpointService
{
    void Save(UploadCheckpoint checkpoint);
    UploadCheckpoint Load(int folderId, string fileName);
    void Delete(int folderId, string fileName);
    void DeleteExpired(TimeSpan maxAge);
}
// Implementation ใช้ IsolatedStorageFile
```

### 5. FilePreviewPanel (ใหม่ใน CMS.Presentation)

```csharp
public class FilePreviewPanel : Panel
{
    public void ShowFile(FileDocument file, string physicalPath);
    public void Clear();
    // ภายในใช้ PictureBox (image), WebBrowser (pdf), Label (unsupported/error)
}
```

### 6. FolderForm — wire events ใหม่

| Event | Handler | Requirement |
|---|---|---|
| `txtFolderSearch.TextChanged` | `TxtFolderSearch_TextChanged` | Req 3 |
| `contentsListView.ColumnClick` | `ContentsListView_ColumnClick` | Req 4 |
| folderTreeView context menu "เพิ่มใน Quick Access" | `CtxAddQuickAccess_Click` | Req 7 |
| btnCopyStructure.Click | `BtnCopyStructure_Click` | Req 8 |
| contentsListView.SelectedIndexChanged | แสดง FilePreviewPanel | Req 9 |

---

## Data Models

### QuickAccessFolder (ใหม่)

```csharp
// CMS.Domain/Models/QuickAccessFolder.cs
public class QuickAccessFolder
{
    public int QuickAccessID { get; set; }
    public int UserID        { get; set; }
    public int FolderID      { get; set; }
    public DateTime AddedDate { get; set; }

    // Transient — populated by service
    public Folder Folder { get; set; }
}
```

### UploadCheckpoint (ใหม่)

```csharp
// CMS.Domain/Models/UploadCheckpoint.cs
public class UploadCheckpoint
{
    public string FileName      { get; set; }
    public long FileSizeBytes   { get; set; }
    public int FolderID         { get; set; }
    public long BytesUploaded   { get; set; }
    public DateTime CheckpointDate { get; set; }
}
```

### SQL Migration — 019_QuickAccess_Table.sql

```sql
CREATE TABLE storage.QuickAccessFolder (
    QuickAccessID INT IDENTITY(1,1) PRIMARY KEY,
    UserID        INT NOT NULL,
    FolderID      INT NOT NULL REFERENCES storage.Folder(FolderID) ON DELETE CASCADE,
    AddedDate     DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT UQ_QuickAccess UNIQUE (UserID, FolderID)
);
```

---

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system — essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Invalid characters are always rejected

*For any* folder name string that contains at least one character from `InvalidFolderNameChars`, calling `CreateFolder()` or `RenameFolder()` SHALL throw `ArgumentException`.

**Validates: Requirements 1.1, 1.3**

---

### Property 2: Valid names are always accepted

*For any* folder name string that contains no character from `InvalidFolderNameChars` and is non-empty after trim, `ValidateFolderName()` SHALL not throw.

**Validates: Requirements 1.2, 1.3**

---

### Property 3: Duplicate sibling names are rejected (case-insensitive)

*For any* parent folder and any two folder names that are equal after `ToLower().Trim()`, attempting to create or rename a folder to the second name SHALL throw `InvalidOperationException`.

**Validates: Requirements 2.1, 2.2, 2.3**

---

### Property 4: Deleted siblings are not counted as duplicates

*For any* parent folder containing a soft-deleted folder with name N, creating a new folder with name N (case-insensitive) SHALL succeed without error.

**Validates: Requirements 2.4, 2.5**

---

### Property 5: Tree search shows only matching nodes (with parents)

*For any* non-empty search string S and folder tree T, the filtered tree SHALL contain exactly the nodes whose `FolderName` contains S (case-insensitive) plus all their ancestor nodes, and no other nodes.

**Validates: Requirements 3.1, 3.3**

---

### Property 6: Empty search restores full tree

*For any* folder tree T, filtering with an empty string SHALL produce a result equal to the original unfiltered tree T.

**Validates: Requirements 3.2**

---

### Property 7: Column sort preserves folders-before-files invariant

*For any* list of mixed Folder and FileDocument items, after sorting by any column in any direction, all Folder items SHALL appear before all FileDocument items.

**Validates: Requirements 4.4**

---

### Property 8: Column sort is a total order

*For any* list of items sorted by column C in direction D, applying the same sort again SHALL produce an identical list (idempotent).

**Validates: Requirements 4.1, 4.2**

---

### Property 9: Non-admin cannot delete non-empty folder

*For any* folder with `FileCount > 0` and any user with `AppRole < 4`, calling `DeleteFolder()` SHALL throw `InvalidOperationException`.

**Validates: Requirements 5.1**

---

### Property 10: Admin can always delete non-empty folder

*For any* folder with `FileCount > 0` and any user with `AppRole = 4`, calling `DeleteFolder()` SHALL succeed (soft delete) without throwing.

**Validates: Requirements 5.2**

---

### Property 11: Empty folder can always be deleted regardless of role

*For any* folder with `FileCount = 0` and any user with any `AppRole`, calling `DeleteFolder()` SHALL succeed.

**Validates: Requirements 5.3**

---

### Property 12: File filter uses AND logic

*For any* set of active filter conditions (extension, date range, uploader), the filtered file list SHALL contain only FileDocument items that satisfy ALL specified conditions simultaneously.

**Validates: Requirements 6.2**

---

### Property 13: Clear filter restores all files

*For any* folder F, after applying any filter and then clearing it, the displayed file list SHALL equal the unfiltered file list for F.

**Validates: Requirements 6.3**

---

### Property 14: Filter does not remove sub-folders

*For any* folder F with sub-folders, applying any file filter SHALL not remove sub-folder items from the display.

**Validates: Requirements 6.5**

---

### Property 15: CopyFolderStructure copies no files

*For any* source folder (with or without FileDocuments), `CopyFolderStructure()` SHALL create a new folder tree with identical hierarchy but zero FileDocuments in every node.

**Validates: Requirements 8.1**

---

### Property 16: CopyFolderStructure respects MaxFolderDepth

*For any* source folder at depth D and destination parent at depth P, if `P + (D_max_subtree - D_source) > MaxFolderDepth`, `CopyFolderStructure()` SHALL throw `InvalidOperationException`.

**Validates: Requirements 8.2**

---

### Property 17: CopyFolderStructure preserves confidentiality

*For any* source folder tree, every node in the copied tree SHALL have the same `ConfidentialityLevel` and `InheritPermission` as its corresponding source node.

**Validates: Requirements 8.4**

---

### Property 18: Upload checkpoint round-trip

*For any* `UploadCheckpoint` object, saving it to isolated storage and loading it back SHALL produce an object with equal field values.

**Validates: Requirements 10.1, 10.3**

---

### Property 19: Expired checkpoints are not offered for resume

*For any* `UploadCheckpoint` with `CheckpointDate` older than 24 hours, `FileUploadForm` SHALL NOT display the resume dialog.

**Validates: Requirements 10.6**

---

## Error Handling

| Scenario | Exception / Behavior |
|---|---|
| Folder name มีอักขระต้องห้าม | `ArgumentException` — message ระบุอักขระที่พบ |
| Folder name ซ้ำใน parent | `InvalidOperationException` — message ระบุชื่อที่ซ้ำ |
| ลบ folder ที่มีไฟล์ โดย non-admin | `InvalidOperationException` — message แนะนำให้ติดต่อ Admin |
| CopyFolderStructure โดย user ที่ไม่มีสิทธิ์ | `UnauthorizedAccessException` |
| CopyFolderStructure เกิน MaxFolderDepth | `InvalidOperationException` |
| File preview โหลดไม่ได้ | แสดง error label ใน `FilePreviewPanel` — ไม่ throw ขึ้น FolderForm |
| UploadCheckpoint อ่านไม่ได้ (corrupt) | ลบ checkpoint แล้วเริ่มใหม่ — log warning |

FolderForm จะ catch `ArgumentException` และ `InvalidOperationException` จาก service แล้วแสดง `MessageBox` ที่เหมาะสม ไม่ให้ exception หลุดถึง user โดยไม่มี message

---

## Testing Strategy

### Unit Tests (CMS.Tests)

เน้น specific examples, edge cases, และ error conditions:

- `FolderServiceTests` — ทดสอบ `ValidateFolderName` ด้วย boundary inputs (ชื่อว่าง, ชื่อที่มีอักขระต้องห้ามแต่ละตัว, ชื่อที่ valid)
- `FolderServiceDuplicateTests` — ทดสอบ duplicate check กับ deleted siblings
- `CopyFolderStructureTests` — ทดสอบ depth limit, permission check, no-files invariant
- `UploadCheckpointServiceTests` — ทดสอบ save/load round-trip, expiry logic
- `FileFilterTests` — ทดสอบ AND logic, clear filter

### Property-Based Tests (CMS.Tests)

ใช้ **FsCheck** (NuGet: `FsCheck` สำหรับ .NET Framework 4.5) — run minimum **100 iterations** ต่อ property

แต่ละ property test ต้องมี comment tag:
```
// Feature: cms-missing-features, Property N: <property_text>
```

| Property | Test Method | Library |
|---|---|---|
| P1 — Invalid chars rejected | `Prop_InvalidChars_AlwaysThrow` | FsCheck |
| P2 — Valid names accepted | `Prop_ValidName_NeverThrow` | FsCheck |
| P3 — Duplicate sibling rejected | `Prop_DuplicateSibling_Throws` | FsCheck |
| P4 — Deleted sibling not duplicate | `Prop_DeletedSibling_Allowed` | FsCheck |
| P5 — Tree search correctness | `Prop_TreeSearch_MatchingNodes` | FsCheck |
| P6 — Empty search restores tree | `Prop_EmptySearch_FullTree` | FsCheck |
| P7 — Sort: folders before files | `Prop_Sort_FoldersBeforeFiles` | FsCheck |
| P8 — Sort idempotent | `Prop_Sort_Idempotent` | FsCheck |
| P9 — Non-admin blocked on non-empty | `Prop_Delete_NonAdmin_NonEmpty_Throws` | FsCheck |
| P10 — Admin can delete non-empty | `Prop_Delete_Admin_NonEmpty_Succeeds` | FsCheck |
| P11 — Empty folder deletable by any role | `Prop_Delete_EmptyFolder_AnyRole` | FsCheck |
| P12 — Filter AND logic | `Prop_Filter_AndLogic` | FsCheck |
| P13 — Clear filter restores all | `Prop_Filter_Clear_RestoresAll` | FsCheck |
| P14 — Filter keeps sub-folders | `Prop_Filter_KeepsFolders` | FsCheck |
| P15 — Copy has no files | `Prop_Copy_NoFiles` | FsCheck |
| P16 — Copy respects depth | `Prop_Copy_DepthLimit` | FsCheck |
| P17 — Copy preserves confidentiality | `Prop_Copy_PreservesConfidentiality` | FsCheck |
| P18 — Checkpoint round-trip | `Prop_Checkpoint_RoundTrip` | FsCheck |
| P19 — Expired checkpoint not offered | `Prop_Checkpoint_Expired_NotOffered` | FsCheck |

**Unit tests** ครอบคลุม integration points (FolderForm → FolderService), specific error messages, และ UI state (sort indicator, filter indicator)
