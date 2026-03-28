# Requirements Document

## Introduction

Feature นี้เพิ่มความสามารถ "ย้าย" (Move) ให้กับ CMS WinForms C# .NET Framework 4.5 ครอบคลุม 2 กรณีหลัก ได้แก่ (1) ย้าย Folder พร้อม sub-folder และไฟล์ทั้งหมดไปยัง Parent Folder ใหม่ และ (2) ย้าย FileDocument เดี่ยวหรือหลายรายการพร้อมกันไปยัง Folder ปลายทาง ระบบต้องตรวจสอบ permission, depth limit, duplicate name, circular reference และอัปเดต PhysicalPath บน network drive ทุกครั้ง รวมถึงบันทึก audit log ทุก action

## Glossary

- **FolderService**: Service class ใน CMS.Services ที่รับผิดชอบ business logic ของ Folder ทั้งหมด รวมถึง MoveFolder ใหม่
- **FileService**: Service class ใน CMS.Services ที่รับผิดชอบ business logic ของ FileDocument รวมถึง MoveFile ใหม่
- **FolderRepository**: Repository class ที่ติดต่อฐานข้อมูลสำหรับ Folder
- **FileDocumentRepository**: Repository class ที่ติดต่อฐานข้อมูลสำหรับ FileDocument
- **FolderForm**: WinForms Form หลักสำหรับจัดการโครงสร้าง Folder และแสดงรายการไฟล์
- **FolderPickerDialog**: WinForms Dialog ใหม่สำหรับให้ผู้ใช้งานเลือก Folder ปลายทาง แสดงโครงสร้าง Folder แบบ TreeView
- **folderTreeView**: TreeView control ใน FolderForm ที่แสดงโครงสร้าง Folder แบบ hierarchy
- **contentsListView**: ListView control ใน FolderForm ที่แสดงรายการ Folder และไฟล์ภายใน Folder ที่เลือก
- **btnMove**: Button control ใน FolderForm ที่ trigger การย้าย (ปัจจุบัน wire ไปที่ OpenFolderStructure() เฉยๆ)
- **menuItemMove**: MenuItem ใน MDI_Container ที่ trigger การย้าย
- **btnRibbonMove**: Ribbon button ใน MDI_Container ที่ trigger การย้าย
- **AuditService**: Service สำหรับบันทึก audit log ทุก action ผ่าน AuditEntry
- **NetworkDriveHelper**: Infrastructure class สำหรับจัดการ physical path บน network drive
- **SourceFolder**: Folder ที่ถูกเลือกเพื่อย้าย
- **DestinationFolder**: Folder ปลายทางที่ SourceFolder หรือ FileDocument จะถูกย้ายไปอยู่ใต้
- **MaxFolderDepth**: ค่าสูงสุดของ FolderDepth ที่อนุญาต = 10
- **Descendant**: Folder ใดๆ ที่อยู่ใต้ SourceFolder ในทุกระดับ (ลูก, หลาน, ฯลฯ)
- **UserContext**: Object ที่เก็บข้อมูล user ที่ login อยู่ รวมถึง AppRole (1=Officer, 2=SrOfficer, 3=Manager, 4=Admin)
- **MoveFolderRequest**: DTO ใหม่สำหรับส่งข้อมูลการย้าย Folder เข้า FolderService
- **MoveFileRequest**: DTO ใหม่สำหรับส่งข้อมูลการย้าย FileDocument เข้า FileService

---

## Requirements

### Requirement 1: ย้าย Folder ไปยัง Parent Folder ใหม่

**User Story:** As a ผู้ใช้งาน, I want ย้าย Folder พร้อม sub-folder และไฟล์ทั้งหมดไปยัง Parent Folder ใหม่, so that สามารถจัดโครงสร้าง Folder ใหม่ได้โดยไม่ต้องสร้างและย้ายไฟล์ทีละรายการ

#### Acceptance Criteria

