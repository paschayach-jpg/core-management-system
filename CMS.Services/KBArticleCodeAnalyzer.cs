using System;
using System.Collections.Generic;
using System.Text.RegularExpressions;
using CMS.Domain.Models;

namespace CMS.Services
{
    /// <summary>ผลวิเคราะห์ Code รวม (ArticleCode 29 ตัว) ของบทความที่พิมพ์จากหน้า editor — กฎเดียวกับตอน import</summary>
    public class KBArticleCodeAnalysis
    {
        /// <summary><see cref="KBImportLayoutTypeCatalog.Law"/> / <see cref="KBImportLayoutTypeCatalog.Decree"/> / <see cref="KBImportLayoutTypeCatalog.NonLaw"/>; null = ผิดรูปแบบ</summary>
        public string     Layout   { get; set; }

        /// <summary>หมวดที่ใช้เก็บบทความ (หมวดกฎหมาย/หมวดปี/หมวดประเภทเอกสาร) — null ถ้ายังหาไม่ได้</summary>
        public KBCategory Category { get; set; }

        /// <summary>ชื่อบทความที่คำนวณจาก Code ("มาตรา N" ของกฎหมาย/พระราชกฤษฎีกา); null = ผู้ใช้ตั้งชื่อเอง (ไม่ใช่กฎหมาย)</summary>
        public string     Title    { get; set; }

        /// <summary>
        /// ArticleCode ที่เก็บลงฐานข้อมูล: กฎหมาย/พระราชกฤษฎีกาเก็บ 24 ตัวแรก (<see cref="KBImportLayoutTypeCatalog.ParentCodeLength"/>) ส่วนที่ไม่ใช่กฎหมายเก็บเต็ม 29 ตัว
        /// — เหมือน import เพื่อให้ import ซ้ำเจอบทความเดิม (UPDATE) ไม่สร้างซ้ำ
        /// </summary>
        public string     StoredCode { get; set; }

        /// <summary>true = หมวดคำนวณจาก Code ผู้ใช้เลือกเองไม่ได้ (กฎหมาย/ไม่ใช่กฎหมาย); false = พระราชกฤษฎีกาต้องเลือกหมวดกลุ่มเอง</summary>
        public bool       CategoryFromCode { get; set; }

        /// <summary>Code มาตราของกฎหมายแม่ (24 ตัวแรก) ของบทความที่ไม่ใช่กฎหมาย — ใช้ผูก link อัตโนมัติ; null = ไม่มี</summary>
        public string     ParentCode { get; set; }

        /// <summary>ข้อความภาษาไทยบอกเหตุที่ใช้ไม่ได้/ยังต้องทำอะไรต่อ; null = ผ่านทั้งหมด</summary>
        public string     Error    { get; set; }

        public bool IsValid { get { return Error == null; } }

        /// <summary>ชื่อชนิดแบบสั้นสำหรับแสดงผล (กฎหมาย / พระราชกฤษฎีกา / ที่ไม่ใช่กฎหมาย)</summary>
        public string LayoutLabel { get { return KBImportLayoutTypeCatalog.DescribeFileKind(Layout); } }
    }

    /// <summary>
    /// ตรวจ/แยก Code รวม (ArticleCode 29 ตัว) ของบทความใหม่ที่สร้างจากหน้า editor ด้วยกฎเดียวกับ <see cref="KBImportProcessor"/>:
    /// ชนิดจาก <see cref="KBImportLayoutTypeCatalog.DetectLayoutFromCode"/> และหมวดจาก <see cref="KBImportCategoryResolver"/>
    /// — editor จึงสร้างบทความที่ import รับได้ (ไม่มีทางสร้าง Code/หมวดที่ import จะปฏิเสธหรือย้ายหมวดไม่ตรง).
    /// ไม่เรียกฐานข้อมูล (รับรายการหมวดจากผู้เรียก) ตรวจว่า Code ซ้ำ/กฎหมายแม่มีจริงที่ชั้น service
    /// </summary>
    public static class KBArticleCodeAnalyzer
    {
        public const string FormatMessage =
            "Code รวมต้องยาว 29 ตัว — กฎหมาย: ตัวที่ 4-5 = 00 และ 5 ตัวสุดท้าย = 00000; พระราชกฤษฎีกา: ตัวที่ 4-5 ไม่ใช่ 00 และ 5 ตัวสุดท้าย = 00000; " +
            "ที่ไม่ใช่กฎหมาย: 5 ตัวสุดท้ายไม่ใช่ 00000 และตัวที่ 25 เป็นรหัสประเภทเอกสาร (1-9)";

