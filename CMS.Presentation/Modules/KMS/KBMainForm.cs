// ✅ Allowed usings: CMS.Controllers, CMS.Domain
// ❌ Forbidden usings: CMS.Services, CMS.Data
using CMS.Controllers.KMS.KB;
using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Domain.Models;
using CMS.Presentation.Services;
using CMS.Services;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Drawing;
using System.Globalization;
using System.Linq;
using System.Windows.Forms;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// หน้าหลัก Knowledge Base System (MOD-006) - KBMain
    /// Layout: SplitContainer — ซ้าย=Category TreeView, ขวา=Article List + Search
    /// รองรับ: Browse by Category, Full-text Search, Add/Edit/Archive Article, Version History
    /// </summary>
    public partial class KBMainForm : Form, IKBMainController
    {
        private KBMainController _presenter;
        private UserContext _user;

        // ── Viewer state ────────────────────────────────────────────────────────
        private ArticleViewerForm _activeViewer          = null;
        private int?              _activeViewerArticleId = null;

        /// <summary>
        /// Navigation history stack — each entry is a viewer that was pushed aside
        /// when the user clicked a related article. Popped on btnBack.
        /// </summary>
        private readonly Stack<ArticleViewerForm> _viewerHistory = new Stack<ArticleViewerForm>();

        private int _currentPage = 1;

        // ── Article cache for grid / card dual-render ────────────────────────────
        private List<KBArticle> _cachedArticles    = new List<KBArticle>();
        private bool            _tab2NeedsRefresh  = true;

        /// <summary>
        /// Header label + การซ่อนแสดงคอลัมน์ของ dgvArticles/ArticleCardControl ที่ resolve แล้วสำหรับหมวด
        /// ที่กำลังดูอยู่ (ตั้งค่าโดย ApplyColumnDisplay) — เริ่มต้นเป็นค่า default ตายตัวเดิมก่อนมีการเลือกหมวดใดๆ
        /// </summary>
        private IDictionary<string, KBColumnDisplaySetting> _currentColumnDisplay = KBArticleDisplaySlotCatalog.GetAllDefaults();

        // ── Suppress AfterSelect during programmatic tree population ─────────────
        private bool _suppressCategorySelect = false;

        // ── Category tree search (txtCategorySearch) ──────────────────────────────
        // ค้นหาเฉพาะตอนกด Enter: เจอ → แสดงเฉพาะหมวดที่เจอ (+สายแม่) เป็นสำเนาโหนดใน treeCategoryResults แล้วซ่อน treeCategories ไว้
        // (ต้นไม้เต็มไม่ถูกถอด/สร้างใหม่ จึงสลับกลับได้ทันทีแม้มีโหนดหลายหมื่นตัว และคงสถานะขยาย/ยุบ/การเลือกเดิมไว้)
        private readonly List<TreeNode> _categorySearchMatches = new List<TreeNode>();   // โหนดที่ตรงใน treeCategoryResults ตามลำดับที่เห็นบนจอ
        private int    _categorySearchCurrent     = -1;                                  // ตัวที่ชี้อยู่ (Enter / Shift+Enter เลื่อน)
        private string _categorySearchAppliedTerm = string.Empty;                        // คำค้นที่กด Enter ไปแล้ว
        private bool   _categorySearchActive;                                            // true = กำลังแสดง treeCategoryResults แทน treeCategories

        /// <summary>ต้นไม้ที่กำลังแสดงอยู่ (ต้นไม้เต็ม หรือผลค้นหา) — ปุ่มขยาย/ยุบทำงานกับอันนี้</summary>
        private TreeView ActiveCategoryTree { get { return _categorySearchActive ? treeCategoryResults : treeCategories; } }
        private static readonly Color CategoryMatchColor   = Color.FromArgb(255, 241, 160);
        private static readonly Color CategoryCurrentColor = Color.FromArgb(255, 196, 90);
        private static readonly Color CategoryNotFoundColor = Color.FromArgb(255, 225, 225);

        [System.Runtime.InteropServices.DllImport("user32.dll", CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private static extern IntPtr SendMessage(IntPtr hWnd, int msg, IntPtr wParam, string lParam);
        private const int EM_SETCUEBANNER = 0x1501;

        // ── Archived mode state ───────────────────────────────────────────────────
        private bool _isArchivedMode         = false;
        private int  _pendingToggleArticleId = 0;

        // ── Category settings embedded panel state ───────────────────────────────
        private KBCategoryForm _activeCategoryForm = null;

        // ── Editor embedded panel state ───────────────────────────────────────────
        private KBEditorDialog _activeEditorForm = null;

        // ── Import wizard embedded panel state ───────────────────────────────────
        private KBImportWizardForm _activeImportForm = null;

        // ── Loading overlay ────────────────────────────────────────────────────────
        private CMS.Presentation.UserControls.LoadingOverlayControl _loadingOverlay;

        // ── Dialog / child-form factories (injected by composition root) ────────
        /// <summary>Creates and returns a pre-wired ArticleViewerForm for a given article.</summary>
        private Func<KBArticle, ArticleViewerForm> _viewerFactory;
        /// <summary>Creates and returns a pre-wired KBEditorDialog ready to be embedded.</summary>
        private Func<KBArticle, KBEditorDialog> _editorFactory;
        /// <summary>Creates and returns a pre-wired KBCategoryForm ready to be embedded.</summary>
        private Func<KBCategoryForm> _categoryFormFactory;
        /// <summary>Creates and returns a pre-wired KBImportWizardForm ready to be embedded.</summary>
        private Func<KBImportWizardForm> _importWizardFactory;
        /// <summary>Creates, wires, and shows a pre-wired KBDashboardForm. Returns DialogResult.</summary>
        private Func<DialogResult> _dashboardFactory;
        /// <summary>Creates, wires, and shows a pre-wired KBImportTemplateConfigForm. Returns DialogResult.</summary>
        private Func<DialogResult> _importTemplateConfigFactory;
        /// <summary>Creates, wires, and shows a pre-wired KBArticleDisplayProfileConfigForm. Returns DialogResult.</summary>
        private Func<DialogResult> _displayProfileConfigFactory;


        public KBMainForm()
        {
            InitializeComponent();
            InitializeArticleGrid();
            treeCategoryResults.BeforeExpand += TreeCategoryResults_BeforeExpand;

            // pnlViewerHost is created here so the designer stays free of a Dock=Fill panel
            // that would block adding other controls. Runtime behaviour is identical.
            // ทำหน้าที่ เป็น Panel ที่ทำหน้าที่เป็น "full-screen layer" สำหรับแสดง ArticleViewerForm (หน้าอ่านบทความ) ทับบน splitMain
            pnlViewerHost = new System.Windows.Forms.Panel
            {
                Name      = "pnlViewerHost",
                BackColor = System.Drawing.Color.White,
                Dock      = System.Windows.Forms.DockStyle.Fill,
                Visible   = false,
            };
            Controls.Add(pnlViewerHost);
            pnlViewerHost.BringToFront();

            // pnlCategoryHost hosts the embedded KBCategoryForm (same pattern as pnlViewerHost)
            pnlCategoryHost = new System.Windows.Forms.Panel
            {
                Name      = "pnlCategoryHost",
                BackColor = System.Drawing.Color.White,
                Dock      = System.Windows.Forms.DockStyle.Fill,
                Visible   = false,
            };
            Controls.Add(pnlCategoryHost);
            pnlCategoryHost.BringToFront();

            // pnlEditorHost hosts the embedded KBEditorDialog (same pattern as pnlCategoryHost)
            pnlEditorHost = new System.Windows.Forms.Panel
            {
                Name      = "pnlEditorHost",
                BackColor = System.Drawing.Color.White,
                Dock      = System.Windows.Forms.DockStyle.Fill,
                Visible   = false,
            };
            Controls.Add(pnlEditorHost);
            pnlEditorHost.BringToFront();

            // pnlImportHost hosts the embedded KBImportWizardForm (same pattern as pnlCategoryHost)
            pnlImportHost = new System.Windows.Forms.Panel
            {
                Name      = "pnlImportHost",
                BackColor = System.Drawing.Color.White,
                Dock      = System.Windows.Forms.DockStyle.Fill,
                Visible   = false,
            };
            Controls.Add(pnlImportHost);
            pnlImportHost.BringToFront();

            splitMain.BringToFront();          // restore splitMain as the visible default layer

            // LoadingOverlayControl — sits in splitMain.Panel2 over panelMainRight.
            // Dock=None keeps it from disturbing panelMainRight's Dock=Fill layout.
            _loadingOverlay = new CMS.Presentation.UserControls.LoadingOverlayControl
            {
                Dock = System.Windows.Forms.DockStyle.None,
            };
            splitMain.Panel2.Controls.Add(_loadingOverlay);

            // Keep overlay sized to match the right panel whenever it resizes
            splitMain.Panel2.SizeChanged += (s, ev) =>
            {
                if (_loadingOverlay.Visible)
                    _loadingOverlay.Bounds = splitMain.Panel2.ClientRectangle;
            };
        }

        /// <summary>
        /// Called by the composition root after the presenter is wired.
        /// Must be called before the form is shown.
        /// </summary>
        public void AttachPresenter(KBMainController presenter, UserContext user)
        {
            if (presenter == null) throw new ArgumentNullException("presenter");
            if (user == null) throw new ArgumentNullException("user");
            _presenter = presenter;
            _user = user;
        }

        /// <summary>
        /// Inject the factories used to open child forms.
        /// Must be called before the form is shown.
        /// </summary>
        public void AttachFactories(
            Func<KBArticle, ArticleViewerForm> viewerFactory,
            Func<KBArticle, KBEditorDialog>    editorFactory,
            Func<KBCategoryForm>               categoryFormFactory)
        {
            _viewerFactory       = viewerFactory;
            _editorFactory       = editorFactory;
            _categoryFormFactory = categoryFormFactory;
        }

        /// <summary>
        /// Inject the factory used to create and show the KB Import Wizard with its
        /// pre-wired presenter. Must be called before the form is shown.
        /// </summary>
        public void AttachImportWizardFactory(Func<KBImportWizardForm> factory)
        {
            _importWizardFactory = factory;
        }

        /// <summary>
        /// Inject the factory used to create and show the KB Dashboard with its
        /// pre-wired presenter. Must be called before the form is shown.
        /// </summary>
        public void AttachDashboardFactory(Func<DialogResult> factory)
        {
            _dashboardFactory = factory;
        }

        /// <summary>
        /// Inject the factory used to create and show the Excel Import Template Config screen
        /// with its pre-wired presenter. Must be called before the form is shown.
        /// </summary>
        public void AttachImportTemplateConfigFactory(Func<DialogResult> factory)
        {
            _importTemplateConfigFactory = factory;
        }

        /// <summary>
        /// Inject the factory used to create and show the Article Display Profile Config screen
        /// with its pre-wired presenter. Must be called before the form is shown.
        /// </summary>
        public void AttachDisplayProfileConfigFactory(Func<DialogResult> factory)
        {
            _displayProfileConfigFactory = factory;
        }

        // ── Form events ──────────────────────────────────────────────────────────

        private void KBMainForm_Load(object sender, EventArgs e)
        {
            SendMessage(txtCategorySearch.Handle, EM_SETCUEBANNER, (IntPtr)1, "พิมพ์รหัส/ชื่อหมวด แล้วกด Enter...");

            if (_presenter == null)
            {
                MessageBox.Show(
                    "Presenter ยังไม่ได้ถูก Attach\n\nกรุณาเรียก AttachPresenter() ก่อนเปิดฟอร์ม",
                    "ข้อผิดพลาดภายใน", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            InitFilterCombo();
            _presenter.Initialize();
        }

        private void KBMainForm_FormClosing(object sender, FormClosingEventArgs e)
        {
            // ปิดโมดูล KMS (MDI child ปิด) หรือ logout (MDI parent ปิด → cascade มาปิด child นี้ก่อน)
            // ต้อง dispose ArticleViewerForm ทั้งตัวที่ active และที่ค้างอยู่ใน history stack
            // ไม่งั้นฟอร์มที่ยังไม่ถูก Back ออกให้หมด (แต่ละอันมี WebBrowser control) จะรั่วอยู่ในหน่วยความจำ
            CloseActiveViewer();
        }

        // work around bug scrollbar in right on Form
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern bool ShowScrollBar(IntPtr hWnd, int wBar, bool bShow);
        private const int SB_BOTH = 3;

        protected override void OnShown(EventArgs e)
        {
            base.OnShown(e);
            BeginInvoke(new Action(() =>
            {
                var mdiClient = MdiParent != null
                    ? MdiParent.Controls.OfType<MdiClient>().FirstOrDefault()
                    : null;
                if (mdiClient != null && mdiClient.IsHandleCreated)
                    ShowScrollBar(mdiClient.Handle, SB_BOTH, false);
            }));
        }
        private void KBMainForm_Resize(object sender, EventArgs e) { ResizeArticleCards(); }

        // ── IKBMainController ───────────────────────────────────────────────────

        public string SearchKeyword      { get { return txtToolbarSearch.Text.Trim(); } }
        public byte?  TypeFilter         { get { return cboType.SelectedValue as byte?; } }
        public byte?  StatusFilter       { get { return cboStatus.SelectedValue as byte?; } }
        public int    SearchFieldId      { get { return cboSearchField.SelectedIndex >= 0 ? cboSearchField.SelectedIndex + 1 : 1; } }

        public int PageSize
        {
            get
            {
                switch (cboPageSize.SelectedIndex)
                {
                    case 1:  return 50;
                    case 2:  return 100;
                    default: return 20;
                }
            }
        }

        public int?   SelectedCategoryId   { get; set; }
        public string SelectedArticleTitle { get; set; }

        public void ShowLoading()
        {
            _loadingOverlay.Bounds = splitMain.Panel2.ClientRectangle;
            Cursor = Cursors.WaitCursor;
            _loadingOverlay.ShowOverlay();
        }

        public void HideLoading()
        {
            _loadingOverlay.HideOverlay();
            Cursor = Cursors.Default;
        }

        public string ArticleTitleText  { set { lblArticleTitle.Text = value; } }
        public string ArticleCountText  { set { lblPageInfo.Text = value; } }

        public void BindCategoryTree(KBCategoryTreeData data)
        {
            BuildCategoryTree(data);
        }

        public bool IsArchivedMode
        {
            get { return _isArchivedMode; }
            set
            {
                _isArchivedMode   = value;
                cboStatus.Enabled = !value;
                var delCol = dgvArticles.Columns["colBtnDelete"];
                if (delCol != null) delCol.Visible = value;
                var reviewCol = dgvArticles.Columns["colBtnReview"];
                if (reviewCol != null) reviewCol.Visible = !value;
            }
        }

        /// <summary>Tag object placed on title leaf-nodes in treeCategories.</summary>
        private sealed class TitleNode
        {
            public int    CategoryId   { get; set; }
            public string CategoryName { get; set; }
            public string Title        { get; set; }
        }

        /// <summary>Tag object placed on the fixed "Archived" tree node.</summary>
        private sealed class ArchivedNode { }

        /// <summary>Tag object placed on the "ไม่พบหมวด" placeholder node shown when a category search finds nothing.</summary>
        private sealed class NoResultNode { }

        /// <summary>Tag object on the placeholder child of a category in the search result: its 📄/🔗 items are copied from <see cref="Source"/> when the category is first expanded.</summary>
        private sealed class LazyItemsNode { public TreeNode Source { get; set; } }

        /// <summary>
        /// Tag object placed on a cross-reference "folder" node (e.g. "คำสั่ง (นายทะเบียน/กระทรวง)"
        /// under a มาตรา node, or "กฎหมาย" under a คำสั่ง/หมวดภายใน node) and on every child leaf
        /// underneath it — clicking the folder or any of its children shows the same full group.
        /// </summary>
        private sealed class LinkedFolderNode
        {
            public List<int> ArticleIds { get; set; }
            public string    Label      { get; set; }
        }

        public void RenderArticles(IEnumerable<KBArticle> articles)
        {
            _cachedArticles = articles.ToList();

            // Tab1 (grid) always renders immediately
            PopulateGrid(_cachedArticles);

            // Tab2 (cards) renders only when it is the active tab — otherwise mark dirty
            if (tabArticles.SelectedTab == tabPage2)
                PopulateCards(_cachedArticles);
            else
                _tab2NeedsRefresh = true;
        }

        /// <summary>
        /// รับ header label/การซ่อนแสดงคอลัมน์ที่ resolve แล้วจาก presenter (ตาม Article Display Profile
        /// ของหมวดที่กำลังดูอยู่) — null/ว่าง แปลว่ากลับไปใช้ default ตายตัวเดิมทั้งหมด (เช่นตอนดู Archived/
        /// cross-reference ที่ไม่มี CategoryID เดียวให้ resolve) ต้อง apply ทั้ง dgvArticles (คอลัมน์ header/
        /// Visible ตรงๆ) และเก็บไว้ให้ PopulateCards ส่งต่อให้ ArticleCardControl แต่ละใบด้วย
        /// </summary>
        public void ApplyColumnDisplay(IDictionary<string, KBColumnDisplaySetting> settings)
        {
            _currentColumnDisplay = (settings != null && settings.Count > 0)
                ? settings
                : KBArticleDisplaySlotCatalog.GetAllDefaults();

            ApplyGridColumnDisplay(KBArticleDisplaySlotCatalog.Title,          "colTitle");
            ApplyGridColumnDisplay(KBArticleDisplaySlotCatalog.TitleCategory,  "colCode");
            ApplyGridColumnDisplay(KBArticleDisplaySlotCatalog.ContentHTML,    "colContentHtml");
            ApplyGridColumnDisplay(KBArticleDisplaySlotCatalog.EffectiveDate,  "colEffectiveDate");
            ApplyGridColumnDisplay(KBArticleDisplaySlotCatalog.ExpiryDate,     "colExpiryDate");
            ApplyGridColumnDisplay(KBArticleDisplaySlotCatalog.LinkedEntities, "colOrgDivDept");
            // PublishedDate ไม่มีคอลัมน์ใน dgvArticles — ใช้แค่ใน ArticleCardControl

            // ถ้ากำลังอยู่ tab การ์ดอยู่แล้วตอนสลับหมวด ต้อง re-render ทันทีด้วย label ชุดใหม่
            if (tabArticles.SelectedTab == tabPage2)
                PopulateCards(_cachedArticles);
            else
                _tab2NeedsRefresh = true;
        }

        private void ApplyGridColumnDisplay(string slotKey, string columnName)
        {
            KBColumnDisplaySetting setting;
            if (!_currentColumnDisplay.TryGetValue(slotKey, out setting)) return;
            var col = dgvArticles.Columns[columnName];
            if (col == null) return;
            col.HeaderText = setting.Label ?? string.Empty;
            col.Visible    = setting.Visible;
        }

        private static bool IsArticleActive(KBArticle a)
        {
            return a.ExpiryDate == null || a.ExpiryDate.Value.Date > DateTime.Today;
        }

        private void PopulateGrid(List<KBArticle> articles)
        {
            dgvArticles.SuspendLayout();
            dgvArticles.Rows.Clear();

            for (int i = 0; i < articles.Count; i++)
            {
                var a = articles[i];
                dgvArticles.Rows.Add(
                    0,                          // 0  colRowNum
                    a.CategoryName,             // 1  colCategory
                    a.Title,                    // 2  colTitle
                    a.TitleCategory,            // 3  colCode (TitleCategory)
                    a.ContentHTML.Length >= 200 ? $"{a.ContentHTML.Substring(0, 200)}...read more" : a.ContentHTML, // 4 colContentHtml
                    a.EffectiveDate,            // 5  colEffectiveDate
                    a.ExpiryDate,               // 6  colExpiryDate (CalendarColumn)
                    IsArticleActive(a) ? "ON" : "OFF", // 7  colActive
                    a.LinkedEntities,           // 8  colOrgDivDept
                    a.Tags,                     // 9  colTags
                    a.CurrentVersion > 0 ? "v" + a.CurrentVersion : "-", // 10 colVersion
                    a.ViewCount,                // 11 colViews
                    "👁️ Read",          // 12 colBtnRead
                    "✏️ Edit",           // 13 colBtnEdit
                    "🔗 Share",          // 14 colBtnShare
                    a.Status == 1 ? "📤 Review" : "", // 15 colBtnReview
                    "🗑 ลบ"             // 16 colBtnDelete
                );
            }
            FitTextColumnWidth("colTitle", articles, a => a.Title);
            FitTextColumnWidth("colCode", articles, a => a.TitleCategory);
            dgvArticles.ResumeLayout();
        }

        // colTitle / colCode (TitleCategory): กฎหมายข้อความสั้น ("มาตรา N") ส่วนที่ไม่ใช่กฎหมายยาว — ความกว้างตามข้อความที่ยาวที่สุดของหน้านั้น
        // (ไม่ต่ำกว่า Min / ไม่เกิน Max) ถ้ายาวกว่านั้น wrap ในช่อง (WrapMode ของคอลัมน์ + AutoSizeRowsMode.AllCells ปรับความสูงแถวให้เอง)
        private const int TextColumnMinWidth = 100;
        private const int TextColumnMaxWidth = 300;

        private void FitTextColumnWidth(string columnName, List<KBArticle> articles, Func<KBArticle, string> getText)
        {
            var col = dgvArticles.Columns[columnName];
            if (col == null) return;

            Font font = col.InheritedStyle.Font ?? dgvArticles.Font;
            const TextFormatFlags flags = TextFormatFlags.NoPrefix | TextFormatFlags.NoPadding;
            var unbounded = new Size(int.MaxValue, int.MaxValue);

            int widest = 0;
            foreach (var a in articles)
            {
                string text = getText(a);
                if (string.IsNullOrEmpty(text)) continue;
                widest = Math.Max(widest, TextRenderer.MeasureText(text, font, unbounded, flags).Width);   // หลายบรรทัด = กว้างสุดของบรรทัด
            }

            int padding = col.DefaultCellStyle.Padding.Horizontal + 16;   // padding ของ cell + ระยะเผื่อขอบ
            col.Width = Math.Min(TextColumnMaxWidth, Math.Max(TextColumnMinWidth, widest + padding));
        }

        private void PopulateCards(List<KBArticle> articles)
        {
            flpArticles.SuspendLayout();
            flpArticles.Controls.Clear();
            foreach (var a in articles)
            {
                var card = new ArticleCardControl();
                card.DisplayColumn     = _currentColumnDisplay;
                card.Article           = a;
                card.CardClicked      += (s, ev) => OnArticleCardClick(ev);
                card.ReadClicked      += (s, ev) => OnArticleCardClick(ev);
                card.EditClicked      += (s, ev) => OnEditClick(ev);
                card.ShareClicked     += (s, ev) => OnShareClick(ev);
                card.SendReviewClicked += (s, ev) => OnSendReviewClick(ev);
                card.LikeClicked      += (s, ev) => _presenter.LikeArticle(ev.ArticleID);
                flpArticles.Controls.Add(card);
            }
            flpArticles.ResumeLayout();
            ResizeArticleCards();
            _tab2NeedsRefresh = false;
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowReport(string reportText)
        {
            MessageBox.Show(reportText, "รายงาน Knowledge Base",
                MessageBoxButtons.OK, MessageBoxIcon.Information);
        }

        public void RenderPagination(int totalCount, int currentPage, int pageSize)
        {
            _currentPage = currentPage;
            int totalPages = totalCount == 0 ? 1 : (int)Math.Ceiling((double)totalCount / pageSize);

            int from = totalCount == 0 ? 0 : (currentPage - 1) * pageSize + 1;
            int to   = Math.Min(currentPage * pageSize, totalCount);
            lblPageInfo.Text = totalCount == 0
                ? "ไม่มีรายการ"
                : string.Format("แสดง {0}-{1} จาก {2} รายการ", from, to, totalCount);

            btnPrevPage.Enabled = currentPage > 1;
            btnNextPage.Enabled = currentPage < totalPages;

            int startPage = Math.Max(1, currentPage - 1);
            if (startPage + 2 > totalPages) startPage = Math.Max(1, totalPages - 2);

            var pageBtns = new Button[] { btnPage1, btnPage2, btnPage3 };
            for (int i = 0; i < pageBtns.Length; i++)
            {
                int pageNum = startPage + i;
                if (pageNum <= totalPages)
                {
                    pageBtns[i].Visible   = true;
                    pageBtns[i].Text      = pageNum.ToString();
                    pageBtns[i].Tag       = pageNum;
                    bool isCurrent        = pageNum == currentPage;
                    pageBtns[i].BackColor = isCurrent
                        ? Color.FromArgb(46, 116, 181)
                        : Color.White;
                    pageBtns[i].ForeColor = isCurrent
                        ? Color.White
                        : Color.FromArgb(26, 35, 50);
                }
                else
                {
                    pageBtns[i].Visible = false;
                }
            }

            //pnlPaginationButtons.Visible = totalPages > 1;
        }

        // ── Tab selection ────────────────────────────────────────────────────────

        private void TabArticles_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tabArticles.SelectedTab == tabPage2 && _tab2NeedsRefresh)
                PopulateCards(_cachedArticles);
        }

        // ── Search / filter handlers ─────────────────────────────────────────────

        private void CboFilter_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            _presenter.LoadArticles();
        }

        private void TxtSearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (_presenter == null) return;
            if (e.KeyCode == Keys.Enter) _presenter.LoadArticles();
        }

        // ── Tree view selection ──────────────────────────────────────────────────

        private void TreeCategories_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (_presenter == null || e.Node == null || _suppressCategorySelect) return;
            if (e.Node.Tag is NoResultNode) return;   // โหนด "ไม่พบหมวด" ไม่ใช่หมวด

            // เลือก node/root node ใด ๆ ในต้นไม้หมวดหมู่ ต้องเคลียร์คำค้นเดิมทิ้งเสมอ — ไม่งั้นคำค้นที่ค้าง
            // จากหมวดก่อนหน้าจะไปกรองซ้ำกับรายการของหมวดใหม่ที่เพิ่งเลือกโดยผู้ใช้ไม่ได้ตั้งใจ
            txtToolbarSearch.Text = string.Empty;

            // Root node (Tag == null) — reset archived mode
            if (e.Node.Tag == null)
            {
                _isArchivedMode = false;
                cboStatus.Enabled = true;
                var delCol = dgvArticles.Columns["colBtnDelete"];
                if (delCol != null) delCol.Visible = false;
                return;
            }

            // Archived fixed node
            if (e.Node.Tag is ArchivedNode)
            {
                _presenter.OnArchivedSelected();
                return;
            }

            // Title leaf-node → filter grid to that specific title
            var tn = e.Node.Tag as TitleNode;
            if (tn != null)
            {
                _presenter.OnTitleSelected(tn.CategoryId, tn.CategoryName, tn.Title);
                return;
            }

            // Cross-reference folder (or one of its child leaves) → show the whole linked group
            var lf = e.Node.Tag as LinkedFolderNode;
            if (lf != null)
            {
                _presenter.OnLinkedArticlesSelected(lf.ArticleIds, lf.Label);
                return;
            }

            // Category node → existing behaviour
            var cat = e.Node.Tag as KBCategory;
            if (cat == null) return;
            _presenter.OnCategorySelected(cat.CategoryID, cat.CategoryName);
        }

        /// <summary>
        /// AfterSelect ไม่ fire ถ้าคลิกโหนดที่ selected อยู่แล้ว (WinForms TreeView behavior)
        /// ทำให้คลิกเมนู/หมวดหมู่ซ้ำที่ node เดิมแล้วไม่มีอะไรเกิดขึ้น — เรียก logic เดิมซ้ำเองในกรณีนี้
        /// </summary>
        private void TreeCategories_NodeMouseClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            var tree = (TreeView)sender;   // treeCategories หรือ treeCategoryResults
            if (e.Node != null && tree.SelectedNode == e.Node)
                TreeCategories_AfterSelect(sender, new TreeViewEventArgs(e.Node));
        }

        // ── Category tree: expand / collapse / refresh + search ──────────────────

        private void BtnExpandAll_Click(object sender, EventArgs e)
        {
            var tree = ActiveCategoryTree;
            if (tree.Nodes.Count == 0) return;

            tree.BeginUpdate();
            try { ExpandCategoryNodes(tree.Nodes[0]); }
            finally { tree.EndUpdate(); }
        }

        /// <summary>
        /// ขยายโหนดรากและหมวดหมู่ทุกชั้น — ไม่ขยายโหนดหัวเรื่อง (📄) / โฟลเดอร์อ้างอิง (🔗) / Archived ที่อยู่ใต้หมวด
        /// (ไม่ใช่โครงสร้างหมวด และมีจำนวนมาก ขยายแล้วต้นไม้ยาวจนหาหมวดไม่เจอ) ผู้ใช้กดขยายเองรายโหนดได้
        /// </summary>
        private void ExpandCategoryNodes(TreeNode node)
        {
            if (node.Tag != null && !(node.Tag is KBCategory)) return;

            EnsureItemsLoaded(node);   // หมวดในผลค้นหา: สร้างรายการข้างในก่อนขยาย
            node.Expand();
            foreach (TreeNode child in node.Nodes)
                ExpandCategoryNodes(child);
        }

        private void BtnCollapseAll_Click(object sender, EventArgs e)
        {
            var tree = ActiveCategoryTree;
            if (tree.Nodes.Count == 0) return;
            var root = tree.Nodes[0];

            // ยุบแล้ว TreeView ย้าย selection ไปโหนดแม่เอง — ไม่ให้ไปโหลดบทความของโหนดนั้นใหม่
            _suppressCategorySelect = true;
            tree.BeginUpdate();
            try
            {
                foreach (TreeNode child in root.Nodes)
                    child.Collapse(false);
                root.Expand();   // ยังเห็นหมวดระดับบนสุดอยู่
            }
            finally
            {
                tree.EndUpdate();
                _suppressCategorySelect = false;
            }
        }

        /// <summary>
        /// รีเฟรชต้นไม้หมวดทั้งหมดให้กลับเป็นสถานะเริ่มต้น: ล้างคำค้น/ผลที่กรอง แล้วโหลดหมวด/จำนวน/หัวเรื่องจาก DB ใหม่
        /// (ยุบทั้งหมด เห็นระดับบนสุด และรับการเปลี่ยนแปลงที่ผู้อื่นเพิ่งบันทึก)
        /// </summary>
        private void BtnRefreshCategoryTree_Click(object sender, EventArgs e)
        {
            txtCategorySearch.Clear();
            ApplyCategorySearch(string.Empty);   // คืนต้นไม้เต็มทันที แม้ยังไม่มี presenter
            if (_presenter != null) _presenter.LoadCategoryTree();
        }

        private void TxtCategorySearch_TextChanged(object sender, EventArgs e)
        {
            btnClearCategorySearch.Visible = txtCategorySearch.Text.Length > 0;

            // ไม่ค้นทุกตัวอักษร (ต้นไม้ใหญ่ ช้า) — ค้นเมื่อกด Enter เท่านั้น ยกเว้นลบจนว่าง = เลิกค้นหา คืนต้นไม้เต็ม (ไม่ใช่การค้น ทำเร็ว)
            string term = txtCategorySearch.Text.Trim();
            if (term.Length == 0)
                ApplyCategorySearch(string.Empty);
            else if (!string.Equals(term, _categorySearchAppliedTerm, StringComparison.Ordinal))
                txtCategorySearch.BackColor = SystemColors.Window;   // คำใหม่ที่ยังไม่ได้กด Enter — เอาสีแดง "ไม่พบ" ของคำก่อนหน้าออก
        }

        private void TxtCategorySearch_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                string term = txtCategorySearch.Text.Trim();
                if (!string.Equals(term, _categorySearchAppliedTerm, StringComparison.Ordinal))
                    ApplyCategorySearch(term);                   // คำใหม่ → ค้นหา (เฉพาะตอนกด Enter)
                else
                    MoveToCategoryMatch(e.Shift ? -1 : 1);       // คำเดิม → Enter = ตัวถัดไป, Shift+Enter = ตัวก่อนหน้า
                e.SuppressKeyPress = true;
            }
            else if (e.KeyCode == Keys.Escape)
            {
                txtCategorySearch.Clear();
                e.SuppressKeyPress = true;
            }
        }

        private void BtnClearCategorySearch_Click(object sender, EventArgs e)
        {
            txtCategorySearch.Clear();
            txtCategorySearch.Focus();
        }

        /// <summary>
        /// ค้นหาหมวดจากต้นไม้ที่โหลดไว้แล้ว (หมวดทั้งหมดอยู่ในหน่วยความจำ จึงไม่ query DB) — เรียกเมื่อกด Enter ด้วยคำใหม่:
        /// ตรงกับ CategoryCode หรือ CategoryName บางส่วน ไม่สนตัวพิมพ์เล็ก-ใหญ่ →
        /// แสดงเฉพาะหมวดที่เจอ + สายแม่ของมัน (ให้เห็นลำดับชั้น) และ "ทุกอย่างข้างในหมวดที่เจอ" (หมวดย่อยทุกชั้น + รายการ 📄/🔗 — ยุบไว้ กดขยายดู)
        /// ไฮไลต์ตัวที่เจอ — สร้างเป็นสำเนาใน treeCategoryResults แล้วซ่อน treeCategories (หมวด/รายการอื่นจึงถูกซ่อน โดยไม่แตะต้นไม้เต็ม).
        /// ไม่เจอ → แสดงโหนด "ไม่พบหมวด" + ซ่อนปุ่มขยาย/ยุบ. <paramref name="term"/> ว่าง = เลิกค้นหา คืนต้นไม้เต็ม
        /// </summary>
        private void ApplyCategorySearch(string term)
        {
            ShowFullCategoryTree();
            _categorySearchAppliedTerm = term ?? string.Empty;
            txtCategorySearch.BackColor = SystemColors.Window;
            SetExpandCollapseVisible(true);

            if (_categorySearchAppliedTerm.Length == 0 || treeCategories.Nodes.Count == 0) return;

            var hits = new HashSet<TreeNode>();
            MarkCategoryHits(treeCategories.Nodes[0], _categorySearchAppliedTerm, hits);

            _suppressCategorySelect = true;   // ล้าง/ใส่โหนดแล้ว TreeView ย้าย selection เอง — ไม่ให้ไปโหลดบทความใหม่
            treeCategoryResults.BeginUpdate();
            try
            {
                treeCategoryResults.Nodes.Clear();
                if (hits.Count == 0)
                {
                    treeCategoryResults.Nodes.Add(new TreeNode(string.Format("ไม่พบหมวดที่ตรงกับ '{0}'", _categorySearchAppliedTerm))
                    {
                        Tag       = new NoResultNode(),
                        ForeColor = Color.Gray,
                    });
                }
                else
                {
                    treeCategoryResults.Nodes.Add(CloneHitBranch(treeCategories.Nodes[0], hits, _categorySearchAppliedTerm, false));
                    ExpandToMatches();   // ขยายสายแม่ให้เห็นทุกตัวที่เจอ (ตัวที่เจอเองยังยุบ กดขยายดูของข้างในได้)
                }
            }
            finally
            {
                treeCategoryResults.EndUpdate();
                treeCategories.Visible       = false;   // ซ่อนเฉย ๆ ไม่ถอดโหนด
                treeCategoryResults.Visible  = true;
                _categorySearchActive        = true;
                _suppressCategorySelect      = false;
            }

            if (hits.Count == 0)
            {
                txtCategorySearch.BackColor = CategoryNotFoundColor;
                SetExpandCollapseVisible(false);
                return;
            }

            MoveToCategoryMatch(1);
        }

        private static bool CategoryMatches(KBCategory cat, string term)
        {
            return (cat.CategoryCode != null && cat.CategoryCode.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0)
                || (cat.CategoryName != null && cat.CategoryName.IndexOf(term, StringComparison.OrdinalIgnoreCase) >= 0);
        }

        /// <summary>ใส่ใน <paramref name="hits"/> ทุกโหนดที่ตรงคำค้น หรือมีลูกหลานที่ตรง (= สายที่ต้องแสดง) — ไม่ลงไปในโหนดหัวเรื่อง/โฟลเดอร์อ้างอิง</summary>
        private bool MarkCategoryHits(TreeNode node, string term, HashSet<TreeNode> hits)
        {
            var cat = node.Tag as KBCategory;
            if (node.Tag != null && cat == null) return false;

            bool hit = cat != null && CategoryMatches(cat, term);
            foreach (TreeNode child in node.Nodes)
            {
                if (MarkCategoryHits(child, term, hits)) hit = true;
            }

            if (hit) hits.Add(node);
            return hit;
        }

        /// <summary>
        /// สร้างสำเนาของสายที่เจอ (Tag เดิม → คลิกแล้วโหลดบทความได้เหมือนเดิม): หมวดที่เจอเก็บ "ทุกอย่างข้างใน" ไว้ — หมวดย่อยทุกชั้น
        /// (ทำเป็นสำเนาทันที มีจำนวนไม่มาก) และรายการ 📄/🔗 (มีหลักหมื่น จึงใส่ placeholder แล้วสร้างสำเนาตอนขยายหมวดนั้น — EnsureItemsLoaded).
        /// หมวดที่เป็นแค่สายแม่เก็บเฉพาะสายที่นำไปสู่ตัวที่เจอ. โหนดที่ตรงคำค้นจริงไฮไลต์และเก็บใน _categorySearchMatches ตามลำดับ pre-order
        /// (= ตามที่เห็นบนจอ). <paramref name="insideMatch"/> = มีหมวดที่เจอเป็นแม่ของโหนดนี้อยู่แล้ว
        /// </summary>
        private TreeNode CloneHitBranch(TreeNode source, HashSet<TreeNode> hits, string term, bool insideMatch)
        {
            var copy = new TreeNode(source.Text)
            {
                Tag       = source.Tag,
                ForeColor = source.ForeColor,
                NodeFont  = source.NodeFont,
            };

            var cat = source.Tag as KBCategory;
            bool isMatch = cat != null && CategoryMatches(cat, term);
            if (isMatch)
            {
                copy.BackColor = CategoryMatchColor;
                _categorySearchMatches.Add(copy);
            }

            bool keepAll   = insideMatch || isMatch;
            bool hasItems  = false;
            foreach (TreeNode child in source.Nodes)
            {
                if (child.Tag is KBCategory)
                {
                    if (keepAll || hits.Contains(child))
                        copy.Nodes.Add(CloneHitBranch(child, hits, term, keepAll));
                }
                else
                {
                    hasItems = true;
                }
            }

            if (keepAll && hasItems && cat != null)
                copy.Nodes.Add(new TreeNode("...") { Tag = new LazyItemsNode { Source = source } });

            return copy;
        }

        /// <summary>
        /// หมวดในผลค้นหาที่ยังมี placeholder ท้ายรายการลูก → สร้างสำเนารายการ 📄/🔗 จากโหนดต้นฉบับมาแทน (ทำซ้ำได้ ไม่เกิดผลซ้ำ).
        /// เรียกจาก BeforeExpand และก่อนสั่ง Expand ในโค้ด (การ Expand ในโค้ดตอน TreeView ยังไม่มี handle ไม่ยิง BeforeExpand)
        /// </summary>
        private void EnsureItemsLoaded(TreeNode node)
        {
            if (node.Nodes.Count == 0) return;

            var placeholder = node.Nodes[node.Nodes.Count - 1];
            var lazy = placeholder.Tag as LazyItemsNode;
            if (lazy == null) return;

            node.Nodes.Remove(placeholder);
            foreach (TreeNode item in lazy.Source.Nodes)
            {
                if (!(item.Tag is KBCategory))
                    node.Nodes.Add((TreeNode)item.Clone());   // Clone = ลึกทั้งสาย (หัวเรื่อง → โฟลเดอร์อ้างอิง → รายการ) และคง Tag เดิม
            }
        }

        private void TreeCategoryResults_BeforeExpand(object sender, TreeViewCancelEventArgs e)
        {
            EnsureItemsLoaded(e.Node);
        }

        /// <summary>ขยายสายแม่ของทุกโหนดที่เจอ ให้เห็นทุกตัวในผลค้นหา</summary>
        private void ExpandToMatches()
        {
            foreach (var match in _categorySearchMatches)
            {
                for (var parent = match.Parent; parent != null; parent = parent.Parent)
                {
                    EnsureItemsLoaded(parent);
                    parent.Expand();
                }
            }
        }

        /// <summary>
        /// กลับไปแสดงต้นไม้เต็ม (ถ้ากำลังแสดงผลค้นหาอยู่) — ซ่อน treeCategoryResults แล้วโชว์ treeCategories ที่ยังอยู่ครบ ไม่ได้ถูกแตะ.
        /// ถ้าผู้ใช้คลิกหมวดในผลค้นหาไว้ ให้เลือกหมวดเดียวกันในต้นไม้เต็ม (ไม่ให้โหลดบทความซ้ำ) ไม่งั้นการเลือกเดิมยังอยู่
        /// </summary>
        private void ShowFullCategoryTree()
        {
            _categorySearchMatches.Clear();
            _categorySearchCurrent = -1;
            if (!_categorySearchActive) return;

            var picked         = treeCategoryResults.SelectedNode;
            var pickedCategory = picked == null ? null : picked.Tag as KBCategory;

            TreeNode target = null;
            if (treeCategories.Nodes.Count > 0 && picked != null)
            {
                var fullRoot = treeCategories.Nodes[0];
                if (pickedCategory != null)
                {
                    target = FindCategoryNode(fullRoot, pickedCategory.CategoryID);
                }
                else if (picked.Tag == null)
                {
                    target = fullRoot;   // รากของผลค้นหา = รากของต้นไม้เต็ม
                }
                else if (!(picked.Tag is NoResultNode))
                {
                    // รายการ 📄/🔗 ที่คลิกในผลค้นหา → โหนดเดียวกันในต้นไม้เต็ม (Tag เป็น object เดียวกัน) หาจากหมวดที่เป็นเจ้าของ
                    var owner = picked.Parent;
                    while (owner != null && !(owner.Tag is KBCategory)) owner = owner.Parent;
                    var fullOwner = owner == null ? null : FindCategoryNode(fullRoot, ((KBCategory)owner.Tag).CategoryID);
                    if (fullOwner != null) target = FindNodeByTag(fullOwner, picked.Tag);
                }
            }

            _categorySearchActive   = false;
            _suppressCategorySelect = true;
            try
            {
                treeCategoryResults.Visible = false;
                treeCategoryResults.Nodes.Clear();
                treeCategories.Visible = true;
                if (target != null)
                    treeCategories.SelectedNode = target;
            }
            finally
            {
                _suppressCategorySelect = false;
            }
        }

        private static TreeNode FindCategoryNode(TreeNode node, int categoryId)
        {
            var cat = node.Tag as KBCategory;
            if (node.Tag != null && cat == null) return null;
            if (cat != null && cat.CategoryID == categoryId) return node;

            foreach (TreeNode child in node.Nodes)
            {
                var found = FindCategoryNode(child, categoryId);
                if (found != null) return found;
            }
            return null;
        }

        private static TreeNode FindNodeByTag(TreeNode node, object tag)
        {
            if (ReferenceEquals(node.Tag, tag)) return node;

            foreach (TreeNode child in node.Nodes)
            {
                var found = FindNodeByTag(child, tag);
                if (found != null) return found;
            }
            return null;
        }

        /// <summary>ชี้ไปตัวที่ตรงคำค้นถัดไป (1) / ก่อนหน้า (-1) วนรอบได้ และเลื่อนต้นไม้ให้เห็นโหนดนั้น</summary>
        private void MoveToCategoryMatch(int direction)
        {
            int count = _categorySearchMatches.Count;
            if (count == 0) return;

            if (_categorySearchCurrent >= 0)
                _categorySearchMatches[_categorySearchCurrent].BackColor = CategoryMatchColor;

            _categorySearchCurrent = _categorySearchCurrent < 0
                ? (direction > 0 ? 0 : count - 1)
                : (_categorySearchCurrent + direction + count) % count;

            var current = _categorySearchMatches[_categorySearchCurrent];
            current.BackColor = CategoryCurrentColor;
            current.EnsureVisible();
        }

        private void SetExpandCollapseVisible(bool visible)
        {
            btnExpandAll.Visible   = visible;
            btnCollapseAll.Visible = visible;
        }

        // ── Article card click ───────────────────────────────────────────────────

        private void OnArticleCardClick(KBArticle article)
        {
            _presenter.OnArticleClicked(article.ArticleID);
            ShowArticleViewer(article);
        }

        // ── Toolbar button handlers ──────────────────────────────────────────────

        private void BtnNewArticle_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            ShowEditorEmbedded(null);
        }

        private void BtnNewCategory_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            ShowCategorySettings();
        }

        private void BtnSettings_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            ShowCategorySettings();
        }

        private void BtnDashboard_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (_dashboardFactory == null) return;
            _dashboardFactory();
        }

        private void BtnImportTemplateConfig_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (_importTemplateConfigFactory == null) return;
            _importTemplateConfigFactory();
        }

        private void BtnDisplayProfileConfig_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (_displayProfileConfigFactory == null) return;
            _displayProfileConfigFactory();

            // Profile ที่แก้อาจกระทบหมวดที่กำลังดูอยู่ — โหลดต้นไม้หมวดหมู่ใหม่ให้แน่ใจว่า tree/label ตรงกัน
            _presenter.LoadCategoryTree();
        }

        private void BtnImportExcel_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;

            // KBImportWizardForm auto-detects the Excel layout (กฎหมาย/มาตรา vs ประกาศ/คำสั่ง)
            // from the file's header row, so there is no type-picker step anymore.
            ShowImportWizardEmbedded();
        }

        private void BtnReports_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            _presenter.ShowReport();
        }

        private async void BtnTestNotification_Click(object sender, EventArgs e)
        {
            try
            {
                if (!NotificationManager.Instance.IsConnected)
                {
                    MessageBox.Show(
                        "Notification system is not connected.\nPlease check your connection.",
                        "Notification Test",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Warning);
                    return;
                }

                // Get current user AD name
                string currentUserAd = _user != null ? _user.ADUserName : "unknown";
                
                // Send test notification to yourself
                var result = await NotificationManager.Instance.SendNotificationAsync(
                    typeCode: "ASSIGN",
                    title: "Test Notification from KMS",
                    message: "This is a test notification from Knowledge Management System. Click to view details.",
                    recipientUserAd: currentUserAd,
                    senderUserAd: currentUserAd,
                    referenceType: "kb_article",
                    referenceId: "TEST001",
                    referenceCode: "KB-TEST-001",
                    priority: 2
                );

                if (result != null)
                {
                    MessageBox.Show(
                        "Test notification sent successfully!\n\n" +
                        "Notification ID: " + result.NotificationId + "\n" +
                        "Recipient: " + currentUserAd + "\n\n" +
                        "You should see a popup notification shortly.",
                        "Notification Test",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Information);
                }
                else
                {
                    MessageBox.Show(
                        "Failed to send test notification.\nPlease check the logs for details.",
                        "Notification Test",
                        MessageBoxButtons.OK,
                        MessageBoxIcon.Error);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Error sending test notification:\n\n" + ex.Message,
                    "Notification Test",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Error);
            }
        }

        // ── Category settings navigation (embedded over splitMain) ──────────────

        private void ShowCategorySettings()
        {
            CloseActiveCategoryForm();

            var catForm = _categoryFormFactory != null ? _categoryFormFactory() : null;
            if (catForm == null) return;

            catForm.BackRequested  += OnCategoryBackRequested;
            catForm.TopLevel        = false;
            catForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            catForm.Dock            = System.Windows.Forms.DockStyle.Fill;

            splitMain.Visible = false;
            pnlCategoryHost.Controls.Add(catForm);
            pnlCategoryHost.Visible = true;
            pnlCategoryHost.BringToFront();
            catForm.Show();

            _activeCategoryForm = catForm;
        }

        private void OnCategoryBackRequested(object sender, EventArgs e)
        {
            CloseActiveCategoryForm();

            pnlCategoryHost.Visible = false;
            splitMain.Visible       = true;

            if (_presenter != null)
                _presenter.LoadCategoryTree();
        }

        private void CloseActiveCategoryForm()
        {
            if (_activeCategoryForm == null) return;
            _activeCategoryForm.BackRequested -= OnCategoryBackRequested;
            pnlCategoryHost.Controls.Remove(_activeCategoryForm);
            _activeCategoryForm.Dispose();
            _activeCategoryForm = null;
        }

        // ── Editor embedded navigation (full-screen over splitMain) ──────────────

        private void ShowEditorEmbedded(KBArticle article)
        {
            CloseActiveEditorForm();

            var editorForm = _editorFactory != null ? _editorFactory(article) : null;
            if (editorForm == null) return;

            editorForm.BackRequested  += OnEditorBackRequested;
            editorForm.TopLevel        = false;
            editorForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            editorForm.Dock            = System.Windows.Forms.DockStyle.Fill;

            splitMain.Visible = false;
            pnlEditorHost.Controls.Add(editorForm);
            pnlEditorHost.Visible = true;
            pnlEditorHost.BringToFront();
            editorForm.Show();

            _activeEditorForm = editorForm;
        }

        private void OnEditorBackRequested(object sender, EventArgs e)
        {
            CloseActiveEditorForm();

            pnlEditorHost.Visible = false;
            splitMain.Visible     = true;

            if (_presenter != null)
            {
                _presenter.LoadCategoryTree();
                _presenter.LoadArticles();
            }
        }

        private void CloseActiveEditorForm()
        {
            if (_activeEditorForm == null) return;
            _activeEditorForm.BackRequested -= OnEditorBackRequested;
            pnlEditorHost.Controls.Remove(_activeEditorForm);
            _activeEditorForm.Dispose();
            _activeEditorForm = null;
        }

        // ── Import wizard embedded navigation (full-screen over splitMain) ────────

        private void ShowImportWizardEmbedded()
        {
            CloseActiveImportForm();

            var importForm = _importWizardFactory != null ? _importWizardFactory() : null;
            if (importForm == null) return;

            importForm.BackRequested  += OnImportBackRequested;
            importForm.TopLevel        = false;
            importForm.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            importForm.Dock            = System.Windows.Forms.DockStyle.Fill;

            splitMain.Visible = false;
            pnlImportHost.Controls.Add(importForm);
            pnlImportHost.Visible = true;
            pnlImportHost.BringToFront();
            importForm.Show();

            _activeImportForm = importForm;
        }

        private void OnImportBackRequested(object sender, EventArgs e)
        {
            CloseActiveImportForm();

            pnlImportHost.Visible = false;
            splitMain.Visible     = true;

            if (_presenter != null)
            {
                _presenter.LoadCategoryTree();
                _presenter.LoadArticles();
            }
        }

        private void CloseActiveImportForm()
        {
            if (_activeImportForm == null) return;
            _activeImportForm.BackRequested -= OnImportBackRequested;
            pnlImportHost.Controls.Remove(_activeImportForm);
            _activeImportForm.Dispose();
            _activeImportForm = null;
        }

        // ── Viewer navigation (full-screen over splitMain) ───────────────────────

        private void ShowArticleViewer(KBArticle article)
        {
            // If the same article is already the active viewer, do nothing
            if (_activeViewerArticleId.HasValue && _activeViewerArticleId.Value == article.ArticleID)
                return;

            // Push the current viewer onto the history stack instead of disposing it,
            // so btnBack can return to it later
            if (_activeViewer != null)
            {
                _activeViewer.Hide();
                pnlViewerHost.Controls.Remove(_activeViewer);
                _viewerHistory.Push(_activeViewer);
                _activeViewer          = null;
                _activeViewerArticleId = null;
            }

            var viewer = _viewerFactory != null ? _viewerFactory(article) : null;
            if (viewer == null) return;

            viewer.BackRequested        += OnViewerBackRequested;
            viewer.ArticleOpenRequested += (s, a) => ShowArticleViewer(a);
            viewer.TopLevel        = false;
            viewer.FormBorderStyle = System.Windows.Forms.FormBorderStyle.None;
            viewer.Dock            = System.Windows.Forms.DockStyle.Fill;

            splitMain.Visible      = false;
            pnlViewerHost.Controls.Add(viewer);
            pnlViewerHost.Visible  = true;
            pnlViewerHost.BringToFront();
            viewer.Show();

            _activeViewer          = viewer;
            _activeViewerArticleId = article.ArticleID;
        }

        private void OnViewerBackRequested(object sender, EventArgs e)
        {
            // Dispose the current viewer
            if (_activeViewer != null)
            {
                _activeViewer.BackRequested -= OnViewerBackRequested;
                pnlViewerHost.Controls.Remove(_activeViewer);
                _activeViewer.Dispose();
                _activeViewer          = null;
                _activeViewerArticleId = null;
            }

            // If there is a previous viewer on the stack, restore it
            if (_viewerHistory.Count > 0)
            {
                var previous = _viewerHistory.Pop();
                pnlViewerHost.Controls.Add(previous);
                previous.Show();

                _activeViewer          = previous;
                _activeViewerArticleId = previous.CurrentArticleId;
                return;
            }

            // Stack is empty — return to the article list
            pnlViewerHost.Visible = false;
            splitMain.Visible     = true;

            if (_presenter != null)
                _presenter.LoadArticles();

            // Force layout update and resize article cards
            splitMain.PerformLayout();
            flpArticles.PerformLayout();
            BeginInvoke(new Action(ResizeArticleCards));
        }

        private void CloseActiveViewer()
        {
            // Dispose the active viewer
            if (_activeViewer != null)
            {
                _activeViewer.BackRequested -= OnViewerBackRequested;
                pnlViewerHost.Controls.Remove(_activeViewer);
                _activeViewer.Dispose();
                _activeViewer          = null;
                _activeViewerArticleId = null;
            }

            // Dispose every viewer in the history stack
            while (_viewerHistory.Count > 0)
            {
                var old = _viewerHistory.Pop();
                old.Dispose();
            }
        }

        // ── Private UI helpers ────────────────────────────────────────────────────

        private void InitFilterCombo()
        {
            // cboType — code 5 = Wiki Page
            cboType.DataSource = new[]
            {
                new { Code = (byte?)null, Description = "ทุกประเภท" },
                new { Code = (byte?)5,    Description = "Wiki Page" },
            };
            cboType.DisplayMember = "Description";
            cboType.ValueMember   = "Code";
            cboType.SelectedIndex = 0;

            // cboStatus — search by code (1=Draft, 2=Review, 3=Published, 4=Archived)
            cboStatus.DataSource = new[]
            {
                new { Code = (byte?)null, Description = "ทุกสถานะ"  },
                new { Code = (byte?)1,    Description = "Draft"     },
                new { Code = (byte?)2,    Description = "Review"    },
                new { Code = (byte?)3,    Description = "Published" },
                //new { Code = (byte?)4,    Description = "Archived"  },
            };
            cboStatus.DisplayMember = "Description";
            cboStatus.ValueMember   = "Code";
            cboStatus.SelectedIndex = 0;

            cboType.SelectedIndex       = 0;
            cboDepartment.SelectedIndex = 0;
            cboSortBy.SelectedIndex     = 0;
            cboPageSize.SelectedIndex   = 0;

            cboSearchField.DataSource = new[]
            {
                new { Id = 1, Description = "ทั้งหมด"        },
                new { Id = 2, Description = "ชื่อบทความ"    },
                new { Id = 3, Description = "เนื้อหาบทความ" },
                new { Id = 4, Description = "Tag"           },
                new { Id = 5, Description = "Summary"       },
                new { Id = 6, Description = "หมวดหมู่ของชื่อบทความ"    },
            };
            cboSearchField.DisplayMember = "Description";
            cboSearchField.ValueMember   = "Id";
            cboSearchField.SelectedIndex = 0;

            // Wire up after initial values are set to avoid spurious loads
            btnPrevPage.Click  += BtnPrevPage_Click;
            btnNextPage.Click  += BtnNextPage_Click;
            btnPage1.Click     += BtnPageNumber_Click;
            btnPage2.Click     += BtnPageNumber_Click;
            btnPage3.Click     += BtnPageNumber_Click;
            cboPageSize.SelectedIndexChanged += CboPageSize_SelectedIndexChanged;
            btnClearFilter.Click += BtnClearFilter_Click;
        }

        private void BuildCategoryTree(KBCategoryTreeData data)
        {
            // ผลค้นหาที่แสดงอยู่เป็นสำเนาของต้นไม้เดิมที่กำลังจะถูกสร้างใหม่ — ทิ้ง แล้วกลับไปแสดงต้นไม้เต็ม
            _categorySearchActive  = false;
            _categorySearchMatches.Clear();
            _categorySearchCurrent = -1;
            _suppressCategorySelect = true;
            treeCategoryResults.Visible = false;
            treeCategoryResults.Nodes.Clear();
            treeCategories.Visible = true;
            _suppressCategorySelect = false;

            treeCategories.BeginUpdate();
            treeCategories.Nodes.Clear();

            var categories    = data.Categories.ToList();
            var articleCounts = data.ArticleCounts;

            int totalCount = articleCounts.Values.Sum();
            string rootLabel = totalCount > 0
                ? string.Format("📚 ทั้งหมด ({0})", totalCount)
                : "📚 ทั้งหมด";

            var rootNode = new TreeNode(rootLabel) { Tag = null };
            rootNode.ForeColor = Color.FromArgb(0, 51, 102);
            rootNode.NodeFont  = new Font("Segoe UI", 12f, FontStyle.Bold);
            treeCategories.Nodes.Add(rootNode);

            var roots = categories
                .Where(c => c.IsActive && !c.ParentCategoryID.HasValue)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName);
            foreach (var cat in roots)
                rootNode.Nodes.Add(BuildCategoryNode(cat, categories, data));

            var archivedTreeNode = new TreeNode("📁 Archived")
            {
                Tag       = new ArchivedNode(),
                //ForeColor = Color.FromArgb(139, 0, 0),
                //NodeFont  = new Font("Segoe UI", 11f, FontStyle.Bold),
            };
            rootNode.Nodes.Add(archivedTreeNode);

            treeCategories.EndUpdate();
            rootNode.Expand();

            _suppressCategorySelect  = true;
            treeCategories.SelectedNode = rootNode;
            _suppressCategorySelect  = false;

            // คำค้นที่กด Enter ไว้แล้วต้อง apply กับต้นไม้ใหม่ (โหนดเดิมถูกทิ้งไปแล้ว)
            if (_categorySearchAppliedTerm.Length > 0)
                ApplyCategorySearch(_categorySearchAppliedTerm);
            else
                SetExpandCollapseVisible(true);
        }

        private TreeNode BuildCategoryNode(
            KBCategory        cat,
            List<KBCategory>  allCategories,
            KBCategoryTreeData data)
        {
            var articleCounts    = data.ArticleCounts;
            var titlesByCategory = data.TitlesByCategory;

            int    count = articleCounts.ContainsKey(cat.CategoryID) ? articleCounts[cat.CategoryID] : 0;
            string label = count > 0
                ? string.Format("📁 {0}", cat.CategoryName)
                : string.Format("📁 {0}", cat.CategoryName);

            var node = new TreeNode(label)
            {
                Tag       = cat,
                ForeColor = Color.FromArgb(30, 30, 30),
            };

            // ── Child categories (existing structure) ────────────────────────────
            var children = allCategories
                .Where(c => c.IsActive && c.ParentCategoryID == cat.CategoryID)
                .OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName);
            foreach (var child in children)
                node.Nodes.Add(BuildCategoryNode(child, allCategories, data));

            // ── Title leaf-nodes: "📄 Title (N)" ────────────────────────────────
            IDictionary<string, int> titleMap;
            if (titlesByCategory != null && titlesByCategory.TryGetValue(cat.CategoryID, out titleMap))
            {
                IDictionary<string, List<KBLinkedFolder>> linkedFoldersByTitle = null;
                if (data.LinkedFoldersByCategoryTitle != null)
                    data.LinkedFoldersByCategoryTitle.TryGetValue(cat.CategoryID, out linkedFoldersByTitle);

                foreach (var kvp in titleMap.OrderBy(x => x.Key, NaturalStringComparer.Instance))
                {
                    string nodeLabel = string.Format("📄 {0}", kvp.Key);
                    var titleNode = new TreeNode(nodeLabel)
                    {
                        Tag       = new TitleNode
                                    {
                                        CategoryId   = cat.CategoryID,
                                        CategoryName = cat.CategoryName,
                                        Title        = kvp.Key,   // raw title, no count
                                    },
                        ForeColor = Color.FromArgb(50, 80, 140),
                    };

                    // ── Cross-reference folders (มาตรา ↔ คำสั่ง/หมวดภายใน) ─────────
                    List<KBLinkedFolder> linkedFolders;
                    if (linkedFoldersByTitle != null && linkedFoldersByTitle.TryGetValue(kvp.Key, out linkedFolders))
                    {
                        foreach (var folder in linkedFolders)
                        {
                            var orderedArticles = folder.Articles
                                .OrderBy(a2 => a2.Title, NaturalStringComparer.Instance)
                                .ToList();

                            var folderTag = new LinkedFolderNode
                            {
                                ArticleIds = orderedArticles.Select(a2 => a2.ArticleID).ToList(),
                                Label      = folder.Label,
                            };

                            //var folderNode = new TreeNode(string.Format("🔗 {0} ({1})", folder.Label, orderedArticles.Count))
                            var folderNode = new TreeNode(string.Format("🔗 {0}", folder.Label))
                            {
                                Tag       = folderTag,
                                ForeColor = Color.FromArgb(120, 90, 40),
                            };

                            foreach (var linkedArticle in orderedArticles)
                            {
                                // item เดี่ยวใต้ folder → คลิกแล้วแสดงเฉพาะรายการนั้น (ต่างจากคลิกที่ตัว folder ซึ่งแสดงทั้งกลุ่ม)
                                folderNode.Nodes.Add(new TreeNode(string.Format("📄 {0}", linkedArticle.Title))
                                {
                                    Tag = new LinkedFolderNode
                                    {
                                        ArticleIds = new List<int> { linkedArticle.ArticleID },
                                        Label      = string.Format("{0}  ›  {1}", folder.Label, linkedArticle.Title),
                                    },
                                    ForeColor = Color.FromArgb(120, 90, 40),
                                });
                            }

                            titleNode.Nodes.Add(folderNode);
                        }
                    }

                    node.Nodes.Add(titleNode);
                }
            }

            return node;
        }

        private void OnEditClick(KBArticle article)
        {
            ShowEditorEmbedded(article);
        }

        private void OnShareClick(KBArticle article)
        {
            MessageBox.Show(
                string.Format("ฟีเจอร์แชร์บทความ \"{0}\"\n\nจะพัฒนาในอนาคต", article.Title),
                "Share Article",
                MessageBoxButtons.OK,
                MessageBoxIcon.Information);
        }

        private void OnSendReviewClick(KBArticle article)
        {
            var confirm = MessageBox.Show(
                string.Format("ต้องการส่งบทความ \"{0}\" ขอ Review ใช่หรือไม่?", article.Title),
                "ยืนยันส่งขอ Review",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            if (_presenter != null)
            {
                _presenter.SendArticleForReview(article.ArticleID);
                _presenter.LoadArticles();
                MessageBox.Show(
                    "ส่งขอ Review เรียบร้อยแล้ว",
                    "สำเร็จ",
                    MessageBoxButtons.OK,
                    MessageBoxIcon.Information);
            }
        }

        private void BtnPrevPage_Click(object sender, EventArgs e)
        {
            if (_presenter == null || _currentPage <= 1) return;
            _presenter.GoToPage(_currentPage - 1);
        }

        private void BtnNextPage_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            _presenter.GoToPage(_currentPage + 1);
        }

        private void BtnPageNumber_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            var btn = sender as Button;
            if (btn != null && btn.Tag != null)
                _presenter.GoToPage((int)btn.Tag);
        }

        private void CboPageSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            _presenter.LoadArticles();
        }

        private void BtnClearFilter_Click(object sender, EventArgs e)
        {
            cboStatus.SelectedIndex     = 0;
            cboType.SelectedIndex       = 0;
            cboDepartment.SelectedIndex = 0;
            cboSortBy.SelectedIndex     = 0;
            txtToolbarSearch.Text       = string.Empty;
            SelectedArticleTitle        = null;  // also clear any active title-subcategory filter
            if (_presenter != null) _presenter.LoadArticles();
        }

        private void ResizeArticleCards()
        {
            int cardWidth = flpArticles.ClientSize.Width - 16;
            if (cardWidth < 200) return;
            foreach (Control c in flpArticles.Controls)
                if (c is ArticleCardControl) c.Width = cardWidth;
        }

        // ── DataGridView setup ────────────────────────────────────────────────────

        private void InitializeArticleGrid()
        {
            dgvArticles.AutoGenerateColumns = false;
            dgvArticles.EnableHeadersVisualStyles = false;
            dgvArticles.ColumnHeadersHeight = 32;
            dgvArticles.RowTemplate.Height = 36;
            dgvArticles.DefaultCellStyle.SelectionBackColor = Color.FromArgb(214, 228, 240);
            dgvArticles.DefaultCellStyle.SelectionForeColor = Color.FromArgb(26, 35, 50);

            var hdrStyle = dgvArticles.ColumnHeadersDefaultCellStyle;
            hdrStyle.BackColor        = Color.FromArgb(240, 244, 249);
            hdrStyle.ForeColor        = Color.FromArgb(30, 58, 95);
            //hdrStyle.Font             = new Font("Segoe UI", 8.5f, FontStyle.Bold);
            hdrStyle.SelectionBackColor = Color.FromArgb(240, 244, 249);
            hdrStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
            //dgvArticles.DefaultCellStyle.Font                = new Font("Segoe UI", 8.5f);            

            var ctr = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter };
            var lft = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleLeft,
                                                   Padding  = new Padding(4, 0, 0, 0) };

            dgvArticles.Columns.Add(new DataGridViewTextBoxColumn {
                Name = "colRowNum", 
                HeaderText = "#", 
                Width = 25, 
                ReadOnly = true,
                DefaultCellStyle = ctr,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            //dgvArticles.Columns.Add(new DataGridViewTextBoxColumn
            //{
            //    Name = "colCode",
            //    HeaderText = "รหัส",
            //    //Width = 0,
            //    ReadOnly = true,
            //    Visible = false
            //});
            dgvArticles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCategory",
                HeaderText = "หมวดหมู่",
                //Width = 0,
                ReadOnly = true,
                Visible = false
                //DefaultCellStyle = lft,
                //AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            dgvArticles.Columns.Add(new DataGridViewTextBoxColumn {
                Name = "colTitle", 
                HeaderText = "อ้างอิง",
                ReadOnly = true,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(4, 0, 0, 0),
                    WrapMode = DataGridViewTriState.True
                },
            });
            dgvArticles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colCode",
                HeaderText = "",
                ReadOnly = true,
                Width = 250,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(4, 0, 0, 0),
                    WrapMode = DataGridViewTriState.True
                },

            });
            dgvArticles.Columns.Add(new DataGridViewLinkColumn
            {
                Name = "colContentHtml",
                HeaderText = "สาระ",
                ReadOnly = true,
                Width = 600,
                DefaultCellStyle = new DataGridViewCellStyle{
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(4, 0, 0, 0),
                    WrapMode = DataGridViewTriState.True
                }
            });
            dgvArticles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colEffectiveDate",
                HeaderText = "วันที่มีผลบังคับใช้",
                ReadOnly = true,
                DefaultCellStyle = { Format = "dd/MM/yyyy" },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            dgvArticles.Columns.Add(new CalendarColumn
            {
                Name             = "colExpiryDate",
                HeaderText       = "วันที่ยกเลิก",
                ReadOnly         = false,
                Width            = 110,
                DefaultCellStyle = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter }
            });
            dgvArticles.Columns.Add(new DataGridViewButtonColumn
            {
                Name                        = "colActive",
                HeaderText                  = "Active",
                Width                       = 70,
                FlatStyle                   = FlatStyle.Flat,
                UseColumnTextForButtonValue = false,
                DefaultCellStyle            = new DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleCenter },
                AutoSizeMode                = DataGridViewAutoSizeColumnMode.AllCells
            });
            dgvArticles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colOrgDivDept",
                HeaderText = "Org./Div./Dept.",
                ReadOnly = true,
                Width = 150,
                DefaultCellStyle = new DataGridViewCellStyle
                {
                    Alignment = DataGridViewContentAlignment.MiddleLeft,
                    Padding = new Padding(4, 0, 0, 0),
                    WrapMode = DataGridViewTriState.True,

                }
            });
            dgvArticles.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colTags",
                HeaderText = "Tags",
                ReadOnly = true,
                DefaultCellStyle = lft,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = false
            });
            //dgvArticles.Columns.Add(new DataGridViewTextBoxColumn
            //{
            //    Name = "colType",
            //    HeaderText = "ประเภท",
            //    Width = 78,
            //    ReadOnly = true,
            //    DefaultCellStyle = ctr
            //});
            //dgvArticles.Columns.Add(new DataGridViewTextBoxColumn {
            //    Name = "colStatus", HeaderText = "สถานะ", Width = 90, ReadOnly = true,
            //    DefaultCellStyle = ctr });

            dgvArticles.Columns.Add(new DataGridViewTextBoxColumn {
                Name = "colVersion", 
                HeaderText = "Ver.", 
                Width = 50, 
                ReadOnly = true,
                DefaultCellStyle = ctr,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = false
            });
            dgvArticles.Columns.Add(new DataGridViewTextBoxColumn {
                Name = "colViews",
                HeaderText = "👁",
                Width = 48,
                ReadOnly = true,
                DefaultCellStyle = ctr,
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells,
                Visible = false
            });
            dgvArticles.Columns.Add(new DataGridViewButtonColumn {
                Name = "colBtnRead", 
                HeaderText = "", 
                Width = 74, 
                FlatStyle = FlatStyle.Flat,
                UseColumnTextForButtonValue = false,
                DefaultCellStyle = new DataGridViewCellStyle {
                    BackColor = Color.FromArgb(46, 116, 181), 
                    ForeColor = Color.White,
                    //Font = new Font("Segoe UI", 8f),
                    Alignment = DataGridViewContentAlignment.MiddleCenter 
                },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            dgvArticles.Columns.Add(new DataGridViewButtonColumn {
                Name = "colBtnEdit", 
                HeaderText = "", 
                Width = 74, 
                FlatStyle = FlatStyle.Flat,
                UseColumnTextForButtonValue = false,
                DefaultCellStyle = new DataGridViewCellStyle {
                    BackColor = Color.FromArgb(204, 92, 30), 
                    ForeColor = Color.White,
                    //Font = new Font("Segoe UI", 8f),
                    Alignment = DataGridViewContentAlignment.MiddleCenter 
                },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            dgvArticles.Columns.Add(new DataGridViewButtonColumn {
                Name = "colBtnShare", 
                HeaderText = "", 
                Width = 74, 
                FlatStyle = FlatStyle.Flat,
                UseColumnTextForButtonValue = false,
                DefaultCellStyle = new DataGridViewCellStyle {
                    BackColor = Color.FromArgb(40, 140, 210), 
                    ForeColor = Color.White,
                    //Font = new Font("Segoe UI", 8f),
                    Alignment = DataGridViewContentAlignment.MiddleCenter 
                },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });
            dgvArticles.Columns.Add(new DataGridViewButtonColumn {
                Name = "colBtnReview", 
                HeaderText = "", 
                Width = 84, 
                FlatStyle = FlatStyle.Flat,
                UseColumnTextForButtonValue = false,
                DefaultCellStyle = new DataGridViewCellStyle {
                    BackColor = Color.FromArgb(39, 174, 96), 
                    ForeColor = Color.White,
                    //Font = new Font("Segoe UI", 8f),
                    Alignment = DataGridViewContentAlignment.MiddleCenter 
                },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });

            dgvArticles.Columns.Add(new DataGridViewButtonColumn
            {
                Name                        = "colBtnDelete",
                HeaderText                  = "",
                Width                       = 74,
                FlatStyle                   = FlatStyle.Flat,
                UseColumnTextForButtonValue = false,
                Visible                     = false,
                DefaultCellStyle            = new DataGridViewCellStyle
                {
                    BackColor = Color.FromArgb(192, 0, 0),
                    ForeColor = Color.White,
                    Alignment = DataGridViewContentAlignment.MiddleCenter
                },
                AutoSizeMode = DataGridViewAutoSizeColumnMode.AllCells
            });

            dgvArticles.CellDoubleClick += DgvArticles_CellDoubleClick;
            dgvArticles.CellClick       += DgvArticles_CellClick;
            dgvArticles.CellFormatting  += DgvArticles_CellFormatting;
            dgvArticles.CellEndEdit     += DgvArticles_CellEndEdit;
        }

        // ── DataGridView events ───────────────────────────────────────────────────

        private void DgvArticles_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _cachedArticles.Count) return;
            var article = _cachedArticles[e.RowIndex];
            string col  = dgvArticles.Columns[e.ColumnIndex].Name;

            if (col != "colContentHtml" && col != "colBtnRead" && col != "colBtnEdit" && col != "colBtnShare" && col != "colBtnReview" && article.Status == 1) OnArticleCardClick(article);
        }
        private void DgvArticles_CellClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _cachedArticles.Count) return;
            var article = _cachedArticles[e.RowIndex];
            string col = dgvArticles.Columns[e.ColumnIndex].Name;

            if      (col == "colContentHtml") OnArticleCardClick(article);
            else if (col == "colBtnRead")     OnArticleCardClick(article);
            else if (col == "colBtnEdit")     OnEditClick(article);
            else if (col == "colBtnShare")    OnShareClick(article);
            else if (col == "colBtnReview" && article.Status == 1) OnSendReviewClick(article);
            else if (col == "colActive")      OnToggleActiveClick(article, e.RowIndex, e.ColumnIndex);
            else if (col == "colBtnDelete")   OnDeleteClick(article);
        }

        private void OnToggleActiveClick(KBArticle article, int rowIndex, int colIndex)
        {
            if (IsArticleActive(article))
            {
                // Currently ON → set ExpiryDate = today to deactivate
                _pendingToggleArticleId = 0;
                _presenter.UpdateArticleExpiryDate(article.ArticleID, DateTime.Today);
                article.ExpiryDate = DateTime.Today;
                dgvArticles[colIndex, rowIndex].Value = "OFF";
                dgvArticles.InvalidateCell(colIndex, rowIndex);
                int expColIdx = dgvArticles.Columns["colExpiryDate"].Index;
                dgvArticles[expColIdx, rowIndex].Value = DateTime.Today;
            }
            else
            {
                // Currently OFF → focus ExpiryDate so user must set a future date
                _pendingToggleArticleId = article.ArticleID;
                int expColIdx = dgvArticles.Columns["colExpiryDate"].Index;
                dgvArticles.CurrentCell = dgvArticles[expColIdx, rowIndex];
                dgvArticles.BeginEdit(true);
            }
        }

        private void OnDeleteClick(KBArticle article)
        {
            var confirm = MessageBox.Show(
                string.Format("ต้องการลบบทความ \"{0}\" หรือไม่?\nการดำเนินการนี้ไม่สามารถย้อนกลับได้", article.Title),
                "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);
            if (confirm == DialogResult.Yes)
                _presenter.DeleteArticle(article.ArticleID);
        }

        private void DgvArticles_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0 || e.RowIndex >= _cachedArticles.Count) return;
            var article = _cachedArticles[e.RowIndex];
            string col  = dgvArticles.Columns[e.ColumnIndex].Name;

            if (col == "colRowNum")
            {
                e.Value = ((_currentPage - 1) * PageSize + e.RowIndex + 1).ToString();
                e.FormattingApplied = true;
            }
            else if (col == "colStatus")
            {
                switch (article.Status)
                {
                    case 1: e.CellStyle.ForeColor = Color.FromArgb(100, 100, 100); break; // Draft
                    case 2: e.CellStyle.ForeColor = Color.FromArgb(180, 90, 0);    break; // Review
                    case 3: e.CellStyle.ForeColor = Color.FromArgb(0, 128, 64);    break; // Published
                    case 4: e.CellStyle.ForeColor = Color.FromArgb(139, 0, 0);     break; // Archived
                }
                e.FormattingApplied = true;
            }
            else if (col == "colBtnReview" && article.Status != 1)
            {
                e.CellStyle.BackColor = Color.FromArgb(235, 237, 240);
                e.CellStyle.ForeColor = Color.FromArgb(180, 180, 180);
                e.FormattingApplied   = true;
            }
            else if (col == "colActive")
            {
                bool active = object.Equals(e.Value, "ON");
                e.CellStyle.BackColor = active
                    ? Color.FromArgb(39, 174, 96)
                    : Color.FromArgb(160, 160, 160);
                e.CellStyle.ForeColor = Color.White;
                e.FormattingApplied   = true;
            }
            else if (col == "colEffectiveDate")
            {
                if (e.Value is DateTime)
                {
                    e.Value = FormatBuddhistDate((DateTime)e.Value);
                    e.FormattingApplied = true;
                }
            }
        }

        /// <summary>ปฏิทินพุทธศักราชของ .NET เอง (ไม่ใช่ CultureInfo.CurrentCulture) ใช้คำนวณปี พ.ศ. โดยตรง
        /// ไม่พึ่งพา region/culture ของเครื่องที่รันเลย — บางเครื่องตั้ง Windows region เป็นไทยและ .NET จะ
        /// เซ็ต CultureInfo.CurrentCulture.Calendar เป็น ThaiBuddhistCalendar โดยอัตโนมัติอยู่แล้ว ถ้าใช้
        /// date.ToString("dd/MM/yyyy") เฉย ๆ (ไม่ระบุ culture) เครื่องแบบนั้นจะเห็น พ.ศ. อยู่แล้วโดยไม่ตั้งใจ
        /// และถ้าโค้ดไปบวก +543 ซ้ำเข้าไปอีกชั้นจะกลายเป็นปีผิดทันที (ค.ศ.+543+543) — ต่างเครื่อง/ต่าง region
        /// จึงได้ผลไม่ตรงกัน วิธีนี้ระบุปฏิทินตรง ๆ (ThaiBuddhistCalendar) และดึง Day/Month จาก DateTime
        /// (ซึ่ง .NET รับประกันว่าเป็นค่าคงที่ไม่ขึ้นกับ culture เสมอ) จึงได้ผลลัพธ์เดียวกันทุกเครื่องแน่นอน</summary>
        private static readonly Calendar ThaiBuddhistCalendarInstance = new ThaiBuddhistCalendar();

        private static string FormatBuddhistDate(DateTime date)
        {
            int buddhistYear = ThaiBuddhistCalendarInstance.GetYear(date);
            return string.Format(CultureInfo.InvariantCulture, "{0:00}/{1:00}/{2}", date.Day, date.Month, buddhistYear);
        }

        private void DgvArticles_CellEndEdit(object sender, DataGridViewCellEventArgs e)
        {
            if (dgvArticles.Columns[e.ColumnIndex].Name != "colExpiryDate") return;
            if (e.RowIndex < 0 || e.RowIndex >= _cachedArticles.Count) return;

            var article = _cachedArticles[e.RowIndex];
            bool isToggleToOn = _pendingToggleArticleId > 0 && article.ArticleID == _pendingToggleArticleId;
            _pendingToggleArticleId = 0;

            object rawVal     = dgvArticles[e.ColumnIndex, e.RowIndex].Value;
            DateTime? newDate = (rawVal is DateTime) ? (DateTime?)((DateTime)rawVal) : null;

            if (isToggleToOn && (!newDate.HasValue || newDate.Value.Date <= DateTime.Today))
            {
                // OFF → ON toggle requires a future date; nothing picked or still in the
                // past — toggle stays OFF, no DB update.
                return;
            }

            _presenter.UpdateArticleExpiryDate(article.ArticleID, newDate);
            article.ExpiryDate = newDate;

            int activeIdx = dgvArticles.Columns["colActive"].Index;
            dgvArticles[activeIdx, e.RowIndex].Value = IsArticleActive(article) ? "ON" : "OFF";
            dgvArticles.InvalidateCell(activeIdx, e.RowIndex);
        }

        private void cboType_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            _presenter.LoadArticles();
        }

        // ── Custom DateTimePicker column for colExpiryDate ──────────────────────

        private class CalendarEditingControl : DateTimePicker, IDataGridViewEditingControl
        {
            private DataGridView _dgv;
            private bool _valueChanged;
            private int  _rowIndex;

            public CalendarEditingControl()
            {
                Format       = DateTimePickerFormat.Short;
                ShowCheckBox = true;
            }

            public DataGridView EditingControlDataGridView
            {
                get { return _dgv; }
                set { _dgv = value; }
            }
            public int EditingControlRowIndex
            {
                get { return _rowIndex; }
                set { _rowIndex = value; }
            }
            public bool EditingControlValueChanged
            {
                get { return _valueChanged; }
                set { _valueChanged = value; }
            }
            public Cursor EditingPanelCursor { get { return Cursors.Default; } }
            public bool   RepositionEditingControlOnValueChange { get { return false; } }

            public object EditingControlFormattedValue
            {
                get  { return Checked ? (object)Value : null; }
                set
                {
                    if (value is DateTime)
                    {
                        Checked = true;
                        Value   = (DateTime)value;
                    }
                    else { Checked = false; }
                }
            }
            public object GetEditingControlFormattedValue(DataGridViewDataErrorContexts ctx)
                => EditingControlFormattedValue;
            public void ApplyCellStyleToEditingControl(DataGridViewCellStyle s) { }
            public bool EditingControlWantsInputKey(Keys k, bool dgvWantsIt) { return false; }
            public void PrepareEditingControlForEdit(bool selectAll) { }

            protected override void OnValueChanged(EventArgs e)
            {
                _valueChanged = true;
                if (_dgv != null)
                {
                    _dgv.NotifyCurrentCellDirty(true);
                    // Commit the edit as soon as a date is picked instead of waiting for
                    // the user to click elsewhere. Deferred via BeginInvoke so the grid
                    // doesn't tear down this control while it's still raising this event.
                    var dgv = _dgv;
                    BeginInvoke(new MethodInvoker(() =>
                    {
                        if (dgv.CurrentCell != null) dgv.EndEdit();
                    }));
                }
                base.OnValueChanged(e);
            }
        }

        private class CalendarCell : DataGridViewTextBoxCell
        {
            public override Type EditType  { get { return typeof(CalendarEditingControl); } }
            public override Type ValueType { get { return typeof(object); } }

            public override void InitializeEditingControl(
                int rowIndex, object initialFormattedValue, DataGridViewCellStyle style)
            {
                base.InitializeEditingControl(rowIndex, initialFormattedValue, style);
                var ctl = DataGridView.EditingControl as CalendarEditingControl;
                if (ctl == null) return;
                if (Value is DateTime)
                {
                    ctl.Checked = true;
                    ctl.Value   = (DateTime)Value;
                }
                else { ctl.Checked = false; }
            }

            public override object ParseFormattedValue(
                object formattedValue,
                DataGridViewCellStyle cellStyle,
                System.ComponentModel.TypeConverter formattedValueTypeConverter,
                System.ComponentModel.TypeConverter valueTypeConverter)
            {
                if (formattedValue is DateTime) return formattedValue;
                return null;
            }

            protected override object GetFormattedValue(
                object value, int rowIndex,
                ref DataGridViewCellStyle cellStyle,
                System.ComponentModel.TypeConverter valueTypeConverter,
                System.ComponentModel.TypeConverter formattedValueTypeConverter,
                DataGridViewDataErrorContexts context)
            {
                if (value is DateTime) return FormatBuddhistDate((DateTime)value);
                return string.Empty;
            }
        }

        private class CalendarColumn : DataGridViewColumn
        {
            public CalendarColumn() : base(new CalendarCell()) { }
        }

        // ── Natural sort: "มาตรา2" < "มาตรา10" ──────────────────────────────────
        private sealed class NaturalStringComparer : IComparer<string>
        {
            public static readonly NaturalStringComparer Instance = new NaturalStringComparer();

            public int Compare(string x, string y)
            {
                if (x == null && y == null) return 0;
                if (x == null) return -1;
                if (y == null) return 1;

                int ix = 0, iy = 0;
                while (ix < x.Length && iy < y.Length)
                {
                    bool xIsDigit = char.IsDigit(x[ix]);
                    bool yIsDigit = char.IsDigit(y[iy]);

                    if (xIsDigit && yIsDigit)
                    {
                        // compare numeric chunks as integers
                        int numX = 0, numY = 0;
                        while (ix < x.Length && char.IsDigit(x[ix])) numX = numX * 10 + (x[ix++] - '0');
                        while (iy < y.Length && char.IsDigit(y[iy])) numY = numY * 10 + (y[iy++] - '0');
                        int cmp = numX.CompareTo(numY);
                        if (cmp != 0) return cmp;
                    }
                    else
                    {
                        int cmp = string.Compare(x[ix].ToString(), y[iy].ToString(),
                                                 StringComparison.OrdinalIgnoreCase);
                        if (cmp != 0) return cmp;
                        ix++; iy++;
                    }
                }
                return x.Length - y.Length;
            }
        }
    }
}
