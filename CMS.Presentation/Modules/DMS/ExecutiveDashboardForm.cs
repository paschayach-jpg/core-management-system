using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    public class ExecutiveDashboardForm : Form
    {
        // ---------------------------------------------------------------
        // Fields
        // ---------------------------------------------------------------
        private readonly IFolderQuotaService             _quotaService;
        private List<FolderQuotaConfig>                  _configs   = new List<FolderQuotaConfig>();
        private Dictionary<int, OrgUnitFileStat>         _fileStats = new Dictionary<int, OrgUnitFileStat>();
        private List<FileDocument>                       _files     = new List<FileDocument>();
        private List<HierarchyRow>                       _rows      = new List<HierarchyRow>();
        private FolderQuotaConfig                        _selected;

        // Left tree
        private DoubleBufferedPanel _pnlTree;
        private VScrollBar          _vScroll;
        private int                 _scrollOffset;
        private int                 _hoverIdx   = -1;
        private int                 _selectedIdx = -1;
        private const int           ROW_H    = 46;
        private const int           HEADER_H = 26;

        // Middle
        private DoubleBufferedPanel _piePanel;
        private Panel               _snapshotPanel;

        // Right
        private Label        _lblFileHeader;
        private DataGridView _dgvFiles;
        private Button       _btnRecent, _btnLargest, _btnToday;
        private int          _fileMode = 0;

        // Top bar
        private Label _lblCrumb;

        // ---------------------------------------------------------------
        // Double-buffered panel helper
        // ---------------------------------------------------------------
        private sealed class DoubleBufferedPanel : Panel
        {
            public DoubleBufferedPanel()
            {
                SetStyle(ControlStyles.OptimizedDoubleBuffer |
                         ControlStyles.AllPaintingInWmPaint  |
                         ControlStyles.UserPaint, true);
                ResizeRedraw = true;
            }
        }

        // ---------------------------------------------------------------
        // Hierarchy row
        // ---------------------------------------------------------------
        private sealed class HierarchyRow
        {
            public bool              IsHeader;
            public string            HeaderText;
            public FolderQuotaConfig Config;
            public int               Level;     // 0=Sub 1=Div 2=Dep
            public bool              Expanded;
        }

        // ---------------------------------------------------------------
        // Constructor
        // ---------------------------------------------------------------
        public ExecutiveDashboardForm(IFolderQuotaService quotaService)
        {
            if (quotaService == null) throw new ArgumentNullException("quotaService");
            _quotaService = quotaService;
            BuildUI();
            LoadData();
        }

        // ---------------------------------------------------------------
        // Data loading
        // ---------------------------------------------------------------
        private void LoadData()
        {
            try
            {
                _configs   = _quotaService.GetAllConfigs().ToList();
                _fileStats = _quotaService.GetAllFileStats();
            }
            catch
            {
                _configs   = new List<FolderQuotaConfig>();
                _fileStats = new Dictionary<int, OrgUnitFileStat>();
            }

            BuildRows();
            _selected    = null;
            _selectedIdx = -1;
            _files       = new List<FileDocument>();
            _scrollOffset = 0;
            UpdateScrollBar();
            _pnlTree.Invalidate();
            UpdateBreadcrumb();
            _piePanel.Invalidate();
            RefreshSnapshotCards();
            RefreshFileGrid();
        }

        // ---------------------------------------------------------------
        // Row building
        // ---------------------------------------------------------------
        private void BuildRows()
        {
            _rows.Clear();
            var subs = _configs.Where(c => OrgType(c) == 1).OrderBy(c => c.OrgUnitName).ToList();
            var divs = _configs.Where(c => OrgType(c) == 2).OrderBy(c => c.OrgUnitName).ToList();
            var deps = _configs.Where(c => OrgType(c) == 3).ToList();

            if (subs.Count > 0)
            {
                _rows.Add(new HierarchyRow { IsHeader = true, HeaderText = string.Format("SubCommittee ({0})", subs.Count) });
                foreach (var s in subs)
                    _rows.Add(new HierarchyRow { Config = s, Level = 0 });
            }

            if (divs.Count > 0)
            {
                _rows.Add(new HierarchyRow { IsHeader = true, HeaderText = string.Format("Division ({0})", divs.Count) });
                foreach (var d in divs)
                {
                    _rows.Add(new HierarchyRow { Config = d, Level = 1, Expanded = false });
                    foreach (var dep in deps.Where(x => x.ParentOrgUnitID == d.OrgUnitID).OrderBy(x => x.OrgUnitName))
                        _rows.Add(new HierarchyRow { Config = dep, Level = 2 });
                }
            }
        }

        private List<int> VisibleIndices()
        {
            var result = new List<int>();
            for (int i = 0; i < _rows.Count; i++)
            {
                var r = _rows[i];
                if (r.IsHeader || r.Level != 2) { result.Add(i); continue; }
                // Dep: find parent Div
                for (int j = i - 1; j >= 0; j--)
                {
                    if (_rows[j].IsHeader) break;
                    if (_rows[j].Level == 1) { if (_rows[j].Expanded) result.Add(i); break; }
                }
            }
            return result;
        }

        private int TotalScrollHeight()
        {
            return VisibleIndices().Sum(i => _rows[i].IsHeader ? HEADER_H : ROW_H);
        }

        private void UpdateScrollBar()
        {
            if (_pnlTree == null) return;
            int total  = TotalScrollHeight();
            int client = _pnlTree.ClientSize.Height;
            if (total <= client)
            {
                _vScroll.Visible = false;
                _scrollOffset    = 0;
            }
            else
            {
                _vScroll.Visible = true;
                int max = total - client;
                _vScroll.Maximum   = max + _vScroll.LargeChange - 1;
                _vScroll.SmallChange = ROW_H;
                _vScroll.LargeChange = Math.Max(client, 1);
                _scrollOffset        = Math.Max(0, Math.Min(_scrollOffset, max));
                _vScroll.Value       = _scrollOffset;
            }
        }

        // ---------------------------------------------------------------
        // Left panel draw
        // ---------------------------------------------------------------
        private void Tree_Paint(object sender, PaintEventArgs e)
        {
            var g   = e.Graphics;
            g.SmoothingMode = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;

            int panelW = _pnlTree.ClientSize.Width - (_vScroll.Visible ? _vScroll.Width : 0);
            int y      = -_scrollOffset;

            foreach (int ri in VisibleIndices())
            {
                var row = _rows[ri];
                int h   = row.IsHeader ? HEADER_H : ROW_H;
                if (y + h > 0 && y < _pnlTree.ClientSize.Height)
                {
                    var rect = new Rectangle(0, y, panelW, h);
                    if (row.IsHeader) DrawHeaderRow(g, rect, row.HeaderText);
                    else              DrawEntityRow(g, rect, ri, row);
                }
                y += h;
                if (y >= _pnlTree.ClientSize.Height) break;
            }
        }

        private void DrawHeaderRow(Graphics g, Rectangle r, string text)
        {
            using (var b = new SolidBrush(Color.FromArgb(232, 238, 250)))
                g.FillRectangle(b, r);
            using (var f = new Font("Segoe UI", 8F, FontStyle.Bold))
            using (var tb = new SolidBrush(Color.FromArgb(50, 80, 140)))
                g.DrawString("  " + text.ToUpperInvariant(), f, tb,
                    r.Left + 6, r.Top + (r.Height - f.GetHeight(g)) / 2f);
        }

        private static readonly Color[] _barColors =
        {
            Color.FromArgb(52, 168, 83),    // < 80%
            Color.FromArgb(210, 130, 0),    // < 95%
            Color.FromArgb(192, 40, 40)     // >= 95%
        };

        private void DrawEntityRow(Graphics g, Rectangle r, int ri, HierarchyRow row)
        {
            bool isSel   = ri == _selectedIdx;
            bool isHover = ri == _hoverIdx;

            Color bg = isSel   ? Color.FromArgb(26, 58, 108)
                     : isHover ? Color.FromArgb(238, 244, 255)
                     :           Color.White;
            using (var b = new SolidBrush(bg))
                g.FillRectangle(b, r);

            // separator line
            using (var p = new Pen(Color.FromArgb(240, 243, 250)))
                g.DrawLine(p, r.Left, r.Bottom - 1, r.Right, r.Bottom - 1);

            int indent = 10 + row.Level * 16;

            // arrow / indent marker
            if (row.Level == 1)
            {
                string arrow = row.Expanded ? "▾" : "▸";
                using (var af = new Font("Segoe UI", 9F))
                using (var ab = new SolidBrush(isSel ? Color.White : Color.FromArgb(110, 140, 190)))
                    g.DrawString(arrow, af, ab, r.Left + indent - 2, r.Top + 6);
            }
            else if (row.Level == 2)
            {
                using (var lp = new Pen(Color.FromArgb(180, 200, 225), 1))
                    g.DrawLine(lp, r.Left + indent - 6, r.Top + r.Height / 2,
                                   r.Left + indent + 2, r.Top + r.Height / 2);
            }

            // Name
            int nameX = r.Left + indent + 16;
            int nameW = r.Right - nameX - 82;
            Color fg  = isSel ? Color.White : Color.FromArgb(25, 25, 25);
            using (var nf = new Font("Segoe UI", row.Level < 2 ? 9F : 8.5F, row.Level == 0 ? FontStyle.Bold : FontStyle.Regular))
            using (var nb = new SolidBrush(fg))
            {
                var fmt = new StringFormat { Trimming = StringTrimming.EllipsisCharacter, FormatFlags = StringFormatFlags.NoWrap };
                g.DrawString(row.Config.OrgUnitName ?? "", nf, nb,
                    new RectangleF(nameX, r.Top + 5, nameW, 16), fmt);
            }

            // Stat lookup
            OrgUnitFileStat stat;
            _fileStats.TryGetValue(row.Config.OrgUnitID, out stat);
            long usedBytes  = stat != null ? stat.UsedBytes  : 0;
            long quotaBytes = row.Config.QuotaBytes;
            double pct      = quotaBytes > 0 ? Math.Min(usedBytes * 100.0 / quotaBytes, 100.0) : -1;

            // % text
            string pctTxt = pct >= 0 ? string.Format("{0:F0}%", pct) : "—";
            using (var pf = new Font("Segoe UI", 8F, FontStyle.Bold))
            {
                Color pc = isSel ? Color.White
                         : pct >= 95 ? _barColors[2]
                         : pct >= 80 ? _barColors[1]
                         :             Color.FromArgb(70, 100, 140);
                g.DrawString(pctTxt, pf, new SolidBrush(pc), r.Right - 78, r.Top + 5);
            }

            // File count
            int fileCount = stat != null ? stat.FileCount : 0;
            string fcTxt  = fileCount > 0 ? string.Format("{0} ไฟล์", fileCount) : "0 ไฟล์";
            using (var ff = new Font("Segoe UI", 7.5F))
            using (var fb = new SolidBrush(isSel ? Color.FromArgb(180, 210, 255) : Color.FromArgb(130, 140, 160)))
                g.DrawString(fcTxt, ff, fb, r.Right - 78, r.Top + 22);

            // Progress bar
            int barX = nameX;
            int barY = r.Top + 26;
            int barW = nameW + 10;
            int barH = 5;
            Color trackColor = isSel ? Color.FromArgb(60, 90, 150) : Color.FromArgb(220, 228, 242);
            using (var tb2 = new SolidBrush(trackColor))
                g.FillRectangle(tb2, barX, barY, barW, barH);

            if (pct > 0)
            {
                int fill = (int)(barW * pct / 100.0);
                Color bc = isSel ? Color.FromArgb(100, 200, 130)
                         : pct >= 95 ? _barColors[2]
                         : pct >= 80 ? _barColors[1]
                         :             _barColors[0];
                using (var fb2 = new SolidBrush(bc))
                    g.FillRectangle(fb2, barX, barY, fill, barH);
            }
        }

        // ---------------------------------------------------------------
        // Left panel mouse
        // ---------------------------------------------------------------
        private void Tree_MouseMove(object sender, MouseEventArgs e)
        {
            int ri = HitTest(e.Y);
            if (ri == _hoverIdx) return;
            _hoverIdx = ri;
            _pnlTree.Invalidate();
        }

        private void Tree_MouseClick(object sender, MouseEventArgs e)
        {
            int ri = HitTest(e.Y);
            if (ri < 0) return;
            var row = _rows[ri];

            if (row.Level == 1)
                row.Expanded = !row.Expanded;

            _selectedIdx = ri;
            _selected    = row.Config;
            UpdateScrollBar();
            UpdateBreadcrumb();
            LoadFilesForSelected();
            _pnlTree.Invalidate();
            _piePanel.Invalidate();
            RefreshSnapshotCards();
        }

        private void Tree_MouseWheel(object sender, MouseEventArgs e)
        {
            if (!_vScroll.Visible) return;
            int delta = -e.Delta / 120 * ROW_H * 2;
            int newVal = Math.Max(0, Math.Min(_scrollOffset + delta,
                Math.Max(0, TotalScrollHeight() - _pnlTree.ClientSize.Height)));
            _scrollOffset  = newVal;
            _vScroll.Value = Math.Min(newVal, _vScroll.Maximum - _vScroll.LargeChange + 1);
            _pnlTree.Invalidate();
        }

        private int HitTest(int mouseY)
        {
            int y = -_scrollOffset;
            foreach (int ri in VisibleIndices())
            {
                var row = _rows[ri];
                int h   = row.IsHeader ? HEADER_H : ROW_H;
                if (mouseY >= y && mouseY < y + h)
                    return row.IsHeader ? -1 : ri;
                y += h;
                if (y > _pnlTree.ClientSize.Height + ROW_H) break;
            }
            return -1;
        }

        // ---------------------------------------------------------------
        // Breadcrumb
        // ---------------------------------------------------------------
        private void UpdateBreadcrumb()
        {
            if (_selected == null) { _lblCrumb.Text = "  ROOT  ›  ทั้งหมด"; return; }
            int t = OrgType(_selected);
            if (t == 1)
                _lblCrumb.Text = string.Format("  ROOT  ›  SubCommittee  ›  {0}", _selected.OrgUnitName);
            else if (t == 2)
                _lblCrumb.Text = string.Format("  ROOT  ›  Division  ›  {0}", _selected.OrgUnitName);
            else
            {
                string par = "";
                if (_selected.ParentOrgUnitID.HasValue)
                {
                    var p = _configs.FirstOrDefault(c => c.OrgUnitID == _selected.ParentOrgUnitID.Value);
                    if (p != null) par = p.OrgUnitName + "  ›  ";
                }
                _lblCrumb.Text = string.Format("  ROOT  ›  Division  ›  {0}Department  ›  {1}", par, _selected.OrgUnitName);
            }
        }

        // ---------------------------------------------------------------
        // File list
        // ---------------------------------------------------------------
        private void LoadFilesForSelected()
        {
            if (_selected == null) { _files = new List<FileDocument>(); RefreshFileGrid(); return; }
            try
            {
                _files = _quotaService.GetFilesByOrgUnit(_selected.OrgUnitID)
                             .Where(f => f != null).ToList();
            }
            catch { _files = new List<FileDocument>(); }
            RefreshFileGrid();
        }

        private void RefreshFileGrid()
        {
            DateTime today = DateTime.Today;
            IEnumerable<FileDocument> src;
            switch (_fileMode)
            {
                case 1: src = _files.OrderByDescending(f => f.FileSizeBytes).Take(10); break;
                case 2: src = _files.Where(f => f.UploadedDate.Date == today).OrderByDescending(f => f.UploadedDate); break;
                default: src = _files.OrderByDescending(f => f.UploadedDate).Take(10); break;
            }

            _dgvFiles.SuspendLayout();
            _dgvFiles.Rows.Clear();
            foreach (var f in src)
            {
                int row = _dgvFiles.Rows.Add(
                    Trunc(f.FolderName ?? "-", 24),
                    f.OriginalFileName ?? f.StoredFileName ?? "-",
                    (f.FileExtension ?? "").TrimStart('.').ToUpperInvariant(),
                    SizeStr(f.FileSizeBytes),
                    f.UploadedDate.ToString("dd/MM/yyyy HH:mm")
                );
                if (row % 2 == 1)
                    _dgvFiles.Rows[row].DefaultCellStyle.BackColor = Color.FromArgb(248, 251, 255);
            }
            _dgvFiles.ResumeLayout();
            int total = _files.Count;
            _lblFileHeader.Text = string.Format("  ไฟล์ในหน่วยงาน  —  รวม {0} ไฟล์", total);
        }

        // ---------------------------------------------------------------
        // Pie chart
        // ---------------------------------------------------------------
        private void Pie_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode    = SmoothingMode.AntiAlias;
            g.TextRenderingHint = System.Drawing.Text.TextRenderingHint.ClearTypeGridFit;
            int pw = _piePanel.ClientSize.Width;
            int ph = _piePanel.ClientSize.Height;

            // title
            using (var tf = new Font("Segoe UI", 9F, FontStyle.Bold))
            using (var tb = new SolidBrush(Color.FromArgb(26, 58, 108)))
                g.DrawString("Breakdown  —  เปรียบเทียบหน่วยงานเดียวกัน", tf, tb, 10, 6);

            if (_selected == null)
            {
                using (var mf = new Font("Segoe UI", 9F))
                using (var mb = new SolidBrush(Color.FromArgb(160, 160, 160)))
                    g.DrawString("เลือกหน่วยงานจากรายการซ้าย", mf, mb, pw / 2f - 100, ph / 2f);
                return;
            }

            var siblings = GetSiblings();
            if (siblings.Count == 0) return;
            long grandTotal = Math.Max(1, siblings.Sum(s => s.Used));

            // pie size / position
            int pieSize = Math.Min(ph - 60, 380);
            int pieX    = 16;
            int pieY    = 28;

            Color[] pal = {
                Color.FromArgb(41, 98, 195),   Color.FromArgb(52, 168, 83),
                Color.FromArgb(192, 57, 43),   Color.FromArgb(230, 126, 34),
                Color.FromArgb(142, 68, 173),  Color.FromArgb(26, 188, 156),
                Color.FromArgb(231, 76, 60),   Color.FromArgb(46, 134, 193)
            };

            float angle = -90f;
            for (int i = 0; i < siblings.Count; i++)
            {
                float sweep = Math.Max(0.5f, (float)(siblings[i].Used * 360.0 / grandTotal));
                Color c     = pal[i % pal.Length];
                bool  sel   = siblings[i].OrgUnitID == _selected.OrgUnitID;
                var   rect  = sel
                    ? new Rectangle(pieX - 5, pieY - 5, pieSize + 10, pieSize + 10)
                    : new Rectangle(pieX, pieY, pieSize, pieSize);
                using (var pb = new SolidBrush(c))
                    g.FillPie(pb, rect, angle, sweep);
                angle += sweep;
            }

            // hole
            int hole = (int)(pieSize * 0.52);
            int hx   = pieX + (pieSize - hole) / 2;
            int hy   = pieY + (pieSize - hole) / 2;
            using (var wb = new SolidBrush(Color.White))
                g.FillEllipse(wb, hx, hy, hole, hole);

            // centre: selected %
            OrgUnitFileStat selStat;
            _fileStats.TryGetValue(_selected.OrgUnitID, out selStat);
            long selUsed = selStat != null ? selStat.UsedBytes : 0;
            string cText = string.Format("{0:F1}%", selUsed * 100.0 / grandTotal);
            using (var cf = new Font("Segoe UI", 14F, FontStyle.Bold))
            using (var cb = new SolidBrush(Color.FromArgb(26, 58, 108)))
            {
                var cs = g.MeasureString(cText, cf);
                g.DrawString(cText, cf, cb, hx + (hole - cs.Width) / 2f, hy + (hole - cs.Height) / 2f);
            }

            // legend (right of pie)
            int lx = pieX + pieSize + 24;
            int ly = pieY + 4;
            int maxLegend = Math.Min(siblings.Count, (ph - ly - 10) / 22);
            for (int i = 0; i < maxLegend; i++)
            {
                bool sel   = siblings[i].OrgUnitID == _selected.OrgUnitID;
                Color c    = pal[i % pal.Length];
                var   swR  = new Rectangle(lx, ly + i * 22 + 4, 11, 11);
                using (var sb2 = new SolidBrush(c))
                    g.FillRectangle(sb2, swR);
                string lab  = Trunc(siblings[i].Name, 20);
                string size = SizeStr(siblings[i].Used);
                using (var lf = new Font("Segoe UI", sel ? 8.5F : 8F, sel ? FontStyle.Bold : FontStyle.Regular))
                using (var lb = new SolidBrush(sel ? Color.FromArgb(26, 58, 108) : Color.FromArgb(50, 50, 60)))
                    g.DrawString(string.Format("{0}   {1}", lab, size), lf, lb, lx + 16, ly + i * 22);
            }
        }

        private sealed class SibEntry { public int OrgUnitID; public string Name; public long Used; }

        private List<SibEntry> GetSiblings()
        {
            int t = OrgType(_selected);
            IEnumerable<FolderQuotaConfig> pool;
            if (t == 3 && _selected.ParentOrgUnitID.HasValue)
                pool = _configs.Where(c => OrgType(c) == 3 && c.ParentOrgUnitID == _selected.ParentOrgUnitID);
            else if (t == 2)
                pool = _configs.Where(c => OrgType(c) == 2);
            else
                pool = _configs.Where(c => OrgType(c) == 1);

            return pool.Select(c =>
            {
                OrgUnitFileStat st;
                _fileStats.TryGetValue(c.OrgUnitID, out st);
                return new SibEntry { OrgUnitID = c.OrgUnitID, Name = c.OrgUnitName ?? "", Used = st != null ? st.UsedBytes : 0 };
            })
            .OrderByDescending(x => x.Used)
            .ToList();
        }

        // ---------------------------------------------------------------
        // Snapshot cards
        // ---------------------------------------------------------------
        private void RefreshSnapshotCards()
        {
            _snapshotPanel.Controls.Clear();
            long usedBytes = 0, quotaBytes = 0;
            int  fileCount = 0;
            if (_selected != null)
            {
                OrgUnitFileStat st;
                _fileStats.TryGetValue(_selected.OrgUnitID, out st);
                usedBytes  = st != null ? st.UsedBytes : 0;
                fileCount  = st != null ? st.FileCount : 0;
                quotaBytes = _selected.QuotaBytes;
            }
            long   freeBytes = quotaBytes > 0 ? Math.Max(0, quotaBytes - usedBytes) : 0;
            double pct       = quotaBytes > 0 ? Math.Min(usedBytes * 100.0 / quotaBytes, 100.0) : 0;

            var data = new[]
            {
                new { Lbl="USED",  Val=SizeStr(usedBytes),
                      Sub= quotaBytes > 0 ? string.Format("{0:F1}% ของ quota", pct) : "ยังไม่กำหนด quota",
                      Clr=Color.FromArgb(26, 80, 160) },
                new { Lbl="QUOTA", Val= quotaBytes > 0 ? SizeStr(quotaBytes) : "ไม่กำหนด",
                      Sub="", Clr=Color.FromArgb(52, 120, 47) },
                new { Lbl="FREE",  Val= quotaBytes > 0 ? SizeStr(freeBytes)  : "ไม่จำกัด",
                      Sub="", Clr=Color.FromArgb(0, 128, 140) },
                new { Lbl="FILES", Val=fileCount.ToString("N0"),
                      Sub="ไฟล์ทั้งหมด", Clr=Color.FromArgb(130, 70, 0) }
            };

            var tbl = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 4, RowCount = 1,
                BackColor = Color.FromArgb(245, 247, 252),
                Padding = new Padding(8, 6, 8, 6)
            };
            tbl.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            for (int i = 0; i < 4; i++) tbl.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 25));

            for (int i = 0; i < data.Length; i++)
            {
                var d = data[i];
                var card = new Panel { Margin = new Padding(4), BackColor = Color.White, Dock = DockStyle.Fill };
                card.Paint += (s, ev) =>
                {
                    var r = new Rectangle(0, 0, ((Panel)s).Width - 1, ((Panel)s).Height - 1);
                    using (var path = RoundedRect(r, 6))
                    {
                        using (var bb = new SolidBrush(Color.White))
                            ev.Graphics.FillPath(bb, path);
                        using (var pen = new Pen(Color.FromArgb(215, 225, 240)))
                            ev.Graphics.DrawPath(pen, path);
                    }
                };

                var pnl = new Panel { Dock = DockStyle.Fill, BackColor = Color.Transparent, Padding = new Padding(8, 6, 8, 4) };

                var lblTop = new Label { Text = d.Lbl, Dock = DockStyle.Top, Height = 20,
                    Font = new Font("Segoe UI", 7.5F, FontStyle.Bold),
                    ForeColor = d.Clr, TextAlign = ContentAlignment.BottomCenter, BackColor = Color.Transparent };
                var lblVal = new Label { Text = d.Val, Dock = DockStyle.Top, Height = 34,
                    Font = new Font("Segoe UI", 11F, FontStyle.Bold),
                    ForeColor = Color.FromArgb(25, 25, 25), TextAlign = ContentAlignment.MiddleCenter, BackColor = Color.Transparent };
                var lblSub = new Label { Text = d.Sub, Dock = DockStyle.Top, Height = 16,
                    Font = new Font("Segoe UI", 7.5F),
                    ForeColor = Color.FromArgb(120, 130, 145), TextAlign = ContentAlignment.TopCenter, BackColor = Color.Transparent };

                pnl.Controls.Add(lblSub);
                pnl.Controls.Add(lblVal);
                pnl.Controls.Add(lblTop);

                card.Controls.Add(pnl);
                tbl.Controls.Add(card, i, 0);
            }
            _snapshotPanel.Controls.Add(tbl);
        }

        private static GraphicsPath RoundedRect(Rectangle r, int rad)
        {
            var p = new GraphicsPath();
            p.AddArc(r.Left, r.Top, rad * 2, rad * 2, 180, 90);
            p.AddArc(r.Right - rad * 2, r.Top, rad * 2, rad * 2, 270, 90);
            p.AddArc(r.Right - rad * 2, r.Bottom - rad * 2, rad * 2, rad * 2, 0, 90);
            p.AddArc(r.Left, r.Bottom - rad * 2, rad * 2, rad * 2, 90, 90);
            p.CloseFigure();
            return p;
        }

        // ---------------------------------------------------------------
        // UI construction
        // ---------------------------------------------------------------
        private void BuildUI()
        {
            Text          = "Executive Dashboard — การใช้พื้นที่จัดเก็บ";
            Size          = new Size(1280, 760);
            MinimumSize   = new Size(1000, 600);
            StartPosition = FormStartPosition.CenterScreen;
            BackColor     = Color.FromArgb(240, 244, 252);
            Font          = new Font("Segoe UI", 9.5F);

            // ── Top bar ──────────────────────────────────────────────
            var pnlTop = new Panel
            {
                Dock = DockStyle.Top, Height = 38,
                BackColor = Color.FromArgb(22, 52, 100)
            };
            _lblCrumb = new Label
            {
                Dock = DockStyle.Fill, ForeColor = Color.FromArgb(200, 215, 240),
                Font = new Font("Segoe UI", 9F), TextAlign = ContentAlignment.MiddleLeft,
                Text = "  ROOT  ›  ทั้งหมด"
            };
            var btnRef = MakeTopBtn("รีเฟรช", Color.FromArgb(50, 90, 160));
            btnRef.Click += (s, e) => LoadData();
            var btnCls = MakeTopBtn("ปิด", Color.FromArgb(130, 40, 40));
            btnCls.Click += (s, e) => Close();
            pnlTop.Controls.Add(_lblCrumb);
            pnlTop.Controls.Add(btnRef);
            pnlTop.Controls.Add(btnCls);
            Controls.Add(pnlTop);

            // ── Main 3-column layout (TableLayoutPanel — reliable sizing) ──
            var mainLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 3, RowCount = 1,
                BackColor = Color.FromArgb(190, 205, 230),
                CellBorderStyle = TableLayoutPanelCellBorderStyle.Single,
                Padding = new Padding(0)
            };
            mainLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 100));
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Absolute, 280));   // Left tree
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 48));     // Middle
            mainLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 52));     // Right

            // ── Left: tree panel ────────────────────────────────────
            var pnlLeftWrap = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            var lblTreeHdr  = MakeSectionHeader("  ลำดับชั้นหน่วยงาน");

            _pnlTree = new DoubleBufferedPanel { Dock = DockStyle.Fill, BackColor = Color.White };
            _vScroll = new VScrollBar { Dock = DockStyle.Right, Width = 14 };
            _vScroll.Scroll  += (s, e) => { _scrollOffset = _vScroll.Value; _pnlTree.Invalidate(); };
            _pnlTree.Controls.Add(_vScroll);
            _pnlTree.Paint      += Tree_Paint;
            _pnlTree.MouseMove  += Tree_MouseMove;
            _pnlTree.MouseClick += Tree_MouseClick;
            _pnlTree.MouseLeave += (s, e) => { _hoverIdx = -1; _pnlTree.Invalidate(); };
            _pnlTree.MouseWheel += Tree_MouseWheel;
            _pnlTree.Resize     += (s, e) => UpdateScrollBar();

            pnlLeftWrap.Controls.Add(_pnlTree);
            pnlLeftWrap.Controls.Add(lblTreeHdr);

            // ── Middle: pie + snapshot ───────────────────────────────
            var pnlMid    = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(245, 248, 254) };
            var lblMidHdr = MakeSectionHeader("  Breakdown  &  Snapshot");

            // Vertical split: pie 62% / snapshot 38%
            var midLayout = new TableLayoutPanel
            {
                Dock = DockStyle.Fill, ColumnCount = 1, RowCount = 2,
                BackColor = Color.FromArgb(245, 248, 254)
            };
            midLayout.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            midLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 72));
            midLayout.RowStyles.Add(new RowStyle(SizeType.Percent, 28));

            _piePanel = new DoubleBufferedPanel { Dock = DockStyle.Fill, BackColor = Color.White };
            _piePanel.Paint += Pie_Paint;

            _snapshotPanel = new Panel { Dock = DockStyle.Fill, BackColor = Color.FromArgb(245, 248, 254), Padding = new Padding(4, 2, 4, 4) };

            midLayout.Controls.Add(_piePanel,      0, 0);
            midLayout.Controls.Add(_snapshotPanel, 0, 1);

            pnlMid.Controls.Add(midLayout);
            pnlMid.Controls.Add(lblMidHdr);

            // ── Right: file list ─────────────────────────────────────
            var pnlRight    = new Panel { Dock = DockStyle.Fill, BackColor = Color.White };
            _lblFileHeader  = MakeSectionHeader("  ไฟล์ในหน่วยงาน");

            var pnlTabs = new Panel
            {
                Dock = DockStyle.Top, Height = 32,
                BackColor = Color.FromArgb(235, 240, 252)
            };
            _btnRecent  = MakeTabBtn("10 ล่าสุด",      true);
            _btnLargest = MakeTabBtn("Top 10 ใหญ่สุด",  false);
            _btnToday   = MakeTabBtn("อัปโหลดวันนี้",   false);
            _btnRecent.Click  += (s, e) => { _fileMode = 0; UpdateTabs(); RefreshFileGrid(); };
            _btnLargest.Click += (s, e) => { _fileMode = 1; UpdateTabs(); RefreshFileGrid(); };
            _btnToday.Click   += (s, e) => { _fileMode = 2; UpdateTabs(); RefreshFileGrid(); };
            pnlTabs.Controls.AddRange(new Control[] { _btnRecent, _btnLargest, _btnToday });

            _dgvFiles = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true, AllowUserToAddRows = false, AllowUserToDeleteRows = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                RowHeadersVisible = false, BorderStyle = BorderStyle.None,
                BackgroundColor = Color.White,
                Font = new Font("Segoe UI", 8.5F),
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                EnableHeadersVisualStyles = false,
                GridColor = Color.FromArgb(228, 234, 246),
                ColumnHeadersHeight = 28
            };
            _dgvFiles.ColumnHeadersDefaultCellStyle.BackColor          = Color.FromArgb(42, 78, 132);
            _dgvFiles.ColumnHeadersDefaultCellStyle.ForeColor          = Color.White;
            _dgvFiles.ColumnHeadersDefaultCellStyle.Font               = new Font("Segoe UI", 8.5F, FontStyle.Bold);
            _dgvFiles.ColumnHeadersDefaultCellStyle.SelectionBackColor = Color.FromArgb(42, 78, 132);
            _dgvFiles.DefaultCellStyle.SelectionBackColor              = Color.FromArgb(180, 200, 240);
            _dgvFiles.DefaultCellStyle.SelectionForeColor              = Color.FromArgb(20, 20, 60);
            _dgvFiles.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name="Folder", HeaderText="Folder",       Width=130 },
                new DataGridViewTextBoxColumn { Name="Name",   HeaderText="ชื่อไฟล์",
                    AutoSizeMode=DataGridViewAutoSizeColumnMode.Fill, MinimumWidth=160 },
                new DataGridViewTextBoxColumn { Name="Type",   HeaderText="ประเภท",        Width=62,
                    DefaultCellStyle=new DataGridViewCellStyle { Alignment=DataGridViewContentAlignment.MiddleCenter } },
                new DataGridViewTextBoxColumn { Name="Size",   HeaderText="ขนาด",          Width=90,
                    DefaultCellStyle=new DataGridViewCellStyle { Alignment=DataGridViewContentAlignment.MiddleRight } },
                new DataGridViewTextBoxColumn { Name="Date",   HeaderText="วันที่อัปโหลด", Width=126 }
            });

            pnlRight.Controls.Add(_dgvFiles);
            pnlRight.Controls.Add(pnlTabs);
            pnlRight.Controls.Add(_lblFileHeader);

            // Assemble table
            mainLayout.Controls.Add(pnlLeftWrap, 0, 0);
            mainLayout.Controls.Add(pnlMid,      1, 0);
            mainLayout.Controls.Add(pnlRight,    2, 0);
            Controls.Add(mainLayout);
        }

        // ── UI helpers ────────────────────────────────────────────────
        private static Label MakeSectionHeader(string text)
        {
            return new Label
            {
                Dock = DockStyle.Top, Height = 30,
                Text = text,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold),
                BackColor = Color.FromArgb(30, 65, 120), ForeColor = Color.White,
                TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(4, 0, 0, 0)
            };
        }

        private static Button MakeTopBtn(string text, Color bg)
        {
            var b = new Button
            {
                Text = text, Width = 72, Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat, BackColor = bg,
                ForeColor = Color.White, Font = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor = Cursors.Hand
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private static Button MakeTabBtn(string text, bool active)
        {
            var b = new Button
            {
                Text = text, Width = 110, Height = 28, Dock = DockStyle.Left,
                FlatStyle = FlatStyle.Flat, Cursor = Cursors.Hand,
                Font = new Font("Segoe UI", 8.5F, active ? FontStyle.Bold : FontStyle.Regular),
                BackColor = active ? Color.FromArgb(30, 65, 120) : Color.FromArgb(235, 240, 252),
                ForeColor = active ? Color.White : Color.FromArgb(55, 80, 130)
            };
            b.FlatAppearance.BorderSize = 0;
            return b;
        }

        private void UpdateTabs()
        {
            SetTab(_btnRecent,  _fileMode == 0);
            SetTab(_btnLargest, _fileMode == 1);
            SetTab(_btnToday,   _fileMode == 2);
        }

        private static void SetTab(Button b, bool active)
        {
            b.BackColor = active ? Color.FromArgb(30, 65, 120) : Color.FromArgb(235, 240, 252);
            b.ForeColor = active ? Color.White : Color.FromArgb(55, 80, 130);
            b.Font      = new Font("Segoe UI", 8.5F, active ? FontStyle.Bold : FontStyle.Regular);
        }

        // ---------------------------------------------------------------
        // Helpers
        // ---------------------------------------------------------------
        private static int OrgType(FolderQuotaConfig c)
        {
            if (c == null || string.IsNullOrWhiteSpace(c.OrgUnitTypeName)) return 0;
            string t = c.OrgUnitTypeName;
            if (t.IndexOf("Sub",        StringComparison.OrdinalIgnoreCase) >= 0) return 1;
            if (t.IndexOf("Division",   StringComparison.OrdinalIgnoreCase) >= 0) return 2;
            if (t.IndexOf("Department", StringComparison.OrdinalIgnoreCase) >= 0) return 3;
            return 0;
        }

        private static string SizeStr(long bytes)
        {
            if (bytes <= 0)         return "0 B";
            if (bytes < 1024)       return bytes + " B";
            if (bytes < 1048576)    return string.Format("{0:F1} KB", bytes / 1024.0);
            if (bytes < 1073741824) return string.Format("{0:F2} MB", bytes / 1048576.0);
            return string.Format("{0:F2} GB", bytes / 1073741824.0);
        }

        private static string Trunc(string s, int max)
        {
            if (s == null) return "";
            return s.Length <= max ? s : s.Substring(0, max - 1) + "…";
        }
    }
}
