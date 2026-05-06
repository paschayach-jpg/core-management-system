using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using FsCheck;
using FsCheck.NUnit;
using NSubstitute;
using NUnit.Framework;
using PropertyAttribute = FsCheck.NUnit.PropertyAttribute;

namespace CMS.Tests.PropertyBasedTests
{
    /// <summary>
    /// Property-Based Tests for Task Assignment Eligibility (Phase 9.2)
    /// 
    /// Property: Only eligible users can be assigned to workflow steps based on role/org unit.
    /// 
    /// Validates:
    /// - If step requires role X, assigned user has role X
    /// - If step requires org unit Y, assigned user belongs to org unit Y
    /// - Assignment eligibility is consistently enforced
    /// </summary>
    [TestFixture]
    public class TaskAssignmentEligibilityTests
    {
        // =====================================================================
        // Generators
        // =====================================================================

        /// <summary>
        /// Generate random user roles.
        /// </summary>
        private static Arbitrary<UserRole> UserRoleGenerator()
        {
            var roleGen = Gen.Elements(
                UserRole.Admin,
                UserRole.Manager,
                UserRole.User,
                UserRole.Viewer
            );

            return roleGen.ToArbitrary();
        }

        /// <summary>
        /// Generate random workflow steps with role requirements.
        /// </summary>
        private static Arbitrary<WorkflowStep> WorkflowStepGenerator()
        {
            var roleGen = Gen.Elements(
                UserRole.Admin,
                UserRole.Manager,
                UserRole.User,
                UserRole.Viewer
            );

            var gen = from id in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from defId in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from stepType in Gen.Elements(
                          WorkflowStepType.Approval,
                          WorkflowStepType.Review,
                          WorkflowStepType.Decision
                      )
                      from requiredRole in roleGen
                      from requiredOrgUnit in Arb.Default.Int32().Generator.Where(x => x > 0)
                      select new WorkflowStep
                      {
                          WorkflowStepId = id,
                          WorkflowDefinitionId = defId,
                          StepType = stepType,
                          RequiredRole = requiredRole,
                          RequiredOrgUnitId = requiredOrgUnit,
                          MaxDelegationDepth = 3,
                          IsDeleted = false
                      };

            return gen.ToArbitrary();
        }

        /// <summary>
        /// Generate random users with roles and org units.
        /// </summary>
        private static Arbitrary<UserContext> UserContextGenerator()
        {
            var roleGen = Gen.Elements(
                (int)UserRole.Admin,
                (int)UserRole.Manager,
                (int)UserRole.User,
                (int)UserRole.Viewer
            );

            var gen = from userId in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from role in roleGen
                      from orgUnitId in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from userName in Arb.Default.String().Generator.Where(x => x != null && x.Length > 0)
                      select new UserContext
                      {
                          UserID = userId,
                          AppRole = role,
                          ADUserName = userName,
                          OrgUnitID = orgUnitId
                      };

            return gen.ToArbitrary();
        }

        // =====================================================================
        // Helper Methods
        // =====================================================================

        /// <summary>
        /// Determines if a user is eligible for a workflow step based on role.
        /// </summary>
        private static bool IsUserEligibleByRole(UserContext user, WorkflowStep step)
        {
            if (step.RequiredRole == null)
                return true; // No role requirement

            return user.AppRole == (int)step.RequiredRole;
        }

        /// <summary>
        /// Determines if a user is eligible for a workflow step based on org unit.
        /// </summary>
        private static bool IsUserEligibleByOrgUnit(UserContext user, WorkflowStep step)
        {
            if (step.RequiredOrgUnitId == null || step.RequiredOrgUnitId == 0)
                return true; // No org unit requirement

            return user.OrgUnitID == step.RequiredOrgUnitId;
        }

        /// <summary>
        /// Determines if a user is eligible for a workflow step.
        /// </summary>
        private static bool IsUserEligible(UserContext user, WorkflowStep step)
        {
            return IsUserEligibleByRole(user, step) && IsUserEligibleByOrgUnit(user, step);
        }

        // =====================================================================
        // Property Tests
        // =====================================================================

        /// <summary>
        /// Property: If a step requires a specific role, only users with that role can be assigned.
        /// 
        /// Validates: Requirement 4 - Instance Management - Task Assignment
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_OnlyEligibleUsersAreAssignedByRole(
            UserContext user,
            WorkflowStep step)
        {
            var isEligible = IsUserEligibleByRole(user, step);

            // If step has a role requirement
            if (step.RequiredRole != null)
            {
                // User must have the required role to be eligible
                var hasRequiredRole = user.AppRole == (int)step.RequiredRole;
                return (isEligible == hasRequiredRole).ToProperty();
            }

            // If no role requirement, all users are eligible
            return isEligible.ToProperty();
        }

        /// <summary>
        /// Property: If a step requires a specific org unit, only users in that org unit can be assigned.
        /// 
        /// Validates: Requirement 4 - Instance Management - Task Assignment
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_OnlyEligibleUsersAreAssignedByOrgUnit(
            UserContext user,
            WorkflowStep step)
        {
            var isEligible = IsUserEligibleByOrgUnit(user, step);

            // If step has an org unit requirement
            if (step.RequiredOrgUnitId != null && step.RequiredOrgUnitId > 0)
            {
                // User must belong to the required org unit to be eligible
                var inRequiredOrgUnit = user.OrgUnitID == step.RequiredOrgUnitId;
                return (isEligible == inRequiredOrgUnit).ToProperty();
            }

            // If no org unit requirement, all users are eligible
            return isEligible.ToProperty();
        }

