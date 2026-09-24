using System;
using System.Drawing;
using System.Windows.Forms;

namespace CMS.Presentation
{
    public enum LoginErrorType
    {
        EmptyCredentials,     // ไม่กรอกข้อมูลให้ครบ
        InvalidCredentials,   // Username หรือ Password ไม่ถูกต้อง
        InvalidUsernameChars  // ชื่อผู้ใช้งานมีอักขระที่ไม่อนุญาต (เช่น ภาษาไทย)
    }

    /// <summary>
    /// Dialog แสดงเมื่อ Login ล้มเหลวจากข้อมูลที่กรอก (กรอกไม่ครบ / กรอกผิด)
    /// </summary>
    public sealed class LoginErrorForm : Form
    {
        public LoginErrorForm(LoginErrorType errorType)
        {
            BuildUI(errorType);
        }

        private void BuildUI(LoginErrorType errorType)
        {
            bool isEmpty = errorType == LoginErrorType.EmptyCredentials;

            Color headerColor = isEmpty
                ? Color.FromArgb(197, 160, 89)   // Premium Gold — กรอกไม่ครบ
                : Color.FromArgb(18, 38, 76);    // Deep Navy — ผิดพลาด

            Color accentColor = isEmpty
                ? Color.FromArgb(170, 130, 60)
                : Color.FromArgb(25, 50, 95);

            string iconText   = isEmpty ? "📝" : "🔒";
            string titleText  = isEmpty ? "กรุณากรอกข้อมูลให้ครบ" : "เข้าสู่ระบบไม่สำเร็จ";
            string caseLabel  = isEmpty
                ? "ข้อมูลไม่ครบถ้วน  |  กรุณากรอก Username และ Password"
                : "ข้อมูลไม่ถูกต้อง  |  Username หรือ Password ผิดพลาด";
            string descText   = isEmpty
                ? "กรุณากรอก Username และ Password\nก่อนเข้าสู่ระบบ"
                : "Username หรือ Password ที่กรอกไม่ถูกต้อง\nกรุณาตรวจสอบและลองใหม่อีกครั้ง";
            string actionText = isEmpty
                ? "กรอกข้อมูลให้ครบถ้วนทั้ง Username และ Password\nแล้วกดปุ่ม Login อีกครั้ง"
                : "ตรวจสอบ Username และ Password ให้ถูกต้อง\nหากลืม Password กรุณาติดต่อ IT Helpdesk";

            if (errorType == LoginErrorType.InvalidUsernameChars)
            {
                iconText   = "⌨️";
                titleText  = "ชื่อผู้ใช้งานไม่ถูกต้อง";
                caseLabel  = "รูปแบบไม่ถูกต้อง  |  ชื่อผู้ใช้งานมีอักขระที่ไม่อนุญาต";
                descText   = "ชื่อผู้ใช้งาน (AD Username) ต้องเป็นภาษาอังกฤษเท่านั้น\nไม่รองรับภาษาไทยหรืออักขระที่ไม่ใช่ ASCII";
                actionText = "กรอกชื่อผู้ใช้งานเป็นตัวอักษรภาษาอังกฤษ ตัวเลข\nหรือ . _ - เท่านั้น แล้วลองใหม่อีกครั้ง";
            }

            // ── Form ───────────────────────────────────────────────────────
            Text            = "DMS Insurance — เข้าสู่ระบบ";
            ClientSize      = new Size(440, 278);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterParent;
            BackColor       = Color.White;
            Font            = new Font("Segoe UI", 9.5f);

            // ── Header ─────────────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(440, 72),
                BackColor = headerColor
            };
            pnlHeader.Controls.Add(new Label
            {
                Text      = iconText,
                Font      = new Font("Segoe UI", 22f),
                ForeColor = Color.White,
                Location  = new Point(12, 8),
                AutoSize  = true
            });
            pnlHeader.Controls.Add(new Label
            {
                Text      = titleText,
                Font      = new Font("Segoe UI", 13f, FontStyle.Bold),
                ForeColor = Color.White,
                Location  = new Point(56, 8),
                Size      = new Size(376, 34),
                AutoSize  = false,
                Padding   = new Padding(2, 0, 0, 0)
            });
            pnlHeader.Controls.Add(new Label
            {
                Text      = caseLabel,
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(255, 235, 180),
                Location  = new Point(58, 44),
                Size      = new Size(374, 22),
                AutoSize  = false,
                Padding   = new Padding(2, 0, 0, 0)
            });

            // ── Status ─────────────────────────────────────────────────────
            int y = 86;
            Controls.Add(MkLabel("สถานะ:", new Point(22, y),
                new Font("Segoe UI", 9f, FontStyle.Bold), Color.FromArgb(90, 90, 90)));
            y += 19;
            Controls.Add(new Label
            {
                Text      = descText,
                Font      = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(40, 40, 40),
                Location  = new Point(22, y),
                Size      = new Size(396, 42),
                AutoSize  = false
            });
            y += 48;

            // ── Divider ────────────────────────────────────────────────────
            Controls.Add(new Panel
            {
                BackColor = Color.FromArgb(215, 215, 215),
                Location  = new Point(20, y),
                Size      = new Size(400, 1)
            });
            y += 10;

            // ── Action ─────────────────────────────────────────────────────
            Controls.Add(MkLabel("วิธีดำเนินการ:", new Point(22, y),
                new Font("Segoe UI", 9f, FontStyle.Bold), Color.FromArgb(90, 90, 90)));
            y += 19;
            Controls.Add(new Label
            {
                Text      = actionText,
                Font      = new Font("Segoe UI", 10.5f, FontStyle.Bold),
                ForeColor = accentColor,
                Location  = new Point(22, y),
                Size      = new Size(396, 46),
                AutoSize  = false
            });
            y += 56;

            // ── Close button ───────────────────────────────────────────────
            var btnClose = MkButton("รับทราบ", new Point(330, y), new Size(90, 32), headerColor);
            btnClose.Font         = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnClose.DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] { pnlHeader, btnClose });
            AcceptButton = btnClose;
            CancelButton = btnClose;
        }

        private static Label MkLabel(string text, Point loc, Font font, Color color)
            => new Label { Text = text, Font = font, ForeColor = color, Location = loc, AutoSize = true };

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
