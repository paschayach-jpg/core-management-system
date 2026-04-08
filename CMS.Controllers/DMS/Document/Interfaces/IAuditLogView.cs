using System;
using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document
{
    /// <summary>
    /// View contract for the Audit Log viewer form.
    /// </summary>
    public interface IAuditLogView
    {
        // ── Filter inputs ─────────────────────────────────────────────────────
        DateTime FromDate { get; set; }
        DateTime ToDate   { get; set; }
        string   FilterModule   { get; set; }
        string   FilterAction   { get; set; }
        string   FilterUserName { get; set; }
        string   FilterResult   { get; set; }

        // ── Grid / status ─────────────────────────────────────────────────────
        void BindResults(IEnumerable<AuditLog> items, int totalCount, int pageNumber, int pageSize);
        void ShowStatus(string message, bool isError);
        void ShowDetail(AuditLog entry);

        // ── Pagination ────────────────────────────────────────────────────────
        int  CurrentPage { get; set; }
        int  PageSize    { get; }

        // ── Events ────────────────────────────────────────────────────────────
        event EventHandler SearchRequested;
        event EventHandler ExportRequested;
        event EventHandler PageChanged;
    }
}
