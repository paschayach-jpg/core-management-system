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
        /// <summary>ไฟล์ที่ไม่ใช่กฎหมายทุกประเภท — โครงสร้างเดียวกัน แยกประเภทด้วยตัวที่ 25 ของ Code รวม (รหัสประเภทตั้งที่หมวด ดู <see cref="KBCategory.ImportTypeDigit"/>)</summary>
        public const string NonLaw = "NonLaw";

        /// <summary>ความยาวของ Code รวม: [รหัสกฎหมาย 5][...][suffix 5] — รหัสกฎหมาย 5 ตัว = ตัวอักษร 3 + เลขลำดับ 2 (เช่น PVI00 กับ PVI01 เป็นคนละกฎหมาย)</summary>
        public const int LawCodeLength = 29;

        /// <summary>ความยาวรหัสกฎหมายที่ขึ้นต้น Code รวม (เช่น "PVI00") = CategoryCode ของหมวดกฎหมายนั้นพอดี</summary>
        public const int LawCodePrefixLength = 5;

        /// <summary>ความยาว Code ที่เก็บเป็น ArticleCode ของมาตรา = Code รวมตัด suffix <see cref="LawCodeSuffix"/> ออก (24 ตัว); ใช้เป็น Code กฎหมายแม่ของเอกสารที่ไม่ใช่กฎหมายด้วย</summary>
        public const int ParentCodeLength = LawCodeLength - 5;

        /// <summary>ตำแหน่ง (0-based) ที่เลขมาตราเริ่มใน Code รวมของกฎหมาย = ตัวที่ 6 (ยาว 4 หลัก)</summary>
        public const int LawSectionStartIndex = LawCodePrefixLength;

        /// <summary>จำนวนหลักของเลขมาตราใน Code รวม (ตัวที่ 6-9)</summary>
        public const int LawSectionDigits = 4;

        /// <summary>5 ตัวสุดท้ายของ Code รวมที่บอกว่าเป็นตัวมาตราเอง (แถวประกาศ/คำสั่งใช้ 1 หลักประเภท + 4 หลักเลขเอกสาร จึงไม่มีทางเป็นค่านี้)</summary>
        public const string LawCodeSuffix = "00000";

        /// <summary>ตำแหน่ง (0-based) ที่ปีของกฎหมายเริ่มใน Code รวม = ตัวที่ 10 (ยาว 3 หลัก ต่อด้วย "2" นำหน้าเป็น พ.ศ. 4 หลัก)</summary>
        public const int LawYearStartIndex = 9;

        /// <summary>จำนวนหลักของปีใน Code รวม (ตัวที่ 10-12) — เช่น "535" คือ พ.ศ. 2535</summary>
        public const int LawYearDigits = 3;

        /// <summary>CategoryCode ของหมวดรากของกฎหมาย — หมวดใดที่ไล่ ParentCategoryID ขึ้นไปเจอรหัสนี้ถือเป็นกฎหมาย</summary>
        public const string LawRootCategoryCode = "LW";

        /// <summary>
        /// CategoryCode ของหมวด "พระราชกฤษฎีกา" (อยู่ใต้ LW) — หมวดกลุ่มพระราชกฤษฎีกา (เช่น ETR00, EXM00) ต้องเป็นหมวดย่อยของหมวดนี้;
        /// ใช้แยกพระราชกฤษฎีกาออกจากกฎหมายใน KB tree ด้วย
        /// </summary>
        public const string DecreeRootCategoryCode = "DDD00";

        /// <summary>
        /// แยกคอลัมน์ "Code กฎหมายอ้างอิง (ออกตาม)" ของพระราชกฤษฎีกา (หลายบรรทัดในเซลล์เดียว):
        /// บรรทัดยาว <see cref="LawCodePrefixLength"/> (5) = CategoryCode ของหมวดกลุ่ม — ต้องมี 1 บรรทัดพอดี;
        /// บรรทัดยาว <see cref="ParentCodeLength"/> (24) = ArticleCode ของมาตราในกฎหมายที่อ้างอิง — ต้องมีอย่างน้อย 1 (ซ้ำนับครั้งเดียว);
        /// ความยาวอื่น = ผิดรูปแบบ. คืน <c>false</c> พร้อมข้อความไทยใน <paramref name="error"/> เมื่อไม่ผ่าน
        /// </summary>
        public static bool TryParseLawReference(string raw, out string categoryCode, out System.Collections.Generic.IList<string> parentCodes, out string error)
        {
            categoryCode = null;
            parentCodes  = new System.Collections.Generic.List<string>();
            error        = null;

            var categoryCodes = new System.Collections.Generic.List<string>();
            var seenParents   = new System.Collections.Generic.HashSet<string>(StringComparer.OrdinalIgnoreCase);

            foreach (string part in (raw ?? string.Empty).Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None))
            {
                string line = part.Trim();
                if (line.Length == 0) continue;

                if (line.Length == LawCodePrefixLength)
                    categoryCodes.Add(line);
                else if (line.Length == ParentCodeLength)
                {
                    if (seenParents.Add(line)) parentCodes.Add(line);
                }
                else
                {
                    error = string.Format("Code กฎหมายอ้างอิง: บรรทัด '{0}' ยาว {1} ตัว (ต้องเป็นรหัสหมวด {2} ตัว หรือรหัสมาตรา {3} ตัว)",
                        line, line.Length, LawCodePrefixLength, ParentCodeLength);
                    return false;
                }
            }

            if (categoryCodes.Count == 0)
            {
                error = string.Format("Code กฎหมายอ้างอิง: ไม่พบรหัสหมวดพระราชกฤษฎีกา ({0} ตัว) ในเซลล์", LawCodePrefixLength);
                return false;
            }
            if (categoryCodes.Count > 1)
            {
                error = string.Format("Code กฎหมายอ้างอิง: พบรหัสหมวด {0} ตัวมากกว่า 1 รายการ ({1})", LawCodePrefixLength, string.Join(", ", categoryCodes));
                return false;
            }
            if (parentCodes.Count == 0)
            {
                error = string.Format("Code กฎหมายอ้างอิง: ไม่พบรหัสมาตราของกฎหมายที่อ้างอิง ({0} ตัว) — พระราชกฤษฎีกาต้องอ้างอิงกฎหมายอย่างน้อย 1 มาตรา", ParentCodeLength);
                return false;
            }

            categoryCode = categoryCodes[0];
            return true;
        }

        /// <summary>
        /// true ถ้า <paramref name="code"/> เป็น Code รวมของกฎหมาย: ยาว 29 ตัว และ 5 ตัวสุดท้ายเป็น "00000"
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
        /// ปี พ.ศ. 4 หลักของกฎหมายจาก Code รวม: ตัวที่ 10-12 (เช่น "535") นำหน้าด้วย "2" → "2535". คืน <c>null</c> ถ้า Code สั้นเกินไป
        /// หรือตัวที่ 10-12 ไม่ใช่ตัวเลข — ใช้แยกหมวดตามปีทั้งฝั่งกฎหมาย (PLI00 ปี 2535 กับ 2540 ใช้รหัสกฎหมายเดียวกัน →
        /// PLI002535/PLI002540) และฝั่งที่ไม่ใช่กฎหมาย (ประเภทเอกสาร OR000 + ปี → OR0002535); ดู <c>KBImportCategoryResolver</c>
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

        /// <summary>ตำแหน่ง (0-based) ของรหัสประเภทเอกสารใน Code รวมของไฟล์ที่ไม่ใช่กฎหมาย = ตัวที่ 25 (ต่อจาก Code กฎหมายแม่ 24 ตัว)</summary>
        public const int NonLawTypeDigitIndex = ParentCodeLength;

        /// <summary>รหัสประเภทเอกสารเป็นตัวเลข 1 หลัก — ต่ำสุดที่ตั้งได้ที่หมวด (<see cref="KBCategory.ImportTypeDigit"/>)</summary>
        public const char MinTypeDigit = '1';

        /// <summary>รหัสประเภทเอกสารเป็นตัวเลข 1 หลัก — สูงสุดที่ตั้งได้ที่หมวด (<see cref="KBCategory.ImportTypeDigit"/>)</summary>
        public const char MaxTypeDigit = '9';

        /// <summary>
        /// ข้อความอธิบายรูปแบบ Code รวมของไฟล์ที่ไม่ใช่กฎหมายสำหรับ error ตอน validate — ตรวจเฉพาะ "รูปแบบ"
        /// ส่วนรหัสประเภทที่มีจริงในระบบตรวจแยกที่ <c>KBImportCategoryResolver</c> (master อยู่ที่ m_KBCategory)
        /// </summary>
        public const string NonLawCodeFormatMessage =
            "Code รวม: ต้องยาว 29 ตัว, 5 ตัวสุดท้ายห้ามเป็น 00000 และตัวที่ 25 ต้องเป็นรหัสประเภทเอกสาร (ตัวเลข 1-9)";

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
        /// รหัสประเภทเอกสารจากตัวที่ 25 ของ Code รวมที่เป็นรูปแบบไฟล์ที่ไม่ใช่กฎหมาย: ยาว 29 ตัว, 5 ตัวสุดท้ายไม่ใช่ "00000",
        /// ตัวที่ 25 เป็น 1-9 — ตรวจเฉพาะรูปแบบ ไม่ตรวจว่ามีประเภทเอกสารรหัสนั้นจริง (master อยู่ที่ m_KBCategory ดู <see cref="KBNonLawTypeMap"/>)
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
