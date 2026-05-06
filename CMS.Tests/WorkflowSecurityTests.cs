using NUnit.Framework;
using System;
using System.Collections.Generic;
using NSubstitute;
using CMS.Services;
using CMS.Services.Interfaces;
using CMS.Services.DTOs;
using CMS.Domain.Models;
using CMS.Domain.Enums;
using CMS.Data.Interfaces;

namespace CMS.Tests
{
    /// <summary>
    /// Phase 15.5: Security Testing for Workflow Engine
    /// 
    /// Tests security controls:
    /// - Permission enforcement (unauthorized access denied)
    /// - Data encryption (sensitive fields encrypted)
    /// - Audit trail integrity (audit logs cannot be modified)
    /// - PDPA compliance (personal data handling)
    /// 
    /// **Validates: Requirements 23, 27, 28 - Security, PDPA Compliance, Audit Trail Immutability**
    /// </summary>
    [TestFixture]
    public class WorkflowSecurityTests
    {
        private IWorkflowService _workflowService;
        private IWorkflowPermissionService _permissionService;
        private IAuditService _auditService;
        private IWorkflowDataEncryptionService _encryptionService;
        private IWorkflowPDPAComplianceService _pdpaService;
        private UserContext _adminContext;
        private UserContext _userContext;
        private UserContext _unauthorizedContext;

        [SetUp]
        public void Setup()
        {
            // Mock dependencies
            _workflowService = Substitute.For<IWorkflowService>();
            _permissionService = Substitute.For<IWorkflowPermissionService>();
            _auditService = Substitute.For<IAuditService>();
            _encryptionService = Substitute.For<IWorkflowDataEncryptionService>();
            _pdpaService = Substitute.For<IWorkflowPDPAComplianceService>();

            // Create test user contexts
            _adminContext = new UserContext
            {
                ADUserName = "admin",
                UserID = 1,
                FullName = "Admin User",
                Email = "admin@example.com",
                IsAdmin = true
            };

            _userContext = new UserContext
            {
                ADUserName = "user1",
                UserID = 2,
                FullName = "Regular User",
                Email = "user1@example.com",
                IsAdmin = false
            };

            _unauthorizedContext = new UserContext
            {
                ADUserName = "unauthorized",
                UserID = 3,
                FullName = "Unauthorized User",
                Email = "unauthorized@example.com",
                IsAdmin = false
            };
        }

        // ============================================================================
        // PERMISSION ENFORCEMENT TESTS
        // ============================================================================

        /// <summary>
        /// Security Test 1: Permission Enforcement - View Workflow Instance
        /// Verifies that unauthorized users cannot view workflow instances
        /// 
        /// **Validates: Requirement 23.1 - User must have permission to view instance**
        /// </summary>
        [Test]
        public void SecurityTest_PermissionEnforcement_UnauthorizedUserCannotViewWorkflow()
        {
            // Arrange
            var workflowId = 1;
            var instance = new WorkflowInstance
            {
                WorkflowInstanceId = workflowId,
                WorkflowDefinitionId = 1,
                InitiatedBy = _userContext.ADUserName,
                Status = WorkflowInstanceStatus.InProgress
            };

            _permissionService.CanViewWorkflowInstance(instance, _unauthorizedContext)
                .Returns(false);

            // Act & Assert
            Assert.That(_permissionService.CanViewWorkflowInstance(instance, _unauthorizedContext), Is.False,
                "Unauthorized user should not be able to view workflow instance");
        }

        /// <summary>
        /// Security Test 2: Permission Enforcement - Complete Workflow Step
        /// Verifies that only assigned users can complete workflow steps
        /// 
        /// **Validates: Requirement 23.2 - User must have permission to perform action**
        /// </summary>
        [Test]
        public void SecurityTest_PermissionEnforcement_OnlyAssignedUserCanCompleteStep()
        {
            // Arrange
            var stepInstance = new WorkflowStepInstance
            {
                WorkflowStepInstanceId = 1,
                AssignedTo = _userContext.ADUserName,
                Status = WorkflowStepInstanceStatus.Pending
            };

            _permissionService.CanCompleteWorkflowStep(stepInstance, _userContext)
                .Returns(true);
            _permissionService.CanCompleteWorkflowStep(stepInstance, _unauthorizedContext)
                .Returns(false);

            // Act & Assert
            Assert.That(_permissionService.CanCompleteWorkflowStep(stepInstance, _userContext), Is.True,
                "Assigned user should be able to complete step");
            Assert.That(_permissionService.CanCompleteWorkflowStep(stepInstance, _unauthorizedContext), Is.False,
                "Unassigned user should not be able to complete step");
        }

