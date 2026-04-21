# Database Schema Design & Documentation

## 📊 Overview

The CMS database follows a normalized relational model with clear separation between master data, configuration, transactions, and logs. This document provides a comprehensive reference for database structure, relationships, and design decisions.

---

## Schema Prefix Convention

| Prefix | Type | Purpose | Example |
|--------|------|---------|---------|
| **m_** | Master | Master/reference data | `m_OrgUnit`, `m_UserAccount` |
| **s_** | Setup | Configuration/settings | `s_FolderPermission`, `s_WorkflowDefinition` |
| **t_** | Transaction | Transactional data | `t_FileDocument`, `t_TaskItem` |
| **l_** | Log | Audit/activity logs | `l_AuditLog`, `l_LoginAttempt` |
| **r_** | Reference | System references | `r_FileType`, `r_Status` |
| **c_** | Configuration | System configuration | `c_SystemConfig`, `c_EmailTemplate` |

---

## Core Tables

### 1. Organizational Structure

#### m_OrgUnit (Organization Unit)
```sql
CREATE TABLE m_OrgUnit (
    OrgUnitID INT PRIMARY KEY IDENTITY(1,1),
    OrgUnitCode NVARCHAR(50) NOT NULL UNIQUE,
    OrgUnitNameTH NVARCHAR(200) NOT NULL COLLATE Thai_CI_AS,
    OrgUnitNameEN NVARCHAR(200),
    ParentOrgUnitID INT NULL,                      -- Self-referential for hierarchy
    UploadQuotaBytes BIGINT NOT NULL DEFAULT 42949672960,  -- 40GB
    IsActive BIT NOT NULL DEFAULT 1,
    
    -- Audit
    CreatedBy NVARCHAR(255) NOT NULL,
    CreatedDate DATETIME NOT NULL,
    UpdatedBy NVARCHAR(255) NULL,
    UpdatedDate DATETIME NULL,
    
    FOREIGN KEY (ParentOrgUnitID) REFERENCES m_OrgUnit(OrgUnitID),
    INDEX idx_code (OrgUnitCode),
    INDEX idx_parent (ParentOrgUnitID)
);
```

#### m_Department, m_Division, m_Position (HR Structure)
```sql
CREATE TABLE m_Department (
    DepartmentID INT PRIMARY KEY IDENTITY(1,1),
    DepartmentCode NVARCHAR(50) NOT NULL UNIQUE,
    DepartmentNameTH NVARCHAR(200) NOT NULL COLLATE Thai_CI_AS,
    DepartmentNameEN NVARCHAR(200),
    OrgUnitID INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    
    FOREIGN KEY (OrgUnitID) REFERENCES m_OrgUnit(OrgUnitID),
    INDEX idx_orgunit (OrgUnitID)
);

CREATE TABLE m_Division (
    DivisionID INT PRIMARY KEY IDENTITY(1,1),
    DivisionCode NVARCHAR(50) NOT NULL UNIQUE,
    DivisionNameTH NVARCHAR(200) NOT NULL COLLATE Thai_CI_AS,
    DepartmentID INT NOT NULL,
    
    FOREIGN KEY (DepartmentID) REFERENCES m_Department(DepartmentID),
    INDEX idx_department (DepartmentID)
);

CREATE TABLE m_Position (
    PositionID INT PRIMARY KEY IDENTITY(1,1),
    PositionCode NVARCHAR(50) NOT NULL UNIQUE,
    PositionNameTH NVARCHAR(200) NOT NULL COLLATE Thai_CI_AS,
    PositionNameEN NVARCHAR(200),
    
    INDEX idx_code (PositionCode)
);
```

