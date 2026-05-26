using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    public partial class FolderQuotaRequestForm : Form
    {
        private readonly IFolderQuotaService  _quotaService;
        private readonly IOrgStructureService _orgService;
        private readonly UserContext          _user;

        private OrgUnit _selectedOrgUnit;

        public FolderQuotaRequestForm(IFolderQuotaService quotaService, IOrgStructureService orgService, UserContext user)
        {
            if (quotaService == null) throw new ArgumentNullException("quotaService");
            if (orgService   == null) throw new ArgumentNullException("orgService");
            if (user         == null) throw new ArgumentNullException("user");

            _quotaService = quotaService;
            _orgService   = orgService;
            _user         = user;

            InitializeComponent();
            LoadOrgTree();
            LoadMyRequests();
        }

        // ── TreeView ──────────────────────────────────────────────────────────
        private void LoadOrgTree()
        {
            var orgMap = _orgService.GetVisibleHierarchy(_user)
                .GroupBy(o => o.OrgUnitID)
                .Select(g => g.First())
                .ToDictionary(o => o.OrgUnitID);

            // ใช้เฉพาะ relationship ที่ตั้งค่าจริง — ไม่ใช้ ParentOrgUnitID fallback
            var rels = _orgService.GetAllActiveRelationships().ToList();

            tvOrgUnits.BeginUpdate();
            tvOrgUnits.Nodes.Clear();

            foreach (var sub in orgMap.Values.Where(o => o.OrgUnitType == 1).OrderBy(o => o.Name))
            {
                var subNode = new TreeNode(sub.Name) { Tag = sub };
                subNode.NodeFont = new Font(tvOrgUnits.Font, FontStyle.Bold);

                // รวม Division จาก 2 แหล่ง แล้วตัดซ้ำด้วย HashSet
                var divIds = new HashSet<int>(
                    rels.Where(r => r.SubCommitteeID == sub.OrgUnitID && orgMap.ContainsKey(r.DivisionID))
                        .Select(r => r.DivisionID));
                foreach (var o in orgMap.Values.Where(o => o.OrgUnitType == 2 && o.ParentOrgUnitID == sub.OrgUnitID))
                    divIds.Add(o.OrgUnitID);

                foreach (var divId in divIds.OrderBy(id => orgMap[id].Name))
                {
                    var div     = orgMap[divId];
                    var divNode = new TreeNode(div.Name) { Tag = div };

                    foreach (var dep in orgMap.Values
                                              .Where(o => o.OrgUnitType == 3 && o.ParentOrgUnitID == div.OrgUnitID)
                                              .OrderBy(o => o.Name))
                        divNode.Nodes.Add(new TreeNode(dep.Name) { Tag = dep });

                    subNode.Nodes.Add(divNode);
                }
                tvOrgUnits.Nodes.Add(subNode);
            }

            tvOrgUnits.ExpandAll();
            tvOrgUnits.EndUpdate();
        }

        private void tvOrgUnits_AfterSelect(object sender, TreeViewEventArgs e)
        {
            _selectedOrgUnit = e.Node != null ? e.Node.Tag as OrgUnit : null;

            if (_selectedOrgUnit == null)
            {
                SetOrgPlaceholder();
                lblCurrentQuota.Text = "";
                return;
            }

            string tier = _selectedOrgUnit.OrgUnitType == 1 ? "Sub"
                        : _selectedOrgUnit.OrgUnitType == 2 ? "Div" : "Dep";
            lblSelectedOrg.Text      = string.Format("[{0}] {1}", tier, _selectedOrgUnit.Name);
            lblSelectedOrg.ForeColor = Color.FromArgb(26, 58, 108);
            lblSelectedOrg.Font      = new Font(lblSelectedOrg.Font.FontFamily, 9, FontStyle.Bold);

            try
            {
                var usage = _quotaService.GetUsage(_selectedOrgUnit.OrgUnitID);
                lblCurrentQuota.Text = string.Format(
                    "Quota ปัจจุบัน: {0:F1} GB   |   ใช้ไป: {1:F1} GB ({2:F1}%)   |   เหลือ: {3:F1} GB",
                    usage.QuotaGB, usage.UsedGB, usage.UsedPercent, usage.FreeGB);

                if (usage.QuotaBytes > 0)
                    nudRequestGB.Minimum = (decimal)Math.Ceiling(usage.QuotaGB) + 1;
                else
                    nudRequestGB.Minimum = 1;
            }
            catch
            {
                lblCurrentQuota.Text = "ไม่พบข้อมูล quota";
            }
        }

        private void SetOrgPlaceholder()
        {
            lblSelectedOrg.Text      = "กรุณาเลือก OrgUnit จากรายการด้านซ้าย";
            lblSelectedOrg.ForeColor = Color.Gray;
            lblSelectedOrg.Font      = new Font(lblSelectedOrg.Font.FontFamily, 9, FontStyle.Italic);
        }

        // ── Grid ──────────────────────────────────────────────────────────────
        private void LoadMyRequests()
        {
            dgvMyRequests.Rows.Clear();
            var requests = _quotaService.GetAllRequests()
                .Where(r => r.RequestedBy == _user.UserID)
                .ToList();

            foreach (var r in requests)
            {
                int row = dgvMyRequests.Rows.Add(
                    r.RequestID,
                    r.OrgUnitName,
                    string.Format("{0:F1} GB", r.CurrentQuotaGB),
                    string.Format("{0:F1} GB", r.RequestedQuotaGB),
                    r.Reason,
                    r.StatusText,
                    r.RequestedDate.ToString("dd/MM/yyyy HH:mm")
                );
                // color-code by status
                var rowObj = dgvMyRequests.Rows[row];
                switch (r.Status)
                {
                    case CMS.Domain.Enums.QuotaRequestStatus.Approved:
                        rowObj.DefaultCellStyle.BackColor = Color.FromArgb(220, 255, 220); break;
                    case CMS.Domain.Enums.QuotaRequestStatus.Rejected:
                        rowObj.DefaultCellStyle.BackColor = Color.FromArgb(255, 220, 220); break;
                    case CMS.Domain.Enums.QuotaRequestStatus.Cancelled:
                        rowObj.DefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240); break;
                }
            }
            lblStatus.Text = string.Format("คำขอของฉัน {0} รายการ", requests.Count);
        }

        // ── Buttons ───────────────────────────────────────────────────────────
        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (_selectedOrgUnit == null)
            {
                MessageBox.Show("กรุณาเลือก OrgUnit จากรายการด้านซ้าย", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtReason.Text))
            {
                MessageBox.Show("กรุณาระบุเหตุผลในการขอเพิ่ม quota", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var dto = new SubmitQuotaChangeRequest
            {
                OrgUnitID           = _selectedOrgUnit.OrgUnitID,
                RequestedQuotaBytes = (long)(nudRequestGB.Value * 1073741824m),
                Reason              = txtReason.Text.Trim()
            };

            try
            {
                int reqId = _quotaService.SubmitChangeRequest(dto, _user);
                MessageBox.Show(
                    string.Format("ส่งคำขอสำเร็จ (Request ID: {0})\nรอผู้ดูแลระบบอนุมัติ", reqId),
                    "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);

                tvOrgUnits.SelectedNode = null;
                _selectedOrgUnit        = null;
                SetOrgPlaceholder();
                lblCurrentQuota.Text = "";
                txtReason.Clear();
                nudRequestGB.Value   = 40;
                nudRequestGB.Minimum = 1;
                LoadMyRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (dgvMyRequests.SelectedRows.Count == 0)
            { MessageBox.Show("กรุณาเลือกคำขอที่ต้องการยกเลิก"); return; }

            int reqId = Convert.ToInt32(dgvMyRequests.SelectedRows[0].Cells[0].Value);

            if (MessageBox.Show("ต้องการยกเลิกคำขอนี้?", "ยืนยัน",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                _quotaService.CancelRequest(reqId, _user);
                LoadMyRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e) { Close(); }
    }
}