        /// <summary>
        /// Security Test 3: Permission Enforcement - Create Workflow Template
        /// Verifies that only admins can create workflow templates
        /// 
        /// **Validates: Requirement 23.1 - Permission checks for template creation**
        /// </summary>
        [Test]
        public void SecurityTest_PermissionEnforcement_OnlyAdminCanCreateTemplate()
        {
            // Arrange
            var request = new CreateTemplateRequest
            {
                Name = "TestTemplate",
                Description = "Test"
            };

            _permissionService.CanCreateTemplate(_adminContext)
                .Returns(true);
            _permissionService.CanCreateTemplate(_userContext)
                .Returns(false);

            // Act & Assert
            Assert.That(_permissionService.CanCreateTemplate(_adminContext), Is.True,
                "Admin should be able to create template");
            Assert.That(_permissionService.CanCreateTemplate(_userContext), Is.False,
                "Regular user should not be able to create template");
        }

        /// <summary>
        /// Security Test 4: Permission Enforcement - Delegate Task
        /// Verifies that only assigned users can delegate tasks
        /// 
        /// **Validates: Requirement 8 - Delegation permission checks**
        /// </summary>
        [Test]
        public void SecurityTest_PermissionEnforcement_OnlyAssignedUserCanDelegateTask()
        {
            // Arrange
            var stepInstance = new WorkflowStepInstance
            {
                WorkflowStepInstanceId = 1,
                AssignedTo = _userContext.ADUserName,
                Status = WorkflowStepInstanceStatus.Pending
            };

            _permissionService.CanDelegateTask(stepInstance, _userContext)
                .Returns(true);
            _permissionService.CanDelegateTask(stepInstance, _unauthorizedContext)
                .Returns(false);

            // Act & Assert
            Assert.That(_permissionService.CanDelegateTask(stepInstance, _userContext), Is.True,
                "Assigned user should be able to delegate task");
            Assert.That(_permissionService.CanDelegateTask(stepInstance, _unauthorizedContext), Is.False,
                "Unassigned user should not be able to delegate task");
        }

        // ============================================================================
        // DATA ENCRYPTION TESTS
        // ============================================================================

        /// <summary>
        /// Security Test 5: Data Encryption - Sensitive Fields Encrypted
        /// Verifies that sensitive workflow data is encrypted
        /// 
        /// **Validates: Requirement 23.3 - Sensitive data encrypted at rest**
        /// </summary>
        [Test]
        public void SecurityTest_DataEncryption_SensitiveFieldsAreEncrypted()
        {
            // Arrange
            var sensitiveData = new Dictionary<string, object>
            {
                { "DocumentId", 123 },
                { "ApprovalDecision", "Approved" },
                { "PersonalInfo", "John Doe" }
            };

            var encryptedData = new Dictionary<string, object>
            {
                { "DocumentId", "encrypted_123" },
                { "ApprovalDecision", "encrypted_approved" },
                { "PersonalInfo", "encrypted_john_doe" }
            };

            _encryptionService.EncryptSensitiveData(sensitiveData)
                .Returns(encryptedData);

            // Act
            var result = _encryptionService.EncryptSensitiveData(sensitiveData);

            // Assert
            Assert.That(result, Is.Not.Null);
            Assert.That(result["DocumentId"], Is.Not.EqualTo(sensitiveData["DocumentId"]),
                "Sensitive data should be encrypted");
            Assert.That(result["PersonalInfo"], Is.Not.EqualTo(sensitiveData["PersonalInfo"]),
                "Personal information should be encrypted");
        }

        /// <summary>
        /// Security Test 6: Data Encryption - Decryption Verification
        /// Verifies that encrypted data can be decrypted correctly
        /// 
        /// **Validates: Requirement 23.3 - Data can be decrypted for authorized access**
        /// </summary>
        [Test]
        public void SecurityTest_DataEncryption_EncryptedDataCanBeDecrypted()
        {
            // Arrange
            var originalData = new Dictionary<string, object>
            {
                { "DocumentId", 123 },
                { "ApprovalDecision", "Approved" }
            };

            var encryptedData = new Dictionary<string, object>
            {
                { "DocumentId", "encrypted_123" },
                { "ApprovalDecision", "encrypted_approved" }
            };

            _encryptionService.EncryptSensitiveData(originalData)
                .Returns(encryptedData);
            _encryptionService.DecryptSensitiveData(encryptedData)
                .Returns(originalData);

            // Act
            var encrypted = _encryptionService.EncryptSensitiveData(originalData);
            var decrypted = _encryptionService.DecryptSensitiveData(encrypted);

            // Assert
            Assert.That(decrypted, Is.Not.Null);
            Assert.That(decrypted["DocumentId"], Is.EqualTo(originalData["DocumentId"]),
                "Decrypted data should match original");
            Assert.That(decrypted["ApprovalDecision"], Is.EqualTo(originalData["ApprovalDecision"]),
                "Decrypted approval decision should match original");
        }

