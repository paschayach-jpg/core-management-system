using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Data.Interfaces;
using System;
using System.Configuration;
using System.Data;
using System.Data.SqlClient;
using System.Net.Http;
using System.Text;
using Newtonsoft.Json;

namespace CMS.Data.Repositories
{
    public class EFilingRepository : IEFilingRepository
    {
        private readonly IConfigManager _configManager;
        private readonly string _apiBaseUrl;

        public EFilingRepository(IConfigManager configManager)
        {
            _configManager = configManager;
            _apiBaseUrl = ConfigurationManager.AppSettings["apiEFiling"] ?? "";
        }

        /// <summary>
        /// ค้นหาเอกสารใน E-Filing ผ่าน REST API
        /// Returns DataTable with columns: c_efiling_ref_no, n_doc_name, c_doc_type, d_doc_date, c_status
        /// </summary>
        public DataTable SearchEFilingDoc(string c_efiling_ref_no, string n_doc_name)
        {
            DataTable dt = new DataTable();
            dt.Columns.Add("c_efiling_ref_no", typeof(string));
            dt.Columns.Add("n_doc_name",       typeof(string));
            dt.Columns.Add("c_doc_type",       typeof(string));
            dt.Columns.Add("d_doc_date",       typeof(string));
            dt.Columns.Add("c_status",         typeof(string));

            try
            {
                if (string.IsNullOrEmpty(_apiBaseUrl))
                    return dt;

                string url = _apiBaseUrl + "/search";
                var payload = new
                {
                    refNo   = c_efiling_ref_no ?? "",
                    docName = n_doc_name       ?? ""
                };

                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(30);
                    string jsonBody = JsonConvert.SerializeObject(payload);
                    var content = new StringContent(jsonBody, Encoding.UTF8, "application/json");
                    HttpResponseMessage response = client.PostAsync(url, content).Result;

                    if (response.IsSuccessStatusCode)
                    {
                        string json = response.Content.ReadAsStringAsync().Result;
                        var result = JsonConvert.DeserializeObject<EFilingSearchResult[]>(json);
                        if (result != null)
                        {
                            foreach (var item in result)
                            {
                                dt.Rows.Add(
                                    item.refNo   ?? "",
                                    item.docName ?? "",
                                    item.docType ?? "",
                                    item.docDate ?? "",
                                    item.status  ?? ""
                                );
                            }
                        }
                    }
                }
            }
            catch (Exception)
            {
                // Return empty DataTable on error; caller shows error msg
            }
            return dt;
        }

        /// <summary>
        /// ตรวจสอบว่า Reference No มีอยู่ใน E-Filing หรือไม่
        /// </summary>
        public bool CheckEFilingRefExists(string c_efiling_ref_no)
        {
            try
            {
                if (string.IsNullOrEmpty(_apiBaseUrl) || string.IsNullOrEmpty(c_efiling_ref_no))
                    return false;

                string url = _apiBaseUrl + "/check?refNo=" + Uri.EscapeDataString(c_efiling_ref_no);
                using (HttpClient client = new HttpClient())
                {
                    client.Timeout = TimeSpan.FromSeconds(15);
                    HttpResponseMessage response = client.GetAsync(url).Result;
                    return response.IsSuccessStatusCode;
                }
            }
            catch
            {
                return false;
            }
        }

        // ─── Write to Claim DB ────────────────────────────────────────────────

