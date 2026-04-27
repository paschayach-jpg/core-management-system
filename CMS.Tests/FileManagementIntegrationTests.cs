using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.Linq;
using NUnit.Framework;
using CMS.Data.Interfaces;
using CMS.Data.Repositories;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using NSubstitute;

namespace CMS.Tests
{
    /// <summary>
    /// Integration tests for Workflow Engine with File Management (MOD-006) integration.
    /// Tests file attachment and management within workflow tasks.
    /// 
    /// Validates Requirements:
    /// - Requirement 14: Integration with File Management (MOD-006)
    /// - Requirement 19: Integration with Audit Logging (MOD-020)
    /// </summary>
    [TestFixture]
    public class FileManagementIntegrationTests
    {
        private IConfigManager _configManager;
        private IWorkflowRepository _workflowRepo;
        private IWorkflowTemplateRepository _templateRepo;
        private IWorkflowStepRepository _stepRepo;
        private IWorkflowStepInstanceRepository _stepInstanceRepo;
        private IFileDocumentRepository _fileDocumentRepo;
        private IAuditRepository _auditRepo;
        private string _connectionString;
        private UserContext _testUserContext;

        [SetUp]
        public void SetUp()
        {
            _connectionString = "Server=.;Database=CMS_Test;Integrated Security=true;";
            
            _configManager = Substitute.For<IConfigManager>();
            _configManager.Get<string>("ConnectionString").Returns(_connectionString);

            _workflowRepo = new WorkflowRepository(_configManager);
            _templateRepo = new WorkflowTemplateRepository(_configManager);
            _stepRepo = new WorkflowStepRepository(_configManager);
            _stepInstanceRepo = new WorkflowStepInstanceRepository(_configManager);
            _fileDocumentRepo = new FileDocumentRepository(_configManager);
            _auditRepo = new AuditRepository(_configManager);

            _testUserContext = new UserContext
            {
                ADUserName = "testuser",
                UserID = 1,
                UserRole = "User"
            };

            CleanupTestData();
        }