        /// <param name="categories">หมวดทั้งหมด (รวมที่ปิดใช้งาน เพื่อให้ resolver ตัดสินเอง)</param>
        /// <param name="articleCode">Code ที่ผู้ใช้พิมพ์</param>
        /// <param name="chosenGroupCategory">หมวดกลุ่มพระราชกฤษฎีกาที่ผู้ใช้เลือก (ใช้เฉพาะ Decree)</param>
        public static KBArticleCodeAnalysis Analyze(IEnumerable<KBCategory> categories, string articleCode, KBCategory chosenGroupCategory)
        {
            var result = new KBArticleCodeAnalysis();
            string code = (articleCode ?? string.Empty).Trim();

            if (code.Length == 0)
            {
                result.Error = "กรุณากรอก Code รวม (29 ตัว)";
                return result;
            }

            result.Layout = KBImportLayoutTypeCatalog.DetectLayoutFromCode(code);
            if (result.Layout == null)
            {
                result.Error = FormatMessage;
                return result;
            }

            result.StoredCode = result.Layout == KBImportLayoutTypeCatalog.NonLaw
                ? code
                : code.Substring(0, KBImportLayoutTypeCatalog.ParentCodeLength);

            var resolver = new KBImportCategoryResolver(categories ?? new List<KBCategory>());
            KBCategory        category;
            string            error;
            KBMissingCategory missing;

            if (result.Layout == KBImportLayoutTypeCatalog.NonLaw)
            {
                if (!resolver.TryResolveNonLawArticle(code, out category, out error, out missing))
                {
                    result.Error = AppendMissing(error, missing);
                    return result;
                }

                result.Category         = category;
                result.CategoryFromCode = true;
                result.ParentCode       = code.Substring(0, KBImportLayoutTypeCatalog.ParentCodeLength);
                return result;
            }

            // Law / Decree: ชื่อ = "มาตรา N" จากตัวที่ 6-9 (เหมือน KBImportProcessor.ValidateLawRow)
            string sectionDigits = code.Substring(KBImportLayoutTypeCatalog.LawSectionStartIndex, KBImportLayoutTypeCatalog.LawSectionDigits);
            int    sectionNumber;
            if (!Regex.IsMatch(sectionDigits, @"^\d{4}$") || !int.TryParse(sectionDigits, out sectionNumber) || sectionNumber <= 0)
            {
                result.Error = string.Format("เลขมาตราไม่ถูกต้อง: ตัวที่ 6-9 ของ '{0}' ต้องเป็นตัวเลข 4 หลักและมากกว่า 0", code);
                return result;
            }
            result.Title = "มาตรา " + sectionNumber;

            if (result.Layout == KBImportLayoutTypeCatalog.Law)
            {
                if (!resolver.TryResolveLawArticle(code, out category, out error, out missing))
                {
                    result.Error = AppendMissing(error, missing);
                    return result;
                }

                result.Category         = category;
                result.CategoryFromCode = true;
                return result;
            }

            // Decree: หมวดมาจากหมวดกลุ่มที่ผู้ใช้เลือก (ต้องอยู่ใต้ DDD00) ไม่ใช่จาก 5 ตัวแรกของ Code
            if (chosenGroupCategory == null)
            {
                result.Error = string.Format("เลือกหมวดกลุ่มพระราชกฤษฎีกา (หมวดย่อยของ {0}) ก่อน", KBImportLayoutTypeCatalog.DecreeRootCategoryCode);
                return result;
            }

            if (!resolver.TryResolveDecreeArticle(code, chosenGroupCategory.CategoryCode, out category, out error, out missing))
            {
                result.Error = AppendMissing(error, missing);
                return result;
            }

            result.Category = category;
            return result;
        }

        private static string AppendMissing(string error, KBMissingCategory missing)
        {
            if (missing == null || string.IsNullOrEmpty(missing.CategoryCode)) return error;
            return string.Format("{0} (ต้องสร้างหมวด {1}{2} ก่อน)", error, missing.CategoryCode,
                string.IsNullOrEmpty(missing.ParentCategoryCode) ? string.Empty : " ใต้ " + missing.ParentCategoryCode);
        }
    }
}
