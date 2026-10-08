using System.IO;
using NPOI.SS.UserModel;
using NPOI.SS.Util;
using NPOI.XSSF.UserModel;

namespace CMS.Services
{
    /// <summary>
    /// Builds the blank authority matrix template that <see cref="AuthorityExcelParser"/> reads:
    /// header rows 1-2 (merged group row + ดู/เพิ่ม/แก้ไข/อนุมัติ row), data from row 3.
    /// </summary>
    public static class AuthorityExcelTemplate
    {
        public const string SheetName = "ชื่อพนักงาน";

        private static readonly string[] FixedHeaders =
            { "No", "รหัสพนักงาน", "ชื่อพนักงาน", "Name (Eng)", "Level", "ตำแหน่ง", "แผนก/ส่วนงาน", "ฝ่าย" };
        private static readonly string[] GroupHeaders =
            { "1.HC\nสูงสุด", "2.SC\nลับมาก", "3.NC\nปกติ", "4.PC\nทั่วไป" };
        private static readonly string[] ActionHeaders = { "ดู", "เพิ่ม", "แก้ไข", "อนุมัติ" };

        public static byte[] Build()
        {
            using (var ms = new MemoryStream())
            {
                var wb = new XSSFWorkbook();
                var sheet = wb.CreateSheet(SheetName);

                var head = wb.CreateCellStyle();
                head.FillForegroundColor = IndexedColors.Grey25Percent.Index;
                head.FillPattern = FillPattern.SolidForeground;
                head.Alignment = HorizontalAlignment.Center;
                head.VerticalAlignment = VerticalAlignment.Center;
                head.WrapText = true;
                var bold = wb.CreateFont();
                bold.IsBold = true;
                head.SetFont(bold);
                SetBorder(head);

                var text = wb.CreateCellStyle();
                text.DataFormat = wb.CreateDataFormat().GetFormat("@"); // keep leading zeros in employee code
                SetBorder(text);
                var center = wb.CreateCellStyle();
                center.Alignment = HorizontalAlignment.Center;
                SetBorder(center);

                var r1 = sheet.CreateRow(0);
                var r2 = sheet.CreateRow(1);
                r1.HeightInPoints = 32;
                for (int c = 0; c < FixedHeaders.Length; c++)
                {
                    Put(r1, c, FixedHeaders[c], head);
                    Put(r2, c, null, head);
                    sheet.AddMergedRegion(new CellRangeAddress(0, 1, c, c));
                }
                for (int g = 0; g < 4; g++)
                {
                    int start = FixedHeaders.Length + g * 4;
                    Put(r1, start, GroupHeaders[g], head);
                    for (int a = 0; a < 4; a++)
                    {
                        if (a > 0) Put(r1, start + a, null, head);
                        Put(r2, start + a, ActionHeaders[a], head);
                    }
                    sheet.AddMergedRegion(new CellRangeAddress(0, 0, start, start + 3));
                }

                // One sample row (row 3). Dummy values: replace or delete before importing.
                var s = sheet.CreateRow(2);
                Put(s, 0, "1", center);
                Put(s, 1, "00000", text);
                Put(s, 2, "ตัวอย่าง ชื่อ นามสกุล", text);
                Put(s, 3, "Sample Name", text);
                Put(s, 4, "1A", center);
                Put(s, 5, "ชื่อตำแหน่ง (ต้องตรงกับ Config สิทธิ์)", text);
                Put(s, 6, "ชื่อแผนก", text);
                Put(s, 7, "ชื่อฝ่าย", text);
                for (int c = FixedHeaders.Length; c < FixedHeaders.Length + 16; c++) Put(s, c, null, center);
                Put(s, FixedHeaders.Length, "X", center); // HC view

                int[] widths = { 6, 14, 28, 28, 8, 36, 22, 28 };
                for (int c = 0; c < widths.Length; c++) sheet.SetColumnWidth(c, widths[c] * 256);
                for (int c = FixedHeaders.Length; c < FixedHeaders.Length + 16; c++) sheet.SetColumnWidth(c, 9 * 256);
                sheet.CreateFreezePane(0, 2);

                wb.Write(ms);
                return ms.ToArray();
            }
        }

        private static void Put(IRow row, int col, string value, ICellStyle style)
        {
            var cell = row.CreateCell(col);
            if (value != null) cell.SetCellValue(value);
            cell.CellStyle = style;
        }

        private static void SetBorder(ICellStyle s)
        {
            s.BorderTop = s.BorderBottom = s.BorderLeft = s.BorderRight = BorderStyle.Thin;
        }
    }
}
