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
        /// คืนรายการแถวสำหรับ grid: คอลัมน์เป้าหมายจาก KMS.t_KBArticle (ไม่รวม CategoryID ซึ่งระบบหาเองจาก ArticleCode ตอน import)
        /// + แถวเสมือน Attachment และ LayoutType + คำอธิบาย + mapping ที่บันทึกไว้แล้ว (ถ้ามี templateId)
        /// — ถ้า templateId เป็น null คือ Template ใหม่ (mapping ว่างทั้งหมด); Template เก่าที่ผูกกับ 1 หมวดจะได้ค่า LayoutType
        /// derive ให้อัตโนมัติจากหมวดเดิม
        /// </summary>
        IList<KBImportTemplateColumnRow> GetTemplateColumnGrid(int? templateId);

        /// <summary>ตัวเลือกประเภทไฟล์ของ Template (กฎหมาย / ไม่ใช่กฎหมาย) สำหรับ dropdown</summary>
        IList<KBImportLayoutOption> GetLayoutOptions();

        /// <summary>รายการค่า ArticleType คงที่ (1-5) สำหรับ dropdown</summary>
        IList<KBArticleTypeOption> GetArticleTypeOptions();

        /// <summary>
        /// ตรวจว่า mapping ของ Template ครบและถูกต้อง (ชุดเดียวกับที่ใช้ตอนบันทึก Template): ต้องเลือกประเภทไฟล์และประเภทบทความ,
        /// map ArticleCode, Template ที่ไม่ใช่กฎหมายต้อง map Title/ContentHTML/Attachment, รูปแบบ {X} ถูกต้อง, ไม่ map คอลัมน์ซ้ำ
        /// คืนรายการปัญหา (ว่าง = ใช้งานได้)
        /// </summary>
        IList<string> GetTemplateProblems(IList<KBImportTemplateColumnRow> rows);

        /// <summary>เพิ่ม/แก้ไข Template พร้อม mapping ทั้งหมด คืน TemplateID</summary>
        int SaveTemplate(KBImportTemplate template, IList<KBImportTemplateColumnRow> rows, UserContext user);

        /// <summary>Soft delete — ตั้ง IsActive = 0</summary>
        void DeleteTemplate(int templateId, UserContext user);

        /// <summary>เปิดใช้งานกลับ — ตั้ง IsActive = 1</summary>
        void ActivateTemplate(int templateId, UserContext user);
    }
}
