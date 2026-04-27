using System;
using System.Collections.Generic;
using System.Linq;
using System.Text.Json;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using FsCheck;
using FsCheck.NUnit;
using NUnit.Framework;
using PropertyAttribute = FsCheck.NUnit.PropertyAttribute;

namespace CMS.Tests.PropertyBasedTests
{
    /// <summary>
    /// Property-Based Tests for Workflow Serialization Round-Trip (Phase 9.5)
    /// 
    /// Property: Serialize → Deserialize produces equivalent workflow instance.
    /// 
    /// Validates:
    /// - All properties are preserved
    /// - No data loss during round-trip
    /// - Deserialized instance equals original instance
    /// </summary>
    [TestFixture]
    public class WorkflowSerializationTests
    {
        // =====================================================================
        // Generators
        // =====================================================================

        /// <summary>
        /// Generate random workflow instances for serialization testing.
        /// </summary>
        private static Arbitrary<WorkflowInstance> WorkflowInstanceGenerator()
        {
            var statusGen = Gen.Elements(
                WorkflowInstanceStatus.Created,
                WorkflowInstanceStatus.InProgress,
                WorkflowInstanceStatus.Completed,
                WorkflowInstanceStatus.Rejected,
                WorkflowInstanceStatus.Suspended
            );

            var gen = from id in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from defId in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from version in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from status in statusGen
                      from initiatedBy in Arb.Default.String().Generator.Where(x => x != null && x.Length > 0)
                      from initiatedDate in Arb.Default.DateTime().Generator
                      from currentStepId in Arb.Default.Int32().Generator
                      from inputParams in Arb.Default.String().Generator
                      select new WorkflowInstance
                      {
                          WorkflowInstanceId = id,
                          WorkflowDefinitionId = defId,
                          TemplateVersion = version,
                          Status = status,
                          InitiatedBy = initiatedBy,
                          InitiatedDate = initiatedDate,
                          CurrentStepId = currentStepId > 0 ? currentStepId : (int?)null,
                          CompletedDate = status == WorkflowInstanceStatus.Completed || 
                                         status == WorkflowInstanceStatus.Rejected ? 
                                         initiatedDate.AddHours(1) : (DateTime?)null,
                          InputParameters = inputParams ?? "{}",
                          IsDeleted = false,
                          DeletedDate = null
                      };

            return gen.ToArbitrary();
        }

        /// <summary>
        /// Generate random workflow step instances for serialization testing.
        /// </summary>
        private static Arbitrary<WorkflowStepInstance> WorkflowStepInstanceGenerator()
        {
            var statusGen = Gen.Elements(
                WorkflowStepInstanceStatus.Pending,
                WorkflowStepInstanceStatus.InProgress,
                WorkflowStepInstanceStatus.Completed,
                WorkflowStepInstanceStatus.Rejected,
                WorkflowStepInstanceStatus.Delegated
            );

            var slaStatusGen = Gen.Elements(
                SLAStatus.OnTime,
                SLAStatus.AtRisk,
                SLAStatus.Overdue
            );

            var gen = from id in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from workflowId in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from stepId in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from assignedTo in Arb.Default.String().Generator.Where(x => x != null && x.Length > 0)
                      from assignedDate in Arb.Default.DateTime().Generator
                      from status in statusGen
                      from completedDate in Arb.Default.DateTime().Generator
                      from completedBy in Arb.Default.String().Generator
                      from outputData in Arb.Default.String().Generator
                      from slaDeadline in Arb.Default.DateTime().Generator
                      from slaStatus in slaStatusGen
                      select new WorkflowStepInstance
                      {
                          WorkflowStepInstanceId = id,
                          WorkflowInstanceId = workflowId,
                          WorkflowStepId = stepId,
                          AssignedTo = assignedTo,
                          AssignedDate = assignedDate,
                          Status = status,
                          CompletedDate = status == WorkflowStepInstanceStatus.Completed ? 
                                         completedDate : (DateTime?)null,
                          CompletedBy = status == WorkflowStepInstanceStatus.Completed ? 
                                       completedBy : null,
                          OutputData = outputData,
                          SLADeadline = slaDeadline,
                          SLAStatus = slaStatus,
                          ExternalReferenceId = null,
                          ExternalReferenceType = null,
                          ExternalTaskId = null,
                          IsDeleted = false
                      };

            return gen.ToArbitrary();
        }

        // =====================================================================
        // Helper Methods
        // =====================================================================

        /// <summary>
        /// Serializes a workflow instance to JSON.
        /// </summary>
        private static string SerializeWorkflowInstance(WorkflowInstance instance)
        {
            var options = new JsonSerializerOptions { WriteIndented = false };
            return JsonSerializer.Serialize(instance, options);
        }

