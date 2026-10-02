using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// ค่า LayoutType คงที่ ("Law"/"NonLaw") ที่ใช้ร่วมกันระหว่าง KBImportRow.RowKind (ตั้งค่าโดย
    /// KBExcelReader จากไฟล์) และประเภทไฟล์ที่ Template เก็บไว้เป็นแถว mapping เสมือน LayoutType
    /// — รวมไว้จุดเดียวกันค่า/label หลุดไม่ตรงกันระหว่างไฟล์ ถ้าเพิ่ม layout ใหม่ในอนาคตให้แก้ที่นี่ที่เดียว
    /// </summary>
    public static class KBImportLayoutTypeCatalog
    {
        public const string Law = "Law";
        /// <summary>ไฟล์ที่ไม่ใช่กฎหมายทุกประเภท — โครงสร้างเดียวกัน แยกประเภทด้วยตัวที่ 23 ของ Code รวม (รหัสประเภทตั้งที่หมวด ดู <see cref="KBCategory.ImportTypeDigit"/>)</summary>
        public const string NonLaw = "NonLaw";

        /// <summary>ความยาวของ Code รวมในไฟล์กฎหมาย: [prefix 3][...][suffix 5]</summary>
        public const int LawCodeLength = 27;

        /// <summary>5 ตัวสุดท้ายของ Code รวมที่บอกว่าเป็นตัวมาตราเอง (แถวประกาศ/คำสั่งใช้ 1 หลักประเภท + 4 หลักเลขเอกสาร จึงไม่มีทางเป็นค่านี้)</summary>
        public const string LawCodeSuffix = "00000";

        /// <summary>ตำแหน่ง (0-based) ที่ปีของกฎหมายเริ่มใน Code รวม = ตัวที่ 8 (ยาว 3 หลัก ต่อด้วย "2" นำหน้าเป็น พ.ศ. 4 หลัก)</summary>
        public const int LawYearStartIndex = 7;

        /// <summary>จำนวนหลักของปีใน Code รวม (ตัวที่ 8-10) — เช่น "535" คือ พ.ศ. 2535</summary>
        public const int LawYearDigits = 3;

        /// <summary>CategoryCode ของหมวดรากของกฎหมาย — หมวดใดที่ไล่ ParentCategoryID ขึ้นไปเจอรหัสนี้ถือเป็นกฎหมาย</summary>
        public const string LawRootCategoryCode = "LW";

        /// <summary>
        /// true ถ้า <paramref name="code"/> เป็น Code รวมของกฎหมาย: ยาว 27 ตัว และ 5 ตัวสุดท้ายเป็น "00000"
        /// (ไม่เช็ค prefix รหัสกฎหมาย: รหัสของไฟล์ที่ไม่ใช่กฎหมายก็ขึ้นต้นด้วยรหัสกฎหมายแม่เหมือนกัน จึงไม่ช่วยแยกประเภท
        /// และ prefix ที่ต้องตรงกับ CategoryCode ของ Template ถูกตรวจรายแถวใน KBImportProcessor อยู่แล้ว)
        /// </summary>
        public static bool IsLawCode(string code)
        {
            return !string.IsNullOrEmpty(code)
                && code.Length == LawCodeLength
                && string.Equals(code.Substring(LawCodeLength - 5), LawCodeSuffix, StringComparison.Ordinal);
        }

        /// <summary>
        /// ปี พ.ศ. 4 หลักของกฎหมายจาก Code รวม: ตัวที่ 8-10 (เช่น "535") นำหน้าด้วย "2" → "2535". คืน <c>null</c> ถ้า Code สั้นเกินไป
        /// หรือตัวที่ 8-10 ไม่ใช่ตัวเลข — ใช้แยกหมวดตามปีทั้งฝั่งกฎหมาย (PLI ปี 2535 กับ 2540 ใช้รหัสกฎหมายเดียวกัน →
        /// PLI2535/PLI2540) และฝั่งที่ไม่ใช่กฎหมาย (ประเภทเอกสาร OR + ปี → OR2535); ดู <c>KBImportCategoryResolver</c>
        /// </summary>
        public static string GetLawYear(string code)
        {
            if (string.IsNullOrEmpty(code) || code.Length < LawYearStartIndex + LawYearDigits) return null;

            string digits = code.Substring(LawYearStartIndex, LawYearDigits);
            foreach (char ch in digits)
            {
                if (ch < '0' || ch > '9') return null;
            }
            return "2" + digits;
        }

        /// <summary>ตำแหน่ง (0-based) ของรหัสประเภทเอกสารใน Code รวมของไฟล์ที่ไม่ใช่กฎหมาย = ตัวที่ 23</summary>
        public const int NonLawTypeDigitIndex = 22;

        /// <summary>รหัสประเภทเอกสารเป็นตัวเลข 1 หลัก — ต่ำสุดที่ตั้งได้ที่หมวด (<see cref="KBCategory.ImportTypeDigit"/>)</summary>
        public const char MinTypeDigit = '1';

        /// <summary>รหัสประเภทเอกสารเป็นตัวเลข 1 หลัก — สูงสุดที่ตั้งได้ที่หมวด (<see cref="KBCategory.ImportTypeDigit"/>)</summary>
        public const char MaxTypeDigit = '9';

        /// <summary>
        /// ข้อความอธิบายรูปแบบ Code รวมของไฟล์ที่ไม่ใช่กฎหมายสำหรับ error ตอน validate — ตรวจเฉพาะ "รูปแบบ"
        /// ส่วนรหัสประเภทที่มีจริงในระบบตรวจแยกที่ <c>KBImportCategoryResolver</c> (master อยู่ที่ m_KBCategory)
        /// </summary>
        public const string NonLawCodeFormatMessage =
            "Code รวม: ต้องยาว 27 ตัว, 5 ตัวสุดท้ายห้ามเป็น 00000 และตัวที่ 23 ต้องเป็นรหัสประเภทเอกสาร (ตัวเลข 1-9)";

        /// <summary>true ถ้า <paramref name="ch"/> เป็นรหัสประเภทเอกสารที่ใช้ได้ (1-9)</summary>
        public static bool IsTypeDigit(char ch)
        {
            return ch >= MinTypeDigit && ch <= MaxTypeDigit;
        }

        /// <summary>แปลงข้อความ (ตัดช่องว่างหัวท้าย) เป็นรหัสประเภทเอกสาร — false ถ้าว่างหรือไม่ใช่ตัวเลข 1-9 ตัวเดียว</summary>
        public static bool TryParseTypeDigit(string value, out char digit)
        {
            digit = '\0';

            string trimmed = value == null ? string.Empty : value.Trim();
            if (trimmed.Length != 1 || !IsTypeDigit(trimmed[0])) return false;

            digit = trimmed[0];
            return true;
        }

        /// <summary>
        /// รหัสประเภทเอกสารจากตัวที่ 23 ของ Code รวมที่เป็นรูปแบบไฟล์ที่ไม่ใช่กฎหมาย: ยาว 27 ตัว, 5 ตัวสุดท้ายไม่ใช่ "00000",
        /// ตัวที่ 23 เป็น 1-9 — ตรวจเฉพาะรูปแบบ ไม่ตรวจว่ามีประเภทเอกสารรหัสนั้นจริง (master อยู่ที่ m_KBCategory ดู <see cref="KBNonLawTypeMap"/>)
        /// </summary>
        public static bool TryGetNonLawTypeDigit(string code, out char digit)
        {
            digit = '\0';
            if (string.IsNullOrEmpty(code) || code.Length != LawCodeLength) return false;
            if (string.Equals(code.Substring(LawCodeLength - 5), LawCodeSuffix, StringComparison.Ordinal)) return false;

            char ch = code[NonLawTypeDigitIndex];
            if (!IsTypeDigit(ch)) return false;

            digit = ch;
            return true;
        }

        /// <summary>true ถ้า <paramref name="code"/> มีรูปแบบ Code รวมของไฟล์ที่ไม่ใช่กฎหมาย (ดู <see cref="TryGetNonLawTypeDigit"/>)</summary>
        public static bool IsNonLawCodeFormat(string code)
        {
            char digit;
            return TryGetNonLawTypeDigit(code, out digit);
        }

        /// <summary>
        /// คืนค่า LayoutType มาตรฐาน ("Law"/"NonLaw") จากข้อความที่อ่านมา (ไม่สนตัวพิมพ์เล็ก-ใหญ่/ช่องว่างหัวท้าย)
        /// หรือ <c>null</c> ถ้าเป็นค่าว่าง/ไม่รู้จัก — ใช้กับค่า LayoutType ที่ Template เก็บเป็นแถว mapping เสมือน
        /// (<see cref="KBArticleColumnCatalog.LayoutTypeTargetColumn"/>)
        /// </summary>
        public static string Normalize(string layoutType)
        {
            if (string.IsNullOrWhiteSpace(layoutType)) return null;

            string trimmed = layoutType.Trim();
            if (string.Equals(trimmed, Law, StringComparison.OrdinalIgnoreCase))    return Law;
            if (string.Equals(trimmed, NonLaw, StringComparison.OrdinalIgnoreCase)) return NonLaw;
            return null;
        }

        /// <summary>true ถ้า <paramref name="layoutType"/> เป็น "Law" หรือ "NonLaw"</summary>
        public static bool IsValid(string layoutType)
        {
            return Normalize(layoutType) != null;
        }

        /// <summary>ตัวเลือกประเภทไฟล์ของ Template สำหรับ dropdown ในหน้า config (ยังไม่รวมตัวเลือก "ยังไม่เลือก" — view เติมเอง)</summary>
        public static System.Collections.Generic.IList<KBImportLayoutOption> GetLayoutOptions()
        {
            return new System.Collections.Generic.List<KBImportLayoutOption>
            {
                new KBImportLayoutOption { LayoutType = Law,    Description = Describe(Law) },
                new KBImportLayoutOption { LayoutType = NonLaw, Description = "ไม่ใช่กฎหมาย (ประเภทเอกสารตามที่ตั้งในหมวดหมู่)" },
            };
        }

        /// <summary>คำอธิบายภาษาไทยของค่า LayoutType; คืนค่าตัวเองถ้าไม่รู้จัก (กันข้อมูลเพี้ยนพังหน้าจอ)</summary>
        public static string Describe(string layoutType)
        {
            if (string.Equals(layoutType, Law, StringComparison.OrdinalIgnoreCase))
                return "กฎหมาย (มาตรา)";
            if (string.Equals(layoutType, NonLaw, StringComparison.OrdinalIgnoreCase))
                return "เอกสารที่ไม่ใช่กฎหมาย (ประเภทเอกสารตามที่ตั้งในหมวดหมู่)";
            return layoutType ?? string.Empty;
        }
    }
}