1. WHEN ผู้ใช้งานเรียก MoveFolder() ด้วย sourceFolderId และ destinationParentFolderId ที่ถูกต้อง, THE FolderService SHALL อัปเดต ParentFolderID ของ SourceFolder ให้เป็น destinationParentFolderId ในฐานข้อมูล
2. WHEN MoveFolder() สำเร็จ, THE FolderService SHALL อัปเดต FolderDepth ของ SourceFolder และ Descendant ทุก node ให้สอดคล้องกับตำแหน่งใหม่
3. WHEN MoveFolder() สำเร็จ, THE FolderService SHALL อัปเดต PhysicalPath ของ SourceFolder และ Descendant ทุก node ผ่าน NetworkDriveHelper
4. WHEN MoveFolder() สำเร็จ, THE FolderService SHALL อัปเดต PhysicalPath ของ FileDocument ทุกรายการใน SourceFolder และ Descendant ทุก node ผ่าน NetworkDriveHelper
5. WHEN MoveFolder() สำเร็จ, THE AuditService SHALL บันทึก AuditEntry ที่มี Action = "MoveFolder", RecordID = sourceFolderId, OldValue = ParentFolderID เดิม, NewValue = destinationParentFolderId

---

### Requirement 2: Validation ก่อนย้าย Folder — Circular Reference

**User Story:** As a ผู้ใช้งาน, I want ระบบป้องกันการย้าย Folder ไปเป็น Descendant ของตัวเอง, so that โครงสร้าง Folder ไม่เกิด circular reference ที่ทำให้ระบบทำงานผิดพลาด

#### Acceptance Criteria

1. WHEN ผู้ใช้งานเรียก MoveFolder() โดยที่ destinationParentFolderId เท่ากับ sourceFolderId, THEN THE FolderService SHALL โยน InvalidOperationException พร้อม message ที่ระบุว่าไม่สามารถย้าย Folder ไปยังตัวเองได้
2. WHEN ผู้ใช้งานเรียก MoveFolder() โดยที่ DestinationFolder เป็น Descendant ของ SourceFolder, THEN THE FolderService SHALL โยน InvalidOperationException พร้อม message ที่ระบุว่าไม่สามารถย้าย Folder ไปยัง Descendant ของตัวเองได้
3. THE FolderService SHALL ตรวจสอบ circular reference โดยใช้ GetDescendants(sourceFolderId) เพื่อดึง Descendant ทั้งหมดก่อนดำเนินการย้าย

---

### Requirement 3: Validation ก่อนย้าย Folder — Depth Limit

**User Story:** As a ผู้ใช้งาน, I want ระบบป้องกันการย้าย Folder ที่จะทำให้ depth เกินขีดจำกัด, so that โครงสร้าง Folder ไม่ลึกเกินกว่าที่ระบบรองรับ

#### Acceptance Criteria

1. WHEN ผู้ใช้งานเรียก MoveFolder() และ depth ของ SourceFolder ที่ตำแหน่งใหม่จะเกิน MaxFolderDepth (10), THEN THE FolderService SHALL โยน InvalidOperationException พร้อม message ที่ระบุ depth ที่จะเกินและค่า MaxFolderDepth
2. THE FolderService SHALL คำนวณ projected depth โดยใช้สูตร: (FolderDepth ของ DestinationFolder + 1) + (depth ของ subtree ที่ลึกที่สุดของ SourceFolder - FolderDepth ของ SourceFolder)
3. WHEN depth ของ SourceFolder ที่ตำแหน่งใหม่ไม่เกิน MaxFolderDepth, THE FolderService SHALL ดำเนินการย้ายต่อโดยไม่มี error เกี่ยวกับ depth

---

### Requirement 4: Validation ก่อนย้าย Folder — Duplicate Name

**User Story:** As a ผู้ใช้งาน, I want ระบบป้องกันการย้าย Folder ที่จะทำให้ชื่อซ้ำกับ Folder อื่นใน DestinationFolder, so that โครงสร้าง Folder ไม่สับสนและสามารถระบุ Folder ได้ชัดเจน

#### Acceptance Criteria

1. WHEN ผู้ใช้งานเรียก MoveFolder() และ DestinationFolder มี child Folder ที่มีชื่อเหมือนกับ SourceFolder (case-insensitive, trim whitespace), THEN THE FolderService SHALL โยน InvalidOperationException พร้อม message ที่ระบุชื่อที่ซ้ำ
2. THE FolderService SHALL ตรวจสอบ duplicate โดยใช้ GetChildren(destinationParentFolderId) และเปรียบเทียบแบบ case-insensitive และ trim whitespace
3. THE FolderService SHALL ไม่นับ Folder ที่ IsDeleted = true เป็น duplicate เมื่อตรวจสอบ

