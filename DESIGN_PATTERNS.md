# Design Patterns & Architectural Patterns

## Overview

This CMS project uses a comprehensive set of design patterns organized across multiple architectural layers. These patterns ensure code maintainability, testability, and separation of concerns.

---

## 1. Repository Pattern

### Purpose
Abstracts data access logic and provides a clean API for data operations.

### Implementation

**Repository Interface:**
```csharp
namespace CMS.Data.Interfaces
{
    public interface IFolderRepository
    {
        int Insert(Folder folder);
        void Update(Folder folder);
        void Delete(int folderId);
        Folder GetById(int folderId);
        IEnumerable<Folder> GetByParentId(int? parentId);
    }
}
```

**Concrete Implementation:**
```csharp
public class FolderRepository : IFolderRepository
{
    private readonly string _connectionString;

    public FolderRepository(IConfigManager configManager)
    {
        _connectionString = configManager.Get<string>("ConnectionString");
    }

    public int Insert(Folder folder)
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand("storage.sp_InsertFolder", conn))
            {
                cmd.CommandType = CommandType.StoredProcedure;
                cmd.Parameters.AddWithValue("@FolderName", folder.FolderName);
                cmd.Parameters.AddWithValue("@OrgUnitID", folder.OrgUnitID);
                // ... additional parameters
                return Convert.ToInt32(cmd.ExecuteScalar());
            }
        }
    }
}
```

### Key Files
- `CMS.Data/Repositories/*.cs` - Repository implementations
- `CMS.Data/Interfaces/*.cs` - Repository interfaces

### Usage Rules

✅ **Always:**
- Use repository interfaces for dependency injection
- Use parameterized queries to prevent SQL injection
- Encapsulate all database operations

❌ **Never:**
- Access database directly from services (bypass repository)
- Use string concatenation for SQL queries
- Pass raw SQL from controllers

---

## 2. Service Layer Pattern

### Purpose
Encapsulates business logic and coordinates repositories and other services.

### Implementation

**Service Interface:**
```csharp
public interface IFileService
{
    IEnumerable<UploadResult> UploadFiles(IEnumerable<FileUploadRequest> files, 
        int folderId, UserContext user);
    FileDownloadResult DownloadFile(int fileId, UserContext user);
    void DeleteFile(int fileId, UserContext user);
    IEnumerable<FileDocument> GetFilesInFolder(int folderId, UserContext user);
    StorageUsageReport GetStorageReport(int orgUnitId, UserContext user);
}
```

**Service Implementation:**
```csharp
public class FileService : IFileService
{
    private const long MaxFileSizeBytes = 100L * 1024 * 1024;
    
    private static readonly HashSet<string> AllowedExtensions = 
        new HashSet<string>(StringComparer.OrdinalIgnoreCase)
        {
            ".pdf", ".doc", ".docx", ".xls", ".xlsx", ".ppt", ".pptx", 
            ".jpg", ".png", ".gif", ".zip"
        };

    private readonly IFileDocumentRepository _fileDocumentRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly IPermissionService _permissionService;
    private readonly IAuditService _auditService;
    private readonly IVirusScanService _virusScanService;

    public FileService(
        IFileDocumentRepository fileDocumentRepository,
        IFolderRepository folderRepository,
        IPermissionService permissionService,
        IAuditService auditService,
        IVirusScanService virusScanService)
    {
        _fileDocumentRepository = fileDocumentRepository ?? 
            throw new ArgumentNullException("fileDocumentRepository");
        _folderRepository = folderRepository ?? 
            throw new ArgumentNullException("folderRepository");
        // ... validate other dependencies
    }

    public IEnumerable<UploadResult> UploadFiles(
        IEnumerable<FileUploadRequest> files, 
        int folderId, 
        UserContext user)
    {
        var results = new List<UploadResult>();
        
        // Business Logic: Validate folder accessibility
        var folder = _folderRepository.GetById(folderId);
        if (folder == null)
            throw new InvalidOperationException($"Folder {folderId} not found");

        // Business Logic: Check user permissions
        if (!_permissionService.CanUploadToFolder(user, folderId))
            throw new UnauthorizedAccessException("Cannot upload to this folder");

        // Business Logic: File processing
        foreach (var file in files)
        {
            // Validate file size
            if (file.SizeBytes > MaxFileSizeBytes)
                throw new ArgumentException($"File exceeds {MaxFileSizeBytes} bytes limit");

            // Validate file type
            var ext = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(ext))
                throw new ArgumentException($"File type {ext} not allowed");

            // Business Logic: Virus scan
            if (!_virusScanService.IsSafe(file.Content))
                throw new InvalidOperationException("File failed virus scan");

            // Persist to database
            var fileDoc = new FileDocument { /* ... */ };
            _fileDocumentRepository.Insert(fileDoc);

            // Audit
            _auditService.LogFileUpload(user.UserID, fileDoc.FileID);

            results.Add(new UploadResult { FileID = fileDoc.FileID, Success = true });
        }

        return results;
    }
}
```

