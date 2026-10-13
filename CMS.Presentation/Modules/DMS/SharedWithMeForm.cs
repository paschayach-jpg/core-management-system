using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    /// <summary>
    /// Inbox แสดง Folder และ File ที่ผู้อื่น share ให้ user ปัจจุบัน (แบบ Inbox Mail)
    /// มีสถานะ ยังไม่อ่าน (🔴) / อ่านแล้ว ต่อผู้ใช้
    /// </summary>
    public class SharedWithMeForm : Form
    {
        private readonly UserContext              _user;
        private readonly IShareLookupController   _shareLookupController;
        private readonly bool                     _isAdmin;

        private DataGridView _grid;
        private Label        _lblCount;
        private Button       _btnRevoke;
        private Button       _btnMarkAllRead;
        private ComboBox     _cboFilter;
        private ContextMenuStrip _gridContextMenu;

        // ── สาธารณะ: จำนวนรายการยังไม่อ่าน ───────────
        public int UnreadCount { get; private set; }
        public int NewItemCount => UnreadCount; // backward compatibility

        public event Action<int> UnreadCountChanged;

        /// <summary>
        /// Fired เมื่อ user ดับเบิลคลิกรายการ — MDI_Container subscribe แล้ว navigate FolderForm
        /// </summary>
        public event Action<int> OpenFolderRequested;

        /// <summary>
        /// Fired เมื่อ user ดับเบิลคลิกไฟล์ — MDI_Container subscribe แล้วเปิด FilesByFolderForm
        /// </summary>
        public event Action<int, int> OpenFileRequested; // (folderId, fileId)

        public SharedWithMeForm(
            UserContext               user,
            IShareLookupController    shareLookupController)
        {
            _user                  = user ?? throw new ArgumentNullException("user");
            _shareLookupController = shareLookupController;
            _isAdmin               = user.AppRole == 4;
            BuildUI();
        }

        // ════════════════════════════════════════════════════════════════════
        // UI Construction
        // ════════════════════════════════════════════════════════════════════

        private void BuildUI()
        {
            // ── Palette ─────────────────────────────────────────────────────
            Color navyDark  = Color.FromArgb(25,  55,  105);
            Color navyLight = Color.FromArgb(215, 228, 248);
            Color bodyBg    = Color.FromArgb(243, 246, 251);
            Color footerBg  = Color.FromArgb(228, 234, 245);
            var   uiFont    = new Font("Segoe UI", 9f);
            var   boldFont  = new Font("Segoe UI", 9f, FontStyle.Bold);

            Text            = "📥  แชร์กับฉัน  (Shared With Me)";
            ClientSize      = new Size(880, 560);
            MinimumSize     = new Size(720, 440);
            StartPosition   = FormStartPosition.CenterParent;
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox     = true;
            MinimizeBox     = false;
            BackColor       = bodyBg;
            Font            = uiFont;

            // ── Header ──────────────────────────────────────────────────────
            var pnlHeader = new Panel { Dock = DockStyle.Top, Height = 70, BackColor = navyDark };

            var pnlIcon = new Panel { Size = new Size(40, 40), Location = new Point(14, 15), BackColor = Color.Transparent };
            pnlIcon.Paint += (s, e) =>
            {
                e.Graphics.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                e.Graphics.FillEllipse(new SolidBrush(Color.FromArgb(50, 90, 150)), 0, 0, 39, 39);
                using (var f = new Font("Segoe UI", 16f))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    e.Graphics.DrawString("📥", f, Brushes.White, new RectangleF(0, 0, 40, 40), sf);
            };

            var lblTitle = new Label
            {
                Text      = "แชร์กับฉัน  —  Shared With Me",
                Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(62, 12),
            };
            _lblCount = new Label
            {
                Text      = "กำลังโหลด...",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(170, 200, 240),
                AutoSize  = true,
                Location  = new Point(63, 38),
            };
            pnlHeader.Controls.AddRange(new Control[] { pnlIcon, lblTitle, _lblCount });

            // ── Filter bar ──────────────────────────────────────────────────
            var pnlFilter = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 36,
                BackColor = navyLight,
                Padding   = new Padding(10, 5, 10, 5),
            };
            var lblFilter = new Label
            {
                Text      = "  กรอง :",
                Font      = boldFont,
                ForeColor = navyDark,
                AutoSize  = true,
                Location  = new Point(8, 9),
            };
            _cboFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = uiFont,
                Width         = 150,
                Location      = new Point(68, 6),
                FlatStyle     = FlatStyle.Flat,
                BackColor     = Color.White,
            };
            _cboFilter.Items.AddRange(new[] { "ทั้งหมด", "ยังไม่อ่าน", "ใช้งานได้", "หมดอายุ", "ถูกลบ", "Deny" });
            _cboFilter.SelectedIndex = 0;
            _cboFilter.SelectedIndexChanged += (s, e) => ApplyFilter(_cboFilter.SelectedItem.ToString());
            pnlFilter.Controls.AddRange(new Control[] { lblFilter, _cboFilter });

            // ── Context Menu for Grid ────────────────────────────────────────
            _gridContextMenu = new ContextMenuStrip();
            var mnuOpen = new ToolStripMenuItem("📂  เปิด", null, (s, e) => OpenCurrentRow());
            var mnuMarkRead = new ToolStripMenuItem("✔  ทำเครื่องหมายว่าอ่านแล้ว", null, (s, e) => MarkCurrentRowRead());
            _gridContextMenu.Items.Add(mnuOpen);
            _gridContextMenu.Items.Add(mnuMarkRead);

            // ── DataGridView ─────────────────────────────────────────────────
            _grid = new DataGridView
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
                ContextMenuStrip          = _gridContextMenu,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font    = uiFont,
                    Padding = new Padding(4, 2, 4, 2),
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Font      = boldFont,
                    BackColor = navyDark,
                    ForeColor = Color.White,
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding   = new Padding(6, 0, 0, 0),
                },
                AlternatingRowsDefaultCellStyle = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(249, 251, 255),
                },
            };

            _grid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "colNew",     HeaderText = "",                Width = 28,  MinimumWidth = 28,  AutoSizeMode = DataGridViewAutoSizeColumnMode.None },
                new DataGridViewTextBoxColumn { Name = "colType",    HeaderText = "ประเภท",           Width = 70,  MinimumWidth = 60,  AutoSizeMode = DataGridViewAutoSizeColumnMode.None },
                new DataGridViewTextBoxColumn { Name = "colFolder",  HeaderText = "รายการที่แชร์",   MinimumWidth = 180, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { Name = "colFileName",HeaderText = "ชื่อไฟล์",          Width = 180, MinimumWidth = 140, AutoSizeMode = DataGridViewAutoSizeColumnMode.None },
                new DataGridViewTextBoxColumn { Name = "colLevel",   HeaderText = "ระดับ",             Width = 60,  AutoSizeMode = DataGridViewAutoSizeColumnMode.None },
                new DataGridViewTextBoxColumn { Name = "colPerms",   HeaderText = "สิทธิ์ที่ได้รับ",  Width = 120, AutoSizeMode = DataGridViewAutoSizeColumnMode.None },
                new DataGridViewTextBoxColumn { Name = "colGrantBy", HeaderText = "แชร์โดย",          Width = 130, AutoSizeMode = DataGridViewAutoSizeColumnMode.None },
                new DataGridViewTextBoxColumn { Name = "colDate",    HeaderText = "วันที่แชร์",       Width = 100, AutoSizeMode = DataGridViewAutoSizeColumnMode.None },
                new DataGridViewTextBoxColumn { Name = "colExpiry",  HeaderText = "หมดอายุ",           Width = 100, AutoSizeMode = DataGridViewAutoSizeColumnMode.None },
                new DataGridViewTextBoxColumn { Name = "colStatus",  HeaderText = "สถานะ",             Width = 85,  AutoSizeMode = DataGridViewAutoSizeColumnMode.None },
            });

            // ตรึง colNew ให้กว้างคงที่
            _grid.Columns["colNew"].DefaultCellStyle.Alignment  = DataGridViewContentAlignment.MiddleCenter;
            _grid.Columns["colNew"].DefaultCellStyle.Font       = new Font("Segoe UI", 10f);
            _grid.Columns["colType"].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            // Row coloring
            _grid.RowPrePaint += Grid_RowPrePaint;

            // Double-click / Enter: fire event → MDI_Container navigate
            _grid.CellDoubleClick += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                OpenCurrentRow();
            };

            _grid.KeyDown += (s, e) =>
            {
                if (e.KeyCode != Keys.Enter || _grid.CurrentRow == null) return;
                OpenCurrentRow();
                e.Handled = true;
            };

            // ── Footer ──────────────────────────────────────────────────────
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = footerBg };

            var lblHint = new Label
            {
                Text      = "💡  ดับเบิลคลิก Folder = เปิด Folder  |  ดับเบิลคลิก ไฟล์ = เปิดไฟล์  |  🔴 = รายการที่ยังไม่อ่าน",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(90, 100, 130),
                AutoSize  = true,
                Location  = new Point(12, 16),
            };

            _btnMarkAllRead = new Button
            {
                Text      = "✔  อ่านทั้งหมด",
                Size      = new Size(110, 32),
                Anchor    = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(230, 240, 255),
                ForeColor = Color.FromArgb(25, 55, 105),
                Font      = boldFont,
            };
            _btnMarkAllRead.FlatAppearance.BorderColor = Color.FromArgb(170, 200, 240);
            _btnMarkAllRead.FlatAppearance.BorderSize  = 1;
            _btnMarkAllRead.Click += BtnMarkAllRead_Click;

            var btnRefresh = new Button
            {
                Text      = "↺  รีเฟรช",
                Size      = new Size(90, 32),
                Anchor    = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(220, 228, 245),
                ForeColor = navyDark,
                Font      = boldFont,
            };
            btnRefresh.FlatAppearance.BorderColor = Color.FromArgb(180, 200, 235);
            btnRefresh.FlatAppearance.BorderSize  = 1;
            btnRefresh.Click += (s, e) => LoadData();

            var btnClose = new Button
            {
                Text         = "✕  ปิด",
                Size         = new Size(90, 32),
                Anchor       = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle    = FlatStyle.Flat,
                BackColor    = navyDark,
                ForeColor    = Color.White,
                Font         = boldFont,
                DialogResult = DialogResult.Cancel,
            };
            btnClose.FlatAppearance.BorderSize = 0;

            _btnRevoke = new Button
            {
                Text      = "🚫 ยกเลิกสิทธิ์",
                Size      = new Size(110, 32),
                Anchor    = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(200, 60, 60),
                ForeColor = Color.White,
                Font      = boldFont,
                Visible   = _isAdmin,
            };
            _btnRevoke.FlatAppearance.BorderSize = 0;
            _btnRevoke.Click += BtnRevoke_Click;

            void LayoutFooter()
            {
                int right = pnlFooter.Width - 10;
                btnClose.Location = new Point(right - btnClose.Width, 9);
                right -= btnClose.Width + 8;
                btnRefresh.Location = new Point(right - btnRefresh.Width, 9);
                right -= btnRefresh.Width + 8;
                _btnMarkAllRead.Location = new Point(right - _btnMarkAllRead.Width, 9);
                right -= _btnMarkAllRead.Width + 8;
                _btnRevoke.Location = new Point(right - _btnRevoke.Width, 9);
            }

            pnlFooter.Resize += (s, e) => LayoutFooter();
            pnlFooter.Controls.AddRange(new Control[] { lblHint, _btnRevoke, _btnMarkAllRead, btnRefresh, btnClose });
            LayoutFooter();

            // ── Layout ──────────────────────────────────────────────────────
            var pnlBody = new Panel { Dock = DockStyle.Fill, Padding = new Padding(10, 6, 10, 0), BackColor = bodyBg };
            pnlBody.Controls.Add(_grid);

            Controls.Add(pnlBody);
            Controls.Add(pnlFilter);
            Controls.Add(pnlHeader);
            Controls.Add(pnlFooter);
        }

        // ════════════════════════════════════════════════════════════════════
        // Data
        // ════════════════════════════════════════════════════════════════════

        private List<SharedRow> _allRows = new List<SharedRow>();

        protected override void OnLoad(EventArgs e)
        {
            base.OnLoad(e);
            LoadData();
        }

        public void LoadData()
        {
            _allRows.Clear();
            _grid.Rows.Clear();
            UnreadCount = 0;

            if (_shareLookupController == null)
            {
                AddInfoRow("(ไม่สามารถโหลดข้อมูล — Share lookup controller ไม่พร้อมใช้งาน)");
                _lblCount.Text = "ไม่สามารถโหลดข้อมูลได้";
                return;
            }

            try
            {
                var readKeys = _shareLookupController.GetReadShareKeys(_user.UserID);

                // ── เงื่อนไขที่ 1: FolderPermission (สิทธิ์ระดับ Folder) ──────
                var folderPerms = _shareLookupController.GetFolderPermissionsForUser(_user.UserID);
                foreach (var p in folderPerms)
                {
                    if (p.RevokedDate.HasValue) continue;

                    string folderName = "(ไม่พบ)";
                    string levelCode  = "—";
                    var folder = _shareLookupController.GetFolderById(p.FolderID);
                    if (folder != null)
                    {
                        folderName = folder.FolderName;
                        levelCode  = LevelCode(folder.ConfidentialityLevel);
                    }

                    bool isUnread  = !readKeys.Contains(string.Format("1_{0}", p.PermissionID));
                    bool isExpired = p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today;
                    bool isDeny    = p.IsDeny;
                    bool isDeleted = IsFolderGone(folder);

                    _allRows.Add(new SharedRow
                    {
                        FolderID     = p.FolderID,
                        PermissionID = p.PermissionID,
                        IsFolderPerm = true,
                        ItemType     = "📁 Folder",
                        FileName     = string.Empty,
                        IsUnread     = isUnread,
                        IsExpired    = isExpired,
                        IsDeny       = isDeny,
                        IsDeleted    = isDeleted,
                        DisplayName  = folderName,
                        LevelCode    = levelCode,
                        Perms        = BuildPermText(p),
                        GrantedBy    = ResolveGranterName(p.GrantedBy),
                        GrantedDate  = p.GrantedDate,
                        Expiry       = p.ExpiryDate.HasValue ? p.ExpiryDate.Value.ToString("dd/MM/yyyy") : "ไม่มีกำหนด",
                        Status       = BuildStatusText(isDeny, isDeleted, isExpired),
                    });

                    if (isUnread && !isDeny && !isExpired && !isDeleted) UnreadCount++;
                }

                // ── เงื่อนไขที่ 2: FilePermission (สิทธิ์ระดับ File) ─
                var filePerms = _shareLookupController.GetFilePermissionsForUser(_user.UserID);
                foreach (var p in filePerms)
                {
                    if (p.RevokedDate.HasValue) continue;

                    var doc = _shareLookupController.GetFileById(p.FileID);
                    if (doc == null) continue;

                    string fileName  = doc.OriginalFileName;
                    int    folderId  = doc.FolderID;
                    string levelCode = "—";

                    var folder = _shareLookupController.GetFolderById(folderId);
                    if (folder != null)
                    {
                        levelCode = LevelCode(folder.ConfidentialityLevel);
                    }

                    bool isUnread  = !readKeys.Contains(string.Format("2_{0}", p.FilePermissionID));
                    bool isExpired = p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today;
                    bool isDeny    = p.IsDeny;
                    bool isDeleted = IsFileGone(doc, folder);

                    _allRows.Add(new SharedRow
                    {
                        FolderID     = folderId,
                        FileID       = p.FileID,
                        PermissionID = p.FilePermissionID,
                        IsFolderPerm = false,
                        ItemType     = "📄 ไฟล์",
                        FileName     = fileName,
                        IsUnread     = isUnread,
                        IsExpired    = isExpired,
                        IsDeny       = isDeny,
                        IsDeleted    = isDeleted,
                        DisplayName  = fileName,
                        LevelCode    = levelCode,
                        Perms        = BuildFilePermText(p),
                        GrantedBy    = ResolveGranterName(p.GrantedBy),
                        GrantedDate  = p.GrantedDate,
                        Expiry       = p.ExpiryDate.HasValue ? p.ExpiryDate.Value.ToString("dd/MM/yyyy") : "ไม่มีกำหนด",
                        Status       = BuildStatusText(isDeny, isDeleted, isExpired),
                    });

                    if (isUnread && !isDeny && !isExpired && !isDeleted) UnreadCount++;
                }

                // ── เรียงลำดับ: ยังไม่อ่านก่อน → วันที่แชร์ล่าสุด ────────────────
                _allRows = _allRows.OrderByDescending(r => r.IsUnread)
                                   .ThenByDescending(r => r.GrantedDate)
                                   .ToList();

                if (_allRows.Count == 0)
                {
                    AddInfoRow("ยังไม่มีรายการที่แชร์ให้คุณ");
                    _lblCount.Text = "ยังไม่มีรายการ";
                    UnreadCountChanged?.Invoke(0);
                    return;
                }

                int activeCount = _allRows.Count(r => r.IsUsable);
                _lblCount.Text = string.Format(
                    "ทั้งหมด {0} รายการ  |  ✔ ใช้งานได้ {1}  |  🔴 ยังไม่อ่าน {2} รายการ",
                    _allRows.Count, activeCount, UnreadCount);

                ApplyFilter(_cboFilter.SelectedItem?.ToString() ?? "ทั้งหมด");
                UnreadCountChanged?.Invoke(UnreadCount);
            }
            catch (Exception ex)
            {
                AddInfoRow(string.Format("เกิดข้อผิดพลาด: {0}", ex.Message));
            }
        }

        private void RenderRows(IEnumerable<SharedRow> rows)
        {
            _grid.Rows.Clear();
            foreach (var r in rows)
            {
                int idx = _grid.Rows.Add(
                    r.IsUnread ? "🔴" : "",
                    r.ItemType,
                    r.DisplayName,
                    r.FileName,
                    r.LevelCode,
                    r.Perms,
                    r.GrantedBy,
                    r.GrantedDate.ToString("dd/MM/yyyy"),
                    r.Expiry,
                    r.Status
                );
                var row = _grid.Rows[idx];
                row.Tag = r;

                if (r.IsDeny)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 235);
                    row.DefaultCellStyle.ForeColor = Color.Crimson;
                }
                else if (r.IsDeleted)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(240, 240, 240);
                    row.DefaultCellStyle.ForeColor = Color.Gray;
                    row.DefaultCellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Italic | FontStyle.Strikeout);
                }
                else if (r.IsExpired)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 215);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(150, 80, 0);
                    row.DefaultCellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Strikeout);
                }
                else if (r.IsUnread)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(235, 248, 255);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(0, 70, 140);
                    row.DefaultCellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Bold);
                }
            }
        }

        private void ApplyFilter(string filter)
        {
            IEnumerable<SharedRow> filtered = _allRows;
            switch (filter)
            {
                case "ยังไม่อ่าน": filtered = _allRows.Where(r => r.IsUnread && r.IsUsable); break;
                case "ใช้งานได้":  filtered = _allRows.Where(r => r.IsUsable);        break;
                case "หมดอายุ":    filtered = _allRows.Where(r => r.IsExpired);                       break;
                case "ถูกลบ":      filtered = _allRows.Where(r => r.IsDeleted);                       break;
                case "Deny":       filtered = _allRows.Where(r => r.IsDeny);                          break;
            }
            RenderRows(filtered);
        }

        private void Grid_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = _grid.Rows[e.RowIndex];
            if (row.DefaultCellStyle.BackColor != Color.Empty
             && row.DefaultCellStyle.BackColor != Color.White
             && row.DefaultCellStyle.BackColor != Color.FromArgb(249, 251, 255))
                return;

            row.DefaultCellStyle.BackColor = e.RowIndex % 2 == 0
                ? Color.White
                : Color.FromArgb(249, 251, 255);
        }

        private void MarkRowAsRead(SharedRow row)
        {
            if (row == null || !row.IsUnread) return;

            _shareLookupController?.MarkShareRead(
                _user.UserID,
                row.IsFolderPerm ? ShareInboxKind.Folder : ShareInboxKind.File,
                row.PermissionID);

            row.IsUnread = false;

            // Update visual in grid
            foreach (DataGridViewRow gridRow in _grid.Rows)
            {
                if (gridRow.Tag == row)
                {
                    gridRow.Cells["colNew"].Value = "";
                    if (row.IsUsable)
                    {
                        gridRow.DefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
                        gridRow.DefaultCellStyle.ForeColor = Color.Black;
                        gridRow.DefaultCellStyle.BackColor = Color.White;
                    }
                    break;
                }
            }

            int activeCount = _allRows.Count(r => r.IsUsable);
            UnreadCount = _allRows.Count(r => r.IsUnread && r.IsUsable);
            _lblCount.Text = string.Format(
                "ทั้งหมด {0} รายการ  |  ✔ ใช้งานได้ {1}  |  🔴 ยังไม่อ่าน {2} รายการ",
                _allRows.Count, activeCount, UnreadCount);

            UnreadCountChanged?.Invoke(UnreadCount);
        }

        private void OpenCurrentRow()
        {
            var row = _grid.CurrentRow?.Tag as SharedRow;
            if (row == null) return;
            if (!CanOpenSharedRow(row)) return;

            MarkRowAsRead(row);

            if (row.FileID > 0)
                OpenFileRequested?.Invoke(row.FolderID, row.FileID);
            else if (row.FolderID > 0)
                OpenFolderRequested?.Invoke(row.FolderID);
        }

        private void MarkCurrentRowRead()
        {
            var row = _grid.CurrentRow?.Tag as SharedRow;
            if (row == null) return;
            MarkRowAsRead(row);
        }

        private void BtnMarkAllRead_Click(object sender, EventArgs e)
        {
            if (_shareLookupController == null) return;

            _shareLookupController.MarkAllSharesRead(_user.UserID);
            foreach (var r in _allRows)
                r.IsUnread = false;

            foreach (DataGridViewRow gridRow in _grid.Rows)
            {
                gridRow.Cells["colNew"].Value = "";
                var r = gridRow.Tag as SharedRow;
                if (r != null && r.IsUsable)
                {
                    gridRow.DefaultCellStyle.Font = new Font("Segoe UI", 9f, FontStyle.Regular);
                    gridRow.DefaultCellStyle.ForeColor = Color.Black;
                    gridRow.DefaultCellStyle.BackColor = Color.White;
                }
            }

            UnreadCount = 0;
            int activeCount = _allRows.Count(r => r.IsUsable);
            _lblCount.Text = string.Format(
                "ทั้งหมด {0} รายการ  |  ✔ ใช้งานได้ {1}  |  🔴 ยังไม่อ่าน 0 รายการ",
                _allRows.Count, activeCount);

            UnreadCountChanged?.Invoke(0);
        }

        // ════════════════════════════════════════════════════════════════════
        // Admin Actions
        // ════════════════════════════════════════════════════════════════════

        private void BtnRevoke_Click(object sender, EventArgs e)
        {
            if (!_isAdmin || _shareLookupController == null) return;

            var row = _grid.CurrentRow?.Tag as SharedRow;
            if (row == null || row.PermissionID <= 0)
            {
                MessageBox.Show("กรุณาเลือกรายการที่ต้องการยกเลิกสิทธิ์", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string itemName = row.DisplayName;
            var confirm = MessageBox.Show(
                string.Format("ต้องการยกเลิกสิทธิ์รายการ \"{0}\" ใช่หรือไม่?", itemName),
                "ยืนยันการยกเลิกสิทธิ์",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                if (row.IsFolderPerm)
                    _shareLookupController.RevokeFolderPermission(row.PermissionID, _user.UserID);
                else
                    _shareLookupController.RevokeFilePermission(row.PermissionID, _user.UserID);

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาด: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Helpers
        // ════════════════════════════════════════════════════════════════════

        private void AddInfoRow(string msg)
        {
            int idx = _grid.Rows.Add("", "", msg, "", "", "", "", "", "", "");
            _grid.Rows[idx].DefaultCellStyle.ForeColor = Color.FromArgb(130, 130, 140);
            _grid.Rows[idx].DefaultCellStyle.Font      = new Font("Segoe UI", 8.5f, FontStyle.Italic);
        }

        private bool CanOpenSharedRow(SharedRow row)
        {
            if (row == null)
                return false;

            if (row.IsDeny)
            {
                MessageBox.Show("รายการนี้ถูกปฏิเสธสิทธิ์ จึงไม่สามารถเปิดใช้งานได้", "แชร์กับฉัน",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (row.IsDeleted)
            {
                MessageBox.Show("รายการนี้ถูกลบแล้ว จึงไม่สามารถเปิดใช้งานได้", "แชร์กับฉัน",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            if (row.IsExpired)
            {
                MessageBox.Show("รายการนี้หมดอายุแล้ว จึงไม่สามารถเปิดใช้งานได้", "แชร์กับฉัน",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                return false;
            }

            // ตรวจซ้ำกับฐานข้อมูล — รายการอาจถูกลบหลังจากโหลดหน้าจอ
            bool deletedSinceLoad = row.FileID > 0
                ? IsFileGone(_shareLookupController.GetFileById(row.FileID),
                             _shareLookupController.GetFolderById(row.FolderID))
                : IsFolderGone(_shareLookupController.GetFolderById(row.FolderID));
            if (deletedSinceLoad)
            {
                MessageBox.Show("รายการนี้ถูกลบแล้ว จึงไม่สามารถเปิดใช้งานได้", "แชร์กับฉัน",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
                LoadData();
                return false;
            }

            if (row.FileID > 0)
            {
                var activeFileShare = _shareLookupController
                    .GetFilePermissionsForUser(_user.UserID)
                    .Any(p => p.FileID == row.FileID
                           && !p.IsDeny
                           && p.CanRead
                           && !p.RevokedDate.HasValue
                           && !(p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today));

                if (!activeFileShare)
                {
                    MessageBox.Show("สิทธิ์แชร์ไฟล์นี้ถูกยกเลิกหรือหมดอายุแล้ว", "แชร์กับฉัน",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    return false;
                }
            }
            else if (row.FolderID > 0)
            {
                var activeFolderShare = _shareLookupController
                    .GetFolderPermissionsForUser(_user.UserID)
                    .Any(p => p.FolderID == row.FolderID
                           && !p.IsDeny
                           && p.CanRead
                           && !p.RevokedDate.HasValue
                           && !(p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today));

                if (!activeFolderShare)
                {
                    MessageBox.Show("สิทธิ์แชร์โฟลเดอร์นี้ถูกยกเลิกหรือหมดอายุแล้ว", "แชร์กับฉัน",
                        MessageBoxButtons.OK, MessageBoxIcon.Information);
                    LoadData();
                    return false;
                }
            }

            return true;
        }

        private string ResolveGranterName(int userId)
        {
            if (_shareLookupController == null)
                return string.Format("UserID: {0}", userId);

            return _shareLookupController.ResolveGranterName(userId);
        }

        private static string BuildPermText(FolderPermission p)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (p.CanRead)        parts.Add("👁 ดู");
            if (p.CanWrite)       parts.Add("✏ เพิ่ม");
            if (p.CanDelete)      parts.Add("🗑 ลบ");
            if (p.CanModify)      parts.Add("📝 แก้ไข");
            if (p.CanFullControl) parts.Add("🔑 ทั้งหมด");
            return parts.Count > 0 ? string.Join("  ", parts) : "(ไม่มีสิทธิ์)";
        }

        private static string BuildFilePermText(FilePermission p)
        {
            var parts = new System.Collections.Generic.List<string>();
            if (p.CanRead)        parts.Add("👁 ดู");
            if (p.CanDownload)    parts.Add("⬇ ดาวน์โหลด");
            if (p.CanWrite)       parts.Add("✏ เพิ่ม");
            if (p.CanDelete)      parts.Add("🗑 ลบ");
            if (p.CanModify)      parts.Add("📝 แก้ไข");
            if (p.CanFullControl) parts.Add("🔑 ทั้งหมด");
            return parts.Count > 0 ? string.Join("  ", parts) : "(ไม่มีสิทธิ์)";
        }

        private static string BuildStatusText(bool isDeny, bool isDeleted, bool isExpired)
        {
            if (isDeny)    return "🚫 Deny";
            if (isDeleted) return "🗑 ถูกลบแล้ว";
            if (isExpired) return "⚠ หมดอายุ";
            return "✔ ใช้งาน";
        }

        // Folder ไม่พบ (ลบถาวร) หรืออยู่ในถังขยะ
        private static bool IsFolderGone(Folder folder)
        {
            return folder == null || folder.IsDeleted;
        }

        // ไฟล์ไม่พบ, อยู่ในถังขยะ, หรือ Folder ที่เก็บไฟล์ถูกลบ
        private static bool IsFileGone(FileDocument doc, Folder folder)
        {
            return doc == null || doc.IsDeleted || doc.DeletedDate.HasValue
                || (folder != null && folder.IsDeleted);
        }

        private static string LevelCode(int level)
        {
            switch (level)
            {
                case 1: return "1HC";
                case 2: return "2SC";
                case 3: return "3NC";
                case 4: return "4PC";
                default: return "—";
            }
        }

        // ── DTO ─────────────────────────────────────────────────────────────
        private class SharedRow
        {
            public int      FolderID;       // navigate ไปยัง folder นี้
            public int      FileID;         // > 0 เมื่อเป็นไฟล์
            public int      PermissionID;   // ID สำหรับ revoke / mark read
            public bool     IsFolderPerm;   // true = FolderPermission, false = FilePermission
            public string   ItemType;       // "📁 Folder" หรือ "📄 ไฟล์"
            public bool     IsUnread;
            public bool     IsExpired;
            public bool     IsDeny;
            public bool     IsDeleted;      // ไฟล์/Folder ถูกลบ (ถังขยะหรือลบถาวร)
            public bool     IsUsable { get { return !IsDeny && !IsExpired && !IsDeleted; } }
            public string   DisplayName;    // ชื่อ folder หรือชื่อไฟล์
            public string   FileName;       // แสดงเฉพาะเมื่อเป็นรายการไฟล์
            public string   LevelCode;
            public string   Perms;
            public string   GrantedBy;
            public DateTime GrantedDate;
            public string   Expiry;
            public string   Status;
        }
    }
}
