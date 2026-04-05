using System;
using System.Collections.Generic;
using System.Linq;
using Xunit;
using Moq;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Domain.Enums;
using CMS.Services;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;
using CMS.Infrastructure.Interfaces;

namespace CMS.Tests
{
    /// <summary>
    /// Unit tests for ShareService methods.
    /// Satisfies Requirements 1.1, 2.1, 3.1, 4.1, 5.1, 14.1, 15.1
    /// </summary>
    public class ShareServiceUnitTests
    {
        private readonly Mock<IExternalShareRepository> _mockExternalShareRepo;
        private readonly Mock<IShareTokenRepository> _mockShareTokenRepo;
        private readonly Mock<IShareAccessLogRepository> _mockShareAccessLogRepo;
        private readonly Mock<IFileDocumentRepository> _mockFileRepo;
        private readonly Mock<IFolderRepository> _mockFolderRepo;
        private readonly Mock<IFilePermissionRepository> _mockFilePermissionRepo;
        private readonly Mock<IPermissionRepository> _mockPermissionRepo;
        private readonly Mock<IShareTokenService> _mockShareTokenService;
        private readonly Mock<IPermissionService> _mockPermissionService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly Mock<ILoggingService> _mockLoggingService;
        private readonly ShareService _shareService;

        public ShareServiceUnitTests()
        {
            _mockExternalShareRepo = new Mock<IExternalShareRepository>();
            _mockShareTokenRepo = new Mock<IShareTokenRepository>();
            _mockShareAccessLogRepo = new Mock<IShareAccessLogRepository>();
            _mockFileRepo = new Mock<IFileDocumentRepository>();
            _mockFolderRepo = new Mock<IFolderRepository>();
            _mockFilePermissionRepo = new Mock<IFilePermissionRepository>();
            _mockPermissionRepo = new Mock<IPermissionRepository>();
            _mockShareTokenService = new Mock<IShareTokenService>();
            _mockPermissionService = new Mock<IPermissionService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockAuditService = new Mock<IAuditService>();
            _mockLoggingService = new Mock<ILoggingService>();

            _shareService = new ShareService(
                _mockExternalShareRepo.Object,
                _mockShareTokenRepo.Object,
                _mockShareAccessLogRepo.Object,
                _mockFileRepo.Object,
                _mockFolderRepo.Object,
                _mockFilePermissionRepo.Object,
                _mockPermissionRepo.Object,
                _mockShareTokenService.Object,
                _mockPermissionService.Object,
                _mockNotificationService.Object,
                _mockAuditService.Object,
                _mockLoggingService.Object);
        }

