# Design Document — move-folder-file

## Overview

Feature นี้เพิ่มความสามารถ "ย้าย" (Move) ให้กับ CMS WinForms C# .NET Framework 4.5 ครอบคลุม 2 กรณีหลัก:

1. **ย้าย Folder** — ย้าย SourceFolder พร้อม sub-folder และ FileDocument ทั้งหมดไปยัง Parent Folder ใหม่ พร้อมอัปเดต FolderDepth และ PhysicalPath ของทุก node
2. **ย้าย FileDocument** — ย้าย FileDocument เดี่ยวหรือหลายรายการพร้อมกันไปยัง Folder ปลายทาง พร้อมอัปเดต PhysicalPath บน network drive

ทุก operation ต้องผ่าน permission check, validation (circular reference, depth limit, duplicate name) และบันทึก audit log ทุกครั้ง ยึดสถาปัตยกรรม MVP + Repository pattern ที่มีอยู่เดิม ไม่เปลี่ยน layer boundary

---

## Architecture

```
┌──────────────────────────────────────────────────────────────────┐
│  CMS.Presentation (WinForms)                                     │
│  FolderForm (+btnMove handler)   FolderPickerDialog (ใหม่)       │
│  MDI_Container (+menuItemMove, btnRibbonMove)                    │
│       │                                                          │
│  FolderPresenter / FilePresenter                                 │
└──────────────┬───────────────────────────────────────────────────┘
               │ IFolderService.MoveFolder()
               │ IFileService.MoveFiles()
┌──────────────▼───────────────────────────────────────────────────┐
│  CMS.Services                                                    │
│  FolderService (+MoveFolder)                                     │
│  FileService   (+MoveFiles)                                      │
│       │                                                          │
│  IPermissionService.CheckPermission()                            │
│  IAuditService.Log()                                             │
│  INetworkDriveHelper.BuildPath() / MoveFile()                    │
└──────────────┬───────────────────────────────────────────────────┘
               │ IFolderRepository / IFileDocumentRepository
┌──────────────▼───────────────────────────────────────────────────┐
│  CMS.Data                                                        │
│  FolderRepository (ใช้ GetDescendants, GetChildren, Update)      │
│  FileDocumentRepository (ใช้ GetByFolder, Update)                │
│  SQL: 021_MoveFolder_StoredProcedures.sql (ใหม่)                 │
└──────────────────────────────────────────────────────────────────┘
```

### Design Decisions

- **MoveFolder อยู่ใน FolderService** — business logic ทั้งหมด (validation, depth recalc, path update) อยู่ใน service layer เพื่อให้ testable และ reusable
- **MoveFiles อยู่ใน FileService** — สอดคล้องกับ pattern ที่มีอยู่เดิม (UploadFiles, DeleteFile, RenameFile อยู่ใน FileService)
- **ไม่ใช้ transaction ระดับ DB** — ใช้ pattern เดิมของ codebase (per-operation Update) เพื่อความสอดคล้อง
- **FolderPickerDialog เป็น Form แยก** — ไม่ embed ใน FolderForm เพื่อ reuse ได้จาก MDI_Container
- **ซ่อน source + descendants ใน picker** — ป้องกัน circular reference ที่ UI layer ก่อนถึง service layer
- **INetworkDriveHelper ไม่มี MoveFile** — ใช้ BuildPath ใหม่แล้ว update PhysicalPath ใน DB แทน (physical file ไม่ถูก move จริง เพราะ network drive ใช้ path-based access)


---

## Components and Interfaces

### 1. IFolderService — เพิ่ม MoveFolder

```csharp
// เพิ่มใน CMS.Services/Interfaces/IFolderService.cs
void MoveFolder(MoveFolderRequest request, UserContext user);
```

### 2. IFileService — เพิ่ม MoveFiles

```csharp
// เพิ่มใน CMS.Services/Interfaces/IFileService.cs
void MoveFiles(MoveFileRequest request, UserContext user);
```

### 3. FolderService.MoveFolder() — Algorithm