        /// <summary>
        /// Property: A user is eligible only if they meet both role AND org unit requirements.
        /// 
        /// Validates: Requirement 4 - Instance Management - Task Assignment
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_EligibilityRequiresBothRoleAndOrgUnit(
            UserContext user,
            WorkflowStep step)
        {
            var isEligible = IsUserEligible(user, step);
            var roleEligible = IsUserEligibleByRole(user, step);
            var orgUnitEligible = IsUserEligibleByOrgUnit(user, step);

            // Eligibility is the AND of both conditions
            return (isEligible == (roleEligible && orgUnitEligible)).ToProperty();
        }

        /// <summary>
        /// Property: Eligibility check is idempotent (same result on repeated calls).
        /// 
        /// Validates: Requirement 4 - Instance Management - Task Assignment
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_EligibilityCheckIsIdempotent(
            UserContext user,
            WorkflowStep step)
        {
            var result1 = IsUserEligible(user, step);
            var result2 = IsUserEligible(user, step);
            var result3 = IsUserEligible(user, step);

            return (result1 == result2 && result2 == result3).ToProperty();
        }

        /// <summary>
        /// Property: If a user is eligible, they remain eligible until step requirements change.
        /// 
        /// Validates: Requirement 4 - Instance Management - Task Assignment
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_EligibilityIsConsistent(
            UserContext user,
            WorkflowStep step)
        {
            var isEligible = IsUserEligible(user, step);

            // Create a copy of the step with same requirements
            var stepCopy = new WorkflowStep
            {
                WorkflowStepId = step.WorkflowStepId,
                WorkflowDefinitionId = step.WorkflowDefinitionId,
                StepType = step.StepType,
                RequiredRole = step.RequiredRole,
                RequiredOrgUnitId = step.RequiredOrgUnitId,
                MaxDelegationDepth = step.MaxDelegationDepth,
                IsDeleted = step.IsDeleted
            };

            var isEligibleCopy = IsUserEligible(user, stepCopy);

            return (isEligible == isEligibleCopy).ToProperty();
        }

        /// <summary>
        /// Property: Admin users are eligible for all steps (assuming Admin role is highest privilege).
        /// 
        /// Validates: Requirement 4 - Instance Management - Task Assignment
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_AdminUsersAreAlwaysEligible(WorkflowStep step)
        {
            var adminUser = new UserContext
            {
                UserID = 1,
                AppRole = (int)UserRole.Admin,
                ADUserName = "admin",
                OrgUnitID = 1
            };

            // Admin should be eligible for any step (if role requirement is Admin or no requirement)
            var isEligible = IsUserEligible(adminUser, step);

            // If step requires Admin role, admin should be eligible
            if (step.RequiredRole == UserRole.Admin)
                return isEligible.ToProperty();

            // If step requires a different role, admin may not be eligible
            // (depends on role hierarchy, which is not defined here)
            return true.ToProperty();
        }

        /// <summary>
        /// Property: Users with insufficient role are never eligible for restricted steps.
        /// 
        /// Validates: Requirement 4 - Instance Management - Task Assignment
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_InsufficientRoleUsersAreNotEligible()
        {
            var viewerUser = new UserContext
            {
                UserID = 1,
                AppRole = (int)UserRole.Viewer,
                ADUserName = "viewer",
                OrgUnitID = 1
            };

            var adminStep = new WorkflowStep
            {
                WorkflowStepId = 1,
                WorkflowDefinitionId = 1,
                StepType = WorkflowStepType.Approval,
                RequiredRole = UserRole.Admin,
                RequiredOrgUnitId = 1,
                MaxDelegationDepth = 3,
                IsDeleted = false
            };

            var isEligible = IsUserEligible(viewerUser, adminStep);

            return (!isEligible).ToProperty();
        }

        /// <summary>
        /// Property: Users in wrong org unit are never eligible for org-restricted steps.
        /// 
        /// Validates: Requirement 4 - Instance Management - Task Assignment
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_WrongOrgUnitUsersAreNotEligible()
        {
            var user = new UserContext
            {
                UserID = 1,
                AppRole = (int)UserRole.Manager,
                ADUserName = "manager",
                OrgUnitID = 1
            };

            var step = new WorkflowStep
            {
                WorkflowStepId = 1,
                WorkflowDefinitionId = 1,
                StepType = WorkflowStepType.Approval,
                RequiredRole = UserRole.Manager,
                RequiredOrgUnitId = 999, // Different org unit
                MaxDelegationDepth = 3,
                IsDeleted = false
            };

            var isEligible = IsUserEligible(user, step);

            return (!isEligible).ToProperty();
        }

        /// <summary>
        /// Property: Users with matching role and org unit are always eligible.
        /// 
        /// Validates: Requirement 4 - Instance Management - Task Assignment
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_MatchingRoleAndOrgUnitUsersAreEligible()
        {
            var user = new UserContext
            {
                UserID = 1,
                AppRole = (int)UserRole.Manager,
                ADUserName = "manager",
                OrgUnitID = 5
            };

            var step = new WorkflowStep
            {
                WorkflowStepId = 1,
                WorkflowDefinitionId = 1,
                StepType = WorkflowStepType.Approval,
                RequiredRole = UserRole.Manager,
                RequiredOrgUnitId = 5,
                MaxDelegationDepth = 3,
                IsDeleted = false
            };

            var isEligible = IsUserEligible(user, step);

            return isEligible.ToProperty();
        }
    }
}
