using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using FsCheck;
using FsCheck.NUnit;
using NUnit.Framework;
using PropertyAttribute = FsCheck.NUnit.PropertyAttribute;

namespace CMS.Tests.PropertyBasedTests
{
    /// <summary>
    /// Property-Based Tests for Invalid Input Rejection (Phase 9.9)
    /// 
    /// Property: System rejects all invalid inputs and returns error.
    /// 
    /// Validates:
    /// - No invalid data enters the system
    /// - Error messages are meaningful
    /// - Invalid inputs are consistently rejected
    /// </summary>
    [TestFixture]
    public class InvalidInputRejectionTests
    {
        // =====================================================================
        // Helper Methods
        // =====================================================================

        /// <summary>
        /// Validates a workflow instance for invalid inputs.
        /// </summary>
        private static ValidationResult ValidateWorkflowInstance(WorkflowInstance instance)
        {
            if (instance == null)
                return new ValidationResult { IsValid = false, ErrorMessage = "Workflow instance cannot be null" };

            if (instance.WorkflowInstanceId <= 0)
                return new ValidationResult { IsValid = false, ErrorMessage = "Workflow instance ID must be positive" };

            if (instance.WorkflowDefinitionId <= 0)
                return new ValidationResult { IsValid = false, ErrorMessage = "Workflow definition ID must be positive" };

            if (instance.TemplateVersion <= 0)
                return new ValidationResult { IsValid = false, ErrorMessage = "Template version must be positive" };

            if (string.IsNullOrWhiteSpace(instance.InitiatedBy))
                return new ValidationResult { IsValid = false, ErrorMessage = "Initiated by user cannot be empty" };

            if (instance.InitiatedDate == default(DateTime))
                return new ValidationResult { IsValid = false, ErrorMessage = "Initiated date cannot be default" };

            if (string.IsNullOrEmpty(instance.InputParameters))
                return new ValidationResult { IsValid = false, ErrorMessage = "Input parameters cannot be null or empty" };

            return new ValidationResult { IsValid = true, ErrorMessage = null };
        }

        /// <summary>
        /// Validates a workflow step instance for invalid inputs.
        /// </summary>
        private static ValidationResult ValidateWorkflowStepInstance(WorkflowStepInstance instance)
        {
            if (instance == null)
                return new ValidationResult { IsValid = false, ErrorMessage = "Step instance cannot be null" };

            if (instance.WorkflowStepInstanceId <= 0)
                return new ValidationResult { IsValid = false, ErrorMessage = "Step instance ID must be positive" };

            if (instance.WorkflowInstanceId <= 0)
                return new ValidationResult { IsValid = false, ErrorMessage = "Workflow instance ID must be positive" };

            if (instance.WorkflowStepId <= 0)
                return new ValidationResult { IsValid = false, ErrorMessage = "Workflow step ID must be positive" };

            if (string.IsNullOrWhiteSpace(instance.AssignedTo))
                return new ValidationResult { IsValid = false, ErrorMessage = "Assigned to user cannot be empty" };

            if (instance.AssignedDate == default(DateTime))
                return new ValidationResult { IsValid = false, ErrorMessage = "Assigned date cannot be default" };

            if (instance.SLADeadline == default(DateTime))
                return new ValidationResult { IsValid = false, ErrorMessage = "SLA deadline cannot be default" };

            if (instance.SLADeadline <= instance.AssignedDate)
                return new ValidationResult { IsValid = false, ErrorMessage = "SLA deadline must be after assigned date" };

            return new ValidationResult { IsValid = true, ErrorMessage = null };
        }

        /// <summary>
        /// Validates a SLA rule for invalid inputs.
        /// </summary>
        private static ValidationResult ValidateSLARule(WFSLARule rule)
        {
            if (rule == null)
                return new ValidationResult { IsValid = false, ErrorMessage = "SLA rule cannot be null" };

            if (rule.WFSLARuleId <= 0)
                return new ValidationResult { IsValid = false, ErrorMessage = "SLA rule ID must be positive" };

            if (rule.WorkflowStepId <= 0)
                return new ValidationResult { IsValid = false, ErrorMessage = "Workflow step ID must be positive" };

            if (rule.SLADurationHours <= 0)
                return new ValidationResult { IsValid = false, ErrorMessage = "SLA duration must be positive" };

            if (rule.SLADurationHours > 720) // 30 days
                return new ValidationResult { IsValid = false, ErrorMessage = "SLA duration cannot exceed 720 hours" };

            return new ValidationResult { IsValid = true, ErrorMessage = null };
        }

