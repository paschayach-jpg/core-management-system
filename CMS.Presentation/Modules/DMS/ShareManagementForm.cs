using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Presentation.Modules.DMS;

namespace CMS.Presentation
{
    /// <summary>
    /// Form for viewing and managing internal and external shares.
    /// Internal shares tab groups recipients by File / Folder (one row per file).
    /// Double-clicking or clicking "ดูรายละเอียด" opens FileShareDetailDialog.
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

        private const string IntActiveHint = "💡 ดับเบิ้ลคลิกเพื่อดูรายชื่อผู้รับ  |  \"ยกเลิกการแชร์\" = ปิดสิทธิ์ทั้งหมด  |  \"ยกเลิกคำขอ\" = ถอนคำขอที่รออนุมัติ";
        private const string ExtActiveHint = "💡 \"ปิดชั่วคราว\" = หยุดลิงก์ไว้ เปิดกลับได้ภายหลัง  |  \"ยกเลิกการแชร์\" = ปิดถาวร";
        private const string InactiveHint  = "รายการในมุมมองนี้สิ้นสุดแล้ว (ยกเลิก/หมดอายุ) — แสดงเป็นประวัติเท่านั้น";

        private readonly IShareManagementController _shareManagementController;
        private readonly UserContext _currentUser;
        private readonly IShareLookupController _shareLookupController;
        private readonly IShareCommandController _shareCommandController;
        private readonly IUserAccountLookupController _userAccountLookupController;
        private readonly IUserAccessOrgLookupController _orgLookupController;
        private readonly IFolderReadLookupController _folderReadLookupController;
        private readonly bool _isAdmin;

        private List<InternalShareGroupRow> _internalRows = new List<InternalShareGroupRow>();
        private List<ExternalShareRow> _externalRows = new List<ExternalShareRow>();
        private readonly Dictionary<string, string> _nameCache = new Dictionary<string, string>();

        public ShareManagementForm(
            IShareManagementController shareManagementController,
            UserContext currentUser,
            IShareLookupController shareLookupController = null,
            IShareCommandController shareCommandController = null,
            IUserAccountLookupController userAccountLookupController = null,
            IUserAccessOrgLookupController orgLookupController = null,
            IFolderReadLookupController folderReadLookupController = null)
        {
            _isAdmin = currentUser != null && currentUser.AppRole == 4;
            _shareManagementController = shareManagementController;
            _currentUser = currentUser;
            _shareLookupController = shareLookupController;
            _shareCommandController = shareCommandController;
            _userAccountLookupController = userAccountLookupController;
            _orgLookupController = orgLookupController;
            _folderReadLookupController = folderReadLookupController;
            InitializeComponent();

            if (_isAdmin)
            {
                lblSub.Text      = "👑 Admin Mode — แสดงรายการ share ทั้งหมดในระบบ";
                lblSub.ForeColor = Color.FromArgb(255, 220, 100);
            }

            btnApproveExt.Visible = _isAdmin;
            btnRejectExt.Visible  = _isAdmin;
            lblExtHint.Left = _isAdmin ? btnRejectExt.Right + 10 : btnRevokeExt.Right + 10;

            StyleFilterButtons(rbIntActive, rbIntInactive, rbIntAll);
            StyleFilterButtons(rbExtActive, rbExtInactive, rbExtAll);

            var ctxInternal = new ContextMenuStrip();
            var mnuDetail = new ToolStripMenuItem("📋  ดูรายละเอียดผู้รับ...", null, (s, e) => ViewSelectedInternalDetail());
            var mnuAdd = new ToolStripMenuItem("➕  เพิ่มผู้รับสิทธิ์...", null, (s, e) => AddSelectedInternalRecipients());
            var mnuEdit = new ToolStripMenuItem("⚙️  ขอปรับสิทธิ์...", null, (s, e) => EditSelectedInternalShare());
            var mnuRevoke = new ToolStripMenuItem("🚫  ยกเลิกการแชร์", null, (s, e) => RevokeInternalShare());
            var mnuWithdraw = new ToolStripMenuItem("↩  ยกเลิกคำขอ", null, (s, e) => WithdrawInternalRequests());
            ctxInternal.Items.Add(mnuDetail);
            ctxInternal.Items.Add(mnuAdd);
            ctxInternal.Items.Add(mnuEdit);
            ctxInternal.Items.Add(mnuRevoke);
            ctxInternal.Items.Add(mnuWithdraw);
            dgvInternal.ContextMenuStrip = ctxInternal;

            dgvInternal.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                ViewSelectedInternalDetail();
            };
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

