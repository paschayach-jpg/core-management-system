using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IKBImportTemplateRepository
    {
        /// <summary>ดึง Template ทั้งหมด (รวม inactive) สำหรับหน้า Admin</summary>
        IEnumerable<KBImportTemplate> GetAll();

        KBImportTemplate GetById(int templateId);

        /// <summary>true เมื่อมีชื่อ Template นี้อยู่แล้ว (ไม่รวม excludeTemplateId เอง กรณีแก้ไข)</summary>
        bool ExistsByName(string templateName, int excludeTemplateId = 0);

        IList<KBImportTemplateColumnMapping> GetMappingsByTemplateId(int templateId);

        /// <summary>
        /// บันทึก Template (insert/update) พร้อม mapping ทั้งหมดในธุรกรรมเดียว
        /// (ลบ mapping เดิมทั้งหมดแล้ว insert ใหม่) คืน TemplateID
        /// </summary>
        int SaveTemplateWithMappings(KBImportTemplate template, IList<KBImportTemplateColumnMapping> mappings);

        /// <summary>Soft delete — ตั้ง IsActive = 0</summary>
        void Deactivate(int templateId, string updatedBy);
    }
}
