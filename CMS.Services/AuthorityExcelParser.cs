using System;
using System.Collections.Generic;
using System.IO;
using CMS.Services.DTOs;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;

namespace CMS.Services
{
    /// <summary>
    /// Reads the authority matrix template. The two header rows (group row + ดู/เพิ่ม/แก้ไข/อนุมัติ row)
    /// are located by scanning for the "รหัสพนักงาน" header, so optional title rows above them are fine.
    /// Columns are resolved from header text (code, name, level, position, department, division, remark)
    /// and from the HC/SC/NC/PC group headers; each group spans view/add/edit/approve.
    /// </summary>
    public class AuthorityExcelParser
    {
        private const int MaxHeaderScanRows = 20;
        private static readonly string[] GroupName = { "HC", "SC", "NC", "PC" };
        private static readonly string[] ActionName = { "ดู", "เพิ่ม", "แก้ไข", "อนุมัติ" };

        private class Layout
        {
            public int HeaderRow, DataStartRow;
            public int Code = -1, Name = -1, Level = -1, Position = -1, Dept = -1, Div = -1, Remark = -1;
            public int[] GroupStart = { -1, -1, -1, -1 };
        }

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

            var fmt = new DataFormatter();
            Layout layout;
            string mismatch = FindLayout(sheet, fmt, out layout);
            if (mismatch != null)
            {
                AddIssue(issues, 0, AuthorityImportSeverity.Error, "TEMPLATE_MISMATCH",
                    "ไม่ใช่ template ที่รองรับ: " + mismatch);
                return rows;
            }

            for (int i = layout.DataStartRow; i <= sheet.LastRowNum; i++)
            {
                var r = sheet.GetRow(i);
                if (r == null) continue;

                string code = Text(fmt, r, layout.Code);
                string position = Text(fmt, r, layout.Position);
                bool anyMark = false;
                var perms = new bool[16];
                for (int g = 0; g < 4; g++)
                    for (int a = 0; a < 4; a++)
                    {
                        string t = Text(fmt, r, layout.GroupStart[g] + a);
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

                var codeCell = r.GetCell(layout.Code);
                if (codeCell != null && codeCell.CellType == CellType.Numeric)
                    AddIssue(issues, i + 1, AuthorityImportSeverity.Warning, "NUMERIC_EMPLOYEE_CODE",
                        "รหัสพนักงานเป็นตัวเลข เลข 0 นำหน้าอาจหายไป");

                rows.Add(new AuthorityImportRow
                {
                    RowNumber = i + 1,
                    EmployeeCode = code,
                    EmployeeName = Text(fmt, r, layout.Name),
                    Level = Text(fmt, r, layout.Level).ToUpperInvariant(),
                    Position = position,
                    Department = Text(fmt, r, layout.Dept),
                    Division = Text(fmt, r, layout.Div),
                    Remark = Text(fmt, r, layout.Remark),
                    Permissions = perms
                });
            }

            if (rows.Count == 0)
                AddIssue(issues, 0, AuthorityImportSeverity.Error, "NO_DATA", "ไม่พบข้อมูลพนักงานใน sheet นี้");
            return rows;
        }

        /// <summary>Locates header rows and column positions; returns an error message, or null on success.</summary>
        private static string FindLayout(ISheet sheet, DataFormatter fmt, out Layout layout)
        {
            layout = null;
            int last = Math.Min(sheet.LastRowNum, MaxHeaderScanRows);
            IRow h1 = null;
            int h1Index = -1;
            for (int i = 0; i <= last; i++)
            {
                var r = sheet.GetRow(i);
                if (r != null && FindColumn(fmt, r, 0, "รหัสพนักงาน") >= 0) { h1 = r; h1Index = i; break; }
            }
            if (h1 == null) return "ไม่พบหัวคอลัมน์ \"รหัสพนักงาน\"";
            var h2 = sheet.GetRow(h1Index + 1);
            if (h2 == null) return "ไม่พบแถวหัวตารางย่อย (ดู/เพิ่ม/แก้ไข/อนุมัติ)";

            var l = new Layout { HeaderRow = h1Index, DataStartRow = h1Index + 2 };
            l.Code = FindColumn(fmt, h1, 0, "รหัสพนักงาน");
            l.Position = FindColumn(fmt, h1, 0, "ตำแหน่ง");
            if (l.Position < 0) return "ไม่พบหัวคอลัมน์ \"ตำแหน่ง\"";
            l.Name = FindColumn(fmt, h1, 0, "ชื่อพนักงาน");
            l.Level = FindColumn(fmt, h1, 0, "level");
            l.Dept = FindColumn(fmt, h1, 0, "แผนก");
            l.Div = FindColumn(fmt, h1, 0, "ฝ่าย");
            l.Remark = FindColumn(fmt, h1, 0, "หมายเหตุ");
            if (l.Remark < 0) l.Remark = FindColumn(fmt, h1, 0, "remark");

            int from = Math.Max(l.Position, Math.Max(l.Dept, l.Div)) + 1;
            for (int g = 0; g < 4; g++)
            {
                l.GroupStart[g] = FindColumn(fmt, h1, from, GroupName[g]);
                if (l.GroupStart[g] < 0) return "ไม่พบกลุ่มสิทธิ์ " + GroupName[g];
                for (int a = 0; a < 4; a++)
                    if (Normalize(Text(fmt, h2, l.GroupStart[g] + a)) != ActionName[a])
                        return "หัวคอลัมน์ย่อยของ " + GroupName[g] + " ต้องเป็น ดู/เพิ่ม/แก้ไข/อนุมัติ";
            }
            if (l.Remark < 0) l.Remark = l.GroupStart[3] + 4;
            layout = l;
            return null;
        }

        private static int FindColumn(DataFormatter fmt, IRow row, int fromCol, string label)
        {
            int end = row.LastCellNum;
            for (int c = fromCol; c < end; c++)
                if (Normalize(Text(fmt, row, c)).IndexOf(label, StringComparison.OrdinalIgnoreCase) >= 0) return c;
            return -1;
        }

        /// <summary>Removes all whitespace so wrapped headers such as "รหัส\nพนักงาน" compare equal.</summary>
        private static string Normalize(string s)
        {
            var sb = new System.Text.StringBuilder(s.Length);
            foreach (char ch in s) if (!char.IsWhiteSpace(ch)) sb.Append(ch);
            return sb.ToString();
        }

        private static string Text(DataFormatter fmt, IRow row, int col)
        {
            if (col < 0) return "";
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
