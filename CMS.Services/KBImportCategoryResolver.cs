using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Domain.Models;

namespace CMS.Services
{
    /// <summary>
    /// หา CategoryID ของแต่ละแถวตอน import จาก ArticleCode — Template ไม่ผูกหมวดแล้ว จึงใช้ 1 Template ได้หลายกฎหมาย/หลายประเภท:
    /// <list type="bullet">
    /// <item><b>กฎหมาย</b> — 5 ตัวแรกของ Code รวม (เช่น "RVA00" — RVA00 กับ RVA01 เป็นคนละกฎหมาย) ต้องตรงกับ CategoryCode ของหมวดที่อยู่ใต้หมวด "กฎหมาย" (LW);
    /// กฎหมายที่มีหลายปีและแยกหมวดตามปี (เช่น กฎหมาย › พระราชบัญญัติประกันชีวิต (PLI00) › PLI002535, PLI002540) ใช้ปีจากตัวที่ 10-12 + "2" นำหน้า
    /// เลือกหมวดปี (ดู <see cref="TryResolveLawArticle"/>) — ลำดับชั้นหมวดอ่านจาก m_KBCategory ที่เดียว เพราะไฟล์ Excel ไม่มีข้อมูลนี้</item>
    /// <item><b>ไม่ใช่กฎหมาย</b> — ตัวที่ 25 ของ Code รวม (รหัสประเภทเอกสาร 1-9) ต้องตรงกับ ImportTypeDigit ของหมวดประเภทที่อยู่นอกหมวด "กฎหมาย"
    /// (master อยู่ที่ m_KBCategory — ดู <see cref="KBNonLawTypeMap"/>) แล้วต่อ CategoryCode ของหมวดประเภทนั้นกับปี (ตัวที่ 10-12 + "2" นำหน้า) เป็นรหัสหมวดปี เช่น OR002535 ที่ต้องมีเป็นหมวดย่อยของหมวดประเภท (ดู
    /// <see cref="TryResolveNonLawArticle"/>); หมวดที่ไม่มีในระบบคืนค่า <see cref="KBMissingCategory"/> ให้แจ้งผู้ใช้ว่าต้องสร้างหมวดอะไร</item>
    /// </list>
    /// สร้างจากรายการหมวดทั้งหมดครั้งเดียวต่อการ validate หนึ่งไฟล์ (ไม่เรียกฐานข้อมูลเอง) — CategoryCode เป็น UNIQUE
    /// (UQ_KMS_KBCat_Code) จึงหาหมวดจากรหัสได้แน่นอนตัวเดียว
    /// </summary>
    public class KBImportCategoryResolver
    {
        private readonly Dictionary<string, KBCategory> _byCode = new Dictionary<string, KBCategory>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, KBCategory>    _byId   = new Dictionary<int, KBCategory>();
        private readonly Dictionary<int, List<KBCategory>> _children = new Dictionary<int, List<KBCategory>>();
        private readonly KBNonLawTypeMap _typeMap;

        public KBImportCategoryResolver(IEnumerable<KBCategory> categories)
        {
            if (categories == null) throw new ArgumentNullException("categories");

            var all = categories.ToList();
            _typeMap = new KBNonLawTypeMap(all);

            foreach (var category in all)
            {
                if (category == null) continue;

                _byId[category.CategoryID] = category;

                if (category.ParentCategoryID.HasValue)
                {
                    List<KBCategory> siblings;
                    if (!_children.TryGetValue(category.ParentCategoryID.Value, out siblings))
                        _children[category.ParentCategoryID.Value] = siblings = new List<KBCategory>();
                    siblings.Add(category);
                }

                string code = category.CategoryCode == null ? null : category.CategoryCode.Trim();
                if (!string.IsNullOrEmpty(code) && !_byCode.ContainsKey(code))
                    _byCode[code] = category;
            }
        }

        /// <summary>หมวดประเภทเอกสารที่ไม่ใช่กฎหมายตามรหัสที่ตั้งไว้ (ตัวที่ 25 ของ Code รวม) — ใช้ทำป้ายประเภท/ข้อความ error</summary>
        public KBNonLawTypeMap NonLawTypes { get { return _typeMap; } }

