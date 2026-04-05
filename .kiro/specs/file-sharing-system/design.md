# File Sharing System - Technical Design Document (BR-005)

## Overview

The File Sharing System enables secure file and folder sharing through two distinct models: internal sharing (with specific users, departments, roles, or groups) and external sharing (via GUID-based tokens with security layers). The system integrates with existing CMS permission models, confidentiality levels, and audit infrastructure to provide comprehensive access control and compliance tracking.

### Key Design Principles

- **Layered Architecture**: Follows existing CMS patterns (Repository → Service → Presentation)
- **Permission Integration**: Respects existing folder/file permission hierarchies and confidentiality levels
- **Immutable Audit Trail**: All share operations logged via IAuditService; access logs are append-only
- **Security First**: Password hashing (bcrypt), token-based external access, confidentiality enforcement
- **Notification Integration**: Email notifications for share lifecycle events and access tracking
- **Soft Delete Pattern**: Shares use status transitions rather than hard deletion

---

## Architecture

### System Components

```
┌─────────────────────────────────────────────────────────────┐
│                    Presentation Layer                        │
│  (WinForms: ShareDialog, ShareManagementForm, AccessLogView) │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                    Service Layer                             │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ IShareService (Create, Revoke, Query shares)         │   │
│  │ IShareTokenService (Token generation, validation)    │   │
│  │ IShareAccessService (Access control, logging)        │   │
│  └──────────────────────────────────────────────────────┘   │
│  Dependencies: INotificationService, IAuditService,          │
│               IPermissionService, IFileService               │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                    Data Layer                                │
│  ┌──────────────────────────────────────────────────────┐   │
│  │ IExternalShareRepository (Token-based shares)        │   │
│  │ IShareAccessLogRepository (Access tracking)          │   │
│  │ IShareTokenRepository (Token lifecycle)              │   │
│  └──────────────────────────────────────────────────────┘   │
│  Existing: IFileDocumentRepository, IFolderRepository,       │
│            IPermissionRepository, IAuditRepository           │
└────────────────────────┬────────────────────────────────────┘
                         │
┌────────────────────────▼────────────────────────────────────┐
│                    Domain Layer                              │
│  Models: ExternalShare, ShareToken, ShareAccessLog,         │
│          InternalShare (via FilePermission)                  │
└────────────────────────────────────────────────────────────┘
```

### Data Flow: Internal Share Creation

```
User initiates share
    ↓
ShareService.CreateInternalShare()
    ├─ Validate: User has Read permission on file/folder
    ├─ Validate: Confidentiality level allows sharing
    ├─ Create FilePermission record (via IFilePermissionService)
    ├─ Log audit entry (via IAuditService)
    ├─ Send notification (via INotificationService)
    └─ Return share confirmation
```

### Data Flow: External Share Creation

```
User initiates external share
    ↓
ShareService.CreateExternalShare()
    ├─ Validate: User has Read permission
    ├─ Validate: File is NOT Highly Confidential (1HC)
    ├─ Validate: Folder does NOT contain 1HC files
    ├─ Validate: Expiry ≤ 90 days from now
    ├─ Generate unique GUID token (via IShareTokenService)
    ├─ Hash password if provided (bcrypt)
    ├─ Create ExternalShare record (status="Pending")
    ├─ Create ShareToken record
    ├─ Route to Manager for approval
    ├─ Log audit entry
    ├─ Send notification to Sharer with link
    └─ Return share confirmation
```

### Data Flow: External Share Access

```
Recipient accesses share link with token
    ↓
ShareAccessService.ValidateAndGrantAccess()
    ├─ Lookup ShareToken by GUID
    ├─ Verify status is "Active"
    ├─ Verify current date is within [start, expiry]
    ├─ If password-protected: verify password hash
    ├─ Verify download count < limit (if set)
    ├─ Log access attempt (via IAuditService)
    ├─ Increment download counter
    ├─ If download limit reached: transition status to "Disabled"
    ├─ Send access notification to Sharer
    └─ Grant file download
```

---

## Data Models

### Domain Models (CMS.Domain/Models)

#### ExternalShare

Represents a token-based external share with security constraints.

```csharp
public class ExternalShare
{
    public int ExternalShareID { get; set; }
    public Guid ShareToken { get; set; }
    public int FileID { get; set; }
    public int? FolderID { get; set; }
    public int SharerUserID { get; set; }
    public int? ApproverUserID { get; set; }
    
    // Security settings
    public string PasswordHash { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? DownloadLimit { get; set; }
    public int CurrentDownloadCount { get; set; }
    
    // Status: Pending, Active, Expired, Disabled, Revoked
    public string ShareTokenStatus { get; set; }
    
    // Lifecycle
    public DateTime CreatedDate { get; set; }
    public DateTime? ApprovedDate { get; set; }
    public DateTime? DisabledDate { get; set; }
    public string DisabledReason { get; set; }
    public int? RevokedBy { get; set; }
    public DateTime? RevokedDate { get; set; }
    public string RevocationReason { get; set; }
    
    // Metadata
    public string SharePurpose { get; set; }
    public bool NotifyOnAccess { get; set; }
}
```

#### ShareAccessLog

Immutable record of access attempts (successful and failed).

```csharp
public class ShareAccessLog
{
    public int AccessLogID { get; set; }
    public Guid ShareToken { get; set; }
    public int ExternalShareID { get; set; }
    public string RecipientIdentifier { get; set; } // User ID or "Anonymous"
    public string AccessType { get; set; } // "View" or "Download"
    public bool IsSuccessful { get; set; }
    public string FailureReason { get; set; } // "WrongPassword", "Expired", "LimitReached", etc.
    public long? FileSizeBytes { get; set; }
    public DateTime AccessTimestamp { get; set; }
    public string IPAddress { get; set; }
    public string UserAgent { get; set; }
}
```

#### ShareToken

Represents the token lifecycle and validation state.

```csharp
public class ShareToken
{
    public int ShareTokenID { get; set; }
    public Guid TokenValue { get; set; }
    public int ExternalShareID { get; set; }
    public string Status { get; set; } // Pending, Active, Expired, Disabled, Revoked
    public DateTime CreatedDate { get; set; }
    public DateTime? ActivatedDate { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public DateTime? DisabledDate { get; set; }
    public int? DisabledByUserID { get; set; }
    public string DisabledReason { get; set; }
}
```