### Key Services (25+)

| Service | Responsibility |
|---------|-----------------|
| `FileService` | File upload, download, versioning, storage management |
| `FolderService` | Folder operations, hierarchy validation, templates |
| `ApprovalService` | Workflow state management, approval routing |
| `PermissionService` | Permission calculations, effective permission resolution |
| `ShareAccessService` | Share delegation and access management |
| `ShareService` | Share link generation and management |
| `AuditService` | Audit log recording (append-only) |
| `AdAuthService` | Active Directory authentication |
| `OrgStructureService` | Org unit operations |
| `KBArticleService` | Knowledge base articles |
| `MenuService` | Application menu management |

### Usage Rules

✅ **Always:**
- Implement IService interfaces
- Inject dependencies through constructor
- Validate business rules early
- Throw meaningful exceptions
- Use audit services for sensitive operations

❌ **Never:**
- Access repositories directly from controllers/UI
- Put validation logic in controllers
- Create services without dependency injection

---

## 3. Dependency Injection Pattern

### Purpose
Manages object creation and dependency resolution, improving testability and flexibility.

### Implementation

**Constructor Injection:**
```csharp
public class FileService : IFileService
{
    private readonly IFileDocumentRepository _fileDocumentRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly IPermissionService _permissionService;

    public FileService(
        IFileDocumentRepository fileDocumentRepository,
        IFolderRepository folderRepository,
        IPermissionService permissionService)
    {
        _fileDocumentRepository = fileDocumentRepository ?? 
            throw new ArgumentNullException(nameof(fileDocumentRepository));
        _folderRepository = folderRepository ?? 
            throw new ArgumentNullException(nameof(folderRepository));
        _permissionService = permissionService ?? 
            throw new ArgumentNullException(nameof(permissionService));
    }
}
```

**Manual Service Creation (in Program.cs):**
```csharp
// Infrastructure
var configManager = new ConfigManager();
var adHelper = new ActiveDirectoryHelper();
var loggingService = new LoggingService();

// Repositories
var folderRepository = new FolderRepository(configManager);
var fileDocumentRepository = new FileDocumentRepository(configManager);
var permissionRepository = new PermissionRepository(configManager);

// Services
var permissionService = new PermissionService(permissionRepository);
var auditService = new AuditService(auditRepository);
var fileService = new FileService(
    fileDocumentRepository,
    folderRepository,
    permissionService,
    auditService,
    virusScanService);

// Controllers
var fileUploadController = new FileUploadController(fileService, folderService);

// Presentation
var loginForm = new LoginForm(loginAuthController, loginAccountController);
```

### Usage Rules

✅ **Always:**
- Use constructor injection
- Validate null dependencies
- Use interfaces for all injected types

❌ **Never:**
- Create services with `new` inside classes
- Use service locators
- Pass dependencies through properties

---

## 4. Controller/Orchestrator Pattern

### Purpose
Acts as an intermediary between UI and services, orchestrating complex operations.

### Implementation

**Controller Interface:**
```csharp
public interface IFileUploadController
{
    void UploadFiles(IFileUploadView view, IList<FileUploadRequest> files, 
        int folderId, UserContext user);
    void DownloadFile(IFileUploadView view, int fileDocumentId, 
        UserContext user, Func<string, string> getSavePath);
    IEnumerable<string> FindDuplicateNames(IEnumerable<string> fileNames, 
        int folderId, UserContext user);
}
```