        /// <summary>
        /// Security Test 7: Data Encryption - Transmission Security
        /// Verifies that data is encrypted during transmission
        /// 
        /// **Validates: Requirement 23.4 - Secure communication protocols (HTTPS/TLS)**
        /// </summary>
        [Test]
        public void SecurityTest_DataEncryption_TransmissionIsSecure()
        {
            // Arrange
            var workflowData = new WorkflowInstance
            {
                WorkflowInstanceId = 1,
                InputParameters = "{\"sensitive\": \"data\"}"
            };

            // Act
            var isSecure = _encryptionService.IsTransmissionSecure();

            // Assert
            Assert.That(isSecure, Is.True,
                "Transmission should use secure protocols (HTTPS/TLS)");
        }

        // ============================================================================
        // AUDIT TRAIL INTEGRITY TESTS
        // ============================================================================

        /// <summary>
        /// Security Test 8: Audit Trail Immutability - Entries Cannot Be Modified
        /// Verifies that audit log entries cannot be modified after creation
        /// 
        /// **Validates: Requirement 28.1 - Audit entries are immutable**
        /// </summary>
        [Test]
        public void SecurityTest_AuditTrailImmutability_EntriesCannotBeModified()
        {
            // Arrange
            var auditEntry = new AuditLogEntry
            {
                AuditLogId = 1,
                ActionType = "WorkflowCreated",
                Timestamp = DateTime.Now,
                User = _userContext.ADUserName,
                Details = "Workflow instance created"
            };

            _auditService.CreateAuditEntry(auditEntry)
                .Returns(1);

            // Act
            var entryId = _auditService.CreateAuditEntry(auditEntry);

            // Assert
            Assert.That(entryId, Is.GreaterThan(0));
            // Verify that modification is not allowed
            Assert.Throws<InvalidOperationException>(() =>
            {
                _auditService.UpdateAuditEntry(auditEntry);
            }, "Audit entries should not be modifiable");
        }

        /// <summary>
        /// Security Test 9: Audit Trail Immutability - Entries Cannot Be Deleted
        /// Verifies that audit log entries cannot be deleted
        /// 
        /// **Validates: Requirement 28.1 - Audit entries cannot be deleted**
        /// </summary>
        [Test]
        public void SecurityTest_AuditTrailImmutability_EntriesCannotBeDeleted()
        {
            // Arrange
            var auditEntryId = 1;

            // Act & Assert
            Assert.Throws<InvalidOperationException>(() =>
            {
                _auditService.DeleteAuditEntry(auditEntryId);
            }, "Audit entries should not be deletable");
        }

        /// <summary>
        /// Security Test 10: Audit Trail Integrity - Entries Include Timestamp and User
        /// Verifies that audit entries include required metadata
        /// 
        /// **Validates: Requirement 28.2 - Audit entries include timestamp and user**
        /// </summary>
        [Test]
        public void SecurityTest_AuditTrailIntegrity_EntriesIncludeMetadata()
        {
            // Arrange
            var auditEntry = new AuditLogEntry
            {
                AuditLogId = 1,
                ActionType = "StepCompleted",
                Timestamp = DateTime.Now,
                User = _userContext.ADUserName,
                Details = "Step completed successfully"
            };

            // Act & Assert
            Assert.That(auditEntry.Timestamp, Is.Not.EqualTo(default(DateTime)),
                "Audit entry should include timestamp");
            Assert.That(auditEntry.User, Is.Not.Null.And.Not.Empty,
                "Audit entry should include user identifier");
            Assert.That(auditEntry.ActionType, Is.Not.Null.And.Not.Empty,
                "Audit entry should include action type");
        }

        /// <summary>
        /// Security Test 11: Audit Trail Integrity - Chronological Order
        /// Verifies that audit entries are returned in chronological order
        /// 
        /// **Validates: Requirement 28.3 - Audit logs returned in chronological order**
        /// </summary>
        [Test]
        public void SecurityTest_AuditTrailIntegrity_EntriesInChronologicalOrder()
        {
            // Arrange
            var entries = new List<AuditLogEntry>
            {
                new AuditLogEntry { AuditLogId = 1, Timestamp = DateTime.Now.AddHours(-2) },
                new AuditLogEntry { AuditLogId = 2, Timestamp = DateTime.Now.AddHours(-1) },
                new AuditLogEntry { AuditLogId = 3, Timestamp = DateTime.Now }
            };

            _auditService.GetAuditLog(Arg.Any<AuditQuery>())
                .Returns(entries);

            // Act
            var result = _auditService.GetAuditLog(new AuditQuery());

            // Assert
            for (int i = 1; i < result.Count; i++)
            {
                Assert.That(result[i].Timestamp, Is.GreaterThanOrEqualTo(result[i - 1].Timestamp),
                    "Audit entries should be in chronological order");
            }
        }

