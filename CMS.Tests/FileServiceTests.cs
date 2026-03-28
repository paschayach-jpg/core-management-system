using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using NUnit.Framework;
using NSubstitute;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Tests
{
    /// <summary>
    /// Unit tests for FileService.
    /// Validates Requirements 5.1–5.16, 10.3–10.5, 12.1–12.6, 15.1.
    /// </summary>
    [TestFixture]
    public class FileServiceTests
    {
        private IFileDocumentRepository _fileDocRepo;
        private IFolderRepository _folderRepo;
        private IOrgUnitRepository _orgUnitRepo;
        private IVirusScanService _virusScanService;
        private IPermissionService _permService;
        private INetworkDriveHelper _networkDriveHelper;
        private IAuditService _auditService;
        private INotificationRepository _notifRepo;
        private IUserAccountRepository _userRepo;
        private ILoggingService _loggingService;
        private FileService _svc;

        private const int UserId = 42;
        private const int FolderId = 100;
        private const int OrgUnitId = 10;
        private const long MaxFileSize = 100L * 1024 * 1024; // 100 MB

        [SetUp]
        public void SetUp()
        {
            _fileDocRepo = Substitute.For<IFileDocumentRepository>();
            _folderRepo = Substitute.For<IFolderRepository>();
            _orgUnitRepo = Substitute.For<IOrgUnitRepository>();
            _virusScanService = Substitute.For<IVirusScanService>();
            _permService = Substitute.For<IPermissionService>();
            _networkDriveHelper = Substitute.For<INetworkDriveHelper>();
            _auditService = Substitute.For<IAuditService>();
            _notifRepo = Substitute.For<INotificationRepository>();
            _userRepo = Substitute.For<IUserAccountRepository>();
            _loggingService = Substitute.For<ILoggingService>();

            _svc = new FileService(
                _fileDocRepo, _folderRepo, _orgUnitRepo, _virusScanService,
                _permService, _networkDriveHelper, _auditService,
                _notifRepo, _userRepo, _loggingService);

            // Default setup
            _permService.CheckPermission(UserId, FolderId, "Write").Returns(true);
            _permService.CheckPermission(UserId, FolderId, "Read").Returns(true);
            _permService.CheckPermission(UserId, FolderId, "Delete").Returns(true);
            _virusScanService.Scan(Arg.Any<Stream>(), Arg.Any<string>())
                .Returns(new ScanResult { IsClean = true, ScannerError = false });
            _networkDriveHelper.BuildPath(OrgUnitId, FolderId, Arg.Any<string>())
                .Returns(x => $"/network/drive/{x[0]}/{x[1]}/{x[2]}");
            _fileDocRepo.Insert(Arg.Any<FileDocument>()).Returns(1);
        }

        // -----------------------------------------------------------------------
        // UploadFiles - Batch Limit Tests (Requirement 5.1, 5.2)
        // -----------------------------------------------------------------------

        [Test]
        public void UploadFiles_WithValidBatch_SucceedsForAllFiles()
        {
            // Arrange
            var folder = new Folder { FolderId = FolderId, OrgUnitID = OrgUnitId, IsDeleted = false, IsArchived = false };
            _folderRepo.GetById(FolderId).Returns(folder);

            var files = new List<FileUploadRequest>
            {
                MakeFileRequest("file1.pdf", 1024),
                MakeFileRequest("file2.docx", 2048)
            };

            var user = MakeUserContext();

            // Act
            var results = _svc.UploadFiles(files, FolderId, user).ToList();

            // Assert
            Assert.AreEqual(2, results.Count);
            Assert.IsTrue(results.All(r => r.Success));
        }

        [Test]
        public void UploadFiles_WithMoreThan10Files_RejectsAllFiles()
        {
            // Arrange
            var files = new List<FileUploadRequest>();
            for (int i = 0; i < 11; i++)
                files.Add(MakeFileRequest($"file{i}.pdf", 1024));

            var user = MakeUserContext();

            // Act
            var results = _svc.UploadFiles(files, FolderId, user).ToList();

            // Assert
            Assert.AreEqual(11, results.Count);
            Assert.IsTrue(results.All(r => !r.Success));
            Assert.IsTrue(results.All(r => r.ErrorMessage.Contains("maximum is 10")));
        }

        [Test]
        public void UploadFiles_WithExactly10Files_Succeeds()
        {
            // Arrange
            var folder = new Folder { FolderId = FolderId, OrgUnitID = OrgUnitId, IsDeleted = false, IsArchived = false };
            _folderRepo.GetById(FolderId).Returns(folder);

            var files = new List<FileUploadRequest>();
            for (int i = 0; i < 10; i++)
                files.Add(MakeFileRequest($"file{i}.pdf", 1024));

            var user = MakeUserContext();

            // Act
            var results = _svc.UploadFiles(files, FolderId, user).ToList();

            // Assert
            Assert.AreEqual(10, results.Count);
            Assert.IsTrue(results.All(r => r.Success));
        }

        // -----------------------------------------------------------------------
        // UploadFiles - File Size Limit Tests (Requirement 5.3, 5.4)
        // -----------------------------------------------------------------------

        [Test]
        public void UploadFiles_WithFileExceeding100MB_RejectsFile()
        {
            // Arrange
            var folder = new Folder { FolderId = FolderId, OrgUnitID = OrgUnitId, IsDeleted = false, IsArchived = false };
            _folderRepo.GetById(FolderId).Returns(folder);

            var files = new List<FileUploadRequest>
            {
                MakeFileRequest("oversized.pdf", MaxFileSize + 1)
            };

            var user = MakeUserContext();

            // Act
            var results = _svc.UploadFiles(files, FolderId, user).ToList();

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.IsFalse(results[0].Success);
            Assert.IsTrue(results[0].ErrorMessage.Contains("exceeds the 100 MB limit"));
        }

        [Test]
        public void UploadFiles_WithFileAt100MB_Succeeds()
        {
            // Arrange
            var folder = new Folder { FolderId = FolderId, OrgUnitID = OrgUnitId, IsDeleted = false, IsArchived = false };
            _folderRepo.GetById(FolderId).Returns(folder);

            var files = new List<FileUploadRequest>
            {
                MakeFileRequest("large.pdf", MaxFileSize)
            };

            var user = MakeUserContext();

            // Act
            var results = _svc.UploadFiles(files, FolderId, user).ToList();

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].Success);
        }

        // -----------------------------------------------------------------------
        // UploadFiles - Extension Whitelist Tests (Requirement 5.5, 5.6)
        // -----------------------------------------------------------------------

        [Test]
        [TestCase(".pdf")]
        [TestCase(".doc")]
        [TestCase(".docx")]
        [TestCase(".xls")]
        [TestCase(".xlsx")]
        [TestCase(".ppt")]
        [TestCase(".pptx")]
        [TestCase(".jpg")]
        [TestCase(".png")]
        [TestCase(".gif")]
        [TestCase(".zip")]
        public void UploadFiles_WithWhitelistedExtension_Succeeds(string ext)
        {
            // Arrange
            var folder = new Folder { FolderId = FolderId, OrgUnitID = OrgUnitId, IsDeleted = false, IsArchived = false };
            _folderRepo.GetById(FolderId).Returns(folder);

            var files = new List<FileUploadRequest>
            {
                MakeFileRequest($"file{ext}", 1024)
            };

            var user = MakeUserContext();

            // Act
            var results = _svc.UploadFiles(files, FolderId, user).ToList();

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].Success);
        }

        [Test]
        [TestCase(".exe")]
        [TestCase(".bat")]
        [TestCase(".com")]
        [TestCase(".txt")]
        public void UploadFiles_WithDisallowedExtension_RejectsFile(string ext)
        {
            // Arrange
            var folder = new Folder { FolderId = FolderId, OrgUnitID = OrgUnitId, IsDeleted = false, IsArchived = false };
            _folderRepo.GetById(FolderId).Returns(folder);

            var files = new List<FileUploadRequest>
            {
                MakeFileRequest($"file{ext}", 1024)
            };

            var user = MakeUserContext();

            // Act
            var results = _svc.UploadFiles(files, FolderId, user).ToList();

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.IsFalse(results[0].Success);
            Assert.IsTrue(results[0].ErrorMessage.Contains("not permitted"));
        }

        // -----------------------------------------------------------------------
        // UploadFiles - Virus Scanning Tests (Requirement 5.7, 5.8, 11.1–11.6)
        // -----------------------------------------------------------------------

        [Test]
        public void UploadFiles_WithCleanFile_Succeeds()
        {
            // Arrange
            var folder = new Folder { FolderId = FolderId, OrgUnitID = OrgUnitId, IsDeleted = false, IsArchived = false };
            _folderRepo.GetById(FolderId).Returns(folder);

            _virusScanService.Scan(Arg.Any<Stream>(), Arg.Any<string>())
                .Returns(new ScanResult { IsClean = true, ScannerError = false });

            var files = new List<FileUploadRequest>
            {
                MakeFileRequest("clean.pdf", 1024)
            };

            var user = MakeUserContext();

            // Act
            var results = _svc.UploadFiles(files, FolderId, user).ToList();

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].Success);
        }

        [Test]
        public void UploadFiles_WithInfectedFile_RejectsFile()
        {
            // Arrange
            var folder = new Folder { FolderId = FolderId, OrgUnitID = OrgUnitId, IsDeleted = false, IsArchived = false };
            _folderRepo.GetById(FolderId).Returns(folder);

            _virusScanService.Scan(Arg.Any<Stream>(), Arg.Any<string>())
                .Returns(new ScanResult { IsClean = false, ThreatName = "Trojan.Generic", ScannerError = false });

            var files = new List<FileUploadRequest>
            {
                MakeFileRequest("infected.pdf", 1024)
            };

            var user = MakeUserContext();

            // Act
            var results = _svc.UploadFiles(files, FolderId, user).ToList();

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.IsFalse(results[0].Success);
            Assert.IsTrue(results[0].ErrorMessage.Contains("Virus threat detected"));
            Assert.IsTrue(results[0].ErrorMessage.Contains("Trojan.Generic"));
        }

        [Test]
        public void UploadFiles_WithScannerError_RejectsFile()
        {
            // Arrange
            var folder = new Folder { FolderId = FolderId, OrgUnitID = OrgUnitId, IsDeleted = false, IsArchived = false };
            _folderRepo.GetById(FolderId).Returns(folder);

            _virusScanService.Scan(Arg.Any<Stream>(), Arg.Any<string>())
                .Returns(new ScanResult { IsClean = false, ScannerError = true });

            var files = new List<FileUploadRequest>
            {
                MakeFileRequest("file.pdf", 1024)
            };

            var user = MakeUserContext();

            // Act
            var results = _svc.UploadFiles(files, FolderId, user).ToList();

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.IsFalse(results[0].Success);
            Assert.IsTrue(results[0].ErrorMessage.Contains("Virus scanner is unavailable"));
        }

        // -----------------------------------------------------------------------
        // UploadFiles - Quota Tests (Requirement 12.1, 12.2)
        // -----------------------------------------------------------------------

        [Test]
        public void UploadFiles_WithinQuota_Succeeds()
        {
            // Arrange
            var folder = new Folder
            {
                FolderId = FolderId,
                OrgUnitID = OrgUnitId,
                IsDeleted = false,
                IsArchived = false,
                FolderSizeBytes = 1000,
                QuotaBytes = 10000
            };
            _folderRepo.GetById(FolderId).Returns(folder);

            var files = new List<FileUploadRequest>
            {
                MakeFileRequest("file.pdf", 5000)
            };

            var user = MakeUserContext();

            // Act
            var results = _svc.UploadFiles(files, FolderId, user).ToList();

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].Success);
        }

        [Test]
        public void UploadFiles_ExceedingQuota_RejectsFile()
        {
            // Arrange
            var folder = new Folder
            {
                FolderId = FolderId,
                OrgUnitID = OrgUnitId,
                IsDeleted = false,
                IsArchived = false,
                FolderSizeBytes = 8000,
                QuotaBytes = 10000
            };
            _folderRepo.GetById(FolderId).Returns(folder);

            var files = new List<FileUploadRequest>
            {
                MakeFileRequest("file.pdf", 5000)
            };

            var user = MakeUserContext();

            // Act
            var results = _svc.UploadFiles(files, FolderId, user).ToList();

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.IsFalse(results[0].Success);
            Assert.IsTrue(results[0].ErrorMessage.Contains("exceed folder quota"));
        }

        [Test]
        public void UploadFiles_WithoutQuotaSet_Succeeds()
        {
            // Arrange
            var folder = new Folder
            {
                FolderId = FolderId,
                OrgUnitID = OrgUnitId,
                IsDeleted = false,
                IsArchived = false,
                FolderSizeBytes = 1000,
                QuotaBytes = null
            };
            _folderRepo.GetById(FolderId).Returns(folder);

            var files = new List<FileUploadRequest>
            {
                MakeFileRequest("file.pdf", 1000000)
            };

            var user = MakeUserContext();

            // Act
            var results = _svc.UploadFiles(files, FolderId, user).ToList();

            // Assert
            Assert.AreEqual(1, results.Count);
            Assert.IsTrue(results[0].Success);
        }

        // -----------------------------------------------------------------------
        // DownloadFile - Integrity Check Tests (Requirement 5.14, 5.15)
        // -----------------------------------------------------------------------

        [Test]
        public void DownloadFile_WithMatchingChecksum_Succeeds()
        {
            // Arrange
            // SHA256 of bytes {1,2,3} = 039058c6f2c0cb492c533b0a4d14ef77cc0f78abccced5287d84a1a2011cfb81
            var doc = new FileDocument
            {
                FileID = 1,
                FolderID = FolderId,
                OriginalFileName = "test.pdf",
                Checksum = "039058c6f2c0cb492c533b0a4d14ef77cc0f78abccced5287d84a1a2011cfb81",
                FileStatus = 1,
                PhysicalPath = "/path/to/file"
            };
            _fileDocRepo.GetById(1).Returns(doc);

            var fileContent = new MemoryStream(new byte[] { 1, 2, 3 });
            _networkDriveHelper.ReadFile("/path/to/file").Returns(fileContent);

            var user = MakeUserContext();

            // Act
            var result = _svc.DownloadFile(1, user);

            // Assert
            Assert.IsTrue(result.Success);
            Assert.AreEqual("test.pdf", result.FileName);
            Assert.IsNotNull(result.FileStream);
        }

        [Test]
        public void DownloadFile_WithMismatchedChecksum_Fails()
        {
            // Arrange
            var doc = new FileDocument
            {
                FileID = 1,
                FolderID = FolderId,
                OriginalFileName = "test.pdf",
                Checksum = "wrongchecksum",
                FileStatus = 1,
                PhysicalPath = "/path/to/file"
            };
            _fileDocRepo.GetById(1).Returns(doc);

            var fileContent = new MemoryStream(new byte[] { 1, 2, 3 });
            _networkDriveHelper.ReadFile("/path/to/file").Returns(fileContent);

            var user = MakeUserContext();

            // Act
            var result = _svc.DownloadFile(1, user);

            // Assert
            Assert.IsFalse(result.Success);
            Assert.IsTrue(result.ErrorMessage.Contains("integrity check"));
        }

        // -----------------------------------------------------------------------
        // DeleteFile Tests (Requirement 5.16)
        // -----------------------------------------------------------------------

        [Test]
        public void DeleteFile_WithValidFile_SoftDeletes()
        {
            // Arrange
            var doc = new FileDocument
            {
                FileID = 1,
                FolderID = FolderId,
                OriginalFileName = "test.pdf",
                FileStatus = 1
            };
            _fileDocRepo.GetById(1).Returns(doc);

            var user = MakeUserContext();

            // Act
            _svc.DeleteFile(1, user);

            // Assert
            _fileDocRepo.Received(1).SoftDelete(1, UserId);
        }

        [Test]
        public void DeleteFile_WithDeletedFile_Throws()
        {
            // Arrange
            var doc = new FileDocument
            {
                FileID = 1,
                FolderID = FolderId,
                OriginalFileName = "test.pdf",
                FileStatus = 3 // Deleted
            };
            _fileDocRepo.GetById(1).Returns(doc);

            var user = MakeUserContext();

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _svc.DeleteFile(1, user));
        }

        // -----------------------------------------------------------------------
        // GetStorageReport Tests (Requirement 12.6)
        // -----------------------------------------------------------------------

        [Test]
        public void GetStorageReport_ReturnsUsagePerFolder()
        {
            // Arrange
            var orgUnit = new OrgUnit { OrgUnitID = OrgUnitId, OrgUnitNameTH = "Test OrgUnit" };
            _orgUnitRepo.GetById(OrgUnitId).Returns(orgUnit);

            var folders = new List<Folder>
            {
                new Folder { FolderID = 1, FolderName = "Folder1", FolderSizeBytes = 1000, IsDeleted = false },
                new Folder { FolderID = 2, FolderName = "Folder2", FolderSizeBytes = 2000, IsDeleted = false }
            };
            _folderRepo.GetRootFoldersByOrgUnit(OrgUnitId).Returns(folders);
            _folderRepo.GetDescendants(Arg.Any<int>()).Returns(new List<Folder>());

            var user = MakeUserContext();

            // Act
            var report = _svc.GetStorageReport(OrgUnitId, user);

            // Assert
            Assert.AreEqual(OrgUnitId, report.OrgUnitID);
            Assert.AreEqual("Test OrgUnit", report.OrgUnitName);
            Assert.AreEqual(2, report.Folders.Count);
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private FileUploadRequest MakeFileRequest(string fileName, long fileSize)
        {
            return new FileUploadRequest
            {
                FileName = fileName,
                FileSizeBytes = fileSize,
                FileStream = new MemoryStream(new byte[fileSize]),
                MimeType = "application/octet-stream"
            };
        }

        private UserContext MakeUserContext()
        {
            return new UserContext
            {
                UserID = UserId,
                ADUserName = "domain\\user",
                AppRole = 1,
                MachineName = "WORKSTATION",
                IPAddress = "192.168.1.1",
                SessionID = "session123"
            };
        }
    }
}
