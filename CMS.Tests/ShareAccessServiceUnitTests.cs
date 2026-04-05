using System;
using System.Collections.Generic;
using Xunit;
using Moq;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;
using CMS.Infrastructure.Interfaces;

namespace CMS.Tests
{
    /// <summary>
    /// Unit tests for ShareAccessService methods.
    /// Satisfies Requirements 6.1, 6.2, 8.2, 8.3, 8.4, 9.1, 9.2, 9.3
    /// </summary>
    public class ShareAccessServiceUnitTests
    {
        private readonly Mock<IExternalShareRepository> _mockExternalShareRepo;
        private readonly Mock<IShareAccessLogRepository> _mockShareAccessLogRepo;
        private readonly Mock<IShareTokenService> _mockShareTokenService;
        private readonly Mock<IFileDocumentRepository> _mockFileDocumentRepo;
        private readonly Mock<IFolderRepository> _mockFolderRepo;
        private readonly Mock<IAuditService> _mockAuditService;
        private readonly Mock<INotificationService> _mockNotificationService;
        private readonly Mock<ILoggingService> _mockLoggingService;
        private readonly ShareAccessService _shareAccessService;

        public ShareAccessServiceUnitTests()
        {
            _mockExternalShareRepo = new Mock<IExternalShareRepository>();
            _mockShareAccessLogRepo = new Mock<IShareAccessLogRepository>();
            _mockShareTokenService = new Mock<IShareTokenService>();
            _mockFileDocumentRepo = new Mock<IFileDocumentRepository>();
            _mockFolderRepo = new Mock<IFolderRepository>();
            _mockAuditService = new Mock<IAuditService>();
            _mockNotificationService = new Mock<INotificationService>();
            _mockLoggingService = new Mock<ILoggingService>();

            _shareAccessService = new ShareAccessService(
                _mockExternalShareRepo.Object,
                _mockShareAccessLogRepo.Object,
                _mockShareTokenService.Object,
                _mockFileDocumentRepo.Object,
                _mockFolderRepo.Object,
                _mockAuditService.Object,
                _mockNotificationService.Object,
                _mockLoggingService.Object);
        }

        private UserContext MakeUser(int userId = 1, string userName = "testuser")
        {
            return new UserContext
            {
                UserID = userId,
                ADUserName = userName,
                AppRole = 4,
                MachineName = "TEST-PC",
                LevelCode = "L1",
                IPAddress = "192.168.1.1"
            };
        }

        // ─────────────────────────────────────────────────────────────────────
        // ValidateAndGrantAccess Tests (Requirement 6.1, 6.2, 6.3, 6.4, 6.5)
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public void ValidateAndGrantAccess_WithActiveShare_GrantsAccess()
        {
            // Arrange
            var token = Guid.NewGuid();
            var user = MakeUser();
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareToken = token,
                FileID = 1,
                FolderID = null,
                SharerUserID = 2,
                PasswordHash = null,
                ExpiryDate = DateTime.Now.AddDays(10),
                DownloadLimit = 5,
                CurrentDownloadCount = 0,
                ShareTokenStatus = "Active",
                CreatedDate = DateTime.Now,
                NotifyOnAccess = true
            };

            var file = new FileDocument
            {
                FileID = 1,
                OriginalFileName = "test.pdf",
                FileSizeBytes = 1024
            };

            _mockExternalShareRepo.Setup(r => r.GetByToken(token)).Returns(share);
            _mockExternalShareRepo.Setup(r => r.GetById(1)).Returns(share);
            _mockFileDocumentRepo.Setup(r => r.GetById(1)).Returns(file);
            _mockExternalShareRepo.Setup(r => r.Update(It.IsAny<ExternalShare>())).Callback<ExternalShare>(s => share = s);
            _mockShareAccessLogRepo.Setup(r => r.Insert(It.IsAny<ShareAccessLog>())).Returns(1);

            // Act
            var result = _shareAccessService.ValidateAndGrantAccess(token, null, user);

            // Assert
            Assert.True(result.IsGranted);
            Assert.Null(result.DenyReason);
            Assert.NotNull(result.File);
            Assert.Equal("test.pdf", result.File.OriginalFileName);
            Assert.Equal(3, result.RemainingDownloads); // 5 - 1 (after increment) - 1 = 3
            _mockShareAccessLogRepo.Verify(r => r.Insert(It.Is<ShareAccessLog>(
                l => l.IsSuccessful && l.ShareToken == token)), Times.Once);
        }