### Integration with Existing Models

**FileDocument** and **Folder** models remain unchanged. Shares reference them via `FileID` and `FolderID`.

**FilePermission** model is used for internal shares:
- `PrincipalType`: 1=User, 2=Role, 3=OrgUnit, 4=Group
- `CanRead`, `CanWrite`, `CanDelete`, `CanModify`, `CanFullControl` flags
- `ExpiryDate` for time-limited shares
- `RevokedDate` and `RevokeReason` for revocation tracking

---

## Database Schema

### New Tables

#### storage.ExternalShare

```sql
CREATE TABLE storage.ExternalShare (
    ExternalShareID INT PRIMARY KEY IDENTITY(1,1),
    ShareToken UNIQUEIDENTIFIER NOT NULL UNIQUE,
    FileID INT NULL,
    FolderID INT NULL,
    SharerUserID INT NOT NULL,
    ApproverUserID INT NULL,
    PasswordHash NVARCHAR(255) NULL,
    ExpiryDate DATETIME NULL,
    DownloadLimit INT NULL,
    CurrentDownloadCount INT NOT NULL DEFAULT 0,
    ShareTokenStatus NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    CreatedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
    ApprovedDate DATETIME NULL,
    DisabledDate DATETIME NULL,
    DisabledReason NVARCHAR(500) NULL,
    RevokedBy INT NULL,
    RevokedDate DATETIME NULL,
    RevocationReason NVARCHAR(500) NULL,
    SharePurpose NVARCHAR(1000) NULL,
    NotifyOnAccess BIT NOT NULL DEFAULT 1,
    
    CONSTRAINT FK_ExternalShare_File FOREIGN KEY (FileID) 
        REFERENCES storage.FileDocument(FileID),
    CONSTRAINT FK_ExternalShare_Folder FOREIGN KEY (FolderID) 
        REFERENCES storage.Folder(FolderID),
    CONSTRAINT FK_ExternalShare_Sharer FOREIGN KEY (SharerUserID) 
        REFERENCES security.UserAccount(UserID),
    CONSTRAINT FK_ExternalShare_Approver FOREIGN KEY (ApproverUserID) 
        REFERENCES security.UserAccount(UserID),
    CONSTRAINT FK_ExternalShare_RevokedBy FOREIGN KEY (RevokedBy) 
        REFERENCES security.UserAccount(UserID),
    CONSTRAINT CK_ExternalShare_Status CHECK (ShareTokenStatus IN 
        ('Pending', 'Active', 'Expired', 'Disabled', 'Revoked')),
    CONSTRAINT CK_ExternalShare_FileOrFolder CHECK 
        ((FileID IS NOT NULL AND FolderID IS NULL) OR 
         (FileID IS NULL AND FolderID IS NOT NULL))
);

CREATE INDEX IX_ExternalShare_Token ON storage.ExternalShare(ShareToken);
CREATE INDEX IX_ExternalShare_Sharer ON storage.ExternalShare(SharerUserID);
CREATE INDEX IX_ExternalShare_Status ON storage.ExternalShare(ShareTokenStatus);
CREATE INDEX IX_ExternalShare_Expiry ON storage.ExternalShare(ExpiryDate);
```

#### storage.ShareAccessLog

```sql
CREATE TABLE storage.ShareAccessLog (
    AccessLogID BIGINT PRIMARY KEY IDENTITY(1,1),
    ShareToken UNIQUEIDENTIFIER NOT NULL,
    ExternalShareID INT NOT NULL,
    RecipientIdentifier NVARCHAR(255) NULL,
    AccessType NVARCHAR(20) NOT NULL,
    IsSuccessful BIT NOT NULL,
    FailureReason NVARCHAR(500) NULL,
    FileSizeBytes BIGINT NULL,
    AccessTimestamp DATETIME NOT NULL DEFAULT GETUTCDATE(),
    IPAddress NVARCHAR(45) NULL,
    UserAgent NVARCHAR(500) NULL,
    
    CONSTRAINT FK_ShareAccessLog_ExternalShare FOREIGN KEY (ExternalShareID) 
        REFERENCES storage.ExternalShare(ExternalShareID),
    CONSTRAINT CK_ShareAccessLog_AccessType CHECK (AccessType IN ('View', 'Download')),
    CONSTRAINT CK_ShareAccessLog_Immutable UNIQUE (AccessLogID)
);

CREATE INDEX IX_ShareAccessLog_Token ON storage.ShareAccessLog(ShareToken);
CREATE INDEX IX_ShareAccessLog_ExternalShare ON storage.ShareAccessLog(ExternalShareID);
CREATE INDEX IX_ShareAccessLog_Timestamp ON storage.ShareAccessLog(AccessTimestamp);
CREATE INDEX IX_ShareAccessLog_Recipient ON storage.ShareAccessLog(RecipientIdentifier);
```

#### storage.ShareToken

```sql
CREATE TABLE storage.ShareToken (
    ShareTokenID INT PRIMARY KEY IDENTITY(1,1),
    TokenValue UNIQUEIDENTIFIER NOT NULL UNIQUE,
    ExternalShareID INT NOT NULL,
    Status NVARCHAR(20) NOT NULL DEFAULT 'Pending',
    CreatedDate DATETIME NOT NULL DEFAULT GETUTCDATE(),
    ActivatedDate DATETIME NULL,
    ExpiryDate DATETIME NULL,
    DisabledDate DATETIME NULL,
    DisabledByUserID INT NULL,
    DisabledReason NVARCHAR(500) NULL,
    
    CONSTRAINT FK_ShareToken_ExternalShare FOREIGN KEY (ExternalShareID) 
        REFERENCES storage.ExternalShare(ExternalShareID),
    CONSTRAINT FK_ShareToken_DisabledBy FOREIGN KEY (DisabledByUserID) 
        REFERENCES security.UserAccount(UserID),
    CONSTRAINT CK_ShareToken_Status CHECK (Status IN 
        ('Pending', 'Active', 'Expired', 'Disabled', 'Revoked'))
);

CREATE INDEX IX_ShareToken_Value ON storage.ShareToken(TokenValue);
CREATE INDEX IX_ShareToken_ExternalShare ON storage.ShareToken(ExternalShareID);
CREATE INDEX IX_ShareToken_Status ON storage.ShareToken(Status);
```

