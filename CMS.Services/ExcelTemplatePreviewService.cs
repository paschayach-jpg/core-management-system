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
    /// อ่านคอลัมน์ (แถวหัวตาราง + ตัวอย่างข้อมูล 1 แถว) จากไฟล์ Excel ที่ admin อัปโหลด
    /// ในหน้า config Excel Import Template โดยไม่ผูกกับ layout ตายตัวแบบ KBExcelReader —
    /// ใช้ NPOI 2.5.6 เช่นเดียวกัน
    /// </summary>
    public class ExcelTemplatePreviewService : IExcelTemplatePreviewService
    {
        public virtual IList<ExcelColumnPreview> ReadColumnPreview(string filePath)
        {
            if (string.IsNullOrWhiteSpace(filePath) || !File.Exists(filePath))
                throw new FileNotFoundException("ไม่พบไฟล์ดังกล่าวในระบบ", filePath);

            IWorkbook workbook;
            using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                workbook = new XSSFWorkbook(fs);

            var result = new List<ExcelColumnPreview>();

            ISheet sheet = workbook.GetSheetAt(0);
            if (sheet == null) return result;

            IRow headerRow = sheet.GetRow(0);
            if (headerRow == null) return result;

            IRow sampleRow = sheet.GetRow(1);
            IFormulaEvaluator evaluator = workbook.GetCreationHelper().CreateFormulaEvaluator();

            int lastCellIndex = headerRow.LastCellNum; // exclusive count, -1 ถ้าแถวว่าง
            for (int colIdx = 0; colIdx < lastCellIndex; colIdx++)
            {
                string headerText = GetCellText(headerRow.GetCell(colIdx), evaluator);
                string sampleValue = sampleRow != null ? GetCellText(sampleRow.GetCell(colIdx), evaluator) : string.Empty;

                // ข้ามคอลัมน์ที่ทั้งหัวตารางและตัวอย่างว่างเปล่า (มักเป็นคอลัมน์เกินขอบเขตข้อมูลจริง)
                if (string.IsNullOrWhiteSpace(headerText) && string.IsNullOrWhiteSpace(sampleValue))
                    continue;

                result.Add(new ExcelColumnPreview
                {
                    ColumnIndex = colIdx,
                    ColumnRef   = ColumnIndexToLetter(colIdx),
                    HeaderText  = headerText,
                    SampleValue = sampleValue,
                });
            }

            return result;
        }

        private static string ColumnIndexToLetter(int index)
        {
            int dividend = index + 1;
            string columnName = string.Empty;
            while (dividend > 0)
            {
                int modulo = (dividend - 1) % 26;
                columnName = Convert.ToChar(65 + modulo) + columnName;
                dividend = (dividend - modulo) / 26;
            }
            return columnName;
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
                    try
                    {
                        CellValue value = evaluator.Evaluate(cell);
                        if (value == null) return string.Empty;
                        switch (value.CellType)
                        {
                            case CellType.Numeric: return value.NumberValue.ToString(CultureInfo.InvariantCulture);
                            case CellType.String:  return value.StringValue ?? string.Empty;
                            case CellType.Boolean: return value.BooleanValue.ToString();
                            default: return string.Empty;
                        }
                    }
                    catch
                    {
                        return string.Empty;
                    }

                default:
                    return string.Empty;
            }
        }
    }
}
