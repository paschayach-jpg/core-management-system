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
        private string _previousPrincipalType = "User";

        private int _lastHoverIndexLeft = -1;
        private int _lastHoverIndexRight = -1;

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

            WireEvents();
            InitializeControls();
        }

        private void WireEvents()
        {
            btnOK.Click += (s, e) => HandleOK();
            btnAdd.Click += (s, e) => AddCurrentPrincipal();
            btnRemove.Click += (s, e) => RemoveSelectedItems();
            chkNoExpiry.CheckedChanged += (s, e) => dtpExpiry.Enabled = !chkNoExpiry.Checked;

            cboPrincipalType.SelectedIndexChanged += CboPrincipalType_SelectedIndexChanged;

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

            lstSelected.KeyDown += LstSelected_KeyDown;
            lstSelected.MouseMove += LstSelected_MouseMove;

            var ctxMenu = new ContextMenu();
            ctxMenu.MenuItems.Add(new System.Windows.Forms.MenuItem("ลบออก", (s, e) => RemoveSelectedItems()));
            lstSelected.ContextMenu = ctxMenu;

            this.Load += (s, e) => SendMessage(txtSearch.Handle, EM_SETCUEBANNER, 1, "ค้นหาชื่อ, AD user, รหัสพนักงาน...");
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
        }

        private void CboPrincipalType_SelectedIndexChanged(object sender, EventArgs e)
        {
            string newType = cboPrincipalType.SelectedItem?.ToString() ?? "User";
            if (newType == _previousPrincipalType) return;

            if (lstSelected.Items.Count > 0)
            {
                var result = MessageBox.Show(
                    "การเปลี่ยนประเภทผู้รับสิทธิ์จะล้างรายการที่เลือกไว้ทั้งหมด ต้องการดำเนินการต่อหรือไม่?",
                    "ยืนยันการเปลี่ยนประเภท",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (result != DialogResult.Yes)
                {
                    cboPrincipalType.SelectedIndexChanged -= CboPrincipalType_SelectedIndexChanged;
                    cboPrincipalType.SelectedItem = _previousPrincipalType;
                    cboPrincipalType.SelectedIndexChanged += CboPrincipalType_SelectedIndexChanged;
                    return;
                }
            }

            _previousPrincipalType = newType;
            _allPrincipals.Clear();
            lstSelected.Items.Clear();
            UpdateSelectedCount();
            LoadPrincipals();
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

        private void LstSelected_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Delete)
            {
                RemoveSelectedItems();
                e.SuppressKeyPress = true;
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

        private void LstSelected_MouseMove(object sender, MouseEventArgs e)
        {
            int idx = lstSelected.IndexFromPoint(e.Location);
            if (idx != _lastHoverIndexRight)
            {
                _lastHoverIndexRight = idx;
                if (idx >= 0 && idx < lstSelected.Items.Count)
                {
                    var item = lstSelected.Items[idx] as PrincipalItem;
                    if (item != null && item.Data is UserAccount u)
                    {
                        string tip = string.Format("AD: {0}  ·  {1}  ·  {2}",
                            u.ADUserName ?? "—",
                            u.DepartmentName ?? "—",
                            u.PositionName ?? "—");
                        toolTipInfo.SetToolTip(lstSelected, tip);
                        return;
                    }
                }
                toolTipInfo.SetToolTip(lstSelected, string.Empty);
            }
        }

        private void InitializeControls()
        {
            if (_selectedFiles.Count > 0)
            {
                txtFiles.Text = string.Join(Environment.NewLine,
                    _selectedFiles.Select(f => "📄  " + f.OriginalFileName));
            }
            else if (_selectedFolder != null)
            {
                txtFiles.Text = _parentFolder != null
                    ? "📁  " + _parentFolder.FolderName + "  ›  " + _selectedFolder.FolderName
                    : "📁  " + _selectedFolder.FolderName;
            }
            else
            {
                txtFiles.Text = "(ไม่ระบุ)";
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
            var selectedIds = new HashSet<int>(lstSelected.Items.Cast<PrincipalItem>()
                .Select(p => p.Id));

            lstPrincipal.BeginUpdate();
            lstPrincipal.Items.Clear();

            var items = _allPrincipals.Where(p =>
            {
                if (selectedIds.Contains(p.Id)) return false;

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
            if (lstSelected.Items.Cast<PrincipalItem>().Any(p => p.Id == item.Id && p.PrincipalType == item.PrincipalType))
                return;

            lstSelected.Items.Add(item);
            UpdateSelectedCount();
            FilterPrincipals(txtSearch.Text);
        }

        private void RemoveSelectedItems()
        {
            var toRemove = lstSelected.SelectedItems.Cast<PrincipalItem>().ToList();
            if (toRemove.Count == 0) return;

            foreach (var item in toRemove)
                lstSelected.Items.Remove(item);

            UpdateSelectedCount();
            FilterPrincipals(txtSearch.Text);
        }

        private void UpdateSelectedCount()
        {
            string principalType = cboPrincipalType.SelectedItem?.ToString() ?? "User";
            string unitLabel = principalType == "User" ? "คน" : "รายการ";
            lblRightHeader.Text = string.Format("  รายชื่อที่เลือก ({0} {1})", lstSelected.Items.Count, unitLabel);
        }

        private void HandleOK()
        {
            if (_selectedFiles.Count == 0 && _selectedFolder == null)
            {
                lblStatus.Text = "ไม่พบไฟล์หรือโฟลเดอร์ที่ต้องการแบ่งปัน";
                return;
            }

            if (lstSelected.Items.Count == 0)
            {
                lblStatus.Text = "กรุณาเลือกผู้รับสิทธิ์อย่างน้อย 1 รายการ";
                return;
            }

            if (!chkRead.Checked && !chkWrite.Checked && !chkDelete.Checked)
            {
                lblStatus.Text = "กรุณาเลือกสิทธิ์การเข้าถึงอย่างน้อย 1 รายการ";
                return;
            }

            if (!chkNoExpiry.Checked)
            {
                if (dtpExpiry.Value.Date < DateTime.Today)
                {
                    lblStatus.Text = "ไม่สามารถกำหนดวันหมดอายุย้อนหลังได้";
                    return;
                }
                if (dtpExpiry.Value.Date > DateTime.Today.AddYears(1))
                {
                    lblStatus.Text = "ไม่สามารถกำหนดอายุเกิน 1 ปีได้";
                    return;
                }
            }

            string principalType   = cboPrincipalType.SelectedItem?.ToString() ?? "User";
            int    principalTypeId = GetPrincipalTypeId(principalType);
            DateTime? expiry       = chkNoExpiry.Checked ? (DateTime?)null : dtpExpiry.Value;

            try
            {
                foreach (PrincipalItem principal in lstSelected.Items)
                {
                    if (_selectedFiles.Count > 0)
                    {
                        foreach (var file in _selectedFiles)
                        {
                            _shareCommandController.CreateInternalShare(new CreateInternalShareRequest
                            {
                                FileID        = file.FileID,
                                PrincipalType = principalTypeId,
                                PrincipalID   = principal.Id,
                                CanRead       = chkRead.Checked,
                                CanWrite      = chkWrite.Checked,
                                CanDelete     = chkDelete.Checked,
                                ExpiryDate    = expiry,
                            }, _currentUser);
                        }
                    }
                    else
                    {
                        var folderPermReq = new SetPermissionRequest
                        {
                            FolderID       = _selectedFolder.FolderID,
                            PrincipalType  = (PrincipalType)principalTypeId,
                            PrincipalID    = principal.Id,
                            CanRead        = chkRead.Checked,
                            CanWrite       = chkWrite.Checked,
                            CanDelete      = chkDelete.Checked,
                            CanModify      = false,
                            CanFullControl = false,
                            IsDeny         = false,
                        };

                        bool folderNeedsApproval = _currentUser.AppRole < 3 || principalTypeId == 3;
                        if (folderNeedsApproval)
                            _shareCommandController.RequestFolderShareApproval(folderPermReq, _currentUser);
                        else
                            _shareCommandController.SetFolderPermission(folderPermReq, _currentUser);
                    }
                }

                bool needsApproval = _currentUser.AppRole < 3 || principalTypeId == 3;

                string targetName = _selectedFiles.Count > 0
                    ? (_selectedFiles.Count == 1
                        ? _selectedFiles[0].OriginalFileName
                        : string.Format("{0} ไฟล์", _selectedFiles.Count))
                    : (_selectedFolder != null ? _selectedFolder.FolderName : "—");

                var recipientNames = lstSelected.Items.Cast<PrincipalItem>()
                    .Select(p => p.Name).ToArray();
                string recipientSummary = recipientNames.Length <= 3
                    ? string.Join(", ", recipientNames)
                    : string.Format("{0}, {1} และอีก {2} รายการ",
                        recipientNames[0], recipientNames[1], recipientNames.Length - 2);

                var perms = new List<string>();
                if (chkRead.Checked)   perms.Add("ดู");
                if (chkWrite.Checked)  perms.Add("แก้ไข");
                if (chkDelete.Checked) perms.Add("ลบ");
                string permSummary = string.Join(" / ", perms);

                string expiryText = chkNoExpiry.Checked
                    ? "ไม่มีกำหนดอายุ"
                    : string.Format("หมดอายุ {0}", expiry.Value.ToString("dd/MM/yyyy"));

                string msgTitle, msgBody;
                MessageBoxIcon msgIcon;

                if (needsApproval)
                {
                    msgTitle = "บันทึกคำขอแบ่งปันเรียบร้อยแล้ว";
                    msgBody  = string.Format(
                        "บันทึกคำขอแบ่งปันเรียบร้อยแล้ว และส่งให้ผู้มีอำนาจอนุมัติแล้ว\r\n\r\n" +
                        "   รายการ    : {0}\r\n" +
                        "   ผู้รับสิทธิ์ : {1}\r\n" +
                        "   สิทธิ์      : {2}\r\n" +
                        "   อายุสิทธิ์  : {3}\r\n\r\n" +
                        "สถานะ: รอการอนุมัติจากผู้มีอำนาจ\r\n" +
                        "ผู้รับจะสามารถเข้าถึงได้หลังจากได้รับการอนุมัติแล้ว",
                        targetName, recipientSummary, permSummary, expiryText);
                    msgIcon = MessageBoxIcon.Information;

                    lblStatus.Text = string.Format(
                        "ส่งคำขออนุมัติเรียบร้อยแล้ว ({0} รายการ) — รอผู้มีอำนาจอนุมัติ",
                        lstSelected.Items.Count);
                }
                else
                {
                    msgTitle = "แบ่งปันเรียบร้อยแล้ว";
                    msgBody  = string.Format(
                        "บันทึกการแบ่งปันเรียบร้อยแล้ว ผู้รับสามารถเข้าถึงได้ทันที\r\n\r\n" +
                        "   รายการ    : {0}\r\n" +
                        "   ผู้รับสิทธิ์ : {1}\r\n" +
                        "   สิทธิ์      : {2}\r\n" +
                        "   อายุสิทธิ์  : {3}",
                        targetName, recipientSummary, permSummary, expiryText);
                    msgIcon = MessageBoxIcon.Information;

                    lblStatus.Text = _selectedFiles.Count > 1
                        ? string.Format("แบ่งปันเรียบร้อยแล้ว ({0} ไฟล์, {1} ผู้รับ)", _selectedFiles.Count, lstSelected.Items.Count)
                        : string.Format("แบ่งปันเรียบร้อยแล้ว ({0} รายการ)", lstSelected.Items.Count);
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
    }
}
