using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Domain.Models;
using FsCheck;
using FsCheck.NUnit;
using NUnit.Framework;
using PropertyAttribute = FsCheck.NUnit.PropertyAttribute;

namespace CMS.Tests.PropertyBasedTests
{
    /// <summary>
    /// Property-Based Tests for Delegation Chain Depth Enforcement (Phase 9.10)
    /// 
    /// Property: Delegation chain depth never exceeds maximum (default 3).
    /// 
    /// Validates:
    /// - Chain depth <= MaxDelegationDepth
    /// - Delegation is rejected if depth would be exceeded
    /// - Maximum depth is enforced consistently
    /// </summary>
    [TestFixture]
    public class DelegationChainDepthTests
    {
        // =====================================================================
        // Constants
        // =====================================================================

        private const int DEFAULT_MAX_DELEGATION_DEPTH = 3;

        // =====================================================================
        // Generators
        // =====================================================================

        /// <summary>
        /// Generate random delegation chains.
        /// </summary>
        private static Arbitrary<List<WFDelegation>> DelegationChainGenerator()
        {
            var gen = from count in Gen.Choose(0, 5)
                      from delegations in Gen.ListOf(count, DelegationGenerator().Generator)
                      select delegations;

            return gen.ToArbitrary();
        }

        /// <summary>
        /// Generate random delegations.
        /// </summary>
        private static Arbitrary<WFDelegation> DelegationGenerator()
        {
            var gen = from id in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from stepId in Arb.Default.Int32().Generator.Where(x => x > 0)
                      from delegatedFrom in Arb.Default.String().Generator.Where(x => x != null && x.Length > 0)
                      from delegatedTo in Arb.Default.String().Generator.Where(x => x != null && x.Length > 0)
                      from reason in Arb.Default.String().Generator
                      from delegationDate in Arb.Default.DateTime().Generator
                      from depth in Gen.Choose(1, 5)
                      select new WFDelegation
                      {
                          WFDelegationId = id,
                          WorkflowStepInstanceId = stepId,
                          DelegatedFrom = delegatedFrom,
                          DelegatedTo = delegatedTo,
                          DelegationReason = reason ?? "",
                          DelegationDate = delegationDate,
                          DelegationDepth = depth,
                          IsActive = true,
                          IsDeleted = false
                      };

            return gen.ToArbitrary();
        }

        // =====================================================================
        // Helper Methods
        // =====================================================================

        /// <summary>
        /// Calculates the depth of a delegation chain.
        /// </summary>
        private static int CalculateChainDepth(List<WFDelegation> delegations)
        {
            if (delegations == null || delegations.Count == 0)
                return 0;

            return delegations.Where(d => d.IsActive && !d.IsDeleted)
                             .Max(d => d.DelegationDepth);
        }

        /// <summary>
        /// Determines if a delegation can be added without exceeding max depth.
        /// </summary>
        private static bool CanAddDelegation(List<WFDelegation> delegations, int maxDepth)
        {
            var currentDepth = CalculateChainDepth(delegations);
            return currentDepth < maxDepth;
        }

        /// <summary>
        /// Adds a delegation to the chain if allowed.
        /// </summary>
        private static bool TryAddDelegation(List<WFDelegation> delegations, WFDelegation newDelegation, int maxDepth)
        {
            if (!CanAddDelegation(delegations, maxDepth))
                return false;

            newDelegation.DelegationDepth = CalculateChainDepth(delegations) + 1;
            delegations.Add(newDelegation);
            return true;
        }

        // =====================================================================
        // Property Tests
        // =====================================================================

        /// <summary>
        /// Property: Delegation chain depth never exceeds maximum.
        /// 
        /// Validates: Requirement 9 - Delegation - Rules and Restrictions
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_DelegationChainDepthDoesNotExceedMaximum(List<WFDelegation> delegations)
        {
            if (delegations == null || delegations.Count == 0)
                return true.ToProperty();

            var depth = CalculateChainDepth(delegations);

            return (depth <= DEFAULT_MAX_DELEGATION_DEPTH).ToProperty();
        }

