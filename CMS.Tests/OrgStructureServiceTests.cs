using System;
using System.Collections.Generic;
using NUnit.Framework;
using NSubstitute;
using CMS.Data.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;
using CMS.Services;
using CMS.Services.DTOs;

namespace CMS.Tests
{
    /// <summary>
    /// Unit tests for OrgStructureService.
    /// Validates Requirements 3.1–3.10.
    /// </summary>
    [TestFixture]
    public class OrgStructureServiceTests
    {
        private IOrgUnitRepository _repo;
        private OrgStructureService _svc;
        private UserContext _adminUser;

        [SetUp]
        public void SetUp()
        {
            _repo = Substitute.For<IOrgUnitRepository>();
            _svc  = new OrgStructureService(_repo);
            _adminUser = new UserContext { UserId = 1, Role = UserRole.Admin, WorkstationId = "WS1" };
        }

        // -----------------------------------------------------------------------
        // CreateOrgUnit — SubCommittee (tier 1)
        // -----------------------------------------------------------------------

        [Test]
        public void CreateOrgUnit_SubCommittee_NoParent_Succeeds()
        {
            _repo.Insert(Arg.Any<OrgUnit>()).Returns(10);

            var req = new CreateOrgUnitRequest { Tier = OrgUnitTier.SubCommittee, Name = "SC-Alpha" };
            var result = _svc.CreateOrgUnit(req, _adminUser);

            Assert.That(result.OrgUnitId, Is.EqualTo(10));
            Assert.That(result.Tier, Is.EqualTo(OrgUnitTier.SubCommittee));
            Assert.That(result.IsActive, Is.True);
        }

        [Test]
        public void CreateOrgUnit_SubCommittee_WithParent_Throws()
        {
            var req = new CreateOrgUnitRequest
            {
                Tier = OrgUnitTier.SubCommittee,
                Name = "SC-Beta",
                ParentId = 5
            };

            Assert.Throws<ArgumentException>(() => _svc.CreateOrgUnit(req, _adminUser));
        }

        // -----------------------------------------------------------------------
        // CreateOrgUnit — Division (tier 2) requires SubCommittee parent (Req 3.3)
        // -----------------------------------------------------------------------

        [Test]
        public void CreateOrgUnit_Division_UnderSubCommittee_Succeeds()
        {
            var parent = new OrgUnit { OrgUnitId = 1, Tier = OrgUnitTier.SubCommittee, IsActive = true };
            _repo.GetById(1).Returns(parent);
            _repo.Insert(Arg.Any<OrgUnit>()).Returns(20);

            var req = new CreateOrgUnitRequest { Tier = OrgUnitTier.Division, Name = "Div-1", ParentId = 1 };
            var result = _svc.CreateOrgUnit(req, _adminUser);

            Assert.That(result.OrgUnitId, Is.EqualTo(20));
            Assert.That(result.Tier, Is.EqualTo(OrgUnitTier.Division));
        }

        [Test]
        public void CreateOrgUnit_Division_WithoutParent_Throws()
        {
            var req = new CreateOrgUnitRequest { Tier = OrgUnitTier.Division, Name = "Div-Orphan" };
            Assert.Throws<ArgumentException>(() => _svc.CreateOrgUnit(req, _adminUser));
        }

        [Test]
        public void CreateOrgUnit_Division_UnderDivision_Throws()
        {
            var parent = new OrgUnit { OrgUnitId = 2, Tier = OrgUnitTier.Division, IsActive = true };
            _repo.GetById(2).Returns(parent);

            var req = new CreateOrgUnitRequest { Tier = OrgUnitTier.Division, Name = "Div-Bad", ParentId = 2 };
            Assert.Throws<ArgumentException>(() => _svc.CreateOrgUnit(req, _adminUser));
        }

        [Test]
        public void CreateOrgUnit_Division_UnderInactiveParent_Throws()
        {
            var parent = new OrgUnit { OrgUnitId = 1, Tier = OrgUnitTier.SubCommittee, IsActive = false };
            _repo.GetById(1).Returns(parent);

            var req = new CreateOrgUnitRequest { Tier = OrgUnitTier.Division, Name = "Div-X", ParentId = 1 };
            Assert.Throws<ArgumentException>(() => _svc.CreateOrgUnit(req, _adminUser));
        }

        // -----------------------------------------------------------------------
        // CreateOrgUnit — Department (tier 3) requires Division parent (Req 3.4)
        // -----------------------------------------------------------------------

        [Test]
        public void CreateOrgUnit_Department_UnderDivision_Succeeds()
        {
            var parent = new OrgUnit { OrgUnitId = 2, Tier = OrgUnitTier.Division, IsActive = true };
            _repo.GetById(2).Returns(parent);
            _repo.Insert(Arg.Any<OrgUnit>()).Returns(30);

            var req = new CreateOrgUnitRequest { Tier = OrgUnitTier.Department, Name = "Dept-1", ParentId = 2 };
            var result = _svc.CreateOrgUnit(req, _adminUser);

            Assert.That(result.OrgUnitId, Is.EqualTo(30));
        }

        [Test]
        public void CreateOrgUnit_Department_WithoutParent_Throws()
        {
            var req = new CreateOrgUnitRequest { Tier = OrgUnitTier.Department, Name = "Dept-Orphan" };
            Assert.Throws<ArgumentException>(() => _svc.CreateOrgUnit(req, _adminUser));
        }

