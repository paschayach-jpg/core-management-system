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
    public class FolderDetailForm : Form
    {
        private readonly int _folderId;
        private readonly Folder _initialFolder;
        private readonly IFolderReadLookupController _folderLookup;
        private readonly IFileBrowseController _fileBrowse;
        private readonly IFolderQuotaService _quotaService;
        private readonly UserContext _user;

        private Folder _folder;
        private List<Folder> _children = new List<Folder>();
        private List<Folder> _descendants = new List<Folder>();
        private List<FileDocument> _files = new List<FileDocument>();

        private Label _lblTitle;
        private Label _lblBreadcrumb;
        private Label _lblSnapshot;
        private Label _lblStatus;
        private TableLayoutPanel _summaryGrid;
        private SplitContainer _split;
        private ListView _lvChildren;
        private DataGridView _gridFiles;

        public FolderDetailForm(
            Folder folder,
            IFolderReadLookupController folderLookup,
            IFileBrowseController fileBrowse,
            IFolderQuotaService quotaService,
            UserContext user)
        {
            if (folder == null) throw new ArgumentNullException("folder");
            if (folderLookup == null) throw new ArgumentNullException("folderLookup");
            if (user == null) throw new ArgumentNullException("user");

            _folderId = folder.FolderID;
            _initialFolder = folder;
            _folderLookup = folderLookup;
            _fileBrowse = fileBrowse;
            _quotaService = quotaService;
            _user = user;

            InitializeComponent();
            LoadSnapshot();
        }

        private void InitializeComponent()
        {
            Text = "รายละเอียด Folder";
            Width = 1180;
            Height = 740;
            MinimumSize = new Size(980, 620);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.White;

            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 44,
                BackColor = Color.FromArgb(248, 250, 253),
                Padding = new Padding(10, 7, 10, 7)
            };

            var btnRefresh = CreateToolbarButton("รีเฟรช", 10, Color.FromArgb(43, 87, 154));
            btnRefresh.Click += (s, e) => LoadSnapshot();

            var btnExport = CreateToolbarButton("Export CSV", 100, Color.FromArgb(52, 120, 47));
            btnExport.Width = 100;
            btnExport.Click += (s, e) => ExportCsv();

            var btnClose = CreateToolbarButton("ปิด", 210, Color.FromArgb(105, 105, 105));
            btnClose.Click += (s, e) => Close();

            _lblSnapshot = new Label
            {
                Dock = DockStyle.Right,
                Width = 260,
                TextAlign = ContentAlignment.MiddleRight,
                ForeColor = Color.FromArgb(70, 70, 70)
            };

            toolbar.Controls.Add(btnRefresh);
            toolbar.Controls.Add(btnExport);
            toolbar.Controls.Add(btnClose);
            toolbar.Controls.Add(_lblSnapshot);

            var header = new Panel
            {
                Dock = DockStyle.Top,
                Height = 76,
                BackColor = Color.FromArgb(26, 58, 108),
                Padding = new Padding(14, 8, 14, 8)
            };
            _lblTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 34,
                ForeColor = Color.White,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                AutoEllipsis = true
            };
            _lblBreadcrumb = new Label
            {
                Dock = DockStyle.Top,
                Height = 24,
                ForeColor = Color.FromArgb(200, 220, 255),
                AutoEllipsis = true
            };
            header.Controls.Add(_lblBreadcrumb);
            header.Controls.Add(_lblTitle);

            _summaryGrid = new TableLayoutPanel
            {
                Dock = DockStyle.Top,
                Height = 94,
                BackColor = Color.White,
                Padding = new Padding(10),
                ColumnCount = 5,
                RowCount = 1
            };
            for (int i = 0; i < 5; i++)
                _summaryGrid.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 20F));

            _split = new SplitContainer
            {
                Dock = DockStyle.Fill,
                SplitterWidth = 5,
                BackColor = Color.FromArgb(225, 230, 238)
            };

            _lvChildren = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
                BorderStyle = BorderStyle.None
            };
            _lvChildren.Columns.Add("Sub-folder", 190);
            _lvChildren.Columns.Add("Files", 58, HorizontalAlignment.Right);
            _lvChildren.Columns.Add("Size", 90, HorizontalAlignment.Right);
            _lvChildren.Columns.Add("%", 48, HorizontalAlignment.Right);

            var leftHeader = new Label
            {
                Dock = DockStyle.Top,
                Height = 32,
                Text = "  โครงสร้างย่อย",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 58, 108),
                BackColor = Color.FromArgb(245, 248, 252)
            };
            _split.Panel1.Controls.Add(_lvChildren);
            _split.Panel1.Controls.Add(leftHeader);

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
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None
            };
            _gridFiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Name", HeaderText = "ชื่อไฟล์", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            _gridFiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Type", HeaderText = "ประเภท", Width = 80 });
            _gridFiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Size", HeaderText = "ขนาด", Width = 105, DefaultCellStyle = RightStyle() });
            _gridFiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Uploaded", HeaderText = "วันที่อัปโหลด", Width = 140 });
            _gridFiles.Columns.Add(new DataGridViewTextBoxColumn { Name = "Version", HeaderText = "Version", Width = 80, DefaultCellStyle = RightStyle() });
            _gridFiles.EnableHeadersVisualStyles = false;
            _gridFiles.ColumnHeadersDefaultCellStyle.BackColor = Color.FromArgb(26, 58, 108);
            _gridFiles.ColumnHeadersDefaultCellStyle.ForeColor = Color.White;
            _gridFiles.ColumnHeadersDefaultCellStyle.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            _gridFiles.AlternatingRowsDefaultCellStyle.BackColor = Color.FromArgb(247, 250, 254);

            var rightHeader = new Label
            {
                Dock = DockStyle.Top,
                Height = 32,
                Text = "  ไฟล์ใน Folder นี้",
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 58, 108),
                BackColor = Color.FromArgb(245, 248, 252)
            };
            _split.Panel2.Controls.Add(_gridFiles);
            _split.Panel2.Controls.Add(rightHeader);

            _lblStatus = new Label
            {
                Dock = DockStyle.Bottom,
                Height = 28,
                Padding = new Padding(10, 0, 10, 0),
                TextAlign = ContentAlignment.MiddleLeft,
                BackColor = Color.FromArgb(240, 244, 250),
                ForeColor = Color.FromArgb(43, 87, 154)
            };

            Controls.Add(_split);
            Controls.Add(_lblStatus);
            Controls.Add(_summaryGrid);
            Controls.Add(header);
            Controls.Add(toolbar);

            Shown += (s, e) => AdjustSplit();
            Resize += (s, e) => AdjustSplit();
        }

        private void AdjustSplit()
        {
            if (_split == null) return;
            int min1 = _split.Panel1MinSize;
            int min2 = _split.Panel2MinSize;
            int max = _split.Width - min2 - _split.SplitterWidth;
            if (max < min1) return;
            int target = Math.Max(min1, Math.Min(max, 360));
            try
            {
                if (_split.SplitterDistance != target)
                    _split.SplitterDistance = target;
            }
            catch (InvalidOperationException) { }
        }

        private void LoadSnapshot()
        {
            try
            {
                Cursor = Cursors.WaitCursor;

                _folder = _folderLookup.GetFolder(_folderId, _user) ?? _initialFolder;
                _children = TryGetChildren(_folderId);
                _descendants = TryGetDescendants(_folderId);
                _files = _fileBrowse != null
                    ? _fileBrowse.GetFilesInFolder(_folderId, _user).Where(f => !f.IsDeleted).ToList()
                    : new List<FileDocument>();

                RenderSnapshot();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถโหลดรายละเอียด Folder ได้: " + ex.Message,
                    "รายละเอียด Folder", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private List<Folder> TryGetChildren(int folderId)
        {
            try { return _folderLookup.GetChildrenWithPermission(folderId, _user).Where(f => !f.IsDeleted).ToList(); }
            catch
            {
                try { return _folderLookup.GetChildren(folderId).Where(f => !f.IsDeleted).ToList(); }
                catch { return new List<Folder>(); }
            }
        }

        private List<Folder> TryGetDescendants(int folderId)
        {
            try { return _folderLookup.GetDescendantsByUser(folderId).Where(f => !f.IsDeleted).ToList(); }
            catch { return new List<Folder>(); }
        }

        private void RenderSnapshot()
        {
            long directBytes = _files.Sum(f => f.FileSizeBytes);
            long childrenBytes = _children.Sum(f => Math.Max(0, f.FolderSizeBytes));
            long totalBytes = Math.Max(Math.Max(0, _folder.FolderSizeBytes), directBytes) + _descendants.Sum(f => Math.Max(0, f.FolderSizeBytes));
            int totalFiles = _files.Count + _descendants.Sum(f => Math.Max(0, f.FileCount));
            long quotaBytes = ResolveQuotaBytes(_folder);
            long freeBytes = quotaBytes > 0 ? Math.Max(0, quotaBytes - totalBytes) : 0;
            double usedPct = quotaBytes > 0 ? Math.Min(totalBytes * 100.0 / quotaBytes, 999.0) : 0;

            Text = string.Format("รายละเอียด Folder - {0}", _folder.FolderName);
            _lblTitle.Text = string.Format("{0}  [{1}]", _folder.FolderName, ConfidentialityBadge(_folder.ConfidentialityLevel));
            _lblBreadcrumb.Text = BuildBreadcrumb(_folder);
            _lblSnapshot.Text = "Snapshot: " + DateTime.Now.ToString("yyyy-MM-dd HH:mm");

            _summaryGrid.Controls.Clear();
            AddSummaryCard("ใช้พื้นที่รวม", FormatFileSize(totalBytes), quotaBytes > 0 ? string.Format("{0:0.0}% ของ quota", usedPct) : "ยังไม่ได้กำหนด quota", UsageColor(usedPct, quotaBytes));
            AddSummaryCard("Quota", quotaBytes > 0 ? FormatFileSize(quotaBytes) : "ไม่กำหนด", "จาก folder/org config", Color.FromArgb(26, 58, 108));
            AddSummaryCard("คงเหลือ", quotaBytes > 0 ? FormatFileSize(freeBytes) : "-", quotaBytes > 0 ? "พื้นที่ที่ยังใช้ได้" : "ไม่จำกัด", Color.FromArgb(52, 120, 47));
            AddSummaryCard("Sub-folder", _children.Count.ToString("N0"), string.Format("รวม descendants {0:N0}", _descendants.Count), Color.FromArgb(92, 92, 92));
            AddSummaryCard("Files", totalFiles.ToString("N0"), string.Format("ใน folder นี้ {0:N0}", _files.Count), Color.FromArgb(92, 92, 92));

            RenderChildren(totalBytes);
            RenderFiles();
            _lblStatus.Text = string.Format("FolderID: {0} | OrgUnitID: {1} | Direct files: {2:N0} | Direct size: {3}",
                _folder.FolderID, _folder.OrgUnitID, _files.Count, FormatFileSize(directBytes));
        }

        private void RenderChildren(long totalBytes)
        {
            _lvChildren.BeginUpdate();
            try
            {
                _lvChildren.Items.Clear();
                foreach (var child in _children.OrderByDescending(f => f.FolderSizeBytes).ThenBy(f => f.FolderName))
                {
                    double pct = totalBytes > 0 ? child.FolderSizeBytes * 100.0 / totalBytes : 0;
                    var item = new ListViewItem(child.FolderName);
                    item.SubItems.Add(child.FileCount.ToString("N0"));
                    item.SubItems.Add(FormatFileSize(child.FolderSizeBytes));
                    item.SubItems.Add(string.Format("{0:0.0}", pct));
                    item.Tag = child;
                    _lvChildren.Items.Add(item);
                }
            }
            finally
            {
                _lvChildren.EndUpdate();
            }
        }

        private void RenderFiles()
        {
            _gridFiles.Rows.Clear();
            foreach (var file in _files.OrderByDescending(f => f.FileSizeBytes).ThenBy(f => f.OriginalFileName))
            {
                _gridFiles.Rows.Add(
                    file.OriginalFileName,
                    (file.FileExtension ?? "").TrimStart('.').ToUpperInvariant(),
                    FormatFileSize(file.FileSizeBytes),
                    file.UploadedDate.ToString("dd/MM/yyyy HH:mm"),
                    file.Version.ToString());
            }
        }

        private long ResolveQuotaBytes(Folder folder)
        {
            if (folder.QuotaBytes.HasValue && folder.QuotaBytes.Value > 0)
                return folder.QuotaBytes.Value;

            if (_quotaService == null)
                return 0;

            try
            {
                var config = _quotaService.GetConfig(folder.OrgUnitID);
                return config != null ? config.QuotaBytes : 0;
            }
            catch
            {
                return 0;
            }
        }

        private string BuildBreadcrumb(Folder folder)
        {
            var parts = new List<string>();
            var visited = new HashSet<int>();
            var current = folder;

            while (current != null && visited.Add(current.FolderID))
            {
                parts.Add(current.FolderName);
                if (!current.ParentFolderID.HasValue || current.ParentFolderID.Value == 0)
                    break;

                try { current = _folderLookup.GetById(current.ParentFolderID.Value); }
                catch { break; }
            }

            parts.Reverse();
            return string.Join(" > ", parts);
        }

        private void AddSummaryCard(string title, string value, string caption, Color accent)
        {
            var panel = new Panel
            {
                Dock = DockStyle.Fill,
                Margin = new Padding(4),
                BackColor = Color.FromArgb(248, 250, 253),
                Padding = new Padding(10)
            };
            panel.Paint += (s, e) =>
            {
                using (var pen = new Pen(Color.FromArgb(220, 226, 235)))
                    e.Graphics.DrawRectangle(pen, 0, 0, panel.Width - 1, panel.Height - 1);
            };

            var lblTitle = new Label
            {
                Dock = DockStyle.Top,
                Height = 20,
                Text = title,
                ForeColor = Color.FromArgb(90, 90, 90)
            };
            var lblValue = new Label
            {
                Dock = DockStyle.Top,
                Height = 32,
                Text = value,
                Font = new Font("Segoe UI", 15F, FontStyle.Bold),
                ForeColor = accent,
                AutoEllipsis = true
            };
            var lblCaption = new Label
            {
                Dock = DockStyle.Fill,
                Text = caption,
                ForeColor = Color.FromArgb(110, 110, 110),
                AutoEllipsis = true
            };

            panel.Controls.Add(lblCaption);
            panel.Controls.Add(lblValue);
            panel.Controls.Add(lblTitle);
            _summaryGrid.Controls.Add(panel);
        }

        private void ExportCsv()
        {
            using (var sfd = new SaveFileDialog())
            {
                sfd.Title = "Export Folder Detail";
                sfd.FileName = string.Format("folder-detail-{0}-{1:yyyyMMddHHmm}.csv", _folderId, DateTime.Now);
                sfd.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                if (sfd.ShowDialog(this) != DialogResult.OK)
                    return;

                var sb = new StringBuilder();
                sb.AppendLine("Section,Name,Type,SizeBytes,Size,Date,Version");
                foreach (var child in _children.OrderBy(f => f.FolderName))
                    sb.AppendLine(string.Format("SubFolder,{0},Folder,{1},{2},,",
                        Csv(child.FolderName), child.FolderSizeBytes, Csv(FormatFileSize(child.FolderSizeBytes))));
                foreach (var file in _files.OrderBy(f => f.OriginalFileName))
                    sb.AppendLine(string.Format("File,{0},{1},{2},{3},{4},{5}",
                        Csv(file.OriginalFileName),
                        Csv((file.FileExtension ?? "").TrimStart('.').ToUpperInvariant()),
                        file.FileSizeBytes,
                        Csv(FormatFileSize(file.FileSizeBytes)),
                        Csv(file.UploadedDate.ToString("yyyy-MM-dd HH:mm:ss")),
                        file.Version));

                File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                MessageBox.Show("Export เรียบร้อยแล้ว", "Folder Detail", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private static Button CreateToolbarButton(string text, int left, Color backColor)
        {
            var btn = new Button
            {
                Text = text,
                Left = left,
                Top = 7,
                Width = 82,
                Height = 29,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
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
    }
}