**Controller Implementation:**
```csharp
public sealed class FileUploadController : IFileUploadController
{
    private readonly IFileService _fileService;
    private readonly IFolderService _folderService;

    public FileUploadController(
        IFileService fileService = null, 
        IFolderService folderService = null)
    {
        _fileService = fileService;
        _folderService = folderService;
    }

    public void UploadFiles(IFileUploadView view, IList<FileUploadRequest> files, 
        int folderId, UserContext user)
    {
        if (_fileService == null || _folderService == null || view == null || user == null)
            return;

        // Orchestrate complex operation using Presenter
        var presenter = new FilePresenter(_fileService, _folderService, view, user);
        presenter.UploadFiles(files, folderId);
    }

    public IEnumerable<string> FindDuplicateNames(IEnumerable<string> fileNames, 
        int folderId, UserContext user)
    {
        if (_fileService == null || user == null) 
            return new List<string>();
        
        return _fileService.FindDuplicateNames(fileNames, folderId, user);
    }
}
```

### Key Controllers
- `FileUploadController` - File operations
- `FolderCommandController` - Folder operations
- `ApprovalInboxController` - Approval workflows
- `AccessShareController` - Share management
- `AuthorityConfigController` - Authority configuration

### Usage Rules

✅ **Always:**
- Use interfaces for dependency injection
- Create Presenter for complex multi-step operations
- Validate inputs before passing to services
- Handle null dependencies gracefully

❌ **Never:**
- Implement business logic in controllers
- Access repositories directly
- Throw exceptions without handling context
- Mix UI logic with orchestration

---

## 5. Presenter / MVP Pattern

### Purpose
Handles complex presentation logic and coordinates between view and services for multi-step operations.

### Implementation

**View Interface:**
```csharp
public interface IFileUploadView
{
    void DisplayFileList(IEnumerable<FileDocument> files);
    void DisplayUploadProgress(int currentFile, int totalFiles);
    void DisplaySuccessMessage(string message);
    void DisplayErrorMessage(string message);
}
```

**Presenter:**
```csharp
public class FilePresenter
{
    private readonly IFileService _fileService;
    private readonly IFolderService _folderService;
    private readonly IFileUploadView _view;
    private readonly UserContext _user;

    public FilePresenter(
        IFileService fileService,
        IFolderService folderService,
        IFileUploadView view,
        UserContext user)
    {
        _fileService = fileService;
        _folderService = folderService;
        _view = view;
        _user = user;
    }

    public void UploadFiles(IList<FileUploadRequest> files, int folderId)
    {
        try
        {
            // Check folder exists
            var folder = _folderService.GetFolderById(folderId, _user);
            if (folder == null)
            {
                _view.DisplayErrorMessage($"Folder {folderId} not found");
                return;
            }

            // Check for duplicates
            var fileNames = files.Select(f => f.FileName).ToList();
            var duplicates = _fileService.FindDuplicateNames(fileNames, folderId, _user);
            if (duplicates.Any())
            {
                _view.DisplayErrorMessage($"Duplicate files: {string.Join(", ", duplicates)}");
                return;
            }

            // Upload files with progress tracking
            var results = _fileService.UploadFiles(files, folderId, _user);

            // Update view with results
            foreach (var result in results)
            {
                if (result.Success)
                    _view.DisplaySuccessMessage($"Uploaded: {result.FileName}");
                else
                    _view.DisplayErrorMessage($"Failed: {result.FileName} - {result.ErrorMessage}");
            }
        }
        catch (Exception ex)
        {
            _view.DisplayErrorMessage($"Upload failed: {ex.Message}");
        }
    }

    public void DownloadFile(int fileDocumentId, Func<string, string> getSavePath)
    {
        try
        {
            var result = _fileService.DownloadFile(fileDocumentId, _user);
            if (!result.Success)
            {
                _view.DisplayErrorMessage(result.ErrorMessage);
                return;
            }

            // Get save path from UI
            string savePath = getSavePath(result.FileName);
            File.WriteAllBytes(savePath, result.FileContent);

            _view.DisplaySuccessMessage($"File downloaded to {savePath}");
        }
        catch (Exception ex)
        {
            _view.DisplayErrorMessage($"Download failed: {ex.Message}");
        }
    }
}
```