#### m_UserAccount (Application Users)
```sql
CREATE TABLE m_UserAccount (
    UserID INT PRIMARY KEY IDENTITY(1,1),
    ADUserName NVARCHAR(255) NOT NULL UNIQUE,      -- domain\username
    DisplayName NVARCHAR(255) NOT NULL,
    EmployeeCode NVARCHAR(50),
    Email NVARCHAR(255),
    OrgUnitID INT NOT NULL,
    AppRole INT NOT NULL DEFAULT 1,                 -- FK to r_UserRole
    IsInactive BIT NOT NULL DEFAULT 0,
    LastLoginDate DATETIME NULL,
    LastLoginIP NVARCHAR(50) NULL,
    
    -- Audit
    CreatedBy NVARCHAR(255) NOT NULL,
    CreatedDate DATETIME NOT NULL,
    UpdatedBy NVARCHAR(255) NULL,
    UpdatedDate DATETIME NULL,
    
    FOREIGN KEY (OrgUnitID) REFERENCES m_OrgUnit(OrgUnitID),
    INDEX idx_username (ADUserName),
    INDEX idx_orgunit (OrgUnitID),
    INDEX idx_role (AppRole)
);
```

---

### 2. Folder Management

#### t_Folder (Folder Hierarchy)
```sql
CREATE TABLE t_Folder (
    FolderID INT PRIMARY KEY IDENTITY(1,1),
    OrgUnitID INT NOT NULL,
    ParentFolderID INT NULL,                        -- Self-referential hierarchy
    FolderName NVARCHAR(255) NOT NULL COLLATE Thai_CI_AS,
    FolderDepth TINYINT NOT NULL DEFAULT 0,        -- Current depth (0 = root)
    PhysicalPath NVARCHAR(500) NOT NULL,           -- Network path
    TemplateID INT NULL,
    ConfidentialityLevel INT NOT NULL,             -- 1HC, 2SC, 3NC, 4PC
    InheritPermission BIT NOT NULL DEFAULT 1,      -- Inherit from parent
    IsMainRoot BIT NOT NULL DEFAULT 0,             -- System root folder
    
    -- Audit
    CreatedBy NVARCHAR(255) NOT NULL,
    CreatedDate DATETIME NOT NULL,
    UpdatedBy NVARCHAR(255) NULL,
    UpdatedDate DATETIME NULL,
    
    -- Soft delete
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedByADUser NVARCHAR(255) NULL,
    DeletedDate DATETIME NULL,
    
    FOREIGN KEY (OrgUnitID) REFERENCES m_OrgUnit(OrgUnitID),
    FOREIGN KEY (ParentFolderID) REFERENCES t_Folder(FolderID),
    INDEX idx_orgunit (OrgUnitID),
    INDEX idx_parent (ParentFolderID),
    INDEX idx_confidentiality (ConfidentialityLevel),
    INDEX idx_deleted (IsDeleted)
);
```

#### s_FolderPermission (Folder-level Permissions)
```sql
CREATE TABLE s_FolderPermission (
    FolderPermissionID INT PRIMARY KEY IDENTITY(1,1),
    FolderID INT NOT NULL,
    UserID INT NULL,                                -- Null = role-based
    RoleID INT NULL,
    PermissionType NVARCHAR(50) NOT NULL,          -- View, Edit, Upload, Delete
    PermissionMode INT NOT NULL,                    -- 1=Allow, 2=Deny
    EffectiveFrom DATETIME NULL,
    EffectiveTo DATETIME NULL,
    
    -- Audit
    CreatedBy NVARCHAR(255) NOT NULL,
    CreatedDate DATETIME NOT NULL,
    
    FOREIGN KEY (FolderID) REFERENCES t_Folder(FolderID) ON DELETE CASCADE,
    FOREIGN KEY (UserID) REFERENCES m_UserAccount(UserID),
    INDEX idx_folder (FolderID),
    INDEX idx_user (UserID),
    INDEX idx_permission (PermissionType),
    UNIQUE (FolderID, UserID, PermissionType)
);
```

#### s_FolderTemplate (Folder Templates)
```sql
CREATE TABLE s_FolderTemplate (
    TemplateID INT PRIMARY KEY IDENTITY(1,1),
    TemplateName NVARCHAR(100) NOT NULL COLLATE Thai_CI_AS,
    Description NVARCHAR(MAX),
    IsActive BIT NOT NULL DEFAULT 1,
    
    CreatedBy NVARCHAR(255) NOT NULL,
    CreatedDate DATETIME NOT NULL,
    
    INDEX idx_active (IsActive)
);

CREATE TABLE s_FolderTemplateItem (
    TemplateItemID INT PRIMARY KEY IDENTITY(1,1),
    TemplateID INT NOT NULL,
    SubFolderName NVARCHAR(255) NOT NULL COLLATE Thai_CI_AS,
    DisplayOrder INT NOT NULL,
    
    FOREIGN KEY (TemplateID) REFERENCES s_FolderTemplate(TemplateID) ON DELETE CASCADE,
    INDEX idx_template (TemplateID)
);
```

