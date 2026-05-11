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
        private readonly IApprovalInboxController    _approvalInboxController;
        private readonly UserContext                _currentUser;
        private readonly IUserAccountLookupController _userLookup;
        private List<ApprovalRequest>               _pendingRequests = new List<ApprovalRequest>();

        // ── Constructor ──────────────────────────────────────────────────────

        public ApprovalInboxForm(
            IApprovalInboxController approvalInboxController,
            UserContext currentUser,
            IUserAccountLookupController userLookup = null)
        {
            if (approvalInboxController == null) throw new ArgumentNullException("approvalInboxController");
            if (currentUser     == null) throw new ArgumentNullException("currentUser");

            _approvalInboxController = approvalInboxController;
            _currentUser             = currentUser;
            _userLookup              = userLookup;

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
                    item.SubItems.Add(ResolveRequesterName(req.RequestedBy));
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
                {
                    if (_currentUser.AppRole < 3)
                        SetStatus("ไม่มีรายการรออนุมัติ (บัญชีนี้ไม่ใช่ผู้อนุมัติ Manager/Admin)", Color.Gray);
                    else
                        SetStatus("ไม่มีรายการรออนุมัติ", Color.Gray);
                }
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
                SetRemarkError(true);
                MessageBox.Show("กรุณากรอกเหตุผลในการปฏิเสธ", "ข้อมูลไม่ครบ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRemark.Focus();
                return;
            }
            SetRemarkError(false);

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
            int stillPending = 0;
            string remark = txtRemark.Text.Trim();
            if (string.IsNullOrEmpty(remark))
                remark = approve ? "อนุมัติ" : "ปฏิเสธ";

            SetStatus(approve ? "กำลังอนุมัติ..." : "กำลังปฏิเสธ...", Color.RoyalBlue);

            foreach (var req in targets)
            {
                try
                {
                    var steps = _approvalInboxController.GetApprovalSteps(req.RequestID);
                    ApprovalStep step = null;

                    // หา step ที่ assign ให้ user นี้ก่อน
                    step = steps.FirstOrDefault(s => s.ApproverID == _currentUser.UserId && s.StepStatus == 1);

                    // CREATE_HC: Admin อนุมัติได้ทุก step ที่ยัง pending
                    if (step == null && _currentUser.AppRole == 4
                        && (req.RequestType ?? "").ToUpperInvariant() == "CREATE_HC")
                    {
                        step = steps.Where(s => s.StepStatus == 1 || s.StepStatus == 4)
                                    .OrderBy(s => s.StepOrder)
                                    .FirstOrDefault();
                    }

                    // Fallback Manager/Admin: delete-type requests
                    if (step == null && approve && _currentUser.AppRole >= 3
                        && IsManagerAdminFallbackRequestType(req.RequestType))
                    {
                        step = steps.Where(s => s.StepStatus == 1 || s.StepStatus == 4)
                                    .OrderBy(s => s.StepOrder)
                                    .FirstOrDefault();
                    }

                    if (step == null) { failed++; continue; }

                    if (approve)
                    {
                        _approvalInboxController.ApproveStep(step.StepID, remark, _currentUser);
                        var updatedSteps = _approvalInboxController.GetApprovalSteps(req.RequestID).ToList();
                        if (updatedSteps.Any(s => s.StepStatus == 1))
                            stillPending++;
                    }
                    else
                        _approvalInboxController.RejectStep(step.StepID, remark, _currentUser);

                    success++;
                }
                catch { failed++; }
            }

            string resultMsg;
            if (approve)
            {
                resultMsg = stillPending > 0
                    ? string.Format("✅ อนุมัติขั้นตอนแล้ว {0} รายการ", success)
                    : string.Format("✅ อนุมัติแล้ว {0} รายการ", success);
            }
            else
            {
                resultMsg = string.Format("❌ ปฏิเสธแล้ว {0} รายการ", success);
            }

            if (failed > 0)
                resultMsg += string.Format(" (ล้มเหลว {0} รายการ)", failed);

            if (approve && stillPending > 0)
                resultMsg += string.Format(" - ยังรอผู้อนุมัติคนอื่นอีก {0} รายการ", stillPending);

            SetStatus(resultMsg, success > 0 ? (approve ? Color.Green : Color.OrangeRed) : Color.Red);
            txtRemark.Clear();
            SetRemarkError(false);
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

        private void txtRemark_TextChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrWhiteSpace(txtRemark.Text))
                SetRemarkError(false);
        }

        private void SetRemarkError(bool hasError)
        {
            txtRemark.BackColor = hasError
                ? Color.FromArgb(255, 230, 230)
                : SystemColors.Window;
        }

        private void SetStatus(string text, Color color)
        {
            lblStatus.Text      = text;
            lblStatus.ForeColor = color;
        }

        private string ResolveRequesterName(int userId)
        {
            if (_userLookup == null) return string.Format("UserID:{0}", userId);
            try
            {
                var account = _userLookup.GetUserById(userId);
                if (account == null) return string.Format("UserID:{0}", userId);
                return !string.IsNullOrEmpty(account.DisplayName)
                    ? account.DisplayName
                    : account.ADUserName ?? string.Format("UserID:{0}", userId);
            }
            catch
            {
                return string.Format("UserID:{0}", userId);
            }
        }

        private static string RequestTypeLabel(string t)
        {
            switch ((t ?? "").ToUpper())
            {
                case "CREATE_HC":         return "🔐 สร้าง Folder 1HC";
                case "PERMISSION":
                case "CHANGE_PERMISSION": return "🔐 สิทธิ์ Folder";
                case "INTERNAL_SHARE":    return "🔗 แชร์ไฟล์ภายใน";
                case "EXTERNAL_SHARE":    return "🌐 แชร์ภายนอก";
                case "UPLOAD":            return "📤 อัปโหลดไฟล์";
                case "DELETE":
                case "DELETE_FILE":
                case "FILE_DELETE":       return "🗑 ลบไฟล์";
                case "DELETE_FOLDER":     return "🗑 ลบ Folder";
                case "FOLDER":            return "📁 จัดการ Folder";
                default:                  return t ?? "—";
            }
        }

        private static bool IsManagerAdminFallbackRequestType(string requestType)
        {
            switch ((requestType ?? string.Empty).Trim().ToUpperInvariant())
            {
                case "CREATE_HC":
                case "DELETE":
                case "DELETE_FILE":
                case "FILE_DELETE":
                case "DELETE_FOLDER":
                    return true;
                default:
                    return false;
            }
        }
    }
}
