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
    /// อ่านไฟล์ Excel ตาราง ประกาศ/คำสั่ง แล้วแปลงเป็น KBOrderImportRow
    /// layout: หัวตารางแถว 1, ข้อมูลเริ่มแถว 2
    /// A=Code กฎหมายแม่(หลายบรรทัด)  B=ประเภทลูก  C=เอกสารลูก  D=Code(สูตร, หลายบรรทัด)
    /// E=ชื่อ  F=คำสั่งที่  G=วันที่ประกาศ  H=วันที่มีผลบังคับใช้
    /// I=บัญญัติไว้ว่า  J=ประเด็นสำคัญ  K=ชื่อไฟล์(+hyperlink)  L=กฎหมายอ้างอิง
    /// </summary>
    public class KBOrderExcelReader : IKBOrderExcelReaderService
    {
        private const int DataStartRowIndex = 1;   // แถว 2 ใน Excel

        private static readonly string[] DateFormats =
        {
            "dd/MM/yyyy",
            "d/M/yyyy",
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

        public IList<string> GetSheetNames(string filePath)
        {
            var names = new List<string>();
            // IWorkbook (NPOI 2.5.6) ไม่ implement IDisposable — ไม่ต้องใช้ using
            IWorkbook wb = OpenWorkbook(filePath);
            for (int i = 0; i < wb.NumberOfSheets; i++)
                names.Add(wb.GetSheetName(i));
            return names;
        }

        public IList<KBOrderImportRow> ReadRows(string filePath, string sheetName)
        {
            var rows = new List<KBOrderImportRow>();

            IWorkbook wb = OpenWorkbook(filePath);
            ISheet sheet = string.IsNullOrEmpty(sheetName)
                ? wb.GetSheetAt(0)
                : wb.GetSheet(sheetName);

            if (sheet == null)
                throw new KBExcelReadException(filePath,
                    string.Format("ไม่พบ worksheet ชื่อ '{0}' ในไฟล์", sheetName));

            // ประเมินสูตรสดแทนการเชื่อค่า cache ที่ Excel เก็บไว้ตอน save — บางไฟล์ save มาโดยยังไม่ได้ recalculate
            // (เช่น calcPr forceFullCalc="1") ทำให้ cache ของ cell สูตรว่างเปล่า และ NPOI จะ fallback เป็น 0
            IFormulaEvaluator evaluator = wb.GetCreationHelper().CreateFormulaEvaluator();

            for (int rowIdx = DataStartRowIndex; rowIdx <= sheet.LastRowNum; rowIdx++)
            {
                IRow excelRow = sheet.GetRow(rowIdx);
                if (excelRow == null || IsRowBlank(excelRow, evaluator)) continue;

                var row = new KBOrderImportRow { RowNumber = rowIdx + 1 };

                try
                {
                    row.ParentCodes        = SplitLines(GetCellText(excelRow.GetCell(0), evaluator));
                    row.FinalCodeChoices   = SplitLines(GetCellText(excelRow.GetCell(3), evaluator));
                    row.Title              = GetCellText(excelRow.GetCell(4), evaluator).Trim();
                    row.TitleCategory      = GetCellText(excelRow.GetCell(5), evaluator).Trim();
                    row.PublishedDateRaw   = GetCellText(excelRow.GetCell(6), evaluator).Trim();
                    row.EffectiveDateRaw   = GetCellText(excelRow.GetCell(7), evaluator).Trim();
                    row.ContentHTML        = GetCellText(excelRow.GetCell(8), evaluator);
                    row.Summary            = GetCellText(excelRow.GetCell(9), evaluator);
                    row.SectionsRaw        = GetCellText(excelRow.GetCell(11), evaluator).Trim();

                    // column K — ข้อความคือชื่อไฟล์ ส่วน URL จริงอยู่ใน hyperlink ของเซลล์
                    ICell fileCell = excelRow.GetCell(10);
                    row.AttachmentFileName = GetCellText(fileCell, evaluator).Trim();
                    row.AttachmentUrl      = GetHyperlinkAddress(fileCell);

                    row.FinalArticleCode = row.FinalCodeChoices.Count > 0 ? row.FinalCodeChoices[0] : null;

                    var notes = new List<string>();

                    string gErr;
                    row.PublishedDate = TryParseDateCell(excelRow.GetCell(6), evaluator, out gErr);
                    if (gErr != null) notes.Add("วันที่ประกาศ: " + gErr);

                    string hErr;
                    row.EffectiveDate = TryParseDateCell(excelRow.GetCell(7), evaluator, out hErr);
                    if (hErr != null) notes.Add("วันที่มีผลบังคับใช้: " + hErr);

                    if (notes.Count > 0)
                    {
                        row.Status     = "Error";
                        row.StatusNote = SafeTruncate(string.Join("; ", notes.ToArray()), 400);
                    }
                }
                catch (Exception ex)
                {
                    row.Status     = "Error";
                    row.StatusNote = SafeTruncate("อ่านข้อมูล Excel ไม่สำเร็จ: " + ex.Message, 200);
                }

                rows.Add(row);
            }

            return rows;
        }

        // ── Helpers ──────────────────────────────────────────────────────────

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
        /// แปลงเซลล์เป็นวันที่ รองรับทั้ง Excel serial และข้อความ
        /// ผลลัพธ์เป็น ค.ศ. เสมอ (ปี พ.ศ. จะถูกลบ 543)
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