---

### 3. File Management

#### t_FileDocument (File Records)
```sql
CREATE TABLE t_FileDocument (
    FileID INT PRIMARY KEY IDENTITY(1,1),
    FolderID INT NOT NULL,
    FileName NVARCHAR(255) NOT NULL COLLATE Thai_CI_AS,
    FileExtension NVARCHAR(10),
    FileSizeBytes BIGINT NOT NULL,
    FileHash NVARCHAR(64),                          -- SHA-256 hash
    PhysicalFileName NVARCHAR(500) NOT NULL,       -- Stored name on disk
    PhysicalPath NVARCHAR(500) NOT NULL,           -- Full UNC path
    UploadedBy NVARCHAR(255) NOT NULL,             -- ADUserName
    UploadedDate DATETIME NOT NULL,
    
    VirusScanStatus INT NOT NULL DEFAULT 0,        -- 0=Pending, 1=Clean, 2=Infected
    VirusScanDate DATETIME NULL,
    
    -- Audit
    CreatedBy NVARCHAR(255) NOT NULL,
    CreatedDate DATETIME NOT NULL,
    UpdatedBy NVARCHAR(255) NULL,
    UpdatedDate DATETIME NULL,
    
    -- Soft delete
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedByADUser NVARCHAR(255) NULL,
    DeletedDate DATETIME NULL,
    
    FOREIGN KEY (FolderID) REFERENCES t_Folder(FolderID),
    INDEX idx_folder (FolderID),
    INDEX idx_filename (FileName),
    INDEX idx_deleted (IsDeleted),
    INDEX idx_upload_date (UploadedDate)
);
```

#### s_FilePermission (File-level Permission Overrides)
```sql
CREATE TABLE s_FilePermission (
    FilePermissionID INT PRIMARY KEY IDENTITY(1,1),
    FileID INT NOT NULL,
    UserID INT NULL,
    PermissionType NVARCHAR(50) NOT NULL,
    PermissionMode INT NOT NULL,                    -- 1=Allow, 2=Deny
    
    CreatedBy NVARCHAR(255) NOT NULL,
    CreatedDate DATETIME NOT NULL,
    
    FOREIGN KEY (FileID) REFERENCES t_FileDocument(FileID) ON DELETE CASCADE,
    FOREIGN KEY (UserID) REFERENCES m_UserAccount(UserID),
    INDEX idx_file (FileID),
    INDEX idx_user (UserID),
    UNIQUE (FileID, UserID, PermissionType)
);
```

---

### 4. Sharing & Access

#### t_ExternalShare (External Share Links)
```sql
CREATE TABLE t_ExternalShare (
    ExternalShareID INT PRIMARY KEY IDENTITY(1,1),
    ShareToken NVARCHAR(100) NOT NULL UNIQUE,      -- Unique access token
    FileID INT NOT NULL,
    FolderID INT NULL,
    SharedByUserID INT NOT NULL,
    SharedDate DATETIME NOT NULL,
    ExpiryDate DATETIME NULL,
    MaxDownloads INT NULL,
    CurrentDownloads INT NOT NULL DEFAULT 0,
    IsActive BIT NOT NULL DEFAULT 1,
    IsPasswordProtected BIT NOT NULL DEFAULT 0,
    PasswordHash NVARCHAR(255) NULL,
    
    CreatedBy NVARCHAR(255) NOT NULL,
    CreatedDate DATETIME NOT NULL,
    
    FOREIGN KEY (FileID) REFERENCES t_FileDocument(FileID),
    FOREIGN KEY (FolderID) REFERENCES t_Folder(FolderID),
    FOREIGN KEY (SharedByUserID) REFERENCES m_UserAccount(UserID),
    INDEX idx_token (ShareToken),
    INDEX idx_file (FileID),
    INDEX idx_expiry (ExpiryDate),
    INDEX idx_active (IsActive)
);
```

