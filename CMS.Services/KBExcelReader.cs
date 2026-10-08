using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text.RegularExpressions;
using CMS.Domain.Models;
using CMS.Services.Interfaces;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace CMS.Services
{
    /// <summary>
    /// Reads a KB import Excel file — header row 1, data from row 2, first worksheet only —
    /// and maps each data row to a <see cref="KBImportRow"/>. Auto-detects which of the two
    /// supported layouts the file uses from its header row (column C text) and reads
    /// accordingly:
    ///
    /// <b>Law</b> (20-column "law-matrix", header C = "มาตรา"): A=Code รวม  C=มาตรา  M=ชื่อ
    /// N=วันที่ประกาศ  O=วันที่มีผลบังคับใช้  P=บัญญัติไว้ว่า คำสั่งที่เกี่ยวข้อง  S=กฎหมายอ้างอิง
    /// (other columns, including B "Code กฎหมายแม่", are ignored for now).
    ///
    /// <b>NonLaw</b> (12-column, header C = "ประเภทลูก" — OR/AN/SO/RG/CI/MR): A=Code รวม (สูตร —
    /// อ่านผ่าน mapping ArticleCode ของ Template)  B=Code กฎหมายแม่(หลายบรรทัด)  C=ประเภทลูก (1=ประกาศ, 2=คำสั่ง)
    /// D=เอกสารลูก (ไม่อ่านแล้ว)  E=ชื่อ  F=คำสั่งที่  G=วันที่ประกาศ  H=วันที่มีผลบังคับใช้  I=บัญญัติไว้ว่า
    /// J=ประเด็นสำคัญ  K=ชื่อไฟล์(+hyperlink)  L=กฎหมายอ้างอิง (ไม่อ่าน).
    ///
    /// Uses NPOI 2.5.6, evaluating formula cells live (a saved-but-not-recalculated workbook
    /// otherwise leaves formula cells' cached value blank). No business-rule validation is
    /// performed here — that is the responsibility of <see cref="KBImportProcessor"/>.
    ///
    /// ArticleCode (Code รวม) บังคับ map ในทุก Template — Law อ่านฟิลด์ที่เหลือจากตำแหน่งตายตัวข้างบนเป็นค่าเริ่มต้น
    /// (Title/TitleCategory/Summary/ContentHTML/PublishedDate/EffectiveDate ทับได้ด้วย mapping) ส่วน NonLaw อ่านทุกฟิลด์
    /// (รวม Attachment) จาก Template mapping เท่านั้น ไม่มีตำแหน่งตายตัว (see <see cref="ApplyFieldOverrides"/>).
    /// CategoryID/ArticleType มาจากค่าคงที่ใน Template
    /// </summary>
    public class KBExcelReader : IKBExcelReaderService
    {
        private const int DataStartRowIndex = 1;   // แถว 2 ใน Excel

        /// <summary>Target column ที่ยอม override ตำแหน่งตายตัวด้วย Template mapping ได้ (ไม่รวม ArticleCode/CategoryID/ArticleType)</summary>
        private static readonly string[] OverridableTargetColumns =
            { "ArticleCode", "Title", "TitleCategory", "Summary", "ContentHTML", "PublishedDate", "EffectiveDate", "Attachment", KBArticleColumnCatalog.LawReferenceTargetColumn };

        /// <summary>จับ token รูปแบบ {ชื่อคอลัมน์} ในค่า mapping ของ Template เช่น "{C}" ใน "มาตรา {C}"</summary>
        private static readonly Regex TemplateExpressionPlaceholderPattern = new Regex(@"\{([^{}]+)\}", RegexOptions.Compiled);

        private static readonly string[] DateFormats =
        {
            "d/M/yyyy",
            "dd/MM/yyyy",
            "yyyy-MM-dd",
            "yyyy/MM/dd"
        };

        private static readonly Dictionary<string, int> ThaiMonths =
            new Dictionary<string, int>(StringComparer.OrdinalIgnoreCase)
        {
            { "มกราคม",   1 }, { "ม.ค.",  1 },
            { "กุมภาพันธ์", 2 }, { "ก.พ.", 2 },
            { "มีนาคม",   3 }, { "มี.ค.", 3 },
            { "เมษายน",   4 }, { "เม.ย.", 4 },
            { "พฤษภาคม",  5 }, { "พ.ค.",  5 },
            { "มิถุนายน", 6 }, { "มิ.ย.", 6 },
            { "กรกฎาคม",  7 }, { "ก.ค.",  7 },
            { "สิงหาคม",  8 }, { "ส.ค.",  8 },
            { "กันยายน",  9 }, { "ก.ย.",  9 },
            { "ตุลาคม",  10 }, { "ต.ค.", 10 },
            { "พฤศจิกายน",11 }, { "พ.ย.", 11 },
            { "ธันวาคม", 12 }, { "ธ.ค.", 12 },
        };

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>
        /// Opens the file at <paramref name="filePath"/> and returns one
        /// <see cref="KBImportRow"/> per data row (row 2 onward on the first worksheet) in
        /// <see cref="KBExcelReadResult.Rows"/> (empty when the file has no data rows), plus the layout detected
        /// from the file and the Template mappings that point at a column/header the file does not have
        /// (<see cref="KBExcelReadResult.MappingProblems"/>).
        /// </summary>
        /// <exception cref="KBExcelReadException">
        /// Thrown when the file does not exist, is not a valid <c>.xlsx</c>, is locked by
        /// another process, or its ArticleCode (Code รวม) is malformed so the layout cannot be determined.
        /// </exception>
        public virtual KBExcelReadResult ReadRows(string filePath, IList<KBImportTemplateColumnRow> templateColumns)
        {
            IWorkbook workbook = OpenWorkbook(filePath);

            var rows   = new List<KBImportRow>();
            var result = new KBExcelReadResult { Rows = rows };

            ISheet sheet = workbook.GetSheetAt(0);
            if (sheet == null)
                return result;

            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            IRow headerRow = sheet.GetRow(0);

            var overrides = BuildFieldOverrides(templateColumns);

            string layout = DetectLayout(filePath, sheet, headerRow, evaluator, overrides);
            if (layout == null)
                return result;   // ไม่มีแถวข้อมูลเลย — ไม่มีอะไรให้ตรวจ/นำเข้า

            // พระราชกฤษฎีกามีรูปแบบแถวเหมือนกฎหมาย (Code รวมลงท้าย 00000) — DetectedLayout บอกชนิดไฟล์ให้ผู้เรียกเทียบกับ Template,
            // ส่วน rowKind ที่ใช้อ่านแถวยังเป็น Law
            result.DetectedLayout = layout;
            string rowKind = layout == KBImportLayoutTypeCatalog.Decree ? KBImportLayoutTypeCatalog.Law : layout;
            int columnCount = ComputeColumnCount(sheet);

            int lastRowIndex = sheet.LastRowNum;
            for (int rowIdx = DataStartRowIndex; rowIdx <= lastRowIndex; rowIdx++)
            {
                IRow excelRow = sheet.GetRow(rowIdx);
                if (excelRow == null)
                    continue;

                // Skip entirely blank rows (all cells null or empty).
                if (IsRowBlank(excelRow, evaluator))
                    continue;

                KBImportRow row;
                try
                {
                    row = rowKind == KBImportLayoutTypeCatalog.NonLaw
                        ? ReadNonLawRow(excelRow, rowIdx, evaluator)
                        : ReadLawRow(excelRow, rowIdx, evaluator);

                    ApplyFieldOverrides(row, excelRow, headerRow, evaluator, overrides);
                }
                catch (Exception ex)
                {
                    // One unreadable row must not abort the entire file read.
                    row = new KBImportRow
                    {
                        RowNumber  = rowIdx + 1,
                        RowKind    = rowKind,
                        Status     = "Error",
                        StatusNote = SafeTruncate("อ่านข้อมูล Excel ไม่สำเร็จ: " + ex.Message, 200),
                    };
                }

                // NonLaw: เซลล์ ArticleCode ที่มีหลาย Code แตกเป็น 1 row ต่อ Code (Law = 1 row ต่อบรรทัด Excel เสมอ)
                if (rowKind == KBImportLayoutTypeCatalog.NonLaw)
                    rows.AddRange(ExpandArticleCodes(row));
                else
                    rows.Add(row);
            }

            // mapping ของ Template ที่ชี้ไปคอลัมน์/หัวตารางที่ไม่มีในไฟล์ (เดิมได้ค่าว่างเงียบ ๆ ทำให้เนื้อหาหายโดยไม่มี error)
            result.MappingProblems = FindMappingProblems(overrides, headerRow, evaluator, columnCount, rowKind);
            return result;
        }

        /// <summary>
        /// NonLaw: เซลล์ ArticleCode ที่มีหลาย Code (หลายบรรทัด เช่นสูตรที่ต่อ Code ให้ทุกกฎหมายแม่) แตกเป็น 1 <see cref="KBImportRow"/> ต่อ Code
        /// — ฟิลด์อื่น (Title/เนื้อหา/ไฟล์แนบ ฯลฯ) เหมือนกันทุก row; กฎหมายแม่ของแต่ละ Code = 24 ตัวแรกของ Code นั้นเอง และแต่ละ Code
        /// ผ่าน/ข้าม/ล้มเหลวแยกกัน (ไม่ข้ามทั้ง Excel row). เซลล์ว่างหรือมี Code เดียว = คืน row เดิม
        /// </summary>
        private static IEnumerable<KBImportRow> ExpandArticleCodes(KBImportRow row)
        {
            IList<string> codes = SplitLines(row.ColA);
            if (codes.Count <= 1)
                return new[] { row };

            var expanded = new List<KBImportRow>(codes.Count);
            for (int i = 0; i < codes.Count; i++)
            {
                KBImportRow copy = row.Clone();
                copy.ColA      = codes[i];
                copy.CodeIndex = i + 1;
                copy.CodeCount = codes.Count;
                expanded.Add(copy);
            }
            return expanded;
        }

        // ── Layout detection ─────────────────────────────────────────────────

        /// <summary>
        /// Detects which layout a file uses จากค่า Code รวม (mapping ArticleCode ของ Template) ของแถวข้อมูลแรกที่ Code ไม่ว่าง
        /// (<see cref="KBImportLayoutTypeCatalog.DetectLayoutFromCode"/>): Law = ลงท้าย 00000 และตัวที่ 4-5 เป็น 00,
        /// Decree = ลงท้าย 00000 และตัวที่ 4-5 ไม่ใช่ 00, NonLaw = รูปแบบไฟล์ที่ไม่ใช่กฎหมาย.
        /// ไม่อ่านหัวตาราง/คอลัมน์อื่นตามตำแหน่งตายตัว — Code ผิดรูปแบบทุกแบบ (หรือไม่มี Code เลยทั้งที่มีข้อมูล) = ไม่ให้ทำรายการต่อ
        /// (throw <see cref="KBExcelReadException"/> พร้อมบอกแถวและรูปแบบที่ถูกต้อง). คืน <c>null</c> เฉพาะเมื่อไฟล์ไม่มีแถวข้อมูลเลย
        /// </summary>
        private static string DetectLayout(string filePath, ISheet sheet, IRow headerRow, IFormulaEvaluator evaluator, Dictionary<string, string> overrides)
        {
            string articleCodeExpr;
            if (!overrides.TryGetValue("ArticleCode", out articleCodeExpr))
                throw new KBExcelReadException(filePath, "Template ไม่ได้ map คอลัมน์ ArticleCode (Code รวม) จึงแยกประเภทไฟล์ไม่ได้ — กรุณาแก้ Template ที่หน้า config");

            bool hasData = false;
            for (int rowIdx = DataStartRowIndex; rowIdx <= sheet.LastRowNum; rowIdx++)
            {
                IRow excelRow = sheet.GetRow(rowIdx);
                if (excelRow == null || IsRowBlank(excelRow, evaluator)) continue;
                hasData = true;

                string code = FirstLine(ResolveTemplateExpression(articleCodeExpr, headerRow, excelRow, evaluator));
                if (string.IsNullOrEmpty(code)) continue;

                string layout = KBImportLayoutTypeCatalog.DetectLayoutFromCode(code);
                if (layout != null) return layout;

                throw new KBExcelReadException(filePath, string.Format(
                    "Code รวม (ArticleCode) ของแถว {0} ผิดรูปแบบ: \"{1}\" (ยาว {2} ตัว) จึงแยกประเภทไฟล์ไม่ได้ — กรุณาแก้ Code ในไฟล์ก่อน\n" +
                    "• กฎหมาย: ยาว {3} ตัว, ตัวที่ 4-5 เป็น {4} และ 5 ตัวสุดท้ายเป็น {5}\n" +
                    "• พระราชกฤษฎีกา: ยาว {3} ตัว, ตัวที่ 4-5 ไม่ใช่ {4} และ 5 ตัวสุดท้ายเป็น {5}\n" +
                    "• ไม่ใช่กฎหมาย: {6}",
                    rowIdx + 1, code, code.Length, KBImportLayoutTypeCatalog.LawCodeLength, KBImportLayoutTypeCatalog.LawSeriesLaw,
                    KBImportLayoutTypeCatalog.LawCodeSuffix, KBImportLayoutTypeCatalog.NonLawCodeFormatMessage));
            }

            if (hasData)
                throw new KBExcelReadException(filePath, "ไม่พบ Code รวม (ArticleCode) ในไฟล์เลย (ตามคอลัมน์ที่ Template map ไว้) จึงแยกประเภทไฟล์ไม่ได้");

            return null;
        }

        /// <summary>บรรทัดแรกที่ไม่ว่างของข้อความหลายบรรทัด (เซลล์ที่ใส่หลายค่าด้วย Alt+Enter) — ว่างถ้าไม่มี</summary>
        private static string FirstLine(string raw)
        {
            var lines = SplitLines(raw);
            return lines.Count > 0 ? lines[0] : string.Empty;
        }

        // ── Row readers ──────────────────────────────────────────────────────

        private static KBImportRow ReadLawRow(IRow excelRow, int rowIdx, IFormulaEvaluator evaluator)
        {
            var row = new KBImportRow
            {
                RowNumber = rowIdx + 1,   // 1-based: row index 1 → RowNumber 2
                RowKind   = KBImportLayoutTypeCatalog.Law,

                // Code รวม (ArticleCode) มาจาก Template mapping (ApplyFieldOverrides) ไม่อ่านคอลัมน์ A ตามตำแหน่ง
                RawSummary      = GetCellText(excelRow.GetCell(12), evaluator),          // M — ชื่อ (ข้อความล้วน: ไม่ Trim — เก็บเว้นวรรค/ขึ้นบรรทัดใหม่ตามต้นฉบับ)
                RawPublishedDate = GetCellText(excelRow.GetCell(13), evaluator).Trim(),  // N — วันที่ประกาศ
                RawEffectiveDate = GetCellText(excelRow.GetCell(14), evaluator).Trim(),  // O — วันที่มีผลบังคับใช้
                RawContent      = GetCellText(excelRow.GetCell(15), evaluator),          // P — บัญญัติไว้ว่า
                RawTitleCategory = GetCellText(excelRow.GetCell(18), evaluator),         // S — กฎหมายอ้างอิง (ข้อความล้วน: ไม่ Trim)
            };

            string ignoredError;
            row.PublishedDateParsed = TryParseDateCell(excelRow.GetCell(13), evaluator, out ignoredError);
            row.EffectiveDateParsed = TryParseDateCell(excelRow.GetCell(14), evaluator, out ignoredError);

            return row;
        }

        private static KBImportRow ReadNonLawRow(IRow excelRow, int rowIdx, IFormulaEvaluator evaluator)
        {
            var row = new KBImportRow
            {
                RowNumber = rowIdx + 1,
                RowKind   = KBImportLayoutTypeCatalog.NonLaw,

            };

            // ทุกฟิลด์ของ NonLaw มาจาก Template mapping (ApplyFieldOverrides): ArticleCode (กฎหมายแม่ = 24 ตัวแรก),
            // Title/TitleCategory/PublishedDate/EffectiveDate/ContentHTML/Summary และ Attachment — ไม่อ่านตามตำแหน่งตายตัว
            // คอลัมน์ L (กฎหมายอ้างอิง) ไม่ถูกใช้งาน จึงไม่อ่าน

            return row;
        }

        // ── Template mapping overrides ───────────────────────────────────────

        /// <summary>
        /// สร้าง lookup ของ target column → ExcelColumnRef expression จาก mapping ที่ config ไว้ใน
        /// Template (เฉพาะ <see cref="OverridableTargetColumns"/>, ข้าม CategoryID/ArticleType
        /// และแถวที่ยังไม่ได้ map) — ว่างเปล่าถ้า <paramref name="templateColumns"/> เป็น null
        /// </summary>
        private static Dictionary<string, string> BuildFieldOverrides(IList<KBImportTemplateColumnRow> templateColumns)
        {
            var result = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);
            if (templateColumns == null) return result;

            foreach (var col in templateColumns)
            {
                if (col == null || string.IsNullOrWhiteSpace(col.ExcelColumnRef)) continue;

                foreach (var name in OverridableTargetColumns)
                {
                    if (string.Equals(col.TargetColumnName, name, StringComparison.OrdinalIgnoreCase))
                    {
                        result[name] = col.ExcelColumnRef.Trim();
                        break;
                    }
                }
            }
            return result;
        }

        /// <summary>
        /// Overwrite ค่า Raw*/parsed-date ของ <paramref name="row"/> (ที่ ReadLawRow/ReadNonLawRow ตั้งไว้
        /// แบบตำแหน่งตายตัวเป็น baseline) ด้วยค่าที่ resolve จาก Template — เฉพาะ target ที่มี mapping ใน
        /// <paramref name="overrides"/> เท่านั้น target ที่ไม่ได้ map ยังคงค่าตำแหน่งตายตัวเดิมไว้
        /// </summary>
        private static void ApplyFieldOverrides(
            KBImportRow row, IRow excelRow, IRow headerRow, IFormulaEvaluator evaluator,
            Dictionary<string, string> overrides)
        {
            string expr;

            // ArticleCode: คอลัมน์ที่ Template map ไว้คือ "Code รวม" 29 หลัก — Law ใช้แทน column A ตายตัว,
            // Order ใช้เทียบประเภท/หมวดกับ Template เท่านั้น (ArticleCode ของ Order ยังคำนวณจาก B+C+D)
            if (overrides.TryGetValue("ArticleCode", out expr))
                row.ColA = ResolveTemplateExpression(expr, headerRow, excelRow, evaluator).Trim();

            // ไฟล์แนบ (ทุกประเภทไฟล์): ข้อความในเซลล์คือชื่อไฟล์ ส่วน URL จริงอยู่ใน hyperlink ของเซลล์ — ไม่ map = AttachmentFileName เป็น null (ไม่มีขั้นไฟล์แนบ)
            if (overrides.TryGetValue("Attachment", out expr))
                ApplyAttachmentOverride(expr, headerRow, excelRow, evaluator, row);

            // พระราชกฤษฎีกา (Template กฎหมายที่ map LawReference): ข้อความหลายบรรทัดของ "Code กฎหมายอ้างอิง (ออกตาม)" — หมวด 5 ตัว + มาตรา 24 ตัวต่อบรรทัด
            // (แยกและตรวจที่ KBImportProcessor) ไม่ map = null = Law ปกติ
            if (row.RowKind == KBImportLayoutTypeCatalog.Law && overrides.TryGetValue(KBArticleColumnCatalog.LawReferenceTargetColumn, out expr))
                row.RawLawReference = ResolveTemplateExpression(expr, headerRow, excelRow, evaluator);

            // Title/TitleCategory/Summary/ContentHTML เป็นข้อความที่ต้องเหมือนต้นฉบับ — ไม่ Trim เว้นวรรคหัว-ท้าย/ขึ้นบรรทัดใหม่
            // (การแปลงเป็นรูปแบบที่เก็บลง DB ทำที่ KBImportProcessor: ContentHTML → HTML, ที่เหลือ → ข้อความล้วน)
            if (overrides.TryGetValue("Title", out expr))
                row.RawTitle = ResolveTemplateExpression(expr, headerRow, excelRow, evaluator, true);

            if (overrides.TryGetValue("TitleCategory", out expr))
                row.RawTitleCategory = ResolveTemplateExpression(expr, headerRow, excelRow, evaluator, true);

            if (overrides.TryGetValue("Summary", out expr))
                row.RawSummary = ResolveTemplateExpression(expr, headerRow, excelRow, evaluator, true);

            if (overrides.TryGetValue("ContentHTML", out expr))
                row.RawContent = ResolveTemplateExpression(expr, headerRow, excelRow, evaluator, true);

            if (overrides.TryGetValue("PublishedDate", out expr))
            {
                string rawText;
                DateTime? parsed;
                ApplyDateOverride(expr, headerRow, excelRow, evaluator, out rawText, out parsed);
                row.RawPublishedDate    = rawText;
                row.PublishedDateParsed = parsed;
            }

            if (overrides.TryGetValue("EffectiveDate", out expr))
            {
                string rawText;
                DateTime? parsed;
                ApplyDateOverride(expr, headerRow, excelRow, evaluator, out rawText, out parsed);
                row.RawEffectiveDate    = rawText;
                row.EffectiveDateParsed = parsed;
            }
        }

        /// <summary>
        /// Resolve ค่า mapping "Attachment": column ref ตรง ๆ (ไม่มี {}) อ่านทั้งชื่อไฟล์และ hyperlink จากเซลล์นั้น;
        /// formula หรือ column ref ที่หาไม่เจอ ได้เฉพาะข้อความชื่อไฟล์ (ไม่มี URL)
        /// </summary>
        private static void ApplyAttachmentOverride(
            string expression, IRow headerRow, IRow dataRow, IFormulaEvaluator evaluator, KBImportRow row)
        {
            if (expression.IndexOf('{') < 0)
            {
                int colIndex = ResolveColumnIndex(expression.Trim(), headerRow, evaluator);
                if (colIndex >= 0)
                {
                    ICell cell = dataRow.GetCell(colIndex);
                    row.AttachmentFileName = GetCellText(cell, evaluator).Trim();
                    row.AttachmentUrl      = GetHyperlinkAddress(cell);
                    return;
                }
            }

            row.AttachmentFileName = ResolveTemplateExpression(expression, headerRow, dataRow, evaluator).Trim();
            row.AttachmentUrl      = null;
        }

        /// <summary>
        /// Resolve ค่า mapping ของ Template สำหรับ PublishedDate/EffectiveDate — ถ้าเป็น column ref ตรง ๆ
        /// (ไม่มี {}) resolve ไปที่ ICell จริงแล้วใช้ TryParseDateCell (รองรับ native Excel date-serial
        /// cell) ถ้าเป็น formula (หรือ column ref ที่หาไม่เจอ) ให้ resolve เป็น text ก่อนแล้ว parse ด้วย
        /// TryParseDateText แทน (คงไว้เป็นค่าว่าง/null ถ้า resolve/parse ไม่ได้ ไม่ throw)
        /// </summary>
        private static void ApplyDateOverride(
            string expression, IRow headerRow, IRow dataRow, IFormulaEvaluator evaluator,
            out string rawText, out DateTime? parsed)
        {
            parsed = null;

            if (expression.IndexOf('{') < 0)
            {
                int colIndex = ResolveColumnIndex(expression.Trim(), headerRow, evaluator);
                if (colIndex >= 0)
                {
                    ICell cell = dataRow.GetCell(colIndex);
                    rawText = GetCellText(cell, evaluator).Trim();
                    string ignoredError;
                    parsed = TryParseDateCell(cell, evaluator, out ignoredError);
                    return;
                }
            }

            // formula composition (หรือ column ref ที่หาไม่เจอ) — เหลือแค่ text ให้ parse แบบ text ล้วน
            rawText = ResolveTemplateExpression(expression, headerRow, dataRow, evaluator);
            string ignoredTextError;
            parsed = TryParseDateText(rawText, out ignoredTextError);
        }

        /// <summary>
        /// Resolve ค่า ExcelColumnRef ของ Template หนึ่งช่อง — ถ้ามี {ชื่อคอลัมน์} ผสมอยู่ จะแทนที่แต่ละ
        /// token ด้วยค่าของคอลัมน์นั้นในแถวเดียวกัน (เช่น "มาตรา {C}") โดย normalize ค่าตัวเลขล้วน (ตัด
        /// เลขศูนย์นำหน้า เช่น "0001"→"1") ก่อนแทรก เพื่อให้ได้ผลแบบเดียวกับตอน hardcode เดิมที่ใช้
        /// int.Parse ตรง ๆ — ถ้าไม่มี {} เลยถือว่าทั้งสตริงคือ column ref เดียว resolve ตรง ๆ แบบ raw text
        /// (ไม่ normalize เพราะอาจตั้งใจ map ค่าที่มีเลขศูนย์นำหน้าแบบมีความหมาย เช่น รหัสเอกสาร)
        /// <paramref name="preserveWhitespace"/> = true (ข้อความ Title/TitleCategory/Summary/ContentHTML) ไม่ Trim ค่าที่อ่านจากเซลล์
        /// เพื่อคงเว้นวรรค/indent/ขึ้นบรรทัดใหม่ตามต้นฉบับ; false (รหัส วันที่ ชื่อไฟล์) Trim เหมือนเดิม
        /// </summary>
        private static string ResolveTemplateExpression(
            string expression, IRow headerRow, IRow dataRow, IFormulaEvaluator evaluator, bool preserveWhitespace = false)
        {
            if (string.IsNullOrWhiteSpace(expression)) return string.Empty;

            if (expression.IndexOf('{') < 0)
                return ResolveColumnRef(expression.Trim(), headerRow, dataRow, evaluator, preserveWhitespace);

            return TemplateExpressionPlaceholderPattern.Replace(expression,
                m => NormalizeNumericToken(ResolveColumnRef(m.Groups[1].Value.Trim(), headerRow, dataRow, evaluator, preserveWhitespace)));
        }

        /// <summary>ถ้า raw (ไม่นับเว้นวรรคหัว-ท้าย) เป็นตัวเลขล้วน แปลงผ่าน int แล้ว ToString กลับ (ตัดเลขศูนย์นำหน้า) ไม่งั้นคืนค่าเดิมตามที่ส่งมา</summary>
        private static string NormalizeNumericToken(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return raw;
            string probe = raw.Trim();
            int number;
            if (Regex.IsMatch(probe, @"^\d+$") && int.TryParse(probe, out number))
                return number.ToString(CultureInfo.InvariantCulture);
            return raw;
        }

        /// <summary>คืนค่า text ของเซลล์ที่ colRef ชี้ถึง (ลองตีความเป็นตัวอักษรคอลัมน์ก่อน ไม่ได้ลองจับคู่หัวตาราง) คืนค่าว่างถ้า resolve ไม่ได้เลย</summary>
        private static string ResolveColumnRef(
            string colRef, IRow headerRow, IRow dataRow, IFormulaEvaluator evaluator, bool preserveWhitespace = false)
        {
            int colIndex = ResolveColumnIndex(colRef, headerRow, evaluator);
            if (colIndex < 0) return string.Empty;

            string text = GetCellText(dataRow.GetCell(colIndex), evaluator);
            return preserveWhitespace ? text : text.Trim();
        }

        private static int ResolveColumnIndex(string colRef, IRow headerRow, IFormulaEvaluator evaluator)
        {
            int letterIndex = ColumnLetterToIndex(colRef);
            int headerWidth = headerRow == null ? 0 : Math.Max((int)headerRow.LastCellNum, 0);

            // ตัวอักษรคอลัมน์ที่อยู่ในความกว้างของแถวหัวตาราง → ใช้ตามนั้น
            if (letterIndex >= 0 && letterIndex < headerWidth)
                return letterIndex;

            // ข้อความล้วนตัวอักษรอาจเป็นหัวตารางภาษาอังกฤษ (เช่น "Summary") ไม่ใช่ตัวอักษรคอลัมน์ — ลองจับคู่หัวตารางก่อน
            int headerIndex = FindColumnIndexByHeaderText(headerRow, colRef, evaluator);
            if (headerIndex >= 0)
                return headerIndex;

            // ไม่เจอหัวตาราง: คงพฤติกรรมเดิม (ตัวอักษรที่เกินความกว้างหัวตารางอาจยังมีข้อมูลอยู่ใต้หัวตารางที่ว่าง)
            return letterIndex;
        }

        /// <summary>แปลงตัวอักษรคอลัมน์ Excel ("A".."Z","AA"...) เป็น index เริ่มที่ 0 คืน -1 ถ้าไม่ใช่รูปแบบตัวอักษรล้วน</summary>
        private static int ColumnLetterToIndex(string letters)
        {
            if (string.IsNullOrEmpty(letters)) return -1;

            int result = 0;
            foreach (char c in letters)
            {
                char up = char.ToUpperInvariant(c);
                if (up < 'A' || up > 'Z') return -1;
                result = result * 26 + (up - 'A' + 1);
            }
            return result - 1;
        }

        /// <summary>หา column index โดยจับคู่ข้อความหัวตาราง (case-insensitive) — สำหรับกรณี admin พิมพ์ชื่อหัวข้อแทนตัวอักษรคอลัมน์</summary>
        private static int FindColumnIndexByHeaderText(IRow headerRow, string headerText, IFormulaEvaluator evaluator)
        {
            if (headerRow == null || string.IsNullOrWhiteSpace(headerText)) return -1;

            for (int c = headerRow.FirstCellNum; c < headerRow.LastCellNum; c++)
            {
                if (c < 0) continue;
                string text = GetCellText(headerRow.GetCell(c), evaluator).Trim();
                if (string.Equals(text, headerText, StringComparison.OrdinalIgnoreCase))
                    return c;
            }
            return -1;
        }

        // ── Template mapping ↔ โครงสร้างไฟล์ ──────────────────────────────────

        /// <summary>จำนวนคอลัมน์สูงสุดที่พบในไฟล์ (นับจากทุกแถวของ sheet แรก รวมแถวหัวตาราง)</summary>
        private static int ComputeColumnCount(ISheet sheet)
        {
            int count = 0;
            for (int i = sheet.FirstRowNum; i <= sheet.LastRowNum; i++)
            {
                IRow row = sheet.GetRow(i);
                if (row != null && row.LastCellNum > count)
                    count = row.LastCellNum;
            }
            return count;
        }

        /// <summary>
        /// ตรวจว่าทุก mapping ของ Template (column ref ตรง ๆ และ {X} ในสูตร) ชี้ไปคอลัมน์/หัวตารางที่มีอยู่จริงในไฟล์ —
        /// ref ที่หาไม่เจอจะอ่านได้ค่าว่างเงียบ ๆ ทำให้เนื้อหาหายหรือผิดคอลัมน์โดยไม่มี error จึงรายงานล่วงหน้า
        /// </summary>
        private static IList<string> FindMappingProblems(
            Dictionary<string, string> overrides, IRow headerRow, IFormulaEvaluator evaluator, int columnCount, string rowKind)
        {
            var problems = new List<string>();

            foreach (var kvp in overrides)
            {
                foreach (string colRef in ExtractColumnRefs(kvp.Value))
                {
                    int letterIndex = ColumnLetterToIndex(colRef);
                    if (letterIndex >= 0 && letterIndex < columnCount) continue;                    // ตัวอักษรคอลัมน์ที่มีในไฟล์
                    if (FindColumnIndexByHeaderText(headerRow, colRef, evaluator) >= 0) continue;   // หัวตารางที่มีในไฟล์

                    problems.Add(letterIndex >= 0
                        ? string.Format("{0} → \"{1}\": ไม่พบคอลัมน์นี้ในไฟล์ (ไฟล์มี {2} คอลัมน์: A–{3})",
                            kvp.Key, colRef, columnCount, columnCount > 0 ? ColumnIndexToLetters(columnCount - 1) : "-")
                        : string.Format("{0} → \"{1}\": ไม่พบหัวตารางนี้ในไฟล์", kvp.Key, colRef));
                }
            }

            return problems;
        }

        /// <summary>คืน column ref ทุกตัวที่ mapping นี้อ่านค่า: ทั้งสตริงถ้าเป็น ref ตรง ๆ, หรือแต่ละ {X} ถ้าเป็นสูตร</summary>
        private static IEnumerable<string> ExtractColumnRefs(string expression)
        {
            if (string.IsNullOrWhiteSpace(expression)) yield break;

            if (expression.IndexOf('{') < 0)
            {
                yield return expression.Trim();
                yield break;
            }

            foreach (Match m in TemplateExpressionPlaceholderPattern.Matches(expression))
            {
                string colRef = m.Groups[1].Value.Trim();
                if (colRef.Length > 0) yield return colRef;
            }
        }

        /// <summary>แปลง index เริ่มที่ 0 เป็นตัวอักษรคอลัมน์ Excel (0→"A", 25→"Z", 26→"AA")</summary>
        private static string ColumnIndexToLetters(int index)
        {
            int dividend = index + 1;
            string name  = string.Empty;
            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                name = (char)('A' + modulo) + name;
                dividend = (dividend - modulo) / 26;
            }
            return name;
        }

        // ── Private helpers ──────────────────────────────────────────────────

        /// <summary>แตกเซลล์ที่มีหลายบรรทัด (Alt+Enter ใน Excel) เป็นรายการ ตัดช่องว่างแต่ละบรรทัด</summary>
        private static IList<string> SplitLines(string raw)
        {
            var result = new List<string>();
            if (string.IsNullOrWhiteSpace(raw)) return result;

            var parts = raw.Split(new[] { "\r\n", "\n", "\r" }, StringSplitOptions.None);
            foreach (var part in parts)
            {
                string trimmed = part.Trim();
                if (trimmed.Length > 0) result.Add(trimmed);
            }
            return result;
        }

        private static IWorkbook OpenWorkbook(string filePath)
        {
            if (!File.Exists(filePath))
                throw new KBExcelReadException(filePath, "ไม่พบไฟล์ดังกล่าวในระบบ");

            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    return new XSSFWorkbook(fs);
            }
            catch (IOException ex)
            {
                throw new KBExcelReadException(filePath, ex.Message, ex);
            }
            catch (Exception ex)
            {
                throw new KBExcelReadException(filePath,
                    "ไฟล์ไม่ใช่รูปแบบ .xlsx ที่ถูกต้อง: " + ex.Message, ex);
            }
        }

        private static string GetHyperlinkAddress(ICell cell)
        {
            if (cell == null) return null;
            try
            {
                IHyperlink link = cell.Hyperlink;
                return link == null ? null : link.Address;
            }
            catch
            {
                return null;
            }
        }

        private static string GetCellText(ICell cell, IFormulaEvaluator evaluator)
        {
            if (cell == null) return string.Empty;

            switch (cell.CellType)
            {
                case CellType.Numeric:
                    if (DateUtil.IsCellDateFormatted(cell))
                        return cell.DateCellValue.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                    return cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);

                case CellType.String:
                    return cell.StringCellValue ?? string.Empty;

                case CellType.Boolean:
                    return cell.BooleanCellValue.ToString();

                case CellType.Formula:
                    // ประเมินสูตรสด แทนการอ่านค่า cache ของไฟล์ (อาจว่างถ้า Excel ยังไม่ได้ recalculate ก่อน save)
                    CellValue result = evaluator.Evaluate(cell);
                    switch (result.CellType)
                    {
                        case CellType.Numeric:
                            if (DateUtil.IsCellDateFormatted(cell))
                                return DateUtil.GetJavaDate(result.NumberValue).ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
                            return result.NumberValue.ToString(CultureInfo.InvariantCulture);
                        case CellType.String:
                            return result.StringValue ?? string.Empty;
                        case CellType.Boolean:
                            return result.BooleanValue.ToString();
                        default:
                            return string.Empty;
                    }

                case CellType.Blank:
                case CellType.Error:
                default:
                    return string.Empty;
            }
        }

        /// <summary>
        /// แปลงเซลล์เป็นวันที่ รองรับทั้ง Excel serial, สูตร, และข้อความ (รวมวันที่แบบไทย)
        /// ผลลัพธ์เป็น ค.ศ. เสมอ (ปี พ.ศ. จะถูกลบ 543) คืน <c>null</c> (ไม่มี error) ถ้าเซลล์ว่าง
        /// </summary>
        private static DateTime? TryParseDateCell(ICell cell, IFormulaEvaluator evaluator, out string errorMessage)
        {
            errorMessage = null;

            if (cell == null || cell.CellType == CellType.Blank)
                return null;

            if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
            {
                try { return ApplyAdYearCorrection(cell.DateCellValue); }
                catch (Exception ex)
                {
                    errorMessage = "ไม่สามารถแปลงค่า serial เป็นวันที่ได้: " + ex.Message;
                    return null;
                }
            }

            if (cell.CellType == CellType.Formula && DateUtil.IsCellDateFormatted(cell))
            {
                CellValue result = evaluator.Evaluate(cell);
                if (result.CellType == CellType.Numeric)
                {
                    try { return ApplyAdYearCorrection(DateUtil.GetJavaDate(result.NumberValue)); }
                    catch (Exception ex)
                    {
                        errorMessage = "ไม่สามารถแปลงค่า serial เป็นวันที่ได้: " + ex.Message;
                        return null;
                    }
                }
            }

            string raw = GetCellText(cell, evaluator).Trim();
            return TryParseDateText(raw, out errorMessage);
        }

        /// <summary>
        /// แปลงข้อความล้วน (ไม่ผูกกับ ICell ใด ๆ) เป็นวันที่ — ใช้เมื่อค่าที่จะ parse มาจาก Template
        /// expression ที่ compose เป็น string แล้ว (เช่น formula) จึงไม่มี native Excel date-serial cell
        /// ให้ตรวจสอบแบบ TryParseDateCell ได้อีกต่อไป รองรับรูปแบบเดียวกับ TryParseDateCell
        /// (DateFormats + วันที่แบบไทย) ผลลัพธ์เป็น ค.ศ. เสมอ
        /// </summary>
        private static DateTime? TryParseDateText(string raw, out string errorMessage)
        {
            errorMessage = null;
            if (string.IsNullOrEmpty(raw))
                return null;

            DateTime parsed;
            if (DateTime.TryParseExact(raw, DateFormats, CultureInfo.InvariantCulture,
                                       DateTimeStyles.None, out parsed))
                return ApplyAdYearCorrection(parsed);

            DateTime? thai = TryParseThaiLongDate(raw);
            if (thai.HasValue) return thai.Value;

            errorMessage = string.Format("รูปแบบวันที่ไม่ถูกต้อง: '{0}'", raw);
            return null;
        }

        /// <summary>แปลงวันที่แบบไทย เช่น "9 เมษายน 2535" / "09 เม.ย. 2535"</summary>
        private static DateTime? TryParseThaiLongDate(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return null;

            var parts = raw.Trim().Split(new[] { ' ' }, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length < 3) return null;

            int day, year;
            if (!int.TryParse(parts[0], out day)) return null;
            if (!int.TryParse(parts[parts.Length - 1], out year)) return null;

            string monthToken = string.Join(" ", parts, 1, parts.Length - 2).Trim();

            int month;
            if (!ThaiMonths.TryGetValue(monthToken, out month)) return null;
            if (day < 1 || day > 31 || month < 1 || month > 12 || year < 1) return null;

            try
            {
                var dt = new DateTime(year, month, day, 0, 0, 0, DateTimeKind.Unspecified);
                return ApplyAdYearCorrection(dt);
            }
            catch
            {
                return null;
            }
        }

        /// <summary>ปีมากกว่า 2400 ถือว่าเป็น พ.ศ. จึงลบ 543 ให้เป็น ค.ศ.</summary>
        private static DateTime ApplyAdYearCorrection(DateTime dt)
        {
            return dt.Year > 2400 ? dt.AddYears(-543) : dt;
        }

        private static string SafeTruncate(string s, int max)
        {
            if (s == null) return string.Empty;
            return s.Length <= max ? s : s.Substring(0, max);
        }

        /// <summary>
        /// Returns <c>true</c> when every cell in <paramref name="row"/> is null or
        /// produces an empty string — used to skip padding rows at the end of the sheet.
        /// </summary>
        private static bool IsRowBlank(IRow row, IFormulaEvaluator evaluator)
        {
            if (row == null) return true;
            for (int c = row.FirstCellNum; c < row.LastCellNum; c++)
            {
                ICell cell = row.GetCell(c);
                if (cell == null || cell.CellType == CellType.Blank) continue;
                if (!string.IsNullOrWhiteSpace(GetCellText(cell, evaluator))) return false;
            }
            return true;
        }
    }
}
