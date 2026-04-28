using System;
using System.Drawing;
using System.Windows.Forms;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// UserControl แสดงบทความ 1 รายการในรูปแบบ Card
    /// Layout ทั้งหมดอยู่ใน ArticleCardControl.Designer.cs
    /// Code-behind นี้มีเฉพาะ event handlers + data binding
    /// </summary>
    public partial class ArticleCardControl : UserControl
    {
        private KBArticle _article;

        // ── Public events ────────────────────────────────────────────────────────
        public event EventHandler<KBArticle> CardClicked;
        public event EventHandler<KBArticle> ReadClicked;
        public event EventHandler<KBArticle> EditClicked;
        public event EventHandler<KBArticle> ShareClicked;
        public event EventHandler<KBArticle> SendReviewClicked;

        public ArticleCardControl()
        {
            InitializeComponent();
        }

        // ── Data binding ─────────────────────────────────────────────────────────

        public KBArticle Article
        {
            get { return _article; }
            set { _article = value; if (value != null) BindArticle(value); }
        }

        private void BindArticle(KBArticle a)
        {
            pnlStripe.BackColor   = GetStatusColor(a.Status);
            lblTitle.Text         = a.IsOfficialPolicy ? "🏛 " + a.Title : a.Title ?? "(ไม่มีชื่อ)";
            lblArticleType.Text   = a.ArticleTypeName;
            lblStatus.Text        = a.StatusName;
            lblVersion.Text       = "Version " + a.CurrentVersion;
            lblCategory.Text      = a.CategoryName ?? "-";
            lblSummary.Text       = a.Summary ?? string.Empty;
            lblViewCount.Text     = a.ViewCount.ToString();
            lblLikeCount.Text     = a.LikeCount.ToString();
            lblTags.Text          = string.IsNullOrEmpty(a.Tags) ? "-" : a.Tags;
            btnSendReview.Visible = (a.Status == 1);
        }

        private static Color GetStatusColor(byte status)
        {
            switch (status)
            {
                case 1:  return Color.FromArgb(180, 180, 180); // Draft
                case 2:  return Color.FromArgb(255, 165, 0);   // Review
                case 3:  return Color.FromArgb(34,  139, 34);  // Published
                case 4:  return Color.FromArgb(139, 0,   0);   // Archived
                default: return Color.FromArgb(200, 200, 200);
            }
        }

        // ── Hover handlers (wired in Designer.cs) ───────────────────────────────

        private void OnCardMouseEnter(object sender, EventArgs e)
        {
            BackColor = Color.FromArgb(240, 246, 255);
        }

        private void OnCardMouseLeave(object sender, EventArgs e)
        {
            BackColor = Color.White;
        }

        // ── Click handlers (wired in Designer.cs) ───────────────────────────────

        private void OnCardClick(object sender, EventArgs e)
        {
            if (_article == null) return;
            if (CardClicked != null) CardClicked(this, _article);
        }

        private void btnRead_Click(object sender, EventArgs e)
        {
            if (_article == null) return;
            if (ReadClicked != null) ReadClicked(this, _article);
        }

        private void btnEdit_Click(object sender, EventArgs e)
        {
            if (_article == null) return;
            if (EditClicked != null) EditClicked(this, _article);
        }

        private void btnShare_Click(object sender, EventArgs e)
        {
            if (_article == null) return;
            if (ShareClicked != null) ShareClicked(this, _article);
        }

        private void btnSendReview_Click(object sender, EventArgs e)
        {
            if (_article == null) return;
            if (SendReviewClicked != null) SendReviewClicked(this, _article);
        }
    }
}