        private UserContext MakeUser(int userId = 1, int role = 4)
        {
            return new UserContext
            {
                UserID = userId,
                ADUserName = $"user{userId}",
                AppRole = role,
                MachineName = "TEST-PC",
                LevelCode = "L1"
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // CreateInternalShare Tests (Requirement 1.1, 1.2, 1.3, 1.4, 15.1, 15.2)
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public void CreateInternalShare_WithValidInputs_CreatesShare()
        {
            // Arrange
            var user = MakeUser();
            var request = new CreateInternalShareRequest
            {
                FileID = 1,
                PrincipalType = 1,
                PrincipalID = 2,
                CanRead = true,
                CanWrite = false,
                CanDelete = false,
                ExpiryDate = null
            };

            var file = new FileDocument { FileID = 1, OriginalFileName = "test.pdf", ConfidentialityLevel = (int)ConfidentialityLevel.NC };
            _mockFileRepo.Setup(r => r.GetById(1)).Returns(file);
            _mockPermissionService.Setup(p => p.CheckPermission(user.UserID, 1, "Read")).Returns(true);
            _mockFilePermissionRepo.Setup(f => f.Insert(It.IsAny<FilePermission>())).Returns(1);

            // Act
            var result = _shareService.CreateInternalShare(request, user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.FileID);
            Assert.Equal(1, result.PrincipalType);
            Assert.Equal(2, result.PrincipalID);
            Assert.True(result.CanRead);
            Assert.False(result.CanWrite);
            _mockFilePermissionRepo.Verify(f => f.Insert(It.IsAny<FilePermission>()), Times.Once);
            _mockAuditService.Verify(a => a.Log(It.IsAny<AuditEntry>()), Times.Once);
            _mockNotificationService.Verify(n => n.Send(It.IsAny<NotificationRequest>()), Times.Once);
        }

        [Fact]
        public void CreateInternalShare_WithoutReadPermission_ThrowsException()
        {
            // Arrange
            var user = MakeUser();
            var request = new CreateInternalShareRequest
            {
                FileID = 1,
                PrincipalType = 1,
                PrincipalID = 2,
                CanRead = true,
                CanWrite = false,
                CanDelete = false
            };

            _mockPermissionService.Setup(p => p.CheckPermission(user.UserID, 1, "Read")).Returns(false);

            // Act & Assert
            var ex = Assert.Throws<UnauthorizedAccessException>(() => _shareService.CreateInternalShare(request, user));
            Assert.Contains("Insufficient permissions", ex.Message);
        }

        [Fact]
        public void CreateInternalShare_WithExpiryDate_RecordsExpiry()
        {
            // Arrange
            var user = MakeUser();
            var expiryDate = DateTime.Now.AddDays(30);
            var request = new CreateInternalShareRequest
            {
                FileID = 1,
                PrincipalType = 1,
                PrincipalID = 2,
                CanRead = true,
                CanWrite = false,
                CanDelete = false,
                ExpiryDate = expiryDate
            };

            var file = new FileDocument { FileID = 1, OriginalFileName = "test.pdf", ConfidentialityLevel = (int)ConfidentialityLevel.NC };
            _mockFileRepo.Setup(r => r.GetById(1)).Returns(file);
            _mockPermissionService.Setup(p => p.CheckPermission(user.UserID, 1, "Read")).Returns(true);
            _mockFilePermissionRepo.Setup(f => f.Insert(It.IsAny<FilePermission>())).Returns(1);

            // Act
            var result = _shareService.CreateInternalShare(request, user);

            // Assert
            Assert.Equal(expiryDate, result.ExpiryDate);
        }

        [Fact]
        public void CreateInternalShare_WithDifferentPrincipalTypes_CreatesShare()
        {
            // Arrange
            var user = MakeUser();
            var file = new FileDocument { FileID = 1, OriginalFileName = "test.pdf", ConfidentialityLevel = (int)ConfidentialityLevel.NC };
            _mockFileRepo.Setup(r => r.GetById(1)).Returns(file);
            _mockPermissionService.Setup(p => p.CheckPermission(user.UserID, 1, "Read")).Returns(true);
            _mockFilePermissionRepo.Setup(f => f.Insert(It.IsAny<FilePermission>())).Returns(1);

            // Test each principal type: 1=User, 2=Role, 3=OrgUnit, 4=Group
            for (int principalType = 1; principalType <= 4; principalType++)
            {
                var request = new CreateInternalShareRequest
                {
                    FileID = 1,
                    PrincipalType = principalType,
                    PrincipalID = 100 + principalType,
                    CanRead = true,
                    CanWrite = false,
                    CanDelete = false
                };

                // Act
                var result = _shareService.CreateInternalShare(request, user);

                // Assert
                Assert.Equal(principalType, result.PrincipalType);
                Assert.Equal(100 + principalType, result.PrincipalID);
            }
        }

        [Fact]
        public void RevokeInternalShare_WithValidShare_RevokesPermission()
        {
            // Arrange
            var user = MakeUser();
            var permission = new FilePermission
            {
                FilePermissionID = 1,
                FileID = 1,
                GrantedBy = user.UserID,
                GrantedDate = DateTime.Now
            };

            _mockFilePermissionRepo.Setup(r => r.GetById(1)).Returns(permission);
            _mockFilePermissionRepo.Setup(r => r.Revoke(1, user.UserID, It.IsAny<string>()));

            // Act
            _shareService.RevokeInternalShare(1, user);

            // Assert
            _mockFilePermissionRepo.Verify(r => r.Revoke(1, user.UserID, It.IsAny<string>()), Times.Once);
            _mockAuditService.Verify(a => a.Log(It.IsAny<AuditEntry>()), Times.Once);
        }

        [Fact]
        public void RevokeInternalShare_ByNonSharer_ThrowsException()
        {
            // Arrange
            var user = MakeUser(userId: 1, role: 1); // Non-admin user
            var permission = new FilePermission
            {
                FilePermissionID = 1,
                FileID = 1,
                GrantedBy = 2, // Different user
                GrantedDate = DateTime.Now
            };

            _mockFilePermissionRepo.Setup(r => r.GetById(1)).Returns(permission);

            // Act & Assert
            var ex = Assert.Throws<UnauthorizedAccessException>(() => _shareService.RevokeInternalShare(1, user));
            Assert.Contains("Only the original Sharer", ex.Message);
        }

        [Fact]
        public void RevokeInternalShare_ByAdmin_Succeeds()
        {
            // Arrange
            var admin = MakeUser(userId: 1, role: 4); // Admin role
            var permission = new FilePermission
            {
                FilePermissionID = 1,
                FileID = 1,
                GrantedBy = 2, // Different user
                GrantedDate = DateTime.Now
            };

            _mockFilePermissionRepo.Setup(r => r.GetById(1)).Returns(permission);
            _mockFilePermissionRepo.Setup(r => r.Revoke(1, admin.UserID, It.IsAny<string>()));

            // Act
            _shareService.RevokeInternalShare(1, admin);

            // Assert
            _mockFilePermissionRepo.Verify(r => r.Revoke(1, admin.UserID, It.IsAny<string>()), Times.Once);
        }

        // ─────────────────────────────────────────────────────────────────────
        // CreateExternalShare Tests (Requirement 2.1, 2.2, 2.3, 2.4, 3.1, 3.2, 4.1, 4.2, 7.1, 15.1, 15.2)
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public void CreateExternalShare_WithValidInputs_CreatesShareInPendingStatus()
        {
            // Arrange
            var user = MakeUser();
            var token = Guid.NewGuid();
            var request = new CreateExternalShareRequest
            {
                FileID = 1,
                FolderID = null,
                Password = "test123",
                ExpiryDate = DateTime.Now.AddDays(30),
                DownloadLimit = 10,
                SharePurpose = "Testing",
                NotifyOnAccess = true
            };

            var file = new FileDocument { FileID = 1, OriginalFileName = "test.pdf", ConfidentialityLevel = (int)ConfidentialityLevel.NC };
            _mockFileRepo.Setup(r => r.GetById(1)).Returns(file);
            _mockPermissionService.Setup(p => p.CheckPermission(user.UserID, 1, "Read")).Returns(true);
            _mockShareTokenService.Setup(s => s.GenerateToken()).Returns(token);
            _mockShareTokenService.Setup(s => s.HashPassword("test123")).Returns("hashed_password");
            _mockExternalShareRepo.Setup(r => r.Insert(It.IsAny<ExternalShare>())).Returns(1);
            _mockShareTokenRepo.Setup(r => r.Insert(It.IsAny<ShareToken>())).Returns(1);

            // Act
            var result = _shareService.CreateExternalShare(request, user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal("Pending", result.ShareTokenStatus);
            Assert.Equal(token, result.ShareToken);
            Assert.Equal(user.UserID, result.SharerUserID);
            Assert.Equal(10, result.DownloadLimit);
            Assert.Equal(0, result.CurrentDownloadCount);
            _mockExternalShareRepo.Verify(r => r.Insert(It.IsAny<ExternalShare>()), Times.Once);
            _mockShareTokenRepo.Verify(r => r.Insert(It.IsAny<ShareToken>()), Times.Once);
            _mockAuditService.Verify(a => a.Log(It.IsAny<AuditEntry>()), Times.Once);
            _mockNotificationService.Verify(n => n.Send(It.IsAny<NotificationRequest>()), Times.Once);
        }

        [Fact]
        public void CreateExternalShare_WithValidFolder_CreatesShare()
        {
            // Arrange
            var user = MakeUser();
            var token = Guid.NewGuid();
            var request = new CreateExternalShareRequest
            {
                FileID = null,
                FolderID = 1,
                Password = null,
                ExpiryDate = DateTime.Now.AddDays(30),
                DownloadLimit = null,
                SharePurpose = "Testing",
                NotifyOnAccess = true
            };

            var folder = new Folder { FolderID = 1, FolderName = "TestFolder", ConfidentialityLevel = (int)ConfidentialityLevel.NC };
            _mockFolderRepo.Setup(r => r.GetById(1)).Returns(folder);
            _mockFileRepo.Setup(r => r.GetByFolder(1)).Returns(new List<FileDocument>());
            _mockPermissionService.Setup(p => p.CheckPermission(user.UserID, 1, "Read")).Returns(true);
            _mockShareTokenService.Setup(s => s.GenerateToken()).Returns(token);
            _mockExternalShareRepo.Setup(r => r.Insert(It.IsAny<ExternalShare>())).Returns(1);
            _mockShareTokenRepo.Setup(r => r.Insert(It.IsAny<ShareToken>())).Returns(1);

            // Act
            var result = _shareService.CreateExternalShare(request, user);

            // Assert
            Assert.NotNull(result);
            Assert.Equal(1, result.FolderID);
            Assert.Null(result.FileID);
        }

        [Fact]
        public void CreateExternalShare_WithHighlyConfidentialFile_ThrowsException()
        {
            // Arrange
            var user = MakeUser();
            var request = new CreateExternalShareRequest
            {
                FileID = 1,
                FolderID = null,
                Password = null,
                ExpiryDate = DateTime.Now.AddDays(30),
                DownloadLimit = 10,
                SharePurpose = "Testing",
                NotifyOnAccess = true
            };

            var file = new FileDocument { FileID = 1, OriginalFileName = "test.pdf", ConfidentialityLevel = (int)ConfidentialityLevel.HC };
            _mockFileRepo.Setup(r => r.GetById(1)).Returns(file);
            _mockPermissionService.Setup(p => p.CheckPermission(user.UserID, 1, "Read")).Returns(true);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _shareService.CreateExternalShare(request, user));
            Assert.Contains("Highly Confidential", ex.Message);
        }

        [Fact]
        public void CreateExternalShare_WithFolderContainingHCFile_ThrowsException()
        {
            // Arrange
            var user = MakeUser();
            var request = new CreateExternalShareRequest
            {
                FileID = null,
                FolderID = 1,
                Password = null,
                ExpiryDate = DateTime.Now.AddDays(30),
                DownloadLimit = 10,
                SharePurpose = "Testing",
                NotifyOnAccess = true
            };

            var folder = new Folder { FolderID = 1, FolderName = "TestFolder", ConfidentialityLevel = (int)ConfidentialityLevel.NC };
            var hcFile = new FileDocument { FileID = 1, OriginalFileName = "secret.pdf", ConfidentialityLevel = (int)ConfidentialityLevel.HC };
            _mockFolderRepo.Setup(r => r.GetById(1)).Returns(folder);
            _mockFileRepo.Setup(r => r.GetByFolder(1)).Returns(new List<FileDocument> { hcFile });
            _mockPermissionService.Setup(p => p.CheckPermission(user.UserID, 1, "Read")).Returns(true);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _shareService.CreateExternalShare(request, user));
            Assert.Contains("restricted content", ex.Message);
        }

        [Fact]
        public void CreateExternalShare_WithExpiryOver90Days_ThrowsException()
        {
            // Arrange
            var user = MakeUser();
            var request = new CreateExternalShareRequest
            {
                FileID = 1,
                FolderID = null,
                Password = null,
                ExpiryDate = DateTime.Now.AddDays(100),
                DownloadLimit = 10,
                SharePurpose = "Testing",
                NotifyOnAccess = true
            };

            var file = new FileDocument { FileID = 1, OriginalFileName = "test.pdf", ConfidentialityLevel = (int)ConfidentialityLevel.NC };
            _mockFileRepo.Setup(r => r.GetById(1)).Returns(file);
            _mockPermissionService.Setup(p => p.CheckPermission(user.UserID, 1, "Read")).Returns(true);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _shareService.CreateExternalShare(request, user));
            Assert.Contains("90 days", ex.Message);
        }

