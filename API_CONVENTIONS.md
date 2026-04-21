# API Conventions & Controller Guidelines

## Overview

This document defines API conventions for Controllers, DTOs, and service interfaces in the CMS project. Controllers act as orchestrators between the UI and business logic.

---

## 1. Controller Structure & Naming

### Naming Convention

```csharp
// ✅ Correct - suffixed with "Controller"
public class FileUploadController { }
public class FolderCommandController { }
public class ApprovalInboxController { }
public class PermissionAclController { }

// Naming patterns:
// - Command controllers: Handle create/update/delete
// - Query controllers: Handle read operations
// - Inbox controllers: Handle user task lists
// - Dialog controllers: Handle modal interactions
```

### Controller Organization

```csharp
public sealed class FileUploadController : IFileUploadController
{
    // 1. Interface definition
    // 2. Private dependencies (readonly)
    // 3. Constructor (validate dependencies)
    // 4. Public methods (implementing interface)
    // 5. Private helper methods
    
    private readonly IFileService _fileService;
    private readonly IFolderService _folderService;
    private readonly ILogger _logger;

    public FileUploadController(
        IFileService fileService = null,
        IFolderService folderService = null,
        ILogger logger = null)
    {
        _fileService = fileService;
        _folderService = folderService;
        _logger = logger;
    }

    // Public API
    public void UploadFiles(
        IFileUploadView view,
        IList<FileUploadRequest> files,
        int folderId,
        UserContext user)
    {
        // Null check and delegate to Presenter
        if (_fileService == null || view == null || user == null)
            return;

        var presenter = new FilePresenter(_fileService, _folderService, view, user);
        presenter.UploadFiles(files, folderId);
    }
}
```

---

## 2. Controller Interfaces

### Interface Definition Patterns

```csharp
namespace CMS.Controllers.DMS.Document.Interfaces
{
    // ✅ Correct - clear, focused interface
    public interface IFileUploadController
    {
        /// <summary>
        /// Uploads multiple files to a folder.
        /// </summary>
        /// <param name="view">View/form implementing IFileUploadView</param>
        /// <param name="files">Files to upload</param>
        /// <param name="folderId">Target folder ID</param>
        /// <param name="user">Current user context</param>
        void UploadFiles(IFileUploadView view, IList<FileUploadRequest> files,
            int folderId, UserContext user);

        /// <summary>
        /// Downloads a file to disk.
        /// </summary>
        /// <param name="view">View for status messages</param>
        /// <param name="fileDocumentId">File to download</param>
        /// <param name="user">Current user context</param>
        /// <param name="getSavePath">Function to get save location from user</param>
        void DownloadFile(IFileUploadView view, int fileDocumentId, UserContext user,
            Func<string, string> getSavePath);

        /// <summary>
        /// Finds duplicate file names in a folder.
        /// </summary>
        /// <returns>List of file names that already exist</returns>
        IEnumerable<string> FindDuplicateNames(IEnumerable<string> fileNames,
            int folderId, UserContext user);
    }

    // View interface (callback from presenter to UI)
    public interface IFileUploadView
    {
        void DisplaySuccessMessage(string message);
        void DisplayErrorMessage(string message);
        void DisplayFileList(IEnumerable<FileDocument> files);
        void DisplayUploadProgress(int currentFile, int totalFiles);
    }
}
```

---

## 3. Method Signatures & Patterns

### Single Responsibility

```csharp
// ✅ Correct - each method does one thing
public interface IFileUploadController
{
    void UploadFiles(IFileUploadView view, IList<FileUploadRequest> files,
        int folderId, UserContext user);

    void DownloadFile(IFileUploadView view, int fileDocumentId,
        UserContext user, Func<string, string> getSavePath);

    IEnumerable<string> FindDuplicateNames(IEnumerable<string> fileNames,
        int folderId, UserContext user);
}

// ❌ Wrong - too many responsibilities
public interface IFileUploadController
{
    void ProcessFiles(IFileUploadView view, FileOperationType operationType,
        IList<FileUploadRequest> files, int folderId, UserContext user,
        Func<string, string> getSavePath);
}
```

### Parameter Requirements

```csharp
// ✅ Correct - all required parameters
public void UploadFiles(
    IFileUploadView view,          // For UI callback
    IList<FileUploadRequest> files, // Data to process
    int folderId,                   // Context
    UserContext user)               // For authorization
{
}

// ❌ Wrong - missing context
public void UploadFiles(
    IList<FileUploadRequest> files)
{
    // How to validate user permission? Unknown user?
}
```