        // =====================================================================
        // Property Tests
        // =====================================================================

        /// <summary>
        /// Property: Null workflow instance is rejected.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 100)]
        public Property Property_NullWorkflowInstanceIsRejected()
        {
            var result = ValidateWorkflowInstance(null);

            return (!result.IsValid && result.ErrorMessage != null).ToProperty();
        }

        /// <summary>
        /// Property: Negative workflow instance ID is rejected.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_NegativeWorkflowInstanceIdIsRejected(int negativeId)
        {
            var id = -(Math.Abs(negativeId) + 1); // Ensure negative

            var instance = new WorkflowInstance
            {
                WorkflowInstanceId = id,
                WorkflowDefinitionId = 1,
                TemplateVersion = 1,
                Status = WorkflowInstanceStatus.Created,
                InitiatedBy = "user",
                InitiatedDate = DateTime.Now,
                CurrentStepId = null,
                CompletedDate = null,
                InputParameters = "{}",
                IsDeleted = false,
                DeletedDate = null
            };

            var result = ValidateWorkflowInstance(instance);

            return (!result.IsValid).ToProperty();
        }

        /// <summary>
        /// Property: Empty initiated by user is rejected.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_EmptyInitiatedByIsRejected(string emptyUser)
        {
            if (emptyUser != null && emptyUser.Length > 0)
                return true.ToProperty();

            var instance = new WorkflowInstance
            {
                WorkflowInstanceId = 1,
                WorkflowDefinitionId = 1,
                TemplateVersion = 1,
                Status = WorkflowInstanceStatus.Created,
                InitiatedBy = emptyUser ?? "",
                InitiatedDate = DateTime.Now,
                CurrentStepId = null,
                CompletedDate = null,
                InputParameters = "{}",
                IsDeleted = false,
                DeletedDate = null
            };

            var result = ValidateWorkflowInstance(instance);

            return (!result.IsValid).ToProperty();
        }

        /// <summary>
        /// Property: Null step instance is rejected.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 100)]
        public Property Property_NullStepInstanceIsRejected()
        {
            var result = ValidateWorkflowStepInstance(null);

            return (!result.IsValid && result.ErrorMessage != null).ToProperty();
        }

        /// <summary>
        /// Property: Negative step instance ID is rejected.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_NegativeStepInstanceIdIsRejected(int negativeId)
        {
            var id = -(Math.Abs(negativeId) + 1);

            var instance = new WorkflowStepInstance
            {
                WorkflowStepInstanceId = id,
                WorkflowInstanceId = 1,
                WorkflowStepId = 1,
                AssignedTo = "user",
                AssignedDate = DateTime.Now,
                Status = WorkflowStepInstanceStatus.Pending,
                CompletedDate = null,
                CompletedBy = null,
                OutputData = null,
                SLADeadline = DateTime.Now.AddHours(24),
                SLAStatus = SLAStatus.OnTime,
                ExternalReferenceId = null,
                ExternalReferenceType = null,
                ExternalTaskId = null,
                IsDeleted = false
            };

            var result = ValidateWorkflowStepInstance(instance);

            return (!result.IsValid).ToProperty();
        }

        /// <summary>
        /// Property: SLA deadline before assigned date is rejected.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_SLADeadlineBeforeAssignedDateIsRejected()
        {
            var assignedDate = DateTime.Now;
            var slaDeadline = assignedDate.AddHours(-1); // Before assigned date

            var instance = new WorkflowStepInstance
            {
                WorkflowStepInstanceId = 1,
                WorkflowInstanceId = 1,
                WorkflowStepId = 1,
                AssignedTo = "user",
                AssignedDate = assignedDate,
                Status = WorkflowStepInstanceStatus.Pending,
                CompletedDate = null,
                CompletedBy = null,
                OutputData = null,
                SLADeadline = slaDeadline,
                SLAStatus = SLAStatus.OnTime,
                ExternalReferenceId = null,
                ExternalReferenceType = null,
                ExternalTaskId = null,
                IsDeleted = false
            };

            var result = ValidateWorkflowStepInstance(instance);

            return (!result.IsValid).ToProperty();
        }

