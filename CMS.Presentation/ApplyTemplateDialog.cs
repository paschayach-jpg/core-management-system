using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Presentation
{
    public partial class ApplyTemplateDialog : Form
    {
        private readonly IFolderService            _folderService;
        private readonly IFolderTemplateRepository _templateRepository;
        private readonly IOrgStructureService      _orgStructureService;
        private readonly UserContext               _currentUser;

        private List<FolderTemplate> _templates = new List<FolderTemplate>();
        private List<OrgUnit>        _orgUnits  = new List<OrgUnit>();

        public ApplyTemplateDialog(
            IFolderService            folderService,
            IFolderTemplateRepository templateRepository,
            IOrgStructureService      orgStructureService,
            UserContext               currentUser)
        {
            if (folderService       == null) throw new ArgumentNullException("folderService");
            if (templateRepository  == null) throw new ArgumentNullException("templateRepository");
            if (orgStructureService == null) throw new ArgumentNullException("orgStructureService");
            if (currentUser         == null) throw new ArgumentNullException("currentUser");

            _folderService       = folderService;
            _templateRepository  = templateRepository;
            _orgStructureService = orgStructureService;
            _currentUser         = currentUser;

            InitializeComponent();
        }

        // ── Form Load ─────────────────────────────────────────────────────────
        private void ApplyTemplateDialog_Load(object sender, EventArgs e)
        {
            // ต้องรอให้ layout เสร็จก่อน จึงใช้ BeginInvoke
            BeginInvoke(new Action(() => {
                if (splitMain.Width > splitMain.Panel1MinSize + splitMain.Panel2MinSize)
                    splitMain.SplitterDistance = 280;
            }));
            LoadData();
        }

        // ── Data loading ──────────────────────────────────────────────────────
        private void LoadData()
        {
            try
            {
                lblStatus.Text = "⏳ กำลังโหลดข้อมูล...";

                _orgUnits = _orgStructureService.GetHierarchy()
                    .Where(u => u.IsActive && !u.IsDeleted)
                    .OrderBy(u => u.OrgUnitType).ThenBy(u => u.Name)
                    .ToList();

                cboOrgUnit.Items.Clear();
                foreach (var ou in _orgUnits)
                {
                    string icon = ou.OrgUnitType == 1 ? "🏢" : ou.OrgUnitType == 2 ? "🏬" : "🏪";
                    cboOrgUnit.Items.Add(string.Format("{0}  {1}", icon, ou.Name));
                }
                if (cboOrgUnit.Items.Count > 0) cboOrgUnit.SelectedIndex = 0;

                _templates = _templateRepository.GetAll()
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

        private void CboOrgUnit_Changed(object sender, EventArgs e)
        {
            var ou = GetSelectedOrgUnit();
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
                var items = _templateRepository.GetItems(tmpl.TemplateID).ToList();
                treePreview.BeginUpdate();
                treePreview.Nodes.Clear();

                var root = new TreeNode(string.Format("📁  {0}", txtRootName.Text.Trim()));
                root.ForeColor = Color.FromArgb(31, 56, 100);
                root.NodeFont  = new Font("Segoe UI", 9.5f, FontStyle.Bold);
                BuildPreviewNodes(root.Nodes, items, null);
                treePreview.Nodes.Add(root);
                treePreview.ExpandAll();
                treePreview.EndUpdate();

                lblStatus.Text = string.Format("Template: {0}  |  {1} folder(s)", tmpl.TemplateName, items.Count);
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
            var ou   = GetSelectedOrgUnit();
            var tmpl = GetSelectedTemplate();

            if (ou == null)   { MessageBox.Show("กรุณาเลือกหน่วยงาน", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (tmpl == null) { MessageBox.Show("กรุณาเลือก Template", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning); return; }
            if (string.IsNullOrWhiteSpace(txtRootName.Text))
            {
                MessageBox.Show("กรุณาระบุชื่อ Root Folder", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            int conf = cboConfidentiality.SelectedIndex + 1;

            if (MessageBox.Show(
                string.Format("ยืนยันการสร้าง Folder?\n\nหน่วยงาน :  {0}\nTemplate  :  {1}\nRoot Folder :  {2}\nระดับความลับ :  {3}",
                    ou.Name, tmpl.TemplateName, txtRootName.Text.Trim(), cboConfidentiality.Text.Trim()),
                "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                btnApply.Enabled = false;
                btnApply.Text    = "⏳  กำลังสร้าง...";
                lblStatus.Text   = "⏳ กำลังสร้าง Folder...";
                Application.DoEvents();

                _folderService.ApplyTemplateToOrgUnit(
                    ou.OrgUnitId, tmpl.TemplateID,
                    txtRootName.Text.Trim(), conf, _currentUser);

                lblStatus.Text      = "✅ สร้าง Folder เรียบร้อยแล้ว";
                lblStatus.ForeColor = Color.FromArgb(0, 120, 0);

                MessageBox.Show(
                    string.Format("✅ สร้าง Folder เรียบร้อยแล้ว\n\nRoot: {0}\nTemplate: {1}",
                        txtRootName.Text.Trim(), tmpl.TemplateName),
                    "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);

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
            int idx = cboOrgUnit.SelectedIndex;
            return (idx >= 0 && idx < _orgUnits.Count) ? _orgUnits[idx] : null;
        }

        private FolderTemplate GetSelectedTemplate()
        {
            int idx = lstTemplates.SelectedIndex;
            return (idx >= 0 && idx < _templates.Count) ? _templates[idx] : null;
        }
    }
}