### View Parameter Pattern

Controllers receive `View` as first parameter for status callbacks:

```csharp
public interface IApprovalInboxController
{
    /// View must be first parameter
    void LoadApprovalTasks(IApprovalInboxView view, UserContext user);
    void ApproveRequest(IApprovalInboxView view, int approvalRequestId,
        UserContext user, string remarks);
    void RejectRequest(IApprovalInboxView view, int approvalRequestId,
        UserContext user, string rejectionReason);
}
```

---

## 4. Null Safety & Defensive Coding

### Parameter Validation

```csharp
// ✅ Correct - defensive null checks
public sealed class FileUploadController : IFileUploadController
{
    public void UploadFiles(IFileUploadView view, IList<FileUploadRequest> files,
        int folderId, UserContext user)
    {
        // Guard clauses - return early if preconditions not met
        if (_fileService == null || _folderService == null)
            return;

        if (view == null || files == null || user == null)
            return;

        // Delegate to presenter
        var presenter = new FilePresenter(_fileService, _folderService, view, user);
        presenter.UploadFiles(files, folderId);
    }
}

// ❌ Wrong - assumes dependencies exist
public void UploadFiles(IFileUploadView view, IList<FileUploadRequest> files,
    int folderId, UserContext user)
{
    // Will throw NullReferenceException if _fileService is null
    var results = _fileService.UploadFiles(files, folderId, user);
}
```

### Graceful Degradation

```csharp
// ✅ Correct - optional dependencies handled gracefully
public sealed class FileUploadController : IFileUploadController
{
    private readonly IFileService _fileService;
    private readonly ILogger _logger;  // Optional

    public FileUploadController(IFileService fileService, ILogger logger = null)
    {
        _fileService = fileService ?? throw new ArgumentNullException(nameof(fileService));
        _logger = logger;  // Can be null
    }

    public void UploadFiles(IFileUploadView view, ...)
    {
        if (_fileService == null || view == null)
            return;

        try
        {
            _fileService.UploadFiles(files, folderId, user);
        }
        catch (Exception ex)
        {
            _logger?.Error($"Upload failed: {ex.Message}");  // Null-safe logging
            view.DisplayErrorMessage("Upload failed");
        }
    }
}
```

---

## 5. Error Handling in Controllers

### Exception Translation

```csharp
// Controllers don't throw exceptions; they handle them and notify view

public sealed class ApprovalInboxController : IApprovalInboxController
{
    public void ApproveRequest(IApprovalInboxView view, int approvalRequestId,
        UserContext user, string remarks)
    {
        if (view == null || user == null)
            return;

        try
        {
            // Service can throw exceptions
            _approvalService.ApproveRequest(approvalRequestId, user, remarks);
            
            view.DisplaySuccessMessage("Request approved successfully");
        }
        catch (InvalidOperationException ex)
        {
            _logger?.Warn($"Cannot approve: {ex.Message}");
            view.DisplayErrorMessage("Cannot approve this request");
        }
        catch (UnauthorizedAccessException ex)
        {
            _logger?.Warn($"Unauthorized: {ex.Message}");
            view.DisplayErrorMessage("You do not have permission to approve");
        }
        catch (CmsBusinessException ex)
        {
            _logger?.Error($"[{ex.ErrorCode}] {ex.Message}");
            view.DisplayErrorMessage($"Operation failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            _logger?.Error($"Unexpected error: {ex.Message}", ex);
            view.DisplayErrorMessage("An unexpected error occurred");
        }
    }
}
```

---

## 6. Data Transfer Objects (DTOs)

### Request DTOs

```csharp
namespace CMS.Services.DTOs
{
    /// <summary>Request DTO for file upload operations</summary>
    public class FileUploadRequest
    {
        /// <summary>Original file name</summary>
        public string FileName { get; set; }

        /// <summary>Binary file content</summary>
        public byte[] Content { get; set; }

        /// <summary>File size in bytes</summary>
        public long SizeBytes { get; set; }

        /// <summary>MIME type (optional)</summary>
        public string ContentType { get; set; }
    }

    /// <summary>Request DTO for file move operation</summary>
    public class MoveFileRequest
    {
        public int FileID { get; set; }
        public int SourceFolderID { get; set; }
        public int DestinationFolderID { get; set; }
    }

    /// <summary>Request DTO for folder creation</summary>
    public class CreateFolderRequest
    {
        public string FolderName { get; set; }
        public int? ParentFolderID { get; set; }
        public int ConfidentialityLevel { get; set; }
        public int? TemplateID { get; set; }
    }
}
```