        [Test]
        public void CreateOrgUnit_Department_UnderSubCommittee_Throws()
        {
            var parent = new OrgUnit { OrgUnitId = 1, Tier = OrgUnitTier.SubCommittee, IsActive = true };
            _repo.GetById(1).Returns(parent);

            var req = new CreateOrgUnitRequest { Tier = OrgUnitTier.Department, Name = "Dept-Bad", ParentId = 1 };
            Assert.Throws<ArgumentException>(() => _svc.CreateOrgUnit(req, _adminUser));
        }

        // -----------------------------------------------------------------------
        // CreateOrgUnit — null/empty guards
        // -----------------------------------------------------------------------

        [Test]
        public void CreateOrgUnit_NullRequest_Throws()
        {
            Assert.Throws<ArgumentNullException>(() => _svc.CreateOrgUnit(null!, _adminUser));
        }

        [Test]
        public void CreateOrgUnit_EmptyName_Throws()
        {
            var req = new CreateOrgUnitRequest { Tier = OrgUnitTier.SubCommittee, Name = "  " };
            Assert.Throws<ArgumentException>(() => _svc.CreateOrgUnit(req, _adminUser));
        }

        // -----------------------------------------------------------------------
        // UpdateOrgUnit (Req 3.5)
        // -----------------------------------------------------------------------

        [Test]
        public void UpdateOrgUnit_ActiveUnit_UpdatesName()
        {
            var existing = new OrgUnit { OrgUnitId = 5, Name = "Old Name", IsActive = true };
            _repo.GetById(5).Returns(existing);

            var req = new UpdateOrgUnitRequest { OrgUnitId = 5, NewName = "New Name" };
            var result = _svc.UpdateOrgUnit(req, _adminUser);

            Assert.That(result.Name, Is.EqualTo("New Name"));
            _repo.Received(1).Update(Arg.Is<OrgUnit>(u => u.Name == "New Name"));
        }

        [Test]
        public void UpdateOrgUnit_InactiveUnit_Throws()
        {
            var existing = new OrgUnit { OrgUnitId = 5, Name = "Old", IsActive = false };
            _repo.GetById(5).Returns(existing);

            var req = new UpdateOrgUnitRequest { OrgUnitId = 5, NewName = "New" };
            Assert.Throws<InvalidOperationException>(() => _svc.UpdateOrgUnit(req, _adminUser));
        }

        [Test]
        public void UpdateOrgUnit_NotFound_Throws()
        {
            _repo.GetById(99).Returns((OrgUnit)null!);

            var req = new UpdateOrgUnitRequest { OrgUnitId = 99, NewName = "X" };
            Assert.Throws<InvalidOperationException>(() => _svc.UpdateOrgUnit(req, _adminUser));
        }

        // -----------------------------------------------------------------------
        // DeactivateOrgUnit (Req 3.6, 3.7)
        // -----------------------------------------------------------------------

        [Test]
        public void DeactivateOrgUnit_NoDependents_Deactivates()
        {
            var existing = new OrgUnit { OrgUnitId = 5, Name = "SC", IsActive = true };
            _repo.GetById(5).Returns(existing);
            _repo.HasActiveDependents(5).Returns(false);

            _svc.DeactivateOrgUnit(5, _adminUser);

            _repo.Received(1).Deactivate(5);
        }

        [Test]
        public void DeactivateOrgUnit_WithActiveDependents_Throws()
        {
            var existing = new OrgUnit { OrgUnitId = 5, Name = "SC", IsActive = true };
            _repo.GetById(5).Returns(existing);
            _repo.HasActiveDependents(5).Returns(true);

            Assert.Throws<InvalidOperationException>(() => _svc.DeactivateOrgUnit(5, _adminUser));
            _repo.DidNotReceive().Deactivate(Arg.Any<int>());
        }

        [Test]
        public void DeactivateOrgUnit_AlreadyInactive_IsIdempotent()
        {
            var existing = new OrgUnit { OrgUnitId = 5, Name = "SC", IsActive = false };
            _repo.GetById(5).Returns(existing);

            // Should not throw and should not call Deactivate again
            Assert.DoesNotThrow(() => _svc.DeactivateOrgUnit(5, _adminUser));
            _repo.DidNotReceive().Deactivate(Arg.Any<int>());
        }

        [Test]
        public void DeactivateOrgUnit_NotFound_Throws()
        {
            _repo.GetById(99).Returns((OrgUnit)null!);
            Assert.Throws<InvalidOperationException>(() => _svc.DeactivateOrgUnit(99, _adminUser));
        }

        // -----------------------------------------------------------------------
        // GetHierarchy / GetByConfidentialityLevel
        // -----------------------------------------------------------------------

        [Test]
        public void GetHierarchy_DelegatesToRepository()
        {
            var units = new List<OrgUnit> { new OrgUnit { OrgUnitId = 1 } };
            _repo.GetHierarchy().Returns(units);

            var result = _svc.GetHierarchy();

            Assert.That(result, Is.SameAs(units));
        }

        [Test]
        public void GetByConfidentialityLevel_DelegatesToRepository()
        {
            var units = new List<OrgUnit> { new OrgUnit { OrgUnitId = 2 } };
            _repo.GetByConfidentialityLevel(ConfidentialityLevel.HC).Returns(units);

            var result = _svc.GetByConfidentialityLevel(ConfidentialityLevel.HC);

            Assert.That(result, Is.SameAs(units));
        }
    }
}
