# Implementation Plan: Org Unit Visibility Permission

## Overview

Implement explicit per-user OrgUnit visibility permission across 5 layers in dependency order:
Database → Domain → Data → Services → Presentation.
Uses .NET Framework 4.5, WinForms, ADO.NET, and FsCheck for property-based tests.

## Tasks

- [x] 1. Database layer — create migration script 047_UserOrgVisibility.sql
  - Create `CMS.Data/Scripts/047_UserOrgVisibility.sql`
  - Define `security.UserOrgVisibility` table with composite PK (UserID, OrgUnitID) and FK constraints to `core.UserAccount` and `core.OrgUnit`
  - Write `usp_UserOrgVisibility_GetByUser @UserID INT`
  - Write `usp_UserOrgVisibility_GetByOrgUnit @OrgUnitID INT`
  - Write `usp_UserOrgVisibility_Grant @UserID INT, @OrgUnitID INT, @GrantedBy INT` using MERGE (upsert)
  - Write `usp_UserOrgVisibility_Revoke @UserID INT, @OrgUnitID INT` using DELETE WHERE (no error if missing)
  - _Requirements: 1.1, 1.2, 1.3, 1.4, 1.5, 1.6_

- [x] 2. Domain layer — create UserOrgVisibility model
  - Create `CMS.Domain/Models/UserOrgVisibility.cs`
  - Define properties: `UserID` (int), `OrgUnitID` (int), `GrantedBy` (int), `GrantedDate` (DateTime)
  - Add transient nullable string properties `OrgUnitName` and `GrantedByName` (populated by service, not DB)
  - No dependencies on CMS.Data, CMS.Services, or CMS.Presentation
  - _Requirements: 2.1, 2.2, 2.3_

- [x] 3. Data layer — IUserOrgVisibilityRepository interface and implementation
  - [x] 3.1 Create `CMS.Data/Interfaces/IUserOrgVisibilityRepository.cs`
    - Define methods: `GetByUser(int userId)`, `GetByOrgUnit(int orgUnitId)`, `Grant(UserOrgVisibility record)`, `Revoke(int userId, int orgUnitId)`
    - _Requirements: 3.1_

  - [x] 3.2 Create `CMS.Data/Repositories/UserOrgVisibilityRepository.cs`
    - Implement `IUserOrgVisibilityRepository` using ADO.NET `SqlCommand` calling stored procedures only
    - `GetByUser` → calls `usp_UserOrgVisibility_GetByUser`; maps result set to `UserOrgVisibility`
    - `GetByOrgUnit` → calls `usp_UserOrgVisibility_GetByOrgUnit`; maps result set to `UserOrgVisibility`
    - `Grant` → calls `usp_UserOrgVisibility_Grant`; upsert — no duplicate key exception
    - `Revoke` → calls `usp_UserOrgVisibility_Revoke`; silent no-op if row not found
    - Accept `IConfigService` (or connection string) via constructor, matching existing repository pattern
    - _Requirements: 3.2, 3.3, 3.4, 3.5_

  - [ ]* 3.3 Write property test for Grant idempotence (Property 1)
    - **Property 1: Grant Idempotence**
    - Calling `Grant` twice with the same (UserID, OrgUnitID) must not throw and must result in exactly one row
    - Use FsCheck `[Property(MaxTest = 100)]` with in-memory stub or test DB
    - Tag: `// Feature: org-unit-visibility-permission, Property 1: Grant idempotence`
    - **Validates: Requirements 3.3**

  - [ ]* 3.4 Write property test for Revoke idempotence (Property 2)
    - **Property 2: Revoke Idempotence**
    - Calling `Revoke` on any (UserID, OrgUnitID) pair regardless of existence must not throw
    - Use FsCheck `[Property(MaxTest = 100)]`
    - Tag: `// Feature: org-unit-visibility-permission, Property 2: Revoke idempotence`
    - **Validates: Requirements 3.4**

