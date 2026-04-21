# Coding Standards & Conventions

## 📋 Overview

This document defines coding standards, naming conventions, and best practices for the CMS project. All developers **MUST** follow these standards to maintain code quality and consistency.

---

## 1. Naming Conventions

### Classes & Methods (PascalCase)

```csharp
// ✅ Correct
public class FileService { }
public class IFileRepository { }
public void CreateSubFolder() { }
public int CalculateStorageUsage() { }

// ❌ Wrong
public class file_service { }
public class fileService { }
public void create_sub_folder() { }
```

### Local Variables & Parameters (camelCase)

```csharp
// ✅ Correct
public void ProcessFiles(int folderId, string fileName)
{
    var fileList = GetFiles();
    var maximumSize = 100 * 1024 * 1024;
    var isValid = ValidateFile();
}

// ❌ Wrong
public void ProcessFiles(int FolderId, string FileName)
{
    var FileList = GetFiles();
    var MAXIMUM_SIZE = 100 * 1024 * 1024;
}
```

### Constants (ALL_CAPS)

```csharp
// ✅ Correct
public const int MAX_FILE_SIZE_MB = 100;
public const int MAX_FOLDER_DEPTH = 10;
public const string DEFAULT_ENCODING = "UTF-8";
private const int BUFFER_SIZE = 4096;

// ❌ Wrong
public const int maxFileSize = 100;
public const int max_file_size = 100;
public const int MaxFileSize = 100;
```

### Private Fields (_camelCase)

```csharp
// ✅ Correct
private readonly IFileRepository _fileRepository;
private string _temporaryBuffer;
private int _retryCount = 0;

// ❌ Wrong
private readonly IFileRepository fileRepository;
private readonly IFileRepository m_fileRepository;
private string TemporaryBuffer;
```

### Properties (PascalCase)

```csharp
// ✅ Correct
public string FolderName { get; set; }
public int FileID { get; set; }
public bool IsDeleted { get; set; }
public DateTime CreatedDate { get; set; }

// ❌ Wrong
public string folderName { get; set; }
public string _folderName { get; set; }
```

### Database Naming (Prefix + PascalCase)

```sql
-- Master tables
m_OrgUnit
m_UserAccount
m_Department

-- Setup/Configuration tables
s_FolderPermission
s_WorkflowDefinition
s_SystemConfig

-- Transaction tables
t_FileDocument
t_TaskItem
t_ApprovalRequest

-- Log tables
l_AuditLog
l_ShareAccessLog
l_LoginAttempt

-- Reference tables
r_FileType
r_Status
r_PermissionType

-- Configuration tables
c_SystemConfig
c_EmailTemplate
```

### Interfaces (I + PascalCase)

```csharp
// ✅ Correct
public interface IFileService { }
public interface IFolderRepository { }
public interface IPermissionValidator { }

// ❌ Wrong
public interface FileService { }
public interface IFile_Service { }
public interface FileServiceInterface { }
```

### Enums (PascalCase)

```csharp
// ✅ Correct
public enum UserRole
{
    Officer    = 1,
    SrOfficer  = 2,
    Manager    = 3,
    Admin      = 4
}

public enum ConfidentialityLevel
{
    HC1_HighConfidential   = 1,
    SC2_SemiConfidential   = 2,
    NC3_NormalConfidential = 3,
    PC4_PublicConfidential = 4
}

// ❌ Wrong
public enum userRole { }
public enum USER_ROLE { }
```

---

## 2. Class Organization

### Member Order

