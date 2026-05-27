using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Presentation
{
    public class QuotaUsageChartForm : Form
    {
        private readonly QuotaUsageResult     _usage;
        private readonly IList<FileTypeUsage> _fileStats;
        private readonly IList<FileDocument>  _files;

        private static readonly Color[] PieColors = new Color[]
        {
            Color.FromArgb(41,  128, 185),
            Color.FromArgb(39,  174,  96),
            Color.FromArgb(231,  76,  60),
            Color.FromArgb(243, 156,  18),
            Color.FromArgb(142,  68, 173),
            Color.FromArgb(26,  188, 156),
            Color.FromArgb(230,  76, 100),
            Color.FromArgb(52,  152, 219),
        };

        // actual used bytes calculated from file list (more accurate than batch query when quota_size=0)
        private long _actualUsedBytes;

        public QuotaUsageChartForm(QuotaUsageResult usage, IList<FileTypeUsage> fileStats,
            IList<FileDocument> files = null)
        {
            _usage     = usage     ?? new QuotaUsageResult();
            _fileStats = fileStats ?? new List<FileTypeUsage>();
            _files     = files     ?? new List<FileDocument>();
            _actualUsedBytes = _files.Sum(f => f.FileSizeBytes);

            Text            = string.Format("การใช้พื้นที่ — {0}", _usage.OrgUnitName);
            Width           = 920;
            Height          = 800;
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            Font            = new Font("Segoe UI", 9F);
            BackColor       = Color.White;

            BuildUI();
        }

        private string BuildKpiText()
        {
            double quotaGB = _usage.QuotaGB;
            double usedGB  = _actualUsedBytes / 1073741824.0;
            double freeGB  = quotaGB > 0 ? quotaGB - usedGB : 0;
            double usedPct = quotaGB > 0 ? Math.Min(_actualUsedBytes * 100.0 / _usage.QuotaBytes, 100.0) : 0;
            return string.Format(
                "Quota: {0:F1} GB    ใช้ไป: {1:F2} GB    คงเหลือ: {2:F2} GB    ใช้งาน: {3:F1}%",
                quotaGB, usedGB, freeGB, usedPct);
        }

        private void BuildUI()
        {
            // Header
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 52,
                BackColor = Color.FromArgb(26, 58, 108)
            };
            var lblTitle = new Label
            {
                Text      = string.Format("  การใช้พื้นที่ Quota — {0}", _usage.OrgUnitName),
                Dock      = DockStyle.Fill,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 13F, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft
            };
            pnlHeader.Controls.Add(lblTitle);

            // KPI bar
            var pnlKpi = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 40,
                BackColor = Color.FromArgb(240, 244, 250),
                Padding   = new Padding(8, 0, 8, 0)
            };
            var lblKpi = new Label
            {
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleLeft,
                Font      = new Font("Segoe UI", 9.5F),
                ForeColor = Color.FromArgb(40, 40, 40),
                Text      = BuildKpiText()
            };
            pnlKpi.Controls.Add(lblKpi);

            // Chart panels (fixed height so file list can fill below)
            var pnlCharts = new Panel { Dock = DockStyle.Top, Height = 310, Padding = new Padding(12) };

            // Left donut — Used space (blue)
            var pnlDonut = new Panel { Width = 260, Dock = DockStyle.Left, BackColor = Color.White };
            var lblDonutTitle = new Label
            {
                Text      = "พื้นที่ใช้งาน",
                Dock      = DockStyle.Top,
                Height    = 26,
                TextAlign = ContentAlignment.MiddleCenter,
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 58, 108)
            };
            var donutCanvas = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            donutCanvas.Paint += (s, e) => DrawUsedDonut(e.Graphics, donutCanvas.ClientRectangle);
            pnlDonut.Controls.Add(donutCanvas);
            pnlDonut.Controls.Add(lblDonutTitle);

            // Middle donut — Free space (gray)
            var pnlFree = new Panel { Width = 260, Dock = DockStyle.Left, BackColor = Color.White };
            var lblFreeTitle = new Label
            {
                Text      = "พื้นที่ว่างคงเหลือ",
                Dock      = DockStyle.Top,
                Height    = 26,
                TextAlign = ContentAlignment.MiddleCenter,
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 58, 108)
            };
            var freeCanvas = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            freeCanvas.Paint += (s, e) => DrawFreeDonut(e.Graphics, freeCanvas.ClientRectangle);
            pnlFree.Controls.Add(freeCanvas);
            pnlFree.Controls.Add(lblFreeTitle);

            // Right — file type pie
            var pnlPie = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            var lblPieTitle = new Label
            {
                Text      = "สัดส่วนประเภทไฟล์",
                Dock      = DockStyle.Top,
                Height    = 26,
                TextAlign = ContentAlignment.MiddleCenter,
                Font      = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 58, 108)
            };
            var pieCanvas = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            pieCanvas.Paint += (s, e) => DrawPie(e.Graphics, pieCanvas.ClientRectangle);
            pnlPie.Controls.Add(pieCanvas);
            pnlPie.Controls.Add(lblPieTitle);

            // Dividers between panels
            var div1 = new Panel { Width = 1, Dock = DockStyle.Left, BackColor = Color.FromArgb(220, 225, 235) };
            var div2 = new Panel { Width = 1, Dock = DockStyle.Left, BackColor = Color.FromArgb(220, 225, 235) };

            pnlCharts.Controls.Add(pnlPie);
            pnlCharts.Controls.Add(div2);
            pnlCharts.Controls.Add(pnlFree);
            pnlCharts.Controls.Add(div1);
            pnlCharts.Controls.Add(pnlDonut);

            // Footer
            var pnlFooter = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 46,
                BackColor = Color.FromArgb(245, 247, 250)
            };
            var btnClose = new Button
            {
                Text     = "ปิด",
                Width    = 90,
                Height   = 30,
                Anchor   = AnchorStyles.Right | AnchorStyles.Bottom,
                Location = new Point(Width - 110, 8),
                BackColor = Color.FromArgb(100, 100, 100),
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();
            pnlFooter.Controls.Add(btnClose);

            // File list panel
            var pnlFiles = new Panel { Dock = DockStyle.Fill, Padding = new Padding(8, 4, 8, 4) };

            var lblFiles = new Label
            {
                Dock      = DockStyle.Top,
                Height    = 26,
                TextAlign = ContentAlignment.MiddleLeft,
                Font      = new Font("Segoe UI", 9.5F, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 58, 108),
                Text      = string.Format("  รายการไฟล์ทั้งหมด ({0} ไฟล์)", _files.Count)
            };

            var dgv = new DataGridView
            {
                Dock                = DockStyle.Fill,
                ReadOnly            = true,
                AllowUserToAddRows  = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible   = false,
                AutoSizeRowsMode    = DataGridViewAutoSizeRowsMode.None,
                RowTemplate         = { Height = 24 },
                SelectionMode       = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect         = false,
                BorderStyle         = BorderStyle.None,
                BackgroundColor     = Color.White,
                GridColor           = Color.FromArgb(220, 225, 235),
                Font                = new Font("Segoe UI", 8.5F)
            };
            dgv.ColumnHeadersDefaultCellStyle.BackColor  = Color.FromArgb(26, 58, 108);
            dgv.ColumnHeadersDefaultCellStyle.ForeColor  = Color.White;
            dgv.ColumnHeadersDefaultCellStyle.Font       = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            dgv.ColumnHeadersDefaultCellStyle.Alignment  = DataGridViewContentAlignment.MiddleCenter;
            dgv.ColumnHeadersHeight = 28;
            dgv.EnableHeadersVisualStyles = false;
            dgv.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(245, 248, 252);

            var colNo = new DataGridViewTextBoxColumn
            {
                HeaderText = "#", Width = 42, MinimumWidth = 42,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            var colName = new DataGridViewTextBoxColumn
            {
                HeaderText = "ชื่อไฟล์", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill
            };
            var colExt = new DataGridViewTextBoxColumn
            {
                HeaderText = "ประเภท", Width = 70, MinimumWidth = 60,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            var colSize = new DataGridViewTextBoxColumn
            {
                HeaderText = "ขนาด", Width = 90, MinimumWidth = 80,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleRight }
            };
            var colDate = new DataGridViewTextBoxColumn
            {
                HeaderText = "วันที่อัปโหลด", Width = 130, MinimumWidth = 110,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            var colVer = new DataGridViewTextBoxColumn
            {
                HeaderText = "เวอร์ชัน", Width = 62, MinimumWidth = 55,
                DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter }
            };
            dgv.Columns.AddRange(colNo, colName, colExt, colSize, colDate, colVer);

            int row = 1;
            foreach (var f in _files)
            {
                string ext  = string.IsNullOrWhiteSpace(f.FileExtension)
                    ? "-" : f.FileExtension.TrimStart('.').ToUpperInvariant();
                string size = FormatFileSize(f.FileSizeBytes);
                dgv.Rows.Add(row++, f.OriginalFileName, ext, size,
                    f.UploadedDate.ToString("dd/MM/yyyy HH:mm"), f.Version);
            }

            // Summary bar — total size
            double quotaGB    = _usage.QuotaGB;
            double usedGB     = _actualUsedBytes / 1073741824.0;
            double usedPctSum = quotaGB > 0 ? Math.Min(_actualUsedBytes * 100.0 / _usage.QuotaBytes, 100.0) : 0;
            var pnlSummary = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 32,
                BackColor = Color.FromArgb(240, 244, 250)
            };
            var lblSummary = new Label
            {
                Dock      = DockStyle.Fill,
                TextAlign = ContentAlignment.MiddleRight,
                Padding   = new Padding(0, 0, 12, 0),
                Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 58, 108),
                Text      = string.Format(
                    "รวมขนาดทั้งหมด: {0}    ({1:F2} GB / Quota {2:F1} GB = {3:F1}% ที่ใช้ไป)",
                    FormatFileSize(_actualUsedBytes), usedGB, quotaGB, usedPctSum)
            };
            pnlSummary.Controls.Add(lblSummary);

            pnlFiles.Controls.Add(dgv);
            pnlFiles.Controls.Add(pnlSummary);
            pnlFiles.Controls.Add(lblFiles);

            Controls.Add(pnlFiles);
            Controls.Add(pnlCharts);
            Controls.Add(pnlKpi);
            Controls.Add(pnlHeader);
            Controls.Add(pnlFooter);
        }

        private void DrawUsedDonut(Graphics g, Rectangle bounds)
        {
            double usedPct = _usage.QuotaBytes > 0
                ? Math.Min(_actualUsedBytes * 100.0 / _usage.QuotaBytes, 100.0) : 0;
            double actualUsedGB = _actualUsedBytes / 1073741824.0;
            Color  usedColor    = GetUsageColor(usedPct);
            Color  freeColor    = Color.FromArgb(220, 225, 235);

            DrawWindowsStyleDonut(g, bounds,
                primaryArcColor : usedColor,
                secondaryArcColor: freeColor,
                primaryDeg       : (float)(usedPct / 100.0 * 360.0),
                centreText       : string.Format("{0:F1}%", usedPct),
                centreColor      : usedColor,
                line1            : string.Format("ใช้งาน {0}", FormatFileSize(_actualUsedBytes)),
                line2            : string.Format("{0:F2} GB", actualUsedGB),
                showThreshold    : true);
        }

        private void DrawFreeDonut(Graphics g, Rectangle bounds)
        {
            long   freeBytes = _usage.QuotaBytes > 0
                ? Math.Max(_usage.QuotaBytes - _actualUsedBytes, 0L) : 0L;
            double freePct   = _usage.QuotaBytes > 0
                ? Math.Min(freeBytes * 100.0 / _usage.QuotaBytes, 100.0) : 0;
            double freeGB    = freeBytes / 1073741824.0;
            Color  freeColor = Color.FromArgb(100, 160, 220);
            Color  usedColor = Color.FromArgb(220, 225, 235);

            DrawWindowsStyleDonut(g, bounds,
                primaryArcColor : freeColor,
                secondaryArcColor: usedColor,
                primaryDeg       : (float)(freePct / 100.0 * 360.0),
                centreText       : string.Format("{0:F1}%", freePct),
                centreColor      : freeColor,
                line1            : string.Format("ว่าง {0}", FormatFileSize(freeBytes)),
                line2            : string.Format("{0:F2} GB", freeGB),
                showThreshold    : false);
        }

        private static void DrawWindowsStyleDonut(Graphics g, Rectangle bounds,
            Color primaryArcColor, Color secondaryArcColor,
            float primaryDeg, string centreText, Color centreColor,
            string line1, string line2, bool showThreshold)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            int margin = 18;
            int size   = Math.Min(bounds.Width, bounds.Height - 90) - margin * 2;
            if (size < 40) return;

            int cx = bounds.Width / 2;
            int cy = (bounds.Height - 90) / 2 + margin;
            var rect = new Rectangle(cx - size / 2, cy - size / 2, size, size);

            float freeDeg = 360f - primaryDeg;

            if (primaryDeg > 0)
                g.FillPie(new SolidBrush(primaryArcColor), rect, -90f, primaryDeg);
            if (freeDeg > 0)
                g.FillPie(new SolidBrush(secondaryArcColor), rect, -90f + primaryDeg, freeDeg);

            // Donut hole (Windows style — thicker ring)
            int holeSize = (int)(size * 0.55);
            var hole = new Rectangle(cx - holeSize / 2, cy - holeSize / 2, holeSize, holeSize);
            g.FillEllipse(new SolidBrush(Color.White), hole);

            // Centre text
            var centreFont = new Font("Segoe UI", 13F, FontStyle.Bold);
            var centreSize = g.MeasureString(centreText, centreFont);
            g.DrawString(centreText, centreFont, new SolidBrush(centreColor),
                cx - centreSize.Width / 2, cy - centreSize.Height / 2);

            if (showThreshold)
            {
                DrawThresholdMarker(g, rect, cx, cy, 80f, Color.FromArgb(230, 150, 0), "80%");
                DrawThresholdMarker(g, rect, cx, cy, 95f, Color.FromArgb(200, 50,  50), "95%");
            }

            // Info lines below ring (Windows style)
            int infoY = rect.Bottom + 12;
            var lineFont = new Font("Segoe UI", 8.5F);
            var boldFont = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            var sz1 = g.MeasureString(line1, boldFont);
            g.DrawString(line1, boldFont, new SolidBrush(primaryArcColor), cx - sz1.Width / 2, infoY);
            var sz2 = g.MeasureString(line2, lineFont);
            g.DrawString(line2, lineFont, Brushes.Gray, cx - sz2.Width / 2, infoY + sz1.Height + 1);
        }

        private static void DrawThresholdMarker(Graphics g, Rectangle rect, int cx, int cy,
            float percent, Color color, string label)
        {
            double angle = (percent / 100.0 * 360.0 - 90.0) * Math.PI / 180.0;
            int    r     = rect.Width / 2;
            int    x1    = cx + (int)(r * Math.Cos(angle));
            int    y1    = cy + (int)(r * Math.Sin(angle));
            int    x0    = cx + (int)(r * 0.50 * Math.Cos(angle));
            int    y0    = cy + (int)(r * 0.50 * Math.Sin(angle));

            using (var pen = new Pen(color, 1.5f) { DashStyle = DashStyle.Dash })
                g.DrawLine(pen, x0, y0, x1, y1);

            var lf = new Font("Segoe UI", 7F);
            g.DrawString(label, lf, new SolidBrush(color), x1 + 2, y1 - 7);
        }

        private static void DrawLegendItem(Graphics g, int x, int y, Color color, string text)
        {
            g.FillRectangle(new SolidBrush(color), x, y + 3, 14, 14);
            g.DrawString(text, new Font("Segoe UI", 8.5F), Brushes.Black, x + 18, y);
        }

        private void DrawPie(Graphics g, Rectangle bounds)
        {
            g.SmoothingMode = SmoothingMode.AntiAlias;

            if (_fileStats == null || _fileStats.Count == 0)
            {
                var noDataFont = new Font("Segoe UI", 10F, FontStyle.Italic);
                g.DrawString("ไม่มีข้อมูลไฟล์", noDataFont, Brushes.Gray,
                    bounds.Width / 2 - 50, bounds.Height / 2 - 10);
                return;
            }

            // Show top 7 + combine the rest as "อื่นๆ"
            var top       = _fileStats.Take(7).ToList();
            double others = _fileStats.Skip(7).Sum(x => x.Percent);
            if (others > 0)
                top.Add(new FileTypeUsage { Extension = "อื่นๆ", Percent = Math.Round(others, 1) });

            int margin   = 20;
            int legendH  = (top.Count * 20) + 8;
            int size     = Math.Min(bounds.Width - margin * 2, bounds.Height - legendH - margin * 2 - 30);
            if (size < 40) return;

            int cx   = bounds.Width / 2;
            int cy   = margin + size / 2 + 4;
            var rect = new Rectangle(cx - size / 2, cy - size / 2, size, size);

            float start = -90f;
            for (int i = 0; i < top.Count; i++)
            {
                float sweep = (float)(top[i].Percent / 100.0 * 360.0);
                if (sweep < 0.5f) continue;
                Color c = PieColors[i % PieColors.Length];
                g.FillPie(new SolidBrush(c), rect, start, sweep);
                g.DrawPie(new Pen(Color.White, 1.5f), rect, start, sweep);

                // Percentage label on slice
                if (sweep > 18)
                {
                    double mid    = (start + sweep / 2.0) * Math.PI / 180.0;
                    int    lx     = cx + (int)(size * 0.32 * Math.Cos(mid));
                    int    ly     = cy + (int)(size * 0.32 * Math.Sin(mid));
                    string pctTxt = top[i].Percent.ToString("F1") + "%";
                    var    pctF   = new Font("Segoe UI", 7.5F, FontStyle.Bold);
                    var    pctSz  = g.MeasureString(pctTxt, pctF);
                    g.DrawString(pctTxt, pctF, Brushes.White, lx - pctSz.Width / 2, ly - pctSz.Height / 2);
                }

                start += sweep;
            }

            // Legend
            int legendY = rect.Bottom + 12;
            for (int i = 0; i < top.Count; i++)
            {
                int col = i % 2;
                int row = i / 2;
                int lx  = margin + col * (bounds.Width / 2 - margin);
                int ly  = legendY + row * 20;
                Color c = PieColors[i % PieColors.Length];
                g.FillRectangle(new SolidBrush(c), lx, ly + 3, 12, 12);
                string lbl = string.Format(".{0}  {1:F1}%", top[i].Extension, top[i].Percent);
                g.DrawString(lbl, new Font("Segoe UI", 8F), Brushes.Black, lx + 16, ly);
            }
        }

        private static Color GetUsageColor(double pct)
        {
            if (pct >= 100) return Color.FromArgb(180, 30, 30);
            if (pct >= 95)  return Color.FromArgb(200, 60, 40);
            if (pct >= 80)  return Color.FromArgb(220, 140, 0);
            return Color.FromArgb(39, 174, 96);
        }

        private static string FormatFileSize(long bytes)
        {
            if (bytes >= 1073741824L) return string.Format("{0:F2} GB", bytes / 1073741824.0);
            if (bytes >= 1048576L)    return string.Format("{0:F2} MB", bytes / 1048576.0);
            if (bytes >= 1024L)       return string.Format("{0:F1} KB", bytes / 1024.0);
            return string.Format("{0} B", bytes);
        }
    }
}