#### s_ShareAccess (Share Delegation)
```sql
CREATE TABLE s_ShareAccess (
    ShareAccessID INT PRIMARY KEY IDENTITY(1,1),
    FileID INT NOT NULL,
    FolderID INT NULL,
    GrantedToUserID INT NOT NULL,                   -- User given access
    GrantedByUserID INT NOT NULL,                   -- User granting access
    PermissionType NVARCHAR(50) NOT NULL,          -- View, Download, Upload
    GrantedDate DATETIME NOT NULL,
    ExpiryDate DATETIME NULL,
    
    CreatedBy NVARCHAR(255) NOT NULL,
    CreatedDate DATETIME NOT NULL,
    
    FOREIGN KEY (FileID) REFERENCES t_FileDocument(FileID),
    FOREIGN KEY (FolderID) REFERENCES t_Folder(FolderID),
    FOREIGN KEY (GrantedToUserID) REFERENCES m_UserAccount(UserID),
    FOREIGN KEY (GrantedByUserID) REFERENCES m_UserAccount(UserID),
    INDEX idx_file (FileID),
    INDEX idx_user (GrantedToUserID),
    INDEX idx_expiry (ExpiryDate)
);
```

---

### 5. Approval Workflow

#### t_ApprovalRequest (Approval Tasks)
```sql
CREATE TABLE t_ApprovalRequest (
    ApprovalRequestID INT PRIMARY KEY IDENTITY(1,1),
    RequestType NVARCHAR(50) NOT NULL,              -- CreateHCFolder, DeleteFile, etc.
    EntityType NVARCHAR(50) NOT NULL,              -- Folder, File
    EntityID INT NOT NULL,                          -- FolderID or FileID
    RequestedByUserID INT NOT NULL,
    RequestedDate DATETIME NOT NULL,
    Status INT NOT NULL DEFAULT 0,                  -- 0=Pending, 1=Approved, 2=Rejected
    Remark NVARCHAR(MAX),                          -- Additional metadata
    
    FOREIGN KEY (RequestedByUserID) REFERENCES m_UserAccount(UserID),
    INDEX idx_status (Status),
    INDEX idx_type (RequestType),
    INDEX idx_requested_date (RequestedDate)
);
```

#### t_ApprovalStep (Workflow Steps)
```sql
CREATE TABLE t_ApprovalStep (
    ApprovalStepID INT PRIMARY KEY IDENTITY(1,1),
    ApprovalRequestID INT NOT NULL,
    StepSequence INT NOT NULL,                      -- 1, 2, 3, etc.
    ApproverUserID INT NOT NULL,
    ApprovalStatus INT NOT NULL DEFAULT 0,          -- 0=Pending, 1=Approved, 2=Rejected
    ApprovalDate DATETIME NULL,
    ApprovalRemarks NVARCHAR(MAX),
    
    FOREIGN KEY (ApprovalRequestID) REFERENCES t_ApprovalRequest(ApprovalRequestID),
    FOREIGN KEY (ApproverUserID) REFERENCES m_UserAccount(UserID),
    INDEX idx_request (ApprovalRequestID),
    INDEX idx_approver (ApproverUserID),
    UNIQUE (ApprovalRequestID, StepSequence)
);
```

---

### 6. Audit & Logs

