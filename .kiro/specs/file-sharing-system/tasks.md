# Implementation Plan: File Sharing System (BR-005)

## Overview

This implementation plan breaks down the File Sharing System feature into discrete, incremental coding tasks organized across six phases: Domain & Data Layer, Service Layer (Token & Access), Service Layer (Share Management), Integration & Notifications, Presentation Layer, and Testing & Validation. Each task builds on previous steps and includes property-based tests for correctness verification.

## Tasks

- [x] 1. Create domain models and database schema
  - [x] 1.1 Create ExternalShare, ShareAccessLog, and ShareToken domain models in CMS.Domain/Models
    - Add ExternalShare class with all properties (ShareToken, FileID, FolderID, SharerUserID, ApproverUserID, PasswordHash, ExpiryDate, DownloadLimit, CurrentDownloadCount, ShareTokenStatus, CreatedDate, ApprovedDate, DisabledDate, DisabledReason, RevokedBy, RevokedDate, RevocationReason, SharePurpose, NotifyOnAccess)
    - Add ShareAccessLog class with all properties (AccessLogID, ShareToken, ExternalShareID, RecipientIdentifier, AccessType, IsSuccessful, FailureReason, FileSizeBytes, AccessTimestamp, IPAddress, UserAgent)
    - Add ShareToken class with all properties (ShareTokenID, TokenValue, ExternalShareID, Status, CreatedDate, ActivatedDate, ExpiryDate, DisabledDate, DisabledByUserID, DisabledReason)
    - _Requirements: 2.1, 2.2, 9.1, 9.2, 9.3_

  - [x] 1.2 Create database migration script (030_FileSharing_Tables.sql) with ExternalShare, ShareAccessLog, and ShareToken tables
    - Create storage.ExternalShare table with all columns, constraints, and foreign keys
    - Create storage.ShareAccessLog table with immutability constraint (unique on AccessLogID)
    - Create storage.ShareToken table with status constraint
    - Add indexes: IX_ExternalShare_Token, IX_ExternalShare_Sharer, IX_ExternalShare_Status, IX_ExternalShare_Expiry, IX_ShareAccessLog_Token, IX_ShareAccessLog_ExternalShare, IX_ShareAccessLog_Timestamp, IX_ShareAccessLog_Recipient, IX_ShareToken_Value, IX_ShareToken_ExternalShare, IX_ShareToken_Status
    - _Requirements: 2.1, 9.5, 17.5_

  - [x] 1.3 Create repository interfaces in CMS.Data/Interfaces
    - Create IExternalShareRepository with methods: Insert, GetById, GetByToken, GetBySharer, GetByStatus, Update, GetAll
    - Create IShareAccessLogRepository with methods: Insert, GetByExternalShare, GetByToken, GetByDateRange
    - Create IShareTokenRepository with methods: Insert, GetById, GetByTokenValue, Update, GetByExternalShare
    - _Requirements: 2.1, 9.1_

  - [x] 1.4 Create repository implementations in CMS.Data/Repositories using ADO.NET
    - Implement ExternalShareRepository calling stored procedures for CRUD operations
    - Implement ShareAccessLogRepository with append-only Insert (no Update/Delete)
    - Implement ShareTokenRepository with token lifecycle methods
    - Use parameterized queries and connection pooling
    - _Requirements: 2.1, 9.5_