### Indexes for Performance

- **IX_ExternalShare_Token**: Fast lookup by GUID token
- **IX_ExternalShare_Sharer**: Query shares by creator
- **IX_ExternalShare_Status**: Filter by status (Active, Pending, etc.)
- **IX_ExternalShare_Expiry**: Identify expired shares for auto-termination
- **IX_ShareAccessLog_Token**: Fast access log lookup
- **IX_ShareAccessLog_Timestamp**: Time-range queries for audit reports
- **IX_ShareToken_Value**: Token validation lookups

---

## Service Layer Design

### IShareService Interface

```csharp
namespace CMS.Services.Interfaces
{
    public interface IShareService
    {
        // Internal Sharing
        FilePermission CreateInternalShare(CreateInternalShareRequest request, UserContext user);
        void RevokeInternalShare(int filePermissionId, UserContext user);
        
        // External Sharing
        ExternalShare CreateExternalShare(CreateExternalShareRequest request, UserContext user);
        void ApproveExternalShare(int externalShareId, UserContext user);
        void RejectExternalShare(int externalShareId, string rejectionReason, UserContext user);
        void RevokeExternalShare(int externalShareId, string revocationReason, UserContext user);
        void ModifyExternalShare(ModifyExternalShareRequest request, UserContext user);
        
        // Querying
        IEnumerable<FilePermission> GetInternalSharesForUser(int userId, UserContext user);
        IEnumerable<ExternalShare> GetExternalSharesForUser(int userId, UserContext user);
        IEnumerable<ExternalShare> GetExternalSharesByStatus(string status, UserContext user);
        IEnumerable<ShareAccessLog> GetAccessLog(int externalShareId, UserContext user);
        
        // Validation
        bool CanShareFile(int fileId, UserContext user);
        bool CanShareFolder(int folderId, UserContext user);
    }
}
```

### IShareTokenService Interface

```csharp
namespace CMS.Services.Interfaces
{
    public interface IShareTokenService
    {
        // Token Generation & Validation
        Guid GenerateToken();
        bool ValidateToken(Guid token);
        ShareToken GetTokenInfo(Guid token);
        
        // Password Hashing
        string HashPassword(string password);
        bool VerifyPassword(string password, string hash);
        
        // Token Lifecycle
        void ActivateToken(Guid token, UserContext user);
        void ExpireToken(Guid token, UserContext user);
        void DisableToken(Guid token, string reason, UserContext user);
        void RevokeToken(Guid token, UserContext user);
    }
}
```

### IShareAccessService Interface

```csharp
namespace CMS.Services.Interfaces
{
    public interface IShareAccessService
    {
        // Access Control
        ShareAccessResult ValidateAndGrantAccess(Guid token, string password, UserContext user);
        void LogAccessAttempt(ShareAccessLog log);
        
        // Download Tracking
        void IncrementDownloadCount(int externalShareId);
        bool IsDownloadLimitReached(int externalShareId);
        
        // Auto-Termination
        void ProcessExpiredShares();
        void ProcessDisabledShares();
    }
}
```

### Service Implementation: ShareService

Key responsibilities:
- Permission validation (user must have Read on file/folder)
- Confidentiality level enforcement (reject 1HC external shares)
- Expiry validation (max 90 days)
- Audit logging for all operations
- Notification triggering
- Manager approval routing

```csharp
public class ShareService : IShareService
{
    private readonly IFileDocumentRepository _fileRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly IFilePermissionService _filePermissionService;
    private readonly IExternalShareRepository _externalShareRepository;
    private readonly IShareTokenService _shareTokenService;
    private readonly IPermissionService _permissionService;
    private readonly INotificationService _notificationService;
    private readonly IAuditService _auditService;
    private readonly ILoggingService _loggingService;
    
    // Constants
    private const int MaxShareDurationDays = 90;
    private const int PendingApprovalExpiryDays = 7;
    
    public ExternalShare CreateExternalShare(CreateExternalShareRequest request, UserContext user)
    {
        // 1. Validate user has Read permission
        if (!_permissionService.CheckPermission(user.UserID, request.FileID ?? request.FolderID, "Read"))
            throw new UnauthorizedAccessException("Insufficient permissions to share this file/folder.");
        
        // 2. Validate confidentiality level
        var file = request.FileID.HasValue ? _fileRepository.GetById(request.FileID.Value) : null;
        var folder = request.FolderID.HasValue ? _folderRepository.GetById(request.FolderID.Value) : null;
        
        if (file?.ConfidentialityLevel == (int)ConfidentialityLevel.HC)
            throw new InvalidOperationException("Highly Confidential files cannot be shared externally.");
        
        if (folder?.ConfidentialityLevel == (int)ConfidentialityLevel.HC)
            throw new InvalidOperationException("Folders with Highly Confidential content cannot be shared externally.");
        
        // 3. Validate expiry duration
        if (request.ExpiryDate.HasValue)
        {
            var duration = (request.ExpiryDate.Value - DateTime.Now).TotalDays;
            if (duration > MaxShareDurationDays)
                throw new InvalidOperationException($"Maximum share duration is {MaxShareDurationDays} days.");
        }
        
        // 4. Generate token and hash password
        var token = _shareTokenService.GenerateToken();
        var passwordHash = request.Password != null ? _shareTokenService.HashPassword(request.Password) : null;
        
        // 5. Create ExternalShare record (status="Pending")
        var externalShare = new ExternalShare
        {
            ShareToken = token,
            FileID = request.FileID,
            FolderID = request.FolderID,
            SharerUserID = user.UserID,
            PasswordHash = passwordHash,
            ExpiryDate = request.ExpiryDate,
            DownloadLimit = request.DownloadLimit,
            CurrentDownloadCount = 0,
            ShareTokenStatus = "Pending",
            CreatedDate = DateTime.Now,
            SharePurpose = request.SharePurpose,
            NotifyOnAccess = request.NotifyOnAccess
        };
        
        _externalShareRepository.Insert(externalShare);
        
        // 6. Log audit entry
        _auditService.Log(new AuditEntry
        {
            Module = "FileSharing",
            Action = "CreateExternalShare",
            UserID = user.UserID,
            UserName = user.ADUserName,
            TableName = "ExternalShare",
            RecordID = externalShare.ExternalShareID,
            NewValue = $"Token={token}, Status=Pending, Expiry={request.ExpiryDate}",
            Result = 1
        });
        
        // 7. Send notification to Sharer
        _notificationService.Send(new NotificationRequest
        {
            RecipientUserId = user.UserID,
            EventType = "ShareCreated",
            RelatedEntityId = externalShare.ExternalShareID.ToString(),
            MessageText = $"External share created. Link: {token}. Status: Pending approval."
        });
        
        return externalShare;
    }
}
```

