using System;
using System.Drawing;
using System.Text;
using System.Windows.Forms;

namespace CMS.Presentation
{
    /// <summary>
    /// กรณีที่ 3: Dialog แสดง Error รายละเอียดพร้อม Stack Trace
    /// ให้ User Screenshot แล้วแจ้ง IT Helpdesk
    /// </summary>
    public sealed class GlobalErrorDialog : Form
    {
        private string _fullReport;

        private GlobalErrorDialog() { }

        // ──────────────────────────────────────────────────────────────────
        // Public entry point — เรียกจาก Application.ThreadException handler
        // ──────────────────────────────────────────────────────────────────
        public static void Show(Exception ex, string username = null)
        {
            if (ex == null) return;
            try
            {
                using (var dlg = new GlobalErrorDialog())
                {
                    dlg.BuildUI(ex, username ?? string.Empty);
                    dlg.ShowDialog();
                }
            }
            catch
            {
                // absolute last resort — ถ้า dialog เองพัง
                MessageBox.Show(ex.Message, "CMS — Error",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ──────────────────────────────────────────────────────────────────
        // Build UI
        // ──────────────────────────────────────────────────────────────────
        private void BuildUI(Exception ex, string username)
        {
            string timestamp = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss");
            string exType    = ex.GetType().Name;
            string message   = ex.Message;
            _fullReport      = BuildReport(ex, username, timestamp);

            // ── Form properties ────────────────────────────────────────────
            Text            = "CMS — เกิดข้อผิดพลาดในระบบ";
            ClientSize      = new Size(580, 495);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            MaximizeBox     = false;
            MinimizeBox     = false;
            StartPosition   = FormStartPosition.CenterScreen;
            BackColor       = Color.White;
            Font            = new Font("Segoe UI", 9.5f);

            Color headerColor = Color.FromArgb(123, 36, 28);

            // ── Header ─────────────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Location  = new Point(0, 0),
                Size      = new Size(580, 72),
                BackColor = headerColor
            };
            pnlHeader.Controls.Add(new Label
            {
                Text      = "❌",
                Font      = new Font("Segoe UI", 20f),
                ForeColor = Color.White,
                Location  = new Point(12, 10),
                AutoSize  = true
            });
            pnlHeader.Controls.Add(new Label
            {
                Text      = "เกิดข้อผิดพลาดในระบบ",
                Font      = new Font("Segoe UI", 14f, FontStyle.Bold),
                ForeColor = Color.White,
                Location  = new Point(56, 8),
                Size      = new Size(516, 36),
                AutoSize  = false,
                Padding   = new Padding(2, 0, 0, 0)
            });
            pnlHeader.Controls.Add(new Label
            {
                Text      = "กรณีที่ 3  |  มี Account มีสิทธิ์ แต่เกิด Error ขณะใช้งาน",
                Font      = new Font("Segoe UI", 8f),
                ForeColor = Color.FromArgb(255, 200, 200),
                Location  = new Point(58, 46),
                Size      = new Size(514, 22),
                AutoSize  = false,
                Padding   = new Padding(2, 0, 0, 0)
            });

            // ── Info bar (user / timestamp) ────────────────────────────────
            var pnlInfo = new Panel
            {
                Location  = new Point(0, 72),
                Size      = new Size(580, 38),
                BackColor = Color.FromArgb(250, 235, 232)
            };
            pnlInfo.Controls.Add(new Label
            {
                Text      = "👤 " + (string.IsNullOrEmpty(username) ? "(ไม่ระบุ)" : username),
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(90, 35, 30),
                Location  = new Point(16, 11),
                AutoSize  = true
            });
            pnlInfo.Controls.Add(new Label
            {
                Text      = "🕐 " + timestamp,
                Font      = new Font("Segoe UI", 9f),
                ForeColor = Color.FromArgb(90, 35, 30),
                Location  = new Point(236, 11),
                AutoSize  = true
            });

            // ── Error type ─────────────────────────────────────────────────
            int y = 120;
            Controls.Add(MkLabel("ประเภท Error:", new Point(20, y),
                new Font("Segoe UI", 8.5f, FontStyle.Bold), Color.FromArgb(100, 100, 100)));
            y += 19;
            Controls.Add(MkLabel(exType, new Point(20, y),
                new Font("Consolas", 9.5f), Color.FromArgb(169, 50, 38)));
            y += 24;

            // ── Error message ──────────────────────────────────────────────
            Controls.Add(MkLabel("ข้อความ Error:", new Point(20, y),
                new Font("Segoe UI", 8.5f, FontStyle.Bold), Color.FromArgb(100, 100, 100)));
            y += 19;
            Controls.Add(new Label
            {
                Text      = message.Length > 200 ? message.Substring(0, 200) + "…" : message,
                Font      = new Font("Segoe UI", 9.5f),
                ForeColor = Color.FromArgb(50, 50, 50),
                Location  = new Point(20, y),
                Size      = new Size(540, 46),
                AutoSize  = false
            });
            y += 52;

            // ── Stack trace ────────────────────────────────────────────────
            Controls.Add(MkLabel("รายละเอียด (Stack Trace):", new Point(20, y),
                new Font("Segoe UI", 8.5f, FontStyle.Bold), Color.FromArgb(100, 100, 100)));
            y += 20;
            var txtStack = new TextBox
            {
                Text        = _fullReport,
                Multiline   = true,
                ReadOnly    = true,
                ScrollBars  = ScrollBars.Vertical,
                Location    = new Point(20, y),
                Size        = new Size(540, 156),
                BackColor   = Color.FromArgb(245, 245, 245),
                Font        = new Font("Consolas", 8f),
                BorderStyle = BorderStyle.FixedSingle,
                WordWrap    = false
            };
            Controls.Add(txtStack);
            y += 163;

            // ── Divider ────────────────────────────────────────────────────
            Controls.Add(new Panel
            {
                BackColor = Color.FromArgb(215, 215, 215),
                Location  = new Point(20, y),
                Size      = new Size(540, 1)
            });
            y += 10;

            // ── Contact info ───────────────────────────────────────────────
            Controls.Add(new Label
            {
                Text      = "กรุณาติดต่อผู้ดูแลระบบ  หรือแจ้งผ่าน  IT Helpdesk  พร้อม Screenshot / Copy Error นี้",
                Font      = new Font("Segoe UI", 9f, FontStyle.Italic),
                ForeColor = Color.FromArgb(140, 60, 60),
                Location  = new Point(20, y),
                Size      = new Size(540, 22),
                AutoSize  = false
            });
            y += 30;

            // ── Buttons ────────────────────────────────────────────────────
            var btnCopy = MkButton("📋 คัดลอก Error", new Point(20, y),
                new Size(148, 32), Color.FromArgb(52, 73, 94));
            btnCopy.Click += (s, e) =>
            {
                Clipboard.SetText(_fullReport);
                btnCopy.Text = "✓ คัดลอกแล้ว";
                var t = new System.Windows.Forms.Timer { Interval = 1500 };
                t.Tick += (_, __) => { btnCopy.Text = "📋 คัดลอก Error"; t.Stop(); t.Dispose(); };
                t.Start();
            };

            var btnClose = MkButton("ปิด", new Point(472, y), new Size(88, 32), headerColor);
            btnClose.Font         = new Font("Segoe UI", 10f, FontStyle.Bold);
            btnClose.DialogResult = DialogResult.Cancel;

            Controls.AddRange(new Control[] { pnlHeader, pnlInfo, btnCopy, btnClose });
            AcceptButton = btnClose;
            CancelButton = btnClose;
        }

        // ──────────────────────────────────────────────────────────────────
        // Build error report text (สำหรับ Copy ส่ง IT Helpdesk)
        // ──────────────────────────────────────────────────────────────────
        private static string BuildReport(Exception ex, string username, string timestamp)
        {
            var sb = new StringBuilder();
            sb.AppendLine("=== CMS Error Report ===");
            sb.AppendLine("Time    : " + timestamp);
            sb.AppendLine("User    : " + (string.IsNullOrEmpty(username) ? "(ไม่ระบุ)" : username));
            sb.AppendLine("Version : " + System.Reflection.Assembly.GetExecutingAssembly().GetName().Version);
            sb.AppendLine("Machine : " + System.Net.Dns.GetHostName());
            sb.AppendLine();

            var cur = ex;
            int level = 0;
            while (cur != null)
            {
                if (level > 0) sb.AppendLine();
                sb.AppendLine(string.Format("--- {0} (Level {1}) ---", cur.GetType().FullName, level));
                sb.AppendLine("Message : " + cur.Message);
                sb.AppendLine("Source  : " + (cur.Source ?? "(null)"));
                sb.AppendLine("Stack   :");
                sb.AppendLine(cur.StackTrace ?? "  (no stack trace available)");
                cur = cur.InnerException;
                level++;
            }
            return sb.ToString();
        }

        // ── Control factories ──────────────────────────────────────────────

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
