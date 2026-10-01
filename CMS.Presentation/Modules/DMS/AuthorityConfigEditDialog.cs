using System;
using System.Collections.Generic;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;

namespace CMS.Presentation.Modules.ImportFile
{
    public partial class AuthorityConfigEditDialog : Form
    {
        private static readonly string[] DefaultLevels = new[]
        {
            "", "1A", "1B", "2A", "2B", "3A", "3B", "4A", "4B", "5A", "5B", "6A", "6B", "7A", "7B", "8A", "8B", "9A", "9B"
        };

        private readonly IAuthorityOrgLookupController _orgLookupController;
        private readonly bool _isEdit;

        private List<HrDivision> _allDivisions = new List<HrDivision>();
        private List<HrDepartment> _allDepartments = new List<HrDepartment>();
        private List<HrPosition> _allPositions = new List<HrPosition>();
        private List<HrPositionMapping> _allMappings = new List<HrPositionMapping>();
        private List<HrPositionLevel> _allLevels = new List<HrPositionLevel>();

        private bool _isInitializing = true;

        public FolderAuthorityConfig Config { get; private set; }

        public AuthorityConfigEditDialog(
            FolderAuthorityConfig config,
            bool isEdit,
            IAuthorityOrgLookupController orgLookupController = null,
            IEnumerable<string> availableLevels = null)
        {
            _isEdit = isEdit;
            _orgLookupController = orgLookupController;
            Config = config ?? new FolderAuthorityConfig();

            InitializeComponent();

            this.Text = isEdit ? "แก้ไขสิทธิ์การเข้าถึง" : "เพิ่มสิทธิ์การเข้าถึงใหม่";
            this.lblTitle.Text = this.Text;

            LoadMasterData(availableLevels);
            SetupEventHandlers();

            if (isEdit)
            {
                LoadData();
            }
            else
            {
                _isInitializing = false;
                PopulateDivisions();
                PopulateDepartments(null);
                PopulatePositions(null, null);
                cboLevelCode.SelectedIndex = 0;
            }
        }

        private void LoadMasterData(IEnumerable<string> fallbackLevels)
        {
            if (_orgLookupController != null)
            {
                try { _allDivisions = _orgLookupController.GetDivisions().ToList(); } catch { }
                try { _allDepartments = _orgLookupController.GetDepartments().ToList(); } catch { }
                try { _allPositions = _orgLookupController.GetPositions().ToList(); } catch { }
                try { _allMappings = _orgLookupController.GetPositionMappings().ToList(); } catch { }
                try { _allLevels = _orgLookupController.GetPositionLevels().ToList(); } catch { }
            }

            // Populate Level ComboBox (preserving SortOrder from master)
            var levelList = new List<string>();
            if (_allLevels != null && _allLevels.Count > 0)
            {
                foreach (var pl in _allLevels)
                {
                    if (!string.IsNullOrWhiteSpace(pl.LevelCode) && !levelList.Contains(pl.LevelCode.Trim().ToUpper(), StringComparer.OrdinalIgnoreCase))
                        levelList.Add(pl.LevelCode.Trim().ToUpper());
                }
            }
            else if (fallbackLevels != null && fallbackLevels.Any())
            {
                foreach (var lv in fallbackLevels)
                {
                    if (!string.IsNullOrWhiteSpace(lv) && !levelList.Contains(lv.Trim().ToUpper(), StringComparer.OrdinalIgnoreCase))
                        levelList.Add(lv.Trim().ToUpper());
                }
            }
            else
            {
                foreach (var lv in DefaultLevels)
                {
                    if (!string.IsNullOrWhiteSpace(lv) && !levelList.Contains(lv.Trim().ToUpper(), StringComparer.OrdinalIgnoreCase))
                        levelList.Add(lv.Trim().ToUpper());
                }
            }

            if (!string.IsNullOrWhiteSpace(Config.LevelCode))
            {
                string cur = Config.LevelCode.Trim().ToUpper();
                if (!levelList.Contains(cur, StringComparer.OrdinalIgnoreCase))
                    levelList.Add(cur);
            }

            cboLevelCode.Items.Clear();
            cboLevelCode.Items.Add(""); // ตัวเลือกว่าง (ไม่ระบุ)
            foreach (var lv in levelList)
            {
                cboLevelCode.Items.Add(lv);
            }
        }

        private void SetupEventHandlers()
        {
            cboDiv.SelectedIndexChanged += cboDiv_SelectedIndexChanged;
            cboDept.SelectedIndexChanged += cboDept_SelectedIndexChanged;
            cboPosition.SelectedIndexChanged += cboPosition_SelectedIndexChanged;
        }

