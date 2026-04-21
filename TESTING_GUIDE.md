# Testing Guide & Unit Testing Standards

## Overview

This document defines testing standards, patterns, and best practices for the CMS project. All business logic must have comprehensive unit test coverage.

---

## 1. Testing Philosophy

### Testing Pyramid

```
         △
        /|\
       / | \
      /  |  \  End-to-End Tests (UI, Integration)
     /   |   \
    ◇───────◇ Integration Tests (Database, Services)
   /     |     \
  /      |      \ Unit Tests (Services, Repositories)
 ◇───────────────◇
```

**Focus:** 70% Unit Tests, 20% Integration Tests, 10% End-to-End

### Test Types

| Type | Scope | Speed | Count | Tools |
|------|-------|-------|-------|-------|
| **Unit** | Single method | Fast | Most | MSTest, NUnit, Moq |
| **Integration** | Service + Database | Slower | Some | MSTest, real DB |
| **End-to-End** | Full workflow via UI | Slowest | Few | Selenium, manual |

---

## 2. Unit Testing Standards

### Test Project Structure

```
CMS.Tests/
├── Services/
│   ├── FileServiceTests.cs
│   ├── FolderServiceTests.cs
│   ├── ApprovalServiceTests.cs
│   └── PermissionServiceTests.cs
├── Repositories/
│   ├── FileRepositoryTests.cs
│   └── FolderRepositoryTests.cs
├── Controllers/
│   ├── FileUploadControllerTests.cs
│   └── ApprovalInboxControllerTests.cs
├── Fixtures/
│   ├── TestDataFixture.cs
│   ├── MockRepositoryFixture.cs
│   └── UserContextFixture.cs
└── Helpers/
    ├── TestHelper.cs
    └── MockHelper.cs
```

### Test Class Naming

```csharp
// ✅ Correct - ClassName + Tests
public class FileServiceTests { }
public class FolderRepositoryTests { }
public class FileUploadControllerTests { }

// ❌ Wrong
public class FileServiceTest { }        // Missing 's'
public class TestFileService { }        // 'Test' prefix
public class FileServiceTestClass { }   // Redundant
```

### Test Method Naming

```csharp
// ✅ Correct - MethodName_Scenario_ExpectedResult
[TestMethod]
public void UploadFile_ExceedsSizeLimit_ThrowsArgumentException() { }

[TestMethod]
public void CreateFolder_ValidInput_ReturnsFolderWithIdGreaterThanZero() { }

[TestMethod]
public void GetFilesInFolder_WithDeletedFiles_ReturnsOnlyActiveFiles() { }

[TestMethod]
public void DeleteFile_UserLacksPermission_ThrowsUnauthorizedAccessException() { }

// ❌ Wrong
[TestMethod]
public void Test1() { }

[TestMethod]
public void UploadTest() { }

[TestMethod]
public void Should_Work_Correctly() { }
```

---

## 3. Arrange-Act-Assert Pattern

### Standard Test Structure