        [TearDown]
        public void TearDown()
        {
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
                        @"DELETE FROM dbo.WFDelegation;
                          DELETE FROM dbo.WorkflowStepInstance;
                          DELETE FROM dbo.WorkflowInstance;
                          DELETE FROM dbo.WorkflowConnection;
                          DELETE FROM dbo.WorkflowStep;
                          DELETE FROM dbo.WorkflowDefinition;", conn))
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
        // Test 1: File Attachment to Workflow Task
        // -----------------------------------------------------------------------

        [Test]
        public void FileManagementIntegration_AttachesFileToWorkflowTask()
        {
            // Arrange
            var templateId = CreateTestTemplate("FileWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            Assert.Greater(stepInstances.Count, 0);
            var taskStep = stepInstances.First();

            var fileDocument = new FileDocument
            {
                FileName = "test_document.pdf",
                FileSize = 1024,
                FileType = "application/pdf",
                FolderID = 1,
                UploadedBy = _testUserContext.ADUserName,
                UploadedDate = DateTime.UtcNow,
                VersionNumber = 1,
                IsDeleted = false
            };

            // Act
            var fileId = _fileDocumentRepo.InsertFileDocument(fileDocument);

            // Store file reference in task output
            taskStep.OutputData = "{\"AttachedFileId\": " + fileId + ", \"FileName\": \"test_document.pdf\"}";
            _stepInstanceRepo.UpdateStepInstance(taskStep);

            // Assert
            Assert.Greater(fileId, 0);
            var persistedStep = _stepInstanceRepo.GetStepInstance(taskStep.WorkflowStepInstanceId);
            Assert.IsTrue(persistedStep.OutputData.Contains("test_document.pdf"));
            Assert.IsTrue(persistedStep.OutputData.Contains(fileId.ToString()));
        }

        [Test]
        public void FileManagementIntegration_StoresMultipleFileAttachments()
        {
            // Arrange
            var templateId = CreateTestTemplate("FileWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var taskStep = stepInstances.First();

            var fileIds = new List<int>();

            // Act - Attach multiple files
            for (int i = 0; i < 3; i++)
            {
                var fileDocument = new FileDocument
                {
                    FileName = $"document_{i}.pdf",
                    FileSize = 1024 * (i + 1),
                    FileType = "application/pdf",
                    FolderID = 1,
                    UploadedBy = _testUserContext.ADUserName,
                    UploadedDate = DateTime.UtcNow,
                    VersionNumber = 1,
                    IsDeleted = false
                };

                var fileId = _fileDocumentRepo.InsertFileDocument(fileDocument);
                fileIds.Add(fileId);
            }

            // Store all file references
            var fileReferences = string.Join(",", fileIds);
            taskStep.OutputData = "{\"AttachedFileIds\": [" + fileReferences + "]}";
            _stepInstanceRepo.UpdateStepInstance(taskStep);

            // Assert
            Assert.AreEqual(3, fileIds.Count);
            var persistedStep = _stepInstanceRepo.GetStepInstance(taskStep.WorkflowStepInstanceId);
            Assert.IsTrue(persistedStep.OutputData.Contains("AttachedFileIds"));
        }

        // -----------------------------------------------------------------------
        // Test 2: File Retrieval from Workflow Task
        // -----------------------------------------------------------------------

        [Test]
        public void FileManagementIntegration_RetrievesAttachedFilesFromTask()
        {
            // Arrange
            var templateId = CreateTestTemplate("FileWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var taskStep = stepInstances.First();

            var fileDocument = new FileDocument
            {
                FileName = "attached_document.pdf",
                FileSize = 2048,
                FileType = "application/pdf",
                FolderID = 1,
                UploadedBy = _testUserContext.ADUserName,
                UploadedDate = DateTime.UtcNow,
                VersionNumber = 1,
                IsDeleted = false
            };
            var fileId = _fileDocumentRepo.InsertFileDocument(fileDocument);

            taskStep.OutputData = "{\"AttachedFileId\": " + fileId + "}";
            _stepInstanceRepo.UpdateStepInstance(taskStep);

            // Act
            var persistedStep = _stepInstanceRepo.GetStepInstance(taskStep.WorkflowStepInstanceId);
            var retrievedFile = _fileDocumentRepo.GetFileDocument(fileId);

            // Assert
            Assert.IsNotNull(retrievedFile);
            Assert.AreEqual("attached_document.pdf", retrievedFile.FileName);
            Assert.AreEqual(2048, retrievedFile.FileSize);
        }

        [Test]
        public void FileManagementIntegration_PreservesFileMetadata()
        {
            // Arrange
            var fileName = "important_document.docx";
            var fileSize = 5120;
            var fileType = "application/vnd.openxmlformats-officedocument.wordprocessingml.document";

            var fileDocument = new FileDocument
            {
                FileName = fileName,
                FileSize = fileSize,
                FileType = fileType,
                FolderID = 1,
                UploadedBy = _testUserContext.ADUserName,
                UploadedDate = DateTime.UtcNow,
                VersionNumber = 1,
                IsDeleted = false
            };

            // Act
            var fileId = _fileDocumentRepo.InsertFileDocument(fileDocument);
            var retrieved = _fileDocumentRepo.GetFileDocument(fileId);

            // Assert
            Assert.AreEqual(fileName, retrieved.FileName);
            Assert.AreEqual(fileSize, retrieved.FileSize);
            Assert.AreEqual(fileType, retrieved.FileType);
        }

        // -----------------------------------------------------------------------
        // Test 3: File Versioning in Workflow
        // -----------------------------------------------------------------------

        [Test]
        public void FileManagementIntegration_TracksFileVersions()
        {
            // Arrange
            var templateId = CreateTestTemplate("FileWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);

            var fileDocument = new FileDocument
            {
                FileName = "versioned_document.pdf",
                FileSize = 1024,
                FileType = "application/pdf",
                FolderID = 1,
                UploadedBy = _testUserContext.ADUserName,
                UploadedDate = DateTime.UtcNow,
                VersionNumber = 1,
                IsDeleted = false
            };

            // Act
            var fileId = _fileDocumentRepo.InsertFileDocument(fileDocument);

            // Create a new version
            var fileDocumentV2 = new FileDocument
            {
                FileName = "versioned_document.pdf",
                FileSize = 2048,
                FileType = "application/pdf",
                FolderID = 1,
                UploadedBy = _testUserContext.ADUserName,
                UploadedDate = DateTime.UtcNow.AddMinutes(1),
                VersionNumber = 2,
                IsDeleted = false
            };
            var fileIdV2 = _fileDocumentRepo.InsertFileDocument(fileDocumentV2);

            // Assert
            Assert.Greater(fileIdV2, fileId);
            var v1 = _fileDocumentRepo.GetFileDocument(fileId);
            var v2 = _fileDocumentRepo.GetFileDocument(fileIdV2);
            Assert.AreEqual(1, v1.VersionNumber);
            Assert.AreEqual(2, v2.VersionNumber);
        }

        // -----------------------------------------------------------------------
        // Test 4: File Archival with Workflow Completion
        // -----------------------------------------------------------------------

        [Test]
        public void FileManagementIntegration_ArchivesFilesWithCompletedWorkflow()
        {
            // Arrange
            var templateId = CreateTestTemplate("FileWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var taskStep = stepInstances.First();

            var fileDocument = new FileDocument
            {
                FileName = "archived_document.pdf",
                FileSize = 1024,
                FileType = "application/pdf",
                FolderID = 1,
                UploadedBy = _testUserContext.ADUserName,
                UploadedDate = DateTime.UtcNow,
                VersionNumber = 1,
                IsDeleted = false
            };
            var fileId = _fileDocumentRepo.InsertFileDocument(fileDocument);

            // Act - Complete the workflow step
            taskStep.Status = WorkflowStepInstanceStatus.Completed;
            taskStep.CompletedDate = DateTime.UtcNow;
            taskStep.CompletedBy = _testUserContext.ADUserName;
            taskStep.OutputData = "{\"AttachedFileId\": " + fileId + ", \"Archived\": true}";
            _stepInstanceRepo.UpdateStepInstance(taskStep);

            // Complete the workflow instance
            var instance = _workflowRepo.GetWorkflowInstance(instanceId);
            instance.Status = WorkflowInstanceStatus.Completed;
            instance.CompletedDate = DateTime.UtcNow;
            _workflowRepo.UpdateWorkflowInstance(instance);

            // Assert
            var persistedStep = _stepInstanceRepo.GetStepInstance(taskStep.WorkflowStepInstanceId);
            Assert.AreEqual(WorkflowStepInstanceStatus.Completed, persistedStep.Status);
            Assert.IsTrue(persistedStep.OutputData.Contains("Archived"));
        }

        // -----------------------------------------------------------------------
        // Test 5: File Audit Trail
        // -----------------------------------------------------------------------

        [Test]
        public void FileManagementIntegration_MaintainsFileAuditTrail()
        {
            // Arrange
            var templateId = CreateTestTemplate("FileWorkflow");
            var instanceId = CreateTestWorkflowInstance(templateId);
            var stepInstances = _stepInstanceRepo.GetStepInstancesByWorkflow(instanceId);
            var taskStep = stepInstances.First();

            var fileDocument = new FileDocument
            {
                FileName = "audit_document.pdf",
                FileSize = 1024,
                FileType = "application/pdf",
                FolderID = 1,
                UploadedBy = _testUserContext.ADUserName,
                UploadedDate = DateTime.UtcNow,
                VersionNumber = 1,
                IsDeleted = false
            };

            // Act
            var fileId = _fileDocumentRepo.InsertFileDocument(fileDocument);
            var retrieved = _fileDocumentRepo.GetFileDocument(fileId);

            // Assert
            Assert.IsNotNull(retrieved);
            Assert.AreEqual(_testUserContext.ADUserName, retrieved.UploadedBy);
            Assert.IsNotNull(retrieved.UploadedDate);
        }

        // -----------------------------------------------------------------------
        // Helper Methods
        // -----------------------------------------------------------------------

        private int CreateTestTemplate(string templateName)
        {
            var template = new WorkflowDefinition
            {
                Name = templateName + "_" + Guid.NewGuid(),
                Description = "Test template",
                Version = 1,
                IsActive = true,
                CreatedBy = _testUserContext.ADUserName,
                CreatedDate = DateTime.UtcNow
            };

            return _templateRepo.CreateTemplate(template);
        }

        private int CreateTestWorkflowInstance(int templateId)
        {
            var instance = new WorkflowInstance
            {
                WorkflowDefinitionId = templateId,
                TemplateVersion = 1,
                InitiatedBy = _testUserContext.ADUserName,
                InitiatedDate = DateTime.UtcNow,
                Status = WorkflowInstanceStatus.InProgress,
                InputParameters = "{}"
            };

            var instanceId = _workflowRepo.CreateWorkflowInstance(instance);

            var steps = _stepRepo.GetStepsByTemplate(templateId);
            if (steps.Count > 0)
            {
                var firstStep = steps.First();
                var stepInstance = new WorkflowStepInstance
                {
                    WorkflowInstanceId = instanceId,
                    WorkflowStepId = firstStep.WorkflowStepId,
                    AssignedTo = _testUserContext.ADUserName,
                    AssignedDate = DateTime.UtcNow,
                    Status = WorkflowStepInstanceStatus.Pending,
                    SLADeadline = DateTime.UtcNow.AddHours(24),
                    SLAStatus = SLAStatus.OnTime
                };

                _stepInstanceRepo.CreateStepInstance(stepInstance);
            }

            return instanceId;
        }
    }
}
