using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.DMS
{
    /// <summary>
    /// Read-only report: every user of a division/department with the level-based rights
    /// (FolderAuthorityConfig, resolved exactly as at login) they get in each confidentiality box.
    /// </summary>
    public partial class AuthorityByOrgReportForm : Form
    {
        private const string AllItems = "— ทั้งหมด —";
        private const int ManagerRole = 3;

        private static readonly string[] BoxCodes = { "HC", "SC", "NC", "PC" };
        private static readonly Color[] BoxColors =
        {
            Color.FromArgb(255, 228, 228), Color.FromArgb(255, 236, 214),
            Color.FromArgb(222, 236, 255), Color.FromArgb(222, 245, 228)
        };

        private readonly IUserAdminController _userAdminController;
        private readonly IAuthorityConfigController _authorityConfigController;
        private readonly UserContext _user;
        private readonly Action<UserAccount> _openUserDetail;

        private List<ReportRow> _rows = new List<ReportRow>();
        private bool _loadingFilters;
        private Font _boldFont;

        private sealed class ReportRow
        {
            public UserAccount User;
            public Dictionary<int, FolderAuthorityConfig> Authority;
            public string Source;
            public bool IsPrivileged;
            public bool HasConfig { get { return Authority != null && Authority.Count > 0; } }
        }

        public AuthorityByOrgReportForm(
            IUserAdminController userAdminController,
            IAuthorityConfigController authorityConfigController,
            UserContext user,
            Action<UserAccount> openUserDetail = null)
        {
            if (userAdminController == null) throw new ArgumentNullException("userAdminController");
            if (authorityConfigController == null) throw new ArgumentNullException("authorityConfigController");
            if (user == null) throw new ArgumentNullException("user");

            _userAdminController = userAdminController;
            _authorityConfigController = authorityConfigController;
            _user = user;
            _openUserDetail = openUserDetail;

            InitializeComponent();
            SetupColumns();
            this.Load += (s, e) => LoadData();
        }

        private void SetupColumns()
        {
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colAd", HeaderText = "ชื่อผู้ใช้ AD", Width = 150 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colName", HeaderText = "ชื่อ-นามสกุล", Width = 200 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colEmpCode", HeaderText = "รหัสพนักงาน", Width = 90 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDivision", HeaderText = "ฝ่าย", Width = 160 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDepartment", HeaderText = "แผนก", Width = 160 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPosition", HeaderText = "ตำแหน่ง", Width = 160 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLevel", HeaderText = "Level", Width = 55 });
            dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "colSource", HeaderText = "ได้สิทธิ์จาก", Width = 200 });
            for (int i = 0; i < BoxCodes.Length; i++)
                dgvUsers.Columns.Add(new DataGridViewTextBoxColumn { Name = "col" + BoxCodes[i], HeaderText = BoxCodes[i], Width = 150 });
        }

        // ── Data ──────────────────────────────────────────────────────────────

        private void LoadData()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                var users = _userAdminController.GetAllUsers() ?? new List<UserAccount>();

                // Same resolution as login. Users sharing level/position/department/division get the
                // same result, so resolve each combination once instead of once per user.
                var cache = new Dictionary<string, Dictionary<int, FolderAuthorityConfig>>(StringComparer.OrdinalIgnoreCase);
                _rows = new List<ReportRow>();
                foreach (var u in users)
                {
                    string key = string.Join("|", Norm(u.LevelCode), Norm(u.PositionName), Norm(u.DepartmentCode), Norm(u.DivisionCode));
                    Dictionary<int, FolderAuthorityConfig> authority;
                    if (!cache.TryGetValue(key, out authority))
                    {
                        authority = _authorityConfigController.ResolveForUser(u) ?? new Dictionary<int, FolderAuthorityConfig>();
                        cache[key] = authority;
                    }

                    var row = new ReportRow { User = u, Authority = authority, IsPrivileged = u.AppRole >= ManagerRole };
                    row.Source = BuildSource(row);
                    _rows.Add(row);
                }

                PopulateFilters();
                ApplyFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถโหลดข้อมูลสิทธิ์ได้: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private static string BuildSource(ReportRow row)
        {
            string labels = string.Join(", ", row.Authority.Values
                .Select(c => (c.ConfigLabel ?? "").Trim())
                .Where(l => l.Length > 0)
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .ToArray());

            if (row.IsPrivileged)
                return "Admin/Manager (ผ่านทุก folder)" + (labels.Length > 0 ? " + " + labels : "");
            return labels.Length > 0 ? labels : "⚠ ไม่พบ config";
        }

        /// <summary>Rights in one box, using the same rules as "สิทธิ์การเข้าใช้งานของฉัน" (UserContext).</summary>
        private static string BoxText(ReportRow row, int level)
        {
            if (row.IsPrivileged) return "ทั้งหมด";

            FolderAuthorityConfig cfg;
            row.Authority.TryGetValue(level, out cfg);
            if (cfg == null)
                return level == 4 ? "ดู (พื้นฐาน)" : "—";   // UserContext.CanViewFolder: PC view is the default

            var parts = new List<string>();
            if (cfg.CanView) parts.Add("ดู");
            if (cfg.CanAdd) parts.Add("เพิ่ม");
            if (cfg.CanEdit) parts.Add("แก้ไข");
            if (cfg.CanApprove) parts.Add("อนุมัติ");
            if (cfg.CanDownload) parts.Add("⬇ดาวน์โหลด");
            return parts.Count > 0 ? string.Join(" ", parts.ToArray()) : "—";
        }

        // ── Filters ───────────────────────────────────────────────────────────

        private void PopulateFilters()
        {
            _loadingFilters = true;
            try
            {
                string selectedDivision = cboDivision.SelectedItem as string;
                cboDivision.Items.Clear();
                cboDivision.Items.Add(AllItems);
                foreach (var d in _rows.Select(r => DisplayOrg(r.User.DivisionName)).Distinct().OrderBy(x => x))
                    cboDivision.Items.Add(d);
                cboDivision.SelectedItem = selectedDivision != null && cboDivision.Items.Contains(selectedDivision) ? selectedDivision : AllItems;
            }
            finally
            {
                _loadingFilters = false;
            }
            PopulateDepartments();
        }

        private void PopulateDepartments()
        {
            _loadingFilters = true;
            try
            {
                string division = cboDivision.SelectedItem as string;
                string selectedDepartment = cboDepartment.SelectedItem as string;

                cboDepartment.Items.Clear();
                cboDepartment.Items.Add(AllItems);
                foreach (var d in _rows
                    .Where(r => division == null || division == AllItems || DisplayOrg(r.User.DivisionName) == division)
                    .Select(r => DisplayOrg(r.User.DepartmentName)).Distinct().OrderBy(x => x))
                    cboDepartment.Items.Add(d);
                cboDepartment.SelectedItem = selectedDepartment != null && cboDepartment.Items.Contains(selectedDepartment) ? selectedDepartment : AllItems;
            }
            finally
            {
                _loadingFilters = false;
            }
        }

        private void ApplyFilter()
        {
            string division = cboDivision.SelectedItem as string;
            string department = cboDepartment.SelectedItem as string;
            string search = txtSearch.Text.Trim();

            var query = _rows.AsEnumerable();
            if (chkActiveOnly.Checked)
                query = query.Where(r => r.User.IsActive);
            if (division != null && division != AllItems)
                query = query.Where(r => DisplayOrg(r.User.DivisionName) == division);
            if (department != null && department != AllItems)
                query = query.Where(r => DisplayOrg(r.User.DepartmentName) == department);
            if (chkNoAuthorityOnly.Checked)
                query = query.Where(r => !r.HasConfig && !r.IsPrivileged);
            if (search.Length > 0)
                query = query.Where(r => (r.User.ADUserName + " " + r.User.DisplayName + " " + r.User.EmployeeCode + " " + r.User.PositionName)
                                         .IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0);

            var shown = query
                .OrderBy(r => DisplayOrg(r.User.DivisionName))
                .ThenBy(r => DisplayOrg(r.User.DepartmentName))
                .ThenBy(r => r.User.DisplayName ?? r.User.ADUserName)
                .ToList();

            dgvUsers.SuspendLayout();
            dgvUsers.Rows.Clear();
            foreach (var r in shown)
            {
                int i = dgvUsers.Rows.Add(
                    r.User.ADUserName,
                    r.User.DisplayName,
                    r.User.EmployeeCode,
                    r.User.DivisionName,
                    r.User.DepartmentName,
                    r.User.PositionName,
                    Norm(r.User.LevelCode),
                    r.Source,
                    BoxText(r, 1), BoxText(r, 2), BoxText(r, 3), BoxText(r, 4));
                dgvUsers.Rows[i].Tag = r;
            }
            dgvUsers.ResumeLayout();

            int noConfig = shown.Count(r => !r.HasConfig && !r.IsPrivileged);
            lblSummary.Text = string.Format("แสดง {0:N0} คน  •  ได้สิทธิ์ตาม config {1:N0}  •  Admin/Manager {2:N0}  •  ไม่พบ config {3:N0}",
                shown.Count, shown.Count(r => r.HasConfig && !r.IsPrivileged), shown.Count(r => r.IsPrivileged), noConfig);
            lblSummary.ForeColor = noConfig > 0 ? Color.DarkRed : Color.FromArgb(31, 56, 100);
        }

        // ── Events ────────────────────────────────────────────────────────────

        private void cboDivision_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_loadingFilters) return;
            PopulateDepartments();
            ApplyFilter();
        }

        private void Filter_Changed(object sender, EventArgs e)
        {
            if (_loadingFilters) return;
            ApplyFilter();
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadData();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void dgvUsers_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = dgvUsers.Rows[e.RowIndex].Tag as ReportRow;
            if (row == null) return;

            string colName = dgvUsers.Columns[e.ColumnIndex].Name;
            int box = Array.IndexOf(BoxCodes, colName.Length > 3 ? colName.Substring(3) : "");
            if (box >= 0)
            {
                string text = e.Value as string;
                bool hasRight = !string.IsNullOrEmpty(text) && text != "—";
                e.CellStyle.BackColor = hasRight ? BoxColors[box] : e.CellStyle.BackColor;
                e.CellStyle.ForeColor = hasRight ? Color.FromArgb(30, 30, 30) : Color.Gray;
            }
            else if (colName == "colSource" && !row.HasConfig && !row.IsPrivileged)
            {
                e.CellStyle.ForeColor = Color.DarkRed;
                if (_boldFont == null) _boldFont = new Font(dgvUsers.Font, FontStyle.Bold);
                e.CellStyle.Font = _boldFont;
            }
        }

        private void dgvUsers_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || _openUserDetail == null) return;
            var row = dgvUsers.Rows[e.RowIndex].Tag as ReportRow;
            if (row != null)
                _openUserDetail(row.User);
        }

        private void btnExport_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog
            {
                Title = "Export สิทธิ์ตามหน่วยงาน",
                Filter = "CSV files (*.csv)|*.csv",
                FileName = string.Format("authority-by-org-{0:yyyyMMddHHmm}.csv", DateTime.Now)
            })
            {
                if (sfd.ShowDialog(this) != DialogResult.OK) return;

                var sb = new StringBuilder();
                sb.AppendLine(string.Join(",", dgvUsers.Columns.Cast<DataGridViewColumn>().Select(c => Csv(c.HeaderText)).ToArray()));
                foreach (DataGridViewRow r in dgvUsers.Rows)
                    sb.AppendLine(string.Join(",", r.Cells.Cast<DataGridViewCell>().Select(c => Csv(Convert.ToString(c.Value))).ToArray()));

                try
                {
                    File.WriteAllText(sfd.FileName, sb.ToString(), Encoding.UTF8);
                    MessageBox.Show("Export เรียบร้อยแล้ว", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Export ไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────

        private static string Norm(string s)
        {
            return string.IsNullOrWhiteSpace(s) ? "" : s.Trim();
        }

        private static string DisplayOrg(string name)
        {
            return string.IsNullOrWhiteSpace(name) ? "(ไม่ระบุ)" : name.Trim();
        }

        private static string Csv(string value)
        {
            if (value == null) return "";
            return "\"" + value.Replace("\"", "\"\"") + "\"";
        }
    }
}