```
1. Validate inputs (null check, user != null)
2. Permission check:
   a. IF user.AppRole < 2 → throw UnauthorizedAccessException("ต้องมีสิทธิ์ระดับ SrOfficer ขึ้นไป")
   b. IF user.AppRole < 4:
      - IF !permissionService.CheckPermission(user.UserID, sourceFolderId, "Write")
        → throw UnauthorizedAccessException
      - IF !permissionService.CheckPermission(user.UserID, destinationParentFolderId, "Write")
        → throw UnauthorizedAccessException("ไม่มีสิทธิ์เขียนใน Folder ปลายทาง")
3. Load source = folderRepository.GetById(sourceFolderId)
   Load destination = folderRepository.GetById(destinationParentFolderId)
4. Circular reference check:
   a. IF sourceFolderId == destinationParentFolderId
      → throw InvalidOperationException("ไม่สามารถย้าย Folder ไปยังตัวเองได้")
   b. descendants = folderRepository.GetDescendants(sourceFolderId)
   c. IF descendants.Any(d => d.FolderID == destinationParentFolderId)
      → throw InvalidOperationException("ไม่สามารถย้าย Folder ไปยัง Descendant ของตัวเองได้")
5. Depth limit check:
   a. maxDescendantDepth = descendants.Any()
        ? descendants.Max(d => d.FolderDepth)
        : source.FolderDepth
   b. subtreeHeight = maxDescendantDepth - source.FolderDepth
   c. projectedDepth = destination.FolderDepth + 1 + subtreeHeight
   d. IF projectedDepth > MaxFolderDepth (10)
      → throw InvalidOperationException(
          string.Format("การย้ายจะทำให้ depth เป็น {0} ซึ่งเกิน MaxFolderDepth ({1})",
                        projectedDepth, MaxFolderDepth))
6. Duplicate name check:
   a. siblings = folderRepository.GetChildren(destinationParentFolderId)
      (GetChildren คืนเฉพาะ active folders — IsDeleted = false)
   b. IF siblings.Any(s => string.Equals(s.FolderName.Trim(), source.FolderName.Trim(),
                                          StringComparison.OrdinalIgnoreCase))
      → throw InvalidOperationException(
          string.Format("มี Folder ชื่อ '{0}' อยู่แล้วใน Folder ปลายทาง", source.FolderName))
7. Record old values for audit:
   oldParentFolderId = source.ParentFolderID
   depthDelta = (destination.FolderDepth + 1) - source.FolderDepth
8. Update source folder:
   source.ParentFolderID = destinationParentFolderId
   source.FolderDepth    = destination.FolderDepth + 1
   source.PhysicalPath   = networkDriveHelper.BuildPath(source.OrgUnitID, source.FolderID, source.FolderName)
   source.UpdatedBy      = user.UserID
   source.UpdatedDate    = DateTime.Now
   folderRepository.Update(source)
9. Update all descendants (depth + path):
   FOR EACH descendant IN descendants:
     descendant.FolderDepth += depthDelta
     descendant.PhysicalPath = networkDriveHelper.BuildPath(...)
     descendant.UpdatedBy    = user.UserID
     descendant.UpdatedDate  = DateTime.Now
     folderRepository.Update(descendant)
10. Update PhysicalPath of all FileDocuments in source + descendants:
    allFolderIds = [sourceFolderId] + descendants.Select(d => d.FolderID)
    FOR EACH folderId IN allFolderIds:
      files = fileDocumentRepository.GetByFolder(folderId)
      FOR EACH file IN files WHERE file.FileStatus == 1 (Active):
        file.PhysicalPath = networkDriveHelper.BuildPath(
            source.OrgUnitID, file.FolderID, file.StoredFileName)
        fileDocumentRepository.Update(file)
11. Audit log:
    auditService.Log(new AuditEntry {
        Action    = "MoveFolder",
        RecordID  = sourceFolderId,
        OldValue  = oldParentFolderId.ToString(),
        NewValue  = destinationParentFolderId.ToString(),
        UserID    = user.UserID, ...
    })
```

### 4. FileService.MoveFiles() — Algorithm