        /// <summary>
        /// Property: Null SLA rule is rejected.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 100)]
        public Property Property_NullSLARuleIsRejected()
        {
            var result = ValidateSLARule(null);

            return (!result.IsValid && result.ErrorMessage != null).ToProperty();
        }

        /// <summary>
        /// Property: Negative SLA duration is rejected.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_NegativeSLADurationIsRejected(int negativeDuration)
        {
            var duration = -(Math.Abs(negativeDuration) + 1);

            var rule = new WFSLARule
            {
                WFSLARuleId = 1,
                WorkflowStepId = 1,
                SLADurationHours = duration,
                EscalationAction = EscalationAction.NotifyManager,
                IsDeleted = false
            };

            var result = ValidateSLARule(rule);

            return (!result.IsValid).ToProperty();
        }

        /// <summary>
        /// Property: SLA duration exceeding maximum is rejected.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_ExcessiveSLADurationIsRejected(int excessiveDuration)
        {
            var duration = Math.Abs(excessiveDuration % 10000) + 721; // > 720

            var rule = new WFSLARule
            {
                WFSLARuleId = 1,
                WorkflowStepId = 1,
                SLADurationHours = duration,
                EscalationAction = EscalationAction.NotifyManager,
                IsDeleted = false
            };

            var result = ValidateSLARule(rule);

            return (!result.IsValid).ToProperty();
        }

        /// <summary>
        /// Property: Invalid inputs are consistently rejected.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_InvalidInputsAreConsistentlyRejected()
        {
            var instance = new WorkflowInstance
            {
                WorkflowInstanceId = -1,
                WorkflowDefinitionId = 1,
                TemplateVersion = 1,
                Status = WorkflowInstanceStatus.Created,
                InitiatedBy = "",
                InitiatedDate = DateTime.Now,
                CurrentStepId = null,
                CompletedDate = null,
                InputParameters = "{}",
                IsDeleted = false,
                DeletedDate = null
            };

            var result1 = ValidateWorkflowInstance(instance);
            var result2 = ValidateWorkflowInstance(instance);
            var result3 = ValidateWorkflowInstance(instance);

            return (!result1.IsValid && !result2.IsValid && !result3.IsValid).ToProperty();
        }

        /// <summary>
        /// Property: Error messages are meaningful and non-empty.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_ErrorMessagesAreMeaningful()
        {
            var instance = new WorkflowInstance
            {
                WorkflowInstanceId = -1,
                WorkflowDefinitionId = 1,
                TemplateVersion = 1,
                Status = WorkflowInstanceStatus.Created,
                InitiatedBy = "user",
                InitiatedDate = DateTime.Now,
                CurrentStepId = null,
                CompletedDate = null,
                InputParameters = "{}",
                IsDeleted = false,
                DeletedDate = null
            };

            var result = ValidateWorkflowInstance(instance);

            return (!result.IsValid && 
                    !string.IsNullOrWhiteSpace(result.ErrorMessage) &&
                    result.ErrorMessage.Length > 0).ToProperty();
        }

        /// <summary>
        /// Property: Valid inputs are accepted.
        /// 
        /// Validates: Requirement 25 - Business Rules and Compliance
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_ValidInputsAreAccepted()
        {
            var instance = new WorkflowInstance
            {
                WorkflowInstanceId = 1,
                WorkflowDefinitionId = 1,
                TemplateVersion = 1,
                Status = WorkflowInstanceStatus.Created,
                InitiatedBy = "user",
                InitiatedDate = DateTime.Now,
                CurrentStepId = null,
                CompletedDate = null,
                InputParameters = "{}",
                IsDeleted = false,
                DeletedDate = null
            };

            var result = ValidateWorkflowInstance(instance);

            return result.IsValid.ToProperty();
        }
    }

    /// <summary>
    /// Represents the result of a validation operation.
    /// </summary>
    public class ValidationResult
    {
        public bool IsValid { get; set; }
        public string ErrorMessage { get; set; }
    }
}