---

### Requirement 5: Permission Check ก่อนย้าย Folder

**User Story:** As a ผู้ใช้งาน, I want ระบบตรวจสอบสิทธิ์ก่อนย้าย Folder, so that ผู้ใช้งานที่ไม่มีสิทธิ์ไม่สามารถย้าย Folder ได้

#### Acceptance Criteria

1. WHEN ผู้ใช้งานที่มี AppRole < 2 (Officer) เรียก MoveFolder(), THEN THE FolderService SHALL โยน UnauthorizedAccessException พร้อม message ที่ระบุว่าต้องมีสิทธิ์ระดับ SrOfficer ขึ้นไป
2. WHEN ผู้ใช้งานที่มี AppRole >= 2 เรียก MoveFolder() บน SourceFolder ที่ตนไม่มีสิทธิ์ Write, THEN THE FolderService SHALL โยน UnauthorizedAccessException
3. WHEN ผู้ใช้งานที่มี AppRole >= 2 เรียก MoveFolder() โดยที่ตนไม่มีสิทธิ์ Write บน DestinationFolder, THEN THE FolderService SHALL โยน UnauthorizedAccessException พร้อม message ที่ระบุว่าไม่มีสิทธิ์เขียนใน Folder ปลายทาง
4. WHEN ผู้ใช้งานที่มี AppRole = 4 (Admin) เรียก MoveFolder(), THE FolderService SHALL ดำเนินการโดยไม่ตรวจสอบ permission ของ Folder

---

### Requirement 6: ย้าย FileDocument เดี่ยวหรือหลายรายการ

**User Story:** As a ผู้ใช้งาน, I want ย้าย FileDocument เดี่ยวหรือหลายรายการพร้อมกันไปยัง Folder ปลายทาง, so that สามารถจัดระเบียบไฟล์ได้โดยไม่ต้องย้ายทีละไฟล์

#### Acceptance Criteria

1. WHEN ผู้ใช้งานเรียก MoveFiles() ด้วย fileIds (หนึ่งหรือหลาย ID) และ destinationFolderId ที่ถูกต้อง, THE FileService SHALL อัปเดต FolderID ของ FileDocument แต่ละรายการให้เป็น destinationFolderId ในฐานข้อมูล
2. WHEN MoveFiles() สำเร็จ, THE FileService SHALL อัปเดต PhysicalPath ของ FileDocument แต่ละรายการผ่าน NetworkDriveHelper โดยใช้ BuildPath(orgUnitId, destinationFolderId, storedFileName)
3. WHEN MoveFiles() สำเร็จ, THE AuditService SHALL บันทึก AuditEntry สำหรับ FileDocument แต่ละรายการที่ย้าย โดยมี Action = "MoveFile", RecordID = fileId, OldValue = FolderID เดิม, NewValue = destinationFolderId
4. WHEN fileIds มีค่าว่างหรือไม่มีรายการ, THEN THE FileService SHALL โยน ArgumentException พร้อม message ที่ระบุว่าต้องระบุไฟล์อย่างน้อย 1 รายการ
5. WHEN FileDocument ใดใน fileIds มี FileStatus ไม่ใช่ Active (FileStatus != 1), THEN THE FileService SHALL โยน InvalidOperationException พร้อม message ที่ระบุ FileID ที่ไม่สามารถย้ายได้

---

### Requirement 7: Permission Check ก่อนย้าย FileDocument

**User Story:** As a ผู้ใช้งาน, I want ระบบตรวจสอบสิทธิ์ก่อนย้ายไฟล์, so that ผู้ใช้งานที่ไม่มีสิทธิ์ไม่สามารถย้ายไฟล์ไปยัง Folder ที่ไม่ได้รับอนุญาตได้

#### Acceptance Criteria