```
1. Validate inputs (null check, user != null)
2. IF request.FileIds == null || !request.FileIds.Any()
   → throw ArgumentException("ต้องระบุไฟล์อย่างน้อย 1 รายการ")
3. Load destination = folderRepository.GetById(request.DestinationFolderId)
   IF destination == null → throw InvalidOperationException
4. Permission check:
   IF user.AppRole < 4:
     IF !permissionService.CheckPermission(user.UserID, request.DestinationFolderId, "Write")
       → throw UnauthorizedAccessException("ไม่มีสิทธิ์เขียนใน Folder ปลายทาง '{0}'")
5. FOR EACH fileId IN request.FileIds:
   a. file = fileDocumentRepository.GetById(fileId)
   b. IF file == null → throw InvalidOperationException
   c. IF file.FileStatus != 1 (Active)
      → throw InvalidOperationException(
          string.Format("ไม่สามารถย้ายไฟล์ ID {0} ได้ เนื่องจากสถานะไม่ใช่ Active", fileId))
   d. IF user.AppRole < 4:
      IF !permissionService.CheckPermission(user.UserID, file.FolderID, "Write")
        → throw UnauthorizedAccessException
   e. oldFolderId = file.FolderID
   f. file.FolderID      = request.DestinationFolderId
   g. file.PhysicalPath  = networkDriveHelper.BuildPath(
          destination.OrgUnitID, request.DestinationFolderId, file.StoredFileName)
   h. fileDocumentRepository.Update(file)
   i. auditService.Log(new AuditEntry {
          Action   = "MoveFile",
          RecordID = fileId,
          OldValue = oldFolderId.ToString(),
          NewValue = request.DestinationFolderId.ToString(),
          UserID   = user.UserID, ...
      })
```

### 5. FolderPickerDialog (ใหม่ใน CMS.Presentation)

```csharp
public class FolderPickerDialog : Form
{
    private TreeView  _treeView;
    private Label     _lblSelected;
    private Button    _btnOk;
    private Button    _btnCancel;

    // SourceFolderId และ descendants ที่ต้องซ่อน
    public int?       SourceFolderId { get; set; }
    public HashSet<int> ExcludedFolderIds { get; private set; }

    // Return value — null ถ้า user กด Cancel
    public int? SelectedFolderId { get; private set; }

    public void LoadFolders(IEnumerable<Folder> allFolders, IEnumerable<int> excludedIds)
    {
        // กรอง IsDeleted = true, IsArchived = true, และ excludedIds ออก
        // สร้าง TreeNode hierarchy จาก allFolders ที่เหลือ
        // Expand ถึง level 2 โดย default
    }

    private void BtnOk_Click(object sender, EventArgs e)
    {
        if (_treeView.SelectedNode == null)
        {
            MessageBox.Show("กรุณาเลือก Folder ปลายทางก่อน", "แจ้งเตือน",
                            MessageBoxButtons.OK, MessageBoxIcon.Warning);
            return;
        }
        SelectedFolderId = (int)_treeView.SelectedNode.Tag;
        DialogResult = DialogResult.OK;
        Close();
    }

    private void TreeView_AfterSelect(object sender, TreeViewEventArgs e)
    {
        _lblSelected.Text = string.Format("เลือก: {0}", e.Node.Text);
    }
}
```

### 6. FolderForm — wire btnMove

```csharp
private void BtnMove_Click(object sender, EventArgs e)
{
    // กรณี 1: เลือก Folder ใน folderTreeView
    if (folderTreeView.SelectedNode != null && _selectedFolderId.HasValue)
    {
        var descendants = _folderService.GetDescendants(_selectedFolderId.Value);
        var excluded    = new HashSet<int>(descendants.Select(d => d.FolderID)) { _selectedFolderId.Value };

        using (var picker = new FolderPickerDialog())
        {
            picker.SourceFolderId = _selectedFolderId.Value;
            picker.LoadFolders(_folderService.GetAllFolders(null, _currentUser), excluded);
            if (picker.ShowDialog() == DialogResult.OK && picker.SelectedFolderId.HasValue)
            {
                try
                {
                    _folderService.MoveFolder(new MoveFolderRequest
                    {
                        SourceFolderId            = _selectedFolderId.Value,
                        DestinationParentFolderId = picker.SelectedFolderId.Value
                    }, _currentUser);
                    MessageBox.Show(string.Format("ย้าย Folder สำเร็จ"), "สำเร็จ",
                                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ReloadFolderTree();
                }
                catch (UnauthorizedAccessException ex) { ShowError("ไม่มีสิทธิ์: " + ex.Message); }
                catch (InvalidOperationException ex)   { ShowError(ex.Message); }
                catch (Exception ex)                   { ShowUnexpectedError(ex); }
            }
        }
        return;
    }

    // กรณี 2: เลือก FileDocument ใน contentsListView
    var selectedFileIds = GetSelectedFileIds();
    if (selectedFileIds.Any())
    {
        using (var picker = new FolderPickerDialog())
        {
            picker.LoadFolders(_folderService.GetAllFolders(null, _currentUser), new HashSet<int>());
            if (picker.ShowDialog() == DialogResult.OK && picker.SelectedFolderId.HasValue)
            {
                try
                {
                    _fileService.MoveFiles(new MoveFileRequest
                    {
                        FileIds             = selectedFileIds,
                        DestinationFolderId = picker.SelectedFolderId.Value
                    }, _currentUser);
                    MessageBox.Show(
                        string.Format("ย้าย {0} ไฟล์ สำเร็จ", selectedFileIds.Count()), "สำเร็จ",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    ReloadContents();
                }
                catch (UnauthorizedAccessException ex) { ShowError("ไม่มีสิทธิ์: " + ex.Message); }
                catch (InvalidOperationException ex)   { ShowError(ex.Message); }
                catch (Exception ex)                   { ShowUnexpectedError(ex); }
            }
        }
    }
}
```

