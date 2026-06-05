using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    public class FolderUsageReportForm : Form
    {
        private readonly IFolderReadLookupController _folderLookup;
        private readonly IFolderQuotaService _quotaService;
        private readonly IOrgStructureService _orgService;
        private readonly UserContext _user;

        private readonly List<FolderUsageRow> _rows = new List<FolderUsageRow>();
        private List<FolderUsageRow> _filteredRows = new List<FolderUsageRow>();
        private readonly List<OrgUnit> _orgUnits = new List<OrgUnit>();
        private readonly Dictionary<int, string> _orgNames = new Dictionary<int, string>();
        private readonly Dictionary<int, FolderQuotaConfig> _quotaConfigsById = new Dictionary<int, FolderQuotaConfig>();
        private readonly Dictionary<string, FolderQuotaConfig> _quotaConfigsByName = new Dictionary<string, FolderQuotaConfig>(StringComparer.OrdinalIgnoreCase);

        private Panel _pnlHeader;
        private Label _lblTitle;
        private Label _lblSubtitle;
        private Panel _pnlKpi;
        private Label _lblTotalUsed;
        private Label _lblTotalQuota;
        private Label _lblTotalRemaining;
        private Label _lblWarningCount;
        private Panel _pnlFilter;
        private TextBox _txtSearch;
        private ComboBox _cmbOrgUnit;
        private ComboBox _cmbStatus;
        private ComboBox _cmbUsageMode;
        private ComboBox _cmbFolderState;
        private NumericUpDown _nudMinSizeMb;
        private Button _btnRefresh;
        private Button _btnExportCsv;
        private Button _btnCopySummary;
        private Button _btnCloseReport;
        private TreeView _treeUsage;
        private DataGridView _grid;
        private Label _lblStatus;

        public FolderUsageReportForm(
            IFolderReadLookupController folderLookup,
            IFolderQuotaService quotaService,
            IOrgStructureService orgService,
            UserContext user)
        {
            if (folderLookup == null) throw new ArgumentNullException("folderLookup");
            if (user == null) throw new ArgumentNullException("user");

            _folderLookup = folderLookup;
            _quotaService = quotaService;
            _orgService = orgService;
            _user = user;

            InitializeComponent();
            LoadOrgUnits();
            LoadReport();
        }

        private void InitializeComponent()
        {
            Text = "รายงานการใช้พื้นที่ Folder";
            Width = 1360;
            Height = 720;
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9F);
            BackColor = Color.White;

            _pnlHeader = new Panel { Dock = DockStyle.Top, Height = 66, BackColor = Color.FromArgb(26, 58, 108), Padding = new Padding(14, 8, 14, 8) };
            _lblTitle = new Label { Text = "รายงานการใช้พื้นที่ Folder", Dock = DockStyle.Top, Height = 28, ForeColor = Color.White, Font = new Font("Segoe UI", 15F, FontStyle.Bold) };
            _lblSubtitle = new Label { Text = "สำหรับ Infra และผู้บริหาร: ตรวจสอบการใช้งานราย Folder / Project แบบอ่านอย่างเดียว", Dock = DockStyle.Top, Height = 22, ForeColor = Color.FromArgb(190, 215, 255) };
            _pnlHeader.Controls.Add(_lblSubtitle);
            _pnlHeader.Controls.Add(_lblTitle);

            _pnlKpi = new Panel { Dock = DockStyle.Top, Height = 58, BackColor = Color.FromArgb(245, 248, 252), Padding = new Padding(10, 8, 10, 8) };
            _lblTotalUsed = CreateKpiLabel("ใช้พื้นที่รวม: —");
            _lblTotalQuota = CreateKpiLabel("Quota รวม: —");
            _lblTotalRemaining = CreateKpiLabel("คงเหลือรวม: —");
            _lblWarningCount = CreateKpiLabel("ใกล้เต็ม/เกิน: —");
            _pnlKpi.Controls.Add(_lblWarningCount);
            _pnlKpi.Controls.Add(_lblTotalRemaining);
            _pnlKpi.Controls.Add(_lblTotalQuota);
            _pnlKpi.Controls.Add(_lblTotalUsed);

            _pnlFilter = new Panel { Dock = DockStyle.Top, Height = 44, BackColor = Color.White, Padding = new Padding(10, 7, 10, 6) };

            _txtSearch = new TextBox { Width = 220, Left = 10, Top = 9, Anchor = AnchorStyles.Left | AnchorStyles.Top };
            _txtSearch.PlaceholderTextCompat("ค้นหา Folder / หน่วยงาน");

            _cmbOrgUnit = new ComboBox { Left = 240, Top = 8, Width = 230, DropDownStyle = ComboBoxStyle.DropDownList };

            _cmbStatus = new ComboBox { Left = 480, Top = 8, Width = 145, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbStatus.Items.AddRange(new object[] { "ทุกสถานะ", "ปกติ", "ใกล้เต็ม", "วิกฤต", "เกิน quota", "ยังไม่กำหนด quota" });
            _cmbStatus.SelectedIndex = 0;

            _cmbUsageMode = new ComboBox { Left = 635, Top = 8, Width = 135, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbUsageMode.Items.AddRange(new object[] { "ทุกการใช้งาน", "ใช้งานจริง", "ไม่มีการใช้งาน" });
            _cmbUsageMode.SelectedIndex = 0;

            _cmbFolderState = new ComboBox { Left = 780, Top = 8, Width = 135, DropDownStyle = ComboBoxStyle.DropDownList };
            _cmbFolderState.Items.AddRange(new object[] { "ทุกสถานะ Folder", "เปิดใช้งาน", "ปิดใช้งาน", "ลบแล้ว" });
            _cmbFolderState.SelectedIndex = 0;

            var lblMin = new Label { Text = "มากกว่า MB:", Left = 635, Top = 12, Width = 78, ForeColor = Color.FromArgb(80, 80, 80) };
            lblMin.Left = 925;
            _nudMinSizeMb = new NumericUpDown { Left = 1006, Top = 8, Width = 78, Minimum = 0, Maximum = 1024 * 1024, DecimalPlaces = 0 };
            _nudMinSizeMb.ValueChanged += (s, e) => ApplyFilters();

            _btnRefresh = CreateButton("รีเฟรช", 805, Color.FromArgb(43, 87, 154));
            _btnRefresh.Click += (s, e) => LoadReport();
            _btnExportCsv = CreateButton("Export CSV", 895, Color.FromArgb(52, 120, 47));
            _btnExportCsv.Click += (s, e) => ExportCsv();
            _btnCopySummary = CreateButton("Copy Summary", 1000, Color.FromArgb(92, 92, 92));
            _btnCopySummary.Click += (s, e) => CopySummary();
            _btnCloseReport = CreateButton("ปิด", 1410, Color.FromArgb(100, 100, 100));
            _btnCloseReport.Click += (s, e) => Close();

            _pnlFilter.Controls.AddRange(new Control[] { _txtSearch, _cmbOrgUnit, _cmbStatus, lblMin, _nudMinSizeMb, _btnRefresh, _btnExportCsv, _btnCopySummary, _btnCloseReport });
            _btnRefresh.Left = 1095;
            _btnExportCsv.Left = 1185;
            _btnCopySummary.Left = 1290;
            _btnCloseReport.Left = 1410;
            _pnlFilter.Controls.Add(_cmbUsageMode);
            _pnlFilter.Controls.Add(_cmbFolderState);

            _grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                ReadOnly = true,
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                MultiSelect = false,
                AutoSizeColumnsMode = DataGridViewAutoSizeColumnsMode.None,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                RowHeadersVisible = false
            };
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "OrgUnit", HeaderText = "หน่วยงาน", Width = 180 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Folder", HeaderText = "Folder / Project", Width = 260 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Level", HeaderText = "ระดับ", Width = 70 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "UsageMode", HeaderText = "การใช้งาน", Width = 95 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "FolderState", HeaderText = "สถานะ Folder", Width = 95 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "SubFolders", HeaderText = "Sub", Width = 55, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Files", HeaderText = "ไฟล์", Width = 65, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "DirectSize", HeaderText = "ใช้ใน Folder", Width = 105, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "TotalSize", HeaderText = "รวม Subfolder", Width = 115, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "ConfigSize", HeaderText = "ขนาด Config", Width = 105, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Quota", HeaderText = "Quota", Width = 95, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Remaining", HeaderText = "คงเหลือ", Width = 95, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "FolderPct", HeaderText = "% Folder", Width = 80, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "OrgPct", HeaderText = "% Org", Width = 75, DefaultCellStyle = RightStyle() });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Status", HeaderText = "สถานะ", Width = 100 });
            _grid.Columns.Add(new DataGridViewTextBoxColumn { Name = "Updated", HeaderText = "อัปเดตล่าสุด", Width = 120 });

            _treeUsage = new TreeView
            {
                Dock = DockStyle.Fill,
                BorderStyle = BorderStyle.None,
                HideSelection = false,
                FullRowSelect = true,
                ShowNodeToolTips = true,
                Font = new Font("Segoe UI", 9F),
                Indent = 16
            };
            _treeUsage.AfterSelect += (s, e) => ApplyTreeSelection(e.Node);

            var pnlContent = new Panel
            {
                Dock = DockStyle.Fill,
                BackColor = Color.White
            };
            var pnlTree = new Panel
            {
                Dock = DockStyle.Left,
                Width = 330,
                BackColor = Color.White
            };
            var pnlDivider = new Panel
            {
                Dock = DockStyle.Left,
                Width = 5,
                BackColor = Color.FromArgb(225, 230, 238)
            };
            pnlTree.Controls.Add(_treeUsage);
            pnlContent.Controls.Add(_grid);
            pnlContent.Controls.Add(pnlDivider);
            pnlContent.Controls.Add(pnlTree);

            _lblStatus = new Label { Dock = DockStyle.Bottom, Height = 28, Padding = new Padding(10, 0, 10, 0), TextAlign = ContentAlignment.MiddleLeft, BackColor = Color.FromArgb(240, 244, 250), ForeColor = Color.FromArgb(43, 87, 154) };

            Controls.Add(pnlContent);
            Controls.Add(_lblStatus);
            Controls.Add(_pnlFilter);
            Controls.Add(_pnlKpi);
            Controls.Add(_pnlHeader);

            _txtSearch.TextChanged += (s, e) => ApplyFilters();
            _cmbOrgUnit.SelectedIndexChanged += (s, e) => ApplyFilters();
            _cmbStatus.SelectedIndexChanged += (s, e) => ApplyFilters();
            _cmbUsageMode.SelectedIndexChanged += (s, e) => ApplyFilters();
            _cmbFolderState.SelectedIndexChanged += (s, e) => ApplyFilters();
        }

        private static Label CreateKpiLabel(string text)
        {
            return new Label
            {
                Dock = DockStyle.Left,
                Width = 250,
                Text = text,
                TextAlign = ContentAlignment.MiddleLeft,
                Font = new Font("Segoe UI", 10F, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 58, 108),
                Padding = new Padding(8, 0, 8, 0)
            };
        }

        private static Button CreateButton(string text, int left, Color backColor)
        {
            return new Button
            {
                Text = text,
                Left = left,
                Top = 7,
                Width = text.Length > 9 ? 112 : 82,
                Height = 29,
                BackColor = backColor,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold)
            };
        }

        private static DataGridViewCellStyle RightStyle()
        {
            return new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight };
        }

        private void AddOrgFilterItems(IEnumerable<OrgUnit> orgUnits)
        {
            if (orgUnits == null)
                return;

            foreach (var org in orgUnits.Where(o => o != null).GroupBy(o => o.OrgUnitID).Select(g => g.First()))
            {
                if (_orgUnits.All(o => o.OrgUnitID != org.OrgUnitID))
                    _orgUnits.Add(org);
            }

            var groups = orgUnits
                .Where(o => o != null && !string.IsNullOrWhiteSpace(o.Name))
                .GroupBy(o => o.Name.Trim(), StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g.Key);

            foreach (var group in groups)
            {
                var ids = group.Select(o => o.OrgUnitID).Distinct().OrderBy(id => id).ToList();
                if (ids.Count == 0)
                    continue;

                foreach (var org in group)
                    _orgNames[org.OrgUnitID] = group.Key;

                _cmbOrgUnit.Items.Add(new OrgFilterItem
                {
                    OrgUnitID = ids[0],
                    OrgUnitIDs = ids,
                    Name = group.Key
                });
            }
        }

        private void LoadOrgUnits()
        {
            _orgNames.Clear();
            _orgUnits.Clear();
            _cmbOrgUnit.Items.Clear();
            _cmbOrgUnit.Items.Add(new OrgFilterItem { OrgUnitID = null, Name = "ทุกหน่วยงาน" });

            if (_orgService != null)
            {
                try
                {
                    var visibleOrgs = _orgService.GetVisibleHierarchy(_user) ?? Enumerable.Empty<OrgUnit>();
                    AddOrgFilterItems(visibleOrgs);
                }
                catch
                {
                    try
                    {
                        var allOrgs = _orgService.GetHierarchy() ?? Enumerable.Empty<OrgUnit>();
                        AddOrgFilterItems(allOrgs);
                    }
                    catch
                    {
                        // Report can still open without the org filter list.
                    }
                }
            }

            _cmbOrgUnit.SelectedIndex = 0;
        }

        private void LoadReport()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                _rows.Clear();

                var selectedOrgUnitIds = SelectedOrgUnitIds();
                int? orgFilter = selectedOrgUnitIds.Count == 1 ? selectedOrgUnitIds[0] : (int?)null;
                var folders = _folderLookup.GetAllFolders(orgFilter, _user)
                    .Where(f => f != null)
                    .Where(f => selectedOrgUnitIds.Count == 0 || selectedOrgUnitIds.Contains(f.OrgUnitID))
                    .GroupBy(f => f.FolderID)
                    .Select(g => g.First())
                    .ToList();

                var childrenByParent = folders
                    .Where(f => f.ParentFolderID.HasValue)
                    .GroupBy(f => f.ParentFolderID.Value)
                    .ToDictionary(g => g.Key, g => g.ToList());
                var quotaByOrg = new Dictionary<int, QuotaUsageResult>();
                var totalSizeMemo = new Dictionary<int, long>();
                var subCountMemo = new Dictionary<int, int>();
                var detailRows = new List<FolderUsageRow>();
                LoadQuotaConfigLookup();

                foreach (var folder in folders.Where(IsQuotaConfigFolder))
                {
                    QuotaUsageResult usage = GetQuota(folder.OrgUnitID, quotaByOrg);
                    string orgName = ResolveOrgName(folder);
                    FolderQuotaConfig config = ResolveQuotaConfig(folder.OrgUnitID, orgName);
                    long totalSize = Math.Max(0, folder.FolderSizeBytes) + SumFolderSize(folder.FolderID, childrenByParent, totalSizeMemo, new HashSet<int>());
                    int subCount = CountSubFolders(folder.FolderID, childrenByParent, subCountMemo, new HashSet<int>());
                    long quotaBytes = config != null && config.QuotaBytes > 0
                        ? config.QuotaBytes
                        : usage != null ? usage.QuotaBytes : 0;
                    long remaining = usage != null && usage.QuotaBytes > 0
                        ? Math.Max(0, usage.QuotaBytes - usage.UsedBytes)
                        : 0;
                    double folderPercent = quotaBytes > 0 ? (double)totalSize / quotaBytes * 100.0 : 0;

                    detailRows.Add(new FolderUsageRow
                    {
                        OrgUnitID = folder.OrgUnitID,
                        OrgUnitName = orgName,
                        FolderID = folder.FolderID,
                        FolderName = folder.FolderName ?? folder.Name ?? ("Folder #" + folder.FolderID),
                        ConfidentialityLevel = folder.ConfidentialityLevel,
                        IsArchived = folder.IsArchived,
                        IsDeleted = folder.IsDeleted,
                        SubFolderCount = subCount,
                        FileCount = folder.FileCount,
                        DirectSizeBytes = folder.FolderSizeBytes,
                        TotalSizeBytes = totalSize,
                        QuotaBytes = quotaBytes,
                        RemainingBytes = remaining,
                        FolderPercentOfQuota = folderPercent,
                        OrgUsedPercent = usage != null ? usage.UsedPercent : 0,
                        Alert = usage != null ? usage.Alert : QuotaAlertLevel.Normal,
                        HasQuota = quotaBytes > 0,
                        UpdatedDate = folder.UpdatedDate ?? folder.CreatedDate
                    });
                }

                ApplyConfiguredQuotaTotals(detailRows);
                _rows.AddRange(AggregateConfigRows(detailRows));
                ApplyFilters();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถโหลดรายงานได้: " + ex.Message, "Folder Usage Report", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private QuotaUsageResult GetQuota(int orgUnitId, Dictionary<int, QuotaUsageResult> cache)
        {
            QuotaUsageResult usage;
            if (cache.TryGetValue(orgUnitId, out usage))
                return usage;

            if (_quotaService == null)
            {
                cache[orgUnitId] = null;
                return null;
            }

            try { usage = _quotaService.GetUsage(orgUnitId); }
            catch { usage = null; }
            cache[orgUnitId] = usage;
            return usage;
        }

        private void LoadQuotaConfigLookup()
        {
            _quotaConfigsById.Clear();
            _quotaConfigsByName.Clear();

            if (_quotaService == null)
                return;

            IEnumerable<FolderQuotaConfig> configs;
            try { configs = _quotaService.GetAllConfigs() ?? Enumerable.Empty<FolderQuotaConfig>(); }
            catch { return; }

            foreach (var config in configs.Where(c => c != null && c.QuotaBytes > 0))
            {
                if (!_quotaConfigsById.ContainsKey(config.OrgUnitID))
                    _quotaConfigsById.Add(config.OrgUnitID, config);

                var name = NormalizeKey(config.OrgUnitName);
                if (name.Length > 0 && !_quotaConfigsByName.ContainsKey(name))
                    _quotaConfigsByName.Add(name, config);
            }
        }

        private FolderQuotaConfig ResolveQuotaConfig(int orgUnitId, string orgUnitName)
        {
            FolderQuotaConfig config;
            if (_quotaConfigsById.TryGetValue(orgUnitId, out config))
                return config;

            var name = NormalizeKey(orgUnitName);
            return name.Length > 0 && _quotaConfigsByName.TryGetValue(name, out config) ? config : null;
        }

        private static string NormalizeKey(string value)
        {
            return (value ?? string.Empty).Trim();
        }

        private static void ApplyConfiguredQuotaTotals(IList<FolderUsageRow> rows)
        {
            if (rows == null || rows.Count == 0)
                return;

            foreach (var group in rows.GroupBy(r => new { r.OrgUnitID, OrgUnit = NormalizeKey(r.OrgUnitName) }))
            {
                long quotaBytes = group.Where(r => r.QuotaBytes > 0).Select(r => r.QuotaBytes).DefaultIfEmpty(0).First();
                if (quotaBytes <= 0)
                    continue;

                long usedBytes = group.Sum(r => r.TotalSizeBytes);
                long remainingBytes = Math.Max(0, quotaBytes - usedBytes);
                double orgPct = quotaBytes > 0 ? (double)usedBytes / quotaBytes * 100.0 : 0;
                var alert = AlertFromPercent(orgPct);

                foreach (var row in group)
                {
                    row.QuotaBytes = quotaBytes;
                    row.RemainingBytes = remainingBytes;
                    row.FolderPercentOfQuota = quotaBytes > 0 ? (double)row.TotalSizeBytes / quotaBytes * 100.0 : 0;
                    row.OrgUsedPercent = orgPct;
                    row.Alert = alert;
                    row.HasQuota = true;
                }
            }
        }

        private static QuotaAlertLevel AlertFromPercent(double pct)
        {
            if (pct >= 100) return QuotaAlertLevel.Full;
            if (pct >= 95) return QuotaAlertLevel.Critical;
            if (pct >= 80) return QuotaAlertLevel.Warning;
            return QuotaAlertLevel.Normal;
        }

        private string ResolveOrgName(Folder folder)
        {
            if (folder == null)
                return "—";
            if (!string.IsNullOrWhiteSpace(folder.OrgUnitName))
                return folder.OrgUnitName;
            string name;
            return _orgNames.TryGetValue(folder.OrgUnitID, out name) ? name : ("OrgUnit #" + folder.OrgUnitID);
        }

        private static bool IsQuotaConfigFolder(Folder folder)
        {
            if (folder == null || folder.ConfidentialityLevel < 1 || folder.ConfidentialityLevel > 4)
                return false;

            var name = (folder.FolderName ?? folder.Name ?? string.Empty).Trim();
            if (string.IsNullOrEmpty(name))
                return false;

            return name.StartsWith(LevelText(folder.ConfidentialityLevel), StringComparison.OrdinalIgnoreCase);
        }

        private static List<FolderUsageRow> AggregateConfigRows(IEnumerable<FolderUsageRow> rows)
        {
            if (rows == null)
                return new List<FolderUsageRow>();

            return rows
                .GroupBy(r => new { OrgUnit = (r.OrgUnitName ?? string.Empty).Trim(), r.ConfidentialityLevel })
                .Select(g =>
                {
                    var ordered = g.OrderBy(r => r.FolderID).ToList();
                    var quotaRows = ordered.Where(r => r.HasQuota)
                        .GroupBy(r => r.OrgUnitID)
                        .Select(x => x.First())
                        .ToList();
                    long quotaBytes = quotaRows.Sum(r => r.QuotaBytes);
                    long orgUsedBytes = quotaRows.Sum(r => Math.Max(0, r.QuotaBytes - r.RemainingBytes));
                    long remainingBytes = quotaRows.Sum(r => r.RemainingBytes);
                    long totalSize = ordered.Sum(r => r.TotalSizeBytes);

                    return new FolderUsageRow
                    {
                        OrgUnitID = ordered[0].OrgUnitID,
                        OrgUnitName = string.IsNullOrWhiteSpace(g.Key.OrgUnit) ? ordered[0].OrgUnitName : g.Key.OrgUnit,
                        FolderID = ordered[0].FolderID,
                        FolderName = ConfigFolderName(g.Key.ConfidentialityLevel),
                        ConfidentialityLevel = g.Key.ConfidentialityLevel,
                        IsArchived = ordered.All(r => r.IsArchived || r.IsDeleted) && ordered.Any(r => r.IsArchived),
                        IsDeleted = ordered.All(r => r.IsDeleted),
                        SubFolderCount = ordered.Sum(r => r.SubFolderCount),
                        FileCount = ordered.Sum(r => r.FileCount),
                        DirectSizeBytes = ordered.Sum(r => r.DirectSizeBytes),
                        TotalSizeBytes = totalSize,
                        QuotaBytes = quotaBytes,
                        RemainingBytes = remainingBytes,
                        FolderPercentOfQuota = quotaBytes > 0 ? (double)totalSize / quotaBytes * 100.0 : 0,
                        OrgUsedPercent = quotaBytes > 0 ? (double)orgUsedBytes / quotaBytes * 100.0 : 0,
                        Alert = WorstAlert(ordered.Select(r => r.Alert)),
                        HasQuota = quotaBytes > 0,
                        UpdatedDate = ordered.Max(r => r.UpdatedDate)
                    };
                })
                .OrderBy(r => r.OrgUnitName)
                .ThenBy(r => r.ConfidentialityLevel)
                .ToList();
        }

        private static QuotaAlertLevel WorstAlert(IEnumerable<QuotaAlertLevel> alerts)
        {
            if (alerts == null)
                return QuotaAlertLevel.Normal;
            if (alerts.Contains(QuotaAlertLevel.Full))
                return QuotaAlertLevel.Full;
            if (alerts.Contains(QuotaAlertLevel.Critical))
                return QuotaAlertLevel.Critical;
            if (alerts.Contains(QuotaAlertLevel.Warning))
                return QuotaAlertLevel.Warning;
            return QuotaAlertLevel.Normal;
        }

        private static long SumFolderSize(int folderId, Dictionary<int, List<Folder>> childrenByParent, Dictionary<int, long> memo, HashSet<int> visited)
        {
            long cached;
            if (memo.TryGetValue(folderId, out cached))
                return cached;
            if (!visited.Add(folderId))
                return 0;

            long total = 0;
            List<Folder> children;
            if (childrenByParent.TryGetValue(folderId, out children))
            {
                foreach (var child in children)
                    total += Math.Max(0, child.FolderSizeBytes) + SumFolderSize(child.FolderID, childrenByParent, memo, visited);
            }

            visited.Remove(folderId);
            memo[folderId] = total;
            return total;
        }

        private static int CountSubFolders(int folderId, Dictionary<int, List<Folder>> childrenByParent, Dictionary<int, int> memo, HashSet<int> visited)
        {
            int cached;
            if (memo.TryGetValue(folderId, out cached))
                return cached;
            if (!visited.Add(folderId))
                return 0;

            int total = 0;
            List<Folder> children;
            if (childrenByParent.TryGetValue(folderId, out children))
            {
                total += children.Count;
                foreach (var child in children)
                    total += CountSubFolders(child.FolderID, childrenByParent, memo, visited);
            }

            visited.Remove(folderId);
            memo[folderId] = total;
            return total;
        }

        private void ApplyFilters()
        {
            if (_grid == null || _txtSearch == null || _cmbOrgUnit == null || _cmbStatus == null || _cmbUsageMode == null || _cmbFolderState == null || _nudMinSizeMb == null)
                return;

            IEnumerable<FolderUsageRow> query = _rows;
            string search = (_txtSearch.Text ?? string.Empty).Trim();
            if (!string.IsNullOrEmpty(search) && search != "ค้นหา Folder / หน่วยงาน")
            {
                query = query.Where(r =>
                    (r.FolderName ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0 ||
                    (r.OrgUnitName ?? string.Empty).IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);
            }

            var orgUnitIds = SelectedOrgUnitIds();
            if (orgUnitIds.Count > 0)
                query = query.Where(r => orgUnitIds.Contains(r.OrgUnitID));

            decimal minBytes = _nudMinSizeMb.Value * 1024m * 1024m;
            if (minBytes > 0)
                query = query.Where(r => r.TotalSizeBytes >= (long)minBytes);

            string status = _cmbStatus.SelectedItem as string;
            if (!string.IsNullOrEmpty(status) && status != "ทุกสถานะ")
                query = query.Where(r => StatusText(r) == status);

            string usageMode = _cmbUsageMode.SelectedItem as string;
            if (usageMode == "ใช้งานจริง")
                query = query.Where(r => r.IsActiveUse);
            else if (usageMode == "ไม่มีการใช้งาน")
                query = query.Where(r => !r.IsActiveUse);

            string folderState = _cmbFolderState.SelectedItem as string;
            if (!string.IsNullOrEmpty(folderState) && folderState != "ทุกสถานะ Folder")
                query = query.Where(r => FolderStateText(r) == folderState);

            var filtered = query.OrderByDescending(r => r.TotalSizeBytes).ThenBy(r => r.FolderName).ToList();
            _filteredRows = filtered;
            PopulateGrid(filtered);
            PopulateUsageTree(filtered);
            UpdateKpis(filtered);
        }

        private void ApplyTreeSelection(TreeNode node)
        {
            if (node == null || _filteredRows == null)
                return;

            var selection = node.Tag as TreeSelection;
            if (selection == null)
                return;

            IEnumerable<FolderUsageRow> query = _filteredRows;
            if (selection.OrgUnitID.HasValue)
                query = query.Where(r => r.OrgUnitID == selection.OrgUnitID.Value);
            if (selection.ConfidentialityLevel.HasValue)
                query = query.Where(r => r.ConfidentialityLevel == selection.ConfidentialityLevel.Value);

            var selectedRows = query.OrderByDescending(r => r.TotalSizeBytes).ThenBy(r => r.FolderName).ToList();
            PopulateGrid(selectedRows);
            _lblStatus.Text = string.Format("แสดง {0:N0} รายการจากที่เลือก: {1}", selectedRows.Count, node.Text);
        }

        private List<int> SelectedOrgUnitIds()
        {
            var item = _cmbOrgUnit.SelectedItem as OrgFilterItem;
            if (item == null || item.OrgUnitIDs == null)
                return new List<int>();
            return item.OrgUnitIDs;
        }

        private void PopulateGrid(IList<FolderUsageRow> rows)
        {
            _grid.Rows.Clear();
            foreach (var row in rows)
            {
                int index = _grid.Rows.Add(
                    row.OrgUnitName,
                    row.FolderName,
                    LevelText(row.ConfidentialityLevel),
                    UsageModeText(row),
                    FolderStateText(row),
                    row.SubFolderCount.ToString("N0"),
                    row.FileCount.ToString("N0"),
                    FormatBytes(row.DirectSizeBytes),
                    FormatBytes(row.TotalSizeBytes),
                    row.HasQuota ? FormatBytes(row.QuotaBytes) : "—",
                    row.HasQuota ? FormatBytes(row.QuotaBytes) : "—",
                    row.HasQuota ? FormatBytes(row.RemainingBytes) : "—",
                    row.HasQuota ? row.FolderPercentOfQuota.ToString("N1") + "%" : "—",
                    row.HasQuota ? row.OrgUsedPercent.ToString("N1") + "%" : "—",
                    StatusText(row),
                    row.UpdatedDate.ToString("dd/MM/yyyy HH:mm"));
                _grid.Rows[index].Tag = row;
                ApplyRowColor(_grid.Rows[index], row);
            }
            _lblStatus.Text = string.Format("แสดง {0:N0} รายการ จากทั้งหมด {1:N0} Folder", rows.Count, _rows.Count);
        }

        private void PopulateUsageTree(IList<FolderUsageRow> rows)
        {
            if (_treeUsage == null)
                return;

            _treeUsage.BeginUpdate();
            try
            {
                _treeUsage.Nodes.Clear();
                var sourceRows = (rows ?? new List<FolderUsageRow>())
                    .Where(r => r != null && r.IsActiveUse)
                    .ToList();
                var activeOrgIds = new HashSet<int>(sourceRows.Select(r => r.OrgUnitID));
                var rowMap = sourceRows
                    .GroupBy(r => new { r.OrgUnitID, r.ConfidentialityLevel })
                    .ToDictionary(g => g.Key, g => g.First());

                var selectedIds = SelectedOrgUnitIds();
                var units = _orgUnits
                    .Where(o => o != null)
                    .Where(o => selectedIds.Count == 0 || selectedIds.Contains(o.OrgUnitID))
                    .Where(o => activeOrgIds.Contains(o.OrgUnitID))
                    .GroupBy(o => o.OrgUnitID)
                    .Select(g => g.First())
                    .ToList();

                if (units.Count == 0)
                {
                    foreach (var row in sourceRows.OrderBy(r => r.OrgUnitName).ThenBy(r => r.ConfidentialityLevel))
                        AddBucketNode(_treeUsage.Nodes, row);
                    return;
                }

                var nodes = units.ToDictionary(o => o.OrgUnitID, o => CreateOrgNode(o, sourceRows));
                foreach (var unit in units.OrderBy(o => o.SortOrder).ThenBy(o => o.Name))
                {
                    TreeNode node;
                    if (!nodes.TryGetValue(unit.OrgUnitID, out node))
                        continue;

                    if (unit.ParentOrgUnitID.HasValue && nodes.ContainsKey(unit.ParentOrgUnitID.Value))
                        nodes[unit.ParentOrgUnitID.Value].Nodes.Add(node);
                    else
                        _treeUsage.Nodes.Add(node);
                }

                foreach (var unit in units)
                {
                    TreeNode node;
                    if (!nodes.TryGetValue(unit.OrgUnitID, out node))
                        continue;

                    for (int level = 1; level <= 4; level++)
                    {
                        FolderUsageRow row;
                        if (rowMap.TryGetValue(new { OrgUnitID = unit.OrgUnitID, ConfidentialityLevel = level }, out row))
                            AddBucketNode(node.Nodes, row);
                    }
                }

                foreach (TreeNode root in _treeUsage.Nodes)
                    root.Expand();
            }
            finally
            {
                _treeUsage.EndUpdate();
            }
        }

        private static TreeNode CreateOrgNode(OrgUnit unit, IList<FolderUsageRow> rows)
        {
            var count = rows.Count(r => r.OrgUnitID == unit.OrgUnitID && r.IsActiveUse);
            var used = rows.Where(r => r.OrgUnitID == unit.OrgUnitID).Sum(r => r.TotalSizeBytes);
            var name = !string.IsNullOrWhiteSpace(unit.Name) ? unit.Name : ("OrgUnit #" + unit.OrgUnitID);
            var node = new TreeNode(string.Format("{0} ({1:N0} ใช้งาน, {2})", name, count, FormatBytes(used)));
            node.Tag = new TreeSelection { OrgUnitID = unit.OrgUnitID };
            node.ToolTipText = string.Format("OrgUnitID: {0}", unit.OrgUnitID);
            return node;
        }

        private static void AddBucketNode(TreeNodeCollection nodes, FolderUsageRow row)
        {
            var text = string.Format("{0}: {1} | {2:N0} ไฟล์ | {3}",
                LevelText(row.ConfidentialityLevel),
                UsageModeText(row),
                row.FileCount,
                FormatBytes(row.TotalSizeBytes));
            var node = new TreeNode(text);
            node.Tag = new TreeSelection { OrgUnitID = row.OrgUnitID, ConfidentialityLevel = row.ConfidentialityLevel };
            node.ForeColor = row.IsActiveUse ? Color.DarkGreen : Color.Gray;
            node.ToolTipText = string.Format("{0} | {1} | Quota: {2} | คงเหลือ: {3}",
                row.FolderName,
                FolderStateText(row),
                row.HasQuota ? FormatBytes(row.QuotaBytes) : "-",
                row.HasQuota ? FormatBytes(row.RemainingBytes) : "-");
            nodes.Add(node);
        }

        private static TreeNode CreateMissingBucketNode(int level)
        {
            var node = new TreeNode(string.Format("{0}: ยังไม่มี Folder", LevelText(level)));
            node.ForeColor = Color.Gray;
            return node;
        }

        private static void ApplyRowColor(DataGridViewRow gridRow, FolderUsageRow row)
        {
            Color color = Color.Black;
            if (row.IsDeleted)
                color = Color.DarkGray;
            else if (row.IsArchived)
                color = Color.Gray;
            else if (!row.HasQuota)
                color = Color.Gray;
            else if (row.Alert == QuotaAlertLevel.Full)
                color = Color.DarkRed;
            else if (row.Alert == QuotaAlertLevel.Critical)
                color = Color.OrangeRed;
            else if (row.Alert == QuotaAlertLevel.Warning)
                color = Color.FromArgb(180, 100, 0);

            gridRow.DefaultCellStyle.ForeColor = color;
            if (row.IsDeleted || row.IsArchived)
                gridRow.DefaultCellStyle.BackColor = Color.FromArgb(248, 248, 248);
        }

        private void UpdateKpis(IList<FolderUsageRow> rows)
        {
            long totalUsed = rows.Sum(r => r.DirectSizeBytes);
            long totalQuota = rows.Where(r => r.HasQuota).GroupBy(r => r.OrgUnitID).Sum(g => g.First().QuotaBytes);
            long totalOrgUsed = rows.Where(r => r.HasQuota).GroupBy(r => r.OrgUnitID).Sum(g => Math.Max(0, g.First().QuotaBytes - g.First().RemainingBytes));
            long remaining = Math.Max(0, totalQuota - totalOrgUsed);
            int warnCount = rows.Count(r => r.HasQuota && (r.Alert == QuotaAlertLevel.Warning || r.Alert == QuotaAlertLevel.Critical || r.Alert == QuotaAlertLevel.Full));
            int activeUseCount = rows.Count(r => r.IsActiveUse);
            int closedCount = rows.Count(r => r.IsArchived || r.IsDeleted);

            _lblTotalUsed.Text = "Folder ใช้รวม: " + FormatBytes(totalUsed);
            _lblTotalQuota.Text = "Quota รวม: " + (totalQuota > 0 ? FormatBytes(totalQuota) : "—");
            _lblTotalRemaining.Text = "คงเหลือรวม: " + (totalQuota > 0 ? FormatBytes(remaining) : "—");
            _lblWarningCount.Text = "ใกล้เต็ม/เกิน: " + warnCount.ToString("N0");
        }

        private void ExportCsv()
        {
            using (var dialog = new SaveFileDialog())
            {
                dialog.Filter = "CSV files (*.csv)|*.csv|All files (*.*)|*.*";
                dialog.FileName = "folder-usage-report-" + DateTime.Now.ToString("yyyyMMdd-HHmm") + ".csv";
                if (dialog.ShowDialog(this) != DialogResult.OK)
                    return;

                var sb = new StringBuilder();
                sb.AppendLine("OrgUnit,Folder,Level,UsageMode,FolderState,SubFolders,Files,DirectSizeBytes,TotalSizeBytes,ConfiguredQuotaBytes,QuotaBytes,RemainingBytes,FolderPercent,OrgPercent,QuotaStatus,UpdatedDate");
                foreach (DataGridViewRow gridRow in _grid.Rows)
                {
                    var row = gridRow.Tag as FolderUsageRow;
                    if (row == null) continue;
                    sb.AppendLine(string.Join(",", new[]
                    {
                        Csv(row.OrgUnitName),
                        Csv(row.FolderName),
                        Csv(LevelText(row.ConfidentialityLevel)),
                        Csv(UsageModeText(row)),
                        Csv(FolderStateText(row)),
                        row.SubFolderCount.ToString(),
                        row.FileCount.ToString(),
                        row.DirectSizeBytes.ToString(),
                        row.TotalSizeBytes.ToString(),
                        row.HasQuota ? row.QuotaBytes.ToString() : string.Empty,
                        row.HasQuota ? row.QuotaBytes.ToString() : string.Empty,
                        row.HasQuota ? row.RemainingBytes.ToString() : string.Empty,
                        row.HasQuota ? row.FolderPercentOfQuota.ToString("F2") : string.Empty,
                        row.HasQuota ? row.OrgUsedPercent.ToString("F2") : string.Empty,
                        Csv(StatusText(row)),
                        Csv(row.UpdatedDate.ToString("yyyy-MM-dd HH:mm"))
                    }));
                }
                File.WriteAllText(dialog.FileName, sb.ToString(), new UTF8Encoding(true));
                MessageBox.Show("Export สำเร็จ", "Folder Usage Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
        }

        private void CopySummary()
        {
            string text = string.Format(
                "Folder Usage Report: แสดง {0:N0} รายการ, {1}, {2}, {3}, {4}",
                _grid.Rows.Count,
                _lblTotalUsed.Text,
                _lblTotalQuota.Text,
                _lblTotalRemaining.Text,
                _lblWarningCount.Text);
            Clipboard.SetText(text);
            MessageBox.Show("คัดลอก summary แล้ว", "Folder Usage Report", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private static string StatusText(FolderUsageRow row)
        {
            if (!row.HasQuota)
                return "ยังไม่กำหนด quota";
            switch (row.Alert)
            {
                case QuotaAlertLevel.Full: return "เกิน quota";
                case QuotaAlertLevel.Critical: return "วิกฤต";
                case QuotaAlertLevel.Warning: return "ใกล้เต็ม";
                default: return "ปกติ";
            }
        }

        private static string UsageModeText(FolderUsageRow row)
        {
            return row.IsActiveUse ? "ใช้งานจริง" : "ไม่มีการใช้งาน";
        }

        private static string FolderStateText(FolderUsageRow row)
        {
            if (row.IsDeleted)
                return "ลบแล้ว";
            if (row.IsArchived)
                return "ปิดใช้งาน";
            return "เปิดใช้งาน";
        }

        private static string LevelText(int level)
        {
            switch (level)
            {
                case 1: return "1HC";
                case 2: return "2SC";
                case 3: return "3NC";
                case 4: return "4PC";
                default: return level.ToString();
            }
        }

        private static string ConfigFolderName(int level)
        {
            switch (level)
            {
                case 1: return "1HC - (High Confidential)";
                case 2: return "2SC - (Semi Confidential)";
                case 3: return "3NC - (Normal Confidential)";
                case 4: return "4PC - (Public Confidential)";
                default: return LevelText(level);
            }
        }

        private static string FormatBytes(long bytes)
        {
            if (bytes >= 1024L * 1024 * 1024)
                return string.Format("{0:N2} GB", bytes / 1073741824.0);
            if (bytes >= 1024L * 1024)
                return string.Format("{0:N1} MB", bytes / 1048576.0);
            if (bytes >= 1024L)
                return string.Format("{0:N1} KB", bytes / 1024.0);
            return string.Format("{0:N0} B", bytes);
        }

        private static string Csv(string value)
        {
            value = value ?? string.Empty;
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }

        private sealed class OrgFilterItem
        {
            public int? OrgUnitID { get; set; }
            public List<int> OrgUnitIDs { get; set; }
            public string Name { get; set; }
            public override string ToString() { return Name ?? string.Empty; }
        }

        private sealed class FolderUsageRow
        {
            public int OrgUnitID { get; set; }
            public string OrgUnitName { get; set; }
            public int FolderID { get; set; }
            public string FolderName { get; set; }
            public int ConfidentialityLevel { get; set; }
            public bool IsArchived { get; set; }
            public bool IsDeleted { get; set; }
            public int SubFolderCount { get; set; }
            public int FileCount { get; set; }
            public long DirectSizeBytes { get; set; }
            public long TotalSizeBytes { get; set; }
            public long QuotaBytes { get; set; }
            public long RemainingBytes { get; set; }
            public double FolderPercentOfQuota { get; set; }
            public double OrgUsedPercent { get; set; }
            public QuotaAlertLevel Alert { get; set; }
            public bool HasQuota { get; set; }
            public DateTime UpdatedDate { get; set; }
            public bool IsActiveUse
            {
                get { return !IsArchived && !IsDeleted && (TotalSizeBytes > 0 || DirectSizeBytes > 0 || FileCount > 0); }
            }
        }

        private sealed class TreeSelection
        {
            public int? OrgUnitID { get; set; }
            public int? ConfidentialityLevel { get; set; }
        }
    }

    internal static class TextBoxPlaceholderCompat
    {
        public static void PlaceholderTextCompat(this TextBox textBox, string placeholder)
        {
            if (textBox == null)
                return;

            textBox.ForeColor = Color.Gray;
            textBox.Text = placeholder;
            textBox.GotFocus += (s, e) =>
            {
                if (textBox.Text == placeholder)
                {
                    textBox.Text = string.Empty;
                    textBox.ForeColor = SystemColors.WindowText;
                }
            };
            textBox.LostFocus += (s, e) =>
            {
                if (string.IsNullOrWhiteSpace(textBox.Text))
                {
                    textBox.Text = placeholder;
                    textBox.ForeColor = Color.Gray;
                }
            };
        }
    }
}
