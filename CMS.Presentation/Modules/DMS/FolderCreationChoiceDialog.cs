using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;

namespace CMS.Presentation
{
    /// <summary>
    /// Dialog เลือกวิธีสร้าง Folder: จาก Template หรือ Blank Folder
    /// แทน MessageBox YesNoCancel เดิม
    /// </summary>
    internal partial class FolderCreationChoiceDialog : Form
    {
        public enum Choice { Template, Blank, Cancel }

        public Choice SelectedChoice { get; private set; } = Choice.Cancel;

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

        /// <summary>For Visual Studio Designer only.</summary>
        internal FolderCreationChoiceDialog() : this("(ชื่อ Folder หลัก)") { }

        public FolderCreationChoiceDialog(string parentFolderName)
        {
            InitializeComponent();
            _lblSub.Text = string.Format("ภายใต้ \"{0}\"", parentFolderName);
        }

        // ── Event Handlers ────────────────────────────────────────────────────

        private void BtnOk_Click(object sender, EventArgs e)
        {
            Confirm();
        }

        private void PnlFooter_Resize(object sender, EventArgs e)
        {
            _btnCancel.Left = _pnlFooter.ClientSize.Width - _btnCancel.Width - 12;
            _btnOk.Left     = _btnCancel.Left - _btnOk.Width - 8;
            _btnOk.Top      = _btnCancel.Top = (_pnlFooter.Height - _btnOk.Height) / 2 + 1;
        }

        // ── Template card events ──────────────────────────────────────────────

        private void PnlTemplate_Paint(object sender, PaintEventArgs e)
        {
            DrawCardBorder(e.Graphics, _pnlTemplate, Choice.Template);
        }

        private void PnlTemplate_MouseEnter(object sender, EventArgs e)
        {
            OnCardHover(_pnlTemplate, Choice.Template, true);
        }

        private void PnlTemplate_MouseLeave(object sender, EventArgs e)
        {
            OnCardHover(_pnlTemplate, Choice.Template, false);
        }

        private void PnlTemplate_Click(object sender, EventArgs e)
        {
            SelectCard(Choice.Template);
        }

        private void PnlTemplate_DoubleClick(object sender, EventArgs e)
        {
            SelectCard(Choice.Template);
            Confirm();
        }

        // ── Blank card events ─────────────────────────────────────────────────

        private void PnlBlank_Paint(object sender, PaintEventArgs e)
        {
            DrawCardBorder(e.Graphics, _pnlBlank, Choice.Blank);
        }

        private void PnlBlank_MouseEnter(object sender, EventArgs e)
        {
            OnCardHover(_pnlBlank, Choice.Blank, true);
        }

        private void PnlBlank_MouseLeave(object sender, EventArgs e)
        {
            OnCardHover(_pnlBlank, Choice.Blank, false);
        }

        private void PnlBlank_Click(object sender, EventArgs e)
        {
            SelectCard(Choice.Blank);
        }

        private void PnlBlank_DoubleClick(object sender, EventArgs e)
        {
            SelectCard(Choice.Blank);
            Confirm();
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
            var cards = new[]
            {
                new KeyValuePair<Panel, Choice>(_pnlTemplate, Choice.Template),
                new KeyValuePair<Panel, Choice>(_pnlBlank, Choice.Blank)
            };

            foreach (var card in cards)
            {
                bool sel = card.Value == choice;
                Panel panel = card.Key;
                panel.BackColor = sel ? Color.FromArgb(235, 245, 255) : _colorNormal;
                foreach (Control c in panel.Controls) c.BackColor = panel.BackColor;
                panel.Invalidate();
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
