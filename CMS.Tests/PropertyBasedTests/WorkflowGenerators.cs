using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using FsCheck;

namespace CMS.Tests.PropertyBasedTests
{
    /// <summary>
    /// Generators for property-based testing of workflow engine.
    /// Provides FsCheck generators for workflow domain models.
    /// </summary>
    public static class WorkflowGenerators
    {
        // =====================================================================
        // Workflow Template Generators
        // =====================================================================

        /// <summary>
        /// Generate valid workflow templates with random names and versions.
        /// </summary>
        public static Arbitrary<WorkflowDefinition> WorkflowTemplateGenerator()
        {
            var gen = from id in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from name in Arb.Default.String().Generator.Where(x => x != null && x.Length > 0 && x.Length <= 100)
                      from description in Arb.Default.String().Generator.Where(x => x != null && x.Length <= 500)
                      from version in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from isActive in Arb.Default.Bool().Generator
                      from createdBy in Arb.Default.String().Generator.Where(x => x != null && x.Length > 0)
                      from createdDate in Arb.Default.DateTime().Generator
                      select new WorkflowDefinition
                      {
                          WorkflowDefinitionId = id,
                          Name = name,
                          Description = description,
                          Version = version,
                          IsActive = isActive,
                          CreatedBy = createdBy,
                          CreatedDate = createdDate,
                          ModifiedBy = createdBy,
                          ModifiedDate = createdDate,
                          IsDeleted = false,
                          DeletedDate = null
                      };

            return gen.ToArbitrary();
        }

        // =====================================================================
        // Workflow Step Generators
        // =====================================================================

        /// <summary>
        /// Generate valid workflow steps with random types and assignments.
        /// </summary>
        public static Arbitrary<WorkflowStep> WorkflowStepGenerator()
        {
            var stepTypeGen = Gen.Elements(
                WorkflowStepType.Start,
                WorkflowStepType.Approval,
                WorkflowStepType.Review,
                WorkflowStepType.Notification,
                WorkflowStepType.Decision,
                WorkflowStepType.End
            );

            var gen = from id in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from defId in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from stepName in Arb.Default.String().Generator.Where(x => x != null && x.Length > 0 && x.Length <= 100)
                      from stepType in stepTypeGen
                      from description in Arb.Default.String().Generator.Where(x => x != null && x.Length <= 500)
                      from assignedRole in Arb.Default.String().Generator.Where(x => x != null && x.Length <= 50)
                      from slaHours in Arb.Default.Int32().Generator.Where(x => x >= 0 && x <= 720)
                      from allowDelegation in Arb.Default.Bool().Generator
                      from maxDelegationDepth in Arb.Default.Int32().Generator.Where(x => x >= 0 && x <= 5)
                      select new WorkflowStep
                      {
                          WorkflowStepId = id,
                          WorkflowDefinitionId = defId,
                          StepName = stepName,
                          StepType = stepType,
                          Description = description,
                          AssignedRole = assignedRole,
                          AssignedUser = null,
                          AssignedOrgUnit = null,
                          SLAHours = slaHours > 0 ? slaHours : (int?)null,
                          SLADays = null,
                          AllowDelegation = allowDelegation,
                          MaxDelegationDepth = maxDelegationDepth,
                          SequenceNumber = 1,
                          IsDeleted = false
                      };

            return gen.ToArbitrary();
        }

        // =====================================================================
        // Workflow Instance Generators
        // =====================================================================

        /// <summary>
        /// Generate valid workflow instances with random statuses and timestamps.
        /// </summary>
        public static Arbitrary<WorkflowInstance> WorkflowInstanceGenerator()
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
                      from initiatedBy in Arb.Default.String().Generator.Where(x => x != null && x.Length > 0)
                      from initiatedDate in Arb.Default.DateTime().Generator
                      from status in statusGen
                      select new WorkflowInstance
                      {
                          WorkflowInstanceId = id,
                          WorkflowDefinitionId = defId,
                          TemplateVersion = version,
                          InitiatedBy = initiatedBy,
                          InitiatedDate = initiatedDate,
                          CurrentStepId = null,
                          Status = status,
                          CompletedDate = status == WorkflowInstanceStatus.Completed || 
                                         status == WorkflowInstanceStatus.Rejected ? 
                                         initiatedDate.AddHours(1) : (DateTime?)null,
                          InputParameters = "{}",
                          IsDeleted = false,
                          DeletedDate = null
                      };

