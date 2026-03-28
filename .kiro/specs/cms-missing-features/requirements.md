# Requirements Document

## Introduction

Feature นี้ครอบคลุมการแก้ไขและเพิ่มฟีเจอร์ที่ยังขาดอยู่ใน CMS (Content Management System) ซึ่งเป็น WinForms C# .NET โดยแบ่งออกเป็น 3 กลุ่มตามความซับซ้อน ได้แก่ (1) Validation และ UI พื้นฐาน (2) Filter และ Quick Access และ (3) ฟีเจอร์ขั้นสูงอย่าง Copy Structure, File Preview และ Resume Upload

## Glossary

- **FolderService**: Service class ใน CMS.Services ที่รับผิดชอบ business logic ของ Folder ทั้งหมด
- **FolderForm**: WinForms Form หลักสำหรับจัดการโครงสร้าง Folder และแสดงรายการไฟล์
- **FileUploadForm**: WinForms Form สำหรับอัพโหลดไฟล์เข้าระบบ
- **FolderRepository**: Repository class ที่ติดต่อกับฐานข้อมูลสำหรับ Folder
- **contentsListView**: ListView control ใน FolderForm ที่แสดงรายการ Folder และไฟล์ภายใน Folder ที่เลือก
- **folderTreeView**: TreeView control ใน FolderForm ที่แสดงโครงสร้าง Folder แบบ hierarchy
- **txtFolderSearch**: TextBox control ใน FolderForm สำหรับค้นหา Folder ใน folderTreeView
- **FileDocument**: Domain model แทนไฟล์เอกสารที่เก็บในระบบ
- **Folder**: Domain model แทน Folder ในระบบ มี property FileCount และ FolderSizeBytes
- **UserContext**: Object ที่เก็บข้อมูล user ที่ login อยู่ รวมถึง AppRole (3=Manager, 4=Admin)
- **ApprovalRequest**: Domain model สำหรับ workflow การขออนุมัติ
- **QuickAccessFolder**: Domain model ใหม่สำหรับเก็บ Folder ที่ user กำหนดเป็น Favorite
- **UploadCheckpoint**: Domain model ใหม่สำหรับเก็บ state ของการอัพโหลดที่ยังไม่เสร็จ
- **FilePreviewPanel**: Panel control ใหม่ใน FolderForm สำหรับแสดง preview ของไฟล์ที่เลือก
- **InvalidFolderNameChars**: ชุดอักขระที่ห้ามใช้ในชื่อ Folder ได้แก่ `\ / : * ? " < > |`

---

## Requirements

### Requirement 1: Folder Name — ห้ามมีอักขระพิเศษ

**User Story:** As a ผู้ใช้งาน, I want ระบบตรวจสอบชื่อ Folder ก่อนบันทึก, so that ชื่อ Folder ไม่มีอักขระที่ทำให้เกิดปัญหาบน File System และฐานข้อมูล

#### Acceptance Criteria

1. WHEN ผู้ใช้งานสร้างหรือเปลี่ยนชื่อ Folder ด้วยชื่อที่มีอักขระใดอักขระหนึ่งใน InvalidFolderNameChars, THEN THE FolderService SHALL โยน ArgumentException พร้อม error message ที่ระบุอักขระที่ไม่อนุญาต
2. WHEN ผู้ใช้งานสร้างหรือเปลี่ยนชื่อ Folder ด้วยชื่อที่ไม่มีอักขระใน InvalidFolderNameChars, THE FolderService SHALL ดำเนินการต่อโดยไม่มี error
3. THE FolderService SHALL ตรวจสอบ InvalidFolderNameChars ทั้งใน CreateFolder() และ RenameFolder()
4. WHEN FolderForm แสดง error จาก FolderService เกี่ยวกับ InvalidFolderNameChars, THE FolderForm SHALL แสดง MessageBox ที่ระบุอักขระที่ไม่อนุญาตทั้งหมดให้ผู้ใช้งานทราบ

---

### Requirement 2: Folder Name — ห้ามซ้ำใน Parent เดียวกัน

**User Story:** As a ผู้ใช้งาน, I want ระบบป้องกันการสร้าง Folder ชื่อซ้ำ, so that โครงสร้าง Folder ไม่สับสนและสามารถระบุ Folder ได้ชัดเจน

#### Acceptance Criteria