                var internalShares = _isAdmin
                    ? _shareManagementController.GetAllInternalShares(_currentUser)
                    : _shareManagementController.GetInternalSharesForUser(_currentUser.UserID, _currentUser);

                var internalRequests = _shareManagementController
                    .GetInternalShareRequestsForUser(_currentUser.UserID, _currentUser);

                _internalRows = GroupInternalShares(internalShares, internalRequests);

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

        private List<InternalShareGroupRow> GroupInternalShares(
            IEnumerable<FilePermission> permissions,
            IEnumerable<InternalShareRequestItem> requests)
        {
            var groups = new Dictionary<string, InternalShareGroupRow>();

            if (permissions != null)
            {
                foreach (var p in permissions)
                {
                    string key = string.Format("F:{0}", p.FileID);
                    if (!groups.TryGetValue(key, out var group))
                    {
                        group = new InternalShareGroupRow
                        {
                            GroupKey = key,
                            FileID = p.FileID,
                            IsFolder = false,
                            Resource = ResolveFileName(p.FileID),
                            Sharer = ResolveUserName(p.GrantedBy)
                        };
                        groups[key] = group;
                    }
                    group.Permissions.Add(p);
                }
            }

            if (requests != null)
            {
                foreach (var r in requests)
                {
                    bool isFolder = r.IsFolder || (r.FolderID > 0 && r.FileID == 0);
                    string key = isFolder ? string.Format("D:{0}", r.FolderID) : string.Format("F:{0}", r.FileID);
                    if (!groups.TryGetValue(key, out var group))
                    {
                        group = new InternalShareGroupRow
                        {
                            GroupKey = key,
                            FileID = isFolder ? (int?)null : r.FileID,
                            FolderID = isFolder ? (int?)r.FolderID : null,
                            IsFolder = isFolder,
                            Resource = isFolder ? ResolveFolderName(r.FolderID) : ResolveFileName(r.FileID),
                            Sharer = ResolveUserName(_currentUser.UserID)
                        };
                        groups[key] = group;
                    }
                    group.Requests.Add(r);
                }
            }

            // Build summaries for each group
            foreach (var g in groups.Values)
            {
                int activePerms = g.Permissions.Count(p => !p.RevokedDate.HasValue && (!p.ExpiryDate.HasValue || p.ExpiryDate.Value.Date >= DateTime.Today));
                int pendingReqs = g.Requests.Count(r => r.IsPending);
                int endedPerms = g.Permissions.Count(p => p.RevokedDate.HasValue || (p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today));
                int endedReqs = g.Requests.Count(r => !r.IsPending);

                g.HasActive = activePerms > 0;
                g.HasPending = pendingReqs > 0;
                g.IsActive = g.HasActive || g.HasPending;

                // Status summary
                if (activePerms > 0 && pendingReqs > 0)
                    g.Status = string.Format("✅ ใช้งาน ({0}), ⏳ รออนุมัติ ({1})", activePerms, pendingReqs);
                else if (activePerms > 0)
                    g.Status = string.Format("✅ ใช้งาน ({0})", activePerms);
                else if (pendingReqs > 0)
                    g.Status = string.Format("⏳ รออนุมัติ ({0})", pendingReqs);
                else
                    g.Status = string.Format("🚫 สิ้นสุดแล้ว ({0})", endedPerms + endedReqs);

                // Recipient summary
                var recipientNames = new List<string>();
                foreach (var p in g.Permissions)
                {
                    string name = ResolvePrincipalName(p.PrincipalType, p.PrincipalID);
                    if (!recipientNames.Contains(name)) recipientNames.Add(name);
                }
                foreach (var r in g.Requests)
                {
                    string name = ResolvePrincipalName(r.PrincipalType, r.PrincipalID);
                    if (!recipientNames.Contains(name)) recipientNames.Add(name);
                }

                int totalCount = g.Permissions.Count + g.Requests.Count;
                g.RecipientCount = totalCount;
                if (recipientNames.Count <= 2)
                {
                    g.Target = string.Format("{0} ผู้รับ ({1})", totalCount, string.Join(", ", recipientNames));
                }
                else
                {
                    g.Target = string.Format("{0} ผู้รับ ({1}, {2}, ...)", totalCount, recipientNames[0], recipientNames[1]);
                }

                // Rights summary
                var rightsList = new List<string>();
                bool canRead = g.Permissions.Any(p => p.CanRead) || g.Requests.Any(r => r.CanRead);
                bool canWrite = g.Permissions.Any(p => p.CanWrite || p.CanModify) || g.Requests.Any(r => r.CanWrite);
                bool canDelete = g.Permissions.Any(p => p.CanDelete) || g.Requests.Any(r => r.CanDelete);
                bool canDownload = g.Permissions.Any(p => p.CanDownload) || g.Requests.Any(r => r.CanDownload);

                if (canRead) rightsList.Add("อ่าน");
                if (canDownload) rightsList.Add("ดาวน์โหลด");
                if (canWrite) rightsList.Add("แก้ไข");
                if (canDelete) rightsList.Add("ลบ");
                g.Rights = rightsList.Count > 0 ? string.Join("/", rightsList) : "-";

                // Latest date
                DateTime maxDate = DateTime.MinValue;
                foreach (var p in g.Permissions)
                    if (p.GrantedDate > maxDate) maxDate = p.GrantedDate;
                foreach (var r in g.Requests)
                    if (r.RequestedDate > maxDate) maxDate = r.RequestedDate;
                g.Created = maxDate != DateTime.MinValue ? maxDate : DateTime.Now;

                // Expiry summary
                var activeExp = g.Permissions
                    .Where(p => !p.RevokedDate.HasValue && p.ExpiryDate.HasValue)
                    .Select(p => p.ExpiryDate.Value)
                    .ToList();
                if (activeExp.Count > 0)
                {
                    var minExp = activeExp.Min();
                    g.Expiry = minExp.ToString("d");
                }
                else
                {
                    g.Expiry = "ไม่มีกำหนด";
                }

                g.Ended = string.Empty;
                g.Note = string.Empty;
            }

            return groups.Values
                .OrderByDescending(g => g.IsActive)
                .ThenByDescending(g => g.Created)
                .ToList();
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

        private List<InternalShareGroupRow> SelectedInternalRows()
        {
            return dgvInternal.SelectedRows.Cast<DataGridViewRow>()
                .Select(r => r.DataBoundItem as InternalShareGroupRow)
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
            var selected = SelectedInternalRows();
            bool singleSelected = selected.Count == 1;
            int activeSelectedCount = selected.Count(r => r.HasActive);
            int pendingSelectedCount = selected.Count(r => r.HasPending);

            btnViewDetailInternal.Enabled = singleSelected;
            btnEditInternal.Enabled = singleSelected && selected[0].HasActive;
            btnRevokeInternal.Enabled = activeSelectedCount > 0;
            btnWithdrawInternal.Enabled = pendingSelectedCount > 0;

            btnRevokeInternal.Text = activeSelectedCount > 1
                ? string.Format("🚫  ยกเลิก {0} ไฟล์", activeSelectedCount)
                : "🚫  ยกเลิกการแชร์";

            btnWithdrawInternal.Text = pendingSelectedCount > 1
                ? string.Format("↩  ยกเลิกคำขอ {0} ไฟล์", pendingSelectedCount)
                : "↩  ยกเลิกคำขอ";

            if (rbIntInactive.Checked)
            {
                lblIntHint.Text = InactiveHint;
            }
            else
            {
                lblIntHint.Text = IntActiveHint;
            }
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
                var item = row.DataBoundItem as InternalShareGroupRow;
                if (item == null) continue;
                if (item.HasPending && !item.HasActive)
                {
                    row.DefaultCellStyle.BackColor = PendingBg;
                    row.DefaultCellStyle.ForeColor = PendingFg;
                }
                else if (item.HasActive)
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

        private void BtnViewDetailInternal_Click(object sender, EventArgs e)
        {
            ViewSelectedInternalDetail();
        }

        private void ViewSelectedInternalDetail()
        {
            var selected = SelectedInternalRows();
            if (selected.Count == 0) return;

            var row = selected[0];
            using (var dlg = new FileShareDetailDialog(
                row.Resource,
                row.Sharer,
                row.FileID,
                row.FolderID,
                row.Permissions,
                row.Requests,
                _shareManagementController,
                _currentUser,
                _shareLookupController,
                _shareCommandController,
                _userAccountLookupController,
                _orgLookupController,
                _folderReadLookupController))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK || dlg.WasModified)
                {
                    LoadShares();
                }
            }
        }

