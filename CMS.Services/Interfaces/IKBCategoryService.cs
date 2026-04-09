using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Service interface สำหรับจัดการ Category ของ Knowledge Base (MOD-006)
    /// </summary>
    public interface IKBCategoryService
    {
        /// <summary>ดึง Category ทั้งหมด (รวม inactive) — สำหรับหน้า Admin</summary>
        IEnumerable<KBCategory> GetAll();

        /// <summary>ดึง Category ที่ Active เท่านั้น — สำหรับ dropdown/selector</summary>
        IEnumerable<KBCategory> GetAllActive();

        /// <summary>ดึง Category ตาม ID</summary>
        KBCategory GetById(int categoryId);

        /// <summary>เพิ่ม Category ใหม่ คืน CategoryID ที่สร้าง</summary>
        int Create(KBCategory category);

        /// <summary>แก้ไข Category ที่มีอยู่</summary>
        void Update(KBCategory category);

        /// <summary>Soft delete — ตั้ง IsActive = 0 (ไม่ลบข้อมูลจริง)</summary>
        void Deactivate(int categoryId);
    }
}