        [Fact]
        public void CreateExternalShare_WithoutReadPermission_ThrowsException()
        {
            // Arrange
            var user = MakeUser();
            var request = new CreateExternalShareRequest
            {
                FileID = 1,
                FolderID = null,
                Password = null,
                ExpiryDate = DateTime.Now.AddDays(30),
                DownloadLimit = 10,
                SharePurpose = "Testing",
                NotifyOnAccess = true
            };

            _mockPermissionService.Setup(p => p.CheckPermission(user.UserID, 1, "Read")).Returns(false);

            // Act & Assert
            var ex = Assert.Throws<UnauthorizedAccessException>(() => _shareService.CreateExternalShare(request, user));
            Assert.Contains("Insufficient permissions", ex.Message);
        }

        [Fact]
        public void CreateExternalShare_WithPassword_HashesPassword()
        {
            // Arrange
            var user = MakeUser();
            var token = Guid.NewGuid();
            var request = new CreateExternalShareRequest
            {
                FileID = 1,
                FolderID = null,
                Password = "SecurePassword123",
                ExpiryDate = DateTime.Now.AddDays(30),
                DownloadLimit = 10,
                SharePurpose = "Testing",
                NotifyOnAccess = true
            };

            var file = new FileDocument { FileID = 1, OriginalFileName = "test.pdf", ConfidentialityLevel = (int)ConfidentialityLevel.NC };
            _mockFileRepo.Setup(r => r.GetById(1)).Returns(file);
            _mockPermissionService.Setup(p => p.CheckPermission(user.UserID, 1, "Read")).Returns(true);
            _mockShareTokenService.Setup(s => s.GenerateToken()).Returns(token);
            _mockShareTokenService.Setup(s => s.HashPassword("SecurePassword123")).Returns("$2a$12$hashed");
            _mockExternalShareRepo.Setup(r => r.Insert(It.IsAny<ExternalShare>())).Returns(1);
            _mockShareTokenRepo.Setup(r => r.Insert(It.IsAny<ShareToken>())).Returns(1);

            // Act
            var result = _shareService.CreateExternalShare(request, user);

            // Assert
            Assert.Equal("$2a$12$hashed", result.PasswordHash);
            _mockShareTokenService.Verify(s => s.HashPassword("SecurePassword123"), Times.Once);
        }

