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

        private List<FolderTemplate> _templates = new List<FolderTemplate>();
        private List<OrgUnit>        _orgUnits  = new List<OrgUnit>();

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
                    splitMain.SplitterDistance = 280;
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
                    var scNode = new TreeNode(string.Format("🏢 {0}", sc.Name)) { Tag = sc };
                    scNode.ForeColor = Color.FromArgb(31, 56, 100);

                    var divisions = _orgUnits.Where(u => u.Tier == CMS.Domain.Enums.OrgUnitTier.Division
                                                      && u.ParentId == sc.OrgUnitId)
                                            .OrderBy(u => u.Name).ToList();
                    foreach (var div in divisions)
                    {
                        var divNode = new TreeNode(string.Format("📁 {0}", div.Name)) { Tag = div };
                        divNode.ForeColor = Color.FromArgb(0, 80, 160);

                        var depts = _orgUnits.Where(u => u.Tier == CMS.Domain.Enums.OrgUnitTier.Department
                                                      && u.ParentId == div.OrgUnitId)
                                            .OrderBy(u => u.Name).ToList();
                        foreach (var dept in depts)
                        {
                            var deptNode = new TreeNode(string.Format("📋 {0}", dept.Name)) { Tag = dept };
                            deptNode.ForeColor = Color.FromArgb(60, 60, 60);
                            divNode.Nodes.Add(deptNode);
                        }
                        scNode.Nodes.Add(divNode);
                    }
                    treeOrgUnit.Nodes.Add(scNode);
                }

                treeOrgUnit.ExpandAll();
                treeOrgUnit.EndUpdate();

                _templates = _templateController.GetAll()
                    .Where(t => t.IsActive)
                    .OrderBy(t => t.TemplateName)
                    .ToList();

                lstTemplates.Items.Clear();
                foreach (var t in _templates)
                    lstTemplates.Items.Add(string.Format("  {0}  [{1}]", t.TemplateName, t.TargetType));
                if (lstTemplates.Items.Count > 0) lstTemplates.SelectedIndex = 0;

                lblStatus.Text = string.Format("✔ โหลดสำเร็จ — {0} หน่วยงาน, {1} Template",
                    _orgUnits.Count, _templates.Count);
            }
            catch (Exception ex)
            {
                lblStatus.Text      = "⚠ โหลดข้อมูลไม่สำเร็จ: " + ex.Message;
                lblStatus.ForeColor = Color.DarkRed;
            }
        }

        private void TreeOrgUnit_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var ou = e.Node?.Tag as OrgUnit;
            if (ou != null && string.IsNullOrWhiteSpace(txtRootName.Text))
                txtRootName.Text = ou.Name;
        }

        private void LstTemplates_Changed(object sender, EventArgs e)
        {
            var tmpl = GetSelectedTemplate();
            if (tmpl == null) { treePreview.Nodes.Clear(); lblTemplateDesc.Text = ""; return; }

            lblTemplateDesc.Text = string.IsNullOrEmpty(tmpl.Description) ? "" : "ℹ  " + tmpl.Description;

            if (string.IsNullOrWhiteSpace(txtRootName.Text))
            {
                var ou = GetSelectedOrgUnit();
                txtRootName.Text = ou != null ? ou.Name : tmpl.TemplateName;
            }

            try
            {
                var items = _templateController.GetItems(tmpl.TemplateID).ToList();
                treePreview.BeginUpdate();
                treePreview.Nodes.Clear();

                var root = new TreeNode(string.Format("📁  {0}", txtRootName.Text.Trim()));
                root.ForeColor = Color.FromArgb(31, 56, 100);
                root.NodeFont  = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                BuildPreviewNodes(root.Nodes, items, null);
                treePreview.Nodes.Add(root);
                treePreview.ExpandAll();
                treePreview.EndUpdate();

                // อัปเดต path breadcrumb เมื่อเลือก template
                UpdatePathBreadcrumb();

                // แสดง summary ใน lblStatus แทน confirmation dialog
                if (_parentFolder != null)
                {
                    lblStatus.Text = string.Format(
                        "จะสร้างใต้: {0}  |  Template: {1}  |  {2} folder(s)  — กด \"สร้าง Folder\" เพื่อดำเนินการ",
                        _parentFolder.FolderName, tmpl.TemplateName, items.Count);
                }
                else
                {
                    var ou2 = GetSelectedOrgUnit();
                    lblStatus.Text = string.Format(
                        "Template: {0}  |  {1} folder(s){2}  — กด \"สร้าง Folder\" เพื่อดำเนินการ",
                        tmpl.TemplateName, items.Count,
                        ou2 != null ? "  |  หน่วยงาน: " + ou2.Name : "");
                }
                lblStatus.ForeColor = Color.FromArgb(0, 100, 180);
            }
            catch { }
        }

        private void BuildPreviewNodes(TreeNodeCollection nodes, IList<FolderTemplateItem> items, int? parentId)
        {
            foreach (var item in items.Where(i => i.ParentItemID == parentId).OrderBy(i => i.SortOrder))
            {
                var node = new TreeNode(string.Format("📂  {0}", item.FolderName));
                BuildPreviewNodes(node.Nodes, items, item.ItemID);
                nodes.Add(node);
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

            int conf = _parentFolder != null
                ? _parentFolder.ConfidentialityLevel
                : (cboConfidentiality.SelectedIndex >= 0 ? cboConfidentiality.SelectedIndex + 1 : 4);

            // ถ้ามี parentFolder → สร้างภายใต้ folder นั้น (ไม่ต้องเลือก OrgUnit)
            if (_parentFolder != null)
            {
                try
                {
                    btnApply.Enabled = false;
                    btnApply.Text    = "⏳  กำลังสร้าง...";
                    lblStatus.Text   = "⏳ กำลังสร้าง Folder...";
                    Application.DoEvents();

                    // สร้างภายใต้ parentFolder โดยตรง
                    _templateController.ApplyTemplateToFolder(
                        _parentFolder.FolderId,
                        tmpl.TemplateID,
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

            // ไม่มี parentFolder → ใช้ OrgUnit (legacy mode)
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
                    ou.OrgUnitId, tmpl.TemplateID,
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
            return (idx >= 0 && idx < _templates.Count) ? _templates[idx] : null;
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
                return;
            }

            if (_parentFolder != null)
            {
                lblPathBreadcrumb.Text    = "📍 " + _parentFolder.FolderName;
                lblPathBreadcrumb.Visible = true;
                return;
            }

            lblPathBreadcrumb.Visible = false;
        }
    }
}
