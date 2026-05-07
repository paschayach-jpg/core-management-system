using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.ImportFile
{
    /// <summary>
    /// แสดงสรุปสิทธิ์การเข้าใช้งานของ user ปัจจุบัน
    ///   เงื่อนไขที่ 1 — FolderAuthorityConfig : สิทธิ์ตามตำแหน่ง / แผนก / ฝ่าย / LevelCode
    ///   เงื่อนไขที่ 2 — FolderPermission (ACL) : สิทธิ์เฉพาะ folder ที่ได้รับโดยตรง
    /// </summary>
    public class UserPermissionSummaryForm : Form
    {
        private readonly UserContext               _user;
        private readonly IShareLookupController   _shareLookupController;
        private DataGridView _aclGrid;
        private DataGridView _shareGrid;

        private sealed class LevelDefinition
        {
            public int Level { get; private set; }
            public string Code { get; private set; }
            public string Label { get; private set; }
            public Color Accent { get; private set; }

            public LevelDefinition(int level, string code, string label, Color accent)
            {
                Level = level;
                Code = code;
                Label = label;
                Accent = accent;
            }
        }

        // ── Level definitions (ต้องตรงกับ 4 root folders) ─────────────────
        private static readonly LevelDefinition[] Levels =
        {
            new LevelDefinition(1, "1HC", "HC\nสูงสุด",   Color.FromArgb(180, 30,  30)),
            new LevelDefinition(2, "2SC", "SC\nลับมาก",   Color.FromArgb(200, 100,  0)),
            new LevelDefinition(3, "3NC", "NC\nปกติ",     Color.FromArgb(0,   90, 160)),
            new LevelDefinition(4, "4PC", "PC\nทั่วไป",   Color.FromArgb(0,  130,  60)),
        };

        public UserPermissionSummaryForm(
            UserContext                user,
            IShareLookupController    shareLookupController = null)
        {
            _user         = user       ?? throw new ArgumentNullException("user");
            _shareLookupController = shareLookupController;
            BuildUI();
        }

        // ════════════════════════════════════════════════════════════════════
        // UI Construction
        // ════════════════════════════════════════════════════════════════════

        private void BuildUI()
        {
            Text            = "สิทธิ์การเข้าใช้งานของฉัน";
            Size            = new Size(960, 780);
            MinimumSize     = new Size(780, 640);
            StartPosition   = FormStartPosition.CenterParent;
            Font            = new Font("Segoe UI", 9f);
            BackColor       = Color.FromArgb(243, 246, 251);
            FormBorderStyle = FormBorderStyle.Sizable;
            MaximizeBox     = false;
            Icon            = SystemIcons.Shield;

            var pnlHeader = BuildHeader();
            var pnlBody   = BuildBody(out var grid, out var pnlCards, out var shareGrid);
            _aclGrid      = grid;
            _shareGrid    = shareGrid;
            var pnlFooter = BuildFooter();

            Controls.Add(pnlBody);
            Controls.Add(pnlHeader);
            Controls.Add(pnlFooter);

            // Resize: ขยาย grid และ cards ตาม form
            Resize += (s, e) =>
            {
                if (pnlCards != null)
                    DistributeCards(pnlCards, pnlBody.ClientSize.Width - 2);
            };
        }

        // ── Header ──────────────────────────────────────────────────────────
        private Panel BuildHeader()
        {
            var pnl = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 76,
                BackColor = Color.FromArgb(25, 55, 105),
                Padding   = new Padding(14, 0, 14, 0),
            };

            // Avatar circle
            var avatar = new Panel
            {
                Size      = new Size(48, 48),
                Location  = new Point(14, 14),
                BackColor = Color.FromArgb(60, 100, 160),
            };
            avatar.Paint += (s, e) =>
            {
                var g = e.Graphics;
                g.SmoothingMode = System.Drawing.Drawing2D.SmoothingMode.AntiAlias;
                g.FillEllipse(new SolidBrush(Color.FromArgb(80, 130, 200)), 0, 0, 47, 47);

                string initials = GetInitials(_user.ADUserName);
                using (var f = new Font("Segoe UI", 16f, FontStyle.Bold))
                using (var sf = new StringFormat { Alignment = StringAlignment.Center, LineAlignment = StringAlignment.Center })
                    g.DrawString(initials, f, Brushes.White, new RectangleF(0, 0, 48, 48), sf);
            };

            string displayName = _user.ADUserName ?? "ผู้ใช้";
            var lblName = new Label
            {
                Text      = displayName,
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                AutoSize  = true,
                Location  = new Point(72, 10),
            };

            var lblSub = new Label
            {
                Text      = BuildPositionLine(),
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(170, 200, 240),
                AutoSize  = true,
                Location  = new Point(74, 38),
            };

            var lblRole = new Label
            {
                Text      = "AppRole: " + RoleLabel(_user.AppRole),
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(140, 180, 230),
                AutoSize  = true,
                Location  = new Point(74, 57),
            };

            pnl.Controls.AddRange(new Control[] { avatar, lblName, lblSub, lblRole });
            return pnl;
        }

        // ── Body ────────────────────────────────────────────────────────────
        private Panel BuildBody(out DataGridView gridOut, out FlowLayoutPanel cardsOut, out DataGridView shareGridOut)
        {
            // ── Layout constants ─────────────────────────────────────────────
            const int SEC1_Y    = 8;
            const int CARDS_Y   = 40;
            const int CARDS_H   = 170;
            const int SEC2_Y    = CARDS_Y + CARDS_H + 12;   // 222
            const int GRID2_Y   = SEC2_Y + 26 + 6;          // 254
            const int GRID2_H   = 140;                       // fixed
            const int SEC3_Y    = GRID2_Y + GRID2_H + 8;    // 402
            const int GRID3_Y   = SEC3_Y + 26 + 6;          // 434

            var pnl = new Panel
            {
                Dock    = DockStyle.Fill,
                Padding = new Padding(1, 8, 1, 4),
            };

            // ── Section 1: FolderAuthorityConfig ────────────────────────────
            var sec1 = MakeSectionBar("เงื่อนไขที่ 1  —  สิทธิ์ตาม FolderAuthorityConfig  (ตำแหน่ง / แผนก / ฝ่าย / LevelCode)");
            sec1.Location = new Point(1, SEC1_Y);

            var pnlCards = new FlowLayoutPanel
            {
                Location      = new Point(1, CARDS_Y),
                Height        = CARDS_H,
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = false,
                AutoScroll    = false,
                BackColor     = Color.Transparent,
            };
            foreach (var lv in Levels)
                pnlCards.Controls.Add(BuildLevelCard(lv.Level, lv.Code, lv.Label, lv.Accent));

            // ── Section 2: FolderPermission ACL ─────────────────────────────
            var sec2 = MakeSectionBar("เงื่อนไขที่ 2  —  สิทธิ์เฉพาะ Folder  (FolderPermission / ACL)");
            sec2.Location = new Point(1, SEC2_Y);

            var grid2 = BuildAclGrid();
            grid2.Location = new Point(1, GRID2_Y);
            grid2.Height   = GRID2_H;

            // ── Section 3: Share (FilePermission) ────────────────────────────
            var sec3 = MakeSectionBar("เงื่อนไขที่ 3  —  สิทธิ์จากการแบ่งปัน  (File / Folder Share)");
            sec3.Location = new Point(1, SEC3_Y);
            sec3.BackColor = Color.FromArgb(200, 230, 215);    // สีเขียวอ่อนแยกจาก sec1/sec2

            var grid3 = BuildShareGrid();
            grid3.Location = new Point(1, GRID3_Y);

            pnl.Controls.AddRange(new Control[] { sec1, pnlCards, sec2, grid2, sec3, grid3 });
            gridOut      = grid2;
            cardsOut     = pnlCards;
            shareGridOut = grid3;

            // Initial sizing
            pnl.Layout += (s, e) =>
            {
                int w = pnl.ClientSize.Width - 2;
                sec1.Width     = w;
                sec2.Width     = w;
                sec3.Width     = w;
                pnlCards.Width = w;
                grid2.Width    = w;
                grid3.Width    = w;
                grid3.Height   = Math.Max(80, pnl.ClientSize.Height - GRID3_Y - 4);
            };

            return pnl;
        }

        // ── Footer ──────────────────────────────────────────────────────────
        private Panel BuildFooter()
        {
            var pnl = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 44,
                BackColor = Color.FromArgb(228, 234, 245),
                Padding   = new Padding(10, 6, 10, 6),
            };

            var lblHint = new Label
            {
                Text      = "เงื่อนไขที่ 1 / 2 / 3  ผ่านข้อใดข้อหนึ่ง = มีสิทธิ์เข้าถึง  |  Admin / Manager ผ่านทุก folder  |  เงื่อนไขที่ 3 = ไฟล์ที่ถูก share ให้",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(90, 100, 130),
                AutoSize  = true,
                Location  = new Point(10, 12),
            };

            var btnRefresh = new Button
            {
                Text      = "↺ รีเฟรช",
                Size      = new Size(82, 28),
                Location  = new Point(pnl.Width - 200, 7),
                Anchor    = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(50, 90, 150),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 8.5f),
            };
            btnRefresh.FlatAppearance.BorderSize = 0;
            btnRefresh.Click += (s, e) =>
            {
                if (_aclGrid != null)   PopulateAclGrid(_aclGrid);
                if (_shareGrid != null) PopulateShareGrid(_shareGrid);
            };

            var btnClose = new Button
            {
                Text      = "ปิด",
                Size      = new Size(82, 28),
                Location  = new Point(pnl.Width - 104, 7),
                Anchor    = AnchorStyles.Right | AnchorStyles.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(25, 55, 105),
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
            };
            btnClose.FlatAppearance.BorderSize = 0;
            btnClose.Click += (s, e) => Close();

            pnl.Controls.AddRange(new Control[] { lblHint, btnRefresh, btnClose });
            return pnl;
        }

        // ════════════════════════════════════════════════════════════════════
        // Level Card (เงื่อนไขที่ 1)
        // ════════════════════════════════════════════════════════════════════

        private Panel BuildLevelCard(int level, string code, string label, Color accent)
        {
            FolderAuthorityConfig cfg = null;
            _user.Authority?.TryGetValue(level, out cfg);

            bool canView    = cfg != null && cfg.CanView;
            bool canAdd     = cfg != null && cfg.CanAdd;
            bool canEdit    = cfg != null && cfg.CanEdit;
            bool canApprove = cfg != null && cfg.CanApprove;
            bool anyPerm    = canView || canAdd || canEdit || canApprove;

            Color barColor    = anyPerm ? accent : Color.FromArgb(200, 200, 200);
            Color textAccent  = anyPerm ? accent : Color.FromArgb(160, 160, 160);
            Color cardBg      = anyPerm ? Color.White : Color.FromArgb(250, 250, 250);

            var card = new Panel
            {
                Margin    = new Padding(0, 0, 8, 0),
                Height    = 162,
                BackColor = cardBg,
            };

            // Top color stripe
            var stripe = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 5,
                BackColor = barColor,
            };

            // Level code (large)
            var lblCode = new Label
            {
                Text      = code,
                Font      = new Font("Segoe UI", 18f, FontStyle.Bold),
                ForeColor = textAccent,
                AutoSize  = false,
                Size      = new Size(70, 36),
                Location  = new Point(10, 10),
                TextAlign = ContentAlignment.MiddleLeft,
            };

            // Status pill
            string statusText  = anyPerm ? "มีสิทธิ์" : "ไม่มีสิทธิ์";
            Color  statusBg    = anyPerm ? Color.FromArgb(220, 250, 230) : Color.FromArgb(250, 220, 220);
            Color  statusFg    = anyPerm ? Color.FromArgb(0, 120, 50)    : Color.FromArgb(160, 0, 0);
            var lblStatus = new Label
            {
                Text      = statusText,
                Font      = new Font("Segoe UI", 7.5f, FontStyle.Bold),
                ForeColor = statusFg,
                BackColor = statusBg,
                AutoSize  = false,
                Size      = new Size(62, 18),
                Location  = new Point(10, 48),
                TextAlign = ContentAlignment.MiddleCenter,
            };

            // Sub-label (full name)
            var lblSub = new Label
            {
                Text      = label.Replace("\n", "\n"),
                Font      = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(100, 100, 110),
                AutoSize  = false,
                Size      = new Size(82, 30),
                Location  = new Point(10, 70),
                TextAlign = ContentAlignment.TopLeft,
            };

            // Permission matrix (2×2 grid) — ใช้ FlowLayoutPanel ป้องกันล้นขอบ
            var perms = new[]
            {
                new KeyValuePair<string, bool>("ดู",      canView),
                new KeyValuePair<string, bool>("เพิ่ม",   canAdd),
                new KeyValuePair<string, bool>("แก้ไข",   canEdit),
                new KeyValuePair<string, bool>("อนุมัติ", canApprove),
            };

            var pnlBadges = new FlowLayoutPanel
            {
                Location      = new Point(8, 104),
                Size          = new Size(card.Width - 16, 50),
                FlowDirection = FlowDirection.LeftToRight,
                WrapContents  = true,
                BackColor     = Color.Transparent,
                Anchor        = AnchorStyles.Left | AnchorStyles.Right | AnchorStyles.Bottom,
            };

            foreach (var perm in perms)
            {
                var pLabel = perm.Key;
                var allowed = perm.Value;
                var badge = new Label
                {
                    Text      = (allowed ? "✓ " : "✗ ") + pLabel,
                    Font      = new Font("Segoe UI", 7.5f, allowed ? FontStyle.Bold : FontStyle.Regular),
                    ForeColor = allowed ? Color.FromArgb(0, 120, 50) : Color.FromArgb(170, 170, 170),
                    BackColor = allowed ? Color.FromArgb(230, 252, 238) : Color.FromArgb(245, 245, 245),
                    AutoSize  = false,
                    Size      = new Size(76, 20),
                    Margin    = new Padding(0, 0, 4, 4),
                    TextAlign = ContentAlignment.MiddleLeft,
                    Padding   = new Padding(4, 0, 0, 0),
                };
                pnlBadges.Controls.Add(badge);
            }

            // อัพเดตความกว้าง badge เมื่อ card ขยาย/หดตาม DistributeCards
            card.Resize += (s, e) =>
            {
                int availW  = card.Width - 16;
                int badgeW  = Math.Max((availW - 4) / 2, 70); // 2 คอลัมน์ กันเกิน
                pnlBadges.Width = availW;
                foreach (Control b in pnlBadges.Controls)
                    b.Width = badgeW;
            };

            // Border paint
            card.Paint += (s, e) =>
            {
                using (var pen = new Pen(anyPerm ? barColor : Color.FromArgb(215, 215, 215), 1.5f))
                    e.Graphics.DrawRectangle(pen, 0, 0, card.Width - 1, card.Height - 1);
            };

            card.Controls.AddRange(new Control[] { stripe, lblCode, lblStatus, lblSub, pnlBadges });
            return card;
        }

        private void DistributeCards(FlowLayoutPanel pnlCards, int totalWidth)
        {
            int count = pnlCards.Controls.Count;
            if (count == 0) return;
            int gap     = 8;
            int cardW   = (totalWidth - gap * (count - 1)) / count;
            pnlCards.SuspendLayout();
            foreach (Control c in pnlCards.Controls)
                c.Width = Math.Max(cardW, 140);
            pnlCards.ResumeLayout();
        }

        // ════════════════════════════════════════════════════════════════════
        // ACL Grid (เงื่อนไขที่ 2)
        // ════════════════════════════════════════════════════════════════════

        private DataGridView BuildAclGrid()
        {
            var grid = new DataGridView
            {
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible     = false,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                GridColor             = Color.FromArgb(228, 232, 240),
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeRowsMode      = DataGridViewAutoSizeRowsMode.AllCells,
                ColumnHeadersHeight   = 30,
                EnableHeadersVisualStyles = false,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font    = new Font("Segoe UI", 9f),
                    Padding = new Padding(4, 2, 4, 2),
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    BackColor = Color.FromArgb(25, 55, 105),
                    ForeColor = Color.White,
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding   = new Padding(6, 0, 0, 0),
                },
            };

            grid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "colFolder",   HeaderText = "ชื่อ Folder",     AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,    MinimumWidth = 180 },
                new DataGridViewTextBoxColumn { Name = "colLevel",    HeaderText = "ระดับ",            Width = 68 },
                new DataGridViewTextBoxColumn { Name = "colType",     HeaderText = "ประเภท",           Width = 100 },
                new DataGridViewTextBoxColumn { Name = "colRead",     HeaderText = "ดู",              Width = 44 },
                new DataGridViewTextBoxColumn { Name = "colWrite",    HeaderText = "เพิ่ม",           Width = 52 },
                new DataGridViewTextBoxColumn { Name = "colDelete",   HeaderText = "ลบ",              Width = 44 },
                new DataGridViewTextBoxColumn { Name = "colModify",   HeaderText = "แก้ไข",           Width = 60 },
                new DataGridViewTextBoxColumn { Name = "colExpiry",   HeaderText = "หมดอายุ",         Width = 90 },
                new DataGridViewTextBoxColumn { Name = "colStatus",   HeaderText = "สถานะ",           Width = 90 },
            });

            // Center check columns
            foreach (var col in new[] { "colRead", "colWrite", "colDelete", "colModify" })
                grid.Columns[col].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            PopulateAclGrid(grid);

            // Alternate row color
            grid.RowPrePaint += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var row = grid.Rows[e.RowIndex];
                // skip rows with explicit color (deny/expired)
                if (row.DefaultCellStyle.BackColor != Color.Empty &&
                    row.DefaultCellStyle.BackColor != Color.White &&
                    row.DefaultCellStyle.BackColor != Color.FromArgb(249, 251, 255))
                    return;
                row.DefaultCellStyle.BackColor = e.RowIndex % 2 == 0
                    ? Color.White
                    : Color.FromArgb(249, 251, 255);
            };

            return grid;
        }

        private void PopulateAclGrid(DataGridView grid)
        {
            grid.Rows.Clear();

            if (_shareLookupController == null)
            {
                AddInfoRow(grid, "(ไม่สามารถโหลด ACL — Share lookup controller ไม่พร้อมใช้งาน)");
                return;
            }

            // ── Direct user ACL ────────────────────────────────────────────
            var perms = _shareLookupController.GetFolderPermissionsForUser(_user.UserID)
                .OrderBy(p => p.FolderID)
                .ToList();

            if (perms.Count == 0)
            {
                AddInfoRow(grid, "ไม่มีสิทธิ์เฉพาะ Folder ที่ grant ให้ user โดยตรง  (ใช้สิทธิ์จากเงื่อนไขที่ 1 เท่านั้น)");
                return;
            }

            foreach (var p in perms)
            {
                string folderName = "(ไม่พบ folder)";
                string levelCode  = "—";
                var folder = _shareLookupController.GetFolderById(p.FolderID);
                if (folder != null)
                {
                    folderName = folder.FolderName;
                    levelCode  = LevelCodeFrom(folder.ConfidentialityLevel);
                }

                bool isExpired = p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today;
                bool isDeny    = p.IsDeny;
                string expiry  = p.ExpiryDate.HasValue
                    ? p.ExpiryDate.Value.ToString("dd/MM/yyyy")
                    : "ไม่มีกำหนด";
                string status  = isDeny    ? "Deny"
                               : isExpired ? "⚠ หมดอายุ"
                               : "✔ ใช้งาน";

                int idx = grid.Rows.Add(
                    isDeny ? "⊘ " + folderName : folderName,
                    levelCode,
                    isDeny ? "ห้ามเข้า (Deny)" : "อนุญาต (Allow)",
                    p.CanRead    ? "✓" : "—",
                    p.CanWrite   ? "✓" : "—",
                    p.CanDelete  ? "✓" : "—",
                    p.CanModify  ? "✓" : "—",
                    isExpired ? "⚠ " + expiry : expiry,
                    status
                );

                var row = grid.Rows[idx];
                if (isDeny)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 235);
                    row.DefaultCellStyle.ForeColor = Color.Crimson;
                }
                else if (isExpired)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 215);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(150, 80, 0);
                    row.DefaultCellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Strikeout);
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Share Grid (เงื่อนไขที่ 3)
        // ════════════════════════════════════════════════════════════════════

        private DataGridView BuildShareGrid()
        {
            var grid = new DataGridView
            {
                ReadOnly              = true,
                AllowUserToAddRows    = false,
                AllowUserToDeleteRows = false,
                RowHeadersVisible     = false,
                BackgroundColor       = Color.White,
                BorderStyle           = BorderStyle.None,
                GridColor             = Color.FromArgb(228, 232, 240),
                SelectionMode         = DataGridViewSelectionMode.FullRowSelect,
                AutoSizeRowsMode      = DataGridViewAutoSizeRowsMode.AllCells,
                ColumnHeadersHeight   = 30,
                EnableHeadersVisualStyles = false,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Font    = new Font("Segoe UI", 9f),
                    Padding = new Padding(4, 2, 4, 2),
                },
                ColumnHeadersDefaultCellStyle = new DataGridViewCellStyle
                {
                    Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                    BackColor = Color.FromArgb(0, 110, 70),   // เขียวเข้ม
                    ForeColor = Color.White,
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding   = new Padding(6, 0, 0, 0),
                },
            };

            grid.Columns.AddRange(new DataGridViewColumn[]
            {
                new DataGridViewTextBoxColumn { Name = "colItem",    HeaderText = "รายการที่ได้รับ",  AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill, MinimumWidth = 180 },
                new DataGridViewTextBoxColumn { Name = "colKind",    HeaderText = "ประเภท",           Width = 72 },
                new DataGridViewTextBoxColumn { Name = "colLevel",   HeaderText = "ระดับ",            Width = 60 },
                new DataGridViewTextBoxColumn { Name = "colRead",    HeaderText = "ดู",               Width = 44 },
                new DataGridViewTextBoxColumn { Name = "colWrite",   HeaderText = "เพิ่ม",            Width = 52 },
                new DataGridViewTextBoxColumn { Name = "colDelete",  HeaderText = "ลบ",               Width = 44 },
                new DataGridViewTextBoxColumn { Name = "colModify",  HeaderText = "แก้ไข",            Width = 60 },
                new DataGridViewTextBoxColumn { Name = "colBy",      HeaderText = "แบ่งปันโดย",      Width = 120 },
                new DataGridViewTextBoxColumn { Name = "colExpiry",  HeaderText = "หมดอายุ",          Width = 90 },
                new DataGridViewTextBoxColumn { Name = "colStatus",  HeaderText = "สถานะ",            Width = 88 },
            });

            foreach (var col in new[] { "colRead", "colWrite", "colDelete", "colModify" })
                grid.Columns[col].DefaultCellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;

            PopulateShareGrid(grid);

            grid.RowPrePaint += (s, e) =>
            {
                if (e.RowIndex < 0) return;
                var row = grid.Rows[e.RowIndex];
                if (row.DefaultCellStyle.BackColor != Color.Empty &&
                    row.DefaultCellStyle.BackColor != Color.White &&
                    row.DefaultCellStyle.BackColor != Color.FromArgb(249, 251, 255))
                    return;
                row.DefaultCellStyle.BackColor = e.RowIndex % 2 == 0
                    ? Color.White
                    : Color.FromArgb(249, 251, 255);
            };

            return grid;
        }

        private void PopulateShareGrid(DataGridView grid)
        {
            grid.Rows.Clear();

            if (_shareLookupController == null)
            {
                AddInfoRow(grid, "(ไม่สามารถโหลดข้อมูล Share — Share lookup controller ไม่พร้อมใช้งาน)", 10);
                return;
            }

            var shares = _shareLookupController.GetFilePermissionsForUser(_user.UserID)
                .OrderByDescending(p => p.GrantedDate)
                .ToList();

            if (shares.Count == 0)
            {
                AddInfoRow(grid, "ยังไม่มีรายการที่แบ่งปันให้คุณ  (ไม่มี FilePermission / FolderShare สำหรับ user นี้)", 10);
                return;
            }

            foreach (var p in shares)
            {
                // resolve ชื่อไฟล์ + parent folder level
                string itemName  = string.Format("ไฟล์ #{0}", p.FileID);
                string itemKind  = "📄 ไฟล์";
                string levelCode = "—";

                try
                {
                    var doc = _shareLookupController.GetFileById(p.FileID);
                    if (doc != null)
                    {
                        itemName = doc.OriginalFileName;
                        var folder = _shareLookupController.GetFolderById(doc.FolderID);
                        if (folder != null)
                        {
                            levelCode = LevelCodeFrom(folder.ConfidentialityLevel);
                        }
                    }
                }
                catch { }

                // resolve ชื่อผู้ให้สิทธิ์
                string grantedBy = _shareLookupController.ResolveGranterName(p.GrantedBy);

                bool isExpired = p.ExpiryDate.HasValue && p.ExpiryDate.Value.Date < DateTime.Today;
                bool isDeny    = p.IsDeny;
                string expiry  = p.ExpiryDate.HasValue ? p.ExpiryDate.Value.ToString("dd/MM/yyyy") : "ไม่มีกำหนด";
                string status  = isDeny    ? "🚫 Deny"
                               : isExpired ? "⚠ หมดอายุ"
                               : "✔ ใช้งาน";

                int idx = grid.Rows.Add(
                    isDeny ? "⊘ " + itemName : itemName,
                    itemKind,
                    levelCode,
                    p.CanRead    ? "✓" : "—",
                    p.CanWrite   ? "✓" : "—",
                    p.CanDelete  ? "✓" : "—",
                    p.CanModify  ? "✓" : "—",
                    grantedBy,
                    isExpired ? "⚠ " + expiry : expiry,
                    status
                );

                var row = grid.Rows[idx];
                if (isDeny)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 235);
                    row.DefaultCellStyle.ForeColor = Color.Crimson;
                }
                else if (isExpired)
                {
                    row.DefaultCellStyle.BackColor = Color.FromArgb(255, 248, 215);
                    row.DefaultCellStyle.ForeColor = Color.FromArgb(150, 80, 0);
                    row.DefaultCellStyle.Font      = new Font("Segoe UI", 9f, FontStyle.Strikeout);
                }
                else
                {
                    // ไฮไลต์สีเขียวอ่อนสำหรับ share ที่ใช้งานได้
                    row.DefaultCellStyle.BackColor = Color.FromArgb(235, 252, 242);
                }
            }
        }

        // ════════════════════════════════════════════════════════════════════
        // Helpers
        // ════════════════════════════════════════════════════════════════════

        private static Label MakeSectionBar(string text)
        {
            return new Label
            {
                Text      = "  " + text,
                Font      = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(25, 55, 105),
                BackColor = Color.FromArgb(215, 228, 248),
                AutoSize  = false,
                Height    = 26,
                TextAlign = ContentAlignment.MiddleLeft,
            };
        }

        private static void AddInfoRow(DataGridView grid, string msg, int colCount = 9)
        {
            var vals = new object[colCount];
            vals[0] = msg;
            for (int i = 1; i < colCount; i++) vals[i] = "";
            int idx = grid.Rows.Add(vals);
            grid.Rows[idx].DefaultCellStyle.ForeColor = Color.FromArgb(130, 130, 140);
            grid.Rows[idx].DefaultCellStyle.Font      = new Font("Segoe UI", 8.5f, FontStyle.Italic);
        }

        private string BuildPositionLine()
        {
            var parts = new List<string>();
            if (!string.IsNullOrEmpty(_user.PositionName))   parts.Add(_user.PositionName);
            if (!string.IsNullOrEmpty(_user.DepartmentName)) parts.Add(_user.DepartmentName);
            if (!string.IsNullOrEmpty(_user.LevelCode))      parts.Add("Level: " + _user.LevelCode);
            if (!string.IsNullOrEmpty(_user.DepartmentCode)) parts.Add(_user.DepartmentCode);
            return parts.Count > 0 ? string.Join("   |   ", parts) : "(ไม่พบข้อมูลตำแหน่ง)";
        }

        private static string GetInitials(string name)
        {
            if (string.IsNullOrEmpty(name)) return "?";
            // "domain\username" → take after backslash
            int bs = name.LastIndexOf('\\');
            string uname = bs >= 0 ? name.Substring(bs + 1) : name;
            return uname.Length >= 2
                ? uname.Substring(0, 2).ToUpper()
                : uname.ToUpper();
        }

        private static string RoleLabel(int role)
        {
            switch (role)
            {
                case 1: return "Officer";
                case 2: return "Sr. Officer";
                case 3: return "Manager";
                case 4: return "Admin";
                default: return role.ToString();
            }
        }

        private static string LevelCodeFrom(int level)
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
    }
}