        private void PopulateDivisions()
        {
            cboDiv.BeginUpdate();
            cboDiv.Items.Clear();
            cboDiv.Items.Add(new DivComboItem { DivisionID = 0, DivisionCode = "", DivisionName = "(ไม่ระบุ)" });

            foreach (var d in _allDivisions.OrderBy(x => x.DivisionCode))
            {
                cboDiv.Items.Add(new DivComboItem
                {
                    DivisionID = d.DivisionID,
                    DivisionCode = d.DivisionCode ?? "",
                    DivisionName = d.DivisionName ?? ""
                });
            }
            cboDiv.SelectedIndex = 0;
            cboDiv.EndUpdate();
        }

        private void PopulateDepartments(int? divisionId)
        {
            cboDept.BeginUpdate();
            cboDept.Items.Clear();
            cboDept.Items.Add(new DeptComboItem { DepartmentID = 0, DepartmentCode = "", DepartmentName = "(ไม่ระบุ)" });

            var query = _allDepartments.AsEnumerable();
            if (divisionId.HasValue && divisionId.Value > 0)
            {
                query = query.Where(d => d.DivisionID == divisionId.Value);
            }

            foreach (var d in query.OrderBy(x => x.DepartmentCode))
            {
                cboDept.Items.Add(new DeptComboItem
                {
                    DepartmentID = d.DepartmentID,
                    DivisionID = d.DivisionID,
                    DepartmentCode = d.DepartmentCode ?? "",
                    DepartmentName = !string.IsNullOrEmpty(d.DepartmentNameTh) ? d.DepartmentNameTh : d.DepartmentName ?? ""
                });
            }
            cboDept.SelectedIndex = 0;
            cboDept.EndUpdate();
        }

        private void PopulatePositions(int? divisionId, int? departmentId)
        {
            cboPosition.BeginUpdate();
            cboPosition.Items.Clear();
            cboPosition.Items.Add(new PosComboItem { PositionID = 0, PositionCode = "", PositionName = "(ไม่ระบุ)", DefaultLevel = "" });

            // Positions mapped to selected division/dept
            var posNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            var posItems = new List<PosComboItem>();

            if (_allMappings != null && _allMappings.Count > 0)
            {
                var mapQuery = _allMappings.AsEnumerable();
                if (departmentId.HasValue && departmentId.Value > 0)
                    mapQuery = mapQuery.Where(m => m.DepartmentID == departmentId.Value);
                else if (divisionId.HasValue && divisionId.Value > 0)
                    mapQuery = mapQuery.Where(m => m.DivisionID == divisionId.Value);

                foreach (var m in mapQuery.OrderBy(x => x.PositionName))
                {
                    if (!string.IsNullOrWhiteSpace(m.PositionName) && !posNames.Contains(m.PositionName))
                    {
                        posNames.Add(m.PositionName);
                        posItems.Add(new PosComboItem
                        {
                            PositionID = m.PositionID,
                            PositionCode = m.PositionCode ?? "",
                            PositionName = m.PositionName,
                            DefaultLevel = m.Level ?? ""
                        });
                    }
                }
            }

            // Fallback to all positions if no mapped items found or no filter applied
            if (posItems.Count == 0 && _allPositions != null)
            {
                foreach (var p in _allPositions.OrderBy(x => x.PositionName))
                {
                    if (!string.IsNullOrWhiteSpace(p.PositionName) && !posNames.Contains(p.PositionName))
                    {
                        posNames.Add(p.PositionName);
                        posItems.Add(new PosComboItem
                        {
                            PositionID = p.PositionID,
                            PositionCode = p.PositionCode ?? "",
                            PositionName = p.PositionName,
                            DefaultLevel = !string.IsNullOrEmpty(p.LevelCode) ? p.LevelCode : p.PositionLevel ?? ""
                        });
                    }
                }
            }

            foreach (var item in posItems)
            {
                cboPosition.Items.Add(item);
            }
            cboPosition.SelectedIndex = 0;
            cboPosition.EndUpdate();
        }

        private void cboDiv_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return;

            var selDiv = cboDiv.SelectedItem as DivComboItem;
            int? divId = (selDiv != null && selDiv.DivisionID > 0) ? (int?)selDiv.DivisionID : null;

            _isInitializing = true;
            PopulateDepartments(divId);
            PopulatePositions(divId, null);
            _isInitializing = false;

            UpdateDefaultLabel();
        }

        private void cboDept_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return;

            var selDiv = cboDiv.SelectedItem as DivComboItem;
            var selDept = cboDept.SelectedItem as DeptComboItem;