### Response DTOs

```csharp
namespace CMS.Services.DTOs
{
    /// <summary>Response DTO for individual file upload result</summary>
    public class UploadResult
    {
        /// <summary>Newly created file ID</summary>
        public int FileID { get; set; }

        /// <summary>File name (as stored)</summary>
        public string FileName { get; set; }

        /// <summary>Upload success indicator</summary>
        public bool Success { get; set; }

        /// <summary>Error message if upload failed</summary>
        public string ErrorMessage { get; set; }

        /// <summary>File size in bytes</summary>
        public long FileSizeBytes { get; set; }

        /// <summary>Upload timestamp</summary>
        public DateTime UploadedDate { get; set; }
    }

    /// <summary>Response DTO for file download operation</summary>
    public class FileDownloadResult
    {
        public bool Success { get; set; }
        public string FileName { get; set; }
        public byte[] FileContent { get; set; }
        public long FileSizeBytes { get; set; }
        public string ErrorMessage { get; set; }
    }

    /// <summary>Response DTO for storage usage report</summary>
    public class StorageUsageReport
    {
        public int OrgUnitId { get; set; }
        public long UsedBytes { get; set; }
        public long QuotaBytes { get; set; }
        public decimal PercentageUsed { get; set; }
        public long RemainingBytes { get; set; }
    }

    /// <summary>Response DTO for AD authentication result</summary>
    public class AdAuthResult
    {
        public bool Success { get; set; }
        public string ADUserName { get; set; }
        public EmployeeApiModels.Employee EmployeeInfo { get; set; }
        public string ErrorMessage { get; set; }
    }
}
```

### DTO Validation Rules

```csharp
// ✅ Correct - validate at service boundary
public class FileUploadRequest
{
    public string FileName { get; set; }
    public byte[] Content { get; set; }
    public long SizeBytes { get; set; }

    // Validation happens in service, not DTO
    // Services validate: FileName not null/empty, SizeBytes within limits, etc.
}

// ❌ Wrong - putting validation logic in DTO
public class FileUploadRequest
{
    private string _fileName;
    public string FileName
    {
        get { return _fileName; }
        set
        {
            if (string.IsNullOrEmpty(value))
                throw new ArgumentException("FileName required");
            _fileName = value;
        }
    }
}
```

---

## 7. UserContext Pattern

### UserContext Structure

```csharp
public class UserContext
{
    /// <summary>Database user ID</summary>
    public int UserID { get; set; }

    /// <summary>Active Directory user name (domain\username)</summary>
    public string ADUserName { get; set; }

    /// <summary>Display name from HR system</summary>
    public string DisplayName { get; set; }

    /// <summary>User's organization unit ID</summary>
    public int OrgUnitID { get; set; }

    /// <summary>User's role (Officer, Manager, Admin, etc.)</summary>
    public int AppRole { get; set; }

    /// <summary>Current session ID</summary>
    public string SessionID { get; set; }

    /// <summary>Client IP address</summary>
    public string ClientIP { get; set; }

    /// <summary>Session start time</summary>
    public DateTime LoginTime { get; set; }

    /// <summary>Last activity time (for timeout detection)</summary>
    public DateTime LastActivityTime { get; set; }

    /// <summary>Is user account still active</summary>
    public bool IsActive { get; set; }
}
```

### UserContext Usage

```csharp
// ✅ Correct - always pass UserContext to services/controllers
public void UploadFile(FileUploadRequest request, UserContext user)
{
    // Validate user
    if (user == null || user.UserID <= 0)
        throw new UnauthorizedAccessException("Invalid user context");

    // Use user for authorization
    if (!_permissionService.CanUploadToFolder(user.UserID, folderId))
        throw new UnauthorizedAccessException("No upload permission");

    // Use ADUserName for audit
    _auditService.LogFileUpload(user.UserID, fileId, user.ADUserName);
}

// ❌ Wrong - creating user context from request
public void UploadFile(FileUploadRequest request)
{
    // Where does user info come from? How is it validated?
    var user = new UserContext { ADUserName = request.UploadedBy };
}
```

---

## 8. Presenter Pattern Implementation

### Presenter Role