**Form Implementation (View):**
```csharp
public partial class FileListForm : Form, IFileUploadView
{
    private readonly IFileUploadController _controller;
    private readonly UserContext _user;

    public FileListForm(IFileUploadController controller, UserContext user)
    {
        _controller = controller;
        _user = user;
        InitializeComponent();
    }

    public void DisplayFileList(IEnumerable<FileDocument> files)
    {
        listViewFiles.Items.Clear();
        foreach (var file in files)
        {
            var item = new ListViewItem(file.FileName);
            item.SubItems.Add(file.FileSizeBytes.ToString());
            listViewFiles.Items.Add(item);
        }
    }

    public void DisplaySuccessMessage(string message)
    {
        MessageBox.Show(message, "Success", MessageBoxButtons.OK, 
            MessageBoxIcon.Information);
    }

    public void DisplayErrorMessage(string message)
    {
        MessageBox.Show(message, "Error", MessageBoxButtons.OK, 
            MessageBoxIcon.Error);
    }

    // Event handlers delegate to controller
    private void btnUpload_Click(object sender, EventArgs e)
    {
        var selectedFiles = GetSelectedFiles();
        int folderId = GetCurrentFolderId();
        
        _controller.UploadFiles(this, selectedFiles, folderId, _user);
    }
}
```

### Presenter Classes
- `FilePresenter` - File upload/download operations
- `AuditLogPresenter` - Audit log display
- `ConfigAdminPresenter` - Configuration management
- `FolderPresenter` - Folder operations

### Usage Rules

✅ **Always:**
- Use Presenter for complex multi-step operations
- Keep view interface simple and focused
- Implement error handling in presenter
- Use dependency injection for services

❌ **Never:**
- Put business logic in view/form
- Access services directly from form
- Mix presentation and business logic

---

## 6. Data Transfer Object (DTO) Pattern

### Purpose
Safely transfer data between layers without exposing domain models.

### Implementation

**Request DTOs:**
```csharp
public class FileUploadRequest
{
    public string FileName { get; set; }
    public byte[] Content { get; set; }
    public long SizeBytes { get; set; }
}

public class MoveFileRequest
{
    public int FileID { get; set; }
    public int SourceFolderID { get; set; }
    public int DestinationFolderID { get; set; }
}
```

**Response DTOs:**
```csharp
public class UploadResult
{
    public int FileID { get; set; }
    public string FileName { get; set; }
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
}

public class FileDownloadResult
{
    public bool Success { get; set; }
    public string FileName { get; set; }
    public byte[] FileContent { get; set; }
    public string ErrorMessage { get; set; }
}

public class StorageUsageReport
{
    public int OrgUnitId { get; set; }
    public long UsedBytes { get; set; }
    public long QuotaBytes { get; set; }
    public decimal PercentageUsed { get; set; }
}
```

### Key DTOs
Located in `CMS.Services/DTOs/`
- `FileUploadRequest`, `UploadResult`
- `MoveFileRequest`, `CopyFileRequest`
- `FileDownloadResult`
- `StorageUsageReport`
- `AdAuthResult`
- `ApprovalWorkflowRequest`
- `ShareAccessRequest`

### Usage Rules

✅ **Always:**
- Use DTOs for service method parameters and returns
- Map domain models to DTOs at service boundaries
- Keep DTOs simple (properties only, no logic)

❌ **Never:**
- Return domain models from services
- Put business logic in DTOs
- Share DTOs between layers (create new ones per boundary)

---

## 7. Business Rule Validation Pattern

### Purpose
Ensures business logic is enforced consistently across the application.

### Implementation

**Validation in Service Constructor:**
```csharp
public FileService(
    IFileDocumentRepository fileDocumentRepository,
    IFolderRepository folderRepository,
    IPermissionService permissionService)
{
    if (fileDocumentRepository == null) 
        throw new ArgumentNullException(nameof(fileDocumentRepository));
    if (folderRepository == null) 
        throw new ArgumentNullException(nameof(folderRepository));
    if (permissionService == null) 
        throw new ArgumentNullException(nameof(permissionService));

    _fileDocumentRepository = fileDocumentRepository;
    _folderRepository = folderRepository;
    _permissionService = permissionService;
}
```