        [Fact]
        public void ValidateAndGrantAccess_WithExpiredShare_DeniesAccess()
        {
            // Arrange
            var token = Guid.NewGuid();
            var user = MakeUser();
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareToken = token,
                FileID = 1,
                SharerUserID = 2,
                PasswordHash = null,
                ExpiryDate = DateTime.Now.AddDays(-1), // Expired
                DownloadLimit = 5,
                CurrentDownloadCount = 0,
                ShareTokenStatus = "Active",
                CreatedDate = DateTime.Now.AddDays(-10),
                NotifyOnAccess = true
            };

            _mockExternalShareRepo.Setup(r => r.GetByToken(token)).Returns(share);
            _mockShareAccessLogRepo.Setup(r => r.Insert(It.IsAny<ShareAccessLog>())).Returns(1);

            // Act
            var result = _shareAccessService.ValidateAndGrantAccess(token, null, user);

            // Assert
            Assert.False(result.IsGranted);
            Assert.Equal("Link expired.", result.DenyReason);
            _mockShareAccessLogRepo.Verify(r => r.Insert(It.Is<ShareAccessLog>(
                l => !l.IsSuccessful && l.FailureReason == "Expired")), Times.Once);
        }

        [Fact]
        public void ValidateAndGrantAccess_WithDisabledShare_DeniesAccess()
        {
            // Arrange
            var token = Guid.NewGuid();
            var user = MakeUser();
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareToken = token,
                FileID = 1,
                SharerUserID = 2,
                PasswordHash = null,
                ExpiryDate = DateTime.Now.AddDays(10),
                DownloadLimit = 5,
                CurrentDownloadCount = 0,
                ShareTokenStatus = "Disabled",
                CreatedDate = DateTime.Now,
                DisabledDate = DateTime.Now,
                DisabledReason = "Download limit reached",
                NotifyOnAccess = true
            };

            _mockExternalShareRepo.Setup(r => r.GetByToken(token)).Returns(share);
            _mockShareAccessLogRepo.Setup(r => r.Insert(It.IsAny<ShareAccessLog>())).Returns(1);

            // Act
            var result = _shareAccessService.ValidateAndGrantAccess(token, null, user);

            // Assert
            Assert.False(result.IsGranted);
            Assert.Equal("Link disabled.", result.DenyReason);
            _mockShareAccessLogRepo.Verify(r => r.Insert(It.Is<ShareAccessLog>(
                l => !l.IsSuccessful && l.FailureReason == "Disabled")), Times.Once);
        }

        [Fact]
        public void ValidateAndGrantAccess_WithRevokedShare_DeniesAccess()
        {
            // Arrange
            var token = Guid.NewGuid();
            var user = MakeUser();
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareToken = token,
                FileID = 1,
                SharerUserID = 2,
                PasswordHash = null,
                ExpiryDate = DateTime.Now.AddDays(10),
                DownloadLimit = 5,
                CurrentDownloadCount = 0,
                ShareTokenStatus = "Revoked",
                CreatedDate = DateTime.Now,
                RevokedBy = 2,
                RevokedDate = DateTime.Now,
                RevocationReason = "User requested revocation",
                NotifyOnAccess = true
            };

            _mockExternalShareRepo.Setup(r => r.GetByToken(token)).Returns(share);
            _mockShareAccessLogRepo.Setup(r => r.Insert(It.IsAny<ShareAccessLog>())).Returns(1);

            // Act
            var result = _shareAccessService.ValidateAndGrantAccess(token, null, user);

            // Assert
            Assert.False(result.IsGranted);
            Assert.Equal("Link revoked.", result.DenyReason);
            _mockShareAccessLogRepo.Verify(r => r.Insert(It.Is<ShareAccessLog>(
                l => !l.IsSuccessful && l.FailureReason == "Revoked")), Times.Once);
        }

        [Fact]
        public void ValidateAndGrantAccess_WithPendingShare_DeniesAccess()
        {
            // Arrange
            var token = Guid.NewGuid();
            var user = MakeUser();
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareToken = token,
                FileID = 1,
                SharerUserID = 2,
                PasswordHash = null,
                ExpiryDate = DateTime.Now.AddDays(10),
                DownloadLimit = 5,
                CurrentDownloadCount = 0,
                ShareTokenStatus = "Pending",
                CreatedDate = DateTime.Now,
                NotifyOnAccess = true
            };

            _mockExternalShareRepo.Setup(r => r.GetByToken(token)).Returns(share);
            _mockShareAccessLogRepo.Setup(r => r.Insert(It.IsAny<ShareAccessLog>())).Returns(1);

            // Act
            var result = _shareAccessService.ValidateAndGrantAccess(token, null, user);

            // Assert
            Assert.False(result.IsGranted);
            Assert.Equal("Link pending approval.", result.DenyReason);
            _mockShareAccessLogRepo.Verify(r => r.Insert(It.Is<ShareAccessLog>(
                l => !l.IsSuccessful && l.FailureReason == "Pending")), Times.Once);
        }

        [Fact]
        public void ValidateAndGrantAccess_WithCorrectPassword_GrantsAccess()
        {
            // Arrange
            var token = Guid.NewGuid();
            var user = MakeUser();
            var password = "SecurePassword123";
            var passwordHash = "$2a$12$hashedpassword";

            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareToken = token,
                FileID = 1,
                SharerUserID = 2,
                PasswordHash = passwordHash,
                ExpiryDate = DateTime.Now.AddDays(10),
                DownloadLimit = 5,
                CurrentDownloadCount = 0,
                ShareTokenStatus = "Active",
                CreatedDate = DateTime.Now,
                NotifyOnAccess = true
            };

            var file = new FileDocument
            {
                FileID = 1,
                OriginalFileName = "test.pdf",
                FileSizeBytes = 1024
            };

            _mockExternalShareRepo.Setup(r => r.GetByToken(token)).Returns(share);
            _mockExternalShareRepo.Setup(r => r.GetById(1)).Returns(share);
            _mockShareTokenService.Setup(s => s.VerifyPassword(password, passwordHash)).Returns(true);
            _mockFileDocumentRepo.Setup(r => r.GetById(1)).Returns(file);
            _mockExternalShareRepo.Setup(r => r.Update(It.IsAny<ExternalShare>())).Callback<ExternalShare>(s => share = s);
            _mockShareAccessLogRepo.Setup(r => r.Insert(It.IsAny<ShareAccessLog>())).Returns(1);

            // Act
            var result = _shareAccessService.ValidateAndGrantAccess(token, password, user);

            // Assert
            Assert.True(result.IsGranted);
            Assert.Null(result.DenyReason);
            _mockShareTokenService.Verify(s => s.VerifyPassword(password, passwordHash), Times.Once);
        }

        [Fact]
        public void ValidateAndGrantAccess_WithIncorrectPassword_DeniesAccess()
        {
            // Arrange
            var token = Guid.NewGuid();
            var user = MakeUser();
            var password = "WrongPassword";
            var passwordHash = "$2a$12$hashedpassword";

            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareToken = token,
                FileID = 1,
                SharerUserID = 2,
                PasswordHash = passwordHash,
                ExpiryDate = DateTime.Now.AddDays(10),
                DownloadLimit = 5,
                CurrentDownloadCount = 0,
                ShareTokenStatus = "Active",
                CreatedDate = DateTime.Now,
                NotifyOnAccess = true
            };

            _mockExternalShareRepo.Setup(r => r.GetByToken(token)).Returns(share);
            _mockShareTokenService.Setup(s => s.VerifyPassword(password, passwordHash)).Returns(false);
            _mockShareAccessLogRepo.Setup(r => r.Insert(It.IsAny<ShareAccessLog>())).Returns(1);

            // Act
            var result = _shareAccessService.ValidateAndGrantAccess(token, password, user);

            // Assert
            Assert.False(result.IsGranted);
            Assert.Equal("Incorrect password.", result.DenyReason);
            _mockShareAccessLogRepo.Verify(r => r.Insert(It.Is<ShareAccessLog>(
                l => !l.IsSuccessful && l.FailureReason == "WrongPassword")), Times.Once);
        }

        [Fact]
        public void ValidateAndGrantAccess_WithDownloadLimitReached_DeniesAccess()
        {
            // Arrange
            var token = Guid.NewGuid();
            var user = MakeUser();
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareToken = token,
                FileID = 1,
                SharerUserID = 2,
                PasswordHash = null,
                ExpiryDate = DateTime.Now.AddDays(10),
                DownloadLimit = 5,
                CurrentDownloadCount = 5, // Already at limit
                ShareTokenStatus = "Active",
                CreatedDate = DateTime.Now,
                NotifyOnAccess = true
            };

            _mockExternalShareRepo.Setup(r => r.GetByToken(token)).Returns(share);
            _mockShareAccessLogRepo.Setup(r => r.Insert(It.IsAny<ShareAccessLog>())).Returns(1);

            // Act
            var result = _shareAccessService.ValidateAndGrantAccess(token, null, user);

            // Assert
            Assert.False(result.IsGranted);
            Assert.Equal("Download limit reached.", result.DenyReason);
            _mockShareAccessLogRepo.Verify(r => r.Insert(It.Is<ShareAccessLog>(
                l => !l.IsSuccessful && l.FailureReason == "LimitReached")), Times.Once);
        }

        // ─────────────────────────────────────────────────────────────────────
        // LogAccessAttempt Tests (Requirement 9.1, 9.2, 9.3)
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public void LogAccessAttempt_WithSuccessfulAccess_CreatesLog()
        {
            // Arrange
            var token = Guid.NewGuid();
            var log = new ShareAccessLog
            {
                ShareToken = token,
                ExternalShareID = 1,
                RecipientIdentifier = "testuser",
                AccessType = "Download",
                IsSuccessful = true,
                FailureReason = null,
                FileSizeBytes = 1024,
                AccessTimestamp = DateTime.Now,
                IPAddress = "192.168.1.1",
                UserAgent = null
            };

            _mockShareAccessLogRepo.Setup(r => r.Insert(It.IsAny<ShareAccessLog>())).Returns(1);

            // Act
            _shareAccessService.LogAccessAttempt(log);

            // Assert
            _mockShareAccessLogRepo.Verify(r => r.Insert(It.Is<ShareAccessLog>(
                l => l.IsSuccessful && l.ShareToken == token && l.ExternalShareID == 1)), Times.Once);
            _mockLoggingService.Verify(l => l.Info(It.IsAny<string>()), Times.Once);
        }

        [Fact]
        public void LogAccessAttempt_WithFailedAccess_CreatesLogWithReason()
        {
            // Arrange
            var token = Guid.NewGuid();
            var log = new ShareAccessLog
            {
                ShareToken = token,
                ExternalShareID = 1,
                RecipientIdentifier = "testuser",
                AccessType = "Download",
                IsSuccessful = false,
                FailureReason = "WrongPassword",
                FileSizeBytes = null,
                AccessTimestamp = DateTime.Now,
                IPAddress = "192.168.1.1",
                UserAgent = null
            };

            _mockShareAccessLogRepo.Setup(r => r.Insert(It.IsAny<ShareAccessLog>())).Returns(2);

            // Act
            _shareAccessService.LogAccessAttempt(log);

            // Assert
            _mockShareAccessLogRepo.Verify(r => r.Insert(It.Is<ShareAccessLog>(
                l => !l.IsSuccessful && l.FailureReason == "WrongPassword")), Times.Once);
            _mockLoggingService.Verify(l => l.Info(It.IsAny<string>()), Times.Once);
        }

        // ─────────────────────────────────────────────────────────────────────
        // IncrementDownloadCount Tests (Requirement 8.2, 8.3, 8.4)
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public void IncrementDownloadCount_IncrementsCounter()
        {
            // Arrange
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareToken = Guid.NewGuid(),
                FileID = 1,
                SharerUserID = 2,
                PasswordHash = null,
                ExpiryDate = DateTime.Now.AddDays(10),
                DownloadLimit = 5,
                CurrentDownloadCount = 2,
                ShareTokenStatus = "Active",
                CreatedDate = DateTime.Now,
                NotifyOnAccess = true
            };

            var file = new FileDocument
            {
                FileID = 1,
                OriginalFileName = "test.pdf"
            };

            _mockExternalShareRepo.Setup(r => r.GetById(1)).Returns(share);
            _mockFileDocumentRepo.Setup(r => r.GetById(1)).Returns(file);
            _mockExternalShareRepo.Setup(r => r.Update(It.IsAny<ExternalShare>())).Verifiable();

            // Act
            _shareAccessService.IncrementDownloadCount(1);

            // Assert
            Assert.Equal(3, share.CurrentDownloadCount);
            _mockExternalShareRepo.Verify(r => r.Update(It.Is<ExternalShare>(
                s => s.CurrentDownloadCount == 3)), Times.Once);
        }

        [Fact]
        public void IncrementDownloadCount_AtLimitTransitionsToDisabled()
        {
            // Arrange
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareToken = Guid.NewGuid(),
                FileID = 1,
                SharerUserID = 2,
                PasswordHash = null,
                ExpiryDate = DateTime.Now.AddDays(10),
                DownloadLimit = 3,
                CurrentDownloadCount = 2, // One more download will reach limit
                ShareTokenStatus = "Active",
                CreatedDate = DateTime.Now,
                NotifyOnAccess = true
            };

            var file = new FileDocument
            {
                FileID = 1,
                OriginalFileName = "test.pdf"
            };

            _mockExternalShareRepo.Setup(r => r.GetById(1)).Returns(share);
            _mockFileDocumentRepo.Setup(r => r.GetById(1)).Returns(file);
            _mockExternalShareRepo.Setup(r => r.Update(It.IsAny<ExternalShare>())).Verifiable();
            _mockNotificationService.Setup(n => n.Send(It.IsAny<NotificationRequest>())).Verifiable();

            // Act
            _shareAccessService.IncrementDownloadCount(1);

            // Assert
            Assert.Equal(3, share.CurrentDownloadCount);
            Assert.Equal("Disabled", share.ShareTokenStatus);
            Assert.NotNull(share.DisabledDate);
            Assert.Equal("Download limit reached", share.DisabledReason);
            _mockExternalShareRepo.Verify(r => r.Update(It.Is<ExternalShare>(
                s => s.ShareTokenStatus == "Disabled")), Times.Once);
            _mockNotificationService.Verify(n => n.Send(It.IsAny<NotificationRequest>()), Times.Once);
        }

        // ─────────────────────────────────────────────────────────────────────
        // IsDownloadLimitReached Tests
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public void IsDownloadLimitReached_WithLimitReached_ReturnsTrue()
        {
            // Arrange
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareToken = Guid.NewGuid(),
                FileID = 1,
                SharerUserID = 2,
                DownloadLimit = 5,
                CurrentDownloadCount = 5,
                ShareTokenStatus = "Active"
            };

            _mockExternalShareRepo.Setup(r => r.GetById(1)).Returns(share);

            // Act
            var result = _shareAccessService.IsDownloadLimitReached(1);

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsDownloadLimitReached_WithLimitNotReached_ReturnsFalse()
        {
            // Arrange
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareToken = Guid.NewGuid(),
                FileID = 1,
                SharerUserID = 2,
                DownloadLimit = 5,
                CurrentDownloadCount = 3,
                ShareTokenStatus = "Active"
            };

            _mockExternalShareRepo.Setup(r => r.GetById(1)).Returns(share);

            // Act
            var result = _shareAccessService.IsDownloadLimitReached(1);

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsDownloadLimitReached_WithNoLimit_ReturnsFalse()
        {
            // Arrange
            var share = new ExternalShare
            {
                ExternalShareID = 1,
                ShareToken = Guid.NewGuid(),
                FileID = 1,
                SharerUserID = 2,
                DownloadLimit = null, // No limit
                CurrentDownloadCount = 100,
                ShareTokenStatus = "Active"
            };

            _mockExternalShareRepo.Setup(r => r.GetById(1)).Returns(share);

            // Act
            var result = _shareAccessService.IsDownloadLimitReached(1);

            // Assert
            Assert.False(result);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ProcessExpiredShares Tests (Requirement 4.5, 12.1)
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public void ProcessExpiredShares_TransitionsExpiredToExpired()
        {
            // Arrange
            var expiredShare = new ExternalShare
            {
                ExternalShareID = 1,
                ShareToken = Guid.NewGuid(),
                FileID = 1,
                SharerUserID = 2,
                ExpiryDate = DateTime.Now.AddDays(-1), // Expired
                DownloadLimit = 5,
                CurrentDownloadCount = 0,
                ShareTokenStatus = "Active",
                CreatedDate = DateTime.Now.AddDays(-10),
                NotifyOnAccess = true
            };

            var file = new FileDocument
            {
                FileID = 1,
                OriginalFileName = "test.pdf"
            };

            var activeShares = new List<ExternalShare> { expiredShare };

            _mockExternalShareRepo.Setup(r => r.GetByStatus("Active")).Returns(activeShares);
            _mockFileDocumentRepo.Setup(r => r.GetById(1)).Returns(file);
            _mockExternalShareRepo.Setup(r => r.Update(It.IsAny<ExternalShare>())).Verifiable();
            _mockNotificationService.Setup(n => n.Send(It.IsAny<NotificationRequest>())).Verifiable();

            // Act
            _shareAccessService.ProcessExpiredShares();

            // Assert
            Assert.Equal("Expired", expiredShare.ShareTokenStatus);
            _mockExternalShareRepo.Verify(r => r.Update(It.Is<ExternalShare>(
                s => s.ShareTokenStatus == "Expired")), Times.Once);
            _mockNotificationService.Verify(n => n.Send(It.IsAny<NotificationRequest>()), Times.Once);
        }

        [Fact]
        public void ProcessExpiredShares_SkipsNonExpiredShares()
        {
            // Arrange
            var activeShare = new ExternalShare
            {
                ExternalShareID = 1,
                ShareToken = Guid.NewGuid(),
                FileID = 1,
                SharerUserID = 2,
                ExpiryDate = DateTime.Now.AddDays(10), // Not expired
                DownloadLimit = 5,
                CurrentDownloadCount = 0,
                ShareTokenStatus = "Active",
                CreatedDate = DateTime.Now,
                NotifyOnAccess = true
            };

            var activeShares = new List<ExternalShare> { activeShare };

            _mockExternalShareRepo.Setup(r => r.GetByStatus("Active")).Returns(activeShares);

            // Act
            _shareAccessService.ProcessExpiredShares();

            // Assert
            Assert.Equal("Active", activeShare.ShareTokenStatus);
            _mockExternalShareRepo.Verify(r => r.Update(It.IsAny<ExternalShare>()), Times.Never);
        }

        // ─────────────────────────────────────────────────────────────────────
        // ProcessDisabledShares Tests (Requirement 12.2)
        // ─────────────────────────────────────────────────────────────────────

        [Fact]
        public void ProcessDisabledShares_TransitionsDisabledToDisabled()
        {
            // Arrange
            var disabledShare = new ExternalShare
            {
                ExternalShareID = 1,
                ShareToken = Guid.NewGuid(),
                FileID = 1,
                SharerUserID = 2,
                ExpiryDate = DateTime.Now.AddDays(10),
                DownloadLimit = 5,
                CurrentDownloadCount = 5,
                ShareTokenStatus = "Disabled",
                CreatedDate = DateTime.Now.AddDays(-5),
                DisabledDate = DateTime.Now,
                DisabledReason = "Download limit reached",
                NotifyOnAccess = true
            };

            var disabledShares = new List<ExternalShare> { disabledShare };

            _mockExternalShareRepo.Setup(r => r.GetByStatus("Disabled")).Returns(disabledShares);

            // Act
            _shareAccessService.ProcessDisabledShares();

            // Assert
            Assert.Equal("Disabled", disabledShare.ShareTokenStatus);
            _mockLoggingService.Verify(l => l.Info(It.IsAny<string>()), Times.AtLeastOnce);
        }

        [Fact]
        public void ProcessDisabledShares_HandlesEmptyList()
        {
            // Arrange
            var disabledShares = new List<ExternalShare>();
            _mockExternalShareRepo.Setup(r => r.GetByStatus("Disabled")).Returns(disabledShares);

            // Act
            _shareAccessService.ProcessDisabledShares();

            // Assert
            _mockLoggingService.Verify(l => l.Info(It.IsAny<string>()), Times.AtLeastOnce);
        }
    }
}
