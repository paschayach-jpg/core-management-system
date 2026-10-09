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
    /// <summary>
    /// Dialog for requesting permission changes and expiry date modifications on active internal FilePermissions.
    /// Sends a SHARE_PERMISSION_CHANGE approval request requiring approver sign-off.
    /// </summary>
    public partial class SharePermissionConfigDialog : Form
    {
        private readonly List<FilePermission> _permissions;
        private readonly IShareManagementController _controller;
        private readonly UserContext _currentUser;
        private readonly string _resourceName;

        public SharePermissionConfigDialog(
            FilePermission permission,
            string resourceName,
            string targetName,
            string sharerName,
            IShareManagementController controller,
            UserContext currentUser)
            : this(new List<FilePermission> { permission ?? throw new ArgumentNullException(nameof(permission)) },
                   resourceName, targetName, sharerName, controller, currentUser)
        {
        }

        public SharePermissionConfigDialog(
            IList<FilePermission> permissions,
            string resourceName,
            string targetName,
            string sharerName,
            IShareManagementController controller,
            UserContext currentUser)
        {
            if (permissions == null || permissions.Count == 0)
                throw new ArgumentException("ต้องมีรายการสิทธิ์อย่างน้อย 1 รายการ", nameof(permissions));

            _permissions = permissions.ToList();
            _resourceName = resourceName;
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

            InitializeComponent();

            var firstPerm = _permissions[0];
            lblFileName.Text = resourceName ?? string.Format("File #{0}", firstPerm.FileID);
            
            if (_permissions.Count == 1)
            {
                lblTarget.Text = targetName ?? string.Format("Principal #{0}", firstPerm.PrincipalID);
            }
            else
            {
                lblTarget.Text = string.Format("เลือกทั้งหมด {0} ผู้รับ ({1})", _permissions.Count, targetName ?? "");
            }

            lblSharer.Text = sharerName ?? string.Format("User #{0}", firstPerm.GrantedBy);
            lblGrantedDate.Text = firstPerm.GrantedDate.ToString("dd/MM/yyyy HH:mm");

            // Populate current permissions (from first item)
            chkRead.Checked = firstPerm.CanRead;
            chkDownload.Checked = firstPerm.CanDownload;
            chkWrite.Checked = firstPerm.CanWrite || firstPerm.CanModify;
            chkDelete.Checked = firstPerm.CanDelete;

            // Populate expiry
            if (firstPerm.ExpiryDate.HasValue)
            {
                rbCustomExpiry.Checked = true;
                dtpExpiry.MinDate = DateTime.Today;
                if (firstPerm.ExpiryDate.Value.Date >= DateTime.Today)
                    dtpExpiry.Value = firstPerm.ExpiryDate.Value.Date;
                else
                    dtpExpiry.Value = DateTime.Today.AddDays(7);
                dtpExpiry.Enabled = true;
            }
            else
            {
                rbNoExpiry.Checked = true;
                dtpExpiry.MinDate = DateTime.Today;
                dtpExpiry.Value = DateTime.Today.AddDays(30);
                dtpExpiry.Enabled = false;
            }
        }

        private void PnlHeaderIcon_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            Color navyMid = Color.FromArgb(50, 90, 150);
            e.Graphics.FillEllipse(new SolidBrush(navyMid), 0, 0, 35, 35);
            using (var f = new Font("Segoe UI", 13f, FontStyle.Bold))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
            {
                e.Graphics.DrawString("⚙️", f, Brushes.White, new RectangleF(0, 0, 36, 36), sf);
            }
        }

        private void PnlInfo_Paint(object sender, PaintEventArgs e)
        {
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            using (var pen = new Pen(Color.FromArgb(200, 215, 235), 1f))
            {
                e.Graphics.DrawRectangle(pen, 0, 0, pnlInfo.Width - 1, pnlInfo.Height - 1);
            }
        }

        private void RbExpiry_CheckedChanged(object sender, EventArgs e)
        {
            dtpExpiry.Enabled = rbCustomExpiry.Checked;
        }

        private void BtnSave_Click(object sender, EventArgs e)
        {
            if (!chkRead.Checked && !chkDownload.Checked && !chkWrite.Checked && !chkDelete.Checked)
            {
                MessageBox.Show(
                    "กรุณากำหนดสิทธิ์อย่างน้อย 1 รายการ\n(หากต้องการเพิกถอนการแชร์ทั้งหมด ให้ใช้ปุ่ม 'ยกเลิกการแชร์')",
                    "แจ้งเตือน",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            string remark = txtRemark.Text.Trim();
            if (string.IsNullOrWhiteSpace(remark))
            {
                MessageBox.Show(
                    "กรุณาระบุเหตุผลในการขอปรับสิทธิ์",
                    "แจ้งเตือน",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                txtRemark.Focus();
                return;
            }

            try
            {
                var changeItems = _permissions.Select(p => new SharePermissionChangeItem
                {
                    FilePermissionID = p.FilePermissionID,
                    PrincipalType = p.PrincipalType,
                    PrincipalID = p.PrincipalID,
                    CanRead = chkRead.Checked,
                    CanDownload = chkDownload.Checked,
                    CanWrite = chkWrite.Checked,
                    CanDelete = chkDelete.Checked,
                    ExpiryDate = rbCustomExpiry.Checked ? (DateTime?)dtpExpiry.Value.Date : null
                }).ToList();

                var req = new SubmitSharePermissionChangeRequest
                {
                    FileID = _permissions[0].FileID > 0 ? (int?)_permissions[0].FileID : null,
                    Remark = remark,
                    Items = changeItems
                };

                // Asynchronous progress via OperationProgressDialog
                var opItems = new List<OperationProgressItem>
                {
                    new OperationProgressItem
                    {
                        Title = string.Format("ส่งคำขอปรับสิทธิ์ {0} ({1} ผู้รับ)", _resourceName ?? "ไฟล์", _permissions.Count),
                        Tag = req
                    }
                };

                using (var progress = new OperationProgressDialog(
                    "ขอปรับสิทธิ์การแชร์",
                    "กำลังส่งคำขอปรับสิทธิ์...",
                    "กำลังส่งคำขอไปยังระบบอนุมัติ",
                    opItems,
                    item =>
                    {
                        var changeReq = (SubmitSharePermissionChangeRequest)item.Tag;
                        var result = _controller.SubmitSharePermissionChange(changeReq, _currentUser);
                        if (!result.Success)
                            return result.Message ?? "เกิดข้อผิดพลาดในการส่งคำขอ";
                        return null;
                    }))
                {
                    if (progress.ShowDialog(this) == DialogResult.OK)
                    {
                        this.DialogResult = DialogResult.OK;
                        this.Close();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("เกิดข้อผิดพลาดในการส่งคำขอปรับสิทธิ์: {0}", ex.Message),
                    "ข้อผิดพลาด",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
