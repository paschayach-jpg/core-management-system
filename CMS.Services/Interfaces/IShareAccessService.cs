using System;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Service for managing access control and download tracking for external shares.
    /// Handles access validation, logging, and auto-termination of shares.
    /// Satisfies Requirements 6.1, 8.2, 8.3, 8.4, 9.1, 9.2, 9.3.
    /// </summary>
    public interface IShareAccessService
    {
        /// <summary>
        /// Validates and grants access to an external share.
        /// Verifies token status, expiry date, password, and download limit.
        /// Increments download counter and logs access attempt.
        /// Validates: Requirements 6.1, 6.2, 6.3, 6.4, 6.5, 8.2, 8.3, 8.4
        /// </summary>
        ShareAccessResult ValidateAndGrantAccess(Guid token, string password, UserContext user);

        /// <summary>
        /// Creates an immutable access log entry for successful or failed access attempts.
        /// Validates: Requirements 9.1, 9.2, 9.3
        /// </summary>
        void LogAccessAttempt(ShareAccessLog log);

        /// <summary>
        /// Increments the download counter for an external share.
        /// If download limit is reached, automatically transitions status to Disabled.
        /// Validates: Requirements 8.2, 8.3, 8.4
        /// </summary>
        void IncrementDownloadCount(int externalShareId);

        /// <summary>
        /// Checks if an external share has reached its download limit.
        /// </summary>
        bool IsDownloadLimitReached(int externalShareId);

        /// <summary>
        /// Processes all expired shares and transitions them to Expired status.
        /// Called by background job or scheduled task.
        /// Validates: Requirements 4.5, 12.1
        /// </summary>
        void ProcessExpiredShares();

        /// <summary>
        /// Processes all disabled shares and performs cleanup.
        /// Called by background job or scheduled task.
        /// Validates: Requirements 12.2
        /// </summary>
        void ProcessDisabledShares();
    }
}
