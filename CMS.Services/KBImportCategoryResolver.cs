using System;
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services
{
    /// <summary>
    /// หา CategoryID ของแต่ละแถวตอน import จาก ArticleCode — Template ไม่ผูกหมวดแล้ว จึงใช้ 1 Template ได้หลายกฎหมาย/หลายประเภท:
    /// <list type="bullet">
    /// <item><b>กฎหมาย</b> — 3 ตัวแรกของ Code รวม (เช่น "RVA") ต้องตรงกับ CategoryCode ของหมวดที่อยู่ใต้หมวด "กฎหมาย" (LW)</item>
    /// <item><b>ไม่ใช่กฎหมาย</b> — ตัวที่ 23 ของ Code รวม (1-6 → AN/OR/SO/RG/CI/MR) ต้องตรงกับ CategoryCode ของหมวดที่อยู่นอกหมวด "กฎหมาย"</item>
    /// </list>
    /// สร้างจากรายการหมวดทั้งหมดครั้งเดียวต่อการ validate หนึ่งไฟล์ (ไม่เรียกฐานข้อมูลเอง) — CategoryCode เป็น UNIQUE
    /// (UQ_KMS_KBCat_Code) จึงหาหมวดจากรหัสได้แน่นอนตัวเดียว
    /// </summary>
    public class KBImportCategoryResolver
    {
        private readonly Dictionary<string, KBCategory> _byCode = new Dictionary<string, KBCategory>(StringComparer.OrdinalIgnoreCase);
        private readonly Dictionary<int, KBCategory>    _byId   = new Dictionary<int, KBCategory>();

        public KBImportCategoryResolver(IEnumerable<KBCategory> categories)
        {
            if (categories == null) throw new ArgumentNullException("categories");

            foreach (var category in categories)
            {
                if (category == null) continue;

                _byId[category.CategoryID] = category;

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