```csharp
[TestClass]
public class FileServiceTests
{
    private FileService _service;
    private Mock<IFileDocumentRepository> _fileRepositoryMock;
    private Mock<IFolderRepository> _folderRepositoryMock;
    private Mock<IPermissionService> _permissionServiceMock;

    [TestInitialize]
    public void Setup()
    {
        // Arrange - setup mocks and fixtures
        _fileRepositoryMock = new Mock<IFileDocumentRepository>();
        _folderRepositoryMock = new Mock<IFolderRepository>();
        _permissionServiceMock = new Mock<IPermissionService>();

        _service = new FileService(
            _fileRepositoryMock.Object,
            _folderRepositoryMock.Object,
            _permissionServiceMock.Object);
    }

    [TestMethod]
    public void UploadFile_FileSizeValid_SavesFileAndReturnsId()
    {
        // ARRANGE - Setup test data and mocks
        var fileRequest = new FileUploadRequest
        {
            FileName = "document.pdf",
            SizeBytes = 1024 * 100,  // 100 KB
            Content = new byte[1024 * 100]
        };

        var user = new UserContext
        {
            UserID = 1,
            ADUserName = "domain\\testuser",
            OrgUnitID = 1
        };

        _folderRepositoryMock
            .Setup(r => r.GetById(It.IsAny<int>()))
            .Returns(new Folder { FolderID = 1, FolderName = "TestFolder" });

        _permissionServiceMock
            .Setup(p => p.CanUploadToFolder(It.IsAny<int>(), It.IsAny<int>()))
            .Returns(true);

        _fileRepositoryMock
            .Setup(r => r.Insert(It.IsAny<FileDocument>()))
            .Returns(42);  // Return new file ID

        // ACT - Execute the method being tested
        var result = _service.UploadFiles(
            new[] { fileRequest },
            folderId: 1,
            user: user);

        // ASSERT - Verify the results
        Assert.IsNotNull(result);
        var uploadResult = result.FirstOrDefault();
        Assert.IsTrue(uploadResult.Success);
        Assert.AreEqual(42, uploadResult.FileID);
        Assert.AreEqual("document.pdf", uploadResult.FileName);

        // Verify mocks were called correctly
        _fileRepositoryMock.Verify(
            r => r.Insert(It.Is<FileDocument>(f =>
                f.FileName == "document.pdf" &&
                f.FileSizeBytes == 1024 * 100)),
            Times.Once);
    }
}
```

---

## 4. Mocking with Moq Framework

### Mock Setup Patterns

```csharp
// ✅ Setup return value
_repositoryMock
    .Setup(r => r.GetById(5))
    .Returns(new FileDocument { FileID = 5, FileName = "test.pdf" });

// ✅ Setup with It.IsAny<T>
_repositoryMock
    .Setup(r => r.GetById(It.IsAny<int>()))
    .Returns(new FileDocument { FileID = 1 });

// ✅ Setup with predicate
_repositoryMock
    .Setup(r => r.GetById(It.Is<int>(id => id > 0)))
    .Returns(new FileDocument { FileID = 1 });

// ✅ Setup exception throw
_repositoryMock
    .Setup(r => r.Delete(It.IsAny<int>()))
    .Throws<InvalidOperationException>();

// ✅ Setup null return
_repositoryMock
    .Setup(r => r.GetById(It.IsAny<int>()))
    .Returns((FileDocument)null);

// ✅ Verify method was called
_repositoryMock.Verify(
    r => r.Insert(It.IsAny<FileDocument>()),
    Times.Once);

// ✅ Verify method never called
_repositoryMock.Verify(
    r => r.Delete(It.IsAny<int>()),
    Times.Never);

// ❌ Wrong - incomplete setup
_repositoryMock.Setup(r => r.GetById(5));  // No .Returns()
```

### Mock Repository Fixture

```csharp
public class MockRepositoryFixture
{
    public Mock<IFileDocumentRepository> FileRepository { get; private set; }
    public Mock<IFolderRepository> FolderRepository { get; private set; }
    public Mock<IPermissionRepository> PermissionRepository { get; private set; }

    public MockRepositoryFixture()
    {
        FileRepository = new Mock<IFileDocumentRepository>();
        FolderRepository = new Mock<IFolderRepository>();
        PermissionRepository = new Mock<IPermissionRepository>();

        // Setup default behaviors
        SetupDefaults();
    }

    private void SetupDefaults()
    {
        // Default: return null for non-existent items
        FileRepository
            .Setup(r => r.GetById(It.IsAny<int>()))
            .Returns((FileDocument)null);

        FolderRepository
            .Setup(r => r.GetById(It.IsAny<int>()))
            .Returns((Folder)null);
    }
}
```

---

## 5. Test Fixture & Test Data

### Test Data Factory

