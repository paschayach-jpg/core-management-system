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
    /// Admin-only, read-only diagnostic form: pick a user and see, in one screen,
    /// every layer that decides what folders they can see —
    ///   1. สถานะผู้ใช้ (role/org/active)
    ///   2. สิทธิ์มองเห็นหน่วยงาน (OrgUnit visibility — role-based + explicit grant)
    ///   3. Authority / ระดับชั้นความลับ (FolderAuthorityConfig ตามตำแหน่ง/แผนก/ฝ่าย)
    ///   4. ACL รายโฟลเดอร์ (explicit FolderPermission grants)
    /// No write actions — grant/revoke stays in UserAccessForm / AuthorityConfigForm.
    /// </summary>
    public class UserPermissionDiagnosticForm : Form
    {
        private readonly IUserAdminController _userAdminController;
        private readonly IUserOrgVisibilityController _visibilityController;
        private readonly IAuthorityConfigController _authorityConfigController;
        private readonly IPermissionAclLookupController _aclLookupController;
        private readonly IShareLookupController _shareLookupController;
        private readonly UserContext _currentUser;

        private List<UserAccount> _allUsers = new List<UserAccount>();

        private ComboBox _cmbUser;
        private Label _lblStatus;
        private FlowLayoutPanel _pnlCards;
        private ListView _lvVisibility;
        private DataGridView _gridAcl;

        private sealed class LevelDefinition
        {
            public int Level;
            public string Code;
            public string Label;
            public Color Accent;
            public LevelDefinition(int level, string code, string label, Color accent)
            { Level = level; Code = code; Label = label; Accent = accent; }
        }

        private static readonly LevelDefinition[] Levels =
        {
            new LevelDefinition(1, "1HC", "HC\nสูงสุด", Color.FromArgb(180, 30, 30)),
            new LevelDefinition(2, "2SC", "SC\nลับมาก", Color.FromArgb(200, 100, 0)),
            new LevelDefinition(3, "3NC", "NC\nปกติ", Color.FromArgb(0, 90, 160)),
            new LevelDefinition(4, "4PC", "PC\nทั่วไป", Color.FromArgb(0, 130, 60)),
        };

        public UserPermissionDiagnosticForm(
            IUserAdminController userAdminController,
            IUserOrgVisibilityController visibilityController,
            IAuthorityConfigController authorityConfigController,
            IPermissionAclLookupController aclLookupController,
            IShareLookupController shareLookupController,
            UserContext currentUser)
        {
            _userAdminController = userAdminController;
            _visibilityController = visibilityController;
            _authorityConfigController = authorityConfigController;
            _aclLookupController = aclLookupController;
            _shareLookupController = shareLookupController;
            _currentUser = currentUser;

            BuildUI();
            LoadUsers();
        }

        // ════════════════════════════════════════════════════════════════════
        // UI Construction
        // ════════════════════════════════════════════════════════════════════

        private void BuildUI()
        {
            Text = "ตรวจสอบสิทธิ์ผู้ใช้";
            Size = new Size(1000, 820);
            StartPosition = FormStartPosition.CenterParent;
            Font = new Font("Segoe UI", 9f);
            BackColor = Color.FromArgb(243, 246, 251);
            MinimumSize = new Size(880, 600);
            Icon = SystemIcons.Shield;

            var pnlHeader = BuildHeader();
            var pnlBody = BuildBody();

            Controls.Add(pnlBody);
            Controls.Add(pnlHeader);
        }

        private Panel BuildHeader()
        {
            var pnl = new Panel
            {
                Dock = DockStyle.Top,
                Height = 64,
                BackColor = Color.FromArgb(25, 55, 105),
                Padding = new Padding(14, 0, 14, 0),
            };

            var lblTitle = new Label
            {
                Text = "🔍 ตรวจสอบสิทธิ์ผู้ใช้",
                Font = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize = true,
                Location = new Point(14, 8),
            };

            var lblPicker = new Label
            {
                Text = "เลือกผู้ใช้:",
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(190, 210, 240),
                AutoSize = true,
                Location = new Point(14, 36),
            };

            _cmbUser = new ComboBox
            {
                Location = new Point(90, 32),
                Size = new Size(420, 26),
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font = new Font("Segoe UI", 9f),
            };
            _cmbUser.SelectedIndexChanged += (s, e) => LoadSelectedUser();

            var btnClose = new Button
            {
                Text = "✕ ปิด",
                Size = new Size(80, 28),
                Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                ForeColor = Color.White,
                BackColor = Color.FromArgb(25, 55, 105),
                FlatStyle = FlatStyle.Flat,
                Cursor = Cursors.Hand,
                Anchor = AnchorStyles.Top | AnchorStyles.Right,
            };
            btnClose.FlatAppearance.BorderColor = Color.FromArgb(190, 210, 240);
            btnClose.FlatAppearance.MouseOverBackColor = Color.FromArgb(45, 85, 145);
            btnClose.Click += (s, e) => Close();
            pnl.Resize += (s, e) => btnClose.Location = new Point(pnl.ClientSize.Width - btnClose.Width - 14, 18);

            pnl.Controls.AddRange(new Control[] { lblTitle, lblPicker, _cmbUser, btnClose });
            return pnl;
        }

        private Panel BuildBody()
        {
            var pnl = new Panel { Dock = DockStyle.Fill, Padding = new Padding(1, 8, 1, 4) };

            // ── สถานะผู้ใช้ ──────────────────────────────────────────────
            var secStatus = MakeSectionBar("สถานะผู้ใช้");
            _lblStatus = new Label
            {
                Dock = DockStyle.Fill,
                Font = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(60, 70, 90),
                Padding = new Padding(8, 6, 8, 6),
                Text = "เลือกผู้ใช้ด้านบนเพื่อดูสถานะ",
            };

            // ── Authority / ระดับชั้นความลับ ────────────────────────────
            var secAuthority = MakeSectionBar("Authority — ระดับชั้นความลับ (FolderAuthorityConfig ตามตำแหน่ง/แผนก/ฝ่าย)");
            _pnlCards = new FlowLayoutPanel
            {
                Dock = DockStyle.Fill,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents = false,
                BackColor = Color.Transparent,
            };
            foreach (var lv in Levels)
                _pnlCards.Controls.Add(BuildLevelCard(lv, null));

            // ── สิทธิ์มองเห็นหน่วยงาน ───────────────────────────────────
            var secVisibility = MakeSectionBar("สิทธิ์มองเห็นหน่วยงาน — OrgUnit Visibility (role-based + explicit grant)");
            _lvVisibility = new ListView
            {
                Dock = DockStyle.Fill,
                View = View.Details,
                FullRowSelect = true,
                GridLines = true,
                HideSelection = false,
            };
            _lvVisibility.Columns.Add("หน่วยงาน", 280);
            _lvVisibility.Columns.Add("รหัสหน่วยงาน", 110);
            _lvVisibility.Columns.Add("ประเภท", 130);
            _lvVisibility.Columns.Add("ให้สิทธิ์โดย", 150);
            _lvVisibility.Columns.Add("วันที่ให้สิทธิ์", 130);

            // ── ACL รายโฟลเดอร์ ─────────────────────────────────────────
            var secAcl = MakeSectionBar("ACL รายโฟลเดอร์ — สิทธิ์เฉพาะที่ถูก grant ตรง (User / Role / Department / Division)");
            _gridAcl = BuildAclGrid();

            var table = new TableLayoutPanel
            {
                Dock = DockStyle.Fill,
                ColumnCount = 1,
                RowCount = 8,
                BackColor = Color.Transparent,
            };
            table.ColumnStyles.Add(new ColumnStyle(SizeType.Percent, 100));
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));   // secStatus
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 34));   // lblStatus
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));   // secAuthority
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 198));  // cards
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));   // secVisibility
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 45));    // visibility grid
            table.RowStyles.Add(new RowStyle(SizeType.Absolute, 26));   // secAcl
            table.RowStyles.Add(new RowStyle(SizeType.Percent, 55));    // acl grid

            table.Controls.Add(secStatus, 0, 0);
            table.Controls.Add(_lblStatus, 0, 1);
            table.Controls.Add(secAuthority, 0, 2);
            table.Controls.Add(_pnlCards, 0, 3);
            table.Controls.Add(secVisibility, 0, 4);
            table.Controls.Add(_lvVisibility, 0, 5);
            table.Controls.Add(secAcl, 0, 6);
            table.Controls.Add(_gridAcl, 0, 7);

            pnl.Controls.Add(table);
            return pnl;
        }

        private DataGridView BuildAclGrid()
        {
            var grid = new DataGridView
            {
                Dock = DockStyle.Fill,
                ReadOnly = true,
                AllowUserToAddRows = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible = false,
                BackgroundColor = Color.White,
                BorderStyle = BorderStyle.None,
                GridColor = Color.FromArgb(228, 232, 240),
                SelectionMode = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells,
                ColumnHeadersHeight = 30,
                EnableHeadersVisualStyles = false,
                DefaultCellStyle = new DataGridViewCellStyle { Font = new Font("Segoe UI", 9f), Padding = new Padding(4, 2, 4, 2) },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    BackColor = Color.FromArgb(25, 55, 105),
                    ForeColor = Color.White,
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(6, 0, 0, 0),
                },
            };

            grid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "colFolder", HeaderText = "ชื่อ Folder", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 180 },
                new DataGridViewTextBoxColumn { Name = "colPrincipal", HeaderText = "ให้สิทธิ์แก่", Width = 150 },
                new DataGridViewTextBoxColumn { Name = "colRead", HeaderText = "ดู", Width = 44 },
                new DataGridViewTextBoxColumn { Name = "colWrite", HeaderText = "เพิ่ม", Width = 52 },
                new DataGridViewTextBoxColumn { Name = "colDelete", HeaderText = "ลบ", Width = 44 },
                new DataGridViewTextBoxColumn { Name = "colModify", HeaderText = "แก้ไข", Width = 60 },
                new DataGridViewTextBoxColumn { Name = "colFullControl", HeaderText = "ควบคุมเต็ม", Width = 80 },
                new DataGridViewTextBoxColumn { Name = "colExpiry", HeaderText = "หมดอายุ", Width = 90 },
                new DataGridViewTextBoxColumn { Name = "colStatus", HeaderText = "สถานะ", Width = 90 },
            });

            foreach (var col in new[] { "colRead", "colWrite", "colDelete", "colModify", "colFullControl" })
                grid.Columns[col].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            return grid;
        }

        // ════════════════════════════════════════════════════════════════════
        // Data Loading
        // ════════════════════════════════════════════════════════════════════

        private void LoadUsers()
        {
            _allUsers = (_userAdminController != null
                ? _userAdminController.GetAllUsers() ?? Enumerable.Empty<UserAccount>()
                : Enumerable.Empty<UserAccount>())
                .OrderBy(u => u.DisplayName ?? u.ADUserName)
                .ToList();

            _cmbUser.BeginUpdate();
            _cmbUser.Items.Clear();
            foreach (var u in _allUsers)
                _cmbUser.Items.Add(u);
            _cmbUser.DisplayMember = "DisplayName";
            _cmbUser.EndUpdate();

            if (_cmbUser.Items.Count > 0)
                _cmbUser.SelectedIndex = 0;
        }

        /// <summary>Pre-selects a user (used when opened from the authority-by-org report).</summary>
        public void SelectUser(int userId)
        {
            var target = _allUsers.FirstOrDefault(u => u.UserID == userId);
            if (target != null)
                _cmbUser.SelectedItem = target;
        }

        private void LoadSelectedUser()
        {
            var target = _cmbUser.SelectedItem as UserAccount;
            if (target == null) return;

            LoadStatus(target);
            LoadAuthorityCards(target);
            LoadVisibility(target);
            LoadAcl(target);
        }

        private void LoadStatus(UserAccount u)
        {
            _lblStatus.Text = string.Format(
                "{0}  ({1})   |   Role: {2}   |   OrgUnitID: {3}   |   {4}   |   {5}",
                u.DisplayName ?? u.ADUserName,
                u.ADUserName,
                RoleName(u.AppRole),
                u.OrgUnitID,
                u.IsActive ? "ใช้งาน" : "ปิดใช้งาน",
                string.IsNullOrEmpty(u.OrgDisplay) ? "(ไม่พบแผนก/ฝ่าย)" : u.OrgDisplay);
            _lblStatus.ForeColor = u.IsActive ? Color.FromArgb(60, 70, 90) : Color.OrangeRed;
        }

        private void LoadAuthorityCards(UserAccount u)
        {
            Dictionary<int, FolderAuthorityConfig> authority = _authorityConfigController != null
                ? (_authorityConfigController.ResolveForUser(u) ?? new Dictionary<int, FolderAuthorityConfig>())
                : new Dictionary<int, FolderAuthorityConfig>();

            _pnlCards.SuspendLayout();
            _pnlCards.Controls.Clear();
            foreach (var lv in Levels)
            {
                FolderAuthorityConfig cfg;
                authority.TryGetValue(lv.Level, out cfg);
                _pnlCards.Controls.Add(BuildLevelCard(lv, cfg));
            }
            _pnlCards.ResumeLayout();
            DistributeCards();
        }

        private void LoadVisibility(UserAccount u)
        {
            _lvVisibility.BeginUpdate();
            _lvVisibility.Items.Clear();

            if (_visibilityController == null || _currentUser == null)
            {
                _lvVisibility.EndUpdate();
                return;
            }

            try
            {
                var roleBased = _visibilityController.GetVisibleOrgUnits(u.UserID, _currentUser) ?? new List<OrgUnit>();
                var explicitGrants = _visibilityController.GetExplicitGrantsForUser(u.UserID, _currentUser) ?? new List<UserOrgVisibility>();
                var grantedOrgIds = new HashSet<int>(explicitGrants.Select(g => g.OrgUnitID));

                foreach (var org in roleBased.OrderBy(o => o.Name))
                {
                    bool isExplicit = grantedOrgIds.Contains(org.OrgUnitID);
                    var grant = isExplicit ? explicitGrants.FirstOrDefault(g => g.OrgUnitID == org.OrgUnitID) : null;

                    var item = new ListViewItem(org.Name);
                    item.SubItems.Add(org.OrgUnitCode ?? "");
                    item.SubItems.Add(isExplicit ? "Explicit grant" : "Role-based");
                    item.SubItems.Add(grant != null ? ResolveUserName(grant.GrantedBy) : "—");
                    item.SubItems.Add(grant != null ? grant.GrantedDate.ToString("dd/MM/yyyy HH:mm") : "—");
                    item.ForeColor = isExplicit ? Color.FromArgb(0, 100, 0) : Color.Gray;
                    _lvVisibility.Items.Add(item);
                }

                if (roleBased.Count == 0)
                {
                    var empty = new ListViewItem("(ไม่มีหน่วยงานที่มองเห็นได้)");
                    empty.ForeColor = Color.OrangeRed;
                    _lvVisibility.Items.Add(empty);
                }
            }
            catch (Exception ex)
            {
                var errItem = new ListViewItem("โหลดไม่สำเร็จ: " + ex.Message);
                errItem.ForeColor = Color.Red;
                _lvVisibility.Items.Add(errItem);
            }

            _lvVisibility.EndUpdate();
        }

        private void LoadAcl(UserAccount u)
        {
            _gridAcl.Rows.Clear();

            if (_aclLookupController == null)
            {
                AddInfoRow("(ไม่สามารถโหลด ACL — Permission ACL lookup controller ไม่พร้อมใช้งาน)");
                return;
            }

            List<FolderPermission> perms;
            try
            {
                perms = _aclLookupController
                    .GetAllForUser(u.UserID, u.AppRole, u.DepartmentId, u.DivisionId)
                    .Where(p => p.RevokedDate == null)
                    .OrderBy(p => p.FolderID)
                    .ToList();
            }
            catch (Exception ex)
            {
                AddInfoRow("โหลด ACL ไม่สำเร็จ: " + ex.Message);
                return;
            }

            if (perms.Count == 0)
            {
                AddInfoRow("ไม่มี ACL ที่ grant ตรงให้ user นี้ (โดยตัวเอง / Role / แผนก / ฝ่าย)");
                return;
            }

            foreach (var p in perms)
            {
                string folderName = string.Format("Folder #{0}", p.FolderID);
                if (_shareLookupController != null)
                {
                    try
                    {
                        var folder = _shareLookupController.GetFolderById(p.FolderID);
                        if (folder != null) folderName = folder.FolderName;
                    }
                    catch { }
                }

                string principal = _shareLookupController != null
                    ? SafeResolvePrincipalName(p.PrincipalTypeInt, p.PrincipalID)
                    : string.Format("{0} #{1}", p.PrincipalType, p.PrincipalID);

                bool isExpired = p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today;
                string expiry = p.ExpiryDate.HasValue ? p.ExpiryDate.Value.ToString("dd/MM/yyyy") : "ไม่มีกำหนด";
                string status = p.IsDeny ? "🚫 Deny" : isExpired ? "⚠ หมดอายุ" : "✔ ใช้งาน";

                int idx = _gridAcl.Rows.Add(
                    p.IsDeny ? "⊘ " + folderName : folderName,
                    principal,
                    p.CanRead ? "✓" : "—",
                    p.CanWrite ? "✓" : "—",
                    p.CanDelete ? "✓" : "—",
                    p.CanModify ? "✓" : "—",
                    p.CanFullControl ? "✓" : "—",
                    isExpired ? "⚠ " + expiry : expiry,
                    status);

                var row = _gridAcl.Rows[idx];
                if (p.IsDeny)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 235);
                    row.DefaultCellStyle.ForeColor = Color.Crimson;
                }
                else if (isExpired)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 215);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(150, 80, 0);
                }
            }
        }

        private string SafeResolvePrincipalName(int principalType, int principalId)
        {
            try { return _shareLookupController.ResolvePrincipalName(principalType, principalId); }
            catch { return string.Format("Type {0} #{1}", principalType, principalId); }
        }

        private string ResolveUserName(int userId)
        {
            var u = _allUsers.FirstOrDefault(x => x.UserID == userId);
            if (u != null) return u.DisplayName ?? u.ADUserName;
            return userId == (_currentUser != null ? _currentUser.UserID : -1)
                ? (_currentUser.ADUserName ?? string.Format("UserID:{0}", userId))
                : string.Format("UserID:{0}", userId);
        }

        // ════════════════════════════════════════════════════════════════════
        // Level Card
        // ════════════════════════════════════════════════════════════════════

        private Panel BuildLevelCard(LevelDefinition lv, FolderAuthorityConfig cfg)
        {
            bool canView = cfg != null && cfg.CanView;
            bool canAdd = cfg != null && cfg.CanAdd;
            bool canEdit = cfg != null && cfg.CanEdit;
            bool canApprove = cfg != null && cfg.CanApprove;
            bool anyPerm = canView || canAdd || canEdit || canApprove;

            Color barColor = anyPerm ? lv.Accent : Color.FromArgb(200, 200, 200);
            Color textAccent = anyPerm ? lv.Accent : Color.FromArgb(160, 160, 160);
            Color cardBg = anyPerm ? Color.White : Color.FromArgb(250, 250, 250);

            var card = new Panel { Margin = new Padding(0, 0, 8, 0), Height = 190, BackColor = cardBg };

            var stripe = new Panel { Dock = DockStyle.Top, Height = 5, BackColor = barColor };

            var lblCode = new Label
            {
                Text = lv.Code, Font = new Font("Segoe UI", 18f, FontStyle.Bold), ForeColor = textAccent,
                Size = new Size(70, 36), Location = new Point(10, 10), TextAlign = ContentAlignment.MiddleLeft,
            };

            string statusText = anyPerm ? "มีสิทธิ์" : "ไม่มีสิทธิ์";
            Color statusBg = anyPerm ? Color.FromArgb(220, 250, 230) : Color.FromArgb(250, 220, 220);
            Color statusFg = anyPerm ? Color.FromArgb(0, 120, 50) : Color.FromArgb(160, 0, 0);
            var lblStatus = new Label
            {
                Text = statusText, Font = new Font("Segoe UI", 7.5f, FontStyle.Bold), ForeColor = statusFg,
                BackColor = statusBg, Size = new Size(62, 18), Location = new Point(10, 48),
                TextAlign = ContentAlignment.MiddleCenter,
            };

            var lblSub = new Label
            {
                Text = lv.Label, Font = new Font("Segoe UI", 7.5f), ForeColor = Color.FromArgb(100, 100, 110),
                Size = new Size(82, 30), Location = new Point(10, 70), TextAlign = ContentAlignment.TopLeft,
            };

            var lblMatch = new Label
            {
                Text = cfg != null ? ("Match: " + (cfg.ConfigLabel ?? "—")) : "Match: (ไม่พบ config)",
                Font = new Font("Segoe UI", 7f, FontStyle.Italic), ForeColor = Color.FromArgb(120, 120, 130),
                Size = new Size(card.Width - 16, 14), Location = new Point(10, 100),
                AutoEllipsis = true,
            };

            var perms = new[]
            {
                new KeyValuePair<string, bool>("ดู", canView),
                new KeyValuePair<string, bool>("เพิ่ม", canAdd),
                new KeyValuePair<string, bool>("แก้ไข", canEdit),
                new KeyValuePair<string, bool>("อนุมัติ", canApprove),
            };

            var pnlBadges = new FlowLayoutPanel
            {
                Location = new Point(8, 118), Size = new Size(card.Width - 16, 64),
                FlowDirection = FlowDirection.LeftToRight, WrapContents = true, BackColor = Color.Transparent,
                Anchor = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            };
            foreach (var perm in perms)
            {
                var allowed = perm.Value;
                var badge = new Label
                {
                    Text = (allowed ? "✓ " : "✗ ") + perm.Key,
                    Font = new Font("Segoe UI", 7.5f, allowed ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = allowed ? Color.FromArgb(0, 120, 50) : Color.FromArgb(170, 170, 170),
                    BackColor = allowed ? Color.FromArgb(230, 252, 238) : Color.FromArgb(245, 245, 245),
                    Size = new Size(76, 20), Margin = new Padding(0, 0, 4, 4),
                    TextAlign = ContentAlignment.MiddleLeft, Padding = new Padding(4, 0, 0, 0),
                };
                pnlBadges.Controls.Add(badge);
            }

            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(anyPerm ? barColor : Color.FromArgb(215, 215, 215), 1.5f))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            card.Controls.AddRange(new Control[] { stripe, lblCode, lblStatus, lblSub, lblMatch, pnlBadges });
            return card;
        }

        private void DistributeCards()
        {
            int count = _pnlCards.Controls.Count;
            if (count == 0 || _pnlCards.Width <= 0) return;
            int gap = 8;
            int cardW = (_pnlCards.Width - gap * (count - 1)) / count;
            foreach (Control c in _pnlCards.Controls)
                c.Width = Math.Max(cardW, 140);
        }

        // ════════════════════════════════════════════════════════════════════
        // Helpers
        // ════════════════════════════════════════════════════════════════════

        private void AddInfoRow(string msg)
        {
            int idx = _gridAcl.Rows.Add(msg, "", "", "", "", "", "", "", "");
            _gridAcl.Rows[idx].DefaultCellStyle.ForeColor = Color.FromArgb(130, 130, 140);
            _gridAcl.Rows[idx].DefaultCellStyle.Font = new Font("Segoe UI", 8.5f, FontStyle.Italic);
        }

        private static string RoleName(int appRole)
        {
            switch (appRole)
            {
                case 1: return "Officer";
                case 2: return "SrOfficer";
                case 3: return "Manager";
                case 4: return "Admin";
                default: return appRole.ToString();
            }
        }

        private static Label MakeSectionBar(string text)
        {
            return new Label
            {
                Dock = DockStyle.Fill,
                Text = "  " + text,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 55, 105),
                BackColor = Color.FromArgb(215, 228, 248),
                TextAlign = ContentAlignment.MiddleLeft,
            };
        }
    }
}