---

## API/Method Contracts

### DTOs (CMS.Services/DTOs)

#### CreateInternalShareRequest

```csharp
public class CreateInternalShareRequest
{
    public int FileID { get; set; }
    public int PrincipalType { get; set; } // 1=User, 2=Role, 3=OrgUnit, 4=Group
    public int PrincipalID { get; set; }
    public bool CanRead { get; set; }
    public bool CanWrite { get; set; }
    public bool CanDelete { get; set; }
    public DateTime? ExpiryDate { get; set; }
}
```

#### CreateExternalShareRequest

```csharp
public class CreateExternalShareRequest
{
    public int? FileID { get; set; }
    public int? FolderID { get; set; }
    public string Password { get; set; }
    public DateTime? ExpiryDate { get; set; }
    public int? DownloadLimit { get; set; }
    public string SharePurpose { get; set; }
    public bool NotifyOnAccess { get; set; } = true;
}
```

#### ModifyExternalShareRequest

```csharp
public class ModifyExternalShareRequest
{
    public int ExternalShareID { get; set; }
    public DateTime? NewExpiryDate { get; set; }
    public int? NewDownloadLimit { get; set; }
    public string NewPassword { get; set; }
}
```

#### ShareAccessResult

```csharp
public class ShareAccessResult
{
    public bool IsGranted { get; set; }
    public string DenyReason { get; set; }
    public FileDocument File { get; set; }
    public Folder Folder { get; set; }
    public int RemainingDownloads { get; set; }
}
```

---

## Security Considerations

### Password Hashing

- **Algorithm**: bcrypt (via BouncyCastle 1.8.9)
- **Work Factor**: 12 (configurable)
- **Storage**: Hashed passwords stored in `ExternalShare.PasswordHash`
- **Verification**: Constant-time comparison to prevent timing attacks

```csharp
public string HashPassword(string password)
{
    return BCrypt.Net.BCrypt.HashPassword(password, workFactor: 12);
}

public bool VerifyPassword(string password, string hash)
{
    return BCrypt.Net.BCrypt.Verify(password, hash);
}
```

### Token Generation

- **Format**: GUID (Globally Unique Identifier)
- **Randomness**: Cryptographically secure (System.Security.Cryptography.RNGCryptoServiceProvider)
- **Storage**: Stored in `ExternalShare.ShareToken` and `ShareToken.TokenValue`
- **Uniqueness**: Database unique constraint enforced

### Confidentiality Level Enforcement

1. **External Share Rejection**: Reject creation if file/folder is 1HC
2. **Automatic Revocation**: If file upgraded to 1HC after share exists, auto-revoke external share
3. **Folder Scanning**: Check all files in folder for 1HC before allowing external folder share

### Permission Validation

- **Read Check**: User must have Read permission on file/folder to create share
- **Revocation Check**: Only Sharer or Admin can revoke share
- **Audit Query Check**: Only Manager+ can query access logs

### Token Lifecycle

- **Pending**: Created but not yet approved by Manager
- **Active**: Approved and accessible
- **Expired**: Reached expiry date or 7-day pending timeout
- **Disabled**: Reached download limit
- **Revoked**: Manually revoked by Sharer or Admin

---

## Notification Integration

### Notification Events

| Event | Recipient | Content |
|-------|-----------|---------|
| ShareCreated (Internal) | Recipient(s) | File name, Sharer, access level, timestamp |
| ShareCreated (External) | Sharer | File name, share link, security settings, timestamp |
| ShareApproved | Sharer | Approval confirmation, link now active |
| ShareRejected | Sharer | Rejection reason, timestamp |
| AccessNotification | Sharer | File name, recipient, access type, timestamp |
| DownloadNotification | Sharer | File name, recipient, file size, timestamp |
| ShareExpired | Sharer | Expiration reason (date or download limit), timestamp |
| ShareRevoked | Sharer | Revocation reason, timestamp |

### Notification Templates

Notifications are sent via `INotificationService.Send(NotificationRequest)`:

```csharp
_notificationService.Send(new NotificationRequest
{
    RecipientUserId = sharerUserId,
    EventType = "ExternalShareApproved",
    RelatedEntityId = externalShareId.ToString(),
    MessageText = $"Your external share for '{fileName}' has been approved. " +
                  $"Link: {shareLink}. Expires: {expiryDate}."
});
```

---

## Audit Trail Integration

### Audit Logging

All share operations logged via `IAuditService.Log(AuditEntry)`:

| Operation | Module | Action | Details |
|-----------|--------|--------|---------|
| Create Internal Share | FileSharing | CreateInternalShare | Target type, target ID, permissions |
| Create External Share | FileSharing | CreateExternalShare | Token, expiry, download limit |
| Approve External Share | FileSharing | ApproveExternalShare | Approver, approval timestamp |
| Reject External Share | FileSharing | RejectExternalShare | Rejection reason |
| Revoke Share | FileSharing | RevokeShare | Revocation reason |
| Modify Share | FileSharing | ModifyShare | Previous values, new values |
| Access Attempt | FileSharing | AccessAttempt | Token, recipient, success/failure, reason |
| Download | FileSharing | Download | Token, recipient, file size |

### Access Log Immutability

- `ShareAccessLog` table has unique constraint on `AccessLogID`
- No UPDATE or DELETE operations allowed on access logs
- Only INSERT operations permitted
- Audit trail ensures all access attempts (successful and failed) are recorded

