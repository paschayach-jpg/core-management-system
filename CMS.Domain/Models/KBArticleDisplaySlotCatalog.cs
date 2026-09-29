using System.Collections.Generic;

namespace CMS.Domain.Models
{
    /// <summary>
    /// รายการ "slot" คงที่ที่ Article Display Profile config ได้ — ไม่ใช่ทุกคอลัมน์ของ KMS.t_KBArticle
    /// (อันนั้นเป็นของ Excel import mapping คนละเรื่อง) แต่เป็นแค่ 7 จุดที่มี header/caption แสดงผลอยู่ใน
    /// KBMainForm.dgvArticles และ ArticleCardControl วันนี้ — label/description เหล่านี้ = ค่าเดิม
    /// hardcode ที่มีอยู่ตอนนี้ ใช้เป็น fallback เมื่อหมวดไม่ได้ผูก Display Profile ไว้ (หรือ slot ไม่ได้
    /// ถูก config ไว้ใน profile ที่ผูกอยู่)
    /// </summary>
    public static class KBArticleDisplaySlotCatalog
    {
        public const string Title          = "Title";
        public const string TitleCategory  = "TitleCategory";
        public const string ContentHTML    = "ContentHTML";
        public const string PublishedDate  = "PublishedDate";
        public const string EffectiveDate  = "EffectiveDate";
        public const string ExpiryDate     = "ExpiryDate";
        public const string LinkedEntities = "LinkedEntities";

        private sealed class SlotInfo
        {
            public string DefaultLabel { get; set; }
            public bool   DefaultVisible { get; set; }
            public string Description { get; set; }
        }

        private static readonly Dictionary<string, SlotInfo> Slots =
            new Dictionary<string, SlotInfo>(System.StringComparer.OrdinalIgnoreCase)
        {
            { Title,          new SlotInfo { DefaultLabel = "อ้างอิง",              DefaultVisible = true,  Description = "หัวเรื่อง/ชื่ออ้างอิงหลักของบทความ — คอลัมน์ colTitle ใน grid" } },
            { TitleCategory,  new SlotInfo { DefaultLabel = "",                    DefaultVisible = true,  Description = "เลขมาตรา/รหัสอ้างอิงย่อย — คอลัมน์ colCode ใน grid, prefix ของ lblTitleCategory ใน card" } },
            { ContentHTML,    new SlotInfo { DefaultLabel = "สาระ",                DefaultVisible = true,  Description = "เนื้อหาบทความแบบย่อ — คอลัมน์ colContentHtml ใน grid, lblContent ใน card" } },
            { PublishedDate,  new SlotInfo { DefaultLabel = "วันที่เผยแพร่",        DefaultVisible = true,  Description = "วันที่เผยแพร่บทความ — แสดงเฉพาะใน card (label2 + lblPublishedDate)" } },
            { EffectiveDate,  new SlotInfo { DefaultLabel = "วันที่มีผลบังคับใช้",  DefaultVisible = true,  Description = "วันที่มีผลบังคับใช้ — แสดงเฉพาะใน grid (colEffectiveDate)" } },
            { ExpiryDate,     new SlotInfo { DefaultLabel = "วันที่ยกเลิก",         DefaultVisible = true,  Description = "วันที่ยกเลิก/สิ้นสุดการบังคับใช้ — คอลัมน์ colExpiryDate ใน grid, label3 ใน card" } },
            { LinkedEntities, new SlotInfo { DefaultLabel = "Org./Div./Dept.",     DefaultVisible = true,  Description = "หน่วยงาน/กอง/แผนกที่เกี่ยวข้อง — คอลัมน์ colOrgDivDept ใน grid, label4 ใน card" } },
        };

        /// <summary>คืนรายชื่อ slot key ทั้งหมด ตามลำดับที่ประกาศไว้ (ใช้สร้างแถวเริ่มต้นของ grid หน้า config)</summary>
        public static IList<string> GetSlotKeys()
        {
            return new List<string>(Slots.Keys);
        }

        public static string GetDefaultLabel(string slotKey)
        {
            SlotInfo info;
            return Slots.TryGetValue(slotKey, out info) ? info.DefaultLabel : string.Empty;
        }

        public static bool GetDefaultVisible(string slotKey)
        {
            SlotInfo info;
            return !Slots.TryGetValue(slotKey, out info) || info.DefaultVisible;
        }

        public static string GetDescription(string slotKey)
        {
            SlotInfo info;
            return Slots.TryGetValue(slotKey, out info) ? info.Description : string.Empty;
        }

        /// <summary>คืนค่า default ของทุก slot พร้อมกัน (key = slot key) — ใช้เป็น baseline ก่อน merge ด้วยค่าที่ profile config ไว้</summary>
        public static Dictionary<string, KBColumnDisplaySetting> GetAllDefaults()
        {
            var result = new Dictionary<string, KBColumnDisplaySetting>(System.StringComparer.OrdinalIgnoreCase);
            foreach (var kvp in Slots)
                result[kvp.Key] = new KBColumnDisplaySetting(kvp.Value.DefaultLabel, kvp.Value.DefaultVisible);
            return result;
        }
    }
}
