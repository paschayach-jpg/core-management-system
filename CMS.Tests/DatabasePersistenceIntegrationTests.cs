using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Diagnostics;
using System.Linq;
using NUnit.Framework;
using CMS.Data.Interfaces;
using CMS.Data.Repositories;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using NSubstitute;
using CMS.Infrastructure;

namespace CMS.Tests
{
    /// <summary>
    /// Integration tests for database persistence of file sharing tables.
    /// Tests ExternalShare, ShareAccessLog, and ShareToken table operations.
    /// Validates Requirements 2.1, 9.5, 17.5.
    /// 
    /// These tests use actual database connections to a test database.
    /// Ensure the test database has the file sharing schema created before running.
    /// </summary>
    [TestFixture]
    public class DatabasePersistenceIntegrationTests
    {
        private IConfigManager _configManager;
        private IExternalShareRepository _externalShareRepo;
        private IShareAccessLogRepository _accessLogRepo;
        private IShareTokenRepository _shareTokenRepo;
        private string _connectionString;

        [SetUp]
        public void SetUp()
        {
            // Use test database connection string
            _connectionString = "Server=.;Database=CMS_Test;Integrated Security=true;";
            
            _configManager = Substitute.For<IConfigManager>();
            _configManager.Get<string>("ConnectionString").Returns(_connectionString);

            _externalShareRepo = new ExternalShareRepository(_configManager);
            _accessLogRepo = new ShareAccessLogRepository(_configManager);
            _shareTokenRepo = new ShareTokenRepository(_configManager);

            // Clean up test data before each test
            CleanupTestData();
        }

        [TearDown]
        public void TearDown()
        {
            // Clean up test data after each test
            CleanupTestData();
        }

