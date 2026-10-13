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
        private List<ApprovalRequest>               _visibleRequests = new List<ApprovalRequest>();
        private IDictionary<int, string>            _deptByRequest   = new Dictionary<int, string>();

        private const string AllDeptLabel = "-- ทุกแผนก --";
        private const string NoDeptLabel  = "(ไม่ระบุแผนก)";

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
            lvRequests.DoubleClick += LvRequests_DoubleClick;
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
                _deptByRequest = _approvalInboxController.GetRequestDepartmentNames(_pendingRequests)
                    ?? new Dictionary<int, string>();

                PopulateDeptFilter();   // เลือกแผนก → RenderRequests()
            }
            catch (Exception ex)
            {
                SetStatus("โหลดข้อมูลไม่สำเร็จ: " + ex.Message, Color.Red);
            }
        }

        private string DepartmentOf(ApprovalRequest req)
        {
            string name;
            return _deptByRequest.TryGetValue(req.RequestID, out name) ? name : NoDeptLabel;
        }

        private void PopulateDeptFilter()
        {
            string prevDept = DeptNameFromFilterText(cmbDeptFilter.SelectedItem as string);

            cmbDeptFilter.SelectedIndexChanged -= cmbDeptFilter_SelectedIndexChanged;
            cmbDeptFilter.BeginUpdate();
            cmbDeptFilter.Items.Clear();
            cmbDeptFilter.Items.Add(AllDeptLabel);
            foreach (var dept in _pendingRequests
                .GroupBy(DepartmentOf, StringComparer.OrdinalIgnoreCase)
                .OrderBy(g => g.Key == NoDeptLabel)
                .ThenBy(g => g.Key, StringComparer.CurrentCultureIgnoreCase))
            {
                cmbDeptFilter.Items.Add(string.Format("{0} ({1})", dept.Key, dept.Count()));
            }
            cmbDeptFilter.EndUpdate();

            // คงแผนกที่เลือกไว้ตอนรีเฟรช (จำนวนในวงเล็บอาจเปลี่ยน → เทียบเฉพาะชื่อ)
            int index = 0;
            if (prevDept != null)
            {
                for (int i = 1; i < cmbDeptFilter.Items.Count; i++)
                {
                    if (string.Equals(DeptNameFromFilterText(cmbDeptFilter.Items[i] as string), prevDept,
                            StringComparison.OrdinalIgnoreCase))
                    {
                        index = i;
                        break;
                    }
                }
            }
            cmbDeptFilter.SelectedIndex = index;
            cmbDeptFilter.SelectedIndexChanged += cmbDeptFilter_SelectedIndexChanged;

            RenderRequests();
        }

        /// <summary>"ชื่อแผนก (3)" → "ชื่อแผนก"; "-- ทุกแผนก --" หรือ null → null</summary>
        private static string DeptNameFromFilterText(string text)
        {
            if (string.IsNullOrEmpty(text) || text == AllDeptLabel) return null;
            int paren = text.LastIndexOf(" (", StringComparison.Ordinal);
            return paren > 0 ? text.Substring(0, paren) : text;
        }

        private void cmbDeptFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            RenderRequests();
        }

        private void RenderRequests()
        {
            string deptFilter = DeptNameFromFilterText(cmbDeptFilter.SelectedItem as string);
            _visibleRequests = deptFilter == null
                ? _pendingRequests.ToList()
                : _pendingRequests
                    .Where(r => string.Equals(DepartmentOf(r), deptFilter, StringComparison.OrdinalIgnoreCase))
                    .ToList();

            lvRequests.BeginUpdate();
            try
            {
                lvRequests.Items.Clear();

                foreach (var req in _visibleRequests)
                {
                    var item = new ListViewItem(req.RequestID.ToString());
                    string effectiveType = EffectiveRequestType(req.RequestType, req.Remark);
                    string userRemark = ExtractUserRemark(req.Remark);
                    string displaySubject = req.Subject ?? "—";
                    if (!string.IsNullOrEmpty(userRemark))
                        displaySubject += string.Format(" [เหตุผล: {0}]", userRemark);

                    item.SubItems.Add(RequestTypeLabel(effectiveType));
                    item.SubItems.Add(DepartmentOf(req));
                    item.SubItems.Add(displaySubject);
                    item.SubItems.Add(PrincipalTypeLabel(effectiveType, req.Remark));
                    item.SubItems.Add(ResolveRequesterName(req.RequestedBy));
                    item.SubItems.Add(req.RequestedDate.ToString("dd/MM/yy HH:mm"));
                    item.SubItems.Add(req.ExpiryDate.HasValue
                        ? req.ExpiryDate.Value.ToString("dd/MM/yy")
                        : "—");
                    item.Tag = req;
                    if (!string.IsNullOrEmpty(userRemark))
                        item.ToolTipText = string.Format("เหตุผล/หมายเหตุ: {0}", userRemark);

                    // Highlight rows expiring within 24 h
                    if (req.ExpiryDate.HasValue && req.ExpiryDate.Value <= DateTime.Now.AddDays(1))
                    {
                        item.BackColor = Color.FromArgb(255, 235, 235);
                        item.ForeColor = Color.DarkRed;
                    }

                    lvRequests.Items.Add(item);
                }
            }
            finally
            {
                lvRequests.EndUpdate();
            }

            chkSelectAll.CheckedChanged -= chkSelectAll_CheckedChanged;
            chkSelectAll.Checked = false;
            chkSelectAll.CheckedChanged += chkSelectAll_CheckedChanged;
            UpdateButtonState();
            UpdateSelCountLabel();

            if (_pendingRequests.Count == 0)
                SetStatus("ไม่มีรายการรออนุมัติ", Color.Gray);
            else if (deptFilter == null)
                SetStatus(string.Format("พบรายการรออนุมัติ {0} รายการ", _pendingRequests.Count), Color.Green);
            else
                SetStatus(string.Format("แผนก {0}: {1} จากทั้งหมด {2} รายการ",
                    deptFilter, _visibleRequests.Count, _pendingRequests.Count), Color.Green);
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
            // cache ค่า target ก่อน loop — ItemChecked handler จะ set chkSelectAll.Checked
            // กลับเป็น false ทันทีที่เจอ partial selection ถ้าอ่าน property สดทุกรอบจะ uncheck ตัวเองกลาง loop
            bool target = chkSelectAll.Checked;
            foreach (ListViewItem item in lvRequests.Items)
                item.Checked = target;
        }

        private void UpdateButtonState()
        {
            int count = lvRequests.CheckedItems.Count;
            if (count == 0) count = lvRequests.SelectedItems.Count;
            btnApprove.Enabled    = count > 0;
            btnReject.Enabled     = count > 0;
            btnApproveAll.Enabled = _visibleRequests.Count > 0;
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

            if (string.IsNullOrWhiteSpace(txtRemark.Text))
            {
                SetRemarkError(true);
                MessageBox.Show("กรุณากรอกเหตุผลในการอนุมัติ", "ข้อมูลไม่ครบ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRemark.Focus();
                return;
            }
            SetRemarkError(false);

            string msg = targets.Count == 1
                ? string.Format("ยืนยันการอนุมัติคำขอ:\n\n{0}\n{1}\n\nเหตุผล: {2}",
                    RequestTypeLabel(targets[0].RequestType), targets[0].Subject, txtRemark.Text)
                : string.Format("ยืนยันการอนุมัติ {0} รายการที่เลือก?\n\nเหตุผล: {1}",
                    targets.Count, txtRemark.Text);

            if (MessageBox.Show(msg, "ยืนยันการอนุมัติ",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ProcessBatch(targets, approve: true);
            }
        }

        private void btnApproveAll_Click(object sender, EventArgs e)
        {
            // อนุมัติทั้งหมด = เฉพาะรายการที่แสดงตามแผนกที่กรองอยู่
            if (_visibleRequests.Count == 0) return;

            if (string.IsNullOrWhiteSpace(txtRemark.Text))
            {
                SetRemarkError(true);
                MessageBox.Show("กรุณากรอกเหตุผลในการอนุมัติ", "ข้อมูลไม่ครบ",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtRemark.Focus();
                return;
            }
            SetRemarkError(false);

            string msg = string.Format("ยืนยันการอนุมัติคำขอทั้งหมด {0} รายการ?\n\nเหตุผล: {1}",
                _visibleRequests.Count, txtRemark.Text);

            if (MessageBox.Show(msg, "ยืนยันการอนุมัติทั้งหมด",
                    MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                ProcessBatch(_visibleRequests.ToList(), approve: true);
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

                    // Fallback Admin: delete-type requests (คนอื่นอนุมัติได้เฉพาะ step ที่ assign ให้ตัวเอง)
                    if (step == null && approve && _currentUser.AppRole == 4
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

        private void LvRequests_DoubleClick(object sender, EventArgs e)
        {
            if (lvRequests.SelectedItems.Count == 0) return;
            var req = lvRequests.SelectedItems[0].Tag as ApprovalRequest;
            if (req == null) return;

            var items = _approvalInboxController.GetApprovalRequestItems(req.RequestID);
            if (items != null && items.Count > 0)
            {
                using (var dlg = new CMS.Presentation.Modules.DMS.ShareRequestDetailDialog(req, items, (pType, pId) => pType == 1 ? ResolveRequesterName(pId) : string.Format("Type:{0} #{1}", pType, pId)))
                {
                    dlg.ShowDialog(this);
                }
            }
        }

        /// <summary>
        /// Resolves the true semantic type of a request.
        /// CHANGE_PERMISSION stored as a fallback for INTERNAL_SHARE (when DB constraint
        /// didn't include INTERNAL_SHARE yet) is detected via the Remark prefix.
        /// </summary>
        private static string EffectiveRequestType(string requestType, string remark)
        {
            string rt = (requestType ?? "").Trim().ToUpperInvariant();
            if ((rt == "CHANGE_PERMISSION" || rt == "PERMISSION") &&
                !string.IsNullOrEmpty(remark) &&
                (remark.StartsWith("InternalShareFile", StringComparison.OrdinalIgnoreCase) ||
                 remark.StartsWith("ShareBatch", StringComparison.OrdinalIgnoreCase)))
                return "INTERNAL_SHARE";
            return rt;
        }

        private static string RequestTypeLabel(string effectiveType)
        {
            switch ((effectiveType ?? "").ToUpperInvariant())
            {
                case "CREATE_HC":         return "🔐 สร้าง Folder 1HC";
                case "PERMISSION":
                case "CHANGE_PERMISSION": return "🔐 สิทธิ์ Folder";
                case "INTERNAL_SHARE":    return "🔗 แชร์ภายใน";
                case "SHARE_PERMISSION_CHANGE": return "⚙️ ปรับสิทธิ์แชร์";
                case "EXTERNAL_SHARE":    return "🌐 แชร์ภายนอก";
                case "UPLOAD_HC":
                case "UPLOAD":            return "📤 อัปโหลดไฟล์";
                case "DOWNLOAD_FILE":      return "⬇️ ดาวน์โหลดไฟล์";
                case "DELETE":
                case "DELETE_FILE":
                case "FILE_DELETE":       return "🗑 ลบไฟล์";
                case "DELETE_FOLDER":     return "🗑 ลบ Folder";
                case "FOLDER":            return "📁 จัดการ Folder";
                default:                  return effectiveType ?? "—";
            }
        }

        private static string PrincipalTypeLabel(string effectiveType, string remark)
        {
            // Only share-type requests have a meaningful recipient group.
            // All other types (delete, create-HC, upload, external-share, folder-mgmt) show "—".
            switch ((effectiveType ?? "").ToUpperInvariant())
            {
                case "INTERNAL_SHARE":
                case "CHANGE_PERMISSION":
                case "PERMISSION":
                    break; // fall through to parse
                default:
                    return "—";
            }

            if (string.IsNullOrEmpty(remark))
                return "—";

            if (remark.StartsWith("ShareBatch", StringComparison.OrdinalIgnoreCase))
                return "👥 หลายผู้รับ";

            // Parse "PrincipalType:N" token from the Remark string
            int principalTypeId = -1;
            foreach (string token in remark.Split(' '))
            {
                if (token.StartsWith("PrincipalType:", StringComparison.OrdinalIgnoreCase))
                {
                    int.TryParse(token.Substring("PrincipalType:".Length), out principalTypeId);
                    break;
                }
            }

            switch (principalTypeId)
            {
                case 1: return "👤 User";
                case 2: return "🎭 Role";
                case 3: return "🏢 Department";
                case 4: return "👥 Group";
                default: return "—";
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
                case "DOWNLOAD_FILE":
                    return true;
                default:
                    return false;
            }
        }

        public static string ExtractUserRemark(string remark)
        {
            if (string.IsNullOrEmpty(remark)) return "";
            foreach (var token in remark.Split(' '))
            {
                if (token.StartsWith("UserRemark:", StringComparison.OrdinalIgnoreCase))
                {
                    string b64 = token.Substring("UserRemark:".Length);
                    try
                    {
                        byte[] bytes = Convert.FromBase64String(b64);
                        return System.Text.Encoding.UTF8.GetString(bytes);
                    }
                    catch
                    {
                        return b64;
                    }
                }
            }
            return "";
        }
    }
}
