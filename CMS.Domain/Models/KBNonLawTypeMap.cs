using System;
using System.Collections.Generic;
using System.Text;

namespace CMS.Domain.Models
{
    /// <summary>
    /// ตาราง "รหัสประเภทเอกสาร (ตัวที่ 23 ของ Code รวม) → หมวดประเภทเอกสารที่ไม่ใช่กฎหมาย" ที่สร้างจากรายการหมวด (master = KMS.m_KBCategory
    /// คอลัมน์ ImportTypeDigit) แทนตารางที่เคย hardcode — เพิ่มประเภทใหม่ = สร้างหมวด + ตั้งรหัสที่หน้าหมวดหมู่ ไม่ต้องแก้โค้ด
    /// รวมหมวดที่ถูกปิดใช้งานด้วย (ให้ผู้ใช้ import เห็นว่า "หมวดประเภทถูกปิดใช้งาน" แทนที่จะเป็น "ไม่รู้จักรหัส"); รหัสซ้ำ (ไม่ควรเกิด
    /// เพราะมี unique index) ใช้หมวดแรกที่เจอ. สร้างใหม่ทุกครั้งที่โหลดรายการหมวด ไม่ cache.
    /// </summary>
    public sealed class KBNonLawTypeMap
    {
        private readonly SortedDictionary<char, KBCategory> _byDigit = new SortedDictionary<char, KBCategory>();

        public KBNonLawTypeMap(IEnumerable<KBCategory> categories)
        {
            if (categories == null) throw new ArgumentNullException("categories");

            foreach (var category in categories)
            {
                char digit;
                if (category == null || !KBImportLayoutTypeCatalog.TryParseTypeDigit(category.ImportTypeDigit, out digit)) continue;

                if (!_byDigit.ContainsKey(digit))
                    _byDigit[digit] = category;
            }
        }

        /// <summary>true ถ้ายังไม่มีหมวดใดตั้งรหัสประเภทเอกสารไว้เลย</summary>
        public bool IsEmpty { get { return _byDigit.Count == 0; } }

        /// <summary>หมวดประเภทเอกสารทั้งหมด เรียงตามรหัส (1, 2, 3, ...)</summary>
        public IList<KBCategory> Types { get { return new List<KBCategory>(_byDigit.Values); } }

        /// <summary>หมวดประเภทเอกสารที่ใช้รหัสนี้ — null ถ้าไม่มี</summary>
        public KBCategory FindByDigit(char digit)
        {
            KBCategory category;
            return _byDigit.TryGetValue(digit, out category) ? category : null;
        }

        /// <summary>
        /// หมวดประเภทเอกสารของ Code รวมที่เป็นรูปแบบไฟล์ที่ไม่ใช่กฎหมาย (ดู <see cref="KBImportLayoutTypeCatalog.TryGetNonLawTypeDigit"/>)
        /// — null ถ้ารูปแบบไม่ใช่หรือไม่มีหมวดที่ใช้รหัสตัวที่ 23 นั้น
        /// </summary>
        public KBCategory FindByArticleCode(string articleCode)
        {
            char digit;
            return KBImportLayoutTypeCatalog.TryGetNonLawTypeDigit(articleCode == null ? null : articleCode.Trim(), out digit)
                ? FindByDigit(digit)
                : null;
        }

        /// <summary>ป้ายแสดงผลของประเภทเอกสาร = ชื่อหมวด — ค่าว่างถ้าไม่มีหมวดที่ใช้รหัสนี้</summary>
        public string GetTypeLabel(char digit)
        {
            KBCategory category = FindByDigit(digit);
            return category == null || category.CategoryName == null ? string.Empty : category.CategoryName.Trim();
        }

        /// <summary>รหัสที่ตั้งไว้แล้วแบบอ่านง่ายสำหรับข้อความ error เช่น "1-6" หรือ "1-3, 5" — ค่าว่างถ้ายังไม่มี</summary>
        public string DescribeDigits()
        {
            var text = new StringBuilder();
            int runStart = -1, runEnd = -1;

            foreach (char digit in _byDigit.Keys)
            {
                if (runStart >= 0 && digit == runEnd + 1)
                {
                    runEnd = digit;
                    continue;
                }

                AppendRun(text, runStart, runEnd);
                runStart = runEnd = digit;
            }
            AppendRun(text, runStart, runEnd);

            return text.ToString();
        }

        private static void AppendRun(StringBuilder text, int runStart, int runEnd)
        {
            if (runStart < 0) return;

            if (text.Length > 0) text.Append(", ");
            text.Append((char)runStart);
            if (runEnd > runStart)
            {
                // สองตัวติดกันเขียน "1, 2" ไม่ใช่ "1-2"
                text.Append(runEnd == runStart + 1 ? ", " : "-");
                text.Append((char)runEnd);
            }
        }
    }
}