        /// <summary>
        /// หาหมวดกฎหมายจากรหัส 5 ตัวแรกของ Code รวม — ต้องมีในระบบ, ใช้งานอยู่, และอยู่ใต้หมวด "กฎหมาย" (ไม่ใช่หมวด "กฎหมาย" เอง)
        /// คืน <c>false</c> พร้อมข้อความภาษาไทยใน <paramref name="error"/> เมื่อไม่ผ่าน
        /// </summary>
        public bool TryResolveLaw(string lawCode, out KBCategory category, out string error)
        {
            category = null;

            string code = (lawCode ?? string.Empty).Trim();
            if (code.Length == 0)
            {
                error = "ไม่พบรหัสกฎหมาย (5 ตัวแรกของ Code รวม)";
                return false;
            }

            KBCategory found;
            if (!_byCode.TryGetValue(code, out found))
            {
                error = string.Format("ไม่พบหมวดกฎหมายรหัส \"{0}\" ในระบบ (ต้องมีหมวดใต้ \"กฎหมาย\" ที่ CategoryCode = {0})", code);
                return false;
            }

            if (string.Equals(found.CategoryCode, KBImportLayoutTypeCatalog.LawRootCategoryCode, StringComparison.OrdinalIgnoreCase))
            {
                error = string.Format("\"{0}\" เป็นหมวดรากของกฎหมาย ไม่ใช่รหัสกฎหมายรายฉบับ", code);
                return false;
            }

            if (!IsLawRootOrUnder(found))
            {
                error = string.Format("รหัส \"{0}\" ไม่ใช่หมวดกฎหมาย (ไม่อยู่ใต้หมวด \"กฎหมาย\")", code);
                return false;
            }

            if (!found.IsActive)
            {
                error = string.Format("หมวดกฎหมาย \"{0}\" ถูกปิดใช้งาน", code);
                return false;
            }

            category = found;
            error    = null;
            return true;
        }

        /// <summary>
        /// หาหมวดของแถวกฎหมายจาก Code รวมเต็ม: หมวดกฎหมายจาก 5 ตัวแรก (<see cref="TryResolveLaw"/>) แล้วถ้ากฎหมายนั้นแยกหมวดตามปี
        /// ใช้หมวดปีที่ตรงกับปีใน Code รวม (ตัวที่ 10-12 + "2" นำหน้า เช่น 535 → 2535):
        /// <list type="bullet">
        /// <item>หมวดปีมีรหัส = [รหัสกฎหมาย][พ.ศ. 4 หลัก] (เช่น PLI002535) และต้องเป็นหมวดย่อยโดยตรงของหมวดกฎหมายนั้น</item>
        /// <item>กฎหมายที่ไม่มีหมวดลูกรูปแบบปีเลย = ไม่แยกปี ใช้หมวดกฎหมายนั้นเอง (เหมือนเดิม)</item>
        /// <item>กฎหมายที่แยกปีแล้วแต่ไม่พบหมวดของปีนี้ = ไม่ผ่าน (กันบทความไปลงหมวดแม่ผิดที่แบบเงียบ ๆ)</item>
        /// </list>
        /// คืน <c>false</c> พร้อมข้อความภาษาไทยใน <paramref name="error"/> เมื่อไม่ผ่าน
        /// </summary>
        public bool TryResolveLawArticle(string articleCode, out KBCategory category, out string error)
        {
            KBMissingCategory missing;
            return TryResolveLawArticle(articleCode, out category, out error, out missing);
        }

