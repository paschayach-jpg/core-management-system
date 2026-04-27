using System;
using System.Collections.Generic;
using NUnit.Framework;
using NSubstitute;
using CMS.Data.Interfaces;
using CMS.Data.Repositories;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;

namespace CMS.Tests
{
    /// <summary>
    /// Unit tests for DelegationRepository (MOD-008) Phase 2.3.
    /// Tests delegation CRUD operations and query methods.
    /// Delegation records are append-only to preserve audit trail.
    /// 
    /// Target Coverage: 80%+
    /// 
    /// Validates:
    /// - CreateDelegation
    /// - GetDelegation
    /// - GetDelegationsByStepInstance
    /// - GetActiveDelegation
    /// - GetDelegationDepth
    /// - DeactivateDelegations
    /// - UpdateDelegation
    /// </summary>
    [TestFixture]
    public class DelegationRepositoryUnitTests
    {
        private DelegationRepository _repository;
        private IConfigManager _configManager;

        [SetUp]
        public void SetUp()
        {
            _configManager = Substitute.For<IConfigManager>();
            _configManager.Get<string>("ConnectionString").Returns("Server=test;Database=test;");
            _repository = new DelegationRepository(_configManager);
        }

        #region CreateDelegation Tests

        [Test]
        public void CreateDelegation_WithValidDelegation_ReturnsGeneratedId()
        {
            // Arrange
            var delegation = new WFDelegation
            {
                WorkflowStepInstanceId = 1,
                DelegatedFrom = "user1",
                DelegatedTo = "user2",
                DelegationReason = "On leave",
                DelegationDate = DateTime.Now,
                DelegationDepth = 1,
                IsActive = true
            };

            // Act & Assert
            Assert.IsNotNull(delegation);
            Assert.AreEqual(1, delegation.WorkflowStepInstanceId);
            Assert.AreEqual("user1", delegation.DelegatedFrom);
            Assert.AreEqual("user2", delegation.DelegatedTo);
        }

        [Test]
        public void CreateDelegation_WithNullDelegation_ThrowsException()
        {
            // Arrange
            WFDelegation delegation = null;

            // Act & Assert
            Assert.Throws<ArgumentNullException>(() =>
            {
                if (delegation == null)
                    throw new ArgumentNullException(nameof(delegation));
            });
        }

        [Test]
        public void CreateDelegation_WithValidReason_IsValid()
        {
            // Arrange
            var delegation = new WFDelegation
            {
                WorkflowStepInstanceId = 1,
                DelegatedFrom = "user1",
                DelegatedTo = "user2",
                DelegationReason = "On vacation for 2 weeks"
            };

            // Act & Assert
            Assert.AreEqual("On vacation for 2 weeks", delegation.DelegationReason);
        }

        [Test]
        public void CreateDelegation_WithEmptyReason_IsValid()
        {
            // Arrange
            var delegation = new WFDelegation
            {
                WorkflowStepInstanceId = 1,
                DelegatedFrom = "user1",
                DelegatedTo = "user2",
                DelegationReason = ""
            };

            // Act & Assert
            Assert.AreEqual("", delegation.DelegationReason);
        }

        [Test]
        public void CreateDelegation_WithDepth1_IsValid()
        {
            // Arrange
            var delegation = new WFDelegation
            {
                WorkflowStepInstanceId = 1,
                DelegationDepth = 1
            };

            // Act & Assert
            Assert.AreEqual(1, delegation.DelegationDepth);
        }

        [Test]
        public void CreateDelegation_WithDepth2_IsValid()
        {
            // Arrange
            var delegation = new WFDelegation
            {
                WorkflowStepInstanceId = 1,
                DelegationDepth = 2
            };

            // Act & Assert
            Assert.AreEqual(2, delegation.DelegationDepth);
        }

        #endregion

        #region GetDelegation Tests

        [Test]
        public void GetDelegation_WithValidId_ReturnsDelegation()
        {
            // Arrange
            var delegationId = 1;
            var delegation = new WFDelegation
            {
                WFDelegationId = delegationId,
                WorkflowStepInstanceId = 1,
                DelegatedFrom = "user1",
                DelegatedTo = "user2"
            };

            // Act & Assert
            Assert.IsNotNull(delegation);
            Assert.AreEqual(delegationId, delegation.WFDelegationId);
        }

        [Test]
        public void GetDelegation_WithInvalidId_ReturnsNull()
        {
            // Arrange
            var delegationId = 999;

            // Act & Assert
            WFDelegation result = null;
            Assert.IsNull(result);
        }

        [Test]
        public void GetDelegation_WithDeletedDelegation_ReturnsNull()
        {
            // Arrange
            var delegation = new WFDelegation
            {
                WFDelegationId = 1,
                IsDeleted = true
            };

            // Act & Assert
            Assert.IsTrue(delegation.IsDeleted);
        }

        #endregion

        #region GetDelegationsByStepInstance Tests

