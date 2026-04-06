using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CMS.Presentation
{
    /// <summary>
    /// Dialog เลือกวิธีสร้าง Folder: จาก Template หรือ Blank Folder
    /// แทน MessageBox YesNoCancel เดิม
    /// </summary>
    internal class FolderCreationChoiceDialog : Form
    {
        public enum Choice { Template, Blank, Cancel }

        public Choice SelectedChoice { get; private set; } = Choice.Cancel;

        private Panel _pnlTemplate;
        private Panel _pnlBlank;
        private Button _btnOk;
        private Choice _hoveredOrSelected = Choice.Cancel;

        // ── Colors ────────────────────────────────────────────────────────────
        private static readonly Color _colorHeader      = Color.FromArgb(0, 51, 102);
        private static readonly Color _colorSelected    = Color.FromArgb(0, 84, 166);
        private static readonly Color _colorSelectedBrd = Color.FromArgb(0, 120, 215);
        private static readonly Color _colorHover       = Color.FromArgb(235, 243, 255);
        private static readonly Color _colorNormal      = Color.White;
        private static readonly Color _colorNormalBrd   = Color.FromArgb(210, 210, 210);
        private static readonly Color _colorText        = Color.FromArgb(30, 30, 30);
        private static readonly Color _colorSubText     = Color.FromArgb(90, 90, 90);

        public FolderCreationChoiceDialog(string parentFolderName)
        {
            Text            = "วิธีสร้าง Folder";
            ClientSize      = new Size(480, 310);
            FormBorderStyle = FormBorderStyle.FixedDialog;
            StartPosition   = FormStartPosition.CenterParent;
            MaximizeBox     = false;
            MinimizeBox     = false;
            BackColor       = Color.FromArgb(245, 247, 250);
            Font            = new Font("Segoe UI", 9f);

            BuildUI(parentFolderName);
        }

        private void BuildUI(string parentFolderName)
        {
            // ── Header ────────────────────────────────────────────────────────
            var pnlHeader = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 52,
                BackColor = _colorHeader
            };
            var lblTitle = new Label
            {
                Text      = "วิธีสร้าง Folder",
                Dock      = DockStyle.Top,
                Height    = 30,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 12f, FontStyle.Bold),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(16, 0, 0, 0)
            };
            var lblSub = new Label
            {
                Text      = string.Format("ภายใต้ \"{0}\"", parentFolderName),
                Dock      = DockStyle.Fill,
                ForeColor = Color.FromArgb(180, 210, 255),
                Font      = new Font("Segoe UI", 9f),
                TextAlign = ContentAlignment.MiddleLeft,
                Padding   = new Padding(18, 0, 0, 4)
            };
            pnlHeader.Controls.Add(lblSub);
            pnlHeader.Controls.Add(lblTitle);

            // ── Choice cards ──────────────────────────────────────────────────
            var pnlCards = new Panel
            {
                Dock      = DockStyle.Fill,
                BackColor = Color.Transparent,
                Padding   = new Padding(20, 16, 20, 8)
            };

            _pnlTemplate = CreateChoiceCard(
                "📋  จาก Template",
                "สร้าง Sub-folder อัตโนมัติตามโครงสร้าง Template ที่กำหนดไว้",
                Choice.Template);

            _pnlBlank = CreateChoiceCard(
                "📁  Blank Folder",
                "สร้าง Folder เปล่า — กำหนดชื่อและระดับเองได้อิสระ",
                Choice.Blank);

            // วาง cards ซ้าย-ขวา
            _pnlTemplate.SetBounds(20, 16, 200, 130);
            _pnlBlank   .SetBounds(240, 16, 200, 130);
            _pnlTemplate.Anchor = AnchorStyles.Top | AnchorStyles.Left;
            _pnlBlank   .Anchor = AnchorStyles.Top | AnchorStyles.Left;

            pnlCards.Controls.Add(_pnlTemplate);
            pnlCards.Controls.Add(_pnlBlank);

            // ── Footer ────────────────────────────────────────────────────────
            var pnlFooter = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 52,
                BackColor = Color.FromArgb(238, 240, 245)
            };

            // separator line
            var line = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 1,
                BackColor = Color.FromArgb(210, 215, 225)
            };

            _btnOk = new Button
            {
                Text      = "ตกลง",
                Size      = new Size(90, 30),
                BackColor = _colorHeader,
                ForeColor = Color.White,
                FlatStyle = FlatStyle.Flat,
                Font      = new Font("Segoe UI", 9.5f, FontStyle.Bold),
                Enabled   = false,
                Anchor    = AnchorStyles.Right | AnchorStyles.Top
            };
            _btnOk.FlatAppearance.BorderSize = 0;
            _btnOk.Click += (s, e) => Confirm();

            var btnCancel = new Button
            {
                Text         = "ยกเลิก",
                Size         = new Size(80, 30),
                BackColor    = Color.White,
                ForeColor    = Color.FromArgb(60, 60, 60),
                FlatStyle    = FlatStyle.Flat,
                DialogResult = DialogResult.Cancel,
                Anchor       = AnchorStyles.Right | AnchorStyles.Top
            };
            btnCancel.FlatAppearance.BorderColor = Color.FromArgb(190, 190, 190);

            pnlFooter.Controls.Add(line);
            pnlFooter.Controls.Add(_btnOk);
            pnlFooter.Controls.Add(btnCancel);
            pnlFooter.Resize += (s, e) =>
            {
                btnCancel.Left = pnlFooter.ClientSize.Width - btnCancel.Width - 12;
                _btnOk.Left    = btnCancel.Left - _btnOk.Width - 8;
                _btnOk.Top     = btnCancel.Top = (pnlFooter.Height - _btnOk.Height) / 2 + 1;
            };

            // ── Assemble ──────────────────────────────────────────────────────
            Controls.Add(pnlCards);
            Controls.Add(pnlFooter);
            Controls.Add(pnlHeader);

            AcceptButton = _btnOk;
            CancelButton = btnCancel;
        }

        // ── Card factory ──────────────────────────────────────────────────────

        private Panel CreateChoiceCard(string title, string description, Choice choice)
        {
            var pnl = new Panel
            {
                BackColor  = _colorNormal,
                Cursor     = Cursors.Hand,
                Padding    = new Padding(14, 14, 14, 14)
            };

            // border via Paint
            pnl.Paint += (s, e) => DrawCardBorder(e.Graphics, pnl, choice);

            var lblIcon = new Label
            {
                Text      = choice == Choice.Template ? "📋" : "📁",
                Font      = new Font("Segoe UI", 28f),
                ForeColor = choice == Choice.Template
                    ? Color.FromArgb(0, 120, 215)
                    : Color.FromArgb(255, 160, 0),
                AutoSize  = false,
                Bounds    = new Rectangle(14, 14, 56, 56),
                TextAlign = ContentAlignment.MiddleCenter
            };
            var lblTitle = new Label
            {
                Text      = title,
                Font      = new Font("Segoe UI", 10f, FontStyle.Bold),
                ForeColor = _colorText,
                AutoSize  = false,
                Bounds    = new Rectangle(14, 74, 172, 22),
                TextAlign = ContentAlignment.MiddleLeft
            };
            var lblDesc = new Label
            {
                Text      = description,
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = _colorSubText,
                AutoSize  = false,
                Bounds    = new Rectangle(14, 96, 172, 30),
                TextAlign = ContentAlignment.TopLeft
            };

            pnl.Controls.Add(lblIcon);
            pnl.Controls.Add(lblTitle);
            pnl.Controls.Add(lblDesc);

            // Wire up all controls inside the card too
            foreach (Control c in new Control[] { pnl, lblIcon, lblTitle, lblDesc })
            {
                c.MouseEnter  += (s, e) => OnCardHover(pnl, choice, true);
                c.MouseLeave  += (s, e) => OnCardHover(pnl, choice, false);
                c.Click       += (s, e) => SelectCard(choice);
                c.DoubleClick += (s, e) => { SelectCard(choice); Confirm(); };
            }

            return pnl;
        }

        private void DrawCardBorder(Graphics g, Panel pnl, Choice choice)
        {
            bool isSelected = _hoveredOrSelected == choice && _btnOk.Enabled
                              && _hoveredOrSelected != Choice.Cancel;

            Color borderColor = isSelected ? _colorSelectedBrd : _colorNormalBrd;
            int   width       = isSelected ? 2 : 1;

            using (var pen = new Pen(borderColor, width))
            {
                var rect = new Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1);
                // rounded corners
                int r = 8;
                using (var path = RoundedRect(rect, r))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var bg = new SolidBrush(pnl.BackColor))
                        g.FillPath(bg, path);
                    g.DrawPath(pen, path);
                }
            }
        }

        private static GraphicsPath RoundedRect(Rectangle bounds, int radius)
        {
            var path = new GraphicsPath();
            int d = radius * 2;
            path.AddArc(bounds.X, bounds.Y, d, d, 180, 90);
            path.AddArc(bounds.Right - d, bounds.Y, d, d, 270, 90);
            path.AddArc(bounds.Right - d, bounds.Bottom - d, d, d, 0, 90);
            path.AddArc(bounds.X, bounds.Bottom - d, d, d, 90, 90);
            path.CloseFigure();
            return path;
        }

        private void OnCardHover(Panel pnl, Choice choice, bool entering)
        {
            // ถ้ายังไม่ได้เลือก → highlight hover
            bool alreadySelected = _btnOk.Enabled && _hoveredOrSelected == choice;
            if (alreadySelected) return;

            pnl.BackColor = entering ? _colorHover : _colorNormal;
            pnl.Invalidate();
            foreach (Control c in pnl.Controls) c.BackColor = pnl.BackColor;
        }

        private void SelectCard(Choice choice)
        {
            _hoveredOrSelected = choice;

            // reset both cards
            foreach (var (p, ch) in new[] { (_pnlTemplate, Choice.Template), (_pnlBlank, Choice.Blank) })
            {
                bool sel = ch == choice;
                p.BackColor = sel ? Color.FromArgb(235, 245, 255) : _colorNormal;
                foreach (Control c in p.Controls) c.BackColor = p.BackColor;
                p.Invalidate();
            }

            _btnOk.Enabled = true;
        }

        private void Confirm()
        {
            if (!_btnOk.Enabled) return;
            SelectedChoice = _hoveredOrSelected;
            DialogResult   = DialogResult.OK;
            Close();
        }
    }
}
