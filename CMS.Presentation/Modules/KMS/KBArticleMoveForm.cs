// ✅ Allowed usings: CMS.Controllers, CMS.Domain
// ❌ Forbidden usings: CMS.Services, CMS.Data
using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.KMS.KB;
using CMS.Controllers.KMS.KB.Interfaces;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.KMS
{
    /// <summary>
    /// หน้าย้ายบทความ Knowledge Base (MOD-006) ไปหมวดที่มีอยู่แล้ว — ① เลือกต้นทาง (ทั้งหมวด หรือรายบทความ)
    /// ② เลือกหมวดปลายทาง ③ ดูสรุป/คำเตือนแล้วกดย้าย. เปลี่ยนเฉพาะหมวดของบทความ ไม่แก้เนื้อหา/version.
    /// เปิดจากปุ่ม "ย้ายบทความ" ในหัวฟอร์ม KBMainForm
    /// </summary>
    public partial class KBArticleMoveForm : Form, IKBArticleMoveController
    {
        private KBArticleMoveController _presenter;
        private readonly UserContext _user;

        private IList<KBCategory> _categories = new List<KBCategory>();
        private bool _binding;   // กำลังสร้างต้นไม้/ตาราง — ไม่ให้ event ย้อนกลับไปโหลดซ้ำ

        public event EventHandler BackRequested;

        public KBArticleMoveForm(UserContext user)
        {
            if (user == null) throw new ArgumentNullException("user");
            _user = user;
            InitializeComponent();
        }

        public void AttachPresenter(KBArticleMoveController presenter)
        {
            if (presenter == null) throw new ArgumentNullException("presenter");
            _presenter = presenter;
        }

        // ── Form events ──────────────────────────────────────────────────────

        private void KBArticleMoveForm_Load(object sender, EventArgs e)
        {
            if (_presenter == null)
            {
                MessageBox.Show(
                    "Presenter ยังไม่ได้ถูก Attach\n\nกรุณาเรียก AttachPresenter() ก่อนเปิดฟอร์ม",
                    "ข้อผิดพลาดภายใน", MessageBoxButtons.OK, MessageBoxIcon.Error);
                Close();
                return;
            }
            ApplySourceModeUi();
            _presenter.Initialize();
        }

        private void BtnBack_Click(object sender, EventArgs e)
        {
            if (BackRequested != null)
                BackRequested(this, EventArgs.Empty);
        }

        private void BtnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void BtnMove_Click(object sender, EventArgs e)
        {
            if (_presenter == null) return;
            _presenter.OnMoveClicked();
        }

        private void RbSourceMode_CheckedChanged(object sender, EventArgs e)
        {
            // radio 2 ตัวยิงพร้อมกัน — ทำเฉพาะตัวที่ถูกเลือก
            if (_presenter == null || _binding || !((RadioButton)sender).Checked) return;
            ApplySourceModeUi();
            _presenter.RefreshSource();
        }

        private void ChkIncludeSub_CheckedChanged(object sender, EventArgs e)
        {
            if (_presenter == null || _binding) return;
            _presenter.RefreshSource();
        }

        private void TreeSource_AfterCheck(object sender, TreeViewEventArgs e)
        {
            if (_presenter == null || _binding || !rbWholeCategory.Checked) return;
            _presenter.RefreshSource();
        }

        private void TreeSource_AfterSelect(object sender, TreeViewEventArgs e)
        {
            if (_presenter == null || _binding || !rbIndividual.Checked) return;
            _presenter.RefreshSource();
        }

        private void TreeDestination_AfterSelect(object sender, TreeViewEventArgs e)
        {
            var cat = e.Node == null ? null : e.Node.Tag as KBCategory;
            lblDestPath.Text = cat == null
                ? "ปลายทางที่เลือก: —"
                : "ปลายทางที่เลือก: " + KBCategoryTree.GetPath(_categories, cat.CategoryID, " › ");
            if (_presenter == null || _binding) return;
            _presenter.RefreshSummary();
        }

        private void DgvArticles_CurrentCellDirtyStateChanged(object sender, EventArgs e)
        {
            // ให้ติ๊ก checkbox แล้วมีผลทันที (ไม่ต้องรอออกจากเซลล์)
            if (dgvArticles.IsCurrentCellDirty)
                dgvArticles.CommitEdit(DataGridViewDataErrorContexts.Commit);
        }

        private void DgvArticles_CellValueChanged(object sender, DataGridViewCellEventArgs e)
        {
            if (_presenter == null || _binding || e.RowIndex < 0 || e.ColumnIndex != colSelect.Index) return;
            UpdateArticleCount();
            _presenter.RefreshSummary();
        }

        private void BtnSelectAll_Click(object sender, EventArgs e)
        {
            SetAllChecked(true);
        }

        private void BtnSelectNone_Click(object sender, EventArgs e)
        {
            SetAllChecked(false);
        }

        // ── helpers ──────────────────────────────────────────────────────────

        /// <summary>โหมดทั้งหมวด = ต้นไม้มีช่องติ๊ก + ตารางอ่านอย่างเดียว; โหมดรายบทความ = คลิกหมวด + ติ๊กในตาราง</summary>
        private void ApplySourceModeUi()
        {
            bool whole = rbWholeCategory.Checked;
            _binding = true;
            try
            {
                treeSource.CheckBoxes = whole;
                colSelect.ReadOnly = whole;
                btnSelectAll.Enabled = !whole;
                btnSelectNone.Enabled = !whole;
            }
            finally { _binding = false; }
        }

        private void SetAllChecked(bool value)
        {
            if (_presenter == null || rbWholeCategory.Checked) return;
            _binding = true;
            try
            {
                foreach (DataGridViewRow row in dgvArticles.Rows)
                    row.Cells[colSelect.Index].Value = value;
            }
            finally { _binding = false; }
            UpdateArticleCount();
            _presenter.RefreshSummary();
        }

        private void UpdateArticleCount()
        {
            int total = dgvArticles.Rows.Count;
            int selected = 0;
            foreach (DataGridViewRow row in dgvArticles.Rows)
            {
                if (row.Cells[colSelect.Index].Value is bool && (bool)row.Cells[colSelect.Index].Value)
                    selected++;
            }
            lblArticleCount.Text = total == 0
                ? "ไม่พบบทความในหมวดที่เลือก"
                : (rbWholeCategory.Checked
                    ? string.Format("บทความทั้งหมด {0} รายการ", total)
                    : string.Format("เลือกแล้ว {0} จาก {1} รายการ", selected, total));
        }

        private void BuildTree(TreeView tree, IList<KBCategory> categories, bool activeOnly)
        {
            tree.BeginUpdate();
            tree.Nodes.Clear();

            var lastAtDepth = new Dictionary<int, TreeNode>();
            foreach (var item in KBCategoryTree.Flatten(categories, activeOnly))
            {
                var cat = item.Category;
                string code = (cat.CategoryCode ?? string.Empty).Trim();
                string text = code.Length > 0 ? string.Format("{0} ({1})", cat.CategoryName, code) : cat.CategoryName;
                if (!cat.IsActive) text += " [ปิดใช้งาน]";

                var node = new TreeNode(text) { Tag = cat };
                if (!cat.IsActive) node.ForeColor = System.Drawing.Color.Gray;

                TreeNode parent;
                if (item.Depth > 0 && lastAtDepth.TryGetValue(item.Depth - 1, out parent))
                    parent.Nodes.Add(node);
                else
                    tree.Nodes.Add(node);
                lastAtDepth[item.Depth] = node;
            }

            tree.CollapseAll();
            foreach (TreeNode root in tree.Nodes) root.Expand();
            tree.EndUpdate();
        }

        private static void CollectChecked(TreeNodeCollection nodes, List<int> result)
        {
            foreach (TreeNode n in nodes)
            {
                var cat = n.Tag as KBCategory;
                if (n.Checked && cat != null) result.Add(cat.CategoryID);
                CollectChecked(n.Nodes, result);
            }
        }

        // ── IKBArticleMoveController ─────────────────────────────────────────

        public bool SourceWholeCategory { get { return rbWholeCategory.Checked; } }

        public bool IncludeSubCategories { get { return chkIncludeSub.Checked; } }

        public IList<int> SelectedSourceCategoryIds
        {
            get
            {
                var result = new List<int>();
                if (rbWholeCategory.Checked)
                {
                    CollectChecked(treeSource.Nodes, result);
                }
                else
                {
                    var cat = treeSource.SelectedNode == null ? null : treeSource.SelectedNode.Tag as KBCategory;
                    if (cat != null) result.Add(cat.CategoryID);
                }
                return result;
            }
        }

        public IList<int> CheckedArticleIds
        {
            get
            {
                var result = new List<int>();
                foreach (DataGridViewRow row in dgvArticles.Rows)
                {
                    var article = row.Tag as KBArticle;
                    var v = row.Cells[colSelect.Index].Value;
                    if (article != null && v is bool && (bool)v) result.Add(article.ArticleID);
                }
                return result;
            }
        }

        public int? DestinationCategoryId
        {
            get
            {
                var cat = treeDestination.SelectedNode == null ? null : treeDestination.SelectedNode.Tag as KBCategory;
                return cat == null ? (int?)null : cat.CategoryID;
            }
        }

        public void BindCategories(IList<KBCategory> allCategories)
        {
            _binding = true;
            try
            {
                _categories = allCategories ?? new List<KBCategory>();
                BuildTree(treeSource, _categories, false);        // ต้นทาง: รวมหมวดที่ปิดใช้งาน (อาจมีบทความค้างอยู่)
                BuildTree(treeDestination, _categories, true);    // ปลายทาง: เฉพาะหมวดที่เปิดใช้งาน
                treeDestination.SelectedNode = null;
                lblDestPath.Text = "ปลายทางที่เลือก: —";
            }
            finally { _binding = false; }
        }

        public void BindSourceArticles(IList<KBArticle> articles, bool checkAll)
        {
            _binding = true;
            try
            {
                dgvArticles.SuspendLayout();
                dgvArticles.Rows.Clear();
                foreach (var a in articles ?? new List<KBArticle>())
                {
                    int idx = dgvArticles.Rows.Add(checkAll, a.ArticleCode, a.Title, a.TitleCategory, a.StatusName, a.CategoryName);
                    dgvArticles.Rows[idx].Tag = a;
                }
                dgvArticles.ResumeLayout();
                UpdateArticleCount();
            }
            finally { _binding = false; }
        }

        public void ShowSummary(string summary, IList<string> warnings, bool canMove)
        {
            lblSummary.Text = summary ?? string.Empty;
            txtWarnings.Text = (warnings == null || warnings.Count == 0)
                ? string.Empty
                : string.Join(Environment.NewLine, warnings.Select(w => "⚠ " + w));
            btnMove.Enabled = canMove;
        }

        public bool ConfirmMove(string message)
        {
            return MessageBox.Show(message, "ยืนยันการย้ายบทความ",
                MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes;
        }

        public void ShowError(string message)
        {
            MessageBox.Show(message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        public void ShowSuccess(string message)
        {
            MessageBox.Show(message, "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
        }
    }
}