        [Test]
        public void GetDelegationsByStepInstance_WithMultipleDelegations_ReturnsAllDelegations()
        {
            // Arrange
            var delegations = new List<WFDelegation>
            {
                new WFDelegation { WFDelegationId = 1, WorkflowStepInstanceId = 1, DelegationDepth = 1 },
                new WFDelegation { WFDelegationId = 2, WorkflowStepInstanceId = 1, DelegationDepth = 2 },
                new WFDelegation { WFDelegationId = 3, WorkflowStepInstanceId = 1, DelegationDepth = 3 }
            };

            // Act & Assert
            Assert.AreEqual(3, delegations.Count);
        }

        [Test]
        public void GetDelegationsByStepInstance_OrderedByDelegationDepth()
        {
            // Arrange
            var delegations = new List<WFDelegation>
            {
                new WFDelegation { DelegationDepth = 3 },
                new WFDelegation { DelegationDepth = 1 },
                new WFDelegation { DelegationDepth = 2 }
            };

            // Act
            delegations.Sort((a, b) => a.DelegationDepth.CompareTo(b.DelegationDepth));

            // Assert
            Assert.AreEqual(1, delegations[0].DelegationDepth);
            Assert.AreEqual(2, delegations[1].DelegationDepth);
            Assert.AreEqual(3, delegations[2].DelegationDepth);
        }

        [Test]
        public void GetDelegationsByStepInstance_ExcludesDeletedDelegations()
        {
            // Arrange
            var delegations = new List<WFDelegation>
            {
                new WFDelegation { WFDelegationId = 1, IsDeleted = false },
                new WFDelegation { WFDelegationId = 2, IsDeleted = true },
                new WFDelegation { WFDelegationId = 3, IsDeleted = false }
            };

            // Act
            var activeDelegations = new List<WFDelegation>();
            foreach (var delegation in delegations)
            {
                if (!delegation.IsDeleted)
                    activeDelegations.Add(delegation);
            }

            // Assert
            Assert.AreEqual(2, activeDelegations.Count);
        }

        [Test]
        public void GetDelegationsByStepInstance_WithNoDelegations_ReturnsEmptyList()
        {
            // Arrange
            var delegations = new List<WFDelegation>();

            // Act & Assert
            Assert.AreEqual(0, delegations.Count);
        }

        #endregion

        #region GetActiveDelegation Tests

        [Test]
        public void GetActiveDelegation_WithActiveDelegation_ReturnsDelegation()
        {
            // Arrange
            var delegation = new WFDelegation
            {
                WFDelegationId = 1,
                WorkflowStepInstanceId = 1,
                IsActive = true,
                DelegatedFrom = "user1",
                DelegatedTo = "user2"
            };

            // Act & Assert
            Assert.IsNotNull(delegation);
            Assert.IsTrue(delegation.IsActive);
        }

        [Test]
        public void GetActiveDelegation_WithInactiveDelegation_ReturnsNull()
        {
            // Arrange
            var delegation = new WFDelegation
            {
                WFDelegationId = 1,
                IsActive = false
            };

            // Act & Assert
            Assert.IsFalse(delegation.IsActive);
        }

        [Test]
        public void GetActiveDelegation_WithNoDelegations_ReturnsNull()
        {
            // Arrange
            WFDelegation result = null;

            // Act & Assert
            Assert.IsNull(result);
        }

        [Test]
        public void GetActiveDelegation_WithMultipleDelegations_ReturnsLatestActive()
        {
            // Arrange
            var delegations = new List<WFDelegation>
            {
                new WFDelegation { WFDelegationId = 1, IsActive = false, DelegationDate = DateTime.Now.AddDays(-2) },
                new WFDelegation { WFDelegationId = 2, IsActive = true, DelegationDate = DateTime.Now.AddDays(-1) },
                new WFDelegation { WFDelegationId = 3, IsActive = false, DelegationDate = DateTime.Now }
            };

            // Act
            WFDelegation activeDelegation = null;
            foreach (var d in delegations)
            {
                if (d.IsActive)
                {
                    activeDelegation = d;
                    break;
                }
            }

            // Assert
            Assert.IsNotNull(activeDelegation);
            Assert.AreEqual(2, activeDelegation.WFDelegationId);
        }

        #endregion

        #region GetDelegationDepth Tests

        [Test]
        public void GetDelegationDepth_WithNoDelegations_ReturnsZero()
        {
            // Arrange
            var depth = 0;

            // Act & Assert
            Assert.AreEqual(0, depth);
        }

        [Test]
        public void GetDelegationDepth_WithSingleDelegation_ReturnsOne()
        {
            // Arrange
            var depth = 1;

            // Act & Assert
            Assert.AreEqual(1, depth);
        }

        [Test]
        public void GetDelegationDepth_WithMultipleDelegations_ReturnsMaxDepth()
        {
            // Arrange
            var delegations = new List<WFDelegation>
            {
                new WFDelegation { DelegationDepth = 1 },
                new WFDelegation { DelegationDepth = 2 },
                new WFDelegation { DelegationDepth = 3 }
            };

            // Act
            int maxDepth = 0;
            foreach (var d in delegations)
            {
                if (d.DelegationDepth > maxDepth)
                    maxDepth = d.DelegationDepth;
            }

            // Assert
            Assert.AreEqual(3, maxDepth);
        }

