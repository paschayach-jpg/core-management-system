using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    public class OrgUnitFolderDetailForm : Form
    {
        private readonly OrgUnit _orgUnit;
        private readonly IFolderReadLookupController _folderLookup;
        private readonly IFileBrowseController _fileBrowse;
        private readonly IFolderQuotaService _quotaService;
        private readonly UserContext _user;

        private List<Folder> _folders = new List<Folder>();
        private List<FileDocument> _files = new List<FileDocument>();
        private Label _lblTitle;
        private Label _lblSubtitle;
        private Label _lblSnapshot;
        private Label _lblStatus;
        private TableLayoutPanel _summaryGrid;
        private Panel _piePanel;
        private Panel _fileTypePanel;
        private SplitContainer _contentSplit;
        private ListView _lvFolders;
        private DataGridView _gridFiles;
        private Button _btnViewAll;
        private Button _btnViewTop10;
        private Button _btnViewToday;
        private Label _lblFileViewStatus;
        // 0=Latest10(default), 1=Top10Size, 2=Today, 3=Category
        private int _fileViewMode = 0;
        // -1=none selected, 0=รูปภาพ, 1=เอกสาร, 2=VOD, 3=อื่นๆ
        private int _selectedFileCategory = -1;
        // cached usage for pie panel donuts
        private long _cachedUsedBytes;
        private long _cachedQuotaBytes;
        private readonly Color[] _pieColors = new Color[]
        {
            Color.FromArgb(192, 0, 0),
            Color.FromArgb(210, 105, 30),
            Color.FromArgb(0, 84, 166),
            Color.FromArgb(0, 128, 0),
            Color.FromArgb(92, 92, 92)
        };
        private static readonly Color[] _fileTypeColors = new Color[]
        {
            Color.FromArgb(52, 120, 47),    // รูปภาพ
            Color.FromArgb(26, 58, 108),    // เอกสาร
            Color.FromArgb(180, 80, 0),     // VOD
            Color.FromArgb(100, 100, 100)   // อื่นๆ
        };
        private static readonly string[] _fileTypeLabels = { "รูปภาพ", "เอกสาร", "VOD", "อื่นๆ" };

        public OrgUnitFolderDetailForm(
            OrgUnit orgUnit,
            IFolderReadLookupController folderLookup,
            IFileBrowseController fileBrowse,
            IFolderQuotaService quotaService,
            UserContext user)
        {
            if (orgUnit == null) throw new ArgumentNullException("orgUnit");
            if (folderLookup == null) throw new ArgumentNullException("folderLookup");
            if (user == null) throw new ArgumentNullException("user");

            _orgUnit = orgUnit;
            _folderLookup = folderLookup;
            _fileBrowse = fileBrowse;
            _quotaService = quotaService;
            _user = user;

            InitializeComponent();
            LoadSnapshot();
        }

        private void InitializeComponent()
        {
            Text = "รายละเอียดพื้นที่ Folder";
            Width = 1200;
            Height = 920;
            MinimumSize = new Size(1000, 840);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.White;

            var toolbar = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.FromArgb(248, 250, 253), Padding = new Padding(10, 7, 10, 7) };
            var btnRefresh = CreateButton("รีเฟรช", 10, Color.FromArgb(43, 87, 154));
            btnRefresh.Click += (s, e) => LoadSnapshot();
            var btnExport = CreateButton("Export CSV", 100, Color.FromArgb(52, 120, 47));
            btnExport.Width = 100;
            btnExport.Click += (s, e) => ExportCsv();
            var btnClose = CreateButton("ปิด", 210, Color.FromArgb(105, 105, 105));
            btnClose.Click += (s, e) => Close();
            _lblSnapshot = new Label { Dock = DockStyle.Right, Width = 260, TextAlign = ContentAlignment.MiddleRight, ForeColor = Color.FromArgb(70, 70, 70) };
            toolbar.Controls.Add(btnRefresh);
            toolbar.Controls.Add(btnExport);
            toolbar.Controls.Add(btnClose);
            toolbar.Controls.Add(_lblSnapshot);

            var header = new Panel { Dock = DockStyle.Top, Height = 76, BackColor = Color.FromArgb(26, 58, 108), Padding = new Padding(14, 8, 14, 8) };
            _lblTitle = new Label { Dock = DockStyle.Top, Height = 34, ForeColor = Color.White, Font = new Font("Segoe UI", 15F, FontStyle.Bold), AutoEllipsis = true };
            _lblSubtitle = new Label { Dock = DockStyle.Top, Height = 24, ForeColor = Color.FromArgb(200, 220, 255), AutoEllipsis = true };
            header.Controls.Add(_lblSubtitle);
            header.Controls.Add(_lblTitle);

            _summaryGrid = new TableLayoutPanel { Dock = DockStyle.Top, Height = 112, BackColor = Color.White, Padding = new Padding(12, 12, 12, 10), ColumnCount = 5, RowCount = 1 };
            for (int i = 0; i < 5; i++)
                _summaryGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));

            _piePanel = new Panel
            {
                Dock = DockStyle.Top,
                Height = 240,
                BackColor = Color.White,
                Padding = new Padding(12)
            };
            _piePanel.Paint += PiePanel_Paint;

            _fileTypePanel = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 120,
                BackColor = Color.FromArgb(252, 253, 255),
                Padding   = new Padding(12, 8, 12, 8)
            };
            _fileTypePanel.Paint   += FileTypePanel_Paint;
            _fileTypePanel.Cursor   = Cursors.Hand;
            _fileTypePanel.MouseClick += FileTypePanel_MouseClick;

            _contentSplit = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterWidth = 6,
                Panel1MinSize = 50,
                Panel2MinSize = 50,
                BackColor = Color.FromArgb(225, 230, 238)
            };

            _lvFolders = new ListView { Dock = DockStyle.Fill, View = View.Details, FullRowSelect = true, GridLines = true, HideSelection = false, BorderStyle = BorderStyle.None };
            _lvFolders.Columns.Add("Folder", 210);
            _lvFolders.Columns.Add("Level", 62);
            _lvFolders.Columns.Add("Files", 58, HorizontalAlignment.Right);
            _lvFolders.Columns.Add("Size", 95, HorizontalAlignment.Right);
            var leftHeader = HeaderLabel("  Folder ในหน่วยงาน");
            _contentSplit.Panel1.Controls.Add(_lvFolders);
            _contentSplit.Panel1.Controls.Add(leftHeader);

            _gridFiles = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                RowTemplate = { Height = 24 }
            };
            _gridFiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Folder", HeaderText = "Folder", Width = 130, MinimumWidth = 100 });
            _gridFiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "ชื่อไฟล์", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 140 });
            _gridFiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "ประเภท", Width = 58, MinimumWidth = 50 });
            _gridFiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Size", HeaderText = "ขนาด", Width = 82, MinimumWidth = 72, DefaultCellStyle = RightStyle() });
            _gridFiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Date", HeaderText = "วันที่อัปโหลด", Width = 120, MinimumWidth = 110, DefaultCellStyle = { Alignment = DataGridViewContentAlignment.MiddleCenter } });
            _gridFiles.EnableHeadersVisualStyles = false;
            _gridFiles.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(26, 58, 108);
            _gridFiles.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _gridFiles.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _gridFiles.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 250, 254);
            _gridFiles.GridColor = Color.FromArgb(222, 228, 236);

            // Filter toolbar above file grid
            var pnlFileToolbar = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 34,
                BackColor = Color.FromArgb(240, 244, 250),
                Padding   = new Padding(6, 4, 6, 4)
            };

            _btnViewAll = MakeFilterButton("10 ล่าสุด", true);
            _btnViewAll.Click += (s, e) =>
            {
                _fileViewMode = 0; _selectedFileCategory = -1;
                UpdateFileViewButtons(); _fileTypePanel.Invalidate(); RenderFiles();
            };

            _btnViewTop10 = MakeFilterButton("Top 10 ใหญ่สุด", false);
            _btnViewTop10.Click += (s, e) =>
            {
                _fileViewMode = 1; _selectedFileCategory = -1;
                UpdateFileViewButtons(); _fileTypePanel.Invalidate(); RenderFiles();
            };

            _btnViewToday = MakeFilterButton("อัปโหลดวันนี้", false);
            _btnViewToday.Click += (s, e) =>
            {
                _fileViewMode = 2; _selectedFileCategory = -1;
                UpdateFileViewButtons(); _fileTypePanel.Invalidate(); RenderFiles();
            };

            _lblFileViewStatus = new Label
            {
                Dock      = DockStyle.Right,
                Width     = 160,
                TextAlign = ContentAlignment.MiddleRight,
                Font      = new Font("Segoe UI", 8F),
                ForeColor = Color.FromArgb(80, 80, 80)
            };

            pnlFileToolbar.Controls.Add(_lblFileViewStatus);
            pnlFileToolbar.Controls.Add(_btnViewToday);
            pnlFileToolbar.Controls.Add(_btnViewTop10);
            pnlFileToolbar.Controls.Add(_btnViewAll);

            _contentSplit.Panel2.Controls.Add(_gridFiles);
            _contentSplit.Panel2.Controls.Add(pnlFileToolbar);
            _contentSplit.Panel2.Controls.Add(HeaderLabel("  ไฟล์ในหน่วยงาน"));

            _lblStatus = new Label { Dock = DockStyle.Bottom, Height = 28, Padding = new Padding(10, 0, 10, 0), TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.FromArgb(240, 244, 250), ForeColor = Color.FromArgb(43, 87, 154) };

            Controls.Add(_contentSplit);
            Controls.Add(_lblStatus);
            Controls.Add(_fileTypePanel);
            Controls.Add(_piePanel);
            Controls.Add(_summaryGrid);
            Controls.Add(header);
            Controls.Add(toolbar);

            Shown += (s, e) => BeginInvoke(new Action(AdjustContentSplit));
            Resize += (s, e) => AdjustContentSplit();
        }

        private void LoadSnapshot()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                _folders = _folderLookup.GetFoldersByOrgUnit(_orgUnit.OrgUnitID, _user)
                    .Where(f => !f.IsDeleted)
                    .ToList();
                _files = _fileBrowse != null
                    ? _fileBrowse.GetFilesByOrgUnit(_orgUnit.OrgUnitID, _user).Where(f => !f.IsDeleted).ToList()
                    : new List<FileDocument>();
                RenderSnapshot();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถโหลดรายละเอียดพื้นที่ Folder ได้: " + ex.Message,
                    "รายละเอียดพื้นที่ Folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void RenderSnapshot()
        {
            long usedBytes = _files.Sum(f => f.FileSizeBytes);
            long folderBytes = _folders.Sum(f => Math.Max(0, f.FolderSizeBytes));
            if (folderBytes > usedBytes)
                usedBytes = folderBytes;

            long quotaBytes = ResolveQuotaBytes();
            long freeBytes = quotaBytes > 0 ? Math.Max(0, quotaBytes - usedBytes) : 0;
            double usedPct = quotaBytes > 0 ? Math.Min(usedBytes * 100.0 / quotaBytes, 999.0) : 0;

            _cachedUsedBytes  = usedBytes;
            _cachedQuotaBytes = quotaBytes;

            Text = string.Format("รายละเอียดพื้นที่ Folder - {0}", _orgUnit.Name);
            _lblTitle.Text = _orgUnit.Name;
            _lblSubtitle.Text = string.Format("OrgUnitID: {0} | {1}", _orgUnit.OrgUnitID, TierText(_orgUnit.OrgUnitType));
            _lblSnapshot.Text = "Snapshot: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            int todayCount = _files.Count(f => f.UploadedDate.Date == DateTime.Today);
            long todayBytes = _files.Where(f => f.UploadedDate.Date == DateTime.Today).Sum(f => f.FileSizeBytes);

            _summaryGrid.Controls.Clear();
            _summaryGrid.ColumnCount = 5;
            _summaryGrid.ColumnStyles.Clear();
            for (int i = 0; i < 5; i++)
                _summaryGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));

            // Quota ก่อน ตามด้วย ใช้พื้นที่รวม
            AddSummaryCard("Quota", quotaBytes > 0 ? FormatFileSize(quotaBytes) : "ไม่กำหนด", "จาก org config", Color.FromArgb(26, 58, 108));
            AddSummaryCard("ใช้พื้นที่รวม", FormatFileSize(usedBytes), quotaBytes > 0 ? string.Format("{0:0.0}% ของ quota", usedPct) : "ยังไม่ได้กำหนด quota", UsageColor(usedPct, quotaBytes));
            AddSummaryCard("คงเหลือ", quotaBytes > 0 ? FormatFileSize(freeBytes) : "-", quotaBytes > 0 ? "พื้นที่ที่ยังใช้ได้" : "ไม่จำกัด", Color.FromArgb(52, 120, 47));
            AddSummaryCard("Folders", _folders.Count.ToString("N0"), "รวมทุกระดับในหน่วยงาน", Color.FromArgb(92, 92, 92));
            AddSummaryCard("Files", _files.Count.ToString("N0"),
                todayCount > 0
                    ? string.Format("วันนี้ {0} ไฟล์ ({1})", todayCount, FormatFileSize(todayBytes))
                    : "ไฟล์ทั้งหมดในหน่วยงาน",
                todayCount > 0 ? Color.FromArgb(180, 80, 0) : Color.FromArgb(92, 92, 92));

            RenderFolders();
            RenderFiles();
            _piePanel.Invalidate();
            _fileTypePanel.Invalidate();
            _lblStatus.Text = string.Format("Selected: {0} | folders {1:N0} | files {2:N0} | used {3}",
                _orgUnit.Name, _folders.Count, _files.Count, FormatFileSize(usedBytes));
        }

        private void PiePanel_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.Clear(Color.White);

            using (var titleFont = new Font("Segoe UI", 10F, FontStyle.Bold))
            using (var textBrush = new SolidBrush(Color.FromArgb(26, 58, 108)))
            {
                e.Graphics.DrawString("สัดส่วนพื้นที่ตามระดับความลับ", titleFont, textBrush, 12, 8);
            }

            var rows = BuildPieRows();
            long total = rows.Sum(r => r.Bytes);
            int size = Math.Min(150, Math.Min(_piePanel.ClientSize.Width - 260, _piePanel.ClientSize.Height - 50));
            if (size < 80) return;

            var pieRect = new Rectangle(30, 48, size, size);
            if (total <= 0)
            {
                using (var pen = new Pen(Color.FromArgb(190, 198, 210), 2f))
                    e.Graphics.DrawEllipse(pen, pieRect);
                DrawCenteredText(e.Graphics, "ไม่มีข้อมูลพื้นที่", pieRect, Color.FromArgb(110, 110, 110));
            }
            else
            {
                float start = -90f;
                for (int i = 0; i < rows.Count; i++)
                {
                    float sweep = (float)(rows[i].Bytes * 360.0 / total);
                    using (var brush = new SolidBrush(rows[i].Color))
                        e.Graphics.FillPie(brush, pieRect, start, sweep);
                    start += sweep;
                }

                using (var pen = new Pen(Color.White, 2f))
                    e.Graphics.DrawEllipse(pen, pieRect);
            }

            DrawPieLegend(e.Graphics, rows, total, pieRect.Right + 34, 48);

            // 3 donut charts — วางในพื้นที่ขวาของ panel
            int panelW    = _piePanel.ClientSize.Width;
            int panelH    = _piePanel.ClientSize.Height;
            int donutSize = Math.Min(170, (panelH - 10));
            int donutY    = Math.Max(4, (panelH - donutSize) / 2 - 8);
            int donutGap  = 12;
            int donutX3   = panelW - donutSize - 14;
            int donutX2   = donutX3 - donutSize - donutGap;
            int donutX0   = donutX2 - donutSize - donutGap;

            if (donutX0 > pieRect.Right + 34 + 160)
            {
                // Donut ①: สัดส่วนประเภทไฟล์ (multi-color, from the red box data)
                DrawFileTypeDonut(e.Graphics, new Rectangle(donutX0, donutY, donutSize, donutSize));

                double usedPct  = _cachedQuotaBytes > 0
                    ? Math.Min(_cachedUsedBytes * 100.0 / _cachedQuotaBytes, 100.0) : 0;
                double freePct  = _cachedQuotaBytes > 0
                    ? Math.Min((_cachedQuotaBytes - _cachedUsedBytes) * 100.0 / _cachedQuotaBytes, 100.0) : 0;
                long   freeBytes = _cachedQuotaBytes > 0
                    ? Math.Max(_cachedQuotaBytes - _cachedUsedBytes, 0L) : 0L;

                // Donut ②: พื้นที่ใช้งาน
                DrawSmallDonut(e.Graphics,
                    new Rectangle(donutX2, donutY, donutSize, donutSize),
                    "พื้นที่ใช้งาน", usedPct,
                    GetUsageColor(usedPct), Color.FromArgb(220, 225, 235),
                    FormatFileSize(_cachedUsedBytes),
                    _cachedQuotaBytes > 0, true);

                // Donut ③: พื้นที่ว่างคงเหลือ
                DrawSmallDonut(e.Graphics,
                    new Rectangle(donutX3, donutY, donutSize, donutSize),
                    "พื้นที่ว่างคงเหลือ", freePct,
                    Color.FromArgb(100, 160, 220), Color.FromArgb(220, 225, 235),
                    FormatFileSize(freeBytes),
                    _cachedQuotaBytes > 0, false);
            }
        }

        private void DrawFileTypeDonut(Graphics g, Rectangle bounds)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int titleH  = 20;
            int legendRows = 2;      // 4 items × 2 columns = 2 rows
            int legendH = legendRows * 14 + 4;
            int size    = Math.Min(bounds.Width - 4, bounds.Height - titleH - legendH - 6);
            if (size < 40) return;

            int cx = bounds.Left + bounds.Width / 2;
            int cy = bounds.Top + titleH + size / 2 + 2;

            // Title
            using (var tf = new Font("Segoe UI", 9.5F, FontStyle.Bold))
            using (var tb = new SolidBrush(Color.FromArgb(26, 58, 108)))
            {
                var ts = g.MeasureString("สัดส่วนประเภทไฟล์", tf);
                g.DrawString("สัดส่วนประเภทไฟล์", tf, tb, cx - ts.Width / 2, bounds.Top + 2);
            }

            var rect = new Rectangle(cx - size / 2, cy - size / 2, size, size);
            var cats = BuildFileTypeRows();
            long totalBytes = cats.Sum(c => c.Bytes);

            if (totalBytes == 0)
            {
                using (var pen = new Pen(Color.FromArgb(200, 205, 215), 2f))
                    g.DrawEllipse(pen, rect);
                using (var gf = new Font("Segoe UI", 8F, FontStyle.Italic))
                using (var gb = new SolidBrush(Color.FromArgb(150, 150, 150)))
                {
                    var gs = g.MeasureString("ไม่มีข้อมูล", gf);
                    g.DrawString("ไม่มีข้อมูล", gf, gb, cx - gs.Width / 2, cy - gs.Height / 2);
                }
            }
            else
            {
                float start = -90f;
                for (int i = 0; i < cats.Count; i++)
                {
                    if (cats[i].Bytes == 0) continue;
                    float sweep = (float)(cats[i].Bytes * 360.0 / totalBytes);
                    using (var b = new SolidBrush(_fileTypeColors[i]))
                        g.FillPie(b, rect, start, sweep);
                    using (var p = new Pen(Color.White, 1.5f))
                        g.DrawPie(p, rect, start, sweep);

                    // % label on slice
                    if (sweep > 20)
                    {
                        double mid = (start + sweep / 2.0) * Math.PI / 180.0;
                        int lx = cx + (int)(size * 0.33 * Math.Cos(mid));
                        int ly = cy + (int)(size * 0.33 * Math.Sin(mid));
                        string pct = string.Format("{0:0}%", cats[i].Bytes * 100.0 / totalBytes);
                        using (var pf = new Font("Segoe UI", 7.5F, FontStyle.Bold))
                        {
                            var ps = g.MeasureString(pct, pf);
                            g.DrawString(pct, pf, Brushes.White, lx - ps.Width / 2, ly - ps.Height / 2);
                        }
                    }
                    start += sweep;
                }

                // Donut hole
                int hole = (int)(size * 0.50);
                using (var wb = new SolidBrush(Color.White))
                    g.FillEllipse(wb, cx - hole / 2, cy - hole / 2, hole, hole);

                // Centre: total file count
                string centre = string.Format("{0}\nไฟล์", _files.Count);
                using (var cf = new Font("Segoe UI", 8F, FontStyle.Bold))
                using (var cb = new SolidBrush(Color.FromArgb(40, 40, 40)))
                {
                    var cs = g.MeasureString(_files.Count.ToString(), cf);
                    g.DrawString(_files.Count.ToString(), cf, cb, cx - cs.Width / 2, cy - cs.Height - 1);
                    using (var sf = new Font("Segoe UI", 7F))
                    using (var sb2 = new SolidBrush(Color.FromArgb(100, 100, 100)))
                    {
                        var ss = g.MeasureString("ไฟล์", sf);
                        g.DrawString("ไฟล์", sf, sb2, cx - ss.Width / 2, cy + 1);
                    }
                }
            }

            // Mini legend (2 columns × 2 rows)
            int legY = rect.Bottom + 5;
            int colW = bounds.Width / 2;
            using (var lf = new Font("Segoe UI", 7.5F))
            using (var lb = new SolidBrush(Color.FromArgb(40, 40, 40)))
            {
                for (int i = 0; i < cats.Count; i++)
                {
                    int col = i % 2;
                    int row = i / 2;
                    int lx = bounds.Left + col * colW + 4;
                    int ly = legY + row * 14;
                    using (var dotB = new SolidBrush(_fileTypeColors[i]))
                        g.FillEllipse(dotB, lx, ly + 3, 8, 8);
                    double pct = totalBytes > 0 ? cats[i].Bytes * 100.0 / totalBytes : 0;
                    string label = string.Format("{0} {1:0.0}%", _fileTypeLabels[i], pct);
                    g.DrawString(label, lf, lb, lx + 12, ly);
                }
            }
        }

        private void DrawSmallDonut(Graphics g, Rectangle bounds,
            string title, double primaryPct,
            Color primaryColor, Color bgColor,
            string sizeLabel, bool hasQuota, bool showThreshold)
        {
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;

            int titleH = 20;
            int footH  = hasQuota ? 18 : 34;
            int size   = Math.Min(bounds.Width, bounds.Height - titleH - footH - 4);
            if (size < 40) return;

            int cx = bounds.Left + bounds.Width / 2;
            int cy = bounds.Top + titleH + size / 2 + 2;

            // Title
            using (var tf = new Font("Segoe UI", 9.5F, FontStyle.Bold))
            using (var tb = new SolidBrush(Color.FromArgb(26, 58, 108)))
            {
                var ts = g.MeasureString(title, tf);
                g.DrawString(title, tf, tb, cx - ts.Width / 2, bounds.Top + 2);
            }

            var rect = new Rectangle(cx - size / 2, cy - size / 2, size, size);

            // Background arc
            using (var bgBrush = new SolidBrush(bgColor))
                g.FillPie(bgBrush, rect, -90f, 360f);

            // Primary arc
            if (hasQuota && primaryPct > 0)
            {
                float sweep = (float)(primaryPct / 100.0 * 360.0);
                using (var pb = new SolidBrush(primaryColor))
                    g.FillPie(pb, rect, -90f, sweep);

                if (showThreshold)
                {
                    DrawThresholdMarker(g, rect, cx, cy, 80f, Color.FromArgb(230, 150, 0), "80%");
                    DrawThresholdMarker(g, rect, cx, cy, 95f, Color.FromArgb(200, 50, 50),  "95%");
                }
            }

            // Donut hole
            int hole = (int)(size * 0.58);
            using (var wb = new SolidBrush(Color.White))
                g.FillEllipse(wb, cx - hole / 2, cy - hole / 2, hole, hole);

            // Centre text — % or N/A
            string pctText = hasQuota ? string.Format("{0:F1}%", primaryPct) : "N/A";
            float  cFontSz = size < 100 ? 10F : (size < 130 ? 12F : 14F);
            using (var cf = new Font("Segoe UI", cFontSz, FontStyle.Bold))
            using (var cb = new SolidBrush(hasQuota ? primaryColor : Color.FromArgb(170, 170, 170)))
            {
                var cs = g.MeasureString(pctText, cf);
                g.DrawString(pctText, cf, cb, cx - cs.Width / 2, cy - cs.Height / 2);
            }

            // Size label below donut
            int infoY = rect.Bottom + 6;
            using (var sf = new Font("Segoe UI", 9F, FontStyle.Bold))
            using (var sb2 = new SolidBrush(primaryColor))
            {
                var ss = g.MeasureString(sizeLabel, sf);
                g.DrawString(sizeLabel, sf, sb2, cx - ss.Width / 2, infoY);
            }
            if (!hasQuota)
            {
                using (var gf = new Font("Segoe UI", 8F, FontStyle.Italic))
                using (var gb2 = new SolidBrush(Color.FromArgb(130, 130, 130)))
                {
                    var gs = g.MeasureString("ไม่ได้กำหนด quota", gf);
                    g.DrawString("ไม่ได้กำหนด quota", gf, gb2, cx - gs.Width / 2, infoY + 16);
                }
            }
        }

        private void FileTypePanel_MouseClick(object sender, MouseEventArgs e)
        {
            int colW = Math.Max(160, (_fileTypePanel.ClientSize.Width - 24) / 4);
            int clickedCat = (e.X - 12) / colW;
            if (clickedCat < 0 || clickedCat > 3) return;

            if (_selectedFileCategory == clickedCat)
            {
                // Deselect — revert to default (latest 10)
                _selectedFileCategory = -1;
                _fileViewMode = 0;
            }
            else
            {
                _selectedFileCategory = clickedCat;
                _fileViewMode = 3;
            }

            UpdateFileViewButtons();
            _fileTypePanel.Invalidate();
            RenderFiles();
        }

        private void FileTypePanel_Paint(object sender, PaintEventArgs e)
        {
            var g = e.Graphics;
            g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            g.Clear(_fileTypePanel.BackColor);

            var rows = BuildFileTypeRows();
            long totalBytes = rows.Sum(r => r.Bytes);

            using (var titleFont = new Font("Segoe UI", 10F, FontStyle.Bold))
            using (var titleBrush = new SolidBrush(Color.FromArgb(26, 58, 108)))
                g.DrawString("ประเภทไฟล์ในหน่วยงาน  (คลิกเพื่อดู Top 10)", titleFont, titleBrush, 0, 2);

            int colW    = Math.Max(160, (_fileTypePanel.ClientSize.Width - 24) / 4);
            int barMaxW = colW - 120;

            using (var labelFont = new Font("Segoe UI", 9F, FontStyle.Bold))
            using (var valueFont = new Font("Segoe UI", 9F))
            using (var grayBrush = new SolidBrush(Color.FromArgb(200, 200, 200)))
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    int x = 12 + i * colW;
                    int y = 28;
                    bool selected = _selectedFileCategory == i;

                    // Highlight selected card
                    if (selected)
                    {
                        using (var hlBrush = new SolidBrush(Color.FromArgb(20, _fileTypeColors[i])))
                            g.FillRectangle(hlBrush, x - 6, y - 4, colW - 4, 96);
                        using (var borderPen = new Pen(_fileTypeColors[i], 2f))
                            g.DrawRectangle(borderPen, x - 6, y - 4, colW - 4, 96);
                    }

                    using (var dotBrush = new SolidBrush(_fileTypeColors[i]))
                        g.FillEllipse(dotBrush, x, y + 3, 12, 12);

                    using (var textBrush = new SolidBrush(selected ? _fileTypeColors[i] : Color.FromArgb(30, 30, 30)))
                    {
                        g.DrawString(row.Label, labelFont, textBrush, x + 18, y);
                        g.DrawString(string.Format("{0} ไฟล์", row.Count), valueFont, textBrush, x + 18, y + 18);
                        g.DrawString(FormatFileSize(row.Bytes), valueFont, textBrush, x + 18, y + 36);
                    }

                    int barY = y + 60;
                    g.FillRectangle(grayBrush, x, barY, barMaxW, 12);
                    if (totalBytes > 0 && row.Bytes > 0)
                    {
                        int filled = (int)(row.Bytes * barMaxW / totalBytes);
                        using (var barBrush = new SolidBrush(_fileTypeColors[i]))
                            g.FillRectangle(barBrush, x, barY, Math.Max(2, filled), 12);
                    }

                    double pct = totalBytes > 0 ? row.Bytes * 100.0 / totalBytes : 0;
                    using (var pctBrush = new SolidBrush(Color.FromArgb(80, 80, 80)))
                    using (var pctFont  = new Font("Segoe UI", 8F))
                        g.DrawString(string.Format("{0:0.0}%", pct), pctFont, pctBrush, x, barY + 16);
                }
            }
        }

        private List<FileTypeRow> BuildFileTypeRows()
        {
            var counts = new long[4];
            var bytes  = new long[4];

            foreach (var f in _files)
            {
                int cat = ClassifyFile(f.FileExtension);
                counts[cat]++;
                bytes[cat] += Math.Max(0, f.FileSizeBytes);
            }

            var result = new List<FileTypeRow>();
            for (int i = 0; i < 4; i++)
                result.Add(new FileTypeRow { Label = _fileTypeLabels[i], Count = (int)counts[i], Bytes = bytes[i] });
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
                    return 0; // รูปภาพ
                case "pdf": case "doc": case "docx": case "xls":
                case "xlsx": case "ppt": case "pptx": case "txt":
                case "csv": case "rtf": case "odt": case "ods": case "odp":
                    return 1; // เอกสาร
                case "mp4": case "avi": case "mov": case "mkv":
                case "wmv": case "flv": case "m4v": case "mpg":
                case "mpeg": case "webm": case "ts": case "m2ts":
                    return 2; // VOD
                default:
                    return 3; // อื่นๆ
            }
        }

        private List<PieRow> BuildPieRows()
        {
            var labels = new Dictionary<int, string>
            {
                { 1, "1HC - High Confidential" },
                { 2, "2SC - Semi Confidential" },
                { 3, "3NC - Normal Confidential" },
                { 4, "4PC - Public Confidential" }
            };

            var rows = new List<PieRow>();
            for (int level = 1; level <= 4; level++)
            {
                long bytes = _folders
                    .Where(f => f.ConfidentialityLevel == level)
                    .Sum(f => Math.Max(0, f.FolderSizeBytes));
                rows.Add(new PieRow
                {
                    Label = labels[level],
                    Bytes = bytes,
                    Color = _pieColors[level - 1]
                });
            }

            long known = rows.Sum(r => r.Bytes);
            long fileTotal = _files.Sum(f => Math.Max(0, f.FileSizeBytes));
            if (fileTotal > known)
            {
                rows.Add(new PieRow
                {
                    Label = "อื่นๆ / ไม่ทราบระดับ",
                    Bytes = fileTotal - known,
                    Color = _pieColors[4]
                });
            }

            return rows;
        }

        private static void DrawCenteredText(Graphics g, string text, Rectangle bounds, Color color)
        {
            using (var font = new Font("Segoe UI", 9F, FontStyle.Regular))
            using (var brush = new SolidBrush(color))
            {
                var size = g.MeasureString(text, font);
                g.DrawString(text, font, brush,
                    bounds.Left + (bounds.Width - size.Width) / 2,
                    bounds.Top + (bounds.Height - size.Height) / 2);
            }
        }

        private void DrawPieLegend(Graphics g, List<PieRow> rows, long total, int x, int y)
        {
            using (var font = new Font("Segoe UI", 9F))
            using (var boldFont = new Font("Segoe UI", 9F, FontStyle.Bold))
            using (var textBrush = new SolidBrush(Color.FromArgb(40, 40, 40)))
            {
                for (int i = 0; i < rows.Count; i++)
                {
                    var row = rows[i];
                    int top = y + (i * 30);
                    using (var colorBrush = new SolidBrush(row.Color))
                        g.FillRectangle(colorBrush, x, top + 3, 14, 14);
                    g.DrawRectangle(Pens.LightGray, x, top + 3, 14, 14);

                    double pct = total > 0 ? row.Bytes * 100.0 / total : 0;
                    string line = string.Format("{0}   {1} ({2:0.0}%)", row.Label, FormatFileSize(row.Bytes), pct);
                    g.DrawString(line, row.Bytes > 0 ? boldFont : font, textBrush, x + 22, top);
                }
            }
        }

        private long ResolveQuotaBytes()
        {
            if (_quotaService == null) return 0;
            try
            {
                var config = _quotaService.GetConfig(_orgUnit.OrgUnitID, _orgUnit.ParentOrgUnitID);
                return config != null ? config.QuotaBytes : 0;
            }
            catch
            {
                return 0;
            }
        }

        private void RenderFolders()
        {
            _lvFolders.BeginUpdate();
            try
            {
                _lvFolders.Items.Clear();
                foreach (var folder in _folders.OrderBy(f => f.FolderDepth).ThenBy(f => f.FolderName))
                {
                    var item = new ListViewItem(new string[]
                    {
                        new string(' ', Math.Max(0, folder.FolderDepth) * 2) + folder.FolderName,
                        ConfidentialityBadge(folder.ConfidentialityLevel),
                        folder.FileCount.ToString("N0"),
                        FormatFileSize(folder.FolderSizeBytes)
                    });
                    item.Tag = folder;
                    item.ForeColor = ConfidentialityColor(folder.ConfidentialityLevel);
                    if (folder.FileCount > 0)
                        item.Font = new Font(_lvFolders.Font, FontStyle.Bold);
                    _lvFolders.Items.Add(item);
                }
            }
            finally
            {
                _lvFolders.EndUpdate();
            }
        }

        private void RenderFiles()
        {
            _gridFiles.Rows.Clear();
            var folderLevelLookup = _folders
                .GroupBy(f => f.FolderID)
                .ToDictionary(g => g.Key, g => g.First().ConfidentialityLevel);

            IEnumerable<FileDocument> source;
            string modeLabel;
            switch (_fileViewMode)
            {
                case 1: // Top 10 largest
                    source    = _files.OrderByDescending(f => f.FileSizeBytes).Take(10);
                    modeLabel = string.Format("Top 10 ใหญ่สุด / {0} ไฟล์", _files.Count);
                    break;
                case 2: // Today
                    source    = _files.Where(f => f.UploadedDate.Date == DateTime.Today)
                                      .OrderByDescending(f => f.UploadedDate);
                    modeLabel = string.Format("วันนี้ {0} ไฟล์ / ทั้งหมด {1}", source.Count(), _files.Count);
                    break;
                case 3: // Category top 10
                    var catFiles = _selectedFileCategory >= 0
                        ? _files.Where(f => ClassifyFile(f.FileExtension) == _selectedFileCategory)
                        : _files;
                    source    = catFiles.OrderByDescending(f => f.FileSizeBytes).Take(10);
                    string catName = _selectedFileCategory >= 0
                        ? _fileTypeLabels[_selectedFileCategory] : "ทั้งหมด";
                    modeLabel = string.Format("{0}: Top 10 ใหญ่สุด / {1} ไฟล์",
                        catName, catFiles.Count());
                    break;
                default: // Latest 10 uploads
                    source    = _files.OrderByDescending(f => f.UploadedDate).Take(10);
                    modeLabel = string.Format("10 ล่าสุด / ทั้งหมด {0} ไฟล์", _files.Count);
                    break;
            }
            if (_lblFileViewStatus != null)
                _lblFileViewStatus.Text = modeLabel;

            bool anyToday = _files.Any(f => f.UploadedDate.Date == DateTime.Today);

            foreach (var file in source)
            {
                bool isToday = file.UploadedDate.Date == DateTime.Today;

                int rowIndex = _gridFiles.Rows.Add(
                    file.FolderName ?? "",
                    file.OriginalFileName,
                    (file.FileExtension ?? "").TrimStart('.').ToUpperInvariant(),
                    FormatFileSize(file.FileSizeBytes),
                    file.UploadedDate.ToString("dd/MM/yyyy HH:mm"));

                var row = _gridFiles.Rows[rowIndex];
                int level;
                if (!folderLevelLookup.TryGetValue(file.FolderID, out level))
                    level = file.ConfidentialityLevel;

                // Highlight today's uploads
                if (isToday)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 225);
                    row.Cells["Date"].Style.ForeColor = Color.FromArgb(160, 70, 0);
                    row.Cells["Date"].Style.Font = new Font(_gridFiles.Font, FontStyle.Bold);
                }

                row.DefaultCellStyle.ForeColor = ConfidentialityColor(level);
                row.DefaultCellStyle.SelectionBackColor = Color.FromArgb(43, 87, 154);
                row.DefaultCellStyle.SelectionForeColor = Color.White;
                row.Cells["Name"].ToolTipText   = file.OriginalFileName;
                row.Cells["Folder"].ToolTipText = string.Format("{0} | uploaded {1:dd/MM/yyyy HH:mm}",
                    file.FolderName ?? "", file.UploadedDate);
            }
        }

        private void ExportCsv()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = "Export Folder Detail";
                sfd.FileName = string.Format("org-folder-detail-{0}-{1:yyyyMMddHHmm}.csv", _orgUnit.OrgUnitID, DateTime.Now);
                sfd.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                var sb = new StringBuilder();

                // File type summary
                sb.AppendLine("=== สรุปประเภทไฟล์ ===");
                sb.AppendLine("ประเภท,จำนวนไฟล์,ขนาดรวม (bytes),ขนาดรวม");
                var ftRows = BuildFileTypeRows();
                foreach (var ft in ftRows)
                    sb.AppendLine(string.Format("{0},{1},{2},{3}",
                        ft.Label, ft.Count, ft.Bytes, FormatFileSize(ft.Bytes)));
                sb.AppendLine();

                // Detail
                sb.AppendLine("=== รายละเอียด ===");
                sb.AppendLine("Section,Folder,Level,FileName,ประเภทไฟล์,Type,SizeBytes,Size,Date");
                foreach (var folder in _folders.OrderBy(f => f.FolderName))
                    sb.AppendLine(string.Format("Folder,{0},{1},,,,{2},{3},",
                        Csv(folder.FolderName), ConfidentialityBadge(folder.ConfidentialityLevel), folder.FolderSizeBytes, Csv(FormatFileSize(folder.FolderSizeBytes))));
                foreach (var file in _files.OrderBy(f => f.OriginalFileName))
                {
                    string typeCat = _fileTypeLabels[ClassifyFile(file.FileExtension)];
                    sb.AppendLine(string.Format("File,{0},,{1},{2},{3},{4},{5},{6}",
                        Csv(file.FolderName ?? ""), Csv(file.OriginalFileName),
                        Csv(typeCat),
                        Csv((file.FileExtension ?? "").TrimStart('.').ToUpperInvariant()),
                        file.FileSizeBytes, Csv(FormatFileSize(file.FileSizeBytes)),
                        Csv(file.UploadedDate.ToString("yyyy-MM-dd HH:mm:ss"))));
                }

                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Export เรียบร้อยแล้ว", "รายละเอียดพื้นที่ Folder", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static Label HeaderLabel(string text)
        {
            return new Label
            {
                Dock = DockStyle.Top,
                Height = 32,
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 58, 108),
                BackColor = Color.FromArgb(245, 248, 252)
            };
        }

        private void AddSummaryCard(string title, string value, string caption, Color accent)
        {
            var panel = new Panel { Dock = DockStyle.Fill, Margin = new Padding(5, 2, 5, 2), BackColor = Color.FromArgb(248, 250, 253), Padding = new Padding(12, 12, 10, 8) };
            panel.Paint += (s, e) =>
            {
                using (var accentBrush = new SolidBrush(accent))
                    e.Graphics.FillRectangle(accentBrush, 0, 0, 5, panel.Height);
                using (var pen = new Pen(Color.FromArgb(214, 222, 234)))
                    e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };
            panel.Controls.Add(new Label { Dock = DockStyle.Fill, Text = caption, ForeColor = Color.FromArgb(98, 108, 124), AutoEllipsis = true, Padding = new Padding(0, 3, 0, 0) });
            panel.Controls.Add(new Label { Dock = DockStyle.Top, Height = 38, Text = value, Font = new Font("Segoe UI", 16F, FontStyle.Bold), ForeColor = accent, AutoEllipsis = true, TextAlign = ContentAlignment.MiddleLeft });
            panel.Controls.Add(new Label { Dock = DockStyle.Top, Height = 22, Text = title, ForeColor = Color.FromArgb(70, 78, 92), Font = new Font("Segoe UI", 8.8F, FontStyle.Regular) });
            _summaryGrid.Controls.Add(panel);
        }

        private void AdjustContentSplit()
        {
            if (_contentSplit == null || !_contentSplit.IsHandleCreated)
                return;
            if (_contentSplit.Width < 540)
                return;

            // Apply real min sizes now that the form is fully laid out.
            _contentSplit.Panel1MinSize = 260;
            _contentSplit.Panel2MinSize = 260;

            int desiredRight = Math.Max(360, Math.Min(560, (int)(_contentSplit.Width * 0.42)));
            int desiredDistance = _contentSplit.Width - desiredRight - _contentSplit.SplitterWidth;
            int minDistance = _contentSplit.Panel1MinSize;
            int maxDistance = _contentSplit.Width - _contentSplit.Panel2MinSize - _contentSplit.SplitterWidth;
            if (maxDistance < minDistance)
                return;

            desiredDistance = Math.Max(minDistance, Math.Min(maxDistance, desiredDistance));
            try
            {
                if (_contentSplit.SplitterDistance != desiredDistance)
                    _contentSplit.SplitterDistance = desiredDistance;
            }
            catch (Exception)
            {
                // SplitContainer rejects distances during early layout — next Resize will retry.
            }
        }

        private static Button CreateButton(string text, int left, Color backColor)
        {
            var btn = new Button { Text = text, Left = left, Top = 7, Width = 82, Height = 29, BackColor = backColor, ForeColor = Color.White, FlatStyle = FlatStyle.Flat, Font = new Font("Segoe UI", 9F, FontStyle.Bold) };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }

        private static Button MakeFilterButton(string text, bool active)
        {
            var btn = new Button
            {
                Text      = text,
                Dock      = DockStyle.Left,
                Width     = 110,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 8.5F, FontStyle.Bold),
                Cursor    = Cursors.Hand
            };
            SetFilterButtonState(btn, active);
            return btn;
        }

        private static void SetFilterButtonState(Button btn, bool active)
        {
            btn.BackColor = active ? Color.FromArgb(26, 58, 108) : Color.FromArgb(200, 210, 225);
            btn.ForeColor = active ? Color.White : Color.FromArgb(40, 40, 40);
            btn.FlatAppearance.BorderSize = 0;
        }

        private void UpdateFileViewButtons()
        {
            SetFilterButtonState(_btnViewAll,   _fileViewMode == 0);
            SetFilterButtonState(_btnViewTop10, _fileViewMode == 1);
            SetFilterButtonState(_btnViewToday, _fileViewMode == 2);
            // mode 3 = category selected — no button active, fileTypePanel shows selection
        }

        private static DataGridViewCellStyle RightStyle()
        {
            return new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight };
        }

        private static string ConfidentialityBadge(int level)
        {
            switch (level)
            {
                case 1: return "1HC";
                case 2: return "2SC";
                case 3: return "3NC";
                case 4: return "4PC";
                default: return "-";
            }
        }

        private static Color ConfidentialityColor(int level)
        {
            switch (level)
            {
                case 1: return Color.FromArgb(192, 0, 0);
                case 2: return Color.FromArgb(210, 105, 30);
                case 3: return Color.FromArgb(0, 84, 166);
                case 4: return Color.FromArgb(0, 128, 0);
                default: return Color.FromArgb(55, 55, 55);
            }
        }

        private static string TierText(int tier)
        {
            switch (tier)
            {
                case 1: return "SubCommittee";
                case 2: return "Division";
                case 3: return "Department";
                default: return "OrgUnit";
            }
        }

        private static Color GetUsageColor(double pct)
        {
            if (pct >= 100) return Color.FromArgb(180, 30, 30);
            if (pct >= 95)  return Color.FromArgb(200, 60, 40);
            if (pct >= 80)  return Color.FromArgb(220, 140, 0);
            return Color.FromArgb(39, 174, 96);
        }

        private static void DrawThresholdMarker(Graphics g, Rectangle rect, int cx, int cy,
            float percent, Color color, string label)
        {
            double angle = (percent / 100.0 * 360.0 - 90.0) * Math.PI / 180.0;
            int    r     = rect.Width / 2;
            int    x1    = cx + (int)(r * Math.Cos(angle));
            int    y1    = cy + (int)(r * Math.Sin(angle));
            int    x0    = cx + (int)(r * 0.55 * Math.Cos(angle));
            int    y0    = cy + (int)(r * 0.55 * Math.Sin(angle));
            using (var pen = new Pen(color, 1.5f) { DashStyle = System.Drawing.Drawing2D.DashStyle.Dash })
                g.DrawLine(pen, x0, y0, x1, y1);
            using (var lf = new Font("Segoe UI", 7F))
                g.DrawString(label, lf, new SolidBrush(color), x1 + 2, y1 - 7);
        }

        private static Color UsageColor(double pct, long quotaBytes)
        {
            if (quotaBytes <= 0) return Color.FromArgb(26, 58, 108);
            if (pct >= 95) return Color.FromArgb(192, 0, 0);
            if (pct >= 80) return Color.FromArgb(210, 105, 30);
            return Color.FromArgb(52, 120, 47);
        }

        private static string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB", "TB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return string.Format("{0:0.##} {1}", len, sizes[order]);
        }

        private static string Csv(string value)
        {
            if (value == null) return "";
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private class PieRow
        {
            public string Label { get; set; }
            public long Bytes { get; set; }
            public Color Color { get; set; }
        }

        private class FileTypeRow
        {
            public string Label { get; set; }
            public int Count { get; set; }
            public long Bytes { get; set; }
        }
    }
}
