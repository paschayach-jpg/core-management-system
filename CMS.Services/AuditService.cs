using System;
using CMS.Data.Interfaces;
using CMS.Data.Models;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Implements IAuditService: writes immutable audit entries and exposes a
    /// role-gated query interface. Satisfies Requirements 9.1–9.7.
    /// </summary>
    public class AuditService : IAuditService
    {
        private const int MaxRetries = 3;

        private readonly IAuditRepository _auditRepository;
        private readonly ILoggingService  _loggingService;

        public AuditService(IAuditRepository auditRepository, ILoggingService loggingService)
        {
            if (auditRepository == null) throw new ArgumentNullException("auditRepository");
            if (loggingService  == null) throw new ArgumentNullException("loggingService");

            _auditRepository = auditRepository;
            _loggingService  = loggingService;
        }

        // -----------------------------------------------------------------------
        // Log  (Req 9.1, 9.2, 9.4, 9.6)
        // -----------------------------------------------------------------------

        public void Log(AuditEntry entry)
        {
            if (entry == null) throw new ArgumentNullException("entry");

            var auditLog = MapToAuditLog(entry);

            Exception lastException = null;

            for (int attempt = 1; attempt <= MaxRetries; attempt++)
            {
                try
                {
                    _auditRepository.Insert(auditLog);
                    return;
                }
                catch (Exception ex)
                {
                    lastException = ex;
                    _loggingService.Warn(string.Format(
                        "AuditService.Log attempt {0}/{1} failed for Module='{2}' Action='{3}': {4}",
                        attempt, MaxRetries, entry.Module, entry.Action, ex.Message));
                }
            }

            _loggingService.Error(string.Format(
                "AuditService.Log FAILED after {0} attempts for Module='{1}' Action='{2}' UserID={3}. " +
                "ALERT: Audit entry could not be persisted.",
                MaxRetries, entry.Module, entry.Action, entry.UserID),
                lastException);
        }

        // -----------------------------------------------------------------------
        // Query  (Req 9.5, 9.7)
        // -----------------------------------------------------------------------

        public CMS.Data.Models.PagedResult<AuditLog> Query(AuditQuery query, UserContext user)
        {
            if (query == null) throw new ArgumentNullException("query");
            if (user  == null) throw new ArgumentNullException("user");

            // Manager=3, Admin=4
            if (user.AppRole < 3)
                throw new UnauthorizedAccessException("AuditLog query requires Manager or Admin role.");

            return _auditRepository.Query(query);
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private static AuditLog MapToAuditLog(AuditEntry entry)
        {
            return new AuditLog
            {
                AuditDate   = entry.AuditDate == default(DateTime) ? DateTime.Now : entry.AuditDate,
                UserID      = entry.UserID,
                UserName    = entry.UserName,
                UserRole    = entry.UserRole,
                Module      = entry.Module,
                Action      = entry.Action,
                TableName   = entry.TableName,
                RecordID    = entry.RecordID,
                OldValue    = entry.OldValue,
                NewValue    = entry.NewValue,
                IPAddress   = entry.IPAddress,
                MachineName = entry.MachineName,
                SessionID   = entry.SessionID,
                Result      = entry.Result > 0 ? entry.Result : 1,
                ErrorMessage = entry.ErrorMessage
            };
        }
    }
}
