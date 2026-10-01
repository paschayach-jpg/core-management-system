using System;
using System.Drawing;
using System.Linq;
using System.Windows.Forms;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Presentation
{
    /// <summary>Updates user Level (by employee code) and the 4-box position permissions from the authority Excel. Update-only, never inserts.</summary>
    public partial class AuthorityImportForm : Form
    {
        private static readonly string[] ActionText = { "ดู", "เพิ่ม", "แก้ไข", "อนุมัติ" };

        private readonly IAuthorityImportController _controller;
        private readonly UserContext _user;
        private AuthorityImportPreview _preview;

        public bool ImportCompleted { get; private set; }

        public AuthorityImportForm()
        {
            InitializeComponent();
        }

        public AuthorityImportForm(IAuthorityImportController controller, UserContext user)
        {
            if (controller == null) throw new ArgumentNullException("controller");
            if (user == null) throw new ArgumentNullException("user");
            _controller = controller;
            _user = user;
            InitializeComponent();
            ClearPreview();
        }

        private void btnBrowse_Click(object sender, EventArgs e)
        {
            using (var dlg = new OpenFileDialog { Filter = "Excel (*.xlsx)|*.xlsx", Title = "เลือกไฟล์ Excel" })
            {
                if (dlg.ShowDialog(this) != DialogResult.OK) return;
                try
                {
                    var sheets = _controller.GetSheetNames(dlg.FileName, _user);
                    txtFile.Text = dlg.FileName;
                    cboSheet.Items.Clear();
                    foreach (var s in sheets) cboSheet.Items.Add(s);
                    int idx = sheets.IndexOf("ชื่อพนักงาน");
                    cboSheet.SelectedIndex = idx >= 0 ? idx : (cboSheet.Items.Count > 0 ? 0 : -1);
                    btnPreview.Enabled = cboSheet.SelectedIndex >= 0;
                    ClearPreview();
                }
                catch (Exception ex)
                {
                    MessageBox.Show(this, ex.Message, "ผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
                }
            }
        }

        private void InputChanged(object sender, EventArgs e)
        {
            ClearPreview();
        }

        private void btnPreview_Click(object sender, EventArgs e)
        {
            try
            {
                Cursor = Cursors.WaitCursor;
                _preview = _controller.BuildPreview(txtFile.Text, cboSheet.Text, true, true, _user);
                BindPreview(_preview);
            }
            catch (Exception ex)
            {
                ClearPreview();
                MessageBox.Show(this, ex.Message, "ผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnCommit_Click(object sender, EventArgs e)
        {
            if (_preview == null || _preview.HasErrors || !_preview.HasChanges) return;
            string msg = string.Format("ยืนยันการอัปเดต?\n\nสิทธิ์ตามตำแหน่ง {0} ตำแหน่ง · เปลี่ยน Level ผู้ใช้ {1} คน",
                _preview.UpdatedCount, _preview.LevelChangeCount);
            if (MessageBox.Show(this, msg, "ยืนยัน", MessageBoxButtons.YesNo, MessageBoxIcon.Question) != DialogResult.Yes) return;

            try
            {
                Cursor = Cursors.WaitCursor;
                var result = _controller.Commit(_preview, _user);
                ImportCompleted = true;
                MessageBox.Show(this,
                    string.Format("อัปเดตสำเร็จ\nสิทธิ์ตามตำแหน่ง {0} · เปลี่ยน Level {1} คน", result.ConfigsUpdated, result.LevelsChanged),
                    "สำเร็จ", MessageBoxButtons.OK, MessageBoxIcon.Information);
                ClearPreview();
            }
            catch (Exception ex)
            {
                MessageBox.Show(this, ex.Message, "ผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
            finally
            {
                Cursor = Cursors.Default;
            }
        }

        private void btnClose_Click(object sender, EventArgs e)
        {
            Close();
        }

        private void ClearPreview()
        {
            _preview = null;
            dgvPlans.Rows.Clear();
            dgvLevels.Rows.Clear();
            dgvIssues.Rows.Clear();
            btnCommit.Enabled = false;
            lblSummary.ForeColor = SystemColors.ControlText;
            lblSummary.Text = string.IsNullOrEmpty(txtFile.Text) ? "เลือกไฟล์ Excel แล้วกด ตรวจสอบ" : "กด ตรวจสอบ เพื่อดูตัวอย่างก่อนนำเข้า";
            UpdateEmptyStates();
        }

        private void UpdateEmptyStates()
        {
            string idle = string.IsNullOrEmpty(txtFile.Text)
                ? "ยังไม่มีข้อมูล\n\nกด \"เลือกไฟล์…\" เพื่อเลือกไฟล์ Excel (.xlsx) ก่อน"
                : "ยังไม่มีข้อมูล\n\nกด \"ตรวจสอบ (Preview)\" เพื่อดูตัวอย่างก่อนนำเข้า";
            bool previewed = _preview != null;

            lblEmptyPlans.Text = previewed ? "ไม่มีตำแหน่งในไฟล์ที่ตรงกับ config ในระบบ" : idle;
            lblEmptyLevels.Text = previewed ? "ไม่พบรายการ Level ในไฟล์นี้" : idle;
            lblEmptyIssues.Text = previewed ? "ไม่พบข้อผิดพลาดหรือคำเตือน ✓" : idle;

            lblEmptyPlans.Visible = dgvPlans.Rows.Count == 0;
            lblEmptyLevels.Visible = dgvLevels.Rows.Count == 0;
            lblEmptyIssues.Visible = dgvIssues.Rows.Count == 0;

            tabPlans.Text = previewed ? string.Format("สิทธิ์ตามตำแหน่ง ({0})", dgvPlans.Rows.Count) : "สิทธิ์ตามตำแหน่ง (4 กล่อง)";
            tabLevels.Text = previewed ? string.Format("Level ผู้ใช้ ({0})", dgvLevels.Rows.Count) : "Level ผู้ใช้";
            tabIssues.Text = previewed ? string.Format("ข้อผิดพลาด/คำเตือน ({0})", dgvIssues.Rows.Count) : "ข้อผิดพลาด/คำเตือน";
        }

        private void BindPreview(AuthorityImportPreview p)
        {
            dgvPlans.Rows.Clear();
            dgvLevels.Rows.Clear();
            dgvIssues.Rows.Clear();

            foreach (var plan in p.PositionPlans)
            {
                int row = dgvPlans.Rows.Add(StatusText(plan.Status), plan.PositionName,
                    PermText(plan.Permissions, 0), PermText(plan.Permissions, 1),
                    PermText(plan.Permissions, 2), PermText(plan.Permissions, 3));
                dgvPlans.Rows[row].Cells[0].Style.ForeColor = StatusColor(plan.Status);
            }

            foreach (var lp in p.LevelPlans)
            {
                int row = dgvLevels.Rows.Add(StatusText(lp.Status), lp.EmployeeCode, lp.EmployeeName, lp.OldLevel, lp.NewLevel);
                dgvLevels.Rows[row].Cells[0].Style.ForeColor = StatusColor(lp.Status);
            }

            foreach (var issue in p.Issues.OrderByDescending(i => i.Severity).ThenBy(i => i.RowNumber))
            {
                bool isError = issue.Severity == AuthorityImportSeverity.Error;
                int idx = dgvIssues.Rows.Add(isError ? "Error" : "Warning",
                    issue.RowNumber == 0 ? "" : issue.RowNumber.ToString(), issue.Code, issue.Message, FixHint(issue.Code));
                if (isError)
                    dgvIssues.Rows[idx].DefaultCellStyle.ForeColor = Color.Firebrick;
            }

            int errors = p.Issues.Count(i => i.Severity == AuthorityImportSeverity.Error);
            int warnings = p.Issues.Count - errors;
            lblSummary.Text = string.Format("สิทธิ์ตำแหน่ง: อัปเดต {5} · ไม่เปลี่ยน {6} | Level: เปลี่ยน {0} คน · ไม่เปลี่ยน {1} · ไม่พบผู้ใช้ {2} | Error {3} · Warning {4}",
                p.LevelChangeCount,
                p.LevelPlans.Count(x => x.Status == AuthorityImportStatus.Unchanged),
                p.LevelPlans.Count(x => x.Status == AuthorityImportStatus.NotFound),
                errors, warnings, p.UpdatedCount, p.UnchangedCount);
            btnCommit.Enabled = !p.HasErrors && p.HasChanges;
            if (errors > 0)
            {
                lblSummary.ForeColor = Color.Firebrick;
                lblSummary.Text += "  — ดูวิธีแก้ในแท็บข้อผิดพลาด แก้ไฟล์แล้วกด ตรวจสอบ ใหม่";
                tabs.SelectedTab = tabIssues;
            }
            else if (!p.HasChanges)
            {
                lblSummary.ForeColor = Color.DimGray;
                lblSummary.Text += "  — ไม่มีอะไรต้องเปลี่ยน";
            }
            else
            {
                lblSummary.ForeColor = warnings > 0 ? Color.DarkOrange : Color.SeaGreen;
                lblSummary.Text += "  — พร้อมนำเข้า";            }
            UpdateEmptyStates();
        }

        /// <summary>What the user should do in the Excel file (or screen) for each issue code.</summary>
        private static string FixHint(string code)
        {
            switch (code)
            {
                case "POSITION_NOT_FOUND": return "ตำแหน่งนี้ยังไม่มี config ในระบบ ตรวจชื่อตำแหน่งให้ตรงกับหน้า Config สิทธิ์ หรือให้ผู้ดูแลสร้าง config ก่อน (ข้ามได้)";
                case "POSITION_MERGED": return "ตำแหน่งเดียวกันติ๊กสิทธิ์ไม่เหมือนกัน ระบบรวมให้แล้ว ถ้าไม่ต้องการ ให้แก้ทุกแถวของตำแหน่งนี้ให้เหมือนกัน";
                case "MISSING_POSITION": return "กรอกชื่อตำแหน่งให้แถวนี้ ถ้าต้องการให้อัปเดตสิทธิ์ (ไม่กรอกก็ได้ Level ยังอัปเดต)";
                case "REMARK": return "เป็นหมายเหตุในไฟล์ ไม่ต้องแก้ ตรวจดูว่ายังต้องมีแถวนี้หรือไม่";
                case "APPROVE_WITHOUT_VIEW": return "ตรวจว่าตั้งใจให้อนุมัติได้โดยไม่มีสิทธิ์ดูหรือไม่ ถ้าไม่ ให้ติ๊กช่อง ดู เพิ่ม";
                case "DUPLICATE_EMPLOYEE": return "ลบแถวที่รหัสพนักงานซ้ำออก ให้เหลือแถวเดียว แล้วบันทึกไฟล์";
                case "INVALID_LEVEL": return "แก้คอลัมน์ Level (E) เป็นเลข 1–9 ตามด้วยตัวอักษรใหญ่ เช่น 3A";
                case "USER_NOT_FOUND": return "ไม่ต้องแก้ก็ได้ (ข้ามคนนี้) หรือให้พนักงานเข้าระบบครั้งแรกก่อน แล้วนำเข้าใหม่";
                case "NUMERIC_EMPLOYEE_CODE": return "ตั้งคอลัมน์รหัสพนักงานเป็น Text เพื่อไม่ให้เลข 0 นำหน้าหาย (เช่น 01244)";
                case "SKIPPED_ROW": return "แถวนี้ไม่มีรหัสพนักงาน จึงถูกข้าม ถ้าเป็นแถวว่างไม่ต้องแก้";
                case "SHEET_NOT_FOUND": return "เลือก Sheet ให้ถูกต้องในขั้นตอน ②";
                case "TEMPLATE_MISMATCH": return "ใช้ไฟล์ template เดิม (หัวคอลัมน์ B = รหัสพนักงาน, F = ตำแหน่ง และหัวกลุ่ม HC/SC/NC/PC) อย่าลบหรือย้ายคอลัมน์";
                case "NO_DATA": return "ไม่พบแถวข้อมูล ตรวจว่าเลือก Sheet ถูกและข้อมูลเริ่มที่แถว 3";
                default: return "";
            }
        }

        private static string PermText(bool[] perms, int levelIndex)
        {
            var on = new System.Collections.Generic.List<string>();
            for (int a = 0; a < 4; a++)
                if (perms[levelIndex * 4 + a]) on.Add(ActionText[a]);
            return on.Count == 0 ? "-" : string.Join(", ", on.ToArray());
        }

        private static Color StatusColor(AuthorityImportStatus s)
        {
            switch (s)
            {
                case AuthorityImportStatus.Updated: return Color.SteelBlue;
                case AuthorityImportStatus.Unchanged: return Color.Gray;
                default: return Color.Firebrick;
            }
        }

        private static string StatusText(AuthorityImportStatus s)
        {
            switch (s)
            {
                case AuthorityImportStatus.New: return "ใหม่";
                case AuthorityImportStatus.Updated: return "อัปเดต";
                case AuthorityImportStatus.Unchanged: return "ไม่เปลี่ยน";
                default: return "ไม่พบผู้ใช้";
            }
        }
    }
}
