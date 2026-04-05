-- File Sharing System Tables
-- Migration: 030_FileSharing_Tables.sql
-- Description: Creates ExternalShare, ShareAccessLog, and ShareToken tables for the File Sharing System (BR-005)

-- =====================================================================
-- storage.ExternalShare Table
-- =====================================================================
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

-- =====================================================================
-- storage.ShareAccessLog Table
-- =====================================================================
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

-- =====================================================================
-- storage.ShareToken Table
-- =====================================================================
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
