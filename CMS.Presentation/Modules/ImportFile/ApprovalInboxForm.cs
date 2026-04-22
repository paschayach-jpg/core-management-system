using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    public partial class ApprovalInboxForm : Form
    {
        private readonly IApprovalInboxController _approvalInboxController;
        private readonly UserContext      _currentUser;
        private List<ApprovalRequest>     _pendingRequests = new List<ApprovalRequest>();

        // ── Constructor ──────────────────────────────────────────────────────

        public ApprovalInboxForm(IApprovalInboxController approvalInboxController, UserContext currentUser)
        {
            if (approvalInboxController == null) throw new ArgumentNullException("approvalInboxController");
            if (currentUser     == null) throw new ArgumentNullException("currentUser");

            _approvalInboxController = approvalInboxController;
            _currentUser     = currentUser;

            InitializeComponent();
            LoadPendingRequests();
        }

        // ── Data loading ─────────────────────────────────────────────────────

        private void LoadPendingRequests()
        {
            try
            {
                SetStatus("กำลังโหลดรายการรออนุมัติ...", Color.RoyalBlue);

                _pendingRequests = _approvalInboxController
                    .GetPendingForApprover(_currentUser.UserId)
                    .ToList();

                lvRequests.Items.Clear();

                foreach (var req in _pendingRequests)
                {
                    var item = new ListViewItem(req.RequestID.ToString());
                    item.SubItems.Add(RequestTypeLabel(req.RequestType));
                    item.SubItems.Add(req.Subject ?? "—");
                    item.SubItems.Add("—");   // requester name — wire to UserAccount if needed
                    item.SubItems.Add(req.RequestedDate.ToString("dd/MM/yy HH:mm"));
                    item.SubItems.Add(req.ExpiryDate.HasValue
                        ? req.ExpiryDate.Value.ToString("dd/MM/yy")
                        : "—");
                    item.Tag = req;

                    // Highlight rows expiring within 24 h
                    if (req.ExpiryDate.HasValue && req.ExpiryDate.Value <= DateTime.Now.AddDays(1))
                    {
                        item.BackColor = Color.FromArgb(255, 235, 235);
                        item.ForeColor = Color.DarkRed;
                    }

                    lvRequests.Items.Add(item);
                }

                if (_pendingRequests.Count > 0)
                    SetStatus(string.Format("พบรายการรออนุมัติ {0} รายการ", _pendingRequests.Count), Color.Green);
                else
                    SetStatus("ไม่มีรายการรออนุมัติ", Color.Gray);
            }
            catch (Exception ex)
            {
                SetStatus("โหลดข้อมูลไม่สำเร็จ: " + ex.Message, Color.Red);
            }
        }

        // ── Event handlers ───────────────────────────────────────────────────

        private void lvRequests_SelectedIndexChanged(object sender, EventArgs e)
        {
            UpdateButtonState();
        }

        private void lvRequests_ItemChecked(object sender, ItemCheckedEventArgs e)
        {
            UpdateButtonState();
            UpdateSelCountLabel();
            // sync chkSelectAll state without re-triggering its event
            chkSelectAll.CheckedChanged -= chkSelectAll_CheckedChanged;
            int checkedCount = lvRequests.CheckedItems.Count;
            chkSelectAll.Checked = checkedCount > 0 && checkedCount == lvRequests.Items.Count;
            chkSelectAll.CheckedChanged += chkSelectAll_CheckedChanged;
        }

        private void chkSelectAll_CheckedChanged(object sender, EventArgs e)
        {
            foreach (ListViewItem item in lvRequests.Items)
                item.Checked = chkSelectAll.Checked;
        }

        private void UpdateButtonState()
        {
            int count = lvRequests.CheckedItems.Count;
            if (count == 0) count = lvRequests.SelectedItems.Count;
            btnApprove.Enabled = count > 0;
            btnReject.Enabled  = count > 0;
        }

        private void UpdateSelCountLabel()
        {
            int count = lvRequests.CheckedItems.Count;
            lblSelCount.Text = string.Format("เลือกแล้ว {0} รายการ", count);
            lblSelCount.ForeColor = count > 0
                ? System.Drawing.Color.FromArgb(0, 100, 0)
                : System.Drawing.Color.FromArgb(0, 51, 102);
        }

        private void btnApprove_Click(object sender, EventArgs e)
        {
            var targets = GetTargetRequests();
            if (targets.Count == 0) return;

            string msg = targets.Count == 1
                ? string.Format("ยืนยันการอนุมัติคำขอ:\n\n{0}\n{1}",
                    RequestTypeLabel(targets[0].RequestType), targets[0].Subject)
                : string.Format("ยืนยันการอนุมัติ {0} รายการที่เลือก?", targets.Count);

            if (MessageBox.Show(msg, "ยืนยันการอนุมัติ",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ProcessBatch(targets, approve: true);
            }
        }

        private void btnReject_Click(object sender, EventArgs e)
        {
            var targets = GetTargetRequests();
            if (targets.Count == 0) return;

            if (string.IsNullOrWhiteSpace(txtRemark.Text))
            {
                MessageBox.Show("กรุณากรอกเหตุผลในการปฏิเสธ", "ข้อมูลไม่ครบ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRemark.Focus();
                return;
            }

            string msg = targets.Count == 1
                ? string.Format("ยืนยันการปฏิเสธคำขอ:\n\n{0}\n{1}\n\nเหตุผล: {2}",
                    RequestTypeLabel(targets[0].RequestType), targets[0].Subject, txtRemark.Text)
                : string.Format("ยืนยันการปฏิเสธ {0} รายการที่เลือก?\n\nเหตุผล: {1}",
                    targets.Count, txtRemark.Text);

            if (MessageBox.Show(msg, "ยืนยันการปฏิเสธ",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Warning) == DialogResult.Yes)
            {
                ProcessBatch(targets, approve: false);
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadPendingRequests();
        }

        // ── Core logic ───────────────────────────────────────────────────────

        private void ProcessBatch(List<ApprovalRequest> targets, bool approve)
        {
            int success = 0, failed = 0;
            string remark = txtRemark.Text.Trim();
            if (string.IsNullOrEmpty(remark))
                remark = approve ? "อนุมัติ" : "ปฏิเสธ";

            SetStatus(approve ? "กำลังอนุมัติ..." : "กำลังปฏิเสธ...", Color.RoyalBlue);

            foreach (var req in targets)
            {
                try
                {
                    var steps = _approvalInboxController.GetApprovalSteps(req.RequestID);
                    var step  = steps.FirstOrDefault(
                        s => s.ApproverID == _currentUser.UserId && s.StepStatus == 1);

                    if (step == null) { failed++; continue; }

                    if (approve)
                        _approvalInboxController.ApproveStep(step.StepID, remark, _currentUser);
                    else
                        _approvalInboxController.RejectStep(step.StepID, remark, _currentUser);

                    success++;
                }
                catch { failed++; }
            }

            string resultMsg = approve
                ? string.Format("✅ อนุมัติแล้ว {0} รายการ", success)
                : string.Format("❌ ปฏิเสธแล้ว {0} รายการ", success);
            if (failed > 0)
                resultMsg += string.Format(" (ล้มเหลว {0} รายการ)", failed);

            SetStatus(resultMsg, success > 0 ? (approve ? Color.Green : Color.OrangeRed) : Color.Red);
            txtRemark.Clear();
            chkSelectAll.Checked = false;
            LoadPendingRequests();
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        /// <summary>
        /// Returns checked items if any, otherwise falls back to selected items.
        /// </summary>
        private List<ApprovalRequest> GetTargetRequests()
        {
            var checked_ = lvRequests.CheckedItems.Cast<ListViewItem>()
                .Select(i => i.Tag as ApprovalRequest)
                .Where(r => r != null)
                .ToList();
            if (checked_.Count > 0) return checked_;

            // fallback: single selected row
            return lvRequests.SelectedItems.Cast<ListViewItem>()
                .Select(i => i.Tag as ApprovalRequest)
                .Where(r => r != null)
                .ToList();
        }

        private void SetStatus(string text, Color color)
        {
            lblStatus.Text      = text;
            lblStatus.ForeColor = color;
        }

        private static string RequestTypeLabel(string t)
        {
            switch ((t ?? "").ToUpper())
            {
                case "PERMISSION":        return "🔐 สิทธิ์ Folder";
                case "CHANGE_PERMISSION": return "🔐 สิทธิ์ Folder";
                case "UPLOAD":            return "📤 อัพโหลดไฟล์";
                case "DELETE":            return "🗑 ลบไฟล์";
                case "DELETE_FOLDER":     return "🗑 ลบ Folder";
                case "FOLDER":            return "📁 จัดการ Folder";
                default:                  return t ?? "—";
            }
        }
    }
}
