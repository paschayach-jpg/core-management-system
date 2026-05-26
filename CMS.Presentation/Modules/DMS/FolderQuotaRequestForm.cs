using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    /// <summary>Form สำหรับผู้ใช้ยื่นคำขอเพิ่ม quota พื้นที่ folder</summary>
    public partial class FolderQuotaRequestForm : Form
    {
        private readonly IFolderQuotaService  _quotaService;
        private readonly IOrgStructureService _orgService;
        private readonly UserContext _user;

        public FolderQuotaRequestForm(IFolderQuotaService quotaService, IOrgStructureService orgService, UserContext user)
        {
            if (quotaService == null) throw new ArgumentNullException("quotaService");
            if (orgService   == null) throw new ArgumentNullException("orgService");
            if (user         == null) throw new ArgumentNullException("user");

            _quotaService = quotaService;
            _orgService   = orgService;
            _user         = user;

            InitializeComponent();
            LoadOrgUnitCombo();
            LoadMyRequests();
        }

        private void LoadOrgUnitCombo()
        {
            var all = _orgService.GetHierarchy().ToList();
            cmbOrgUnit.DataSource    = all;
            cmbOrgUnit.DisplayMember = "Name";
            cmbOrgUnit.ValueMember   = "OrgUnitID";
            cmbOrgUnit.SelectedIndex = -1;
        }

        private void LoadMyRequests()
        {
            dgvMyRequests.Rows.Clear();
            var requests = _quotaService.GetAllRequests()
                .Where(r => r.RequestedBy == _user.UserID)
                .ToList();

            foreach (var r in requests)
            {
                dgvMyRequests.Rows.Add(
                    r.RequestID,
                    r.OrgUnitName,
                    string.Format("{0:F1} GB", r.CurrentQuotaGB),
                    string.Format("{0:F1} GB", r.RequestedQuotaGB),
                    r.Reason,
                    r.StatusText,
                    r.RequestedDate.ToString("dd/MM/yyyy HH:mm")
                );
            }
            lblStatus.Text = string.Format("คำขอของฉัน {0} รายการ", requests.Count);
        }

        private void cmbOrgUnit_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cmbOrgUnit.SelectedValue == null) { lblCurrentQuota.Text = ""; return; }
            int orgId = (int)cmbOrgUnit.SelectedValue;

            var usage = _quotaService.GetUsage(orgId);
            lblCurrentQuota.Text = string.Format("ปัจจุบัน: {0:F1} GB  ใช้ไป: {1:F1} GB ({2:F1}%)",
                usage.QuotaGB, usage.UsedGB, usage.UsedPercent);

            if (usage.QuotaBytes > 0)
                nudRequestGB.Minimum = (decimal)Math.Ceiling(usage.QuotaGB) + 1;
        }

        private void btnSubmit_Click(object sender, EventArgs e)
        {
            if (cmbOrgUnit.SelectedValue == null)
            { MessageBox.Show("กรุณาเลือก OrgUnit", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (string.IsNullOrWhiteSpace(txtReason.Text))
            { MessageBox.Show("กรุณาระบุเหตุผลในการขอเพิ่ม quota", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            var dto = new SubmitQuotaChangeRequest
            {
                OrgUnitID           = (int)cmbOrgUnit.SelectedValue,
                RequestedQuotaBytes = (long)(nudRequestGB.Value * 1073741824m),
                Reason              = txtReason.Text.Trim()
            };

            try
            {
                int reqId = _quotaService.SubmitChangeRequest(dto, _user);
                MessageBox.Show(string.Format("ส่งคำขอสำเร็จ (Request ID: {0})\nรอผู้ดูแลระบบอนุมัติ", reqId),
                    "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                cmbOrgUnit.SelectedIndex = -1;
                txtReason.Clear();
                nudRequestGB.Value = 40;
                lblCurrentQuota.Text = "";
                LoadMyRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            if (dgvMyRequests.SelectedRows.Count == 0) { MessageBox.Show("กรุณาเลือกคำขอที่ต้องการยกเลิก"); return; }
            int reqId = Convert.ToInt32(dgvMyRequests.SelectedRows[0].Cells[0].Value);

            if (MessageBox.Show("ต้องการยกเลิกคำขอนี้?", "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                _quotaService.CancelRequest(reqId, _user);
                LoadMyRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnClose_Click(object sender, EventArgs e) { Close(); }
    }
}
