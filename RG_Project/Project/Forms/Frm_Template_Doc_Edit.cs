using RG_Project.Login;
using RG_Project.Project.Controllers;
using RG_Project.Project.Models;
using System;
using System.Data;
using System.Drawing;
using System.Windows.Forms;

namespace RG_Project.Project.Forms
{
    /// <summary>
    /// Form จัดการ:
    ///   Tab 1 — ประเภทโครงการ (m_project_template): เพิ่ม / แก้ไข / ลบ
    ///   Tab 2 — เอกสาร Template (m_template_doc):  mapping State ↔ เอกสาร
    ///            ซ้าย: รายการ State ของ Template + CRUD ขั้นตอน
    ///            ขวา: เอกสารของ State ที่เลือก + เพิ่ม/แก้ไข/ลบ
    /// </summary>
    public partial class Frm_Template_Doc_Edit : Form
    {
        // ── Services / Controllers ──────────────────────────────────────────────
        private readonly ProjectTemplateController _tplCtrl;
        private readonly TemplateDocController     _templateDocCtrl;
        private readonly string                    _userName;

        // ── Navy palette ────────────────────────────────────────────────────────
        private static readonly Color NavyDark   = Color.FromArgb(30,  58,  95);
        private static readonly Color NavyMedium = Color.FromArgb(43,  87, 154);
        private static readonly Color NavyMediumH= Color.FromArgb(62, 110, 183);
        private static readonly Color NavyLight  = Color.FromArgb(70, 110, 165);
        private static readonly Color NavyLightH = Color.FromArgb(90, 130, 185);
        private static readonly Color SlateBlue  = Color.FromArgb(100, 130, 168);
        private static readonly Color SlateBlueH = Color.FromArgb(120, 150, 190);
        private static readonly Color DangerRed  = Color.FromArgb(196,  53,  53);
        private static readonly Color DangerRedH = Color.FromArgb(218,  73,  73);

        // ── State: Tab 1 (Template CRUD) ────────────────────────────────────────
        private int _editingTplId = -1;   // -1 = add mode, >0 = edit mode

        // ── State: Tab 2 (Doc management) ───────────────────────────────────────
        private DataTable _allDocuments;
        private int  _selectedTemplateId = -1;
        private int  _selectedStateId    = -1;
        private int  _editingId          = -1;

        // ════════════════════════════════════════════════════════════════════════
        public Frm_Template_Doc_Edit()
        {
            InitializeComponent();
            _tplCtrl         = new ProjectTemplateController();
            _templateDocCtrl = new TemplateDocController();
            _userName        = LoginManager.Instance.GetUserName;
        }

        // ════════════════════════════════════════════════════════════════════════
        // FORM LOAD
        // ════════════════════════════════════════════════════════════════════════
        private void Frm_Template_Doc_Edit_Load(object sender, EventArgs e)
        {
            ApplyHovers();

            // Tab 1
            SetupDgvTemplates();
            LoadTemplatesGrid();
            ClearTplEditArea();

            // Tab 2
            SetupDgvStates();
            SetupDgvDocs();
            LoadTemplatesCombo();
            LoadDocumentTypes();
            LoadMasterDocuments();
            ClearDocEditArea();
            btnSaveDoc.Enabled = false;
            btnDelete.Enabled  = false;
        }

        // ════════════════════════════════════════════════════════════════════════
        // TAB HEADER DRAW (Navy style)
        // ════════════════════════════════════════════════════════════════════════
        private void tabMain_DrawItem(object sender, DrawItemEventArgs e)
        {
            TabPage page = tabMain.TabPages[e.Index];
            Color clr = (e.State == DrawItemState.Selected) ? NavyDark : NavyLight;
            using (var brush = new SolidBrush(clr))
                e.Graphics.FillRectangle(brush, e.Bounds);
            SizeF sz   = e.Graphics.MeasureString(page.Text, e.Font);
            float xPos = e.Bounds.Left + (e.Bounds.Width  - sz.Width)  / 2;
            float yPos = e.Bounds.Top  + (e.Bounds.Height - sz.Height) / 2;
            e.Graphics.DrawString(page.Text, e.Font, Brushes.White, xPos, yPos);
        }

        // ════════════════════════════════════════════════════════════════════════
        // TAB 1 — ประเภทโครงการ (Template CRUD)
        // ════════════════════════════════════════════════════════════════════════