- [x] 2. Implement token and access management services
  - [x] 2.1 Create IShareTokenService interface in CMS.Services/Interfaces
    - Define methods: GenerateToken(), ValidateToken(Guid), GetTokenInfo(Guid), HashPassword(string), VerifyPassword(string, string), ActivateToken(Guid, UserContext), ExpireToken(Guid, UserContext), DisableToken(Guid, string, UserContext), RevokeToken(Guid, UserContext)
    - _Requirements: 2.1, 7.1, 7.3_

  - [x] 2.2 Implement ShareTokenService with bcrypt password hashing
    - Implement GenerateToken() using System.Security.Cryptography.RNGCryptoServiceProvider for GUID generation
    - Implement HashPassword() using BCrypt.Net.BCrypt with work factor 12
    - Implement VerifyPassword() with constant-time comparison
    - Implement token lifecycle methods (Activate, Expire, Disable, Revoke)
    - Add logging via ILoggingService
    - _Requirements: 7.1, 7.3, 2.1_

  - [ ]* 2.3 Write property test for token generation uniqueness
    - **Property 7: Token Uniqueness**
    - **Validates: Requirements 2.1**
    - Generate 1000+ tokens and verify all are unique GUIDs

  - [ ]* 2.4 Write property test for password hashing
    - **Property 12: Password Hashing**
    - **Validates: Requirements 7.1, 7.3**
    - Generate random passwords, verify hashes are unique and verification works correctly

  - [x] 2.5 Create IShareAccessService interface in CMS.Services/Interfaces
    - Define methods: ValidateAndGrantAccess(Guid, string, UserContext), LogAccessAttempt(ShareAccessLog), IncrementDownloadCount(int), IsDownloadLimitReached(int), ProcessExpiredShares(), ProcessDisabledShares()
    - _Requirements: 6.1, 8.2, 8.3, 8.4_

  - [x] 2.6 Implement ShareAccessService with access control and download tracking
    - Implement ValidateAndGrantAccess() to verify token status, expiry, password, and download limit
    - Implement LogAccessAttempt() to create immutable access log entries
    - Implement IncrementDownloadCount() to track downloads and trigger auto-disable at limit
    - Implement ProcessExpiredShares() and ProcessDisabledShares() for auto-termination
    - Integrate with IAuditService for access logging
    - Integrate with INotificationService for access notifications
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 8.2, 8.3, 8.4, 9.1, 9.2, 9.3, 11.1, 11.2, 11.4_

  - [ ]* 2.7 Write property test for download limit enforcement
    - **Property 5: Download Limit Enforcement**
    - **Validates: Requirements 4.4, 8.2, 8.3, 8.4**
    - Create shares with random download limits, simulate downloads, verify status transitions

  - [ ]* 2.8 Write property test for expiry date enforcement
    - **Property 6: Expiry Date Enforcement**
    - **Validates: Requirements 4.5, 6.4, 6.5**
    - Create shares with random expiry dates, simulate time passing, verify status transitions

  - [ ]* 2.9 Write property test for access control by status
    - **Property 11: Access Control by Status**
    - **Validates: Requirements 6.1, 6.2, 12.5**
    - Create shares with random statuses, verify access denied for non-Active statuses

  - [x] 2.10 Checkpoint - Ensure all token and access service tests pass
    - Ensure all tests pass, ask the user if questions arise.