```csharp
public static class TestDataFactory
{
    public static FileDocument CreateTestFile(int fileId = 1, string fileName = "test.pdf")
    {
        return new FileDocument
        {
            FileID = fileId,
            FileName = fileName,
            FolderID = 1,
            FileSizeBytes = 1024 * 100,
            IsDeleted = false,
            CreatedBy = "domain\\testuser",
            CreatedDate = DateTime.Now
        };
    }

    public static Folder CreateTestFolder(int folderId = 1, string folderName = "TestFolder")
    {
        return new Folder
        {
            FolderID = folderId,
            FolderName = folderName,
            OrgUnitID = 1,
            FolderDepth = 1,
            ConfidentialityLevel = 3,
            CreatedBy = "domain\\testuser",
            CreatedDate = DateTime.Now
        };
    }

    public static UserContext CreateTestUser(int userId = 1)
    {
        return new UserContext
        {
            UserID = userId,
            ADUserName = "domain\\testuser",
            DisplayName = "Test User",
            OrgUnitID = 1,
            AppRole = 1,
            IsActive = true,
            ClientIP = "127.0.0.1"
        };
    }

    public static FileUploadRequest CreateTestUploadRequest(string fileName = "test.pdf")
    {
        return new FileUploadRequest
        {
            FileName = fileName,
            SizeBytes = 1024 * 100,
            Content = new byte[1024 * 100]
        };
    }
}
```

### Shared Test Fixture

```csharp
[TestClass]
public class FileServiceTests
{
    private MockRepositoryFixture _mockFixture;
    private FileService _service;

    [TestInitialize]
    public void SetupTest()
    {
        _mockFixture = new MockRepositoryFixture();

        _service = new FileService(
            _mockFixture.FileRepository.Object,
            _mockFixture.FolderRepository.Object,
            _mockFixture.PermissionRepository.Object);
    }

    [TestMethod]
    public void UploadFile_FileExists_ReturnsExistingFile()
    {
        // Use factory to create test data
        var existingFile = TestDataFactory.CreateTestFile(1, "existing.pdf");

        _mockFixture.FileRepository
            .Setup(r => r.FindByName("existing.pdf", 1))
            .Returns(existingFile);

        var request = TestDataFactory.CreateTestUploadRequest("existing.pdf");
        var user = TestDataFactory.CreateTestUser();

        // Act & Assert...
    }
}
```

---

## 6. Testing Business Logic

### Service Logic Tests