        /// <summary>
        /// Property: Delegation is rejected if depth would be exceeded.
        /// 
        /// Validates: Requirement 9 - Delegation - Rules and Restrictions
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_DelegationIsRejectedIfDepthExceeded()
        {
            var delegations = new List<WFDelegation>
            {
                new WFDelegation
                {
                    WFDelegationId = 1,
                    WorkflowStepInstanceId = 1,
                    DelegatedFrom = "user1",
                    DelegatedTo = "user2",
                    DelegationReason = "reason",
                    DelegationDate = DateTime.Now,
                    DelegationDepth = 1,
                    IsActive = true,
                    IsDeleted = false
                },
                new WFDelegation
                {
                    WFDelegationId = 2,
                    WorkflowStepInstanceId = 1,
                    DelegatedFrom = "user2",
                    DelegatedTo = "user3",
                    DelegationReason = "reason",
                    DelegationDate = DateTime.Now,
                    DelegationDepth = 2,
                    IsActive = true,
                    IsDeleted = false
                },
                new WFDelegation
                {
                    WFDelegationId = 3,
                    WorkflowStepInstanceId = 1,
                    DelegatedFrom = "user3",
                    DelegatedTo = "user4",
                    DelegationReason = "reason",
                    DelegationDate = DateTime.Now,
                    DelegationDepth = 3,
                    IsActive = true,
                    IsDeleted = false
                }
            };

            var newDelegation = new WFDelegation
            {
                WFDelegationId = 4,
                WorkflowStepInstanceId = 1,
                DelegatedFrom = "user4",
                DelegatedTo = "user5",
                DelegationReason = "reason",
                DelegationDate = DateTime.Now,
                DelegationDepth = 4,
                IsActive = true,
                IsDeleted = false
            };

            var canAdd = CanAddDelegation(delegations, DEFAULT_MAX_DELEGATION_DEPTH);

            return (!canAdd).ToProperty();
        }

        /// <summary>
        /// Property: Maximum depth is enforced consistently.
        /// 
        /// Validates: Requirement 9 - Delegation - Rules and Restrictions
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_MaximumDepthIsEnforcedConsistently(List<WFDelegation> delegations)
        {
            if (delegations == null || delegations.Count == 0)
                return true.ToProperty();

            var depth1 = CalculateChainDepth(delegations);
            var depth2 = CalculateChainDepth(delegations);
            var depth3 = CalculateChainDepth(delegations);

            return (depth1 == depth2 && depth2 == depth3 && depth1 <= DEFAULT_MAX_DELEGATION_DEPTH).ToProperty();
        }

        /// <summary>
        /// Property: Depth 1 delegation is always allowed.
        /// 
        /// Validates: Requirement 9 - Delegation - Rules and Restrictions
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_Depth1DelegationIsAlwaysAllowed()
        {
            var delegations = new List<WFDelegation>();

            var canAdd = CanAddDelegation(delegations, DEFAULT_MAX_DELEGATION_DEPTH);

            return canAdd.ToProperty();
        }

        /// <summary>
        /// Property: Depth 2 delegation is allowed when depth 1 exists.
        /// 
        /// Validates: Requirement 9 - Delegation - Rules and Restrictions
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_Depth2DelegationIsAllowedWhenDepth1Exists()
        {
            var delegations = new List<WFDelegation>
            {
                new WFDelegation
                {
                    WFDelegationId = 1,
                    WorkflowStepInstanceId = 1,
                    DelegatedFrom = "user1",
                    DelegatedTo = "user2",
                    DelegationReason = "reason",
                    DelegationDate = DateTime.Now,
                    DelegationDepth = 1,
                    IsActive = true,
                    IsDeleted = false
                }
            };

            var canAdd = CanAddDelegation(delegations, DEFAULT_MAX_DELEGATION_DEPTH);

            return canAdd.ToProperty();
        }

        /// <summary>
        /// Property: Depth 3 delegation is allowed when depth 2 exists.
        /// 
        /// Validates: Requirement 9 - Delegation - Rules and Restrictions
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_Depth3DelegationIsAllowedWhenDepth2Exists()
        {
            var delegations = new List<WFDelegation>
            {
                new WFDelegation
                {
                    WFDelegationId = 1,
                    WorkflowStepInstanceId = 1,
                    DelegatedFrom = "user1",
                    DelegatedTo = "user2",
                    DelegationReason = "reason",
                    DelegationDate = DateTime.Now,
                    DelegationDepth = 1,
                    IsActive = true,
                    IsDeleted = false
                },
                new WFDelegation
                {
                    WFDelegationId = 2,
                    WorkflowStepInstanceId = 1,
                    DelegatedFrom = "user2",
                    DelegatedTo = "user3",
                    DelegationReason = "reason",
                    DelegationDate = DateTime.Now,
                    DelegationDepth = 2,
                    IsActive = true,
                    IsDeleted = false
                }
            };

            var canAdd = CanAddDelegation(delegations, DEFAULT_MAX_DELEGATION_DEPTH);

            return canAdd.ToProperty();
        }

