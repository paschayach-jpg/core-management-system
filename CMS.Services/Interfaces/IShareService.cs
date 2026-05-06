using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Services.Interfaces
{
    /// <summary>
    /// Service for managing internal and external file/folder shares.
    /// 
    /// Internal shares grant permissions directly to specific targets (Users, Roles, OrgUnits, Groups).
    /// External shares create GUID-based token links with optional security layers (password, expiry, download limit).
    /// 
    /// All share operations are logged via IAuditService and trigger notifications via INotificationService.
    /// </summary>
    public interface IShareService
    {
        // =====================================================================
        // Internal Sharing Methods
        // =====================================================================

        /// <summary>
        /// Creates an internal share by granting file permissions to a specific target.
        /// 
        /// Validates:
        /// - User has Read permission on the file
        /// - Granted permissions do not exceed Sharer's permissions
        /// 
        /// Logs audit entry and sends notification to recipient(s).
        /// 
        /// Requirements: 1.1, 1.2, 1.3, 1.4, 15.1, 15.2
        /// </summary>
        FilePermission CreateInternalShare(CreateInternalShareRequest request, UserContext user);

        /// <summary>
        /// Revokes an internal share by removing the file permission entry.
        /// 
        /// Validates:
        /// - User is the original Sharer or a system administrator
        /// 
        /// Logs audit entry and sends notification to recipient(s).
        /// 
        /// Requirements: 1.5, 15.3, 15.4
        /// </summary>
        void RevokeInternalShare(int filePermissionId, UserContext user);

        // =====================================================================
        // External Sharing Methods
        // =====================================================================

        /// <summary>
        /// Creates an external share via a GUID-based token link.
        /// 
        /// Validates:
        /// - User has Read permission on file/folder
        /// - File/folder is NOT Highly Confidential (1HC)
        /// - Folder does NOT contain Highly Confidential files
        /// - Expiry date (if provided) is ≤ 90 days from now
        /// - Download limit (if provided) is positive
        /// 
        /// Generates unique token, hashes password (if provided), creates ExternalShare record with status="Pending".
        /// Routes to Manager for approval.
        /// Logs audit entry and sends notification to Sharer with generated link.
        /// 
        /// Requirements: 2.1, 2.2, 2.3, 2.4, 3.1, 3.2, 4.1, 4.2, 7.1, 10.2, 15.1, 15.2
        /// </summary>
        ExternalShare CreateExternalShare(CreateExternalShareRequest request, UserContext user);

        /// <summary>
        /// Approves an external share request, transitioning status from "Pending" to "Active".
        /// 
        /// Sets ApprovedDate and ApproverUserID.
        /// Logs audit entry and sends notification to Sharer.
        /// 
        /// Requirements: 5.1, 5.2, 5.3, 10.3
        /// </summary>
        void ApproveExternalShare(int externalShareId, UserContext user);

        /// <summary>
        /// Rejects an external share request, transitioning status to "Revoked".
        /// 
        /// Logs rejection reason and sends notification to Sharer.
        /// 
        /// Requirements: 5.1, 5.4, 10.4
        /// </summary>
        void RejectExternalShare(int externalShareId, string rejectionReason, UserContext user);

        /// <summary>
        /// Revokes an external share, transitioning status to "Revoked".
        /// 
        /// Validates:
        /// - User is the original Sharer or a system administrator
        /// 
        /// Logs revocation reason and sends notification to Sharer.
        /// Prevents any further access using the share token.
        /// 
        /// Requirements: 14.1, 14.2, 15.3, 15.4
        /// </summary>
        void RevokeExternalShare(int externalShareId, string revocationReason, UserContext user);

        /// <summary>
        /// Toggles an external share between Active and Disabled.
        /// Active → Disabled (pause), Disabled → Active (resume).
        /// Not applicable for Pending, Revoked, Expired.
        /// </summary>
        void ToggleExternalShare(int externalShareId, UserContext user);

        /// <summary>
        /// Modifies an external share (expiry date, download limit, password).
        /// 
        /// Validates:
        /// - User is the original Sharer or a system administrator
        /// - Share status is NOT "Expired" or "Disabled"
        /// - New expiry date (if provided) is ≤ 90 days from now
        /// - New download limit (if provided) is positive
        /// 
        /// Logs modification with previous and new values.
        /// Sends notification to Sharer.
        /// 
        /// Requirements: 14.3, 14.4, 15.3, 15.4
        /// </summary>
        void ModifyExternalShare(ModifyExternalShareRequest request, UserContext user);

        // =====================================================================
        // Query Methods
        // =====================================================================

        /// <summary>
        /// Returns all internal shares (file permissions) created by the specified user.
        /// 
        /// Validates:
        /// - User is querying their own shares or is a system administrator
        /// 
        /// Requirements: 13.1, 13.2, 15.5
        /// </summary>
        IEnumerable<FilePermission> GetInternalSharesForUser(int userId, UserContext user);
        IEnumerable<FilePermission> GetAllInternalShares(UserContext admin);
        IEnumerable<ExternalShare>  GetAllExternalShares(UserContext admin);

        /// <summary>
        /// Returns all external shares created by the specified user.
        /// 
        /// Validates:
        /// - User is querying their own shares or is a system administrator
        /// 
        /// Includes: Share_Token, creation timestamp, expiry date, download limit, current download count, 
        ///           password protection status, and Share_Token_Status.
        /// 
        /// Requirements: 13.1, 13.2, 15.5
        /// </summary>
        IEnumerable<ExternalShare> GetExternalSharesForUser(int userId, UserContext user);

        /// <summary>
        /// Returns all external shares filtered by status (Active, Pending, Expired, Disabled, Revoked).
        /// 
        /// Validates:
        /// - User is a system administrator or Manager with audit permissions
        /// 
        /// Requirements: 13.4
        /// </summary>
        IEnumerable<ExternalShare> GetExternalSharesByStatus(string status, UserContext user);

        /// <summary>
        /// Returns the access log for a specific external share, ordered by timestamp (newest first).
        /// 
        /// Includes: Recipient identifier, access type (View/Download), timestamp, success/failure status, 
        ///           failure reason (if applicable), and file size (if applicable).
        /// 
        /// Validates:
        /// - User is the original Sharer or a system administrator with audit permissions
        /// 
        /// Requirements: 9.1, 9.2, 9.3, 9.4, 15.5
        /// </summary>
        IEnumerable<ShareAccessLog> GetAccessLog(int externalShareId, UserContext user);

        // =====================================================================
        // Validation Methods
        // =====================================================================

        /// <summary>
        /// Checks whether the user can create a share for the specified file.
        /// 
        /// Returns true if:
        /// - User has Read permission on the file
        /// - File is NOT Highly Confidential (1HC)
        /// 
        /// Requirements: 15.1, 15.2
        /// </summary>
        bool CanShareFile(int fileId, UserContext user);

        /// <summary>
        /// Checks whether the user can create a share for the specified folder.
        /// 
        /// Returns true if:
        /// - User has Read permission on the folder
        /// - Folder is NOT Highly Confidential (1HC)
        /// - Folder does NOT contain any Highly Confidential files
        /// 
        /// Requirements: 15.1, 15.2
        /// </summary>
        bool CanShareFolder(int folderId, UserContext user);
    }
}
