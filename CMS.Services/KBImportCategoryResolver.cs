using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Domain.Models;

namespace CMS.Services
{
    /// <summary>
    /// หา CategoryID ของแต่ละแถวตอน import จาก ArticleCode — Template ไม่ผูกหมวดแล้ว จึงใช้ 1 Template ได้หลายกฎหมาย/หลายประเภท:
    /// <list type="bullet">
    /// <item><b>กฎหมาย</b> — 3 ตัวแรกของ Code รวม (เช่น "RVA") ต้องตรงกับ CategoryCode ของหมวดที่อยู่ใต้หมวด "กฎหมาย" (LW);
    /// กฎหมายที่มีหลายปีและแยกหมวดตามปี (เช่น กฎหมาย › พระราชบัญญัติประกันชีวิต (PLI) › PLI2535, PLI2540) ใช้ปีจากตัวที่ 8-10 + "2" นำหน้า
    /// เลือกหมวดปี (ดู <see cref="TryResolveLawArticle"/>) — ลำดับชั้นหมวดอ่านจาก m_KBCategory ที่เดียว เพราะไฟล์ Excel ไม่มีข้อมูลนี้</item>
    /// <item><b>ไม่ใช่กฎหมาย</b> — ตัวที่ 23 ของ Code รวม (1-6 → AN/OR/SO/RG/CI/MR) ต้องตรงกับ CategoryCode ของหมวดที่อยู่นอกหมวด "กฎหมาย"
    /// แล้วต่อรหัสนั้นกับปี (ตัวที่ 8-10 + "2" นำหน้า) เป็นรหัสหมวดปี เช่น OR2535 ที่ต้องมีเป็นหมวดย่อยของหมวดประเภท (ดู
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

        public KBImportCategoryResolver(IEnumerable<KBCategory> categories)
        {
            if (categories == null) throw new ArgumentNullException("categories");

            foreach (var category in categories)
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

        /// <summary>
        /// หาหมวดกฎหมายจากรหัส 3 ตัวแรกของ Code รวม — ต้องมีในระบบ, ใช้งานอยู่, และอยู่ใต้หมวด "กฎหมาย" (ไม่ใช่หมวด "กฎหมาย" เอง)
        /// คืน <c>false</c> พร้อมข้อความภาษาไทยใน <paramref name="error"/> เมื่อไม่ผ่าน
        /// </summary>
        public bool TryResolveLaw(string lawCode, out KBCategory category, out string error)
        {
            category = null;

            string code = (lawCode ?? string.Empty).Trim();
            if (code.Length == 0)
            {
                error = "ไม่พบรหัสกฎหมาย (3 ตัวแรกของ Code รวม)";
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
        /// หาหมวดของแถวกฎหมายจาก Code รวมเต็ม: หมวดกฎหมายจาก 3 ตัวแรก (<see cref="TryResolveLaw"/>) แล้วถ้ากฎหมายนั้นแยกหมวดตามปี
        /// ใช้หมวดปีที่ตรงกับปีใน Code รวม (ตัวที่ 8-10 + "2" นำหน้า เช่น 535 → 2535):
        /// <list type="bullet">
        /// <item>หมวดปีมีรหัส = [รหัสกฎหมาย][พ.ศ. 4 หลัก] (เช่น PLI2535) และต้องเป็นหมวดย่อยโดยตรงของหมวดกฎหมายนั้น</item>
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
        /// คืน <paramref name="missing"/> = หมวดปีที่ต้องสร้าง (เช่น PLI2551 ใต้ PLI) ให้แจ้งผู้ใช้
        /// </summary>
        public bool TryResolveLawArticle(string articleCode, out KBCategory category, out string error, out KBMissingCategory missing)
        {
            category = null;
            missing  = null;

            string code = (articleCode ?? string.Empty).Trim();
            if (code.Length < 3)
            {
                error = "ไม่พบรหัสกฎหมาย (3 ตัวแรกของ Code รวม)";
                return false;
            }

            KBCategory law;
            if (!TryResolveLaw(code.Substring(0, 3), out law, out error))
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
                ? string.Format("กฎหมาย \"{0}\" แยกหมวดตามปี แต่ตัวที่ 8-10 ของ Code รวมไม่ใช่ตัวเลข จึงหาปีไม่ได้", lawCode)
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
        /// หาหมวดประเภทเอกสารที่ไม่ใช่กฎหมายจากรหัสประเภท (AN/OR/SO/RG/CI/MR ที่ได้จากตัวที่ 23 ของ Code รวม) —
        /// ต้องมีในระบบ, ใช้งานอยู่, และไม่อยู่ใต้หมวด "กฎหมาย"
        /// คืน <c>false</c> พร้อมข้อความภาษาไทยใน <paramref name="error"/> เมื่อไม่ผ่าน
        /// </summary>
        public bool TryResolveNonLaw(string typeCode, out KBCategory category, out string error)
        {
            category = null;

            string code = (typeCode ?? string.Empty).Trim();
            if (code.Length == 0)
            {
                error = "ไม่พบรหัสประเภทเอกสาร (ตัวที่ 23 ของ Code รวม)";
                return false;
            }

            KBCategory found;
            if (!_byCode.TryGetValue(code, out found))
            {
                error = string.Format("ไม่พบหมวดประเภทเอกสารรหัส \"{0}\" ในระบบ", code);
                return false;
            }

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
        /// 1) ตัวที่ 23 (1-6 → AN/OR/SO/RG/CI/MR) → หมวดประเภทเอกสารที่ตรง CategoryCode ใน m_KBCategory (<see cref="TryResolveNonLaw"/>)
        /// 2) ต่อรหัสนั้นกับปี = "2" + ตัวที่ 8-10 (เช่น OR + 2535 → "OR2535") แล้วหาใน m_KBCategory อีกครั้ง —
        ///    ต้องเป็นหมวดย่อยโดยตรงของหมวดประเภท และเปิดใช้งานอยู่; หมวดนี้คือหมวดที่บทความจะลง
        /// หมวดไม่พบ (ข้อ 1 หรือ 2) = ไม่ผ่านพร้อม <paramref name="missing"/> บอกว่าต้องสร้างหมวดรหัสอะไรใต้หมวดไหน
        /// </summary>
        public bool TryResolveNonLawArticle(string articleCode, out KBCategory category, out string error, out KBMissingCategory missing)
        {
            category = null;
            missing  = null;

            string code     = (articleCode ?? string.Empty).Trim();
            string typeCode = KBImportLayoutTypeCatalog.GetNonLawCategoryCode(code);
            if (typeCode == null)
            {
                error = "Code รวม: ต้องยาว 27 ตัว, 5 ตัวสุดท้ายห้ามเป็น 00000 และตัวที่ 23 ต้องเป็น 1-6";
                return false;
            }

            // 1) ประเภทเอกสาร
            KBCategory type;
            if (!TryResolveNonLaw(typeCode, out type, out error))
            {
                if (!_byCode.ContainsKey(typeCode))
                {
                    missing = new KBMissingCategory { CategoryCode = typeCode };
                    error  += string.Format(" — กรุณาสร้างหมวดรหัส {0} (หมวดหลัก) ก่อน", typeCode);
                }
                return false;
            }

            // 2) หมวดปี
            string year = KBImportLayoutTypeCatalog.GetLawYear(code);
            if (year == null)
            {
                error = "ตัวที่ 8-10 ของ Code รวมไม่ใช่ตัวเลข จึงหาปีเพื่อเลือกหมวดไม่ได้";
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