**Validation in Service Methods:**
```csharp
public class FolderService : IFolderService
{
    private const int MaxFolderDepth = 10;
    private static readonly char[] InvalidFolderNameChars = 
        { '\\', '/', ':', '*', '?', '"', '<', '>', '|' };

    public Folder CreateSubFolder(Folder parentFolder, string folderName, 
        UserContext user, int confidentialityLevel)
    {
        // BR-001: Validate folder name
        if (string.IsNullOrWhiteSpace(folderName))
            throw new ArgumentException("Folder name is required.");

        ValidateFolderName(folderName); // Check for invalid characters

        // BR-002: Check max depth
        int currentDepth = parentFolder.FolderDepth ?? 0;
        if (currentDepth >= MaxFolderDepth)
            throw new InvalidOperationException(
                $"Maximum folder depth ({MaxFolderDepth}) exceeded");

        // BR-003: Validate confidentiality level
        if (!IsValidConfidentialityLevel(confidentialityLevel))
            throw new ArgumentException("Invalid confidentiality level");

        // BR-004: Check permission
        if (!_permissionService.CanCreateFolder(user, parentFolder.FolderID))
            throw new UnauthorizedAccessException(
                "User does not have permission to create folder");

        // Business Logic: Create folder
        var newFolder = new Folder
        {
            ParentFolderID = parentFolder.FolderID,
            FolderName = folderName,
            ConfidentialityLevel = confidentialityLevel,
            FolderDepth = (parentFolder.FolderDepth ?? 0) + 1,
            CreatedBy = user.ADUserName,
            CreatedDate = DateTime.Now
        };

        return _folderRepository.Insert(newFolder);
    }

    private static void ValidateFolderName(string name)
    {
        var found = name.Where(c => InvalidFolderNameChars.Contains(c))
            .Distinct()
            .ToArray();
        
        if (found.Length > 0)
            throw new ArgumentException(
                $"Folder name contains invalid characters: {string.Join(", ", found)}");
    }
}
```

**Business Rules Enforced:**
- BR-001-06: Every department must have 4 main root folders
- BR-002-02: Maximum folder depth is 10 levels
- BR-002-07: Sub-folder cannot have lower confidentiality than parent
- BR-003-01: Maximum file size is 100 MB
- BR-003-02: Only whitelisted file types allowed
- BR-004-04: Explicit deny overrides allow
- BR-004-08: 1HC folders cannot be shared externally

### Usage Rules

✅ **Always:**
- Validate business rules in services, not controllers
- Throw specific exceptions with clear messages
- Use business rule constants
- Check permissions before operations

❌ **Never:**
- Skip validation for "admin" users
- Implement same validation in multiple places
- Validate only at UI level

---

## 8. Exception Handling Pattern

### Purpose
Provide meaningful error information while maintaining security and usability.

### Implementation

**Custom Business Exception:**
```csharp
public class CmsBusinessException : Exception
{
    public string ErrorCode { get; }
    
    public CmsBusinessException(string errorCode, string message)
        : base(message)
    {
        ErrorCode = errorCode;
    }
}
```

**Exception Throwing:**
```csharp
public class FileService : IFileService
{
    public IEnumerable<UploadResult> UploadFiles(
        IEnumerable<FileUploadRequest> files, 
        int folderId, 
        UserContext user)
    {
        var folder = _folderRepository.GetById(folderId);
        
        if (folder == null)
            throw new InvalidOperationException($"Folder {folderId} not found");

        if (!_permissionService.CanUploadToFolder(user, folderId))
            throw new UnauthorizedAccessException(
                "User does not have upload permission for this folder");

        foreach (var file in files)
        {
            if (file.SizeBytes > MaxFileSizeBytes)
                throw new CmsBusinessException("FILE-001",
                    $"File size {file.SizeBytes} exceeds limit {MaxFileSizeBytes}");

            var ext = Path.GetExtension(file.FileName);
            if (!AllowedExtensions.Contains(ext))
                throw new CmsBusinessException("FILE-002",
                    $"File type '{ext}' is not allowed. " +
                    $"Allowed types: {string.Join(", ", AllowedExtensions)}");
        }
        
        // ... rest of logic
    }
}
```

