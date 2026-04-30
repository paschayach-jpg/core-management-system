using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using CMS.Domain.Models;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace CMS.Services
{
    /// <summary>
    /// Reads <c>import_article.xlsx</c> and maps each data row (row 4 onward on the
    /// first worksheet) to a <see cref="KBImportRow"/>.
    /// Uses NPOI 2.5.6. No validation logic is performed here — that is the
    /// responsibility of <see cref="KBImportProcessor"/>.
    /// </summary>
    public class KBExcelReader
    {
        // ── Supported text-date formats (InvariantCulture) ───────────────────
        private static readonly string[] DateFormats =
        {
            "dd/MM/yyyy",
            "d/M/yyyy",
            "yyyy-MM-dd",
            "yyyy/MM/dd"
        };

        // ── Public API ───────────────────────────────────────────────────────

        /// <summary>
        /// Opens the file at <paramref name="filePath"/> and returns one
        /// <see cref="KBImportRow"/> per data row (rows 4 onward on the first
        /// worksheet).  Returns an empty list when the file has no data rows.
        /// </summary>
        /// <exception cref="KBExcelReadException">
        /// Thrown when the file does not exist, is not a valid <c>.xlsx</c>, or is
        /// locked by another process.
        /// </exception>
        public virtual IList<KBImportRow> ReadRows(string filePath)
        {
            if (!File.Exists(filePath))
                throw new KBExcelReadException(filePath, "ไม่พบไฟล์ดังกล่าวในระบบ");

            IWorkbook workbook;
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                {
                    workbook = new XSSFWorkbook(fs);
                }
            }
            catch (IOException ex)
            {
                throw new KBExcelReadException(filePath, ex.Message, ex);
            }
            catch (Exception ex)
            {
                // Catches InvalidDataException, ZipException, etc. from NPOI when the
                // file is not a valid .xlsx archive.
                throw new KBExcelReadException(filePath, "ไฟล์ไม่ใช่รูปแบบ .xlsx ที่ถูกต้อง: " + ex.Message, ex);
            }

            var rows = new List<KBImportRow>();

            ISheet sheet = workbook.GetSheetAt(0);
            if (sheet == null)
                return rows;

            // Rows 0–2 are header rows; data starts at row index 3 (Excel row 4).
            int lastRowIndex = sheet.LastRowNum;
            for (int rowIdx = 3; rowIdx <= lastRowIndex; rowIdx++)
            {
                IRow excelRow = sheet.GetRow(rowIdx);
                if (excelRow == null)
                    continue;

                // Skip entirely blank rows (all cells null or empty).
                if (IsRowBlank(excelRow))
                    continue;

                var row = new KBImportRow
                {
                    RowNumber = rowIdx + 1,   // 1-based: row index 3 → RowNumber 4

                    ColA = GetCellText(excelRow.GetCell(0)),
                    ColB = GetCellText(excelRow.GetCell(1)),
                    ColC = GetCellText(excelRow.GetCell(2)),
                    ColD = GetCellText(excelRow.GetCell(3)),
                    ColE = GetCellText(excelRow.GetCell(4)),
                    ColF = GetCellText(excelRow.GetCell(5)),
                    ColG = GetCellText(excelRow.GetCell(6)),
                    ColH = GetCellText(excelRow.GetCell(7)),
                    ColI = GetCellText(excelRow.GetCell(8)),
                    ColJ = GetCellText(excelRow.GetCell(9)),
                    ColK = GetCellText(excelRow.GetCell(10)),
                    ColL = GetCellText(excelRow.GetCell(11)),
                    ColM = GetCellText(excelRow.GetCell(12)),
                    ColN = GetCellText(excelRow.GetCell(13)),
                    ColO = GetCellText(excelRow.GetCell(14)),
                    ColP = GetCellText(excelRow.GetCell(15)),
                    ColQ = GetCellText(excelRow.GetCell(16)),
                    ColT = GetCellText(excelRow.GetCell(19)),
                };

                // ── UI display fields ────────────────────────────────────────
                row.Title    = row.ColE;
                row.Tags     = string.Format("{0}, {1}", row.ColC, row.ColE);
                row.Category = string.Format("{0} > {1}", row.ColA, row.ColB);

                // ── Column R — PublishedDate ─────────────────────────────────
                ICell cellR = excelRow.GetCell(17);
                string rError;
                DateTime? publishedDate = TryParseDateCell(cellR, out rError);
                row.PublishedDateParsed = publishedDate;
                // Store raw text for display / error reporting
                row.ColR = publishedDate.HasValue
                    ? publishedDate.Value.ToString("dd/MM/yyyy")
                    : GetCellText(cellR);

                // ── Column S — ExpiryDate ────────────────────────────────────
                ICell cellS = excelRow.GetCell(18);
                string sError;
                DateTime? expiryDate = TryParseDateCell(cellS, out sError);
                row.ExpiryDateParsed = expiryDate;
                row.ColS = expiryDate.HasValue
                    ? expiryDate.Value.ToString("dd/MM/yyyy")
                    : GetCellText(cellS);

                // Keep ExpiryDate display string in sync (used by UI grid)
                row.ExpiryDate = row.ColS;

                rows.Add(row);
            }

            return rows;
        }

        // ── Private helpers ──────────────────────────────────────────────────

        /// <summary>
        /// Returns the string representation of <paramref name="cell"/>'s value,
        /// handling all common cell types.  Returns <see cref="string.Empty"/> for
        /// a null cell.
        /// </summary>
        private static string GetCellText(ICell cell)
        {
            if (cell == null)
                return string.Empty;

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
                    // Use the cached result rather than re-evaluating (avoids needing
                    // a formula evaluator and works even when the workbook is read-only).
                    switch (cell.CachedFormulaResultType)
                    {
                        case CellType.Numeric:
                            if (DateUtil.IsCellDateFormatted(cell))
                                return cell.DateCellValue.ToString("dd/MM/yyyy");
                            return cell.NumericCellValue.ToString(CultureInfo.InvariantCulture);
                        case CellType.String:
                            return cell.StringCellValue ?? string.Empty;
                        case CellType.Boolean:
                            return cell.BooleanCellValue.ToString();
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
        /// Attempts to parse <paramref name="cell"/> as a <see cref="DateTime"/>.
        /// Handles both native Excel date serial numbers and text strings.
        /// Always applies <see cref="ApplyAdYearCorrection"/> so the result is A.D.
        /// </summary>
        /// <param name="cell">The cell to parse (may be null).</param>
        /// <param name="errorMessage">
        /// Set to a non-null Thai error string when the cell contains a non-empty
        /// value that cannot be parsed; otherwise <c>null</c>.
        /// </param>
        /// <returns>
        /// The parsed <see cref="DateTime"/> in A.D., or <c>null</c> when the cell
        /// is empty/null (no error) or unparseable (error set).
        /// </returns>
        private static DateTime? TryParseDateCell(ICell cell, out string errorMessage)
        {
            errorMessage = null;

            if (cell == null || cell.CellType == CellType.Blank)
                return null;

            // ── Case 1: native Excel date serial (Numeric cell with date format) ──
            if (cell.CellType == CellType.Numeric && DateUtil.IsCellDateFormatted(cell))
            {
                try
                {
                    DateTime dt = cell.DateCellValue;
                    return ApplyAdYearCorrection(dt);
                }
                catch (Exception ex)
                {
                    errorMessage = string.Format("ไม่สามารถแปลงค่า serial เป็นวันที่ได้: {0}", ex.Message);
                    return null;
                }
            }

            // ── Case 2: formula that resolves to a date ──────────────────────
            if (cell.CellType == CellType.Formula
                && cell.CachedFormulaResultType == CellType.Numeric
                && DateUtil.IsCellDateFormatted(cell))
            {
                try
                {
                    DateTime dt = cell.DateCellValue;
                    return ApplyAdYearCorrection(dt);
                }
                catch (Exception ex)
                {
                    errorMessage = string.Format("ไม่สามารถแปลงค่า serial เป็นวันที่ได้: {0}", ex.Message);
                    return null;
                }
            }

            // ── Case 3: text string ──────────────────────────────────────────
            string raw = GetCellText(cell).Trim();

            if (string.IsNullOrEmpty(raw))
                return null;

            DateTime parsed;
            if (DateTime.TryParseExact(
                    raw,
                    DateFormats,
                    CultureInfo.InvariantCulture,
                    DateTimeStyles.None,
                    out parsed))
            {
                return ApplyAdYearCorrection(parsed);
            }

            // Could not parse with any supported format.
            errorMessage = string.Format("รูปแบบวันที่ไม่ถูกต้อง: '{0}'", raw);
            return null;
        }

        /// <summary>
        /// Converts a Thai Buddhist Era year to A.D. when necessary.
        /// If <paramref name="dt"/>.Year is greater than 2400 (indicating B.E.),
        /// subtracts 543 years.  Otherwise returns <paramref name="dt"/> unchanged.
        /// </summary>
        private static DateTime ApplyAdYearCorrection(DateTime dt)
        {
            // Thai B.E. years are ~543 years ahead of A.D.
            // e.g. B.E. 2567 → A.D. 2024
            return dt.Year > 2400 ? dt.AddYears(-543) : dt;
        }

        /// <summary>
        /// Returns <c>true</c> when every cell in <paramref name="row"/> is null or
        /// produces an empty string — used to skip padding rows at the end of the sheet.
        /// </summary>
        private static bool IsRowBlank(IRow row)
        {
            if (row == null) return true;
            for (int c = row.FirstCellNum; c < row.LastCellNum; c++)
            {
                ICell cell = row.GetCell(c);
                if (cell != null && cell.CellType != CellType.Blank
                    && !string.IsNullOrEmpty(GetCellText(cell)))
                    return false;
            }
            return true;
        }
    }
}
