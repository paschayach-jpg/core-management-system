-- =============================================================================
-- MOD-006: ลบคอลัมน์ LayoutType ออกจาก KMS.m_KBImportTemplate
-- เหตุผล: LayoutType ("Law"/"Order") derive ได้จากคอลัมน์ CategoryID ที่ config ไว้อยู่แล้วใน
-- m_KBImportTemplateColumn — ไล่ ParentCategoryID ของ CategoryID นั้นขึ้นไปใน KMS.m_KBCategory
-- ถ้าเจอ CategoryCode = 'LW' คือ "กฎหมาย" ถ้าไม่เจอ (root, CategoryCode <> 'LW') คือ "บทความอื่นๆ"
-- (ดู KBImportTemplateService.GetEffectiveLayoutType) ทำให้ไม่ต้องเก็บ LayoutType แยกอีกคอลัมน์
-- ซึ่งเสี่ยงตั้งค่าขัดแย้งกับ CategoryID เอง
--
-- รันสคริปต์นี้แทนสคริปต์ 05_...AddLayoutType.sql เดิม (ย้อนกลับการเพิ่มคอลัมน์นั้น)
-- =============================================================================

IF EXISTS (
    SELECT 1 FROM sys.columns
    WHERE object_id = OBJECT_ID('KMS.m_KBImportTemplate') AND name = 'LayoutType'
)
BEGIN
    IF EXISTS (SELECT 1 FROM sys.default_constraints WHERE name = 'DF_KMS_ImpTpl_Layout')
        ALTER TABLE KMS.m_KBImportTemplate DROP CONSTRAINT DF_KMS_ImpTpl_Layout;

    ALTER TABLE KMS.m_KBImportTemplate DROP COLUMN LayoutType;
END
GO
