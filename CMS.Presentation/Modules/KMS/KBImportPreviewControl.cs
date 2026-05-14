using System;
using System.Collections.Generic;
using System.Drawing;
using System.Windows.Forms;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// UserControl that hosts a grid preview of imported KB rows with paging.
    /// Call <see cref="BindRows"/> to populate the view.
    /// Layout is defined in <c>KBImportPreviewControl.Designer.cs</c>.
    /// </summary>
    public partial class KBImportPreviewControl : UserControl
    {
        // ── Paging constants ─────────────────────────────────────────────────────
        private const int GridPageSize = 50;

        // ── Paging state ─────────────────────────────────────────────────────────
        private IList<KBImportRow> _allRows = new List<KBImportRow>();
        private int _gridPage = 0;   // 0-based

        // ── Constructor ──────────────────────────────────────────────────────────

        public KBImportPreviewControl()
        {
            InitializeComponent();
        }

        // ── Public API ───────────────────────────────────────────────────────────

        /// <summary>
        /// Populates the grid view with <paramref name="rows"/>.
        /// Resets paging to page 1.
        /// </summary>
        public void BindRows(IList<KBImportRow> rows)
        {
            _allRows  = rows ?? new List<KBImportRow>();
            _gridPage = 0;
            RefreshGridPage();
        }

        /// <summary>Clears the grid and resets paging.</summary>
        public void Clear()
        {
            _allRows  = new List<KBImportRow>();
            _gridPage = 0;
            dgvPreview.Rows.Clear();
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

        // ── Private — grid paging ────────────────────────────────────────────────

        private void RefreshGridPage()
        {
            dgvPreview.SuspendLayout();
            dgvPreview.Rows.Clear();

            try
            {
                int start = _gridPage * GridPageSize;
                int end   = Math.Min(start + GridPageSize, _allRows.Count);

                for (int i = start; i < end; i++)
                {
                    var r = _allRows[i];
                    try
                    {
                        int idx = dgvPreview.Rows.Add(
                            r.RowNumber.ToString(),         // colNo
                            S(r.ArticleCode),               // colArticleCode
                            S(r.ColA),                      // colColA
                            S(r.ColB),                      // colColB
                            S(r.ColC),                      // colCatName
                            S(r.ColD),                      // colColD
                            S(r.ColE),                      // colTitle
                            S(r.ColF),                      // colColF
                            S(r.ColG),                      // colColG
                            S(r.ColH),                      // colColH
                            S(r.ColI),                      // colColI
                            S(r.ColJ),                      // colColJ
                            S(r.ColK),                      // colColK
                            S(r.ColL),                      // colColL
                            S(r.ColM),                      // colColM
                            S(r.ColN),                      // colColN
                            S(r.ColO),                      // colColO
                            S(r.ColP),                      // colColP
                            S(r.ColQ),                      // colColQ
                            S(r.ColR),                      // colPublished
                            S(r.ColS),                      // colExpiry
                            S(r.ColT, 300),                 // colContentHtml — truncate long HTML
                            S(r.ColU),                      // colColU
                            S(r.ColV),                      // colColV
                            S(r.Summary),                   // colSummary
                            StatusLabel(r.Status),          // colStatus
                            S(r.StatusNote));               // colNote

                        var dgvRow = dgvPreview.Rows[idx];
                        if (r.Status == "Error")
                            dgvRow.DefaultCellStyle.BackColor = Color.FromArgb(253, 237, 236);
                        else if (r.Status == "Warn")
                            dgvRow.DefaultCellStyle.BackColor = Color.FromArgb(254, 249, 231);
                        else if ((i - start) % 2 == 1)
                            dgvRow.DefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
                    }
                    catch
                    {
                        // One row that fails to render must not stop subsequent rows from
                        // being added. Continue the loop — the grid skips this row silently.
                    }
                }
            }
            finally
            {
                // Always resume layout even when a row throws, so the grid repaints correctly.
                dgvPreview.ResumeLayout();
            }

            UpdatePagerLabels();
        }

        // Coalesces null to empty string and clips to maxLen to prevent DataGridView
        // rendering issues caused by very long cell values (ColT ContentHTML can be MB-sized).
        private static string S(string value, int maxLen = 500)
        {
            if (value == null) return string.Empty;
            return value.Length <= maxLen ? value : value.Substring(0, maxLen) + "…";
        }

        // ── Private — pager state ────────────────────────────────────────────────

        private void UpdatePagerLabels()
        {
            int gridTotal = TotalPages(_allRows.Count, GridPageSize);

            lblGridPage.Text = string.Format("หน้า {0} / {1}  ({2} รายการ)",
                _gridPage + 1, gridTotal, _allRows.Count);

            btnGridPrev.Enabled = _gridPage > 0;
            btnGridNext.Enabled = _gridPage < gridTotal - 1;
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
