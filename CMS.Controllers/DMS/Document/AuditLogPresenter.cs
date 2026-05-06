using System;
using System.IO;
using System.Text;
using CMS.Data.Models;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    /// <summary>
    /// Presenter for the Audit Log viewer.
    /// Handles search, pagination, and CSV export.
    /// </summary>
    public class AuditLogPresenter
    {
        private readonly IAuditLogView _view;
        private readonly IAuditService _auditService;
        private readonly UserContext   _user;

        public AuditLogPresenter(IAuditLogView view, IAuditService auditService, UserContext user)
        {
            if (view         == null) throw new ArgumentNullException("view");
            if (auditService == null) throw new ArgumentNullException("auditService");
            if (user         == null) throw new ArgumentNullException("user");

            _view         = view;
            _auditService = auditService;
            _user         = user;

            _view.SearchRequested += (s, e) => { _view.CurrentPage = 1; ExecuteSearch(); };
            _view.PageChanged     += (s, e) => ExecuteSearch();
            _view.ExportRequested += (s, e) => ExecuteExport();

            // Load last 7 days on open
            _view.FromDate = DateTime.Today.AddDays(-7);
            _view.ToDate   = DateTime.Today;
            ExecuteSearch();
        }

        // -----------------------------------------------------------------------
        // Search
        // -----------------------------------------------------------------------

        private void ExecuteSearch()
        {
            try
            {
                _view.ShowStatus("กำลังค้นหา...", false);

                var query  = BuildQuery();
                var result = _auditService.Query(query, _user);

                _view.BindResults(result.Items, result.TotalCount, result.PageNumber, result.PageSize);

                int totalPages = result.PageSize > 0
                    ? (int)Math.Ceiling((double)result.TotalCount / result.PageSize)
                    : 1;

                _view.ShowStatus(
                    string.Format("พบ {0} รายการ  (หน้า {1}/{2})",
                        result.TotalCount, result.PageNumber, Math.Max(1, totalPages)),
                    false);
            }
            catch (UnauthorizedAccessException)
            {
                _view.ShowStatus("คุณไม่มีสิทธิ์ดู Audit Log (ต้องการสิทธิ์ Manager หรือ Admin)", true);
            }
            catch (Exception ex)
            {
                _view.ShowStatus("เกิดข้อผิดพลาด: " + ex.Message, true);
            }
        }

        // -----------------------------------------------------------------------
        // Export CSV
        // -----------------------------------------------------------------------

        private void ExecuteExport()
        {
            try
            {
                var defaultName = string.Format("AuditLog_{0}.csv", DateTime.Now.ToString("yyyyMMdd_HHmmss"));
                var path = _view.PromptSaveFilePath(defaultName);
                if (string.IsNullOrEmpty(path))
                    return; // user cancelled

                var query = BuildQuery();
                query.PageNumber = 1;
                query.PageSize   = 10000;

                var result = _auditService.Query(query, _user);

                var sb = new StringBuilder();
                sb.AppendLine("AuditID,วันที่เวลา,ผู้ใช้,Module,Action,ตาราง,RecordID,ผลลัพธ์,IP Address,เครื่อง,ข้อความผิดพลาด");

                foreach (var log in result.Items)
                {
                    sb.AppendLine(string.Format("{0},{1},{2},{3},{4},{5},{6},{7},{8},{9},{10}",
                        log.AuditID,
                        log.AuditDate.ToString("yyyy-MM-dd HH:mm:ss"),
                        CsvEscape(log.UserName),
                        CsvEscape(log.Module),
                        CsvEscape(log.Action),
                        CsvEscape(log.TableName),
                        log.RecordID.HasValue ? log.RecordID.Value.ToString() : "",
                        log.Outcome,
                        CsvEscape(log.IPAddress),
                        CsvEscape(log.MachineName),
                        CsvEscape(log.ErrorMessage)));
                }

                // UTF-8 with BOM so Excel opens Thai text correctly
                File.WriteAllText(path, sb.ToString(), new UTF8Encoding(encoderShouldEmitUTF8Identifier: true));
                _view.ShowStatus(string.Format("ส่งออกสำเร็จ ({0} รายการ): {1}", result.TotalCount, path), false);
            }
            catch (Exception ex)
            {
                _view.ShowStatus("ส่งออกไม่สำเร็จ: " + ex.Message, true);
            }
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private AuditQuery BuildQuery()
        {
            return new AuditQuery
            {
                FromDate   = _view.FromDate.Date,
                ToDate     = _view.ToDate.Date.AddDays(1).AddSeconds(-1),
                UserName   = string.IsNullOrWhiteSpace(_view.FilterUserName) ? null : _view.FilterUserName.Trim(),
                Module     = string.IsNullOrWhiteSpace(_view.FilterModule)   ? null : _view.FilterModule.Trim(),
                Action     = string.IsNullOrWhiteSpace(_view.FilterAction)   ? null : _view.FilterAction.Trim(),
                Result     = MapResultFilter(_view.FilterResult),
                PageNumber = _view.CurrentPage,
                PageSize   = _view.PageSize
            };
        }

        /// <summary>Maps Thai result label → int (1=Success, 2=Failure, 3=Denied).</summary>
        private static int? MapResultFilter(string filterResult)
        {
            if (string.IsNullOrWhiteSpace(filterResult)) return null;
            switch (filterResult.Trim())
            {
                case "สำเร็จ":      return 1;
                case "ล้มเหลว":    return 2;
                case "ถูกปฏิเสธ": return 3;
                default:            return null;
            }
        }

        private static string CsvEscape(string value)
        {
            if (string.IsNullOrEmpty(value)) return "";
            if (value.Contains(",") || value.Contains("\"") || value.Contains("\n"))
                return "\"" + value.Replace("\"", "\"\"") + "\"";
            return value;
        }
    }
}