```csharp
[TestClass]
public class FolderServiceTests
{
    private FolderService _service;
    private Mock<IFolderRepository> _folderRepositoryMock;
    private Mock<IPermissionService> _permissionServiceMock;

    // ... Setup ...

    // Test business rule: Max folder depth
    [TestMethod]
    public void CreateSubFolder_ExceedsMaxDepth_ThrowsInvalidOperationException()
    {
        // ARRANGE
        var parentFolder = new Folder
        {
            FolderID = 1,
            FolderDepth = 10  // At max depth
        };

        _folderRepositoryMock
            .Setup(r => r.GetById(1))
            .Returns(parentFolder);

        var user = TestDataFactory.CreateTestUser();

        // ACT & ASSERT
        var ex = Assert.ThrowsException<InvalidOperationException>(
            () => _service.CreateSubFolder("NewFolder", 1, user, 3));

        Assert.IsTrue(ex.Message.Contains("Maximum folder depth"));
    }

    // Test business rule: Cannot set lower confidentiality than parent
    [TestMethod]
    public void CreateSubFolder_ConfidentialityLowerThanParent_ThrowsException()
    {
        // ARRANGE
        var parentFolder = new Folder
        {
            FolderID = 1,
            FolderDepth = 2,
            ConfidentialityLevel = 1  // HC (High Confidential)
        };

        _folderRepositoryMock
            .Setup(r => r.GetById(1))
            .Returns(parentFolder);

        var user = TestDataFactory.CreateTestUser();

        // ACT & ASSERT
        var ex = Assert.ThrowsException<InvalidOperationException>(
            () => _service.CreateSubFolder("NewFolder", 1, user, 3));  // NC level

        Assert.IsTrue(ex.Message.Contains("confidentiality"));
    }

    // Test business rule: Soft delete
    [TestMethod]
    public void DeleteFolder_SetsIsDeletedFlag_DoesNotRemoveRecord()
    {
        // ARRANGE
        var folder = TestDataFactory.CreateTestFolder(1);
        _folderRepositoryMock
            .Setup(r => r.GetById(1))
            .Returns(folder);

        var user = TestDataFactory.CreateTestUser();

        // ACT
        _service.DeleteFolder(1, user);

        // ASSERT
        _folderRepositoryMock.Verify(
            r => r.Update(It.Is<Folder>(f =>
                f.FolderID == 1 &&
                f.IsDeleted == true &&
                f.DeletedByADUser == user.ADUserName)),
            Times.Once);
    }

    // Test business rule: Folder name validation
    [TestMethod]
    [DataRow("valid_folder")]
    [DataRow("الاسم")]  // Arabic
    [DataRow("หนังสือ")]  // Thai
    public void CreateSubFolder_ValidName_Succeeds(string folderName)
    {
        // ARRANGE
        _folderRepositoryMock
            .Setup(r => r.GetById(It.IsAny<int>()))
            .Returns(TestDataFactory.CreateTestFolder());

        _permissionServiceMock
            .Setup(p => p.CanCreateFolder(It.IsAny<UserContext>(), It.IsAny<int>()))
            .Returns(true);

        var user = TestDataFactory.CreateTestUser();

        // ACT & ASSERT - should not throw
        _service.CreateSubFolder(folderName, 1, user, 3);
    }

    // Test business rule: Invalid folder name characters
    [TestMethod]
    [DataRow("folder\\name")]
    [DataRow("folder/name")]
    [DataRow("folder:name")]
    [DataRow("folder*name")]
    public void CreateSubFolder_InvalidCharacters_ThrowsException(string invalidName)
    {
        var user = TestDataFactory.CreateTestUser();

        // ACT & ASSERT
        Assert.ThrowsException<ArgumentException>(
            () => _service.CreateSubFolder(invalidName, 1, user, 3));
    }
}
```

---

## 7. Testing Permission Logic

### Permission Test Scenarios

```csharp
[TestClass]
public class PermissionServiceTests
{
    private PermissionService _service;
    private Mock<IPermissionRepository> _permissionRepositoryMock;
    private Mock<IFolderRepository> _folderRepositoryMock;

    // ... Setup ...

    // Test: Explicit deny overrides allow
    [TestMethod]
    public void HasPermission_ExplicitDenyExists_ReturnsFalse()
    {
        // ARRANGE
        int fileId = 1, userId = 5;

        // Explicit DENY for user 5 on file 1
        _permissionRepositoryMock
            .Setup(r => r.GetDenyPermission(userId, fileId))
            .Returns(new FilePermission
            {
                FilePermissionID = 1,
                FileID = fileId,
                UserID = userId,
                PermissionType = PermissionType.Delete,
                PermissionMode = PermissionMode.Deny
            });

        // Also has ALLOW (but deny should override)
        _permissionRepositoryMock
            .Setup(r => r.GetAllowPermission(userId, fileId))
            .Returns(new FilePermission
            {
                PermissionType = PermissionType.Delete,
                PermissionMode = PermissionMode.Allow
            });

        // ACT
        bool hasPermission = _service.HasPermission(userId, fileId, PermissionType.Delete);

        // ASSERT
        Assert.IsFalse(hasPermission, "Explicit deny should override allow");
    }

    // Test: Permission inheritance from parent folder
    [TestMethod]
    public void HasPermission_InheritFromParent_ReturnsParentPermission()
    {
        // ARRANGE
        int userId = 5, fileId = 1;

        // No explicit permission on file
        _permissionRepositoryMock
            .Setup(r => r.GetDenyPermission(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((FilePermission)null);

        _permissionRepositoryMock
            .Setup(r => r.GetAllowPermission(userId, fileId))
            .Returns((FilePermission)null);

        // File has parent folder with inheritance
        var parentFolder = TestDataFactory.CreateTestFolder(10, "Parent");
        parentFolder.InheritPermission = true;

        _folderRepositoryMock
            .Setup(r => r.GetById(fileId))
            .Returns(parentFolder);

        // Parent folder allows permission
        _permissionRepositoryMock
            .Setup(r => r.GetAllowPermission(userId, parentFolder.FolderID))
            .Returns(new FolderPermission
            {
                PermissionType = PermissionType.View,
                PermissionMode = PermissionMode.Allow
            });

        // ACT
        bool hasPermission = _service.HasPermission(userId, fileId, PermissionType.View);

        // ASSERT
        Assert.IsTrue(hasPermission, "Should inherit permission from parent");
    }

    // Test: Default deny (no explicit permission)
    [TestMethod]
    public void HasPermission_NoExplicitPermission_ReturnsFalse()
    {
        // ARRANGE
        _permissionRepositoryMock
            .Setup(r => r.GetDenyPermission(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((FilePermission)null);

        _permissionRepositoryMock
            .Setup(r => r.GetAllowPermission(It.IsAny<int>(), It.IsAny<int>()))
            .Returns((FilePermission)null);

        _folderRepositoryMock
            .Setup(r => r.GetById(It.IsAny<int>()))
            .Returns((Folder)null);

        // ACT
        bool hasPermission = _service.HasPermission(5, 1, PermissionType.Delete);

        // ASSERT
        Assert.IsFalse(hasPermission, "Default deny when no permission set");
    }
}
```