        private void AddSelectedInternalRecipients()
        {
            var selected = SelectedInternalRows();
            if (selected.Count == 0) return;

            var row = selected[0];
            if (_shareCommandController == null || _userAccountLookupController == null || _orgLookupController == null)
            {
                ViewSelectedInternalDetail();
                return;
            }

            List<FileDocument> files = null;
            Folder folder = null;

            if (row.FileID.HasValue && row.FileID.Value > 0)
            {
                var fileDoc = _shareLookupController?.GetFileById(row.FileID.Value);
                if (fileDoc == null)
                {
                    fileDoc = new FileDocument
                    {
                        FileID = row.FileID.Value,
                        OriginalFileName = row.Resource
                    };
                }
                files = new List<FileDocument> { fileDoc };
            }
            else if (row.FolderID.HasValue && row.FolderID.Value > 0)
            {
                folder = _shareLookupController?.GetFolderById(row.FolderID.Value);
                if (folder == null)
                {
                    folder = new Folder
                    {
                        FolderID = row.FolderID.Value,
                        FolderName = row.Resource
                    };
                }
            }

            if ((files == null || files.Count == 0) && folder == null)
            {
                ViewSelectedInternalDetail();
                return;
            }

            using (var shareDlg = new ShareDialogInternal(
                _shareCommandController,
                _userAccountLookupController,
                _orgLookupController,
                _currentUser,
                _folderReadLookupController,
                files,
                folder))
            {
                if (shareDlg.ShowDialog(this) == DialogResult.OK)
                {
                    LoadShares();
                }
            }
        }

