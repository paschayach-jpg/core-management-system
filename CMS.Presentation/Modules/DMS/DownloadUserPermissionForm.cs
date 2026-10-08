using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.DMS
{
    /// <summary>
    /// Admin screen: grant individual users direct download (no approval request) per confidentiality box.
    /// Level/position-based direct download is configured in AuthorityConfigForm (column ⬇).
    /// </summary>
    public partial class DownloadUserPermissionForm : Form
    {
        private readonly IDownloadPolicyController _policyController;
        private readonly IUserAccountLookupController _userLookupController;
        private readonly UserContext _user;

        private List<RowItem> _rows = new List<RowItem>();
        private readonly HashSet<int> _removed = new HashSet<int>();
        private List<UserAccount> _activeUsers = new List<UserAccount>();
        private List<UserAccount> _pickList = new List<UserAccount>();
        private bool _binding;

        private sealed class RowItem
        {
            public int UserID;
            public string Name;
            public string AdUser;
            public string Position;
            public string Level;
            public string Department;
            public bool HC, SC, NC, PC;
            public bool Dirty;
        }

        public DownloadUserPermissionForm(
            IDownloadPolicyController policyController,
            IUserAccountLookupController userLookupController,
            UserContext user)
        {
            if (policyController     == null) throw new ArgumentNullException("policyController");
            if (userLookupController == null) throw new ArgumentNullException("userLookupController");
            if (user                 == null) throw new ArgumentNullException("user");

            _policyController     = policyController;
            _userLookupController = userLookupController;
            _user                 = user;

            InitializeComponent();
            Load += (s, e) => LoadAll();
        }

        // ── Loading ──────────────────────────────────────────────────────────

        private void LoadAll()
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                _removed.Clear();

                _activeUsers = _userLookupController.GetAllUsers()
                    .Where(u => u.IsActive)
                    .OrderBy(u => DisplayNameOf(u))
                    .ToList();
                FillPicker(_activeUsers);
                cboUser.Text = "";

                _rows = _policyController.GetUserPermissions()
                    .Select(p => new RowItem
                    {
                        UserID     = p.UserID,
                        Name       = !string.IsNullOrEmpty(p.DisplayName) ? p.DisplayName : p.ADUserName,
                        AdUser     = p.ADUserName,
                        Position   = p.PositionName,
                        Level      = p.LevelCode,
                        Department = p.DepartmentName,
                        HC = p.CanHC, SC = p.CanSC, NC = p.CanNC, PC = p.CanPC
                    })
                    .ToList();

                BindGrid();
                SetStatus(string.Format("มีผู้ได้รับสิทธิ์ดาวน์โหลดรายคน {0} คน", _rows.Count), Color.FromArgb(31, 56, 100));
            }
            catch (Exception ex)
            {
                SetStatus("โหลดข้อมูลไม่สำเร็จ: " + ex.Message, Color.Red);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void BindGrid()
        {
            string filter = (txtFilter.Text ?? "").Trim();

            _binding = true;
            dgvPermissions.Rows.Clear();
            foreach (var r in _rows)
            {
                if (filter.Length > 0 && !Contains(r.Name, filter) && !Contains(r.AdUser, filter)
                    && !Contains(r.Department, filter) && !Contains(r.Position, filter))
                    continue;

                int idx = dgvPermissions.Rows.Add(r.Name, r.AdUser, r.Position, r.Level, r.Department,
                    r.HC, r.SC, r.NC, r.PC);
                dgvPermissions.Rows[idx].Tag = r;
            }
            _binding = false;
        }

        private void FillPicker(List<UserAccount> users)
        {
            _pickList = users;
            cboUser.BeginUpdate();
            cboUser.Items.Clear();
            foreach (var u in _pickList)
                cboUser.Items.Add(PickerLabel(u));
            cboUser.EndUpdate();
        }

        // ── Event handlers ───────────────────────────────────────────────────

        private void cboUser_TextUpdate(object sender, EventArgs e)
        {
            string query = cboUser.Text ?? "";
            var matches = query.Trim().Length == 0
                ? _activeUsers
                : _activeUsers.Where(u => MatchesUser(u, query.Trim())).ToList();

            FillPicker(matches);

            if (matches.Count > 0 && query.Length > 0)
                cboUser.DroppedDown = true;
            cboUser.Text = query;
            cboUser.SelectionStart = query.Length;
            cboUser.SelectionLength = 0;
            Cursor.Current = Cursors.Default;
        }

        private void cboUser_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Enter) return;
            e.SuppressKeyPress = true;
            btnAdd_Click(sender, EventArgs.Empty);
        }

        private void btnAdd_Click(object sender, EventArgs e)
        {
            int idx = cboUser.SelectedIndex;
            if (idx < 0)
                idx = cboUser.Items.IndexOf(cboUser.Text);
            if (idx < 0 && _pickList.Count == 1)
                idx = 0;
            if (idx < 0 || idx >= _pickList.Count)
            {
                SetStatus(_pickList.Count > 1
                    ? "พบหลายคน — กรุณาเลือกจากรายการ"
                    : "ไม่พบผู้ใช้ — ค้นหาด้วยชื่อ หรือ AD username", Color.OrangeRed);
                cboUser.Focus();
                return;
            }

            var u = _pickList[idx];
            if (_rows.Any(r => r.UserID == u.UserID))
            {
                SetStatus(DisplayNameOf(u) + " อยู่ในรายการแล้ว — ติ๊กระดับที่ต้องการในตาราง", Color.OrangeRed);
                return;
            }

            _removed.Remove(u.UserID);
            _rows.Insert(0, new RowItem
            {
                UserID     = u.UserID,
                Name       = DisplayNameOf(u),
                AdUser     = u.ADUserName,
                Position   = u.PositionName,
                Level      = string.IsNullOrWhiteSpace(u.LevelCode) ? null : u.LevelCode.Trim(),
                Department = u.DepartmentName,
                Dirty      = true
            });

            txtFilter.Text = "";
            BindGrid();
            FillPicker(_activeUsers);
            cboUser.Text = "";
            SetStatus("เพิ่ม " + DisplayNameOf(u) + " แล้ว — ติ๊ก HC/SC/NC/PC แล้วกดบันทึก", Color.DarkOrange);
        }

        private void dgvPermissions_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // commit checkbox clicks immediately so CellValueChanged fires
            if (dgvPermissions.IsCurrentCellDirty)
                dgvPermissions.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void dgvPermissions_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_binding || e.RowIndex < 0) return;
            var r = dgvPermissions.Rows[e.RowIndex].Tag as RowItem;
            if (r == null) return;

            bool value = dgvPermissions.Rows[e.RowIndex].Cells[e.ColumnIndex].Value is bool
                && (bool)dgvPermissions.Rows[e.RowIndex].Cells[e.ColumnIndex].Value;

            if (e.ColumnIndex == colHC.Index) r.HC = value;
            else if (e.ColumnIndex == colSC.Index) r.SC = value;
            else if (e.ColumnIndex == colNC.Index) r.NC = value;
            else if (e.ColumnIndex == colPC.Index) r.PC = value;
            else return;

            r.Dirty = true;
            SetStatus("มีการแก้ไขที่ยังไม่บันทึก — กด 💾 บันทึก", Color.DarkOrange);
        }

        private void btnRemove_Click(object sender, EventArgs e)
        {
            var r = dgvPermissions.CurrentRow != null ? dgvPermissions.CurrentRow.Tag as RowItem : null;
            if (r == null) return;

            _rows.Remove(r);
            _removed.Add(r.UserID);
            BindGrid();
            SetStatus("ลบ " + r.Name + " ออกจากรายการแล้ว — กด 💾 บันทึก เพื่อยืนยัน", Color.DarkOrange);
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            dgvPermissions.EndEdit();
            try
            {
                Cursor = Cursors.WaitCursor;
                int saved = 0, removed = 0;

                foreach (int userId in _removed)
                {
                    _policyController.RemoveUserPermission(userId, _user);
                    removed++;
                }

                foreach (var r in _rows.Where(x => x.Dirty))
                {
                    // ไม่ติ๊กเลย → service ลบสิทธิ์รายคนนั้น
                    _policyController.SaveUserPermission(new DownloadUserPermission
                    {
                        UserID = r.UserID, CanHC = r.HC, CanSC = r.SC, CanNC = r.NC, CanPC = r.PC
                    }, _user);
                    saved++;
                }

                LoadAll();
                SetStatus(string.Format("บันทึกเรียบร้อย (แก้ไข {0} คน, ลบ {1} คน)", saved, removed), Color.Green);
            }
            catch (Exception ex)
            {
                SetStatus("บันทึกไม่สำเร็จ: " + ex.Message, Color.Red);
                MessageBox.Show("บันทึกไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (!ConfirmDiscardChanges()) return;
            LoadAll();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void txtFilter_TextChanged(object sender, EventArgs e)
        {
            BindGrid();
        }

        protected override void OnFormClosing(FormClosingEventArgs e)
        {
            if (e.CloseReason == CloseReason.UserClosing && !ConfirmDiscardChanges())
                e.Cancel = true;
            base.OnFormClosing(e);
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private bool HasChanges()
        {
            return _removed.Count > 0 || _rows.Any(r => r.Dirty);
        }

        private bool ConfirmDiscardChanges()
        {
            if (!HasChanges()) return true;
            bool discard = MessageBox.Show("มีการแก้ไขที่ยังไม่บันทึก ต้องการยกเลิกการแก้ไขหรือไม่?",
                "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
            if (discard)
            {
                _removed.Clear();
                foreach (var r in _rows) r.Dirty = false;
            }
            return discard;
        }

        private static bool MatchesUser(UserAccount u, string query)
        {
            return CMS.Presentation.Helpers.UserDisplayHelper.Matches(u, query);
        }

        private static bool Contains(string value, string query)
        {
            return !string.IsNullOrEmpty(value) && value.IndexOf(query, StringComparison.OrdinalIgnoreCase) >= 0;
        }

        private static string PickerLabel(UserAccount u)
        {
            string level = string.IsNullOrWhiteSpace(u.LevelCode) ? "-" : u.LevelCode.Trim();
            return string.Format("{0}  ({1})  [{2} · {3}]",
                DisplayNameOf(u), u.ADUserName, level, u.DepartmentName ?? "-");
        }

        private static string DisplayNameOf(UserAccount u)
        {
            return !string.IsNullOrEmpty(u.DisplayName) ? u.DisplayName : u.ADUserName;
        }

        private void SetStatus(string text, Color color)
        {
            lblStatus.Text = text;
            lblStatus.ForeColor = color;
        }
    }
}