---

## 8. Testing Exception Handling

### Exception Test Pattern

```csharp
[TestClass]
public class FileServiceExceptionTests
{
    private FileService _service;
    private Mock<IFileDocumentRepository> _fileRepositoryMock;

    // ... Setup ...

    [TestMethod]
    [ExpectedException(typeof(ArgumentNullException))]
    public void UploadFile_NullFiles_ThrowsArgumentNullException()
    {
        // ACT
        _service.UploadFiles(null, 1, new UserContext());
    }

    [TestMethod]
    public void UploadFile_NullFiles_ThrowsArgumentNullExceptionWithMessage()
    {
        // ACT & ASSERT
        var ex = Assert.ThrowsException<ArgumentNullException>(
            () => _service.UploadFiles(null, 1, new UserContext()));

        Assert.IsTrue(ex.Message.Contains("files"));
    }

    [TestMethod]
    public void DeleteFile_FileNotFound_ThrowsInvalidOperationException()
    {
        // ARRANGE
        _fileRepositoryMock
            .Setup(r => r.GetById(999))
            .Returns((FileDocument)null);

        // ACT & ASSERT
        var ex = Assert.ThrowsException<InvalidOperationException>(
            () => _service.DeleteFile(999, new UserContext { UserID = 1 }));

        Assert.AreEqual("File 999 not found", ex.Message);
    }

    [TestMethod]
    public void UploadFile_FileSizeExceedsLimit_ThrowsCmsBusinessException()
    {
        // ARRANGE
        var largeFile = new FileUploadRequest
        {
            FileName = "huge.iso",
            SizeBytes = 200_000_000  // 200 MB > 100 MB limit
        };

        // ACT & ASSERT
        var ex = Assert.ThrowsException<CmsBusinessException>(
            () => _service.UploadFiles(new[] { largeFile }, 1, new UserContext { UserID = 1 }));

        Assert.AreEqual("FILE-001", ex.ErrorCode);
    }
}
```

---

## 9. Testing Audit & Logging

### Audit Log Test

