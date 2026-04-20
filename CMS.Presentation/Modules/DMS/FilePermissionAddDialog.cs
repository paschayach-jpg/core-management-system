using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    /// <summary>
    /// Dialog สำหรับเพิ่ม/แก้ไข สิทธิ์ไฟล์ (แยกออกจาก FilePermissionForm)
    /// </summary>
    public partial class FilePermissionAddDialog : Form
    {
        private readonly IUserAccountLookupController _userAccountLookupController;
        private List<UserAccount> _userList = new List<UserAccount>();

        // ── Result properties (อ่านได้หลัง DialogResult.OK) ─────────────────
        public int  SelectedPrincipalType { get; private set; }
        public int  SelectedPrincipalId   { get; private set; }
        public bool CanRead        { get; private set; }
        public bool CanWrite       { get; private set; }
        public bool CanDelete      { get; private set; }
        public bool CanModify      { get; private set; }
        public bool CanFullControl { get; private set; }
        public bool IsDeny         { get; private set; }

        public FilePermissionAddDialog(IUserAccountLookupController userAccountLookupController = null)
        {
            _userAccountLookupController = userAccountLookupController;
            InitializeComponent();
        }

        // ── Load ──────────────────────────────────────────────────────────────

        private void FilePermissionAddDialog_Load(object sender, EventArgs e)
        {
            ReloadUsers(true);
        }

        // ── Radio button handlers ─────────────────────────────────────────────

        private void rdUser_CheckedChanged(object sender, EventArgs e)
        {
            if (rdUser.Checked)
            {
                lblSelect.Text = "เลือกผู้ใช้:";
                ReloadUsers(true);
            }
        }

        private void rdGroup_CheckedChanged(object sender, EventArgs e)
        {
            if (rdGroup.Checked)
            {
                lblSelect.Text = "เลือกหน่วยงาน:";
                ReloadUsers(false);
            }
        }

        private void rdRole_CheckedChanged(object sender, EventArgs e)
        {
            if (rdRole.Checked)
            {
                lblSelect.Text = "เลือก Role:";
                ReloadUsers(false);
            }
        }

        // ── OK ────────────────────────────────────────────────────────────────

        private void btnOk_Click(object sender, EventArgs e)
        {
            int principalId = 0;

            if (rdUser.Checked)
            {
                SelectedPrincipalType = 1;
                string txt   = cboSelect.Text;
                int    idxId = txt.LastIndexOf("(ID:");
                if (idxId >= 0)
                {
                    string idStr = txt.Substring(idxId + 4).TrimEnd(')');
                    int.TryParse(idStr, out principalId);
                }
                else
                {
                    int.TryParse(txt, out principalId);
                }
            }
            else if (rdGroup.Checked)
            {
                SelectedPrincipalType = 4;
                int.TryParse(cboSelect.Text, out principalId);
            }
            else
            {
                SelectedPrincipalType = 2;
                int.TryParse(cboSelect.Text, out principalId);
            }

            SelectedPrincipalId = principalId;

            if (SelectedPrincipalId <= 0)
            {
                MessageBox.Show("กรุณาระบุ Principal ให้ถูกต้อง", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            CanRead        = rdAllowRead.Checked;
            CanWrite       = rdAllowWrite.Checked;
            CanDelete      = rdAllowDelete.Checked;
            CanModify      = rdAllowModify.Checked;
            CanFullControl = rdAllowFull.Checked;
            IsDeny         = rdDenyRead.Checked  || rdDenyWrite.Checked  || rdDenyDelete.Checked
                          || rdDenyModify.Checked || rdDenyFull.Checked;

            DialogResult = DialogResult.OK;
        }

        // ── Helper ────────────────────────────────────────────────────────────

        private void ReloadUsers(bool isUser)
        {
            cboSelect.Items.Clear();
            cboSelect.Text = "";

            if (!isUser || _userAccountLookupController == null) return;

            try
            {
                _userList = (_userAccountLookupController?.GetAllUsers() ?? Enumerable.Empty<UserAccount>()).ToList();
                foreach (var u in _userList.OrderBy(x => x.ADUserName))
                    cboSelect.Items.Add(string.Format("{0} — {1} (ID:{2})", u.ADUserName, u.DisplayName, u.UserID));
                if (cboSelect.Items.Count > 0)
                    cboSelect.SelectedIndex = 0;
            }
            catch { }
        }
    }
}
