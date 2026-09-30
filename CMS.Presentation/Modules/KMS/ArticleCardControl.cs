using System;
using System.Collections.Generic;
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
        public event EventHandler<KBArticle> LikeClicked;

        public ArticleCardControl()
        {
            InitializeComponent();
            flowLayoutPanel1.SizeChanged += (s, e) => UpdateCardHeight();
        }

        /// <summary>
        /// flowLayoutPanel1 เป็น AutoSize ตาม lblSummary แล้ว (ไม่ fix width/height) — ส่วนพี่น้องที่เหลือ
        /// ใน pnlContent (pHeaderRight/pnlMeta/pnlActions/panelFooter) ยังคงสูงคงที่ จึงรวมความสูงทั้งหมด
        /// แล้วปรับความสูงของการ์ดทั้งใบตาม เพื่อไม่ให้ summary ที่ยาวขึ้นโดนตัด/บีบซ่อนอยู่ใน panel ที่ fix สูงเท่าเดิม
        /// </summary>
        private void UpdateCardHeight()
        {
            int total = pnlContent.Padding.Top + pnlContent.Padding.Bottom
                      + pHeaderRight.Height + pnlMeta.Height + flowLayoutPanel1.Height
                      + pnlActions.Height + panelFooter.Height;

            if (total > 0 && Height != total)
                Height = total;
        }

        // ── Data binding ─────────────────────────────────────────────────────────

        public KBArticle Article
        {
            get { return _article; }
            set { _article = value; if (value != null) BindArticle(value); }
        }

        /// <summary>
        /// Header label + การซ่อนแสดงของแต่ละ slot ที่ resolve แล้วจาก Article Display Profile ของหมวดที่
        /// บทความนี้อยู่ (ตั้งค่าโดย KBMainForm.PopulateCards ก่อน set Article) — null หรือไม่มี slot ที่ถาม
        /// จะ fallback ไปใช้ default ของ KBArticleDisplaySlotCatalog (พฤติกรรมเดิมทุกประการ)
        /// </summary>
        public IDictionary<string, KBColumnDisplaySetting> DisplayColumn { get; set; }

        private KBColumnDisplaySetting GetDisplaySetting(string slotKey)
        {
            KBColumnDisplaySetting setting;
            if (DisplayColumn != null && DisplayColumn.TryGetValue(slotKey, out setting))
                return setting;
            return new KBColumnDisplaySetting(
                KBArticleDisplaySlotCatalog.GetDefaultLabel(slotKey),
                KBArticleDisplaySlotCatalog.GetDefaultVisible(slotKey));
        }

        /// <summary>ความยาวข้อความสูงสุดของ lblTitle/lblTitleCategory บน card — ยาวเกินนี้จะดันให้ card บิดเบี้ยว</summary>
        private const int MaxCardLabelLength = 100;

        private static string TruncateCardLabel(string text)
        {
            if (string.IsNullOrEmpty(text) || text.Length <= MaxCardLabelLength) return text;
            return text.Substring(0, MaxCardLabelLength) + ".....";
        }

        private void BindArticle(KBArticle a)
        {
            pnlStripe.BackColor   = GetStatusColor(a.Status);
            lblArticleType.Text   = a.ArticleTypeName;
            lblStatus.Text        = a.StatusName;
            lblVersion.Text       = "Version " + a.CurrentVersion;
            lblCategory.Text      = a.CategoryName ?? "-";
            lblSummary.Text       = a.Summary ?? string.Empty;
            lblViewCount.Text     = a.ViewCount.ToString();
            lblLikeCount.Text     = a.LikeCount.ToString();
            lblTags.Text          = string.IsNullOrEmpty(a.Tags) ? "-" : a.Tags;
            btnSendReview.Visible = (a.Status == 1);

            // ── Slot ที่ label/visible มาจาก Article Display Profile ของหมวดนี้ (fallback = default เดิม) ──
            var title = GetDisplaySetting(KBArticleDisplaySlotCatalog.Title);
            lblTitle.Visible = title.Visible;
            lblTitle.Text    = TruncateCardLabel(a.IsOfficialPolicy ? "🏛 " + a.Title : a.Title ?? "(ไม่มีชื่อ)");

            var titleCategory = GetDisplaySetting(KBArticleDisplaySlotCatalog.TitleCategory);
            lblTitleCategory.Visible = titleCategory.Visible;
            string titleCategoryPrefix = string.IsNullOrEmpty(titleCategory.Label) ? string.Empty : titleCategory.Label + " ";
            lblTitleCategory.Text = TruncateCardLabel(titleCategoryPrefix + (a.TitleCategory ?? "-"));

            var content = GetDisplaySetting(KBArticleDisplaySlotCatalog.ContentHTML);
            lblContent.Visible = content.Visible;
            lblContent.Text    = a.ContentHTML.Length >= 185 ? $"{a.ContentHTML.Substring(0, 185)}...read more" : a.ContentHTML;

            var published = GetDisplaySetting(KBArticleDisplaySlotCatalog.PublishedDate);
            label2.Visible           = published.Visible;
            label2.Text              = published.Label;
            lblPublishedDate.Visible = published.Visible;
            lblPublishedDate.Text    = !string.IsNullOrEmpty(a.PublishedDate?.ToString()) ? a.PublishedDate.Value.ToString("dd/MM/yyyy") : "-";

            var expiry = GetDisplaySetting(KBArticleDisplaySlotCatalog.ExpiryDate);
            label3.Visible        = expiry.Visible;
            label3.Text           = expiry.Label;
            lblExpiryDate.Visible = expiry.Visible;
            lblExpiryDate.Text    = !string.IsNullOrEmpty(a.ExpiryDate?.ToString()) ? a.ExpiryDate.Value.ToString("dd/MM/yyyy") : "-";

            var linkedEntities = GetDisplaySetting(KBArticleDisplaySlotCatalog.LinkedEntities);
            label4.Visible        = linkedEntities.Visible;
            label4.Text           = linkedEntities.Label;
            lblShowEntity.Visible = linkedEntities.Visible;
            lblShowEntity.Text    = a.LinkedEntities ?? "-";

            UpdateCardHeight();
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

        private void btnLike_Click(object sender, EventArgs e)
        {
            if (_article == null) return;
            
            // Optimistic UI update — increment immediately for responsiveness
            _article.LikeCount++;
            lblLikeCount.Text = _article.LikeCount.ToString();
            
            // Fire event so parent can persist to DB
            if (LikeClicked != null) LikeClicked(this, _article);
        }
    }
}
