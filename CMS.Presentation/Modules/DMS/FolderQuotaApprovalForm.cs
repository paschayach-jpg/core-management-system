using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    /// <summary>Form สำหรับ Admin อนุมัติ/ปฏิเสธคำขอเปลี่ยน quota</summary>
    public partial class FolderQuotaApprovalForm : Form
    {
        private readonly IFolderQuotaService _quotaService;
        private readonly UserContext _user;

        private List<FolderQuotaChangeRequest> _requests = new List<FolderQuotaChangeRequest>();
        private FolderQuotaChangeRequest _selected;

        public FolderQuotaApprovalForm(IFolderQuotaService quotaService, UserContext user)
        {
            if (quotaService == null) throw new ArgumentNullException("quotaService");
            if (user         == null) throw new ArgumentNullException("user");

            _quotaService = quotaService;
            _user         = user;

            InitializeComponent();
            LoadRequests();
        }

        private void LoadRequests()
        {
            dgvRequests.SelectionChanged -= dgvRequests_SelectionChanged;

            bool pendingOnly = chkPendingOnly.Checked;
            _requests = (pendingOnly
                ? _quotaService.GetPendingRequests()
                : _quotaService.GetAllRequests()).ToList();

            dgvRequests.Rows.Clear();
            foreach (var r in _requests)
            {
                int row = dgvRequests.Rows.Add(
                    r.RequestID,
                    r.OrgUnitName,
                    string.Format("{0:F1} GB", r.CurrentQuotaGB),
                    string.Format("{0:F1} GB", r.RequestedQuotaGB),
                    r.Reason,
                    r.RequestedByName,
                    r.RequestedDate.ToString("dd/MM/yyyy HH:mm"),
                    r.StatusText
                );

                // Color-code rows by status
                var rowObj = dgvRequests.Rows[row];
                switch (r.Status)
                {
                    case QuotaRequestStatus.Approved:  rowObj.DefaultCellStyle.BackColor = Color.FromArgb(225, 255, 225); break;
                    case QuotaRequestStatus.Rejected:  rowObj.DefaultCellStyle.BackColor = Color.FromArgb(255, 225, 225); break;
                    case QuotaRequestStatus.Cancelled: rowObj.DefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240); break;
                }
                rowObj.Tag = r;
            }

            int pending = _requests.Count(r => r.Status == QuotaRequestStatus.Pending);
            lblStatus.Text = string.Format("ทั้งหมด {0} รายการ  รอดำเนินการ: {1}", _requests.Count, pending);
            txtRemark.Clear();

            dgvRequests.SelectionChanged += dgvRequests_SelectionChanged;
            dgvRequests_SelectionChanged(null, EventArgs.Empty);
        }

        private void dgvRequests_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvRequests.SelectedRows.Count == 0) { _selected = null; return; }
            _selected = dgvRequests.SelectedRows[0].Tag as FolderQuotaChangeRequest;

            bool isPending = _selected != null && _selected.Status == QuotaRequestStatus.Pending;
            btnApprove.Enabled = isPending;
            btnReject.Enabled  = isPending;

            if (_selected != null)
            {
                lblDetail.Text = string.Format(
                    "OrgUnit: {0}  |  Quota เดิม: {1:F1} GB  →  ขอใหม่: {2:F1} GB\r\nเหตุผล: {3}",
                    _selected.OrgUnitName, _selected.CurrentQuotaGB,
                    _selected.RequestedQuotaGB, _selected.Reason);
            }
        }

        private void btnApprove_Click(object sender, EventArgs e)
        {
            if (_selected == null) return;
            if (MessageBox.Show(
                string.Format("อนุมัติคำขอเพิ่ม quota ของ {0} เป็น {1:F1} GB?",
                    _selected.OrgUnitName, _selected.RequestedQuotaGB),
                "ยืนยันอนุมัติ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                _quotaService.ApproveRequest(_selected.RequestID, txtRemark.Text.Trim(), _user);
                MessageBox.Show("อนุมัติสำเร็จ quota ถูกอัปเดตทันที", "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnReject_Click(object sender, EventArgs e)
        {
            if (_selected == null) return;
            if (string.IsNullOrWhiteSpace(txtRemark.Text))
            { MessageBox.Show("กรุณาระบุเหตุผลในการปฏิเสธ", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }

            if (MessageBox.Show(
                string.Format("ปฏิเสธคำขอเพิ่ม quota ของ {0}?", _selected.OrgUnitName),
                "ยืนยันปฏิเสธ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning) != DialogResult.Yes) return;

            try
            {
                _quotaService.RejectRequest(_selected.RequestID, txtRemark.Text.Trim(), _user);
                LoadRequests();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void chkPendingOnly_CheckedChanged(object sender, EventArgs e) { LoadRequests(); }
        private void btnRefresh_Click(object sender, EventArgs e)               { LoadRequests(); }
        private void btnClose_Click(object sender, EventArgs e)                 { Close(); }
    }
}
