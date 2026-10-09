using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Presentation.Helpers;
using CMS.Presentation.Modules.DMS;
using CMS.Services.DTOs;

namespace CMS.Presentation
{
    public partial class ShareDialogInternal : Form
    {
        private readonly IShareCommandController _shareCommandController;
        private readonly IUserAccountLookupController _userAccountLookupController;
        private readonly IUserAccessOrgLookupController _orgLookupController;
        private readonly IFolderReadLookupController _folderReadLookupController;
        private readonly UserContext _currentUser;

        private List<FileDocument> _selectedFiles;
        private Folder _selectedFolder;
        private Folder _parentFolder;
        private List<PrincipalItem> _allPrincipals = new List<PrincipalItem>();

        private int _lastHoverIndexLeft = -1;

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Auto)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int Msg, int wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        public ShareDialogInternal(
            IShareCommandController shareCommandController,
            IUserAccountLookupController userAccountLookupController,
            IUserAccessOrgLookupController orgLookupController,
            UserContext currentUser,
            IFolderReadLookupController folderReadLookupController = null,
            List<FileDocument> files = null,
            Folder folder = null,
            Folder parentFolder = null)
        {
            InitializeComponent();
            _shareCommandController = shareCommandController;
            _userAccountLookupController = userAccountLookupController;
            _orgLookupController = orgLookupController;
            _folderReadLookupController = folderReadLookupController;
            _currentUser = currentUser;
            _selectedFiles = files ?? new List<FileDocument>();
            _selectedFolder = folder;
            _parentFolder = parentFolder;

            SetupGrid();
            WireEvents();
            InitializeControls();
        }

        private void SetupGrid()
        {
            dgvSelected.AutoGenerateColumns = false;
            dgvSelected.Columns.Clear();

            var colName = new DataGridViewTextBoxColumn
            {
                Name = "colName",
                HeaderText = "ชื่อผู้รับสิทธิ์",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                FillWeight = 32,
                ReadOnly = true
            };

            var colType = new DataGridViewTextBoxColumn
            {
                Name = "colType",
                HeaderText = "ประเภท",
                Width = 75,
                ReadOnly = true
            };

            var colRead = new DataGridViewCheckBoxColumn
            {
                Name = "colRead",
                HeaderText = "ดู",
                Width = 40
            };

            var colWrite = new DataGridViewCheckBoxColumn
            {
                Name = "colWrite",
                HeaderText = "แก้ไข",
                Width = 44
            };

            var colDelete = new DataGridViewCheckBoxColumn
            {
                Name = "colDelete",
                HeaderText = "ลบ",
                Width = 40
            };

            var colDownload = new DataGridViewCheckBoxColumn
            {
                Name = "colDownload",
                HeaderText = "ดาวน์โหลด",
                Width = 72,
                Visible = (_selectedFolder == null && _selectedFiles.Count > 0)
            };

            var colExpiry = new DataGridViewTextBoxColumn
            {
                Name = "colExpiry",
                HeaderText = "หมดอายุ",
                Width = 95,
                ReadOnly = true
            };

            dgvSelected.Columns.AddRange(colName, colType, colRead, colWrite, colDelete, colDownload, colExpiry);
        }