            return gen.ToArbitrary();
        }

        // =====================================================================
        // Workflow Step Instance Generators
        // =====================================================================

        /// <summary>
        /// Generate valid workflow step instances with random statuses and SLA information.
        /// </summary>
        public static Arbitrary<WorkflowStepInstance> WorkflowStepInstanceGenerator()
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
                          CompletedDate = status == WorkflowStepInstanceStatus.Completed || 
                                         status == WorkflowStepInstanceStatus.Rejected ? 
                                         assignedDate.AddHours(1) : (DateTime?)null,
                          CompletedBy = status == WorkflowStepInstanceStatus.Completed || 
                                       status == WorkflowStepInstanceStatus.Rejected ? 
                                       assignedTo : null,
                          OutputData = "{}",
                          SLADeadline = slaDeadline,
                          SLAStatus = slaStatus,
                          IsDeleted = false
                      };

            return gen.ToArbitrary();
        }

        // =====================================================================
        // Delegation Generators
        // =====================================================================

        /// <summary>
        /// Generate valid delegation records with random users and depths.
        /// </summary>
        public static Arbitrary<WFDelegation> DelegationGenerator()
        {
            var gen = from id in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from stepInstanceId in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from delegatedFrom in Arb.Default.String().Generator.Where(x => x != null && x.Length > 0)
                      from delegatedTo in Arb.Default.String().Generator.Where(x => x != null && x.Length > 0 && x != delegatedFrom)
                      from reason in Arb.Default.String().Generator.Where(x => x != null && x.Length <= 500)
                      from delegationDate in Arb.Default.DateTime().Generator
                      from depth in Arb.Default.Int32().Generator.Where(x => x >= 1 && x <= 5)
                      from isActive in Arb.Default.Bool().Generator
                      select new WFDelegation
                      {
                          WFDelegationId = id,
                          WorkflowStepInstanceId = stepInstanceId,
                          DelegatedFrom = delegatedFrom,
                          DelegatedTo = delegatedTo,
                          DelegationReason = reason,
                          DelegationDate = delegationDate,
                          DelegationDepth = depth,
                          IsActive = isActive,
                          IsDeleted = false
                      };

            return gen.ToArbitrary();
        }

        // =====================================================================
        // SLA Rule Generators
        // =====================================================================

        /// <summary>
        /// Generate valid SLA rules with random durations and escalation actions.
        /// </summary>
        public static Arbitrary<WFSLARule> SLARuleGenerator()
        {
            var escalationActionGen = Gen.Elements(
                EscalationAction.NotifyManager,
                EscalationAction.IncreasePriority,
                EscalationAction.Reassign,
                EscalationAction.AutoReject
            );

            var gen = from id in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from stepId in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from durationHours in Arb.Default.Int32().Generator.Where(x => x > 0 && x <= 720)
                      from warningThreshold in Arb.Default.Int32().Generator.Where(x => x >= 50 && x <= 100)
                      from escalationAction in escalationActionGen
                      from escalationRecipient in Arb.Default.String().Generator.Where(x => x != null && x.Length > 0)
                      from isActive in Arb.Default.Bool().Generator
                      from createdDate in Arb.Default.DateTime().Generator
                      select new WFSLARule
                      {
                          WFSLARuleId = id,
                          WorkflowStepId = stepId,
                          SLADurationHours = durationHours,
                          WarningThresholdPercent = warningThreshold,
                          EscalationAction = escalationAction,
                          EscalationRecipient = escalationRecipient,
                          IsActive = isActive,
                          CreatedDate = createdDate,
                          ModifiedDate = createdDate
                      };

            return gen.ToArbitrary();
        }

        // =====================================================================
        // Execution Sequence Generators
        // =====================================================================

        /// <summary>
        /// Generate valid workflow execution sequences (lists of actions).
        /// </summary>
        public static Gen<List<WorkflowAction>> ExecutionSequenceGenerator()
        {
            var actionGen = Gen.Elements(
                WorkflowAction.Complete,
                WorkflowAction.Reject,
                WorkflowAction.Suspend,
                WorkflowAction.Resume
            );

            return Gen.ListOf(actionGen);
        }

        /// <summary>
        /// Generate valid task creation times and SLA durations.
        /// </summary>
        public static Gen<(DateTime CreationTime, int SLAHours)> TaskCreationAndSLAGenerator()
        {
            var creationTimeGen = Arb.Default.DateTime().Generator;
            var slaHoursGen = Arb.Default.Int32().Generator.Where(x => x > 0 && x <= 720);

            return from creationTime in creationTimeGen
                   from slaHours in slaHoursGen
                   select (creationTime, slaHours);
        }

        /// <summary>
        /// Generate valid user roles and task assignments.
        /// </summary>
        public static Gen<(string Role, string User)> UserRoleAndAssignmentGenerator()
        {
            var roles = new[] { "Approver", "Reviewer", "Manager", "User", "Admin" };
            var roleGen = Gen.Elements(roles);
            var userGen = Arb.Default.String().Generator.Where(x => x != null && x.Length > 0 && x.Length <= 50);

            return from role in roleGen
                   from user in userGen
                   select (role, user);
        }

        /// <summary>
        /// Generate valid audit log operations.
        /// </summary>
        public static Gen<string> AuditLogOperationGenerator()
        {
            var operations = new[]
            {
                "CREATE",
                "UPDATE",
                "DELETE",
                "COMPLETE",
                "REJECT",
                "DELEGATE",
                "ESCALATE",
                "APPROVE",
                "DENY"
            };

            return Gen.Elements(operations);
        }

        /// <summary>
        /// Generate valid condition expressions.
        /// </summary>
        public static Gen<string> ConditionExpressionGenerator()
        {
            var conditions = new[]
            {
                "status == 'approved'",
                "amount > 1000",
                "priority == 'high'",
                "department == 'finance'",
                "days_elapsed > 5",
                "retry_count < 3",
                "is_urgent == true",
                "budget_available > 0"
            };

            return Gen.Elements(conditions);
        }

        /// <summary>
        /// Generate valid invalid inputs for error handling testing.
        /// </summary>
        public static Gen<object> InvalidInputGenerator()
        {
            var invalidInputs = new object[]
            {
                null,
                "",
                -1,
                -999,
                new DateTime(1900, 1, 1),
                new DateTime(2100, 12, 31),
                "   ",
                "x".PadRight(1001, 'x'),
                double.NaN,
                double.PositiveInfinity
            };

            return Gen.Elements(invalidInputs);
        }

        /// <summary>
        /// Generate valid delegation chains with depth information.
        /// </summary>
        public static Gen<List<(string From, string To, int Depth)>> DelegationChainGenerator()
        {
            var userGen = Arb.Default.String().Generator.Where(x => x != null && x.Length > 0 && x.Length <= 50);
            var depthGen = Arb.Default.Int32().Generator.Where(x => x >= 1 && x <= 5);

            var delegationGen = from from_ in userGen
                               from to in userGen.Where(x => x != from_)
                               from depth in depthGen
                               select (from_, to, depth);

            return Gen.ListOf(delegationGen);
        }

        /// <summary>
        /// Generate valid workflow instances for dashboard performance testing.
        /// </summary>
        public static Gen<List<WorkflowInstance>> DashboardWorkflowInstancesGenerator()
        {
            var statusGen = Gen.Elements(
                WorkflowInstanceStatus.Created,
                WorkflowInstanceStatus.InProgress,
                WorkflowInstanceStatus.Completed,
                WorkflowInstanceStatus.Rejected,
                WorkflowInstanceStatus.Suspended
            );

            var instanceGen = from id in Arb.Default.Int32().Generator.Where(x => x > 0)
                             from defId in Arb.Default.Int32().Generator.Where(x => x > 0)
                             from initiatedBy in Arb.Default.String().Generator.Where(x => x != null && x.Length > 0)
                             from initiatedDate in Arb.Default.DateTime().Generator
                             from status in statusGen
                             select new WorkflowInstance
                             {
                                 WorkflowInstanceId = id,
                                 WorkflowDefinitionId = defId,
                                 TemplateVersion = 1,
                                 InitiatedBy = initiatedBy,
                                 InitiatedDate = initiatedDate,
                                 Status = status,
                                 CompletedDate = status == WorkflowInstanceStatus.Completed ? 
                                               initiatedDate.AddHours(1) : (DateTime?)null,
                                 InputParameters = "{}",
                                 IsDeleted = false
                             };

            return Gen.ListOf(instanceGen).Where(list => list.Count <= 1000);
        }
    }

    /// <summary>
    /// Represents a workflow action that can be performed on a workflow instance.
    /// </summary>
    public enum WorkflowAction
    {
        Complete = 1,
        Reject = 2,
        Suspend = 3,
        Resume = 4
    }
}
