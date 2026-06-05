using CMS.Data.Models;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Business logic service for writing and querying the immutable audit log.
    /// Satisfies Requirements 9.1–9.7.
    /// </summary>
    public interface IAuditService
    {
        /// <summary>
        /// Records an audit event. Retries up to 3 times on storage failure (Req 9.6).
        /// </summary>
        void Log(AuditEntry entry);

        /// <summary>
        /// Returns a paged set of AuditLog records matching the query filters.
        /// Caller must have Manager or Admin role (Req 9.5).
        /// </summary>
        CMS.Data.Models.PagedResult<AuditLog> Query(AuditQuery query, UserContext user);

        /// <summary>
        /// Returns all AuditLog records related to a specific workflow instance.
        /// </summary>
        System.Collections.Generic.List<AuditLog> GetWorkflowAuditLogs(int workflowInstanceId, UserContext user);
    }
}
