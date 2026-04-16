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
            tabAttach.Controls.Add(CreateAttachToolbar());
            tabRelated.Controls.Add(CreateRelatedToolbar());
            tabWorkflow.Controls.Add(CreateWorkflowToolbar());
            tabPrograms.Controls.Add(CreateProgramsToolbar());
            tabDocs.Controls.Add(CreateDocsToolbar());
            
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
            return lstRelated;
        }

        public System.Windows.Forms.ListView GetWorkflowListView()
        {
            return lstWorkflow;
        }

        public System.Windows.Forms.ListView GetProgramsListView()
        {
            return lstPrograms;
        }

        public System.Windows.Forms.ListView GetDocsListView()
        {
            return lstDocs;
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

        private Panel CreateAttachToolbar()
        {
            var toolbar = new Panel
            {
                Dock = DockStyle.Top,
                Height = 36,
                BackColor = Color.FromArgb(245, 247, 250)
            };
            
            _txtSearchAttach = new TextBox
            {
                Name = "txtSearchAttach",
                Location = new Point(8, 6),
                Size = new Size(250, 24),
                Font = new Font("Segoe UI", 9f)
            };
            _txtSearchAttach.TextChanged += TxtSearchAttach_TextChanged;
            
            // Add placeholder text using a label overlay
            var lblPlaceholder = new Label
            {
                Name = "lblSearchAttachPlaceholder",
                Text = "🔍 ค้นหาไฟล์แนบ...",
                Location = new Point(10, 9),
                AutoSize = true,
                BackColor = Color.Transparent,
                ForeColor = Color.Gray,
                Font = new Font("Segoe UI", 9f),
                Cursor = Cursors.IBeam
            };
            lblPlaceholder.Click += (s, e) => _txtSearchAttach.Focus();
            
            // Hide placeholder when textbox has focus or text
            _txtSearchAttach.Enter += (s, e) => lblPlaceholder.Visible = string.IsNullOrEmpty(_txtSearchAttach.Text);
            _txtSearchAttach.Leave += (s, e) => lblPlaceholder.Visible = string.IsNullOrEmpty(_txtSearchAttach.Text);
            _txtSearchAttach.TextChanged += (s, e) => lblPlaceholder.Visible = string.IsNullOrEmpty(_txtSearchAttach.Text);
            
            var btnUpload = new Button
            {
                Location = new Point(266, 6),
                Size = new Size(120, 24),
                Text = "📤 อัพโหลดไฟล์",
                Font = new Font("Segoe UI", 9f),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.FromArgb(46, 116, 181),
                ForeColor = Color.White,
                Cursor = Cursors.Hand
            };
            btnUpload.FlatAppearance.BorderColor = Color.FromArgb(30, 95, 154);
            btnUpload.Click += BtnUploadAttachment_Click;
            
            var btnRemove = new Button
            {
                Location = new Point(394, 6),
                Size = new Size(100, 24),
                Text = "🗑️ ลบไฟล์",
                Font = new Font("Segoe UI", 9f),
                FlatStyle = FlatStyle.Flat,
                BackColor = Color.White,
                ForeColor = Color.FromArgb(231, 76, 60),
                Cursor = Cursors.Hand
            };
            btnRemove.FlatAppearance.BorderColor = Color.FromArgb(231, 76, 60);
            btnRemove.Click += BtnRemoveAttachment_Click;
            
            toolbar.Controls.Add(_txtSearchAttach);
            toolbar.Controls.Add(lblPlaceholder);
            toolbar.Controls.Add(btnUpload);
            toolbar.Controls.Add(btnRemove);
            
            lblPlaceholder.BringToFront();
            
            return toolbar;
        }

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
            
            toolbar.Controls.Add(_txtSearchRelated);
            toolbar.Controls.Add(lblPlaceholder);
            toolbar.Controls.Add(btnRemove);
            
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
            
            toolbar.Controls.Add(_txtSearchWorkflow);
            toolbar.Controls.Add(lblPlaceholder);
            toolbar.Controls.Add(btnRemove);
            
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
            
            toolbar.Controls.Add(_txtSearchPrograms);
            toolbar.Controls.Add(lblPlaceholder);
            toolbar.Controls.Add(btnRemove);
            
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
            
            toolbar.Controls.Add(_txtSearchDocs);
            toolbar.Controls.Add(lblPlaceholder);
            toolbar.Controls.Add(btnRemove);
            
            lblPlaceholder.BringToFront();
            
            return toolbar;
        }

        // ── Tab Mapping Event Handlers ───────────────────────────────────────

        private void BtnUploadAttachment_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog())
            {
                ofd.Title = "เลือกไฟล์ที่ต้องการแนบ";
                ofd.Filter = "All Files (*.*)|*.*";
                ofd.Multiselect = true;
                
                if (ofd.ShowDialog() == DialogResult.OK)
                {
                    foreach (var file in ofd.FileNames)
                    {
                        var fi = new System.IO.FileInfo(file);
                        var item = new ListViewItem(fi.Name);
                        item.SubItems.Add(FormatFileSize(fi.Length));
                        item.SubItems.Add(DateTime.Now.ToString("dd/MM/yyyy HH:mm"));
                        item.Tag = file;
                        lstAttach.Items.Add(item);
                    }
                    
                    ShowInfo(string.Format("เพิ่มไฟล์แนบ {0} ไฟล์เรียบร้อยแล้ว", ofd.FileNames.Length));
                }
            }
        }

        private void BtnRemoveAttachment_Click(object sender, EventArgs e)
        {
            if (lstAttach.SelectedItems.Count == 0)
            {
                ShowInfo("กรุณาเลือกไฟล์ที่ต้องการลบ");
                return;
            }
            
            var confirm = MessageBox.Show(
                string.Format("ต้องการลบไฟล์ที่เลือก {0} ไฟล์ ใช่หรือไม่?", lstAttach.SelectedItems.Count),
                "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (confirm == DialogResult.Yes)
            {
                while (lstAttach.SelectedItems.Count > 0)
                {
                    lstAttach.Items.Remove(lstAttach.SelectedItems[0]);
                }
                
                // Refresh search list so removed items may reappear
                PerformAttachSearch();
                
                ShowInfo("ลบไฟล์เรียบร้อยแล้ว");
            }
        }

        private void BtnAddRelated_Click(object sender, EventArgs e)
        {
            // TODO: Open article selection dialog
            ShowInfo("ฟีเจอร์นี้จะเปิด Dialog เลือกบทความที่เกี่ยวข้อง\n\nสามารถเพิ่มการเชื่อมโยงไปยังบทความอื่นๆ ในระบบ");
        }

        private void BtnRemoveRelated_Click(object sender, EventArgs e)
        {
            if (lstRelated.SelectedItems.Count == 0)
            {
                ShowInfo("กรุณาเลือกรายการที่ต้องการลบ");
                return;
            }
            
            var confirm = MessageBox.Show(
                string.Format("ต้องการลบความรู้ที่เกี่ยวข้อง {0} รายการ ใช่หรือไม่?", lstRelated.SelectedItems.Count),
                "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (confirm == DialogResult.Yes)
            {
                while (lstRelated.SelectedItems.Count > 0)
                {
                    lstRelated.Items.Remove(lstRelated.SelectedItems[0]);
                }
                
                // Refresh search list so removed items may reappear
                PerformRelatedSearch();
                
                ShowInfo("ลบรายการเรียบร้อยแล้ว");
            }
        }

        private void BtnAddWorkflow_Click(object sender, EventArgs e)
        {
            // TODO: Open workflow selection dialog
            ShowInfo("ฟีเจอร์นี้จะเปิด Dialog เลือก Workflow\n\nสามารถเชื่อมโยงบทความกับเส้นงาน (Workflow) ที่เกี่ยวข้อง");
        }

        private void BtnRemoveWorkflow_Click(object sender, EventArgs e)
        {
            if (lstWorkflow.SelectedItems.Count == 0)
            {
                ShowInfo("กรุณาเลือกรายการที่ต้องการลบ");
                return;
            }
            
            var confirm = MessageBox.Show(
                string.Format("ต้องการลบ Workflow {0} รายการ ใช่หรือไม่?", lstWorkflow.SelectedItems.Count),
                "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (confirm == DialogResult.Yes)
            {
                while (lstWorkflow.SelectedItems.Count > 0)
                {
                    lstWorkflow.Items.Remove(lstWorkflow.SelectedItems[0]);
                }
                
                // Refresh search list so removed items may reappear
                PerformWorkflowSearch();
                
                ShowInfo("ลบรายการเรียบร้อยแล้ว");
            }
        }

        private void BtnAddProgram_Click(object sender, EventArgs e)
        {
            // TODO: Open program selection dialog
            ShowInfo("ฟีเจอร์นี้จะเปิด Dialog เลือกโปรแกรม\n\nสามารถเชื่อมโยงบทความกับโปรแกรมที่เกี่ยวข้อง");
        }

        private void BtnRemoveProgram_Click(object sender, EventArgs e)
        {
            if (lstPrograms.SelectedItems.Count == 0)
            {
                ShowInfo("กรุณาเลือกรายการที่ต้องการลบ");
                return;
            }
            
            var confirm = MessageBox.Show(
                string.Format("ต้องการลบโปรแกรม {0} รายการ ใช่หรือไม่?", lstPrograms.SelectedItems.Count),
                "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (confirm == DialogResult.Yes)
            {
                while (lstPrograms.SelectedItems.Count > 0)
                {
                    lstPrograms.Items.Remove(lstPrograms.SelectedItems[0]);
                }
                
                // Refresh search list so removed items may reappear
                PerformProgramsSearch();
                
                ShowInfo("ลบรายการเรียบร้อยแล้ว");
            }
        }

        private void BtnAddDoc_Click(object sender, EventArgs e)
        {
            // TODO: Open document selection dialog (from DMS)
            ShowInfo("ฟีเจอร์นี้จะเปิด Dialog เลือกเอกสารจากระบบ DMS\n\nสามารถเชื่อมโยงบทความกับเอกสารในระบบจัดการเอกสาร");
        }

        private void BtnRemoveDoc_Click(object sender, EventArgs e)
        {
            if (lstDocs.SelectedItems.Count == 0)
            {
                ShowInfo("กรุณาเลือกรายการที่ต้องการลบ");
                return;
            }
            
            var confirm = MessageBox.Show(
                string.Format("ต้องการลบเอกสาร {0} รายการ ใช่หรือไม่?", lstDocs.SelectedItems.Count),
                "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            
            if (confirm == DialogResult.Yes)
            {
                while (lstDocs.SelectedItems.Count > 0)
                {
                    lstDocs.Items.Remove(lstDocs.SelectedItems[0]);
                }
                
                // Refresh search list so removed items may reappear
                PerformDocsSearch();
                
                ShowInfo("ลบรายการเรียบร้อยแล้ว");
            }
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
            lstSearchRelated.Items.Clear();
            
            if (string.IsNullOrEmpty(searchText)) return;
            
            try
            {
                // Call service to search published articles
                var results = _presenter.SearchPublishedArticles(searchText);
                
                // Exclude current article being edited
                if (_presenter.EditingArticle != null)
                    results = results.Where(a => a.ArticleID != _presenter.EditingArticle.ArticleID);
                
                // Exclude articles already in lstRelated
                var existingIds = new HashSet<int>(
                    lstRelated.Items.Cast<ListViewItem>()
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
                    lstSearchRelated.Items.Add(item);
                }
            }
            catch (Exception ex)
            {
                ShowError("ค้นหาไม่สำเร็จ: " + ex.Message);
            }
        }

        private void PerformAttachSearch()
        {
            if (_txtSearchAttach == null) return;
            
            string searchText = _txtSearchAttach.Text.Trim();
            lstSearchAttach.Items.Clear();
            
            if (string.IsNullOrEmpty(searchText)) return;
            
            try
            {
                // TODO: Implement attachment search when data source is available
                // For now, show placeholder message
                var item = new ListViewItem("ยังไม่รองรับการค้นหาไฟล์แนบ");
                item.SubItems.Add("-");
                item.SubItems.Add("-");
                lstSearchAttach.Items.Add(item);
            }
            catch (Exception ex)
            {
                ShowError("ค้นหาไม่สำเร็จ: " + ex.Message);
            }
        }

        private void PerformWorkflowSearch()
        {
            if (_txtSearchWorkflow == null) return;
            
            string searchText = _txtSearchWorkflow.Text.Trim();
            lstSearchWorkflow.Items.Clear();
            
            if (string.IsNullOrEmpty(searchText)) return;
            
            try
            {
                // TODO: Implement workflow search when data source is available
                // For now, show placeholder message
                var item = new ListViewItem("ยังไม่รองรับการค้นหา Workflow");
                item.SubItems.Add("-");
                item.SubItems.Add("-");
                lstSearchWorkflow.Items.Add(item);
            }
            catch (Exception ex)
            {
                ShowError("ค้นหาไม่สำเร็จ: " + ex.Message);
            }
        }

        private void PerformProgramsSearch()
        {
            if (_txtSearchPrograms == null) return;
            
            string searchText = _txtSearchPrograms.Text.Trim();
            lstSearchPrograms.Items.Clear();
            
            if (string.IsNullOrEmpty(searchText)) return;
            
            try
            {
                // TODO: Implement programs search when data source is available
                // For now, show placeholder message
                var item = new ListViewItem("ยังไม่รองรับการค้นหาโปรแกรม");
                item.SubItems.Add("-");
                lstSearchPrograms.Items.Add(item);
            }
            catch (Exception ex)
            {
                ShowError("ค้นหาไม่สำเร็จ: " + ex.Message);
            }
        }

        private void PerformDocsSearch()
        {
            if (_txtSearchDocs == null) return;
            
            string searchText = _txtSearchDocs.Text.Trim();
            lstSearchDocs.Items.Clear();
            
            if (string.IsNullOrEmpty(searchText)) return;
            
            try
            {
                // TODO: Implement documents search when data source is available
                // For now, show placeholder message
                var item = new ListViewItem("ยังไม่รองรับการค้นหาเอกสาร");
                item.SubItems.Add("-");
                lstSearchDocs.Items.Add(item);
            }
            catch (Exception ex)
            {
                ShowError("ค้นหาไม่สำเร็จ: " + ex.Message);
            }
        }

        // ── Double-Click Event Handlers ──────────────────────────────────────

        private void LstSearchRelated_DoubleClick(object sender, EventArgs e)
        {
            if (lstSearchRelated.SelectedItems.Count == 0) return;
            
            var selectedItem = lstSearchRelated.SelectedItems[0];
            
            // Clone item to main list
            var newItem = (ListViewItem)selectedItem.Clone();
            lstRelated.Items.Add(newItem);
            
            // Remove from search list
            lstSearchRelated.Items.Remove(selectedItem);
            
            ShowInfo("เพิ่มบทความที่เกี่ยวข้องเรียบร้อยแล้ว");
        }

        private void LstSearchAttach_DoubleClick(object sender, EventArgs e)
        {
            if (lstSearchAttach.SelectedItems.Count == 0) return;
            
            var selectedItem = lstSearchAttach.SelectedItems[0];
            
            // Clone item to main list
            var newItem = (ListViewItem)selectedItem.Clone();
            lstAttach.Items.Add(newItem);
            
            // Remove from search list
            lstSearchAttach.Items.Remove(selectedItem);
            
            ShowInfo("เพิ่มไฟล์แนบเรียบร้อยแล้ว");
        }

        private void LstSearchWorkflow_DoubleClick(object sender, EventArgs e)
        {
            if (lstSearchWorkflow.SelectedItems.Count == 0) return;
            
            var selectedItem = lstSearchWorkflow.SelectedItems[0];
            
            // Clone item to main list
            var newItem = (ListViewItem)selectedItem.Clone();
            lstWorkflow.Items.Add(newItem);
            
            // Remove from search list
            lstSearchWorkflow.Items.Remove(selectedItem);
            
            ShowInfo("เพิ่ม Workflow เรียบร้อยแล้ว");
        }

        private void LstSearchPrograms_DoubleClick(object sender, EventArgs e)
        {
            if (lstSearchPrograms.SelectedItems.Count == 0) return;
            
            var selectedItem = lstSearchPrograms.SelectedItems[0];
            
            // Clone item to main list
            var newItem = (ListViewItem)selectedItem.Clone();
            lstPrograms.Items.Add(newItem);
            
            // Remove from search list
            lstSearchPrograms.Items.Remove(selectedItem);
            
            ShowInfo("เพิ่มโปรแกรมเรียบร้อยแล้ว");
        }

        private void LstSearchDocs_DoubleClick(object sender, EventArgs e)
        {
            if (lstSearchDocs.SelectedItems.Count == 0) return;
            
            var selectedItem = lstSearchDocs.SelectedItems[0];
            
            // Clone item to main list
            var newItem = (ListViewItem)selectedItem.Clone();
            lstDocs.Items.Add(newItem);
            
            // Remove from search list
            lstSearchDocs.Items.Remove(selectedItem);
            
            ShowInfo("เพิ่มเอกสารเรียบร้อยแล้ว");
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