        [Fact]
        public void CreateExternalShare_WithoutPassword_NoHashGenerated()
        {
            // Arrange
            var user = MakeUser();
            var token = Guid.NewGuid();
            var request = new CreateExternalShareRequest
            {
                FileID = 1,
                FolderID = null,
                Password = null,
                ExpiryDate = DateTime.Now.AddDays(30),
                DownloadLimit = 10,
                SharePurpose = "Testing",
                NotifyOnAccess = true
            };

            var file = new FileDocument { FileID = 1, OriginalFileName = "test.pdf", ConfidentialityLevel = (int)ConfidentialityLevel.NC };
            _mockFileRepo.Setup(r => r.GetById(1)).Returns(file);
            _mockPermissionService.Setup(p => p.CheckPermission(user.UserID, 1, "Read")).Returns(true);
            _mockShareTokenService.Setup(s => s.GenerateToken()).Returns(token);
            _mockExternalShareRepo.Setup(r => r.Insert(It.IsAny<ExternalShare>())).Returns(1);
            _mockShareTokenRepo.Setup(r => r.Insert(It.IsAny<ShareToken>())).Returns(1);

            // Act
            var result = _shareService.CreateExternalShare(request, user);

            // Assert
            Assert.Null(result.PasswordHash);
            _mockShareTokenService.Verify(s => s.HashPassword(It.IsAny<string>()), Times.Never);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ApproveExternalShare Tests (Requirement 5.1, 5.2, 5.3)
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public void ApproveExternalShare_TransitionsStatusToActive()
        {
            // Arrange
            var manager = MakeUser(userId: 2, role: 2); // Manager
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareTokenStatus = "Pending",
                SharerUserID = 1,
                CreatedDate = DateTime.Now
            };

            var token = new ShareToken
            {
                ShareTokenID = 1,
                ExternalShareID = 1,
                Status = "Pending"
            };

            _mockExternalShareRepo.Setup(r => r.GetById(1)).Returns(share);
            _mockExternalShareRepo.Setup(r => r.Update(It.IsAny<ExternalShare>()));
            _mockShareTokenRepo.Setup(r => r.GetByExternalShare(1)).Returns(new List<ShareToken> { token });
            _mockShareTokenRepo.Setup(r => r.Update(It.IsAny<ShareToken>()));

            // Act
            _shareService.ApproveExternalShare(1, manager);

            // Assert
            _mockExternalShareRepo.Verify(r => r.Update(It.Is<ExternalShare>(s => 
                s.ShareTokenStatus == "Active" && 
                s.ApproverUserID == manager.UserID && 
                s.ApprovedDate != null)), Times.Once);
            _mockShareTokenRepo.Verify(r => r.Update(It.Is<ShareToken>(t => 
                t.Status == "Active" && 
                t.ActivatedDate != null)), Times.Once);
            _mockAuditService.Verify(a => a.Log(It.IsAny<AuditEntry>()), Times.Once);
            _mockNotificationService.Verify(n => n.Send(It.IsAny<NotificationRequest>()), Times.Once);
        }

