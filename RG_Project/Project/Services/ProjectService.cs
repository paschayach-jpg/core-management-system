using RG_Project.Project.Controllers;
using RG_Project.Project.Models;
using RG_Project.Controllers;
using System;
using System.Data;
using System.Data.SqlClient;
using System.Windows.Forms;

namespace RG_Project.Project.Services
{
    public class ProjectService
    {
        private readonly ProjectController          _projectCtrl;
        private readonly ProjectStateController     _stateCtrl;
        private readonly ProjectDocumentController  _docCtrl;

        public ProjectService()
        {
            _projectCtrl = new ProjectController();
            _stateCtrl   = new ProjectStateController();
            _docCtrl     = new ProjectDocumentController();
        }

        /// <summary>
        /// สร้าง Running Number จากตาราง RunningNumber WHERE running_type = 'PRJ'
        /// รองรับ format เช่น PPP-YY-###### และ reset counter เมื่อขึ้นปีใหม่
        /// </summary>
        public string GenerateProjectNo()
        {
            string projectNo = "";
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            using (SqlTransaction tx = conn.connection.BeginTransaction())
            {
                try
                {
                    string sqlSelect = @"
                        SELECT current_number, prefix, format, digit_number, Year_UW
                        FROM RunningNumber WITH (UPDLOCK, ROWLOCK)
                        WHERE running_type = 'PRJ'";

                    int    currentNum = 0;
                    string prefix     = "PRJ";
                    string fmt        = "PPP-YY-######";
                    int    digits     = 6;
                    int    yearUW     = 0;

                    using (SqlCommand cmd = new SqlCommand(sqlSelect, conn.connection, tx))
                    using (SqlDataReader reader = cmd.ExecuteReader())
                    {
                        if (!reader.Read())
                            throw new Exception("ไม่พบข้อมูล RunningNumber สำหรับ running_type = 'PRJ'");

                        currentNum = Convert.ToInt32(reader["current_number"]);
                        prefix     = reader["prefix"].ToString();
                        fmt        = reader["format"].ToString();
                        digits     = Convert.ToInt32(reader["digit_number"]);
                        yearUW     = Convert.ToInt32(reader["Year_UW"]);
                    }

                    int currentYear = DateTime.Now.Year;

                    // Reset counter เมื่อขึ้นปีใหม่
                    if (yearUW != currentYear)
                        currentNum = 0;

                    int nextNum = currentNum + 1;

                    string sqlUpdate = @"
                        UPDATE RunningNumber
                        SET current_number = @next, Year_UW = @year
                        WHERE running_type = 'PRJ'";

                    using (SqlCommand cmd = new SqlCommand(sqlUpdate, conn.connection, tx))
                    {
                        cmd.Parameters.AddWithValue("@next", nextNum);
                        cmd.Parameters.AddWithValue("@year", currentYear);
                        cmd.ExecuteNonQuery();
                    }

                    tx.Commit();

                    string yy     = DateTime.Now.ToString("yy");
                    string yymm   = DateTime.Now.ToString("yyMM");
                    string numStr = nextNum.ToString().PadLeft(digits, '0');

                    projectNo = fmt
                        .Replace("PPP", prefix)
                        .Replace("YYMM", yymm)
                        .Replace("YY",   yy)
                        .Replace("######", numStr);
                }
                catch (Exception ex)
                {
                    tx.Rollback();
                    projectNo = "";
                    System.Windows.Forms.MessageBox.Show(
                        "GenerateProjectNo error: " + ex.Message,
                        "Error", System.Windows.Forms.MessageBoxButtons.OK,
                        System.Windows.Forms.MessageBoxIcon.Error);
                }
            }
            conn.Disconnect();
            return projectNo;
        }

        /// <summary>
        /// ตรวจสอบว่า Project No. มีในระบบแล้วหรือไม่
        /// </summary>
        public bool IsProjectNoExists(string c_project_no)
        {
            return _projectCtrl.IsProjectNoExists(c_project_no);
        }

