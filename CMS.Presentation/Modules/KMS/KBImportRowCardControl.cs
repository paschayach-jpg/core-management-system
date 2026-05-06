using System;
using System.Drawing;
using System.Drawing.Drawing2D;
using System.Windows.Forms;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// A card-style UserControl that displays one <see cref="KBImportRow"/> in the
    /// Card-view tab of the KB Import Wizard preview.
    /// Layout is defined in <c>KBImportRowCardControl.Designer.cs</c>.
    /// </summary>
    public partial class KBImportRowCardControl : UserControl
    {
        public KBImportRowCardControl()
        {
            InitializeComponent();
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>Binds a <see cref="KBImportRow"/> to this card.</summary>
        public void BindRow(KBImportRow row)
        {
            if (row == null) return;

            Color accent = StatusColor(row.Status);
            Color bg     = StatusBg(row.Status);

            BackColor         = bg;
            pnlStripe.BackColor = accent;

            lblRowNum.Text    = string.Format("แถว {0}", row.RowNumber);
            lblStatus.Text    = StatusLabel(row.Status);
            lblStatus.ForeColor = accent;

            lblCode.Text      = string.IsNullOrEmpty(row.ArticleCode)
                ? "(รหัสยังไม่ถูกสร้าง)"
                : row.ArticleCode;
            lblCode.ForeColor  = string.IsNullOrEmpty(row.ArticleCode)
                ? Color.FromArgb(127, 140, 141)
                : Color.FromArgb(46, 116, 181);

            lblTitle.Text     = string.IsNullOrEmpty(row.Title) ? row.ColE ?? string.Empty : row.Title;
            lblCategory.Text  = string.IsNullOrEmpty(row.ColC)
                ? string.Empty
                : string.Format("📁  {0}  ({1} > {2})", row.ColC, row.ColA, row.ColB);

            lblSummary.Text   = string.IsNullOrEmpty(row.Summary)
                ? string.Empty
                : string.Format("📝  {0}", row.Summary);

            // ContentHTML — strip tags and show a short plain-text preview
            string contentPreview = StripHtmlTags(row.ColT);
            if (!string.IsNullOrEmpty(contentPreview))
            {
                string display = contentPreview.Length >= 50
                    ? contentPreview.Substring(0, 50) + "....."
                    : contentPreview;
                lblContentHtml.Text = string.Format("📄  {0}", display);
            }
            else
            {
                lblContentHtml.Text = string.Empty;
            }

            string pub = string.IsNullOrEmpty(row.ColR) ? "—" : row.ColR;
            string exp = string.IsNullOrEmpty(row.ColS) ? "—" : row.ColS;
            lblDates.Text = string.Format("📅  เผยแพร่: {0}   |   หมดอายุ: {1}", pub, exp);

            bool hasNote    = !string.IsNullOrEmpty(row.StatusNote);
            lblNote.Visible = hasNote;
            lblNote.Text    = hasNote ? string.Format("⚠  {0}", row.StatusNote) : string.Empty;
            lblNote.ForeColor = row.Status == "Error"
                ? Color.FromArgb(192, 57, 43)
                : Color.FromArgb(243, 156, 18);
        }

        // ── Paint — border ───────────────────────────────────────────────────────

        protected override void OnPaint(PaintEventArgs e)
        {
            base.OnPaint(e);
            e.Graphics.SmoothingMode = SmoothingMode.AntiAlias;
            using (var pen = new Pen(Color.FromArgb(208, 215, 222), 1f))
                e.Graphics.DrawRectangle(pen, 0, 0, Width - 1, Height - 1);
        }

        // ── Static helpers ───────────────────────────────────────────────────────

        private static Color StatusColor(string status)
        {
            if (status == "Valid") return Color.FromArgb(39, 174, 96);
            if (status == "Warn")  return Color.FromArgb(243, 156, 18);
            return Color.FromArgb(192, 57, 43);
        }

        private static Color StatusBg(string status)
        {
            if (status == "Valid") return Color.FromArgb(240, 255, 244);
            if (status == "Warn")  return Color.FromArgb(254, 249, 231);
            if (status == "Error") return Color.FromArgb(253, 237, 236);
            return Color.White;
        }

        private static string StatusLabel(string status)
        {
            if (status == "Valid") return "✅  Valid";
            if (status == "Warn")  return "⚠️  Warn";
            if (status == "Error") return "❌  Error";
            return "⏳";
        }

        /// <summary>
        /// Removes HTML tags from <paramref name="html"/> and collapses whitespace,
        /// returning plain text suitable for a single-line preview label.
        /// </summary>
        private static string StripHtmlTags(string html)
        {
            if (string.IsNullOrEmpty(html)) return string.Empty;

            // Remove all <tag> and </tag> occurrences
            var sb = new System.Text.StringBuilder(html.Length);
            bool inTag = false;
            foreach (char c in html)
            {
                if (c == '<')      { inTag = true;  continue; }
                if (c == '>')      { inTag = false; continue; }
                if (inTag)         continue;
                if (c == '\r' || c == '\n' || c == '\t')
                    sb.Append(' ');
                else
                    sb.Append(c);
            }

            // Collapse multiple spaces into one
            string result = sb.ToString();
            while (result.Contains("  "))
                result = result.Replace("  ", " ");

            return result.Trim();
        }
    }
}