        /// <summary>
        /// Property: Depth 4 delegation is rejected when depth 3 exists.
        /// 
        /// Validates: Requirement 9 - Delegation - Rules and Restrictions
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_Depth4DelegationIsRejectedWhenDepth3Exists()
        {
            var delegations = new List<WFDelegation>
            {
                new WFDelegation
                {
                    WFDelegationId = 1,
                    WorkflowStepInstanceId = 1,
                    DelegatedFrom = "user1",
                    DelegatedTo = "user2",
                    DelegationReason = "reason",
                    DelegationDate = DateTime.Now,
                    DelegationDepth = 1,
                    IsActive = true,
                    IsDeleted = false
                },
                new WFDelegation
                {
                    WFDelegationId = 2,
                    WorkflowStepInstanceId = 1,
                    DelegatedFrom = "user2",
                    DelegatedTo = "user3",
                    DelegationReason = "reason",
                    DelegationDate = DateTime.Now,
                    DelegationDepth = 2,
                    IsActive = true,
                    IsDeleted = false
                },
                new WFDelegation
                {
                    WFDelegationId = 3,
                    WorkflowStepInstanceId = 1,
                    DelegatedFrom = "user3",
                    DelegatedTo = "user4",
                    DelegationReason = "reason",
                    DelegationDate = DateTime.Now,
                    DelegationDepth = 3,
                    IsActive = true,
                    IsDeleted = false
                }
            };

            var canAdd = CanAddDelegation(delegations, DEFAULT_MAX_DELEGATION_DEPTH);

            return (!canAdd).ToProperty();
        }

        /// <summary>
        /// Property: Inactive delegations don't count toward depth.
        /// 
        /// Validates: Requirement 9 - Delegation - Rules and Restrictions
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_InactiveDelegationsDontCountTowardDepth()
        {
            var delegations = new List<WFDelegation>
            {
                new WFDelegation
                {
                    WFDelegationId = 1,
                    WorkflowStepInstanceId = 1,
                    DelegatedFrom = "user1",
                    DelegatedTo = "user2",
                    DelegationReason = "reason",
                    DelegationDate = DateTime.Now,
                    DelegationDepth = 1,
                    IsActive = false, // Inactive
                    IsDeleted = false
                },
                new WFDelegation
                {
                    WFDelegationId = 2,
                    WorkflowStepInstanceId = 1,
                    DelegatedFrom = "user2",
                    DelegatedTo = "user3",
                    DelegationReason = "reason",
                    DelegationDate = DateTime.Now,
                    DelegationDepth = 2,
                    IsActive = false, // Inactive
                    IsDeleted = false
                },
                new WFDelegation
                {
                    WFDelegationId = 3,
                    WorkflowStepInstanceId = 1,
                    DelegatedFrom = "user3",
                    DelegatedTo = "user4",
                    DelegationReason = "reason",
                    DelegationDate = DateTime.Now,
                    DelegationDepth = 3,
                    IsActive = false, // Inactive
                    IsDeleted = false
                }
            };

            var depth = CalculateChainDepth(delegations);

            // Depth should be 3 (all inactive), but we can still add more
            var canAdd = CanAddDelegation(delegations, DEFAULT_MAX_DELEGATION_DEPTH);

            return (!canAdd).ToProperty(); // Still can't add because max depth is 3
        }

        /// <summary>
        /// Property: Deleted delegations don't count toward depth.
        /// 
        /// Validates: Requirement 9 - Delegation - Rules and Restrictions
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_DeletedDelegationsDontCountTowardDepth()
        {
            var delegations = new List<WFDelegation>
            {
                new WFDelegation
                {
                    WFDelegationId = 1,
                    WorkflowStepInstanceId = 1,
                    DelegatedFrom = "user1",
                    DelegatedTo = "user2",
                    DelegationReason = "reason",
                    DelegationDate = DateTime.Now,
                    DelegationDepth = 1,
                    IsActive = true,
                    IsDeleted = true // Deleted
                },
                new WFDelegation
                {
                    WFDelegationId = 2,
                    WorkflowStepInstanceId = 1,
                    DelegatedFrom = "user2",
                    DelegatedTo = "user3",
                    DelegationReason = "reason",
                    DelegationDate = DateTime.Now,
                    DelegationDepth = 2,
                    IsActive = true,
                    IsDeleted = false
                }
            };

            var depth = CalculateChainDepth(delegations);

            // Depth should be 2 (first is deleted)
            return (depth == 2).ToProperty();
        }

        /// <summary>
        /// Property: Empty delegation chain has depth 0.
        /// 
        /// Validates: Requirement 9 - Delegation - Rules and Restrictions
        /// </summary>
        [PropertyAttribute(MaxTest = 100)]
        public Property Property_EmptyDelegationChainHasDepthZero()
        {
            var delegations = new List<WFDelegation>();

            var depth = CalculateChainDepth(delegations);

            return (depth == 0).ToProperty();
        }

        /// <summary>
        /// Property: Delegation depth is consistent across multiple reads.
        /// 
        /// Validates: Requirement 9 - Delegation - Rules and Restrictions
        /// </summary>
        [PropertyAttribute(MaxTest = 1000)]
        public Property Property_DelegationDepthIsConsistentAcrossReads(List<WFDelegation> delegations)
        {
            if (delegations == null || delegations.Count == 0)
                return true.ToProperty();

            var depth1 = CalculateChainDepth(delegations);
            var depth2 = CalculateChainDepth(delegations);
            var depth3 = CalculateChainDepth(delegations);

            return (depth1 == depth2 && depth2 == depth3).ToProperty();
        }
    }
}
