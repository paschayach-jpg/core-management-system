using System;
using System.Drawing;
using System.IO;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.DMS
{
    public partial class OrgMasterImportDialog : Form
    {
        private readonly IOrgMasterController _controller;
        private readonly UserContext _user;
        private OrgMasterPreview _preview;

        public bool HasImported { get; private set; }

        public OrgMasterImportDialog(IOrgMasterController controller, UserContext user)
        {
            _controller = controller ?? throw new ArgumentNullException(nameof(controller));
            _user = user ?? throw new ArgumentNullException(nameof(user));

            InitializeComponent();
            SetupGridColumns();
        }

        private void SetupGridColumns()
        {
            dgvPreview.Columns.Clear();
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "colRow", HeaderText = "แถวที่", Width = 60 });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "colStatus", HeaderText = "สถานะ", Width = 90 });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "colType", HeaderText = "ประเภท", Width = 90 });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDiv", HeaderText = "ฝ่าย (Div)", Width = 140 });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "colDept", HeaderText = "แผนก (Dept)", Width = 160 });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "colPos", HeaderText = "ตำแหน่ง (Position)", Width = 160 });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "colLevel", HeaderText = "ระดับ (Level)", Width = 90 });
            dgvPreview.Columns.Add(new DataGridViewTextBoxColumn { Name = "colMsg", HeaderText = "หมายเหตุ / ข้อผิดพลาด", AutoSizeMode = DataGridViewAutoSizeColumnMode.Fill });
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var ofd = new OpenFileDialog
            {
                Filter = "Excel Files (*.xlsx;*.xls)|*.xlsx;*.xls|All Files (*.*)|*.*",
                Title = "เลือกไฟล์ Master Data Excel สำหรับนำเข้า"
            })
            {
                if (ofd.ShowDialog(this) == DialogResult.OK)
                {
                    txtFilePath.Text = ofd.FileName;
                    LoadSheetNames(ofd.FileName);
                }
            }
        }

        private void LoadSheetNames(string filePath)
        {
            try
            {
                cboSheet.Items.Clear();
                var sheets = _controller.GetExcelSheetNames(filePath);
                foreach (var s in sheets)
                {
                    cboSheet.Items.Add(s);
                }
                if (cboSheet.Items.Count > 0)
                {
                    cboSheet.SelectedIndex = 0;
                    LoadPreviewData();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("ไม่สามารถอ่านไฟล์ Excel ได้: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void cboSheet_SelectedIndexChanged(object sender, EventArgs e)
        {
            if (!string.IsNullOrEmpty(txtFilePath.Text))
            {
                LoadPreviewData();
            }
        }

        private void btnLoadPreview_Click(object sender, EventArgs e)
        {
            LoadPreviewData();
        }

        private void LoadPreviewData()
        {
            if (string.IsNullOrEmpty(txtFilePath.Text) || !File.Exists(txtFilePath.Text))
            {
                MessageBox.Show("กรุณาเลือกไฟล์ Excel ที่มีอยู่จริง", "แจ้งเตือน", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            try
            {
                string sheetName = cboSheet.SelectedItem?.ToString();
                _preview = _controller.ParseAndValidateExcel(txtFilePath.Text, sheetName);

                dgvPreview.Rows.Clear();
                foreach (var r in _preview.Rows)
                {
                    string divDisplay = !string.IsNullOrEmpty(r.DivisionName) ? $"[{r.DivisionCode}] {r.DivisionName}" : r.DivisionCode;
                    string deptDisplay = !string.IsNullOrEmpty(r.DepartmentNameTh) ? $"[{r.DepartmentCode}] {r.DepartmentNameTh}" : r.DepartmentCode;
                    string statusText = r.Status == OrgMasterImportStatus.Error ? "❌ ผิดพลาด"
                        : r.Status == OrgMasterImportStatus.Warning ? "⚠️ เตือน"
                        : r.Status == OrgMasterImportStatus.ValidUpdate ? "🟡 อัปเดต" : "🟢 พร้อมบันทึก";

                    int rowIndex = dgvPreview.Rows.Add(
                        r.RowIndex,
                        statusText,
                        r.MasterType.ToString(),
                        divDisplay,
                        deptDisplay,
                        r.PositionName,
                        r.PositionLevel ?? r.LevelCode,
                        r.ErrorMessage
                    );

                    if (r.Status == OrgMasterImportStatus.Error)
                    {
                        dgvPreview.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 235, 235);
                        dgvPreview.Rows[rowIndex].DefaultCellStyle.ForeColor = Color.DarkRed;
                    }
                    else if (r.Status == OrgMasterImportStatus.Warning)
                    {
                        dgvPreview.Rows[rowIndex].DefaultCellStyle.BackColor = Color.FromArgb(255, 250, 230);
                    }
                }

                lblSummary.Text = $"ตรวจสอบพบทั้งหมด {_preview.TotalCount} รายการ | พร้อมนำเข้า: {_preview.ValidCount} | ผิดพลาด: {_preview.ErrorCount}";
                lblSummary.ForeColor = _preview.ErrorCount > 0 ? Color.DarkRed : Color.FromArgb(30, 110, 50);

                btnImport.Enabled = _preview.ValidCount > 0;
            }
            catch (Exception ex)
            {
                MessageBox.Show("เกิดข้อผิดพลาดในการตรวจสอบไฟล์: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
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

        private void btnImport_Click(object sender, EventArgs e)
        {
            if (_preview == null || _preview.ValidCount == 0) return;

            var confirm = MessageBox.Show(
                $"ยืนยันการนำเข้าข้อมูล Master จำนวน {_preview.ValidCount} รายการ เข้าฐานข้อมูลหรือไม่?",
                "ยืนยันการนำเข้า",
                MessageBoxButtons.YesNo,
                MessageBoxIcon.Question);

            if (confirm == DialogResult.Yes)
            {
                try
                {
                    var result = _controller.ExecuteImport(_preview, _user.UserID);
                    MessageBox.Show(result.Message, "ผลการนำเข้า", MessageBoxButtons.OK, MessageBoxIcon.Information);
                    HasImported = true;
                    this.DialogResult = DialogResult.OK;
                    this.Close();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("เกิดข้อผิดพลาดขณะนำเข้า: " + ex.Message, "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            this.Close();
        }
    }
}