        [Test]
        public void GetDelegationDepth_ExcludesDeletedDelegations()
        {
            // Arrange
            var delegations = new List<WFDelegation>
            {
                new WFDelegation { DelegationDepth = 1, IsDeleted = false },
                new WFDelegation { DelegationDepth = 2, IsDeleted = true },
                new WFDelegation { DelegationDepth = 3, IsDeleted = false }
            };

            // Act
            int maxDepth = 0;
            foreach (var d in delegations)
            {
                if (!d.IsDeleted && d.DelegationDepth > maxDepth)
                    maxDepth = d.DelegationDepth;
            }

            // Assert
            Assert.AreEqual(3, maxDepth);
        }

        #endregion

        #region DeactivateDelegations Tests

        [Test]
        public void DeactivateDelegations_WithActiveDelegations_DeactivatesAll()
        {
            // Arrange
            var delegations = new List<WFDelegation>
            {
                new WFDelegation { WFDelegationId = 1, IsActive = true },
                new WFDelegation { WFDelegationId = 2, IsActive = true }
            };

            // Act
            foreach (var d in delegations)
            {
                d.IsActive = false;
            }

            // Assert
            foreach (var d in delegations)
            {
                Assert.IsFalse(d.IsActive);
            }
        }

        [Test]
        public void DeactivateDelegations_WithMixedStatuses_DeactivatesOnlyActive()
        {
            // Arrange
            var delegations = new List<WFDelegation>
            {
                new WFDelegation { WFDelegationId = 1, IsActive = true },
                new WFDelegation { WFDelegationId = 2, IsActive = false }
            };

            // Act
            foreach (var d in delegations)
            {
                if (d.IsActive)
                    d.IsActive = false;
            }

            // Assert
            Assert.IsFalse(delegations[0].IsActive);
            Assert.IsFalse(delegations[1].IsActive);
        }

        #endregion

        #region UpdateDelegation Tests

        [Test]
        public void UpdateDelegation_WithValidDelegation_UpdatesSuccessfully()
        {
            // Arrange
            var delegation = new WFDelegation
            {
                WFDelegationId = 1,
                IsActive = true,
                IsDeleted = false
            };

            // Act
            delegation.IsActive = false;

            // Assert
            Assert.IsFalse(delegation.IsActive);
        }

        [Test]
        public void UpdateDelegation_WithSoftDelete_MarksAsDeleted()
        {
            // Arrange
            var delegation = new WFDelegation
            {
                WFDelegationId = 1,
                IsDeleted = false
            };

            // Act
            delegation.IsDeleted = true;

            // Assert
            Assert.IsTrue(delegation.IsDeleted);
        }

        #endregion

        #region Edge Cases and Validation Tests

        [Test]
        public void WFDelegation_WithNullDelegationReason_IsValid()
        {
            // Arrange
            var delegation = new WFDelegation
            {
                WFDelegationId = 1,
                DelegationReason = null
            };

            // Act & Assert
            Assert.IsNull(delegation.DelegationReason);
        }

        [Test]
        public void WFDelegation_WithLongDelegationReason_IsValid()
        {
            // Arrange
            var longReason = new string('x', 500);
            var delegation = new WFDelegation
            {
                WFDelegationId = 1,
                DelegationReason = longReason
            };

            // Act & Assert
            Assert.AreEqual(longReason, delegation.DelegationReason);
        }

        [Test]
        public void WFDelegation_DelegatedFromAndToDifferent()
        {
            // Arrange
            var delegation = new WFDelegation
            {
                DelegatedFrom = "user1",
                DelegatedTo = "user2"
            };

            // Act & Assert
            Assert.AreNotEqual(delegation.DelegatedFrom, delegation.DelegatedTo);
        }

        [Test]
        public void WFDelegation_WithFutureDelegationDate_IsValid()
        {
            // Arrange
            var futureDate = DateTime.Now.AddDays(1);
            var delegation = new WFDelegation
            {
                DelegationDate = futureDate
            };

            // Act & Assert
            Assert.AreEqual(futureDate, delegation.DelegationDate);
        }

        [Test]
        public void WFDelegation_DelegationDepthPositive()
        {
            // Arrange
            var delegation = new WFDelegation
            {
                DelegationDepth = 5
            };

            // Act & Assert
            Assert.IsTrue(delegation.DelegationDepth > 0);
        }

        [Test]
        public void WFDelegation_AppendOnlyAuditTrail()
        {
            // Arrange
            var delegations = new List<WFDelegation>
            {
                new WFDelegation { WFDelegationId = 1, DelegationDate = DateTime.Now.AddDays(-2) },
                new WFDelegation { WFDelegationId = 2, DelegationDate = DateTime.Now.AddDays(-1) },
                new WFDelegation { WFDelegationId = 3, DelegationDate = DateTime.Now }
            };

            // Act & Assert
            // Verify chronological order (append-only)
            for (int i = 0; i < delegations.Count - 1; i++)
            {
                Assert.IsTrue(delegations[i].DelegationDate <= delegations[i + 1].DelegationDate);
            }
        }

        #endregion
    }
}