        [Fact]
        public void ApproveExternalShare_WithNonPendingShare_ThrowsException()
        {
            // Arrange
            var manager = MakeUser(userId: 2, role: 2);
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareTokenStatus = "Active",
                SharerUserID = 1
            };

            _mockExternalShareRepo.Setup(r => r.GetById(1)).Returns(share);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _shareService.ApproveExternalShare(1, manager));
            Assert.Contains("Only Pending shares", ex.Message);
        }

        // ─────────────────────────────────────────────────────────────────────
        // RejectExternalShare Tests (Requirement 5.1, 5.2, 5.4)
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public void RejectExternalShare_TransitionsStatusToRevoked()
        {
            // Arrange
            var manager = MakeUser(userId: 2, role: 2);
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareTokenStatus = "Pending",
                SharerUserID = 1,
                CreatedDate = DateTime.Now
            };

            var token = new ShareToken
            {
                ShareTokenID = 1,
                ExternalShareID = 1,
                Status = "Pending"
            };

            _mockExternalShareRepo.Setup(r => r.GetById(1)).Returns(share);
            _mockExternalShareRepo.Setup(r => r.Update(It.IsAny<ExternalShare>()));
            _mockShareTokenRepo.Setup(r => r.GetByExternalShare(1)).Returns(new List<ShareToken> { token });
            _mockShareTokenRepo.Setup(r => r.Update(It.IsAny<ShareToken>()));

            // Act
            _shareService.RejectExternalShare(1, "Not approved", manager);

            // Assert
            _mockExternalShareRepo.Verify(r => r.Update(It.Is<ExternalShare>(s => 
                s.ShareTokenStatus == "Revoked" && 
                s.RevocationReason == "Not approved" &&
                s.RevokedBy == manager.UserID)), Times.Once);
            _mockAuditService.Verify(a => a.Log(It.IsAny<AuditEntry>()), Times.Once);
            _mockNotificationService.Verify(n => n.Send(It.IsAny<NotificationRequest>()), Times.Once);
        }

        [Fact]
        public void RejectExternalShare_WithNonPendingShare_ThrowsException()
        {
            // Arrange
            var manager = MakeUser(userId: 2, role: 2);
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareTokenStatus = "Active",
                SharerUserID = 1
            };

            _mockExternalShareRepo.Setup(r => r.GetById(1)).Returns(share);

            // Act & Assert
            var ex = Assert.Throws<InvalidOperationException>(() => _shareService.RejectExternalShare(1, "Not approved", manager));
            Assert.Contains("Only Pending shares", ex.Message);
        }

        // ─────────────────────────────────────────────────────────────────────
        // RevokeExternalShare Tests (Requirement 14.1, 14.2, 15.3, 15.4)
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public void RevokeExternalShare_BySharer_TransitionsToRevoked()
        {
            // Arrange
            var user = MakeUser(userId: 1);
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareTokenStatus = "Active",
                SharerUserID = user.UserID,
                CreatedDate = DateTime.Now
            };

            var token = new ShareToken
            {
                ShareTokenID = 1,
                ExternalShareID = 1,
                Status = "Active"
            };

            _mockExternalShareRepo.Setup(r => r.GetById(1)).Returns(share);
            _mockExternalShareRepo.Setup(r => r.Update(It.IsAny<ExternalShare>()));
            _mockShareTokenRepo.Setup(r => r.GetByExternalShare(1)).Returns(new List<ShareToken> { token });
            _mockShareTokenRepo.Setup(r => r.Update(It.IsAny<ShareToken>()));

            // Act
            _shareService.RevokeExternalShare(1, "User revoked", user);

            // Assert
            _mockExternalShareRepo.Verify(r => r.Update(It.Is<ExternalShare>(s => 
                s.ShareTokenStatus == "Revoked" && 
                s.RevokedBy == user.UserID)), Times.Once);
            _mockAuditService.Verify(a => a.Log(It.IsAny<AuditEntry>()), Times.Once);
        }

        [Fact]
        public void RevokeExternalShare_ByNonSharer_ThrowsException()
        {
            // Arrange
            var user = MakeUser(userId: 1, role: 1); // Non-admin user
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareTokenStatus = "Active",
                SharerUserID = 2, // Different user
                CreatedDate = DateTime.Now
            };

            _mockExternalShareRepo.Setup(r => r.GetById(1)).Returns(share);

            // Act & Assert
            var ex = Assert.Throws<UnauthorizedAccessException>(() => _shareService.RevokeExternalShare(1, "User revoked", user));
            Assert.Contains("Only the original Sharer", ex.Message);
        }

        [Fact]
        public void RevokeExternalShare_ByAdmin_Succeeds()
        {
            // Arrange
            var admin = MakeUser(userId: 1, role: 4);
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareTokenStatus = "Active",
                SharerUserID = 2, // Different user
                CreatedDate = DateTime.Now
            };

            var token = new ShareToken
            {
                ShareTokenID = 1,
                ExternalShareID = 1,
                Status = "Active"
            };

            _mockExternalShareRepo.Setup(r => r.GetById(1)).Returns(share);
            _mockExternalShareRepo.Setup(r => r.Update(It.IsAny<ExternalShare>()));
            _mockShareTokenRepo.Setup(r => r.GetByExternalShare(1)).Returns(new List<ShareToken> { token });
            _mockShareTokenRepo.Setup(r => r.Update(It.IsAny<ShareToken>()));

            // Act
            _shareService.RevokeExternalShare(1, "Admin revoked", admin);

            // Assert
            _mockExternalShareRepo.Verify(r => r.Update(It.Is<ExternalShare>(s => s.ShareTokenStatus == "Revoked")), Times.Once);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ModifyExternalShare Tests
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public void ModifyExternalShare_WithValidShare_UpdatesMetadata()
        {
            // Arrange
            var user = MakeUser();
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareTokenStatus = "Active",
                SharerUserID = user.UserID,
                ExpiryDate = DateTime.Now.AddDays(30),
                DownloadLimit = 10
            };

            var request = new ModifyExternalShareRequest
            {
                ExternalShareID = 1,
                NewExpiryDate = DateTime.Now.AddDays(60),
                NewDownloadLimit = 20,
                NewPassword = null
            };

            _mockExternalShareRepo.Setup(r => r.GetById(1)).Returns(share);
            _mockExternalShareRepo.Setup(r => r.Update(It.IsAny<ExternalShare>()));

            // Act
            _shareService.ModifyExternalShare(request, user);

            // Assert
            _mockExternalShareRepo.Verify(r => r.Update(It.Is<ExternalShare>(s => s.DownloadLimit == 20)), Times.Once);
            _mockAuditService.Verify(a => a.Log(It.IsAny<AuditEntry>()), Times.Once);
        }

        [Fact]
        public void ModifyExternalShare_WithExpiredShare_ThrowsException()
        {
            // Arrange
            var user = MakeUser();
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareTokenStatus = "Expired",
                SharerUserID = user.UserID
            };

            var request = new ModifyExternalShareRequest
            {
                ExternalShareID = 1,
                NewExpiryDate = DateTime.Now.AddDays(60),
                NewDownloadLimit = 20,
                NewPassword = null
            };

            _mockExternalShareRepo.Setup(r => r.GetById(1)).Returns(share);

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() => _shareService.ModifyExternalShare(request, user));
        }

        // ─────────────────────────────────────────────────────────────────────
        // Query Tests
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public void GetExternalSharesByStatus_FiltersCorrectly()
        {
            // Arrange
            var user = MakeUser();
            var shares = new List<ExternalShare>
            {
                new ExternalShare { ExternalShareID = 1, ShareTokenStatus = "Active", SharerUserID = user.UserID },
                new ExternalShare { ExternalShareID = 2, ShareTokenStatus = "Pending", SharerUserID = user.UserID },
                new ExternalShare { ExternalShareID = 3, ShareTokenStatus = "Active", SharerUserID = user.UserID }
            };

            _mockExternalShareRepo.Setup(r => r.GetByStatus("Active")).Returns(shares.Where(s => s.ShareTokenStatus == "Active"));

            // Act
            var result = _shareService.GetExternalSharesByStatus("Active", user);

            // Assert
            Assert.Equal(2, result.Count());
        }

        // ─────────────────────────────────────────────────────────────────────
        // Additional Tests for Task 6.1
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public void RevokeExternalShare_PreventsAccess()
        {
            // Arrange
            var user = MakeUser(userId: 1);
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareToken = Guid.NewGuid(),
                ShareTokenStatus = "Active",
                SharerUserID = user.UserID,
                CreatedDate = DateTime.Now
            };

            var token = new ShareToken
            {
                ShareTokenID = 1,
                ExternalShareID = 1,
                Status = "Active"
            };

            _mockExternalShareRepo.Setup(r => r.GetById(1)).Returns(share);
            _mockExternalShareRepo.Setup(r => r.Update(It.IsAny<ExternalShare>()));
            _mockShareTokenRepo.Setup(r => r.GetByExternalShare(1)).Returns(new List<ShareToken> { token });
            _mockShareTokenRepo.Setup(r => r.Update(It.IsAny<ShareToken>()));

            // Act
            _shareService.RevokeExternalShare(1, "User revoked", user);

            // Assert - Verify share is revoked
            _mockExternalShareRepo.Verify(r => r.Update(It.Is<ExternalShare>(s => 
                s.ShareTokenStatus == "Revoked" && 
                s.RevokedBy == user.UserID)), Times.Once);

            // Verify token is also revoked
            _mockShareTokenRepo.Verify(r => r.Update(It.Is<ShareToken>(t => 
                t.Status == "Revoked")), Times.Once);

            // Verify audit log created
            _mockAuditService.Verify(a => a.Log(It.IsAny<AuditEntry>()), Times.Once);

            // Verify notification sent
            _mockNotificationService.Verify(n => n.Send(It.IsAny<NotificationRequest>()), Times.Once);
        }

        [Fact]
        public void GetExternalSharesForUser_ReturnsCorrectShares()
        {
            // Arrange
            var user = MakeUser(userId: 1);
            var otherUser = MakeUser(userId: 2);
            var shares = new List<ExternalShare>
            {
                new ExternalShare 
                { 
                    ExternalShareID = 1, 
                    ShareTokenStatus = "Active", 
                    SharerUserID = user.UserID,
                    FileID = 1,
                    CreatedDate = DateTime.Now.AddDays(-5)
                },
                new ExternalShare 
                { 
                    ExternalShareID = 2, 
                    ShareTokenStatus = "Pending", 
                    SharerUserID = user.UserID,
                    FileID = 2,
                    CreatedDate = DateTime.Now.AddDays(-3)
                },
                new ExternalShare 
                { 
                    ExternalShareID = 3, 
                    ShareTokenStatus = "Active", 
                    SharerUserID = otherUser.UserID,
                    FileID = 3,
                    CreatedDate = DateTime.Now.AddDays(-1)
                }
            };

            _mockExternalShareRepo.Setup(r => r.GetBySharer(user.UserID)).Returns(
                shares.Where(s => s.SharerUserID == user.UserID)
            );

            // Act
            var result = _shareService.GetExternalSharesForUser(user.UserID, user);

            // Assert
            Assert.NotNull(result);
            var resultList = result.ToList();
            Assert.Equal(2, resultList.Count);
            Assert.All(resultList, s => Assert.Equal(user.UserID, s.SharerUserID));
            Assert.Contains(resultList, s => s.ExternalShareID == 1);
            Assert.Contains(resultList, s => s.ExternalShareID == 2);
            Assert.DoesNotContain(resultList, s => s.ExternalShareID == 3);
        }
    }
}
