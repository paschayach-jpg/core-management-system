using System.Collections.Generic;
using System.Text;

namespace CMS.Domain.Models
{
    /// <summary>
    /// คำอธิบายความหมายของแต่ละคอลัมน์ใน KMS.t_KBArticle (ภาษาไทย)
    /// ใช้ประกอบหน้า config Excel Import Template — ไม่เก็บลง DB เพราะเป็น metadata คงที่ของคอลัมน์
    /// อ้างอิงจาก MOD-006\CMS_DataDictionary_Full2.md และคอมเมนต์ใน KBArticle.cs
    /// </summary>
    public static class KBArticleColumnCatalog
    {
        /// <summary>
        /// ชื่อแถวเสมือนใน mapping ของ Template สำหรับคอลัมน์ Excel ที่เก็บชื่อไฟล์แนบ (+ hyperlink) ของไฟล์ประกาศ/คำสั่ง —
        /// ไม่ใช่คอลัมน์จริงของ t_KBArticle (ไฟล์แนบอยู่ใน DMS) จึงถูกต่อท้ายรายการคอลัมน์โดย KBImportTemplateService
        /// </summary>
        public const string AttachmentTargetColumn = "Attachment";

        private static readonly Dictionary<string, string> Descriptions = new Dictionary<string, string>(System.StringComparer.OrdinalIgnoreCase)
        {
            { "ArticleCode",         "Code รวม 27 หลัก (กฎหมายลงท้าย 00000 / ประกาศ-คำสั่งตัวที่ 23 = ประเภท 1-6) ใช้ตรวจประเภทไฟล์และหากฎหมายแม่ (22 ตัวแรก)" },
            { AttachmentTargetColumn, "ไฟล์แนบของประกาศ/คำสั่ง — คอลัมน์ Excel ที่เป็นชื่อไฟล์ (มี hyperlink) ใช้เฉพาะ Template ประกาศ/คำสั่ง" },
            { "Title",               "ชื่อเรื่อง/หัวข้อบทความ — ถ้าต้องคำนวณจากคอลัมน์ Excel (เช่น กฎหมาย/มาตรา) ให้ผสมข้อความคงที่กับคอลัมน์ในรูปแบบ {X} เช่น \"มาตรา {C}\"" },
            { "ContentHTML",         "เนื้อหาบทความ (HTML)" },
            { "Summary",             "สรุปย่อของบทความ" },
            { "TitleCategory",       "หมวดหัวข้อ/เลขมาตรา (nvarchar(500), migrated from Summary)" },
            { "CategoryID",          "รหัสหมวดหมู่ (FK → KMS.m_KBCategory)" },
            { "ArticleType",         "ประเภทบทความ: 1=ขั้นตอนงาน 2=คำถามที่พบบ่อย 3=แนวทางปฏิบัติ 4=แม่แบบ 5=กฎ/นโยบาย" },
            { "Status",              "สถานะ: 1=Draft 2=Review 3=Published 4=Archived" },
            { "PublishedDate",       "วันที่เผยแพร่บทความ" },
            { "EffectiveDate",       "วันที่มีผลบังคับใช้" },
            { "ExpiryDate",          "วันที่หมดอายุ/สิ้นสุดการบังคับใช้" },
            { "ReviewDueDate",       "วันครบกำหนดทบทวนเนื้อหา" },
            { "IsOfficialPolicy",    "เป็นนโยบาย/กฎที่เป็นทางการหรือไม่" },
            { "Tags",                "แท็กค้นหา คั่นด้วยเครื่องหมายจุลภาค" },
            { "ConfidentialityLevel","ระดับชั้นความลับ: 1=HC 2=SC 3=NC 4=PC" },
        };

        /// <summary>คืนคำอธิบายความหมายของคอลัมน์ <paramref name="columnName"/>; ถ้าไม่มีในรายการ จะแปลงชื่อคอลัมน์ PascalCase ให้อ่านง่ายขึ้นแทน</summary>
        public static string DescribeColumn(string columnName)
        {
            if (string.IsNullOrEmpty(columnName)) return string.Empty;

            string description;
            if (Descriptions.TryGetValue(columnName, out description))
                return description;

            return Humanize(columnName);
        }

        private static string Humanize(string pascalCaseName)
        {
            var sb = new StringBuilder();
            for (int i = 0; i < pascalCaseName.Length; i++)
            {
                char c = pascalCaseName[i];
                if (i > 0 && char.IsUpper(c) && !char.IsUpper(pascalCaseName[i - 1]))
                    sb.Append(' ');
                sb.Append(c);
            }
            return sb.ToString();
        }
    }
}