- [x] 4. Service layer — IUserOrgVisibilityService interface and implementation
  - [x] 4.1 Create `CMS.Services/Interfaces/IUserOrgVisibilityService.cs`
    - Define methods: `GetVisibleOrgUnits(int userId, UserContext caller)`, `GrantVisibility(int userId, int orgUnitId, UserContext caller)`, `RevokeVisibility(int userId, int orgUnitId, UserContext caller)`, `GetGrantsForOrgUnit(int orgUnitId, UserContext caller)`
    - _Requirements: 4.1_

  - [x] 4.2 Create `CMS.Services/UserOrgVisibilityService.cs`
    - Constructor: `(IUserOrgVisibilityRepository visibilityRepo, IOrgUnitRepository orgUnitRepo, IAuditService auditService = null)`
    - `GetVisibleOrgUnits`: if no grants exist for user → apply role-based fallback (Admin=all active, Manager=own subtree BFS, Officer=own OrgUnit only); if grants exist → return only granted OrgUnits
    - `GrantVisibility`: throw `UnauthorizedAccessException` if `caller.AppRole < 4`; call `visibilityRepo.Grant`; call `auditService` to record entry
    - `RevokeVisibility`: throw `UnauthorizedAccessException` if `caller.AppRole < 4`; call `visibilityRepo.Revoke`; call `auditService` to record entry
    - `GetGrantsForOrgUnit`: delegate to `visibilityRepo.GetByOrgUnit`; populate transient `OrgUnitName` and `GrantedByName`
    - _Requirements: 4.2, 4.3, 4.4, 4.5, 4.6, 4.7, 10.1, 10.2, 10.3_

  - [ ]* 4.3 Write property test for fallback matches legacy logic (Property 3)
    - **Property 3: Fallback Matches Legacy Role-Based Logic**
    - For any user with zero grants, `GetVisibleOrgUnits` result must equal legacy `GetVisibleOrgUnitIds` output for same user and role
    - Use FsCheck `[Property(MaxTest = 100)]`
    - Tag: `// Feature: org-unit-visibility-permission, Property 3: Fallback matches legacy role-based logic`
    - **Validates: Requirements 4.2, 10.1, 10.2**

  - [ ]* 4.4 Write property test for explicit grants override fallback (Property 4)
    - **Property 4: Explicit Grants Override Fallback**
    - For any user with >= 1 grant, `GetVisibleOrgUnits` returns exactly the granted set regardless of AppRole
    - Use FsCheck `[Property(MaxTest = 100)]`
    - Tag: `// Feature: org-unit-visibility-permission, Property 4: Explicit grants override fallback`
    - **Validates: Requirements 4.3, 10.3**

  - [ ]* 4.5 Write property test for non-Admin callers rejected (Property 5)
    - **Property 5: Non-Admin Callers Are Rejected for Mutating Operations**
    - For any caller with `AppRole < 4`, `GrantVisibility` and `RevokeVisibility` must throw `UnauthorizedAccessException`
    - Use FsCheck `[Property(MaxTest = 100)]` generating AppRole values in range [1, 3]
    - Tag: `// Feature: org-unit-visibility-permission, Property 5: Non-Admin callers rejected`
    - **Validates: Requirements 4.4, 4.5**

  - [ ]* 4.6 Write property test for audit entry on mutation (Property 6)
    - **Property 6: Mutating Operations Produce Audit Entries**
    - For any Admin caller and valid (userId, orgUnitId), `GrantVisibility` and `RevokeVisibility` must record at least one audit entry via `IAuditService`
    - Use FsCheck `[Property(MaxTest = 100)]` with mock `IAuditService`
    - Tag: `// Feature: org-unit-visibility-permission, Property 6: Mutating operations produce audit entries`
    - **Validates: Requirements 4.6**

- [x] 5. Checkpoint — ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

- [x] 6. Service layer — modify FolderService to use visibility
  - Modify `CMS.Services/FolderService.cs`
  - Add optional constructor parameter `IUserOrgVisibilityService visibilityService = null`
  - In `GetRootFolders(int? orgUnitId, UserContext user)`: resolve visible OrgUnit IDs via `_visibilityService.GetVisibleOrgUnits`; filter root folders to only those whose `OrgUnitID` is in the visible set; return empty collection when visible set is empty
  - When `_visibilityService` is null, fall back to existing unfiltered behavior (backward compat)
  - Do NOT apply role-based OrgUnit filtering directly in FolderService
  - _Requirements: 5.1, 5.2, 5.3, 5.4_

  - [ ]* 6.1 Write property test for GetRootFolders filters by visible set (Property 7)
    - **Property 7: GetRootFolders Filters by Visible Set**
    - For any user and any list of root folders, result contains only folders whose `OrgUnitID` is in the visible set; empty visible set → empty result
    - Use FsCheck `[Property(MaxTest = 100)]` with mock `IUserOrgVisibilityService`
    - Tag: `// Feature: org-unit-visibility-permission, Property 7: GetRootFolders filters by visible set`
    - **Validates: Requirements 5.1, 5.2, 5.3**