        /// <summary>
        /// เหมือน <see cref="TryResolveLawArticle(string, out KBCategory, out string)"/> และเมื่อกฎหมายแยกหมวดตามปีแต่ยังไม่มีหมวดของปีนั้น
        /// คืน <paramref name="missing"/> = หมวดปีที่ต้องสร้าง (เช่น PLI002551 ใต้ PLI00) ให้แจ้งผู้ใช้
        /// </summary>
        public bool TryResolveLawArticle(string articleCode, out KBCategory category, out string error, out KBMissingCategory missing)
        {
            category = null;
            missing  = null;

            string code = (articleCode ?? string.Empty).Trim();
            if (code.Length < KBImportLayoutTypeCatalog.LawCodePrefixLength)
            {
                error = "ไม่พบรหัสกฎหมาย (5 ตัวแรกของ Code รวม)";
                return false;
            }

            KBCategory law;
            if (!TryResolveLaw(code.Substring(0, KBImportLayoutTypeCatalog.LawCodePrefixLength), out law, out error))
                return false;

            string lawCode = law.CategoryCode.Trim();

            // กฎหมายนี้แยกหมวดตามปีหรือไม่ = มีหมวดลูกที่รหัสเป็น [รหัสกฎหมาย][พ.ศ. 4 หลัก]
            List<KBCategory> children;
            bool splitByYear = _children.TryGetValue(law.CategoryID, out children)
                && children.Any(c => IsYearCategoryCode(c.CategoryCode, lawCode));

            string year     = KBImportLayoutTypeCatalog.GetLawYear(code);
            string yearCode = year == null ? null : lawCode + year;

            if (yearCode != null)
            {
                KBCategory found;
                if (_byCode.TryGetValue(yearCode, out found))
                {
                    if (found.ParentCategoryID != law.CategoryID)
                    {
                        error = string.Format("หมวด \"{0}\" มีอยู่แต่ไม่ได้อยู่ใต้หมวดกฎหมาย \"{1}\" (ต้องเป็นหมวดย่อยโดยตรงของหมวดนี้)", yearCode, law.CategoryName);
                        return false;
                    }

                    if (!found.IsActive)
                    {
                        error = string.Format("หมวดปี \"{0}\" ถูกปิดใช้งาน", yearCode);
                        return false;
                    }

                    category = found;
                    error    = null;
                    return true;
                }
            }

            if (!splitByYear)
            {
                category = law;
                error    = null;
                return true;
            }

            if (year != null)
                missing = new KBMissingCategory { CategoryCode = yearCode, ParentCategoryCode = lawCode, ParentCategoryName = law.CategoryName };

            error = year == null
                ? string.Format("กฎหมาย \"{0}\" แยกหมวดตามปี แต่ตัวที่ 10-12 ของ Code รวมไม่ใช่ตัวเลข จึงหาปีไม่ได้", lawCode)
                : string.Format("กฎหมาย \"{0}\" แยกหมวดตามปี แต่ไม่พบหมวดปี {1} (ต้องสร้างหมวดย่อยรหัส {2} ใต้ \"{3}\")", lawCode, year, yearCode, law.CategoryName);
            return false;
        }

        /// <summary>true ถ้า <paramref name="categoryCode"/> เป็นรหัสหมวดปีของกฎหมาย <paramref name="lawCode"/>: [รหัสกฎหมาย] ตามด้วยตัวเลข 4 หลัก</summary>
        private static bool IsYearCategoryCode(string categoryCode, string lawCode)
        {
            string code = (categoryCode ?? string.Empty).Trim();
            if (code.Length != lawCode.Length + 4) return false;
            if (!code.StartsWith(lawCode, StringComparison.OrdinalIgnoreCase)) return false;

            for (int i = lawCode.Length; i < code.Length; i++)
            {
                if (code[i] < '0' || code[i] > '9') return false;
            }
            return true;
        }

        /// <summary>
        /// หาหมวดประเภทเอกสารที่ไม่ใช่กฎหมายจากรหัสประเภท (ตัวที่ 25 ของ Code รวม) — หมวดที่ตั้ง <see cref="KBCategory.ImportTypeDigit"/>
        /// เป็นรหัสนั้น (master อยู่ในตาราง m_KBCategory ไม่ hardcode ในโค้ด) ต้องใช้งานอยู่ และไม่อยู่ใต้หมวด "กฎหมาย"
        /// คืน <c>false</c> พร้อมข้อความภาษาไทยใน <paramref name="error"/> เมื่อไม่ผ่าน
        /// </summary>
        public bool TryResolveNonLawType(char typeDigit, out KBCategory category, out string error)
        {
            category = null;

            KBCategory found = _typeMap.FindByDigit(typeDigit);
            if (found == null)
            {
                error = _typeMap.IsEmpty
                    ? "ยังไม่ได้ตั้งรหัสประเภทเอกสารให้หมวดใดเลย (ตั้งที่หน้าหมวดหมู่ ช่อง \"รหัสประเภทเอกสาร\")"
                    : string.Format("ไม่มีหมวดประเภทเอกสารที่ใช้รหัส \"{0}\" (รหัสที่ตั้งไว้แล้ว: {1}) — ประเภทใหม่ให้สร้างหมวดแล้วตั้งรหัสที่หน้าหมวดหมู่ ช่อง \"รหัสประเภทเอกสาร\"",
                        typeDigit, _typeMap.DescribeDigits());
                return false;
            }

            string code = (found.CategoryCode ?? string.Empty).Trim();

            if (IsLawRootOrUnder(found))
            {
                error = string.Format("หมวด \"{0}\" อยู่ใต้หมวด \"กฎหมาย\" ไม่ใช่ประเภทเอกสารที่ไม่ใช่กฎหมาย", code);
                return false;
            }

            if (!found.IsActive)
            {
                error = string.Format("หมวดประเภทเอกสาร \"{0}\" ถูกปิดใช้งาน", code);
                return false;
            }

            category = found;
            error    = null;
            return true;
        }

