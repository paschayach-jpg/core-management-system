using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.DMS
{
    public partial class ShareRequestDetailDialog : Form
    {
        public ShareRequestDetailDialog(ApprovalRequest request, IEnumerable<ApprovalRequestItem> items, Func<int, int, string> resolvePrincipalName = null, Func<int, string> resolveFileName = null, Func<int, string> resolveFolderName = null)
        {
            InitializeComponent();
            SetupGrid();

            bool isPermissionChange = request != null &&
                string.Equals(request.RequestType, "SHARE_PERMISSION_CHANGE", StringComparison.OrdinalIgnoreCase);

            if (isPermissionChange)
            {
                lblHeaderTitle.Text = "รายละเอียดคำขอปรับสิทธิ์การแชร์";
                lblHeaderSubtitle.Text = "รายชื่อผู้รับสิทธิ์และระดับการเข้าถึงที่ขอปรับปรุง (รอการอนุมัติ)";
                Text = "รายละเอียดคำขอปรับสิทธิ์การแชร์";
            }

            if (request != null)
            {
                lblSubject.Text = "คำขอ: " + (request.Subject ?? "—");
                string userRemark = ExtractUserRemark(request.Remark);
                lblRemark.Text = string.IsNullOrEmpty(userRemark) ? "หมายเหตุ: —" : ("หมายเหตุ: " + userRemark);
            }

            if (items != null)
            {
                foreach (var item in items)
                {
                    string pType = PrincipalTypeToString(item.PrincipalType);
                    string pName = resolvePrincipalName != null
                        ? resolvePrincipalName(item.PrincipalType, item.PrincipalID)
                        : (item.PrincipalName ?? string.Format("{0} #{1}", pType, item.PrincipalID));

                    string resource = "—";
                    if (item.FileID.HasValue && item.FileID.Value > 0)
                    {
                        resource = resolveFileName != null ? resolveFileName(item.FileID.Value) : ("File #" + item.FileID.Value);
                    }
                    else if (item.FolderID.HasValue && item.FolderID.Value > 0)
                    {
                        resource = resolveFolderName != null ? resolveFolderName(item.FolderID.Value) : ("Folder #" + item.FolderID.Value);
                    }

                    string expiryStr = item.ExpiryDate.HasValue
                        ? item.ExpiryDate.Value.ToString("dd/MM/yyyy")
                        : "ไม่มีกำหนด";

                    dgvItems.Rows.Add(
                        pName,
                        pType,
                        resource,
                        item.CanRead,
                        item.CanWrite,
                        item.CanDelete,
                        item.CanDownload,
                        expiryStr
                    );
                }
            }
        }

        private void SetupGrid()
        {
            dgvItems.AutoGenerateColumns = false;
            dgvItems.Columns.Clear();

            var colTarget = new DataGridViewTextBoxColumn
            {
                Name = "colTarget",
                HeaderText = "ชื่อผู้รับสิทธิ์",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 30
            };

            var colType = new DataGridViewTextBoxColumn
            {
                Name = "colType",
                HeaderText = "ประเภท",
                Width = 90
            };

            var colResource = new DataGridViewTextBoxColumn
            {
                Name = "colResource",
                HeaderText = "รายการ",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 30
            };

            var colRead = new DataGridViewCheckBoxColumn
            {
                Name = "colRead",
                HeaderText = "ดู",
                Width = 50
            };

            var colWrite = new DataGridViewCheckBoxColumn
            {
                Name = "colWrite",
                HeaderText = "แก้ไข",
                Width = 55
            };

            var colDelete = new DataGridViewCheckBoxColumn
            {
                Name = "colDelete",
                HeaderText = "ลบ",
                Width = 50
            };

            var colDownload = new DataGridViewCheckBoxColumn
            {
                Name = "colDownload",
                HeaderText = "ดาวน์โหลด",
                Width = 75
            };

            var colExpiry = new DataGridViewTextBoxColumn
            {
                Name = "colExpiry",
                HeaderText = "หมดอายุ",
                Width = 100
            };

            dgvItems.Columns.AddRange(colTarget, colType, colResource, colRead, colWrite, colDelete, colDownload, colExpiry);
        }

        private static string PrincipalTypeToString(int type)
        {
            switch (type)
            {
                case 1: return "ผู้ใช้งาน";
                case 2: return "บทบาท";
                case 3: return "ฝ่าย/แผนก";
                case 4: return "กลุ่ม";
                default: return "User";
            }
        }

        private static string ExtractUserRemark(string remark)
        {
            if (string.IsNullOrEmpty(remark)) return null;
            int idx = remark.IndexOf("UserRemark:", StringComparison.OrdinalIgnoreCase);
            if (idx < 0) return null;

            string b64 = remark.Substring(idx + "UserRemark:".Length).Trim();
            int endIdx = b64.IndexOf(' ');
            if (endIdx >= 0) b64 = b64.Substring(0, endIdx);

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
}
