using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Service interface สำหรับจัดการ Article Display Profile ของ Knowledge Base (MOD-006) —
    /// กำหนด header label/การซ่อนแสดงคอลัมน์ของ dgvArticles/ArticleCardControl แบบ dynamic ตาม CategoryID
    /// </summary>
    public interface IKBArticleDisplayProfileService
    {
        /// <summary>ดึง Profile ทั้งหมด (รวม inactive) — สำหรับหน้า Admin</summary>
        IEnumerable<KBArticleDisplayProfile> GetAllProfiles();

        KBArticleDisplayProfile GetProfileById(int profileId);

        /// <summary>
        /// คืนรายการแถวสำหรับ grid: ทุก slot จาก KBArticleDisplaySlotCatalog + ค่าที่บันทึกไว้แล้ว
        /// (ถ้ามี profileId) — ถ้า profileId เป็น null คือ Profile ใหม่ (ใช้ค่า default ของแต่ละ slot ทั้งหมด)
        /// </summary>
        IList<KBArticleDisplayProfileColumnRow> GetProfileColumnGrid(int? profileId);

        /// <summary>เพิ่ม/แก้ไข Profile พร้อม column ทั้งหมด คืน ProfileID</summary>
        int SaveProfile(KBArticleDisplayProfile profile, IList<KBArticleDisplayProfileColumnRow> rows, UserContext user);

        /// <summary>Soft delete — ตั้ง IsActive = 0 (ไม่ยกเลิกการผูกกับ category ที่ใช้อยู่ — เจ้าของหน้า config ต้องจัดการเอง)</summary>
        void DeleteProfile(int profileId, UserContext user);

        /// <summary>รายชื่อ Category ที่ผูก DisplayProfileID ตรงกับ profileId นี้โดยตรง (ไม่รวมที่ inherit มาจาก ancestor)</summary>
        IList<KBCategory> GetCategoriesUsingProfile(int profileId);

        /// <summary>ตั้ง/ยกเลิกการผูก Profile ให้กับ category ที่ระบุ (profileId เป็น null = ยกเลิก)</summary>
        void AssignProfileToCategory(int categoryId, int? profileId, UserContext user);

        /// <summary>
        /// คืนค่า header label + การซ่อนแสดงที่ resolve แล้วของทุก slot สำหรับ categoryId — ไล่
        /// ParentCategoryID ขึ้นไปหา ancestor-หรือตัวเอง ที่ใกล้ที่สุดซึ่งมี DisplayProfileID ตั้งไว้
        /// ไม่เจอเลยตลอดสาย (หรือไม่พบ categoryId) จะคืนค่า default ของ KBArticleDisplaySlotCatalog ทั้งหมด
        /// (ไม่ throw — เป็นแค่เรื่องการแสดงผล ไม่ใช่ path ที่ critical เท่าการ import)
        /// </summary>
        IDictionary<string, KBColumnDisplaySetting> GetEffectiveDisplaySettings(int categoryId);
    }
}
