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
    /// KBEditor Dialog — แก้ไข/เพิ่มบทความ Knowledge Base (MOD-006)
    /// Redesigned from ArticleEditorDialog based on kms_editor.html
    /// Layout: General Info, Content Editor, Mapping Tabs (Attachments, Related, Workflow, Programs, Docs, Versions)
    /// Supports: Create / Edit / Send Review / Publish / Archive + Version History
    /// </summary>
    public partial class KBEditorDialog : Form, IKBEditorController
    {
        private KBEditorController _presenter;
        private readonly UserContext _user;

        public KBEditorDialog(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            _user = user;
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

            webEditor.DocumentText = GetEditorHtml();
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
            
            _presenter.Initialize();
        }

        private void BtnSaveDraft_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            if (!ValidateInput()) return;

            try
            {
                _presenter.Save(txtChangeDesc.Text.Trim());
            }
            catch (Exception ex)
            {
                ShowError("บันทึกไม่สำเร็จ:\n" + ex.Message);
            }
        }

        private void BtnSendReview_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;

            string action = btnSendReview.Text.Contains("Review") ? "ส่งขอ Review" : "เผยแพร่";
            var confirm = MessageBox.Show(
                string.Format("ต้องการ{0}บทความ \"{1}\" ใช่หรือไม่?", action, txtTitle.Text.Trim()),
                "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question);

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

        private void BtnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
            Close();
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

        private string _pendingHtml  = string.Empty;
        private bool   _editorReady = false;

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
                _pendingHtml = value ?? string.Empty;
                if (_editorReady && webEditor.Document != null && webEditor.Document.Body != null)
                    webEditor.Document.Body.InnerHtml = _pendingHtml;
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
                
                // Add chips from comma-separated string
                if (!string.IsNullOrEmpty(value))
                {
                    foreach (var tag in value.Split(new[] { ',' }, StringSplitOptions.RemoveEmptyEntries))
                    {
                        AddTagChip(tag.Trim());
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

        public string VersionTabText
        {
            set { tabVersions.Text = value; }
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

        public void BindVersionHistory(IEnumerable<KBVersion> versions)
        {
            lstVersions.Items.Clear();

            foreach (var v in versions)
            {
                var item = new ListViewItem(string.Format("v{0}", v.VersionNumber));
                item.SubItems.Add(v.CreatedDate.ToString("dd/MM/yyyy HH:mm"));
                item.SubItems.Add(v.CreatedBy ?? "-");
                item.SubItems.Add(v.ChangeDescription ?? "-");
                item.Tag       = v;
                item.BackColor = v.VersionNumber % 2 == 0
                    ? Color.White
                    : Color.FromArgb(248, 250, 253);
                lstVersions.Items.Add(item);
            }
        }

        public void CloseOk()
        {
            DialogResult = DialogResult.OK;
            Close();
        }

        public void CloseCancel()
        {
            DialogResult = DialogResult.Cancel;
            Close();
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
        
        [Obsolete("Use GetRelatedDataGridView() instead")]
        public System.Windows.Forms.ListView GetRelatedListView()
        {
            throw new NotSupportedException("ListView has been replaced with DataGridView. Use GetRelatedDataGridView() instead.");
        }

        public System.Windows.Forms.DataGridView GetWorkflowLinkedDataGridView()
        {
            return dgvWorkflowLinked;
        }
        
        public System.Windows.Forms.DataGridView GetWorkflowSearchDataGridView()
        {
            return dgvWorkflowSearch;
        }
        
        [Obsolete("Use GetWorkflowLinkedDataGridView() and GetWorkflowSearchDataGridView() instead")]
        public System.Windows.Forms.ListView GetWorkflowListView()
        {
            throw new NotSupportedException("ListView has been replaced with DataGridView. Use GetWorkflowLinkedDataGridView() and GetWorkflowSearchDataGridView() instead.");
        }

        public System.Windows.Forms.DataGridView GetProgramsLinkedDataGridView()
        {
            return dgvProgramsLinked;
        }
        
        public System.Windows.Forms.DataGridView GetProgramsSearchDataGridView()
        {
            return dgvProgramsSearch;
        }
        
        [Obsolete("Use GetProgramsLinkedDataGridView() and GetProgramsSearchDataGridView() instead")]
        public System.Windows.Forms.ListView GetProgramsListView()
        {
            throw new NotSupportedException("ListView has been replaced with DataGridView. Use GetProgramsLinkedDataGridView() and GetProgramsSearchDataGridView() instead.");
        }

        public System.Windows.Forms.DataGridView GetDocsLinkedDataGridView()
        {
            return dgvDocsLinked;
        }
        
        public System.Windows.Forms.DataGridView GetDocsSearchDataGridView()
        {
            return dgvDocsSearch;
        }
        
        [Obsolete("Use GetDocsLinkedDataGridView() and GetDocsSearchDataGridView() instead")]
        public System.Windows.Forms.ListView GetDocsListView()
        {
            throw new NotSupportedException("ListView has been replaced with DataGridView. Use GetDocsLinkedDataGridView() and GetDocsSearchDataGridView() instead.");
        }

        // ── Private UI Helpers ───────────────────────────────────────────────

        private void InitTypeCombo()
        {
            cboType.Items.Clear();
            cboType.Items.Add(new ComboItem("SOP (Standard Operating Procedure)", 1));
            cboType.Items.Add(new ComboItem("FAQ (คำถามที่พบบ่อย)",              2));
            cboType.Items.Add(new ComboItem("Guideline (แนวปฏิบัติ)",             3));
            cboType.Items.Add(new ComboItem("Template (แม่แบบเอกสาร)",            4));
            cboType.Items.Add(new ComboItem("Policy (นโยบาย)",                    5));
            cboType.SelectedIndex = 0;
        }

        private void InitCrafCombo()
        {
            cboCraf.Items.Clear();
            cboCraf.Items.Add(new ComboItem("1HC - (High Confidential)",   1));
            cboCraf.Items.Add(new ComboItem("2SC - (Semi Confidential)",   2));
            cboCraf.Items.Add(new ComboItem("3NC - (Normal Confidential)", 3));
            cboCraf.Items.Add(new ComboItem("4PC - (Public Confidential)", 4));
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
                    lblStatusInfo.Text = "🔍 รอ Review";
                    break;
                case 3:
                    lblStatusInfo.Text = "✅ Published";
                    break;
                case 4:
                    lblStatusInfo.Text = "📦 Archived";
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

        // ── RTE Toolbar Handlers ─────────────────────────────────────────────

        private void WebEditor_DocumentCompleted(object sender, System.Windows.Forms.WebBrowserDocumentCompletedEventArgs e)
        {
            if (_editorReady) return;
            if (webEditor.Document == null || webEditor.Document.Body == null) return;
            // Editability is set via contenteditable="true" on the body in GetEditorHtml()
            _editorReady = true;
            if (!string.IsNullOrEmpty(_pendingHtml))
                webEditor.Document.Body.InnerHtml = _pendingHtml;
            UpdateEditorStatus();
            webEditor.Document.Body.KeyPress += (s, ev) => UpdateEditorStatus();
            
            // Attach link click handler to open links in external browser
            webEditor.Document.Click += WebEditor_Document_Click;
        }

        private void WebEditor_Document_Click(object sender, HtmlElementEventArgs e)
        {
            if (webEditor.Document == null) return;
            
            var element = webEditor.Document.GetElementFromPoint(e.ClientMousePosition);
            if (element == null) return;
            
            // Check if clicked element or its parent is a link
            while (element != null)
            {
                if (element.TagName != null && element.TagName.ToUpper() == "A")
                {
                    string href = element.GetAttribute("href");
                    if (!string.IsNullOrEmpty(href) && (href.StartsWith("http://") || href.StartsWith("https://")))
                    {
                        e.ReturnValue = false; // Prevent default navigation
                        try
                        {
                            System.Diagnostics.Process.Start(href);
                        }
                        catch (Exception ex)
                        {
                            ShowError("ไม่สามารถเปิดลิงก์ได้:\n" + ex.Message);
                        }
                    }
                    break;
                }
                element = element.Parent;
            }
        }

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
            if (e.KeyCode == Keys.Enter || e.KeyCode == Keys.Oemcomma)
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

        private void AddTagChip(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return;
            
            // Remove comma if present
            tag = tag.TrimEnd(',');
            if (string.IsNullOrWhiteSpace(tag)) return;
            
            // Validate tag input - allow only alphanumeric, spaces, hyphens, and underscores
            if (!System.Text.RegularExpressions.Regex.IsMatch(tag, @"^[a-zA-Z0-9\s\-_]+$"))
            {
                MessageBox.Show("Tag can only contain letters, numbers, spaces, hyphens, and underscores.", 
                    "Invalid Tag", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            
            // Limit tag length
            if (tag.Length > 50)
            {
                MessageBox.Show("Tag cannot exceed 50 characters.", 
                    "Tag Too Long", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
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
                                MessageBox.Show($"Tag '{tag}' already exists.", 
                                    "Duplicate Tag", MessageBoxButtons.OK, MessageBoxIcon.Information);
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
            if (webEditor.Document == null || webEditor.Document.Body == null) return;
            int len = (webEditor.Document.Body.InnerText ?? string.Empty).Length;
            lblRteStatus.Text = string.Format("🖊️ กำลังแก้ไข  |  จำนวนอักขระ: {0}", len);
        }

        private void ExecCmd(string command, object value = null)
        {
            if (webEditor.Document == null) return;
            webEditor.Document.ExecCommand(command, false, value);
            webEditor.Focus();
        }

        private void PasteHtmlAtCursor(string html)
        {
            if (webEditor.Document == null) return;
            webEditor.Document.InvokeScript("pasteHtml", new object[] { html });
            webEditor.Focus();
            UpdateEditorStatus();
        }

        private void TsRteBold_Click(object sender, EventArgs e)         { ExecCmd("Bold"); }
        private void TsRteItalic_Click(object sender, EventArgs e)       { ExecCmd("Italic"); }
        private void TsRteUnder_Click(object sender, EventArgs e)        { ExecCmd("Underline"); }
        private void TsRteH1_Click(object sender, EventArgs e)           { ExecCmd("FormatBlock", "<h1>"); }
        private void TsRteH2_Click(object sender, EventArgs e)           { ExecCmd("FormatBlock", "<h2>"); }
        private void TsRteH3_Click(object sender, EventArgs e)           { ExecCmd("FormatBlock", "<h3>"); }
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

        private void TsRteUndo_Click(object sender, EventArgs e) { ExecCmd("Undo"); }
        private void TsRteRedo_Click(object sender, EventArgs e) { ExecCmd("Redo"); }

        private void TsRteFontFamily_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (tsRteFontFamily.SelectedItem == null) return;
            ExecCmd("FontName", tsRteFontFamily.SelectedItem.ToString());
        }

        private void TsRteFontSize_SelectedIndexChanged(object sender, EventArgs e)
        {
            int idx = tsRteFontSize.SelectedIndex;
            if (idx < 0) return;
            // HTML font sizes 1–7 map to: 8pt 10pt 12pt 14pt 18pt 24pt 36pt
            ExecCmd("FontSize", (idx + 1).ToString());
        }

        private static string GetEditorHtml()
        {
            return
                "<!DOCTYPE html><html><head><meta charset='utf-8'>" +
                "<style>" +
                "body{font-family:'Segoe UI',Tahoma,sans-serif;font-size:14px;margin:8px;" +
                "line-height:1.6;color:#333;}" +
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
                "function pasteHtml(h){" +
                "try{if(document.selection){var r=document.selection.createRange();r.pasteHTML(h);}}" +
                "catch(e){}}" +
                "</script>" +
                "</head><body contenteditable='true' style='outline:none;'></body></html>";
        }

        // ── Tab Mapping Toolbars ─────────────────────────────────────────────
        // CreateAttachToolbar() removed - buttons now integrated into DataGridView
        // CreateRelatedToolbar() removed - search integrated into pnlRelatedSearch
        // CreateWorkflowToolbar() removed - search integrated into pnlWorkflowSearch

        [Obsolete("Toolbar removed - tabPrograms now uses integrated search panel like tabRelated")]
        private Panel CreateProgramsToolbar()
        {
            // This method is no longer used - tabPrograms now has integrated search
            // The search panel (pnlProgramsSearch) is created in the Designer
            return null;
        }



        // ── Tab Mapping Event Handlers ───────────────────────────────────────

        private void BtnUploadAttachment_Click(object sender, EventArgs e)
        {
            // OLD toolbar button - redirect to new implementation
            BtnUploadFileNew_Click(sender, e);
        }

        private void BtnLoadAttachFromKB_Click(object sender, EventArgs e)
        {
            // OLD toolbar button - deprecated
            // This functionality is now handled by the new two-section design
            ShowInfo("ฟีเจอร์นี้ถูกแทนที่ด้วยการออกแบบใหม่\n\nใช้ปุ่ม 'อัพโหลดไฟล์' หรือ 'เลือกจาก DMS' แทน");
        }

        private void BtnRemoveAttachment_Click(object sender, EventArgs e)
        {
            // OLD toolbar button - deprecated
            // This functionality is now handled by button columns in DataGridView
            ShowInfo("ฟีเจอร์นี้ถูกแทนที่ด้วยการออกแบบใหม่\n\nใช้ปุ่ม 'ลบไฟล์' ใน DataGridView แทน");
        }

        private void BtnAddRelated_Click(object sender, EventArgs e)
        {
            // OLD toolbar button - deprecated
            // This functionality is now handled by the new two-section design with search
            ShowInfo("ฟีเจอร์นี้ถูกแทนที่ด้วยการออกแบบใหม่\n\nใช้ช่องค้นหาและฟิลเตอร์ในแท็บ 'บทความที่เกี่ยวข้อง' แทน");
        }

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

        private void BtnRemoveRelated_Click(object sender, EventArgs e)
        {
            // This is the OLD toolbar button handler - redirects to new implementation
            BtnRemoveRelatedNew_Click(sender, e);
        }

        private void BtnAddWorkflow_Click(object sender, EventArgs e)
        {
            // TODO: Open workflow selection dialog
            ShowInfo("ฟีเจอร์นี้จะเปิด Dialog เลือก Workflow\n\nสามารถเชื่อมโยงบทความกับเส้นงาน (Workflow) ที่เกี่ยวข้อง");
        }

        [Obsolete("Replaced by search-based approach in tabWorkflow")]
        private void BtnLoadWorkflowFromKB_Click(object sender, EventArgs e)
        {
            // OLD toolbar button - deprecated (references non-existent lstSearchWorkflow control)
            // This functionality is now handled by the new two-section design
            ShowInfo("ฟีเจอร์นี้ถูกแทนที่ด้วยการออกแบบใหม่\n\nใช้การดับเบิลคลิกจากรายการ Workflow ที่มีอยู่แทน");
        }

        [Obsolete("Replaced by delete button in DataGridView")]
        private void BtnRemoveWorkflow_Click(object sender, EventArgs e)
        {
            // OLD toolbar button - redirect to new implementation
            BtnRemoveWorkflowNew_Click(sender, e);
        }

        private void BtnAddProgram_Click(object sender, EventArgs e)
        {
            // TODO: Open program selection dialog
            ShowInfo("ฟีเจอร์นี้จะเปิด Dialog เลือกโปรแกรม\n\nสามารถเชื่อมโยงบทความกับโปรแกรมที่เกี่ยวข้อง");
        }

        private void BtnLoadProgramsFromKB_Click(object sender, EventArgs e)
        {
            // OLD toolbar button - deprecated (references non-existent lstSearchPrograms control)
            // This functionality is now handled by the new two-section design
            ShowInfo("ฟีเจอร์นี้ถูกแทนที่ด้วยการออกแบบใหม่\n\nใช้การดับเบิลคลิกจากรายการโปรแกรมที่มีอยู่แทน");
        }

        [Obsolete("Toolbar removed - functionality integrated into DataGridView")]
        private void BtnRemoveProgram_Click(object sender, EventArgs e)
        {
            // OLD toolbar button - no longer used
            // Functionality moved to delete button in DataGridView
        }

        private void BtnAddDoc_Click(object sender, EventArgs e)
        {
            // TODO: Open document selection dialog (from DMS)
            ShowInfo("ฟีเจอร์นี้จะเปิด Dialog เลือกเอกสารจากระบบ DMS\n\nสามารถเชื่อมโยงบทความกับเอกสารในระบบจัดการเอกสาร");
        }

        private void BtnLoadDocsFromKB_Click(object sender, EventArgs e)
        {
            // OLD toolbar button - deprecated (references non-existent lstSearchDocs control)
            // This functionality is now handled by the new two-section design
            ShowInfo("ฟีเจอร์นี้ถูกแทนที่ด้วยการออกแบบใหม่\n\nใช้ปุ่ม 'อัพโหลดเอกสาร' หรือ 'เลือกจาก DMS' แทน");
        }

        private void BtnRemoveDoc_Click(object sender, EventArgs e)
        {
            // OLD toolbar button - redirect to new implementation
            BtnRemoveDocNew_Click(sender, e);
        }

        // ── Search Event Handlers ────────────────────────────────────────────

        private System.Threading.Timer _searchRelatedTimer;
        private System.Threading.Timer _searchAttachTimer;
        
        // Search textbox references (deprecated - using Designer controls now)
        private TextBox _txtSearchAttach;
        private System.Threading.Timer _searchProgramsTimer;
        private System.Threading.Timer _searchDocsTimer;

        private void TxtSearchRelated_TextChanged(object sender, EventArgs e)
        {
            // Debounce search to avoid excessive queries (300ms delay)
            _searchRelatedTimer?.Dispose();
            _searchRelatedTimer = new System.Threading.Timer(
                _ => this.Invoke(new Action(PerformRelatedSearch)),
                null, 300, System.Threading.Timeout.Infinite);
        }

        private void TxtSearchAttach_TextChanged(object sender, EventArgs e)
        {
            // Debounce search (300ms delay)
            _searchAttachTimer?.Dispose();
            _searchAttachTimer = new System.Threading.Timer(
                _ => this.Invoke(new Action(PerformAttachSearch)),
                null, 300, System.Threading.Timeout.Infinite);
        }

        [Obsolete("Replaced by BtnSearchWorkflow_Click with integrated search panel")]
        private void TxtSearchWorkflow_TextChanged(object sender, EventArgs e)
        {
            // No longer used - search is now triggered by button click or Enter key
        }

        private void TxtSearchPrograms_TextChanged(object sender, EventArgs e)
        {
            // Debounce search (300ms delay)
            _searchProgramsTimer?.Dispose();
            _searchProgramsTimer = new System.Threading.Timer(
                _ => this.Invoke(new Action(PerformProgramsSearch)),
                null, 300, System.Threading.Timeout.Infinite);
        }

        private void TxtSearchDocs_TextChanged(object sender, EventArgs e)
        {
            // Debounce search (300ms delay)
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

        private void PerformAttachSearch()
        {
            // OLD method - deprecated (references non-existent lstSearchAttach control)
            // This functionality is now handled by the new two-section design
            // No search functionality needed for file attachments
        }

        [Obsolete("Replaced by BtnSearchWorkflow_Click with integrated search panel")]
        private void PerformWorkflowSearch()
        {
            // OLD method - deprecated (references non-existent lstSearchWorkflow control)
            // This functionality is now handled by the new two-section design
            // Workflows are selected from available list, not searched
        }

        private void PerformProgramsSearch()
        {
            // OLD method - deprecated (references non-existent lstSearchPrograms control)
            // This functionality is now handled by the new two-section design
            // Programs are selected from available list, not searched
        }

        private void PerformDocsSearch()
        {
            // OLD method - deprecated (references non-existent lstSearchDocs control)
            // This functionality is now handled by the new two-section design
            // No search functionality needed for document attachments
        }

        // ── Double-Click Event Handlers ──────────────────────────────────────

        // OLD handler removed - now using LstRelatedSearch_DoubleClick in new implementation

        // OLD handler removed - now using new implementation

        // OLD handler removed - now using new implementation

        // OLD handler removed - now using new implementation

        private void LstSearchDocs_DoubleClick(object sender, EventArgs e)
        {
            // OLD handler - deprecated (references non-existent lstSearchDocs and lstDocs controls)
            // This functionality is now handled by the new two-section design
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

        [Obsolete("Replaced by DgvProgramsLinked_CellContentClick with delete button column")]
        private void LstProgramsLinked_DoubleClick(object sender, EventArgs e)
        {
            // No longer used - DataGridView doesn't need double-click on linked items
            // Delete functionality moved to button column in DataGridView
        }

        [Obsolete("Replaced by delete button column in DataGridView")]
        private void BtnRemoveProgramNew_Click(object sender, EventArgs e)
        {
            // Replaced by delete button in DataGridView (DgvProgramsLinked_CellContentClick)
            ShowInfo("ฟีเจอร์นี้ถูกแทนที่ด้วยการออกแบบใหม่\n\nใช้ปุ่ม 'ลบ' ใน DataGridView แทน");
        }

        [Obsolete("Replaced by integrated view in DataGridView")]
        private void BtnViewProgramDetails_Click(object sender, EventArgs e)
        {
            // This functionality can be integrated into the DataGridView if needed
            ShowInfo("ฟีเจอร์นี้ถูกแทนที่ด้วยการออกแบบใหม่\n\nดับเบิลคลิกที่แถวเพื่อดูรายละเอียด");
        }

        [Obsolete("Replaced by DgvProgramsSearch_CellDoubleClick")]
        private void LstProgramsAvailable_DoubleClick(object sender, EventArgs e)
        {
            // Replaced by DgvProgramsSearch_CellDoubleClick
        }

        // ══════════════════════════════════════════════════════════════════════════
        // TAB WORKFLOW - NEW IMPLEMENTATION (Two-Section Design - Mockup)
        // ══════════════════════════════════════════════════════════════════════════

        // ══════════════════════════════════════════════════════════════════════════
        // TAB WORKFLOW - NEW IMPLEMENTATION (Two-Section Design - Mockup with DataGridView)
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
        // TAB WORKFLOW - DEPRECATED METHODS (Old ListView-based implementation)
        // ══════════════════════════════════════════════════════════════════════════

        [Obsolete("Replaced by DataGridView-based implementation. Use InitializeWorkflowDataGridViews() instead.")]
        private Panel CreateWorkflowToolbar()
        {
            // This method is no longer used - workflow actions are now integrated into the DataGridView
            return new Panel { Visible = false };
        }

        [Obsolete("Replaced by DgvWorkflowLinked_CellContentClick. Use delete button in DataGridView instead.")]
        private void LstWorkflowLinked_DoubleClick(object sender, EventArgs e)
        {
            throw new NotSupportedException("ListView has been replaced with DataGridView. Use DgvWorkflowLinked_CellContentClick() instead.");
        }

        [Obsolete("Replaced by DgvWorkflowLinked_CellContentClick. Use delete button in DataGridView instead.")]
        private void BtnRemoveWorkflowNew_Click(object sender, EventArgs e)
        {
            throw new NotSupportedException("ListView has been replaced with DataGridView. Use delete button column in dgvWorkflowLinked instead.");
        }

        [Obsolete("Workflow diagram viewing should be implemented separately. Not part of the core linking functionality.")]
        private void BtnViewWorkflowDiagram_Click(object sender, EventArgs e)
        {
            throw new NotSupportedException("This functionality should be implemented separately from the workflow linking interface.");
        }

        [Obsolete("Workflow starting should be implemented separately. Not part of the core linking functionality.")]
        private void BtnStartWorkflow_Click(object sender, EventArgs e)
        {
            throw new NotSupportedException("This functionality should be implemented separately from the workflow linking interface.");
        }

        [Obsolete("Replaced by DgvWorkflowSearch_CellDoubleClick. Use search-based approach instead.")]
        private void LstWorkflowAvailable_DoubleClick(object sender, EventArgs e)
        {
            throw new NotSupportedException("ListView has been replaced with DataGridView. Use DgvWorkflowSearch_CellDoubleClick() instead.");
        }

        [Obsolete("Replaced by search-based approach. Use BtnSearchWorkflow_Click() instead.")]
        private void LoadAvailableWorkflows()
        {
            // No longer needed - workflows are loaded via search
        }

        // ══════════════════════════════════════════════════════════════════════════
        // TAB ATTACH - NEW IMPLEMENTATION (Two-Section Design - Mockup with DataGridView)
        // ══════════════════════════════════════════════════════════════════════════

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
            var mockupFiles = new[]
            {
                new { Name = "Claims_Form_2569.pdf", Size = 245 * 1024L, Date = DateTime.Now.AddDays(-5) },
                new { Name = "UW_Criteria_Table.xlsx", Size = 89 * 1024L, Date = DateTime.Now.AddDays(-3) }
            };
            
            int index = 1;
            foreach (var file in mockupFiles)
            {
                int rowIndex = dgvAttachLinked.Rows.Add(
                    index.ToString(),
                    file.Name,
                    FormatFileSize(file.Size),
                    file.Date.ToString("dd/MM/yyyy HH:mm")
                );
                
                dgvAttachLinked.Rows[rowIndex].Tag = index;
                dgvAttachLinked.Rows[rowIndex].DefaultCellStyle.BackColor = 
                    index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                index++;
            }
            
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
                ShowInfo(string.Format("เปิดไฟล์: {0}\n\n(Mockup - ยังไม่ได้เชื่อมต่อกับ DMS)", fileName));
            }
            // Check if Delete button clicked
            else if (e.ColumnIndex == dgvAttachLinked.Columns["colDelete"].Index)
            {
                string fileName = dgvAttachLinked.Rows[e.RowIndex].Cells["colFilename"].Value?.ToString();
                
                var confirm = MessageBox.Show(
                    string.Format("ต้องการลบไฟล์ '{0}' ใช่หรือไม่?", fileName),
                    "ยืนยันการลบ", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                
                if (confirm == DialogResult.Yes)
                {
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

        private void BtnUploadFileNew_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "เลือกไฟล์ที่ต้องการแนบ";
                ofd.Filter = "All Files (*.*)|*.*|" +
                             "PDF Files (*.pdf)|*.pdf|" +
                             "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls|" +
                             "Word Files (*.docx;*.doc)|*.docx;*.doc|" +
                             "Image Files (*.jpg;*.png;*.gif)|*.jpg;*.png;*.gif";
                ofd.FilterIndex = 1;
                ofd.Multiselect = true;
                
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (var filePath in ofd.FileNames)
                    {
                        var fi = new System.IO.FileInfo(filePath);
                        int newIndex = dgvAttachLinked.Rows.Count + 1;
                        
                        int rowIndex = dgvAttachLinked.Rows.Add(
                            newIndex.ToString(),
                            fi.Name,
                            FormatFileSize(fi.Length),
                            DateTime.Now.ToString("dd/MM/yyyy HH:mm")
                        );
                        
                        dgvAttachLinked.Rows[rowIndex].Tag = newIndex;
                        dgvAttachLinked.Rows[rowIndex].DefaultCellStyle.BackColor = 
                            newIndex % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                    }
                    
                    UpdateAttachLinkedCount();
                    ShowInfo(string.Format("เพิ่มไฟล์แนบ {0} ไฟล์เรียบร้อยแล้ว\n\n(Mockup - ไฟล์จะถูกอัพโหลดจริงเมื่อบันทึกบทความ)", ofd.FileNames.Length));
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
            txtSearchRelated.Text = "🔍 ค้นหาชื่อบทความ / รหัส...";
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

        [Obsolete("Replaced by DgvDocsLinked_CellContentClick with button columns")]
        private void LstDocsLinked_DoubleClick(object sender, EventArgs e)
        {
            // No longer used - DataGridView uses button columns for view/delete actions
            // Functionality moved to DgvDocsLinked_CellContentClick
        }

        [Obsolete("Replaced by delete button column in DataGridView")]
        private void BtnRemoveDocNew_Click(object sender, EventArgs e)
        {
            // Replaced by delete button in DataGridView (DgvDocsLinked_CellContentClick)
            ShowInfo("ฟีเจอร์นี้ถูกแทนที่ด้วยการออกแบบใหม่\n\nใช้ปุ่ม 'ลบ' ใน DataGridView แทน");
        }

        [Obsolete("Replaced by view button column in DataGridView")]
        private void BtnViewDocNew_Click(object sender, EventArgs e)
        {
            // Replaced by view button in DataGridView (DgvDocsLinked_CellContentClick)
            ShowInfo("ฟีเจอร์นี้ถูกแทนที่ด้วยการออกแบบใหม่\n\nใช้ปุ่ม 'ดู' ใน DataGridView แทน");
        }

        [Obsolete("Replaced by integrated upload functionality in tabDocs")]
        private void BtnUploadDocNew_Click(object sender, EventArgs e)
        {
            // This method is deprecated - tabDocs now uses the same pattern as tabAttach
            // with integrated upload buttons in the tab itself
            ShowInfo("ฟีเจอร์นี้ถูกแทนที่ด้วยการออกแบบใหม่\n\nใช้ปุ่ม 'อัพโหลดเอกสาร' หรือ 'เลือกจาก DMS' ในแท็บเอกสารแทน");
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
            if (e.ColumnIndex == dgvRelatedLinked.Columns["colDelete"].Index)
            {
                int articleId = (int)dgvRelatedLinked.Rows[e.RowIndex].Tag;
                string title = dgvRelatedLinked.Rows[e.RowIndex].Cells["colTitle"].Value?.ToString();
                
                var confirm = MessageBox.Show(
                    string.Format("ต้องการลบความเชื่อมโยงกับบทความ '{0}' ใช่หรือไม่?", title),
                    "ยืนยันการลบ", 
                    MessageBoxButtons.YesNo, 
                    MessageBoxIcon.Question);
                
                if (confirm == DialogResult.Yes)
                {
                    try
                    {
                        _presenter.RemoveRelatedArticleLink(articleId);
                        dgvRelatedLinked.Rows.RemoveAt(e.RowIndex);
                        
                        // Renumber
                        for (int i = 0; i < dgvRelatedLinked.Rows.Count; i++)
                        {
                            dgvRelatedLinked.Rows[i].Cells["colNum"].Value = (i + 1).ToString();
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
                
                byte? status = 3; // Published
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
            
            try
            {
                // Add link
                _presenter.AddRelatedArticleLink(articleId);
                
                // Add to linked grid
                int newIndex = dgvRelatedLinked.Rows.Count + 1;
                int rowIndex = dgvRelatedLinked.Rows.Add(
                    newIndex.ToString(),
                    dgvRelatedSearch.Rows[e.RowIndex].Cells["colRelatedSearchCode"].Value,
                    dgvRelatedSearch.Rows[e.RowIndex].Cells["colRelatedSearchTitle"].Value,
                    dgvRelatedSearch.Rows[e.RowIndex].Cells["colRelatedSearchType"].Value
                );
                
                dgvRelatedLinked.Rows[rowIndex].Tag = articleId;
                dgvRelatedLinked.Rows[rowIndex].DefaultCellStyle.BackColor = 
                    newIndex % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                
                // Remove from search results
                dgvRelatedSearch.Rows.RemoveAt(e.RowIndex);
                
                UpdateRelatedLinkedCount();
                lblRelatedSearch.Text = string.Format("ผลการค้นหา ({0} รายการ) — ดับเบิลคลิกเพื่อเพิ่ม", dgvRelatedSearch.Rows.Count);
                
                ShowInfo("เพิ่มความเชื่อมโยงเรียบร้อยแล้ว");
            }
            catch (Exception ex)
            {
                ShowError("เพิ่มความเชื่อมโยงไม่สำเร็จ: " + ex.Message);
            }
        }

        [Obsolete("Replaced by DgvRelatedLinked_CellContentClick with delete button column")]
        private void LstRelatedLinked_DoubleClick(object sender, EventArgs e)
        {
            // No longer used - DataGridView doesn't need double-click on linked items
            // Delete functionality moved to button column in DataGridView
        }

        [Obsolete("Replaced by DgvRelatedSearch_CellDoubleClick")]
        private void LstRelatedSearch_DoubleClick(object sender, EventArgs e)
        {
            // Replaced by DgvRelatedSearch_CellDoubleClick
        }

        [Obsolete("Replaced by delete button column in DataGridView")]
        private void BtnRemoveRelatedNew_Click(object sender, EventArgs e)
        {
            // Replaced by delete button in DataGridView (DgvRelatedLinked_CellContentClick)
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