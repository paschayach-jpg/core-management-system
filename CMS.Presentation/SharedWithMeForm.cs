using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    /// <summary>
    /// Inbox แสดง Folder ที่ผู้อื่น share ให้ user ปัจจุบัน
    /// ดึงข้อมูลจาก FolderPermission (PrincipalType=User) และ
    /// นับรายการใหม่ (GrantedDate ภายใน 7 วัน) เพื่อแสดง badge
    /// </summary>
    public class SharedWithMeForm : Form
    {
        private readonly UserContext              _user;
        private readonly IPermissionRepository    _permRepo;
        private readonly IFolderRepository        _folderRepo;
        private readonly IUserAccountRepository   _userRepo;
        private readonly IFilePermissionRepository _filePermRepo;
        private readonly IFileDocumentRepository  _fileDocRepo;

        private DataGridView _grid;
        private Label        _lblCount;

        // ── สาธารณะ: จำนวนรายการใหม่ (ใช้อัพเดต badge ใน MDI) ───────────
        public int NewItemCount { get; private set; }

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
            IPermissionRepository     permRepo,
            IFolderRepository         folderRepo,
            IUserAccountRepository    userRepo      = null,
            IFilePermissionRepository filePermRepo  = null,
            IFileDocumentRepository   fileDocRepo   = null)
        {
            _user         = user       ?? throw new ArgumentNullException("user");
            _permRepo     = permRepo;
            _folderRepo   = folderRepo;
            _userRepo     = userRepo;
            _filePermRepo = filePermRepo;
            _fileDocRepo  = fileDocRepo;
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

            Text            = "📥  แบ่งปันให้ฉัน  (Shared With Me)";
            ClientSize      = new Size(860, 560);
            MinimumSize     = new Size(700, 440);
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
                Text      = "แบ่งปันให้ฉัน  —  Shared With Me",
                Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(62, 12),
            };
            _lblCount = new Label
            {
                Text      = "กำลังโหลด...",
                Font      = new Font("Segoe UI", 8f),
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
            var cboFilter = new ComboBox
            {
                DropDownStyle = ComboBoxStyle.DropDownList,
                Font          = uiFont,
                Width         = 150,
                Location      = new Point(68, 6),
                FlatStyle     = FlatStyle.Flat,
                BackColor     = Color.White,
            };
            cboFilter.Items.AddRange(new[] { "ทั้งหมด", "ใหม่ (7 วัน)", "ใช้งานได้", "หมดอายุ", "Deny" });
            cboFilter.SelectedIndex = 0;
            cboFilter.SelectedIndexChanged += (s, e) => ApplyFilter(cboFilter.SelectedItem.ToString());
            pnlFilter.Controls.AddRange(new Control[] { lblFilter, cboFilter });

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
                new DataGridViewTextBoxColumn { Name = "colFolder",  HeaderText = "รายการที่แบ่งปัน", MinimumWidth = 180, AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill },
                new DataGridViewTextBoxColumn { Name = "colLevel",   HeaderText = "ระดับ",             Width = 60,  AutoSizeMode = DataGridViewAutoSizeColumnMode.None },
                new DataGridViewTextBoxColumn { Name = "colPerms",   HeaderText = "สิทธิ์ที่ได้รับ",  Width = 120, AutoSizeMode = DataGridViewAutoSizeColumnMode.None },
                new DataGridViewTextBoxColumn { Name = "colGrantBy", HeaderText = "แบ่งปันโดย",      Width = 130, AutoSizeMode = DataGridViewAutoSizeColumnMode.None },
                new DataGridViewTextBoxColumn { Name = "colDate",    HeaderText = "วันที่รับ",        Width = 100, AutoSizeMode = DataGridViewAutoSizeColumnMode.None },
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
                var row = _grid.Rows[e.RowIndex].Tag as SharedRow;
                if (row == null) return;
                if (row.FileID > 0)
                    OpenFileRequested?.Invoke(row.FolderID, row.FileID);
                else if (row.FolderID > 0)
                    OpenFolderRequested?.Invoke(row.FolderID);
            };

            _grid.KeyDown += (s, e) =>
            {
                if (e.KeyCode != Keys.Enter || _grid.CurrentRow == null) return;
                var row = _grid.CurrentRow.Tag as SharedRow;
                if (row == null) return;
                if (row.FileID > 0)
                    OpenFileRequested?.Invoke(row.FolderID, row.FileID);
                else if (row.FolderID > 0)
                    OpenFolderRequested?.Invoke(row.FolderID);
                e.Handled = true;
            };

            // ── Footer ──────────────────────────────────────────────────────
            var pnlFooter = new Panel { Dock = DockStyle.Bottom, Height = 50, BackColor = footerBg };

            var lblHint = new Label
            {
                Text      = "💡  ดับเบิลคลิก Folder = เปิด Folder  |  ดับเบิลคลิก ไฟล์ = เปิดไฟล์  |  🔴 = รายการใหม่ภายใน 7 วัน",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(90, 100, 130),
                AutoSize  = true,
                Location  = new Point(12, 16),
            };

            var btnRefresh = new Button
            {
                Text      = "↺  รีเฟรช",
                Size      = new Size(96, 32),
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
                Size         = new Size(96, 32),
                Anchor       = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle    = FlatStyle.Flat,
                BackColor    = navyDark,
                ForeColor    = Color.White,
                Font         = boldFont,
                DialogResult = DialogResult.Cancel,
            };
            btnClose.FlatAppearance.BorderSize = 0;

            pnlFooter.Resize += (s, e) =>
            {
                btnClose.Location   = new Point(pnlFooter.Width - 108, 9);
                btnRefresh.Location = new Point(pnlFooter.Width - 210, 9);
            };
            btnClose.Location   = new Point(752, 9);
            btnRefresh.Location = new Point(650, 9);
            pnlFooter.Controls.AddRange(new Control[] { lblHint, btnRefresh, btnClose });

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
            NewItemCount = 0;

            if (_permRepo == null)
            {
                AddInfoRow("(ไม่สามารถโหลดข้อมูล — PermissionRepository ไม่พร้อมใช้งาน)");
                _lblCount.Text = "ไม่สามารถโหลดข้อมูลได้";
                return;
            }

            try
            {
                var cutoff = DateTime.Today.AddDays(-7);

                // ── เงื่อนไขที่ 1: FolderPermission (สิทธิ์ระดับ Folder จาก Admin) ──────
                if (_permRepo != null)
                {
                    var folderPerms = _permRepo.GetByPrincipal((int)PrincipalType.User, _user.UserID)
                        .Where(p => p.RevokedDate == null)
                        .OrderByDescending(p => p.GrantedDate)
                        .ToList();

                    foreach (var p in folderPerms)
                    {
                        string folderName = "(ไม่พบ)";
                        string levelCode  = "—";
                        if (_folderRepo != null)
                        {
                            var folder = _folderRepo.GetById(p.FolderID);
                            if (folder != null)
                            {
                                folderName = folder.FolderName;
                                levelCode  = LevelCode(folder.ConfidentialityLevel);
                            }
                        }

                        bool isNew     = p.GrantedDate.Date >= cutoff;
                        bool isExpired = p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today;
                        bool isDeny    = p.IsDeny;

                        _allRows.Add(new SharedRow
                        {
                            FolderID    = p.FolderID,
                            ItemType    = "📁 Folder",
                            IsNew       = isNew,
                            IsExpired   = isExpired,
                            IsDeny      = isDeny,
                            DisplayName = folderName,
                            LevelCode   = levelCode,
                            Perms       = BuildPermText(p),
                            GrantedBy   = ResolveGranterName(p.GrantedBy),
                            GrantedDate = p.GrantedDate,
                            Expiry      = p.ExpiryDate.HasValue ? p.ExpiryDate.Value.ToString("dd/MM/yyyy") : "ไม่มีกำหนด",
                            Status      = isDeny ? "🚫 Deny" : isExpired ? "⚠ หมดอายุ" : "✔ ใช้งาน",
                        });

                        if (isNew && !isDeny && !isExpired) NewItemCount++;
                    }
                }

                // ── เงื่อนไขที่ 2: FilePermission (share ไฟล์ให้โดยตรง จาก Internal Share) ─
                if (_filePermRepo != null)
                {
                    var filePerms = _filePermRepo.GetByPrincipal((int)PrincipalType.User, _user.UserID)
                        .ToList();

                    foreach (var p in filePerms)
                    {
                        // ข้ามถ้าไม่มี fileDocRepo หรือหาไฟล์ไม่เจอ (data ไม่ consistent)
                        if (_fileDocRepo == null) continue;
                        var doc = _fileDocRepo.GetById(p.FileID);
                        if (doc == null) continue;  // FileID ไม่มีใน storage.FileDocument

                        string fileName  = doc.OriginalFileName;
                        int    folderId  = doc.FolderID;
                        string levelCode = "—";

                        if (_folderRepo != null)
                        {
                            var folder = _folderRepo.GetById(folderId);
                            if (folder != null)
                                levelCode = LevelCode(folder.ConfidentialityLevel);
                        }

                        bool isNew     = p.GrantedDate.Date >= cutoff;
                        bool isExpired = p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today;
                        bool isDeny    = p.IsDeny;

                        _allRows.Add(new SharedRow
                        {
                            FolderID    = folderId,
                            FileID      = p.FileID,
                            ItemType    = "📄 ไฟล์",
                            IsNew       = isNew,
                            IsExpired   = isExpired,
                            IsDeny      = isDeny,
                            DisplayName = fileName,
                            LevelCode   = levelCode,
                            Perms       = BuildFilePermText(p),
                            GrantedBy   = ResolveGranterName(p.GrantedBy),
                            GrantedDate = p.GrantedDate,
                            Expiry      = p.ExpiryDate.HasValue ? p.ExpiryDate.Value.ToString("dd/MM/yyyy") : "ไม่มีกำหนด",
                            Status      = isDeny ? "🚫 Deny" : isExpired ? "⚠ หมดอายุ" : "✔ ใช้งาน",
                        });

                        if (isNew && !isDeny && !isExpired) NewItemCount++;
                    }
                }

                // ── เรียงลำดับใหม่: ใหม่ก่อน → ชื่อ ─────────────────────────────────
                _allRows = _allRows.OrderByDescending(r => r.GrantedDate).ToList();

                if (_allRows.Count == 0)
                {
                    AddInfoRow("ยังไม่มีรายการที่แบ่งปันให้คุณ");
                    _lblCount.Text = "ยังไม่มีรายการ";
                    return;
                }

                int activeCount = _allRows.Count(r => !r.IsDeny && !r.IsExpired);
                _lblCount.Text = string.Format(
                    "ทั้งหมด {0} รายการ  |  ✔ ใช้งานได้ {1}  |  🔴 ใหม่ {2} รายการ",
                    _allRows.Count, activeCount, NewItemCount);

                RenderRows(_allRows);
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
                    r.IsNew ? "🔴" : "",
                    r.ItemType,
                    r.DisplayName,
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
                else if (r.IsExpired)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 215);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(150, 80, 0);
                    row.DefaultCellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Strikeout);
                }
                else if (r.IsNew)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(235, 248, 255);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(0, 70, 140);
                }
            }
        }

        private void ApplyFilter(string filter)
        {
            var cutoff = DateTime.Today.AddDays(-7);
            IEnumerable<SharedRow> filtered = _allRows;
            switch (filter)
            {
                case "ใหม่ (7 วัน)": filtered = _allRows.Where(r => r.IsNew);                          break;
                case "ใช้งานได้":    filtered = _allRows.Where(r => !r.IsDeny && !r.IsExpired);        break;
                case "หมดอายุ":      filtered = _allRows.Where(r => r.IsExpired);                       break;
                case "Deny":         filtered = _allRows.Where(r => r.IsDeny);                          break;
            }
            RenderRows(filtered);
        }

        private void Grid_RowPrePaint(object sender, DataGridViewRowPrePaintEventArgs e)
        {
            if (e.RowIndex < 0) return;
            var row = _grid.Rows[e.RowIndex];
            // skip rows with explicit color
            if (row.DefaultCellStyle.BackColor != Color.Empty
             && row.DefaultCellStyle.BackColor != Color.White
             && row.DefaultCellStyle.BackColor != Color.FromArgb(249, 251, 255))
                return;
            row.DefaultCellStyle.BackColor = e.RowIndex % 2 == 0
                ? Color.White
                : Color.FromArgb(249, 251, 255);
        }

        // ════════════════════════════════════════════════════════════════════
        // Helpers
        // ════════════════════════════════════════════════════════════════════

        private void AddInfoRow(string msg)
        {
            int idx = _grid.Rows.Add("", "", msg, "", "", "", "", "", "");
            _grid.Rows[idx].DefaultCellStyle.ForeColor = Color.FromArgb(130, 130, 140);
            _grid.Rows[idx].DefaultCellStyle.Font      = new Font("Segoe UI", 8.5f, FontStyle.Italic);
        }

        private string ResolveGranterName(int userId)
        {
            if (_userRepo == null) return string.Format("UserID: {0}", userId);
            try
            {
                var u = _userRepo.GetById(userId);
                if (u != null) return string.IsNullOrEmpty(u.DisplayName) ? u.ADUserName : u.DisplayName;
            }
            catch { }
            return string.Format("UserID: {0}", userId);
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
            if (p.CanWrite)       parts.Add("✏ เพิ่ม");
            if (p.CanDelete)      parts.Add("🗑 ลบ");
            if (p.CanModify)      parts.Add("📝 แก้ไข");
            if (p.CanFullControl) parts.Add("🔑 ทั้งหมด");
            return parts.Count > 0 ? string.Join("  ", parts) : "(ไม่มีสิทธิ์)";
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
            public string   ItemType;       // "📁 Folder" หรือ "📄 ไฟล์"
            public bool     IsNew;
            public bool     IsExpired;
            public bool     IsDeny;
            public string   DisplayName;    // ชื่อ folder หรือชื่อไฟล์
            public string   LevelCode;
            public string   Perms;
            public string   GrantedBy;
            public DateTime GrantedDate;
            public string   Expiry;
            public string   Status;
        }
    }
}
