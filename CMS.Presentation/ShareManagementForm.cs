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
    /// Satisfies Requirements 13.1, 13.2, 13.3, 13.4, 13.5, 14.1, 14.2, 14.3
    /// </summary>
    public partial class ShareManagementForm : Form
    {
        private readonly IShareManagementController _shareManagementController;
        private readonly UserContext _currentUser;
        private readonly IShareLookupController _shareLookupController;
        private readonly bool _isAdmin;

        private TabControl _tabControl;
        private DataGridView _dgvInternalShares;
        private DataGridView _dgvExternalShares;
        private Dictionary<int, int> _internalShareIdMap; // row → FilePermissionID
        private Dictionary<int, int> _externalShareIdMap; // row → ExternalShareID
        private Dictionary<int, string> _externalShareStatusMap; // row → ShareTokenStatus

        public ShareManagementForm(IShareManagementController shareManagementController, UserContext currentUser,
            IShareLookupController shareLookupController = null)
        {
            _isAdmin = currentUser != null && currentUser.AppRole == 4;
            InitializeComponent();
            _shareManagementController = shareManagementController;
            _currentUser = currentUser;
            _shareLookupController = shareLookupController;
            _internalShareIdMap = new Dictionary<int, int>();
            _externalShareIdMap = new Dictionary<int, int>();
            _externalShareStatusMap = new Dictionary<int, string>();
        }

        private void InitializeComponent()
        {
            // ── Palette ─────────────────────────────────────────────────────
            Color navyDark   = Color.FromArgb(25,  55,  105);
            Color navyMid    = Color.FromArgb(50,  90,  150);
            Color navyLight  = Color.FromArgb(215, 228, 248);
            Color bodyBg     = Color.FromArgb(243, 246, 251);
            Color footerBg   = Color.FromArgb(228, 234, 245);
            Color gridHeader = Color.FromArgb(25,  55,  105);
            var   uiFont     = new Font("Segoe UI", 9f);
            var   boldFont   = new Font("Segoe UI", 9f, FontStyle.Bold);

            this.SuspendLayout();
            this.Text            = "จัดการการแบ่งปัน";
            this.ClientSize      = new Size(960, 620);
            this.MinimumSize     = new Size(800, 520);
            this.StartPosition   = FormStartPosition.CenterParent;
            this.FormBorderStyle = FormBorderStyle.Sizable;
            this.MaximizeBox     = true;
            this.MinimizeBox     = true;
            this.BackColor       = bodyBg;
            this.Font            = uiFont;

            // ── Header ──────────────────────────────────────────────────────
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 60, BackColor = navyDark };
            var lblTitle  = new Label
            {
                Text      = "📋  จัดการการแบ่งปัน  (Share Management)",
                Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(16, 10),
            };
            var lblSub = new Label
            {
                Text      = _isAdmin
                    ? "👑 Admin Mode — แสดงรายการ share ทั้งหมดในระบบ"
                    : "ดูและจัดการ Internal / External share ของคุณ",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = _isAdmin ? Color.FromArgb(255, 220, 100) : Color.FromArgb(170, 200, 240),
                AutoSize  = true,
                Location  = new Point(17, 36),
            };
            pnlHeader.Controls.AddRange(new Control[] { lblTitle, lblSub });

            // ── Footer ──────────────────────────────────────────────────────
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = footerBg };

            var btnRefresh = new Button
            {
                Text      = "↺  รีเฟรช",
                Size      = new Size(96, 32),
                Location  = new Point(12, 9),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 228, 245),
                ForeColor = navyDark,
                Font      = boldFont,
            };
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(180, 200, 235);
            btnRefresh.FlatAppearance.BorderSize  = 1;
            btnRefresh.Click += (s, e) => LoadShares();

            var btnClose = new Button
            {
                Text         = "✕  ปิด",
                Size         = new Size(96, 32),
                Anchor       = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle    = FlatStyle.Flat,
                BackColor    = navyDark,
                ForeColor    = Color.White,
                Font         = boldFont,
                DialogResult = DialogResult.OK,
            };
            btnClose.FlatAppearance.BorderSize = 0;
            pnlFooter.Resize += (s, e) => btnClose.Location = new Point(pnlFooter.Width - 112, 9);
            btnClose.Location = new Point(852, 9);
            pnlFooter.Controls.AddRange(new Control[] { btnRefresh, btnClose });

            // ── Helper: build styled DataGridView ───────────────────────────
            Func<DataGridView> mkGrid = () =>
            {
                var g = new DataGridView
                {
                    Dock                      = DockStyle.Fill,
                    AllowUserToAddRows        = false,
                    AllowUserToDeleteRows     = false,
                    ReadOnly                  = true,
                    SelectionMode             = DataGridViewSelectionMode.FullRowSelect,
                    RowHeadersVisible         = false,
                    BackgroundColor           = Color.White,
                    BorderStyle               = BorderStyle.None,
                    GridColor                 = Color.FromArgb(228, 232, 240),
                    AutoSizeColumnsMode       = DataGridViewAutoSizeColumnsMode.Fill,
                    ColumnHeadersHeight       = 32,
                    EnableHeadersVisualStyles = false,
                    DefaultCellStyle = new DataGridViewCellStyle
                    {
                        Font    = uiFont,
                        Padding = new Padding(4, 2, 4, 2),
                    },
                    ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                    {
                        Font      = boldFont,
                        BackColor = gridHeader,
                        ForeColor = Color.White,
                        Alignment = DataGridViewContentAlignment.MiddleLeft,
                        Padding   = new Padding(6, 0, 0, 0),
                    },
                    AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                    {
                        BackColor = Color.FromArgb(249, 251, 255),
                    },
                };
                return g;
            };

            // ── Helper: action button ────────────────────────────────────────
            Func<string, Color, Color, Button> mkBtn = (text, bg, fg) =>
            {
                var b = new Button
                {
                    Text      = text,
                    Size      = new Size(96, 30),
                    FlatStyle = FlatStyle.Flat,
                    BackColor = bg,
                    ForeColor = fg,
                    Font      = boldFont,
                };
                b.FlatAppearance.BorderSize  = bg == navyDark ? 0 : 1;
                b.FlatAppearance.BorderColor = Color.FromArgb(180, 200, 235);
                return b;
            };

            // ── Tab: Internal Shares ─────────────────────────────────────────
            _dgvInternalShares = mkGrid();
            _dgvInternalShares.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "ผู้แบ่งปัน",    DataPropertyName = "SharerName",   MinimumWidth = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "เป้าหมาย",      DataPropertyName = "TargetName",   MinimumWidth = 120 },
                new DataGridViewTextBoxColumn { HeaderText = "File / Folder",  DataPropertyName = "ResourceName", MinimumWidth = 160 },
                new DataGridViewTextBoxColumn { HeaderText = "วันที่สร้าง",   DataPropertyName = "CreatedDate",  MinimumWidth = 130,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } },
                new DataGridViewTextBoxColumn { HeaderText = "สถานะ",          DataPropertyName = "Status",       MinimumWidth = 90  }
            );

            var btnRevokeInternal = mkBtn("🚫  ยกเลิกสิทธิ์", Color.FromArgb(250, 235, 235), Color.FromArgb(160, 30, 30));
            btnRevokeInternal.Click += (s, e) => RevokeInternalShare();

            var pnlIntBar = new Panel { Dock = DockStyle.Bottom, Height = 46, BackColor = Color.FromArgb(235, 240, 250), Padding = new Padding(8, 7, 8, 7) };
            btnRevokeInternal.Location = new Point(8, 8);
            pnlIntBar.Controls.Add(btnRevokeInternal);

            var tabInternal = new TabPage { Text = "  📋 Internal Shares  ", BackColor = bodyBg, UseVisualStyleBackColor = false };
            tabInternal.Controls.Add(_dgvInternalShares);
            tabInternal.Controls.Add(pnlIntBar);

            // ── Tab: External Shares ─────────────────────────────────────────
            _dgvExternalShares = mkGrid();
            _dgvExternalShares.Columns.AddRange(
                new DataGridViewTextBoxColumn { HeaderText = "ผู้แบ่งปัน",         DataPropertyName = "SharerName",           MinimumWidth = 110 },
                new DataGridViewTextBoxColumn { HeaderText = "File / Folder",       DataPropertyName = "ResourceName",         MinimumWidth = 150 },
                new DataGridViewTextBoxColumn { HeaderText = "วันที่สร้าง",        DataPropertyName = "CreatedDate",          MinimumWidth = 120,
                    DefaultCellStyle = new DataGridViewCellStyle { Format = "g" } },
                new DataGridViewTextBoxColumn { HeaderText = "สถานะ",               DataPropertyName = "ShareTokenStatus",     MinimumWidth = 90  },
                new DataGridViewTextBoxColumn { HeaderText = "หมดอายุ",            DataPropertyName = "ExpiryDate",           MinimumWidth = 110 },
                new DataGridViewTextBoxColumn { HeaderText = "จำกัดดาวน์โหลด",    DataPropertyName = "DownloadLimit",        MinimumWidth = 110 },
                new DataGridViewTextBoxColumn { HeaderText = "ดาวน์โหลดแล้ว",     DataPropertyName = "CurrentDownloadCount", MinimumWidth = 110 }
            );

            var btnApproveExt = mkBtn("✔  อนุมัติ",    navyDark,                         Color.White);
            var btnRejectExt  = mkBtn("✕  ปฏิเสธ",    Color.FromArgb(250, 235, 235),     Color.FromArgb(160, 30, 30));
            var btnRevokeExt  = mkBtn("🚫  ยกเลิก",    Color.FromArgb(255, 248, 215),     Color.FromArgb(140, 80, 0));
            var btnModifyExt  = mkBtn("✏  แก้ไข",     Color.FromArgb(220, 228, 245),     navyDark);
            var btnToggleExt  = mkBtn("⏸  ปิดใช้งาน", Color.FromArgb(232, 248, 240),     Color.FromArgb(0, 100, 50));
            btnApproveExt.Click += (s, e) => ApproveExternalShare();
            btnRejectExt.Click  += (s, e) => RejectExternalShare();
            btnRevokeExt.Click  += (s, e) => RevokeExternalShare();
            btnModifyExt.Click  += (s, e) => ModifyExternalShare();
            btnToggleExt.Click  += (s, e) => ToggleExternalShare();

            // อัปเดต label ปุ่ม toggle ตาม selection
            _dgvExternalShares.SelectionChanged += (s, e) =>
            {
                if (_dgvExternalShares.SelectedRows.Count == 0) { btnToggleExt.Text = "⏸  เปิด/ปิด"; return; }
                int row = _dgvExternalShares.SelectedRows[0].Index;
                if (_externalShareStatusMap.TryGetValue(row, out string st) && st == "Active")
                {
                    btnToggleExt.Text      = "⏸  ปิดใช้งาน";
                    btnToggleExt.BackColor = Color.FromArgb(255, 248, 230);
                    btnToggleExt.ForeColor = Color.FromArgb(140, 80, 0);
                }
                else if (_externalShareStatusMap.TryGetValue(row, out string st2) && st2 == "Disabled")
                {
                    btnToggleExt.Text      = "▶  เปิดใช้งาน";
                    btnToggleExt.BackColor = Color.FromArgb(232, 248, 240);
                    btnToggleExt.ForeColor = Color.FromArgb(0, 100, 50);
                }
                else
                {
                    btnToggleExt.Text = "⏸  เปิด/ปิด";
                }
            };

            var pnlExtBar = new Panel { Dock = DockStyle.Bottom, Height = 46, BackColor = Color.FromArgb(235, 240, 250), Padding = new Padding(8, 7, 8, 7) };
            int bx = 8;
            foreach (var b in new[] { btnApproveExt, btnRejectExt, btnRevokeExt, btnModifyExt, btnToggleExt })
            {
                b.Location = new Point(bx, 8);
                pnlExtBar.Controls.Add(b);
                bx += 104;
            }

            var tabExternal = new TabPage { Text = "  🌐 External Shares  ", BackColor = bodyBg, UseVisualStyleBackColor = false };
            tabExternal.Controls.Add(_dgvExternalShares);
            tabExternal.Controls.Add(pnlExtBar);

            // ── TabControl ────────────────────────────────────────────────────
            _tabControl = new TabControl
            {
                Dock     = DockStyle.Fill,
                Font     = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Padding  = new Point(12, 4),
            };
            _tabControl.TabPages.AddRange(new TabPage[] { tabInternal, tabExternal });

            // ── Body wrapper ─────────────────────────────────────────────────
            var pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 8, 10, 0), BackColor = bodyBg };
            pnlBody.Controls.Add(_tabControl);

            this.Controls.Add(pnlBody);
            this.Controls.Add(pnlHeader);
            this.Controls.Add(pnlFooter);

            this.ResumeLayout();
        }

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadShares();
        }

        private void LoadShares()
        {
            try
            {
                _internalShareIdMap.Clear();
                _externalShareIdMap.Clear();
                _externalShareStatusMap.Clear();

                // Admin เห็นทั้งหมด, user ปกติเห็นแค่ของตัวเอง
                var internalShares = _isAdmin
                    ? _shareManagementController.GetAllInternalShares(_currentUser).ToList()
                    : _shareManagementController.GetInternalSharesForUser(_currentUser.UserID, _currentUser).ToList();
                var internalData = new List<object>();
                for (int i = 0; i < internalShares.Count; i++)
                {
                    var s = internalShares[i];
                    string sharerLabel = _isAdmin
                        ? string.Format("UserID:{0}", s.GrantedBy)
                        : _currentUser.ADUserName ?? "Current User";
                    internalData.Add(new
                    {
                        SharerName   = sharerLabel,
                        TargetName   = ResolvePrincipalName(s.PrincipalType, s.PrincipalID),
                        ResourceName = ResolveFileName(s.FileID),
                        CreatedDate  = s.GrantedDate,
                        Status       = s.RevokedDate.HasValue ? "🚫 ยกเลิกแล้ว" : "✅ ใช้งาน"
                    });
                    _internalShareIdMap[i] = s.FilePermissionID;
                }
                _dgvInternalShares.DataSource = null;
                _dgvInternalShares.DataSource = internalData;

                // Load external shares
                var externalShares = _isAdmin
                    ? _shareManagementController.GetAllExternalShares(_currentUser).ToList()
                    : _shareManagementController.GetExternalSharesForUser(_currentUser.UserID, _currentUser).ToList();
                var externalData = new List<object>();
                for (int i = 0; i < externalShares.Count; i++)
                {
                    var s = externalShares[i];
                    string extResource = s.FileID.HasValue
                        ? ResolveFileName(s.FileID.Value)
                        : ResolveExternalFolderName(s.FolderID);
                    string statusTh = s.ShareTokenStatus == "Active"   ? "✅ ใช้งาน"
                                    : s.ShareTokenStatus == "Disabled" ? "⏸ ปิดชั่วคราว"
                                    : s.ShareTokenStatus == "Pending"  ? "⏳ รออนุมัติ"
                                    : s.ShareTokenStatus == "Revoked"  ? "🚫 ยกเลิกแล้ว"
                                    : s.ShareTokenStatus == "Expired"  ? "⌛ หมดอายุ"
                                    : s.ShareTokenStatus ?? "-";
                    externalData.Add(new
                    {
                        SharerName           = _currentUser.ADUserName ?? "Current User",
                        ResourceName         = extResource,
                        CreatedDate          = s.CreatedDate,
                        ShareTokenStatus     = statusTh,
                        ExpiryDate           = s.ExpiryDate?.ToString("yyyy-MM-dd") ?? "ไม่มีกำหนด",
                        DownloadLimit        = s.DownloadLimit?.ToString() ?? "ไม่จำกัด",
                        CurrentDownloadCount = s.CurrentDownloadCount
                    });
                    _externalShareIdMap[i]    = s.ExternalShareID;
                    _externalShareStatusMap[i] = s.ShareTokenStatus ?? string.Empty;
                }
                _dgvExternalShares.DataSource = null;
                _dgvExternalShares.DataSource = externalData;
                ColorCodeExternalRows();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error loading shares: {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ColorCodeExternalRows()
        {
            foreach (DataGridViewRow row in _dgvExternalShares.Rows)
            {
                if (!_externalShareStatusMap.TryGetValue(row.Index, out string status)) continue;
                switch (status)
                {
                    case "Active":
                        row.DefaultCellStyle.BackColor = Color.FromArgb(232, 248, 240);
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(0, 80, 30);
                        break;
                    case "Disabled":
                        row.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 220);
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(130, 80, 0);
                        break;
                    case "Pending":
                        row.DefaultCellStyle.BackColor = Color.FromArgb(232, 240, 254);
                        row.DefaultCellStyle.ForeColor = Color.FromArgb(0, 51, 102);
                        break;
                    case "Revoked":
                    case "Expired":
                        row.DefaultCellStyle.BackColor = Color.FromArgb(245, 245, 245);
                        row.DefaultCellStyle.ForeColor = Color.Gray;
                        break;
                }
            }
        }

        private void RevokeInternalShare()
        {
            if (_dgvInternalShares.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a share to revoke.", "Info");
                return;
            }

            if (MessageBox.Show("Are you sure you want to revoke this share?", "Confirm",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                int rowIndex = _dgvInternalShares.SelectedRows[0].Index;
                if (!_internalShareIdMap.TryGetValue(rowIndex, out int filePermissionId))
                {
                    MessageBox.Show("Unable to identify the selected share.", "Error");
                    return;
                }

                _shareManagementController.RevokeInternalShare(filePermissionId, _currentUser);
                MessageBox.Show("Internal share revoked.", "Success");
                LoadShares();
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error: {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ToggleExternalShare()
        {
            if (_dgvExternalShares.SelectedRows.Count == 0)
            {
                MessageBox.Show("กรุณาเลือกรายการที่ต้องการเปิด/ปิด", "แจ้งเตือน");
                return;
            }

            int rowIndex = _dgvExternalShares.SelectedRows[0].Index;
            if (!_externalShareStatusMap.TryGetValue(rowIndex, out string status) ||
                (status != "Active" && status != "Disabled"))
            {
                MessageBox.Show("สามารถเปิด/ปิดได้เฉพาะรายการที่มีสถานะ 'ใช้งาน' หรือ 'ปิดชั่วคราว' เท่านั้น", "แจ้งเตือน");
                return;
            }

            string action = status == "Active" ? "ปิดชั่วคราว" : "เปิดใช้งาน";
            if (MessageBox.Show(string.Format("ต้องการ{0}การแชร์นี้ใช่หรือไม่?", action), "ยืนยัน",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes)
                return;

            try
            {
                if (!_externalShareIdMap.TryGetValue(rowIndex, out int externalShareId))
                {
                    MessageBox.Show("ไม่พบข้อมูลการแชร์ที่เลือก", "Error");
                    return;
                }

                _shareManagementController.ToggleExternalShare(externalShareId, _currentUser);
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
            if (_dgvExternalShares.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a share to approve.", "Info");
                return;
            }

            try
            {
                int rowIndex = _dgvExternalShares.SelectedRows[0].Index;
                if (!_externalShareIdMap.TryGetValue(rowIndex, out int externalShareId))
                {
                    MessageBox.Show("Unable to identify the selected share.", "Error");
                    return;
                }

                _shareManagementController.ApproveExternalShare(externalShareId, _currentUser);
                MessageBox.Show("External share approved.", "Success");
                LoadShares();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RejectExternalShare()
        {
            if (_dgvExternalShares.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a share to reject.", "Info");
                return;
            }

            var reason = PromptForReason("Enter rejection reason:");
            if (reason == null) return;

            try
            {
                int rowIndex = _dgvExternalShares.SelectedRows[0].Index;
                if (!_externalShareIdMap.TryGetValue(rowIndex, out int externalShareId))
                {
                    MessageBox.Show("Unable to identify the selected share.", "Error");
                    return;
                }

                _shareManagementController.RejectExternalShare(externalShareId, reason, _currentUser);
                MessageBox.Show("External share rejected.", "Success");
                LoadShares();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RevokeExternalShare()
        {
            if (_dgvExternalShares.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a share to revoke.", "Info");
                return;
            }

            var reason = PromptForReason("Enter revocation reason:");
            if (reason == null) return;

            try
            {
                int rowIndex = _dgvExternalShares.SelectedRows[0].Index;
                if (!_externalShareIdMap.TryGetValue(rowIndex, out int externalShareId))
                {
                    MessageBox.Show("Unable to identify the selected share.", "Error");
                    return;
                }

                _shareManagementController.RevokeExternalShare(externalShareId, reason, _currentUser);
                MessageBox.Show("External share revoked.", "Success");
                LoadShares();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error: {ex.Message}", "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ModifyExternalShare()
        {
            if (_dgvExternalShares.SelectedRows.Count == 0)
            {
                MessageBox.Show("Please select a share to modify.", "Info");
                return;
            }

            MessageBox.Show("Share modification dialog would be shown here.", "Info");
        }

        // ── Name resolution helpers ──────────────────────────────────────────
        private string ResolvePrincipalName(int principalType, int principalId)
        {
            // PrincipalType: 1=User, 2=Role, 3=OrgUnit, 4=Group
            if (_shareLookupController != null)
                return _shareLookupController.ResolvePrincipalName(principalType, principalId);

            // Fallback: label by type
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
                return _shareLookupController.ResolveFileName(fileId);

            return string.Format("File #{0}", fileId);
        }

        private string ResolveExternalFolderName(int? folderId)
        {
            if (folderId == null) return "(ไม่ระบุ)";
            return string.Format("Folder #{0}", folderId);
        }

        private string PromptForReason(string prompt)
        {
            var uiFont   = new System.Drawing.Font("Tahoma", 9f);
            var boldFont = new System.Drawing.Font("Tahoma", 9f, System.Drawing.FontStyle.Bold);
            var navyDark = System.Drawing.Color.FromArgb(25, 55, 105);

            var form = new Form
            {
                Text            = "ระบุเหตุผล",
                ClientSize      = new System.Drawing.Size(420, 150),
                StartPosition   = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false,
                MinimizeBox     = false,
                Font            = uiFont,
                BackColor       = System.Drawing.Color.FromArgb(243, 246, 251),
            };

            var label = new Label
            {
                Text     = prompt,
                Left     = 14, Top = 14,
                Width    = 392, Height = 32,
                AutoSize = false,
                Font     = uiFont,
            };

            var textBox = new TextBox
            {
                Left      = 14, Top = 50,
                Width     = 392, Height = 52,
                Multiline = true,
                Font      = uiFont,
            };

            var btnOK = new Button
            {
                Text         = "ตกลง",
                Left         = 230, Top = 110,
                Width        = 82, Height = 28,
                DialogResult = DialogResult.OK,
                FlatStyle    = FlatStyle.Flat,
                BackColor    = navyDark,
                ForeColor    = System.Drawing.Color.White,
                Font         = boldFont,
            };
            btnOK.FlatAppearance.BorderSize = 0;

            var btnCancel = new Button
            {
                Text         = "ยกเลิก",
                Left         = 324, Top = 110,
                Width        = 82, Height = 28,
                DialogResult = DialogResult.Cancel,
                FlatStyle    = FlatStyle.Flat,
                BackColor    = System.Drawing.Color.FromArgb(220, 228, 245),
                ForeColor    = navyDark,
                Font         = boldFont,
            };
            btnCancel.FlatAppearance.BorderColor = System.Drawing.Color.FromArgb(180, 200, 235);
            btnCancel.FlatAppearance.BorderSize  = 1;

            form.Controls.AddRange(new Control[] { label, textBox, btnOK, btnCancel });
            form.AcceptButton = btnOK;
            form.CancelButton = btnCancel;

            return form.ShowDialog(this) == DialogResult.OK ? textBox.Text.Trim() : null;
        }
    }
}
