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
        private readonly IUserAccountLookupController  _userAccountLookupController;
        private readonly IUserAccessOrgLookupController _orgLookupController;

        private List<UserAccount> _userList = new List<UserAccount>();
        private List<OrgUnit>     _orgList  = new List<OrgUnit>();

        // ── Result properties (อ่านได้หลัง DialogResult.OK) ─────────────────
        public int  SelectedPrincipalType { get; private set; }
        public int  SelectedPrincipalId   { get; private set; }
        public bool CanRead        { get; private set; }
        public bool CanWrite       { get; private set; }
        public bool CanDelete      { get; private set; }
        public bool CanModify      { get; private set; }
        public bool CanFullControl { get; private set; }
        public bool IsDeny         { get; private set; }

        public FilePermissionAddDialog(
            IUserAccountLookupController  userAccountLookupController = null,
            IUserAccessOrgLookupController orgLookupController        = null)
        {
            _userAccountLookupController = userAccountLookupController;
            _orgLookupController         = orgLookupController;
            InitializeComponent();
        }

        // ── Load ──────────────────────────────────────────────────────────────

        private void FilePermissionAddDialog_Load(object sender, EventArgs e)
        {
            AddSectionPanels();
            ReloadUsers(true);
        }

        private void AddSectionPanels()
        {
            // White card behind ประเภทผู้ใช้ + เลือกผู้ใช้ section
            var pnlSection = new Panel
            {
                BackColor   = System.Drawing.Color.White,
                BorderStyle = BorderStyle.None,
                Location    = new System.Drawing.Point(8, 42),
                Size        = new System.Drawing.Size(378, 64),
            };
            // Drop card behind existing controls (SendToBack)
            Controls.Add(pnlSection);
            pnlSection.SendToBack();
            // Re-bring header to front
            pnlHeader.BringToFront();

            // Footer separator line above buttons
            var pnlFooterLine = new Panel
            {
                BackColor = System.Drawing.Color.FromArgb(200, 210, 225),
                Location  = new System.Drawing.Point(0, 354),
                Size      = new System.Drawing.Size(394, 1),
            };
            Controls.Add(pnlFooterLine);
            pnlFooterLine.BringToFront();
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
                ReloadOrgUnits();
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
                int idx = cboSelect.SelectedIndex;
                if (idx < 0 || idx >= _orgList.Count)
                {
                    MessageBox.Show("กรุณาเลือกหน่วยงานให้ถูกต้อง", "แจ้งเตือน",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var org = _orgList[idx];
                principalId           = org.OrgUnitID;
                // OrgUnitType 2=Division → PrincipalType 4, OrgUnitType 3=Department → PrincipalType 3
                SelectedPrincipalType = org.OrgUnitType == 2 ? 4 : 3;
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

        private void ReloadOrgUnits()
        {
            cboSelect.Items.Clear();
            cboSelect.Text = "";
            _orgList.Clear();

            if (_orgLookupController == null) return;

            try
            {
                // De-duplicate: same OrgUnitNameTH + OrgUnitType may exist with multiple IDs
                // (DB data issue — same dept linked under multiple parent nodes).
                // Keep lowest OrgUnitID per name+type = original/canonical record.
                var deduped = _orgLookupController.GetHierarchy()
                    .Where(o => o.IsActive && !o.IsDeleted)
                    .GroupBy(o => new { Name = (o.OrgUnitNameTH ?? "").Trim(), o.OrgUnitType })
                    .Select(g => g.OrderBy(o => o.OrgUnitID).First())
                    .OrderBy(o => o.OrgUnitType)   // Division(2) ก่อน Department(3)
                    .ThenBy(o => o.Name)
                    .ToList();

                foreach (var org in deduped)
                {
                    string tier  = org.OrgUnitType == 2 ? "ฝ่าย" : "แผนก";
                    string label = string.Format("[{0}] {1} (ID:{2})", tier, org.Name, org.OrgUnitID);
                    cboSelect.Items.Add(label);
                    _orgList.Add(org);
                }

                if (cboSelect.Items.Count > 0)
                    cboSelect.SelectedIndex = 0;
            }
            catch { }
        }
    }
}
