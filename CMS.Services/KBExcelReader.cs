using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
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
    /// </summary>
    public class KBExcelReader : IKBExcelReaderService
    {
        private const int DataStartRowIndex = 1;   // แถว 2 ใน Excel

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
        public virtual IList<KBImportRow> ReadRows(string filePath)
        {
            IWorkbook workbook = OpenWorkbook(filePath);

            var rows = new List<KBImportRow>();

            ISheet sheet = workbook.GetSheetAt(0);
            if (sheet == null)
                return rows;

            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();

            string rowKind = DetectRowKind(sheet.GetRow(0), evaluator);
            if (rowKind == null)
                throw new KBExcelReadException(filePath,
                    "ไม่รู้จักรูปแบบไฟล์ Excel นี้ — ตรวจสอบว่าเป็น template กฎหมาย (มาตรา) หรือ ประกาศ/คำสั่ง ที่ถูกต้อง");

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
                    row = rowKind == "Order"
                        ? ReadOrderRow(excelRow, rowIdx, evaluator)
                        : ReadLawRow(excelRow, rowIdx, evaluator);
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
            if (colC == "มาตรา") return "Law";
            if (colC == "ประเภทลูก") return "Order";
            return null;
        }

        // ── Row readers ──────────────────────────────────────────────────────

        private static KBImportRow ReadLawRow(IRow excelRow, int rowIdx, IFormulaEvaluator evaluator)
        {
            var row = new KBImportRow
            {
                RowNumber = rowIdx + 1,   // 1-based: row index 1 → RowNumber 2
                RowKind   = "Law",

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
                RowKind   = "Order",

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
