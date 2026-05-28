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

        private static readonly Color[] CategoryColors = new Color[]
        {
            Color.FromArgb(52,  120,  47),   // รูปภาพ
            Color.FromArgb(26,   58, 108),   // เอกสาร
            Color.FromArgb(180,  80,   0),   // VOD
            Color.FromArgb(110, 110, 110),   // อื่นๆ
        };
        private static readonly string[] CategoryLabels = { "รูปภาพ", "เอกสาร", "VOD", "อื่นๆ" };

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
            var colCat = new DataGridViewTextBoxColumn
            {
                HeaderText = "หมวดหมู่", Width = 82, MinimumWidth = 72,
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
            dgv.Columns.AddRange(colNo, colName, colExt, colCat, colSize, colDate, colVer);

            int row = 1;
            foreach (var f in _files)
            {
                string ext  = string.IsNullOrWhiteSpace(f.FileExtension)
                    ? "-" : f.FileExtension.TrimStart('.').ToUpperInvariant();
                string cat  = CategoryLabels[ClassifyFile(f.FileExtension)];
                string size = FormatFileSize(f.FileSizeBytes);
                int rowIdx = dgv.Rows.Add(row++, f.OriginalFileName, ext, cat, size,
                    f.UploadedDate.ToString("dd/MM/yyyy HH:mm"), f.Version);

                // สีตาม category
                dgv.Rows[rowIdx].Cells[colCat.Index].Style.ForeColor =
                    CategoryColors[ClassifyFile(f.FileExtension)];
                dgv.Rows[rowIdx].Cells[colCat.Index].Style.Font =
                    new Font("Segoe UI", 8.5F, FontStyle.Bold);
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

            var cats = BuildFileCategoryStats();
            long total = cats.Sum(c => c.Bytes);

            if (total == 0)
            {
                using (var f = new Font("Segoe UI", 10F, FontStyle.Italic))
                    g.DrawString("ไม่มีข้อมูลไฟล์", f, Brushes.Gray,
                        bounds.Width / 2 - 50, bounds.Height / 2 - 10);
                return;
            }

            int margin  = 20;
            int legendH = (CategoryLabels.Length * 22) + 10;
            int size    = Math.Min(bounds.Width - margin * 2, bounds.Height - legendH - margin * 2 - 20);
            if (size < 40) return;

            int cx   = bounds.Width / 2;
            int cy   = margin + size / 2 + 4;
            var rect = new Rectangle(cx - size / 2, cy - size / 2, size, size);

            float start = -90f;
            for (int i = 0; i < cats.Count; i++)
            {
                if (cats[i].Bytes == 0) continue;
                float sweep = (float)(cats[i].Bytes * 360.0 / total);
                if (sweep < 0.5f) continue;

                using (var brush = new SolidBrush(CategoryColors[i]))
                    g.FillPie(brush, rect, start, sweep);
                using (var pen = new Pen(Color.White, 1.5f))
                    g.DrawPie(pen, rect, start, sweep);

                if (sweep > 15)
                {
                    double mid    = (start + sweep / 2.0) * Math.PI / 180.0;
                    int    lx     = cx + (int)(size * 0.32 * Math.Cos(mid));
                    int    ly     = cy + (int)(size * 0.32 * Math.Sin(mid));
                    string pctTxt = string.Format("{0:F1}%", cats[i].Bytes * 100.0 / total);
                    using (var pf = new Font("Segoe UI", 8F, FontStyle.Bold))
                    {
                        var sz = g.MeasureString(pctTxt, pf);
                        g.DrawString(pctTxt, pf, Brushes.White, lx - sz.Width / 2, ly - sz.Height / 2);
                    }
                }

                start += sweep;
            }

            // Legend — 1 column, 4 rows
            int legendY = rect.Bottom + 14;
            using (var lf = new Font("Segoe UI", 8.5F))
            using (var bf = new Font("Segoe UI", 8.5F, FontStyle.Bold))
            {
                for (int i = 0; i < cats.Count; i++)
                {
                    int lx = margin;
                    int ly = legendY + i * 22;
                    using (var b = new SolidBrush(CategoryColors[i]))
                        g.FillRectangle(b, lx, ly + 3, 14, 14);
                    double pct = total > 0 ? cats[i].Bytes * 100.0 / total : 0;
                    string lbl = string.Format("{0}  {1}  ({2:F1}%)",
                        CategoryLabels[i], FormatFileSize(cats[i].Bytes), pct);
                    g.DrawString(lbl, cats[i].Count > 0 ? bf : lf,
                        cats[i].Count > 0 ? Brushes.Black : Brushes.Gray,
                        lx + 20, ly);
                }
            }
        }

        private List<FileCategoryStats> BuildFileCategoryStats()
        {
            var counts = new int[4];
            var bytes  = new long[4];
            foreach (var f in _files)
            {
                int cat = ClassifyFile(f.FileExtension);
                counts[cat]++;
                bytes[cat] += Math.Max(0, f.FileSizeBytes);
            }
            var result = new List<FileCategoryStats>();
            for (int i = 0; i < 4; i++)
                result.Add(new FileCategoryStats { Label = CategoryLabels[i], Count = counts[i], Bytes = bytes[i] });
            return result;
        }

        private static int ClassifyFile(string ext)
        {
            var e = (ext ?? string.Empty).TrimStart('.').ToLowerInvariant();
            switch (e)
            {
                case "jpg": case "jpeg": case "png": case "gif":
                case "bmp": case "tiff": case "tif": case "svg":
                case "webp": case "ico": case "heic": case "heif":
                    return 0;
                case "pdf": case "doc": case "docx": case "xls":
                case "xlsx": case "ppt": case "pptx": case "txt":
                case "csv": case "rtf": case "odt": case "ods": case "odp":
                    return 1;
                case "mp4": case "avi": case "mov": case "mkv":
                case "wmv": case "flv": case "m4v": case "mpg":
                case "mpeg": case "webm": case "ts": case "m2ts":
                    return 2;
                default:
                    return 3;
            }
        }

        private class FileCategoryStats
        {
            public string Label { get; set; }
            public int    Count { get; set; }
            public long   Bytes { get; set; }
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
