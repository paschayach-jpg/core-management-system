-- =============================================================================
-- MOD-006: เพิ่มคอลัมน์ ImportTypeDigit ให้ KMS.m_KBCategory (non-destructive, รันซ้ำได้)
--
-- เดิมตารางตัวเลข → หมวดประเภทเอกสารที่ไม่ใช่กฎหมาย (1=AN 2=OR 3=SO 4=RG 5=CI 6=MR) hardcode อยู่ใน
-- KBImportLayoutTypeCatalog — เพิ่มประเภทใหม่ต้องแก้โค้ดและ release ใหม่ ตอนนี้ master อยู่ที่หมวดประเภทเอง:
-- ตัวที่ 23 ของ Code รวม (ArticleCode) ตอน import ไฟล์ที่ไม่ใช่กฎหมาย = ImportTypeDigit ของหมวดประเภทนั้น
-- เพิ่มประเภทใหม่ = สร้างหมวด แล้วตั้ง "รหัสประเภทเอกสาร" ที่หน้าหมวดหมู่ ไม่ต้องแก้โค้ด
--   1) เพิ่มคอลัมน์ ImportTypeDigit CHAR(1) NULL (ถ้ายังไม่มี) — NULL = ไม่ใช่หมวดประเภทเอกสาร (หมวดส่วนใหญ่)
--   2) จำกัดค่าเป็นตัวเลข 1-9 (CHECK) และห้ามซ้ำกันทั้งตาราง (filtered UNIQUE index เฉพาะแถวที่ไม่ NULL)
--   3) ตั้งค่าเริ่มต้นให้หมวดประเภทเดิมตามตารางที่เคย hardcode: AN=1, OR=2, SO=3, RG=4, CI=5, MR=6
--      เฉพาะหมวดที่ยังเป็น NULL และรหัสนั้นยังไม่มีหมวดอื่นใช้ — จึงไม่ทับรหัสที่ตั้งไปแล้ว และรันซ้ำได้ปลอดภัย
--      หมวดที่ยังไม่มีในระบบ (เช่น ยังไม่ได้สร้าง MR) ไม่ถูกสร้างให้ — สร้างแล้วตั้งรหัสที่หน้าหมวดหมู่
-- ไม่ลบหรือแก้ข้อมูลอื่นใด — รันก่อนใช้โปรแกรมเวอร์ชันที่อ่านคอลัมน์นี้ (ไม่งั้นหน้าหมวดหมู่/นำเข้า Excel จะ error)
-- ต้องการเปลี่ยนรหัสทีหลัง: แก้ที่หน้าหมวดหมู่ หรือ UPDATE คอลัมน์นี้ตรงๆ (ค่าต้องไม่ซ้ำ)
-- =============================================================================

SET QUOTED_IDENTIFIER ON;
SET ANSI_NULLS ON;
GO

IF OBJECT_ID('KMS.m_KBCategory', 'U') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID('KMS.m_KBCategory') AND name = 'ImportTypeDigit'
   )
BEGIN
    ALTER TABLE KMS.m_KBCategory ADD ImportTypeDigit CHAR(1) NULL;
END
GO

IF COL_LENGTH('KMS.m_KBCategory', 'ImportTypeDigit') IS NOT NULL
BEGIN
    IF NOT EXISTS (
        SELECT 1 FROM sys.check_constraints
        WHERE name = 'CK_KMS_KBCat_ImportTypeDigit' AND parent_object_id = OBJECT_ID('KMS.m_KBCategory')
    )
    BEGIN
        ALTER TABLE KMS.m_KBCategory
            ADD CONSTRAINT CK_KMS_KBCat_ImportTypeDigit CHECK (ImportTypeDigit IS NULL OR ImportTypeDigit LIKE '[1-9]');
    END

    IF NOT EXISTS (
        SELECT 1 FROM sys.indexes
        WHERE name = 'UX_KMS_KBCat_ImportTypeDigit' AND object_id = OBJECT_ID('KMS.m_KBCategory')
    )
    BEGIN
        CREATE UNIQUE NONCLUSTERED INDEX UX_KMS_KBCat_ImportTypeDigit
            ON KMS.m_KBCategory (ImportTypeDigit) WHERE ImportTypeDigit IS NOT NULL;
    END
END
GO

IF COL_LENGTH('KMS.m_KBCategory', 'ImportTypeDigit') IS NOT NULL
BEGIN
    DECLARE @Types TABLE (Digit CHAR(1) NOT NULL, TypeCode VARCHAR(20) NOT NULL);
    INSERT INTO @Types (Digit, TypeCode) VALUES
        ('1', 'AN'), ('2', 'OR'), ('3', 'SO'), ('4', 'RG'), ('5', 'CI'), ('6', 'MR');

    UPDATE c
    SET    c.ImportTypeDigit = t.Digit
    FROM   KMS.m_KBCategory c
    JOIN   @Types t ON t.TypeCode = UPPER(LTRIM(RTRIM(c.CategoryCode)))
    WHERE  c.ImportTypeDigit IS NULL
      AND  NOT EXISTS (SELECT 1 FROM KMS.m_KBCategory x WHERE x.ImportTypeDigit = t.Digit);

    PRINT N'ตั้งค่าเริ่มต้น ImportTypeDigit แล้ว ' + CAST(@@ROWCOUNT AS NVARCHAR(20)) + N' หมวด';

    DECLARE @Missing NVARCHAR(200) = N'';
    SELECT @Missing = @Missing + t.TypeCode + N' '
    FROM   @Types t
    WHERE  NOT EXISTS (SELECT 1 FROM KMS.m_KBCategory c WHERE UPPER(LTRIM(RTRIM(c.CategoryCode))) = t.TypeCode);

    IF LEN(@Missing) > 0
        PRINT N'ยังไม่มีหมวดรหัส: ' + @Missing + N'— สร้างหมวดแล้วตั้ง "รหัสประเภทเอกสาร" ที่หน้าหมวดหมู่';
END
GO
