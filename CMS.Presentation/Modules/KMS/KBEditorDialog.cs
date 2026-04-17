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
            
            // Add toolbars to tabs
            tabRelated.Controls.Add(CreateRelatedToolbar());
            tabWorkflow.Controls.Add(CreateWorkflowToolbar());
            tabPrograms.Controls.Add(CreateProgramsToolbar());
            tabDocs.Controls.Add(CreateDocsToolbar());
            
            // Initialize tabPrograms - NEW IMPLEMENTATION (Mockup)
            LoadLinkedPrograms();
            
            // Initialize tabWorkflow - NEW IMPLEMENTATION (Mockup)
            LoadLinkedWorkflows();
            
            // Initialize tabAttach - NEW IMPLEMENTATION (Mockup with DataGridView)
            InitializeAttachDataGridView();
            LoadAttachedFiles();
            
            // Initialize tabDocs - NEW IMPLEMENTATION (Mockup)
            LoadLinkedDocuments();
            
            // Initialize tabRelated - NEW IMPLEMENTATION
            InitializeRelatedCategoryDropdown();
            if (_presenter.EditingArticle != null)
            {
                LoadRelatedArticles();
            }
            
            _presenter.Initialize();
        }

        private void ChkNoExpiry_CheckedChanged(object sender, EventArgs e)
        {
            dtpExpiry.Enabled = !chkNoExpiry.Checked;
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
            get { return chkOfficialPolicy.Checked; }
            set { chkOfficialPolicy.Checked = value; }
        }

        public bool NoExpiry
        {
            get { return chkNoExpiry.Checked; }
            set
            {
                chkNoExpiry.Checked = value;
                dtpExpiry.Enabled   = !value;
            }
        }

        public DateTime? ExpiryDate
        {
            get { return chkNoExpiry.Checked ? (DateTime?)null : dtpExpiry.Value; }
            set { if (value.HasValue) dtpExpiry.Value = value.Value; }
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

        // ── ListView Access for Article Links ────────────────────────────────

        public System.Windows.Forms.ListView GetRelatedListView()
        {
            return lstRelatedLinked;
        }

        public System.Windows.Forms.ListView GetWorkflowListView()
        {
            return lstWorkflowLinked;
        }

        public System.Windows.Forms.ListView GetProgramsListView()
        {
            return lstProgramsLinked;
        }

        public System.Windows.Forms.ListView GetDocsListView()
        {
            return lstDocsLinked;
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

        private void AddTagChip(string tag)
        {
            if (string.IsNullOrWhiteSpace(tag)) return;
            
            // Remove comma if present
            tag = tag.TrimEnd(',');
            if (string.IsNullOrWhiteSpace(tag)) return;
            
            // Create chip panel
            var chip = new Panel
            {
                AutoSize = true,
                Height = 22,
                Margin = new Padding(2),
                Padding = new Padding(6, 2, 2, 2),
                BackColor = Color.FromArgb(214, 234, 248),
                BorderStyle = BorderStyle.FixedSingle
            };
            
            var lblTag = new Label
            {
                Text = tag,
                AutoSize = true,
                Font = new Font("Segoe UI", 8.5f),
                ForeColor = Color.FromArgb(26, 82, 118),
                Padding = new Padding(0, 2, 4, 0)
            };
            
            var btnRemove = new Button
            {
                Text = "×",
                Size = new Size(16, 16),
                FlatStyle = FlatStyle.Flat,
                Font = new Font("Segoe UI", 8f, FontStyle.Bold),
                ForeColor = Color.FromArgb(26, 82, 118),
                Cursor = Cursors.Hand,
                Margin = new Padding(0),
                Padding = new Padding(0)
            };
            btnRemove.FlatAppearance.BorderSize = 0;
            btnRemove.Click += (s, ev) => flpTags.Controls.Remove(chip);
            
            chip.Controls.Add(lblTag);
            chip.Controls.Add(btnRemove);
            lblTag.Location = new Point(6, 2);
            btnRemove.Location = new Point(lblTag.Right + 2, 2);
            chip.Width = lblTag.Width + btnRemove.Width + 12;
            
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

        private Panel CreateRelatedToolbar()
        {
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(245, 247, 250)
            };
            
            _txtSearchRelated = new TextBox
            {
                Name = "txtSearchRelated",
                Location = new Point(8, 6),
                Size = new Size(250, 24),
                Font = new Font("Segoe UI", 9f)
            };
            _txtSearchRelated.TextChanged += TxtSearchRelated_TextChanged;
            
            // Add placeholder text using a label overlay (WinForms doesn't have native placeholder)
            var lblPlaceholder = new Label
            {
                Name = "lblSearchRelatedPlaceholder",
                Text = "🔍 ค้นหาบทความ...",
                Location = new Point(10, 9),
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.IBeam
            };
            lblPlaceholder.Click += (s, e) => _txtSearchRelated.Focus();
            
            // Hide placeholder when textbox has focus or text
            _txtSearchRelated.Enter += (s, e) => lblPlaceholder.Visible = string.IsNullOrEmpty(_txtSearchRelated.Text);
            _txtSearchRelated.Leave += (s, e) => lblPlaceholder.Visible = string.IsNullOrEmpty(_txtSearchRelated.Text);
            _txtSearchRelated.TextChanged += (s, e) => lblPlaceholder.Visible = string.IsNullOrEmpty(_txtSearchRelated.Text);
            
            var btnRemove = new Button
            {
                Location = new Point(266, 6),
                Size = new Size(80, 24),
                Text = "🗑️ ลบ",
                Font = new Font("Segoe UI", 9f),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(231, 76, 60),
                Cursor = Cursors.Hand
            };
            btnRemove.FlatAppearance.BorderColor = Color.FromArgb(231, 76, 60);
            btnRemove.Click += BtnRemoveRelated_Click;
            
            var btnLoadFromKB = new Button
            {
                Location = new Point(354, 6),
                Size = new Size(150, 24),
                Text = "📚 โหลดจาก KB",
                Font = new Font("Segoe UI", 9f),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnLoadFromKB.FlatAppearance.BorderColor = Color.FromArgb(30, 132, 73);
            btnLoadFromKB.Click += BtnLoadRelatedFromKB_Click;
            
            toolbar.Controls.Add(_txtSearchRelated);
            toolbar.Controls.Add(lblPlaceholder);
            toolbar.Controls.Add(btnRemove);
            toolbar.Controls.Add(btnLoadFromKB);
            
            lblPlaceholder.BringToFront();
            
            return toolbar;
        }

        private Panel CreateWorkflowToolbar()
        {
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(245, 247, 250)
            };
            
            _txtSearchWorkflow = new TextBox
            {
                Name = "txtSearchWorkflow",
                Location = new Point(8, 6),
                Size = new Size(250, 24),
                Font = new Font("Segoe UI", 9f)
            };
            _txtSearchWorkflow.TextChanged += TxtSearchWorkflow_TextChanged;
            
            // Add placeholder text using a label overlay
            var lblPlaceholder = new Label
            {
                Name = "lblSearchWorkflowPlaceholder",
                Text = "🔍 ค้นหา Workflow...",
                Location = new Point(10, 9),
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.IBeam
            };
            lblPlaceholder.Click += (s, e) => _txtSearchWorkflow.Focus();
            
            // Hide placeholder when textbox has focus or text
            _txtSearchWorkflow.Enter += (s, e) => lblPlaceholder.Visible = string.IsNullOrEmpty(_txtSearchWorkflow.Text);
            _txtSearchWorkflow.Leave += (s, e) => lblPlaceholder.Visible = string.IsNullOrEmpty(_txtSearchWorkflow.Text);
            _txtSearchWorkflow.TextChanged += (s, e) => lblPlaceholder.Visible = string.IsNullOrEmpty(_txtSearchWorkflow.Text);
            
            var btnRemove = new Button
            {
                Location = new Point(266, 6),
                Size = new Size(80, 24),
                Text = "🗑️ ลบ",
                Font = new Font("Segoe UI", 9f),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(231, 76, 60),
                Cursor = Cursors.Hand
            };
            btnRemove.FlatAppearance.BorderColor = Color.FromArgb(231, 76, 60);
            btnRemove.Click += BtnRemoveWorkflow_Click;
            
            var btnLoadFromKB = new Button
            {
                Location = new Point(354, 6),
                Size = new Size(150, 24),
                Text = "📚 โหลดจาก KB",
                Font = new Font("Segoe UI", 9f),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnLoadFromKB.FlatAppearance.BorderColor = Color.FromArgb(30, 132, 73);
            btnLoadFromKB.Click += BtnLoadWorkflowFromKB_Click;
            
            toolbar.Controls.Add(_txtSearchWorkflow);
            toolbar.Controls.Add(lblPlaceholder);
            toolbar.Controls.Add(btnRemove);
            toolbar.Controls.Add(btnLoadFromKB);
            
            lblPlaceholder.BringToFront();
            
            return toolbar;
        }

        private Panel CreateProgramsToolbar()
        {
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(245, 247, 250)
            };
            
            _txtSearchPrograms = new TextBox
            {
                Name = "txtSearchPrograms",
                Location = new Point(8, 6),
                Size = new Size(250, 24),
                Font = new Font("Segoe UI", 9f)
            };
            _txtSearchPrograms.TextChanged += TxtSearchPrograms_TextChanged;
            
            // Add placeholder text using a label overlay
            var lblPlaceholder = new Label
            {
                Name = "lblSearchProgramsPlaceholder",
                Text = "🔍 ค้นหาโปรแกรม...",
                Location = new Point(10, 9),
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.IBeam
            };
            lblPlaceholder.Click += (s, e) => _txtSearchPrograms.Focus();
            
            // Hide placeholder when textbox has focus or text
            _txtSearchPrograms.Enter += (s, e) => lblPlaceholder.Visible = string.IsNullOrEmpty(_txtSearchPrograms.Text);
            _txtSearchPrograms.Leave += (s, e) => lblPlaceholder.Visible = string.IsNullOrEmpty(_txtSearchPrograms.Text);
            _txtSearchPrograms.TextChanged += (s, e) => lblPlaceholder.Visible = string.IsNullOrEmpty(_txtSearchPrograms.Text);
            
            var btnRemove = new Button
            {
                Location = new Point(266, 6),
                Size = new Size(80, 24),
                Text = "🗑️ ลบ",
                Font = new Font("Segoe UI", 9f),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(231, 76, 60),
                Cursor = Cursors.Hand
            };
            btnRemove.FlatAppearance.BorderColor = Color.FromArgb(231, 76, 60);
            btnRemove.Click += BtnRemoveProgram_Click;
            
            var btnLoadFromKB = new Button
            {
                Location = new Point(354, 6),
                Size = new Size(150, 24),
                Text = "📚 โหลดจาก KB",
                Font = new Font("Segoe UI", 9f),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnLoadFromKB.FlatAppearance.BorderColor = Color.FromArgb(30, 132, 73);
            btnLoadFromKB.Click += BtnLoadProgramsFromKB_Click;
            
            toolbar.Controls.Add(_txtSearchPrograms);
            toolbar.Controls.Add(lblPlaceholder);
            toolbar.Controls.Add(btnRemove);
            toolbar.Controls.Add(btnLoadFromKB);
            
            lblPlaceholder.BringToFront();
            
            return toolbar;
        }

        private Panel CreateDocsToolbar()
        {
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(245, 247, 250)
            };
            
            _txtSearchDocs = new TextBox
            {
                Name = "txtSearchDocs",
                Location = new Point(8, 6),
                Size = new Size(250, 24),
                Font = new Font("Segoe UI", 9f)
            };
            _txtSearchDocs.TextChanged += TxtSearchDocs_TextChanged;
            
            // Add placeholder text using a label overlay
            var lblPlaceholder = new Label
            {
                Name = "lblSearchDocsPlaceholder",
                Text = "🔍 ค้นหาเอกสาร...",
                Location = new Point(10, 9),
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.IBeam
            };
            lblPlaceholder.Click += (s, e) => _txtSearchDocs.Focus();
            
            // Hide placeholder when textbox has focus or text
            _txtSearchDocs.Enter += (s, e) => lblPlaceholder.Visible = string.IsNullOrEmpty(_txtSearchDocs.Text);
            _txtSearchDocs.Leave += (s, e) => lblPlaceholder.Visible = string.IsNullOrEmpty(_txtSearchDocs.Text);
            _txtSearchDocs.TextChanged += (s, e) => lblPlaceholder.Visible = string.IsNullOrEmpty(_txtSearchDocs.Text);
            
            var btnRemove = new Button
            {
                Location = new Point(266, 6),
                Size = new Size(80, 24),
                Text = "🗑️ ลบ",
                Font = new Font("Segoe UI", 9f),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(231, 76, 60),
                Cursor = Cursors.Hand
            };
            btnRemove.FlatAppearance.BorderColor = Color.FromArgb(231, 76, 60);
            btnRemove.Click += BtnRemoveDoc_Click;
            
            var btnLoadFromKB = new Button
            {
                Location = new Point(354, 6),
                Size = new Size(150, 24),
                Text = "📚 โหลดจาก KB",
                Font = new Font("Segoe UI", 9f),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(39, 174, 96),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnLoadFromKB.FlatAppearance.BorderColor = Color.FromArgb(30, 132, 73);
            btnLoadFromKB.Click += BtnLoadDocsFromKB_Click;
            
            toolbar.Controls.Add(_txtSearchDocs);
            toolbar.Controls.Add(lblPlaceholder);
            toolbar.Controls.Add(btnRemove);
            toolbar.Controls.Add(btnLoadFromKB);
            
            lblPlaceholder.BringToFront();
            
            return toolbar;
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
                
                lstRelatedSearch.Items.Clear();
                
                // Exclude current article being edited
                if (_presenter.EditingArticle != null)
                    articles = articles.Where(a => a.ArticleID != _presenter.EditingArticle.ArticleID);
                
                // Exclude articles already in lstRelatedLinked (NEW control)
                var existingIds = new HashSet<int>(
                    lstRelatedLinked.Items.Cast<ListViewItem>()
                        .Select(item => (int)item.Tag)
                );
                articles = articles.Where(a => !existingIds.Contains(a.ArticleID));
                
                // Populate search list with KB data
                foreach (var article in articles.Take(50))
                {
                    var item = new ListViewItem(article.ArticleCode ?? "-");
                    item.SubItems.Add(article.Title ?? "-");
                    item.SubItems.Add(article.ArticleTypeName ?? "-");
                    item.Tag = article.ArticleID;
                    item.BackColor = Color.FromArgb(248, 250, 253);
                    lstRelatedSearch.Items.Add(item);
                }
                
                ShowInfo(string.Format("โหลดข้อมูลจาก KB เรียบร้อย ({0} รายการ)", lstRelatedSearch.Items.Count));
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

        private void BtnLoadWorkflowFromKB_Click(object sender, EventArgs e)
        {
            // OLD toolbar button - deprecated (references non-existent lstSearchWorkflow control)
            // This functionality is now handled by the new two-section design
            ShowInfo("ฟีเจอร์นี้ถูกแทนที่ด้วยการออกแบบใหม่\n\nใช้การดับเบิลคลิกจากรายการ Workflow ที่มีอยู่แทน");
        }

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

        private void BtnRemoveProgram_Click(object sender, EventArgs e)
        {
            // OLD toolbar button - redirect to new implementation
            BtnRemoveProgramNew_Click(sender, e);
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
        private System.Threading.Timer _searchWorkflowTimer;
        
        // Search textbox references
        private TextBox _txtSearchRelated;
        private TextBox _txtSearchAttach;
        private TextBox _txtSearchWorkflow;
        private TextBox _txtSearchPrograms;
        private TextBox _txtSearchDocs;
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

        private void TxtSearchWorkflow_TextChanged(object sender, EventArgs e)
        {
            // Debounce search (300ms delay)
            _searchWorkflowTimer?.Dispose();
            _searchWorkflowTimer = new System.Threading.Timer(
                _ => this.Invoke(new Action(PerformWorkflowSearch)),
                null, 300, System.Threading.Timeout.Infinite);
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
            if (_txtSearchRelated == null) return;
            
            string searchText = _txtSearchRelated.Text.Trim();
            lstRelatedSearch.Items.Clear();
            
            if (string.IsNullOrEmpty(searchText)) return;
            
            try
            {
                // Call service to search published articles
                var results = _presenter.SearchPublishedArticles(searchText);
                
                // Exclude current article being edited
                if (_presenter.EditingArticle != null)
                    results = results.Where(a => a.ArticleID != _presenter.EditingArticle.ArticleID);
                
                // Exclude articles already in lstRelatedLinked (NEW control)
                var existingIds = new HashSet<int>(
                    lstRelatedLinked.Items.Cast<ListViewItem>()
                        .Select(item => (int)item.Tag)
                );
                results = results.Where(a => !existingIds.Contains(a.ArticleID));
                
                // Populate search list (limit to 50 results)
                foreach (var article in results.Take(50))
                {
                    var item = new ListViewItem(article.ArticleCode ?? "-");
                    item.SubItems.Add(article.Title ?? "-");
                    item.SubItems.Add(article.ArticleTypeName ?? "-");
                    item.Tag = article.ArticleID;
                    lstRelatedSearch.Items.Add(item);
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
        // TAB PROGRAMS - NEW IMPLEMENTATION (Two-Section Design - Mockup)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Load linked programs when form initializes (Mockup data)
        /// </summary>
        private void LoadLinkedPrograms()
        {
            lstProgramsLinked.Items.Clear();
            
            // Mockup data - will be replaced with actual PMS integration
            var mockupPrograms = new[]
            {
                new { Code = "PRG-INS-001", Name = "Insurance Management System", Type = "Core System" },
                new { Code = "PRG-CRM-002", Name = "Customer Relationship Management", Type = "Business Application" }
            };
            
            int index = 1;
            foreach (var prog in mockupPrograms)
            {
                var item = new ListViewItem(index.ToString());
                item.SubItems.Add(prog.Code);
                item.SubItems.Add(prog.Name);
                item.SubItems.Add(prog.Type);
                item.Tag = index; // Mockup ID
                item.BackColor = index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                lstProgramsLinked.Items.Add(item);
                index++;
            }
            
            UpdateProgramsLinkedCount();
            LoadAvailablePrograms();
        }

        private void LoadAvailablePrograms()
        {
            lstProgramsAvailable.Items.Clear();
            
            // Mockup available programs
            var mockupAvailable = new[]
            {
                new { Code = "PRG-RPT-003", Name = "Reporting & Analytics Platform", Type = "Business Intelligence" },
                new { Code = "PRG-DOC-004", Name = "Document Management System", Type = "Core System" },
                new { Code = "PRG-WF-005", Name = "Workflow Automation Engine", Type = "Business Application" }
            };
            
            foreach (var prog in mockupAvailable)
            {
                var item = new ListViewItem(prog.Code);
                item.SubItems.Add(prog.Name);
                item.SubItems.Add(prog.Type);
                item.Tag = prog.Code;
                item.BackColor = Color.FromArgb(248, 250, 253);
                lstProgramsAvailable.Items.Add(item);
            }
        }

        private void UpdateProgramsLinkedCount()
        {
            lblProgramsLinked.Text = string.Format("โปรแกรมที่เชื่อมโยง ({0} รายการ)", lstProgramsLinked.Items.Count);
        }

        private void LstProgramsLinked_DoubleClick(object sender, EventArgs e)
        {
            if (lstProgramsLinked.SelectedItems.Count == 0) return;
            
            try
            {
                string programName = lstProgramsLinked.SelectedItems[0].SubItems[2].Text;
                ShowInfo(string.Format("ดูรายละเอียดโปรแกรม: {0}\n\n(Mockup - ยังไม่ได้เชื่อมต่อกับ PMS)", programName));
            }
            catch (Exception ex)
            {
                ShowError("เปิดโปรแกรมไม่สำเร็จ: " + ex.Message);
            }
        }

        private void BtnRemoveProgramNew_Click(object sender, EventArgs e)
        {
            if (lstProgramsLinked.SelectedItems.Count == 0)
            {
                ShowInfo("กรุณาเลือกโปรแกรมที่ต้องการลบ");
                return;
            }
            
            var confirm = MessageBox.Show(
                string.Format("ต้องการลบโปรแกรมที่เลือก {0} รายการ ใช่หรือไม่?", lstProgramsLinked.SelectedItems.Count),
                "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (confirm == DialogResult.Yes)
            {
                while (lstProgramsLinked.SelectedItems.Count > 0)
                    lstProgramsLinked.Items.Remove(lstProgramsLinked.SelectedItems[0]);
                
                // Renumber
                for (int i = 0; i < lstProgramsLinked.Items.Count; i++)
                {
                    lstProgramsLinked.Items[i].Text = (i + 1).ToString();
                    lstProgramsLinked.Items[i].BackColor = (i + 1) % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                }
                
                UpdateProgramsLinkedCount();
                ShowInfo("ลบโปรแกรมเรียบร้อยแล้ว");
            }
        }

        private void BtnViewProgramDetails_Click(object sender, EventArgs e)
        {
            if (lstProgramsLinked.SelectedItems.Count == 0)
            {
                ShowInfo("กรุณาเลือกโปรแกรมที่ต้องการดูรายละเอียด");
                return;
            }
            
            string programName = lstProgramsLinked.SelectedItems[0].SubItems[2].Text;
            string programType = lstProgramsLinked.SelectedItems[0].SubItems[3].Text;
            ShowInfo(string.Format("รายละเอียดโปรแกรม: {0}\n\n" +
                     "ประเภท: {1}\n" +
                     "สถานะ: ใช้งานอยู่\n" +
                     "เจ้าของระบบ: IT Department\n" +
                     "ผู้ดูแล: System Admin\n\n" +
                     "ข้อมูลเพิ่มเติม:\n" +
                     "• เอกสารคู่มือการใช้งาน\n" +
                     "• ข้อมูลการติดต่อ Support\n" +
                     "• ประวัติการอัพเดท\n\n" +
                     "(Mockup - ยังไม่ได้เชื่อมต่อกับ PMS)", programName, programType));
        }

        private void LstProgramsAvailable_DoubleClick(object sender, EventArgs e)
        {
            if (lstProgramsAvailable.SelectedItems.Count == 0) return;
            
            try
            {
                var selectedItem = lstProgramsAvailable.SelectedItems[0];
                
                int newIndex = lstProgramsLinked.Items.Count + 1;
                var newItem = new ListViewItem(newIndex.ToString());
                newItem.SubItems.Add(selectedItem.SubItems[0].Text);
                newItem.SubItems.Add(selectedItem.SubItems[1].Text);
                newItem.SubItems.Add(selectedItem.SubItems[2].Text);
                newItem.Tag = newIndex;
                newItem.BackColor = newIndex % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                
                lstProgramsLinked.Items.Add(newItem);
                lstProgramsAvailable.Items.Remove(selectedItem);
                
                UpdateProgramsLinkedCount();
                ShowInfo("เพิ่มโปรแกรมเรียบร้อยแล้ว");
            }
            catch (Exception ex)
            {
                ShowError("เพิ่มโปรแกรมไม่สำเร็จ: " + ex.Message);
            }
        }

        // ══════════════════════════════════════════════════════════════════════════
        // TAB WORKFLOW - NEW IMPLEMENTATION (Two-Section Design - Mockup)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Load linked workflows when form initializes (Mockup data)
        /// </summary>
        private void LoadLinkedWorkflows()
        {
            lstWorkflowLinked.Items.Clear();
            
            // Mockup data - will be replaced with actual Workflow Engine integration
            var mockupWorkflows = new[]
            {
                new { Code = "WF-CLAIM-001", Name = "Claims Approval Process", Status = "Active" },
                new { Code = "WF-UW-002", Name = "Underwriting Review Workflow", Status = "Active" }
            };
            
            int index = 1;
            foreach (var wf in mockupWorkflows)
            {
                var item = new ListViewItem(index.ToString());
                item.SubItems.Add(wf.Code);
                item.SubItems.Add(wf.Name);
                item.SubItems.Add(wf.Status);
                item.Tag = index; // Mockup ID
                item.BackColor = index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                lstWorkflowLinked.Items.Add(item);
                index++;
            }
            
            UpdateWorkflowLinkedCount();
            LoadAvailableWorkflows();
        }

        private void LoadAvailableWorkflows()
        {
            lstWorkflowAvailable.Items.Clear();
            
            // Mockup available workflows
            var mockupAvailable = new[]
            {
                new { Code = "WF-DOC-003", Name = "Document Review Workflow", Status = "Active" },
                new { Code = "WF-APPR-004", Name = "Multi-Level Approval", Status = "Active" },
                new { Code = "WF-AUDIT-005", Name = "Audit Trail Workflow", Status = "Active" }
            };
            
            foreach (var wf in mockupAvailable)
            {
                var item = new ListViewItem(wf.Code);
                item.SubItems.Add(wf.Name);
                item.SubItems.Add(wf.Status);
                item.Tag = wf.Code;
                item.BackColor = Color.FromArgb(248, 250, 253);
                lstWorkflowAvailable.Items.Add(item);
            }
        }

        private void UpdateWorkflowLinkedCount()
        {
            lblWorkflowLinked.Text = string.Format("Workflow ที่เชื่อมโยง ({0} รายการ)", lstWorkflowLinked.Items.Count);
        }

        private void LstWorkflowLinked_DoubleClick(object sender, EventArgs e)
        {
            if (lstWorkflowLinked.SelectedItems.Count == 0) return;
            
            try
            {
                string workflowName = lstWorkflowLinked.SelectedItems[0].SubItems[2].Text;
                ShowInfo(string.Format("ดู Workflow: {0}\n\n(Mockup - ยังไม่ได้เชื่อมต่อกับ Workflow Engine)", workflowName));
            }
            catch (Exception ex)
            {
                ShowError("เปิด Workflow ไม่สำเร็จ: " + ex.Message);
            }
        }

        private void BtnRemoveWorkflowNew_Click(object sender, EventArgs e)
        {
            if (lstWorkflowLinked.SelectedItems.Count == 0)
            {
                ShowInfo("กรุณาเลือก Workflow ที่ต้องการลบ");
                return;
            }
            
            var confirm = MessageBox.Show(
                string.Format("ต้องการลบ Workflow ที่เลือก {0} รายการ ใช่หรือไม่?", lstWorkflowLinked.SelectedItems.Count),
                "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (confirm == DialogResult.Yes)
            {
                while (lstWorkflowLinked.SelectedItems.Count > 0)
                    lstWorkflowLinked.Items.Remove(lstWorkflowLinked.SelectedItems[0]);
                
                // Renumber
                for (int i = 0; i < lstWorkflowLinked.Items.Count; i++)
                {
                    lstWorkflowLinked.Items[i].Text = (i + 1).ToString();
                    lstWorkflowLinked.Items[i].BackColor = (i + 1) % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                }
                
                UpdateWorkflowLinkedCount();
                ShowInfo("ลบ Workflow เรียบร้อยแล้ว");
            }
        }

        private void BtnViewWorkflowDiagram_Click(object sender, EventArgs e)
        {
            if (lstWorkflowLinked.SelectedItems.Count == 0)
            {
                ShowInfo("กรุณาเลือก Workflow ที่ต้องการดู Diagram");
                return;
            }
            
            string workflowName = lstWorkflowLinked.SelectedItems[0].SubItems[2].Text;
            ShowInfo(string.Format("ดู Workflow Diagram: {0}\n\n" +
                     "จะแสดง:\n" +
                     "• ขั้นตอนการทำงาน (Steps)\n" +
                     "• เงื่อนไขการอนุมัติ (Conditions)\n" +
                     "• ผู้รับผิดชอบแต่ละขั้นตอน (Assignees)\n" +
                     "• เส้นทางการไหล (Flow)\n\n" +
                     "(Mockup - ยังไม่ได้เชื่อมต่อกับ Workflow Engine)", workflowName));
        }

        private void BtnStartWorkflow_Click(object sender, EventArgs e)
        {
            if (lstWorkflowLinked.SelectedItems.Count == 0)
            {
                ShowInfo("กรุณาเลือก Workflow ที่ต้องการเริ่ม");
                return;
            }
            
            string workflowName = lstWorkflowLinked.SelectedItems[0].SubItems[2].Text;
            var confirm = MessageBox.Show(
                string.Format("ต้องการเริ่ม Workflow: {0} ใช่หรือไม่?\n\n" +
                             "Workflow จะถูกเริ่มต้นและส่งไปยังผู้รับผิดชอบขั้นตอนแรก",
                             workflowName),
                "ยืนยันการเริ่ม Workflow", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (confirm == DialogResult.Yes)
            {
                ShowInfo(string.Format("เริ่ม Workflow: {0} เรียบร้อยแล้ว\n\n" +
                         "สถานะ: กำลังดำเนินการ\n" +
                         "ขั้นตอนปัจจุบัน: รอการอนุมัติ\n\n" +
                         "(Mockup - ยังไม่ได้เชื่อมต่อกับ Workflow Engine)", workflowName));
            }
        }

        private void LstWorkflowAvailable_DoubleClick(object sender, EventArgs e)
        {
            if (lstWorkflowAvailable.SelectedItems.Count == 0) return;
            
            try
            {
                var selectedItem = lstWorkflowAvailable.SelectedItems[0];
                
                int newIndex = lstWorkflowLinked.Items.Count + 1;
                var newItem = new ListViewItem(newIndex.ToString());
                newItem.SubItems.Add(selectedItem.SubItems[0].Text);
                newItem.SubItems.Add(selectedItem.SubItems[1].Text);
                newItem.SubItems.Add(selectedItem.SubItems[2].Text);
                newItem.Tag = newIndex;
                newItem.BackColor = newIndex % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                
                lstWorkflowLinked.Items.Add(newItem);
                lstWorkflowAvailable.Items.Remove(selectedItem);
                
                UpdateWorkflowLinkedCount();
                ShowInfo("เพิ่ม Workflow เรียบร้อยแล้ว");
            }
            catch (Exception ex)
            {
                ShowError("เพิ่ม Workflow ไม่สำเร็จ: " + ex.Message);
            }
        }

        // ══════════════════════════════════════════════════════════════════════════
        // TAB ATTACH - NEW IMPLEMENTATION (Two-Section Design - Mockup with DataGridView)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Initialize DataGridView columns for attached files
        /// </summary>
        private void InitializeAttachDataGridView()
        {
            dgvAttachLinked.Columns.Clear();
            
            // Column 0: #
            dgvAttachLinked.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colNum",
                HeaderText = "#",
                Width = 50,
                ReadOnly = true
            });
            
            // Column 1: ชื่อไฟล์
            dgvAttachLinked.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colFilename",
                HeaderText = "ชื่อไฟล์",
                Width = 400,
                ReadOnly = true
            });
            
            // Column 2: ขนาด
            dgvAttachLinked.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colSize",
                HeaderText = "ขนาด",
                Width = 100,
                ReadOnly = true
            });
            
            // Column 3: วันที่แนบ
            dgvAttachLinked.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "colDate",
                HeaderText = "วันที่แนบ",
                Width = 150,
                ReadOnly = true
            });
            
            // Column 4: View Button
            var colView = new DataGridViewButtonColumn
            {
                Name = "colView",
                HeaderText = "ดูไฟล์",
                Text = "👁️ ดู",
                UseColumnTextForButtonValue = true,
                Width = 80
            };
            dgvAttachLinked.Columns.Add(colView);
            
            // Column 5: Delete Button
            var colDelete = new DataGridViewButtonColumn
            {
                Name = "colDelete",
                HeaderText = "ลบไฟล์",
                Text = "🗑️ ลบ",
                UseColumnTextForButtonValue = true,
                Width = 80
            };
            dgvAttachLinked.Columns.Add(colDelete);
            
            // Style button columns
            dgvAttachLinked.Columns["colView"].DefaultCellStyle.BackColor = Color.FromArgb(46, 116, 181);
            dgvAttachLinked.Columns["colView"].DefaultCellStyle.ForeColor = Color.White;
            dgvAttachLinked.Columns["colView"].DefaultCellStyle.SelectionBackColor = Color.FromArgb(30, 95, 154);
            
            dgvAttachLinked.Columns["colDelete"].DefaultCellStyle.BackColor = Color.FromArgb(231, 76, 60);
            dgvAttachLinked.Columns["colDelete"].DefaultCellStyle.ForeColor = Color.White;
            dgvAttachLinked.Columns["colDelete"].DefaultCellStyle.SelectionBackColor = Color.FromArgb(192, 57, 43);
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
        // TAB DOCS - NEW IMPLEMENTATION (Two-Section Design - Mockup)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Load linked documents when form initializes (Mockup data)
        /// </summary>
        private void LoadLinkedDocuments()
        {
            lstDocsLinked.Items.Clear();
            
            // Mockup data - will be replaced with actual DMS integration
            var mockupDocs = new[]
            {
                new { Name = "Policy_Document_2569.pdf", Size = 1024 * 1024L, Date = DateTime.Now.AddDays(-10) },
                new { Name = "Compliance_Guidelines.docx", Size = 512 * 1024L, Date = DateTime.Now.AddDays(-7) }
            };
            
            int index = 1;
            foreach (var doc in mockupDocs)
            {
                var item = new ListViewItem(index.ToString());
                item.SubItems.Add(doc.Name);
                item.SubItems.Add(FormatFileSize(doc.Size));
                item.SubItems.Add(doc.Date.ToString("dd/MM/yyyy HH:mm"));
                item.Tag = index; // Mockup ID
                item.BackColor = index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                lstDocsLinked.Items.Add(item);
                index++;
            }
            
            UpdateDocsLinkedCount();
        }

        private void UpdateDocsLinkedCount()
        {
            lblDocsLinked.Text = string.Format("เอกสารที่แนบ ({0} เอกสาร)", lstDocsLinked.Items.Count);
        }

        private void LstDocsLinked_DoubleClick(object sender, EventArgs e)
        {
            if (lstDocsLinked.SelectedItems.Count == 0) return;
            
            try
            {
                string docName = lstDocsLinked.SelectedItems[0].SubItems[1].Text;
                ShowInfo(string.Format("เปิดเอกสาร: {0}\n\n(Mockup - ยังไม่ได้เชื่อมต่อกับ DMS)", docName));
            }
            catch (Exception ex)
            {
                ShowError("เปิดเอกสารไม่สำเร็จ: " + ex.Message);
            }
        }

        private void BtnRemoveDocNew_Click(object sender, EventArgs e)
        {
            if (lstDocsLinked.SelectedItems.Count == 0)
            {
                ShowInfo("กรุณาเลือกเอกสารที่ต้องการลบ");
                return;
            }
            
            var confirm = MessageBox.Show(
                string.Format("ต้องการลบเอกสารที่เลือก {0} เอกสาร ใช่หรือไม่?", lstDocsLinked.SelectedItems.Count),
                "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (confirm == DialogResult.Yes)
            {
                while (lstDocsLinked.SelectedItems.Count > 0)
                    lstDocsLinked.Items.Remove(lstDocsLinked.SelectedItems[0]);
                
                // Renumber
                for (int i = 0; i < lstDocsLinked.Items.Count; i++)
                {
                    lstDocsLinked.Items[i].Text = (i + 1).ToString();
                    lstDocsLinked.Items[i].BackColor = (i + 1) % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                }
                
                UpdateDocsLinkedCount();
                ShowInfo("ลบเอกสารเรียบร้อยแล้ว");
            }
        }

        private void BtnViewDocNew_Click(object sender, EventArgs e)
        {
            if (lstDocsLinked.SelectedItems.Count == 0)
            {
                ShowInfo("กรุณาเลือกเอกสารที่ต้องการดู");
                return;
            }
            
            string docName = lstDocsLinked.SelectedItems[0].SubItems[1].Text;
            ShowInfo(string.Format("ดูเอกสาร: {0}\n\n(Mockup - ยังไม่ได้เชื่อมต่อกับ DMS)", docName));
        }

        private void BtnUploadDocNew_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "เลือกเอกสารที่ต้องการแนบ";
                ofd.Filter = "All Files (*.*)|*.*|" +
                             "PDF Files (*.pdf)|*.pdf|" +
                             "Word Files (*.docx;*.doc)|*.docx;*.doc|" +
                             "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls|" +
                             "PowerPoint Files (*.pptx;*.ppt)|*.pptx;*.ppt";
                ofd.FilterIndex = 1;
                ofd.Multiselect = true;
                
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (var filePath in ofd.FileNames)
                    {
                        var fi = new System.IO.FileInfo(filePath);
                        int newIndex = lstDocsLinked.Items.Count + 1;
                        
                        var item = new ListViewItem(newIndex.ToString());
                        item.SubItems.Add(fi.Name);
                        item.SubItems.Add(FormatFileSize(fi.Length));
                        item.SubItems.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                        item.Tag = newIndex;
                        item.BackColor = newIndex % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                        lstDocsLinked.Items.Add(item);
                    }
                    
                    UpdateDocsLinkedCount();
                    ShowInfo(string.Format("เพิ่มเอกสารแนบ {0} เอกสารเรียบร้อยแล้ว\n\n(Mockup - เอกสารจะถูกอัพโหลดจริงเมื่อบันทึกบทความ)", ofd.FileNames.Length));
                }
            }
        }

        private void BtnSelectDocFromDMSNew_Click(object sender, EventArgs e)
        {
            ShowInfo("ฟีเจอร์เลือกเอกสารจาก DMS จะเปิด Document Browser\n\n" +
                     "สามารถเลือกเอกสารจากระบบจัดการเอกสาร (DMS) ที่มีอยู่แล้ว\n\n" +
                     "(Mockup - ยังไม่ได้เชื่อมต่อกับ DMS Module)");
        }

        // ══════════════════════════════════════════════════════════════════════════
        // TAB RELATED - NEW IMPLEMENTATION (Two-Section Design)
        // ══════════════════════════════════════════════════════════════════════════

        /// <summary>
        /// Load linked articles when form initializes
        /// </summary>
        private void LoadRelatedArticles()
        {
            if (_presenter == null || _presenter.EditingArticle == null) return;
            
            try
            {
                var linkedArticles = _presenter.GetLinkedArticles(_presenter.EditingArticle.ArticleID);
                lstRelatedLinked.Items.Clear();
                
                int index = 1;
                foreach (var article in linkedArticles)
                {
                    var item = new ListViewItem(index.ToString());
                    item.SubItems.Add(article.ArticleCode ?? "-");
                    item.SubItems.Add(article.Title ?? "-");
                    item.SubItems.Add(article.ArticleTypeName ?? "-");
                    item.Tag = article.ArticleID;
                    item.BackColor = index % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                    lstRelatedLinked.Items.Add(item);
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
            lblRelatedLinked.Text = string.Format("รายการที่ Link อยู่แล้ว ({0} รายการ)", lstRelatedLinked.Items.Count);
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
                    lstRelatedLinked.Items.Cast<ListViewItem>().Select(item => (int)item.Tag)
                );
                results = results.Where(a => !linkedIds.Contains(a.ArticleID));
                
                lstRelatedSearch.Items.Clear();
                foreach (var article in results.Take(50))
                {
                    var item = new ListViewItem(article.ArticleCode ?? "-");
                    item.SubItems.Add(article.Title ?? "-");
                    item.SubItems.Add(article.ArticleTypeName ?? "-");
                    item.SubItems.Add(article.ViewCount.ToString());
                    item.Tag = article.ArticleID;
                    item.BackColor = Color.FromArgb(248, 250, 253);
                    lstRelatedSearch.Items.Add(item);
                }
                
                lblRelatedSearch.Text = string.Format("ผลการค้นหา ({0} รายการ) — ดับเบิลคลิกเพื่อเพิ่ม", lstRelatedSearch.Items.Count);
                
                if (lstRelatedSearch.Items.Count == 0)
                    ShowInfo("ไม่พบบทความที่ตรงกับเงื่อนไขการค้นหา");
            }
            catch (Exception ex)
            {
                ShowError("ค้นหาไม่สำเร็จ: " + ex.Message);
            }
        }

        private void LstRelatedLinked_DoubleClick(object sender, EventArgs e)
        {
            if (lstRelatedLinked.SelectedItems.Count == 0) return;
            
            try
            {
                int articleId = (int)lstRelatedLinked.SelectedItems[0].Tag;
                var viewerForm = new ArticleViewerForm();
                viewerForm.ShowDialog();
            }
            catch (Exception ex)
            {
                ShowError("เปิดบทความไม่สำเร็จ: " + ex.Message);
            }
        }

        private void LstRelatedSearch_DoubleClick(object sender, EventArgs e)
        {
            if (lstRelatedSearch.SelectedItems.Count == 0) return;
            
            try
            {
                var selectedItem = lstRelatedSearch.SelectedItems[0];
                int articleId = (int)selectedItem.Tag;
                
                int newIndex = lstRelatedLinked.Items.Count + 1;
                var newItem = new ListViewItem(newIndex.ToString());
                newItem.SubItems.Add(selectedItem.SubItems[0].Text);
                newItem.SubItems.Add(selectedItem.SubItems[1].Text);
                newItem.SubItems.Add(selectedItem.SubItems[2].Text);
                newItem.Tag = articleId;
                newItem.BackColor = newIndex % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                
                lstRelatedLinked.Items.Add(newItem);
                lstRelatedSearch.Items.Remove(selectedItem);
                
                UpdateRelatedLinkedCount();
                lblRelatedSearch.Text = string.Format("ผลการค้นหา ({0} รายการ) — ดับเบิลคลิกเพื่อเพิ่ม", lstRelatedSearch.Items.Count);
                
                ShowInfo("เพิ่มบทความที่เกี่ยวข้องเรียบร้อยแล้ว");
            }
            catch (Exception ex)
            {
                ShowError("เพิ่มรายการไม่สำเร็จ: " + ex.Message);
            }
        }

        private void BtnRemoveRelatedNew_Click(object sender, EventArgs e)
        {
            if (lstRelatedLinked.SelectedItems.Count == 0)
            {
                ShowInfo("กรุณาเลือกรายการที่ต้องการลบ");
                return;
            }
            
            var confirm = MessageBox.Show(
                string.Format("ต้องการลบรายการที่เลือก {0} รายการ ใช่หรือไม่?", lstRelatedLinked.SelectedItems.Count),
                "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (confirm == DialogResult.Yes)
            {
                while (lstRelatedLinked.SelectedItems.Count > 0)
                    lstRelatedLinked.Items.Remove(lstRelatedLinked.SelectedItems[0]);
                
                for (int i = 0; i < lstRelatedLinked.Items.Count; i++)
                {
                    lstRelatedLinked.Items[i].Text = (i + 1).ToString();
                    lstRelatedLinked.Items[i].BackColor = (i + 1) % 2 == 0 ? Color.White : Color.FromArgb(248, 250, 253);
                }
                
                UpdateRelatedLinkedCount();
                ShowInfo("ลบรายการเรียบร้อยแล้ว");
            }
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