- [x] 3. Implement share management service
  - [x] 3.1 Create IShareService interface in CMS.Services/Interfaces
    - Define methods for internal sharing: CreateInternalShare(CreateInternalShareRequest, UserContext), RevokeInternalShare(int, UserContext)
    - Define methods for external sharing: CreateExternalShare(CreateExternalShareRequest, UserContext), ApproveExternalShare(int, UserContext), RejectExternalShare(int, string, UserContext), RevokeExternalShare(int, string, UserContext), ModifyExternalShare(ModifyExternalShareRequest, UserContext)
    - Define query methods: GetInternalSharesForUser(int, UserContext), GetExternalSharesForUser(int, UserContext), GetExternalSharesByStatus(string, UserContext), GetAccessLog(int, UserContext)
    - Define validation methods: CanShareFile(int, UserContext), CanShareFolder(int, UserContext)
    - _Requirements: 1.1, 2.1, 5.1, 13.1, 13.2, 15.1_

  - [x] 3.2 Create DTOs in CMS.Services/DTOs
    - Create CreateInternalShareRequest with FileID, PrincipalType, PrincipalID, CanRead, CanWrite, CanDelete, ExpiryDate
    - Create CreateExternalShareRequest with FileID, FolderID, Password, ExpiryDate, DownloadLimit, SharePurpose, NotifyOnAccess
    - Create ModifyExternalShareRequest with ExternalShareID, NewExpiryDate, NewDownloadLimit, NewPassword
    - Create ShareAccessResult with IsGranted, DenyReason, File, Folder, RemainingDownloads
    - _Requirements: 2.3, 4.1, 4.3, 7.2_

  - [x] 3.3 Implement ShareService - internal share creation
    - Implement CreateInternalShare() to validate user has Read permission, create FilePermission record, log audit entry, send notification
    - Validate permission hierarchy (granted permissions ≤ Sharer's permissions)
    - Integrate with IFilePermissionService, IAuditService, INotificationService
    - _Requirements: 1.1, 1.2, 1.3, 1.4, 15.1, 15.2, 16.4_

  - [~]* 3.4 Write property test for internal share immediate application
    - **Property 31: Internal Share Immediate Application**
    - **Validates: Requirements 1.4**
    - Create internal shares, verify recipient immediately has permissions

  - [~]* 3.5 Write property test for internal share metadata recording
    - **Property 32: Internal Share Metadata Recording**
    - **Validates: Requirements 1.2**
    - Create internal shares, verify all metadata is recorded correctly

  - [x] 3.6 Implement ShareService - external share creation with validation
    - Implement CreateExternalShare() to validate user has Read permission, check confidentiality level, validate expiry duration, generate token, hash password, create ExternalShare record with status="Pending"
    - Validate: file/folder is not 1HC, expiry ≤ 90 days, user has Read permission
    - Integrate with IShareTokenService, IExternalShareRepository, IAuditService, INotificationService
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 3.1, 3.2, 4.1, 4.2, 7.1, 10.2, 15.1, 15.2_

  - [~]* 3.7 Write property test for highly confidential file rejection
    - **Property 1: External Share Rejection for Highly Confidential Files**
    - **Validates: Requirements 3.1**
    - Generate random files with varying confidentiality levels, verify only HC files are rejected

  - [~]* 3.8 Write property test for folder confidentiality scanning
    - **Property 2: Folder Confidentiality Scanning**
    - **Validates: Requirements 3.2**
    - Create folders with HC files, verify external share creation is rejected

  - [~]* 3.9 Write property test for expiry duration validation
    - **Property 4: Expiry Duration Validation**
    - **Validates: Requirements 4.1, 4.2**
    - Generate random expiry dates, verify dates > 90 days are rejected

  - [~]* 3.10 Write property test for permission validation
    - **Property 20: Permission Validation for Share Creation**
    - **Validates: Requirements 15.1, 15.2**
    - Generate users with varying permissions, verify only those with Read can create shares

  - [x] 3.11 Implement ShareService - external share approval workflow
    - Implement ApproveExternalShare() to transition status from Pending to Active, set ApprovedDate and ApproverUserID, log audit entry, send notification
    - Implement RejectExternalShare() to transition status to Revoked, log rejection reason, send notification
    - Implement auto-expiration of Pending shares after 7 days (via background job or scheduled task)
    - _Requirements: 5.1, 5.2, 5.3, 5.4, 5.5, 10.3, 10.4_

  - [~]* 3.12 Write property test for status transition Pending to Active
    - **Property 8: Status Transition: Pending to Active**
    - **Validates: Requirements 2.5, 5.3**
    - Create shares, approve them, verify status transitions and ApprovedDate is set

  - [~]* 3.13 Write property test for status transition Pending to Expired
    - **Property 9: Status Transition: Pending to Expired**
    - **Validates: Requirements 5.5**
    - Create shares, wait 7 days, verify auto-expiration

  - [~]* 3.14 Write property test for status transition Pending to Revoked
    - **Property 10: Status Transition: Pending to Revoked**
    - **Validates: Requirements 5.4**
    - Create shares, reject them, verify status transitions to Revoked

  - [x] 3.15 Implement ShareService - external share revocation and modification
    - Implement RevokeExternalShare() to transition status to Revoked, set RevokedBy and RevokedDate, log audit entry, send notification
    - Implement ModifyExternalShare() to update ExpiryDate, DownloadLimit, Password; reject if status is Expired or Disabled
    - Validate modification authorization (only Sharer or Admin)
    - _Requirements: 14.1, 14.2, 14.3, 14.4, 15.3, 15.4_

  - [~]* 3.16 Write property test for revocation authorization
    - **Property 21: Revocation Authorization**
    - **Validates: Requirements 15.3, 15.4**
    - Create shares, attempt revocation by non-Sharer, verify rejection

  - [~]* 3.17 Write property test for share modification validation
    - **Property 38: Share Modification Validation**
    - **Validates: Requirements 14.4**
    - Create shares, transition to Expired/Disabled, attempt modification, verify rejection

  - [x] 3.18 Implement ShareService - automatic confidentiality upgrade revocation
    - Implement logic to detect when file's ConfidentialityLevel changes to 1HC
    - Automatically revoke all external shares for that file
    - Log revocation reason as "Confidentiality level upgraded to HC"
    - Send notification to Sharer
    - _Requirements: 3.3, 16.2, 16.3_

  - [~]* 3.19 Write property test for automatic revocation on confidentiality upgrade
    - **Property 3: Automatic Revocation on Confidentiality Upgrade**
    - **Validates: Requirements 3.3**
    - Create external shares, upgrade file confidentiality to HC, verify auto-revocation

  - [x] 3.20 Implement ShareService - query and filtering methods
    - Implement GetInternalSharesForUser() to return all internal shares created by user
    - Implement GetExternalSharesForUser() to return all external shares created by user
    - Implement GetExternalSharesByStatus() to filter by status (Active, Pending, Expired, Disabled, Revoked)
    - Implement GetAccessLog() to return access log entries for a share, ordered by timestamp
    - Validate authorization (only Sharer or Admin can query)
    - _Requirements: 13.1, 13.2, 13.3, 13.4, 13.5, 15.5_

  - [~]* 3.21 Write property test for share status filtering
    - **Property 36: Share Status Filtering**
    - **Validates: Requirements 13.4**
    - Create shares with random statuses, filter by each status, verify correct results

  - [~]* 3.22 Write property test for share date range filtering
    - **Property 37: Share Date Range Filtering**
    - **Validates: Requirements 13.5**
    - Create shares with random creation dates, filter by date ranges, verify correct results

  - [~]* 3.23 Write property test for access log authorization
    - **Property 22: Access Log Authorization**
    - **Validates: Requirements 15.5**
    - Create shares, attempt access log query by non-Sharer, verify rejection

  - [x] 3.24 Checkpoint - Ensure all share management service tests pass
    - Ensure all tests pass, ask the user if questions arise.

- [x] 4. Integrate with notification and audit services
  - [x] 4.1 Integrate INotificationService for share lifecycle events
    - Call INotificationService.Send() for: ShareCreated (internal), ShareCreated (external), ShareApproved, ShareRejected, AccessNotification, DownloadNotification, ShareExpired, ShareRevoked
    - Pass correct EventType, RecipientUserId, and MessageText for each event
    - Respect NotifyOnAccess preference for access notifications
    - _Requirements: 1.3, 10.1, 10.2, 10.3, 10.4, 10.5, 11.1, 11.2, 11.4, 11.5_

  - [~]* 4.2 Write property test for notification on internal share creation
    - **Property 39: Notification on Internal Share Creation**
    - **Validates: Requirements 1.3, 10.1**
    - Create internal shares, verify notification sent with correct content

  - [~]* 4.3 Write property test for notification on external share creation
    - **Property 40: Notification on External Share Creation**
    - **Validates: Requirements 2.4, 10.2**
    - Create external shares, verify notification sent with correct content

  - [~]* 4.4 Write property test for notification on share approval
    - **Property 41: Notification on Share Approval**
    - **Validates: Requirements 10.3**
    - Approve shares, verify notification sent to Sharer

  - [~]* 4.5 Write property test for notification on share rejection
    - **Property 42: Notification on Share Rejection**
    - **Validates: Requirements 10.4**
    - Reject shares, verify notification sent to Sharer

  - [~]* 4.6 Write property test for notification on auto-disablement
    - **Property 43: Notification on Auto-Disablement**
    - **Validates: Requirements 10.5**
    - Disable shares (expiry or download limit), verify notification sent

  - [~]* 4.7 Write property test for notification on access
    - **Property 44: Notification on Access**
    - **Validates: Requirements 11.1, 11.2**
    - Access shares, verify notification sent to Sharer with correct content

  - [~]* 4.8 Write property test for notification on failed access
    - **Property 45: Notification on Failed Access**
    - **Validates: Requirements 11.4**
    - Attempt failed access, verify notification sent to Sharer

  - [~]* 4.9 Write property test for notification preference respect
    - **Property 46: Notification Preference Respect**
    - **Validates: Requirements 11.5**
    - Disable notifications, verify no access notifications sent

  - [x] 4.10 Integrate IAuditService for audit logging
    - Call IAuditService.Log() for all share operations: CreateInternalShare, CreateExternalShare, ApproveExternalShare, RejectExternalShare, RevokeShare, ModifyShare, AccessAttempt, Download
    - Log Module="FileSharing", Action=operation type, UserID, UserName, TableName, RecordID, OldValue (for modifications), NewValue, Result=1 (success)
    - _Requirements: 3.4, 9.1, 9.3, 17.1, 17.2, 17.3, 17.4_

  - [~]* 4.11 Write property test for audit trail completeness on share creation
    - **Property 25: Audit Trail Completeness for Share Creation**
    - **Validates: Requirements 17.1**
    - Create shares, verify audit entries with all required fields

  - [~]* 4.12 Write property test for audit trail completeness on share modification
    - **Property 26: Audit Trail Completeness for Share Modification**
    - **Validates: Requirements 17.2**
    - Modify shares, verify audit entries with OldValue and NewValue

  - [~]* 4.13 Write property test for audit trail completeness on share revocation
    - **Property 27: Audit Trail Completeness for Share Revocation**
    - **Validates: Requirements 17.3, 17.4**
    - Revoke/disable shares, verify audit entries with revocation reason

  - [x] 4.14 Checkpoint - Ensure all notification and audit integration tests pass
    - Ensure all tests pass, ask the user if questions arise.

- [x] 5. Create presentation layer components
  - [x] 5.1 Create ShareDialog for internal share creation
    - Add form controls: file/folder selector, principal type dropdown (User, Department, Role, Group), principal selector, permission checkboxes (Read, Write, Delete), expiry date picker
    - Implement validation: file/folder selected, principal selected, at least one permission checked
    - Call IShareService.CreateInternalShare() on OK button
    - Display success/error messages
    - _Requirements: 1.1, 1.2, 1.3_

  - [x] 5.2 Create ShareDialog for external share creation
    - Add form controls: file/folder selector, password textbox, expiry date picker, download limit spinner, share purpose textbox, notify on access checkbox
    - Implement validation: file/folder selected, expiry ≤ 90 days, download limit > 0
    - Call IShareService.CreateExternalShare() on OK button
    - Display generated share link and security settings summary
    - Display success/error messages
    - _Requirements: 2.1, 2.2, 2.3, 2.4, 4.1, 4.2, 7.2_

  - [x] 5.3 Create ShareManagementForm for viewing and managing shares
    - Add tabs: Internal Shares, External Shares
    - Internal Shares tab: DataGridView showing Sharer, Target, File/Folder, Created Date, Status; Revoke button
    - External Shares tab: DataGridView showing Sharer, File/Folder, Created Date, Status, Expiry, Download Limit, Current Downloads; Approve/Reject buttons (for Managers), Revoke button, Modify button
    - Implement filtering by status and date range
    - Call IShareService methods to populate grids and handle actions
    - _Requirements: 13.1, 13.2, 13.3, 13.4, 13.5, 14.1, 14.2, 14.3_

  - [x] 5.4 Create AccessLogView for audit trail display
    - Add DataGridView showing: Recipient, Access Type, Timestamp, Success/Failure, Failure Reason, File Size
    - Implement filtering by date range and access type
    - Call IShareService.GetAccessLog() to populate grid
    - Display access log for selected external share
    - _Requirements: 9.1, 9.2, 9.3, 9.4_

  - [x] 5.5 Integrate share dialogs with existing file/folder UI
    - Add "Share" button to file context menu and folder context menu
    - Add "Share" button to file/folder details panel
    - On click, show ShareDialog with file/folder pre-selected
    - Add "Manage Shares" button to main menu
    - On click, show ShareManagementForm
    - _Requirements: 1.1, 2.1, 13.1_

  - [x] 5.6 Implement share link access UI (external recipient view)
    - Create AccessShareForm for recipients accessing external share links
    - Add controls: password textbox (if password-protected), download button, file preview (if applicable)
    - Implement password validation via IShareAccessService.ValidateAndGrantAccess()
    - On successful access, enable download button
    - Display error messages for invalid/expired/disabled links
    - _Requirements: 6.1, 6.2, 6.3, 6.4, 6.5, 7.2, 7.4, 7.5_

- [~] 6. Testing and validation
  - [x] 6.1 Write unit tests for ShareService methods
    - Test CreateInternalShare with valid/invalid inputs
    - Test CreateExternalShare with confidentiality validation, expiry validation, permission validation
    - Test ApproveExternalShare, RejectExternalShare, RevokeExternalShare
    - Test ModifyExternalShare with status validation
    - Test query methods with filtering
    - _Requirements: 1.1, 2.1, 3.1, 4.1, 5.1, 14.1, 15.1_

  - [x] 6.2 Write unit tests for ShareAccessService methods
    - Test ValidateAndGrantAccess with various token statuses, expiry dates, passwords, download limits
    - Test LogAccessAttempt with successful and failed attempts
    - Test IncrementDownloadCount and IsDownloadLimitReached
    - Test ProcessExpiredShares and ProcessDisabledShares
    - _Requirements: 6.1, 6.2, 8.2, 8.3, 8.4, 9.1, 9.2, 9.3_

  - [x] 6.3 Write integration tests for database persistence
    - Test ExternalShare, ShareAccessLog, ShareToken table operations
    - Test foreign key constraints and cascading deletes
    - Test index performance on large datasets
    - Test immutability of ShareAccessLog (UPDATE/DELETE should fail)
    - _Requirements: 2.1, 9.5, 17.5_

  - [x] 6.4 Write integration tests for audit logging
    - Test audit entries created for all share operations
    - Test audit entries contain all required fields
    - Test audit trail immutability
    - _Requirements: 17.1, 17.2, 17.3, 17.4, 17.5_

  - [x] 6.5 Write integration tests for notification triggering
    - Test notifications sent at correct events (create, approve, reject, access, expiry, revoke)
    - Test notification content contains required information
    - Test notification preferences respected
    - _Requirements: 10.1, 10.2, 10.3, 10.4, 10.5, 11.1, 11.2, 11.4, 11.5_

  - [x] 6.6 Write end-to-end workflow tests
    - Test complete internal share lifecycle: create → access → revoke
    - Test complete external share lifecycle: create (Pending) → approve (Active) → access → expire → verify access denied
    - Test external share with download limit: create → download N times → verify disabled at limit
    - Test external share with password: create → access with wrong password (denied) → access with correct password (granted)
    - Test confidentiality upgrade: create external share → upgrade file to 1HC → verify auto-revocation
    - _Requirements: 1.1, 2.1, 3.1, 4.1, 5.1, 6.1, 8.2, 12.1, 12.2, 16.2, 16.3_

  - [x] 6.7 Validate all acceptance criteria from requirements
    - Verify each requirement acceptance criterion is covered by at least one test
    - Verify all error messages are user-friendly and descriptive
    - Verify all error messages do not expose sensitive system details
    - _Requirements: 18.1, 18.2, 18.3, 18.4, 18.5_

  - [x] 6.8 Final checkpoint - Ensure all tests pass
    - Ensure all unit tests pass
    - Ensure all property-based tests pass (100+ iterations each)
    - Ensure all integration tests pass
    - Ensure all end-to-end tests pass
    - Ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP, but are strongly recommended for correctness verification
- Each task references specific requirements for traceability
- Property-based tests validate universal correctness properties; unit tests validate specific examples
- Checkpoints ensure incremental validation and early error detection
- All code follows CMS architecture patterns: Repository → Service → Presentation, with manual DI in Program.cs
- All database access uses ADO.NET with parameterized queries; no ORM
- All share operations must call IAuditService for immutable audit trail
- All share lifecycle events must call INotificationService for email notifications
- Password hashing uses bcrypt (BouncyCastle 1.8.9) with work factor 12
- Token generation uses cryptographically secure GUID generation
