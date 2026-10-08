using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    /// <summary>
    /// Form for viewing and managing internal and external shares.
    /// Each tab splits shares into "ใช้งานอยู่" (still effective / manageable) and
    /// "ไม่ได้ใช้งาน" (revoked / expired — history, read-only).
    /// Satisfies Requirements 13.1, 13.2, 13.3, 13.4, 13.5, 14.1, 14.2, 14.3
    /// </summary>
    public partial class ShareManagementForm : Form
    {
        private enum ShareView { Active, Inactive, All }

        private static readonly Color NavyDark   = Color.FromArgb(25, 55, 105);
        private static readonly Color ActiveFg   = Color.FromArgb(0, 100, 45);
        private static readonly Color DisabledBg = Color.FromArgb(255, 248, 220);
        private static readonly Color DisabledFg = Color.FromArgb(130, 80, 0);
        private static readonly Color PendingBg  = Color.FromArgb(232, 240, 254);
        private static readonly Color PendingFg  = Color.FromArgb(0, 51, 102);
        private static readonly Color InactiveBg = Color.FromArgb(245, 245, 245);

        private const string IntActiveHint = "💡 ต้องการปิดการแชร์ก่อนกำหนด: เลือกรายการ (Ctrl/Shift = หลายรายการ) แล้วกด \"ยกเลิกการแชร์\" — ผู้รับจะเข้าถึงไฟล์ไม่ได้ทันที";
        private const string ExtActiveHint = "💡 \"ปิดชั่วคราว\" = หยุดลิงก์ไว้ เปิดกลับได้ภายหลัง  |  \"ยกเลิกการแชร์\" = ปิดถาวร";
        private const string InactiveHint  = "รายการในมุมมองนี้สิ้นสุดแล้ว (ยกเลิก/หมดอายุ) — แสดงเป็นประวัติเท่านั้น";

        private readonly IShareManagementController _shareManagementController;
        private readonly UserContext _currentUser;
        private readonly IShareLookupController _shareLookupController;
        private readonly bool _isAdmin;

        private List<InternalShareRow> _internalRows = new List<InternalShareRow>();
        private List<ExternalShareRow> _externalRows = new List<ExternalShareRow>();
        private readonly Dictionary<string, string> _nameCache = new Dictionary<string, string>();

        public ShareManagementForm(IShareManagementController shareManagementController, UserContext currentUser,
            IShareLookupController shareLookupController = null)
        {
            _isAdmin = currentUser != null && currentUser.AppRole == 4;
            _shareManagementController = shareManagementController;
            _currentUser = currentUser;
            _shareLookupController = shareLookupController;
            InitializeComponent();

            if (_isAdmin)
            {
                lblSub.Text      = "👑 Admin Mode — แสดงรายการ share ทั้งหมดในระบบ";
                lblSub.ForeColor = Color.FromArgb(255, 220, 100);
            }

            // อนุมัติ/ปฏิเสธ เป็นหน้าที่ของ Admin — เจ้าของไม่ต้องเห็นปุ่มนี้
            btnApproveExt.Visible = _isAdmin;
            btnRejectExt.Visible  = _isAdmin;
            lblExtHint.Left = _isAdmin ? btnRejectExt.Right + 10 : btnRevokeExt.Right + 10;

            StyleFilterButtons(rbIntActive, rbIntInactive, rbIntAll);
            StyleFilterButtons(rbExtActive, rbExtInactive, rbExtAll);
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadShares();
        }

        // ── Loading ──────────────────────────────────────────────────────────

        private void LoadShares()
        {
            try
            {
                _nameCache.Clear();

                // Admin เห็นทั้งหมด, user ปกติเห็นแค่ของตัวเอง (รวมรายการที่ยกเลิกแล้ว)
                var internalShares = _isAdmin
                    ? _shareManagementController.GetAllInternalShares(_currentUser)
                    : _shareManagementController.GetInternalSharesForUser(_currentUser.UserID, _currentUser);
                _internalRows = internalShares.Select(BuildInternalRow).ToList();

                var externalShares = _isAdmin
                    ? _shareManagementController.GetAllExternalShares(_currentUser)
                    : _shareManagementController.GetExternalSharesForUser(_currentUser.UserID, _currentUser);
                _externalRows = externalShares.Select(BuildExternalRow).ToList();

                ApplyInternalFilter();
                ApplyExternalFilter();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error loading shares: {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private InternalShareRow BuildInternalRow(FilePermission s)
        {
            bool expired = !s.RevokedDate.HasValue && s.ExpiryDate.HasValue && s.ExpiryDate.Value.Date < DateTime.Today;
            bool active  = !s.RevokedDate.HasValue && !expired;

            string status = s.RevokedDate.HasValue ? "🚫 ยกเลิกแล้ว"
                          : expired               ? "⌛ หมดอายุ"
                          : "✅ ใช้งาน";

            string ended = s.RevokedDate.HasValue ? s.RevokedDate.Value.ToString("g")
                         : expired               ? s.ExpiryDate.Value.ToString("d")
                         : string.Empty;

            return new InternalShareRow
            {
                Id       = s.FilePermissionID,
                IsActive = active,
                Status   = status,
                Resource = ResolveFileName(s.FileID),
                Target   = ResolvePrincipalName(s.PrincipalType, s.PrincipalID),
                Rights   = DescribeRights(s),
                Sharer   = ResolveUserName(s.GrantedBy),
                Created  = s.GrantedDate,
                Expiry   = s.ExpiryDate.HasValue ? s.ExpiryDate.Value.ToString("d") : "ไม่มีกำหนด",
                Ended    = ended,
                Note     = s.RevokeReason ?? string.Empty
            };
        }

        private ExternalShareRow BuildExternalRow(ExternalShare s)
        {
            string code = s.ShareTokenStatus ?? string.Empty;
            string status = code == "Active"   ? "✅ ใช้งาน"
                          : code == "Disabled" ? "⏸ ปิดชั่วคราว"
                          : code == "Pending"  ? "⏳ รออนุมัติ"
                          : code == "Revoked"  ? "🚫 ยกเลิกแล้ว"
                          : code == "Expired"  ? "⌛ หมดอายุ"
                          : (code.Length > 0 ? code : "-");

            string ended = s.RevokedDate.HasValue ? s.RevokedDate.Value.ToString("g")
                         : code == "Expired" && s.ExpiryDate.HasValue ? s.ExpiryDate.Value.ToString("d")
                         : string.Empty;

            return new ExternalShareRow
            {
                Id         = s.ExternalShareID,
                StatusCode = code,
                IsActive   = code == "Active" || code == "Disabled" || code == "Pending",
                Status     = status,
                Resource   = s.FileID.HasValue ? ResolveFileName(s.FileID.Value) : ResolveExternalFolderName(s.FolderID),
                SharerUserId = s.SharerUserID,
                Sharer     = ResolveUserName(s.SharerUserID),
                Created    = s.CreatedDate,
                Expiry     = s.ExpiryDate.HasValue ? s.ExpiryDate.Value.ToString("d") : "ไม่มีกำหนด",
                Limit      = s.DownloadLimit.HasValue ? s.DownloadLimit.Value.ToString() : "ไม่จำกัด",
                Downloaded = s.CurrentDownloadCount,
                Ended      = ended,
                Note       = s.RevocationReason ?? s.DisabledReason ?? string.Empty
            };
        }

        private static string DescribeRights(FilePermission s)
        {
            if (s.IsDeny) return "ห้ามเข้าถึง";
            if (s.CanFullControl) return "ควบคุมทั้งหมด";

            var parts = new List<string>();
            if (s.CanRead) parts.Add("อ่าน");
            if (s.CanWrite || s.CanModify) parts.Add("แก้ไข");
            if (s.CanDelete) parts.Add("ลบ");
            return parts.Count > 0 ? string.Join("/", parts) : "-";
        }

        // ── Filtering ────────────────────────────────────────────────────────

        private static ShareView CurrentView(RadioButton active, RadioButton inactive)
        {
            if (active.Checked) return ShareView.Active;
            if (inactive.Checked) return ShareView.Inactive;
            return ShareView.All;
        }

        private static bool MatchesView(bool isActive, ShareView view)
        {
            return view == ShareView.All || (view == ShareView.Active) == isActive;
        }

        private static bool MatchesSearch(string keyword, params string[] fields)
        {
            if (string.IsNullOrEmpty(keyword)) return true;
            return fields.Any(f => f != null && f.IndexOf(keyword, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        private void ApplyInternalFilter()
        {
            var view    = CurrentView(rbIntActive, rbIntInactive);
            var keyword = txtIntSearch.Text.Trim();

            int activeCount = _internalRows.Count(r => r.IsActive);
            rbIntActive.Text   = string.Format("● ใช้งานอยู่ ({0})", activeCount);
            rbIntInactive.Text = string.Format("○ ไม่ได้ใช้งาน ({0})", _internalRows.Count - activeCount);
            rbIntAll.Text      = string.Format("ทั้งหมด ({0})", _internalRows.Count);

            dgvInternal.DataSource = _internalRows
                .Where(r => MatchesView(r.IsActive, view)
                         && MatchesSearch(keyword, r.Resource, r.Target, r.Sharer))
                .ToList();

            colIntEnded.Visible = view != ShareView.Active;
            colIntNote.Visible  = view != ShareView.Active;
            UpdateInternalActions();
        }

        private void ApplyExternalFilter()
        {
            var view    = CurrentView(rbExtActive, rbExtInactive);
            var keyword = txtExtSearch.Text.Trim();

            int activeCount = _externalRows.Count(r => r.IsActive);
            rbExtActive.Text   = string.Format("● ใช้งานอยู่ ({0})", activeCount);
            rbExtInactive.Text = string.Format("○ ไม่ได้ใช้งาน ({0})", _externalRows.Count - activeCount);
            rbExtAll.Text      = string.Format("ทั้งหมด ({0})", _externalRows.Count);

            dgvExternal.DataSource = _externalRows
                .Where(r => MatchesView(r.IsActive, view)
                         && MatchesSearch(keyword, r.Resource, r.Sharer))
                .ToList();

            colExtEnded.Visible = view != ShareView.Active;
            UpdateExternalActions();
        }

        private static void StyleFilterButtons(params RadioButton[] buttons)
        {
            foreach (var rb in buttons)
            {
                rb.BackColor = rb.Checked ? rb.FlatAppearance.CheckedBackColor : Color.White;
                rb.ForeColor = rb.Checked ? Color.White : NavyDark;
            }
        }

        // ── Selection → action buttons ───────────────────────────────────────

        private List<InternalShareRow> SelectedInternalRows()
        {
            return dgvInternal.SelectedRows.Cast<DataGridViewRow>()
                .Select(r => r.DataBoundItem as InternalShareRow)
                .Where(r => r != null)
                .ToList();
        }

        private List<ExternalShareRow> SelectedExternalRows()
        {
            return dgvExternal.SelectedRows.Cast<DataGridViewRow>()
                .Select(r => r.DataBoundItem as ExternalShareRow)
                .Where(r => r != null)
                .ToList();
        }

        private void UpdateInternalActions()
        {
            if (rbIntInactive.Checked)
            {
                btnRevokeInternal.Enabled = false;
                lblIntHint.Text = InactiveHint;
                return;
            }

            int activeSelected = SelectedInternalRows().Count(r => r.IsActive);
            btnRevokeInternal.Enabled = activeSelected > 0;
            btnRevokeInternal.Text = activeSelected > 1
                ? string.Format("🚫  ยกเลิก {0} รายการ", activeSelected)
                : "🚫  ยกเลิกการแชร์";
            lblIntHint.Text = IntActiveHint;
        }

        private void UpdateExternalActions()
        {
            if (rbExtInactive.Checked)
            {
                btnToggleExt.Enabled  = false;
                btnRevokeExt.Enabled  = false;
                btnApproveExt.Enabled = false;
                btnRejectExt.Enabled  = false;
                lblExtHint.Text = InactiveHint;
                return;
            }

            var selected = SelectedExternalRows();
            var first    = selected.FirstOrDefault();
            string code  = first != null ? first.StatusCode : string.Empty;
            bool single  = selected.Count == 1;

            btnRevokeExt.Enabled  = selected.Any(r => r.IsActive);
            // ห้ามอนุมัติคำขอของตัวเอง (service ตรวจซ้ำอีกชั้น)
            bool canDecide = single && code == "Pending" && first.SharerUserId != _currentUser.UserID;
            btnApproveExt.Enabled = canDecide;
            btnRejectExt.Enabled  = canDecide;
            btnToggleExt.Enabled  = single && (code == "Active" || code == "Disabled");

            if (code == "Disabled")
            {
                btnToggleExt.Text      = "▶  เปิดใช้งาน";
                btnToggleExt.BackColor = Color.FromArgb(232, 248, 240);
                btnToggleExt.ForeColor = ActiveFg;
            }
            else
            {
                btnToggleExt.Text      = "⏸  ปิดชั่วคราว";
                btnToggleExt.BackColor = Color.FromArgb(255, 248, 230);
                btnToggleExt.ForeColor = DisabledFg;
            }
            lblExtHint.Text = ExtActiveHint;
        }

        // ── Row colouring ────────────────────────────────────────────────────

        private void DgvInternal_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in dgvInternal.Rows)
            {
                var item = row.DataBoundItem as InternalShareRow;
                if (item == null) continue;
                if (item.IsActive)
                {
                    row.Cells[colIntStatus.Index].Style.ForeColor = ActiveFg;
                }
                else
                {
                    row.DefaultCellStyle.BackColor = InactiveBg;
                    row.DefaultCellStyle.ForeColor = Color.Gray;
                }
            }
            dgvInternal.ClearSelection();
            UpdateInternalActions();
        }

        private void DgvExternal_DataBindingComplete(object sender, DataGridViewBindingCompleteEventArgs e)
        {
            foreach (DataGridViewRow row in dgvExternal.Rows)
            {
                var item = row.DataBoundItem as ExternalShareRow;
                if (item == null) continue;
                switch (item.StatusCode)
                {
                    case "Active":
                        row.Cells[colExtStatus.Index].Style.ForeColor = ActiveFg;
                        break;
                    case "Disabled":
                        row.DefaultCellStyle.BackColor = DisabledBg;
                        row.DefaultCellStyle.ForeColor = DisabledFg;
                        break;
                    case "Pending":
                        row.DefaultCellStyle.BackColor = PendingBg;
                        row.DefaultCellStyle.ForeColor = PendingFg;
                        break;
                    default:
                        row.DefaultCellStyle.BackColor = InactiveBg;
                        row.DefaultCellStyle.ForeColor = Color.Gray;
                        break;
                }
            }
            dgvExternal.ClearSelection();
            UpdateExternalActions();
        }

        // ── Event handlers ───────────────────────────────────────────────────

        private void BtnRefresh_Click(object sender, EventArgs e)
        {
            LoadShares();
        }

        private void InternalFilter_CheckedChanged(object sender, EventArgs e)
        {
            StyleFilterButtons(rbIntActive, rbIntInactive, rbIntAll);
            if (((RadioButton)sender).Checked) ApplyInternalFilter();
        }

        private void ExternalFilter_CheckedChanged(object sender, EventArgs e)
        {
            StyleFilterButtons(rbExtActive, rbExtInactive, rbExtAll);
            if (((RadioButton)sender).Checked) ApplyExternalFilter();
        }

        private void TxtIntSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyInternalFilter();
        }

        private void TxtExtSearch_TextChanged(object sender, EventArgs e)
        {
            ApplyExternalFilter();
        }

        private void DgvInternal_SelectionChanged(object sender, EventArgs e)
        {
            UpdateInternalActions();
        }

        private void DgvExternal_SelectionChanged(object sender, EventArgs e)
        {
            UpdateExternalActions();
        }

        private void BtnRevokeInternal_Click(object sender, EventArgs e)
        {
            RevokeInternalShare();
        }

        private void BtnToggleExt_Click(object sender, EventArgs e)
        {
            ToggleExternalShare();
        }

        private void BtnRevokeExt_Click(object sender, EventArgs e)
        {
            RevokeExternalShare();
        }

        private void BtnApproveExt_Click(object sender, EventArgs e)
        {
            ApproveExternalShare();
        }

        private void BtnRejectExt_Click(object sender, EventArgs e)
        {
            RejectExternalShare();
        }

        // ── Actions ──────────────────────────────────────────────────────────

        private void RevokeInternalShare()
        {
            var toRevoke = SelectedInternalRows().Where(r => r.IsActive).ToList();
            if (toRevoke.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกรายการที่ยังใช้งานอยู่ที่ต้องการยกเลิกการแชร์", "แจ้งเตือน");
                return;
            }

            string confirmMsg = toRevoke.Count == 1
                ? string.Format("ต้องการยกเลิกการแชร์ \"{0}\" ให้ {1} ใช่หรือไม่?\n\nผู้รับจะไม่สามารถเข้าถึงไฟล์นี้ได้ทันที",
                    toRevoke[0].Resource, toRevoke[0].Target)
                : string.Format("ต้องการยกเลิกการแชร์ทั้งหมด {0} รายการที่เลือกใช่หรือไม่?\n\nผู้รับจะไม่สามารถเข้าถึงไฟล์ได้ทันที", toRevoke.Count);

            if (MessageBox.Show(confirmMsg, "ยืนยันการยกเลิกการแชร์",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int successCount = 0;
            var errors = new List<string>();
            foreach (var row in toRevoke)
            {
                try
                {
                    _shareManagementController.RevokeInternalShare(row.Id, _currentUser);
                    successCount++;
                }
                catch (Exception ex)
                {
                    errors.Add(string.Format("{0}: {1}", row.Resource, ex.Message));
                }
            }

            ShowBatchResult(successCount, errors, "ยกเลิกการแชร์เรียบร้อยแล้ว {0} รายการ\n(ดูย้อนหลังได้ที่ \"ไม่ได้ใช้งาน\")");
            LoadShares();
        }

        private void ToggleExternalShare()
        {
            var selected = SelectedExternalRows();
            if (selected.Count != 1)
            {
                MessageBox.Show("กรุณาเลือก 1 รายการที่ต้องการเปิด/ปิด", "แจ้งเตือน");
                return;
            }

            var row = selected[0];
            if (row.StatusCode != "Active" && row.StatusCode != "Disabled")
            {
                MessageBox.Show("สามารถเปิด/ปิดได้เฉพาะรายการที่มีสถานะ 'ใช้งาน' หรือ 'ปิดชั่วคราว' เท่านั้น", "แจ้งเตือน");
                return;
            }

            string action = row.StatusCode == "Active" ? "ปิดชั่วคราว" : "เปิดใช้งาน";
            if (MessageBox.Show(string.Format("ต้องการ{0}การแชร์ \"{1}\" ใช่หรือไม่?", action, row.Resource), "ยืนยัน",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                _shareManagementController.ToggleExternalShare(row.Id, _currentUser);
                MessageBox.Show(string.Format("{0}เรียบร้อยแล้ว", action), "สำเร็จ");
                LoadShares();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error: {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ApproveExternalShare()
        {
            var selected = SelectedExternalRows();
            if (selected.Count != 1)
            {
                MessageBox.Show("กรุณาเลือก 1 รายการที่ต้องการอนุมัติ", "แจ้งเตือน");
                return;
            }

            try
            {
                _shareManagementController.ApproveExternalShare(selected[0].Id, _currentUser);
                MessageBox.Show("อนุมัติการแชร์ภายนอกเรียบร้อยแล้ว", "สำเร็จ");
                LoadShares();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error: {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RejectExternalShare()
        {
            var selected = SelectedExternalRows();
            if (selected.Count != 1)
            {
                MessageBox.Show("กรุณาเลือก 1 รายการที่ต้องการปฏิเสธ", "แจ้งเตือน");
                return;
            }

            var reason = PromptForReason("ระบุเหตุผลในการปฏิเสธ:");
            if (reason == null) return;

            try
            {
                _shareManagementController.RejectExternalShare(selected[0].Id, reason, _currentUser);
                MessageBox.Show("ปฏิเสธการแชร์ภายนอกเรียบร้อยแล้ว", "สำเร็จ");
                LoadShares();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error: {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RevokeExternalShare()
        {
            var toRevoke = SelectedExternalRows().Where(r => r.IsActive).ToList();
            if (toRevoke.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกรายการที่ยังใช้งานอยู่ที่ต้องการยกเลิก", "แจ้งเตือน");
                return;
            }

            string promptLabel = toRevoke.Count == 1
                ? "ระบุเหตุผลในการยกเลิก (ลิงก์จะใช้งานไม่ได้ถาวร):"
                : string.Format("ระบุเหตุผลในการยกเลิก (ใช้กับทั้ง {0} รายการ, ลิงก์จะใช้งานไม่ได้ถาวร):", toRevoke.Count);

            var reason = PromptForReason(promptLabel);
            if (reason == null) return;

            int successCount = 0;
            var errors = new List<string>();
            foreach (var row in toRevoke)
            {
                try
                {
                    _shareManagementController.RevokeExternalShare(row.Id, reason, _currentUser);
                    successCount++;
                }
                catch (Exception ex)
                {
                    errors.Add(string.Format("{0}: {1}", row.Resource, ex.Message));
                }
            }

            ShowBatchResult(successCount, errors, "ยกเลิกเรียบร้อยแล้ว {0} รายการ\n(ดูย้อนหลังได้ที่ \"ไม่ได้ใช้งาน\")");
            LoadShares();
        }

        private void ShowBatchResult(int successCount, List<string> errors, string successFormat)
        {
            if (errors.Count == 0)
            {
                MessageBox.Show(string.Format(successFormat, successCount),
                    "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            MessageBox.Show(
                string.Format("สำเร็จ {0} รายการ\nล้มเหลว {1} รายการ:\n{2}", successCount, errors.Count, string.Join("\n", errors)),
                "เสร็จสิ้นบางส่วน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
        }

        private string PromptForReason(string prompt)
        {
            using (var dlg = new ShareReasonDialog(prompt))
            {
                return dlg.ShowDialog(this) == DialogResult.OK ? dlg.Reason : null;
            }
        }

        // ── Name resolution helpers (cached per load) ────────────────────────

        private string Cached(string key, Func<string> resolve)
        {
            string value;
            if (!_nameCache.TryGetValue(key, out value))
            {
                value = resolve();
                _nameCache[key] = value;
            }
            return value;
        }

        private string ResolveUserName(int userId)
        {
            if (_shareLookupController == null)
                return userId == _currentUser.UserID ? (_currentUser.ADUserName ?? "Current User") : string.Format("User #{0}", userId);

            return Cached("U" + userId, () => _shareLookupController.ResolveGranterName(userId));
        }

        private string ResolvePrincipalName(int principalType, int principalId)
        {
            // PrincipalType: 1=User, 2=Role, 3=OrgUnit, 4=Group
            if (_shareLookupController != null)
                return Cached("P" + principalType + ":" + principalId,
                    () => _shareLookupController.ResolvePrincipalName(principalType, principalId));

            switch (principalType)
            {
                case 1: return string.Format("User #{0}", principalId);
                case 2: return string.Format("Role #{0}", principalId);
                case 3: return string.Format("OrgUnit #{0}", principalId);
                case 4: return string.Format("Group #{0}", principalId);
                default: return string.Format("#{0}", principalId);
            }
        }

        private string ResolveFileName(int fileId)
        {
            if (_shareLookupController != null)
                return Cached("F" + fileId, () => _shareLookupController.ResolveFileName(fileId));

            return string.Format("File #{0}", fileId);
        }

        private string ResolveExternalFolderName(int? folderId)
        {
            if (folderId == null) return "(ไม่ระบุ)";
            return string.Format("Folder #{0}", folderId);
        }

        // ── Grid row models ──────────────────────────────────────────────────

        private sealed class InternalShareRow
        {
            public int Id { get; set; }
            public bool IsActive { get; set; }
            public string Status { get; set; }
            public string Resource { get; set; }
            public string Target { get; set; }
            public string Rights { get; set; }
            public string Sharer { get; set; }
            public DateTime Created { get; set; }
            public string Expiry { get; set; }
            public string Ended { get; set; }
            public string Note { get; set; }
        }

        private sealed class ExternalShareRow
        {
            public int Id { get; set; }
            public string StatusCode { get; set; }
            public bool IsActive { get; set; }
            public string Status { get; set; }
            public string Resource { get; set; }
            public int SharerUserId { get; set; }
            public string Sharer { get; set; }
            public DateTime Created { get; set; }
            public string Expiry { get; set; }
            public string Limit { get; set; }
            public int Downloaded { get; set; }
            public string Ended { get; set; }
            public string Note { get; set; }
        }
    }
}