**Exception Handling in UI:**
```csharp
private async void btnLogin_Click(object sender, EventArgs e)
{
    try
    {
        string username = txtUsername.Text.Trim();
        string password = txtPassword.Text;

        AdAuthResult result = await _loginAuthController.LoginAsync(username, password);

        if (result.Success)
        {
            AuthResult = result;
            DialogResult = DialogResult.OK;
        }
        else
        {
            lblStatus.Text = "Username or password is incorrect";
            txtPassword.Clear();
            txtPassword.Focus();
        }
    }
    catch (UnauthorizedAccessException ex)
    {
        lblStatus.Text = $"Access denied: {ex.Message}";
        Logger.Warn($"Login unauthorized: {ex.Message}");
    }
    catch (CmsBusinessException ex)
    {
        lblStatus.Text = $"Operation failed [{ex.ErrorCode}]: {ex.Message}";
        Logger.Error($"[{ex.ErrorCode}] {ex.Message}");
    }
    catch (Exception ex)
    {
        lblStatus.Text = "Server connection error. Please try again.";
        Logger.Error($"Login error: {ex.Message}");
    }
}
```

### Exception Types Used
- `ArgumentException` - Invalid parameters
- `ArgumentNullException` - Null dependency injection
- `InvalidOperationException` - Invalid state/operation
- `UnauthorizedAccessException` - Permission denied
- `CmsBusinessException` - Business rule violation

### Usage Rules

✅ **Always:**
- Throw exceptions early for validation failures
- Use specific exception types
- Include helpful error messages
- Log exceptions appropriately

❌ **Never:**
- Swallow exceptions silently (bare catch {})
- Include sensitive data in error messages
- Throw generic Exception class

---

## 9. Audit Trail Pattern

### Purpose
Maintain immutable record of all significant operations for compliance and debugging.

### Implementation

**Audit Service:**
```csharp
public interface IAuditService
{
    void LogFileUpload(int userId, int fileId, string fileName, 
        long fileSizeBytes, string adUserName);
    void LogFileDelete(int userId, int fileId, string deletionReason);
    void LogPermissionChange(int userId, int folderId, int granteeId, 
        string permissionType, string changeDescription);
    void LogApprovalAction(int userId, int approvalRequestId, 
        string action, string remarks);
}

public class AuditService : IAuditService
{
    private readonly IAuditRepository _auditRepository;

    public void LogFileUpload(int userId, int fileId, string fileName, 
        long fileSizeBytes, string adUserName)
    {
        var auditLog = new AuditLog
        {
            TableName = "t_FileDocument",
            RecordID = fileId,
            Action = "FILE_UPLOAD",
            ActionBy = adUserName, // Use ADUserName, not UserID
            ActionDate = DateTime.Now,
            IPAddress = GetClientIP(),
            ModuleCode = "MOD-003",
            Details = $"File: {fileName}, Size: {fileSizeBytes} bytes"
        };

        _auditRepository.Insert(auditLog);
    }
}
```

**Audit Log Model:**
```csharp
public class AuditLog
{
    public int AuditID { get; set; }
    public string TableName { get; set; }      // Table being modified
    public int? RecordID { get; set; }          // Record ID in table
    public string Action { get; set; }          // INSERT, UPDATE, DELETE, etc.
    public string ActionBy { get; set; }        // ADUserName (snapshot)
    public DateTime ActionDate { get; set; }
    public string IPAddress { get; set; }       // Client IP
    public string ModuleCode { get; set; }      // Module performing action
    public string Details { get; set; }         // Additional details
    public string OldValue { get; set; }        // Previous value (for updates)
    public string NewValue { get; set; }        // New value (for updates)
    public bool IsDeleted { get; set; }         // Soft delete flag
}
```

**Database Configuration:**
```sql
-- Audit table: Append-only
CREATE TABLE l_AuditLog (
    AuditID INT PRIMARY KEY IDENTITY(1,1),
    TableName NVARCHAR(100) NOT NULL,
    RecordID INT,
    Action NVARCHAR(50) NOT NULL,
    ActionBy NVARCHAR(255) NOT NULL,
    ActionDate DATETIME NOT NULL,
    IPAddress NVARCHAR(50),
    ModuleCode NVARCHAR(20),
    Details NVARCHAR(MAX),
    OldValue NVARCHAR(MAX),
    NewValue NVARCHAR(MAX)
);

-- Permissions: INSERT, SELECT only (no UPDATE, DELETE)
GRANT INSERT, SELECT ON dbo.l_AuditLog TO [DMS_App_Role];
REVOKE UPDATE, DELETE ON dbo.l_AuditLog TO [DMS_App_Role];
```