        private void CleanupTestData()
        {
            try
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        @"DELETE FROM storage.ShareAccessLog;
                          DELETE FROM storage.ShareToken;
                          DELETE FROM storage.ExternalShare;", conn))
                    {
                        cmd.ExecuteNonQuery();
                    }
                }
            }
            catch
            {
                // Ignore cleanup errors
            }
        }

        // -----------------------------------------------------------------------
        // ExternalShare Repository Tests
        // -----------------------------------------------------------------------

        [Test]
        public void ExternalShareRepository_Insert_CreatesRecord()
        {
            // Arrange
            var share = new ExternalShare
            {
                ShareToken = Guid.NewGuid(),
                FileID = 1,
                FolderID = null,
                SharerUserID = 100,
                ApproverUserID = null,
                PasswordHash = "hashed_password",
                ExpiryDate = DateTime.Now.AddDays(30),
                DownloadLimit = 5,
                CurrentDownloadCount = 0,
                ShareTokenStatus = "Pending",
                CreatedDate = DateTime.Now,
                SharePurpose = "Test share",
                NotifyOnAccess = true
            };

            // Act
            var insertedId = _externalShareRepo.Insert(share);

            // Assert
            Assert.Greater(insertedId, 0);
            var retrieved = _externalShareRepo.GetById(insertedId);
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(share.ShareToken, retrieved.ShareToken);
            Assert.AreEqual(share.FileID, retrieved.FileID);
            Assert.AreEqual(share.SharerUserID, retrieved.SharerUserID);
            Assert.AreEqual("Pending", retrieved.ShareTokenStatus);
        }

        [Test]
        public void ExternalShareRepository_GetById_ReturnsRecord()
        {
            // Arrange
            var token = Guid.NewGuid();
            var share = new ExternalShare
            {
                ShareToken = token,
                FileID = 1,
                FolderID = null,
                SharerUserID = 100,
                ApproverUserID = null,
                PasswordHash = null,
                ExpiryDate = null,
                DownloadLimit = null,
                CurrentDownloadCount = 0,
                ShareTokenStatus = "Active",
                CreatedDate = DateTime.Now,
                SharePurpose = null,
                NotifyOnAccess = false
            };
            var insertedId = _externalShareRepo.Insert(share);

            // Act
            var retrieved = _externalShareRepo.GetById(insertedId);

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(insertedId, retrieved.ExternalShareID);
            Assert.AreEqual(token, retrieved.ShareToken);
            Assert.AreEqual("Active", retrieved.ShareTokenStatus);
        }

        [Test]
        public void ExternalShareRepository_GetByToken_ReturnsRecord()
        {
            // Arrange
            var token = Guid.NewGuid();
            var share = new ExternalShare
            {
                ShareToken = token,
                FileID = 2,
                FolderID = null,
                SharerUserID = 101,
                ApproverUserID = null,
                PasswordHash = null,
                ExpiryDate = null,
                DownloadLimit = 10,
                CurrentDownloadCount = 0,
                ShareTokenStatus = "Active",
                CreatedDate = DateTime.Now,
                SharePurpose = "Test",
                NotifyOnAccess = true
            };
            _externalShareRepo.Insert(share);

            // Act
            var retrieved = _externalShareRepo.GetByToken(token);

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(token, retrieved.ShareToken);
            Assert.AreEqual(2, retrieved.FileID);
            Assert.AreEqual(101, retrieved.SharerUserID);
        }

        [Test]
        public void ExternalShareRepository_Update_UpdatesRecord()
        {
            // Arrange
            var share = new ExternalShare
            {
                ShareToken = Guid.NewGuid(),
                FileID = 1,
                FolderID = null,
                SharerUserID = 100,
                ApproverUserID = null,
                PasswordHash = null,
                ExpiryDate = null,
                DownloadLimit = 5,
                CurrentDownloadCount = 0,
                ShareTokenStatus = "Pending",
                CreatedDate = DateTime.Now,
                SharePurpose = null,
                NotifyOnAccess = true
            };
            var insertedId = _externalShareRepo.Insert(share);
            share.ExternalShareID = insertedId;

            // Act
            share.ShareTokenStatus = "Active";
            share.ApprovedDate = DateTime.Now;
            share.ApproverUserID = 200;
            share.CurrentDownloadCount = 2;
            _externalShareRepo.Update(share);

            // Assert
            var updated = _externalShareRepo.GetById(insertedId);
            Assert.AreEqual("Active", updated.ShareTokenStatus);
            Assert.IsNotNull(updated.ApprovedDate);
            Assert.AreEqual(200, updated.ApproverUserID);
            Assert.AreEqual(2, updated.CurrentDownloadCount);
        }

        [Test]
        public void ExternalShareRepository_GetByStatus_FiltersCorrectly()
        {
            // Arrange
            var share1 = new ExternalShare
            {
                ShareToken = Guid.NewGuid(),
                FileID = 1,
                FolderID = null,
                SharerUserID = 100,
                ApproverUserID = null,
                PasswordHash = null,
                ExpiryDate = null,
                DownloadLimit = null,
                CurrentDownloadCount = 0,
                ShareTokenStatus = "Pending",
                CreatedDate = DateTime.Now,
                SharePurpose = null,
                NotifyOnAccess = true
            };
            var share2 = new ExternalShare
            {
                ShareToken = Guid.NewGuid(),
                FileID = 2,
                FolderID = null,
                SharerUserID = 101,
                ApproverUserID = null,
                PasswordHash = null,
                ExpiryDate = null,
                DownloadLimit = null,
                CurrentDownloadCount = 0,
                ShareTokenStatus = "Active",
                CreatedDate = DateTime.Now,
                SharePurpose = null,
                NotifyOnAccess = true
            };
            var share3 = new ExternalShare
            {
                ShareToken = Guid.NewGuid(),
                FileID = 3,
                FolderID = null,
                SharerUserID = 102,
                ApproverUserID = null,
                PasswordHash = null,
                ExpiryDate = null,
                DownloadLimit = null,
                CurrentDownloadCount = 0,
                ShareTokenStatus = "Active",
                CreatedDate = DateTime.Now,
                SharePurpose = null,
                NotifyOnAccess = true
            };
            _externalShareRepo.Insert(share1);
            _externalShareRepo.Insert(share2);
            _externalShareRepo.Insert(share3);

            // Act
            var activeShares = _externalShareRepo.GetByStatus("Active").ToList();
            var pendingShares = _externalShareRepo.GetByStatus("Pending").ToList();

            // Assert
            Assert.AreEqual(2, activeShares.Count);
            Assert.AreEqual(1, pendingShares.Count);
            Assert.IsTrue(activeShares.All(s => s.ShareTokenStatus == "Active"));
            Assert.IsTrue(pendingShares.All(s => s.ShareTokenStatus == "Pending"));
        }

        // -----------------------------------------------------------------------
        // ShareAccessLog Repository Tests
        // -----------------------------------------------------------------------

        [Test]
        public void ShareAccessLogRepository_Insert_CreatesRecord()
        {
            // Arrange
            var externalShareId = InsertTestExternalShare();
            var token = Guid.NewGuid();
            var log = new ShareAccessLog
            {
                ShareToken = token,
                ExternalShareID = externalShareId,
                RecipientIdentifier = "user@example.com",
                AccessType = "Download",
                IsSuccessful = true,
                FailureReason = null,
                FileSizeBytes = 1024000,
                AccessTimestamp = DateTime.Now,
                IPAddress = "192.168.1.1",
                UserAgent = "Mozilla/5.0"
            };

            // Act
            var insertedId = _accessLogRepo.Insert(log);

            // Assert
            Assert.Greater(insertedId, 0);
            var retrieved = _accessLogRepo.GetByExternalShare(externalShareId).FirstOrDefault();
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(token, retrieved.ShareToken);
            Assert.AreEqual("Download", retrieved.AccessType);
            Assert.IsTrue(retrieved.IsSuccessful);
            Assert.AreEqual(1024000, retrieved.FileSizeBytes);
        }

        [Test]
        public void ShareAccessLogRepository_GetByExternalShare_ReturnsRecords()
        {
            // Arrange
            var externalShareId = InsertTestExternalShare();
            var log1 = new ShareAccessLog
            {
                ShareToken = Guid.NewGuid(),
                ExternalShareID = externalShareId,
                RecipientIdentifier = "user1@example.com",
                AccessType = "View",
                IsSuccessful = true,
                FailureReason = null,
                FileSizeBytes = null,
                AccessTimestamp = DateTime.Now.AddMinutes(-10),
                IPAddress = "192.168.1.1",
                UserAgent = "Mozilla/5.0"
            };
            var log2 = new ShareAccessLog
            {
                ShareToken = Guid.NewGuid(),
                ExternalShareID = externalShareId,
                RecipientIdentifier = "user2@example.com",
                AccessType = "Download",
                IsSuccessful = true,
                FailureReason = null,
                FileSizeBytes = 2048000,
                AccessTimestamp = DateTime.Now,
                IPAddress = "192.168.1.2",
                UserAgent = "Chrome/90"
            };
            _accessLogRepo.Insert(log1);
            _accessLogRepo.Insert(log2);

            // Act
            var logs = _accessLogRepo.GetByExternalShare(externalShareId).ToList();

            // Assert
            Assert.AreEqual(2, logs.Count);
            Assert.IsTrue(logs.All(l => l.ExternalShareID == externalShareId));
            // Verify chronological ordering
            Assert.IsTrue(logs[0].AccessTimestamp <= logs[1].AccessTimestamp);
        }

        [Test]
        public void ShareAccessLogRepository_Update_ThrowsException()
        {
            // Arrange
            var externalShareId = InsertTestExternalShare();
            var log = new ShareAccessLog
            {
                ShareToken = Guid.NewGuid(),
                ExternalShareID = externalShareId,
                RecipientIdentifier = "user@example.com",
                AccessType = "Download",
                IsSuccessful = true,
                FailureReason = null,
                FileSizeBytes = 1024000,
                AccessTimestamp = DateTime.Now,
                IPAddress = "192.168.1.1",
                UserAgent = "Mozilla/5.0"
            };
            var insertedId = _accessLogRepo.Insert(log);

            // Act & Assert
            // Attempt to update should fail due to immutability constraint
            Assert.Throws<SqlException>(() =>
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        @"UPDATE storage.ShareAccessLog 
                          SET AccessType = 'View' 
                          WHERE AccessLogID = @AccessLogID", conn))
                    {
                        cmd.Parameters.AddWithValue("@AccessLogID", insertedId);
                        cmd.ExecuteNonQuery();
                    }
                }
            });
        }

        [Test]
        public void ShareAccessLogRepository_Delete_ThrowsException()
        {
            // Arrange
            var externalShareId = InsertTestExternalShare();
            var log = new ShareAccessLog
            {
                ShareToken = Guid.NewGuid(),
                ExternalShareID = externalShareId,
                RecipientIdentifier = "user@example.com",
                AccessType = "Download",
                IsSuccessful = true,
                FailureReason = null,
                FileSizeBytes = 1024000,
                AccessTimestamp = DateTime.Now,
                IPAddress = "192.168.1.1",
                UserAgent = "Mozilla/5.0"
            };
            var insertedId = _accessLogRepo.Insert(log);

            // Act & Assert
            // Attempt to delete should fail due to immutability constraint
            Assert.Throws<SqlException>(() =>
            {
                using (var conn = new SqlConnection(_connectionString))
                {
                    conn.Open();
                    using (var cmd = new SqlCommand(
                        @"DELETE FROM storage.ShareAccessLog 
                          WHERE AccessLogID = @AccessLogID", conn))
                    {
                        cmd.Parameters.AddWithValue("@AccessLogID", insertedId);
                        cmd.ExecuteNonQuery();
                    }
                }
            });
        }

        // -----------------------------------------------------------------------
        // ShareToken Repository Tests
        // -----------------------------------------------------------------------

        [Test]
        public void ShareTokenRepository_Insert_CreatesRecord()
        {
            // Arrange
            var externalShareId = InsertTestExternalShare();
            var tokenValue = Guid.NewGuid();
            var token = new ShareToken
            {
                TokenValue = tokenValue,
                ExternalShareID = externalShareId,
                Status = "Pending",
                CreatedDate = DateTime.Now,
                ActivatedDate = null,
                ExpiryDate = null,
                DisabledDate = null,
                DisabledByUserID = null,
                DisabledReason = null
            };

            // Act
            var insertedId = _shareTokenRepo.Insert(token);

            // Assert
            Assert.Greater(insertedId, 0);
            var retrieved = _shareTokenRepo.GetById(insertedId);
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(tokenValue, retrieved.TokenValue);
            Assert.AreEqual("Pending", retrieved.Status);
        }

        [Test]
        public void ShareTokenRepository_GetByTokenValue_ReturnsRecord()
        {
            // Arrange
            var externalShareId = InsertTestExternalShare();
            var tokenValue = Guid.NewGuid();
            var token = new ShareToken
            {
                TokenValue = tokenValue,
                ExternalShareID = externalShareId,
                Status = "Active",
                CreatedDate = DateTime.Now,
                ActivatedDate = DateTime.Now,
                ExpiryDate = DateTime.Now.AddDays(30),
                DisabledDate = null,
                DisabledByUserID = null,
                DisabledReason = null
            };
            _shareTokenRepo.Insert(token);

            // Act
            var retrieved = _shareTokenRepo.GetByTokenValue(tokenValue);

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(tokenValue, retrieved.TokenValue);
            Assert.AreEqual("Active", retrieved.Status);
            Assert.IsNotNull(retrieved.ActivatedDate);
        }

        [Test]
        public void ShareTokenRepository_Update_UpdatesRecord()
        {
            // Arrange
            var externalShareId = InsertTestExternalShare();
            var token = new ShareToken
            {
                TokenValue = Guid.NewGuid(),
                ExternalShareID = externalShareId,
                Status = "Pending",
                CreatedDate = DateTime.Now,
                ActivatedDate = null,
                ExpiryDate = null,
                DisabledDate = null,
                DisabledByUserID = null,
                DisabledReason = null
            };
            var insertedId = _shareTokenRepo.Insert(token);
            token.ShareTokenID = insertedId;

            // Act
            token.Status = "Active";
            token.ActivatedDate = DateTime.Now;
            _shareTokenRepo.Update(token);

            // Assert
            var updated = _shareTokenRepo.GetById(insertedId);
            Assert.AreEqual("Active", updated.Status);
            Assert.IsNotNull(updated.ActivatedDate);
        }

        // -----------------------------------------------------------------------
        // Foreign Key Constraint Tests
        // -----------------------------------------------------------------------

        [Test]
        public void ForeignKeyConstraint_ExternalShare_File_EnforcesConstraint()
        {
            // Arrange
            var invalidFileId = 999999; // Non-existent file

            // Act & Assert
            Assert.Throws<SqlException>(() =>
            {
                var share = new ExternalShare
                {
                    ShareToken = Guid.NewGuid(),
                    FileID = invalidFileId,
                    FolderID = null,
                    SharerUserID = 100,
                    ApproverUserID = null,
                    PasswordHash = null,
                    ExpiryDate = null,
                    DownloadLimit = null,
                    CurrentDownloadCount = 0,
                    ShareTokenStatus = "Pending",
                    CreatedDate = DateTime.Now,
                    SharePurpose = null,
                    NotifyOnAccess = true
                };
                _externalShareRepo.Insert(share);
            });
        }

        [Test]
        public void ForeignKeyConstraint_ExternalShare_Folder_EnforcesConstraint()
        {
            // Arrange
            var invalidFolderId = 999999; // Non-existent folder

            // Act & Assert
            Assert.Throws<SqlException>(() =>
            {
                var share = new ExternalShare
                {
                    ShareToken = Guid.NewGuid(),
                    FileID = null,
                    FolderID = invalidFolderId,
                    SharerUserID = 100,
                    ApproverUserID = null,
                    PasswordHash = null,
                    ExpiryDate = null,
                    DownloadLimit = null,
                    CurrentDownloadCount = 0,
                    ShareTokenStatus = "Pending",
                    CreatedDate = DateTime.Now,
                    SharePurpose = null,
                    NotifyOnAccess = true
                };
                _externalShareRepo.Insert(share);
            });
        }

        [Test]
        public void ForeignKeyConstraint_ShareAccessLog_ExternalShare_EnforcesConstraint()
        {
            // Arrange
            var invalidExternalShareId = 999999; // Non-existent external share

            // Act & Assert
            Assert.Throws<SqlException>(() =>
            {
                var log = new ShareAccessLog
                {
                    ShareToken = Guid.NewGuid(),
                    ExternalShareID = invalidExternalShareId,
                    RecipientIdentifier = "user@example.com",
                    AccessType = "Download",
                    IsSuccessful = true,
                    FailureReason = null,
                    FileSizeBytes = 1024000,
                    AccessTimestamp = DateTime.Now,
                    IPAddress = "192.168.1.1",
                    UserAgent = "Mozilla/5.0"
                };
                _accessLogRepo.Insert(log);
            });
        }

        // -----------------------------------------------------------------------
        // Index Performance Tests
        // -----------------------------------------------------------------------

        [Test]
        public void IndexPerformance_IX_ExternalShare_Token_PerformsWell()
        {
            // Arrange
            var tokens = new List<Guid>();
            for (int i = 0; i < 100; i++)
            {
                var share = new ExternalShare
                {
                    ShareToken = Guid.NewGuid(),
                    FileID = 1,
                    FolderID = null,
                    SharerUserID = 100 + i,
                    ApproverUserID = null,
                    PasswordHash = null,
                    ExpiryDate = null,
                    DownloadLimit = null,
                    CurrentDownloadCount = 0,
                    ShareTokenStatus = "Active",
                    CreatedDate = DateTime.Now,
                    SharePurpose = null,
                    NotifyOnAccess = true
                };
                _externalShareRepo.Insert(share);
                tokens.Add(share.ShareToken);
            }

            // Act
            var stopwatch = Stopwatch.StartNew();
            foreach (var token in tokens)
            {
                var retrieved = _externalShareRepo.GetByToken(token);
                Assert.IsNotNull(retrieved);
            }
            stopwatch.Stop();

            // Assert - 100 lookups should complete in reasonable time (< 5 seconds)
            Assert.Less(stopwatch.ElapsedMilliseconds, 5000,
                $"Index lookup performance degraded: {stopwatch.ElapsedMilliseconds}ms for 100 lookups");
        }

        [Test]
        public void IndexPerformance_IX_ShareAccessLog_Timestamp_PerformsWell()
        {
            // Arrange
            var externalShareId = InsertTestExternalShare();
            var baseTime = DateTime.Now.AddDays(-10);
            
            for (int i = 0; i < 100; i++)
            {
                var log = new ShareAccessLog
                {
                    ShareToken = Guid.NewGuid(),
                    ExternalShareID = externalShareId,
                    RecipientIdentifier = $"user{i}@example.com",
                    AccessType = "Download",
                    IsSuccessful = true,
                    FailureReason = null,
                    FileSizeBytes = 1024000,
                    AccessTimestamp = baseTime.AddMinutes(i),
                    IPAddress = "192.168.1.1",
                    UserAgent = "Mozilla/5.0"
                };
                _accessLogRepo.Insert(log);
            }

            // Act
            var stopwatch = Stopwatch.StartNew();
            var logs = _accessLogRepo.GetByDateRange(
                baseTime.AddMinutes(-1),
                baseTime.AddMinutes(101)
            ).ToList();
            stopwatch.Stop();

            // Assert
            Assert.AreEqual(100, logs.Count);
            Assert.Less(stopwatch.ElapsedMilliseconds, 1000,
                $"Date range query performance degraded: {stopwatch.ElapsedMilliseconds}ms");
        }

        // -----------------------------------------------------------------------
        // Cascading Delete Tests
        // -----------------------------------------------------------------------

        [Test]
        public void CascadingDelete_ExternalShare_DeletesAccessLogs()
        {
            // Arrange
            var externalShareId = InsertTestExternalShare();
            var log1 = new ShareAccessLog
            {
                ShareToken = Guid.NewGuid(),
                ExternalShareID = externalShareId,
                RecipientIdentifier = "user1@example.com",
                AccessType = "Download",
                IsSuccessful = true,
                FailureReason = null,
                FileSizeBytes = 1024000,
                AccessTimestamp = DateTime.Now,
                IPAddress = "192.168.1.1",
                UserAgent = "Mozilla/5.0"
            };
            var log2 = new ShareAccessLog
            {
                ShareToken = Guid.NewGuid(),
                ExternalShareID = externalShareId,
                RecipientIdentifier = "user2@example.com",
                AccessType = "View",
                IsSuccessful = true,
                FailureReason = null,
                FileSizeBytes = null,
                AccessTimestamp = DateTime.Now,
                IPAddress = "192.168.1.2",
                UserAgent = "Chrome/90"
            };
            _accessLogRepo.Insert(log1);
            _accessLogRepo.Insert(log2);

            // Act
            // Delete the external share (should cascade to access logs)
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"DELETE FROM storage.ExternalShare WHERE ExternalShareID = @ExternalShareID", conn))
                {
                    cmd.Parameters.AddWithValue("@ExternalShareID", externalShareId);
                    cmd.ExecuteNonQuery();
                }
            }

            // Assert
            var remainingLogs = _accessLogRepo.GetByExternalShare(externalShareId).ToList();
            Assert.AreEqual(0, remainingLogs.Count);
        }

        [Test]
        public void CascadingDelete_ExternalShare_DeletesTokens()
        {
            // Arrange
            var externalShareId = InsertTestExternalShare();
            var token1 = new ShareToken
            {
                TokenValue = Guid.NewGuid(),
                ExternalShareID = externalShareId,
                Status = "Active",
                CreatedDate = DateTime.Now,
                ActivatedDate = DateTime.Now,
                ExpiryDate = null,
                DisabledDate = null,
                DisabledByUserID = null,
                DisabledReason = null
            };
            var token2 = new ShareToken
            {
                TokenValue = Guid.NewGuid(),
                ExternalShareID = externalShareId,
                Status = "Pending",
                CreatedDate = DateTime.Now,
                ActivatedDate = null,
                ExpiryDate = null,
                DisabledDate = null,
                DisabledByUserID = null,
                DisabledReason = null
            };
            _shareTokenRepo.Insert(token1);
            _shareTokenRepo.Insert(token2);

            // Act
            // Delete the external share (should cascade to tokens)
            using (var conn = new SqlConnection(_connectionString))
            {
                conn.Open();
                using (var cmd = new SqlCommand(
                    @"DELETE FROM storage.ExternalShare WHERE ExternalShareID = @ExternalShareID", conn))
                {
                    cmd.Parameters.AddWithValue("@ExternalShareID", externalShareId);
                    cmd.ExecuteNonQuery();
                }
            }

            // Assert
            var remainingTokens = _shareTokenRepo.GetByExternalShare(externalShareId).ToList();
            Assert.AreEqual(0, remainingTokens.Count);
        }

        // -----------------------------------------------------------------------
        // Helper Methods
        // -----------------------------------------------------------------------

        private int InsertTestExternalShare()
        {
            var share = new ExternalShare
            {
                ShareToken = Guid.NewGuid(),
                FileID = 1,
                FolderID = null,
                SharerUserID = 100,
                ApproverUserID = null,
                PasswordHash = null,
                ExpiryDate = null,
                DownloadLimit = null,
                CurrentDownloadCount = 0,
                ShareTokenStatus = "Active",
                CreatedDate = DateTime.Now,
                SharePurpose = null,
                NotifyOnAccess = true
            };
            return _externalShareRepo.Insert(share);
        }
    }
}