### 7. MDI_Container — wire menuItemMove / btnRibbonMove

```csharp
private void MenuItemMove_Click(object sender, EventArgs e)
{
    var activeFolder = ActiveMdiChild as FolderForm;
    if (activeFolder != null)
        activeFolder.TriggerMove(); // public method ที่ delegate ไปยัง BtnMove_Click logic
}
```


---

## Data Models

### MoveFolderRequest (ใหม่)

```csharp
// CMS.Services/DTOs/MoveFolderRequest.cs
namespace CMS.Services.DTOs
{
    /// <summary>Request DTO สำหรับย้าย Folder ไปยัง Parent Folder ใหม่</summary>
    public class MoveFolderRequest
    {
        /// <summary>FolderID ของ Folder ที่ต้องการย้าย</summary>
        public int SourceFolderId { get; set; }

        /// <summary>FolderID ของ Parent Folder ปลายทาง</summary>
        public int DestinationParentFolderId { get; set; }
    }
}
```

### MoveFileRequest (ใหม่)

```csharp
// CMS.Services/DTOs/MoveFileRequest.cs
namespace CMS.Services.DTOs
{
    /// <summary>Request DTO สำหรับย้าย FileDocument หนึ่งหรือหลายรายการ</summary>
    public class MoveFileRequest
    {
        /// <summary>รายการ FileID ที่ต้องการย้าย (อย่างน้อย 1 รายการ)</summary>
        public IEnumerable<int> FileIds { get; set; }

        /// <summary>FolderID ของ Folder ปลายทาง</summary>
        public int DestinationFolderId { get; set; }
    }
}
```

### SQL Migration — 021_MoveFolder_StoredProcedures.sql

ไม่ต้องการ stored procedure ใหม่สำหรับ move operation เพราะ:
- `FolderRepository.Update()` ใช้ `sp_UpdateFolder` ที่มีอยู่แล้ว
- `FileDocumentRepository.Update()` ใช้ `sp_UpdateFileDocument` ที่มีอยู่แล้ว
- `FolderRepository.GetDescendants()` และ `GetChildren()` มีอยู่แล้ว

อย่างไรก็ตาม ควรเพิ่ม index เพื่อ performance:

```sql
-- 021_MoveFolder_Index.sql
-- Index สำหรับ GetDescendants query ที่ใช้บ่อยใน MoveFolder
IF NOT EXISTS (
    SELECT 1 FROM sys.indexes
    WHERE name = 'IX_Folder_ParentFolderID_IsDeleted'
      AND object_id = OBJECT_ID('storage.Folder')
)
BEGIN
    CREATE NONCLUSTERED INDEX IX_Folder_ParentFolderID_IsDeleted
    ON storage.Folder (ParentFolderID, IsDeleted)
    INCLUDE (FolderID, FolderName, FolderDepth, PhysicalPath);
END
GO
```

---

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system — essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Move correctness invariant

*For any* valid (sourceFolderId, destinationParentFolderId) pair ที่ไม่มี circular reference และ projected depth ไม่เกิน MaxFolderDepth หลังจาก MoveFolder() สำเร็จ `GetById(sourceFolderId).ParentFolderID` ต้องเท่ากับ `destinationParentFolderId`

**Validates: Requirements 1.1, 11.1**

---

### Property 2: Depth consistency invariant

*For any* SourceFolder ที่มี Descendants หลังจาก MoveFolder() สำเร็จ FolderDepth ของทุก Descendant ต้องเพิ่มขึ้นหรือลดลงเท่ากับ `(destination.FolderDepth + 1 - source.FolderDepth เดิม)` และ FolderDepth ของ SourceFolder ต้องเท่ากับ `destination.FolderDepth + 1`