### Audit Rules
- **Append-only:** No updates or deletes
- **Username snapshot:** Store ADUserName, not UserID
- **Module tracking:** Record which module performed action
- **IP logging:** Track client IP for access patterns
- **Soft delete:** Use `IsDeleted` flag, never hard-delete

### Usage Rules

✅ **Always:**
- Log sensitive operations (upload, delete, permission changes)
- Use ADUserName for audit stamps
- Include enough detail to recreate action
- Call audit service after successful operation

❌ **Never:**
- Hard-delete audit records
- Use UserID (FK) in audit stamps
- Log to console instead of database
- Skip audit for "system" operations

---

## 10. Soft Delete Pattern

### Purpose
Maintain data integrity while allowing logical deletion without hard database deletes.

### Implementation

**Domain Model:**
```csharp
public class FileDocument
{
    public int FileID { get; set; }
    public string FileName { get; set; }
    public bool IsDeleted { get; set; }
    public string DeletedByADUser { get; set; }
    public DateTime? DeletedDate { get; set; }
    // ... other properties
}
```

**Service Delete Logic:**
```csharp
public void DeleteFile(int fileId, UserContext user)
{
    var file = _fileDocumentRepository.GetById(fileId);
    
    if (file == null)
        throw new InvalidOperationException($"File {fileId} not found");

    if (!_permissionService.CanDeleteFile(user, fileId))
        throw new UnauthorizedAccessException("Cannot delete this file");

    // Soft delete
    file.IsDeleted = true;
    file.DeletedByADUser = user.ADUserName;
    file.DeletedDate = DateTime.Now;

    _fileDocumentRepository.Update(file);

    // Audit
    _auditService.LogFileDelete(user.UserID, fileId, 
        "File deleted by user action");
}
```

**Repository Query:**
```csharp
public IEnumerable<FileDocument> GetFilesInFolder(int folderId)
{
    using (var conn = new SqlConnection(_connectionString))
    {
        conn.Open();
        using (var cmd = new SqlCommand(
            "SELECT * FROM t_FileDocument WHERE FolderID = @FolderID AND IsDeleted = 0",
            conn))
        {
            cmd.Parameters.AddWithValue("@FolderID", folderId);
            // Execute and return only non-deleted files
        }
    }
}
```

### Usage Rules

✅ **Always:**
- Use `IsDeleted` flag instead of hard delete
- Record deletion timestamp and user
- Exclude soft-deleted items from queries
- Use soft delete for audit compliance

❌ **Never:**
- Hard-delete data (DELETE statements)
- Forget to set deletion timestamp/user
- Query without checking IsDeleted flag

---

## 11. Permission/Authorization Pattern

### Purpose
Enforce access control at service layer before data access.

### Implementation

**Permission Check Pattern:**
```csharp
public class FileService : IFileService
{
    public void DeleteFile(int fileId, UserContext user)
    {
        // 1. Check user exists and is active
        var userAccount = _userAccountRepository.GetById(user.UserID);
        if (userAccount == null || userAccount.IsInactive)
            throw new UnauthorizedAccessException("User account is invalid");

        // 2. Check file exists
        var file = _fileDocumentRepository.GetById(fileId);
        if (file == null)
            throw new InvalidOperationException("File not found");

        // 3. Check explicit permission
        if (!_permissionService.HasPermission(user.UserID, fileId, 
            PermissionType.Delete))
            throw new UnauthorizedAccessException(
                "User does not have delete permission");

        // 4. Perform operation if authorized
        PerformDelete(file, user);
    }
}

public class PermissionService : IPermissionService
{
    public bool HasPermission(int userId, int fileId, PermissionType permissionType)
    {
        // 1. Check explicit deny (overrides allow)
        var explicitDeny = _permissionRepository.GetDenyPermission(userId, fileId);
        if (explicitDeny != null && explicitDeny.PermissionType == permissionType)
            return false;

        // 2. Check explicit allow
        var allow = _permissionRepository.GetAllowPermission(userId, fileId);
        if (allow != null && allow.PermissionType == permissionType)
            return true;

        // 3. Check inherited from folder
        var folder = _folderRepository.GetById(fileId);
        if (folder != null && folder.InheritPermission)
        {
            return HasFolderPermission(userId, folder.FolderID, permissionType);
        }

        return false;
    }
}
```

