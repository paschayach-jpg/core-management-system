using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    /// <summary>
    /// Form for viewing and managing internal and external shares.
    /// Satisfies Requirements 13.1, 13.2, 13.3, 13.4, 13.5, 14.1, 14.2, 14.3
    /// </summary>
    public partial class ShareManagementForm : Form
    {
        private readonly IShareService _shareService;
        private readonly UserContext _currentUser;
        private readonly IUserAccountRepository _userRepo;
        private readonly IFileDocumentRepository _fileRepo;

        private TabControl _tabControl;
        private DataGridView _dgvInternalShares;
        private DataGridView _dgvExternalShares;
        private Dictionary<int, int> _internalShareIdMap; // row → FilePermissionID
        private Dictionary<int, int> _externalShareIdMap; // row → ExternalShareID

        public ShareManagementForm(IShareService shareService, UserContext currentUser,
            IUserAccountRepository userRepo = null, IFileDocumentRepository fileRepo = null)
        {
            InitializeComponent();
            _shareService = shareService;
            _currentUser = currentUser;
            _userRepo = userRepo;
            _fileRepo = fileRepo;
            _internalShareIdMap = new Dictionary<int, int>();
            _externalShareIdMap = new Dictionary<int, int>();
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
                Text      = "ดูและจัดการ Internal / External share ของคุณ",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(170, 200, 240),
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
            btnApproveExt.Click += (s, e) => ApproveExternalShare();
            btnRejectExt.Click  += (s, e) => RejectExternalShare();
            btnRevokeExt.Click  += (s, e) => RevokeExternalShare();
            btnModifyExt.Click  += (s, e) => ModifyExternalShare();

            var pnlExtBar = new Panel { Dock = DockStyle.Bottom, Height = 46, BackColor = Color.FromArgb(235, 240, 250), Padding = new Padding(8, 7, 8, 7) };
            int bx = 8;
            foreach (var b in new[] { btnApproveExt, btnRejectExt, btnRevokeExt, btnModifyExt })
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

                // Load internal shares
                var internalShares = _shareService.GetInternalSharesForUser(_currentUser.UserID, _currentUser).ToList();
                var internalData = new List<object>();
                for (int i = 0; i < internalShares.Count; i++)
                {
                    var s = internalShares[i];
                    internalData.Add(new
                    {
                        SharerName   = _currentUser.ADUserName ?? "Current User",
                        TargetName   = ResolvePrincipalName(s.PrincipalType, s.PrincipalID),
                        ResourceName = ResolveFileName(s.FileID),
                        CreatedDate  = s.GrantedDate,
                        Status       = s.RevokedDate.HasValue ? "ยกเลิกแล้ว" : "ใช้งาน"
                    });
                    _internalShareIdMap[i] = s.FilePermissionID;
                }
                _dgvInternalShares.DataSource = null;
                _dgvInternalShares.DataSource = internalData;

                // Load external shares
                var externalShares = _shareService.GetExternalSharesForUser(_currentUser.UserID, _currentUser).ToList();
                var externalData = new List<object>();
                for (int i = 0; i < externalShares.Count; i++)
                {
                    var s = externalShares[i];
                    string extResource = s.FileID.HasValue
                        ? ResolveFileName(s.FileID.Value)
                        : ResolveExternalFolderName(s.FolderID);
                    externalData.Add(new
                    {
                        SharerName           = _currentUser.ADUserName ?? "Current User",
                        ResourceName         = extResource,
                        CreatedDate          = s.CreatedDate,
                        ShareTokenStatus     = s.ShareTokenStatus,
                        ExpiryDate           = s.ExpiryDate?.ToString("yyyy-MM-dd") ?? "ไม่มีกำหนด",
                        DownloadLimit        = s.DownloadLimit?.ToString() ?? "ไม่จำกัด",
                        CurrentDownloadCount = s.CurrentDownloadCount
                    });
                    _externalShareIdMap[i] = s.ExternalShareID;
                }
                _dgvExternalShares.DataSource = null;
                _dgvExternalShares.DataSource = externalData;
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("Error loading shares: {0}", ex.Message), "Error", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

                _shareService.RevokeInternalShare(filePermissionId, _currentUser);
                MessageBox.Show("Internal share revoked.", "Success");
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

                _shareService.ApproveExternalShare(externalShareId, _currentUser);
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

                _shareService.RejectExternalShare(externalShareId, reason, _currentUser);
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

                _shareService.RevokeExternalShare(externalShareId, reason, _currentUser);
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
            if (principalType == 1 && _userRepo != null)
            {
                try
                {
                    var u = _userRepo.GetById(principalId);
                    if (u != null)
                    {
                        var name = u.DisplayName ?? u.ADUserName;
                        if (!string.IsNullOrEmpty(name)) return name;
                    }
                }
                catch { }
            }

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
            if (_fileRepo != null)
            {
                try
                {
                    var f = _fileRepo.GetById(fileId);
                    if (f != null && !string.IsNullOrEmpty(f.OriginalFileName))
                        return f.OriginalFileName;
                }
                catch { }
            }
            return string.Format("File #{0}", fileId);
        }

        private string ResolveExternalFolderName(int? folderId)
        {
            if (folderId == null) return "(ไม่ระบุ)";
            return string.Format("Folder #{0}", folderId);
        }

        private string PromptForReason(string prompt)
        {
            var form = new Form
            {
                Text = "Enter Reason",
                Width = 400,
                Height = 150,
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false,
                MinimizeBox = false
            };

            var label = new Label { Text = prompt, Left = 10, Top = 10, Width = 370 };
            var textBox = new TextBox { Left = 10, Top = 40, Width = 370, Height = 60, Multiline = true };
            var btnOK = new Button { Text = "OK", Left = 310, Top = 110, Width = 70, DialogResult = DialogResult.OK };
            var btnCancel = new Button { Text = "Cancel", Left = 310, Top = 110, Width = 70, DialogResult = DialogResult.Cancel };

            form.Controls.AddRange(new Control[] { label, textBox, btnOK, btnCancel });

            return form.ShowDialog() == DialogResult.OK ? textBox.Text : null;
        }
    }
}
