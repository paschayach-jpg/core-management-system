# แก้ไข Error: Could not find stored procedure 'storage.sp_GetFolderByID'

## สาเหตุของ Error

Application พยายามเรียกใช้ stored procedure `storage.sp_GetFolderByID` แต่ stored procedure นี้ยังไม่ได้ถูกสร้างหรืออัปเดตในฐานข้อมูล

## วิธีแก้ไข

### ขั้นตอนที่ 1: รัน SQL Script 070

1. เปิด SQL Server Management Studio (SSMS)
2. เชื่อมต่อกับ database `SCS_CMS`
3. เปิดไฟล์ `CMS.Data/Scripts/070_Add_UploadedByName_To_FileDocument_SPs.sql`
4. กด F5 หรือคลิก Execute เพื่อรัน script

Script นี้จะสร้าง/อัปเดต stored procedures ต่อไปนี้:
- `storage.sp_GetFileDocumentsByFolder`
- `storage.sp_GetAllFileDocumentsGroupedByFolder`
- `storage.sp_GetFileDocumentByID`
- `storage.sp_GetFileDocumentsByOrgUnit`
- `storage.sp_GetFolderByID` ← นี่คือตัวที่ขาดหายไป
- `storage.sp_GetAllFolders`

### ขั้นตอนที่ 2: ตรวจสอบว่า Stored Procedure ถูกสร้างแล้ว

รันคำสั่ง SQL นี้เพื่อตรวจสอบ:

```sql
USE [SCS_CMS];
GO

-- ตรวจสอบว่า stored procedure มีอยู่หรือไม่
SELECT 
    s.name AS SchemaName,
    p.name AS ProcedureName,
    p.create_date,
    p.modify_date
FROM sys.procedures p
INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
WHERE s.name = 'storage' 
  AND p.name IN (
    'sp_GetFolderByID',
    'sp_GetAllFolders',
    'sp_GetFileDocumentsByFolder',
    'sp_GetAllFileDocumentsGroupedByFolder',
    'sp_GetFileDocumentByID',
    'sp_GetFileDocumentsByOrgUnit'
  )
ORDER BY p.name;
```

ควรจะเห็น stored procedures ทั้ง 6 ตัว

### ขั้นตอนที่ 3: ทดสอบ Stored Procedure

```sql
USE [SCS_CMS];
GO

-- ทดสอบเรียกใช้ sp_GetFolderByID
EXEC storage.sp_GetFolderByID @FolderID = 1;
```

ควรจะได้ผลลัพธ์ที่มีคอลัมน์ `CreatedByName` ด้วย

### ขั้นตอนที่ 4: รัน Application อีกครั้ง

หลังจากรัน SQL script แล้ว:
1. ปิด application (ถ้าเปิดอยู่)
2. เปิด application ใหม่
3. ลองเปิด Folder Explorer อีกครั้ง

Error ควรจะหายไป และคอลัมน์ "โดย" จะแสดงชื่อผู้ใช้แทนตัวเลข UserID

## หมายเหตุ

- ไม่จำเป็นต้อง rebuild application เพราะเราแก้ไขแค่ stored procedures เท่านั้น
- ถ้ายังมี error ให้ตรวจสอบว่า connection string ชี้ไปที่ database ที่ถูกต้อง
- ถ้า stored procedure ยังไม่มี ให้ตรวจสอบว่ารัน script 070 สำเร็จหรือไม่ (ดูที่ Messages tab ใน SSMS)

## การตรวจสอบเพิ่มเติม

ถ้ายังมีปัญหา ให้ตรวจสอบว่า stored procedures อื่นๆ ที่เกี่ยวข้องมีอยู่หรือไม่:

```sql
USE [SCS_CMS];
GO

-- ดู stored procedures ทั้งหมดใน schema storage
SELECT 
    p.name AS ProcedureName,
    p.create_date,
    p.modify_date
FROM sys.procedures p
INNER JOIN sys.schemas s ON s.schema_id = p.schema_id
WHERE s.name = 'storage'
ORDER BY p.name;
```