        private void SetupDgvTemplates()
        {
            dgvTemplates.AutoGenerateColumns = false;
            dgvTemplates.Columns.Clear();

            dgvTemplates.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_no", HeaderText = "#", Width = 40, ReadOnly = true
            });
            dgvTemplates.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_code", DataPropertyName = "c_template_code",
                HeaderText = "รหัส", Width = 110, ReadOnly = true
            });
            dgvTemplates.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_name", DataPropertyName = "n_template_name",
                HeaderText = "ชื่อประเภทโครงการ",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });
            dgvTemplates.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_active", DataPropertyName = "f_active",
                HeaderText = "สถานะ", Width = 70, ReadOnly = true
            });
            dgvTemplates.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_states", DataPropertyName = "i_state_count",
                HeaderText = "ขั้นตอน", Width = 70, ReadOnly = true
            });
            dgvTemplates.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_projects", DataPropertyName = "i_project_count",
                HeaderText = "โครงการ", Width = 70, ReadOnly = true
            });
            // hidden key
            dgvTemplates.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_id", DataPropertyName = "i_template_id", Visible = false
            });

            StyleDgv(dgvTemplates);

            dgvTemplates.CellFormatting -= DgvTemplates_CellFormatting;
            dgvTemplates.CellFormatting += DgvTemplates_CellFormatting;
        }

        private void DgvTemplates_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            // ลำดับ
            if (dgvTemplates.Columns[e.ColumnIndex].Name == "col_no")
            {
                e.Value             = (e.RowIndex + 1).ToString();
                e.FormattingApplied = true;
                return;
            }
            // สถานะ badge
            if (dgvTemplates.Columns[e.ColumnIndex].Name == "col_active")
            {
                if (e.Value?.ToString() == "Y")
                {
                    e.Value = "✅ เปิด";
                    e.CellStyle.ForeColor = Color.FromArgb(0, 97, 0);
                }
                else
                {
                    e.Value = "⛔ ปิด";
                    e.CellStyle.ForeColor = Color.FromArgb(156, 0, 6);
                }
                e.FormattingApplied = true;
            }
        }

        private void LoadTemplatesGrid()
        {
            try
            {
                dgvTemplates.DataSource = _tplCtrl.GetAll();
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลด Template ไม่สำเร็จ: " + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── Grid Selection ──────────────────────────────────────────────────────
        private void dgvTemplates_SelectionChanged(object sender, EventArgs e)
        {
            bool hasRow = dgvTemplates.SelectedRows.Count > 0 &&
                          !dgvTemplates.SelectedRows[0].IsNewRow;
            btnEditTpl.Enabled   = hasRow;
            btnDeleteTpl.Enabled = hasRow;
        }

        private void dgvTemplates_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            LoadTplIntoEditArea();
        }

        // ─── Toolbar Buttons ─────────────────────────────────────────────────────
        private void btnAddTpl_Click(object sender, EventArgs e)
        {
            dgvTemplates.ClearSelection();
            ClearTplEditArea();
            txtTplName.Focus();
        }

        private void btnEditTpl_Click(object sender, EventArgs e)
        {
            LoadTplIntoEditArea();
        }

        private void btnDeleteTpl_Click(object sender, EventArgs e)
        {
            if (dgvTemplates.SelectedRows.Count == 0) return;
            DataGridViewRow row  = dgvTemplates.SelectedRows[0];
            var idCell = row.Cells["col_id"].Value;
            if (idCell == null || idCell is DBNull) return;

            int    id   = Convert.ToInt32(idCell);
            string name = row.Cells["col_name"].Value?.ToString() ?? "(ไม่ระบุ)";

            if (MessageBox.Show(
                    $"ต้องการลบประเภทโครงการ\n\"{name}\"\n\n" +
                    "หากลบ จะลบขั้นตอนและเอกสาร Template ที่เกี่ยวข้องทั้งหมดด้วย\nยืนยันใช่หรือไม่?",
                    "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                != DialogResult.Yes) return;

            string err;
            if (_tplCtrl.Delete(id, out err))
            {
                LoadTemplatesGrid();
                ClearTplEditArea();
                RefreshTemplatesCombo();
                MessageBox.Show("ลบเรียบร้อยแล้ว", "สำเร็จ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(err, "ไม่สามารถลบได้",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ─── Edit Area ───────────────────────────────────────────────────────────
        private void LoadTplIntoEditArea()
        {
            if (dgvTemplates.SelectedRows.Count == 0) return;
            DataGridViewRow row = dgvTemplates.SelectedRows[0];
            var idCell = row.Cells["col_id"].Value;
            if (idCell == null || idCell is DBNull) return;

            _editingTplId   = Convert.ToInt32(idCell);
            txtTplCode.Text = row.Cells["col_code"].Value?.ToString() ?? "";
            txtTplName.Text = row.Cells["col_name"].Value?.ToString() ?? "";

            // ดึง desc, f_active, f_is_legal_dict จาก DataBoundItem
            var drv = row.DataBoundItem as System.Data.DataRowView;
            if (drv != null)
            {
                txtTplDesc.Text         = drv["c_description"] != DBNull.Value ? drv["c_description"].ToString() : "";
                chkTplActive.Checked    = drv["f_active"]?.ToString() == "Y";
                chkIsLegalDict.Checked  = drv["f_is_legal_dict"]?.ToString() == "Y";
            }
            lblTplEditTitle.Text = "── แก้ไขประเภทโครงการ ──";
            txtTplName.Focus();
        }

        private void btnSaveTpl_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrWhiteSpace(txtTplName.Text))
            {
                MessageBox.Show("กรุณาระบุชื่อประเภทโครงการ", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTplName.Focus();
                return;
            }

            string code = txtTplCode.Text.Trim();

            if (!string.IsNullOrWhiteSpace(code) &&
                _tplCtrl.IsCodeDuplicate(code, _editingTplId))
            {
                MessageBox.Show($"รหัส '{code}' มีในระบบแล้ว กรุณาระบุรหัสใหม่",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                txtTplCode.Focus();
                return;
            }

            bool isLegalDict = chkIsLegalDict.Checked;
            bool success;
            if (_editingTplId > 0)
            {
                success = _tplCtrl.Update(
                    _editingTplId, code, txtTplName.Text.Trim(), txtTplDesc.Text.Trim(),
                    chkTplActive.Checked ? "Y" : "N", _userName, isLegalDict);
            }
            else
            {
                int newId = _tplCtrl.Insert(code, txtTplName.Text.Trim(), txtTplDesc.Text.Trim(), _userName, isLegalDict);
                success = newId > 0;
            }

            if (success)
            {
                LoadTemplatesGrid();
                RefreshTemplatesCombo();
                ClearTplEditArea();
                MessageBox.Show("บันทึกสำเร็จ", "สำเร็จ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("บันทึกไม่สำเร็จ กรุณาลองใหม่", "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelTpl_Click(object sender, EventArgs e)
        {
            ClearTplEditArea();
            dgvTemplates.ClearSelection();
            btnEditTpl.Enabled   = false;
            btnDeleteTpl.Enabled = false;
        }

        private void ClearTplEditArea()
        {
            _editingTplId           = -1;
            txtTplCode.Text         = "";
            txtTplName.Text         = "";
            txtTplDesc.Text         = "";
            chkTplActive.Checked    = true;
            chkIsLegalDict.Checked  = false;
            lblTplEditTitle.Text    = "── เพิ่มประเภทโครงการใหม่ ──";
        }

        // ════════════════════════════════════════════════════════════════════════
        // TAB 2 — เอกสาร Template  (Left: State grid | Right: Doc grid + edit)
        // ════════════════════════════════════════════════════════════════════════

        // ─── Setup dgvStates ────────────────────────────────────────────────────
        private void SetupDgvStates()
        {
            dgvStates.AutoGenerateColumns = false;
            dgvStates.Columns.Clear();

            dgvStates.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_seq", DataPropertyName = "i_seq_no",
                HeaderText = "#", Width = 36, ReadOnly = true
            });
            dgvStates.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_state_name", DataPropertyName = "n_state_name",
                HeaderText = "ชื่อขั้นตอน",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });
            dgvStates.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_doc_count", DataPropertyName = "i_doc_count",
                HeaderText = "เอกสาร", Width = 56, ReadOnly = true
            });
            // hidden key
            dgvStates.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_state_id", DataPropertyName = "i_state_id", Visible = false
            });

            StyleDgv(dgvStates);

            dgvStates.CellFormatting -= DgvStates_CellFormatting;
            dgvStates.CellFormatting += DgvStates_CellFormatting;
        }

        private void DgvStates_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvStates.Columns[e.ColumnIndex].Name == "col_doc_count")
            {
                var v = e.Value;
                if (v == null || v is DBNull || Convert.ToInt32(v) == 0)
                {
                    e.Value = "-";
                    e.CellStyle.ForeColor = Color.Silver;
                }
                else
                {
                    e.Value = v.ToString() + " รายการ";
                    e.CellStyle.ForeColor = Color.FromArgb(43, 87, 154);
                }
                e.FormattingApplied = true;
            }
        }

        // ─── Load dgvStates ──────────────────────────────────────────────────────
        private void LoadStatesGrid(int templateId)
        {
            try
            {
                dgvStates.DataSource = _tplCtrl.GetStates(templateId);
                btnAddState.Enabled  = true;
                // reset doc side
                dgvDocs.DataSource            = null;
                lblSelectedState.Text         = "← เลือกขั้นตอนจากรายการทางซ้าย";
                lblSelectedState.BackColor    = NavyDark;
                _selectedStateId              = -1;
                btnEditState.Enabled          = false;
                btnDeleteState.Enabled        = false;
                btnSaveDoc.Enabled            = false;
                btnDelete.Enabled             = false;
                ClearDocEditArea();
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลดขั้นตอนไม่สำเร็จ: " + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── dgvStates events ────────────────────────────────────────────────────
        private void dgvStates_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvStates.SelectedRows.Count == 0)
            {
                btnEditState.Enabled   = false;
                btnDeleteState.Enabled = false;
                return;
            }
            DataGridViewRow row = dgvStates.SelectedRows[0];
            if (row.IsNewRow) return;

            var idCell = row.Cells["col_state_id"].Value;
            if (idCell == null || idCell is DBNull) return;

            _selectedStateId       = Convert.ToInt32(idCell);
            btnEditState.Enabled   = true;
            btnDeleteState.Enabled = true;

            string stateName = row.Cells["col_state_name"].Value?.ToString() ?? "";
            int seqNo        = 0;
            var seqCell = row.Cells["col_seq"].Value;
            if (seqCell != null && !(seqCell is DBNull)) seqNo = Convert.ToInt32(seqCell);

            lblSelectedState.Text = $"  ขั้นตอนที่ {seqNo}: {stateName}";

            LoadDocs(_selectedStateId);
            btnSaveDoc.Enabled = true;
        }

        private void dgvStates_CellDoubleClick(object sender, DataGridViewCellEventArgs e)
        {
            if (e.RowIndex < 0) return;
            btnEditState_Click(sender, EventArgs.Empty);
        }

        // ─── State CRUD Buttons ──────────────────────────────────────────────────
        private void btnAddState_Click(object sender, EventArgs e)
        {
            if (_selectedTemplateId < 0)
            {
                MessageBox.Show("กรุณาเลือก Template ก่อน", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }
            int nextSeq = _tplCtrl.GetNextStateSeqNo(_selectedTemplateId);
            ShowStateDialog(-1, "", "", true, nextSeq, null);
        }

        private void btnEditState_Click(object sender, EventArgs e)
        {
            if (dgvStates.SelectedRows.Count == 0) return;
            DataGridViewRow row = dgvStates.SelectedRows[0];
            if (row.IsNewRow) return;

            var idCell = row.Cells["col_state_id"].Value;
            if (idCell == null || idCell is DBNull) return;

            int    stateId  = Convert.ToInt32(idCell);
            string name     = row.Cells["col_state_name"].Value?.ToString() ?? "";
            int    seqNo    = Convert.ToInt32(row.Cells["col_seq"].Value ?? 1);

            // ดึง desc, required, estimatedDays จาก DataSource
            string desc     = "";
            bool   required = true;
            if (dgvStates.DataSource is DataTable dt)
            {
                foreach (DataRow dr in dt.Rows)
                {
                    if (dr["i_state_id"] != DBNull.Value &&
                        Convert.ToInt32(dr["i_state_id"]) == stateId)
                    {
                        desc     = dr["c_description"] != DBNull.Value ? dr["c_description"].ToString() : "";
                        required = dr["f_required"]?.ToString() == "Y";
                        break;
                    }
                }
            }
            ShowStateDialog(stateId, name, desc, required, seqNo, null);
        }

        private void btnDeleteState_Click(object sender, EventArgs e)
        {
            if (dgvStates.SelectedRows.Count == 0) return;
            DataGridViewRow row = dgvStates.SelectedRows[0];
            if (row.IsNewRow) return;

            var idCell = row.Cells["col_state_id"].Value;
            if (idCell == null || idCell is DBNull) return;

            int    stateId   = Convert.ToInt32(idCell);
            string stateName = row.Cells["col_state_name"].Value?.ToString() ?? "(ไม่ระบุ)";

            if (MessageBox.Show(
                    $"ต้องการลบขั้นตอน\n\"{stateName}\"\n\nยืนยันใช่หรือไม่?",
                    "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Warning)
                != DialogResult.Yes) return;

            string err;
            if (_tplCtrl.DeleteState(stateId, out err))
            {
                _selectedStateId           = -1;
                dgvDocs.DataSource         = null;
                lblSelectedState.Text      = "← เลือกขั้นตอนจากรายการทางซ้าย";
                btnEditState.Enabled       = false;
                btnDeleteState.Enabled     = false;
                btnSaveDoc.Enabled         = false;
                btnDelete.Enabled          = false;
                ClearDocEditArea();
                LoadStatesGrid(_selectedTemplateId);
                MessageBox.Show("ลบเรียบร้อยแล้ว", "สำเร็จ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show(err, "ไม่สามารถลบได้",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        // ─── Inline State Dialog ─────────────────────────────────────────────────
        /// <summary>
        /// แสดง dialog เพิ่ม/แก้ไขขั้นตอน (สร้าง Form ใน code)
        /// stateId = -1 คือ add mode
        /// </summary>
        private void ShowStateDialog(int stateId, string name, string desc,
                                     bool required, int seqNo, int? estimatedDays)
        {
            bool isAdd = stateId < 0;

            // ── สร้าง Form ────────────────────────────────────────────────────
            var dlg = new Form
            {
                Text            = isAdd ? "เพิ่มขั้นตอนใหม่" : "แก้ไขขั้นตอน",
                Size            = new Size(460, 260),
                MinimumSize     = new Size(460, 260),
                MaximumSize     = new Size(460, 260),
                StartPosition   = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox     = false,
                MinimizeBox     = false,
                BackColor       = Color.FromArgb(242, 246, 252),
                Font            = new Font("Segoe UI", 9F)
            };

            int lx = 12, cx = 120, cw = 310;

            // ชื่อขั้นตอน
            var lblName = new Label { Text = "ชื่อขั้นตอน *:", Location = new Point(lx, 20), Size = new Size(102, 23),
                TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9F) };
            var txtName = new TextBox { Location = new Point(cx, 18), Size = new Size(cw, 23),
                BorderStyle = BorderStyle.FixedSingle, MaxLength = 200, Text = name };

            // คำอธิบาย
            var lblDesc = new Label { Text = "คำอธิบาย:", Location = new Point(lx, 56), Size = new Size(102, 23),
                TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9F) };
            var txtDesc = new TextBox { Location = new Point(cx, 54), Size = new Size(cw, 55),
                BorderStyle = BorderStyle.FixedSingle, Multiline = true, ScrollBars = ScrollBars.Vertical,
                MaxLength = 500, Text = desc };

            // ลำดับ
            var lblSeq = new Label { Text = "ลำดับ:", Location = new Point(lx, 124), Size = new Size(102, 23),
                TextAlign = ContentAlignment.MiddleRight, Font = new Font("Segoe UI", 9F) };
            var nudSeq = new NumericUpDown { Location = new Point(cx, 122), Size = new Size(70, 23),
                Minimum = 1, Maximum = 999, Value = seqNo, Font = new Font("Segoe UI", 9F) };

            // บังคับ
            var chkReq = new CheckBox { Text = "บังคับ (Required)", Location = new Point(cx, 124),
                Size = new Size(200, 22), Checked = required, Font = new Font("Segoe UI", 9F) };

            // ปุ่ม
            var btnOk = new Button
            {
                Text      = isAdd ? "เพิ่ม" : "บันทึก",
                Location  = new Point(cx, 162),
                Size      = new Size(100, 30),
                FlatStyle = FlatStyle.Flat,
                BackColor = NavyMedium,
                ForeColor = Color.White,
                Font      = new Font("Segoe UI", 9F, FontStyle.Bold),
                Cursor    = Cursors.Hand,
                DialogResult = DialogResult.None
            };
            btnOk.FlatAppearance.BorderSize = 0;

            var btnCancel = new Button
            {
                Text         = "ยกเลิก",
                Location     = new Point(cx + 110, 162),
                Size         = new Size(90, 30),
                FlatStyle    = FlatStyle.Flat,
                BackColor    = SlateBlue,
                ForeColor    = Color.White,
                Font         = new Font("Segoe UI", 9F),
                Cursor       = Cursors.Hand,
                DialogResult = DialogResult.Cancel
            };
            btnCancel.FlatAppearance.BorderSize = 0;

            dlg.Controls.AddRange(new Control[] {
                lblName, txtName, lblDesc, txtDesc,
                lblSeq, nudSeq,
                chkReq, btnOk, btnCancel
            });
            dlg.AcceptButton = btnOk;
            dlg.CancelButton = btnCancel;

            btnOk.Click += (s, ev) =>
            {
                if (string.IsNullOrWhiteSpace(txtName.Text))
                {
                    MessageBox.Show("กรุณาระบุชื่อขั้นตอน", "แจ้งเตือน",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    txtName.Focus();
                    return;
                }

                bool   reqVal  = chkReq.Checked;
                int    seqVal  = Convert.ToInt32(nudSeq.Value);

                if (isAdd)
                {
                    int newId = _tplCtrl.InsertState(
                        _selectedTemplateId, txtName.Text.Trim(), txtDesc.Text.Trim(),
                        reqVal, seqVal, null, _userName);
                    if (newId > 0)
                    {
                        dlg.DialogResult = DialogResult.OK;
                        dlg.Close();
                    }
                    else
                    {
                        MessageBox.Show("บันทึกไม่สำเร็จ กรุณาลองใหม่", "ข้อผิดพลาด",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
                else
                {
                    bool ok = _tplCtrl.UpdateState(
                        stateId, txtName.Text.Trim(), txtDesc.Text.Trim(),
                        reqVal, seqVal, null, _userName);
                    if (ok)
                    {
                        dlg.DialogResult = DialogResult.OK;
                        dlg.Close();
                    }
                    else
                    {
                        MessageBox.Show("บันทึกไม่สำเร็จ กรุณาลองใหม่", "ข้อผิดพลาด",
                            MessageBoxButtons.OK, MessageBoxIcon.Error);
                    }
                }
            };

            var result = dlg.ShowDialog(this);
            if (result == DialogResult.OK)
            {
                LoadStatesGrid(_selectedTemplateId);
                // ถ้าเป็น edit mode ให้ re-select state เดิม
                if (!isAdd && stateId > 0)
                    SelectStateById(stateId);
            }
        }

        /// <summary>Select row ใน dgvStates ด้วย state_id</summary>
        private void SelectStateById(int stateId)
        {
            foreach (DataGridViewRow row in dgvStates.Rows)
            {
                var idCell = row.Cells["col_state_id"].Value;
                if (idCell != null && !(idCell is DBNull) &&
                    Convert.ToInt32(idCell) == stateId)
                {
                    row.Selected = true;
                    dgvStates.FirstDisplayedScrollingRowIndex = row.Index;
                    return;
                }
            }
        }

        // ─── Setup dgvDocs ───────────────────────────────────────────────────────
        private void SetupDgvDocs()
        {
            dgvDocs.AutoGenerateColumns = false;
            dgvDocs.Columns.Clear();

            dgvDocs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_seq",      DataPropertyName = "i_seq_no",
                HeaderText = "#",      Width = 40,    ReadOnly = true
            });
            dgvDocs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_doc_type", DataPropertyName = "c_doc_type",
                HeaderText = "รหัส",   Width = 110,   ReadOnly = true
            });
            dgvDocs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_doc_name", DataPropertyName = "n_doc_name",
                HeaderText = "ชื่อเอกสาร",
                AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill,
                ReadOnly = true
            });
            dgvDocs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_required", DataPropertyName = "f_required",
                HeaderText = "บังคับ",  Width = 65,    ReadOnly = true
            });
            dgvDocs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_remark",   DataPropertyName = "n_remark",
                HeaderText = "หมายเหตุ", Width = 150,  ReadOnly = true
            });
            dgvDocs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_id",     DataPropertyName = "i_template_doc_id", Visible = false
            });
            dgvDocs.Columns.Add(new DataGridViewTextBoxColumn
            {
                Name = "col_doc_id", DataPropertyName = "i_doc_id",          Visible = false
            });

            StyleDgv(dgvDocs);

            dgvDocs.CellFormatting -= DgvDocs_CellFormatting;
            dgvDocs.CellFormatting += DgvDocs_CellFormatting;
        }

        private void DgvDocs_CellFormatting(object sender, DataGridViewCellFormattingEventArgs e)
        {
            if (e.RowIndex < 0) return;
            if (dgvDocs.Columns[e.ColumnIndex].Name == "col_required")
            {
                e.Value = e.Value?.ToString() == "Y" ? "✅" : "-";
                e.CellStyle.Alignment = DataGridViewContentAlignment.MiddleCenter;
                e.FormattingApplied   = true;
            }
        }

        // ─── Load Combos (Tab 2) ─────────────────────────────────────────────────
        private void LoadTemplatesCombo()
        {
            try
            {
                var conn = new RG_Project.Controllers.ConectionDB();
                conn.Connect('c');
                string sql = @"
                    SELECT i_template_id, n_template_name
                    FROM   m_project_template
                    WHERE  f_active = 'Y'
                    ORDER BY n_template_name";
                DataTable dt = new DataTable();
                using (var cmd = new System.Data.SqlClient.SqlCommand(sql, conn.connection))
                using (var r   = cmd.ExecuteReader())
                    dt.Load(r);
                conn.Disconnect();

                foreach (DataColumn col in dt.Columns) col.AllowDBNull = true;
                DataRow blank = dt.NewRow();
                blank["i_template_id"]   = DBNull.Value;
                blank["n_template_name"] = "--- เลือก Template ---";
                dt.Rows.InsertAt(blank, 0);

                cbbTemplate.DisplayMember = "n_template_name";
                cbbTemplate.ValueMember   = "i_template_id";
                cbbTemplate.DataSource    = dt;
                cbbTemplate.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลด Template ไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void RefreshTemplatesCombo()
        {
            int curVal = (_selectedTemplateId > 0) ? _selectedTemplateId : -1;
            LoadTemplatesCombo();
            if (curVal > 0)
            {
                DataTable dt = cbbTemplate.DataSource as DataTable;
                if (dt == null) return;
                foreach (DataRowView drv in dt.DefaultView)
                {
                    if (drv["i_template_id"] != DBNull.Value &&
                        Convert.ToInt32(drv["i_template_id"]) == curVal)
                    { cbbTemplate.SelectedItem = drv; return; }
                }
            }
        }

        private void LoadDocumentTypes()
        {
            try
            {
                DataTable dt = _templateDocCtrl.GetDocumentTypes();
                foreach (DataColumn col in dt.Columns) col.AllowDBNull = true;
                DataRow blank = dt.NewRow();
                blank["doc_type_id"]   = DBNull.Value;
                blank["doc_type_name"] = "--- ทุกประเภท ---";
                dt.Rows.InsertAt(blank, 0);

                cbbDocType.DisplayMember = "doc_type_name";
                cbbDocType.ValueMember   = "doc_type_id";
                cbbDocType.DataSource    = dt;
                cbbDocType.SelectedIndex = 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลดประเภทเอกสารไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void LoadMasterDocuments()
        {
            try
            {
                DataTable dt = _templateDocCtrl.GetMasterDocuments();
                foreach (DataColumn col in dt.Columns) col.AllowDBNull = true;
                _allDocuments = dt;
                FilterDocuments(null);
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "โหลดรายการเอกสารไม่สำเร็จ\nตรวจสอบการเชื่อมต่อ BUI_RG\n\n" + ex.Message,
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Warning);
            }
        }

        private void FilterDocuments(int? docTypeId)
        {
            if (_allDocuments == null) return;
            DataTable filtered;
            if (docTypeId == null)
            {
                filtered = _allDocuments.Copy();
            }
            else
            {
                filtered = _allDocuments.Clone();
                foreach (DataRow row in _allDocuments.Rows)
                {
                    var tv = row["doc_type_id"];
                    if (tv != null && !(tv is DBNull) && Convert.ToInt32(tv) == docTypeId.Value)
                        filtered.ImportRow(row);
                }
            }
            DataRow blank2 = filtered.NewRow();
            blank2["doc_id"]       = DBNull.Value;
            blank2["display_name"] = "--- เลือก เอกสาร ---";
            filtered.Rows.InsertAt(blank2, 0);

            cbbDocument.DisplayMember = "display_name";
            cbbDocument.ValueMember   = "doc_id";
            cbbDocument.DataSource    = filtered;
            cbbDocument.SelectedIndex = 0;
        }

        private void LoadDocs(int stateId)
        {
            try
            {
                dgvDocs.DataSource = _templateDocCtrl.GetDocsByState(stateId);
                ClearDocEditArea();
                btnDelete.Enabled  = false;
            }
            catch (Exception ex)
            {
                MessageBox.Show("โหลดเอกสารไม่สำเร็จ: " + ex.Message, "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        // ─── ComboBox Events ─────────────────────────────────────────────────────
        private void cbbTemplate_SelectedIndexChanged(object sender, EventArgs e)
        {
            var val = cbbTemplate.SelectedValue;
            if (val == null || val is DBNull)
            {
                _selectedTemplateId    = -1;
                dgvStates.DataSource   = null;
                btnAddState.Enabled    = false;
                btnEditState.Enabled   = false;
                btnDeleteState.Enabled = false;
                dgvDocs.DataSource     = null;
                lblSelectedState.Text  = "← เลือกขั้นตอนจากรายการทางซ้าย";
                _selectedStateId       = -1;
                ClearDocEditArea();
                btnSaveDoc.Enabled     = false;
                btnDelete.Enabled      = false;
                return;
            }

            int tid;
            if (val is int)      tid = (int)val;
            else if (!int.TryParse(val.ToString(), out tid)) return;

            _selectedTemplateId = tid;
            LoadStatesGrid(_selectedTemplateId);
        }

        private void cbbDocType_SelectedIndexChanged(object sender, EventArgs e)
        {
            var val = cbbDocType.SelectedValue;
            if (val == null || val is DBNull)
                FilterDocuments(null);
            else
            {
                int tid;
                if (val is int) tid = (int)val;
                else if (!int.TryParse(val.ToString(), out tid)) { FilterDocuments(null); return; }
                FilterDocuments(tid);
            }
        }

        // ─── Doc Grid Selection ──────────────────────────────────────────────────
        private void dgvDocs_SelectionChanged(object sender, EventArgs e)
        {
            if (dgvDocs.SelectedRows.Count == 0)
            { btnDelete.Enabled = false; return; }

            DataGridViewRow row = dgvDocs.SelectedRows[0];
            if (row.IsNewRow) return;
            btnDelete.Enabled = true;

            var idCell    = row.Cells["col_id"].Value;
            var docIdCell = row.Cells["col_doc_id"].Value;
            if (idCell == null || idCell is DBNull) return;

            _editingId = Convert.ToInt32(idCell);

            int docId = (docIdCell != null && !(docIdCell is DBNull))
                        ? Convert.ToInt32(docIdCell) : -1;
            if (docId > 0)
            {
                if (cbbDocType.Items.Count > 0) cbbDocType.SelectedIndex = 0;
                SelectComboByDocId(docId);
            }

            chkRequired.Checked = row.Cells["col_required"].Value?.ToString() == "Y";

            var seqCell = row.Cells["col_seq"].Value;
            nudSeqNoEdit.Value = (seqCell != null && !(seqCell is DBNull))
                                 ? Convert.ToInt32(seqCell) : 1;

            txtRemarkEdit.Text = row.Cells["col_remark"].Value?.ToString() ?? "";
            lblEditTitle.Text  = "── แก้ไขเอกสาร ──";
        }

        private void SelectComboByDocId(int docId)
        {
            DataTable dt = cbbDocument.DataSource as DataTable;
            if (dt == null) return;
            foreach (DataRowView drv in dt.DefaultView)
            {
                var v = drv["doc_id"];
                if (v != null && !(v is DBNull) && Convert.ToInt32(v) == docId)
                { cbbDocument.SelectedItem = drv; return; }
            }
        }

        // ─── Save Doc ────────────────────────────────────────────────────────────
        private void btnSaveDoc_Click(object sender, EventArgs e)
        {
            if (_selectedStateId < 0)
            {
                MessageBox.Show("กรุณาเลือก ขั้นตอน ก่อนบันทึก", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var docVal = cbbDocument.SelectedValue;
            if (docVal == null || docVal is DBNull)
            {
                MessageBox.Show("กรุณาเลือก เอกสาร", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                cbbDocument.Focus(); return;
            }

            int    docId;
            if (docVal is int) docId = (int)docVal;
            else if (!int.TryParse(docVal.ToString(), out docId)) return;

            string docName = "", docCode = "";
            if (cbbDocument.SelectedItem is DataRowView drv2)
            {
                docName = drv2["doc_name"]?.ToString() ?? "";
                docCode = drv2["doc_code"]?.ToString() ?? "";
            }

            if (string.IsNullOrEmpty(docName))
            {
                MessageBox.Show("ไม่พบชื่อเอกสาร กรุณาเลือกใหม่", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            if (_templateDocCtrl.IsDuplicate(_selectedStateId, docId, _editingId))
            {
                MessageBox.Show("เอกสารนี้ถูก mapping กับขั้นตอนนี้ไว้แล้ว",
                    "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            var model = new TemplateDocModel
            {
                i_template_id  = _selectedTemplateId,
                i_state_id     = _selectedStateId,
                i_doc_id       = docId,
                n_doc_name     = docName,
                c_doc_type     = docCode,
                f_required     = chkRequired.Checked ? "Y" : "N",
                i_seq_no       = (int)nudSeqNoEdit.Value,
                n_remark       = txtRemarkEdit.Text.Trim(),
                i_create_user  = _userName,
                i_update_user  = _userName
            };

            bool success;
            if (_editingId > 0)
            {
                model.i_template_doc_id = _editingId;
                success = _templateDocCtrl.UpdateTemplateDoc(model);
            }
            else
            {
                int newId = _templateDocCtrl.InsertTemplateDoc(model);
                success = newId > 0;
            }

            if (success)
            {
                LoadDocs(_selectedStateId);
                // refresh doc count ใน dgvStates
                LoadStatesGrid(_selectedTemplateId);
                SelectStateById(_selectedStateId);
                ClearDocEditArea();
                MessageBox.Show("บันทึกสำเร็จ", "สำเร็จ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
            {
                MessageBox.Show("บันทึกไม่สำเร็จ กรุณาลองใหม่", "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void btnCancelEdit_Click(object sender, EventArgs e)
        {
            ClearDocEditArea();
            dgvDocs.ClearSelection();
            btnDelete.Enabled = false;
        }

        // ─── Delete Doc ──────────────────────────────────────────────────────────
        private void btnDelete_Click(object sender, EventArgs e)
        {
            if (dgvDocs.SelectedRows.Count == 0) return;
            DataGridViewRow row     = dgvDocs.SelectedRows[0];
            string          docName = row.Cells["col_doc_name"].Value?.ToString() ?? "(ไม่ระบุ)";
            var             idCell  = row.Cells["col_id"].Value;
            if (idCell == null || idCell is DBNull) return;
            int id = Convert.ToInt32(idCell);

            if (MessageBox.Show(
                    $"ต้องการลบเอกสาร\n\"{docName}\"\nออกจาก Template ขั้นตอนนี้ใช่หรือไม่?",
                    "ยืนยันการลบ", MessageBoxButtons.YesNo, MessageBoxIcon.Question)
                != DialogResult.Yes) return;

            if (_templateDocCtrl.DeleteTemplateDoc(id))
            {
                int savedStateId = _selectedStateId;
                LoadStatesGrid(_selectedTemplateId);
                SelectStateById(savedStateId);
                ClearDocEditArea();
            }
            else
                MessageBox.Show("ลบข้อมูลไม่สำเร็จ", "ข้อผิดพลาด",
                    MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }

        private void ClearDocEditArea()
        {
            _editingId = -1;
            if (cbbDocType.Items.Count > 0)  cbbDocType.SelectedIndex  = 0;
            if (cbbDocument.Items.Count > 0) cbbDocument.SelectedIndex = 0;
            chkRequired.Checked = true;
            nudSeqNoEdit.Value  = GetNextDocSeq();
            txtRemarkEdit.Text  = "";
            lblEditTitle.Text   = "── เพิ่ม / แก้ไขเอกสาร ──";
        }

        private int GetNextDocSeq()
        {
            if (_selectedStateId < 0) return 1;
            try { return _templateDocCtrl.GetNextSeqNo(_selectedStateId); }
            catch { return 1; }
        }

        // ─── คัดลอกขั้นตอน ──────────────────────────────────────────────────────
        private void btnCopyStates_Click(object sender, EventArgs e)
        {
            if (_selectedTemplateId < 0)
            {
                MessageBox.Show("กรุณาเลือก Template เป้าหมายก่อน", "แจ้งเตือน",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            // สร้าง dialog เลือก source template
            var dlg = new Form
            {
                Text = "เลือก Template ต้นทางที่จะคัดลอกขั้นตอน",
                Size = new Size(420, 170), MinimumSize = new Size(420, 170),
                MaximumSize = new Size(420, 170),
                StartPosition = FormStartPosition.CenterParent,
                FormBorderStyle = FormBorderStyle.FixedDialog,
                MaximizeBox = false, MinimizeBox = false,
                BackColor = Color.FromArgb(242, 246, 252), Font = new Font("Segoe UI", 9F)
            };

            var lblSrc = new Label { Text = "Template ต้นทาง:", Location = new Point(12, 20), Size = new Size(120, 23),
                TextAlign = ContentAlignment.MiddleRight };
            var cbbSrc = new ComboBox { Location = new Point(138, 18), Size = new Size(260, 23),
                DropDownStyle = ComboBoxStyle.DropDownList };

            DataTable templates = _tplCtrl.GetAllActive();
            // กรอง target ออก
            foreach (DataRow dr in templates.Rows)
            {
                if (Convert.ToInt32(dr["i_template_id"]) != _selectedTemplateId)
                    cbbSrc.Items.Add(new { Id = Convert.ToInt32(dr["i_template_id"]), Name = dr["n_template_name"].ToString() });
            }
            cbbSrc.DisplayMember = "Name";
            if (cbbSrc.Items.Count > 0) cbbSrc.SelectedIndex = 0;

            var lblWarn = new Label { Text = "⚠ ขั้นตอนเดิมของ Template นี้จะถูกแทนที่ทั้งหมด",
                Location = new Point(12, 50), Size = new Size(390, 20),
                ForeColor = Color.FromArgb(180, 80, 0), Font = new Font("Segoe UI", 8.5F, FontStyle.Italic) };

            var btnOk = new Button { Text = "คัดลอก", Location = new Point(138, 82), Size = new Size(100, 28),
                FlatStyle = FlatStyle.Flat, BackColor = NavyMedium, ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F, FontStyle.Bold), Cursor = Cursors.Hand, DialogResult = DialogResult.OK };
            btnOk.FlatAppearance.BorderSize = 0;
            var btnCancel = new Button { Text = "ยกเลิก", Location = new Point(246, 82), Size = new Size(90, 28),
                FlatStyle = FlatStyle.Flat, BackColor = SlateBlue, ForeColor = Color.White,
                Font = new Font("Segoe UI", 9F), Cursor = Cursors.Hand, DialogResult = DialogResult.Cancel };
            btnCancel.FlatAppearance.BorderSize = 0;

            dlg.Controls.AddRange(new Control[] { lblSrc, cbbSrc, lblWarn, btnOk, btnCancel });
            dlg.AcceptButton = btnOk; dlg.CancelButton = btnCancel;

            if (dlg.ShowDialog(this) != DialogResult.OK) return;
            if (cbbSrc.SelectedItem == null) return;

            dynamic selected = cbbSrc.SelectedItem;
            int sourceId = selected.Id;

            int copied = _tplCtrl.CopyStatesFrom(sourceId, _selectedTemplateId, _userName);
            if (copied >= 0)
            {
                LoadStatesGrid(_selectedTemplateId);
                MessageBox.Show($"คัดลอก {copied} ขั้นตอน เรียบร้อยแล้ว", "สำเร็จ",
                    MessageBoxButtons.OK, MessageBoxIcon.Information);
            }
            else
                MessageBox.Show("คัดลอกไม่สำเร็จ", "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
        }

        // ════════════════════════════════════════════════════════════════════════
        // HELPERS — Style & Hover
        // ════════════════════════════════════════════════════════════════════════
        private void StyleDgv(DataGridView dgv)
        {
            var hStyle = dgv.ColumnHeadersDefaultCellStyle;
            hStyle.BackColor  = NavyDark;
            hStyle.ForeColor  = Color.White;
            hStyle.Font       = new Font("Segoe UI", 9F, FontStyle.Bold);
            hStyle.Padding    = new Padding(6, 0, 0, 0);
            hStyle.Alignment  = DataGridViewContentAlignment.MiddleLeft;
            dgv.ColumnHeadersHeight         = 32;
            dgv.ColumnHeadersHeightSizeMode = DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            dgv.EnableHeadersVisualStyles   = false;

            var dStyle = dgv.DefaultCellStyle;
            dStyle.Font               = new Font("Segoe UI", 9F);
            dStyle.ForeColor          = Color.FromArgb(42, 57, 80);
            dStyle.BackColor          = Color.White;
            dStyle.SelectionBackColor = Color.FromArgb(190, 215, 255);
            dStyle.SelectionForeColor = Color.FromArgb(20,  40,  80);
            dStyle.Padding            = new Padding(4, 0, 0, 0);

            var altStyle = dgv.AlternatingRowsDefaultCellStyle;
            altStyle.BackColor = Color.FromArgb(236, 244, 255);
            altStyle.ForeColor = Color.FromArgb(42, 57, 80);

            dgv.GridColor          = Color.FromArgb(208, 220, 240);
            dgv.CellBorderStyle    = DataGridViewCellBorderStyle.SingleHorizontal;
            dgv.BackgroundColor    = Color.White;
            dgv.RowHeadersVisible  = false;
            dgv.RowTemplate.Height = 28;
            dgv.SelectionMode      = DataGridViewSelectionMode.FullRowSelect;
            dgv.ReadOnly           = true;
            dgv.AllowUserToAddRows    = false;
            dgv.AllowUserToDeleteRows = false;
        }

        private void ApplyHovers()
        {
            Hover(btnAddTpl,      NavyMediumH, NavyMedium);
            Hover(btnEditTpl,     NavyLightH,  NavyLight);
            Hover(btnDeleteTpl,   DangerRedH,  DangerRed);
            Hover(btnSaveTpl,     NavyMediumH, NavyMedium);
            Hover(btnCancelTpl,   SlateBlueH,  SlateBlue);
            Hover(btnAddState,    NavyMediumH, NavyMedium);
            Hover(btnEditState,   NavyLightH,  NavyLight);
            Hover(btnDeleteState, DangerRedH,  DangerRed);
            Hover(btnSaveDoc,     NavyMediumH, NavyMedium);
            Hover(btnCancelEdit,  SlateBlueH,  SlateBlue);
            Hover(btnDelete,      DangerRedH,  DangerRed);
            Hover(btnClose,       SlateBlueH,  SlateBlue);
        }

        private static void Hover(Button btn, Color on, Color off)
        {
            btn.MouseEnter += (s, e) => btn.BackColor = on;
            btn.MouseLeave += (s, e) => btn.BackColor = off;
        }
    }
}