```csharp
[TestClass]
public class AuditServiceTests
{
    private AuditService _service;
    private Mock<IAuditRepository> _auditRepositoryMock;

    // ... Setup ...

    [TestMethod]
    public void LogFileUpload_RecordsAuditWithCorrectUser()
    {
        // ARRANGE
        int fileId = 42;
        string adUserName = "domain\\testuser";
        var user = TestDataFactory.CreateTestUser();
        user.ADUserName = adUserName;

        // ACT
        _service.LogFileUpload(user.UserID, fileId, "report.pdf", 1024000, adUserName);

        // ASSERT
        _auditRepositoryMock.Verify(
            r => r.Insert(It.Is<AuditLog>(a =>
                a.TableName == "t_FileDocument" &&
                a.RecordID == fileId &&
                a.Action == "FILE_UPLOAD" &&
                a.ActionBy == adUserName &&  // Use ADUserName, not UserID
                a.ModuleCode == "MOD-003")),
            Times.Once);
    }

    [TestMethod]
    public void LogFileDelete_RecordsDeletionTimestamp()
    {
        // ARRANGE
        var user = TestDataFactory.CreateTestUser();
        var beforeTime = DateTime.Now;

        // ACT
        _service.LogFileDelete(user.UserID, 42, "User requested deletion");

        // ASSERT
        _auditRepositoryMock.Verify(
            r => r.Insert(It.Is<AuditLog>(a =>
                a.Action == "FILE_DELETE" &&
                a.ActionDate >= beforeTime &&
                a.ActionDate <= DateTime.Now)),
            Times.Once);
    }
}
```

---

## 10. Integration Tests

### Database Integration Test

```csharp
[TestClass]
public class FileRepositoryIntegrationTests
{
    private string _connectionString = "Data Source=TEST_SERVER;Initial Catalog=CMS_TEST;";
    private FileDocumentRepository _repository;

    [TestInitialize]
    public void Setup()
    {
        // Use test database
        var configManager = new TestConfigManager(_connectionString);
        _repository = new FileDocumentRepository(configManager);

        // Cleanup before each test
        CleanupTestData();
    }

    [TestMethod]
    public void Insert_NewFile_ReturnsNewId()
    {
        // ARRANGE
        var file = TestDataFactory.CreateTestFile();
        file.FolderID = EnsureTestFolderExists();

        // ACT
        int newId = _repository.Insert(file);

        // ASSERT
        Assert.IsTrue(newId > 0);

        // Verify in database
        var retrieved = _repository.GetById(newId);
        Assert.IsNotNull(retrieved);
        Assert.AreEqual(file.FileName, retrieved.FileName);
    }

    [TestMethod]
    public void GetFilesInFolder_ExcludesDeletedFiles()
    {
        // ARRANGE
        int folderId = EnsureTestFolderExists();
        var file1 = TestDataFactory.CreateTestFile(1, "active.pdf");
        var file2 = TestDataFactory.CreateTestFile(2, "deleted.pdf");
        file2.IsDeleted = true;

        _repository.Insert(file1);
        _repository.Insert(file2);

        // ACT
        var files = _repository.GetFilesInFolder(folderId);

        // ASSERT
        Assert.AreEqual(1, files.Count());
        Assert.AreEqual("active.pdf", files.First().FileName);
    }

    private void CleanupTestData()
    {
        using (var conn = new SqlConnection(_connectionString))
        {
            conn.Open();
            using (var cmd = new SqlCommand("DELETE FROM t_FileDocument WHERE FileName LIKE 'test_%'", conn))
            {
                cmd.ExecuteNonQuery();
            }
        }
    }
}
```

---

## 11. Test Coverage Goals

### Minimum Coverage by Layer

| Layer | Minimum Coverage | Target |
|-------|------------------|--------|
| Services | 80% | 90%+ |
| Repositories | 70% | 85%+ |
| Controllers | 60% | 75%+ |
| Models | 0% | Not required |

### Critical Paths - 100% Coverage

```csharp
// MUST have 100% coverage:
// - Permission checks
// - Business rule validations
// - Exception handling paths
// - Soft delete logic
// - Audit logging

[TestMethod]
public void UploadFile_UserLacksPermission_ReturnsFalseAndLogsEvent()
{
    // Every code path related to permission check must be tested
}

[TestMethod]
public void CreateFolder_ExceedsMaxDepth_ThrowsException()
{
    // Every business rule must be tested
}
```

---

## 12. Test Execution

### Running Tests in Visual Studio