### Permission Rules
- **Explicit deny overrides allow** (BR-004-04)
- **Inheritance:** Sub-folders inherit parent permissions
- **User context:** Always validate UserContext.UserID
- **Permission type:** Validate against specific operations

### Usage Rules

✅ **Always:**
- Check permission before every operation
- Validate user context early
- Use explicit deny pattern
- Log permission denials for audit

❌ **Never:**
- Skip permission checks
- Allow operations based on role alone (use explicit permissions)
- Return partial data (deny entire operation if unauthorized)

---

## 12. Singleton Pattern (Configuration)

### Purpose
Provide centralized, application-wide configuration access.

### Implementation

**Config Manager (Likely Implementation):**
```csharp
public interface IConfigManager
{
    T Get<T>(string key);
    void Set<T>(string key, T value);
}

public class ConfigManager : IConfigManager
{
    private static readonly Lazy<ConfigManager> _instance =
        new Lazy<ConfigManager>(() => new ConfigManager());

    public static ConfigManager Instance => _instance.Value;

    private readonly Dictionary<string, object> _config;

    public ConfigManager()
    {
        _config = LoadFromAppConfig();
    }

    public T Get<T>(string key)
    {
        if (_config.TryGetValue(key, out var value))
            return (T)value;

        throw new KeyNotFoundException($"Configuration key '{key}' not found");
    }

    private Dictionary<string, object> LoadFromAppConfig()
    {
        return new Dictionary<string, object>
        {
            ["ConnectionString"] = 
                ConfigurationManager.ConnectionStrings["DefaultConnection"].ConnectionString,
            ["NetworkDriveRoot"] = 
                ConfigurationManager.AppSettings["NetworkDriveRoot"],
            ["MaxFileSizeBytes"] = 
                long.Parse(ConfigurationManager.AppSettings["MaxFileSizeBytes"]),
            ["UploadQuotaPerDepartment"] = 
                long.Parse(ConfigurationManager.AppSettings["UploadQuotaPerDepartment"])
        };
    }
}
```

### Usage

```csharp
var configManager = new ConfigManager();
var connectionString = configManager.Get<string>("ConnectionString");
var maxFileSize = configManager.Get<long>("MaxFileSizeBytes");
```

---

## Summary: Pattern Implementation Guide

| Pattern | Layer | Key Classes | When to Use |
|---------|-------|------------|-----------|
| **Repository** | Data | `*Repository.cs`, `IRepository` | All data access |
| **Service** | Business | `*Service.cs`, `IService` | All business logic |
| **Dependency Injection** | All | Constructor parameters | Manage dependencies |
| **Controller/Orchestrator** | Controller | `*Controller.cs` | Coordinate complex ops |
| **Presenter/MVP** | Controller | `*Presenter.cs` | Complex UI flows |
| **DTO** | Services | `*Request.cs`, `*Result.cs` | Data transfer |
| **Business Rule Validation** | Services | Service methods | Enforce business rules |
| **Exception Handling** | All | Custom exceptions | Error handling |
| **Audit Trail** | Services | `AuditService` | Track changes |
| **Soft Delete** | All | `IsDeleted` flag | Data retention |
| **Permission Check** | Services | `PermissionService` | Authorization |
| **Singleton** | Infrastructure | `ConfigManager` | Shared resources |

---

## Best Practices

1. **Always use interfaces** for dependency injection
2. **Validate early** in service constructors and methods
3. **Use meaningful exception** types and messages
4. **Maintain audit trail** for compliance
5. **Enforce permissions** at service layer
6. **Keep controllers thin** - delegate to services
7. **Keep views simple** - use presenters for complex logic
8. **Map DTOs** at layer boundaries
9. **Never skip** business rule validation
10. **Document** complex business logic with comments