---

## Error Handling

### Error Categories

| Category | Examples | HTTP Status |
|----------|----------|-------------|
| Validation | Invalid expiry, missing file, invalid principal | 400 Bad Request |
| Authorization | Insufficient permissions, not Sharer | 403 Forbidden |
| Not Found | Token not found, share not found | 404 Not Found |
| Conflict | File is 1HC, share already revoked | 409 Conflict |
| Server Error | Database failure, token generation failure | 500 Internal Server Error |

### Error Messages

User-friendly error messages returned to UI:

```
"File is Highly Confidential and cannot be shared externally."
"Expiry date exceeds maximum allowed duration of 90 days."
"Insufficient permissions to share this file."
"Share link has expired."
"Share link is disabled (download limit reached)."
"Incorrect password."
"Share link is pending approval."
"Share link has been revoked."
```

---

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system—essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: External Share Rejection for Highly Confidential Files

*For any* file with ConfidentialityLevel = HC (1), attempting to create an external share SHALL result in rejection with error message "Highly Confidential files cannot be shared externally."

**Validates: Requirements 3.1**

### Property 2: Folder Confidentiality Scanning

*For any* folder containing at least one file with ConfidentialityLevel = HC, attempting to create an external share for that folder SHALL result in rejection with error message "Folder contains restricted content."

**Validates: Requirements 3.2**

### Property 3: Automatic Revocation on Confidentiality Upgrade

*For any* external share in Active status, if the associated file's ConfidentialityLevel is changed to HC, the share SHALL automatically transition to Revoked status with reason "Confidentiality level upgraded to HC."

**Validates: Requirements 3.3**

### Property 4: Expiry Duration Validation

*For any* expiry date specified more than 90 days from share creation, the system SHALL reject the request with error message "Maximum share duration is 90 days."

**Validates: Requirements 4.1, 4.2**

### Property 5: Download Limit Enforcement

*For any* external share with a download limit N, after exactly N downloads, the share status SHALL automatically transition to Disabled and further download attempts SHALL be denied.

**Validates: Requirements 4.4, 8.2, 8.3, 8.4**

### Property 6: Expiry Date Enforcement

*For any* external share with an expiry date, after that date passes, the share status SHALL automatically transition to Expired and access attempts SHALL be denied with error message "Link expired."

**Validates: Requirements 4.5, 6.4, 6.5**

### Property 7: Token Uniqueness

*For any* two external shares created in the system, their ShareToken GUIDs SHALL be unique.

**Validates: Requirements 2.1**

### Property 8: Status Transition: Pending to Active

*For any* external share in Pending status, after Manager approval, the status SHALL transition to Active and the ApprovedDate SHALL be set to current timestamp.

**Validates: Requirements 2.5, 5.3**

### Property 9: Status Transition: Pending to Expired

*For any* external share in Pending status that remains unapproved for more than 7 days, the status SHALL automatically transition to Expired.

**Validates: Requirements 5.5**

### Property 10: Status Transition: Pending to Revoked

*For any* external share in Pending status that is rejected by a Manager, the status SHALL transition to Revoked.

**Validates: Requirements 5.4**

### Property 11: Access Control by Status

*For any* external share with status other than Active, access attempts SHALL be denied with appropriate error message (Pending: "Link pending approval", Expired: "Link expired", Disabled: "Link disabled", Revoked: "Link revoked").

**Validates: Requirements 6.1, 6.2, 12.5**

### Property 12: Password Hashing