```csharp
public class FileService : IFileService
{
    // 1. Constants
    private const int MAX_FILE_SIZE = 100;
    
    // 2. Static fields
    private static readonly string[] ValidExtensions = { ".pdf", ".doc" };
    
    // 3. Private fields
    private readonly IFileRepository _fileRepository;
    private readonly IPermissionService _permissionService;
    
    // 4. Properties
    public int TotalFilesProcessed { get; private set; }
    
    // 5. Constructor
    public FileService(IFileRepository fileRepository, 
        IPermissionService permissionService)
    {
        _fileRepository = fileRepository ?? 
            throw new ArgumentNullException(nameof(fileRepository));
        _permissionService = permissionService ?? 
            throw new ArgumentNullException(nameof(permissionService));
    }
    
    // 6. Public methods (alphabetical or by logical grouping)
    public void DeleteFile(int fileId) { }
    public void DownloadFile(int fileId) { }
    public void UploadFile(FileUploadRequest request) { }
    
    // 7. Private/helper methods
    private bool ValidateFileSize(long sizeBytes) { }
    private void LogOperation(string operation) { }
}
```

### Regions (Optional but recommended for large classes)

```csharp
public class ComplexService
{
    #region Constants & Fields
    private const int MAX_RETRIES = 3;
    private readonly IRepository _repository;
    #endregion

    #region Constructors
    public ComplexService(IRepository repository) { }
    #endregion

    #region Public Methods
    public void DoSomething() { }
    public void DoSomethingElse() { }
    #endregion

    #region Private Methods
    private void ValidateInput() { }
    private void LogError() { }
    #endregion
}
```

---

## 3. Code Style & Formatting

### Brace Style (Allman for classes, K&R for methods)

```csharp
// ✅ Correct - Allman for classes
public class FileService
{
    public void UploadFile()
    {
        if (isValid)
        {
            ProcessFile();
        }
    }
}

// ❌ Wrong - mixing styles
public class FileService {
    public void UploadFile() {
        if (isValid) { ProcessFile(); }
    }
}
```

### Indentation (4 spaces, NOT tabs)

```csharp
// ✅ Correct
public class FileService
{
    public void ProcessFiles()
    {
        foreach (var file in files)
        {
            if (IsValid(file))
            {
                Save(file);
            }
        }
    }
}
```

### Line Length (Max 120 characters)

```csharp
// ✅ Correct - break long lines
var result = _fileService.UploadFile(
    fileId, 
    fileName, 
    fileContent, 
    userContext);

// ❌ Wrong - too long
var result = _fileService.UploadFile(fileId, fileName, fileContent, userContext);
```

### Method Parameters

```csharp
// ✅ Correct - clear parameter names
public void UploadFile(
    int folderId,
    string fileName,
    byte[] fileContent,
    UserContext userContext)
{
}

// ❌ Wrong - unclear names
public void UploadFile(int f, string n, byte[] c, UserContext u)
{
}
```

### LINQ Formatting

```csharp
// ✅ Correct
var activeFiles = files
    .Where(f => !f.IsDeleted)
    .OrderBy(f => f.CreatedDate)
    .Select(f => new FileDto { ID = f.FileID, Name = f.FileName })
    .ToList();

// ❌ Wrong
var activeFiles = files.Where(f => !f.IsDeleted).OrderBy(f => f.CreatedDate).Select(f => new FileDto { ID = f.FileID, Name = f.FileName }).ToList();
```

---

## 4. ADO.NET & SQL Requirements

### Parameterized Queries (MANDATORY)

```csharp
// ✅ Correct - parameterized query
public List<FileDocument> GetFiles(int folderId)
{
    using (var conn = new SqlConnection(_connectionString))
    {
        conn.Open();
        using (var cmd = new SqlCommand(
            "SELECT * FROM t_FileDocument WHERE FolderID = @FolderID AND IsDeleted = 0",
            conn))
        {
            cmd.Parameters.AddWithValue("@FolderID", folderId);
            
            var files = new List<FileDocument>();
            using (var reader = cmd.ExecuteReader())
            {
                while (reader.Read())
                {
                    files.Add(new FileDocument
                    {
                        FileID = (int)reader["FileID"],
                        FileName = (string)reader["FileName"],
                        CreatedDate = (DateTime)reader["CreatedDate"]
                    });
                }
            }
            return files;
        }
    }
}

// ❌ CRITICAL: Never use string concatenation (SQL INJECTION!)
public List<FileDocument> GetFiles(int folderId)
{
    using (var conn = new SqlConnection(_connectionString))
    {
        conn.Open();
        // ❌ WRONG - SQL INJECTION VULNERABILITY!
        using (var cmd = new SqlCommand(
            $"SELECT * FROM t_FileDocument WHERE FolderID = {folderId}",
            conn))
        {
            // ... rest of code
        }
    }
}
```

