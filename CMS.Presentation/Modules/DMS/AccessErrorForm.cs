using System;
using System.Drawing;
using System.Windows.Forms;

namespace CMS.Presentation
{
    public enum AccessErrorType
    {
        NoAccount,        // กรณีที่ 1: ยังไม่เคยได้รับสิทธิ์ → ติดต่อ HR
        NoMenuPermission  // กรณีที่ 2: มี Account แต่ไม่มีสิทธิ์ Menu → ติดต่อ IT Helpdesk
    }

    /// <summary>
    /// Dialog แสดงเมื่อ User ไม่มีสิทธิ์เข้าใช้งาน (Case 1 หรือ Case 2)
    /// </summary>
    public sealed class AccessErrorForm : Form
    {
        private readonly string _referenceCode;

        public AccessErrorForm(AccessErrorType errorType, string username)
        {
            _referenceCode = GenerateReference(username);
            BuildUI(errorType, username ?? string.Empty);
        }

        // ──────────────────────────────────────────────────────────────────
        // Reference code — ใช้แจ้ง IT Helpdesk เพื่อ trace ปัญหา
        // ──────────────────────────────────────────────────────────────────
        private static string GenerateReference(string username)
        {
            string raw = string.IsNullOrEmpty(username) ? "USR" :
                         (username.Contains("\\")
                             ? username.Substring(username.LastIndexOf('\\') + 1)
                             : username);
            string u = raw.ToUpperInvariant().PadRight(3).Substring(0, 3);
            return string.Format("DMS-{0}-{1}-{2:D4}",
                DateTime.Now.ToString("yyyyMMdd-HHmm"), u,
                new Random().Next(1000, 9999));
        }

        private void BuildUI(AccessErrorType errorType, string username)
        {
            bool isCase1 = errorType == AccessErrorType.NoAccount;

            Color headerColor = isCase1 ? Color.FromArgb(169, 50, 38)   // เข้ม-แดง (HR)
                                        : Color.FromArgb(175, 96, 26);   // เข้ม-ส้ม (IT)
            Color accentColor = isCase1 ? Color.FromArgb(192, 57, 43)
                                        : Color.FromArgb(211, 84, 0);

            string iconText   = isCase1 ? "🚫" : "⚠️";
            string titleText  = isCase1 ? "ไม่มีสิทธิ์เข้าใช้งานระบบ"
                                        : "ไม่มีสิทธิ์ Menu ในระบบ";
            string caseLabel  = isCase1 ? "กรณีที่ 1  |  ยังไม่เคยได้รับสิทธิ์"
                                        : "กรณีที่ 2  |  มี Account แต่ไม่มีสิทธิ์ Menu";
            string descText   = isCase1
                ? string.Format("ผู้ใช้งาน  \"{0}\"  ยังไม่ได้รับสิทธิ์เข้าใช้งานระบบนี้", username)
                : string.Format("ผู้ใช้งาน  \"{0}\"  สามารถ Login ได้\nแต่ยังไม่มีสิทธิ์ Menu ที่กำหนดในระบบ", username);
            string actionText = isCase1
                ? "กรุณาติดต่อ  แผนก HR\nเพื่อขอสิทธิ์เข้าใช้งานระบบ DMS Insurance"
                : "กรุณาติดต่อผู้ดูแลระบบ  หรือแจ้งผ่าน  IT Helpdesk\nเพื่อขอเพิ่มสิทธิ์ Menu การใช้งาน";

            // ── Form properties ────────────────────────────────────────────
            Text            = "DMS Insurance — ไม่สามารถเข้าใช้งานได้";
            ClientSize      = new Size(488, 342);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterScreen;
            BackColor       = Color.White;
            Font            = new Font("Segoe UI", 9.5f);

            // ── Header ─────────────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(488, 78),
                BackColor = headerColor
            };
            pnlHeader.Controls.Add(new Label
            {
                Text      = iconText,
                Font      = new Font("Segoe UI", 22f),
                ForeColor = Color.White,
                Location  = new Point(12, 10),
                AutoSize  = true
            });
            pnlHeader.Controls.Add(new Label
            {
                Text      = titleText,
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                Location  = new Point(60, 8),
                Size      = new Size(420, 36),
                AutoSize  = false,
                Padding   = new Padding(2, 0, 0, 0)
            });
            pnlHeader.Controls.Add(new Label
            {
                Text      = caseLabel,
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(255, 215, 185),
                Location  = new Point(62, 46),
                Size      = new Size(418, 22),
                AutoSize  = false,
                Padding   = new Padding(2, 0, 0, 0)
            });