#### l_AuditLog (Append-only Audit Trail)
```sql
CREATE TABLE l_AuditLog (
    AuditID INT PRIMARY KEY IDENTITY(1,1),
    TableName NVARCHAR(100) NOT NULL,              -- Table being modified
    RecordID INT,                                   -- ID in that table
    Action NVARCHAR(50) NOT NULL,                  -- INSERT, UPDATE, DELETE
    ActionBy NVARCHAR(255) NOT NULL,               -- ADUserName (snapshot)
    ActionDate DATETIME NOT NULL,
    IPAddress NVARCHAR(50),                        -- Client IP
    ModuleCode NVARCHAR(20),                       -- MOD-001, MOD-003, etc.
    Details NVARCHAR(MAX),
    OldValue NVARCHAR(MAX),                        -- For updates
    NewValue NVARCHAR(MAX),                        -- For updates
    
    INDEX idx_table (TableName),
    INDEX idx_action (Action),
    INDEX idx_actionby (ActionBy),
    INDEX idx_date (ActionDate)
);

-- SECURITY: Grant INSERT, SELECT only
GRANT INSERT, SELECT ON dbo.l_AuditLog TO [DMS_App_Role];
REVOKE UPDATE, DELETE ON dbo.l_AuditLog TO [DMS_App_Role];
```

#### l_LoginAttempt (Login Tracking)
```sql
CREATE TABLE l_LoginAttempt (
    LoginAttemptID INT PRIMARY KEY IDENTITY(1,1),
    ADUserName NVARCHAR(255) NOT NULL,
    Timestamp DATETIME NOT NULL,
    IPAddress NVARCHAR(50) NOT NULL,
    Success BIT NOT NULL,
    FailureReason NVARCHAR(255) NULL,              -- Invalid password, account locked, etc.
    
    INDEX idx_username (ADUserName),
    INDEX idx_timestamp (Timestamp),
    INDEX idx_ip (IPAddress)
);
```

#### l_ShareAccessLog (Share Activity Log)
```sql
CREATE TABLE l_ShareAccessLog (
    ShareAccessLogID INT PRIMARY KEY IDENTITY(1,1),
    ShareTokenID NVARCHAR(100),
    AccessedByIP NVARCHAR(50),
    AccessedDate DATETIME NOT NULL,
    ResourceType NVARCHAR(50),                      -- File, Folder
    ResourceID INT,
    ActionType NVARCHAR(50),                       -- Download, View, Upload
    Success BIT NOT NULL,
    
    INDEX idx_token (ShareTokenID),
    INDEX idx_date (AccessedDate)
);
```

---

### 7. Knowledge Base

#### t_KBCategory (KB Categories)
```sql
CREATE TABLE t_KBCategory (
    CategoryID INT PRIMARY KEY IDENTITY(1,1),
    CategoryName NVARCHAR(200) NOT NULL COLLATE Thai_CI_AS,
    CategoryCode NVARCHAR(50) NOT NULL UNIQUE,
    ParentCategoryID INT NULL,                      -- Hierarchical categories
    DisplayOrder INT NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    
    CreatedBy NVARCHAR(255) NOT NULL,
    CreatedDate DATETIME NOT NULL,
    
    FOREIGN KEY (ParentCategoryID) REFERENCES t_KBCategory(CategoryID),
    INDEX idx_parent (ParentCategoryID)
);
```

#### t_KBArticle (KB Articles)
```sql
CREATE TABLE t_KBArticle (
    ArticleID INT PRIMARY KEY IDENTITY(1,1),
    CategoryID INT NOT NULL,
    Title NVARCHAR(255) NOT NULL COLLATE Thai_CI_AS,
    Content NVARCHAR(MAX) NOT NULL,
    AuthorUserID INT NOT NULL,
    PublishedDate DATETIME,
    ViewCount INT NOT NULL DEFAULT 0,
    IsPublished BIT NOT NULL DEFAULT 0,
    
    CreatedBy NVARCHAR(255) NOT NULL,
    CreatedDate DATETIME NOT NULL,
    UpdatedBy NVARCHAR(255) NULL,
    UpdatedDate DATETIME NULL,
    
    FOREIGN KEY (CategoryID) REFERENCES t_KBCategory(CategoryID),
    FOREIGN KEY (AuthorUserID) REFERENCES m_UserAccount(UserID),
    INDEX idx_category (CategoryID),
    INDEX idx_published (IsPublished)
);
```

---

### 8. Configuration