            int? divId = (selDiv != null && selDiv.DivisionID > 0) ? (int?)selDiv.DivisionID : null;
            int? deptId = (selDept != null && selDept.DepartmentID > 0) ? (int?)selDept.DepartmentID : null;

            _isInitializing = true;
            PopulatePositions(divId, deptId);
            _isInitializing = false;

            UpdateDefaultLabel();
        }

        private void cboPosition_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (_isInitializing) return;

            var selPos = cboPosition.SelectedItem as PosComboItem;
            if (selPos != null && !string.IsNullOrEmpty(selPos.DefaultLevel))
            {
                string lvl = selPos.DefaultLevel.Trim().ToUpper();
                if (!cboLevelCode.Items.Contains(lvl))
                {
                    cboLevelCode.Items.Add(lvl);
                }
                cboLevelCode.SelectedItem = lvl;
            }

            UpdateDefaultLabel();
        }

        private void UpdateDefaultLabel()
        {
            if (_isEdit) return; // อย่าเขียนทับถ้าเป็นโหมดแก้ไข

            var selPos = cboPosition.SelectedItem as PosComboItem;
            var selDept = cboDept.SelectedItem as DeptComboItem;
            var selDiv = cboDiv.SelectedItem as DivComboItem;

            if (selPos != null && !string.IsNullOrEmpty(selPos.PositionName) && selPos.PositionID > 0)
            {
                txtLabel.Text = selPos.PositionName;
            }
            else if (selDept != null && !string.IsNullOrEmpty(selDept.DepartmentName) && selDept.DepartmentID > 0)
            {
                txtLabel.Text = selDept.DepartmentName;
            }
            else if (selDiv != null && !string.IsNullOrEmpty(selDiv.DivisionName) && selDiv.DivisionID > 0)
            {
                txtLabel.Text = selDiv.DivisionName;
            }
        }

        private void LoadData()
        {
            _isInitializing = true;

            PopulateDivisions();

            // Match Division
            if (!string.IsNullOrEmpty(Config.DivisionCode))
            {
                for (int i = 0; i < cboDiv.Items.Count; i++)
                {
                    var item = cboDiv.Items[i] as DivComboItem;
                    if (item != null && string.Equals(item.DivisionCode, Config.DivisionCode, StringComparison.OrdinalIgnoreCase))
                    {
                        cboDiv.SelectedIndex = i;
                        break;
                    }
                }
            }

            var currentDiv = cboDiv.SelectedItem as DivComboItem;
            int? divId = (currentDiv != null && currentDiv.DivisionID > 0) ? (int?)currentDiv.DivisionID : null;

            PopulateDepartments(divId);

            // Match Department
            if (!string.IsNullOrEmpty(Config.DepartmentCode))
            {
                for (int i = 0; i < cboDept.Items.Count; i++)
                {
                    var item = cboDept.Items[i] as DeptComboItem;
                    if (item != null && string.Equals(item.DepartmentCode, Config.DepartmentCode, StringComparison.OrdinalIgnoreCase))
                    {
                        cboDept.SelectedIndex = i;
                        break;
                    }
                }
            }

            var currentDept = cboDept.SelectedItem as DeptComboItem;
            int? deptId = (currentDept != null && currentDept.DepartmentID > 0) ? (int?)currentDept.DepartmentID : null;

            PopulatePositions(divId, deptId);

            // Match Position
            if (!string.IsNullOrEmpty(Config.PositionName))
            {
                bool matched = false;
                for (int i = 0; i < cboPosition.Items.Count; i++)
                {
                    var item = cboPosition.Items[i] as PosComboItem;
                    if (item != null && string.Equals(item.PositionName, Config.PositionName, StringComparison.OrdinalIgnoreCase))
                    {
                        cboPosition.SelectedIndex = i;
                        matched = true;
                        break;
                    }
                }
                if (!matched)
                {
                    var customPos = new PosComboItem { PositionID = -1, PositionName = Config.PositionName };
                    cboPosition.Items.Add(customPos);
                    cboPosition.SelectedItem = customPos;
                }
            }

            // Match Level
            string lvl = (Config.LevelCode ?? "").Trim().ToUpper();
            if (!string.IsNullOrEmpty(lvl) && !cboLevelCode.Items.Contains(lvl))
            {
                cboLevelCode.Items.Add(lvl);
            }
            int idx = cboLevelCode.FindStringExact(lvl);
            cboLevelCode.SelectedIndex = idx >= 0 ? idx : 0;

            txtLabel.Text = Config.ConfigLabel ?? "";

            chkHC_V.Checked = Config.HC_View;
            chkHC_A.Checked = Config.HC_Add;
            chkHC_E.Checked = Config.HC_Edit;
            chkHC_Ap.Checked = Config.HC_Approve;

            chkSC_V.Checked = Config.SC_View;
            chkSC_A.Checked = Config.SC_Add;
            chkSC_E.Checked = Config.SC_Edit;
            chkSC_Ap.Checked = Config.SC_Approve;

            chkNC_V.Checked = Config.NC_View;
            chkNC_A.Checked = Config.NC_Add;
            chkNC_E.Checked = Config.NC_Edit;
            chkNC_Ap.Checked = Config.NC_Approve;

            chkPC_V.Checked = Config.PC_View;
            chkPC_A.Checked = Config.PC_Add;
            chkPC_E.Checked = Config.PC_Edit;
            chkPC_Ap.Checked = Config.PC_Approve;

            _isInitializing = false;
        }

        private void btnSave_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtLabel.Text))
            {
                MessageBox.Show("กรุณากรอก ชื่อสิทธิ์ (Label)", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtLabel.Focus();
                return;
            }

            Config.ConfigLabel = txtLabel.Text.Trim();

            var selPos = cboPosition.SelectedItem as PosComboItem;
            Config.PositionName = (selPos != null && selPos.PositionID != 0 && !string.IsNullOrWhiteSpace(selPos.PositionName))
                ? selPos.PositionName.Trim()
                : null;

            string selectedLevel = cboLevelCode.SelectedItem != null ? cboLevelCode.SelectedItem.ToString().Trim() : "";
            Config.LevelCode = string.IsNullOrWhiteSpace(selectedLevel) ? null : selectedLevel.ToUpper();

            var selDept = cboDept.SelectedItem as DeptComboItem;
            Config.DepartmentCode = (selDept != null && selDept.DepartmentID != 0 && !string.IsNullOrWhiteSpace(selDept.DepartmentCode))
                ? selDept.DepartmentCode.Trim()
                : null;

            var selDiv = cboDiv.SelectedItem as DivComboItem;
            Config.DivisionCode = (selDiv != null && selDiv.DivisionID != 0 && !string.IsNullOrWhiteSpace(selDiv.DivisionCode))
                ? selDiv.DivisionCode.Trim()
                : null;

            Config.HC_View = chkHC_V.Checked;
            Config.HC_Add = chkHC_A.Checked;
            Config.HC_Edit = chkHC_E.Checked;
            Config.HC_Approve = chkHC_Ap.Checked;

            Config.SC_View = chkSC_V.Checked;
            Config.SC_Add = chkSC_A.Checked;
            Config.SC_Edit = chkSC_E.Checked;
            Config.SC_Approve = chkSC_Ap.Checked;

            Config.NC_View = chkNC_V.Checked;
            Config.NC_Add = chkNC_A.Checked;
            Config.NC_Edit = chkNC_E.Checked;
            Config.NC_Approve = chkNC_Ap.Checked;

            Config.PC_View = chkPC_V.Checked;
            Config.PC_Add = chkPC_A.Checked;
            Config.PC_Edit = chkPC_E.Checked;
            Config.PC_Approve = chkPC_Ap.Checked;

            this.DialogResult = DialogResult.OK;
            this.Close();
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            this.DialogResult = DialogResult.Cancel;
            this.Close();
        }

        #region Helper Item Classes

        private class DivComboItem
        {
            public int DivisionID { get; set; }
            public string DivisionCode { get; set; }
            public string DivisionName { get; set; }

            public override string ToString()
            {
                if (DivisionID == 0 || string.IsNullOrEmpty(DivisionCode))
                    return "(ไม่ระบุ)";
                return string.Format("[{0}] {1}", DivisionCode, DivisionName);
            }
        }

        private class DeptComboItem
        {
            public int DepartmentID { get; set; }
            public int DivisionID { get; set; }
            public string DepartmentCode { get; set; }
            public string DepartmentName { get; set; }

            public override string ToString()
            {
                if (DepartmentID == 0 || string.IsNullOrEmpty(DepartmentCode))
                    return "(ไม่ระบุ)";
                return string.Format("[{0}] {1}", DepartmentCode, DepartmentName);
            }
        }

        private class PosComboItem
        {
            public int PositionID { get; set; }
            public string PositionCode { get; set; }
            public string PositionName { get; set; }
            public string DefaultLevel { get; set; }

            public override string ToString()
            {
                if (PositionID == 0 || string.IsNullOrEmpty(PositionName))
                    return "(ไม่ระบุ)";
                return PositionName;
            }
        }

        #endregion
    }
}