- [x] 7. Service layer — modify OrgStructureService and IOrgStructureService
  - Modify `CMS.Services/Interfaces/IOrgStructureService.cs`
  - Add method signature `IEnumerable<OrgUnit> GetVisibleHierarchy(UserContext user)`
  - _Requirements: 6.1_

  - Modify `CMS.Services/OrgStructureService.cs`
  - Add optional constructor parameter `IUserOrgVisibilityService visibilityService = null`
  - Implement `GetVisibleHierarchy(UserContext user)`: delegate to `_visibilityService.GetVisibleOrgUnits` when wired; fall back to `_orgUnitRepository.GetHierarchy()` when null (Admin-only internal use)
  - Keep existing `GetHierarchy()` method unchanged
  - _Requirements: 6.1, 6.2, 6.3_

  - [ ]* 7.1 Write property test for GetVisibleHierarchy returns only visible OrgUnits (Property 8)
    - **Property 8: GetVisibleHierarchy Returns Only Visible OrgUnits**
    - For any user, every OrgUnit returned by `GetVisibleHierarchy` must have its ID present in the set returned by `IUserOrgVisibilityService.GetVisibleOrgUnits` for that same user
    - Use FsCheck `[Property(MaxTest = 100)]` with mock `IUserOrgVisibilityService`
    - Tag: `// Feature: org-unit-visibility-permission, Property 8: GetVisibleHierarchy returns only visible OrgUnits`
    - **Validates: Requirements 6.1, 6.2**

- [x] 8. Presentation layer — modify UserAccessForm
  - Modify `CMS.Presentation/UserAccessForm.cs`
  - Add optional constructor parameter `IUserOrgVisibilityService visibilityService = null`
  - Add Tab/Panel "สิทธิ์มองเห็นหน่วยงาน" — visible only when `_currentUser.AppRole == 4`
  - Add `ListView` showing OrgUnits currently granted to the selected user (loaded via `GetVisibleOrgUnits`)
  - Add "เพิ่มสิทธิ์" button: open OrgUnit picker (full hierarchy from `GetVisibleHierarchy`), call `GrantVisibility`, refresh list, show success message
  - Add "ลบสิทธิ์" button: call `RevokeVisibility` on selected item, refresh list, show success message
  - On exception from Grant/Revoke: display error message without closing form
  - When current user is not Admin: hide/disable the panel
  - Replace inline `GetVisibleOrgUnitIds` logic with calls to `visibilityService.GetVisibleOrgUnits`
  - _Requirements: 8.1, 8.2, 8.3, 8.4, 8.5, 8.6, 8.7_

- [x] 9. Presentation layer — wire dependencies in Program.cs
  - Modify `CMS.Presentation/Program.cs`
  - Instantiate `UserOrgVisibilityRepository` with `IConfigService` (matching existing pattern)
  - Instantiate `UserOrgVisibilityService(userOrgVisibilityRepo, orgUnitRepository, auditService)`
  - Pass `userOrgVisibilityService` to `FolderService` constructor
  - Pass `userOrgVisibilityService` to `OrgStructureService` constructor
  - Pass `userOrgVisibilityService` to `UserAccessForm` constructor
  - All wiring via explicit constructor calls — no DI container
  - _Requirements: 9.1, 9.2, 9.3_

- [x] 10. Final checkpoint — ensure all tests pass
  - Ensure all tests pass, ask the user if questions arise.

## Notes

- Tasks marked with `*` are optional and can be skipped for faster MVP
- Property tests use FsCheck (NuGet: `FsCheck` for .NET Framework 4.5) with `[Property(MaxTest = 100)]`
- Each property test must include the tag comment: `// Feature: org-unit-visibility-permission, Property N: ...`
- Dependency order must be respected: Database → Domain → Data → Services → Presentation
- All DB access via stored procedures only — no inline SQL
- `visibilityService` optional parameters ensure backward compatibility when not wired