**Validates: Requirements 1.2, 11.2**

---

### Property 3: Path consistency after move

*For any* SourceFolder ที่มี Descendants และ FileDocuments หลังจาก MoveFolder() สำเร็จ PhysicalPath ของ SourceFolder, Descendants ทุก node, และ FileDocuments ทุกรายการต้องถูก rebuild ใหม่ผ่าน `NetworkDriveHelper.BuildPath()` ให้สอดคล้องกับตำแหน่งใหม่

**Validates: Requirements 1.3, 1.4**

---

### Property 4: Circular reference rejection

*For any* sourceFolderId และ destinationParentFolderId ที่ `destinationParentFolderId == sourceFolderId` หรือ `destinationParentFolderId` เป็น Descendant ของ `sourceFolderId` การเรียก MoveFolder() ต้องโยน `InvalidOperationException` เสมอ

**Validates: Requirements 2.1, 2.2, 11.3**

---

### Property 5: Depth limit rejection

*For any* (sourceFolderId, destinationParentFolderId) ที่ projected depth เกิน MaxFolderDepth (10) การเรียก MoveFolder() ต้องโยน `InvalidOperationException` เสมอ

**Validates: Requirements 3.1, 11.4**

---

### Property 6: Duplicate name rejection

*For any* SourceFolder ที่มีชื่อเหมือนกับ active child Folder ของ DestinationFolder (case-insensitive, trim whitespace) การเรียก MoveFolder() ต้องโยน `InvalidOperationException` เสมอ

**Validates: Requirements 4.1, 11.5**

---

### Property 7: Role-based access control

*For any* user ที่มี `AppRole < 2` การเรียก MoveFolder() ต้องโยน `UnauthorizedAccessException` เสมอ และ *for any* user ที่มี `AppRole = 4` (Admin) การเรียก MoveFolder() ต้องไม่โยน exception เนื่องจาก role เพียงอย่างเดียว

**Validates: Requirements 5.1, 5.4, 11.8**

---

### Property 8: Write permission required for move

*For any* user ที่มี `AppRole >= 2` และ `AppRole < 4` ที่ไม่มีสิทธิ์ Write บน SourceFolder หรือ DestinationFolder การเรียก MoveFolder() ต้องโยน `UnauthorizedAccessException` เสมอ

**Validates: Requirements 5.2, 5.3**

---

### Property 9: File move correctness invariant

*For any* valid fileIds และ destinationFolderId หลังจาก MoveFiles() สำเร็จ `GetById(fileId).FolderID` ต้องเท่ากับ `destinationFolderId` สำหรับทุก fileId และ PhysicalPath ของทุก file ต้องถูก rebuild ใหม่ผ่าน `NetworkDriveHelper.BuildPath()`

**Validates: Requirements 6.1, 6.2, 11.6**

---

### Property 10: Inactive file rejection

*For any* fileIds ที่มี FileDocument ที่มี `FileStatus != 1` (ไม่ใช่ Active) การเรียก MoveFiles() ต้องโยน `InvalidOperationException` เสมอ

**Validates: Requirements 6.5**

---

### Property 11: File permission check

*For any* user ที่มี `AppRole < 4` ที่ไม่มีสิทธิ์ Write บน DestinationFolder หรือ source Folder ของ FileDocument การเรียก MoveFiles() ต้องโยน `UnauthorizedAccessException` เสมอ

**Validates: Requirements 7.1, 7.2, 7.3**

---

### Property 12: Audit completeness

*For any* การเรียก MoveFolder() หรือ MoveFiles() ที่สำเร็จ `AuditService.Log()` ต้องถูกเรียกอย่างน้อย 1 ครั้งพร้อม Action = "MoveFolder" หรือ "MoveFile" ตามลำดับ และต้องมี OldValue และ NewValue ที่ถูกต้อง

**Validates: Requirements 1.5, 6.3, 11.7**

---

## Error Handling