Presenters handle complex multi-step operations and coordinate between service layer and UI:

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

    /// <summary>
    /// Orchestrates multi-step upload process:
    /// 1. Validate folder exists and is accessible
    /// 2. Check for duplicate file names
    /// 3. Upload files with progress tracking
    /// 4. Handle errors and update UI
    /// </summary>
    public void UploadFiles(IList<FileUploadRequest> files, int folderId)
    {
        try
        {
            // Step 1: Validate folder
            var folder = _folderService.GetFolderById(folderId, _user);
            if (folder == null)
            {
                _view.DisplayErrorMessage($"Folder {folderId} not found");
                return;
            }

            // Step 2: Check duplicates
            var fileNames = files.Select(f => f.FileName).ToList();
            var duplicates = _fileService.FindDuplicateNames(fileNames, folderId, _user);
            if (duplicates.Any())
            {
                _view.DisplayErrorMessage(
                    $"Files already exist: {string.Join(", ", duplicates)}");
                return;
            }

            // Step 3: Upload with progress
            var results = _fileService.UploadFiles(files, folderId, _user);

            // Step 4: Display results
            int successCount = 0;
            foreach (var result in results)
            {
                if (result.Success)
                {
                    successCount++;
                    _view.DisplaySuccessMessage($"Uploaded: {result.FileName}");
                }
                else
                {
                    _view.DisplayErrorMessage(
                        $"Failed: {result.FileName} - {result.ErrorMessage}");
                }
            }

            // Final summary
            _view.DisplaySuccessMessage(
                $"Upload complete: {successCount}/{files.Count} files uploaded");
        }
        catch (UnauthorizedAccessException ex)
        {
            _view.DisplayErrorMessage("You do not have permission to upload");
        }
        catch (InvalidOperationException ex)
        {
            _view.DisplayErrorMessage($"Operation failed: {ex.Message}");
        }
        catch (Exception ex)
        {
            _view.DisplayErrorMessage("An unexpected error occurred");
        }
    }
}
```

---

## 9. Collections & List Operations

### Return Types

```csharp
// ✅ Correct - explicit collection types
public interface IFileService
{
    /// Returns list of files (could be empty)
    IEnumerable<FileDocument> GetFilesInFolder(int folderId, UserContext user);

    /// Returns list of upload results
    IEnumerable<UploadResult> UploadFiles(IEnumerable<FileUploadRequest> files,
        int folderId, UserContext user);
}

// ✅ Acceptable - List<T> if modification expected
public class FilePresenter
{
    public void ProcessFiles(List<FileUploadRequest> files)
    {
        files.RemoveAll(f => string.IsNullOrEmpty(f.FileName));
        // Process remaining files
    }
}

// ❌ Wrong - vague return type
public object GetFiles() { }
public dynamic GetUploadResults() { }
```

### Null Safety with Collections

```csharp
// ✅ Correct - never return null, return empty collection
public IEnumerable<FileDocument> GetFilesInFolder(int folderId, UserContext user)
{
    if (folderId <= 0)
        return new List<FileDocument>();  // Empty, not null

    var files = _repository.GetFilesByFolderId(folderId);
    return files ?? new List<FileDocument>();  // Safe null coalescing
}

// ❌ Wrong - returning null
public IEnumerable<FileDocument> GetFilesInFolder(int folderId, UserContext user)
{
    if (folderId <= 0)
        return null;  // Caller must check for null!

    return _repository.GetFilesByFolderId(folderId);  // Could be null
}
```

---

## 10. Async/Await (if used)

### Service Interface with Async

```csharp
public interface ILoginAuthService
{
    /// <summary>Authenticate user against Active Directory asynchronously</summary>
    Task<AdAuthResult> LoginAsync(string username, string password);
}

public interface IFileService
{
    /// <summary>Asynchronous file download (large files)</summary>
    Task<FileDownloadResult> DownloadFileAsync(int fileId, UserContext user);
}
```

### Controller Pattern with Async

```csharp
// ✅ Correct - async pattern if services support it
public sealed class LoginAuthController
{
    private readonly ILoginAuthService _authService;