### Stored Procedure Calls

```csharp
// ✅ Correct
using (var cmd = new SqlCommand("storage.sp_InsertFile", conn))
{
    cmd.CommandType = CommandType.StoredProcedure;
    cmd.Parameters.AddWithValue("@FileName", fileName);
    cmd.Parameters.AddWithValue("@FolderID", folderId);
    cmd.Parameters.AddWithValue("@CreatedBy", adUserName);
    
    int fileId = (int)cmd.ExecuteScalar();
    return fileId;
}
```

### NULL Handling

```csharp
// ✅ Correct
cmd.Parameters.AddWithValue("@ParentFolderID", 
    (object)folder.ParentFolderID ?? DBNull.Value);

cmd.Parameters.AddWithValue("@TemplateID", 
    (object)folder.TemplateID ?? DBNull.Value);

// ❌ Wrong - will cause issues with NULL
cmd.Parameters.AddWithValue("@ParentFolderID", folder.ParentFolderID);
```

---

## 5. Audit Stamp Requirements (MANDATORY)

**Every record created or modified MUST have audit stamps:**

### Domain Model

```csharp
public class FileDocument
{
    public int FileID { get; set; }
    public string FileName { get; set; }
    
    // ✅ Audit stamps - REQUIRED
    public string CreatedBy { get; set; }        // ADUserName (domain\username)
    public DateTime CreatedDate { get; set; }
    public string UpdatedBy { get; set; }        // ADUserName
    public DateTime? UpdatedDate { get; set; }
    
    // ✅ Soft delete stamps
    public bool IsDeleted { get; set; }
    public string DeletedByADUser { get; set; }
    public DateTime? DeletedDate { get; set; }
}
```

### Service Usage

```csharp
// ✅ Correct - use SessionContext for ADUserName
public void CreateFile(FileDocument file, UserContext user)
{
    // NEVER use UserID - use ADUserName snapshot
    file.CreatedBy = user.ADUserName;      // "domain\\john.doe"
    file.CreatedDate = DateTime.Now;
    
    _fileRepository.Insert(file);
    
    // Audit
    _auditService.LogFileCreated(
        user.UserID,
        file.FileID,
        file.FileName,
        user.ADUserName);  // Pass ADUserName
}

public void UpdateFile(FileDocument file, UserContext user)
{
    file.UpdatedBy = user.ADUserName;
    file.UpdatedDate = DateTime.Now;
    
    _fileRepository.Update(file);
}

public void DeleteFile(int fileId, UserContext user)
{
    var file = _fileRepository.GetById(fileId);
    
    // Soft delete
    file.IsDeleted = true;
    file.DeletedByADUser = user.ADUserName;
    file.DeletedDate = DateTime.Now;
    
    _fileRepository.Update(file);
}
```

### Database Schema

```sql
CREATE TABLE t_FileDocument (
    FileID INT PRIMARY KEY IDENTITY(1,1),
    FileName NVARCHAR(255) NOT NULL,
    
    -- Audit stamps
    CreatedBy NVARCHAR(255) NOT NULL,        -- ADUserName snapshot
    CreatedDate DATETIME NOT NULL,
    UpdatedBy NVARCHAR(255) NULL,
    UpdatedDate DATETIME NULL,
    
    -- Soft delete
    IsDeleted BIT NOT NULL DEFAULT 0,
    DeletedByADUser NVARCHAR(255) NULL,
    DeletedDate DATETIME NULL
);
```

---

## 6. Exception Handling

### Throwing Exceptions

