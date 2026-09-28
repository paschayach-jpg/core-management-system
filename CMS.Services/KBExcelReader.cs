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
    /// <b>Order</b> (12-column "ประกาศ/คำสั่ง", header C = "ประเภทลูก"): A=Code รวม (สูตร —
    /// ไม่อ่าน, คำนวณเองจาก B+C+D)  B=Code กฎหมายแม่(หลายบรรทัด)  C=ประเภทลูก (1=ประกาศ, 2=คำสั่ง)
    /// D=เอกสารลูก  E=ชื่อ  F=คำสั่งที่  G=วันที่ประกาศ  H=วันที่มีผลบังคับใช้  I=บัญญัติไว้ว่า
    /// J=ประเด็นสำคัญ  K=ชื่อไฟล์(+hyperlink)  L=กฎหมายอ้างอิง.
    ///
    /// Uses NPOI 2.5.6, evaluating formula cells live (a saved-but-not-recalculated workbook
    /// otherwise leaves formula cells' cached value blank). No business-rule validation is
    /// performed here — that is the responsibility of <see cref="KBImportProcessor"/>.
    ///
    /// The fixed column positions above are the baseline for Title/TitleCategory/Summary/
    /// ContentHTML/PublishedDate/EffectiveDate only — <see cref="ReadRows"/> overrides them per
    /// the selected KB Import Template's column mapping when configured (see
    /// <see cref="ApplyFieldOverrides"/>). ArticleCode/CategoryID/ArticleType and everything
    /// Order-specific (ParentCodes, attachment) always come from the fixed positions.
    /// </summary>
    public class KBExcelReader : IKBExcelReaderService
    {
        private const int DataStartRowIndex = 1;   // แถว 2 ใน Excel

        /// <summary>Target column ที่ยอม override ตำแหน่งตายตัวด้วย Template mapping ได้ (ไม่รวม ArticleCode/CategoryID/ArticleType)</summary>
        private static readonly string[] OverridableTargetColumns =
            { "Title", "TitleCategory", "Summary", "ContentHTML", "PublishedDate", "EffectiveDate" };

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
        /// <see cref="KBImportRow"/> per data row (row 2 onward on the first worksheet).
        /// Returns an empty list when the file has no data rows.
        /// </summary>
        /// <exception cref="KBExcelReadException">
        /// Thrown when the file does not exist, is not a valid <c>.xlsx</c>, is locked by
        /// another process, or its header row doesn't match either supported layout.
        /// </exception>
        public virtual IList<KBImportRow> ReadRows(string filePath, IList<KBImportTemplateColumnRow> templateColumns)
        {
            IWorkbook workbook = OpenWorkbook(filePath);

            var rows = new List<KBImportRow>();

            ISheet sheet = workbook.GetSheetAt(0);
            if (sheet == null)
                return rows;

            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();
            IRow headerRow = sheet.GetRow(0);

            string rowKind = DetectRowKind(headerRow, evaluator);
            if (rowKind == null)
                throw new KBExcelReadException(filePath,
                    "ไม่รู้จักรูปแบบไฟล์ Excel นี้ — ตรวจสอบว่าเป็น template กฎหมาย (มาตรา) หรือ ประกาศ/คำสั่ง ที่ถูกต้อง");

            var overrides = BuildFieldOverrides(templateColumns);

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
                    row = rowKind == KBImportLayoutTypeCatalog.Order
                        ? ReadOrderRow(excelRow, rowIdx, evaluator)
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

                rows.Add(row);
            }

            return rows;
        }

        // ── Layout detection ─────────────────────────────────────────────────

        /// <summary>
        /// Detects which layout a file uses from its header row's column C text.
        /// Returns "Law", "Order", or <c>null</c> when neither matches.
        /// </summary>
        private static string DetectRowKind(IRow headerRow, IFormulaEvaluator evaluator)
        {
            string colC = GetCellText(headerRow == null ? null : headerRow.GetCell(2), evaluator).Trim();
            if (colC == "มาตรา") return KBImportLayoutTypeCatalog.Law;
            if (colC == "ประเภทลูก") return KBImportLayoutTypeCatalog.Order;
            return null;
        }

        // ── Row readers ──────────────────────────────────────────────────────

        private static KBImportRow ReadLawRow(IRow excelRow, int rowIdx, IFormulaEvaluator evaluator)
        {
            var row = new KBImportRow
            {
                RowNumber = rowIdx + 1,   // 1-based: row index 1 → RowNumber 2
                RowKind   = KBImportLayoutTypeCatalog.Law,

                ColA            = GetCellText(excelRow.GetCell(0), evaluator).Trim(),    // A — Code รวม
                ColC            = GetCellText(excelRow.GetCell(2), evaluator).Trim(),    // C — มาตรา
                RawSummary      = GetCellText(excelRow.GetCell(12), evaluator).Trim(),   // M — ชื่อ
                RawPublishedDate = GetCellText(excelRow.GetCell(13), evaluator).Trim(),  // N — วันที่ประกาศ
                RawEffectiveDate = GetCellText(excelRow.GetCell(14), evaluator).Trim(),  // O — วันที่มีผลบังคับใช้
                RawContent      = GetCellText(excelRow.GetCell(15), evaluator),          // P — บัญญัติไว้ว่า
                RawTitleCategory = GetCellText(excelRow.GetCell(18), evaluator).Trim(),  // S — กฎหมายอ้างอิง
            };

            string ignoredError;
            row.PublishedDateParsed = TryParseDateCell(excelRow.GetCell(13), evaluator, out ignoredError);
            row.EffectiveDateParsed = TryParseDateCell(excelRow.GetCell(14), evaluator, out ignoredError);

            return row;
        }

        private static KBImportRow ReadOrderRow(IRow excelRow, int rowIdx, IFormulaEvaluator evaluator)
        {
            var row = new KBImportRow
            {
                RowNumber = rowIdx + 1,
                RowKind   = KBImportLayoutTypeCatalog.Order,

                ParentCodes      = SplitLines(GetCellText(excelRow.GetCell(1), evaluator)),   // B — Code กฎหมายแม่
                OrderTypeDigit   = GetCellText(excelRow.GetCell(2), evaluator).Trim(),         // C — ประเภทลูก
                OrderDocNumber   = GetCellText(excelRow.GetCell(3), evaluator).Trim(),         // D — เอกสารลูก
                RawTitle         = GetCellText(excelRow.GetCell(4), evaluator).Trim(),         // E — ชื่อ
                RawTitleCategory = GetCellText(excelRow.GetCell(5), evaluator).Trim(),         // F — คำสั่งที่
                RawPublishedDate = GetCellText(excelRow.GetCell(6), evaluator).Trim(),         // G — วันที่ประกาศ
                RawEffectiveDate = GetCellText(excelRow.GetCell(7), evaluator).Trim(),         // H — วันที่มีผลบังคับใช้
                RawContent       = GetCellText(excelRow.GetCell(8), evaluator),                // I — บัญญัติไว้ว่า
                RawSummary       = GetCellText(excelRow.GetCell(9), evaluator),                // J — ประเด็นสำคัญ
                SectionsRaw      = GetCellText(excelRow.GetCell(11), evaluator).Trim(),        // L — กฎหมายอ้างอิง
            };

            row.FinalCodeChoices = BuildFinalCodeChoices(row.ParentCodes, row.OrderTypeDigit, row.OrderDocNumber);

            // column K — ข้อความคือชื่อไฟล์ ส่วน URL จริงอยู่ใน hyperlink ของเซลล์
            ICell fileCell = excelRow.GetCell(10);
            row.AttachmentFileName = GetCellText(fileCell, evaluator).Trim();
            row.AttachmentUrl      = GetHyperlinkAddress(fileCell);

            string ignoredError;
            row.PublishedDateParsed = TryParseDateCell(excelRow.GetCell(6), evaluator, out ignoredError);
            row.EffectiveDateParsed = TryParseDateCell(excelRow.GetCell(7), evaluator, out ignoredError);

            return row;
        }

        // ── Template mapping overrides ───────────────────────────────────────

        /// <summary>
        /// สร้าง lookup ของ target column → ExcelColumnRef expression จาก mapping ที่ config ไว้ใน
        /// Template (เฉพาะ <see cref="OverridableTargetColumns"/>, ข้าม ArticleCode/CategoryID/ArticleType
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
        /// Overwrite ค่า Raw*/parsed-date ของ <paramref name="row"/> (ที่ ReadLawRow/ReadOrderRow ตั้งไว้
        /// แบบตำแหน่งตายตัวเป็น baseline) ด้วยค่าที่ resolve จาก Template — เฉพาะ target ที่มี mapping ใน
        /// <paramref name="overrides"/> เท่านั้น target ที่ไม่ได้ map ยังคงค่าตำแหน่งตายตัวเดิมไว้
        /// </summary>
        private static void ApplyFieldOverrides(
            KBImportRow row, IRow excelRow, IRow headerRow, IFormulaEvaluator evaluator,
            Dictionary<string, string> overrides)
        {
            string expr;

            if (overrides.TryGetValue("Title", out expr))
                row.RawTitle = ResolveTemplateExpression(expr, headerRow, excelRow, evaluator);

            if (overrides.TryGetValue("TitleCategory", out expr))
                row.RawTitleCategory = ResolveTemplateExpression(expr, headerRow, excelRow, evaluator);

            if (overrides.TryGetValue("Summary", out expr))
                row.RawSummary = ResolveTemplateExpression(expr, headerRow, excelRow, evaluator);

            if (overrides.TryGetValue("ContentHTML", out expr))
                row.RawContent = ResolveTemplateExpression(expr, headerRow, excelRow, evaluator);

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
        /// </summary>
        private static string ResolveTemplateExpression(string expression, IRow headerRow, IRow dataRow, IFormulaEvaluator evaluator)
        {
            if (string.IsNullOrWhiteSpace(expression)) return string.Empty;

            if (expression.IndexOf('{') < 0)
                return ResolveColumnRef(expression.Trim(), headerRow, dataRow, evaluator);

            return TemplateExpressionPlaceholderPattern.Replace(expression,
                m => NormalizeNumericToken(ResolveColumnRef(m.Groups[1].Value.Trim(), headerRow, dataRow, evaluator)));
        }

        /// <summary>ถ้า raw เป็นตัวเลขล้วน แปลงผ่าน int แล้ว ToString กลับ (ตัดเลขศูนย์นำหน้า) ไม่งั้นคืนค่าเดิม</summary>
        private static string NormalizeNumericToken(string raw)
        {
            if (string.IsNullOrEmpty(raw)) return raw;
            int number;
            if (Regex.IsMatch(raw, @"^\d+$") && int.TryParse(raw, out number))
                return number.ToString(CultureInfo.InvariantCulture);
            return raw;
        }

        /// <summary>คืนค่า text ของเซลล์ที่ colRef ชี้ถึง (ลองตีความเป็นตัวอักษรคอลัมน์ก่อน ไม่ได้ลองจับคู่หัวตาราง) คืนค่าว่างถ้า resolve ไม่ได้เลย</summary>
        private static string ResolveColumnRef(string colRef, IRow headerRow, IRow dataRow, IFormulaEvaluator evaluator)
        {
            int colIndex = ResolveColumnIndex(colRef, headerRow, evaluator);
            if (colIndex < 0) return string.Empty;
            return GetCellText(dataRow.GetCell(colIndex), evaluator).Trim();
        }

        private static int ResolveColumnIndex(string colRef, IRow headerRow, IFormulaEvaluator evaluator)
        {
            int colIndex = ColumnLetterToIndex(colRef);
            if (colIndex < 0)
                colIndex = FindColumnIndexByHeaderText(headerRow, colRef, evaluator);
            return colIndex;
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

        // ── Private helpers ──────────────────────────────────────────────────

        /// <summary>
        /// สร้าง final combined code ต่อ parent code หนึ่งบรรทัด = parentCode + typeDigit + docNumber
        /// (mirror ของสูตร Excel เดิมใน column A ของไฟล์ "Order": <c>IF(OR(B="",C="",D="",C="NA",D="NA"),"",...)</c>)
        /// คืนรายการว่างถ้า typeDigit/docNumber ว่างหรือเป็น "NA" หรือไม่มี parent code เลย
        /// </summary>
        private static IList<string> BuildFinalCodeChoices(IList<string> parentCodes, string typeDigit, string docNumber)
        {
            var result = new List<string>();

            bool typeValid = !string.IsNullOrEmpty(typeDigit) && !string.Equals(typeDigit, "NA", StringComparison.OrdinalIgnoreCase);
            bool docValid  = !string.IsNullOrEmpty(docNumber) && !string.Equals(docNumber, "NA", StringComparison.OrdinalIgnoreCase);
            if (!typeValid || !docValid || parentCodes == null)
                return result;

            foreach (var parent in parentCodes)
                result.Add(parent + typeDigit + docNumber);

            return result;
        }

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
                        return cell.DateCellValue.ToString("dd/MM/yyyy");
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
                                return DateUtil.GetJavaDate(result.NumberValue).ToString("dd/MM/yyyy");
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