#### c_SystemConfig (System Settings)
```sql
CREATE TABLE c_SystemConfig (
    ConfigID INT PRIMARY KEY IDENTITY(1,1),
    ConfigKey NVARCHAR(100) NOT NULL UNIQUE,
    ConfigValue NVARCHAR(MAX) NOT NULL,
    ConfigType NVARCHAR(50),                       -- String, Int, Bool, etc.
    Description NVARCHAR(500),
    IsEncrypted BIT NOT NULL DEFAULT 0,
    
    UpdatedBy NVARCHAR(255) NULL,
    UpdatedDate DATETIME NULL,
    
    INDEX idx_key (ConfigKey)
);
```

**Common Config Keys:**
- `NetworkDriveRoot` - UNC path to file storage
- `MaxFileSizeBytes` - Maximum upload size (default: 100 MB)
- `UploadQuotaPerDepartment` - Department quota (default: 40 GB)
- `SessionTimeout` - Session timeout in minutes (default: 30)
- `MaxAccountLockoutAttempts` - Failed login threshold (default: 5)
- `AccountLockoutDuration` - Lockout duration in minutes (default: 30)
- `AdminEmail` - Admin notification email
- `SmtpServer` - SMTP server address
- `VirusScanEnabled` - Enable virus scanning
- `MaxFolderDepth` - Maximum folder nesting (default: 10)

#### c_EmailTemplate (Email Templates)
```sql
CREATE TABLE c_EmailTemplate (
    TemplateID INT PRIMARY KEY IDENTITY(1,1),
    TemplateName NVARCHAR(100) NOT NULL UNIQUE,
    Subject NVARCHAR(255) NOT NULL,
    Body NVARCHAR(MAX) NOT NULL,
    IsActive BIT NOT NULL DEFAULT 1,
    
    CreatedBy NVARCHAR(255) NOT NULL,
    CreatedDate DATETIME NOT NULL
);
```

---

### 9. Reference Tables

#### r_UserRole (User Roles)
```sql
CREATE TABLE r_UserRole (
    RoleID INT PRIMARY KEY IDENTITY(1,1),
    RoleName NVARCHAR(100) NOT NULL UNIQUE,
    RoleCode NVARCHAR(20) NOT NULL,
    Description NVARCHAR(500),
    
    -- Standard values (immutable):
    -- 1 = Officer
    -- 2 = Senior Officer
    -- 3 = Manager
    -- 4 = Admin
    
    INDEX idx_code (RoleCode)
);
```

#### r_FileType (Allowed File Types)
```sql
CREATE TABLE r_FileType (
    FileTypeID INT PRIMARY KEY IDENTITY(1,1),
    Extension NVARCHAR(10) NOT NULL UNIQUE,        -- .pdf, .doc, .docx, etc.
    MimeType NVARCHAR(100),
    IsAllowed BIT NOT NULL DEFAULT 1,
    
    -- Whitelisted types (REQUIRED for upload):
    -- .pdf, .doc, .docx, .xls, .xlsx, .ppt, .pptx
    -- .jpg, .png, .gif, .zip
);
```

#### r_PermissionType (Permission Types)
```sql
CREATE TABLE r_PermissionType (
    PermissionTypeID INT PRIMARY KEY IDENTITY(1,1),
    PermissionName NVARCHAR(50) NOT NULL UNIQUE,
    
    -- Standard values:
    -- View, Edit, Upload, Delete, Share, Approve
);
```

---

## Key Design Decisions

### 1. Soft Delete Pattern

All deletable entities use `IsDeleted` flag:
```sql
-- Instead of DELETE, set flag
UPDATE t_FileDocument SET IsDeleted = 1, DeletedByADUser = @User WHERE FileID = @ID

-- All queries exclude deleted records
SELECT * FROM t_FileDocument WHERE IsDeleted = 0
```

**Benefits:**
- Audit compliance (complete deletion history)
- Data recovery possibility
- Referential integrity maintained

### 2. Audit Stamp (CreatedBy, UpdatedBy)

**CRITICAL:** Use ADUserName snapshot, NOT UserID
```sql
-- ✅ Correct
CreatedBy NVARCHAR(255) NOT NULL,        -- "domain\john.doe"
UpdatedBy NVARCHAR(255) NULL,

-- ❌ Wrong
CreatedByUserID INT NOT NULL,             -- Creates FK dependency
UpdatedByUserID INT NULL,
```