```csharp
// ✅ Correct - specific exception types with clear messages
public void CreateFolder(string folderName, int parentFolderId)
{
    // Validate input
    if (string.IsNullOrWhiteSpace(folderName))
        throw new ArgumentException("Folder name is required.", nameof(folderName));

    // Check business rules
    if (folderName.Length > 255)
        throw new ArgumentException("Folder name cannot exceed 255 characters.", 
            nameof(folderName));

    // Validate state
    var parentFolder = _folderRepository.GetById(parentFolderId);
    if (parentFolder == null)
        throw new InvalidOperationException(
            $"Parent folder {parentFolderId} not found.");

    // Check permissions
    if (!_permissionService.CanCreateFolder(_currentUser, parentFolderId))
        throw new UnauthorizedAccessException(
            "User does not have permission to create folders here.");

    // Business rule violations
    if (parentFolder.FolderDepth >= 10)
        throw new InvalidOperationException(
            "Maximum folder depth (10) exceeded. Cannot create sub-folder.");
}

// ❌ Wrong - generic, unclear messages
public void CreateFolder(string folderName, int parentFolderId)
{
    throw new Exception("Error");  // ❌ Too generic
    throw new Exception($"folder {folderName}");  // ❌ Unclear
    throw new ApplicationException("failed");  // ❌ Too vague
}
```

### Catching Exceptions

```csharp
// ✅ Correct - specific catch blocks
try
{
    _fileService.DeleteFile(fileId, userContext);
}
catch (ArgumentException ex)
{
    _logger.Warn($"Invalid argument: {ex.Message}");
    DisplayUserMessage("Invalid file ID provided.");
}
catch (UnauthorizedAccessException ex)
{
    _logger.Warn($"Access denied: {ex.Message}");
    DisplayUserMessage("You do not have permission to delete this file.");
}
catch (InvalidOperationException ex)
{
    _logger.Error($"Operation failed: {ex.Message}");
    DisplayUserMessage("Cannot delete file. Please try again later.");
}
catch (Exception ex)
{
    _logger.Error($"Unexpected error: {ex.Message}", ex);
    DisplayUserMessage("An unexpected error occurred. Contact support.");
}

// ❌ Wrong - generic catch blocks
try { }
catch { }  // ❌ Swallows all exceptions silently

try { }
catch (Exception) { }  // ❌ Catches everything without logging
```

---

## 7. Comments & Documentation

### XML Comments (for public members)

```csharp
/// <summary>
/// Uploads files to the specified folder.
/// </summary>
/// <param name="files">Collection of files to upload.</param>
/// <param name="folderId">Target folder ID.</param>
/// <param name="user">Current user context for permission validation.</param>
/// <returns>Collection of upload results with success/error status for each file.</returns>
/// <exception cref="ArgumentNullException">Thrown when files or user is null.</exception>
/// <exception cref="InvalidOperationException">Thrown when folder does not exist.</exception>
/// <exception cref="UnauthorizedAccessException">Thrown when user lacks upload permission.</exception>
public IEnumerable<UploadResult> UploadFiles(
    IEnumerable<FileUploadRequest> files,
    int folderId,
    UserContext user)
{
    // Implementation
}

/// <summary>Gets the effective permission for a user on a resource.</summary>
/// <remarks>
/// Permission resolution follows this order:
/// 1. Explicit deny (overrides all)
/// 2. Explicit allow
/// 3. Inherited from parent folder
/// 4. No permission (default deny)
/// </remarks>
public PermissionType GetEffectivePermission(int userId, int resourceId)
{
    // Implementation
}
```

### Inline Comments (for complex logic)

```csharp
// ✅ Correct - explain WHY, not WHAT
public void ProcessFolder(Folder folder)
{
    // We must check for cycles in folder hierarchy to prevent infinite loops
    // when traversing up to root ancestor (BR-001-03)
    var visitedIds = new HashSet<int>();
    var current = folder;
    
    while (current != null && current.ParentFolderID.HasValue)
    {
        if (!visitedIds.Add(current.FolderID))
            throw new InvalidOperationException("Folder hierarchy contains cycle");
        
        current = _folderRepository.GetById(current.ParentFolderID.Value);
    }
}

// ❌ Wrong - comments state obvious facts
public void ProcessFolder(Folder folder)
{
    // Create a new hash set
    var visitedIds = new HashSet<int>();
    
    // Set current to folder
    var current = folder;
    
    // Loop while current is not null
    while (current != null && current.ParentFolderID.HasValue)
    {
        // Add current folder ID
        visitedIds.Add(current.FolderID);
    }
}
```

