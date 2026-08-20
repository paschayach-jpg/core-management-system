using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace RG_Project.Project.Forms
{
    public class DataGridViewProgressBarCell : DataGridViewTextBoxCell
    {
        protected override void Paint(
            Graphics g,
            Rectangle clipBounds,
            Rectangle cellBounds,
            int rowIndex,
            DataGridViewElementStates cellState,
            object value,
            object formattedValue,
            string errorText,
            DataGridViewCellStyle cellStyle,
            DataGridViewAdvancedBorderStyle advancedBorderStyle,
            DataGridViewPaintParts paintParts)
        {
            // วาด background + border ปกติ
            base.Paint(g, clipBounds, cellBounds, rowIndex, cellState, value,
                formattedValue, errorText, cellStyle, advancedBorderStyle,
                paintParts & ~(DataGridViewPaintParts.ContentForeground | DataGridViewPaintParts.ContentBackground));

            // ถ้า column นี้ bind กับ i_pct_complete และ row มี i_ref_project_id
            // ให้ใช้ i_ref_pct_overall แทน
            int pct = 0;
            try
            {
                if (this.DataGridView != null && rowIndex >= 0
                    && this.OwningColumn?.DataPropertyName == "i_pct_complete")
                {
                    var src = this.DataGridView.DataSource as System.Data.DataTable;
                    if (src != null && rowIndex < src.Rows.Count)
                    {
                        var row = src.Rows[rowIndex];
                        bool hasRef = src.Columns.Contains("i_ref_project_id")
                                   && row["i_ref_project_id"] != System.DBNull.Value;
                        if (hasRef && src.Columns.Contains("i_ref_pct_overall"))
                        {
                            int.TryParse(row["i_ref_pct_overall"].ToString(), out pct);
                            pct = Math.Max(0, Math.Min(100, pct));
                            goto DrawBar;
                        }
                    }
                }
                // ค่าปกติ
                if (value != null && value != System.DBNull.Value)
                    int.TryParse(value.ToString(), out pct);
                pct = Math.Max(0, Math.Min(100, pct));
            }
            catch { pct = 0; }

            DrawBar:

            // พื้นที่วาด bar (เว้น padding 3px)
            var barRect = new Rectangle(
                cellBounds.X + 3,
                cellBounds.Y + 4,
                cellBounds.Width - 7,
                cellBounds.Height - 9);

            // พื้นหลัง bar
            using (var bgBrush = new SolidBrush(Color.FromArgb(220, 228, 240)))
                g.FillRectangle(bgBrush, barRect);

            // bar เติม
            if (pct > 0)
            {
                int fillW = (int)(barRect.Width * pct / 100.0);
                var fillRect = new Rectangle(barRect.X, barRect.Y, fillW, barRect.Height);

                // gradient สีตาม %
                Color c1, c2;
                if (pct >= 100)     { c1 = Color.FromArgb(56, 161, 105);  c2 = Color.FromArgb(39, 120, 78);  }
                else if (pct >= 60) { c1 = Color.FromArgb(66, 153, 225);  c2 = Color.FromArgb(43, 108, 176); }
                else if (pct >= 30) { c1 = Color.FromArgb(246, 173, 85);  c2 = Color.FromArgb(221, 107, 32); }
                else                { c1 = Color.FromArgb(252, 129, 129); c2 = Color.FromArgb(229, 62, 62);  }

                using (var grad = new LinearGradientBrush(fillRect, c1, c2, LinearGradientMode.Horizontal))
                    g.FillRectangle(grad, fillRect);
            }

            // border bar
            using (var pen = new Pen(Color.FromArgb(180, 200, 230)))
                g.DrawRectangle(pen, barRect);

            // ข้อความ %
            string txt = $"{pct}%";
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            using (var fnt = new Font("Segoe UI", 8F, FontStyle.Bold))
            {
                // shadow เล็กน้อย
                g.DrawString(txt, fnt, Brushes.White,
                    new Rectangle(barRect.X + 1, barRect.Y + 1, barRect.Width, barRect.Height), sf);
                g.DrawString(txt, fnt, new SolidBrush(Color.FromArgb(30, 58, 95)), barRect, sf);
            }
        }

        public override Type ValueType => typeof(int);
    }

    public class DataGridViewProgressBarColumn : DataGridViewTextBoxColumn
    {
        public DataGridViewProgressBarColumn()
        {
            CellTemplate = new DataGridViewProgressBarCell();
            DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
        }
    }
}