**Reason:** User account can be deleted; audit trail must remain intact.

### 3. Physical File Path Storage

Separate logical (FolderID) from physical storage:
```
Network Drive: \\bui-filecenter\Core Management
Physical Path: {OrgUnitID}\{ConfidentialityCode}\{FolderID:D8}
Example: \DEPT-001\3NC\00000042\0000000123_reportname.pdf
```

**Benefits:**
- Flexible file storage locations
- Migration possible without database changes
- Folder reorganization without file moves

### 4. Permission Hierarchy

```
File-level override → Folder-level permission → Inherited from parent
Explicit DENY (overrides ALLOW) → Explicit ALLOW → No permission (implicit DENY)
```

### 5. Collation for Thai

All Thai text columns use `Thai_CI_AS`:
```sql
FolderName NVARCHAR(255) NOT NULL COLLATE Thai_CI_AS
```

---

## Indexes & Performance

### Key Indexes

```sql
-- Folder hierarchy
CREATE INDEX idx_folder_parent ON t_Folder(ParentFolderID);
CREATE INDEX idx_folder_orgunit ON t_Folder(OrgUnitID);

-- File lookups
CREATE INDEX idx_file_folder ON t_FileDocument(FolderID);
CREATE INDEX idx_file_deleted ON t_FileDocument(IsDeleted);

-- Permission lookups (critical path)
CREATE INDEX idx_folder_perm_folder ON s_FolderPermission(FolderID);
CREATE INDEX idx_folder_perm_user ON s_FolderPermission(UserID);

-- Audit trail
CREATE INDEX idx_audit_date ON l_AuditLog(ActionDate);
CREATE INDEX idx_audit_table ON l_AuditLog(TableName, RecordID);

-- Share access
CREATE INDEX idx_share_token ON t_ExternalShare(ShareToken);
CREATE INDEX idx_share_expiry ON t_ExternalShare(ExpiryDate);
```

---

## Access Control & Security

### Role-Based Access

```sql
-- Stored procedures enforce security
-- All data access through SP, not direct SQL

-- Grant only EXECUTE to application role
GRANT EXECUTE ON storage.sp_GetFilesByFolder TO [DMS_App_Role];
GRANT EXECUTE ON storage.sp_InsertAuditLog TO [DMS_App_Role];

-- Revoke direct table access
REVOKE SELECT, INSERT, UPDATE, DELETE ON t_FileDocument TO [DMS_App_Role];
```

### Audit Log Protection

```sql
-- Append-only, no modifications
GRANT INSERT, SELECT ON dbo.l_AuditLog TO [DMS_App_Role];
REVOKE UPDATE, DELETE ON dbo.l_AuditLog TO [DMS_App_Role];
```

---

## Data Retention & Archival

### Soft Deleted Records

Keep soft-deleted records for compliance (minimum 7 years recommended):
```sql
-- Archive query
SELECT * FROM t_FileDocument 
WHERE IsDeleted = 1 
AND DeletedDate < DATEADD(YEAR, -7, GETDATE())
```

### Audit Log Retention

Immutable append-only logs retained indefinitely:
```sql
-- Monthly archives (optional)
SELECT * INTO l_AuditLog_2025_04 FROM l_AuditLog 
WHERE YEAR(ActionDate) = 2025 AND MONTH(ActionDate) = 4
```

---

## Backup & Recovery

### Critical Tables (daily full backup)
- m_UserAccount
- t_Folder
- t_FileDocument
- s_FolderPermission
- t_ApprovalRequest
- l_AuditLog

### Configuration (included in backup)
- c_SystemConfig
- c_EmailTemplate

---

## Migration & Deployment

### Schema Deployment Order
1. Reference tables (r_*)
2. Master tables (m_*)
3. Configuration tables (c_*, s_*)
4. Transaction tables (t_*)
5. Log tables (l_*)
6. Stored procedures
7. Indexes

### Rollback Strategy
- Keep previous schema versions
- Test migrations in staging first
- Document all schema changes with purpose