    public async void LoginAsync(string username, string password, 
        ILoginView view, UserContext user)
    {
        try
        {
            var result = await _authService.LoginAsync(username, password);
            
            if (result.Success)
                view.DisplaySuccessMessage("Login successful");
            else
                view.DisplayErrorMessage("Invalid credentials");
        }
        catch (Exception ex)
        {
            view.DisplayErrorMessage("Login failed");
        }
    }
}
```

---

## 11. API Documentation

### XML Comments for Public API

```csharp
/// <summary>
/// Uploads multiple files to a folder with validation and virus scanning.
/// </summary>
/// <remarks>
/// This is a complex operation involving:
/// - Folder accessibility check
/// - Duplicate file name detection
/// - File size validation
/// - File type whitelist validation
/// - Virus scanning (if enabled)
/// - Audit logging
///
/// The operation is atomic - all files succeed or all fail.
/// </remarks>
/// <param name="view">View for displaying upload progress and results</param>
/// <param name="files">
/// List of files to upload. Each file must include FileName, Content, and SizeBytes.
/// </param>
/// <param name="folderId">
/// ID of target folder. User must have upload permission.
/// </param>
/// <param name="user">
/// Current user context for permission validation and audit logging.
/// </param>
/// <exception cref="ArgumentNullException">
/// Thrown when files, view, or user is null.
/// </exception>
/// <exception cref="InvalidOperationException">
/// Thrown when folder doesn't exist or operation cannot complete.
/// </exception>
/// <exception cref="UnauthorizedAccessException">
/// Thrown when user lacks upload permission for the folder.
/// </exception>
/// <example>
/// <code>
/// var controller = new FileUploadController(fileService, folderService);
/// var files = new List<FileUploadRequest>
/// {
///     new FileUploadRequest { FileName = "report.pdf", SizeBytes = 1024000, Content = data }
/// };
/// controller.UploadFiles(view, files, folderId, userContext);
/// </code>
/// </example>
public void UploadFiles(IFileUploadView view, IList<FileUploadRequest> files,
    int folderId, UserContext user)
{
}
```

---

## 12. Controller Checklist

Before implementing a controller, ensure:

- [ ] **Interface defined** - `IControllerController` interface created
- [ ] **Null safety** - All parameters validated with guard clauses
- [ ] **Error handling** - Try-catch around service calls
- [ ] **View callbacks** - Results communicated to view, not returned
- [ ] **UserContext** - Passed to all service calls
- [ ] **DTOs used** - No domain models passed to/from service
- [ ] **Logging** - Errors logged appropriately
- [ ] **Presenter pattern** - Complex operations use Presenter
- [ ] **XML docs** - All public members documented
- [ ] **No business logic** - All logic delegated to services
- [ ] **Dependency injection** - Constructor injection used
- [ ] **Async if needed** - Long operations use async/await

---

## Example: Complete Controller

```csharp
namespace CMS.Controllers.DMS.Document
{
    /// <summary>
    /// Handles approval workflow operations.
    /// Delegates to ApprovalService for business logic.
    /// </summary>
    public sealed class ApprovalInboxController : IApprovalInboxController
    {
        private readonly IApprovalService _approvalService;
        private readonly ILogger _logger;

        /// <summary>
        /// Initializes controller with dependencies.
        /// All parameters are validated in service constructors.
        /// </summary>
        public ApprovalInboxController(
            IApprovalService approvalService = null,
            ILogger logger = null)
        {
            _approvalService = approvalService;
            _logger = logger;
        }

        /// <summary>
        /// Loads pending approval tasks for current user.
        /// </summary>
        public void LoadPendingApprovals(IApprovalInboxView view, UserContext user)
        {
            // Null checks - guard clauses
            if (_approvalService == null || view == null || user == null)
                return;

            try
            {
                // Delegate to service
                var approvals = _approvalService.GetPendingApprovals(user.UserID);

                // Display results
                view.DisplayApprovals(approvals);
            }
            catch (Exception ex)
            {
                _logger?.Error($"Error loading approvals: {ex.Message}");
                view.DisplayErrorMessage("Failed to load approval tasks");
            }
        }

        /// <summary>
        /// Approves an approval request.
        /// Handles complex multi-step approval workflow.
        /// </summary>
        public void ApproveRequest(IApprovalInboxView view, int approvalRequestId,
            UserContext user, string remarks)
        {
            // Guard clauses
            if (_approvalService == null || view == null || user == null)
                return;

            try
            {
                // Service validates permission and state
                _approvalService.ApproveRequest(approvalRequestId, user, remarks);

                view.DisplaySuccessMessage("Request approved");

                // Reload list after successful operation
                LoadPendingApprovals(view, user);
            }
            catch (UnauthorizedAccessException ex)
            {
                _logger?.Warn($"Unauthorized approval: {ex.Message}");
                view.DisplayErrorMessage("You do not have approval authority");
            }
            catch (InvalidOperationException ex)
            {
                _logger?.Warn($"Cannot approve: {ex.Message}");
                view.DisplayErrorMessage($"Cannot approve: {ex.Message}");
            }
            catch (Exception ex)
            {
                _logger?.Error($"Approval failed: {ex.Message}");
                view.DisplayErrorMessage("Approval operation failed");
            }
        }
    }
}
```

