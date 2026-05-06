using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// UserControl that hosts a two-tab preview of imported KB rows:
    ///   Tab 1 — Grid view  (DataGridView + paging)
    ///   Tab 2 — Card view  (FlowLayoutPanel of <see cref="KBImportRowCardControl"/> + paging)
    ///
    /// Call <see cref="BindRows"/> to populate both views.
    /// Layout is defined in <c>KBImportPreviewControl.Designer.cs</c>.
    /// </summary>
    public partial class KBImportPreviewControl : UserControl
    {
        // ── Paging constants ─────────────────────────────────────────────────────
        private const int GridPageSize = 50;
        private const int CardPageSize = 20;

        // ── Paging state ─────────────────────────────────────────────────────────
        private IList<KBImportRow> _allRows = new List<KBImportRow>();
        private int _gridPage = 0;   // 0-based
        private int _cardPage = 0;

        // ── Constructor ──────────────────────────────────────────────────────────

        public KBImportPreviewControl()
        {
            InitializeComponent();
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Populates both the Grid and Card views with <paramref name="rows"/>.
        /// Resets paging to page 1.
        /// </summary>
        public void BindRows(IList<KBImportRow> rows)
        {
            _allRows  = rows ?? new List<KBImportRow>();
            _gridPage = 0;
            _cardPage = 0;
            RefreshGridPage();
            RefreshCardPage();
        }

        /// <summary>Clears both views and resets paging.</summary>
        public void Clear()
        {
            _allRows  = new List<KBImportRow>();
            _gridPage = 0;
            _cardPage = 0;
            dgvPreview.Rows.Clear();
            flowCards.Controls.Clear();
            UpdatePagerLabels();
        }

        // ── Event handlers ───────────────────────────────────────────────────────

        private void btnGridPrev_Click(object sender, EventArgs e)
        {
            if (_gridPage > 0) { _gridPage--; RefreshGridPage(); }
        }

        private void btnGridNext_Click(object sender, EventArgs e)
        {
            int maxPage = MaxPage(_allRows.Count, GridPageSize);
            if (_gridPage < maxPage) { _gridPage++; RefreshGridPage(); }
        }

        private void btnCardPrev_Click(object sender, EventArgs e)
        {
            if (_cardPage > 0) { _cardPage--; RefreshCardPage(); }
        }

        private void btnCardNext_Click(object sender, EventArgs e)
        {
            int maxPage = MaxPage(_allRows.Count, CardPageSize);
            if (_cardPage < maxPage) { _cardPage++; RefreshCardPage(); }
        }

        // ── Private — grid paging ────────────────────────────────────────────────

        private void RefreshGridPage()
        {
            dgvPreview.SuspendLayout();
            dgvPreview.Rows.Clear();

            int start = _gridPage * GridPageSize;
            int end   = Math.Min(start + GridPageSize, _allRows.Count);

            for (int i = start; i < end; i++)
            {
                var r   = _allRows[i];
                int idx = dgvPreview.Rows.Add(
                    r.RowNumber.ToString(),
                    r.ArticleCode,
                    r.ColC,
                    r.Title ?? r.ColE,
                    r.Summary,
                    r.ColT,
                    string.Format("{0}, {1}", r.ColC, r.ColE),
                    r.ColR,
                    r.ColS,
                    StatusLabel(r.Status),
                    r.StatusNote ?? string.Empty);

                var row = dgvPreview.Rows[idx];
                if (r.Status == "Error")
                    row.DefaultCellStyle.BackColor = Color.FromArgb(253, 237, 236);
                else if (r.Status == "Warn")
                    row.DefaultCellStyle.BackColor = Color.FromArgb(254, 249, 231);
                else if ((i - start) % 2 == 1)
                    row.DefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
            }

            dgvPreview.ResumeLayout();
            UpdatePagerLabels();
        }

        // ── Private — card paging ────────────────────────────────────────────────

        private void RefreshCardPage()
        {
            flowCards.SuspendLayout();
            flowCards.Controls.Clear();

            int start = _cardPage * CardPageSize;
            int end   = Math.Min(start + CardPageSize, _allRows.Count);

            for (int i = start; i < end; i++)
            {
                var card = new KBImportRowCardControl();
                card.BindRow(_allRows[i]);
                flowCards.Controls.Add(card);
            }

            flowCards.ResumeLayout();
            UpdatePagerLabels();
        }

        // ── Private — pager state ────────────────────────────────────────────────

        private void UpdatePagerLabels()
        {
            int gridTotal = TotalPages(_allRows.Count, GridPageSize);
            int cardTotal = TotalPages(_allRows.Count, CardPageSize);

            lblGridPage.Text = string.Format("หน้า {0} / {1}  ({2} รายการ)",
                _gridPage + 1, gridTotal, _allRows.Count);
            lblCardPage.Text = string.Format("หน้า {0} / {1}  ({2} รายการ)",
                _cardPage + 1, cardTotal, _allRows.Count);

            btnGridPrev.Enabled = _gridPage > 0;
            btnGridNext.Enabled = _gridPage < gridTotal - 1;
            btnCardPrev.Enabled = _cardPage > 0;
            btnCardNext.Enabled = _cardPage < cardTotal - 1;
        }

        // ── Static helpers ───────────────────────────────────────────────────────

        private static int TotalPages(int count, int pageSize)
        {
            return count == 0 ? 1 : (int)Math.Ceiling(count / (double)pageSize);
        }

        private static int MaxPage(int count, int pageSize)
        {
            return Math.Max(0, TotalPages(count, pageSize) - 1);
        }

        private static string StatusLabel(string status)
        {
            if (status == "Valid") return "✅  Valid";
            if (status == "Warn")  return "⚠️  Warn";
            if (status == "Error") return "❌  Error";
            return "⏳";
        }
    }
}
