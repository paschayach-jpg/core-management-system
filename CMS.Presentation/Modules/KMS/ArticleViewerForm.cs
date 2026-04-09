using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// KMS-03 Article Viewer — แสดงรายละเอียดบทความ Knowledge Base
    /// ออกแบบตาม KMS_Wireframe_UI_v2.html / screen-kms03
    /// ฝังใน panelMainRight ของ KnowledgeBaseForm (TopLevel = false)
    /// </summary>
    public partial class ArticleViewerForm : Form
    {
        private readonly IKBCategoryService _categoryService;
        private readonly IKBArticleService  _articleService;
        private readonly UserContext        _user;
        private KBArticle                   _article;

        /// <summary>ยิงเมื่อผู้ใช้กด "← กลับ" ให้ Parent จัดการ navigation</summary>
        public event EventHandler BackRequested;

        public ArticleViewerForm(
            IKBCategoryService categoryService,
            IKBArticleService  articleService,
            UserContext        user,
            KBArticle          article)
        {
            if (categoryService == null) throw new ArgumentNullException("categoryService");
            if (articleService  == null) throw new ArgumentNullException("articleService");
            if (user            == null) throw new ArgumentNullException("user");
            if (article         == null) throw new ArgumentNullException("article");
            _categoryService = categoryService;
            _articleService  = articleService;
            _user            = user;
            _article         = article;
            InitializeComponent();
        }

        // ── Load ──────────────────────────────────────────────────────────────

        private void ArticleViewerForm_Load(object sender, EventArgs e)
        {
            PopulateHeader();
            PopulateInfoBox();
            BuildSidebarSections();
            LoadVersionHistory();
            // Adjust splitter once actual width is known, then wire resize
            AdjustSplitter();
            splitViewer.Resize += (s, ev) => AdjustSplitter();
            // Defer WebBrowser load until control handle is fully ready
            BeginInvoke(new Action(LoadContent));
        }

        private void AdjustSplitter()
        {
            int available = splitViewer.Width
                            - splitViewer.Panel2MinSize
                            - splitViewer.SplitterWidth;
            if (available >= splitViewer.Panel1MinSize)
                splitViewer.SplitterDistance = available;
        }

        // ── Header & toolbar ──────────────────────────────────────────────────

        private void PopulateHeader()
        {
            lblViewerTitle.Text = "  👁️ ดูบทความ: " + (_article.Title ?? "(ไม่มีชื่อ)");
        }

        // ── Info box (badges + metadata grid) ────────────────────────────────

        private void PopulateInfoBox()
        {
            // ── Type badge ────────────────────────────────────────────────────
            lblTypeBadge.Text      = _article.ArticleTypeName.ToUpper();
            lblTypeBadge.BackColor = GetTypeBadgeColor(_article.ArticleType);

            // ── Category badge ────────────────────────────────────────────────
            lblCategoryBadge.Text  = _article.CategoryName ?? "-";

            // ── Status badge ──────────────────────────────────────────────────
            Color statusFore, statusBack;
            GetStatusColors(_article.Status, out statusFore, out statusBack);
            lblStatusBadge.ForeColor = statusFore;
            lblStatusBadge.BackColor = statusBack;
            lblStatusBadge.Text      = GetStatusText(_article.Status);

            // ── Official policy (extra badge) ─────────────────────────────────
            if (_article.IsOfficialPolicy)
            {
                lblOfficialBadge.Visible = true;
                flpBadges.Controls.Add(lblOfficialBadge);
            }

            // ── Metadata 2-column grid ────────────────────────────────────────
            lblCodeVersion.Text  = string.Format("รหัส: {0}  |  Version {1}",
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

        // ── Article content (WebBrowser) ──────────────────────────────────────

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
                    ".info-box{{padding:10px 12px;border-radius:4px;margin:4px 0;" +
                        "font-size:12px;border-left:4px solid}}" +
                    ".info-box.info{{background:#EBF5FB;border-color:#4A9FD4;color:#1E3A5F}}" +
                    ".info-box.warning{{background:#FEF9E7;border-color:#F39C12;color:#7D5A00}}" +
                    ".info-box.danger{{background:#FDEDEC;border-color:#E74C3C;color:#922B21}}" +
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

        // ── Sidebar sections ──────────────────────────────────────────────────

        /// <summary>สร้าง sidebar sections แบบ dynamic — เรียกครั้งเดียวจาก Load</summary>
        private void BuildSidebarSections()
        {
            pnlSidebar.SuspendLayout();

            // DockStyle.Top: Controls added LAST appear at TOP
            // → add bottom sections first, top sections last

            // 5. Version History — added first → appears bottom
            pnlSidebar.Controls.Add(
                CreateSidebarSection("🕐  VERSION HISTORY", null,
                    BuildVersionHistoryContent(), 190));

            // 4. Related Articles
            pnlSidebar.Controls.Add(
                CreateSidebarSection("🔗  บทความเกี่ยวข้อง", null,
                    new[] { "(อยู่ระหว่างการพัฒนา)" }, 52));

            // 3. Workflows
            pnlSidebar.Controls.Add(
                CreateSidebarSection("⚖️  เส้นงาน", null,
                    new[] { "(อยู่ระหว่างการพัฒนา)" }, 52));

            // 2. Attachments
            pnlSidebar.Controls.Add(
                CreateSidebarSection("📎  ไฟล์แนบ", null,
                    new[] { "(อยู่ระหว่างการพัฒนา)" }, 52));

            // 1. Related Programs — added last → appears at TOP
            pnlSidebar.Controls.Add(
                CreateSidebarSection("📋  โปรแกรมที่เกี่ยวข้อง", null,
                    new[] { "(อยู่ระหว่างการพัฒนา)" }, 52));

            pnlSidebar.ResumeLayout();
        }

        private Panel CreateSidebarSection(string header, string[] links,
            Control extraContent, int height)
        {
            var pnlSection = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = height,
                BackColor = Color.FromArgb(250, 250, 250),
                Padding   = new Padding(10, 8, 10, 6),
            };

            // Bottom border line
            var pnlBorder = new Panel
            {
                Dock      = DockStyle.Bottom,
                Height    = 1,
                BackColor = Color.FromArgb(238, 238, 238),
            };
            pnlSection.Controls.Add(pnlBorder);

            // Section header label — added LAST with DockStyle.Top → appears first
            var lblHeader = new Label
            {
                Dock      = DockStyle.Top,
                Height    = 22,
                AutoSize  = false,
                Font      = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(46, 116, 181),    // #2E74B5
                Text      = header,
                TextAlign = ContentAlignment.MiddleLeft,
            };

            // Link labels (added in reverse so first appears at top)
            if (links != null)
            {
                for (int i = links.Length - 1; i >= 0; i--)
                {
                    bool isPlaceholder = links[i].StartsWith("(");
                    var lbl = new Label
                    {
                        Dock      = DockStyle.Top,
                        Height    = 18,
                        AutoSize  = false,
                        Font      = new Font("Segoe UI", 8f),
                        ForeColor = isPlaceholder
                            ? Color.FromArgb(170, 170, 170)
                            : Color.FromArgb(46, 116, 181),  // #2E74B5
                        Text      = links[i],
                        Padding   = new Padding(2, 1, 0, 0),
                        Cursor    = isPlaceholder ? Cursors.Default : Cursors.Hand,
                    };
                    pnlSection.Controls.Add(lbl);
                }
            }

            // Extra content (e.g., version history panel)
            if (extraContent != null)
                pnlSection.Controls.Add(extraContent);

            pnlSection.Controls.Add(lblHeader);
            return pnlSection;
        }

        private Panel CreateSidebarSection(string header, string[] links,
            string[] placeholders, int height)
        {
            return CreateSidebarSection(header, placeholders, (Control)null, height);
        }

        /// <summary>สร้าง content สำหรับ section Version History</summary>
        private Control BuildVersionHistoryContent()
        {
            var pnl = new Panel
            {
                Dock      = DockStyle.Top,
                Height    = 162,
                BackColor = Color.Transparent,
            };

            var btnAll = new Button
            {
                Dock                        = DockStyle.Bottom,
                Height                      = 24,
                Text                        = "ดู History ทั้งหมด",
                Font                        = new Font("Segoe UI", 8f),
                BackColor                   = Color.FromArgb(214, 228, 240),  // #D6E4F0
                ForeColor                   = Color.FromArgb(30, 58, 95),     // #1E3A5F
                FlatStyle                   = FlatStyle.Flat,
                Cursor                      = Cursors.Hand,
                UseVisualStyleBackColor     = false,
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

        // ── Version history data ──────────────────────────────────────────────

        private void LoadVersionHistory()
        {
            try
            {
                var versions = _articleService.GetVersionHistory(_article.ArticleID)
                                              .OrderByDescending(v => v.VersionNumber)
                                              .ToList();
                pnlVersionList.SuspendLayout();
                pnlVersionList.Controls.Clear();

                if (versions.Count == 0)
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
                    // DockStyle.Top: add in reverse so first version appears at top
                    for (int i = Math.Min(versions.Count, 8) - 1; i >= 0; i--)
                    {
                        var v   = versions[i];
                        var row = new Panel
                        {
                            Dock      = DockStyle.Top,
                            Height    = 20,
                            BackColor = Color.White,
                        };
                        var sep = new Panel
                        {
                            Dock      = DockStyle.Bottom,
                            Height    = 1,
                            BackColor = Color.FromArgb(238, 238, 238),
                        };
                        var lblRow = new Label
                        {
                            Dock      = DockStyle.Fill,
                            Font      = new Font("Segoe UI", 8f),
                            ForeColor = Color.FromArgb(85, 85, 85),
                            Padding   = new Padding(4, 1, 4, 1),
                            Text      = string.Format("v{0}  —  {1}",
                                v.VersionNumber, v.CreatedDate.ToString("dd MMM yy")),
                        };
                        row.Controls.Add(lblRow);
                        row.Controls.Add(sep);
                        pnlVersionList.Controls.Add(row);
                    }
                }
                pnlVersionList.ResumeLayout();
            }
            catch { /* non-critical */ }
        }

        // ── Colour helpers ────────────────────────────────────────────────────

        private static Color GetTypeBadgeColor(byte articleType)
        {
            switch (articleType)
            {
                case 1:  return Color.FromArgb(231, 76, 60);   // SOP       — #E74C3C
                case 2:  return Color.FromArgb(243, 156, 18);  // FAQ       — #F39C12
                case 3:  return Color.FromArgb(142, 68, 173);  // Guideline — #8E44AD
                case 4:  return Color.FromArgb(22, 160, 133);  // Template  — #16A085
                case 5:  return Color.FromArgb(46, 116, 181);  // Policy    — #2E74B5
                default: return Color.FromArgb(100, 110, 130);
            }
        }

        private static void GetStatusColors(byte status,
            out Color fore, out Color back)
        {
            switch (status)
            {
                case 1:                                         // Draft
                    fore = Color.FromArgb(192, 57, 43);
                    back = Color.FromArgb(253, 237, 236); break;
                case 2:                                         // Review
                    fore = Color.FromArgb(183, 119, 13);
                    back = Color.FromArgb(254, 249, 231); break;
                case 3:                                         // Published
                    fore = Color.FromArgb(30, 132, 73);
                    back = Color.FromArgb(213, 245, 227); break;
                case 4:                                         // Archived
                    fore = Color.FromArgb(127, 140, 141);
                    back = Color.FromArgb(238, 238, 238); break;
                default:
                    fore = Color.Gray;
                    back = Color.FromArgb(238, 238, 238); break;
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

        // ── Button handlers ───────────────────────────────────────────────────

        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (BackRequested != null)
                BackRequested(this, EventArgs.Empty);
        }

        private void BtnEdit_Click(object sender, EventArgs e)
        {
            using (var dlg = new ArticleEditorDialog(
                _categoryService, _articleService, _user, _article))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK)
                {
                    var updated = _articleService.GetById(_article.ArticleID);
                    if (updated != null)
                    {
                        _article = updated;
                        PopulateHeader();
                        PopulateInfoBox();
                        LoadContent();
                        LoadVersionHistory();
                    }
                }
            }
        }

        private void BtnShare_Click(object sender, EventArgs e)
        {
            Clipboard.SetText(string.Format("{0}  [{1}]",
                _article.Title, _article.ArticleCode));
            MessageBox.Show("คัดลอกข้อมูลบทความไปยัง Clipboard แล้ว",
                "แชร์บทความ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnPrint_Click(object sender, EventArgs e)
        {
            try { webContent.Print(); }
            catch
            {
                MessageBox.Show("ไม่สามารถพิมพ์ได้",
                    "พิมพ์", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void BtnExportPdf_Click(object sender, EventArgs e)
        {
            MessageBox.Show("ฟังก์ชัน Export PDF อยู่ระหว่างการพัฒนา",
                "Export PDF — MOD-006", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnPin_Click(object sender, EventArgs e)
        {
            MessageBox.Show("ฟังก์ชัน Pin Article อยู่ระหว่างการพัฒนา",
                "Pin Article", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void BtnShowAllHistory_Click(object sender, EventArgs e)
        {
            MessageBox.Show("ฟังก์ชัน Version History เต็มอยู่ระหว่างการพัฒนา",
                "Version History", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        private void webContent_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {

        }
    }
}
