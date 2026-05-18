using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    public partial class ApplyTemplateDialog : Form
    {
        private readonly IFolderTemplateController _templateController;
        private readonly IUserAccessOrgLookupController _orgLookupController;
        private readonly UserContext               _currentUser;
        private readonly Folder                    _parentFolder;  // folder ที่เข้ามาสร้าง
        private readonly string                    _parentFolderPath; // full breadcrumb path จาก FolderForm

        private List<FolderTemplate>     _templates         = new List<FolderTemplate>();
        private List<FolderTemplate>     _filteredTemplates = new List<FolderTemplate>();
        private List<FolderTemplateItem> _currentItems      = null;
        private List<OrgUnit>            _orgUnits          = new List<OrgUnit>();
        private int                      _nextTempId        = -1;

        public int? SelectedOrgUnitId { get; private set; }

        public ApplyTemplateDialog(
            IFolderTemplateController templateController,
            IUserAccessOrgLookupController orgLookupController,
            UserContext               currentUser,
            Folder                    parentFolder = null,
            string                    parentFolderPath = null)
        {
            if (templateController  == null) throw new ArgumentNullException("templateController");
            if (orgLookupController == null) throw new ArgumentNullException("orgLookupController");
            if (currentUser         == null) throw new ArgumentNullException("currentUser");

            _templateController   = templateController;
            _orgLookupController  = orgLookupController;
            _currentUser          = currentUser;
            _parentFolder         = parentFolder;
            _parentFolderPath     = parentFolderPath ?? string.Empty;

            InitializeComponent();
        }

        // ── Form Load ─────────────────────────────────────────────────────────
        private void ApplyTemplateDialog_Load(object sender, EventArgs e)
        {
            BeginInvoke(new Action(() => {
                if (splitMain.Width > splitMain.Panel1MinSize + splitMain.Panel2MinSize)
                    splitMain.SplitterDistance = Math.Max(splitMain.Panel1MinSize, splitMain.Width / 2);
                AutoSizeFormWidth();
            }));

            LoadData();

            // Lock ระดับตาม parent folder ที่เข้ามาสร้าง
            if (_parentFolder != null)
            {
                int level = _parentFolder.ConfidentialityLevel;
                // SelectedIndex: 0=1HC, 1=2SC, 2=3NC, 3=4PC
                cboConfidentiality.SelectedIndex = level - 1;
                cboConfidentiality.Enabled       = false;
                cboConfidentiality.BackColor      = System.Drawing.Color.FromArgb(240, 240, 240);

                // แสดงชื่อ folder ที่เข้ามาสร้างใน title
                this.Text = string.Format("สร้าง Folder จาก Template ใต้ \"{0}\"", _parentFolder.FolderName);

                // Set root name ตาม parent
                if (string.IsNullOrWhiteSpace(txtRootName.Text))
                    txtRootName.Text = _parentFolder.FolderName;
            }

            // แสดง full path breadcrumb ให้ผู้ใช้เห็นตำแหน่งที่จะสร้าง
            UpdatePathBreadcrumb();
        }

        // ── Data loading ──────────────────────────────────────────────────────
        private void LoadData()
        {
            try
            {
                lblStatus.Text = "⏳ กำลังโหลดข้อมูล...";

                _orgUnits = _orgLookupController.GetHierarchy()
                    .Where(u => u.IsActive && !u.IsDeleted)
                    .ToList();

                // สร้าง TreeView แทน ComboBox flat list
                treeOrgUnit.BeginUpdate();
                treeOrgUnit.Nodes.Clear();

                // จัดกลุ่มตาม Tier: SubCommittee → Division → Department
                var subCommittees = _orgUnits.Where(u => u.Tier == CMS.Domain.Enums.OrgUnitTier.SubCommittee)
                                             .OrderBy(u => u.Name).ToList();

                foreach (var sc in subCommittees)
                {
                    var scNode = new TreeNode(string.Format("🏢 {0}", sc.Name)) { Tag = sc, ToolTipText = sc.Name };
                    scNode.ForeColor = Color.FromArgb(31, 56, 100);

                    var divisions = _orgUnits.Where(u => u.Tier == CMS.Domain.Enums.OrgUnitTier.Division
                                                      && u.ParentId == sc.OrgUnitId)
                                            .OrderBy(u => u.Name).ToList();
                    foreach (var div in divisions)
                    {
                        var divNode = new TreeNode(string.Format("📁 {0}", div.Name)) { Tag = div, ToolTipText = div.Name };
                        divNode.ForeColor = Color.FromArgb(0, 80, 160);

                        var depts = _orgUnits.Where(u => u.Tier == CMS.Domain.Enums.OrgUnitTier.Department
                                                      && u.ParentId == div.OrgUnitId)
                                            .OrderBy(u => u.Name).ToList();
                        foreach (var dept in depts)
                        {
                            var deptNode = new TreeNode(string.Format("📋 {0}", dept.Name)) { Tag = dept, ToolTipText = dept.Name };
                            deptNode.ForeColor = Color.FromArgb(60, 60, 60);
                            divNode.Nodes.Add(deptNode);
                        }
                        scNode.Nodes.Add(divNode);
                    }
                    treeOrgUnit.Nodes.Add(scNode);
                }

                treeOrgUnit.CollapseAll();
                treeOrgUnit.EndUpdate();

                _templates = _templateController.GetAll()
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.TemplateName)
                    .ToList();

                FilterAndShowTemplates(null);

                lblStatus.Text = string.Format("✔ โหลดสำเร็จ — {0} หน่วยงาน, {1} Template",
                    _orgUnits.Count, _templates.Count);
            }
            catch (Exception ex)
            {
                lblStatus.Text      = "⚠ โหลดข้อมูลไม่สำเร็จ: " + ex.Message;
                lblStatus.ForeColor = Color.DarkRed;
            }
        }

        private void BtnExpandAll_Click(object sender, EventArgs e)
        {
            treeOrgUnit.ExpandAll();
        }

        private void BtnCollapseAll_Click(object sender, EventArgs e)
        {
            treeOrgUnit.CollapseAll();
        }

        private void TreeOrgUnit_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var ou = e.Node?.Tag as OrgUnit;
            FilterAndShowTemplates(ou);
        }

        private void FilterAndShowTemplates(OrgUnit selectedOU)
        {
            IEnumerable<FolderTemplate> query = _templates;

            if (selectedOU != null)
            {
                string tierName = selectedOU.Tier.ToString(); // "SubCommittee" | "Division" | "Department"
                query = _templates.Where(t =>
                    (t.OrgUnitID.HasValue && t.OrgUnitID.Value == selectedOU.OrgUnitId) ||
                    (!t.OrgUnitID.HasValue && (t.TargetType == "Any" || t.TargetType == tierName)));
            }

            _filteredTemplates = query.ToList();

            lstTemplates.Items.Clear();
            foreach (var t in _filteredTemplates)
                lstTemplates.Items.Add(string.Format("  {0}  [{1}]", t.TemplateName, t.TargetType));

            if (lstTemplates.Items.Count > 0)
                lstTemplates.SelectedIndex = 0;
            else
            {
                _currentItems = null;
                treePreview.Nodes.Clear();
                lblTemplateDesc.Text = string.Empty;
            }
        }

        private void LstTemplates_Changed(object sender, EventArgs e)
        {
            var tmpl = GetSelectedTemplate();
            if (tmpl == null) { treePreview.Nodes.Clear(); lblTemplateDesc.Text = string.Empty; return; }

            lblTemplateDesc.Text = string.IsNullOrEmpty(tmpl.Description) ? string.Empty : "ℹ  " + tmpl.Description;
            txtRootName.Text = tmpl.TemplateName;

            try
            {
                _currentItems = _templateController.GetItems(tmpl.TemplateID).ToList();
                _nextTempId   = -1;
                RefreshPreview();
                UpdatePathBreadcrumb();

                if (_parentFolder != null)
                {
                    lblStatus.Text = string.Format(
                        "จะสร้างใต้: {0}  |  Template: {1}  |  {2} folder(s)  — กด \"สร้าง Folder\" เพื่อดำเนินการ",
                        _parentFolder.FolderName, tmpl.TemplateName, _currentItems.Count);
                }
                else
                {
                    var ou = GetSelectedOrgUnit();
                    lblStatus.Text = string.Format(
                        "Template: {0}  |  {1} folder(s){2}  — กด \"สร้าง Folder\" เพื่อดำเนินการ",
                        tmpl.TemplateName, _currentItems.Count,
                        ou != null ? "  |  หน่วยงาน: " + ou.Name : string.Empty);
                }
                lblStatus.ForeColor = Color.FromArgb(0, 100, 180);
            }
            catch { }
        }

        private void TxtRootName_TextChanged(object sender, EventArgs e)
        {
            RefreshPreview();
        }

        private void RefreshPreview()
        {
            if (_currentItems == null) return;

            string rootName = string.IsNullOrWhiteSpace(txtRootName.Text)
                ? "Root Folder"
                : txtRootName.Text.Trim();

            treePreview.BeginUpdate();
            treePreview.Nodes.Clear();
            var root = new TreeNode(string.Format("📁  {0}", rootName));
            root.ForeColor = Color.FromArgb(31, 56, 100);
            root.NodeFont  = new Font("Segoe UI", 9.5f, FontStyle.Bold);
            BuildPreviewNodes(root.Nodes, _currentItems, null);
            treePreview.Nodes.Add(root);
            treePreview.ExpandAll();
            treePreview.EndUpdate();
        }

        private void BuildPreviewNodes(TreeNodeCollection nodes, IList<FolderTemplateItem> items, int? parentId)
        {
            foreach (var item in items.Where(i => i.ParentItemID == parentId).OrderBy(i => i.SortOrder))
            {
                var node = new TreeNode(string.Format("📂  {0}", item.FolderName)) { Tag = item, ToolTipText = item.FolderName };
                BuildPreviewNodes(node.Nodes, items, item.ItemID);
                nodes.Add(node);
            }
        }

        // ── Add / Remove sub-folder in preview ────────────────────────────────
        private void BtnAddSubFolder_Click(object sender, EventArgs e)
        {
            if (_currentItems == null)
            {
                MessageBox.Show("กรุณาเลือก Template ก่อน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var selectedNode = treePreview.SelectedNode;
            if (selectedNode == null)
            {
                MessageBox.Show("กรุณาเลือก Folder ที่ต้องการเพิ่ม Sub-folder", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            string folderName = PromptFolderName();
            if (string.IsNullOrWhiteSpace(folderName)) return;

            var parentItem  = selectedNode.Tag as FolderTemplateItem;
            int? parentItemId = parentItem != null ? parentItem.ItemID : (int?)null;
            int  sortOrder    = _currentItems.Count(i => i.ParentItemID == parentItemId) + 1;
            int  templateId   = _currentItems.Count > 0 ? _currentItems[0].TemplateID : 0;

            _currentItems.Add(new FolderTemplateItem
            {
                ItemID               = _nextTempId--,
                TemplateID           = templateId,
                ParentItemID         = parentItemId,
                FolderName           = folderName.Trim(),
                ConfidentialityLevel = 0,
                SortOrder            = sortOrder,
                InheritPermission    = true
            });

            RefreshPreview();
            lblStatus.ForeColor = Color.FromArgb(0, 100, 0);
            lblStatus.Text      = string.Format("เพิ่ม Sub-folder \"{0}\" เรียบร้อย — กด \"สร้าง Folder\" เพื่อดำเนินการ", folderName.Trim());
        }

        private void BtnRemovePreviewNode_Click(object sender, EventArgs e)
        {
            var selectedNode = treePreview.SelectedNode;
            if (selectedNode == null) return;

            var item = selectedNode.Tag as FolderTemplateItem;
            if (item == null)
            {
                MessageBox.Show("ไม่สามารถลบ Root Folder ได้", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Information);
                return;
            }

            var confirm = MessageBox.Show(
                string.Format("ลบ \"{0}\" และ Sub-folder ทั้งหมดออกจาก Preview?\n\nTemplate เดิมใน Database จะไม่ถูกแก้ไข", item.FolderName),
                "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question);
            if (confirm != DialogResult.Yes) return;

            RemoveItemAndDescendants(item.ItemID);
            RefreshPreview();
            lblStatus.ForeColor = Color.FromArgb(80, 80, 80);
            lblStatus.Text      = string.Format("ลบ \"{0}\" ออกจาก Preview เรียบร้อย", item.FolderName);
        }

        private void RemoveItemAndDescendants(int itemId)
        {
            var children = _currentItems.Where(i => i.ParentItemID == itemId).ToList();
            foreach (var child in children)
                RemoveItemAndDescendants(child.ItemID);
            _currentItems.RemoveAll(i => i.ItemID == itemId);
        }

        // ── Prompt helper ─────────────────────────────────────────────────────
        private string PromptFolderName()
        {
            using (var frm = new Form())
            {
                frm.Text            = "เพิ่ม Sub-folder";
                frm.Size            = new Size(360, 130);
                frm.StartPosition   = FormStartPosition.CenterParent;
                frm.FormBorderStyle = FormBorderStyle.FixedDialog;
                frm.MaximizeBox     = false;
                frm.MinimizeBox     = false;
                frm.Font            = new Font("Segoe UI", 9f);

                var lbl    = new Label  { Text = "ชื่อ Folder ย่อย:", Dock = DockStyle.Top, Height = 26, Padding = new Padding(8, 6, 0, 0) };
                var txt    = new TextBox { Dock = DockStyle.Top, Height = 24 };
                var pnlBtn = new Panel  { Dock = DockStyle.Bottom, Height = 42 };
                var btnOk  = new Button { Text = "ตกลง",   DialogResult = DialogResult.OK,     Size = new Size(80, 28), Location = new Point(186, 7) };
                var btnCnl = new Button { Text = "ยกเลิก", DialogResult = DialogResult.Cancel, Size = new Size(80, 28), Location = new Point(270, 7) };

                btnOk.FlatStyle  = FlatStyle.Flat;
                btnCnl.FlatStyle = FlatStyle.Flat;
                btnOk.BackColor  = Color.FromArgb(31, 56, 100);
                btnOk.ForeColor  = Color.White;
                btnOk.FlatAppearance.BorderSize = 0;

                pnlBtn.Controls.Add(btnOk);
                pnlBtn.Controls.Add(btnCnl);
                frm.Controls.Add(txt);
                frm.Controls.Add(lbl);
                frm.Controls.Add(pnlBtn);
                frm.AcceptButton = btnOk;
                frm.CancelButton = btnCnl;

                return frm.ShowDialog(this) == DialogResult.OK ? txt.Text : null;
            }
        }

        // ── Apply ─────────────────────────────────────────────────────────────
        private void BtnApply_Click(object sender, EventArgs e)
        {
            var tmpl = GetSelectedTemplate();

            if (tmpl == null)
            {
                MessageBox.Show("กรุณาเลือก Template", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_currentItems == null || _currentItems.Count == 0)
            {
                MessageBox.Show("Template นี้ไม่มี Sub-Folder กำหนดไว้", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int conf = _parentFolder != null
                ? _parentFolder.ConfidentialityLevel
                : (cboConfidentiality.SelectedIndex >= 0 ? cboConfidentiality.SelectedIndex + 1 : 4);

            // ถ้ามี parentFolder → สร้างภายใต้ folder นั้น (ใช้ _currentItems โดยตรง ไม่ re-fetch DB)
            if (_parentFolder != null)
            {
                try
                {
                    btnApply.Enabled = false;
                    btnApply.Text    = "⏳  กำลังสร้าง...";
                    lblStatus.Text   = "⏳ กำลังสร้าง Folder...";
                    Application.DoEvents();

                    _templateController.ApplyTemplateToFolder(
                        _parentFolder.FolderId,
                        _currentItems,
                        _parentFolder.ConfidentialityLevel,
                        _currentUser);

                    SelectedOrgUnitId = null;
                    lblStatus.Text      = "✅ สร้าง Folder เรียบร้อยแล้ว";
                    lblStatus.ForeColor = Color.FromArgb(0, 120, 0);
                    DialogResult = DialogResult.OK;
                }
                catch (UnauthorizedAccessException ex)
                {
                    MessageBox.Show("ไม่มีสิทธิ์ดำเนินการ:\n" + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                }
                catch (Exception ex)
                {
                    MessageBox.Show("ไม่สามารถสร้าง Folder ได้:\n" + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
                finally
                {
                    btnApply.Enabled = true;
                    btnApply.Text    = "✅  สร้าง Folder";
                }
                return;
            }

            // ไม่มี parentFolder → ใช้ OrgUnit (ใช้ _currentItems โดยตรง ไม่ re-fetch DB)
            var ou = GetSelectedOrgUnit();
            if (ou == null)
            {
                MessageBox.Show("กรุณาเลือกหน่วยงาน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            if (string.IsNullOrWhiteSpace(txtRootName.Text))
            {
                MessageBox.Show("กรุณาระบุชื่อ Root Folder", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                btnApply.Enabled = false;
                btnApply.Text    = "⏳  กำลังสร้าง...";
                lblStatus.Text   = "⏳ กำลังสร้าง Folder...";
                Application.DoEvents();

                _templateController.ApplyTemplateToOrgUnit(
                    ou.OrgUnitId, _currentItems,
                    txtRootName.Text.Trim(), conf, _currentUser);

                SelectedOrgUnitId = ou.OrgUnitId;
                lblStatus.Text      = "✅ สร้าง Folder เรียบร้อยแล้ว";
                lblStatus.ForeColor = Color.FromArgb(0, 120, 0);
                DialogResult = DialogResult.OK;
            }
            catch (UnauthorizedAccessException ex)
            {
                MessageBox.Show("ไม่มีสิทธิ์ดำเนินการ:\n" + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถสร้าง Folder ได้:\n" + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                btnApply.Enabled = true;
                btnApply.Text    = "✅  สร้าง Folder";
            }
        }

        // ── Helpers ───────────────────────────────────────────────────────────
        private OrgUnit GetSelectedOrgUnit()
        {
            return treeOrgUnit.SelectedNode?.Tag as OrgUnit;
        }

        private FolderTemplate GetSelectedTemplate()
        {
            int idx = lstTemplates.SelectedIndex;
            return (idx >= 0 && idx < _filteredTemplates.Count) ? _filteredTemplates[idx] : null;
        }

        /// <summary>
        /// อัปเดต lblPathBreadcrumb ให้แสดง full path ของตำแหน่งที่จะสร้าง folder
        /// เช่น "📍 🏠 PC  ›  ฝ่ายผู้บริหาร  ›  4PC - (Public Confidential)"
        /// </summary>
        private void UpdatePathBreadcrumb()
        {
            if (!string.IsNullOrEmpty(_parentFolderPath))
            {
                lblPathBreadcrumb.Text    = "📍 " + _parentFolderPath;
                lblPathBreadcrumb.Visible = true;
                toolTipMain.SetToolTip(lblPathBreadcrumb, _parentFolderPath);
                return;
            }

            if (_parentFolder != null)
            {
                lblPathBreadcrumb.Text    = "📍 " + _parentFolder.FolderName;
                lblPathBreadcrumb.Visible = true;
                toolTipMain.SetToolTip(lblPathBreadcrumb, _parentFolder.FolderName);
                return;
            }

            lblPathBreadcrumb.Visible = false;
        }

        // ── Auto-resize to fit widest org-unit name ────────────────────────────
        private void AutoSizeFormWidth()
        {
            if (treeOrgUnit.Nodes.Count == 0) return;

            int maxWidth = 0;
            using (var g = treeOrgUnit.CreateGraphics())
                MeasureMaxNodeWidth(treeOrgUnit.Nodes, g, treeOrgUnit.Indent, 1, ref maxWidth);

            // account for expand icon (~20px), scrollbar (~20px), left panel padding (24px)
            int leftNeeded = maxWidth + 60;
            int currentLeft = splitMain.Panel1.ClientSize.Width;
            if (leftNeeded <= currentLeft) return;

            int extra = leftNeeded - currentLeft;
            var workArea = Screen.FromControl(this).WorkingArea;
            int newWidth = Math.Min(this.Width + extra, workArea.Width - 40);
            if (newWidth > this.Width)
                this.Width = newWidth;
        }

        private void MeasureMaxNodeWidth(TreeNodeCollection nodes, System.Drawing.Graphics g, int indent, int depth, ref int maxWidth)
        {
            foreach (TreeNode node in nodes)
            {
                int w = (int)g.MeasureString(node.Text, treeOrgUnit.Font).Width + depth * indent;
                if (w > maxWidth) maxWidth = w;
                MeasureMaxNodeWidth(node.Nodes, g, indent, depth + 1, ref maxWidth);
            }
        }
    }
}
