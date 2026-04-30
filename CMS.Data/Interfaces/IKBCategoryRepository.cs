using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IKBCategoryRepository
    {
        /// <summary>ดึง Category ทั้งหมด (รวม inactive) สำหรับหน้า Admin</summary>
        IEnumerable<KBCategory> GetAll();

        /// <summary>ดึง Category ที่ Active เท่านั้น (สำหรับ dropdown)</summary>
        IEnumerable<KBCategory> GetAllActive();

        KBCategory GetById(int categoryId);

        /// <summary>
        /// Returns the KBCategory whose CategoryCode matches <paramref name="categoryCode"/>,
        /// or null if no such record exists.
        /// Used by KBImportProcessor to detect duplicate categories before insert.
        /// </summary>
        KBCategory GetByCode(string categoryCode);

        /// <summary>เพิ่ม Category ใหม่ คืน CategoryID ที่สร้าง</summary>
        int Insert(KBCategory category);

        void Update(KBCategory category);

        /// <summary>Soft delete — ตั้ง IsActive = 0</summary>
        void SoftDelete(int categoryId);
    }
}
