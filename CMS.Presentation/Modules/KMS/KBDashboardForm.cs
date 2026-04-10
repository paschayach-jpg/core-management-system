// ✅ Allowed usings: CMS.Controllers, CMS.Domain
// ❌ Forbidden usings: CMS.Services, CMS.Data
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Controllers.KMS.KB;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// KMS Dashboard & Reports — แสดงสถิติ, รายงาน, และ KPI ของระบบ Knowledge Base
    /// Layout ตาม MOD-006/kms_dashboard.html
    /// </summary>
    public partial class KBDashboardForm : Form, IKBDashboardController
    {
        private KBDashboardController _presenter;
        private readonly UserContext _user;

        // ── Constructor ─────────────────────────────────────────────────────────
        public KBDashboardForm(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            _user = user;
            InitializeComponent();
        }

        /// <summary>
        /// Called by the composition root after the presenter is fully wired.
        /// Must be called before ShowDialog().
        /// </summary>
        public void AttachPresenter(KBDashboardController presenter)
        {
            if (presenter == null) throw new ArgumentNullException("presenter");
            _presenter = presenter;
        }

        // ── Form Load ───────────────────────────────────────────────────────────
        private void KBDashboardForm_Load(object sender, EventArgs e)
        {
            if (_presenter == null)
            {
                MessageBox.Show(
                    "Presenter ยังไม่ได้ถูก Attach\n\nกรุณาเรียก AttachPresenter() ก่อนเปิดฟอร์ม",
                    "ข้อผิดพลาดภายใน", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            // Status bar: user info
            lblStatusUser.Text = string.Format("👤  {0}  |  📅  {1}",
                _user.AdIdentity ?? "ผู้ใช้งาน",
                DateTime.Now.ToString("dd/MM/yyyy"));

            // Initialize tabs
            ShowDashboardTab();
            
            // Load data
            _presenter.Initialize();
        }

        // ── IKBDashboardView Implementation ─────────────────────────────────────

        public void DisplayStats(KBDashboardStats stats)
        {
            if (stats == null) return;

            // KPI Cards
            lblTotalArticles.Text = stats.TotalArticles.ToString();
            lblTotalArticlesSub.Text = string.Format("↑ +{0} เดือนนี้", 
                stats.TotalArticles > 90 ? stats.TotalArticles - 90 : 0);

            lblMonthlyViews.Text = stats.MonthlyViews.ToString("#,##0");
            lblMonthlyViewsSub.Text = "↑ +12% จากเดือนที่แล้ว";

            lblAvgRating.Text = stats.AverageRating.ToString("0.0");
            lblAvgRatingSub.Text = "/ 5.0 — ดีขึ้น 0.2";

            lblPublished.Text = stats.PublishedArticles.ToString();
            lblPublishedSub.Text = string.Format("✅ คิดเป็น {0}% ของทั้งหมด",
                stats.TotalArticles > 0 ? (stats.PublishedArticles * 100 / stats.TotalArticles) : 0);

            // Alert counts
            lblOverdueCount.Text = stats.OverdueReviews.ToString();
            lblDraftCount.Text = stats.DraftArticles.ToString();
            lblExpiringCount.Text = stats.ExpiringArticles.ToString();
            lblArchivedCount.Text = stats.ArchivedArticles.ToString();

            // Approval badge
            lblApprovalBadge.Text = stats.PendingApprovals.ToString();
        }

        public void DisplayMonthlyChart(IEnumerable<KBMonthlyArticle> monthlyData)
        {
            if (monthlyData == null) return;

            pnlChart.Controls.Clear();
            var data = monthlyData.ToList();
            if (data.Count == 0) return;

            int maxCount = data.Max(m => m.ArticleCount);
            if (maxCount == 0) maxCount = 1;

            int barWidth = (pnlChart.Width - 40) / data.Count;
            int x = 10;

            foreach (var month in data)
            {
                int barHeight = (int)((month.ArticleCount / (double)maxCount) * 100);
                
                var lblValue = new Label
                {
                    Location = new Point(x, 10),
                    Size = new Size(barWidth - 5, 20),
                    Text = month.ArticleCount.ToString(),
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(46, 116, 181),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                var bar = new Panel
                {
                    Location = new Point(x, 120 - barHeight),
                    Size = new Size(barWidth - 10, barHeight),
                    BackColor = Color.FromArgb(46, 116, 181),
                    Cursor = Cursors.Hand
                };
                bar.MouseEnter += (s, e) => bar.BackColor = Color.FromArgb(30, 90, 150);
                bar.MouseLeave += (s, e) => bar.BackColor = Color.FromArgb(46, 116, 181);

                var lblMonth = new Label
                {
                    Location = new Point(x, 125),
                    Size = new Size(barWidth - 5, 20),
                    Text = month.MonthName,
                    Font = new Font("Segoe UI", 8f),
                    ForeColor = Color.FromArgb(127, 140, 141),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                pnlChart.Controls.Add(lblValue);
                pnlChart.Controls.Add(bar);
                pnlChart.Controls.Add(lblMonth);

                x += barWidth;
            }
        }

        public void DisplayTopArticles(IEnumerable<KBTopArticle> topArticles)
        {
            if (topArticles == null) return;

            pnlTopArticles.Controls.Clear();
            var articles = topArticles.ToList();
            int y = 0;
            int rank = 1;

            foreach (var article in articles)
            {
                var pnlItem = new Panel
                {
                    Location = new Point(0, y),
                    Size = new Size(pnlTopArticles.Width - 5, 45),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                var lblRank = new Label
                {
                    Location = new Point(10, 12),
                    Size = new Size(25, 22),
                    Text = rank.ToString(),
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = Color.White,
                    BackColor = GetRankColor(rank),
                    TextAlign = ContentAlignment.MiddleCenter
                };

                var lblTitle = new Label
                {
                    Location = new Point(45, 8),
                    Size = new Size(pnlItem.Width - 150, 18),
                    Text = article.Title,
                    Font = new Font("Segoe UI", 9f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(26, 35, 50),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                var lblMeta = new Label
                {
                    Location = new Point(45, 26),
                    Size = new Size(pnlItem.Width - 150, 16),
                    Text = string.Format("{0} | {1} | {2} | ⭐ {3:0.0}",
                        article.ArticleCode, article.ArticleType, article.Category, article.Rating),
                    Font = new Font("Segoe UI", 8f),
                    ForeColor = Color.FromArgb(127, 140, 141),
                    Anchor = AnchorStyles.Top | AnchorStyles.Left | AnchorStyles.Right
                };

                var lblViews = new Label
                {
                    Location = new Point(pnlItem.Width - 90, 15),
                    Size = new Size(80, 18),
                    Text = string.Format("{0} v", article.ViewCount),
                    Font = new Font("Segoe UI", 10f, FontStyle.Bold),
                    ForeColor = Color.FromArgb(46, 116, 181),
                    TextAlign = ContentAlignment.MiddleRight,
                    Anchor = AnchorStyles.Top | AnchorStyles.Right
                };

                var separator = new Panel
                {
                    Dock = DockStyle.Bottom,
                    Height = 1,
                    BackColor = Color.FromArgb(240, 244, 249)
                };

                pnlItem.Controls.Add(lblRank);
                pnlItem.Controls.Add(lblTitle);
                pnlItem.Controls.Add(lblMeta);
                pnlItem.Controls.Add(lblViews);
                pnlItem.Controls.Add(separator);

                pnlTopArticles.Controls.Add(pnlItem);

                y += 45;
                rank++;
            }
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowInfo(string message)
        {
            MessageBox.Show(message, "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void SetStatus(string message)
        {
            lblStatusMessage.Text = message;
        }

        // ── Helper Methods ──────────────────────────────────────────────────────

        private Color GetRankColor(int rank)
        {
            switch (rank)
            {
                case 1: return Color.FromArgb(212, 172, 13);  // Gold
                case 2: return Color.FromArgb(127, 140, 141); // Silver
                case 3: return Color.FromArgb(230, 126, 34);  // Bronze
                default: return Color.FromArgb(46, 116, 181); // Blue
            }
        }

        private void ShowDashboardTab()
        {
            tabControl.SelectedIndex = 0;
        }

        private void ShowApprovalTab()
        {
            tabControl.SelectedIndex = 1;
        }

        // ── Event Handlers ──────────────────────────────────────────────────────

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            if (_presenter != null)
                _presenter.RefreshDashboard();
        }

        private void btnExportExcel_Click(object sender, EventArgs e)
        {
            if (_presenter != null)
                _presenter.ExportToExcel();
        }

        private void btnPrint_Click(object sender, EventArgs e)
        {
            if (_presenter != null)
                _presenter.PrintReport();
        }

        private void btnSendReport_Click(object sender, EventArgs e)
        {
            if (_presenter != null)
                _presenter.SendReport();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }
    }
}