        /// <summary>
        /// สร้าง Project ใหม่: Insert → Copy States จาก Template
        /// ถ้า model.c_project_no ว่าง จะ Auto Generate ให้อัตโนมัติ
        /// คืนค่า i_project_id ใหม่ หรือ -1 ถ้า fail
        /// </summary>
        public int CreateProject(ProjectModel model)
        {
            try
            {
                if (string.IsNullOrEmpty(model.c_project_no))
                {
                    model.c_project_no = GenerateProjectNo();
                    if (string.IsNullOrEmpty(model.c_project_no))
                        return -1;
                }

                model.c_status = "Open";
                int newId = _projectCtrl.InsertProject(model);

                if (newId > 0 && model.i_template_id.HasValue)
                {
                    // 1. Copy States จาก template → t_project_state
                    _stateCtrl.InsertStatesFromTemplate(newId, model.i_template_id.Value, model.i_create_user);

                    // 2. Copy Document Workflow จาก m_template_doc → t_project_doc_workflow (auto-populate)
                    _docCtrl.InsertDocWorkflowFromTemplate(newId, model.i_create_user);
                }

                return newId;
            }
            catch
            {
                return -1;
            }
        }

        /// <summary>
        /// Update Project header
        /// </summary>
        public bool UpdateProject(ProjectModel model)
        {
            return _projectCtrl.UpdateProject(model);
        }

        public bool UpdateLegalDictJson(int i_project_id, string json, string i_user) =>
            _projectCtrl.UpdateLegalDictJson(i_project_id, json, i_user);

        /// <summary>
        /// ลบ Project (soft-delete) — blocked ถ้ามี progress แล้ว
        /// </summary>
        public bool DeleteProject(int i_project_id, string i_user, out string errorMessage)
        {
            errorMessage = "";
            if (_projectCtrl.HasActiveProgress(i_project_id))
            {
                errorMessage = "ไม่สามารถลบโครงการได้ เนื่องจากมีการอัปเดตความคืบหน้าแล้ว";
                return false;
            }
            return _projectCtrl.DeleteProject(i_project_id, i_user);
        }

        /// <summary>
        /// ปิด Project — blocked ถ้า required states ยังไม่ครบ Completed
        /// </summary>
        public bool CloseProject(int i_project_id, string i_user, out string errorMessage)
        {
            errorMessage = "";
            if (!_projectCtrl.AllRequiredStatesCompleted(i_project_id))
            {
                errorMessage = "ยังมีขั้นตอนที่จำเป็น (Required) ยังไม่เสร็จสมบูรณ์";
                return false;
            }
            return _projectCtrl.CloseProject(i_project_id, i_user);
        }

        /// <summary>
        /// ค้นหาโครงการ
        /// </summary>
        public DataTable SearchProjects(string c_project_no, string n_project_name,
            string c_status, string i_template_id, string d_from, string d_to)
        {
            return _projectCtrl.SearchProjects(c_project_no, n_project_name,
                c_status, i_template_id, d_from, d_to);
        }

        /// <summary>
        /// ดึง Templates ทั้งหมด (f_active='Y') สำหรับ ComboBox
        /// </summary>
        public DataTable GetTemplates()
        {
            DataTable dt = new DataTable();
            ConectionDB conn = new ConectionDB();
            conn.Connect('c');
            string sql = @"
                SELECT i_template_id, c_template_code, n_template_name
                FROM m_project_template
                WHERE f_active = 'Y'
                ORDER BY n_template_name";
            using (SqlCommand cmd = new SqlCommand(sql, conn.connection))
            using (SqlDataReader reader = cmd.ExecuteReader())
            {
                dt.Load(reader);
            }
            conn.Disconnect();
            return dt;
        }

        /// <summary>
        /// ดึงข้อมูล Project เดียว
        /// </summary>
        public DataTable GetProjectById(int i_project_id)
        {
            return _projectCtrl.GetProjectById(i_project_id);
        }
    }
}
