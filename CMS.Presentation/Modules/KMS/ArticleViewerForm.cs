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
    /// KMS-03 Article Viewer — แสดงรายละเอียดบทความ Knowledge Base
    /// ออกแบบตาม KMS_Wireframe_UI_v2.html / screen-kms03
    /// ฝังใน panelMainRight ของ KBMainForm (TopLevel = false)
    /// </summary>
    public partial class ArticleViewerForm : Form, IArticleViewerController
    {
        private ArticleViewerController _presenter;

        /// <summary>
        /// Factory injected by the composition root to open KBEditorDialog
        /// pre-wired with its own presenter. Returns DialogResult.OK if saved.
        /// </summary>
        private Func<KBArticle, DialogResult> _openEditorFactory;

        /// <summary>Local display copy of the article — set by PopulateArticle().</summary>
        private KBArticle _article;

        /// <summary>
        /// The ArticleID of the article currently displayed.
        /// Used by KBMainForm to restore _activeViewerArticleId when popping the history stack.
        /// </summary>
        public int CurrentArticleId { get { return _article != null ? _article.ArticleID : 0; } }

        /// <summary>Related articles loaded from DB — populated by BindRelatedArticles().</summary>
        private IEnumerable<KBArticle> _relatedArticles = new List<KBArticle>();

        /// <summary>Fired when the user clicks "← กลับ"; parent form handles navigation.</summary>
        public event EventHandler BackRequested;

        /// <summary>
        /// Fired when the user clicks a related article item.
        /// The EventArgs carries the <see cref="KBArticle"/> to open.
        /// Parent form (KBMainForm) subscribes and calls ShowArticleViewer.
        /// </summary>
        public event EventHandler<KBArticle> ArticleOpenRequested;

        public ArticleViewerForm()
        {
            InitializeComponent();
        }

        /// <summary>
        /// Called by the composition root after the presenter is wired.
        /// Must be called before the form is shown.
        /// </summary>
        public void AttachPresenter(ArticleViewerController presenter)
        {
            if (presenter == null) throw new ArgumentNullException("presenter");
            _presenter = presenter;
        }

        /// <summary>
        /// Inject the factory used to open a pre-wired KBEditorDialog.
        /// Must be called before the form is shown.
        /// </summary>
        public void AttachEditorFactory(Func<KBArticle, DialogResult> factory)
        {
            _openEditorFactory = factory;
        }

        // ── Form events ──────────────────────────────────────────────────────

        private void ArticleViewerForm_Load(object sender, EventArgs e)
        {
            if (_presenter == null)
            {
                MessageBox.Show(
                    "Presenter ยังไม่ได้ถูก Attach\n\nกรุณาเรียก AttachPresenter() ก่อนเปิดฟอร์ม",
                    "ข้อผิดพลาดภายใน", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            AdjustSplitter();
            splitViewer.Resize += (s, ev) => AdjustSplitter();
            _presenter.Initialize();
            
            // Initialize with Attachments tab
            SwitchToTab(btnTabAttach);
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
            // Version history is no longer displayed in the new tabbed design
            // This method is kept for interface compatibility
        }

        public void BindRelatedArticles(IEnumerable<KBArticle> articles)
        {
            _relatedArticles = articles ?? new List<KBArticle>();

            // If the Related tab is currently active, refresh it immediately
            if (btnTabRelated.BackColor == Color.White)
                LoadRelatedTab();
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
            if (_presenter == null) return;
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

        private void BtnTab_Click(object sender, EventArgs e)
        {
            if (sender is Button btn)
                SwitchToTab(btn);
        }

        private void webContent_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            // Attach click handler to intercept link clicks
            if (webContent.Document != null)
            {
                webContent.Document.Click -= WebContent_Document_Click;
                webContent.Document.Click += WebContent_Document_Click;
            }
        }

        private void WebContent_Document_Click(object sender, HtmlElementEventArgs e)
        {
            // Check if the clicked element is an anchor tag
            var element = webContent.Document.GetElementFromPoint(e.ClientMousePosition);
            if (element == null) return;

            // Walk up the DOM tree to find an anchor tag
            while (element != null && element.TagName.ToLower() != "a")
                element = element.Parent;

            if (element == null || element.TagName.ToLower() != "a") return;

            // Get the href attribute
            string href = element.GetAttribute("href");
            if (string.IsNullOrWhiteSpace(href)) return;

            // Only handle external links (http:// or https://)
            if (href.StartsWith("http://", StringComparison.OrdinalIgnoreCase) ||
                href.StartsWith("https://", StringComparison.OrdinalIgnoreCase))
            {
                e.ReturnValue = false; // Cancel navigation in WebBrowser
                try
                {
                    System.Diagnostics.Process.Start(href);
                }
                catch (Exception ex)
                {
                    ShowError("ไม่สามารถเปิดลิงก์ได้: " + ex.Message);
                }
            }
        }

        // ── Private UI helpers ───────────────────────────────────────────────

        private void SwitchToTab(Button activeBtn)
        {
            // Reset all tab buttons
            foreach (Control ctrl in pnlTabHeaders.Controls)
            {
                if (ctrl is Button btn)
                {
                    btn.BackColor = Color.FromArgb(240, 244, 248);
                    btn.ForeColor = Color.FromArgb(85, 85, 85);
                    btn.Font = new Font("Segoe UI", 8.5f, FontStyle.Regular);
                }
            }

            // Highlight active tab
            activeBtn.BackColor = Color.White;
            activeBtn.ForeColor = Color.FromArgb(46, 116, 181);
            activeBtn.Font = new Font("Segoe UI", 8.5f, FontStyle.Bold);

            // Load content based on tab
            if (activeBtn == btnTabAttach)
                LoadAttachmentsTab();
            else if (activeBtn == btnTabRelated)
                LoadRelatedTab();
            else if (activeBtn == btnTabWorkflow)
                LoadWorkflowTab();
            else if (activeBtn == btnTabProgram)
                LoadProgramTab();
            else if (activeBtn == btnTabDoc)
                LoadDocumentTab();
        }

        private void LoadAttachmentsTab()
        {
            pnlTabContent.SuspendLayout();
            pnlTabContent.Controls.Clear();

            // Sample attachment 1
            var attach1 = CreateAttachmentItem(
                "📄", "Claims_Form_2569.pdf", "245 KB", "3NC", false);
            attach1.Dock = DockStyle.Top;
            pnlTabContent.Controls.Add(attach1);

            // Sample attachment 2 (locked)
            var attach2 = CreateAttachmentItem(
                "📊", "UW_Criteria_Table.xlsx", "89 KB", "1HC", true);
            attach2.Dock = DockStyle.Top;
            pnlTabContent.Controls.Add(attach2);

            pnlTabContent.ResumeLayout();
        }

        private Panel CreateAttachmentItem(string icon, string fileName, string size, string craf, bool locked)
        {
            var pnl = new Panel
            {
                Height = 48,
                BackColor = locked ? Color.FromArgb(254, 249, 231) : Color.White,
                Padding = new Padding(5, 4, 5, 4),
                Margin = new Padding(0, 0, 0, 6),
            };

            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 14f),
                Width = 32,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleCenter,
            };

            var pnlInfo = new Panel { Dock = DockStyle.Fill };
            var lblName = new Label
            {
                Text = fileName,
                Font = new Font("Segoe UI", 9f, locked ? FontStyle.Regular : FontStyle.Bold),
                ForeColor = locked ? Color.FromArgb(160, 160, 160) : Color.FromArgb(46, 116, 181),
                Dock = DockStyle.Top,
                Height = 20,
                Cursor = locked ? Cursors.Default : Cursors.Hand,
            };
            if (!locked)
                lblName.Click += (s, e) => ShowInfo($"เปิด {fileName}");

            var lblMeta = new Label
            {
                Text = $"{size}  ",
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(120, 120, 120),
                Dock = DockStyle.Top,
                Height = 18,
            };

            var lblBadge = new Label
            {
                Text = craf,
                Font = new Font("Segoe UI", 7f, FontStyle.Bold),
                ForeColor = craf == "1HC" ? Color.FromArgb(192, 57, 43) : Color.FromArgb(22, 160, 133),
                BackColor = craf == "1HC" ? Color.FromArgb(253, 237, 236) : Color.FromArgb(213, 245, 227),
                AutoSize = true,
                Padding = new Padding(4, 1, 4, 1),
                Location = new Point(lblMeta.Text.Length * 6, 0),
            };
            lblMeta.Controls.Add(lblBadge);

            pnlInfo.Controls.Add(lblMeta);
            pnlInfo.Controls.Add(lblName);

            if (locked)
            {
                var lblLocked = new Label
                {
                    Text = "🔒 ต้องสิทธิ์ " + craf,
                    Font = new Font("Segoe UI", 7.5f),
                    ForeColor = Color.FromArgb(183, 119, 13),
                    Dock = DockStyle.Right,
                    Width = 90,
                    TextAlign = ContentAlignment.MiddleRight,
                };
                pnl.Controls.Add(lblLocked);
            }
            else
            {
                var pnlButtons = new Panel { Dock = DockStyle.Right, Width = 50 };
                var btnView = new Button
                {
                    Text = "👁️ ดู",
                    Font = new Font("Segoe UI", 7.5f),
                    Height = 18,
                    Dock = DockStyle.Top,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(214, 228, 240),
                    Cursor = Cursors.Hand,
                };
                btnView.FlatAppearance.BorderColor = Color.FromArgb(170, 170, 170);
                btnView.Click += (s, e) => ShowInfo($"กำลังเปิด {fileName}");

                var btnDownload = new Button
                {
                    Text = "📥",
                    Font = new Font("Segoe UI", 7.5f),
                    Height = 18,
                    Dock = DockStyle.Top,
                    FlatStyle = FlatStyle.Flat,
                    BackColor = Color.FromArgb(214, 228, 240),
                    Cursor = Cursors.Hand,
                    Margin = new Padding(0, 2, 0, 0),
                };
                btnDownload.FlatAppearance.BorderColor = Color.FromArgb(170, 170, 170);
                btnDownload.Click += (s, e) => ShowInfo($"กำลังดาวน์โหลด {fileName}");

                pnlButtons.Controls.Add(btnDownload);
                pnlButtons.Controls.Add(btnView);
                pnl.Controls.Add(pnlButtons);
            }

            pnl.Controls.Add(pnlInfo);
            pnl.Controls.Add(lblIcon);

            var sep = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(238, 238, 238) };
            pnl.Controls.Add(sep);

            return pnl;
        }

        private void LoadRelatedTab()
        {
            pnlTabContent.SuspendLayout();
            pnlTabContent.Controls.Clear();

            var articles = _relatedArticles.ToList();

            if (!articles.Any())
            {
                var lblEmpty = new Label
                {
                    Text      = "ไม่มีบทความที่เชื่อมโยง",
                    Font      = new Font("Segoe UI", 9f),
                    ForeColor = Color.FromArgb(150, 150, 150),
                    Dock      = DockStyle.Top,
                    Height    = 40,
                    TextAlign = ContentAlignment.MiddleCenter,
                };
                pnlTabContent.Controls.Add(lblEmpty);
                pnlTabContent.ResumeLayout();
                return;
            }

            // Render newest-first so the list matches insertion order from the editor
            foreach (var article in articles)
            {
                var pnl = CreateRelatedItem(article);
                pnl.Dock = DockStyle.Top;
                pnlTabContent.Controls.Add(pnl);
            }

            pnlTabContent.ResumeLayout();
        }

        private Panel CreateRelatedItem(KBArticle article)
        {
            var pnl = new Panel
            {
                Height      = 52,
                BackColor   = Color.White,
                Padding     = new Padding(5, 4, 5, 4),
                Cursor      = Cursors.Hand,
            };

            var lblDot = new Label
            {
                Text      = "●",
                Font      = new Font("Segoe UI", 10f),
                ForeColor = Color.FromArgb(46, 116, 181),
                Width     = 20,
                Dock      = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleCenter,
            };

            var pnlInfo = new Panel { Dock = DockStyle.Fill };

            var lblTitle = new Label
            {
                Text      = article.Title ?? "(ไม่มีชื่อ)",
                Font      = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(46, 116, 181),
                Dock      = DockStyle.Top,
                Height    = 20,
            };

            var lblMeta = new Label
            {
                Text      = string.Format("{0}  |  {1}  |  v{2}",
                    article.ArticleCode ?? "-",
                    article.ArticleTypeName ?? "-",
                    article.CurrentVersion),
                Font      = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(120, 120, 120),
                Dock      = DockStyle.Top,
                Height    = 16,
            };

            pnlInfo.Controls.Add(lblMeta);
            pnlInfo.Controls.Add(lblTitle);
            pnl.Controls.Add(pnlInfo);
            pnl.Controls.Add(lblDot);

            // Capture for lambda
            var captured = article;
            EventHandler openHandler = (s, e) =>
            {
                if (ArticleOpenRequested != null)
                    ArticleOpenRequested(this, captured);
            };
            pnl.Click      += openHandler;
            lblTitle.Click += openHandler;
            lblMeta.Click  += openHandler;

            var sep = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(238, 238, 238) };
            pnl.Controls.Add(sep);

            return pnl;
        }

        private void LoadWorkflowTab()
        {
            pnlTabContent.SuspendLayout();
            pnlTabContent.Controls.Clear();

            var workflows = new[]
            {
                ("การรับแจ้งอุบัติเหตุ (RG01000)", "5 Step | v2.1", "Active"),
                ("การออกสำรวจภัย (RG02000)", "8 Step | v1.5", "Active"),
            };

            foreach (var wf in workflows)
            {
                var pnl = CreateWorkflowItem(wf.Item1, wf.Item2, wf.Item3);
                pnl.Dock = DockStyle.Top;
                pnlTabContent.Controls.Add(pnl);
            }

            var btnStart = new Button
            {
                Text = "▶️ เริ่ม Workflow",
                Font = new Font("Segoe UI", 8.5f),
                Height = 32,
                Dock = DockStyle.Top,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(46, 116, 181),
                ForeColor = Color.White,
                Cursor = Cursors.Hand,
                Margin = new Padding(0, 8, 0, 0),
            };
            btnStart.FlatAppearance.BorderColor = Color.FromArgb(30, 58, 95);
            btnStart.Click += (s, e) => ShowInfo("เปิด Workflow Module");
            pnlTabContent.Controls.Add(btnStart);

            pnlTabContent.ResumeLayout();
        }

        private Panel CreateWorkflowItem(string name, string meta, string status)
        {
            var pnl = new Panel
            {
                Height = 48,
                BackColor = Color.White,
                Padding = new Padding(5, 4, 5, 4),
            };

            var lblIcon = new Label
            {
                Text = "⚖️",
                Font = new Font("Segoe UI", 14f),
                Width = 32,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleCenter,
            };

            var pnlInfo = new Panel { Dock = DockStyle.Fill };
            var lblName = new Label
            {
                Text = name,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(85, 85, 85),
                Dock = DockStyle.Top,
                Height = 20,
            };

            var lblMeta = new Label
            {
                Text = meta + "  ",
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(120, 120, 120),
                Dock = DockStyle.Top,
                Height = 18,
            };

            var lblStatus = new Label
            {
                Text = status,
                Font = new Font("Segoe UI", 7f, FontStyle.Bold),
                ForeColor = Color.FromArgb(30, 132, 73),
                BackColor = Color.FromArgb(213, 245, 227),
                AutoSize = true,
                Padding = new Padding(4, 1, 4, 1),
                Location = new Point(lblMeta.Text.Length * 6, 0),
            };
            lblMeta.Controls.Add(lblStatus);

            pnlInfo.Controls.Add(lblMeta);
            pnlInfo.Controls.Add(lblName);
            pnl.Controls.Add(pnlInfo);
            pnl.Controls.Add(lblIcon);

            var sep = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(238, 238, 238) };
            pnl.Controls.Add(sep);

            return pnl;
        }

        private void LoadProgramTab()
        {
            pnlTabContent.SuspendLayout();
            pnlTabContent.Controls.Clear();

            var programs = new[]
            {
                ("RISC Claim", "ฝ่ายสินไหมทดแทน"),
                ("ทะเบียนสินไหม", "ฝ่ายสินไหมทดแทน"),
                ("New E-Filing", "IT"),
                ("BIS Claim", "ฝ่ายสินไหมทดแทน"),
                ("TFRS17", "แผนกบัญชี"),
            };

            foreach (var prog in programs)
            {
                var pnl = CreateProgramItem(prog.Item1, prog.Item2);
                pnl.Dock = DockStyle.Top;
                pnlTabContent.Controls.Add(pnl);
            }

            pnlTabContent.ResumeLayout();
        }

        private Panel CreateProgramItem(string name, string dept)
        {
            var pnl = new Panel
            {
                Height = 42,
                BackColor = Color.White,
                Padding = new Padding(5, 4, 5, 4),
            };

            var lblIcon = new Label
            {
                Text = "💻",
                Font = new Font("Segoe UI", 12f),
                Width = 28,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleCenter,
            };

            var pnlInfo = new Panel { Dock = DockStyle.Fill };
            var lblName = new Label
            {
                Text = name,
                Font = new Font("Segoe UI", 8.5f, FontStyle.Bold),
                ForeColor = Color.FromArgb(85, 85, 85),
                Dock = DockStyle.Top,
                Height = 18,
            };
            var lblDept = new Label
            {
                Text = dept,
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(120, 120, 120),
                Dock = DockStyle.Top,
                Height = 16,
            };

            pnlInfo.Controls.Add(lblDept);
            pnlInfo.Controls.Add(lblName);

            var btnOpen = new Button
            {
                Text = "เปิด",
                Font = new Font("Segoe UI", 7.5f),
                Width = 50,
                Height = 18,
                Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(214, 228, 240),
                Cursor = Cursors.Hand,
            };
            btnOpen.FlatAppearance.BorderColor = Color.FromArgb(170, 170, 170);
            btnOpen.Click += (s, e) => ShowInfo($"เปิด {name}");

            pnl.Controls.Add(btnOpen);
            pnl.Controls.Add(pnlInfo);
            pnl.Controls.Add(lblIcon);

            var sep = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(238, 238, 238) };
            pnl.Controls.Add(sep);

            return pnl;
        }

        private void LoadDocumentTab()
        {
            pnlTabContent.SuspendLayout();
            pnlTabContent.Controls.Clear();

            var docs = new[]
            {
                ("☑️", "FM-CLM-001 แบบฟอร์มรับแจ้งสินไหม", "Rev.5", "3NC"),
                ("☐", "FM-CLM-002 แบบฟอร์มประเมินความเสียหาย", "Rev.3", "3NC"),
            };

            foreach (var doc in docs)
            {
                var pnl = CreateDocumentItem(doc.Item1, doc.Item2, doc.Item3, doc.Item4);
                pnl.Dock = DockStyle.Top;
                pnlTabContent.Controls.Add(pnl);
            }

            pnlTabContent.ResumeLayout();
        }

        private Panel CreateDocumentItem(string icon, string name, string rev, string craf)
        {
            var pnl = new Panel
            {
                Height = 42,
                BackColor = Color.White,
                Padding = new Padding(5, 4, 5, 4),
            };

            var lblIcon = new Label
            {
                Text = icon,
                Font = new Font("Segoe UI", 12f),
                Width = 28,
                Dock = DockStyle.Left,
                TextAlign = ContentAlignment.MiddleCenter,
            };

            var pnlInfo = new Panel { Dock = DockStyle.Fill };
            var lblName = new Label
            {
                Text = name,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(85, 85, 85),
                Dock = DockStyle.Top,
                Height = 18,
            };

            var lblMeta = new Label
            {
                Text = rev + "  ",
                Font = new Font("Segoe UI", 7.5f),
                ForeColor = Color.FromArgb(120, 120, 120),
                Dock = DockStyle.Top,
                Height = 16,
            };

            var lblBadge = new Label
            {
                Text = craf,
                Font = new Font("Segoe UI", 7f, FontStyle.Bold),
                ForeColor = Color.FromArgb(22, 160, 133),
                BackColor = Color.FromArgb(213, 245, 227),
                AutoSize = true,
                Padding = new Padding(4, 1, 4, 1),
                Location = new Point(lblMeta.Text.Length * 6, 0),
            };
            lblMeta.Controls.Add(lblBadge);

            pnlInfo.Controls.Add(lblMeta);
            pnlInfo.Controls.Add(lblName);

            var btnDownload = new Button
            {
                Text = "📥",
                Font = new Font("Segoe UI", 7.5f),
                Width = 50,
                Height = 18,
                Dock = DockStyle.Right,
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(214, 228, 240),
                Cursor = Cursors.Hand,
            };
            btnDownload.FlatAppearance.BorderColor = Color.FromArgb(170, 170, 170);
            btnDownload.Click += (s, e) => ShowInfo("ดาวน์โหลดแบบฟอร์ม");

            pnl.Controls.Add(btnDownload);
            pnl.Controls.Add(pnlInfo);
            pnl.Controls.Add(lblIcon);

            var sep = new Panel { Dock = DockStyle.Bottom, Height = 1, BackColor = Color.FromArgb(238, 238, 238) };
            pnl.Controls.Add(sep);

            return pnl;
        }

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
