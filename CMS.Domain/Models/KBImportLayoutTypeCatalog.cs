using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// ค่า LayoutType คงที่ ("Law"/"NonLaw") ที่ใช้ร่วมกันระหว่าง KBImportRow.RowKind (ตั้งค่าโดย
    /// KBExcelReader) และค่าที่ KBImportTemplateService.GetEffectiveLayoutType derive จาก CategoryID
    /// — รวมไว้จุดเดียวกันค่า/label หลุดไม่ตรงกันระหว่างไฟล์ ถ้าเพิ่ม layout ใหม่ในอนาคตให้แก้ที่นี่ที่เดียว
    /// </summary>
    public static class KBImportLayoutTypeCatalog
    {
        public const string Law = "Law";
        /// <summary>ไฟล์ที่ไม่ใช่กฎหมายทุกประเภท (OR, AN, SO, RG, CI, MR) — โครงสร้างเดียวกัน แยกประเภทด้วยตัวที่ 23 ของ Code รวม</summary>
        public const string NonLaw = "NonLaw";

        /// <summary>ความยาวของ Code รวมในไฟล์กฎหมาย: [prefix 3][...][suffix 5]</summary>
        public const int LawCodeLength = 27;

        /// <summary>5 ตัวสุดท้ายของ Code รวมที่บอกว่าเป็นตัวมาตราเอง (แถวประกาศ/คำสั่งใช้ 1 หลักประเภท + 4 หลักเลขเอกสาร จึงไม่มีทางเป็นค่านี้)</summary>
        public const string LawCodeSuffix = "00000";

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

        /// <summary>ตัวที่ 23 ของ Code รวมในไฟล์ที่ไม่ใช่กฎหมาย (ประเภทเอกสาร) → CategoryCode ของหมวดนั้น</summary>
        private static readonly System.Collections.Generic.Dictionary<char, string> NonLawTypeCategoryCodes =
            new System.Collections.Generic.Dictionary<char, string>
        {
            { '1', "AN" },   // หมวดภายใน/ประกาศ
            { '2', "OR" },   // คำสั่ง (นายทะเบียน/กระทรวง)
            { '3', "SO" },   // คำสั่งพิเศษ
            { '4', "RG" },   // ระเบียบ
            { '5', "CI" },   // หนังสือเวียน
            { '6', "MR" },   // กฎกระทรวง
        };

        /// <summary>CategoryCode ของประเภทเอกสารที่ไม่ใช่กฎหมายทั้งหมด เรียงตามตัวเลขตัวที่ 23 (AN, OR, SO, RG, CI, MR)</summary>
        public static System.Collections.Generic.IList<string> GetNonLawCategoryCodes()
        {
            var result = new System.Collections.Generic.List<string>();
            foreach (char digit in "123456")
                result.Add(NonLawTypeCategoryCodes[digit]);
            return result;
        }

        /// <summary>CategoryCode ที่ตัวที่ 23 ของ Code รวมชี้ถึง — null ถ้า <paramref name="code"/> ไม่ใช่ Code รวมของไฟล์ที่ไม่ใช่กฎหมาย</summary>
        public static string GetNonLawCategoryCode(string code)
        {
            if (string.IsNullOrEmpty(code) || code.Length != LawCodeLength) return null;
            if (string.Equals(code.Substring(LawCodeLength - 5), LawCodeSuffix, StringComparison.Ordinal)) return null;

            string categoryCode;
            return NonLawTypeCategoryCodes.TryGetValue(code[22], out categoryCode) ? categoryCode : null;
        }

        /// <summary>
        /// true ถ้า <paramref name="code"/> เป็น Code รวมของไฟล์ที่ไม่ใช่กฎหมาย: ยาว 27 ตัว, 5 ตัวสุดท้ายไม่ใช่ "00000",
        /// และตัวที่ 23 เป็น 1-6
        /// </summary>
        public static bool IsNonLawCode(string code)
        {
            return GetNonLawCategoryCode(code) != null;
        }

        /// <summary>ป้ายแสดงผลของประเภทเอกสาร จากตัวที่ 23 ของ Code รวม (1-6) — ค่าว่างถ้าไม่ใช่ 1-6</summary>
        public static string GetNonLawTypeLabel(char typeDigit)
        {
            switch (typeDigit)
            {
                case '1': return "ประกาศ";        // AN
                case '2': return "คำสั่ง";         // OR
                case '3': return "คำสั่งพิเศษ";    // SO
                case '4': return "ระเบียบ";        // RG
                case '5': return "หนังสือเวียน";   // CI
                case '6': return "กฎกระทรวง";     // MR
                default:  return string.Empty;
            }
        }

        /// <summary>คำอธิบายภาษาไทยของค่า LayoutType; คืนค่าตัวเองถ้าไม่รู้จัก (กันข้อมูลเพี้ยนพังหน้าจอ)</summary>
        public static string Describe(string layoutType)
        {
            if (string.Equals(layoutType, Law, StringComparison.OrdinalIgnoreCase))
                return "กฎหมาย (มาตรา)";
            if (string.Equals(layoutType, NonLaw, StringComparison.OrdinalIgnoreCase))
                return "เอกสารที่ไม่ใช่กฎหมาย (AN/OR/SO/RG/CI/MR)";
            return layoutType ?? string.Empty;
        }
    }
}