### TODO Comments

```csharp
// ✅ Correct - specific TODOs with context
// TODO: Implement caching for folder hierarchy (performance issue with deep nesting)
// TODO: Add transaction support for multi-file operations (handle partial failures)
// FIXME: Virus scan service timeout too low (3 seconds insufficient for large files)

// ❌ Wrong - vague TODOs
// TODO: fix this
// TODO: optimize
// TODO: refactor later
```

---

## 8. Testing Conventions

### Test Method Naming

```csharp
// ✅ Correct - MethodName_Condition_ExpectedResult
[TestMethod]
public void UploadFile_ExceedsSizeLimit_ThrowsArgumentException()
{
    // Arrange
    var service = new FileService(/* dependencies */);
    var largeFile = new FileUploadRequest { SizeBytes = 200_000_000 };
    
    // Act & Assert
    Assert.ThrowsException<ArgumentException>(
        () => service.UploadFiles(new[] { largeFile }, 1, testUser));
}

[TestMethod]
public void CreateFolder_ValidInput_ReturnsNewFolderWithId()
{
    // Arrange
    var service = new FolderService(/* dependencies */);
    
    // Act
    var folder = service.CreateFolder("TestFolder", 1, testUser);
    
    // Assert
    Assert.IsNotNull(folder);
    Assert.IsTrue(folder.FolderID > 0);
    Assert.AreEqual("TestFolder", folder.FolderName);
}

// ❌ Wrong - unclear test names
[TestMethod]
public void Test1() { }

[TestMethod]
public void FileUploadTest() { }

[TestMethod]
public void Should_Work() { }
```

---

## 9. String Handling

### String Formatting

```csharp
// ✅ Correct - string interpolation
var message = $"User {userName} uploaded {fileCount} files";
var path = $"{networkDrive}\\{departmentCode}\\{folderId}";

// ✅ Acceptable - String.Format for complex scenarios
var message = string.Format(
    "File '{0}' exceeds maximum size of {1:N0} bytes",
    fileName,
    maxFileSize);

// ❌ Wrong - concatenation
var message = "User " + userName + " uploaded " + fileCount + " files";
var path = networkDrive + "\\" + departmentCode + "\\" + folderId;
```

### String Validation

```csharp
// ✅ Correct
if (string.IsNullOrWhiteSpace(folderName))
    throw new ArgumentException("Folder name is required.");

if (string.IsNullOrEmpty(fileName))
    throw new ArgumentException("File name cannot be empty.");

// ❌ Wrong
if (folderName == null || folderName == "" || folderName.Trim() == "")
{
}

if (folderName?.Length == 0)
{
}
```

---

## 10. Collation & Thai Language Support

### Database Collation

```sql
-- ✅ Correct - Thai_CI_AS for Thai content
CREATE TABLE m_OrgUnit (
    OrgUnitID INT PRIMARY KEY,
    OrgUnitNameTH NVARCHAR(200) NOT NULL COLLATE Thai_CI_AS,
    OrgUnitNameEN NVARCHAR(200) NOT NULL,
    Description NVARCHAR(MAX) COLLATE Thai_CI_AS
);

-- All Thai columns MUST use Thai_CI_AS collation
```

### C# String Handling for Thai

```csharp
// ✅ Correct - preserve Thai text
public class Folder
{
    public string FolderNameTh { get; set; }   // Thai name
    public string FolderNameEn { get; set; }   // English name
}

var folder = new Folder
{
    FolderNameTh = "รายงานการขาย",
    FolderNameEn = "Sales Report"
};

// ✅ Correct - Thai string comparisons
if (folderNameTh.Equals("รายงาน", StringComparison.OrdinalIgnoreCase))
{
}

// ❌ Wrong - lose Thai characters
var name = folder.FolderNameTh.ToUpper();  // May corrupt Thai
```