1. WHEN ผู้ใช้งานสร้าง Folder ที่มีชื่อเหมือนกับ Folder อื่นที่มี ParentFolderID เดียวกัน (case-insensitive), THEN THE FolderService SHALL โยน InvalidOperationException พร้อม error message ที่ระบุชื่อที่ซ้ำ
2. WHEN ผู้ใช้งานเปลี่ยนชื่อ Folder เป็นชื่อที่เหมือนกับ Folder อื่นที่มี ParentFolderID เดียวกัน (case-insensitive), THEN THE FolderService SHALL โยน InvalidOperationException
3. THE FolderService SHALL ตรวจสอบ duplicate โดยเปรียบเทียบแบบ case-insensitive และ trim whitespace
4. THE FolderService SHALL ไม่นับ Folder ที่ IsDeleted = true เป็น duplicate
5. WHEN ผู้ใช้งานสร้าง Folder ที่ชื่อเหมือนกับ Folder ที่ถูกลบแล้ว (IsDeleted = true) ใน Parent เดียวกัน, THE FolderService SHALL อนุญาตให้สร้างได้

---

### Requirement 3: ค้นหา Folder ใน Tree

**User Story:** As a ผู้ใช้งาน, I want พิมพ์ค้นหาชื่อ Folder ใน folderTreeView, so that สามารถหา Folder ที่ต้องการได้รวดเร็วโดยไม่ต้องเลื่อนดูทั้ง tree

#### Acceptance Criteria

1. WHEN ผู้ใช้งานพิมพ์ข้อความใน txtFolderSearch, THE FolderForm SHALL filter node ใน folderTreeView ให้แสดงเฉพาะ node ที่ชื่อ Folder มีข้อความที่พิมพ์เป็น substring (case-insensitive)
2. WHEN txtFolderSearch มีข้อความว่าง, THE FolderForm SHALL แสดง node ทั้งหมดใน folderTreeView เหมือนสถานะปกติ
3. WHEN folderTreeView แสดงผลการค้นหา, THE FolderForm SHALL แสดง parent node ของ node ที่ตรงกับการค้นหาด้วย เพื่อให้เห็น context ของ Folder
4. WHEN ผู้ใช้งานพิมพ์ข้อความที่ไม่ตรงกับ Folder ใดเลย, THE FolderForm SHALL แสดง node พิเศษที่มีข้อความ "(ไม่พบ Folder)" ใน folderTreeView
5. THE FolderForm SHALL wire event TextChanged ของ txtFolderSearch เพื่อ trigger การ filter ทุกครั้งที่ข้อความเปลี่ยน

---

### Requirement 4: Sort ตามคอลัมน์ใน File List

**User Story:** As a ผู้ใช้งาน, I want คลิกหัวคอลัมน์ใน contentsListView เพื่อ sort รายการ, so that สามารถจัดเรียงไฟล์และ Folder ตามชื่อ ขนาด หรือวันที่ได้ตามต้องการ

#### Acceptance Criteria

1. WHEN ผู้ใช้งานคลิกหัวคอลัมน์ใน contentsListView เป็นครั้งแรก, THE FolderForm SHALL sort รายการตามคอลัมน์นั้นในลำดับ ascending
2. WHEN ผู้ใช้งานคลิกหัวคอลัมน์เดิมซ้ำ, THE FolderForm SHALL สลับลำดับ sort ระหว่าง ascending และ descending
3. THE FolderForm SHALL แสดง sort indicator (▲ หรือ ▼) ที่หัวคอลัมน์ที่กำลัง sort อยู่
4. THE FolderForm SHALL sort Folder และ FileDocument แยกกัน โดย Folder แสดงก่อน FileDocument เสมอ
5. THE FolderForm SHALL wire event ColumnClick ของ contentsListView เพื่อ trigger การ sort

---

### Requirement 5: Folder ที่มีไฟล์ต้องขอ Admin Approve ก่อนลบ

**User Story:** As a Admin, I want ระบบป้องกันการลบ Folder ที่ยังมีไฟล์อยู่โดยไม่ผ่านการอนุมัติ, so that ไฟล์สำคัญไม่ถูกลบโดยไม่ตั้งใจ

#### Acceptance Criteria

1. WHEN ผู้ใช้งานที่มี AppRole < 4 (ไม่ใช่ Admin) เรียก DeleteFolder() บน Folder ที่มี FileCount > 0, THEN THE FolderService SHALL โยน InvalidOperationException พร้อม message ที่แจ้งว่าต้องขออนุมัติจาก Admin ก่อน
2. WHEN ผู้ใช้งานที่มี AppRole = 4 (Admin) เรียก DeleteFolder() บน Folder ที่มี FileCount > 0, THE FolderService SHALL ดำเนินการ soft delete ได้ทันทีโดยไม่ต้องขออนุมัติ
3. WHEN ผู้ใช้งานเรียก DeleteFolder() บน Folder ที่มี FileCount = 0, THE FolderService SHALL ดำเนินการ soft delete ได้ทันทีโดยไม่คำนึงถึง AppRole
4. WHEN FolderForm แสดง error เกี่ยวกับการลบ Folder ที่มีไฟล์, THE FolderForm SHALL แสดง dialog ที่แจ้งจำนวนไฟล์ใน Folder และแนะนำให้ติดต่อ Admin

