using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.DMS
{
    public sealed class FileShareRecipientRow
    {
        public bool IsPermission { get; set; }
        public FilePermission Permission { get; set; }
        public InternalShareRequestItem Request { get; set; }

        public string PrincipalTypeName { get; set; }
        public string PrincipalName { get; set; }
        public string RightsText { get; set; }
        public string ExpiryText { get; set; }
        public string StatusText { get; set; }
        public Color StatusColor { get; set; }
        public DateTime Date { get; set; }
        public string Note { get; set; }
        public bool IsActive { get; set; }
        public bool IsPending { get; set; }
    }

    public partial class FileShareDetailDialog : Form
    {
        private readonly string _resourceName;
        private readonly string _sharerName;
        private readonly int? _fileId;
        private readonly int? _folderId;
        private readonly IShareManagementController _controller;
        private readonly UserContext _currentUser;
        private readonly IShareLookupController _lookupController;
        private readonly IShareCommandController _shareCommandController;
        private readonly IUserAccountLookupController _userAccountLookupController;
        private readonly IUserAccessOrgLookupController _orgLookupController;
        private readonly IFolderReadLookupController _folderReadLookupController;
        private List<FileShareRecipientRow> _rows = new List<FileShareRecipientRow>();

        public bool WasModified { get; private set; }

        public FileShareDetailDialog(
            string resourceName,
            string sharerName,
            int? fileId,
            int? folderId,
            IEnumerable<FilePermission> permissions,
            IEnumerable<InternalShareRequestItem> requests,
            IShareManagementController controller,
            UserContext currentUser,
            IShareLookupController lookupController = null,
            IShareCommandController shareCommandController = null,
            IUserAccountLookupController userAccountLookupController = null,
            IUserAccessOrgLookupController orgLookupController = null,
            IFolderReadLookupController folderReadLookupController = null)
        {
            _resourceName = resourceName ?? "รายการที่แชร์";
            _sharerName = sharerName ?? "-";
            _fileId = fileId;
            _folderId = folderId;
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));
            _lookupController = lookupController;
            _shareCommandController = shareCommandController;
            _userAccountLookupController = userAccountLookupController;
            _orgLookupController = orgLookupController;
            _folderReadLookupController = folderReadLookupController;

            InitializeComponent();

            lblResourceName.Text = _resourceName;
            lblSharer.Text = _sharerName;

            btnAddRecipient.Visible = _shareCommandController != null && _userAccountLookupController != null && _orgLookupController != null;

            BuildRows(permissions, requests);
            PopulateGrid();
        }

        private void BuildRows(IEnumerable<FilePermission> permissions, IEnumerable<InternalShareRequestItem> requests)
        {
            _rows = new List<FileShareRecipientRow>();

            if (permissions != null)
            {
                foreach (var p in permissions)
                {
                    bool isRevoked = p.RevokedDate.HasValue;
                    bool isExpired = !isRevoked && p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today;
                    bool isActive = !isRevoked && !isExpired;

                    string statusText;
                    Color statusColor;
                    if (isActive)
                    {
                        statusText = "✅ ใช้งานอยู่";
                        statusColor = Color.FromArgb(0, 100, 45);
                    }
                    else if (isRevoked)
                    {
                        statusText = "🚫 ยกเลิกแล้ว";
                        statusColor = Color.FromArgb(120, 120, 120);
                    }
                    else
                    {
                        statusText = "⌛ หมดอายุ";
                        statusColor = Color.FromArgb(185, 28, 28);
                    }

                    var rights = new List<string>();
                    if (p.CanRead) rights.Add("อ่าน");
                    if (p.CanDownload) rights.Add("ดาวน์โหลด");
                    if (p.CanWrite || p.CanModify) rights.Add("แก้ไข");
                    if (p.CanDelete) rights.Add("ลบ");

                    string targetName = ResolveTargetName(p.PrincipalType, p.PrincipalID);

                    _rows.Add(new FileShareRecipientRow
                    {
                        IsPermission = true,
                        Permission = p,
                        PrincipalTypeName = GetPrincipalTypeName(p.PrincipalType),
                        PrincipalName = targetName,
                        RightsText = rights.Count > 0 ? string.Join(", ", rights) : "-",
                        ExpiryText = p.ExpiryDate.HasValue ? p.ExpiryDate.Value.ToString("dd/MM/yyyy") : "ไม่มีกำหนด",
                        StatusText = statusText,
                        StatusColor = statusColor,
                        Date = p.GrantedDate,
                        Note = isRevoked ? (p.RevokeReason ?? "-") : "-",
                        IsActive = isActive,
                        IsPending = false
                    });
                }
            }

            if (requests != null)
            {
                foreach (var r in requests)
                {
                    bool isPending = r.RequestStatus == 1;
                    string statusText;
                    Color statusColor;

                    if (isPending)
                    {
                        statusText = "⏳ รออนุมัติ";
                        statusColor = Color.FromArgb(0, 51, 102);
                    }
                    else if (r.IsWithdrawn)
                    {
                        statusText = "↩️ ถอนคำขอ";
                        statusColor = Color.FromArgb(120, 120, 120);
                    }
                    else if (r.RequestStatus == 3)
                    {
                        statusText = "❌ ไม่อนุมัติ";
                        statusColor = Color.FromArgb(185, 28, 28);
                    }
                    else
                    {
                        statusText = "⌛ หมดอายุ";
                        statusColor = Color.FromArgb(120, 120, 120);
                    }

                    var rights = new List<string>();
                    if (r.CanRead) rights.Add("อ่าน");
                    if (r.CanDownload) rights.Add("ดาวน์โหลด");
                    if (r.CanWrite) rights.Add("แก้ไข");
                    if (r.CanDelete) rights.Add("ลบ");

                    string targetName = ResolveTargetName(r.PrincipalType, r.PrincipalID);

                    _rows.Add(new FileShareRecipientRow
                    {
                        IsPermission = false,
                        Request = r,
                        PrincipalTypeName = GetPrincipalTypeName(r.PrincipalType),
                        PrincipalName = targetName,
                        RightsText = rights.Count > 0 ? string.Join(", ", rights) : "-",
                        ExpiryText = r.ShareExpiry.HasValue ? r.ShareExpiry.Value.ToString("dd/MM/yyyy") : "ไม่มีกำหนด",
                        StatusText = statusText,
                        StatusColor = statusColor,
                        Date = r.RequestedDate,
                        Note = r.ResolveRemark ?? "-",
                        IsActive = false,
                        IsPending = isPending
                    });
                }
            }

            _rows = _rows.OrderByDescending(r => r.IsActive)
                         .ThenByDescending(r => r.IsPending)
                         .ThenByDescending(r => r.Date)
                         .ToList();
        }

        private void PopulateGrid()
        {
            dgvRecipients.Rows.Clear();
            foreach (var r in _rows)
            {
                int rowIndex = dgvRecipients.Rows.Add(
                    r.PrincipalTypeName,
                    r.PrincipalName,
                    r.RightsText,
                    r.ExpiryText,
                    r.StatusText,
                    r.Date.ToString("dd/MM/yyyy HH:mm"),
                    r.Note);

                var dgvRow = dgvRecipients.Rows[rowIndex];
                dgvRow.Tag = r;
                dgvRow.Cells[4].Style.ForeColor = r.StatusColor;
                dgvRow.Cells[4].Style.Font = new Font("Segoe UI", 9F, FontStyle.Bold);
            }

            lblRecipientCount.Text = string.Format("{0} รายการ", _rows.Count);
        }

        private string GetPrincipalTypeName(int principalType)
        {
            switch (principalType)
            {
                case 1: return "👤 ผู้ใช้";
                case 2: return "👥 บทบาท";
                case 3: return "🏢 หน่วยงาน";
                case 4: return "👥 กลุ่ม";
                default: return "❓ อื่นๆ";
            }
        }

        private string ResolveTargetName(int principalType, int principalId)
        {
            if (_lookupController != null)
            {
                try
                {
                    string name = _lookupController.ResolvePrincipalName(principalType, principalId);
                    if (!string.IsNullOrEmpty(name)) return name;
                }
                catch { }
            }

            return string.Format("{0} #{1}", GetPrincipalTypeName(principalType), principalId);
        }

        private void PnlInfo_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var pen = new Pen(Color.FromArgb(200, 215, 235), 1f))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, pnlInfo.Width - 1, pnlInfo.Height - 1);
            }
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            var selectedRows = dgvRecipients.SelectedRows.Cast<DataGridViewRow>()
                .Select(r => r.Tag as FileShareRecipientRow)
                .Where(r => r != null && r.IsPermission && r.IsActive)
                .ToList();

            if (selectedRows.Count == 0)
            {
                MessageBox.Show(
                    "กรุณาเลือกรายการที่ 'ใช้งานอยู่' เพื่อขอปรับสิทธิ์",
                    "แจ้งเตือน",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var permissions = selectedRows.Select(r => r.Permission).ToList();
            string targetNames = string.Join(", ", selectedRows.Select(r => r.PrincipalName));

            using (var configDlg = new SharePermissionConfigDialog(
                permissions,
                _resourceName,
                targetNames,
                _sharerName,
                _controller,
                _currentUser))
            {
                if (configDlg.ShowDialog(this) == DialogResult.OK)
                {
                    WasModified = true;
                    MessageBox.Show(
                        "ส่งคำขอปรับสิทธิ์เรียบร้อยแล้ว รายการจะได้รับการอัปเดตเมื่อได้รับการอนุมัติ",
                        "ส่งคำขอสำเร็จ",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
            }
        }

        private void BtnRevoke_Click(object sender, EventArgs e)
        {
            var selectedRows = dgvRecipients.SelectedRows.Cast<DataGridViewRow>()
                .Select(r => r.Tag as FileShareRecipientRow)
                .Where(r => r != null && r.IsPermission && r.IsActive)
                .ToList();

            if (selectedRows.Count == 0)
            {
                MessageBox.Show(
                    "กรุณาเลือกรายการที่ 'ใช้งานอยู่' ที่ต้องการยกเลิกการแชร์",
                    "แจ้งเตือน",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                string.Format("ต้องการยกเลิกการแชร์จำนวน {0} รายการใช่หรือไม่?\nผู้รับจะไม่สามารถเข้าถึงไฟล์ได้ทันที", selectedRows.Count),
                "ยืนยันการยกเลิก",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            var opItems = selectedRows.Select(r => new OperationProgressItem
            {
                Title = string.Format("ยกเลิกสิทธิ์ของ {0}", r.PrincipalName),
                Tag = r.Permission.FilePermissionID
            }).ToList();

            using (var progress = new OperationProgressDialog(
                "ยกเลิกการแชร์",
                string.Format("กำลังยกเลิกการแชร์ {0} รายการ...", opItems.Count),
                "กำลังเพิกถอนสิทธิ์การเข้าถึง",
                opItems,
                item =>
                {
                    int permId = (int)item.Tag;
                    _controller.RevokeInternalShare(permId, _currentUser);
                    return null;
                }))
            {
                if (progress.ShowDialog(this) == DialogResult.OK)
                {
                    WasModified = true;
                    // Mark selected rows revoked in UI
                    foreach (var r in selectedRows)
                    {
                        r.IsActive = false;
                        r.StatusText = "🚫 ยกเลิกแล้ว";
                        r.StatusColor = Color.FromArgb(120, 120, 120);
                        r.Note = "Revoked by Sharer";
                    }
                    PopulateGrid();
                }
            }
        }

        private void BtnWithdraw_Click(object sender, EventArgs e)
        {
            var selectedRows = dgvRecipients.SelectedRows.Cast<DataGridViewRow>()
                .Select(r => r.Tag as FileShareRecipientRow)
                .Where(r => r != null && !r.IsPermission && r.IsPending)
                .ToList();

            if (selectedRows.Count == 0)
            {
                MessageBox.Show(
                    "กรุณาเลือกคำขอที่ 'รออนุมัติ' ที่ต้องการถอนคำขอ",
                    "แจ้งเตือน",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            var confirm = MessageBox.Show(
                string.Format("ต้องการถอนคำขอแชร์จำนวน {0} รายการใช่หรือไม่?", selectedRows.Count),
                "ยืนยันการถอนคำขอ",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            // Group by RequestID to avoid duplicate withdraw calls
            var distinctRequestIds = selectedRows.Select(r => r.Request.RequestID).Distinct().ToList();
            var opItems = distinctRequestIds.Select(reqId => new OperationProgressItem
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
                    _controller.WithdrawInternalShareRequest(reqId, _currentUser);
                    return null;
                }))
            {
                if (progress.ShowDialog(this) == DialogResult.OK)
                {
                    WasModified = true;
                    foreach (var r in selectedRows)
                    {
                        r.IsPending = false;
                        r.StatusText = "↩️ ถอนคำขอ";
                        r.StatusColor = Color.FromArgb(120, 120, 120);
                    }
                    PopulateGrid();
                }
            }
        }

        private void BtnAddRecipient_Click(object sender, EventArgs e)
        {
            if (_shareCommandController == null || _userAccountLookupController == null || _orgLookupController == null)
            {
                MessageBox.Show("ไม่สามารถเปิดหน้าต่างแชร์ได้เนื่องจากระบบยังไม่ได้กำหนด Controller ที่จำเป็น",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            List<FileDocument> files = null;
            Folder folder = null;

            if (_fileId.HasValue && _fileId.Value > 0)
            {
                var fileDoc = _lookupController?.GetFileById(_fileId.Value);
                if (fileDoc == null)
                {
                    fileDoc = new FileDocument
                    {
                        FileID = _fileId.Value,
                        OriginalFileName = _resourceName
                    };
                }
                files = new List<FileDocument> { fileDoc };
            }
            else if (_folderId.HasValue && _folderId.Value > 0)
            {
                folder = _lookupController?.GetFolderById(_folderId.Value);
                if (folder == null)
                {
                    folder = new Folder
                    {
                        FolderID = _folderId.Value,
                        FolderName = _resourceName
                    };
                }
            }

            if ((files == null || files.Count == 0) && folder == null)
            {
                MessageBox.Show("ไม่พบข้อมูลไฟล์หรือโฟลเดอร์สำหรับแบ่งปัน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
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
                    WasModified = true;
                    ReloadRecipients();
                }
            }
        }

        private void ReloadRecipients()
        {
            try
            {
                bool isAdmin = _currentUser != null && _currentUser.AppRole == 4;
                var allPerms = isAdmin
                    ? _controller.GetAllInternalShares(_currentUser)
                    : _controller.GetInternalSharesForUser(_currentUser.UserID, _currentUser);

                var allReqs = _controller.GetInternalShareRequestsForUser(_currentUser.UserID, _currentUser);

                IEnumerable<FilePermission> matchedPerms = Enumerable.Empty<FilePermission>();
                IEnumerable<InternalShareRequestItem> matchedReqs = Enumerable.Empty<InternalShareRequestItem>();

                if (_fileId.HasValue && _fileId.Value > 0)
                {
                    if (allPerms != null)
                        matchedPerms = allPerms.Where(p => p.FileID == _fileId.Value);
                    if (allReqs != null)
                        matchedReqs = allReqs.Where(r => !r.IsFolder && r.FileID == _fileId.Value);
                }
                else if (_folderId.HasValue && _folderId.Value > 0)
                {
                    if (allReqs != null)
                        matchedReqs = allReqs.Where(r => r.IsFolder && r.FolderID == _folderId.Value);
                }

                BuildRows(matchedPerms, matchedReqs);
                PopulateGrid();
            }
            catch
            {
                // Silently keep existing rows if reload encounters an issue
            }
        }
    }
}
