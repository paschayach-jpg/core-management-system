using System;
using System.Collections.Generic;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Linq;
using System.Windows.Forms;

namespace CMS.Presentation
{
    internal partial class DuplicateFileChoiceDialog : Form
    {
        public enum Choice { SaveCopy, SkipDuplicates, Cancel }

        public Choice SelectedChoice { get; private set; } = Choice.Cancel;

        private Choice _hoveredOrSelected = Choice.Cancel;

        internal DuplicateFileChoiceDialog() : this("(ไม่ได้ระบุโฟลเดอร์)", Enumerable.Empty<string>()) { }

        public DuplicateFileChoiceDialog(string folderName, IEnumerable<string> duplicateFiles)
        {
            InitializeComponent();

            _lblSub.Text = string.Format("ภายใน \"{0}\"", folderName);

            var files = (duplicateFiles ?? Enumerable.Empty<string>())
                .Where(name => !string.IsNullOrWhiteSpace(name))
                .Distinct(StringComparer.OrdinalIgnoreCase)
                .OrderBy(name => name)
                .ToArray();

            _lblSummary.Text = string.Format("พบไฟล์ซ้ำ {0} รายการ กรุณาเลือกวิธีดำเนินการ", files.Length);
            _lstDuplicates.Items.Clear();
            _lstDuplicates.Items.AddRange(files.Cast<object>().ToArray());
        }

        private void BtnOk_Click(object sender, EventArgs e)
        {
            Confirm();
        }

        private void PnlFooter_Resize(object sender, EventArgs e)
        {
            _btnCancel.Left = _pnlFooter.ClientSize.Width - _btnCancel.Width - 12;
            _btnOk.Left = _btnCancel.Left - _btnOk.Width - 8;
            _btnOk.Top = _btnCancel.Top = (_pnlFooter.Height - _btnOk.Height) / 2 + 1;
        }

        private void PnlSaveCopy_Paint(object sender, PaintEventArgs e)
        {
            DrawCardBorder(e.Graphics, _pnlSaveCopy, Choice.SaveCopy);
        }

        private void PnlSaveCopy_MouseEnter(object sender, EventArgs e)
        {
            OnCardHover(_pnlSaveCopy, Choice.SaveCopy, true);
        }

        private void PnlSaveCopy_MouseLeave(object sender, EventArgs e)
        {
            OnCardHover(_pnlSaveCopy, Choice.SaveCopy, false);
        }

        private void PnlSaveCopy_Click(object sender, EventArgs e)
        {
            SelectCard(Choice.SaveCopy);
        }

        private void PnlSaveCopy_DoubleClick(object sender, EventArgs e)
        {
            SelectCard(Choice.SaveCopy);
            Confirm();
        }

        private void PnlSkip_Paint(object sender, PaintEventArgs e)
        {
            DrawCardBorder(e.Graphics, _pnlSkip, Choice.SkipDuplicates);
        }

        private void PnlSkip_MouseEnter(object sender, EventArgs e)
        {
            OnCardHover(_pnlSkip, Choice.SkipDuplicates, true);
        }

        private void PnlSkip_MouseLeave(object sender, EventArgs e)
        {
            OnCardHover(_pnlSkip, Choice.SkipDuplicates, false);
        }

        private void PnlSkip_Click(object sender, EventArgs e)
        {
            SelectCard(Choice.SkipDuplicates);
        }

        private void PnlSkip_DoubleClick(object sender, EventArgs e)
        {
            SelectCard(Choice.SkipDuplicates);
            Confirm();
        }

        private void DrawCardBorder(Graphics g, Panel panel, Choice choice)
        {
            bool isSelected = _hoveredOrSelected == choice && _btnOk.Enabled && _hoveredOrSelected != Choice.Cancel;
            Color borderColor = isSelected ? Color.FromArgb(0, 120, 215) : Color.FromArgb(210, 210, 210);
            int width = isSelected ? 2 : 1;

            using (var pen = new Pen(borderColor, width))
            {
                var rect = new Rectangle(0, 0, panel.Width - 1, panel.Height - 1);
                using (var path = RoundedRect(rect, 8))
                {
                    g.SmoothingMode = SmoothingMode.AntiAlias;
                    using (var bg = new SolidBrush(panel.BackColor))
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

        private void OnCardHover(Panel panel, Choice choice, bool entering)
        {
            bool alreadySelected = _btnOk.Enabled && _hoveredOrSelected == choice;
            if (alreadySelected) return;

            panel.BackColor = entering ? Color.FromArgb(235, 243, 255) : Color.White;
            panel.Invalidate();
            foreach (Control control in panel.Controls)
                control.BackColor = panel.BackColor;
        }

        private void SelectCard(Choice choice)
        {
            _hoveredOrSelected = choice;

            foreach (var entry in new[]
            {
                new KeyValuePair<Panel, Choice>(_pnlSaveCopy, Choice.SaveCopy),
                new KeyValuePair<Panel, Choice>(_pnlSkip, Choice.SkipDuplicates)
            })
            {
                bool selected = entry.Value == choice;
                entry.Key.BackColor = selected ? Color.FromArgb(235, 245, 255) : Color.White;
                foreach (Control control in entry.Key.Controls)
                    control.BackColor = entry.Key.BackColor;
                entry.Key.Invalidate();
            }

            _btnOk.Enabled = true;
        }

        private void Confirm()
        {
            if (!_btnOk.Enabled) return;
            SelectedChoice = _hoveredOrSelected;
            DialogResult = DialogResult.OK;
            Close();
        }
    }
}