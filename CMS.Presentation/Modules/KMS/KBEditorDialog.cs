// ✅ Allowed usings: CMS.Controllers, CMS.Domain
// ❌ Forbidden usings: CMS.Services, CMS.Data
using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using Microsoft.Web.WebView2.Core;
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
    public partial class KBEditorDialog : Form, IKBEditorController
    {
        public event EventHandler BackRequested;

        private KBEditorController _presenter;
        private readonly UserContext _user;
        private readonly IFileUploadController _fileUploadController;
        private readonly IUploadCheckpointController _checkpointController;
        private readonly IFileBrowseController _fileBrowseController;

        public KBEditorDialog(UserContext user) : this(user, null, null, null) { }

        public KBEditorDialog(UserContext user, IFileUploadController fileUploadController, IUploadCheckpointController checkpointController, IFileBrowseController fileBrowseController = null)
        {
            if (user == null) throw new ArgumentNullException("user");
            _user = user;
            _fileUploadController = fileUploadController;
            _checkpointController = checkpointController;
            _fileBrowseController = fileBrowseController;
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

            InitWebEditorAsync();
            tsRteZoom.Text = _zoomPct + "%"; // default 100%
            InitTypeCombo();
            InitCrafCombo();
            
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

            string action = btnSendReview.Text.Contains("Review") ? "ส่งขอ Review" : "เผยแพร่";
            var confirm = MessageBox.Show(
                string.Format("ต้องการ{0}บทความ \"{1}\" ใช่หรือไม่?", action, txtTitle.Text.Trim()),
                "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

            if (confirm != DialogResult.Yes) return;

            try
            {
                // Check if this is "Send Review" or "Publish" based on button text
                if (btnSendReview.Text.Contains("Review"))
                {
                    _presenter.SendReview();
                }
                else
                {
                    _presenter.Publish();
                }
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
        private string _cachedHtml    = string.Empty;
        private string _cachedState   = string.Empty;
        private bool   _editorReady   = false;
        private int    _genInfoExpandedHeight  = 220;
        private int    _mappingExpandedWidth   = 529;
        private bool   _syncingToolbar = false;
        private bool   _applyingFormat = false;
        private int    _zoomPct        = 100;
        private static readonly int[] _zoomLevels = { 50, 75, 100, 125, 150, 175, 200 };
        private bool   _savedSuccessfully = false;
        private string _originalContentHtml = string.Empty;

        public string ContentHTML
        {
            get { return _editorReady ? _cachedHtml : _pendingHtml; }
            set
            {
                _pendingHtml = value ?? string.Empty;
                if (_editorReady)
                {
                    string escaped = Newtonsoft.Json.JsonConvert.SerializeObject(_pendingHtml);
                    _ = webEditor.ExecuteScriptAsync("document.body.innerHTML=" + escaped + ";notifyHtml();");
                }
            }
        }

        private string ContentText
        {
            get
            {
                if (!_editorReady || string.IsNullOrEmpty(_cachedHtml)) return _pendingHtml;
                return System.Text.RegularExpressions.Regex
                    .Replace(_cachedHtml, @"<[^>]+>", "").Replace("&nbsp;", " ").Trim();
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
                if (cboStatus.SelectedIndex < 0) return _currentStatus;
                return (byte)(cboStatus.SelectedIndex + 1);  // 1=Draft, 2=Review, 3=Published, 4=Archived
            }
            set
            {
                _currentStatus = value;
                if (value >= 1 && value <= 4)
                {
                    cboStatus.SelectedIndex = value - 1;
                }
                SetStatusDisplay(value);  // Still update status bar
            }
        }
        private byte _currentStatus = 1;

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

        // ── WebView2 Initialisation ──────────────────────────────────────────────

        private async void InitWebEditorAsync()
        {
            try
            {
                await webEditor.EnsureCoreWebView2Async(null);

                webEditor.CoreWebView2.Settings.AreDevToolsEnabled         = false;
                webEditor.CoreWebView2.Settings.IsStatusBarEnabled         = false;
                webEditor.CoreWebView2.Settings.AreDefaultContextMenusEnabled = false;

                // Open http/https links in the system browser; cancel navigation inside the editor.
                webEditor.CoreWebView2.NavigationStarting += (s, ev) =>
                {
                    if (ev.Uri.StartsWith("http://") || ev.Uri.StartsWith("https://"))
                    {
                        ev.Cancel = true;
                        try { System.Diagnostics.Process.Start(ev.Uri); }
                        catch (Exception ex) { ShowError("ไม่สามารถเปิดลิงก์ได้:\n" + ex.Message); }
                    }
                };

                // JS → C# messaging: state and html updates pushed by notifyState()/notifyHtml()
                webEditor.CoreWebView2.WebMessageReceived += WebEditor_WebMessageReceived;

                webEditor.CoreWebView2.NavigationCompleted += WebEditor_NavigationCompleted;

                // Sync zoom display when user uses Ctrl+scroll inside WebView2
                webEditor.ZoomFactorChanged += WebEditor_ZoomFactorChanged;

                webEditor.NavigateToString(GetEditorHtml());
            }
            catch (Exception ex)
            {
                ShowError("ไม่สามารถเริ่มต้น WebView2 ได้:\n" + ex.Message);
            }
        }

        private void WebEditor_NavigationCompleted(object sender, CoreWebView2NavigationCompletedEventArgs e)
        {
            if (_editorReady) return;
            _editorReady = true;

            // Inject pending HTML if the presenter already called ContentHTML = "..." before init finished
            if (!string.IsNullOrEmpty(_pendingHtml))
            {
                string escaped = Newtonsoft.Json.JsonConvert.SerializeObject(_pendingHtml);
                _ = webEditor.ExecuteScriptAsync("document.body.innerHTML=" + escaped + ";notifyHtml();");
            }
            else
            {
                _ = webEditor.ExecuteScriptAsync("notifyChange();");
            }
        }

        private void WebEditor_WebMessageReceived(object sender, CoreWebView2WebMessageReceivedEventArgs e)
        {
            var msg = e.TryGetWebMessageAsString();
            if (msg == null) return;

            if (msg.StartsWith("STATE:"))
            {
                _cachedState = msg.Substring(6);
                SyncToolbarState();
            }
            else if (msg.StartsWith("HTML:"))
            {
                bool isFirst = string.IsNullOrEmpty(_cachedHtml) && string.IsNullOrEmpty(_originalContentHtml);
                _cachedHtml = msg.Substring(5);
                if (isFirst)
                    _originalContentHtml = _cachedHtml;
                UpdateEditorStatus();
            }
        }

        // ── RTE Toolbar Handlers ─────────────────────────────────────────────

        // ── Tag Chip Management ──────────────────────────────────────────────

        private void CboStatus_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (cboStatus.SelectedIndex < 0) return;
            byte status = (byte)(cboStatus.SelectedIndex + 1);
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
            if (!_editorReady || string.IsNullOrEmpty(_cachedState)) return;
            if (_applyingFormat) return;

            try
            {
                var raw = _cachedState;
                if (!raw.Contains("|")) return;

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

        private void ExecCmd(string command, string value = null)
        {
            if (!_editorReady) return;
            string js = value == null
                ? string.Format("document.execCommand('{0}',false,null);notifyChange();", command)
                : string.Format("document.execCommand('{0}',false,{1});notifyChange();",
                      command, Newtonsoft.Json.JsonConvert.SerializeObject(value));
            _ = webEditor.ExecuteScriptAsync(js);
            webEditor.Focus();
        }

        private void PasteHtmlAtCursor(string html)
        {
            if (!_editorReady) return;
            string escaped = Newtonsoft.Json.JsonConvert.SerializeObject(html);
            _ = webEditor.ExecuteScriptAsync("pasteHtml(" + escaped + ");notifyChange();");
            webEditor.Focus();
        }

        private void TsRteBold_Click(object sender, EventArgs e)           { ExecCmd("bold"); }
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
        }

        private void TsRteFontSizeInc_Click(object sender, EventArgs e) { _ = StepFontSizeAsync(+1); }
        private void TsRteFontSizeDec_Click(object sender, EventArgs e) { _ = StepFontSizeAsync(-1); }

        private async System.Threading.Tasks.Task StepFontSizeAsync(int direction)
        {
            if (!_editorReady) return;
            if (_applyingFormat) return;
            _applyingFormat = true;
            try
            {
                // Ask JS whether there is a text selection, then read current font size
                var hasSel = await webEditor.ExecuteScriptAsync("hasTextSel()");
                if (hasSel != "true") return;

                // ExecuteScriptAsync returns a JSON string — strip surrounding quotes
                var rawSz = await webEditor.ExecuteScriptAsync("getSelFontSizePt()");
                var current = rawSz.Trim('"');

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
        }

        private void ApplyFontSizePt(string pt)
        {
            if (!_editorReady) return;
            string markerId = "fsm" + Environment.TickCount.ToString();
            string js = string.Format("applyFontSizePt({0},{1});notifyChange();",
                Newtonsoft.Json.JsonConvert.SerializeObject(pt),
                Newtonsoft.Json.JsonConvert.SerializeObject(markerId));
            _ = webEditor.ExecuteScriptAsync(js);
        }

        // ── Zoom controls ────────────────────────────────────────────────────────

        private void WebEditor_ZoomFactorChanged(object sender, EventArgs e)
        {
            int pct = (int)Math.Round(webEditor.ZoomFactor * 100);
            _zoomPct = pct;
            _syncingToolbar = true;
            tsRteZoom.Text = pct + "%";
            _syncingToolbar = false;
        }

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
            if (int.TryParse(s, out int pct) && pct >= 10 && pct <= 500)
                SetZoom(pct);
            else
            {
                // Reset display to the last valid zoom
                _syncingToolbar = true;
                tsRteZoom.Text = _zoomPct + "%";
                _syncingToolbar = false;
            }
        }

        private void SetZoom(int pct)
        {
            _zoomPct = pct;
            webEditor.ZoomFactor = pct / 100.0;
            _syncingToolbar = true;
            tsRteZoom.Text = pct + "%";
            _syncingToolbar = false;
            webEditor.Focus();
        }

        private static string GetEditorHtml()
        {
            // WebView2 (Chromium) version — all JavaScript uses W3C standard APIs.
            // No document.selection / TextRange / pasteHTML (IE-only APIs).
            // JS pushes state and html to C# via window.chrome.webview.postMessage.
            return
                "<!DOCTYPE html><html><head><meta charset='utf-8'>" +
                "<style>" +
                "html,body{height:100%;margin:0;}" +
                "body{font-family:'Segoe UI',Tahoma,sans-serif;font-size:14px;" +
                "padding:8px;min-height:100%;box-sizing:border-box;line-height:1.4;color:#333;" +
                "outline:none;}" +
                // Remove default <p> margins so lines from Enter key match <br> spacing.
                "p{margin:0;}" +
                // Prevent tab-span characters from inheriting a large font-size (e.g. 18pt)
                // that would stretch the line height beyond the surrounding text.
                "[data-kms-tab]{font-size:0!important;line-height:0!important;}" +
                "h1{font-size:22px;}h2{font-size:18px;}h3{font-size:15px;}" +
                "table{border-collapse:collapse;width:100%;margin:8px 0;}" +
                "td,th{border:1px solid #ccc;padding:4px 8px;}" +
                "th{background:#f0f0f0;}" +
                "pre{background:#f4f4f4;font-family:Consolas,monospace;padding:8px;" +
                "border-radius:4px;border:1px solid #ddd;overflow:auto;}" +
                "code{background:#f4f4f4;font-family:Consolas,monospace;padding:2px 4px;border-radius:2px;}" +
                "a{color:#2980b9;}" +
                "</style>" +
                "<script>" +
                // ── Push helpers — send state/html to C# via postMessage ──────────
                "function notifyState(){" +
                "try{window.chrome.webview.postMessage('STATE:'+getRteState());}catch(e){}}" +
                "function notifyHtml(){" +
                "try{window.chrome.webview.postMessage('HTML:'+document.body.innerHTML);}catch(e){}}" +
                "function notifyChange(){notifyState();notifyHtml();}" +
                // ── Format state query ───────────────────────────────────────────
                "function getRteState(){" +
                "try{" +
                "var b=document.queryCommandState('bold')?'1':'0';" +
                "var i=document.queryCommandState('italic')?'1':'0';" +
                "var u=document.queryCommandState('underline')?'1':'0';" +
                "var jl=document.queryCommandState('justifyLeft')?'1':'0';" +
                "var jc=document.queryCommandState('justifyCenter')?'1':'0';" +
                "var jr=document.queryCommandState('justifyRight')?'1':'0';" +
                "var ul=document.queryCommandState('insertUnorderedList')?'1':'0';" +
                "var ol=document.queryCommandState('insertOrderedList')?'1':'0';" +
                "var fn=(document.queryCommandValue('fontName')||'').toLowerCase();" +
                "var fs=getSelFontSizePt();" +
                "return b+i+u+jl+jc+jr+ul+ol+'|'+fn+'|'+fs;" +
                "}catch(e){return '00000000||';}}" +
                // ── Selection helpers ────────────────────────────────────────────
                "function hasTextSel(){" +
                "var s=window.getSelection();return !!(s&&!s.isCollapsed&&s.toString().length>0);}" +
                // ── Font size read ───────────────────────────────────────────────
                "var _htmlSzPt={'1':'8','2':'10','3':'12','4':'14','5':'18','6':'24','7':'36'};" +
                "function getSelFontSizePt(){" +
                "try{var s=window.getSelection();" +
                "if(!s||s.rangeCount===0)return '';" +
                "var el=s.getRangeAt(0).commonAncestorContainer;" +
                "if(el.nodeType===3)el=el.parentNode;" +
                "while(el&&el.tagName!=='BODY'){" +
                "if(el.style&&el.style.fontSize){var f=el.style.fontSize;" +
                "if(f.indexOf('pt')>-1)return Math.round(parseFloat(f)).toString();" +
                "if(f.indexOf('px')>-1)return Math.round(parseFloat(f)*0.75).toString();}" +
                "if(el.tagName==='FONT'){var a=el.getAttribute('size');if(a&&_htmlSzPt[a])return _htmlSzPt[a];}" +
                "el=el.parentElement;}" +
                "}catch(e){}return '';}" +
                // ── Font size apply ──────────────────────────────────────────────
                "function applyFontSizePt(pt,markerId){" +
                "try{" +
                "var s=window.getSelection();" +
                "if(!s||s.rangeCount===0||s.isCollapsed)return;" +
                "var range=s.getRangeAt(0);" +
                "var el=range.commonAncestorContainer;" +
                "if(el.nodeType===3)el=el.parentNode;" +
                // Fast path: immediate parent already has inline font-size → update in-place
                "if(el&&el.tagName!=='BODY'&&el.style&&el.style.fontSize){" +
                "el.style.fontSize=pt+'pt';return;}" +
                // Wrap selection in a new span
                "var div=document.createElement('div');" +
                "div.appendChild(range.cloneContents());" +
                "var inner=div.innerHTML.replace(/font-size\\s*:[^;'\"]+;?/gi,'');" +
                "document.execCommand('insertHTML',false," +
                "'<span id=\"'+markerId+'\" style=\"font-size:'+pt+'pt\">'+inner+'</span>');" +
                "var newEl=document.getElementById(markerId);" +
                "if(newEl){var nr=document.createRange();" +
                "nr.selectNodeContents(newEl);s.removeAllRanges();s.addRange(nr);" +
                "newEl.removeAttribute('id');}" +
                "}catch(e){}}" +
                // ── Paste HTML at cursor ──────────────────────────────────────────
                "function pasteHtml(h){" +
                "try{" +
                "var s=window.getSelection();" +
                "if(!s||s.rangeCount===0)return;" +
                "var range=s.getRangeAt(0);" +
                "range.deleteContents();" +
                "var frag=range.createContextualFragment(h);" +
                "var last=frag.lastChild;" +
                "range.insertNode(frag);" +
                "if(last){var r2=document.createRange();" +
                "r2.setStartAfter(last);r2.collapse(true);" +
                "s.removeAllRanges();s.addRange(r2);}" +
                "}catch(e){}}" +
                // ── Tab indent (pixel-accurate, Chromium) ─────────────────────────
                "var _tabPx=40;" +
                "function insertTab(){" +
                "try{" +
                "var s=window.getSelection();" +
                "if(!s||s.rangeCount===0){document.body.focus();s=window.getSelection();}" +
                "if(!s||s.rangeCount===0)return;" +
                "var range=s.getRangeAt(0);range.deleteContents();range.collapse(true);" +
                "var left=range.getBoundingClientRect().left-document.body.getBoundingClientRect().left;" +
                "var w=_tabPx-(left%_tabPx);if(w<4)w+=_tabPx;" +
                // Insert via DOM so cursor placement is explicit (execCommand leaves cursor
                // inside contenteditable=false span in Chromium, causing it to disappear).
                "var span=document.createElement('span');" +
                "span.setAttribute('data-kms-tab','1');" +
                "span.setAttribute('contenteditable','false');" +
                "span.style.cssText='display:inline-block;width:'+w+'px;overflow:hidden';" +
                "span.innerHTML='\\u00a0';" +
                "range.insertNode(span);" +
                "var r2=document.createRange();r2.setStartAfter(span);r2.collapse(true);" +
                // After insert: recalculate the next tab span's width from the new cursor
                // position so the tab-stop grid stays correct (the next span now starts
                // further right and its old width no longer hits the right column).
                "var nxt=span.nextSibling;" +
                "if(nxt&&nxt.nodeType===1&&nxt.getAttribute&&nxt.getAttribute('data-kms-tab')){" +
                "var nl=nxt.getBoundingClientRect().left-document.body.getBoundingClientRect().left;" +
                "var nw=_tabPx-(nl%_tabPx);if(nw<4)nw+=_tabPx;nxt.style.width=nw+'px';}" +
                "s.removeAllRanges();s.addRange(r2);" +
                "}catch(e){}}" +
                "function removeTab(){" +
                "try{" +
                "var s=window.getSelection();" +
                "if(!s||s.rangeCount===0)return;" +
                "var range=s.getRangeAt(0);range.collapse(true);" +
                "var node=range.startContainer,offset=range.startOffset;" +
                // Helper: remove span element, reposition cursor where span was
                "var rmSpan=function(sp){" +
                "var par=sp.parentNode,idx=Array.prototype.indexOf.call(par.childNodes,sp);" +
                "par.removeChild(sp);" +
                // After removal: recalculate the next tab span's width from its new left
                // position so the tab-stop grid stays correct (subsequent spans were
                // calculated for a cursor position that no longer exists after removal).
                "var nxt=par.childNodes[idx];" +
                "if(nxt&&nxt.nodeType===1&&nxt.getAttribute&&nxt.getAttribute('data-kms-tab')){" +
                "var nl=nxt.getBoundingClientRect().left-document.body.getBoundingClientRect().left;" +
                "var nw=_tabPx-(nl%_tabPx);if(nw<4)nw+=_tabPx;nxt.style.width=nw+'px';}" +
                "var r=document.createRange();r.setStart(par,idx);r.collapse(true);" +
                "s.removeAllRanges();s.addRange(r);};" +
                // Helper: remove consecutive &nbsp;/space ending at off in text node tn
                "var rmNbsp=function(tn,off){" +
                "var t=tn.textContent,cnt=0;" +
                "for(var i=off-1;i>=0;i--){var c=t.charCodeAt(i);if(c===160||c===32)cnt++;else break;}" +
                "if(cnt>0){tn.textContent=t.slice(0,off-cnt)+t.slice(off);" +
                "var r=document.createRange();r.setStart(tn,off-cnt);r.collapse(true);" +
                "s.removeAllRanges();s.addRange(r);};};" +
                // Case A: cursor is inside (or on) a tab span
                "var check=node.nodeType===3?node.parentNode:node;" +
                "if(check&&check.getAttribute&&check.getAttribute('data-kms-tab')){rmSpan(check);return;}" +
                // Case B: cursor is in a text node at offset > 0
                // Only examine THIS text node — do NOT fall through to previousSibling.
                // A sibling span to the left is not immediately before this cursor position.
                "if(node.nodeType===3&&offset>0){" +
                "var cb=node.textContent.charCodeAt(offset-1);" +
                "if(cb===160||cb===32)rmNbsp(node,offset);" +
                "return;}" +
                // Case C: cursor at text offset=0, or inside an element node
                // → look at the node directly before the cursor position
                "var prev;" +
                "if(node.nodeType===1)" +
                "{prev=offset>0?node.childNodes[offset-1]:node.previousSibling;}" +
                "else{prev=node.previousSibling;}" +
                // Walk up when no immediate prev: cursor may be inside a formatting span
                // e.g. <span style="font-size:18pt">[cursor]กิจสมใจ</span> where the
                // tab span is a sibling of the formatting span, not of the text node.
                "if(!prev){" +
                "var wn=node.nodeType===3?node.parentNode:node;" +
                "while(wn&&wn!==document.body&&!prev){prev=wn.previousSibling;wn=wn.parentNode;}" +
                "}" +
                "if(!prev)return;" +
                "if(prev.nodeType===1&&prev.getAttribute&&prev.getAttribute('data-kms-tab')){rmSpan(prev);return;}" +
                "if(prev.nodeType===3){var pl=prev.textContent.length;" +
                "if(pl>0){var cp=prev.textContent.charCodeAt(pl-1);" +
                "if(cp===160||cp===32)rmNbsp(prev,pl);}}" +
                "}catch(e){}}" +
                // Returns true when cursor is inside a list item (<li>)
                "function isInList(){" +
                "var s=window.getSelection();" +
                "if(!s||s.rangeCount===0)return false;" +
                "var n=s.getRangeAt(0).startContainer;" +
                "if(n.nodeType===3)n=n.parentNode;" +
                "while(n&&n!==document.body){if(n.tagName==='LI')return true;n=n.parentNode;}" +
                "return false;}" +
                // ── Tab key handled in JS so host ProcessCmdKey is not needed ─────
                "document.addEventListener('keydown',function(e){" +
                "if(e.key==='Tab'){e.preventDefault();" +
                "if(isInList()){" +
                // Inside a list: indent / outdent the list item
                "if(e.shiftKey)document.execCommand('outdent',false,null);" +
                "else document.execCommand('indent',false,null);" +
                "}else{" +
                // Outside a list: custom tab span / nbsp removal
                "if(e.shiftKey)removeTab();else insertTab();" +
                "}" +
                "notifyChange();" +
                "}});" +
                // ── Push state/html on every edit and selection change ────────────
                "document.addEventListener('input',notifyChange);" +
                "document.addEventListener('selectionchange',notifyState);" +
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
            using (var uploadForm = new FrmExternalUpload(_fileUploadController, _user, folderId, "KMS Attachments", 4, _checkpointController))
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
                }
            }
        }

        private void BtnSelectFromDMS_Click(object sender, EventArgs e)
        {
            // Mockup: Show message and add sample file
            ShowInfo("ฟีเจอร์เลือกไฟล์จาก DMS จะเปิด File Browser\n\n" +
                     "สามารถเลือกไฟล์จากระบบจัดการเอกสาร (DMS) ที่มีอยู่แล้ว\n\n" +
                     "(Mockup - ยังไม่ได้เชื่อมต่อกับ DMS Module)\n\n" +
                     "เพิ่มไฟล์ตัวอย่างเข้าไปแทน...");
            
            // Add sample file from "DMS"
            int newIndex = dgvAttachLinked.Rows.Count + 1;
            int rowIndex = dgvAttachLinked.Rows.Add(
                newIndex.ToString(),
                "Sample_From_DMS.pdf",
                "1.2 MB",
                DateTime.Now.ToString("dd/MM/yyyy HH:mm")
            );
            
            dgvAttachLinked.Rows[rowIndex].Tag = newIndex;
            dgvAttachLinked.Rows[rowIndex].DefaultCellStyle.BackColor = 
                newIndex % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
            
            UpdateAttachLinkedCount();
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