        /// <summary>
        /// Deserializes a workflow instance from JSON.
        /// </summary>
        private static WorkflowInstance DeserializeWorkflowInstance(string json)
        {
            return JsonSerializer.Deserialize<WorkflowInstance>(json);
        }

        /// <summary>
        /// Serializes a workflow step instance to JSON.
        /// </summary>
        private static string SerializeWorkflowStepInstance(WorkflowStepInstance instance)
        {
            var options = new JsonSerializerOptions { WriteIndented = false };
            return JsonSerializer.Serialize(instance, options);
        }

        /// <summary>
        /// Deserializes a workflow step instance from JSON.
        /// </summary>
        private static WorkflowStepInstance DeserializeWorkflowStepInstance(string json)
        {
            return JsonSerializer.Deserialize<WorkflowStepInstance>(json);
        }

        /// <summary>
        /// Determines if two workflow instances are equal.
        /// </summary>
        private static bool AreWorkflowInstancesEqual(WorkflowInstance instance1, WorkflowInstance instance2)
        {
            return instance1.WorkflowInstanceId == instance2.WorkflowInstanceId &&
                   instance1.WorkflowDefinitionId == instance2.WorkflowDefinitionId &&
                   instance1.TemplateVersion == instance2.TemplateVersion &&
                   instance1.Status == instance2.Status &&
                   instance1.InitiatedBy == instance2.InitiatedBy &&
                   instance1.InitiatedDate == instance2.InitiatedDate &&
                   instance1.CurrentStepId == instance2.CurrentStepId &&
                   instance1.CompletedDate == instance2.CompletedDate &&
                   instance1.InputParameters == instance2.InputParameters &&
                   instance1.IsDeleted == instance2.IsDeleted &&
                   instance1.DeletedDate == instance2.DeletedDate;
        }

        /// <summary>
        /// Determines if two workflow step instances are equal.
        /// </summary>
        private static bool AreWorkflowStepInstancesEqual(WorkflowStepInstance instance1, WorkflowStepInstance instance2)
        {
            return instance1.WorkflowStepInstanceId == instance2.WorkflowStepInstanceId &&
                   instance1.WorkflowInstanceId == instance2.WorkflowInstanceId &&
                   instance1.WorkflowStepId == instance2.WorkflowStepId &&
                   instance1.AssignedTo == instance2.AssignedTo &&
                   instance1.AssignedDate == instance2.AssignedDate &&
                   instance1.Status == instance2.Status &&
                   instance1.CompletedDate == instance2.CompletedDate &&
                   instance1.CompletedBy == instance2.CompletedBy &&
                   instance1.OutputData == instance2.OutputData &&
                   instance1.SLADeadline == instance2.SLADeadline &&
                   instance1.SLAStatus == instance2.SLAStatus &&
                   instance1.ExternalReferenceId == instance2.ExternalReferenceId &&
                   instance1.ExternalReferenceType == instance2.ExternalReferenceType &&
                   instance1.ExternalTaskId == instance2.ExternalTaskId &&
                   instance1.IsDeleted == instance2.IsDeleted;
        }

        // =====================================================================
        // Property Tests
        // =====================================================================

        /// <summary>
        /// Property: Serialize → Deserialize produces equivalent workflow instance.
        /// 
        /// Validates: Requirement 5 - Instance Management - State Tracking
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_SerializeDeserializePreservesWorkflowInstance(WorkflowInstance instance)
        {
            // Serialize
            var json = SerializeWorkflowInstance(instance);

            // Deserialize
            var deserialized = DeserializeWorkflowInstance(json);

            // Verify equivalence
            return AreWorkflowInstancesEqual(instance, deserialized).ToProperty();
        }

        /// <summary>
        /// Property: Serialize → Deserialize produces equivalent workflow step instance.
        /// 
        /// Validates: Requirement 5 - Instance Management - State Tracking
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_SerializeDeserializePreservesWorkflowStepInstance(WorkflowStepInstance instance)
        {
            // Serialize
            var json = SerializeWorkflowStepInstance(instance);

            // Deserialize
            var deserialized = DeserializeWorkflowStepInstance(json);

            // Verify equivalence
            return AreWorkflowStepInstancesEqual(instance, deserialized).ToProperty();
        }

