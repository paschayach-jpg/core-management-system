using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IKBArticleDisplayProfileRepository
    {
        /// <summary>ดึง Profile ทั้งหมด (รวม inactive) สำหรับหน้า Admin</summary>
        IEnumerable<KBArticleDisplayProfile> GetAll();

        KBArticleDisplayProfile GetById(int profileId);

        /// <summary>true เมื่อมีชื่อ Profile นี้อยู่แล้ว (ไม่รวม excludeProfileId เอง กรณีแก้ไข)</summary>
        bool ExistsByName(string profileName, int excludeProfileId = 0);

        IList<KBArticleDisplayProfileColumn> GetColumnsByProfileId(int profileId);

        /// <summary>
        /// บันทึก Profile (insert/update) พร้อม column ทั้งหมดในธุรกรรมเดียว
        /// (ลบ column เดิมทั้งหมดแล้ว insert ใหม่) คืน ProfileID
        /// </summary>
        int SaveProfileWithColumns(KBArticleDisplayProfile profile, IList<KBArticleDisplayProfileColumn> columns);

        /// <summary>Soft delete — ตั้ง IsActive = 0</summary>
        void Deactivate(int profileId);
    }
}
