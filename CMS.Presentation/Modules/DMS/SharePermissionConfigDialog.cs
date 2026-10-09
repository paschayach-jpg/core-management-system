using System;
using System.Drawing;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.DMS
{
    /// <summary>
    /// Dialog for modifying permissions and expiry date of an active internal FilePermission.
    /// Accessible by the share owner (Sharer) or System Administrators.
    /// </summary>
    public partial class SharePermissionConfigDialog : Form
    {
        private readonly FilePermission _permission;
        private readonly IShareManagementController _controller;
        private readonly UserContext _currentUser;

        public SharePermissionConfigDialog(
            FilePermission permission,
            string resourceName,
            string targetName,
            string sharerName,
            IShareManagementController controller,
            UserContext currentUser)
        {
            _permission = permission ?? throw new ArgumentNullException(nameof(permission));
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _currentUser = currentUser ?? throw new ArgumentNullException(nameof(currentUser));

            InitializeComponent();

            lblFileName.Text = resourceName ?? string.Format("File #{0}", permission.FileID);
            lblTarget.Text = targetName ?? string.Format("Principal #{0}", permission.PrincipalID);
            lblSharer.Text = sharerName ?? string.Format("User #{0}", permission.GrantedBy);
            lblGrantedDate.Text = permission.GrantedDate.ToString("dd/MM/yyyy HH:mm");

            // Populate current permissions
            chkRead.Checked = permission.CanRead;
            chkDownload.Checked = permission.CanDownload;
            chkWrite.Checked = permission.CanWrite || permission.CanModify;
            chkDelete.Checked = permission.CanDelete;

            // Populate expiry
            if (permission.ExpiryDate.HasValue)
            {
                rbCustomExpiry.Checked = true;
                dtpExpiry.MinDate = DateTime.Today;
                if (permission.ExpiryDate.Value.Date >= DateTime.Today)
                    dtpExpiry.Value = permission.ExpiryDate.Value.Date;
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
                    "กรุณากำหนดสิทธิ์อย่างน้อย 1 รายการ\n(หากต้องการเพิกถอนการแชร์ทั้งหมด ให้ใช้ปุ่ม 'ยกเลิกการแชร์' จากหน้าหลัก)",
                    "แจ้งเตือน",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Warning);
                return;
            }

            try
            {
                var request = new UpdateSharePermissionRequest
                {
                    FilePermissionID = _permission.FilePermissionID,
                    CanRead = chkRead.Checked,
                    CanDownload = chkDownload.Checked,
                    CanWrite = chkWrite.Checked,
                    CanDelete = chkDelete.Checked,
                    ExpiryDate = rbCustomExpiry.Checked ? (DateTime?)dtpExpiry.Value.Date : null,
                    Remark = txtRemark.Text.Trim()
                };

                _controller.UpdateInternalSharePermission(request, _currentUser);

                MessageBox.Show("ปรับปรุงสิทธิ์การแชร์เรียบร้อยแล้ว", "สำเร็จ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    string.Format("เกิดข้อผิดพลาดในการบันทึกสิทธิ์: {0}", ex.Message),
                    "ข้อผิดพลาด",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }
    }
}