        private void WireEvents()
        {
            btnOK.Click += (s, e) => HandleOK();
            btnAdd.Click += (s, e) => AddCurrentPrincipal();
            btnRemove.Click += (s, e) => RemoveSelectedItems();
            btnApplyToSelected.Click += (s, e) => ApplyDefaultsToSelectedRows();
            chkNoExpiry.CheckedChanged += (s, e) => dtpExpiry.Enabled = !chkNoExpiry.Checked;

            cboPrincipalType.SelectedIndexChanged += (s, e) =>
            {
                _allPrincipals.Clear();
                LoadPrincipals();
            };

            txtSearch.TextChanged += (s, e) =>
            {
                tmrSearchDebounce.Stop();
                tmrSearchDebounce.Start();
            };
            tmrSearchDebounce.Tick += (s, e) =>
            {
                tmrSearchDebounce.Stop();
                FilterPrincipals(txtSearch.Text);
            };
            txtSearch.KeyDown += TxtSearch_KeyDown;

            lstPrincipal.DoubleClick += (s, e) => AddCurrentPrincipal();
            lstPrincipal.KeyDown += LstPrincipal_KeyDown;
            lstPrincipal.MouseMove += LstPrincipal_MouseMove;

            dgvSelected.CurrentCellDirtyStateChanged += (s, e) =>
            {
                if (dgvSelected.IsCurrentCellDirty)
                    dgvSelected.CommitEdit(DataGridViewDataErrorContexts.Commit);
            };

            dgvSelected.CellDoubleClick += DgvSelected_CellDoubleClick;
            dgvSelected.KeyDown += DgvSelected_KeyDown;

            var ctxMenu = new ContextMenu();
            ctxMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("ลบออก", (s, e) => RemoveSelectedItems()));
            ctxMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("แก้ไขวันหมดอายุ...", (s, e) => EditSelectedRowExpiry()));
            dgvSelected.ContextMenu = ctxMenu;

            this.Load += (s, e) =>
            {
                SendMessage(txtSearch.Handle, EM_SETCUEBANNER, 1, "ค้นหาชื่อ, AD user, รหัสพนักงาน...");
                SendMessage(txtRemark.Handle, EM_SETCUEBANNER, 1, "ระบุหมายเหตุ หรือเหตุผลในการขออนุมัติ (ถ้ามี)...");
            };
        }

        private void pnlIcon_Paint(object sender, PaintEventArgs e)
        {
            Color navyMid = Color.FromArgb(50, 90, 150);
            e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
            e.Graphics.FillEllipse(new SolidBrush(navyMid), 0, 0, 35, 35);
            using (var f = new Font("Segoe UI", 15f, FontStyle.Bold))
            using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                e.Graphics.DrawString("🔗", f, Brushes.White, new RectangleF(0, 0, 36, 36), sf);
        }

        private void pnlLeftBorder_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(210, 218, 232), 1))
                e.Graphics.DrawRectangle(pen, 0, 0, pnlLeftBorder.Width - 1, pnlLeftBorder.Height - 1);
        }

        private void pnlRightBorder_Paint(object sender, PaintEventArgs e)
        {
            using (var pen = new Pen(Color.FromArgb(160, 195, 240), 1))
                e.Graphics.DrawRectangle(pen, 0, 0, pnlRightBorder.Width - 1, pnlRightBorder.Height - 1);
        }

        private void pnlFooter_Resize(object sender, EventArgs e)
        {
            btnOK.Left = pnlFooter.Width - 210;
            btnCancel.Left = pnlFooter.Width - 108;
            lblStatus.Width = pnlFooter.Width - 230;
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Down)
            {
                if (lstPrincipal.Items.Count > 0)
                {
                    lstPrincipal.Focus();
                    lstPrincipal.SelectedIndex = 0;
                    e.Handled = true;
                }
            }
            else if (e.KeyCode == Keys.Enter)
            {
                tmrSearchDebounce.Stop();
                FilterPrincipals(txtSearch.Text);

                if (lstPrincipal.Items.Count > 0)
                {
                    var item = lstPrincipal.Items[0] as PrincipalItem;
                    if (item != null)
                    {
                        AddToSelected(item);
                        txtSearch.Text = "";
                    }
                }
                e.SuppressKeyPress = true;
            }
        }

        private void LstPrincipal_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                AddCurrentPrincipal();
                e.SuppressKeyPress = true;
            }
        }

        private void DgvSelected_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                RemoveSelectedItems();
                e.SuppressKeyPress = true;
            }
        }

        private void DgvSelected_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (e.ColumnIndex == dgvSelected.Columns["colExpiry"].Index)
            {
                EditRowExpiry(e.RowIndex);
            }
        }

        private void EditSelectedRowExpiry()
        {
            if (dgvSelected.SelectedRows.Count == 0) return;
            EditRowExpiry(dgvSelected.SelectedRows[0].Index);
        }

        private void EditRowExpiry(int rowIndex)
        {
            if (rowIndex < 0 || rowIndex >= dgvSelected.Rows.Count) return;
            var row = dgvSelected.Rows[rowIndex];
            var tag = row.Tag as SelectedPrincipalRow;
            if (tag == null) return;

            using (var dlg = new ShareExpiryPickerDialog(tag.ExpiryDate))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    tag.ExpiryDate = dlg.SelectedExpiryDate;
                    row.Cells["colExpiry"].Value = tag.ExpiryDate.HasValue
                        ? tag.ExpiryDate.Value.ToString("dd/MM/yyyy")
                        : "ไม่มีกำหนด";
                }
            }
        }

        private void LstPrincipal_MouseMove(object sender, MouseEventArgs e)
        {
            int idx = lstPrincipal.IndexFromPoint(e.Location);
            if (idx != _lastHoverIndexLeft)
            {
                _lastHoverIndexLeft = idx;
                if (idx >= 0 && idx < lstPrincipal.Items.Count)
                {
                    var item = lstPrincipal.Items[idx] as PrincipalItem;
                    if (item != null && item.Data is UserAccount u)
                    {
                        string tip = string.Format("AD: {0}  ·  {1}  ·  {2}",
                            u.ADUserName ?? "—",
                            u.DepartmentName ?? "—",
                            u.PositionName ?? "—");
                        toolTipInfo.SetToolTip(lstPrincipal, tip);
                        return;
                    }
                }
                toolTipInfo.SetToolTip(lstPrincipal, string.Empty);
            }
        }

        private void InitializeControls()
        {
            if (_selectedFiles.Count > 0)
            {
                txtFiles.Text = string.Join(Environment.NewLine,
                    _selectedFiles.Select(f => "📄  " + f.OriginalFileName));
                chkDownload.Visible = true;
            }
            else if (_selectedFolder != null)
            {
                txtFiles.Text = _parentFolder != null
                    ? "📁  " + _parentFolder.FolderName + "  ›  " + _selectedFolder.FolderName
                    : "📁  " + _selectedFolder.FolderName;
                chkDownload.Visible = false;
            }
            else
            {
                txtFiles.Text = "(ไม่ระบุ)";
                chkDownload.Visible = false;
            }

            LoadPrincipals();
        }

        private void LoadPrincipals()
        {
            string principalType = cboPrincipalType.SelectedItem?.ToString() ?? "User";

            if (_allPrincipals.Count == 0)
            {
                try
                {
                    if (principalType == "User")
                    {
                        var users = (_userAccountLookupController?.GetAllUsers() ?? Enumerable.Empty<UserAccount>())
                            .Where(u => u.IsActive)
                            .OrderBy(u => UserDisplayHelper.NameOf(u), StringComparer.CurrentCulture);

                        foreach (var u in users)
                        {
                            _allPrincipals.Add(new PrincipalItem
                            {
                                Id = u.UserID,
                                Name = UserDisplayHelper.NameOf(u),
                                PrincipalType = "User",
                                Data = u
                            });
                        }
                    }
                    else if (principalType == "Department")
                    {
                        var depts = (_orgLookupController != null ? _orgLookupController.GetHierarchy() : Enumerable.Empty<OrgUnit>())
                            .Where(o => o.Tier == OrgUnitTier.Department)
                            .GroupBy(o => o.OrgUnitID)
                            .Select(g => g.First())
                            .GroupBy(o => o.Name, StringComparer.OrdinalIgnoreCase)
                            .Select(g => g.First())
                            .ToList();
                        foreach (var d in depts)
                            _allPrincipals.Add(new PrincipalItem { Id = d.OrgUnitID, Name = d.Name, PrincipalType = "Department", Data = d });
                    }
                    else if (principalType == "Role")
                    {
                        _allPrincipals.Add(new PrincipalItem { Id = 4, Name = "Admin",       PrincipalType = "Role" });
                        _allPrincipals.Add(new PrincipalItem { Id = 3, Name = "Manager",     PrincipalType = "Role" });
                        _allPrincipals.Add(new PrincipalItem { Id = 2, Name = "Sr. Officer", PrincipalType = "Role" });
                        _allPrincipals.Add(new PrincipalItem { Id = 1, Name = "Officer",     PrincipalType = "Role" });
                    }
                    else if (principalType == "Group")
                    {
                        _allPrincipals.Add(new PrincipalItem { Id = 1, Name = "Finance Team", PrincipalType = "Group" });
                        _allPrincipals.Add(new PrincipalItem { Id = 2, Name = "HR Team",      PrincipalType = "Group" });
                        _allPrincipals.Add(new PrincipalItem { Id = 3, Name = "IT Team",      PrincipalType = "Group" });
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error loading principals: " + ex.Message, "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }

            FilterPrincipals(txtSearch.Text);
        }

        private void FilterPrincipals(string filter)
        {
            var selectedKeys = new HashSet<string>(dgvSelected.Rows.Cast<DataGridViewRow>()
                .Select(r => r.Tag as SelectedPrincipalRow)
                .Where(t => t != null)
                .Select(t => t.PrincipalType + ":" + t.Id));

            lstPrincipal.BeginUpdate();
            lstPrincipal.Items.Clear();

            var items = _allPrincipals.Where(p =>
            {
                string key = p.PrincipalType + ":" + p.Id;
                if (selectedKeys.Contains(key)) return false;

                if (p.Data is UserAccount u)
                    return UserDisplayHelper.Matches(u, filter);

                return string.IsNullOrWhiteSpace(filter) || p.Name.IndexOf(filter, StringComparison.OrdinalIgnoreCase) >= 0;
            }).ToList();

            foreach (var item in items)
                lstPrincipal.Items.Add(item);

            lstPrincipal.DisplayMember = "Name";
            lstPrincipal.EndUpdate();

            string principalType = cboPrincipalType.SelectedItem?.ToString() ?? "User";
            string unitLabel = principalType == "User" ? "คน" : "รายการ";

            if (lstPrincipal.Items.Count == 0 && !string.IsNullOrWhiteSpace(filter))
                lblLeftHeader.Text = "  เลือกเป้าหมาย (ไม่พบรายชื่อ)";
            else
                lblLeftHeader.Text = string.Format("  เลือกเป้าหมาย ({0} {1})", lstPrincipal.Items.Count, unitLabel);
        }

        private void AddCurrentPrincipal()
        {
            if (lstPrincipal.SelectedItem is PrincipalItem item)
            {
                AddToSelected(item);
            }
        }

        private void AddToSelected(PrincipalItem item)
        {
            foreach (DataGridViewRow r in dgvSelected.Rows)
            {
                if (r.Tag is SelectedPrincipalRow tag && tag.Id == item.Id && tag.PrincipalType == item.PrincipalType)
                    return;
            }

            DateTime? defaultExpiry = chkNoExpiry.Checked ? (DateTime?)null : dtpExpiry.Value.Date;
            string expiryStr = defaultExpiry.HasValue ? defaultExpiry.Value.ToString("dd/MM/yyyy") : "ไม่มีกำหนด";
            bool defaultDownload = (_selectedFolder == null && _selectedFiles.Count > 0) && chkDownload.Checked;

            int rowIdx = dgvSelected.Rows.Add(
                item.Name,
                PrincipalTypeLabel(item.PrincipalType),
                chkRead.Checked,
                chkWrite.Checked,
                chkDelete.Checked,
                defaultDownload,
                expiryStr
            );

            dgvSelected.Rows[rowIdx].Tag = new SelectedPrincipalRow
            {
                Id              = item.Id,
                Name            = item.Name,
                PrincipalType   = item.PrincipalType,
                PrincipalTypeId = GetPrincipalTypeId(item.PrincipalType),
                ExpiryDate      = defaultExpiry,
                Data            = item.Data
            };

            UpdateSelectedCount();
            FilterPrincipals(txtSearch.Text);
        }

        private void RemoveSelectedItems()
        {
            var toRemove = dgvSelected.SelectedRows.Cast<DataGridViewRow>().ToList();
            if (toRemove.Count == 0 && dgvSelected.CurrentRow != null)
                toRemove.Add(dgvSelected.CurrentRow);

            if (toRemove.Count == 0) return;

            foreach (var row in toRemove)
            {
                if (row.Index >= 0 && row.Index < dgvSelected.Rows.Count)
                    dgvSelected.Rows.Remove(row);
            }

            UpdateSelectedCount();
            FilterPrincipals(txtSearch.Text);
        }

        private void ApplyDefaultsToSelectedRows()
        {
            if (dgvSelected.SelectedRows.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกแถวในตารางรายชื่อที่ต้องการตั้งค่า", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            DateTime? defaultExpiry = chkNoExpiry.Checked ? (DateTime?)null : dtpExpiry.Value.Date;
            string expiryStr = defaultExpiry.HasValue ? defaultExpiry.Value.ToString("dd/MM/yyyy") : "ไม่มีกำหนด";
            bool defaultDownload = (_selectedFolder == null && _selectedFiles.Count > 0) && chkDownload.Checked;

            foreach (DataGridViewRow row in dgvSelected.SelectedRows)
            {
                row.Cells["colRead"].Value = chkRead.Checked;
                row.Cells["colWrite"].Value = chkWrite.Checked;
                row.Cells["colDelete"].Value = chkDelete.Checked;
                if (_selectedFolder == null && _selectedFiles.Count > 0)
                    row.Cells["colDownload"].Value = defaultDownload;

                var tag = row.Tag as SelectedPrincipalRow;
                if (tag != null)
                {
                    tag.ExpiryDate = defaultExpiry;
                    row.Cells["colExpiry"].Value = expiryStr;
                }
            }
        }

        private void UpdateSelectedCount()
        {
            lblRightHeader.Text = string.Format("  รายชื่อที่เลือก ({0} รายการ)", dgvSelected.Rows.Count);
        }

        private void HandleOK()
        {
            if (_selectedFiles.Count == 0 && _selectedFolder == null)
            {
                lblStatus.Text = "ไม่พบไฟล์หรือโฟลเดอร์ที่ต้องการแบ่งปัน";
                return;
            }

            if (dgvSelected.Rows.Count == 0)
            {
                lblStatus.Text = "กรุณาเลือกผู้รับสิทธิ์อย่างน้อย 1 รายการ";
                return;
            }

            // Reset highlight
            foreach (DataGridViewRow r in dgvSelected.Rows)
                r.DefaultCellStyle.BackColor = Color.White;

            var recipients = new List<ShareRecipientItem>();

            for (int i = 0; i < dgvSelected.Rows.Count; i++)
            {
                var row = dgvSelected.Rows[i];
                var tag = row.Tag as SelectedPrincipalRow;
                if (tag == null) continue;

                bool canRead = Convert.ToBoolean(row.Cells["colRead"].Value);
                bool canWrite = Convert.ToBoolean(row.Cells["colWrite"].Value);
                bool canDelete = Convert.ToBoolean(row.Cells["colDelete"].Value);
                bool canDownload = Convert.ToBoolean(row.Cells["colDownload"].Value);

                if (!canRead && !canWrite && !canDelete && !canDownload)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230);
                    lblStatus.Text = string.Format("แถวที่ {0} ({1}): ต้องเลือกสิทธิ์อย่างน้อย 1 อย่าง", i + 1, tag.Name);
                    return;
                }

                if (tag.ExpiryDate.HasValue)
                {
                    if (tag.ExpiryDate.Value.Date < DateTime.Today)
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230);
                        lblStatus.Text = string.Format("แถวที่ {0} ({1}): วันหมดอายุต้องไม่ย้อนหลัง", i + 1, tag.Name);
                        return;
                    }
                    if (tag.ExpiryDate.Value.Date > DateTime.Today.AddYears(1))
                    {
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 230, 230);
                        lblStatus.Text = string.Format("แถวที่ {0} ({1}): วันหมดอายุต้องไม่เกิน 1 ปี", i + 1, tag.Name);
                        return;
                    }
                }

                recipients.Add(new ShareRecipientItem
                {
                    PrincipalType = tag.PrincipalTypeId,
                    PrincipalID   = tag.Id,
                    PrincipalName = tag.Name,
                    CanRead       = canRead,
                    CanWrite      = canWrite,
                    CanDelete     = canDelete,
                    CanDownload   = canDownload,
                    ExpiryDate    = tag.ExpiryDate
                });
            }

            string remarkText = txtRemark.Text?.Trim();

            var batchReq = new CreateInternalShareBatchRequest
            {
                FileIDs    = _selectedFiles.Select(f => f.FileID).ToList(),
                FolderID   = _selectedFolder?.FolderID,
                Recipients = recipients,
                Remark     = remarkText
            };

            try
            {
                var result = _shareCommandController.CreateInternalShareBatch(batchReq, _currentUser);

                string targetName = _selectedFiles.Count > 0
                    ? (_selectedFiles.Count == 1
                        ? _selectedFiles[0].OriginalFileName
                        : string.Format("{0} ไฟล์", _selectedFiles.Count))
                    : (_selectedFolder != null ? _selectedFolder.FolderName : "—");

                string remarkSummary = string.IsNullOrWhiteSpace(remarkText) ? "" : string.Format("\r\n   หมายเหตุ  : {0}", remarkText);

                string msgTitle, msgBody;
                MessageBoxIcon msgIcon = MessageBoxIcon.Information;

                if (result.ApprovalRequestIDs.Count > 0 && result.ImmediateGrantedCount > 0)
                {
                    msgTitle = "ดำเนินการแบ่งปันเรียบร้อยแล้ว";
                    msgBody = string.Format(
                        "ดำเนินการบันทึกการแบ่งปันสำหรับ \"{0}\" เรียบร้อยแล้ว:\r\n\r\n" +
                        "   ✔ ให้สิทธิ์ทันที       : {1} รายการ\r\n" +
                        "   ⏳ ส่งคำขอรออนุมัติ : {2} รายการ (คำขอ ID #{3}){4}\r\n\r\n" +
                        "รายการที่ต้องอนุมัติจะสามารถเข้าถึงได้หลังจากผู้มีอำนาจอนุมัติแล้ว",
                        targetName, result.ImmediateGrantedCount, recipients.Count - result.ImmediateGrantedCount,
                        string.Join(", ", result.ApprovalRequestIDs), remarkSummary);
                }
                else if (result.ApprovalRequestIDs.Count > 0)
                {
                    msgTitle = "บันทึกคำขอแบ่งปันเรียบร้อยแล้ว";
                    msgBody = string.Format(
                        "บันทึกคำขอแบ่งปันเรียบร้อยแล้ว และส่งให้ผู้มีอำนาจอนุมัติแล้ว\r\n\r\n" +
                        "   รายการ      : {0}\r\n" +
                        "   ผู้รับสิทธิ์   : {1} รายการ\r\n" +
                        "   คำขอเลขที่  : #{2}{3}\r\n\r\n" +
                        "สถานะ: รอการอนุมัติจากผู้มีอำนาจ\r\n" +
                        "ผู้รับจะสามารถเข้าถึงได้หลังจากได้รับการอนุมัติแล้ว",
                        targetName, recipients.Count, string.Join(", ", result.ApprovalRequestIDs), remarkSummary);
                }
                else
                {
                    msgTitle = "แบ่งปันเรียบร้อยแล้ว";
                    msgBody = string.Format(
                        "บันทึกการแบ่งปันเรียบร้อยแล้ว ผู้รับสามารถเข้าถึงได้ทันที\r\n\r\n" +
                        "   รายการ    : {0}\r\n" +
                        "   ผู้รับสิทธิ์ : {1} รายการ{2}",
                        targetName, result.ImmediateGrantedCount, remarkSummary);
                }

                MessageBox.Show(msgBody, msgTitle, MessageBoxButtons.OK, msgIcon);

                this.DialogResult = DialogResult.OK;
                this.Close();
            }
            catch (Exception ex)
            {
                lblStatus.Text = "เกิดข้อผิดพลาด: " + ex.Message;
                MessageBox.Show(
                    "เกิดข้อผิดพลาดในการบันทึก:\r\n\r\n" + ex.Message,
                    "ไม่สามารถดำเนินการได้",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        private static string PrincipalTypeLabel(string type)
        {
            switch (type)
            {
                case "User":       return "ผู้ใช้งาน";
                case "Department": return "ฝ่าย/แผนก";
                case "Role":       return "บทบาท";
                case "Group":      return "กลุ่ม";
                default:           return type;
            }
        }

        private int GetPrincipalTypeId(string principalType)
        {
            switch (principalType)
            {
                case "User":       return 1;
                case "Department": return 3;
                case "Role":       return 2;
                case "Group":      return 4;
                default:           return 1;
            }
        }

        private class PrincipalItem
        {
            public int    Id            { get; set; }
            public string Name          { get; set; }
            public string PrincipalType { get; set; }
            public object Data          { get; set; }
            public override string ToString() => Name;
        }

        private class SelectedPrincipalRow
        {
            public int       Id              { get; set; }
            public string    Name            { get; set; }
            public string    PrincipalType   { get; set; }
            public int       PrincipalTypeId { get; set; }
            public DateTime? ExpiryDate      { get; set; }
            public object    Data            { get; set; }
        }
    }
}