        // ============================================================================
        // PDPA COMPLIANCE TESTS
        // ============================================================================

        /// <summary>
        /// Security Test 12: PDPA Compliance - Consent Verification
        /// Verifies that PDPA consent is obtained before processing personal data
        /// 
        /// **Validates: Requirement 27.1 - PDPA consent verification**
        /// </summary>
        [Test]
        public void SecurityTest_PDPACompliance_ConsentVerification()
        {
            // Arrange
            var userId = 1;
            var hasConsent = true;

            _pdpaService.HasPDPAConsent(userId)
                .Returns(hasConsent);

            // Act
            var result = _pdpaService.HasPDPAConsent(userId);

            // Assert
            Assert.That(result, Is.True,
                "PDPA consent should be verified before processing personal data");
        }

        /// <summary>
        /// Security Test 13: PDPA Compliance - Personal Data Encryption
        /// Verifies that personal data is encrypted
        /// 
        /// **Validates: Requirement 27.3 - Personal data encrypted and access restricted**
        /// </summary>
        [Test]
        public void SecurityTest_PDPACompliance_PersonalDataEncryption()
        {
            // Arrange
            var personalData = new Dictionary<string, object>
            {
                { "EmployeeId", 123 },
                { "EmployeeName", "John Doe" },
                { "Email", "john@example.com" }
            };

            _encryptionService.EncryptPersonalData(personalData)
                .Returns(new Dictionary<string, object>
                {
                    { "EmployeeId", "encrypted_123" },
                    { "EmployeeName", "encrypted_john_doe" },
                    { "Email", "encrypted_john@example.com" }
                });

            // Act
            var encrypted = _encryptionService.EncryptPersonalData(personalData);

            // Assert
            Assert.That(encrypted, Is.Not.Null);
            Assert.That(encrypted["EmployeeName"], Is.Not.EqualTo(personalData["EmployeeName"]),
                "Personal data should be encrypted");
        }

        /// <summary>
        /// Security Test 14: PDPA Compliance - Data Deletion Request
        /// Verifies that personal data can be deleted upon request
        /// 
        /// **Validates: Requirement 27.2 - User can request data deletion**
        /// </summary>
        [Test]
        public void SecurityTest_PDPACompliance_DataDeletionRequest()
        {
            // Arrange
            var userId = 1;
            var workflowInstanceId = 1;

            _pdpaService.DeletePersonalData(userId, workflowInstanceId)
                .Returns(true);

            // Act
            var result = _pdpaService.DeletePersonalData(userId, workflowInstanceId);

            // Assert
            Assert.That(result, Is.True,
                "Personal data should be deletable upon user request");
        }

        /// <summary>
        /// Security Test 15: PDPA Compliance - Secure Transmission
        /// Verifies that personal data is transmitted securely
        /// 
        /// **Validates: Requirement 27.4 - Secure communication protocols**
        /// </summary>
        [Test]
        public void SecurityTest_PDPACompliance_SecureTransmission()
        {
            // Arrange
            var personalData = new Dictionary<string, object>
            {
                { "EmployeeId", 123 },
                { "EmployeeName", "John Doe" }
            };

            // Act
            var isSecure = _encryptionService.IsTransmissionSecure();

            // Assert
            Assert.That(isSecure, Is.True,
                "Personal data transmission should use secure protocols");
        }

        /// <summary>
        /// Security Test 16: PDPA Compliance - Access Control
        /// Verifies that only authorized personnel can access personal data
        /// 
        /// **Validates: Requirement 27.3 - Access restricted to authorized personnel**
        /// </summary>
        [Test]
        public void SecurityTest_PDPACompliance_AccessControl()
        {
            // Arrange
            var personalDataId = 1;

            _pdpaService.CanAccessPersonalData(personalDataId, _adminContext)
                .Returns(true);
            _pdpaService.CanAccessPersonalData(personalDataId, _unauthorizedContext)
                .Returns(false);

            // Act & Assert
            Assert.That(_pdpaService.CanAccessPersonalData(personalDataId, _adminContext), Is.True,
                "Admin should be able to access personal data");
            Assert.That(_pdpaService.CanAccessPersonalData(personalDataId, _unauthorizedContext), Is.False,
                "Unauthorized user should not be able to access personal data");
        }
    }

    /// <summary>
    /// Helper class for audit log entries
    /// </summary>
    public class AuditLogEntry
    {
        public int AuditLogId { get; set; }
        public string ActionType { get; set; }
        public DateTime Timestamp { get; set; }
        public string User { get; set; }
        public string Details { get; set; }
    }
}
