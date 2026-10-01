using System;
using System.Collections.Generic;
using System.IO;
using CMS.Services.DTOs;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace CMS.Services
{
    /// <summary>
    /// Reads the authority matrix template (header rows 1-2, data from row 3):
    /// B=employee code, E=level, F=position, G=department, H=division,
    /// I-L=HC, M-P=SC, Q-T=NC, U-X=PC (view/add/edit/approve), Y=remark.
    /// </summary>
    public class AuthorityExcelParser
    {
        private const int ColCode = 1, ColName = 2, ColLevel = 4, ColPosition = 5, ColDept = 6, ColDiv = 7, ColRemark = 24;
        private const int FirstDataRowIndex = 2;
        private static readonly int[] GroupStart = { 8, 12, 16, 20 };
        private static readonly string[] GroupName = { "HC", "SC", "NC", "PC" };
        private static readonly string[] ActionName = { "ดู", "เพิ่ม", "แก้ไข", "อนุมัติ" };

        public IList<string> GetSheetNames(string filePath)
        {
            var wb = Open(filePath);
            var names = new List<string>();
            for (int i = 0; i < wb.NumberOfSheets; i++) names.Add(wb.GetSheetName(i));
            return names;
        }

        public IList<AuthorityImportRow> Parse(string filePath, string sheetName, IList<AuthorityImportIssue> issues)
        {
            if (issues == null) throw new ArgumentNullException("issues");
            var rows = new List<AuthorityImportRow>();
            var wb = Open(filePath);
            var sheet = string.IsNullOrEmpty(sheetName) ? wb.GetSheetAt(0) : wb.GetSheet(sheetName);
            if (sheet == null)
            {
                AddIssue(issues, 0, AuthorityImportSeverity.Error, "SHEET_NOT_FOUND", "ไม่พบ sheet ที่เลือก");
                return rows;
            }

            string mismatch = CheckTemplate(sheet);
            if (mismatch != null)
            {
                AddIssue(issues, 0, AuthorityImportSeverity.Error, "TEMPLATE_MISMATCH",
                    "ไม่ใช่ template ที่รองรับ: " + mismatch);
                return rows;
            }

            var fmt = new DataFormatter();
            for (int i = FirstDataRowIndex; i <= sheet.LastRowNum; i++)
            {
                var r = sheet.GetRow(i);
                if (r == null) continue;

                string code = Text(fmt, r, ColCode);
                string position = Text(fmt, r, ColPosition);
                bool anyMark = false;
                var perms = new bool[16];
                for (int g = 0; g < 4; g++)
                    for (int a = 0; a < 4; a++)
                    {
                        string t = Text(fmt, r, GroupStart[g] + a);
                        if (t.Length == 0) continue;
                        if (t.Equals("x", StringComparison.OrdinalIgnoreCase)) { perms[g * 4 + a] = true; anyMark = true; }
                        else AddIssue(issues, i + 1, AuthorityImportSeverity.Warning, "UNEXPECTED_CELL",
                            "ค่าในช่องสิทธิ์ไม่ใช่ X (\"" + t + "\") ถือเป็นไม่มีสิทธิ์");
                    }

                if (code.Length == 0 && position.Length == 0 && !anyMark) continue;
                if (code.Length == 0)
                {
                    AddIssue(issues, i + 1, AuthorityImportSeverity.Warning, "SKIPPED_ROW", "ไม่มีรหัสพนักงาน ข้ามแถวนี้");
                    continue;
                }

                var codeCell = r.GetCell(ColCode);
                if (codeCell != null && codeCell.CellType == CellType.Numeric)
                    AddIssue(issues, i + 1, AuthorityImportSeverity.Warning, "NUMERIC_EMPLOYEE_CODE",
                        "รหัสพนักงานเป็นตัวเลข เลข 0 นำหน้าอาจหายไป");

                rows.Add(new AuthorityImportRow
                {
                    RowNumber = i + 1,
                    EmployeeCode = code,
                    EmployeeName = Text(fmt, r, ColName),
                    Level = Text(fmt, r, ColLevel).ToUpperInvariant(),
                    Position = position,
                    Department = Text(fmt, r, ColDept),
                    Division = Text(fmt, r, ColDiv),
                    Remark = Text(fmt, r, ColRemark),
                    Permissions = perms
                });
            }

            if (rows.Count == 0)
                AddIssue(issues, 0, AuthorityImportSeverity.Error, "NO_DATA", "ไม่พบข้อมูลพนักงานใน sheet นี้");
            return rows;
        }

        private static string CheckTemplate(ISheet sheet)
        {
            var fmt = new DataFormatter();
            var h1 = sheet.GetRow(0);
            var h2 = sheet.GetRow(1);
            if (h1 == null || h2 == null) return "ไม่พบแถวหัวตาราง";
            if (!Text(fmt, h1, ColCode).Contains("รหัสพนักงาน")) return "คอลัมน์ B ต้องเป็น \"รหัสพนักงาน\"";
            if (!Text(fmt, h1, ColPosition).Contains("ตำแหน่ง")) return "คอลัมน์ F ต้องเป็น \"ตำแหน่ง\"";
            for (int g = 0; g < 4; g++)
            {
                if (Text(fmt, h1, GroupStart[g]).IndexOf(GroupName[g], StringComparison.OrdinalIgnoreCase) < 0)
                    return "ไม่พบกลุ่มสิทธิ์ " + GroupName[g];
                for (int a = 0; a < 4; a++)
                    if (Text(fmt, h2, GroupStart[g] + a) != ActionName[a])
                        return "หัวคอลัมน์ย่อยของ " + GroupName[g] + " ต้องเป็น ดู/เพิ่ม/แก้ไข/อนุมัติ";
            }
            return null;
        }

        private static string Text(DataFormatter fmt, IRow row, int col)
        {
            var c = row.GetCell(col);
            return c == null ? "" : (fmt.FormatCellValue(c) ?? "").Trim();
        }

        private static IWorkbook Open(string filePath)
        {
            if (string.IsNullOrEmpty(filePath)) throw new ArgumentNullException("filePath");
            if (!File.Exists(filePath)) throw new InvalidOperationException("ไม่พบไฟล์: " + filePath);
            try
            {
                using (var fs = new FileStream(filePath, FileMode.Open, FileAccess.Read, FileShare.ReadWrite))
                    return new XSSFWorkbook(fs);
            }
            catch (Exception ex)
            {
                throw new InvalidOperationException("เปิดไฟล์ไม่ได้ (ต้องเป็น .xlsx ที่ไม่เสียหาย): " + ex.Message, ex);
            }
        }

        private static void AddIssue(IList<AuthorityImportIssue> issues, int row, AuthorityImportSeverity sev, string code, string msg)
        {
            issues.Add(new AuthorityImportIssue { RowNumber = row, Severity = sev, Code = code, Message = msg });
        }
    }
}
