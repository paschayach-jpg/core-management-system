using System;
using System.Collections.Generic;
using System.Drawing;
using System.IO;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.DMS
{
    public partial class OrgMasterManagementForm : Form
    {
        private readonly IOrgMasterController _controller;
        private readonly UserContext _user;

        private List<HrDivision> _divisions = new List<HrDivision>();
        private List<HrDepartment> _departments = new List<HrDepartment>();
        private List<HrPosition> _positions = new List<HrPosition>();
        private List<HrPositionLevel> _levels = new List<HrPositionLevel>();
        private List<HrPositionMapping> _mappings = new List<HrPositionMapping>();

        public OrgMasterManagementForm(IOrgMasterController controller, UserContext user)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _user = user ?? throw new ArgumentNullException(nameof(user));

            InitializeComponent();
            SetupColumns();
            this.Load += (s, e) => LoadAllData();
        }

        private void SetupColumns()
        {
            // Division Grid
            dgvDivision.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDivCode", HeaderText = "รหัสฝ่าย (DivisionCode)", Width = 160 });
            dgvDivision.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDivName", HeaderText = "ชื่อฝ่าย (DivisionName)", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvDivision.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDivActive", HeaderText = "สถานะ", Width = 90 });
            dgvDivision.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDivDate", HeaderText = "วันที่สร้าง", Width = 140 });

            // Department Grid
            dgvDepartment.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDeptDiv", HeaderText = "ฝ่าย (Division)", Width = 160 });
            dgvDepartment.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDeptCode", HeaderText = "รหัสแผนก (Code)", Width = 130 });
            dgvDepartment.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDeptNameTh", HeaderText = "ชื่อแผนกภาษาไทย", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvDepartment.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDeptNameEn", HeaderText = "ชื่อแผนกภาษาอังกฤษ", Width = 200 });
            dgvDepartment.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDeptActive", HeaderText = "สถานะ", Width = 80 });

            // Position Grid
            dgvPosition.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPosCode", HeaderText = "รหัสตำแหน่ง", Width = 130 });
            dgvPosition.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPosName", HeaderText = "ชื่อตำแหน่ง (PositionName)", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvPosition.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPosLevel", HeaderText = "ระดับ (Level)", Width = 100 });
            dgvPosition.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPosActive", HeaderText = "สถานะ", Width = 80 });

            // Level Grid
            dgvLevel.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLvlCode", HeaderText = "รหัสระดับ (LevelCode)", Width = 160 });
            dgvLevel.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLvlName", HeaderText = "ชื่อระดับ (LevelName)", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvLevel.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLvlCategory", HeaderText = "หมวดหมู่ (Category)", Width = 180 });
            dgvLevel.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLvlSort", HeaderText = "ลำดับ (Sort)", Width = 90 });

            // Mapping Grid
            dgvMapping.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMapDiv", HeaderText = "ฝ่าย (Division)", Width = 160 });
            dgvMapping.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMapDept", HeaderText = "แผนก (Department)", Width = 180 });
            dgvMapping.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMapPos", HeaderText = "ตำแหน่ง (Position)", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
            dgvMapping.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMapLevel", HeaderText = "ระดับ (Level)", Width = 90 });
            dgvMapping.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMapHc", HeaderText = "กรอบอัตรา", Width = 80 });
            dgvMapping.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMapActive", HeaderText = "สถานะ", Width = 80 });
        }

        private void LoadAllData()
        {
            try
            {
                _divisions = _controller.GetAllDivisions().ToList();
                _departments = _controller.GetAllDepartments().ToList();
                _positions = _controller.GetAllPositions().ToList();
                _levels = _controller.GetAllLevels().ToList();
                _mappings = _controller.GetAllMappings().ToList();

                PopulateDivFilters();

                FilterDivisionGrid();
                FilterDepartmentGrid();
                FilterPositionGrid();
                FilterLevelGrid();
                FilterMappingGrid();
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถโหลดข้อมูล Master ได้: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void PopulateDivFilters()
        {
            cboFilterDivOnDept.Items.Clear();
            cboFilterDivOnDept.Items.Add(new FilterItem { Id = 0, Text = "— ทุกฝ่าย —" });
            foreach (var d in _divisions.OrderBy(x => x.DivisionCode))
                cboFilterDivOnDept.Items.Add(new FilterItem { Id = d.DivisionID, Text = $"[{d.DivisionCode}] {d.DivisionName}" });
            cboFilterDivOnDept.SelectedIndex = 0;

            cboFilterDivOnMap.Items.Clear();
            cboFilterDivOnMap.Items.Add(new FilterItem { Id = 0, Text = "— ทุกฝ่าย —" });
            foreach (var d in _divisions.OrderBy(x => x.DivisionCode))
                cboFilterDivOnMap.Items.Add(new FilterItem { Id = d.DivisionID, Text = $"[{d.DivisionCode}] {d.DivisionName}" });
            cboFilterDivOnMap.SelectedIndex = 0;
        }

        #region Filter and Render Grids

        private void FilterDivisionGrid()
        {
            string search = txtSearchDiv.Text.Trim();
            dgvDivision.Rows.Clear();

            var query = _divisions.AsEnumerable();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(d => (d.DivisionCode?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                         (d.DivisionName?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0));

            foreach (var d in query.OrderBy(x => x.DivisionCode))
            {
                int r = dgvDivision.Rows.Add(d.DivisionCode, d.DivisionName, d.IsActive ? "ใช้งาน" : "ปิด", d.CreatedDate.ToString("yyyy-MM-dd HH:mm"));
                dgvDivision.Rows[r].Tag = d;
            }
        }

        private void FilterDepartmentGrid()
        {
            string search = txtSearchDept.Text.Trim();
            var filterDiv = cboFilterDivOnDept.SelectedItem as FilterItem;
            dgvDepartment.Rows.Clear();

            var divMap = _divisions.ToDictionary(d => d.DivisionID, d => $"[{d.DivisionCode}] {d.DivisionName}");

            var query = _departments.AsEnumerable();
            if (filterDiv != null && filterDiv.Id > 0)
                query = query.Where(d => d.DivisionID == filterDiv.Id);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(d => (d.DepartmentCode?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                         (d.DepartmentNameTh?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                         (d.DepartmentNameEn?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0));

            foreach (var d in query.OrderBy(x => x.DepartmentCode))
            {
                divMap.TryGetValue(d.DivisionID, out string divName);
                int r = dgvDepartment.Rows.Add(divName ?? "-", d.DepartmentCode, d.DepartmentNameTh ?? d.DepartmentName, d.DepartmentNameEn, d.IsActive ? "ใช้งาน" : "ปิด");
                dgvDepartment.Rows[r].Tag = d;
            }
        }

        private void FilterPositionGrid()
        {
            string search = txtSearchPos.Text.Trim();
            dgvPosition.Rows.Clear();

            var query = _positions.AsEnumerable();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(p => (p.PositionCode?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                         (p.PositionName?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                         (p.LevelCode?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0));

            foreach (var p in query.OrderBy(x => x.PositionName))
            {
                int r = dgvPosition.Rows.Add(p.PositionCode, p.PositionName, p.LevelCode, p.IsActive ? "ใช้งาน" : "ปิด");
                dgvPosition.Rows[r].Tag = p;
            }
        }

        private void FilterLevelGrid()
        {
            string search = txtSearchLevel.Text.Trim();
            dgvLevel.Rows.Clear();

            var query = _levels.AsEnumerable();
            if (!string.IsNullOrEmpty(search))
                query = query.Where(l => (l.LevelCode?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                         (l.LevelName?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                         (l.LevelCategory?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0));

            foreach (var l in query.OrderBy(x => x.SortOrder ?? 999).ThenBy(x => x.LevelCode))
            {
                int r = dgvLevel.Rows.Add(l.LevelCode, l.LevelName, l.LevelCategory, l.SortOrder);
                dgvLevel.Rows[r].Tag = l;
            }
        }

        private void FilterMappingGrid()
        {
            string search = txtSearchMap.Text.Trim();
            var filterDiv = cboFilterDivOnMap.SelectedItem as FilterItem;
            dgvMapping.Rows.Clear();

            var query = _mappings.AsEnumerable();
            if (filterDiv != null && filterDiv.Id > 0)
                query = query.Where(m => m.DivisionID == filterDiv.Id);

            if (!string.IsNullOrEmpty(search))
                query = query.Where(m => (m.DivisionName?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                         (m.DepartmentName?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0) ||
                                         (m.PositionName?.IndexOf(search, StringComparison.OrdinalIgnoreCase) >= 0));

            foreach (var m in query.OrderBy(x => x.DivisionCode).ThenBy(x => x.DepartmentCode).ThenBy(x => x.PositionName))
            {
                string divStr = $"[{m.DivisionCode}] {m.DivisionName}";
                string deptStr = $"[{m.DepartmentCode}] {m.DepartmentName}";
                int r = dgvMapping.Rows.Add(divStr, deptStr, m.PositionName, m.Level, m.HeadCount, m.IsActive ? "ใช้งาน" : "ปิด");
                dgvMapping.Rows[r].Tag = m;
            }

            BuildMappingTree(query);
        }

        private void BuildMappingTree(IEnumerable<HrPositionMapping> mappings)
        {
            var deptById = _departments.ToDictionary(d => d.DepartmentID);
            bool expandAll = !string.IsNullOrEmpty(txtSearchMap.Text.Trim());

            tvMapping.BeginUpdate();
            try
            {
                tvMapping.Nodes.Clear();

                foreach (var divGroup in mappings.GroupBy(m => m.DivisionID)
                                                 .OrderBy(g => g.First().DivisionCode))
                {
                    var first = divGroup.First();
                    var divNode = new TreeNode($"🏢 [{first.DivisionCode}] {first.DivisionName}  ({divGroup.Count()} ตำแหน่ง)")
                    {
                        NodeFont = new Font(tvMapping.Font, FontStyle.Bold),
                        ForeColor = Color.FromArgb(31, 56, 100)
                    };

                    foreach (var deptGroup in divGroup.GroupBy(m => m.DepartmentID ?? 0)
                                                      .OrderBy(g => g.First().DepartmentCode))
                    {
                        TreeNodeCollection posParent;
                        if (deptGroup.Key == 0)
                        {
                            posParent = divNode.Nodes;
                        }
                        else
                        {
                            var dm = deptGroup.First();
                            string deptName = dm.DepartmentName;
                            HrDepartment dept;
                            if (string.IsNullOrEmpty(deptName) && deptById.TryGetValue(deptGroup.Key, out dept))
                                deptName = dept.DepartmentNameTh ?? dept.DepartmentName;

                            var deptNode = new TreeNode($"📂 [{dm.DepartmentCode}] {deptName}  ({deptGroup.Count()})")
                            {
                                ForeColor = Color.FromArgb(0, 105, 148)
                            };
                            divNode.Nodes.Add(deptNode);
                            posParent = deptNode.Nodes;
                            if (expandAll) deptNode.Expand();
                        }

                        foreach (var m in deptGroup.OrderBy(x => x.PositionName))
                        {
                            string text = "👤 " + m.PositionName;
                            if (!string.IsNullOrEmpty(m.Level)) text += "  · ระดับ " + m.Level;
                            if (m.HeadCount > 0) text += "  · กรอบ " + m.HeadCount;
                            var posNode = new TreeNode(text) { Tag = m };
                            if (!m.IsActive)
                            {
                                posNode.Text += "  (ปิด)";
                                posNode.ForeColor = Color.Gray;
                            }
                            posParent.Add(posNode);
                        }
                    }

                    tvMapping.Nodes.Add(divNode);
                    divNode.Expand();
                }
            }
            finally
            {
                tvMapping.EndUpdate();
            }
        }

        private HrPositionMapping GetSelectedMapping()
        {
            if (chkMapTreeView.Checked)
                return tvMapping.SelectedNode != null ? tvMapping.SelectedNode.Tag as HrPositionMapping : null;
            return dgvMapping.SelectedRows.Count > 0 ? dgvMapping.SelectedRows[0].Tag as HrPositionMapping : null;
        }

        private void chkMapTreeView_CheckedChanged(object sender, EventArgs e)
        {
            tvMapping.Visible = chkMapTreeView.Checked;
            dgvMapping.Visible = !chkMapTreeView.Checked;
            chkMapTreeView.Text = chkMapTreeView.Checked ? "📋 มุมมองแบบตาราง" : "🌳 มุมมองแบบ Tree";
        }

        private void tvMapping_NodeMouseDoubleClick(object sender, TreeNodeMouseClickEventArgs e)
        {
            if (e.Node.Tag is HrPositionMapping)
                btnEditMap_Click(sender, e);
        }

        #endregion

        #region CRUD Actions - Division

        private void btnAddDiv_Click(object sender, EventArgs e)
        {
            using (var dlg = new OrgMasterEditDialog(OrgMasterType.Division, false, null))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.ResultItem is HrDivision div)
                {
                    _controller.SaveDivision(div, _user.UserID);
                    LoadAllData();
                }
            }
        }

        private void btnEditDiv_Click(object sender, EventArgs e)
        {
            if (dgvDivision.SelectedRows.Count == 0) return;
            var div = dgvDivision.SelectedRows[0].Tag as HrDivision;
            if (div == null) return;

            using (var dlg = new OrgMasterEditDialog(OrgMasterType.Division, true, div))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.ResultItem is HrDivision updated)
                {
                    updated.DivisionID = div.DivisionID;
                    _controller.SaveDivision(updated, _user.UserID);
                    LoadAllData();
                }
            }
        }

        private void btnDeleteDiv_Click(object sender, EventArgs e)
        {
            if (dgvDivision.SelectedRows.Count == 0) return;
            var div = dgvDivision.SelectedRows[0].Tag as HrDivision;
            if (div == null) return;

            if (MessageBox.Show($"ยืนยันการลบฝ่าย '{div.DivisionName}' หรือไม่?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _controller.DeleteDivision(div.DivisionID, _user.UserID);
                LoadAllData();
            }
        }

        #endregion

        #region CRUD Actions - Department

        private void btnAddDept_Click(object sender, EventArgs e)
        {
            using (var dlg = new OrgMasterEditDialog(OrgMasterType.Department, false, null, _divisions))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.ResultItem is HrDepartment dept)
                {
                    _controller.SaveDepartment(dept, _user.UserID);
                    LoadAllData();
                }
            }
        }

        private void btnEditDept_Click(object sender, EventArgs e)
        {
            if (dgvDepartment.SelectedRows.Count == 0) return;
            var dept = dgvDepartment.SelectedRows[0].Tag as HrDepartment;
            if (dept == null) return;

            using (var dlg = new OrgMasterEditDialog(OrgMasterType.Department, true, dept, _divisions))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.ResultItem is HrDepartment updated)
                {
                    updated.DepartmentID = dept.DepartmentID;
                    _controller.SaveDepartment(updated, _user.UserID);
                    LoadAllData();
                }
            }
        }

        private void btnDeleteDept_Click(object sender, EventArgs e)
        {
            if (dgvDepartment.SelectedRows.Count == 0) return;
            var dept = dgvDepartment.SelectedRows[0].Tag as HrDepartment;
            if (dept == null) return;

            if (MessageBox.Show($"ยืนยันการลบแผนก '{dept.DepartmentNameTh}' หรือไม่?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _controller.DeleteDepartment(dept.DepartmentID, _user.UserID);
                LoadAllData();
            }
        }

        #endregion

        #region CRUD Actions - Position

        private void btnAddPos_Click(object sender, EventArgs e)
        {
            using (var dlg = new OrgMasterEditDialog(OrgMasterType.Position, false, null, null, null, null, _levels))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.ResultItem is HrPosition pos)
                {
                    _controller.SavePosition(pos, _user.UserID);
                    LoadAllData();
                }
            }
        }

        private void btnEditPos_Click(object sender, EventArgs e)
        {
            if (dgvPosition.SelectedRows.Count == 0) return;
            var pos = dgvPosition.SelectedRows[0].Tag as HrPosition;
            if (pos == null) return;

            using (var dlg = new OrgMasterEditDialog(OrgMasterType.Position, true, pos, null, null, null, _levels))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.ResultItem is HrPosition updated)
                {
                    updated.PositionID = pos.PositionID;
                    _controller.SavePosition(updated, _user.UserID);
                    LoadAllData();
                }
            }
        }

        private void btnDeletePos_Click(object sender, EventArgs e)
        {
            if (dgvPosition.SelectedRows.Count == 0) return;
            var pos = dgvPosition.SelectedRows[0].Tag as HrPosition;
            if (pos == null) return;

            if (MessageBox.Show($"ยืนยันการลบตำแหน่ง '{pos.PositionName}' หรือไม่?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _controller.DeletePosition(pos.PositionID, _user.UserID);
                LoadAllData();
            }
        }

        #endregion

        #region CRUD Actions - Level

        private void btnAddLevel_Click(object sender, EventArgs e)
        {
            using (var dlg = new OrgMasterEditDialog(OrgMasterType.Level, false, null))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.ResultItem is HrPositionLevel lvl)
                {
                    _controller.SaveLevel(lvl, _user.UserID);
                    LoadAllData();
                }
            }
        }

        private void btnEditLevel_Click(object sender, EventArgs e)
        {
            if (dgvLevel.SelectedRows.Count == 0) return;
            var lvl = dgvLevel.SelectedRows[0].Tag as HrPositionLevel;
            if (lvl == null) return;

            using (var dlg = new OrgMasterEditDialog(OrgMasterType.Level, true, lvl))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.ResultItem is HrPositionLevel updated)
                {
                    _controller.SaveLevel(updated, _user.UserID);
                    LoadAllData();
                }
            }
        }

        private void btnDeleteLevel_Click(object sender, EventArgs e)
        {
            if (dgvLevel.SelectedRows.Count == 0) return;
            var lvl = dgvLevel.SelectedRows[0].Tag as HrPositionLevel;
            if (lvl == null) return;

            if (MessageBox.Show($"ยืนยันการลบระดับ '{lvl.LevelCode}' หรือไม่?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _controller.DeleteLevel(lvl.LevelCode, _user.UserID);
                LoadAllData();
            }
        }

        #endregion

        #region CRUD Actions - Mapping

        private void btnAddMap_Click(object sender, EventArgs e)
        {
            using (var dlg = new OrgMasterEditDialog(OrgMasterType.Mapping, false, null, _divisions, _departments, _positions, _levels))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.ResultItem is HrPositionMapping map)
                {
                    _controller.SaveMapping(map, _user.UserID);
                    LoadAllData();
                }
            }
        }

        private void btnEditMap_Click(object sender, EventArgs e)
        {
            var map = GetSelectedMapping();
            if (map == null) return;

            using (var dlg = new OrgMasterEditDialog(OrgMasterType.Mapping, true, map, _divisions, _departments, _positions, _levels))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.ResultItem is HrPositionMapping updated)
                {
                    updated.MappingID = map.MappingID;
                    _controller.SaveMapping(updated, _user.UserID);
                    LoadAllData();
                }
            }
        }

        private void btnDeleteMap_Click(object sender, EventArgs e)
        {
            var map = GetSelectedMapping();
            if (map == null) return;

            if (MessageBox.Show($"ยืนยันการลบผังตำแหน่ง '{map.PositionName}' ในแผนก '{map.DepartmentName}' หรือไม่?", "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question) == DialogResult.Yes)
            {
                _controller.DeleteMapping(map.MappingID, _user.UserID);
                LoadAllData();
            }
        }

        #endregion

        #region Toolbar Actions

        private void btnImportExcel_Click(object sender, EventArgs e)
        {
            using (var dlg = new OrgMasterImportDialog(_controller, _user))
            {
                if (dlg.ShowDialog(this) == DialogResult.OK && dlg.HasImported)
                {
                    LoadAllData();
                }
            }
        }

        private void btnDownloadTemplate_Click(object sender, EventArgs e)
        {
            using (var sfd = new SaveFileDialog
            {
                Filter = "Excel Files (*.xlsx)|*.xlsx",
                FileName = "Template_MasterData_EMS.xlsx",
                Title = "บันทึกไฟล์ Excel Template"
            })
            {
                if (sfd.ShowDialog(this) == DialogResult.OK)
                {
                    try
                    {
                        byte[] bytes = _controller.GenerateExcelTemplate();
                        File.WriteAllBytes(sfd.FileName, bytes);
                        MessageBox.Show("ดาวน์โหลด Template เรียบร้อยแล้ว:\n" + sfd.FileName, "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show("ไม่สามารถดาวน์โหลด Template ได้: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            }
        }

        private void btnRefresh_Click(object sender, EventArgs e)
        {
            LoadAllData();
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        #endregion

        private class FilterItem
        {
            public int Id { get; set; }
            public string Text { get; set; }
            public override string ToString() => Text;
        }
    }
}