        private void BtnRevokeInternal_Click(object sender, EventArgs e)
        {
            RevokeInternalShare();
        }

        private void BtnWithdrawInternal_Click(object sender, EventArgs e)
        {
            WithdrawInternalRequests();
        }

        private void BtnEditInternal_Click(object sender, EventArgs e)
        {
            EditSelectedInternalShare();
        }

        private void EditSelectedInternalShare()
        {
            var selected = SelectedInternalRows().Where(r => r.HasActive).ToList();
            if (selected.Count == 0)
            {
                if (SelectedInternalRows().Any(r => r.HasPending))
                {
                    MessageBox.Show("รายการนี้อยู่ในสถานะรออนุมัติ จึงยังไม่สามารถปรับสิทธิ์ได้ (สามารถถอนคำขอและสร้างใหม่ได้)",
                        "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show("กรุณาเลือกรายการที่ยังใช้งานอยู่ที่ต้องการปรับสิทธิ์", "แจ้งเตือน",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                }
                return;
            }

            var row = selected[0];
            var activePerms = row.Permissions
                .Where(p => !p.RevokedDate.HasValue && (!p.ExpiryDate.HasValue || p.ExpiryDate.Value.Date >= DateTime.Today))
                .ToList();

            if (activePerms.Count == 0)
            {
                ViewSelectedInternalDetail();
                return;
            }

            try
            {
                using (var dlg = new SharePermissionConfigDialog(
                    activePerms,
                    row.Resource,
                    row.Target,
                    row.Sharer,
                    _shareManagementController,
                    _currentUser))
                {
                    if (dlg.ShowDialog(this) == DialogResult.OK)
                    {
                        LoadShares();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("เกิดข้อผิดพลาด: {0}", ex.Message), "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
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
            var selected = SelectedInternalRows().Where(r => r.HasActive).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกรายการที่ยังใช้งานอยู่ที่ต้องการยกเลิกการแชร์", "แจ้งเตือน");
                return;
            }

            var allActivePerms = new List<FilePermission>();
            foreach (var g in selected)
            {
                allActivePerms.AddRange(g.Permissions.Where(p => !p.RevokedDate.HasValue && (!p.ExpiryDate.HasValue || p.ExpiryDate.Value.Date >= DateTime.Today)));
            }

            if (allActivePerms.Count == 0)
            {
                MessageBox.Show("ไม่พบรายการสิทธิ์ที่ใช้งานอยู่ในการเลือกนี้", "แจ้งเตือน");
                return;
            }

            string confirmMsg = selected.Count == 1
                ? string.Format("ต้องการยกเลิกการแชร์ \"{0}\" ทั้งหมด ({1} ผู้รับ) ใช่หรือไม่?\n\nผู้รับจะไม่สามารถเข้าถึงไฟล์ได้ทันที",
                    selected[0].Resource, allActivePerms.Count)
                : string.Format("ต้องการยกเลิกการแชร์ทั้งหมด {0} ไฟล์ ({1} ผู้รับ) ใช่หรือไม่?\n\nผู้รับจะไม่สามารถเข้าถึงไฟล์ได้ทันที",
                    selected.Count, allActivePerms.Count);

            if (MessageBox.Show(confirmMsg, "ยืนยันการยกเลิกการแชร์",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var opItems = allActivePerms.Select(p => new OperationProgressItem
            {
                Title = string.Format("ยกเลิกสิทธิ์ #{0} (ผู้รับ: {1})", p.FilePermissionID, ResolvePrincipalName(p.PrincipalType, p.PrincipalID)),
                Tag = p.FilePermissionID
            }).ToList();

            using (var progress = new OperationProgressDialog(
                "ยกเลิกการแชร์",
                string.Format("กำลังยกเลิกการแชร์ {0} รายการ...", opItems.Count),
                "กำลังเพิกถอนสิทธิ์การเข้าถึง",
                opItems,
                item =>
                {
                    int permId = (int)item.Tag;
                    _shareManagementController.RevokeInternalShare(permId, _currentUser);
                    return null;
                }))
            {
                progress.ShowDialog(this);
                LoadShares();
            }
        }

        private void WithdrawInternalRequests()
        {
            var selected = SelectedInternalRows().Where(r => r.HasPending).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกรายการที่รออนุมัติที่ต้องการถอนคำขอ", "แจ้งเตือน");
                return;
            }

            var allPendingReqs = new List<InternalShareRequestItem>();
            foreach (var g in selected)
            {
                allPendingReqs.AddRange(g.Requests.Where(r => r.IsPending));
            }

            var distinctReqIds = allPendingReqs.Select(r => r.RequestID).Distinct().ToList();

            if (distinctReqIds.Count == 0)
            {
                MessageBox.Show("ไม่พบคำขอที่รออนุมัติในการเลือกนี้", "แจ้งเตือน");
                return;
            }

            string confirmMsg = distinctReqIds.Count == 1
                ? string.Format("ต้องการถอนคำขอแชร์ #{0} ใช่หรือไม่?\n\nผู้อนุมัติจะไม่เห็นคำขอนี้อีก", distinctReqIds[0])
                : string.Format("ต้องการถอนคำขอแชร์ทั้งหมด {0} คำขอใช่หรือไม่?\n\nผู้อนุมัติจะไม่เห็นคำขอเหล่านี้อีก", distinctReqIds.Count);

            if (MessageBox.Show(confirmMsg, "ยืนยันการถอนคำขอ",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            var opItems = distinctReqIds.Select(reqId => new OperationProgressItem
            {
                Title = string.Format("ถอนคำขออนุมัติ #{0}", reqId),
                Tag = reqId
            }).ToList();

            using (var progress = new OperationProgressDialog(
                "ถอนคำขอแชร์",
                string.Format("กำลังถอนคำขอ {0} รายการ...", opItems.Count),
                "กำลังยกเลิกคำขอในระบบอนุมัติ",
                opItems,
                item =>
                {
                    int reqId = (int)item.Tag;
                    _shareManagementController.WithdrawInternalShareRequest(reqId, _currentUser);
                    return null;
                }))
            {
                progress.ShowDialog(this);
                LoadShares();
            }
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

            string reason = PromptInput("ระบุเหตุผลในการปฏิเสธคำขอแชร์ภายนอก:", "ปฏิเสธคำขอแชร์ภายนอก");
            if (reason == null) return;
            if (string.IsNullOrWhiteSpace(reason))
            {
                MessageBox.Show("กรุณาระบุเหตุผล", "แจ้งเตือน");
                return;
            }

            try
            {
                _shareManagementController.RejectExternalShare(selected[0].Id, reason.Trim(), _currentUser);
                MessageBox.Show("ปฏิเสธคำขอแชร์ภายนอกเรียบร้อยแล้ว", "สำเร็จ");
                LoadShares();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error: {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RevokeExternalShare()
        {
            var selected = SelectedExternalRows().Where(r => r.IsActive).ToList();
            if (selected.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกรายการที่ยังใช้งานอยู่ (Active/Pending/Disabled) ที่ต้องการยกเลิก", "แจ้งเตือน");
                return;
            }

            string reason = PromptInput("ระบุเหตุผลในการยกเลิกการแชร์ภายนอก (ทางเลือก):", "ยกเลิกการแชร์ภายนอก");
            if (reason == null) return;

            string confirmMsg = selected.Count == 1
                ? string.Format("ต้องการยกเลิกการแชร์ \"{0}\" ใช่หรือไม่?\n\nลิงก์จะใช้งานไม่ได้ถาวร", selected[0].Resource)
                : string.Format("ต้องการยกเลิกการแชร์ทั้งหมด {0} รายการที่เลือกใช่หรือไม่?\n\nลิงก์ทั้งหมดจะใช้งานไม่ได้ถาวร", selected.Count);

            if (MessageBox.Show(confirmMsg, "ยืนยันการยกเลิก",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            int successCount = 0;
            var errors = new List<string>();
            foreach (var row in selected)
            {
                try
                {
                    _shareManagementController.RevokeExternalShare(row.Id, reason.Trim(), _currentUser);
                    successCount++;
                }
                catch (Exception ex)
                {
                    errors.Add(string.Format("{0}: {1}", row.Resource, ex.Message));
                }
            }

            ShowBatchResult(successCount, errors, "ยกเลิกการแชร์ภายนอกเรียบร้อยแล้ว {0} รายการ\n(ดูย้อนหลังได้ที่ \"ไม่ได้ใช้งาน\")");
            LoadShares();
        }

        private static string PromptInput(string prompt, string title)
        {
            using (var form = new Form())
            using (var lbl = new Label())
            using (var txt = new TextBox())
            using (var btnOk = new Button())
            using (var btnCancel = new Button())
            {
                form.Text = title;
                form.ClientSize = new Size(420, 150);
                form.FormBorderStyle = FormBorderStyle.FixedDialog;
                form.StartPosition = FormStartPosition.CenterParent;
                form.MaximizeBox = false;
                form.MinimizeBox = false;
                form.Font = new Font("Segoe UI", 9F);
                form.BackColor = Color.FromArgb(245, 248, 253);

                lbl.Text = prompt;
                lbl.Location = new Point(16, 16);
                lbl.Size = new Size(388, 32);

                txt.Location = new Point(16, 54);
                txt.Size = new Size(388, 23);

                btnOk.Text = "ตกลง";
                btnOk.DialogResult = DialogResult.OK;
                btnOk.Location = new Point(236, 96);
                btnOk.Size = new Size(80, 30);
                btnOk.BackColor = NavyDark;
                btnOk.ForeColor = Color.White;
                btnOk.FlatStyle = FlatStyle.Flat;

                btnCancel.Text = "ยกเลิก";
                btnCancel.DialogResult = DialogResult.Cancel;
                btnCancel.Location = new Point(324, 96);
                btnCancel.Size = new Size(80, 30);
                btnCancel.BackColor = Color.FromArgb(220, 228, 245);
                btnCancel.ForeColor = NavyDark;
                btnCancel.FlatStyle = FlatStyle.Flat;

                form.Controls.AddRange(new Control[] { lbl, txt, btnOk, btnCancel });
                form.AcceptButton = btnOk;
                form.CancelButton = btnCancel;

                return form.ShowDialog() == DialogResult.OK ? txt.Text : null;
            }
        }

        private void ShowBatchResult(int successCount, List<string> errors, string successTemplate)
        {
            if (errors.Count == 0 && successCount > 0)
            {
                MessageBox.Show(string.Format(successTemplate, successCount), "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else if (errors.Count > 0 && successCount > 0)
            {
                MessageBox.Show(string.Format("สำเร็จ {0} รายการ แต่มี {1} รายการที่ล้มเหลว:\n\n{2}",
                    successCount, errors.Count, string.Join("\n", errors)),
                    "เสร็จสิ้นบางส่วน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            else if (errors.Count > 0)
            {
                MessageBox.Show(string.Format("เกิดข้อผิดพลาด {0} รายการ:\n\n{1}", errors.Count, string.Join("\n", errors)),
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ── Name lookups (cached per LoadShares call) ─────────────────────────

        private string Cached(string key, Func<string> resolver)
        {
            if (!_nameCache.TryGetValue(key, out var val))
            {
                try { val = resolver() ?? "-"; }
                catch { val = "-"; }
                _nameCache[key] = val;
            }
            return val;
        }

        private string ResolveUserName(int userId)
        {
            if (_shareLookupController == null)
                return string.Format("User #{0}", userId);

            return Cached("U" + userId, () => _shareLookupController.ResolveGranterName(userId));
        }

        private string ResolvePrincipalName(int principalType, int principalId)
        {
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

        private string ResolveFolderName(int folderId)
        {
            return "📁 " + Cached("D" + folderId, () =>
            {
                var folder = _shareLookupController != null ? _shareLookupController.GetFolderById(folderId) : null;
                return folder != null && !string.IsNullOrEmpty(folder.FolderName)
                    ? folder.FolderName
                    : string.Format("Folder #{0}", folderId);
            });
        }

        private string ResolveExternalFolderName(int? folderId)
        {
            if (folderId == null) return "(ไม่ระบุ)";
            return string.Format("Folder #{0}", folderId);
        }

        // ── Grid row models ──────────────────────────────────────────────────

        private sealed class InternalShareGroupRow
        {
            public string GroupKey { get; set; }
            public int? FileID { get; set; }
            public int? FolderID { get; set; }
            public bool IsFolder { get; set; }
            public string Resource { get; set; }
            public string Sharer { get; set; }
            public string Target { get; set; }
            public int RecipientCount { get; set; }
            public string Rights { get; set; }
            public string Status { get; set; }
            public DateTime Created { get; set; }
            public string Expiry { get; set; }
            public string Ended { get; set; }
            public string Note { get; set; }
            public bool IsActive { get; set; }
            public bool HasActive { get; set; }
            public bool HasPending { get; set; }

            public List<FilePermission> Permissions { get; set; } = new List<FilePermission>();
            public List<InternalShareRequestItem> Requests { get; set; } = new List<InternalShareRequestItem>();
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
