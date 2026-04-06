# Feature: แสดงชื่อผู้ Upload File และผู้สร้าง Folder

## สรุปการเปลี่ยนแปลง

ปัญหาเดิม: 
1. ในหน้า UI คอลัมน์ "โดย" แสดงเป็นตัวเลข UserID แทนที่จะเป็นชื่อผู้ใช้ (สำหรับไฟล์)
2. คอลัมน์ "โดย" สำหรับ folder แสดงเป็น "—" แทนที่จะแสดงชื่อผู้สร้าง

การแก้ไข: 
1. เพิ่มการ JOIN กับตาราง `core.UserAccount` ใน stored procedures เพื่อดึงชื่อผู้ใช้มาแสดง
2. แก้ไข UI ให้แสดง `folder.CreatedByName` แทน "—"

## ไฟล์ที่แก้ไข

### 1. Database (SQL Scripts)
- **CMS.Data/Scripts/070_Add_UploadedByName_To_FileDocument_SPs.sql** (ไฟล์ใหม่)
  - แก้ไข `sp_GetFileDocumentsByFolder` - เพิ่ม `UploadedByName`
  - แก้ไข `sp_GetAllFileDocumentsGroupedByFolder` - เพิ่ม `UploadedByName`
  - แก้ไข `sp_GetFileDocumentByID` - เพิ่ม `UploadedByName`
  - แก้ไข `sp_GetFileDocumentsByOrgUnit` - เพิ่ม `UploadedByName`
  - แก้ไข `sp_GetFolderByID` - เพิ่ม `CreatedByName`
  - แก้ไข `sp_GetAllFolders` - เพิ่ม `CreatedByName`

### 2. Domain Models
- **CMS.Domain/Models/FileDocument.cs**
  - มีฟิลด์ `UploadedByName` อยู่แล้ว (ไม่ต้องแก้)

- **CMS.Domain/Models/Folder.cs**
  - เพิ่มฟิลด์ `CreatedByName` (transient property)

### 3. Data Repositories
- **CMS.Data/Repositories/FileDocumentRepository.cs**
  - อัปเดต `MapFileDocument()` ให้อ่าน `UploadedByName` จาก database

- **CMS.Data/Repositories/FolderRepository.cs**
  - อัปเดต `MapFolder()` ให้อ่าน `CreatedByName` จาก database

### 4. Presentation Layer
- **CMS.Presentation/Modules/ImportFile/FolderForm.cs**
  - แก้ไขการแสดง folder ในคอลัมน์ "โดย" จาก `"—"` เป็น `folder.CreatedByName ?? "—"`
  - โค้ดเดิมสำหรับไฟล์แสดง `file.UploadedByName` อยู่แล้ว (ไม่ต้องแก้)

## ขั้นตอนการ Deploy

### 1. รัน SQL Script
```sql
-- เปิด SQL Server Management Studio
-- เชื่อมต่อกับ database SCS_CMS
-- รันไฟล์ CMS.Data/Scripts/070_Add_UploadedByName_To_FileDocument_SPs.sql
```

### 2. Build และ Deploy Application
```bash
# Build solution
msbuild CMS.sln /p:Configuration=Release

# Deploy ไฟล์ที่เปลี่ยนแปลง:
# - CMS.Domain.dll
# - CMS.Data.dll
# - CMS.Presentation.exe
```

## ผลลัพธ์ที่คาดหวัง

### ก่อนแก้ไข
- ไฟล์: คอลัมน์ "โดย" แสดง `1`, `2`, `3` (UserID)
- Folder: คอลัมน์ "โดย" แสดง `—`

### หลังแก้ไข
- ไฟล์: คอลัมน์ "โดย" แสดง `สมชาย ใจดี`, `สมหญิง รักงาน` (DisplayName)
- Folder: คอลัมน์ "โดย" แสดง `ผู้จัดการ ทีม`, `หัวหน้าแผนก` (DisplayName)

## หมายเหตุ

- ถ้า `DisplayName` เป็น NULL ระบบจะใช้ `ADUserName` แทน
- ถ้าทั้ง 2 ฟิลด์เป็น NULL จะแสดงเป็น "—"
- การเปลี่ยนแปลงนี้ backward compatible - ถ้า stored procedure เก่ายังไม่ได้อัปเดต โค้ดจะไม่ error (แค่ไม่แสดงชื่อ)

## การทดสอบ

1. รัน SQL script 070
2. Build และรัน application
3. เปิดหน้า Folder Explorer
4. ตรวจสอบว่าคอลัมน์ "โดย" แสดงชื่อผู้ใช้สำหรับทั้งไฟล์และ folder
5. ทดสอบ upload file ใหม่และตรวจสอบว่าแสดงชื่อถูกต้อง
6. ทดสอบสร้าง folder ใหม่และตรวจสอบว่าแสดงชื่อผู้สร้างถูกต้อง