        /// <summary>
        /// หาหมวดของแถวที่ไม่ใช่กฎหมายจาก Code รวมเต็ม ตามลำดับ:
        /// 1) ตัวที่ 25 (รหัสประเภทเอกสาร) → หมวดประเภทเอกสารที่ตั้ง ImportTypeDigit เป็นรหัสนั้นใน m_KBCategory (<see cref="TryResolveNonLawType"/>)
        /// 2) ต่อรหัสนั้นกับปี = "2" + ตัวที่ 10-12 (เช่น OR00 + 2535 → "OR002535") แล้วหาใน m_KBCategory อีกครั้ง —
        ///    ต้องเป็นหมวดย่อยโดยตรงของหมวดประเภท และเปิดใช้งานอยู่; หมวดนี้คือหมวดที่บทความจะลง
        /// หมวดปีไม่พบ = ไม่ผ่านพร้อม <paramref name="missing"/> บอกว่าต้องสร้างหมวดรหัสอะไรใต้หมวดไหน
        /// (หมวดประเภทไม่พบ = ไม่มี <paramref name="missing"/> เพราะไม่มีรหัสหมวดให้แนะนำ ต้องตั้งรหัสประเภทที่หน้าหมวดหมู่)
        /// </summary>
        public bool TryResolveNonLawArticle(string articleCode, out KBCategory category, out string error, out KBMissingCategory missing)
        {
            category = null;
            missing  = null;

            string code = (articleCode ?? string.Empty).Trim();
            char typeDigit;
            if (!KBImportLayoutTypeCatalog.TryGetNonLawTypeDigit(code, out typeDigit))
            {
                error = KBImportLayoutTypeCatalog.NonLawCodeFormatMessage;
                return false;
            }

            // 1) ประเภทเอกสาร
            KBCategory type;
            if (!TryResolveNonLawType(typeDigit, out type, out error))
                return false;

            // 2) หมวดปี
            string year = KBImportLayoutTypeCatalog.GetLawYear(code);
            if (year == null)
            {
                error = "ตัวที่ 10-12 ของ Code รวมไม่ใช่ตัวเลข จึงหาปีเพื่อเลือกหมวดไม่ได้";
                return false;
            }

            string typeCategoryCode = type.CategoryCode.Trim();
            string yearCode         = typeCategoryCode + year;

            KBCategory found;
            if (!_byCode.TryGetValue(yearCode, out found))
            {
                missing = new KBMissingCategory { CategoryCode = yearCode, ParentCategoryCode = typeCategoryCode, ParentCategoryName = type.CategoryName };
                error   = string.Format("ไม่พบหมวดรหัส \"{0}\" ในระบบ — กรุณาสร้างเป็นหมวดย่อยของ \"{1}\" ({2}) ก่อน", yearCode, type.CategoryName, typeCategoryCode);
                return false;
            }

            if (found.ParentCategoryID != type.CategoryID)
            {
                error = string.Format("หมวด \"{0}\" มีอยู่แต่ไม่ได้อยู่ใต้หมวดประเภทเอกสาร \"{1}\" (ต้องเป็นหมวดย่อยโดยตรงของหมวดนี้)", yearCode, type.CategoryName);
                return false;
            }

            if (!found.IsActive)
            {
                error = string.Format("หมวดปี \"{0}\" ถูกปิดใช้งาน", yearCode);
                return false;
            }

            category = found;
            error    = null;
            return true;
        }

        /// <summary>true ถ้า <paramref name="category"/> เป็นหมวด "กฎหมาย" (LW) เอง หรืออยู่ใต้หมวดนั้น (ไล่ ParentCategoryID ขึ้นไป มีตัวกันวนลูป)</summary>
        private bool IsLawRootOrUnder(KBCategory category)
        {
            var visited = new HashSet<int>();
            KBCategory current = category;

            while (current != null)
            {
                if (string.Equals(current.CategoryCode, KBImportLayoutTypeCatalog.LawRootCategoryCode, StringComparison.OrdinalIgnoreCase))
                    return true;

                if (!current.ParentCategoryID.HasValue || !visited.Add(current.CategoryID))
                    return false;

                KBCategory parent;
                current = _byId.TryGetValue(current.ParentCategoryID.Value, out parent) ? parent : null;
            }
            return false;
        }
    }
}
