using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Text;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// ดึงข้อมูลพนักงานจาก Employee REST API
    /// Config keys: EmployeeApiBaseUrl, EmployeeApiToken (optional)
    /// </summary>
    public class EmployeeApiService : IEmployeeApiService
    {
        private readonly string _baseUrl;
        private readonly string _apiToken;

        private List<EmployeeApiResponse> _cache;
        private DateTime _cacheTime = DateTime.MinValue;
        private static readonly TimeSpan CacheDuration = TimeSpan.FromMinutes(10);

        public EmployeeApiService(IConfigManager configManager)
        {
            if (configManager == null) throw new ArgumentNullException("configManager");
            try { _baseUrl  = configManager.Get<string>("EmployeeApiBaseUrl"); } catch { _baseUrl = string.Empty; }
            try { _apiToken = configManager.Get<string>("EmployeeApiToken");   } catch { _apiToken = string.Empty; }
        }

        public IEnumerable<EmployeeSummary> GetAllEmployees()
        {
            return LoadAll().Select(ToSummary);
        }

        public EmployeeApiResponse GetByCode(string employeeCode)
        {
            // ลอง call endpoint รายคนก่อน: GET /api/Employee/{code}
            if (!string.IsNullOrEmpty(_baseUrl) && !string.IsNullOrEmpty(employeeCode))
            {
                try
                {
                    string json = FetchJson(string.Format("{0}/Employee/{1}",
                        _baseUrl.TrimEnd('/'), employeeCode));
                    var list = ParseJson(json);
                    if (list.Count > 0) return list[0];
                }
                catch { /* fallback to cache */ }
            }

            // fallback: หาจาก cache ทั้งหมด
            return LoadAll().FirstOrDefault(e =>
                string.Equals(e.EmployeeCode, employeeCode, StringComparison.OrdinalIgnoreCase));
        }

        public IEnumerable<DepartmentSummary> GetDepartments()
        {
            return LoadAll()
                .Where(e => e.PrimaryPosition != null)
                .Select(e => e.PrimaryPosition)
                .GroupBy(p => p.DepartmentId)
                .Select(g => new DepartmentSummary
                {
                    DepartmentId   = g.Key,
                    DepartmentCode = g.First().DepartmentCode,
                    NameTh         = g.First().DepartmentNameTh,
                    NameEn         = g.First().DepartmentNameEn
                })
                .OrderBy(d => d.DisplayName)
                .ToList();
        }

        public IEnumerable<DivisionSummary> GetDivisions()
        {
            return LoadAll()
                .Where(e => e.PrimaryPosition != null)
                .Select(e => e.PrimaryPosition)
                .GroupBy(p => p.DivisionId)
                .Select(g => new DivisionSummary
                {
                    DivisionId   = g.Key,
                    DivisionCode = g.First().DivisionCode,
                    NameTh       = g.First().DivisionNameTh,
                    NameEn       = g.First().DivisionNameEn
                })
                .OrderBy(d => d.DisplayName)
                .ToList();
        }

        public IEnumerable<EmployeeSummary> GetByDepartment(int departmentId)
        {
            return LoadAll()
                .Where(e => e.PrimaryPosition != null && e.PrimaryPosition.DepartmentId == departmentId)
                .Select(ToSummary);
        }

        public IEnumerable<EmployeeSummary> GetByDivision(int divisionId)
        {
            return LoadAll()
                .Where(e => e.PrimaryPosition != null && e.PrimaryPosition.DivisionId == divisionId)
                .Select(ToSummary);
        }

        // ── Private ──────────────────────────────────────────────────────────

        private List<EmployeeApiResponse> LoadAll()
        {
            if (_cache != null && DateTime.Now - _cacheTime < CacheDuration)
                return _cache;

            if (string.IsNullOrEmpty(_baseUrl))
                throw new InvalidOperationException("ไม่พบ EmployeeApiBaseUrl ใน config");

            // endpoint: GET /api/Employee
            string json = FetchJson(_baseUrl.TrimEnd('/') + "/Employee");
            _cache     = ParseJson(json);
            _cacheTime = DateTime.Now;
            return _cache;
        }

        private string FetchJson(string url)
        {
            var req = (HttpWebRequest)WebRequest.Create(url);
            req.Method  = "GET";
            req.Timeout = 10000;
            req.Accept  = "application/json";
            if (!string.IsNullOrEmpty(_apiToken))
                req.Headers.Add("Authorization", "Bearer " + _apiToken);

            using (var resp   = (HttpWebResponse)req.GetResponse())
            using (var stream = resp.GetResponseStream())
            using (var reader = new StreamReader(stream, Encoding.UTF8))
                return reader.ReadToEnd();
        }

        private static List<EmployeeApiResponse> ParseJson(string json)
        {
            if (string.IsNullOrEmpty(json)) return new List<EmployeeApiResponse>();
            string s = json.Trim();
            if (!s.StartsWith("[")) s = "[" + s + "]";

            var ser = new DataContractJsonSerializer(typeof(List<EmployeeDto>));
            List<EmployeeDto> dtos;
            using (var ms = new MemoryStream(Encoding.UTF8.GetBytes(s)))
                dtos = (List<EmployeeDto>)ser.ReadObject(ms);

            return dtos.Select(d => new EmployeeApiResponse
            {
                EmployeeCode = d.EmployeeCode,
                PrefixId     = d.PrefixId,
                FullNameEn   = d.FullNameEn,
                FullNameTh   = d.FullNameTh,
                Company      = d.Company,
                Remark       = d.Remark,
                Positions    = (d.Positions ?? new List<PositionDto>()).Select(p => new EmployeePosition
                {
                    Id               = p.Id,
                    DepartmentId     = p.DepartmentId,
                    DepartmentCode   = p.DepartmentCode,
                    DepartmentNameEn = p.DepartmentNameEn,
                    DepartmentNameTh = p.DepartmentNameTh,
                    DivisionId       = p.DivisionId,
                    DivisionCode     = p.DivisionCode,
                    DivisionNameEn   = p.DivisionNameEn,
                    DivisionNameTh   = p.DivisionNameTh,
                    PositionName     = p.Position,
                    Level            = p.Level,
                    Remark           = p.Remark,
                    DateBegin        = p.DateBegin,
                    DateEnd          = p.DateEnd
                }).ToList()
            }).ToList();
        }

        private static EmployeeSummary ToSummary(EmployeeApiResponse emp)
        {
            var pos = emp.PrimaryPosition;
            return new EmployeeSummary
            {
                EmployeeCode     = emp.EmployeeCode,
                DisplayName      = emp.DisplayName,
                FullNameEn       = emp.FullNameEn,
                FullNameTh       = emp.FullNameTh,
                Company          = emp.Company,
                DepartmentId     = pos != null ? pos.DepartmentId     : 0,
                DepartmentCode   = pos != null ? pos.DepartmentCode   : string.Empty,
                DepartmentNameTh = pos != null ? pos.DepartmentNameTh : string.Empty,
                DepartmentNameEn = pos != null ? pos.DepartmentNameEn : string.Empty,
                DivisionId       = pos != null ? pos.DivisionId       : 0,
                DivisionCode     = pos != null ? pos.DivisionCode     : string.Empty,
                DivisionNameTh   = pos != null ? pos.DivisionNameTh   : string.Empty,
                DivisionNameEn   = pos != null ? pos.DivisionNameEn   : string.Empty
            };
        }

        // ── Internal DTOs ────────────────────────────────────────────────────

        [DataContract]
        private class EmployeeDto
        {
            [DataMember(Name = "employeeCode")] public string EmployeeCode { get; set; }
            [DataMember(Name = "prefixId")]     public int    PrefixId     { get; set; }
            [DataMember(Name = "fullNameEn")]   public string FullNameEn   { get; set; }
            [DataMember(Name = "fullNameTh")]   public string FullNameTh   { get; set; }
            [DataMember(Name = "company")]      public string Company      { get; set; }
            [DataMember(Name = "remark")]       public string Remark       { get; set; }
            [DataMember(Name = "position")]     public List<PositionDto> Positions { get; set; }
        }

        [DataContract]
        private class PositionDto
        {
            [DataMember(Name = "id")]               public int    Id               { get; set; }
            [DataMember(Name = "departmentId")]     public int    DepartmentId     { get; set; }
            [DataMember(Name = "departmentCode")]   public string DepartmentCode   { get; set; }
            [DataMember(Name = "departmentNameEn")] public string DepartmentNameEn { get; set; }
            [DataMember(Name = "departmentNameTh")] public string DepartmentNameTh { get; set; }
            [DataMember(Name = "divisionId")]       public int    DivisionId       { get; set; }
            [DataMember(Name = "divisionCode")]     public string DivisionCode     { get; set; }
            [DataMember(Name = "divisionNameEn")]   public string DivisionNameEn   { get; set; }
            [DataMember(Name = "divisionNameTh")]   public string DivisionNameTh   { get; set; }
            [DataMember(Name = "position")]         public string Position         { get; set; }
            [DataMember(Name = "level")]            public string Level            { get; set; }
            [DataMember(Name = "remark")]           public string Remark           { get; set; }
            // ใช้ string แทน DateTime? เพราะ DataContractJsonSerializer ไม่รองรับ null DateTime
            [DataMember(Name = "dateBegin")]        public string DateBeginRaw     { get; set; }
            [DataMember(Name = "dateEnd")]          public string DateEndRaw       { get; set; }

            public DateTime? DateBegin
            {
                get
                {
                    DateTime dt;
                    return DateTime.TryParse(DateBeginRaw, out dt) ? dt : (DateTime?)null;
                }
            }
            public DateTime? DateEnd
            {
                get
                {
                    DateTime dt;
                    return DateTime.TryParse(DateEndRaw, out dt) ? dt : (DateTime?)null;
                }
            }
        }
    }
}
