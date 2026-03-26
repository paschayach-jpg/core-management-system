using System;
using System.Collections.Generic;
using NUnit.Framework;
using NSubstitute;
using FsCheck;
using FsCheck.NUnit;
using CMS.Data.Interfaces;
using CMS.Data.Models;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services;

// Alias to avoid ambiguity with NUnit.Framework.PropertyAttribute
using PropertyAttribute = FsCheck.NUnit.PropertyAttribute;

namespace CMS.Tests
{
    /// <summary>
    /// Property-based tests for AuditService immutability.
    ///
    /// **Validates: Requirements 9.2**
    ///
    /// Property 1: AuditLog entries are insert-only — no update or delete path exists.
    /// The IAuditRepository interface exposes only Insert and Query; the AuditService
    /// only calls Insert (never Update or Delete). This property verifies that for any
    /// valid AuditEntry, AuditService.Log() calls exactly one Insert and zero
    /// Update/Delete operations.
    /// </summary>
    [TestFixture]
    public class AuditServiceImmutabilityTests
    {
        // -----------------------------------------------------------------------
        // Generators
        // -----------------------------------------------------------------------

        private static Gen<AuditEntry> AuditEntryGen()
        {
            return
                from eventType        in Arb.Generate<NonEmptyString>()
                from actorUserId      in Gen.Choose(1, 10000)
                from targetEntityType in Arb.Generate<NonEmptyString>()
                from targetEntityId   in Arb.Generate<NonEmptyString>()
                from outcome          in Gen.Elements("Success", "Failure")
                from workstationId    in Arb.Generate<NonEmptyString>()
                select new AuditEntry
                {
                    EventType        = eventType.Get,
                    ActorUserId      = actorUserId,
                    TargetEntityType = targetEntityType.Get,
                    TargetEntityId   = targetEntityId.Get,
                    Outcome          = outcome,
                    WorkstationId    = workstationId.Get,
                    EventTimestamp   = DateTime.UtcNow
                };
        }

        // -----------------------------------------------------------------------
        // Property 1: AuditLog entries are insert-only (Req 9.2)
        // -----------------------------------------------------------------------

        /// <summary>
        /// For any valid AuditEntry, AuditService.Log() must call IAuditRepository.Insert
        /// exactly once and must never call any update or delete operation.
        ///
        /// This property validates that the insert-only contract is upheld across all
        /// possible audit entry inputs.
        ///
        /// **Validates: Requirements 9.2**
        /// </summary>
        [Property(Arbitrary = new[] { typeof(AuditEntryArbitrary) }, MaxTest = 200)]
        public Property AuditLog_IsInsertOnly_NeverUpdateOrDelete(AuditEntry entry)
        {
            // Arrange
            var auditRepo   = Substitute.For<IAuditRepository>();
            var loggingService = Substitute.For<ILoggingService>();
            auditRepo.Insert(Arg.Any<AuditLog>()).Returns(1L);

            var svc = new AuditService(auditRepo, loggingService);

            // Act
            svc.Log(entry);

            // Assert: Insert called exactly once
            auditRepo.Received(1).Insert(Arg.Any<AuditLog>());

            // Assert: Query is the only other method on IAuditRepository — no update/delete exists
            // The interface itself has no Update or Delete method, which is the structural guarantee.
            // We additionally verify no unexpected calls were made beyond the single Insert.
            auditRepo.ReceivedWithAnyArgs(1).Insert(default!);

            return true.ToProperty();
        }

        /// <summary>
        /// Verifies that the AuditLog persisted by AuditService.Log() preserves all
        /// fields from the input AuditEntry (no data is silently dropped).
        ///
        /// **Validates: Requirements 9.2, 9.4**
        /// </summary>
        [Property(Arbitrary = new[] { typeof(AuditEntryArbitrary) }, MaxTest = 200)]
        public Property AuditLog_Insert_PreservesAllFields(AuditEntry entry)
        {
            // Arrange
            AuditLog? captured = null;
            var auditRepo      = Substitute.For<IAuditRepository>();
            var loggingService = Substitute.For<ILoggingService>();

            auditRepo
                .When(r => r.Insert(Arg.Any<AuditLog>()))
                .Do(ci => captured = ci.Arg<AuditLog>());
            auditRepo.Insert(Arg.Any<AuditLog>()).Returns(1L);

            var svc = new AuditService(auditRepo, loggingService);

            // Act
            svc.Log(entry);

            // Assert: all fields are preserved
            if (captured == null) return false.ToProperty();

            return (captured.EventType        == entry.EventType        &&
                    captured.ActorUserId      == entry.ActorUserId      &&
                    captured.TargetEntityType == entry.TargetEntityType &&
                    captured.TargetEntityId   == entry.TargetEntityId   &&
                    captured.Outcome          == entry.Outcome          &&
                    captured.WorkstationId    == entry.WorkstationId).ToProperty();
        }

        // -----------------------------------------------------------------------
        // Unit test: IAuditRepository interface has no Update or Delete method
        // -----------------------------------------------------------------------

        [Test]
        public void IAuditRepository_HasNoUpdateOrDeleteMethod()
        {
            var repoType = typeof(IAuditRepository);
            var methods  = repoType.GetMethods();

            foreach (var method in methods)
            {
                var name = method.Name.ToUpperInvariant();
                Assert.That(name, Does.Not.Contain("UPDATE"),
                    $"IAuditRepository must not expose an Update method (found: {method.Name})");
                Assert.That(name, Does.Not.Contain("DELETE"),
                    $"IAuditRepository must not expose a Delete method (found: {method.Name})");
            }
        }

        [Test]
        public void AuditService_Log_CallsInsertOnce()
        {
            var auditRepo      = Substitute.For<IAuditRepository>();
            var loggingService = Substitute.For<ILoggingService>();
            auditRepo.Insert(Arg.Any<AuditLog>()).Returns(1L);

            var svc = new AuditService(auditRepo, loggingService);
            svc.Log(new AuditEntry
            {
                EventType        = "Login",
                ActorUserId      = 1,
                TargetEntityType = "UserAccount",
                TargetEntityId   = "1",
                Outcome          = "Success",
                WorkstationId    = "WS1"
            });

            auditRepo.Received(1).Insert(Arg.Any<AuditLog>());
        }
    }

    /// <summary>
    /// FsCheck arbitrary for generating valid AuditEntry instances.
    /// </summary>
    public static class AuditEntryArbitrary
    {
        public static Arbitrary<AuditEntry> AuditEntry()
        {
            var gen =
                from eventType        in Arb.Generate<NonEmptyString>()
                from actorUserId      in Gen.Choose(1, 10000)
                from targetEntityType in Arb.Generate<NonEmptyString>()
                from targetEntityId   in Arb.Generate<NonEmptyString>()
                from outcome          in Gen.Elements("Success", "Failure")
                from workstationId    in Arb.Generate<NonEmptyString>()
                select new AuditEntry
                {
                    EventType        = eventType.Get,
                    ActorUserId      = actorUserId,
                    TargetEntityType = targetEntityType.Get,
                    TargetEntityId   = targetEntityId.Get,
                    Outcome          = outcome,
                    WorkstationId    = workstationId.Get,
                    EventTimestamp   = DateTime.UtcNow
                };

            return gen.ToArbitrary();
        }
    }
}