---

## 11. Performance Considerations

### Avoid N+1 Queries

```csharp
// ❌ Wrong - N+1 query problem
var folders = _folderRepository.GetAllFolders();
foreach (var folder in folders)
{
    var files = _fileRepository.GetFilesByFolderId(folder.FolderID);
    // Process files
}

// ✅ Correct - single query or batch load
var folderIds = folders.Select(f => f.FolderID).ToList();
var allFiles = _fileRepository.GetFilesByFolderIds(folderIds);
var filesByFolder = allFiles.GroupBy(f => f.FolderID);
```

### Use LINQ Wisely

```csharp
// ❌ Wrong - loads all data into memory
var activeFiles = _fileRepository.GetAllFiles()
    .Where(f => !f.IsDeleted)
    .OrderBy(f => f.CreatedDate)
    .ToList();

// ✅ Correct - filter at database level
var activeFiles = _fileRepository.GetActiveFiles()
    .OrderBy(f => f.CreatedDate)
    .ToList();
```

### Connection Management

```csharp
// ✅ Correct - always use using statement
public List<FileDocument> GetFiles(int folderId)
{
    using (var conn = new SqlConnection(_connectionString))
    {
        conn.Open();
        using (var cmd = new SqlCommand(/* ... */))
        {
            // Use connection
        }
    } // Connection properly disposed
}

// ❌ Wrong - connection leak
public List<FileDocument> GetFiles(int folderId)
{
    var conn = new SqlConnection(_connectionString);
    conn.Open();
    var cmd = new SqlCommand(/* ... */);
    // Connection never closed!
}
```

---

## 12. Dependency Injection Best Practices

### Constructor Validation

```csharp
// ✅ Correct - validate all dependencies
public class FileService : IFileService
{
    private readonly IFileRepository _fileRepository;
    private readonly IFolderRepository _folderRepository;
    private readonly IPermissionService _permissionService;

    public FileService(
        IFileRepository fileRepository,
        IFolderRepository folderRepository,
        IPermissionService permissionService)
    {
        _fileRepository = fileRepository ?? 
            throw new ArgumentNullException(nameof(fileRepository));
        _folderRepository = folderRepository ?? 
            throw new ArgumentNullException(nameof(folderRepository));
        _permissionService = permissionService ?? 
            throw new ArgumentNullException(nameof(permissionService));
    }
}

// ❌ Wrong - don't validate silently
public class FileService : IFileService
{
    private readonly IFileRepository _fileRepository;

    public FileService(IFileRepository fileRepository)
    {
        _fileRepository = fileRepository;  // Null not checked
    }
    
    public void UploadFile()
    {
        if (_fileRepository == null)  // Discovered too late
            throw new InvalidOperationException();
    }
}
```

---

## Checklist Before Committing

- [ ] Method/class names use correct casing (PascalCase/camelCase)
- [ ] Private fields use `_camelCase` prefix
- [ ] Constants use `ALL_CAPS`
- [ ] All SQL queries are parameterized
- [ ] ADUserName used for audit stamps, not UserID
- [ ] Soft delete flag set instead of hard delete
- [ ] Public methods have XML documentation
- [ ] No empty catch blocks (bare `catch {}`)
- [ ] Specific exception types thrown with clear messages
- [ ] Dependencies validated in constructors
- [ ] Line length under 120 characters
- [ ] 4-space indentation (no tabs)
- [ ] No hardcoded credentials or sensitive data
- [ ] Thai text preserved with proper collation
- [ ] Tests follow naming convention
- [ ] Code compiles without warnings

---

## Tools & Enforcement

### Visual Studio Code Analysis
Enable these in project properties:
- Enable Code Analysis on Build
- Treat Warnings as Errors (for critical rules)

### StyleCop Configuration
Consider adding `.stylecop` file to enforce these rules automatically.

### Pre-commit Hooks
Implement git hooks to:
- Run code analysis
- Check for hardcoded credentials
- Validate naming conventions