1. WHEN ผู้ใช้งานเรียก MoveFiles() และตนไม่มีสิทธิ์ Write บน DestinationFolder, THEN THE FileService SHALL โยน UnauthorizedAccessException พร้อม message ที่ระบุชื่อ Folder ปลายทางที่ไม่มีสิทธิ์
2. WHEN ผู้ใช้งานเรียก MoveFiles() และตนไม่มีสิทธิ์ Write บน Folder ต้นทางของ FileDocument, THEN THE FileService SHALL โยน UnauthorizedAccessException
3. WHEN ผู้ใช้งานที่มี AppRole = 4 (Admin) เรียก MoveFiles(), THE FileService SHALL ดำเนินการโดยไม่ตรวจสอบ permission ของ Folder

---

### Requirement 8: UI — FolderPickerDialog สำหรับเลือก Folder ปลายทาง

**User Story:** As a ผู้ใช้งาน, I want เลือก Folder ปลายทางผ่าน dialog ที่แสดงโครงสร้าง Folder แบบ tree, so that สามารถเลือก Folder ปลายทางได้ง่ายและถูกต้อง

#### Acceptance Criteria

1. THE FolderPickerDialog SHALL แสดง TreeView ที่มีโครงสร้าง Folder ทั้งหมดที่ผู้ใช้งานมีสิทธิ์เข้าถึง โดยไม่แสดง Folder ที่ IsDeleted = true หรือ IsArchived = true
2. WHEN ผู้ใช้งานเปิด FolderPickerDialog เพื่อย้าย SourceFolder, THE FolderPickerDialog SHALL ซ่อน SourceFolder และ Descendant ทั้งหมดของ SourceFolder ออกจาก TreeView เพื่อป้องกัน circular reference
3. WHEN ผู้ใช้งานเลือก Folder ใน TreeView และกดปุ่ม "ตกลง", THE FolderPickerDialog SHALL ส่งคืน FolderID ของ Folder ที่เลือกกลับไปยัง FolderForm
4. WHEN ผู้ใช้งานกดปุ่ม "ยกเลิก" หรือปิด FolderPickerDialog, THE FolderPickerDialog SHALL ส่งคืน null และ FolderForm SHALL ไม่ดำเนินการย้ายใดๆ
5. THE FolderPickerDialog SHALL แสดงชื่อ Folder ที่เลือกอยู่ปัจจุบันใน label เพื่อให้ผู้ใช้งานยืนยันก่อนกด "ตกลง"
6. WHEN ผู้ใช้งานกดปุ่ม "ตกลง" โดยไม่ได้เลือก Folder ใดใน TreeView, THE FolderPickerDialog SHALL แสดง MessageBox แจ้งให้เลือก Folder ปลายทางก่อน และไม่ปิด dialog

---

### Requirement 9: UI — Trigger การย้ายจาก FolderForm และ MDI_Container

**User Story:** As a ผู้ใช้งาน, I want กดปุ่มหรือเมนูเพื่อเริ่มการย้าย Folder หรือไฟล์, so that สามารถเข้าถึงฟีเจอร์ Move ได้จากหลายจุดใน UI

#### Acceptance Criteria

1. WHEN ผู้ใช้งานเลือก Folder ใน folderTreeView และกด btnMove, THE FolderForm SHALL เปิด FolderPickerDialog เพื่อให้เลือก DestinationFolder แล้วเรียก FolderService.MoveFolder()
2. WHEN ผู้ใช้งานเลือก FileDocument หนึ่งหรือหลายรายการใน contentsListView และกด btnMove, THE FolderForm SHALL เปิด FolderPickerDialog เพื่อให้เลือก DestinationFolder แล้วเรียก FileService.MoveFiles()
3. WHEN ผู้ใช้งานคลิก menuItemMove หรือ btnRibbonMove ใน MDI_Container, THE MDI_Container SHALL ส่ง event ไปยัง FolderForm ที่ active อยู่เพื่อ trigger การย้ายเหมือนกับการกด btnMove
4. WHEN ไม่มี Folder หรือ FileDocument ถูกเลือกอยู่, THE FolderForm SHALL disable btnMove และ THE MDI_Container SHALL disable menuItemMove และ btnRibbonMove
5. WHEN MoveFolder() หรือ MoveFiles() สำเร็จ, THE FolderForm SHALL reload folderTreeView และ contentsListView เพื่อแสดงโครงสร้างที่อัปเดตแล้ว