---

### Requirement 6: Filter ตาม Type / Date / User ใน File List

**User Story:** As a ผู้ใช้งาน, I want กรองรายการไฟล์ใน contentsListView ตาม file type, วันที่อัพโหลด, หรือผู้อัพโหลด, so that สามารถหาไฟล์ที่ต้องการได้รวดเร็วเมื่อมีไฟล์จำนวนมาก

#### Acceptance Criteria

1. THE FolderForm SHALL แสดง filter panel ที่มี control สำหรับกรองตาม: (a) File Extension เช่น .pdf, .docx, (b) ช่วงวันที่อัพโหลด (DateFrom–DateTo), (c) ชื่อผู้อัพโหลด (UploadedByName)
2. WHEN ผู้ใช้งานเลือก filter condition และกดปุ่ม Apply, THE FolderForm SHALL แสดงเฉพาะ FileDocument ที่ตรงกับ filter ทุกเงื่อนไขที่ระบุ (AND logic)
3. WHEN ผู้ใช้งานกดปุ่ม Clear Filter, THE FolderForm SHALL แสดง FileDocument ทั้งหมดใน Folder ที่เลือกอยู่
4. WHEN filter ทำงานอยู่, THE FolderForm SHALL แสดง indicator ที่ชัดเจนว่ากำลัง filter อยู่ พร้อมจำนวนรายการที่แสดง
5. THE FolderForm SHALL ไม่ filter Folder (sub-folder) ออก — filter ใช้กับ FileDocument เท่านั้น

---

### Requirement 7: Quick Access / Favorite Folder

**User Story:** As a ผู้ใช้งาน, I want บันทึก Folder ที่ใช้บ่อยเป็น Favorite, so that สามารถเข้าถึง Folder นั้นได้รวดเร็วโดยไม่ต้องเลื่อนหา tree ทุกครั้ง

#### Acceptance Criteria

1. THE FolderForm SHALL แสดง Quick Access panel ที่แสดงรายการ Folder ที่ user กำหนดเป็น Favorite
2. WHEN ผู้ใช้งาน right-click บน Folder ใน folderTreeView และเลือก "เพิ่มใน Quick Access", THE FolderForm SHALL บันทึก QuickAccessFolder record ลงฐานข้อมูลสำหรับ user นั้น
3. WHEN ผู้ใช้งานคลิก Folder ใน Quick Access panel, THE FolderForm SHALL navigate ไปยัง Folder นั้นใน folderTreeView และโหลด contents ของ Folder นั้น
4. WHEN ผู้ใช้งาน right-click บน Folder ใน Quick Access panel และเลือก "ลบออกจาก Quick Access", THE FolderForm SHALL ลบ QuickAccessFolder record ออกจากฐานข้อมูล
5. THE System SHALL จัดเก็บ QuickAccessFolder ใน DB table ใหม่ที่มี column: UserID, FolderID, AddedDate
6. WHEN Folder ที่อยู่ใน Quick Access ถูกลบหรือ Archive, THE FolderForm SHALL แสดง Folder นั้นใน Quick Access ด้วย visual indicator ที่บ่งบอกว่า Folder ไม่พร้อมใช้งาน

---

### Requirement 8: Copy โครงสร้าง Folder (ไม่รวมไฟล์)

**User Story:** As a Manager หรือ Admin, I want คัดลอกโครงสร้าง Folder ทั้งหมดไปยัง parent ใหม่โดยไม่รวมไฟล์, so that สามารถสร้างโครงสร้าง Folder มาตรฐานซ้ำได้รวดเร็ว

#### Acceptance Criteria