| Scenario | Exception / Behavior | Requirement |
|---|---|---|
| `user.AppRole < 2` เรียก MoveFolder | `UnauthorizedAccessException` — "ต้องมีสิทธิ์ระดับ SrOfficer ขึ้นไป" | Req 5.1 |
| ไม่มีสิทธิ์ Write บน SourceFolder | `UnauthorizedAccessException` | Req 5.2 |
| ไม่มีสิทธิ์ Write บน DestinationFolder | `UnauthorizedAccessException` — ระบุชื่อ Folder ปลายทาง | Req 5.3, 7.1 |
| destination == source (self-move) | `InvalidOperationException` — "ไม่สามารถย้าย Folder ไปยังตัวเองได้" | Req 2.1 |
| destination เป็น Descendant ของ source | `InvalidOperationException` — "ไม่สามารถย้าย Folder ไปยัง Descendant ของตัวเองได้" | Req 2.2 |
| projected depth > MaxFolderDepth | `InvalidOperationException` — ระบุ depth ที่จะเกินและค่า MaxFolderDepth | Req 3.1 |
| ชื่อซ้ำใน DestinationFolder | `InvalidOperationException` — ระบุชื่อที่ซ้ำ | Req 4.1 |
| fileIds ว่างหรือ null | `ArgumentException` — "ต้องระบุไฟล์อย่างน้อย 1 รายการ" | Req 6.4 |
| FileDocument มี FileStatus != 1 | `InvalidOperationException` — ระบุ FileID ที่ไม่สามารถย้ายได้ | Req 6.5 |
| SourceFolder ไม่พบใน DB | `InvalidOperationException` — ระบุ FolderID | - |
| DestinationFolder ไม่พบใน DB | `InvalidOperationException` — ระบุ FolderID | - |
| Exception ที่ไม่คาดคิด | FolderForm แสดง MessageBox + log ผ่าน LoggingService — ไม่ crash | Req 10.4 |

FolderForm จะ catch `UnauthorizedAccessException`, `InvalidOperationException`, และ `Exception` ทั่วไป แล้วแสดง `MessageBox` ที่เหมาะสม ไม่ให้ exception หลุดถึง user โดยไม่มี message

---

## Testing Strategy

### Unit Tests (CMS.Tests)

เน้น specific examples, edge cases, และ integration points:

- `MoveFolderServiceTests` — ทดสอบ permission check, circular reference, depth limit, duplicate name ด้วย boundary inputs
- `MoveFilesServiceTests` — ทดสอบ empty fileIds, inactive file status, permission check
- `FolderPickerDialogTests` — ทดสอบ exclusion logic (source + descendants ถูกซ่อน)
- `FolderFormMoveTests` — ทดสอบ UI state (btnMove disabled เมื่อไม่มี selection, reload หลัง move สำเร็จ)

### Property-Based Tests (CMS.Tests)

ใช้ **FsCheck** (NuGet: `FsCheck` สำหรับ .NET Framework 4.5) — run minimum **100 iterations** ต่อ property

แต่ละ property test ต้องมี comment tag:
```
// Feature: move-folder-file, Property N: <property_text>
```

แต่ละ Correctness Property ต้องถูก implement โดย property-based test **เพียง 1 test** ต่อ property:

| Property | Test Method | Library |
|---|---|---|
| P1 — Move correctness invariant | `Prop_MoveFolder_UpdatesParentFolderId` | FsCheck |
| P2 — Depth consistency invariant | `Prop_MoveFolder_DepthConsistency` | FsCheck |
| P3 — Path consistency after move | `Prop_MoveFolder_PathConsistency` | FsCheck |
| P4 — Circular reference rejection | `Prop_MoveFolder_CircularReference_Throws` | FsCheck |
| P5 — Depth limit rejection | `Prop_MoveFolder_DepthLimit_Throws` | FsCheck |
| P6 — Duplicate name rejection | `Prop_MoveFolder_DuplicateName_Throws` | FsCheck |
| P7 — Role-based access control | `Prop_MoveFolder_LowRole_Throws` | FsCheck |
| P8 — Write permission required | `Prop_MoveFolder_NoWritePermission_Throws` | FsCheck |
| P9 — File move correctness invariant | `Prop_MoveFiles_UpdatesFolderIdAndPath` | FsCheck |
| P10 — Inactive file rejection | `Prop_MoveFiles_InactiveFile_Throws` | FsCheck |
| P11 — File permission check | `Prop_MoveFiles_NoPermission_Throws` | FsCheck |
| P12 — Audit completeness | `Prop_Move_AuditLogged` | FsCheck |

**Unit tests** ครอบคลุม:
- Integration points (FolderForm → FolderService → FolderRepository)
- Specific error messages ที่ต้องแสดงใน MessageBox
- UI state transitions (btnMove enable/disable, reload หลัง move)
- Edge cases: deleted sibling ไม่นับเป็น duplicate, Admin bypass permission check