---

### Requirement 10: UI — Feedback และ Error Handling ใน FolderForm

**User Story:** As a ผู้ใช้งาน, I want ได้รับ feedback ที่ชัดเจนเมื่อการย้ายสำเร็จหรือล้มเหลว, so that ทราบผลลัพธ์ของการดำเนินการและสามารถแก้ไขได้ถูกต้อง

#### Acceptance Criteria

1. WHEN MoveFolder() หรือ MoveFiles() สำเร็จ, THE FolderForm SHALL แสดง MessageBox แจ้งผลสำเร็จพร้อมระบุชื่อ Folder หรือจำนวนไฟล์ที่ย้ายและชื่อ DestinationFolder
2. WHEN FolderService หรือ FileService โยน InvalidOperationException, THE FolderForm SHALL แสดง MessageBox ที่แสดง message จาก exception โดยไม่ให้ exception หลุดถึง user โดยไม่มี message
3. WHEN FolderService หรือ FileService โยน UnauthorizedAccessException, THE FolderForm SHALL แสดง MessageBox แจ้งว่าไม่มีสิทธิ์ดำเนินการ
4. IF เกิด exception ที่ไม่คาดคิดระหว่าง MoveFolder() หรือ MoveFiles(), THEN THE FolderForm SHALL แสดง MessageBox แจ้ง error และ log ผ่าน LoggingService โดยไม่ทำให้ FolderForm crash
5. WHEN การย้ายมีจำนวน Descendant มากกว่า 50 node, THE FolderForm SHALL แสดง progress indicator ระหว่างดำเนินการ

---

### Requirement 11: Correctness Properties สำหรับ Property-Based Testing

**User Story:** As a developer, I want มี correctness properties ที่ครอบคลุมพฤติกรรมของ Move feature, so that สามารถใช้ property-based testing ตรวจสอบความถูกต้องของ implementation ได้อย่างครอบคลุม

#### Acceptance Criteria

1. FOR ALL valid (sourceFolderId, destinationParentFolderId) pairs ที่ไม่มี circular reference และ depth ไม่เกิน MaxFolderDepth, หลังจาก MoveFolder() สำเร็จ THE FolderService SHALL ทำให้ GetById(sourceFolderId).ParentFolderID เท่ากับ destinationParentFolderId (move correctness invariant)

2. FOR ALL SourceFolder ที่มี Descendant, หลังจาก MoveFolder() สำเร็จ THE FolderService SHALL ทำให้ FolderDepth ของทุก Descendant เพิ่มขึ้นหรือลดลงเท่ากับ (FolderDepth ของ DestinationFolder + 1 - FolderDepth ของ SourceFolder เดิม) (depth consistency invariant)

3. FOR ALL destinationParentFolderId ที่เป็น Descendant ของ sourceFolderId หรือเท่ากับ sourceFolderId, THE FolderService SHALL โยน InvalidOperationException เสมอ (circular reference rejection property)

4. FOR ALL (sourceFolderId, destinationParentFolderId) ที่ projected depth เกิน MaxFolderDepth, THE FolderService SHALL โยน InvalidOperationException เสมอ (depth limit rejection property)

5. FOR ALL SourceFolder ที่มีชื่อเหมือนกับ child ที่ active ของ DestinationFolder (case-insensitive), THE FolderService SHALL โยน InvalidOperationException เสมอ (duplicate name rejection property)

6. FOR ALL fileIds ที่ valid และ destinationFolderId ที่ valid, หลังจาก MoveFiles() สำเร็จ THE FileService SHALL ทำให้ GetById(fileId).FolderID เท่ากับ destinationFolderId สำหรับทุก fileId (file move correctness invariant)

7. FOR ALL การเรียก MoveFolder() หรือ MoveFiles() ที่สำเร็จ, THE AuditService SHALL ถูกเรียกอย่างน้อย 1 ครั้งพร้อม Action ที่ถูกต้อง (audit completeness property)

8. FOR ALL ผู้ใช้งานที่มี AppRole < 2, THE FolderService SHALL โยน UnauthorizedAccessException เสมอเมื่อเรียก MoveFolder() (role-based access control property)