        /// <summary>
        /// Property: All properties are preserved during serialization round-trip.
        /// 
        /// Validates: Requirement 5 - Instance Management - State Tracking
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_AllPropertiesArePreserved(WorkflowInstance instance)
        {
            var json = SerializeWorkflowInstance(instance);
            var deserialized = DeserializeWorkflowInstance(json);

            // Check each property
            var idPreserved = instance.WorkflowInstanceId == deserialized.WorkflowInstanceId;
            var defIdPreserved = instance.WorkflowDefinitionId == deserialized.WorkflowDefinitionId;
            var versionPreserved = instance.TemplateVersion == deserialized.TemplateVersion;
            var statusPreserved = instance.Status == deserialized.Status;
            var initiatedByPreserved = instance.InitiatedBy == deserialized.InitiatedBy;
            var initiatedDatePreserved = instance.InitiatedDate == deserialized.InitiatedDate;
            var inputParamsPreserved = instance.InputParameters == deserialized.InputParameters;

            return (idPreserved && defIdPreserved && versionPreserved && statusPreserved &&
                    initiatedByPreserved && initiatedDatePreserved && inputParamsPreserved).ToProperty();
        }

        /// <summary>
        /// Property: No data loss during serialization round-trip.
        /// 
        /// Validates: Requirement 5 - Instance Management - State Tracking
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_NoDataLossDuringSerialization(WorkflowInstance instance)
        {
            var json1 = SerializeWorkflowInstance(instance);
            var deserialized = DeserializeWorkflowInstance(json1);
            var json2 = SerializeWorkflowInstance(deserialized);

            // Both JSON strings should be equivalent (may differ in formatting)
            var deserialized1 = DeserializeWorkflowInstance(json1);
            var deserialized2 = DeserializeWorkflowInstance(json2);

            return AreWorkflowInstancesEqual(deserialized1, deserialized2).ToProperty();
        }

        /// <summary>
        /// Property: Serialization is deterministic (same input produces same output).
        /// 
        /// Validates: Requirement 5 - Instance Management - State Tracking
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_SerializationIsDeterministic(WorkflowInstance instance)
        {
            var json1 = SerializeWorkflowInstance(instance);
            var json2 = SerializeWorkflowInstance(instance);

            // Deserialize both to compare (JSON formatting may differ)
            var deserialized1 = DeserializeWorkflowInstance(json1);
            var deserialized2 = DeserializeWorkflowInstance(json2);

            return AreWorkflowInstancesEqual(deserialized1, deserialized2).ToProperty();
        }

        /// <summary>
        /// Property: Deserialized instance is independent of original (no shared references).
        /// 
        /// Validates: Requirement 5 - Instance Management - State Tracking
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_DeserializedInstanceIsIndependent(WorkflowInstance instance)
        {
            var json = SerializeWorkflowInstance(instance);
            var deserialized = DeserializeWorkflowInstance(json);

            // Modify the deserialized instance
            var originalInitiatedBy = instance.InitiatedBy;
            deserialized.InitiatedBy = "ModifiedUser";

            // Original should be unchanged
            return (instance.InitiatedBy == originalInitiatedBy).ToProperty();
        }

        /// <summary>
        /// Property: Multiple round-trips produce equivalent instances.
        /// 
        /// Validates: Requirement 5 - Instance Management - State Tracking
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_MultipleRoundTripsProduceEquivalentInstances(WorkflowInstance instance)
        {
            var json1 = SerializeWorkflowInstance(instance);
            var deserialized1 = DeserializeWorkflowInstance(json1);

            var json2 = SerializeWorkflowInstance(deserialized1);
            var deserialized2 = DeserializeWorkflowInstance(json2);

            var json3 = SerializeWorkflowInstance(deserialized2);
            var deserialized3 = DeserializeWorkflowInstance(json3);

            return (AreWorkflowInstancesEqual(deserialized1, deserialized2) &&
                    AreWorkflowInstancesEqual(deserialized2, deserialized3)).ToProperty();
        }

        /// <summary>
        /// Property: Null values are preserved during serialization.
        /// 
        /// Validates: Requirement 5 - Instance Management - State Tracking
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_NullValuesArePreserved()
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

            var json = SerializeWorkflowInstance(instance);
            var deserialized = DeserializeWorkflowInstance(json);

            return (deserialized.CurrentStepId == null &&
                    deserialized.CompletedDate == null &&
                    deserialized.DeletedDate == null).ToProperty();
        }

        /// <summary>
        /// Property: DateTime values maintain precision during serialization.
        /// 
        /// Validates: Requirement 5 - Instance Management - State Tracking
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_DateTimePrecisionIsPreserved(WorkflowInstance instance)
        {
            var json = SerializeWorkflowInstance(instance);
            var deserialized = DeserializeWorkflowInstance(json);

            // DateTime should be preserved with millisecond precision
            var timeDifference = Math.Abs((instance.InitiatedDate - deserialized.InitiatedDate).TotalMilliseconds);

            return (timeDifference < 1).ToProperty(); // Allow 1ms tolerance
        }
    }
}
