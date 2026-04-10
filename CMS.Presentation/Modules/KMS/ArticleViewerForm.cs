// ✅ Allowed usings: CMS.Controllers, CMS.Domain
// ❌ Forbidden usings: CMS.Services, CMS.Data
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.KMS.Interfaces;
using CMS.Controllers.KMS.KB;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// KMS-03 Article Viewer — แสดงรายละเอียดบทความ Knowledge Base
    /// ออกแบบตาม KMS_Wireframe_UI_v2.html / screen-kms03
    /// ฝังใน panelMainRight ของ KnowledgeBaseForm (TopLevel = false)
    /// </summary>
    public partial class ArticleViewerForm : Form, IArticleViewerView
    {
        private ArticleViewerPresenter _presenter;

        /// <summary>
        /// Factory injected by the composition root to open ArticleEditorDialog
        /// pre-wired with its own presenter. Returns DialogResult.OK if saved.
        /// </summary>
        private Func<KBArticle, DialogResult> _openEditorFactory;

        /// <summary>Local display copy of the article — set by PopulateArticle().</summary>
        private KBArticle _article;

        /// <summary>Fired when the user clicks "← กลับ"; parent form handles navigation.</summary>
        public event EventHandler BackRequested;

        public ArticleViewerForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called by the composition root after the presenter is wired.
        /// Must be called before the form is shown.
        /// </summary>
        public void AttachPresenter(ArticleViewerPresenter presenter)
        {
            if (presenter == null) throw new ArgumentNullException("presenter");
            _presenter = presenter;
        }

        /// <summary>
        /// Inject the factory used to open a pre-wired ArticleEditorDialog.
        /// Must be called before the form is shown.
        /// </summary>
        public void AttachEditorFactory(Func<KBArticle, DialogResult> factory)
        {
            _openEditorFactory = factory;
        }

        // ── Form events ──────────────────────────────────────────────────────

        private void ArticleViewerForm_Load(object sender, EventArgs e)
        {
            BuildSidebarSections();
            AdjustSplitter();
            splitViewer.Resize += (s, ev) => AdjustSplitter();
            _presenter.Initialize();
        }

        private void AdjustSplitter()
        {
            int available = splitViewer.Width
                            - splitViewer.Panel2MinSize
                            - splitViewer.SplitterWidth;
            if (available >= splitViewer.Panel1MinSize)
                splitViewer.SplitterDistance = available;
        }

        // ── IArticleViewerView ───────────────────────────────────────────────

        public void PopulateArticle(KBArticle article)
        {
            if (article == null) throw new ArgumentNullException("article");
            _article = article;
            PopulateHeader();
            PopulateInfoBox();
            BeginInvoke(new Action(LoadContent));
        }

        public void BindVersionHistory(IEnumerable<KBVersion> versions)
        {
            pnlVersionList.SuspendLayout();
            pnlVersionList.Controls.Clear();

            var list = versions.ToList();
            if (list.Count == 0)
            {
                pnlVersionList.Controls.Add(new Label
                {
                    Dock      = DockStyle.Top,
                    Height    = 24,
                    Font      = new Font("Segoe UI", 8f),
                    ForeColor = Color.FromArgb(160, 160, 160),
                    Padding   = new Padding(4, 4, 0, 0),
                    Text      = "ยังไม่มี Version History",
                });
            }
            else
            {
                // DockStyle.Top: add in reverse so newest version appears at top
                for (int i = Math.Min(list.Count, 8) - 1; i >= 0; i--)
                {
                    var v   = list[i];
                    var row = new Panel { Dock = DockStyle.Top, Height = 20, BackColor = Color.White };
                    var sep = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(238, 238, 238) };
                    var lbl = new Label
                    {
                        Dock      = DockStyle.Fill,
                        Font      = new Font("Segoe UI", 8f),
                        ForeColor = Color.FromArgb(85, 85, 85),
                        Padding   = new Padding(4, 1, 4, 1),
                        Text      = string.Format("v{0}  —  {1}", v.VersionNumber, v.CreatedDate.ToString("dd MMM yy")),
                    };
                    row.Controls.Add(lbl);
                    row.Controls.Add(sep);
                    pnlVersionList.Controls.Add(row);
                }
            }
            pnlVersionList.ResumeLayout();
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowInfo(string message)
        {
            MessageBox.Show(message, "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        // ── Button handlers ──────────────────────────────────────────────────

        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (BackRequested != null)
                BackRequested(this, EventArgs.Empty);
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            if (_openEditorFactory == null || _article == null) return;
            if (_openEditorFactory(_article) == DialogResult.OK)
                _presenter.RefreshAfterEdit();
        }

        private void BtnShare_Click(object sender, EventArgs e)
        {
            if (_article == null) return;
            Clipboard.SetText(string.Format("{0}  [{1}]", _article.Title, _article.ArticleCode));
            ShowInfo("คัดลอกข้อมูลบทความไปยัง Clipboard แล้ว");
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            try { webContent.Print(); }
            catch { ShowInfo("ไม่สามารถพิมพ์ได้"); }
        }

        private void BtnExportPdf_Click(object sender, EventArgs e)
        {
            ShowInfo("ฟังก์ชัน Export PDF อยู่ระหว่างการพัฒนา");
        }

        private void BtnPin_Click(object sender, EventArgs e)
        {
            ShowInfo("ฟังก์ชัน Pin Article อยู่ระหว่างการพัฒนา");
        }

        private void BtnShowAllHistory_Click(object sender, EventArgs e)
        {
            ShowInfo("ฟังก์ชัน Version History เต็มอยู่ระหว่างการพัฒนา");
        }

        private void webContent_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e) { }

        // ── Private UI helpers ───────────────────────────────────────────────

        private void PopulateHeader()
        {
            lblViewerTitle.Text = "  👁️ ดูบทความ: " + (_article.Title ?? "(ไม่มีชื่อ)");
        }

        private void PopulateInfoBox()
        {
            lblTypeBadge.Text      = _article.ArticleTypeName.ToUpper();
            lblTypeBadge.BackColor = GetTypeBadgeColor(_article.ArticleType);

            lblCategoryBadge.Text = _article.CategoryName ?? "-";

            Color statusFore, statusBack;
            GetStatusColors(_article.Status, out statusFore, out statusBack);
            lblStatusBadge.ForeColor = statusFore;
            lblStatusBadge.BackColor = statusBack;
            lblStatusBadge.Text      = GetStatusText(_article.Status);

            lblOfficialBadge.Visible = _article.IsOfficialPolicy;
            if (_article.IsOfficialPolicy && !flpBadges.Controls.Contains(lblOfficialBadge))
                flpBadges.Controls.Add(lblOfficialBadge);

            lblCodeVersion.Text = string.Format("รหัส: {0}  |  Version {1}",
                _article.ArticleCode ?? "-", _article.CurrentVersion);
            lblAuthorVal.Text    = "ผู้เขียน: " + (_article.CreatedBy ?? "-");
            lblCategoryVal.Text  = "หมวดหมู่: " + (_article.CategoryName ?? "-");
            lblPublishVal.Text   = _article.PublishedDate.HasValue
                ? "Publish: " + _article.PublishedDate.Value.ToString("dd MMM yyyy")
                : "Publish: -";
            lblConfidential.Text = "ระดับความลับ: -";
            lblReviewDueVal.Text = _article.ReviewDueDate.HasValue
                ? "Review Due: " + _article.ReviewDueDate.Value.ToString("dd MMM yyyy")
                : "Review Due: -";
        }

        private void LoadContent()
        {
            try
            {
                string body = _article.ContentHTML ?? string.Empty;
                string html = string.Format(
                    "<!DOCTYPE html><html><head><meta charset='utf-8'>" +
                    "<style>" +
                    "body{{font-family:'Segoe UI',Tahoma,sans-serif;font-size:13px;" +
                        "margin:12px 16px;color:#333;line-height:1.8}}" +
                    "h2{{font-size:16px;color:#1E3A5F;border-bottom:2px solid #2E74B5;" +
                        "padding-bottom:4px;margin:0 0 10px}}" +
                    "h3{{font-size:14px;color:#2E74B5;margin:10px 0 5px}}" +
                    "p{{margin-bottom:8px}}" +
                    "pre,code{{background:#f4f6f8;padding:3px 7px;border-radius:3px;" +
                        "font-family:Consolas,monospace;font-size:12px}}" +
                    "table{{border-collapse:collapse;width:100%;margin:8px 0}}" +
                    "td,th{{border:1px solid #ddd;padding:6px 10px;text-align:left}}" +
                    "th{{background:#2E74B5;color:#fff}}" +
                    "a{{color:#2E74B5}}" +
                    "</style></head><body>{0}</body></html>",
                    body);
                webContent.DocumentText = html;
            }
            catch
            {
                webContent.DocumentText =
                    "<p style='font-family:Segoe UI;color:#888'>ไม่สามารถแสดงเนื้อหาได้</p>";
            }
        }

        private void BuildSidebarSections()
        {
            pnlSidebar.SuspendLayout();

            // DockStyle.Top — add bottom sections first, top sections last
            pnlSidebar.Controls.Add(
                CreateSidebarSection("🕐  VERSION HISTORY", null, BuildVersionHistoryContent(), 190));
            pnlSidebar.Controls.Add(
                CreateSidebarSection("🔗  บทความเกี่ยวข้อง", new[] { "(อยู่ระหว่างการพัฒนา)" }, null, 52));
            pnlSidebar.Controls.Add(
                CreateSidebarSection("⚖️  เส้นงาน", new[] { "(อยู่ระหว่างการพัฒนา)" }, null, 52));
            pnlSidebar.Controls.Add(
                CreateSidebarSection("📎  ไฟล์แนบ", new[] { "(อยู่ระหว่างการพัฒนา)" }, null, 52));
            pnlSidebar.Controls.Add(
                CreateSidebarSection("📋  โปรแกรมที่เกี่ยวข้อง", new[] { "(อยู่ระหว่างการพัฒนา)" }, null, 52));

            pnlSidebar.ResumeLayout();
        }

        private Panel CreateSidebarSection(string header, string[] links, Control extraContent, int height)
        {
            var pnlSection = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = height,
                BackColor = Color.FromArgb(250, 250, 250),
                Padding   = new Padding(10, 8, 10, 6),
            };
            pnlSection.Controls.Add(new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 1,
                BackColor = Color.FromArgb(238, 238, 238),
            });

            if (links != null)
            {
                for (int i = links.Length - 1; i >= 0; i--)
                {
                    bool isPlaceholder = links[i].StartsWith("(");
                    pnlSection.Controls.Add(new Label
                    {
                        Dock      = DockStyle.Top,
                        Height    = 18,
                        AutoSize  = false,
                        Font      = new Font("Segoe UI", 8f),
                        ForeColor = isPlaceholder ? Color.FromArgb(170, 170, 170) : Color.FromArgb(46, 116, 181),
                        Text      = links[i],
                        Padding   = new Padding(2, 1, 0, 0),
                        Cursor    = isPlaceholder ? Cursors.Default : Cursors.Hand,
                    });
                }
            }

            if (extraContent != null) pnlSection.Controls.Add(extraContent);

            pnlSection.Controls.Add(new Label
            {
                Dock      = DockStyle.Top,
                Height    = 22,
                AutoSize  = false,
                Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 116, 181),
                Text      = header,
                TextAlign = ContentAlignment.MiddleLeft,
            });
            return pnlSection;
        }

        private Control BuildVersionHistoryContent()
        {
            var pnl = new Panel { Dock = DockStyle.Top, Height = 162, BackColor = Color.Transparent };
            var btnAll = new Button
            {
                Dock                    = DockStyle.Bottom,
                Height                  = 24,
                Text                    = "ดู History ทั้งหมด",
                Font                    = new Font("Segoe UI", 8f),
                BackColor               = Color.FromArgb(214, 228, 240),
                ForeColor               = Color.FromArgb(30, 58, 95),
                FlatStyle               = FlatStyle.Flat,
                Cursor                  = Cursors.Hand,
                UseVisualStyleBackColor = false,
            };
            btnAll.FlatAppearance.BorderColor = Color.FromArgb(46, 116, 181);
            btnAll.FlatAppearance.BorderSize  = 1;
            btnAll.Click += BtnShowAllHistory_Click;

            pnlVersionList.Dock       = DockStyle.Fill;
            pnlVersionList.BackColor  = Color.White;
            pnlVersionList.AutoScroll = true;

            pnl.Controls.Add(pnlVersionList);
            pnl.Controls.Add(btnAll);
            return pnl;
        }

        // ── Colour helpers ────────────────────────────────────────────────────

        private static Color GetTypeBadgeColor(byte articleType)
        {
            switch (articleType)
            {
                case 1:  return Color.FromArgb(231, 76, 60);
                case 2:  return Color.FromArgb(243, 156, 18);
                case 3:  return Color.FromArgb(142, 68, 173);
                case 4:  return Color.FromArgb(22, 160, 133);
                case 5:  return Color.FromArgb(46, 116, 181);
                default: return Color.FromArgb(100, 110, 130);
            }
        }

        private static void GetStatusColors(byte status, out Color fore, out Color back)
        {
            switch (status)
            {
                case 1:  fore = Color.FromArgb(192, 57, 43);  back = Color.FromArgb(253, 237, 236); break;
                case 2:  fore = Color.FromArgb(183, 119, 13); back = Color.FromArgb(254, 249, 231); break;
                case 3:  fore = Color.FromArgb(30, 132, 73);  back = Color.FromArgb(213, 245, 227); break;
                case 4:  fore = Color.FromArgb(127, 140, 141);back = Color.FromArgb(238, 238, 238); break;
                default: fore = Color.Gray;                   back = Color.FromArgb(238, 238, 238); break;
            }
        }

        private static string GetStatusText(byte status)
        {
            switch (status)
            {
                case 1:  return "Draft";
                case 2:  return "รอ Review";
                case 3:  return "✅ Published";
                case 4:  return "Archived";
                default: return "Unknown";
            }
        }
    }
}
