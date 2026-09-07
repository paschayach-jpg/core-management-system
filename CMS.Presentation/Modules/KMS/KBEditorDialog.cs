// ✅ Allowed usings: CMS.Controllers, CMS.Domain
// ❌ Forbidden usings: CMS.Services, CMS.Data
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Controllers.KMS.KB;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// KBEditor Dialog — แก้ไข/เพิ่มบทความ Knowledge Base (MOD-006)
    /// Redesigned from ArticleEditorDialog based on kms_editor.html
    /// Layout: General Info, Content Editor, Mapping Tabs (Attachments, Related, Workflow, Programs, Docs, Versions)
    /// Supports: Create / Edit / Send Review / Publish / Archive + Version History
    /// </summary>
    public partial class KBEditorDialog : Form, IKBEditorController, IMessageFilter
    {
        public event EventHandler BackRequested;

        private KBEditorController _presenter;
        private readonly UserContext _user;
        private readonly IFileUploadController _fileUploadController;
        private readonly IUploadCheckpointController _checkpointController;
        private readonly IFileBrowseController _fileBrowseController;
        private readonly IFolderCommandController _folderCommandController;
        private readonly IFolderReadLookupController _folderReadLookupController;
        private readonly IFileCommandController _fileCommandController;
        private readonly CMS.Infrastructure.Interfaces.INetworkDriveHelper _networkDriveHelper;

        public KBEditorDialog(UserContext user) : this(user, null, null, null) { }

        public KBEditorDialog(UserContext user, IFileUploadController fileUploadController, IUploadCheckpointController checkpointController, IFileBrowseController fileBrowseController = null,
            IFolderCommandController folderCommandController = null, IFolderReadLookupController folderReadLookupController = null,
            IFileCommandController fileCommandController = null,
            CMS.Infrastructure.Interfaces.INetworkDriveHelper networkDriveHelper = null)
        {
            if (user == null) throw new ArgumentNullException("user");
            _user = user;
            _fileUploadController = fileUploadController;
            _checkpointController = checkpointController;
            _fileBrowseController = fileBrowseController;
            _folderCommandController = folderCommandController;
            _folderReadLookupController = folderReadLookupController;
            _fileCommandController = fileCommandController;
            _networkDriveHelper = networkDriveHelper;
            InitializeComponent();
        }

        /// <summary>
        /// Called by the composition root after the presenter is wired.
        /// Must be called before the dialog is shown.
        /// </summary>
        public void AttachPresenter(KBEditorController presenter)
        {
            if (presenter == null) throw new ArgumentNullException("presenter");
            _presenter = presenter;
        }

        // ── Form Events ──────────────────────────────────────────────────────

        private void KBEditorDialog_Load(object sender, EventArgs e)
        {
            if (_presenter == null)
            {
                MessageBox.Show(
                    "Presenter ยังไม่ได้ถูก Attach\n\nกรุณาเรียก AttachPresenter() ก่อนเปิด Dialog",
                    "ข้อผิดพลาดภายใน", MessageBoxButtons.OK, MessageBoxIcon.Error);
                DialogResult = DialogResult.Cancel;
                Close();
                return;
            }

            // Bug-fix: splitMain.Dock=Top gives it a fixed height (1166px in Designer),
            // which overflows pnlScroll (AutoScroll=True, ~880px on a 1080p screen).
            // When webEditor receives focus WinForms calls ScrollControlIntoView(),
            // causing the view to jump to the bottom.  Switching to Dock=Fill makes
            // splitMain always match pnlScroll's client area — no overflow, no jump.
            splitMain.Dock = DockStyle.Fill;

            InitWebEditor();
            tsRteZoom.Text = _zoomPct + "%"; // default 100%

            // Intercept Ctrl+MouseWheel before the hosted browser can zoom itself.
            // Message filters are application-wide, so it is removed again on dispose —
            // this dialog is used both modally and embedded in KBMainForm's panel.
            Application.AddMessageFilter(this);
            Disposed += (s, ev) => Application.RemoveMessageFilter(this);
            InitTypeCombo();
            InitCrafCombo();
            InitStatusCombo();
            
            // Initialize tabPrograms - NEW IMPLEMENTATION (Mockup)
            InitializeProgramsDataGridViews();
            LoadLinkedPrograms();
            
            // Initialize tabWorkflow - NEW IMPLEMENTATION (Mockup)
            InitializeWorkflowDataGridViews();
            LoadLinkedWorkflows();
            
            // Initialize tabAttach - NEW IMPLEMENTATION (Mockup with DataGridView)
            InitializeAttachDataGridView();
            LoadAttachedFiles();
            
            // Initialize tabDocs - NEW IMPLEMENTATION (Mockup)
            InitializeDocsDataGridViews();
            LoadLinkedDocuments();
            
            // Initialize tabRelated - NEW IMPLEMENTATION with DataGridView
            InitializeRelatedDataGridViews();
            InitializeRelatedCategoryDropdown();
            LoadRelatedArticles();
            
            // Initialize tabDepartments - NEW IMPLEMENTATION (Mockup)
            InitializeDepartmentsDataGridView();
            LoadLinkedDepartments();
            
            _presenter.Initialize();
        }

        // When the user closes the dialog via the X button (or Alt+F4) after a successful
        // Save Draft, we still need to return OK so ArticleViewerForm.BtnEdit_Click knows
        // to call RefreshAfterEdit().  SendReview/Publish/Archive already call CloseOk()
        // themselves, so this only affects the "save then manually close" path.
        private void KBEditorDialog_FormClosing(object sender, System.Windows.Forms.FormClosingEventArgs e)
        {
            if (_savedSuccessfully && DialogResult != DialogResult.OK)
                DialogResult = DialogResult.OK;
        }

        private void BtnSaveDraft_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (!ValidateInput()) return;

            try
            {
                _presenter.Save(txtChangeDesc.Text.Trim());
                _savedSuccessfully = true;
                _originalContentHtml = ContentHTML;

                // Check if already linked
                foreach (DataGridViewRow row in dgvRelatedLinked.Rows)
                {
                    try
                    {
                        _presenter.AddRelatedArticleLink((int)row.Tag);
                    }
                    catch (Exception)
                    {

                        continue;
                    }
                }
            }
            catch (Exception ex)
            {
                ShowError("บันทึกไม่สำเร็จ:\n" + ex.Message);
            }
        }

        private void BtnSendReview_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (!ValidateInput()) return;

            var confirm = MessageBox.Show(
                string.Format("ต้องการส่งขอ Review บทความ \"{0}\" ใช่หรือไม่?", txtTitle.Text.Trim()),
                "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                _presenter.SendReview();
            }
            catch (Exception ex)
            {
                ShowError("เกิดข้อผิดพลาด:\n" + ex.Message);
            }
        }

        // Any user can currently click this while the article is in Review status —
        // there is no reviewer/approver restriction yet. Approval logic (who is allowed
        // to publish, audit trail, etc.) is intended to be added on top of this later.
        private void BtnPublish_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;

            var confirm = MessageBox.Show(
                string.Format("ต้องการเผยแพร่บทความ \"{0}\" ใช่หรือไม่?", txtTitle.Text.Trim()),
                "ยืนยัน Publish", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                _presenter.Publish();
            }
            catch (Exception ex)
            {
                ShowError("เกิดข้อผิดพลาด:\n" + ex.Message);
            }
        }

        private void BtnArchive_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;

            var confirm = MessageBox.Show(
                string.Format(
                    "ต้องการ Archive บทความ \"{0}\" ใช่หรือไม่?\n\n" +
                    "(บทความที่ Archive แล้วไม่สามารถนำกลับมาแก้ไขได้)",
                    txtTitle.Text.Trim()),
                "ยืนยัน Archive", MessageBoxButtons.YesNo, MessageBoxIcon.Warning);

            if (confirm != DialogResult.Yes) return;

            try
            {
                _presenter.Archive();
            }
            catch (Exception ex)
            {
                ShowError("เกิดข้อผิดพลาด:\n" + ex.Message);
            }
        }

        private void BtnPreview_Click(object sender, EventArgs e)
        {
            string html = ContentHTML;
            string tmpPath = System.IO.Path.Combine(
                System.IO.Path.GetTempPath(),
                "kms_preview_" + System.IO.Path.GetRandomFileName() + ".html");

            string fullHtml =
                "<!DOCTYPE html><html><head><meta charset='utf-8'>" +
                "<title>Preview — " + System.Net.WebUtility.HtmlEncode(txtTitle.Text.Trim()) + "</title>" +
                "<style>body{font-family:'Segoe UI',Tahoma,sans-serif;font-size:14px;margin:24px;" +
                "line-height:1.6;color:#333;max-width:900px;}" +
                "h1{font-size:22px;}h2{font-size:18px;}h3{font-size:15px;}" +
                "table{border-collapse:collapse;width:100%;margin:8px 0;}" +
                "td,th{border:1px solid #ccc;padding:4px 8px;}th{background:#f0f0f0;}" +
                "pre{background:#f4f4f4;padding:8px;border-radius:4px;border:1px solid #ddd;" +
                "font-family:Consolas,monospace;overflow:auto;}" +
                "a{color:#2980b9;}</style></head><body>" +
                "<h2>" + System.Net.WebUtility.HtmlEncode(txtTitle.Text.Trim()) + "</h2>" +
                html + "</body></html>";

            try
            {
                System.IO.File.WriteAllText(tmpPath, fullHtml, System.Text.Encoding.UTF8);
                System.Diagnostics.Process.Start(tmpPath);
            }
            catch (Exception ex)
            {
                ShowError("ไม่สามารถเปิด Preview ได้:\n" + ex.Message);
            }
        }

        private bool ShouldConfirmBeforeBack()
        {
            if (_presenter == null) return false;
            if (ContentText.Length == 0) return false;

            bool isNewArticle = _presenter.EditingArticle == null;
            if (isNewArticle)
                return !_savedSuccessfully;

            return ContentHTML != _originalContentHtml;
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (ShouldConfirmBeforeBack())
            {
                var result = MessageBox.Show(
                    "ต้องการบันทึกข้อมูลก่อนออก?",
                    "ยืนยัน",
                    MessageBoxButtons.YesNoCancel,
                    MessageBoxIcon.Question);

                if (result == DialogResult.Cancel)
                    return;

                if (result == DialogResult.Yes)
                {
                    if (!ValidateInput()) return;
                    try
                    {
                        _presenter.Save(txtChangeDesc.Text.Trim());
                        _savedSuccessfully = true;
                        _originalContentHtml = ContentHTML;
                    }
                    catch (Exception ex)
                    {
                        ShowError("บันทึกไม่สำเร็จ:\n" + ex.Message);
                        return;
                    }
                }
            }

            if (BackRequested != null)
                BackRequested(this, EventArgs.Empty);
            else
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            if (BackRequested != null)
                BackRequested(this, EventArgs.Empty);
            else
            {
                DialogResult = DialogResult.Cancel;
                Close();
            }
        }

        private void btnCollapseGenInfo_Click(object sender, EventArgs e)
        {
            bool isExpanded = splitTopBottom.SplitterDistance > 28;
            if (isExpanded)
            {
                _genInfoExpandedHeight = splitTopBottom.SplitterDistance;
                splitTopBottom.Panel1MinSize = 0;
                splitTopBottom.SplitterDistance = 28;
                btnCollapseGenInfo.Text = "▼";
            }
            else
            {
                splitTopBottom.Panel1MinSize = 220;
                splitTopBottom.SplitterDistance = _genInfoExpandedHeight;
                btnCollapseGenInfo.Text = "▲";
            }
        }

        private void btnCollapseMappingPanel_Click(object sender, EventArgs e)
        {
            int collapsedWidth = btnCollapseMappingPanel.Width;
            int panel2Width = splitMain.Width - splitMain.SplitterDistance - splitMain.SplitterWidth;
            bool isExpanded = panel2Width > collapsedWidth;
            if (isExpanded)
            {
                _mappingExpandedWidth = panel2Width;
                splitMain.Panel2MinSize = 0;
                splitMain.SplitterDistance = splitMain.Width - splitMain.SplitterWidth - collapsedWidth;
                btnCollapseMappingPanel.Text = "◀";
            }
            else
            {
                splitMain.Panel2MinSize = 400;
                splitMain.SplitterDistance = splitMain.Width - splitMain.SplitterWidth - _mappingExpandedWidth;
                btnCollapseMappingPanel.Text = "▶";
            }
        }

        // ── IKBEditorView Implementation ─────────────────────────────────────

        public string ArticleCode
        {
            get { return txtCode.Text.Trim(); }
            set { txtCode.Text = value; }
        }

        public string Title
        {
            get { return txtTitle.Text.Trim(); }
            set { txtTitle.Text = value; }
        }

        public string Summary
        {
            get { return txtSummary.Text.Trim(); }
            set { txtSummary.Text = value; }
        }

        private string _pendingHtml   = string.Empty;
        private bool   _editorReady   = false;
        private int    _genInfoExpandedHeight  = 220;
        private int    _mappingExpandedWidth   = 529;
        private bool   _syncingToolbar = false;
        private bool   _applyingFormat = false;
        // C# is the single source of truth for the zoom level. Both ways of changing it —
        // the toolbar, and Ctrl+MouseWheel via PreFilterMessage — go through SetZoom and are
        // clamped to these bounds, so tsRteZoom can never drift out of sync with the page.
        private const int ZoomMinPct = 50;
        private const int ZoomMaxPct = 200;
        private int    _zoomPct        = 100;
        private static readonly int[] _zoomLevels = { 50, 75, 100, 125, 150, 175, 200 };
        private bool   _savedSuccessfully = false;
        private string _originalContentHtml = string.Empty;

        public string ContentHTML
        {
            get
            {
                if (!_editorReady || webEditor.Document == null || webEditor.Document.Body == null)
                    return _pendingHtml;
                return webEditor.Document.Body.InnerHtml ?? string.Empty;
            }
            set
            {
                _pendingHtml = NormalizeLegacyEditorHtml(value ?? string.Empty);
                if (_editorReady && webEditor.Document != null && webEditor.Document.Body != null)
                    webEditor.Document.Body.InnerHtml = _pendingHtml;
            }
        }

        // Tab markers used to be a <span data-kms-tab contenteditable="false"> element.
        // MSHTML treats any non-editable element inside an editable body as an embedded
        // "control" and draws a hatched border with resize handles around it, so markers
        // saved by those builds surface as stray boxes in the middle of the text.
        // Convert them back to the plain &nbsp; run this editor now uses, and strip any
        // other contenteditable="false" left in stored content for the same reason.
        // Normalising on load means the next Save persists the cleaned-up HTML.
        private static readonly System.Text.RegularExpressions.Regex _legacyTabSpanRx =
            new System.Text.RegularExpressions.Regex(
                @"<span[^>]*\bdata-kms-tab\b[^>]*>.*?</span>",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase |
                System.Text.RegularExpressions.RegexOptions.Singleline);

        private static readonly System.Text.RegularExpressions.Regex _ceFalseAttrRx =
            new System.Text.RegularExpressions.Regex(
                @"\s*contenteditable\s*=\s*([""']?)false\1",
                System.Text.RegularExpressions.RegexOptions.IgnoreCase);

        private static string NormalizeLegacyEditorHtml(string html)
        {
            if (string.IsNullOrEmpty(html)) return html;
            html = _legacyTabSpanRx.Replace(html, "&nbsp;&nbsp;&nbsp;&nbsp;");
            html = _ceFalseAttrRx.Replace(html, string.Empty);
            return html;
        }

        private string ContentText
        {
            get
            {
                if (!_editorReady || webEditor.Document == null || webEditor.Document.Body == null)
                    return _pendingHtml;
                return webEditor.Document.Body.InnerText ?? string.Empty;
            }
        }

        public string Tags
        {
            get
            {
                var tags = new List<string>();
                foreach (Control ctrl in flpTags.Controls)
                {
                    if (ctrl is Panel chip && chip != txtTagInput.Parent)
                    {
                        var lbl = chip.Controls.OfType<Label>().FirstOrDefault();
                        if (lbl != null) tags.Add(lbl.Text);
                    }
                }
                return string.Join(", ", tags);
            }
            set
            {
                // Clear existing chips (keep input box)
                for (int i = flpTags.Controls.Count - 1; i >= 0; i--)
                {
                    if (flpTags.Controls[i] != txtTagInput)
                        flpTags.Controls.RemoveAt(i);
                }
                
                // Add chips from comma-separated string (skip validation for existing data)
                if (!string.IsNullOrEmpty(value))
                {
                    foreach (var tag in value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        AddTagChip(tag.Trim(), skipValidation: true);
                    }
                }
            }
        }

        public bool IsOfficialPolicy
        {
            get { return false; }
            set { }
        }

        public bool NoExpiry
        {
            get { return true; }
            set { }
        }

        public DateTime? EffectiveDate
        {
            get { return dtpEffective.Value; }
            set { if (value.HasValue) dtpEffective.Value = value.Value; }
        }

        public DateTime? ExpiryDate
        {
            get { return dtpExpiry.Value; }
            set { if (value.HasValue) dtpExpiry.Value = value.Value; }
        }

        public string SelectedCrafLevel
        {
            get
            {
                var item = cboCraf.SelectedItem as ComboItem;
                return item != null ? item.Display : string.Empty;
            }
            set
            {
                for (int i = 0; i < cboCraf.Items.Count; i++)
                {
                    var ci = cboCraf.Items[i] as ComboItem;
                    if (ci != null && ci.Display == value)
                    {
                        cboCraf.SelectedIndex = i;
                        return;
                    }
                }
                if (cboCraf.Items.Count > 0)
                    cboCraf.SelectedIndex = 0;
            }
        }

        public int SelectedCrafLevelCode
        {
            get
            {
                var item = cboCraf.SelectedItem as ComboItem;
                return item != null ? item.Value : (int)ConfidentialityLevel.HC;
            }
            set { SelectComboByValue(cboCraf, value); }
        }

        public int SelectedCategoryId
        {
            get
            {
                var ci = cboCategory.SelectedItem as ComboItem;
                return ci != null ? ci.Value : 0;
            }
            set { SelectComboByValue(cboCategory, value); }
        }

        public byte SelectedArticleType
        {
            get
            {
                var ci = cboType.SelectedItem as ComboItem;
                return ci != null ? (byte)ci.Value : (byte)1;
            }
            set { SelectComboByValue(cboType, value); }
        }

        public byte SelectedStatus
        {
            get
            {
                var ci = cboStatus.SelectedItem as ComboItem;
                return ci != null ? (byte)ci.Value : _currentStatus;
            }
            set
            {
                _currentStatus = value;
                SelectComboByValue(cboStatus, value);
                SetStatusDisplay(value);  // Still update status bar
            }
        }
        private byte _currentStatus = 1;

        public bool StatusEnabled
        {
            set { cboStatus.Enabled = value; }
        }

        /// <summary>
        /// Restrict the status dropdown to a subset of statuses (e.g. an article in
        /// Published status cannot be moved back to Draft/Review from here).
        /// The currently selected status is preserved if it remains in the allowed set.
        /// </summary>
        public void SetAllowedStatuses(IEnumerable<byte> allowedStatuses)
        {
            var allowed = new HashSet<byte>(allowedStatuses);
            byte current = SelectedStatus;

            cboStatus.Items.Clear();
            foreach (var item in _statusOptions)
            {
                if (allowed.Contains((byte)item.Value))
                    cboStatus.Items.Add(item);
            }
            SelectComboByValue(cboStatus, current);
        }

        public string HeaderText
        {
            set { lblHeader.Text = value; }
        }

        public string ViewCountText
        {
            set { lblViewCount.Text = value; }
        }

        public string LikeCountText
        {
            set { lblLikeCount.Text = value; }
        }

        public bool PublishButtonVisible
        {
            set { btnSendReview.Visible = value; }
        }

        public bool ArchiveButtonVisible
        {
            set { btnArchive.Visible = value; }
        }

        public bool PublishNowButtonVisible
        {
            set { btnPublish.Visible = value; }
        }

        public bool SaveEnabled
        {
            set { btnSaveDraft.Enabled = value; }
        }

        public string PublishButtonText
        {
            set { btnSendReview.Text = value; }
        }

        public Color PublishButtonColor
        {
            set { btnSendReview.BackColor = value; }
        }

        public string VersionText
        {
            set { lblVerInfo.Text = value; }
        }

        public string CurVersionText
        {
            set { lblCurVersion.Text = value; }
        }

        public string AuthorText
        {
            set { lblAuthorInfo.Text = value; }
        }

        public string DateInfoText
        {
            set { lblDateInfo.Text = value; }
        }

        public void BindCategories(IEnumerable<KBCategory> categories)
        {
            cboCategory.Items.Clear();
            cboCategory.Items.Add(new ComboItem("— เลือกหมวดหมู่ —", 0));

            foreach (var c in categories.OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName))
            {
                cboCategory.Items.Add(new ComboItem(
                    c.ParentCategoryID.HasValue
                        ? string.Format("  └ {0}", c.CategoryName)
                        : c.CategoryName,
                    c.CategoryID));
            }

            cboCategory.SelectedIndex = 0;
        }

        public void CloseOk()
        {
            if (BackRequested != null)
                BackRequested(this, EventArgs.Empty);
            else
            {
                DialogResult = DialogResult.OK;
                Close();
            }
        }

        public void CloseCancel()
        {
            if (BackRequested != null)
                BackRequested(this, EventArgs.Empty);
            else
            {
                DialogResult = DialogResult.Cancel;
                Close();
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

        // ── DataGridView Access for Article Links ────────────────────────────────

        public System.Windows.Forms.DataGridView GetRelatedDataGridView()
        {
            return dgvRelatedLinked;
        }

        public System.Windows.Forms.DataGridView GetAttachLinkedDataGridView()
        {
            return dgvAttachLinked;
        }

        public System.Windows.Forms.DataGridView GetWorkflowLinkedDataGridView()
        {
            return dgvWorkflowLinked;
        }
        
        public System.Windows.Forms.DataGridView GetWorkflowSearchDataGridView()
        {
            return dgvWorkflowSearch;
        }

        public System.Windows.Forms.DataGridView GetProgramsLinkedDataGridView()
        {
            return dgvProgramsLinked;
        }
        
        public System.Windows.Forms.DataGridView GetProgramsSearchDataGridView()
        {
            return dgvProgramsSearch;
        }

        public System.Windows.Forms.DataGridView GetDocsLinkedDataGridView()
        {
            return dgvDocsLinked;
        }
        
        public System.Windows.Forms.DataGridView GetDocsSearchDataGridView()
        {
            return dgvDocsSearch;
        }

        // ── Private UI Helpers ───────────────────────────────────────────────

        private void InitTypeCombo()
        {
            cboType.Items.Clear();
            cboType.Items.Add(new ComboItem("Wiki Page", 5));
            cboType.SelectedIndex = 0;
        }

        // Master list of status options; SetAllowedStatuses() filters a subset of these
        // into cboStatus.Items while keeping the value-to-status-code mapping stable.
        private static readonly ComboItem[] _statusOptions =
        {
            new ComboItem("📝 Draft", 1),
            new ComboItem("👁️ Review", 2),
            new ComboItem("✅ Published", 3),
            new ComboItem("📦 Archived", 4),
        };

        private void InitStatusCombo()
        {
            cboStatus.Items.Clear();
            cboStatus.Items.AddRange(_statusOptions);
            cboStatus.SelectedIndex = 0;
        }

        private void InitCrafCombo()
        {
            cboCraf.Items.Clear();
            cboCraf.Items.Add(new ComboItem("--- เลือกระดับชั้นความลับ ---",   0));
            cboCraf.Items.Add(new ComboItem(ConfidentialityLevel.HC.ToString() + " - (High Confidential)",   (int)ConfidentialityLevel.HC));
            cboCraf.Items.Add(new ComboItem(ConfidentialityLevel.SC.ToString() + " - (Semi Confidential)",   (int)ConfidentialityLevel.SC));
            cboCraf.Items.Add(new ComboItem(ConfidentialityLevel.NC.ToString() + " - (Normal Confidential)", (int)ConfidentialityLevel.NC));
            cboCraf.Items.Add(new ComboItem(ConfidentialityLevel.PC.ToString() + " - (Public Confidential)", (int)ConfidentialityLevel.PC));
            cboCraf.SelectedIndex = 0;
        }

        private void SetStatusDisplay(byte status)
        {
            // Update status bar info only (removed lblStatusDisplay references)
            switch (status)
            {
                case 1:
                    lblStatusInfo.Text = "📝 Draft";
                    break;
                case 2:
                    lblStatusInfo.Text = "✅ Active";
                    break;
                case 3:
                    lblStatusInfo.Text = "📦 Expire";
                    break;
                default:
                    lblStatusInfo.Text = "—";
                    break;
            }
        }

        private bool ValidateInput()
        {
            if (string.IsNullOrWhiteSpace(txtTitle.Text))
            {
                ShowInfo("กรุณากรอกชื่อบทความ");
                txtTitle.Focus();
                return false;
            }

            var catItem = cboCategory.SelectedItem as ComboItem;
            if (catItem == null || catItem.Value == 0)
            {
                ShowInfo("กรุณาเลือกหมวดหมู่");
                cboCategory.Focus();
                return false;
            }

            return true;
        }

        private void SelectComboByValue(ComboBox cbo, int value)
        {
            for (int i = 0; i < cbo.Items.Count; i++)
            {
                var ci = cbo.Items[i] as ComboItem;
                if (ci != null && ci.Value == value)
                {
                    cbo.SelectedIndex = i;
                    return;
                }
            }
        }

        // ── WebBrowser Initialisation ─────────────────────────────────────────────

        private void InitWebEditor()
        {
            webEditor.DocumentText = GetEditorHtml();
        }

        private void WebEditor_DocumentCompleted(object sender, WebBrowserDocumentCompletedEventArgs e)
        {
            if (_editorReady) return;
            if (webEditor.Document == null || webEditor.Document.Body == null) return;
            _editorReady = true;

            // Inject pending HTML if the presenter already called ContentHTML = "..." before init finished
            if (!string.IsNullOrEmpty(_pendingHtml))
                webEditor.Document.Body.InnerHtml = _pendingHtml;
            _originalContentHtml = webEditor.Document.Body.InnerHtml ?? string.Empty;
            UpdateEditorStatus();

            // KeyUp fires after the browser updates the selection — giving accurate queryCommandState results.
            // onselectionchange fires on any cursor movement (click, arrow keys, Shift+select, etc.)
            webEditor.Document.Body.KeyUp += (s, ev) => { UpdateEditorStatus(); SyncToolbarState(); };
            webEditor.Document.AttachEventHandler("onselectionchange", (s, ev) => SyncToolbarState());

            // Attach link click handler to open links in external browser
            webEditor.Document.Click += WebEditor_Document_Click;

            SyncToolbarState(); // sync once immediately after load
        }

        private void WebEditor_Document_Click(object sender, HtmlElementEventArgs e)
        {
            if (webEditor.Document == null) return;

            var element = webEditor.Document.GetElementFromPoint(e.ClientMousePosition);
            if (element == null) { SyncToolbarState(); return; }

            // Check if clicked element or its parent is a link
            while (element != null)
            {
                if (element.TagName != null && element.TagName.ToUpper() == "A")
                {
                    string href = element.GetAttribute("href");
                    if (!string.IsNullOrEmpty(href) && (href.StartsWith("http://") || href.StartsWith("https://")))
                    {
                        e.ReturnValue = false; // Prevent default navigation
                        try { System.Diagnostics.Process.Start(href); }
                        catch (Exception ex) { ShowError("ไม่สามารถเปิดลิงก์ได้:\n" + ex.Message); }
                    }
                    break;
                }
                element = element.Parent;
            }

            SyncToolbarState(); // update toolbar after cursor placement
        }

        // ── Tab key interception for webEditor ───────────────────────────────────
        // Two independent paths capture Tab, and exactly one runs per keypress:
        //   1. This ProcessCmdKey override — when WinForms pre-translates the key.
        //      Returning true consumes the message, so MSHTML never sees the keydown.
        //   2. document.onkeydown inside GetEditorHtml() — when the key goes straight
        //      to MSHTML instead (e.g. the embedded TopLevel=false hosting path).
        // Keep this body minimal: it runs inside keyboard pre-translation, where extra
        // synchronous COM calls into MSHTML can throw and escape before `return true`,
        // which silently un-consumes the key and lets IE fall back to moving focus.
        // The status bar / toolbar are refreshed by the Document.Body.KeyUp handler.

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        private static extern IntPtr GetFocus();

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool IsChild(IntPtr hWndParent, IntPtr hWnd);

        private bool WebEditorContentHasFocus()
        {
            var hwnd = GetFocus();
            return hwnd != IntPtr.Zero &&
                   (hwnd == webEditor.Handle || IsChild(webEditor.Handle, hwnd));
        }

        protected override bool ProcessCmdKey(ref Message msg, Keys keyData)
        {
            const int WM_KEYDOWN = 0x0100;
            if (msg.Msg == WM_KEYDOWN && WebEditorContentHasFocus() && _editorReady && webEditor.Document != null)
            {
                if (keyData == Keys.Tab)
                {
                    try { webEditor.Document.InvokeScript("kmsTabIndent"); } catch { }
                    return true;
                }
                if (keyData == (Keys.Tab | Keys.Shift))
                {
                    try { webEditor.Document.InvokeScript("kmsTabOutdent"); } catch { }
                    return true;
                }
            }
            return base.ProcessCmdKey(ref msg, keyData);
        }

        // ── RTE Toolbar Handlers ─────────────────────────────────────────────

        // ── Tag Chip Management ──────────────────────────────────────────────

        private void CboStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            var ci = cboStatus.SelectedItem as ComboItem;
            if (ci == null) return;
            byte status = (byte)ci.Value;
            _currentStatus = status;
            SetStatusDisplay(status);
        }

        private void TxtTagInput_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                AddTagChip(txtTagInput.Text.Trim());
                txtTagInput.Text = string.Empty;
            }
            else if (e.KeyCode == Keys.Back && string.IsNullOrEmpty(txtTagInput.Text))
            {
                // Remove last chip on backspace
                if (flpTags.Controls.Count > 1)
                {
                    flpTags.Controls.RemoveAt(flpTags.Controls.Count - 2);
                }
            }
        }

        // KeyPress fires after keyboard-layout translation, so e.KeyChar == ','
        // only when the user actually typed a comma — not when pressing the physical
        // comma key in Thai layout (which produces "ม" via Keys.Oemcomma in KeyDown).
        private void TxtTagInput_KeyPress(object sender, KeyPressEventArgs e)
        {
            if (e.KeyChar == ',')
            {
                e.Handled = true;
                AddTagChip(txtTagInput.Text.Trim());
                txtTagInput.Text = string.Empty;
            }
        }

        private Panel CloneTagChipTemplate(string tagText)
        {
            // Create new instances based on template properties
            var chip = new Panel
            {
                AutoSize = pnlTagChipTemplate.AutoSize,
                Height = pnlTagChipTemplate.Height,
                Margin = pnlTagChipTemplate.Margin,
                Padding = pnlTagChipTemplate.Padding,
                BackColor = pnlTagChipTemplate.BackColor,
                BorderStyle = pnlTagChipTemplate.BorderStyle
            };
            
            var lblTag = new Label
            {
                Text = tagText,
                AutoSize = true,  // Ensure AutoSize is enabled
                Font = lblTagChipTemplate.Font,
                ForeColor = lblTagChipTemplate.ForeColor,
                Padding = lblTagChipTemplate.Padding,
                Location = lblTagChipTemplate.Location
            };
            
            var btnRemove = new Button
            {
                Text = btnTagChipRemoveTemplate.Text,
                Size = btnTagChipRemoveTemplate.Size,
                FlatStyle = btnTagChipRemoveTemplate.FlatStyle,
                Font = btnTagChipRemoveTemplate.Font,
                ForeColor = btnTagChipRemoveTemplate.ForeColor,
                Cursor = btnTagChipRemoveTemplate.Cursor,
                Margin = btnTagChipRemoveTemplate.Margin,
                Padding = btnTagChipRemoveTemplate.Padding
            };
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Click += (s, ev) => flpTags.Controls.Remove(chip);
            
            // Add label first to allow it to calculate its size
            chip.Controls.Add(lblTag);
            
            // Force the label to calculate its preferred size based on text
            using (var g = lblTag.CreateGraphics())
            {
                var textSize = g.MeasureString(tagText, lblTag.Font);
                lblTag.Width = (int)Math.Ceiling(textSize.Width) + lblTag.Padding.Horizontal;
            }
            
            // Position remove button after label
            btnRemove.Location = new Point(lblTag.Right + 2, 2);
            chip.Controls.Add(btnRemove);
            
            // Calculate panel width based on actual label width
            chip.Width = lblTag.Width + btnRemove.Width + 12;
            
            return chip;
        }

        private void AddTagChip(string tag, bool skipValidation = false)
        {
            if (string.IsNullOrWhiteSpace(tag)) return;
            
            // Remove comma if present
            tag = tag.TrimEnd(',');
            if (string.IsNullOrWhiteSpace(tag)) return;
            
            // Validate tag input only when not loading existing data
            if (!skipValidation)
            {
                // Allow alphanumeric, Thai characters, spaces, hyphens, and underscores
                if (!System.Text.RegularExpressions.Regex.IsMatch(tag, @"^[a-zA-Z0-9฀-๿\s\-_]+$"))
                {
                    MessageBox.Show("Tag สามารถประกอบด้วยตัวอักษร ตัวเลข ภาษาไทย ช่องว่าง ขีด (-) และขีดล่าง (_) เท่านั้น",
                        "Tag ไม่ถูกต้อง", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
                
                // Limit tag length
                if (tag.Length > 50)
                {
                    MessageBox.Show("Tag cannot exceed 50 characters.", 
                        "Tag Too Long", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }
            }
            
            // Check for duplicate tags (case-insensitive)
            foreach (Control control in flpTags.Controls)
            {
                if (control is Panel panel)
                {
                    foreach (Control childControl in panel.Controls)
                    {
                        if (childControl is Label label)
                        {
                            if (string.Equals(label.Text, tag, StringComparison.OrdinalIgnoreCase))
                            {
                                if (!skipValidation)
                                {
                                    MessageBox.Show($"Tag '{tag}' already exists.", 
                                        "Duplicate Tag", MessageBoxButtons.OK, MessageBoxIcon.Information);
                                }
                                return;
                            }
                        }
                    }
                }
            }
            
            // Clone the template panel
            var chip = CloneTagChipTemplate(tag);
            
            // Insert before the input textbox
            int inputIndex = flpTags.Controls.IndexOf(txtTagInput);
            flpTags.Controls.Add(chip);
            flpTags.Controls.SetChildIndex(chip, inputIndex);
        }

        private void UpdateEditorStatus()
        {
            int len = ContentText.Length;
            lblRteStatus.Text = string.Format("🖊️ กำลังแก้ไข  |  จำนวนอักขระ: {0}", len);
        }

        private void SyncToolbarState()
        {
            if (!_editorReady || webEditor.Document == null) return;
            if (_applyingFormat) return;

            try
            {
                var raw = webEditor.Document.InvokeScript("getRteState") as string;
                if (string.IsNullOrEmpty(raw) || !raw.Contains("|")) return;

                var parts    = raw.Split('|');
                var f        = parts[0].PadRight(8, '0');
                var fontName = parts.Length > 1 ? parts[1].Trim() : string.Empty;
                var fontSz   = parts.Length > 2 ? parts[2].Trim() : string.Empty;

                SetBtnActive(tsRteBold,        f[0] == '1');
                SetBtnActive(tsRteItalic,      f[1] == '1');
                SetBtnActive(tsRteUnder,       f[2] == '1');
                SetBtnActive(tsRteAlignLeft,   f[3] == '1');
                SetBtnActive(tsRteAlignCenter, f[4] == '1');
                SetBtnActive(tsRteAlignRight,  f[5] == '1');
                SetBtnActive(tsRteBullet,      f[6] == '1');
                SetBtnActive(tsRteNumList,     f[7] == '1');

                _syncingToolbar = true;
                try
                {
                    int fi = -1;
                    if (!string.IsNullOrEmpty(fontName))
                        for (int i = 0; i < tsRteFontFamily.Items.Count; i++)
                            if (string.Equals(tsRteFontFamily.Items[i].ToString(), fontName,
                                              StringComparison.OrdinalIgnoreCase))
                            { fi = i; break; }
                    tsRteFontFamily.SelectedIndex = fi;

                    int newSzIdx = -1;
                    if (!string.IsNullOrEmpty(fontSz))
                        for (int i = 0; i < tsRteFontSize.Items.Count; i++)
                            if (tsRteFontSize.Items[i].ToString() == fontSz)
                            { newSzIdx = i; break; }
                    tsRteFontSize.SelectedIndex = newSzIdx;
                }
                finally { _syncingToolbar = false; }
            }
            catch { }
        }

        /// <summary>
        /// Applies or clears the active highlight on a ToolStripButton.
        /// Active: light-blue fill matching the app's selection colour palette.
        /// Inactive: transparent (inherits the ToolStrip background).
        /// </summary>
        private static void SetBtnActive(ToolStripButton btn, bool active)
        {
            btn.BackColor = active
                ? Color.FromArgb(198, 217, 238)   // light-blue highlight (app selection shade)
                : Color.Transparent;              // normal — inherits tsRte background
            btn.ForeColor = active
                ? Color.FromArgb(26, 58, 95)      // dark navy (active emphasis)
                : Color.Empty;                    // default — inherited
        }

        private void ExecCmd(string command, object value = null)
        {
            if (webEditor.Document == null) return;
            webEditor.Document.ExecCommand(command, false, value);
            webEditor.Focus();
            SyncToolbarState();
        }

        private void PasteHtmlAtCursor(string html)
        {
            if (webEditor.Document == null) return;
            webEditor.Document.InvokeScript("pasteHtml", new object[] { html });
            webEditor.Focus();
            UpdateEditorStatus();
        }

        private void TsRteBold_Click(object sender, EventArgs e)
        {
            if (!_editorReady || webEditor.Document == null) return;
            webEditor.Document.InvokeScript("applyBold");
            webEditor.Focus();
            SyncToolbarState();
        }
        private void TsRteItalic_Click(object sender, EventArgs e)       { ExecCmd("Italic"); }
        private void TsRteUnder_Click(object sender, EventArgs e)        { ExecCmd("Underline"); }
        private void TsRteAlignLeft_Click(object sender, EventArgs e)    { ExecCmd("JustifyLeft"); }
        private void TsRteAlignCenter_Click(object sender, EventArgs e)  { ExecCmd("JustifyCenter"); }
        private void TsRteAlignRight_Click(object sender, EventArgs e)   { ExecCmd("JustifyRight"); }
        private void TsRteBullet_Click(object sender, EventArgs e)       { ExecCmd("InsertUnorderedList"); }
        private void TsRteNumList_Click(object sender, EventArgs e)      { ExecCmd("InsertOrderedList"); }

        private void TsRteLink_Click(object sender, EventArgs e)
        {
            string url = Microsoft.VisualBasic.Interaction.InputBox(
                "กรอก URL:", "แทรก Hyperlink", "https://");
            if (string.IsNullOrWhiteSpace(url)) return;
            ExecCmd("CreateLink", url);
        }

        private void TsRteImage_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "เลือกรูปภาพ";
                ofd.Filter = "Image Files (*.jpg;*.jpeg;*.png;*.gif;*.bmp)|*.jpg;*.jpeg;*.png;*.gif;*.bmp|" +
                             "JPEG Files (*.jpg;*.jpeg)|*.jpg;*.jpeg|" +
                             "PNG Files (*.png)|*.png|" +
                             "GIF Files (*.gif)|*.gif|" +
                             "BMP Files (*.bmp)|*.bmp|" +
                             "All Files (*.*)|*.*";
                ofd.FilterIndex = 1; // Default to "Image Files"
                ofd.Multiselect = false;
                
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    try
                    {
                        // Convert local file path to data URI for embedding
                        string filePath = ofd.FileName;
                        byte[] imageBytes = System.IO.File.ReadAllBytes(filePath);
                        string base64 = Convert.ToBase64String(imageBytes);
                        string ext = System.IO.Path.GetExtension(filePath).ToLower();
                        
                        // Determine MIME type
                        string mimeType = "image/jpeg";
                        if (ext == ".png") mimeType = "image/png";
                        else if (ext == ".gif") mimeType = "image/gif";
                        else if (ext == ".bmp") mimeType = "image/bmp";
                        
                        string dataUri = string.Format("data:{0};base64,{1}", mimeType, base64);
                        string fileName = System.IO.Path.GetFileName(filePath);
                        
                        PasteHtmlAtCursor(string.Format(
                            "<img src='{0}' alt='{1}' style='max-width:100%;' />", 
                            dataUri, 
                            System.Net.WebUtility.HtmlEncode(fileName)));
                    }
                    catch (Exception ex)
                    {
                        ShowError("ไม่สามารถแทรกรูปภาพได้:\n" + ex.Message);
                    }
                }
            }
        }

        private void TsRteTable_Click(object sender, EventArgs e)
        {
            string input = Microsoft.VisualBasic.Interaction.InputBox(
                "จำนวน แถว x คอลัมน์ (เช่น 3x4):", "แทรกตาราง", "3x3");
            if (string.IsNullOrWhiteSpace(input)) return;

            string[] parts = input.Split(new char[] { 'x', 'X' });
            int rows = 3, cols = 3;
            if (parts.Length == 2)
            {
                int.TryParse(parts[0].Trim(), out rows);
                int.TryParse(parts[1].Trim(), out cols);
            }
            if (rows < 1) rows = 1;
            if (cols < 1) cols = 1;

            var sb = new System.Text.StringBuilder();
            sb.Append("<table border='1' style='border-collapse:collapse;width:100%;margin:8px 0;'>");
            sb.Append("<tr>");
            for (int c = 0; c < cols; c++)
                sb.AppendFormat("<th style='border:1px solid #ccc;padding:4px 8px;background:#f0f0f0;'>Header {0}</th>", c + 1);
            sb.Append("</tr>");
            for (int r = 1; r < rows; r++)
            {
                sb.Append("<tr>");
                for (int c = 0; c < cols; c++)
                    sb.Append("<td style='border:1px solid #ccc;padding:4px 8px;'>&nbsp;</td>");
                sb.Append("</tr>");
            }
            sb.Append("</table><p></p>");
            PasteHtmlAtCursor(sb.ToString());
        }

        private void TsRteCode_Click(object sender, EventArgs e)
        {
            string lang = Microsoft.VisualBasic.Interaction.InputBox(
                "ภาษาโปรแกรม (เช่น csharp, sql, javascript) หรือว่างเปล่า:",
                "แทรก Code Block", "");
            PasteHtmlAtCursor(string.Format(
                "<pre style='background:#f4f4f4;padding:8px;border-radius:4px;" +
                "font-family:Consolas,monospace;border:1px solid #ddd;'>" +
                "<code class='language-{0}'>// Code here</code></pre><p></p>", lang ?? ""));
        }

        private void TsRteAttach_Click(object sender, EventArgs e)
        {
            tabMapping.SelectedTab = tabAttach;
        }

        private void TsRteFontFamily_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_syncingToolbar || tsRteFontFamily.SelectedItem == null) return;
            ExecCmd("FontName", tsRteFontFamily.SelectedItem.ToString());
        }

        private void TsRteFontSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_syncingToolbar || tsRteFontSize.SelectedIndex < 0) return;
            if (_applyingFormat) return;
            ApplyFontSizePt(tsRteFontSize.Items[tsRteFontSize.SelectedIndex].ToString());
            webEditor.Focus();
            SyncToolbarState();
        }

        private void TsRteFontSizeInc_Click(object sender, EventArgs e) { StepFontSize(+1); }
        private void TsRteFontSizeDec_Click(object sender, EventArgs e) { StepFontSize(-1); }

        private void StepFontSize(int direction)
        {
            if (!_editorReady || webEditor.Document == null) return;
            if (_applyingFormat) return;
            _applyingFormat = true;
            try
            {
                if (!true.Equals(webEditor.Document.InvokeScript("hasTextSel"))) return;

                var current = webEditor.Document.InvokeScript("getSelFontSizePt") as string ?? string.Empty;

                int currentIdx = -1;
                for (int i = 0; i < tsRteFontSize.Items.Count; i++)
                    if (tsRteFontSize.Items[i].ToString() == current)
                    { currentIdx = i; break; }

                int nextIdx = currentIdx < 0
                    ? (direction > 0 ? 0 : tsRteFontSize.Items.Count - 1)
                    : Math.Max(0, Math.Min(tsRteFontSize.Items.Count - 1, currentIdx + direction));

                ApplyFontSizePt(tsRteFontSize.Items[nextIdx].ToString());
                webEditor.Focus();
            }
            finally { _applyingFormat = false; }
            SyncToolbarState();
        }

        private void ApplyFontSizePt(string pt)
        {
            if (webEditor.Document == null) return;
            string markerId = "fsm" + Environment.TickCount.ToString();
            webEditor.Document.InvokeScript("applyFontSizePt", new object[] { pt, markerId });
        }

        // ── Zoom controls ────────────────────────────────────────────────────────

        private void TsRteZoomOut_Click(object sender, EventArgs e)
        {
            int prev = _zoomLevels.LastOrDefault(l => l < _zoomPct);
            if (prev > 0) SetZoom(prev);
        }

        private void TsRteZoomIn_Click(object sender, EventArgs e)
        {
            int next = _zoomLevels.FirstOrDefault(l => l > _zoomPct);
            if (next > 0) SetZoom(next);
        }

        private void TsRteZoom_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_syncingToolbar || tsRteZoom.SelectedItem == null) return;
            string s = tsRteZoom.SelectedItem.ToString().Replace("%", "");
            if (int.TryParse(s, out int pct)) SetZoom(pct);
        }

        private void TsRteZoom_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode != Keys.Return) return;
            e.Handled = true;
            e.SuppressKeyPress = true;
            ApplyZoomFromText();
            webEditor.Focus();
        }

        private void TsRteZoom_Leave(object sender, EventArgs e)
        {
            ApplyZoomFromText();
        }

        private void ApplyZoomFromText()
        {
            string s = tsRteZoom.Text.Replace("%", "").Trim();
            if (int.TryParse(s, out int pct) && pct >= ZoomMinPct && pct <= ZoomMaxPct)
                SetZoom(pct);
            else
            {
                // Reset display to the last valid zoom
                ShowZoomInToolbar(_zoomPct);
            }
        }

        private void SetZoom(int pct)
        {
            SetZoom(pct, true);
        }

        private void SetZoom(int pct, bool focusEditor)
        {
            if (pct < ZoomMinPct) pct = ZoomMinPct;
            if (pct > ZoomMaxPct) pct = ZoomMaxPct;

            _zoomPct = pct;
            if (_editorReady && webEditor.Document != null)
            {
                try { webEditor.Document.InvokeScript("setZoomPct", new object[] { pct }); }
                catch { }
            }
            ShowZoomInToolbar(pct);
            if (focusEditor) webEditor.Focus();
        }

        private void ShowZoomInToolbar(int pct)
        {
            _syncingToolbar = true;
            tsRteZoom.Text = pct + "%";
            _syncingToolbar = false;
        }

        // ── Ctrl+MouseWheel zoom ──────────────────────────────────────────────────
        // The classic WebBrowser has no ZoomFactor property and raises no zoom event, and
        // IE's own Ctrl+wheel optical zoom happens inside the control where neither the DOM
        // nor WinForms can observe it — which is why tsRteZoom never followed along. So the
        // wheel message is intercepted here before the control sees it: we apply the zoom
        // ourselves (CSS zoom, via setZoomPct) and consume the message, so IE's optical zoom
        // never fires and cannot compound with ours.

        private const int WM_MOUSEWHEEL = 0x020A;

        bool IMessageFilter.PreFilterMessage(ref Message m)
        {
            if (m.Msg != WM_MOUSEWHEEL) return false;
            if ((ModifierKeys & Keys.Control) != Keys.Control) return false;

            try
            {
                if (!_editorReady || webEditor.Document == null) return false;
                if (!WebEditorContentHasFocus()) return false;

                int delta = (short)((m.WParam.ToInt64() >> 16) & 0xFFFF);
                if (delta == 0) return false;

                int notches = delta / 120;
                if (notches == 0) notches = delta > 0 ? 1 : -1;

                SetZoom(_zoomPct + (notches * 10), false);
            }
            catch { }

            // Always consume: the editor is zoomed by us, never by IE.
            return true;
        }

        private static string GetEditorHtml()
        {
            // Classic WebBrowser (IE/Trident) version — deliberately NO <meta X-UA-Compatible>.
            // The control renders in IE7-emulation mode by default on every machine regardless
            // of installed IE version unless overridden; forcing a newer mode would depend on
            // that override succeeding, which defeats the whole point of this control (maximum
            // compatibility on old/locked-down machines). So all JS below uses only legacy
            // document.selection / TextRange APIs, which work in the default emulation mode.
            // C# talks to this document synchronously via Document.ExecCommand / InvokeScript —
            // there is no postMessage bridge; state/html are read on demand, not pushed.
            return
                "<!DOCTYPE html><html><head><meta charset='utf-8'>" +
                "<style>" +
                "html,body{height:100%;margin:0;}" +
                "body{font-family:'Segoe UI',Tahoma,sans-serif;font-size:14px;" +
                "padding:8px;min-height:100%;box-sizing:border-box;line-height:1.4;color:#333;" +
                "outline:none;}" +
                // Remove default <p> margins so lines from Enter key match <br> spacing.
                "p{margin:0;}" +
                "h1{font-size:22px;}h2{font-size:18px;}h3{font-size:15px;}" +
                "table{border-collapse:collapse;width:100%;margin:8px 0;}" +
                "td,th{border:1px solid #ccc;padding:4px 8px;}" +
                "th{background:#f0f0f0;}" +
                "pre{background:#f4f4f4;font-family:Consolas,monospace;padding:8px;" +
                "border-radius:4px;border:1px solid #ddd;overflow:auto;}" +
                "code{background:#f4f4f4;font-family:Consolas,monospace;padding:2px 4px;border-radius:2px;}" +
                "a{color:#2980b9;}" +
                "</style>" +
                "<script type='text/javascript'>" +
                // ── Paste HTML at cursor (IE TextRange) ────────────────────────────
                "function pasteHtml(h){" +
                "try{if(document.selection){var r=document.selection.createRange();r.pasteHTML(h);}}" +
                "catch(e){}}" +
                // ── Format state query — called by C# via InvokeScript ─────────────
                // Returns a compact string: 8 binary flags + '|' + fontName + '|' + fontSize.
                // Flags order: bold italic underline justifyLeft justifyCenter justifyRight ul ol
                "function getRteState(){" +
                "try{" +
                "var b=document.queryCommandState('Bold')?'1':'0';" +
                "var i=document.queryCommandState('Italic')?'1':'0';" +
                "var u=document.queryCommandState('Underline')?'1':'0';" +
                "var jl=document.queryCommandState('JustifyLeft')?'1':'0';" +
                "var jc=document.queryCommandState('JustifyCenter')?'1':'0';" +
                "var jr=document.queryCommandState('JustifyRight')?'1':'0';" +
                "var ul=document.queryCommandState('InsertUnorderedList')?'1':'0';" +
                "var ol=document.queryCommandState('InsertOrderedList')?'1':'0';" +
                "var fn=(document.queryCommandValue('FontName')||'').toLowerCase();" +
                "var fs=getSelFontSizePt();" +
                "return b+i+u+jl+jc+jr+ul+ol+'|'+fn+'|'+fs;" +
                "}catch(e){return '00000000||';}}" +
                // Bold is routed through this JS wrapper instead of Document.ExecCommand from
                // C# — a deliberate, previously-shipped workaround (kept as-is; do not "clean
                // up" back to a direct ExecCommand("Bold") call from C#).
                "function applyBold(){document.execCommand('Bold',false,null);}" +
                // ── Selection helpers ────────────────────────────────────────────
                "function hasTextSel(){try{var s=document.selection;return !!(s&&s.type=='Text');}catch(e){return false;}}" +
                // HTML font size 1-7 → pt equivalents (matches tsRteFontSize items)
                "var _htmlSzPt={'1':'8','2':'10','3':'12','4':'14','5':'18','6':'24','7':'36'};" +
                "function getSelFontSizePt(){" +
                "try{var s=document.selection;" +
                "if(s&&s.type=='Text'){var r=s.createRange();" +
                "if(r&&r.parentElement){var el=r.parentElement();" +
                "while(el&&el.tagName!='BODY'){" +
                "if(el.style&&el.style.fontSize){var f=el.style.fontSize;" +
                "if(f.indexOf('pt')>-1)return Math.round(parseFloat(f)).toString();" +
                "if(f.indexOf('px')>-1)return Math.round(parseFloat(f)*0.75).toString();}" +
                "if(el.tagName=='FONT'){var a=el.getAttribute('size');if(a&&_htmlSzPt[a])return _htmlSzPt[a];}" +
                "el=el.parentElement;}}}" +
                "}catch(e){}return '';}" +
                // ── Font size apply ──────────────────────────────────────────────
                "function applyFontSizePt(pt,markerId){" +
                "try{if(!hasTextSel())return;" +
                "var r=document.selection.createRange();" +
                "var el=r.parentElement();" +
                "if(el&&el.tagName!='BODY'&&el.style&&el.style.fontSize){" +
                "el.style.fontSize=pt+'pt';r.select();return;}" +
                "var inner=r.htmlText.replace(/font-size\\s*:[^;'\"]+;?/gi,'');" +
                "r.pasteHTML('<span id=\"'+markerId+'\" style=\"font-size:'+pt+'pt\">'+inner+'</span>');" +
                "var newEl=document.getElementById(markerId);" +
                "if(newEl){var r2=document.body.createTextRange();" +
                "r2.moveToElementText(newEl);r2.select();newEl.removeAttribute('id');}}" +
                "catch(e){}}" +
                // ── Zoom (CSS, IE/Trident proprietary `zoom` property) ─────────────
                "function setZoomPct(pct){try{document.body.style.zoom=pct+'%';}catch(e){}}" +
                // Redirect clicks on <html> (outside body padding) into the body.
                "document.onclick=function(e){" +
                "if(e&&e.srcElement===document.documentElement){document.body.focus();}" +
                "};" +
                // ── Tab indent (plain &nbsp; run — the proven pre-WebView2 mechanism) ──
                // NOTE: a <span contenteditable="false"> marker was tried here (to match the
                // pixel-accurate tab-stop look of the WebView2 version) but IE's legacy
                // rendering treats a contenteditable=false element nested inside an editable
                // body as a "control" (like an embedded object), drawing a visible selection
                // box around it that grows with every Tab press. Plain &nbsp; characters have
                // none of that baggage and are exactly what shipped and worked before.
                "function insertTabAtCaret(){" +
                "try{" +
                "var sel=document.selection;" +
                "if(!sel||sel.type==='None'){document.body.focus();sel=document.selection;}" +
                "if(!sel)return;" +
                "var r=sel.createRange();r.collapse(true);" +
                "r.pasteHTML('&nbsp;&nbsp;&nbsp;&nbsp;');" +
                "}catch(e){}}" +
                // Remove up to 4 trailing spaces/&nbsp; immediately before the caret.
                "function removeTabAtCaret(){" +
                "try{" +
                "var sel=document.selection;" +
                "if(!sel||sel.type==='None'){document.body.focus();sel=document.selection;}" +
                "if(!sel)return;" +
                "var r=sel.createRange();r.collapse(true);" +
                "var r2=r.duplicate();r2.moveStart('character',-4);" +
                "var text=r2.text,cnt=0;" +
                "for(var i=text.length-1;i>=0;i--){var c=text.charCodeAt(i);if(c===32||c===160)cnt++;else break;}" +
                "if(cnt>0){var del=r.duplicate();del.moveStart('character',-cnt);del.text='';}" +
                "}catch(e){}}" +
                // Returns true when the caret/selection is inside a list item (<li>)
                "function isInList(){" +
                "var s=document.selection;if(!s)return false;" +
                "var r=s.createRange();var n=r.parentElement?r.parentElement():null;" +
                "while(n&&n.tagName!=='BODY'){if(n.tagName==='LI')return true;n=n.parentElement;}" +
                "return false;}" +
                // Nearest block-level ancestor of el (P/DIV/LI/heading/table-cell/etc.), or null
                // if none is found before reaching <body> (e.g. raw <br>-separated content).
                "function nearestBlock(el){" +
                "var blocks={P:1,DIV:1,LI:1,H1:1,H2:1,H3:1,H4:1,H5:1,H6:1,BLOCKQUOTE:1,TD:1,TH:1,PRE:1};" +
                "while(el&&el!==document.body){" +
                "if(el.tagName&&blocks[el.tagName])return el;el=el.parentElement;}" +
                "return null;}" +
                // Ordered list of block elements from startBlock to endBlock (inclusive), when
                // they are siblings under the same parent; otherwise degrades to just startBlock.
                // Returns null when neither end has a block ancestor (bare text/&lt;br&gt; content) —
                // callers then fall back to single-caret indent rather than touching the body.
                // Only element nodes are collected: nextSibling also yields text/comment nodes,
                // and moveToElementText/innerHTML on those would throw.
                "function collectLines(startBlock,endBlock){" +
                "if(!startBlock&&!endBlock)return null;" +
                "if(!startBlock)startBlock=endBlock;" +
                "if(!endBlock)endBlock=startBlock;" +
                "if(startBlock.parentNode!==endBlock.parentNode)return [startBlock];" +
                "var lines=[],node=startBlock;" +
                "while(node){" +
                "if(node.nodeType===1)lines.push(node);" +
                "if(node===endBlock)break;node=node.nextSibling;}" +
                "if(!lines.length||lines[lines.length-1]!==endBlock)return [startBlock];" +
                "return lines;}" +
                // Reselect the given line range so a repeated Tab press indents further.
                "function _reselectLines(lines){" +
                "try{" +
                "var sr=document.body.createTextRange();sr.moveToElementText(lines[0]);" +
                "var er=document.body.createTextRange();er.moveToElementText(lines[lines.length-1]);" +
                "sr.setEndPoint('EndToEnd',er);sr.select();" +
                "}catch(e){}}" +
                // Tab with a real (possibly multi-line) selection: indent every touched line.
                "function indentSelectionLines(){" +
                "try{" +
                "var r=document.selection.createRange();" +
                "if(r.text===''){insertTabAtCaret();return;}" +
                "var rs=r.duplicate();rs.collapse(true);var re=r.duplicate();re.collapse(false);" +
                "var startBlock=nearestBlock(rs.parentElement?rs.parentElement():null);" +
                "var endBlock=nearestBlock(re.parentElement?re.parentElement():null);" +
                "var lines=collectLines(startBlock,endBlock);" +
                // No block ancestor (bare text/<br> content) — indent at the caret instead of
                // pasting into the start of the whole body, which would wreck the layout.
                "if(!lines){insertTabAtCaret();return;}" +
                "for(var i=0;i<lines.length;i++){" +
                "var line=lines[i];" +
                "var lr=document.body.createTextRange();lr.moveToElementText(line);lr.collapse(true);" +
                "lr.pasteHTML('&nbsp;&nbsp;&nbsp;&nbsp;');" +
                "}" +
                "_reselectLines(lines);" +
                "}catch(e){}}" +
                // Shift+Tab with a real (possibly multi-line) selection: outdent every touched line.
                "function outdentSelectionLines(){" +
                "try{" +
                "var r=document.selection.createRange();" +
                "if(r.text===''){removeTabAtCaret();return;}" +
                "var rs=r.duplicate();rs.collapse(true);var re=r.duplicate();re.collapse(false);" +
                "var startBlock=nearestBlock(rs.parentElement?rs.parentElement():null);" +
                "var endBlock=nearestBlock(re.parentElement?re.parentElement():null);" +
                "var lines=collectLines(startBlock,endBlock);" +
                "if(!lines){removeTabAtCaret();return;}" +
                "for(var i=0;i<lines.length;i++){" +
                "var line=lines[i];" +
                "line.innerHTML=line.innerHTML.replace(/^(?:&nbsp;|\\s){1,4}/i,'');" +
                "}" +
                "_reselectLines(lines);" +
                "}catch(e){}}" +
                // Shared implementation for both Tab capture paths.
                "function handleTabIndent(isShift){" +
                "try{" +
                "if(isInList()){document.execCommand(isShift?'outdent':'indent',false,null);return;}" +
                "if(isShift)outdentSelectionLines();else indentSelectionLines();" +
                "}catch(e){}}" +
                // Zero-argument entry points called by C# (Form.ProcessCmdKey) — no IDispatch
                // argument marshaling, matching the call shape that shipped previously.
                "function kmsTabIndent(){handleTabIndent(false);}" +
                "function kmsTabOutdent(){handleTabIndent(true);}" +
                // Second capture path: when the key reaches MSHTML instead of being consumed by
                // ProcessCmdKey, handle it here. Legacy IE event model (window.event/returnValue)
                // — addEventListener/preventDefault do not exist in the default IE7 document mode.
                // Cancelling keydown stops IE's default Tab action of moving focus out of the
                // editor, which is what makes focus rectangles march across the form.
                "document.onkeydown=function(){" +
                "var ev=window.event;if(!ev)return;" +
                "if(ev.keyCode===9){" +
                "handleTabIndent(!!ev.shiftKey);" +
                "ev.returnValue=false;ev.cancelBubble=true;ev.keyCode=0;" +
                "return false;}" +
                "};" +
                "</script>" +
                "</head><body contenteditable='true'></body></html>";
        }

        // ── Tab Mapping Toolbars ─────────────────────────────────────────────
        // CreateAttachToolbar() removed - buttons now integrated into DataGridView
        // ── Tab Mapping Event Handlers ───────────────────────────────────────

        private void BtnLoadRelatedFromKB_Click(object sender, EventArgs e)
        {
            try
            {
                // Load categories and articles from KB
                var categories = _presenter.GetAllCategories();
                var articles = _presenter.GetAllPublishedArticles();
                
                dgvRelatedSearch.Rows.Clear();
                
                // Exclude current article being edited
                if (_presenter.EditingArticle != null)
                    articles = articles.Where(a => a.ArticleID != _presenter.EditingArticle.ArticleID);
                
                // Exclude articles already in dgvRelatedLinked
                var existingIds = new HashSet<int>(
                    dgvRelatedLinked.Rows.Cast<DataGridViewRow>()
                        .Select(row => (int)row.Tag)
                );
                articles = articles.Where(a => !existingIds.Contains(a.ArticleID));
                
                // Populate search list with KB data
                foreach (var article in articles.Take(50))
                {
                    int rowIndex = dgvRelatedSearch.Rows.Add(
                        article.ArticleCode ?? "-",
                        article.Title ?? "-",
                        article.Summary ?? "-",
                        article.ArticleTypeName ?? "-",
                        article.ViewCount.ToString()
                    );
                    dgvRelatedSearch.Rows[rowIndex].Tag = article.ArticleID;
                }
                
                ShowInfo(string.Format("โหลดข้อมูลจาก KB เรียบร้อย ({0} รายการ)", dgvRelatedSearch.Rows.Count));
            }
            catch (Exception ex)
            {
                ShowError("โหลดข้อมูลจาก KB ไม่สำเร็จ: " + ex.Message);
            }
        }

        private void BtnAddWorkflow_Click(object sender, EventArgs e)
        {
            // TODO: Open workflow selection dialog
            ShowInfo("ฟีเจอร์นี้จะเปิด Dialog เลือก Workflow\n\nสามารถเชื่อมโยงบทความกับเส้นงาน (Workflow) ที่เกี่ยวข้อง");
        }

        private void BtnAddProgram_Click(object sender, EventArgs e)
        {
            // TODO: Open program selection dialog
            ShowInfo("ฟีเจอร์นี้จะเปิด Dialog เลือกโปรแกรม\n\nสามารถเชื่อมโยงบทความกับโปรแกรมที่เกี่ยวข้อง");
        }

        private void BtnAddDoc_Click(object sender, EventArgs e)
        {
            // TODO: Open document selection dialog (from DMS)
            ShowInfo("ฟีเจอร์นี้จะเปิด Dialog เลือกเอกสารจากระบบ DMS\n\nสามารถเชื่อมโยงบทความกับเอกสารในระบบจัดการเอกสาร");
        }

        // ── Search Event Handlers ────────────────────────────────────────────

        private System.Threading.Timer _searchRelatedTimer;
        private System.Threading.Timer _searchProgramsTimer;
        private System.Threading.Timer _searchDocsTimer;

        private void TxtSearchRelated_TextChanged(object sender, EventArgs e)
        {
            _searchRelatedTimer?.Dispose();
            _searchRelatedTimer = new System.Threading.Timer(
                _ => this.Invoke(new Action(PerformRelatedSearch)),
                null, 300, System.Threading.Timeout.Infinite);
        }

        private void TxtSearchPrograms_TextChanged(object sender, EventArgs e)
        {
            _searchProgramsTimer?.Dispose();
            _searchProgramsTimer = new System.Threading.Timer(
                _ => this.Invoke(new Action(PerformProgramsSearch)),
                null, 300, System.Threading.Timeout.Infinite);
        }

        private void TxtSearchDocs_TextChanged(object sender, EventArgs e)
        {
            _searchDocsTimer?.Dispose();
            _searchDocsTimer = new System.Threading.Timer(
                _ => this.Invoke(new Action(PerformDocsSearch)),
                null, 300, System.Threading.Timeout.Infinite);
        }

        // ── Search Implementation Methods ─────────────────────────────────────

        private void PerformRelatedSearch()
        {
            if (txtSearchRelated == null) return;
            
            string searchText = txtSearchRelated.Text.Trim();
            
            // Skip if placeholder text
            if (searchText == "🔍 ค้นหาชื่อบทความ / รหัส / แท็ก...")
                searchText = string.Empty;
            
            dgvRelatedSearch.Rows.Clear();
            
            if (string.IsNullOrEmpty(searchText)) return;
            
            try
            {
                // Call service to search published articles
                var results = _presenter.SearchPublishedArticles(searchText);
                
                // Exclude current article being edited
                if (_presenter.EditingArticle != null)
                    results = results.Where(a => a.ArticleID != _presenter.EditingArticle.ArticleID);
                
                // Exclude articles already in dgvRelatedLinked
                var existingIds = new HashSet<int>(
                    dgvRelatedLinked.Rows.Cast<DataGridViewRow>()
                        .Select(row => (int)row.Tag)
                );
                results = results.Where(a => !existingIds.Contains(a.ArticleID));
                
                // Populate search list (limit to 50 results)
                foreach (var article in results.Take(50))
                {
                    int rowIndex = dgvRelatedSearch.Rows.Add(
                        article.ArticleCode ?? "-",
                        article.Title ?? "-",
                        article.Summary ?? "-",
                        article.ArticleTypeName ?? "-",
                        article.ViewCount.ToString()
                    );
                    dgvRelatedSearch.Rows[rowIndex].Tag = article.ArticleID;
                }
            }
            catch (Exception ex)
            {
                ShowError("ค้นหาไม่สำเร็จ: " + ex.Message);
            }
        }

        private void PerformProgramsSearch()
        {
            // Placeholder — replace with real program search when PMS integration is available
        }

        private void PerformDocsSearch()
        {
            // Placeholder — replace with real document search when DMS integration is available
        }

        // ── Helper Methods ───────────────────────────────────────────────────

        private string FormatFileSize(long bytes)
        {
            string[] sizes = { "B", "KB", "MB", "GB" };
            double len = bytes;
            int order = 0;
            while (len >= 1024 && order < sizes.Length - 1)
            {
                order++;
                len = len / 1024;
            }
            return string.Format("{0:0.##} {1}", len, sizes[order]);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // TAB PROGRAMS - NEW IMPLEMENTATION (Two-Section Design with DataGridView)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Initialize DataGridView columns for programs
        /// </summary>
        private void InitializeProgramsDataGridViews()
        {
            // Columns are now defined in Designer.cs
            
            // ── Initialize Search Filters ──
            // Type filter
            cboProgramsType.Items.Clear();
            cboProgramsType.Items.Add("ทุกประเภท");
            cboProgramsType.Items.Add("Web Application");
            cboProgramsType.Items.Add("Desktop Application");
            cboProgramsType.Items.Add("Mobile App");
            cboProgramsType.Items.Add("API Service");
            cboProgramsType.SelectedIndex = 0;
            
            // Category filter
            cboProgramsCategory.Items.Clear();
            cboProgramsCategory.Items.Add("ทุกหมวด");
            cboProgramsCategory.Items.Add("Core System");
            cboProgramsCategory.Items.Add("Business Application");
            cboProgramsCategory.Items.Add("Utility");
            cboProgramsCategory.Items.Add("Integration");
            cboProgramsCategory.SelectedIndex = 0;
            
            // Status filter
            cboProgramsStatus.Items.Clear();
            cboProgramsStatus.Items.Add("ทั้งหมด");
            cboProgramsStatus.Items.Add("Active");
            cboProgramsStatus.Items.Add("Inactive");
            cboProgramsStatus.Items.Add("Maintenance");
            cboProgramsStatus.SelectedIndex = 0;
            
            // Placeholder text for search textbox
            txtSearchPrograms.Text = "🔍 ค้นหาชื่อโปรแกรม / รหัส...";
            txtSearchPrograms.ForeColor = Color.Gray;
        }

        /// <summary>
        /// Load linked programs when form initializes (Mockup data)
        /// </summary>
        private void LoadLinkedPrograms()
        {
            dgvProgramsLinked.Rows.Clear();
            
            // Mockup data - will be replaced with actual PMS integration
            var mockupPrograms = new[]
            {
                new { Code = "PRG-INS-001", Name = "Insurance Management System", Type = "Core System" },
                new { Code = "PRG-CRM-002", Name = "Customer Relationship Management", Type = "Business Application" }
            };
            
            int index = 1;
            foreach (var prog in mockupPrograms)
            {
                int rowIndex = dgvProgramsLinked.Rows.Add(
                    index.ToString(),
                    prog.Code,
                    prog.Name,
                    prog.Type
                );
                dgvProgramsLinked.Rows[rowIndex].Tag = index; // Mockup ID
                dgvProgramsLinked.Rows[rowIndex].DefaultCellStyle.BackColor = 
                    index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                index++;
            }
            
            UpdateProgramsLinkedCount();
        }

        private void UpdateProgramsLinkedCount()
        {
            lblProgramsLinked.Text = string.Format("โปรแกรมที่เชื่อมโยง ({0} รายการ)", dgvProgramsLinked.Rows.Count);
        }

        private void DgvProgramsLinked_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // Header clicked
            
            // Check if Delete button clicked
            if (e.ColumnIndex == dgvProgramsLinked.Columns["colDelete"].Index)
            {
                string programName = dgvProgramsLinked.Rows[e.RowIndex].Cells["colName"].Value?.ToString();
                
                var confirm = MessageBox.Show(
                    string.Format("ต้องการลบโปรแกรม '{0}' ใช่หรือไม่?", programName),
                    "ยืนยันการลบ", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                
                if (confirm == DialogResult.Yes)
                {
                    dgvProgramsLinked.Rows.RemoveAt(e.RowIndex);
                    
                    // Renumber
                    for (int i = 0; i < dgvProgramsLinked.Rows.Count; i++)
                    {
                        dgvProgramsLinked.Rows[i].Cells["colNum"].Value = (i + 1).ToString();
                        dgvProgramsLinked.Rows[i].DefaultCellStyle.BackColor = 
                            (i + 1) % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                    }
                    
                    UpdateProgramsLinkedCount();
                    ShowInfo("ลบโปรแกรมเรียบร้อยแล้ว");
                }
            }
        }

        private void DgvProgramsSearch_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // Header clicked
            
            try
            {
                // Add program to linked list
                int newIndex = dgvProgramsLinked.Rows.Count + 1;
                int rowIndex = dgvProgramsLinked.Rows.Add(
                    newIndex.ToString(),
                    dgvProgramsSearch.Rows[e.RowIndex].Cells["colSearchCode"].Value,
                    dgvProgramsSearch.Rows[e.RowIndex].Cells["colSearchName"].Value,
                    dgvProgramsSearch.Rows[e.RowIndex].Cells["colSearchType"].Value
                );
                
                dgvProgramsLinked.Rows[rowIndex].Tag = newIndex;
                dgvProgramsLinked.Rows[rowIndex].DefaultCellStyle.BackColor = 
                    newIndex % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                
                // Remove from search results
                dgvProgramsSearch.Rows.RemoveAt(e.RowIndex);
                
                UpdateProgramsLinkedCount();
                lblProgramsSearch.Text = string.Format("ผลการค้นหา ({0} รายการ) — ดับเบิลคลิกเพื่อเพิ่ม", dgvProgramsSearch.Rows.Count);
                
                ShowInfo("เพิ่มโปรแกรมเรียบร้อยแล้ว");
            }
            catch (Exception ex)
            {
                ShowError("เพิ่มโปรแกรมไม่สำเร็จ: " + ex.Message);
            }
        }

        private void TxtSearchPrograms_Enter(object sender, EventArgs e)
        {
            if (txtSearchPrograms.Text == "🔍 ค้นหาชื่อโปรแกรม / รหัส...")
            {
                txtSearchPrograms.Text = string.Empty;
                txtSearchPrograms.ForeColor = Color.Black;
            }
        }

        private void TxtSearchPrograms_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearchPrograms.Text))
            {
                txtSearchPrograms.Text = "🔍 ค้นหาชื่อโปรแกรม / รหัส...";
                txtSearchPrograms.ForeColor = Color.Gray;
            }
        }

        private void TxtSearchPrograms_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BtnSearchPrograms_Click(sender, e);
            }
        }

        private void BtnSearchPrograms_Click(object sender, EventArgs e)
        {
            try
            {
                string searchText = txtSearchPrograms.Text.Trim();
                if (searchText == "🔍 ค้นหาชื่อโปรแกรม / รหัส...")
                    searchText = string.Empty;
                
                // Mockup search results
                dgvProgramsSearch.Rows.Clear();
                
                if (string.IsNullOrEmpty(searchText))
                {
                    ShowInfo("กรุณากรอกคำค้นหา");
                    return;
                }
                
                // Mockup data - will be replaced with actual PMS search
                var mockupResults = new[]
                {
                    new { Code = "PRG-RPT-003", Name = "Reporting & Analytics Platform", Type = "Business Intelligence", Status = "Active" },
                    new { Code = "PRG-DOC-004", Name = "Document Management System", Type = "Core System", Status = "Active" },
                    new { Code = "PRG-WF-005", Name = "Workflow Automation Engine", Type = "Business Application", Status = "Active" }
                };
                
                foreach (var prog in mockupResults)
                {
                    int rowIndex = dgvProgramsSearch.Rows.Add(
                        prog.Code,
                        prog.Name,
                        prog.Type,
                        prog.Status
                    );
                    dgvProgramsSearch.Rows[rowIndex].Tag = prog.Code;
                }
                
                lblProgramsSearch.Text = string.Format("ผลการค้นหา ({0} รายการ) — ดับเบิลคลิกเพื่อเพิ่ม", dgvProgramsSearch.Rows.Count);
                
                if (dgvProgramsSearch.Rows.Count == 0)
                    ShowInfo("ไม่พบโปรแกรมที่ตรงกับเงื่อนไขการค้นหา");
            }
            catch (Exception ex)
            {
                ShowError("ค้นหาไม่สำเร็จ: " + ex.Message);
            }
        }

        // ══════════════════════════════════════════════════════════════════════════
        // TAB WORKFLOW
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Initialize DataGridView columns for workflow grids
        /// </summary>
        private void InitializeWorkflowDataGridViews()
        {
            // ── dgvWorkflowLinked (Linked Workflows) ──
            // Columns are now defined in Designer.cs
            dgvWorkflowLinked.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvWorkflowLinked.AllowUserToResizeRows = false;
            
            // ── dgvWorkflowSearch (Search Results) ──
            // Columns are now defined in Designer.cs
            dgvWorkflowSearch.AutoSizeRowsMode = DataGridViewAutoSizeRowsMode.AllCells;
            dgvWorkflowSearch.AllowUserToResizeRows = false;
            
            // ── Initialize Search Filters ──
            // Type filter
            cboWorkflowType.Items.Clear();
            cboWorkflowType.Items.Add("ทุกประเภท");
            cboWorkflowType.Items.Add("Approval");
            cboWorkflowType.Items.Add("Review");
            cboWorkflowType.Items.Add("Sequential");
            cboWorkflowType.Items.Add("Parallel");
            cboWorkflowType.SelectedIndex = 0;
            
            // Category filter
            cboWorkflowCategory.Items.Clear();
            cboWorkflowCategory.Items.Add("ทุกหมวด");
            cboWorkflowCategory.Items.Add("Document");
            cboWorkflowCategory.Items.Add("Claims");
            cboWorkflowCategory.Items.Add("Underwriting");
            cboWorkflowCategory.Items.Add("Audit");
            cboWorkflowCategory.SelectedIndex = 0;
            
            // Status filter
            cboWorkflowStatus.Items.Clear();
            cboWorkflowStatus.Items.Add("ทั้งหมด");
            cboWorkflowStatus.Items.Add("Active");
            cboWorkflowStatus.Items.Add("Inactive");
            cboWorkflowStatus.Items.Add("Draft");
            cboWorkflowStatus.SelectedIndex = 0;
            
            // Placeholder text for search textbox
            txtSearchWorkflow.Text = "🔍 ค้นหา...";
            txtSearchWorkflow.ForeColor = Color.Gray;
        }

        /// <summary>
        /// Load linked workflows when form initializes (Mockup data)
        /// </summary>
        private void LoadLinkedWorkflows()
        {
            dgvWorkflowLinked.Rows.Clear();
            
            // Mockup data - will be replaced with actual Workflow Engine integration
            var mockupWorkflows = new[]
            {
                new { Code = "WF-CLAIM-001", Name = "Claims Approval Process", Status = "Active" },
                new { Code = "WF-UW-002", Name = "Underwriting Review Workflow", Status = "Active" }
            };
            
            int index = 1;
            foreach (var wf in mockupWorkflows)
            {
                int rowIndex = dgvWorkflowLinked.Rows.Add(
                    index.ToString(),
                    wf.Code,
                    wf.Name,
                    wf.Status,
                    "🗑️ ลบ"
                );
                
                // Alternating row colors
                dgvWorkflowLinked.Rows[rowIndex].DefaultCellStyle.BackColor = 
                    index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                
                index++;
            }
            
            UpdateWorkflowLinkedCount();
        }

        private void UpdateWorkflowLinkedCount()
        {
            lblWorkflowLinked.Text = string.Format("Workflow ที่เชื่อมโยง ({0} รายการ)", dgvWorkflowLinked.Rows.Count);
        }

        /// <summary>
        /// Handle delete button click in dgvWorkflowLinked
        /// </summary>
        private void DgvWorkflowLinked_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            
            // Check if Delete button column was clicked
            if (dgvWorkflowLinked.Columns[e.ColumnIndex].Name == "colDelete")
            {
                string workflowName = dgvWorkflowLinked.Rows[e.RowIndex].Cells["colName"].Value?.ToString() ?? "";
                
                var confirm = MessageBox.Show(
                    string.Format("ต้องการลบ Workflow: {0} ใช่หรือไม่?", workflowName),
                    "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
                
                if (confirm == DialogResult.Yes)
                {
                    dgvWorkflowLinked.Rows.RemoveAt(e.RowIndex);
                    
                    // Renumber rows
                    for (int i = 0; i < dgvWorkflowLinked.Rows.Count; i++)
                    {
                        dgvWorkflowLinked.Rows[i].Cells["colNum"].Value = (i + 1).ToString();
                        dgvWorkflowLinked.Rows[i].DefaultCellStyle.BackColor = 
                            (i + 1) % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                    }
                    
                    UpdateWorkflowLinkedCount();
                    ShowInfo("ลบ Workflow เรียบร้อยแล้ว");
                }
            }
        }

        /// <summary>
        /// Handle double-click on search results to add workflow
        /// </summary>
        private void DgvWorkflowSearch_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            
            try
            {
                var row = dgvWorkflowSearch.Rows[e.RowIndex];
                string code = row.Cells["colCode"].Value?.ToString() ?? "";
                string name = row.Cells["colName"].Value?.ToString() ?? "";
                string status = row.Cells["colStatus"].Value?.ToString() ?? "";
                
                // Add to linked workflows
                int newIndex = dgvWorkflowLinked.Rows.Count + 1;
                int rowIndex = dgvWorkflowLinked.Rows.Add(
                    newIndex.ToString(),
                    code,
                    name,
                    status,
                    "🗑️ ลบ"
                );
                
                // Alternating row colors
                dgvWorkflowLinked.Rows[rowIndex].DefaultCellStyle.BackColor = 
                    newIndex % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                
                UpdateWorkflowLinkedCount();
                ShowInfo("เพิ่ม Workflow เรียบร้อยแล้ว");
            }
            catch (Exception ex)
            {
                ShowError("เพิ่ม Workflow ไม่สำเร็จ: " + ex.Message);
            }
        }

        /// <summary>
        /// Handle search textbox Enter event (clear placeholder)
        /// </summary>
        private void TxtSearchWorkflow_Enter(object sender, EventArgs e)
        {
            if (txtSearchWorkflow.Text == "🔍 ค้นหา..." && txtSearchWorkflow.ForeColor == Color.Gray)
            {
                txtSearchWorkflow.Text = "";
                txtSearchWorkflow.ForeColor = Color.Black;
            }
        }

        /// <summary>
        /// Handle search textbox Leave event (restore placeholder)
        /// </summary>
        private void TxtSearchWorkflow_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearchWorkflow.Text))
            {
                txtSearchWorkflow.Text = "🔍 ค้นหา...";
                txtSearchWorkflow.ForeColor = Color.Gray;
            }
        }

        /// <summary>
        /// Handle search textbox KeyDown event (Enter key triggers search)
        /// </summary>
        private void TxtSearchWorkflow_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BtnSearchWorkflow_Click(sender, e);
            }
        }

        /// <summary>
        /// Handle search button click
        /// </summary>
        private void BtnSearchWorkflow_Click(object sender, EventArgs e)
        {
            dgvWorkflowSearch.Rows.Clear();
            
            // Mockup search results - will be replaced with actual Workflow Engine integration
            var mockupResults = new[]
            {
                new { Code = "WF-DOC-003", Name = "Document Review Workflow", Type = "Review", Status = "Active" },
                new { Code = "WF-APPR-004", Name = "Multi-Level Approval", Type = "Approval", Status = "Active" },
                new { Code = "WF-AUDIT-005", Name = "Audit Trail Workflow", Type = "Sequential", Status = "Active" },
                new { Code = "WF-PAR-006", Name = "Parallel Processing Workflow", Type = "Parallel", Status = "Active" }
            };
            
            foreach (var wf in mockupResults)
            {
                int rowIndex = dgvWorkflowSearch.Rows.Add(
                    wf.Code,
                    wf.Name,
                    wf.Type,
                    wf.Status
                );
                
                dgvWorkflowSearch.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(248, 250, 253);
            }
            
            lblWorkflowSearch.Text = string.Format("ผลการค้นหา ({0} รายการ) — ดับเบิลคลิกเพื่อเพิ่ม", dgvWorkflowSearch.Rows.Count);
        }

        // ══════════════════════════════════════════════════════════════════════════
        // TAB ATTACH

        /// <summary>
        /// Initialize DataGridView columns for attached files
        /// </summary>
        private void InitializeAttachDataGridView()
        {
            // Columns are now defined in Designer.cs
            // No column creation needed here
        }

        /// <summary>
        /// Load attached files when form initializes (Mockup data)
        /// </summary>
        private void LoadAttachedFiles()
        {
            dgvAttachLinked.Rows.Clear();
            
            // Mockup data - will be replaced with actual DMS integration
            
            UpdateAttachLinkedCount();
        }

        private void UpdateAttachLinkedCount()
        {
            lblAttachLinked.Text = string.Format("ไฟล์ที่แนบ ({0} ไฟล์)", dgvAttachLinked.Rows.Count);
        }

        private void DgvAttachLinked_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // Header clicked
            
            // Check if View button clicked
            if (e.ColumnIndex == dgvAttachLinked.Columns["colView"].Index)
            {
                string fileName = dgvAttachLinked.Rows[e.RowIndex].Cells["colFilename"].Value?.ToString();
                object tag = dgvAttachLinked.Rows[e.RowIndex].Tag;

                if (tag is int fileId)
                {
                    if (fileId <= 2)
                    {
                        // Mockup files (UW_Criteria_Table.xlsx, Claims_Form_2569.pdf)
                        ShowInfo(string.Format("เปิดไฟล์จำลอง: {0}\n\n(ไฟล์นี้เป็นตัวอย่าง mockup ที่ไม่ได้อัปโหลดขึ้นเซิร์ฟเวอร์จริง)", fileName));
                    }
                    else
                    {
                        // Real files uploaded via FrmExternalUpload
                        DownloadAndOpenFile(fileId, fileName);
                    }
                }
                else
                {
                    ShowInfo(string.Format("เปิดไฟล์: {0}\n\n(ไม่พบรหัสไฟล์สำหรับดาวน์โหลด)", fileName));
                }
            }
            // Check if Delete button clicked
            else if (e.ColumnIndex == dgvAttachLinked.Columns["colDelete"].Index)
            {
                string fileName = dgvAttachLinked.Rows[e.RowIndex].Cells["colFilename"].Value?.ToString();
                object tag = dgvAttachLinked.Rows[e.RowIndex].Tag;

                var confirm = MessageBox.Show(
                    string.Format("ต้องการลบไฟล์ '{0}' ใช่หรือไม่?", fileName),
                    "ยืนยันการลบ",
                    MessageBoxButtons.YesNo,
                    MessageBoxIcon.Question);

                if (confirm == DialogResult.Yes)
                {
                    if (tag is int fileId && _presenter != null)
                    {
                        try
                        {
                            _presenter.RemoveArticleAttachmentLink(fileId);

                            // Deletion already persisted to DB (existing article) — make sure
                            // ArticleViewerForm refreshes its own attachment grid on return.
                            if (_presenter.EditingArticle != null)
                                _savedSuccessfully = true;
                        }
                        catch (Exception ex)
                        {
                            ShowError("ลบข้อมูลการเชื่อมโยงไฟล์ไม่สำเร็จ: " + ex.Message);
                            return;
                        }
                    }

                    dgvAttachLinked.Rows.RemoveAt(e.RowIndex);

                    // Renumber
                    for (int i = 0; i < dgvAttachLinked.Rows.Count; i++)
                    {
                        dgvAttachLinked.Rows[i].Cells["colNum"].Value = (i + 1).ToString();
                        dgvAttachLinked.Rows[i].DefaultCellStyle.BackColor = 
                            (i + 1) % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                    }
                    
                    UpdateAttachLinkedCount();
                    ShowInfo("ลบไฟล์เรียบร้อยแล้ว");
                }
            }
        }

        private void DownloadAndOpenFile(int fileId, string fileName)
        {
            if (_fileBrowseController == null)
            {
                MessageBox.Show("ระบบดูไฟล์ (File Browse Controller) ยังไม่ได้เชื่อมต่อ", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            try
            {
                this.Cursor = Cursors.WaitCursor;

                var result = _fileBrowseController.DownloadFile(fileId, _user, bypassPermissionCheck: true);
                if (result == null || !result.Success || result.FileStream == null)
                {
                    string err = (result != null && !string.IsNullOrWhiteSpace(result.ErrorMessage))
                        ? result.ErrorMessage
                        : "ไม่สามารถดาวน์โหลดไฟล์จากระบบได้";
                    MessageBox.Show(err, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    return;
                }

                // Create secure temp path
                string safeName = string.Join("_", fileName.Split(System.IO.Path.GetInvalidFileNameChars()));
                string tempDir = System.IO.Path.Combine(System.IO.Path.GetTempPath(), "CMS", "OpenFiles");
                System.IO.Directory.CreateDirectory(tempDir);

                string tempPath = System.IO.Path.Combine(tempDir, safeName);
                
                // Avoid conflicts if same file is already created
                try
                {
                    if (System.IO.File.Exists(tempPath))
                    {
                        System.IO.File.Delete(tempPath);
                    }
                }
                catch { /* ignore if file is locked/open */ }

                using (result.FileStream)
                using (var fs = new System.IO.FileStream(tempPath, System.IO.FileMode.Create, System.IO.FileAccess.Write, System.IO.FileShare.Read))
                {
                    result.FileStream.CopyTo(fs);
                }

                System.Diagnostics.Process.Start(new System.Diagnostics.ProcessStartInfo
                {
                    FileName = tempPath,
                    UseShellExecute = true
                });
            }
            catch (Exception ex)
            {
                MessageBox.Show(string.Format("ไม่สามารถเปิดไฟล์ได้: {0}\nข้อผิดพลาด: {1}", fileName, ex.Message),
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                this.Cursor = Cursors.Default;
            }
        }

        private void BtnUploadFileNew_Click(object sender, EventArgs e)
        {
            if (_fileUploadController == null)
            {
                MessageBox.Show("ระบบอัปโหลดไฟล์ยังไม่ถูกตั้งค่าผ่าน Composition Root", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                return;
            }

            int folderId = 1; // Default target folder ID for KMS Attachments
            using (var uploadForm = new FrmExternalUpload(_fileUploadController, _user, folderId, "KMS Attachments", 4, _checkpointController, _folderCommandController, _folderReadLookupController, _fileBrowseController, _fileCommandController, _networkDriveHelper))
            {
                if (uploadForm.ShowDialog(this) == DialogResult.OK)
                {
                    foreach (var file in uploadForm.UploadedFiles)
                    {
                        int newIndex = dgvAttachLinked.Rows.Count + 1;
                        int rowIndex = dgvAttachLinked.Rows.Add(
                            newIndex.ToString(),
                            file.FileName,
                            FormatFileSize(file.FileSize),
                            DateTime.Now.ToString("dd/MM/yyyy HH:mm")
                        );

                        dgvAttachLinked.Rows[rowIndex].Tag = file.FileId;
                        dgvAttachLinked.Rows[rowIndex].DefaultCellStyle.BackColor =
                            newIndex % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                    }
                    UpdateAttachLinkedCount();

                    // If the article already exists, persist the attachments/links right away
                    // (same as Save Draft). New, not-yet-saved articles keep the old behavior:
                    // attachments stay local until btnSaveDraft is clicked.
                    if (_presenter != null && _presenter.EditingArticle != null)
                    {
                        try
                        {
                            _presenter.SaveAttachmentsAndLinksIfExisting();
                            _savedSuccessfully = true;
                        }
                        catch (Exception ex)
                        {
                            ShowError("บันทึกไฟล์แนบไม่สำเร็จ: " + ex.Message);
                        }
                    }
                }
            }
        }

        private void BtnSelectFromDMS_Click(object sender, EventArgs e)
        {
            if (_folderReadLookupController == null || _fileBrowseController == null)
            {
                MessageBox.Show("ระบบเลือกไฟล์จาก DMS ยังไม่ถูกตั้งค่าผ่าน Composition Root",
                    "ไม่พร้อมใช้งาน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            using (var picker = new FrmDmsFilePicker(_folderReadLookupController, _fileBrowseController, _user, 4, _networkDriveHelper))
            {
                if (picker.ShowDialog(this) != DialogResult.OK)
                    return;

                var existing = new HashSet<int>();
                foreach (DataGridViewRow r in dgvAttachLinked.Rows)
                    if (r.Tag is int id) existing.Add(id);

                foreach (var f in picker.SelectedFiles)
                {
                    if (existing.Contains(f.FileID)) continue;

                    int newIndex = dgvAttachLinked.Rows.Count + 1;
                    int rowIndex = dgvAttachLinked.Rows.Add(
                        newIndex.ToString(),
                        f.OriginalFileName,
                        FormatFileSize(f.FileSizeBytes),
                        DateTime.Now.ToString("dd/MM/yyyy HH:mm"));

                    dgvAttachLinked.Rows[rowIndex].Tag = f.FileID;
                    dgvAttachLinked.Rows[rowIndex].DefaultCellStyle.BackColor =
                        newIndex % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                    existing.Add(f.FileID);
                }

                UpdateAttachLinkedCount();
            }
        }

        // ══════════════════════════════════════════════════════════════════════════
        // TAB RELATED - NEW IMPLEMENTATION (Two-Section Design with DataGridView)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Initialize DataGridView columns for related articles
        /// </summary>
        private void InitializeRelatedDataGridViews()
        {
            // Columns are now defined in Designer.cs
            // No need to create columns here anymore
            
            // ── Initialize Search Filters ──
            // Type filter
            cboRelatedType.Items.Clear();
            cboRelatedType.Items.Add("ทุกประเภท");
            cboRelatedType.Items.Add("SOP");
            cboRelatedType.Items.Add("FAQ");
            cboRelatedType.Items.Add("Guideline");
            cboRelatedType.Items.Add("Template");
            cboRelatedType.Items.Add("Policy");
            cboRelatedType.SelectedIndex = 0;
            
            // Category filter - will be populated by InitializeRelatedCategoryDropdown()
            cboRelatedCategory.Items.Clear();
            cboRelatedCategory.Items.Add("ทุกหมวด");
            cboRelatedCategory.SelectedIndex = 0;
            
            // Status filter
            cboRelatedStatus.Items.Clear();
            cboRelatedStatus.Items.Add("ทั้งหมด");
            cboRelatedStatus.Items.Add("Draft");
            cboRelatedStatus.Items.Add("Review");
            cboRelatedStatus.Items.Add("Published");
            cboRelatedStatus.Items.Add("Archived");
            cboRelatedStatus.SelectedIndex = 0;
            
            // Placeholder text for search textbox
            txtSearchRelated.Text = "";
            txtSearchRelated.ForeColor = Color.Gray;
        }

        // ══════════════════════════════════════════════════════════════════════════
        // TAB DOCS - NEW IMPLEMENTATION (Two-Section Design - Mockup)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Initialize DataGridView columns for documents
        /// </summary>
        private void InitializeDocsDataGridViews()
        {
            // Columns are now defined in Designer.cs
            // No column creation needed here
            
            // ── Initialize Search Filters ──
            // Type filter
            cboDocsType.Items.Clear();
            cboDocsType.Items.Add("ทุกประเภท");
            cboDocsType.Items.Add("PDF");
            cboDocsType.Items.Add("Word");
            cboDocsType.Items.Add("Excel");
            cboDocsType.Items.Add("PowerPoint");
            cboDocsType.Items.Add("Image");
            cboDocsType.Items.Add("Other");
            cboDocsType.SelectedIndex = 0;
            
            // Folder filter
            cboDocsFolder.Items.Clear();
            cboDocsFolder.Items.Add("ทุกโฟลเดอร์");
            cboDocsFolder.Items.Add("Policies");
            cboDocsFolder.Items.Add("Guidelines");
            cboDocsFolder.Items.Add("Templates");
            cboDocsFolder.Items.Add("Reports");
            cboDocsFolder.Items.Add("Archives");
            cboDocsFolder.SelectedIndex = 0;
            
            // Status filter
            cboDocsStatus.Items.Clear();
            cboDocsStatus.Items.Add("ทั้งหมด");
            cboDocsStatus.Items.Add("Active");
            cboDocsStatus.Items.Add("Archived");
            cboDocsStatus.Items.Add("Draft");
            cboDocsStatus.SelectedIndex = 0;
            
            // Placeholder text for search textbox
            txtSearchDocs.Text = "🔍 ค้นหาชื่อเอกสาร / รหัส...";
            txtSearchDocs.ForeColor = Color.Gray;
        }

        /// <summary>
        /// Load linked documents when form initializes (Mockup data)
        /// </summary>
        private void LoadLinkedDocuments()
        {
            dgvDocsLinked.Rows.Clear();
            
            // Mockup data - will be replaced with actual DMS integration
            var mockupDocs = new[]
            {
                new { Name = "Policy_Document_2569.pdf", Size = 1024 * 1024L, Date = DateTime.Now.AddDays(-10) },
                new { Name = "Compliance_Guidelines.docx", Size = 512 * 1024L, Date = DateTime.Now.AddDays(-7) }
            };
            
            int index = 1;
            foreach (var doc in mockupDocs)
            {
                int rowIndex = dgvDocsLinked.Rows.Add(
                    index.ToString(),
                    doc.Name,
                    FormatFileSize(doc.Size),
                    doc.Date.ToString("dd/MM/yyyy HH:mm")
                );
                dgvDocsLinked.Rows[rowIndex].Tag = index; // Mockup ID
                dgvDocsLinked.Rows[rowIndex].DefaultCellStyle.BackColor = 
                    index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                index++;
            }
            
            UpdateDocsLinkedCount();
        }

        private void UpdateDocsLinkedCount()
        {
            lblDocsLinked.Text = string.Format("เอกสารที่เชื่อมโยง ({0} เอกสาร)", dgvDocsLinked.Rows.Count);
        }

        private void DgvDocsLinked_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // Header clicked
            
            // Check if View button clicked
            if (dgvDocsLinked.Columns.Contains("colView") && e.ColumnIndex == dgvDocsLinked.Columns["colView"].Index)
            {
                string docName = dgvDocsLinked.Rows[e.RowIndex].Cells["colDocName"].Value?.ToString();
                ShowInfo(string.Format("เปิดเอกสาร: {0}\n\n(Mockup - ยังไม่ได้เชื่อมต่อกับ DMS)", docName));
            }
            // Check if Delete button clicked
            else if (dgvDocsLinked.Columns.Contains("colDelete") && e.ColumnIndex == dgvDocsLinked.Columns["colDelete"].Index)
            {
                string docName = dgvDocsLinked.Rows[e.RowIndex].Cells["colDocName"].Value?.ToString();
                
                var confirm = MessageBox.Show(
                    string.Format("ต้องการลบเอกสาร '{0}' ใช่หรือไม่?", docName),
                    "ยืนยันการลบ", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                
                if (confirm == DialogResult.Yes)
                {
                    dgvDocsLinked.Rows.RemoveAt(e.RowIndex);
                    
                    // Renumber
                    for (int i = 0; i < dgvDocsLinked.Rows.Count; i++)
                    {
                        dgvDocsLinked.Rows[i].Cells["colNum"].Value = (i + 1).ToString();
                        dgvDocsLinked.Rows[i].DefaultCellStyle.BackColor = 
                            (i + 1) % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                    }
                    
                    UpdateDocsLinkedCount();
                    ShowInfo("ลบเอกสารเรียบร้อยแล้ว");
                }
            }
        }

        private void DgvDocsSearch_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // Header clicked
            
            try
            {
                // Add document to linked list
                int newIndex = dgvDocsLinked.Rows.Count + 1;
                int rowIndex = dgvDocsLinked.Rows.Add(
                    newIndex.ToString(),
                    dgvDocsSearch.Rows[e.RowIndex].Cells["colSearchTitle"].Value,
                    dgvDocsSearch.Rows[e.RowIndex].Cells["colSearchSize"].Value,
                    DateTime.Now.ToString("dd/MM/yyyy HH:mm")
                );
                
                dgvDocsLinked.Rows[rowIndex].Tag = newIndex;
                dgvDocsLinked.Rows[rowIndex].DefaultCellStyle.BackColor = 
                    newIndex % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                
                // Remove from search results
                dgvDocsSearch.Rows.RemoveAt(e.RowIndex);
                
                UpdateDocsLinkedCount();
                lblDocsSearch.Text = string.Format("ผลการค้นหา ({0} รายการ) — ดับเบิลคลิกเพื่อเพิ่ม", dgvDocsSearch.Rows.Count);
                
                ShowInfo("เพิ่มเอกสารเรียบร้อยแล้ว");
            }
            catch (Exception ex)
            {
                ShowError("เพิ่มเอกสารไม่สำเร็จ: " + ex.Message);
            }
        }

        private void TxtSearchDocs_Enter(object sender, EventArgs e)
        {
            if (txtSearchDocs.Text == "🔍 ค้นหาชื่อเอกสาร / รหัส...")
            {
                txtSearchDocs.Text = string.Empty;
                txtSearchDocs.ForeColor = Color.Black;
            }
        }

        private void TxtSearchDocs_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearchDocs.Text))
            {
                txtSearchDocs.Text = "🔍 ค้นหาชื่อเอกสาร / รหัส...";
                txtSearchDocs.ForeColor = Color.Gray;
            }
        }

        private void TxtSearchDocs_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BtnSearchDocs_Click(sender, e);
            }
        }

        private void BtnSearchDocs_Click(object sender, EventArgs e)
        {
            try
            {
                string searchText = txtSearchDocs.Text.Trim();
                if (searchText == "🔍 ค้นหาชื่อเอกสาร / รหัส...")
                    searchText = string.Empty;
                
                // Mockup search results
                dgvDocsSearch.Rows.Clear();
                
                if (string.IsNullOrEmpty(searchText))
                {
                    ShowInfo("กรุณากรอกคำค้นหา");
                    return;
                }
                
                // Mockup data - will be replaced with actual DMS search
                var mockupResults = new[]
                {
                    new { Code = "DOC-2024-001", Title = "Annual Report 2024.pdf", Folder = "Reports", Size = "2.5 MB" },
                    new { Code = "DOC-2024-002", Title = "Budget Plan 2024.xlsx", Folder = "Finance", Size = "1.8 MB" },
                    new { Code = "DOC-2024-003", Title = "Meeting Minutes.docx", Folder = "Meetings", Size = "450 KB" }
                };
                
                foreach (var doc in mockupResults)
                {
                    int rowIndex = dgvDocsSearch.Rows.Add(
                        doc.Code,
                        doc.Title,
                        doc.Folder,
                        doc.Size
                    );
                    dgvDocsSearch.Rows[rowIndex].Tag = doc.Code;
                }
                
                lblDocsSearch.Text = string.Format("ผลการค้นหา ({0} รายการ) — ดับเบิลคลิกเพื่อเพิ่ม", dgvDocsSearch.Rows.Count);
                
                if (dgvDocsSearch.Rows.Count == 0)
                    ShowInfo("ไม่พบเอกสารที่ตรงกับเงื่อนไขการค้นหา");
            }
            catch (Exception ex)
            {
                ShowError("ค้นหาไม่สำเร็จ: " + ex.Message);
            }
        }

        private void BtnSelectDocFromDMSNew_Click(object sender, EventArgs e)
        {
            ShowInfo("ฟีเจอร์เลือกเอกสารจาก DMS จะเปิด Document Browser\n\n" +
                     "สามารถเลือกเอกสารจากระบบจัดการเอกสาร (DMS) ที่มีอยู่แล้ว\n\n" +
                     "(Mockup - ยังไม่ได้เชื่อมต่อกับ DMS Module)");
        }

        /// <summary>
        /// Load linked articles when form initializes
        /// </summary>
        private void LoadRelatedArticles()
        {
            if (_presenter == null) return;
            
            dgvRelatedLinked.Rows.Clear();
            
            try
            {
                if (_presenter.EditingArticle == null)
                {
                    UpdateRelatedLinkedCount();
                    return;
                }
                
                var linkedArticles = _presenter.GetLinkedArticles(_presenter.EditingArticle.ArticleID);
                
                int index = 1;
                foreach (var article in linkedArticles)
                {
                    int rowIndex = dgvRelatedLinked.Rows.Add(
                        index.ToString(),
                        article.ArticleCode ?? "-",
                        article.Title ?? "-",
                        article.Summary ?? "-",
                        article.ArticleTypeName ?? "-"
                    );
                    
                    dgvRelatedLinked.Rows[rowIndex].Tag = article.ArticleID;
                    dgvRelatedLinked.Rows[rowIndex].DefaultCellStyle.BackColor = 
                        index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                    index++;
                }
                
                UpdateRelatedLinkedCount();
            }
            catch (Exception ex)
            {
                ShowError("โหลดบทความที่เกี่ยวข้องไม่สำเร็จ: " + ex.Message);
            }
        }

        private void UpdateRelatedLinkedCount()
        {
            lblRelatedLinked.Text = string.Format("รายการที่ Link อยู่แล้ว ({0} รายการ)", dgvRelatedLinked.Rows.Count);
        }
        
        private void DgvRelatedLinked_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // Header clicked
            
            // Check if Delete button clicked
            if (e.ColumnIndex == dgvRelatedLinked.Columns["colRelatedLinkedDelete"].Index)
            {
                var row = dgvRelatedLinked.Rows[e.RowIndex];
                if (row.Tag == null) return;

                int articleId = (int)row.Tag;
                string title = row.Cells["colTitle"].Value?.ToString();
                
                var confirm = MessageBox.Show(
                    string.Format("ต้องการลบความเชื่อมโยงกับบทความ '{0}' ใช่หรือไม่?", title),
                    "ยืนยันการลบ", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                
                if (confirm == DialogResult.Yes)
                {
                    try
                    {
                        if (_presenter != null && _presenter.EditingArticle != null)
                            _presenter.RemoveRelatedArticleLink(articleId);

                        dgvRelatedLinked.Rows.RemoveAt(e.RowIndex);
                        
                        // Renumber
                        for (int i = 0; i < dgvRelatedLinked.Rows.Count; i++)
                        {
                            dgvRelatedLinked.Rows[i].Cells["colRelatedLinkedNum"].Value = (i + 1).ToString();
                            dgvRelatedLinked.Rows[i].DefaultCellStyle.BackColor = 
                                (i + 1) % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                        }
                        
                        UpdateRelatedLinkedCount();
                        ShowInfo("ลบความเชื่อมโยงเรียบร้อยแล้ว");
                    }
                    catch (Exception ex)
                    {
                        ShowError("ลบความเชื่อมโยงไม่สำเร็จ: " + ex.Message);
                    }
                }
            }
        }

        private void InitializeRelatedCategoryDropdown()
        {
            try
            {
                cboRelatedCategory.Items.Clear();
                cboRelatedCategory.Items.Add(new ComboItem("หมวดหมู่ ▼", 0));
                
                var categories = _presenter.GetAllCategories();
                foreach (var category in categories.OrderBy(c => c.SortOrder).ThenBy(c => c.CategoryName))
                {
                    cboRelatedCategory.Items.Add(new ComboItem(
                        category.ParentCategoryID.HasValue 
                            ? string.Format("  └ {0}", category.CategoryName)
                            : category.CategoryName,
                        category.CategoryID));
                }
                
                cboRelatedCategory.DisplayMember = "Display";
                cboRelatedCategory.ValueMember = "Value";
                cboRelatedCategory.SelectedIndex = 0;
            }
            catch { }
        }

        private void BtnSearchRelated_Click(object sender, EventArgs e)
        {
            try
            {
                string searchText = txtSearchRelated.Text.Trim();
                if (searchText == "🔍 ค้นหาชื่อบทความ / รหัส / แท็ก...")
                    searchText = string.Empty;
                
                byte? articleType = cboRelatedType.SelectedIndex > 0 ? (byte?)cboRelatedType.SelectedIndex : null;
                
                int? categoryId = null;
                if (cboRelatedCategory.SelectedIndex > 0)
                {
                    var selectedItem = cboRelatedCategory.SelectedItem as ComboItem;
                    if (selectedItem != null && selectedItem.Value > 0)
                        categoryId = selectedItem.Value;
                }
                
                byte? status = null; // Published
                if (cboRelatedStatus.SelectedIndex > 0)
                    status = (byte)cboRelatedStatus.SelectedIndex;
                
                var results = _presenter.SearchArticles(searchText, categoryId, status, articleType);
                
                if (_presenter.EditingArticle != null)
                    results = results.Where(a => a.ArticleID != _presenter.EditingArticle.ArticleID);
                
                var linkedIds = new HashSet<int>(
                    dgvRelatedLinked.Rows.Cast<DataGridViewRow>().Select(row => (int)row.Tag)
                );
                results = results.Where(a => !linkedIds.Contains(a.ArticleID));
                
                dgvRelatedSearch.Rows.Clear();
                foreach (var article in results.Take(50))
                {
                    int rowIndex = dgvRelatedSearch.Rows.Add(
                        article.ArticleCode ?? "-",
                        article.Title ?? "-",
                        article.Summary ?? "-",
                        article.ArticleTypeName ?? "-",
                        article.ViewCount.ToString()
                    );
                    
                    dgvRelatedSearch.Rows[rowIndex].Tag = article.ArticleID;
                }
                
                lblRelatedSearch.Text = string.Format("ผลการค้นหา ({0} รายการ) — ดับเบิลคลิกเพื่อเพิ่ม", dgvRelatedSearch.Rows.Count);
                
                if (dgvRelatedSearch.Rows.Count == 0)
                    ShowInfo("ไม่พบบทความที่ตรงกับเงื่อนไขการค้นหา");
            }
            catch (Exception ex)
            {
                ShowError("ค้นหาไม่สำเร็จ: " + ex.Message);
            }
        }
        
        private void DgvRelatedSearch_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // Header clicked
            
            if (_presenter == null)
            {
                ShowError("ระบบยังไม่พร้อม กรุณาลองใหม่อีกครั้ง");
                return;
            }
            
            if (_presenter.EditingArticle == null)
            {
                ShowError("ไม่สามารถเพิ่มความเชื่อมโยงได้ เนื่องจากยังไม่ได้บันทึกบทความ\n\nกรุณาบันทึกบทความก่อนเพิ่มความเชื่อมโยง");
                return;
            }
            
            int articleId = (int)dgvRelatedSearch.Rows[e.RowIndex].Tag;
            
            // Check if already linked
            foreach (DataGridViewRow row in dgvRelatedLinked.Rows)
            {
                if ((int)row.Tag == articleId)
                {
                    ShowInfo("บทความนี้ถูก Link อยู่แล้ว");
                    return;
                }
            }

            //try
            //{
            //    // Add link
            //    _presenter.AddRelatedArticleLink(articleId);

            //    // Add to linked grid
            int newIndex = dgvRelatedLinked.Rows.Count + 1;
            int rowIndex = dgvRelatedLinked.Rows.Add(
                newIndex.ToString(),
                dgvRelatedSearch.Rows[e.RowIndex].Cells["colRelatedSearchCode"].Value,
                dgvRelatedSearch.Rows[e.RowIndex].Cells["colRelatedSearchTitle"].Value,
                dgvRelatedSearch.Rows[e.RowIndex].Cells["colRelatedSummary"].Value,
                dgvRelatedSearch.Rows[e.RowIndex].Cells["colRelatedSearchType"].Value
            );

            dgvRelatedLinked.Rows[rowIndex].Tag = articleId;
            dgvRelatedLinked.Rows[rowIndex].DefaultCellStyle.BackColor =
                newIndex % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);

            // Remove from search results
            dgvRelatedSearch.Rows.RemoveAt(e.RowIndex);

            UpdateRelatedLinkedCount();
            lblRelatedSearch.Text = string.Format("ผลการค้นหา ({0} รายการ) — ดับเบิลคลิกเพื่อเพิ่ม", dgvRelatedSearch.Rows.Count);

            //    ShowInfo("เพิ่มความเชื่อมโยงเรียบร้อยแล้ว");
            //}
            //catch (Exception ex)
            //{
            //    ShowError("เพิ่มความเชื่อมโยงไม่สำเร็จ: " + ex.Message);
            //}
        }

        private void TxtSearchRelatedNew_Enter(object sender, EventArgs e)
        {
            if (txtSearchRelated.Text == "🔍 ค้นหาชื่อบทความ / รหัส / แท็ก...")
            {
                txtSearchRelated.Text = string.Empty;
                txtSearchRelated.ForeColor = Color.Black;
            }
        }

        private void TxtSearchRelatedNew_Leave(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtSearchRelated.Text))
            {
                txtSearchRelated.Text = "🔍 ค้นหาชื่อบทความ / รหัส / แท็ก...";
                txtSearchRelated.ForeColor = Color.Gray;
            }
        }

        private void TxtSearchRelatedNew_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.KeyCode == Keys.Enter)
            {
                e.SuppressKeyPress = true;
                BtnSearchRelated_Click(sender, e);
            }
        }

        // ══════════════════════════════════════════════════════════════════════════
        // TAB DEPARTMENTS - NEW IMPLEMENTATION (Mockup with DataGridView)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Initialize DataGridView for departments
        /// </summary>
        private void InitializeDepartmentsDataGridView()
        {
            // Columns are now defined in Designer.cs
            // No column creation needed here
        }

        /// <summary>
        /// Load linked departments when form initializes (Mockup data)
        /// </summary>
        private void LoadLinkedDepartments()
        {
            dgvDepartmentsLinked.Rows.Clear();
            
            // Mockup data - will be replaced with actual HR/Org integration
            var mockupDepartments = new[]
            {
                new { Code = "DEPT-UW-001", Name = "ฝ่ายรับประกันภัย (Underwriting)", Type = "ฝ่าย" },
                new { Code = "DEPT-CLM-002", Name = "ฝ่ายสินไหมทดแทน (Claims)", Type = "ฝ่าย" },
                new { Code = "DEPT-ACT-003", Name = "แผนกคณิตศาสตร์ประกันภัย (Actuarial)", Type = "แผนก" },
                new { Code = "DEPT-IT-004", Name = "ฝ่ายเทคโนโลยีสารสนเทศ (IT)", Type = "ฝ่าย" }
            };
            
            int index = 1;
            foreach (var dept in mockupDepartments)
            {
                int rowIndex = dgvDepartmentsLinked.Rows.Add(
                    index.ToString(),
                    dept.Code,
                    dept.Name,
                    dept.Type
                );
                
                dgvDepartmentsLinked.Rows[rowIndex].Tag = index; // Mockup ID
                dgvDepartmentsLinked.Rows[rowIndex].DefaultCellStyle.BackColor = 
                    index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                index++;
            }
            
            UpdateDepartmentsLinkedCount();
        }

        private void UpdateDepartmentsLinkedCount()
        {
            lblDepartmentsLinked.Text = string.Format("ฝ่าย/แผนก ที่เกี่ยวข้อง ({0} รายการ)", dgvDepartmentsLinked.Rows.Count);
        }

        private void DgvDepartmentsLinked_CellContentClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return; // Header clicked
            
            // Check if Delete button clicked
            if (e.ColumnIndex == dgvDepartmentsLinked.Columns["colDeptDelete"].Index)
            {
                string deptName = dgvDepartmentsLinked.Rows[e.RowIndex].Cells["colDeptName"].Value?.ToString();
                
                var confirm = MessageBox.Show(
                    string.Format("ต้องการลบ '{0}' ใช่หรือไม่?", deptName),
                    "ยืนยันการลบ", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                
                if (confirm == DialogResult.Yes)
                {
                    dgvDepartmentsLinked.Rows.RemoveAt(e.RowIndex);
                    
                    // Renumber
                    for (int i = 0; i < dgvDepartmentsLinked.Rows.Count; i++)
                    {
                        dgvDepartmentsLinked.Rows[i].Cells["colDeptNum"].Value = (i + 1).ToString();
                        dgvDepartmentsLinked.Rows[i].DefaultCellStyle.BackColor = 
                            (i + 1) % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                    }
                    
                    UpdateDepartmentsLinkedCount();
                    ShowInfo("ลบฝ่าย/แผนกเรียบร้อยแล้ว");
                }
            }
        }

        // ── Helper Classes ───────────────────────────────────────────────────

        private class ComboItem
        {
            public string Display { get; private set; }
            public int    Value   { get; private set; }

            public ComboItem(string display, int value)
            {
                Display = display;
                Value   = value;
            }

            public override string ToString()
            {
                return Display;
            }
        }
    }
}