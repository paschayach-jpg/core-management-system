-- =============================================================================
-- MOD-006: เพิ่มคอลัมน์ LayoutType ให้ KMS.m_KBImportTemplate (non-destructive)
-- ใช้กับฐานข้อมูลที่มีตาราง KMS.m_KBImportTemplate อยู่แล้ว (รันแทนการรื้อสร้างใหม่จาก
-- 04_KMS_ImportTemplateConfig_Schema.sql ซึ่งจะ DROP ตารางและข้อมูล Template ที่มีอยู่ทิ้ง)
--
-- Template เดิมทุกแถวจะถูกตั้งเป็น 'Law' โดย DEFAULT ของคอลัมน์ใหม่นี้ — ถ้ามี Template ที่ใช้กับไฟล์
-- ประกาศ/คำสั่ง (LayoutType ควรเป็น 'Order') อยู่ก่อนแล้ว ต้องเข้าไปแก้ในหน้า config ให้ถูกต้องอีกครั้ง
-- หลังรันสคริปต์นี้
-- =============================================================================

IF NOT EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('KMS.m_KBImportTemplate') AND name = 'LayoutType'
)
BEGIN
    ALTER TABLE KMS.m_KBImportTemplate
        ADD LayoutType NVARCHAR(20) NOT NULL CONSTRAINT DF_KMS_ImpTpl_Layout DEFAULT('Law');
END
GO
