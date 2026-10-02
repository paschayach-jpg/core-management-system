-- =============================================================================
-- MOD-006: เพิ่มคอลัมน์ SlotDescription ให้ KMS.m_KBArticleDisplayProfileColumn (non-destructive, รันซ้ำได้)
--
-- เดิมคำอธิบายของแต่ละ slot (คอลัมน์ "จุดที่แสดงผล" ในหน้า config Article Display Profile) hardcode อยู่ใน
-- KBArticleDisplaySlotCatalog แก้ไม่ได้ — ตอนนี้เก็บต่อ Profile ต่อ Slot ในตารางนี้ และผู้ใช้แก้เองได้ในหน้า config
--   1) เพิ่มคอลัมน์ SlotDescription NVARCHAR(500) NULL (ถ้ายังไม่มี)
--   2) ตั้งค่าเริ่มต้นให้ "ทุกแถวที่มีอยู่แล้ว" ตาม SlotKey (ข้อความเดียวกับที่เคย hardcode)
--      เฉพาะแถวที่ยังเป็น NULL — จึงไม่ทับคำอธิบายที่ผู้ใช้แก้ไปแล้ว และรันซ้ำได้ปลอดภัย
--      SlotKey ที่ไม่อยู่ในรายการด้านล่างไม่ถูกแตะ (NULL = โปรแกรมใช้ค่าเริ่มต้นจากโค้ดแทน)
-- ไม่ลบหรือแก้ข้อมูลอื่นใด — ต้องรัน 07_KMS_ArticleDisplayProfile_Schema.sql มาก่อน
-- ต้องการเปลี่ยนข้อความทีหลัง: แก้ในหน้า config ได้เลย หรือ UPDATE คอลัมน์นี้ตรงๆ
-- =============================================================================

IF OBJECT_ID('KMS.m_KBArticleDisplayProfileColumn', 'U') IS NOT NULL
   AND NOT EXISTS (
        SELECT 1 FROM sys.columns
        WHERE object_id = OBJECT_ID('KMS.m_KBArticleDisplayProfileColumn') AND name = 'SlotDescription'
   )
BEGIN
    ALTER TABLE KMS.m_KBArticleDisplayProfileColumn ADD SlotDescription NVARCHAR(500) NULL;
END
GO

IF COL_LENGTH('KMS.m_KBArticleDisplayProfileColumn', 'SlotDescription') IS NOT NULL
BEGIN
    UPDATE c
    SET    c.SlotDescription = d.Description
    FROM   KMS.m_KBArticleDisplayProfileColumn c
    JOIN   (VALUES
        (N'Title', N'หัวเรื่อง/ชื่ออ้างอิงหลักของบทความ — คอลัมน์ colTitle ใน grid'),
        (N'TitleCategory', N'เลขมาตรา/รหัสอ้างอิงย่อย — คอลัมน์ colCode ใน grid, prefix ของ lblTitleCategory ใน card'),
        (N'ContentHTML', N'เนื้อหาบทความแบบย่อ — คอลัมน์ colContentHtml ใน grid, lblContent ใน card'),
        (N'PublishedDate', N'วันที่เผยแพร่บทความ — แสดงเฉพาะใน card (label2 + lblPublishedDate)'),
        (N'EffectiveDate', N'วันที่มีผลบังคับใช้ — แสดงเฉพาะใน grid (colEffectiveDate)'),
        (N'ExpiryDate', N'วันที่ยกเลิก/สิ้นสุดการบังคับใช้ — คอลัมน์ colExpiryDate ใน grid, label3 ใน card'),
        (N'LinkedEntities', N'หน่วยงาน/กอง/แผนกที่เกี่ยวข้อง — คอลัมน์ colOrgDivDept ใน grid, label4 ใน card')
           ) AS d (SlotKey, Description) ON d.SlotKey = c.SlotKey
    WHERE  c.SlotDescription IS NULL;

    PRINT N'ตั้งค่าเริ่มต้น SlotDescription แล้ว ' + CAST(@@ROWCOUNT AS NVARCHAR(20)) + N' แถว';
END
GO
