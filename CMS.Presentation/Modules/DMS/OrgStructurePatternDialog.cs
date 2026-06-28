using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CMS.Domain.Enums;

namespace CMS.Presentation
{
    /// <summary>
    /// Dialog เลือกรูปแบบโครงสร้าง OrgUnit ก่อนสร้าง:
    /// Pattern 1 = Sub→Div→Dep (3 ระดับ), Pattern 2 = Level 0 (Sub→Div, 2 ระดับ)
    /// </summary>
    internal partial class OrgStructurePatternDialog : Form
    {
        public OrgStructurePattern? SelectedPattern { get; private set; }

        private OrgStructurePattern? _current;

        private static readonly Color _colorHeader      = Color.FromArgb(0, 51, 102);
        private static readonly Color _colorSelectedBrd = Color.FromArgb(0, 120, 215);
        private static readonly Color _colorNormalBrd   = Color.FromArgb(210, 210, 210);
        private static readonly Color _colorHover       = Color.FromArgb(235, 243, 255);
        private static readonly Color _colorNormal      = Color.White;

        internal OrgStructurePatternDialog()
        {
            InitializeComponent();
        }

        // ── Card events: DivDep ───────────────────────────────────────────────

        private void PnlDivDep_Paint(object sender, PaintEventArgs e)
        {
            DrawCardBorder(e.Graphics, _pnlDivDep, OrgStructurePattern.DivisionToDepartment);
        }

        private void PnlDivDep_MouseEnter(object sender, EventArgs e)
        {
            OnCardHover(_pnlDivDep, OrgStructurePattern.DivisionToDepartment, true);
        }

        private void PnlDivDep_MouseLeave(object sender, EventArgs e)
        {
            OnCardHover(_pnlDivDep, OrgStructurePattern.DivisionToDepartment, false);
        }

        private void PnlDivDep_Click(object sender, EventArgs e)
        {
            SelectCard(OrgStructurePattern.DivisionToDepartment);
        }

        private void PnlDivDep_DoubleClick(object sender, EventArgs e)
        {
            SelectCard(OrgStructurePattern.DivisionToDepartment);
            Confirm();
        }

        // ── Card events: LevelZero ────────────────────────────────────────────

        private void PnlLevel0_Paint(object sender, PaintEventArgs e)
        {
            DrawCardBorder(e.Graphics, _pnlLevel0, OrgStructurePattern.LevelZeroOnly);
        }

        private void PnlLevel0_MouseEnter(object sender, EventArgs e)
        {
            OnCardHover(_pnlLevel0, OrgStructurePattern.LevelZeroOnly, true);
        }

        private void PnlLevel0_MouseLeave(object sender, EventArgs e)
        {
            OnCardHover(_pnlLevel0, OrgStructurePattern.LevelZeroOnly, false);
        }

        private void PnlLevel0_Click(object sender, EventArgs e)
        {
            SelectCard(OrgStructurePattern.LevelZeroOnly);
        }

        private void PnlLevel0_DoubleClick(object sender, EventArgs e)
        {
            SelectCard(OrgStructurePattern.LevelZeroOnly);
            Confirm();
        }

        // ── Footer ────────────────────────────────────────────────────────────

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

        // ── Helpers ───────────────────────────────────────────────────────────

        private void DrawCardBorder(Graphics g, Panel pnl, OrgStructurePattern pattern)
        {
            bool selected = _current == pattern;
            Color border  = selected ? _colorSelectedBrd : _colorNormalBrd;
            int   width   = selected ? 2 : 1;

            using (var pen = new Pen(border, width))
            {
                var rect = new Rectangle(0, 0, pnl.Width - 1, pnl.Height - 1);
                using (var path = RoundedRect(rect, 8))
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
            path.AddArc(bounds.X,            bounds.Y,             d, d, 180, 90);
            path.AddArc(bounds.Right - d,     bounds.Y,             d, d, 270, 90);
            path.AddArc(bounds.Right - d,     bounds.Bottom - d,    d, d,   0, 90);
            path.AddArc(bounds.X,            bounds.Bottom - d,    d, d,  90, 90);
            path.CloseFigure();
            return path;
        }

        private void OnCardHover(Panel pnl, OrgStructurePattern pattern, bool entering)
        {
            if (_current == pattern) return;
            pnl.BackColor = entering ? _colorHover : _colorNormal;
            pnl.Invalidate();
            foreach (Control c in pnl.Controls) c.BackColor = pnl.BackColor;
        }

        private void SelectCard(OrgStructurePattern pattern)
        {
            _current = pattern;

            var cards = new[]
            {
                new { Panel = _pnlDivDep, Pattern = OrgStructurePattern.DivisionToDepartment },
                new { Panel = _pnlLevel0, Pattern = OrgStructurePattern.LevelZeroOnly }
            };

            foreach (var card in cards)
            {
                bool sel = card.Pattern == pattern;
                card.Panel.BackColor = sel ? Color.FromArgb(235, 245, 255) : _colorNormal;
                foreach (Control c in card.Panel.Controls) c.BackColor = card.Panel.BackColor;
                card.Panel.Invalidate();
            }

            _btnOk.Enabled = true;
        }

        private void Confirm()
        {
            if (!_btnOk.Enabled || !_current.HasValue) return;
            SelectedPattern = _current;
            DialogResult    = DialogResult.OK;
            Close();
        }
    }
}
