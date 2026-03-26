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
    /// Unit tests for PermissionService.
    /// Validates Requirements 6.1–6.9.
    /// </summary>
    [TestFixture]
    public class PermissionServiceTests
    {
        private IPermissionRepository _permRepo;
        private IFolderRepository _folderRepo;
        private IUserAccountRepository _userRepo;
        private PermissionService _svc;

        private const int UserId   = 42;
        private const int FolderId = 100;

        [SetUp]
        public void SetUp()
        {
            _permRepo   = Substitute.For<IPermissionRepository>();
            _folderRepo = Substitute.For<IFolderRepository>();
            _userRepo   = Substitute.For<IUserAccountRepository>();
            _svc        = new PermissionService(_permRepo, _folderRepo, _userRepo);

            // Default: user is an Officer
            _userRepo.GetById(UserId).Returns(new UserAccount
            {
                UserId = UserId,
                Role   = UserRole.Officer
            });
        }

        // -----------------------------------------------------------------------
        // Helpers
        // -----------------------------------------------------------------------

        private Folder MakeFolder(int id, int? parentId = null)
        {
            return new Folder { FolderId = id, ParentFolderId = parentId };
        }

        private FolderPermission MakeEntry(
            int folderId,
            PrincipalType principalType,
            int principalId,
            PermissionType permType,
            PermissionState state,
            bool inheritanceBroken = false)
        {
            return new FolderPermission
            {
                FolderId          = folderId,
                PrincipalType     = principalType,
                PrincipalId       = principalId,
                PermissionType    = permType,
                PermissionState   = state,
                InheritanceBroken = inheritanceBroken
            };
        }

        // -----------------------------------------------------------------------
        // Default deny — root folder with no ACL entries (Req 6.3, 6.8)
        // -----------------------------------------------------------------------

        [Test]
        public void CheckPermission_RootFolderNoAcl_ReturnsFalse()
        {
            _folderRepo.GetById(FolderId).Returns(MakeFolder(FolderId));
            _permRepo.GetAclEntries(FolderId).Returns(new List<FolderPermission>());

            Assert.That(_svc.CheckPermission(UserId, FolderId, PermissionType.Read), Is.False);
        }

        [Test]
        public void GetEffectivePermission_RootFolderNoAcl_AllDenied()
        {
            _folderRepo.GetById(FolderId).Returns(MakeFolder(FolderId));
            _permRepo.GetAclEntries(FolderId).Returns(new List<FolderPermission>());

            var ep = _svc.GetEffectivePermission(UserId, FolderId);

            Assert.That(ep.CanRead,        Is.False);
            Assert.That(ep.CanWrite,       Is.False);
            Assert.That(ep.CanDelete,      Is.False);
            Assert.That(ep.CanModify,      Is.False);
            Assert.That(ep.HasFullControl, Is.False);
        }

        // -----------------------------------------------------------------------
        // Explicit Allow (Req 6.1, 6.3)
        // -----------------------------------------------------------------------

        [Test]
        public void CheckPermission_UserHasAllowEntry_ReturnsTrue()
        {
            _folderRepo.GetById(FolderId).Returns(MakeFolder(FolderId));
            _permRepo.GetAclEntries(FolderId).Returns(new List<FolderPermission>
            {
                MakeEntry(FolderId, PrincipalType.User, UserId, PermissionType.Read, PermissionState.Allow)
            });

            Assert.That(_svc.CheckPermission(UserId, FolderId, PermissionType.Read), Is.True);
        }

        // -----------------------------------------------------------------------
        // Explicit Deny overrides Allow (Req 6.2, 6.3)
        // -----------------------------------------------------------------------

        [Test]
        public void CheckPermission_ExplicitDenyOverridesAllow_ReturnsFalse()
        {
            _folderRepo.GetById(FolderId).Returns(MakeFolder(FolderId));
            _permRepo.GetAclEntries(FolderId).Returns(new List<FolderPermission>
            {
                MakeEntry(FolderId, PrincipalType.User, UserId, PermissionType.Read, PermissionState.Allow),
                MakeEntry(FolderId, PrincipalType.User, UserId, PermissionType.Read, PermissionState.ExplicitDeny)
            });

            Assert.That(_svc.CheckPermission(UserId, FolderId, PermissionType.Read), Is.False);
        }

        [Test]
        public void CheckPermission_RoleDenyOverridesUserAllow_ReturnsFalse()
        {
            // User has Allow via direct entry, but their role has ExplicitDeny
            _folderRepo.GetById(FolderId).Returns(MakeFolder(FolderId));
            _permRepo.GetAclEntries(FolderId).Returns(new List<FolderPermission>
            {
                MakeEntry(FolderId, PrincipalType.User, UserId, PermissionType.Write, PermissionState.Allow),
                MakeEntry(FolderId, PrincipalType.Role, (int)UserRole.Officer, PermissionType.Write, PermissionState.ExplicitDeny)
            });

            Assert.That(_svc.CheckPermission(UserId, FolderId, PermissionType.Write), Is.False);
        }

        // -----------------------------------------------------------------------
        // FullControl covers all permission types (Req 6.1)
        // -----------------------------------------------------------------------

        [Test]
        public void CheckPermission_FullControlAllow_GrantsAllTypes()
        {
            _folderRepo.GetById(FolderId).Returns(MakeFolder(FolderId));
            _permRepo.GetAclEntries(FolderId).Returns(new List<FolderPermission>
            {
                MakeEntry(FolderId, PrincipalType.User, UserId, PermissionType.FullControl, PermissionState.Allow)
            });

            Assert.That(_svc.CheckPermission(UserId, FolderId, PermissionType.Read),   Is.True);
            Assert.That(_svc.CheckPermission(UserId, FolderId, PermissionType.Write),  Is.True);
            Assert.That(_svc.CheckPermission(UserId, FolderId, PermissionType.Delete), Is.True);
            Assert.That(_svc.CheckPermission(UserId, FolderId, PermissionType.Modify), Is.True);
        }

        [Test]
        public void CheckPermission_FullControlDeny_DeniesAllTypes()
        {
            _folderRepo.GetById(FolderId).Returns(MakeFolder(FolderId));
            _permRepo.GetAclEntries(FolderId).Returns(new List<FolderPermission>
            {
                MakeEntry(FolderId, PrincipalType.User, UserId, PermissionType.FullControl, PermissionState.ExplicitDeny)
            });

            Assert.That(_svc.CheckPermission(UserId, FolderId, PermissionType.Read),   Is.False);
            Assert.That(_svc.CheckPermission(UserId, FolderId, PermissionType.Write),  Is.False);
        }

        // -----------------------------------------------------------------------
        // Permission inheritance (Req 6.4)
        // -----------------------------------------------------------------------

        [Test]
        public void CheckPermission_NoAclOnChild_InheritsFromParent()
        {
            int parentId = 50;
            _folderRepo.GetById(FolderId).Returns(MakeFolder(FolderId, parentId));
            _folderRepo.GetById(parentId).Returns(MakeFolder(parentId));

            // Child has no ACL; parent has Allow for Read
            _permRepo.GetAclEntries(FolderId).Returns(new List<FolderPermission>());
            _permRepo.GetAclEntries(parentId).Returns(new List<FolderPermission>
            {
                MakeEntry(parentId, PrincipalType.User, UserId, PermissionType.Read, PermissionState.Allow)
            });

            Assert.That(_svc.CheckPermission(UserId, FolderId, PermissionType.Read), Is.True);
        }

        [Test]
        public void CheckPermission_InheritanceBrokenOnChild_DoesNotInheritFromParent()
        {
            int parentId = 50;
            _folderRepo.GetById(FolderId).Returns(MakeFolder(FolderId, parentId));
            _folderRepo.GetById(parentId).Returns(MakeFolder(parentId));

            // Child has an entry with InheritanceBroken = true but no Allow for Read
            _permRepo.GetAclEntries(FolderId).Returns(new List<FolderPermission>
            {
                MakeEntry(FolderId, PrincipalType.User, UserId, PermissionType.Write, PermissionState.Allow, inheritanceBroken: true)
            });
            _permRepo.GetAclEntries(parentId).Returns(new List<FolderPermission>
            {
                MakeEntry(parentId, PrincipalType.User, UserId, PermissionType.Read, PermissionState.Allow)
            });

            // Read should be denied because inheritance is broken at child
            Assert.That(_svc.CheckPermission(UserId, FolderId, PermissionType.Read), Is.False);
        }

        [Test]
        public void CheckPermission_MultiLevelInheritance_WalksToGrandparent()
        {
            int childId       = 100;
            int parentId      = 50;
            int grandparentId = 10;

            _folderRepo.GetById(childId).Returns(MakeFolder(childId, parentId));
            _folderRepo.GetById(parentId).Returns(MakeFolder(parentId, grandparentId));
            _folderRepo.GetById(grandparentId).Returns(MakeFolder(grandparentId));

            _permRepo.GetAclEntries(childId).Returns(new List<FolderPermission>());
            _permRepo.GetAclEntries(parentId).Returns(new List<FolderPermission>());
            _permRepo.GetAclEntries(grandparentId).Returns(new List<FolderPermission>
            {
                MakeEntry(grandparentId, PrincipalType.User, UserId, PermissionType.Read, PermissionState.Allow)
            });

            Assert.That(_svc.CheckPermission(UserId, childId, PermissionType.Read), Is.True);
        }

        // -----------------------------------------------------------------------
        // Role-based ACL entries (Req 6.6)
        // -----------------------------------------------------------------------

        [Test]
        public void CheckPermission_RoleAllowEntry_GrantsAccessToUserWithThatRole()
        {
            _folderRepo.GetById(FolderId).Returns(MakeFolder(FolderId));
            _permRepo.GetAclEntries(FolderId).Returns(new List<FolderPermission>
            {
                MakeEntry(FolderId, PrincipalType.Role, (int)UserRole.Officer, PermissionType.Read, PermissionState.Allow)
            });

            // UserId has Officer role (set in SetUp)
            Assert.That(_svc.CheckPermission(UserId, FolderId, PermissionType.Read), Is.True);
        }

        // -----------------------------------------------------------------------
        // SetFolderPermission (Req 6.5)
        // -----------------------------------------------------------------------

        [Test]
        public void SetFolderPermission_ManagerBreaksInheritance_SetsFlag()
        {
            var folder = new Folder { FolderId = FolderId };
            _folderRepo.GetById(FolderId).Returns(folder);

            var managerUser = new UserContext { UserId = 99, Role = UserRole.Manager };
            var req = new SetPermissionRequest
            {
                FolderId        = FolderId,
                PrincipalType   = PrincipalType.User,
                PrincipalId     = UserId,
                PermissionType  = PermissionType.Read,
                PermissionState = PermissionState.Allow,
                BreakInheritance = true
            };

            _svc.SetFolderPermission(req, managerUser);

            _permRepo.Received(1).Insert(Arg.Is<FolderPermission>(fp =>
                fp.InheritanceBroken == true &&
                fp.GrantedByUserId   == 99));
        }

        [Test]
        public void SetFolderPermission_OfficerCannotSetPermissions_Throws()
        {
            // Req 6.5: only Manager or Admin may set folder permissions.
            // An Officer calling SetFolderPermission must receive UnauthorizedAccessException.
            var officerUser = new UserContext { UserId = 55, Role = UserRole.Officer };
            var req = new SetPermissionRequest
            {
                FolderId         = FolderId,
                PrincipalType    = PrincipalType.User,
                PrincipalId      = UserId,
                PermissionType   = PermissionType.Read,
                PermissionState  = PermissionState.Allow,
                BreakInheritance = true
            };

            Assert.Throws<UnauthorizedAccessException>(() =>
                _svc.SetFolderPermission(req, officerUser));

            // No permission entry should have been inserted
            _permRepo.DidNotReceive().Insert(Arg.Any<FolderPermission>());
        }

        [Test]
        public void SetFolderPermission_FolderNotFound_Throws()
        {
            _folderRepo.GetById(999).Returns((Folder)null!);

            var req = new SetPermissionRequest { FolderId = 999 };
            Assert.Throws<InvalidOperationException>(() =>
                _svc.SetFolderPermission(req, new UserContext { UserId = 1, Role = UserRole.Admin }));
        }

        // -----------------------------------------------------------------------
        // Null guards
        // -----------------------------------------------------------------------

        [Test]
        public void Constructor_NullPermissionRepo_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new PermissionService(null!, _folderRepo, _userRepo));
        }

        [Test]
        public void Constructor_NullFolderRepo_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new PermissionService(_permRepo, null!, _userRepo));
        }

        [Test]
        public void Constructor_NullUserRepo_Throws()
        {
            Assert.Throws<ArgumentNullException>(() =>
                new PermissionService(_permRepo, _folderRepo, null!));
        }
    }
}