```bash
# Run all tests
Test > Run > All Tests

# Run specific test class
Test > Run > Tests from Current Selection

# Run with code coverage
Test > Analyze Code Coverage > Analyze Code Coverage for All Tests
```

### Running Tests from Command Line

```bash
# MSTest
"C:\Program Files (x86)\Microsoft Visual Studio\2019\Professional\Common7\IDE\CommonExtensions\Microsoft\TestWindow\vstest.console.exe" CMS.Tests.dll

# Run specific test class
vstest.console.exe CMS.Tests.dll /Tests:FileServiceTests

# With code coverage
vstest.console.exe CMS.Tests.dll /Enablecodecoverage
```

### Test Results Analysis

- **Red** - Failing test (needs fix)
- **Green** - Passing test
- **Yellow** - Skipped test (mark with [Ignore])
- **Coverage %** - Code path coverage

---

## 13. Best Practices

### Test Quality

✅ **DO:**
- Write focused tests (one assertion per test, or related assertions)
- Use descriptive test names
- Keep tests simple and readable
- Test edge cases and error conditions
- Mock external dependencies
- Use test data factories
- Test one concern per test method

❌ **DON'T:**
- Test multiple concerns in one test
- Write tests that depend on other tests
- Use Thread.Sleep() in tests
- Create complex test setup
- Test framework code (Moq, MSTest)
- Skip tests permanently without documenting why

### Mocking Guidelines

```csharp
// ✅ Correct - mock external dependencies
private Mock<IFileRepository> _repositoryMock;
private FileService _service;

[TestInitialize]
public void Setup()
{
    _repositoryMock = new Mock<IFileRepository>();
    _service = new FileService(_repositoryMock.Object);
}

[TestMethod]
public void UploadFile_Success()
{
    _repositoryMock.Setup(r => r.Insert(It.IsAny<FileDocument>())).Returns(1);
    // Test service logic, not repository
}

// ❌ Wrong - testing with real dependencies
private FileRepository _repository;  // Real database connection!
private FileService _service;

[TestInitialize]
public void Setup()
{
    _repository = new FileRepository("Server=prod-db;Database=CMS;");  // Production!
    _service = new FileService(_repository);
}
```

### Test Isolation

```csharp
// ✅ Correct - independent tests
[TestMethod]
public void Test1_ShouldPass()
{
    var result = _service.Method1();
    Assert.AreEqual(expected, result);
}

[TestMethod]
public void Test2_ShouldPass()
{
    // Does not depend on Test1 result
    var result = _service.Method2();
    Assert.AreEqual(expected, result);
}

// ❌ Wrong - dependent tests
[TestMethod]
public void Test1_CreateData()
{
    _service.CreateData();
}

[TestMethod]
public void Test2_ReadData()
{
    // Depends on Test1 running first!
    var data = _service.ReadData();
}
```

---

## 14. Continuous Integration

### Pre-commit Testing

Before committing, ensure:
```bash
# Run all tests
dotnet test CMS.sln

# Check code coverage
dotnet test CMS.sln /p:CollectCoverageMetrics=true

# Build with warnings as errors
dotnet build CMS.sln /p:TreatWarningsAsErrors=true
```

### Build Pipeline

```yaml
# .gitlab-ci.yml or similar
test:
  script:
    - dotnet test CMS.Tests.dll
    - dotnet test CMS.Tests.dll /p:CollectCoverageMetrics=true
  coverage: '/Covered.*?%/'
  allow_failure: false
```

---

## 15. Test Checklist

Before marking test complete:

- [ ] Test name follows MethodName_Scenario_ExpectedResult
- [ ] Test has Arrange-Act-Assert sections
- [ ] All dependencies mocked (except integration tests)
- [ ] Test passes consistently
- [ ] Test fails if logic is broken
- [ ] No dependencies between tests
- [ ] Meaningful assertion messages
- [ ] Uses test data factories
- [ ] No hardcoded values (except test data)
- [ ] Handles edge cases
- [ ] Exception types verified
- [ ] Mocks verified with correct parameters