        /// <summary>
        /// บันทึก E-Filing Request log ลง Claim DB คืนค่า i_efiling_req_id
        /// </summary>
        public int InsertEFilingRequest(EFilingRequestModel model)
        {
            int newId = -1;
            using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ProjectDbConnectionString")))
            {
                conn.Open();
            string sql = @"
                INSERT INTO t_project_efiling_request
                    (i_proj_state_id, i_project_id, c_efiling_ref_no, n_doc_name,
                     c_request_status, d_request_date, n_remark, i_request_user)
                OUTPUT INSERTED.i_efiling_req_id
                VALUES
                    (@i_proj_state_id, @i_project_id, @c_efiling_ref_no, @n_doc_name,
                     'Requested', GETDATE(), @n_remark, @i_request_user)";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@i_proj_state_id",  model.i_proj_state_id);
                cmd.Parameters.AddWithValue("@i_project_id",     model.i_project_id);
                cmd.Parameters.AddWithValue("@c_efiling_ref_no",
                    string.IsNullOrEmpty(model.c_efiling_ref_no) ? (object)DBNull.Value : model.c_efiling_ref_no);
                cmd.Parameters.AddWithValue("@n_doc_name",       model.n_doc_name);
                cmd.Parameters.AddWithValue("@n_remark",
                    string.IsNullOrEmpty(model.n_remark) ? (object)DBNull.Value : model.n_remark);
                cmd.Parameters.AddWithValue("@i_request_user",   model.i_request_user);
                object result = cmd.ExecuteScalar();
                if (result != null && result != DBNull.Value)
                    newId = Convert.ToInt32(result);
            }
            }
            return newId;
        }

        /// <summary>
        /// Update สถานะ E-Filing Request (Received / NotFound)
        /// </summary>
        public bool UpdateEFilingRequestStatus(int i_efiling_req_id, string c_request_status,
            DateTime? d_receive_date, string n_remark, string i_user)
        {
            
            int rows = 0;
using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ProjectDbConnectionString")))
            {
                conn.Open();
            string sql = @"
                UPDATE t_project_efiling_request SET
                    c_request_status = @c_request_status,
                    d_receive_date   = @d_receive_date,
                    n_remark         = @n_remark,
                    i_update_user    = @i_user,
                    d_update_date    = GETDATE()
                WHERE i_efiling_req_id = @id";            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@c_request_status", c_request_status);
                cmd.Parameters.AddWithValue("@d_receive_date",
                    d_receive_date.HasValue ? (object)d_receive_date.Value : DBNull.Value);
                cmd.Parameters.AddWithValue("@n_remark",
                    string.IsNullOrEmpty(n_remark) ? (object)DBNull.Value : n_remark);
                cmd.Parameters.AddWithValue("@i_user", i_user);
                cmd.Parameters.AddWithValue("@id",     i_efiling_req_id);
                rows = cmd.ExecuteNonQuery();
            }
            }
            return rows > 0;
        }

        /// <summary>
        /// ดึง E-Filing Requests ทั้งหมดของ Project
        /// </summary>
        public DataTable GetEFilingRequestsByProject(int i_project_id)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ProjectDbConnectionString")))
            {
                conn.Open();
            string sql = @"
                SELECT
                    er.i_efiling_req_id,
                    er.i_proj_state_id,
                    er.i_project_id,
                    er.c_efiling_ref_no,
                    er.n_doc_name,
                    er.c_request_status,
                    er.d_request_date,
                    er.d_receive_date,
                    er.n_remark,
                    er.i_request_user,
                    er.i_update_user,
                    er.d_update_date,
                    ps.n_state_name
                FROM t_project_efiling_request er
                JOIN t_project_state ps ON ps.i_proj_state_id = er.i_proj_state_id
                WHERE er.i_project_id = @i_project_id
                ORDER BY er.d_request_date DESC";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@i_project_id", i_project_id);
                using (SqlDataReader reader = cmd.ExecuteReader()) { dt.Load(reader); }
            }
            }
            return dt;
        }

        /// <summary>
        /// ดึง E-Filing Requests ของ State เดียว
        /// </summary>
        public DataTable GetEFilingRequestsByState(int i_proj_state_id)
        {
            DataTable dt = new DataTable();
            using (SqlConnection conn = new SqlConnection(_configManager.Get<string>("ProjectDbConnectionString")))
            {
                conn.Open();
            string sql = @"
                SELECT
                    er.i_efiling_req_id,
                    er.i_proj_state_id,
                    er.i_project_id,
                    er.c_efiling_ref_no,
                    er.n_doc_name,
                    er.c_request_status,
                    er.d_request_date,
                    er.d_receive_date,
                    er.n_remark,
                    er.i_request_user,
                    ps.n_state_name
                FROM t_project_efiling_request er
                JOIN t_project_state ps ON ps.i_proj_state_id = er.i_proj_state_id
                WHERE er.i_proj_state_id = @i_proj_state_id
                ORDER BY er.d_request_date DESC";
            using (SqlCommand cmd = new SqlCommand(sql, conn))
            {
                cmd.Parameters.AddWithValue("@i_proj_state_id", i_proj_state_id);
                using (SqlDataReader reader = cmd.ExecuteReader()) { dt.Load(reader); }
            }
            }
            return dt;
        }

        // ─── Internal DTO for JSON deserialization ─────────────────────────────
        private class EFilingSearchResult
        {
            public string refNo   { get; set; }
            public string docName { get; set; }
            public string docType { get; set; }
            public string docDate { get; set; }
            public string status  { get; set; }
        }
    }
}