            // ── Status ─────────────────────────────────────────────────────
            var lblDescHead = MkLabel("สถานะ:", new Point(22, 90),
                new Font("Segoe UI", 9f, FontStyle.Bold), Color.FromArgb(90, 90, 90));
            var lblDesc = new Label
            {
                Text      = descText,
                Font      = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(40, 40, 40),
                Location  = new Point(22, 108),
                Size      = new Size(446, 42),
                AutoSize  = false
            };

            var div1 = MkDivider(new Point(20, 156));

            // ── Action ─────────────────────────────────────────────────────
            var lblActionHead = MkLabel("วิธีดำเนินการ:", new Point(22, 166),
                new Font("Segoe UI", 9f, FontStyle.Bold), Color.FromArgb(90, 90, 90));
            var lblAction = new Label
            {
                Text      = actionText,
                Font      = new Font("Segoe UI", 11f, FontStyle.Bold),
                ForeColor = accentColor,
                Location  = new Point(22, 184),
                Size      = new Size(446, 48),
                AutoSize  = false
            };

            var div2 = MkDivider(new Point(20, 238));

            // ── Reference ──────────────────────────────────────────────────
            var lblRefHead = MkLabel("Reference No. (สำหรับแจ้ง IT Helpdesk):",
                new Point(22, 248), new Font("Segoe UI", 8.5f), Color.Gray);

            var txtRef = new TextBox
            {
                Text        = _referenceCode,
                ReadOnly    = true,
                Location    = new Point(22, 268),
                Size        = new Size(314, 24),
                BackColor   = Color.FromArgb(248, 248, 248),
                Font        = new Font("Consolas", 9.5f),
                BorderStyle = BorderStyle.FixedSingle
            };

            var btnCopy = MkButton("📋 คัดลอก", new Point(344, 267), new Size(120, 26),
                Color.FromArgb(52, 73, 94));
            btnCopy.Click += (s, e) =>
            {
                Clipboard.SetText(_referenceCode);
                btnCopy.Text = "✓ คัดลอกแล้ว";
                var t = new System.Windows.Forms.Timer { Interval = 1500 };
                t.Tick += (_, __) => { btnCopy.Text = "📋 คัดลอก"; t.Stop(); t.Dispose(); };
                t.Start();
            };

            // ── Close ──────────────────────────────────────────────────────
            var btnClose = MkButton("ปิด", new Point(382, 303), new Size(88, 32), headerColor);
            btnClose.Font         = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnClose.DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[]
            {
                pnlHeader,
                lblDescHead, lblDesc,
                div1, lblActionHead, lblAction,
                div2, lblRefHead, txtRef, btnCopy,
                btnClose
            });
            AcceptButton = btnClose;
            CancelButton = btnClose;
        }

        // ── Control factories ──────────────────────────────────────────────

        private static Label MkLabel(string text, Point loc, Font font, Color color)
            => new Label { Text = text, Font = font, ForeColor = color, Location = loc, AutoSize = true };

        private static Panel MkDivider(Point loc)
            => new Panel { BackColor = Color.FromArgb(215, 215, 215), Location = loc, Size = new Size(448, 1) };

        private static Button MkButton(string text, Point loc, Size size, Color back)
        {
            var btn = new Button
            {
                Text      = text,
                Location  = loc,
                Size      = size,
                FlatStyle = FlatStyle.Flat,
                BackColor = back,
                ForeColor = Color.White,
                Cursor    = Cursors.Hand
            };
            btn.FlatAppearance.BorderSize = 0;
            return btn;
        }
    }
}
