using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Service interface สำหรับจัดการ Excel Import Template ของ Knowledge Base (MOD-006)
    /// </summary>
    public interface IKBImportTemplateService
    {
        /// <summary>ดึง Template ทั้งหมด (รวม inactive) — สำหรับหน้า Admin</summary>
        IEnumerable<KBImportTemplate> GetAllTemplates();

        KBImportTemplate GetTemplateById(int templateId);

        /// <summary>
        /// คืนรายการแถวสำหรับ grid: รวมคอลัมน์เป้าหมายทั้งหมดจาก KMS.t_KBArticle + คำอธิบาย
        /// + mapping ที่บันทึกไว้แล้ว (ถ้ามี templateId) — ถ้า templateId เป็น null คือ Template ใหม่ (mapping ว่างทั้งหมด)
        /// </summary>
        IList<KBImportTemplateColumnRow> GetTemplateColumnGrid(int? templateId);

        /// <summary>รายการหมวดหมู่สำหรับ dropdown เลือก CategoryID (ไม่รวม CategoryCode = "LW")</summary>
        IEnumerable<KBCategory> GetCategoryOptions();

        /// <summary>รายการค่า ArticleType คงที่ (1-5) สำหรับ dropdown</summary>
        IList<KBArticleTypeOption> GetArticleTypeOptions();

        /// <summary>
        /// Derive LayoutType ("Law"/"NonLaw") จาก categoryId — ไล่ ParentCategoryID ขึ้นไปจนเจอหมวด
        /// "กฎหมาย" (CategoryCode = "LW") หรือไม่ ใช้แทนการเก็บ LayoutType แยกใน Template
        /// </summary>
        string GetEffectiveLayoutType(int categoryId);

        /// <summary>CategoryCode ของ category (ใช้เป็น prefix 3 ตัวแรกของ Code รวมในไฟล์กฎหมาย เช่น "PVI") — null ถ้าไม่พบ</summary>
        string GetTemplateCategoryCode(int categoryId);

        /// <summary>เพิ่ม/แก้ไข Template พร้อม mapping ทั้งหมด คืน TemplateID</summary>
        int SaveTemplate(KBImportTemplate template, IList<KBImportTemplateColumnRow> rows, UserContext user);

        /// <summary>Soft delete — ตั้ง IsActive = 0</summary>
        void DeleteTemplate(int templateId, UserContext user);

        /// <summary>เปิดใช้งานกลับ — ตั้ง IsActive = 1</summary>
        void ActivateTemplate(int templateId, UserContext user);
    }
}