*For any* password string P, the hash H = HashPassword(P) SHALL satisfy: VerifyPassword(P, H) = true AND VerifyPassword(P', H) = false for any P' ≠ P.

**Validates: Requirements 7.1, 7.3**

### Property 13: Password Verification

*For any* password-protected external share, access with correct password SHALL proceed to further validation, and access with incorrect password SHALL be denied with error message "Incorrect password."

**Validates: Requirements 7.4, 7.5**

### Property 14: Download Counter Initialization

*For any* external share created with download limit L, the CurrentDownloadCount SHALL be initialized to 0.

**Validates: Requirements 8.1**

### Property 15: Download Counter Increment

*For any* successful download via external share, the CurrentDownloadCount SHALL increment by exactly 1.

**Validates: Requirements 8.2**

### Property 16: Access Log Immutability

*For any* ShareAccessLog entry created, subsequent UPDATE or DELETE operations on that entry SHALL fail with database constraint violation.

**Validates: Requirements 9.5**

### Property 17: Access Log Completeness

*For any* successful access to an external share, a ShareAccessLog entry SHALL be created containing: ShareToken, RecipientIdentifier, AccessType, IsSuccessful=true, AccessTimestamp, and FileSizeBytes (if download).

**Validates: Requirements 9.1, 9.2**

### Property 18: Failed Access Logging

*For any* failed access attempt (wrong password, expired link, disabled link), a ShareAccessLog entry SHALL be created with IsSuccessful=false and FailureReason set to the specific reason.

**Validates: Requirements 9.3**

### Property 19: Access Log Chronological Ordering

*For any* query of access logs for a shared file, returned entries SHALL be ordered by AccessTimestamp in ascending order.

**Validates: Requirements 9.4**

### Property 20: Permission Validation for Share Creation

*For any* user without Read permission on a file/folder, attempting to create a share SHALL be rejected with error message "Insufficient permissions to share this file/folder."

**Validates: Requirements 15.1, 15.2**

### Property 21: Revocation Authorization

*For any* external share, only the original Sharer or a system administrator SHALL be able to revoke it; other users SHALL receive error message "Insufficient permissions to revoke this share."

**Validates: Requirements 15.3, 15.4**

### Property 22: Access Log Authorization

*For any* access log query, only the original Sharer or a system administrator with audit permissions SHALL be able to view it; other users SHALL receive error message "Insufficient permissions to view access logs."

**Validates: Requirements 15.5**

### Property 23: Confidentiality Level Inheritance

*For any* file or folder that is shared, the share record SHALL inherit the ConfidentialityLevel from the file/folder metadata.

**Validates: Requirements 16.1**

### Property 24: Permission Hierarchy Enforcement

*For any* internal share created, the granted permissions SHALL not exceed the permissions the Sharer possesses on the file/folder.

**Validates: Requirements 16.4**

### Property 25: Audit Trail Completeness for Share Creation

*For any* share created (internal or external), an audit entry SHALL be created with: Module="FileSharing", Action="CreateInternalShare" or "CreateExternalShare", UserID, UserName, TableName, RecordID, NewValue containing security settings, and Result=1 (Success).

**Validates: Requirements 17.1**

### Property 26: Audit Trail Completeness for Share Modification

*For any* share modified, an audit entry SHALL be created with: Module="FileSharing", Action="ModifyShare", UserID, TableName, RecordID, OldValue containing previous values, NewValue containing new values, and timestamp.

**Validates: Requirements 17.2**

### Property 27: Audit Trail Completeness for Share Revocation

*For any* share revoked or disabled, an audit entry SHALL be created with: Module="FileSharing", Action="RevokeShare" or "DisableShare", UserID, TableName, RecordID, NewValue containing revocation/disablement reason, and timestamp.

**Validates: Requirements 17.3, 17.4**

### Property 28: Audit Trail Immutability

*For any* audit entry related to share operations, subsequent UPDATE or DELETE operations SHALL fail with database constraint violation.

**Validates: Requirements 17.5**

### Property 29: Error Message Descriptiveness

*For any* share operation failure, the returned error message SHALL be descriptive and indicate the specific reason for failure (e.g., "File is Highly Confidential", "Expiry exceeds 90 days", "Insufficient permissions").

**Validates: Requirements 18.1, 18.2**

### Property 30: Error Message Security

*For any* error message returned to the user, it SHALL NOT expose sensitive system details (e.g., database schema, internal IDs, file paths).

**Validates: Requirements 18.5**

### Property 31: Internal Share Immediate Application

*For any* internal share created, the recipient SHALL immediately have the granted permissions without requiring approval.

**Validates: Requirements 1.4**

### Property 32: Internal Share Metadata Recording

*For any* internal share created, the system SHALL record: Sharer, target type, target identifier, and creation timestamp.

**Validates: Requirements 1.2**

### Property 33: External Share Metadata Recording

*For any* external share created, the system SHALL record: ShareToken, Sharer, file/folder reference, creation timestamp, and initial status="Pending".

**Validates: Requirements 2.2**

### Property 34: Share Query Completeness

*For any* user querying their shares, the system SHALL return all shares (internal and external) created by that user with: share type, target/recipient, file/folder name, creation timestamp, and current status.

**Validates: Requirements 13.1**

### Property 35: External Share Details Completeness

*For any* external share queried, the system SHALL display: ShareToken, creation timestamp, expiry date (if set), download limit (if set), current download count, password protection status, and ShareTokenStatus.

**Validates: Requirements 13.2**

### Property 36: Share Status Filtering

*For any* filter by ShareTokenStatus, the system SHALL return only shares with that status (Active, Pending, Expired, Disabled, or Revoked).

**Validates: Requirements 13.4**

### Property 37: Share Date Range Filtering

*For any* filter by date range [StartDate, EndDate], the system SHALL return only shares created within that range (CreatedDate >= StartDate AND CreatedDate <= EndDate).

**Validates: Requirements 13.5**

### Property 38: Share Modification Validation

*For any* attempt to modify an external share with status Expired or Disabled, the system SHALL reject the modification with error message "Cannot modify inactive shares."

**Validates: Requirements 14.4**

### Property 39: Notification on Internal Share Creation

*For any* internal share created, the Notification_Service SHALL be called with: RecipientUserId, EventType="InternalShareCreated", MessageText containing file/folder name, Sharer name, and access level.

**Validates: Requirements 1.3, 10.1**

### Property 40: Notification on External Share Creation

*For any* external share created, the Notification_Service SHALL be called with: RecipientUserId=Sharer, EventType="ExternalShareCreated", MessageText containing file/folder name, share link, security settings, and timestamp.

**Validates: Requirements 2.4, 10.2**

### Property 41: Notification on Share Approval

*For any* external share approved by a Manager, the Notification_Service SHALL be called with: RecipientUserId=Sharer, EventType="ShareApproved", MessageText confirming approval and indicating link is now active.

**Validates: Requirements 10.3**

### Property 42: Notification on Share Rejection

*For any* external share rejected by a Manager, the Notification_Service SHALL be called with: RecipientUserId=Sharer, EventType="ShareRejected", MessageText containing rejection reason and timestamp.

**Validates: Requirements 10.4**

### Property 43: Notification on Auto-Disablement

*For any* external share automatically disabled (expiry or download limit), the Notification_Service SHALL be called with: RecipientUserId=Sharer, EventType="ShareDisabled", MessageText containing disablement reason and timestamp.

**Validates: Requirements 10.5**

### Property 44: Notification on Access

*For any* successful access to an external share, if NotifyOnAccess=true, the Notification_Service SHALL be called with: RecipientUserId=Sharer, EventType="FileAccessed", MessageText containing file name, recipient identifier, access timestamp, and access type.

**Validates: Requirements 11.1, 11.2**

### Property 45: Notification on Failed Access

*For any* failed access attempt to an external share, if NotifyOnAccess=true, the Notification_Service SHALL be called with: RecipientUserId=Sharer, EventType="AccessFailed", MessageText containing file name, failure reason, timestamp, and recipient identifier.

**Validates: Requirements 11.4**

### Property 46: Notification Preference Respect

*For any* user with NotifyOnAccess=false, access notifications SHALL NOT be sent regardless of access attempts.

**Validates: Requirements 11.5**

### Property 47: Notification Batching

*For any* multiple downloads within a configurable time window (e.g., 1 hour), the Notification_Service MAY batch notifications into a single email to avoid excessive volume.

**Validates: Requirements 11.3**

---

## Testing Strategy

### Property-Based Testing Applicability

The File Sharing System is well-suited for property-based testing because:

1. **Pure Business Logic**: Share validation, permission checks, status transitions are deterministic functions
2. **Universal Properties**: Rules like "1HC files cannot be shared externally" apply to all files, not just specific examples
3. **Large Input Space**: Expiry dates, download limits, passwords, and user combinations create a large input space where randomization reveals edge cases
4. **Correctness Verification**: Properties like "download counter increments by 1" and "status transitions are correct" are universal invariants

### Unit Tests (Property-Based)

Each property-based test runs minimum 100 iterations with randomized inputs.

**Property 1: External Share Rejection for Highly Confidential Files**
- **Test**: Generate random files with varying ConfidentialityLevels; verify only HC files are rejected
- **Tag**: Feature: file-sharing-system, Property 1: External Share Rejection for Highly Confidential Files
- **Iterations**: 100+

**Property 3: Automatic Revocation on Confidentiality Upgrade**
- **Test**: Create external shares, upgrade file confidentiality to HC, verify auto-revocation
- **Tag**: Feature: file-sharing-system, Property 3: Automatic Revocation on Confidentiality Upgrade
- **Iterations**: 100+

**Property 4: Expiry Duration Validation**
- **Test**: Generate random expiry dates; verify dates > 90 days are rejected
- **Tag**: Feature: file-sharing-system, Property 4: Expiry Duration Validation
- **Iterations**: 100+

**Property 5: Download Limit Enforcement**
- **Test**: Create shares with random download limits; simulate downloads; verify status transitions at limit
- **Tag**: Feature: file-sharing-system, Property 5: Download Limit Enforcement
- **Iterations**: 100+

**Property 6: Expiry Date Enforcement**
- **Test**: Create shares with random expiry dates; simulate time passing; verify status transitions
- **Tag**: Feature: file-sharing-system, Property 6: Expiry Date Enforcement
- **Iterations**: 100+

**Property 7: Token Uniqueness**
- **Test**: Generate 1000+ tokens; verify all are unique GUIDs
- **Tag**: Feature: file-sharing-system, Property 7: Token Uniqueness
- **Iterations**: 100+

**Property 8: Status Transition: Pending to Active**
- **Test**: Create shares, approve them; verify status transitions and ApprovedDate is set
- **Tag**: Feature: file-sharing-system, Property 8: Status Transition: Pending to Active
- **Iterations**: 100+

**Property 11: Access Control by Status**
- **Test**: Create shares with random statuses; verify access is denied for non-Active statuses
- **Tag**: Feature: file-sharing-system, Property 11: Access Control by Status
- **Iterations**: 100+

**Property 12: Password Hashing**
- **Test**: Generate random passwords; verify hashes are unique and verification works correctly
- **Tag**: Feature: file-sharing-system, Property 12: Password Hashing
- **Iterations**: 100+

**Property 15: Download Counter Increment**
- **Test**: Create shares, simulate downloads; verify counter increments by exactly 1 each time
- **Tag**: Feature: file-sharing-system, Property 15: Download Counter Increment
- **Iterations**: 100+

**Property 16: Access Log Immutability**
- **Test**: Create access logs; attempt UPDATE/DELETE operations; verify they fail
- **Tag**: Feature: file-sharing-system, Property 16: Access Log Immutability
- **Iterations**: 100+

**Property 20: Permission Validation for Share Creation**
- **Test**: Generate users with varying permissions; verify only those with Read can create shares
- **Tag**: Feature: file-sharing-system, Property 20: Permission Validation for Share Creation
- **Iterations**: 100+

**Property 23: Confidentiality Level Inheritance**
- **Test**: Create shares for files with random confidentiality levels; verify inheritance
- **Tag**: Feature: file-sharing-system, Property 23: Confidentiality Level Inheritance
- **Iterations**: 100+

**Property 36: Share Status Filtering**
- **Test**: Create shares with random statuses; filter by each status; verify correct results
- **Tag**: Feature: file-sharing-system, Property 36: Share Status Filtering
- **Iterations**: 100+

**Property 37: Share Date Range Filtering**
- **Test**: Create shares with random creation dates; filter by date ranges; verify correct results
- **Tag**: Feature: file-sharing-system, Property 37: Share Date Range Filtering
- **Iterations**: 100+

### Unit Tests (Example-Based)

**Test**: CreateExternalShare_RejectsHighlyConfidentialFile
```csharp
[TestMethod]
public void CreateExternalShare_RejectsHighlyConfidentialFile()
{
    // Arrange
    var file = new FileDocument { FileID = 1, ConfidentialityLevel = (int)ConfidentialityLevel.HC };
    var user = new UserContext { UserID = 1, AppRole = 1 };
    var request = new CreateExternalShareRequest { FileID = 1 };
    
    _fileRepository.Setup(r => r.GetById(1)).Returns(file);
    _permissionService.Setup(p => p.CheckPermission(1, 1, "Read")).Returns(true);
    
    // Act & Assert
    Assert.ThrowsException<InvalidOperationException>(() =>
        _shareService.CreateExternalShare(request, user),
        "Highly Confidential files cannot be shared externally.");
}
```

**Test**: ApproveExternalShare_TransitionsStatusToActive
```csharp
[TestMethod]
public void ApproveExternalShare_TransitionsStatusToActive()
{
    // Arrange
    var share = new ExternalShare 
    { 
        ExternalShareID = 1, 
        ShareTokenStatus = "Pending",
        ApprovedDate = null
    };
    var user = new UserContext { UserID = 2, AppRole = 2 }; // Manager
    
    _externalShareRepository.Setup(r => r.GetById(1)).Returns(share);
    
    // Act
    _shareService.ApproveExternalShare(1, user);
    
    // Assert
    Assert.AreEqual("Active", share.ShareTokenStatus);
    Assert.IsNotNull(share.ApprovedDate);
    Assert.AreEqual(2, share.ApproverUserID);
}
```

**Test**: RevokeExternalShare_PreventsAccess
```csharp
[TestMethod]
public void RevokeExternalShare_PreventsAccess()
{
    // Arrange
    var share = new ExternalShare 
    { 
        ExternalShareID = 1, 
        ShareToken = Guid.NewGuid(),
        ShareTokenStatus = "Active"
    };
    var user = new UserContext { UserID = 1, AppRole = 1 };
    
    _externalShareRepository.Setup(r => r.GetById(1)).Returns(share);
    
    // Act
    _shareService.RevokeExternalShare(1, "User requested revocation", user);
    
    // Assert
    Assert.AreEqual("Revoked", share.ShareTokenStatus);
    var result = _shareAccessService.ValidateAndGrantAccess(share.ShareToken, null, user);
    Assert.IsFalse(result.IsGranted);
    Assert.AreEqual("Link revoked", result.DenyReason);
}
```

### Integration Tests

**Test Scope**: Database persistence, audit logging, notification triggering, end-to-end workflows

**Test**: ShareLifecycle_CreateApproveAccessExpire
```csharp
[TestMethod]
public void ShareLifecycle_CreateApproveAccessExpire()
{
    // Arrange
    var file = new FileDocument { FileID = 1, ConfidentialityLevel = (int)ConfidentialityLevel.NC };
    var sharer = new UserContext { UserID = 1, ADUserName = "user1" };
    var manager = new UserContext { UserID = 2, ADUserName = "manager1", AppRole = 2 };
    var recipient = new UserContext { UserID = 3, ADUserName = "user3" };
    
    // Act 1: Create external share
    var request = new CreateExternalShareRequest 
    { 
        FileID = 1,
        ExpiryDate = DateTime.Now.AddDays(30),
        DownloadLimit = 5,
        NotifyOnAccess = true
    };
    var share = _shareService.CreateExternalShare(request, sharer);
    
    // Assert 1: Share created in Pending status
    Assert.AreEqual("Pending", share.ShareTokenStatus);
    Assert.IsNull(share.ApprovedDate);
    
    // Act 2: Manager approves share
    _shareService.ApproveExternalShare(share.ExternalShareID, manager);
    
    // Assert 2: Share is now Active
    var activeShare = _externalShareRepository.GetById(share.ExternalShareID);
    Assert.AreEqual("Active", activeShare.ShareTokenStatus);
    Assert.IsNotNull(activeShare.ApprovedDate);
    
    // Act 3: Recipient accesses share
    var accessResult = _shareAccessService.ValidateAndGrantAccess(share.ShareToken, null, recipient);
    
    // Assert 3: Access granted
    Assert.IsTrue(accessResult.IsGranted);
    
    // Act 4: Verify access log created
    var logs = _shareAccessLogRepository.GetByExternalShare(share.ExternalShareID);
    Assert.AreEqual(1, logs.Count());
    Assert.AreEqual("View", logs.First().AccessType);
    
    // Act 5: Simulate expiry
    _shareService.ProcessExpiredShares();
    
    // Assert 5: Share is now Expired
    var expiredShare = _externalShareRepository.GetById(share.ExternalShareID);
    Assert.AreEqual("Expired", expiredShare.ShareTokenStatus);
    
    // Act 6: Attempt access after expiry
    var deniedResult = _shareAccessService.ValidateAndGrantAccess(share.ShareToken, null, recipient);
    
    // Assert 6: Access denied
    Assert.IsFalse(deniedResult.IsGranted);
    Assert.AreEqual("Link expired", deniedResult.DenyReason);
}
```

**Test**: AuditTrail_AllOperationsLogged
```csharp
[TestMethod]
public void AuditTrail_AllOperationsLogged()
{
    // Arrange
    var file = new FileDocument { FileID = 1, ConfidentialityLevel = (int)ConfidentialityLevel.NC };
    var user = new UserContext { UserID = 1, ADUserName = "user1" };
    
    // Act: Create external share
    var request = new CreateExternalShareRequest { FileID = 1 };
    var share = _shareService.CreateExternalShare(request, user);
    
    // Assert: Audit entry created
    var auditEntries = _auditRepository.Query(new AuditQuery 
    { 
        Module = "FileSharing",
        Action = "CreateExternalShare",
        UserID = 1
    }).Items;
    
    Assert.IsTrue(auditEntries.Any());
    var entry = auditEntries.First();
    Assert.AreEqual("FileSharing", entry.Module);
    Assert.AreEqual("CreateExternalShare", entry.Action);
    Assert.AreEqual(1, entry.UserID);
    Assert.AreEqual("user1", entry.UserName);
    Assert.IsTrue(entry.NewValue.Contains("Pending"));
}
```

**Test**: Notifications_SentAtCorrectEvents
```csharp
[TestMethod]
public void Notifications_SentAtCorrectEvents()
{
    // Arrange
    var file = new FileDocument { FileID = 1, ConfidentialityLevel = (int)ConfidentialityLevel.NC };
    var user = new UserContext { UserID = 1, ADUserName = "user1" };
    
    _notificationService.Setup(n => n.Send(It.IsAny<NotificationRequest>())).Verifiable();
    
    // Act: Create external share
    var request = new CreateExternalShareRequest { FileID = 1 };
    var share = _shareService.CreateExternalShare(request, user);
    
    // Assert: Notification sent to Sharer
    _notificationService.Verify(n => n.Send(It.Is<NotificationRequest>(r =>
        r.RecipientUserId == 1 &&
        r.EventType == "ExternalShareCreated"
    )), Times.Once);
}
```

### Test Coverage Summary

- **Unit Tests**: 50+ tests covering service layer logic, validation, permission checks
- **Property-Based Tests**: 15+ properties with 100+ iterations each
- **Integration Tests**: 10+ tests covering database persistence, audit logging, notifications
- **Total Coverage**: 75+ tests ensuring comprehensive correctness verification

---

## Implementation Roadmap

### Phase 1: Domain & Data Layer
- Create domain models (ExternalShare, ShareAccessLog, ShareToken)
- Create database tables and indexes
- Create repository interfaces and implementations
- Create migration script

### Phase 2: Service Layer
- Implement IShareTokenService (token generation, password hashing)
- Implement IShareAccessService (access control, logging)
- Implement IShareService (share creation, approval, revocation)
- Integrate with existing services (INotificationService, IAuditService)

### Phase 3: Presentation Layer
- Create ShareDialog (internal and external share creation)
- Create ShareManagementForm (view/revoke shares)
- Create AccessLogView (audit trail)
- Integrate with existing file/folder UI

### Phase 4: Testing & Deployment
- Unit tests for all service methods
- Integration tests for database and audit logging
- Manual testing of UI workflows
- Database migration deployment