1. WHEN ผู้ใช้งานที่มี AppRole >= 3 เรียก CopyFolderStructure() ด้วย sourceFolderId และ destinationParentFolderId, THE FolderService SHALL สร้าง Folder ใหม่ที่มีโครงสร้าง hierarchy เหมือน source Folder ทุกระดับ โดยไม่คัดลอก FileDocument ใดๆ
2. THE FolderService SHALL ตรวจสอบว่า depth ของ Folder ที่สร้างใหม่ไม่เกิน MaxFolderDepth (10) และโยน InvalidOperationException หากเกิน
3. WHEN ผู้ใช้งานที่มี AppRole < 3 เรียก CopyFolderStructure(), THEN THE FolderService SHALL โยน UnauthorizedAccessException
4. THE FolderService SHALL คัดลอก ConfidentialityLevel และ InheritPermission จาก source Folder ไปยัง Folder ที่สร้างใหม่ทุก node
5. THE FolderForm SHALL แสดง dialog สำหรับเลือก destination parent Folder ก่อนดำเนินการ copy
6. WHEN CopyFolderStructure() สำเร็จ, THE FolderForm SHALL reload folderTreeView และ select Folder ที่สร้างใหม่

---

### Requirement 9: File Preview

**User Story:** As a ผู้ใช้งาน, I want ดู preview ของไฟล์ที่เลือกใน FolderForm โดยไม่ต้องเปิดโปรแกรมอื่น, so that สามารถตรวจสอบเนื้อหาไฟล์ได้รวดเร็ว

#### Acceptance Criteria

1. WHEN ผู้ใช้งานเลือก FileDocument ใน contentsListView, THE FolderForm SHALL แสดง FilePreviewPanel ทางด้านขวาของ contentsListView
2. WHERE ไฟล์ที่เลือกมี extension เป็น .jpg, .png, หรือ .gif, THE FilePreviewPanel SHALL แสดง image ของไฟล์นั้นโดยใช้ PictureBox
3. WHERE ไฟล์ที่เลือกมี extension เป็น .pdf, THE FilePreviewPanel SHALL แสดง preview โดยใช้ WebBrowser control หรือ third-party PDF viewer component
4. WHERE ไฟล์ที่เลือกมี extension เป็น .doc, .docx, .xls, .xlsx, .ppt, หรือ .pptx, THE FilePreviewPanel SHALL แสดงข้อความ "ไม่รองรับ preview สำหรับไฟล์ประเภทนี้ กรุณาดาวน์โหลดเพื่อเปิดดู"
5. WHERE ไฟล์ที่เลือกมี extension ที่ไม่รองรับ preview, THE FilePreviewPanel SHALL แสดง icon ของประเภทไฟล์และ metadata (ชื่อ, ขนาด, วันที่อัพโหลด)
6. WHEN ผู้ใช้งานคลิกปุ่ม "ซ่อน Preview", THE FolderForm SHALL ซ่อน FilePreviewPanel และขยาย contentsListView ให้เต็มพื้นที่
7. IF ไม่สามารถโหลดไฟล์สำหรับ preview ได้ (เช่น network error), THEN THE FilePreviewPanel SHALL แสดงข้อความ error และ icon แจ้งเตือน โดยไม่ทำให้ FolderForm crash

---

### Requirement 10: Resume Upload

**User Story:** As a ผู้ใช้งาน, I want กลับมาอัพโหลดไฟล์ต่อจากจุดที่ค้างไว้ได้, so that ไม่ต้องเริ่มอัพโหลดไฟล์ขนาดใหญ่ใหม่ทั้งหมดเมื่อเกิดการขัดจังหวะ

#### Acceptance Criteria

1. WHEN FileUploadForm เริ่มอัพโหลดไฟล์, THE FileUploadForm SHALL บันทึก UploadCheckpoint ที่มี: FileName, FileSizeBytes, FolderID, BytesUploaded, CheckpointDate ลงใน local storage (เช่น temp file หรือ isolated storage)
2. WHEN FileUploadForm เปิดขึ้นมาและพบ UploadCheckpoint ที่ยังไม่เสร็จสำหรับ FolderID เดียวกัน, THE FileUploadForm SHALL แสดง dialog ถามผู้ใช้งานว่าต้องการ resume การอัพโหลดที่ค้างไว้หรือไม่
3. WHEN ผู้ใช้งานเลือก resume, THE FileUploadForm SHALL อ่าน UploadCheckpoint และเริ่มอัพโหลดต่อจาก BytesUploaded ที่บันทึกไว้
4. WHEN ผู้ใช้งานเลือกไม่ resume, THE FileUploadForm SHALL ลบ UploadCheckpoint ที่เกี่ยวข้องและเริ่มอัพโหลดใหม่ตั้งแต่ต้น
5. WHEN การอัพโหลดเสร็จสมบูรณ์, THE FileUploadForm SHALL ลบ UploadCheckpoint ที่เกี่ยวข้องออกจาก local storage
6. IF UploadCheckpoint มีอายุเกิน 24 ชั่วโมง, THEN THE FileUploadForm SHALL ลบ UploadCheckpoint นั้นโดยอัตโนมัติและไม่แสดง resume dialog
