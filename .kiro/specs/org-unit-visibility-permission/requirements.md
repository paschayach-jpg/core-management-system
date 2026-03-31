# Requirements Document

## Introduction

Feature นี้เพิ่มกลไก **Org Unit Visibility Permission** (แนวทาง B) ให้กับระบบ CMS โดยแทนที่การกำหนด visibility ของ OrgUnit แบบ role-based อย่างเดียว ด้วยการเก็บ explicit permission ต่อ user ใน table `security.UserOrgVisibility` ทำให้ Admin สามารถกำหนดได้อย่างละเอียดว่า user แต่ละคนมองเห็น OrgUnit ใดได้บ้าง โดยไม่ขึ้นกับ role เพียงอย่างเดียว

การเปลี่ยนแปลงครอบคลุม 5 layers: Database, CMS.Domain, CMS.Data, CMS.Services, และ CMS.Presentation

---

## Glossary

- **UserOrgVisibility**: Permission record ที่ระบุว่า UserAccount หนึ่งๆ มีสิทธิ์มองเห็น OrgUnit หนึ่งๆ
- **Visibility_Service**: `IUserOrgVisibilityService` — service ที่จัดการ UserOrgVisibility
- **Visibility_Repository**: `IUserOrgVisibilityRepository` — repository ที่เข้าถึง table `security.UserOrgVisibility`
- **Folder_Service**: `IFolderService` — service ที่จัดการ folder และ root folder listing
- **OrgStructure_Service**: `IOrgStructureService` — service ที่จัดการ OrgUnit hierarchy
- **FolderForm**: WinForms form ที่แสดง folder tree และกรอง OrgUnit ที่ user มองเห็น
- **UserAccessForm**: WinForms form ที่ Admin ใช้จัดการสิทธิ์ผู้ใช้ รวมถึง assign OrgUnit visibility
- **Admin**: UserAccount ที่มี AppRole = 4
- **Manager**: UserAccount ที่มี AppRole = 3
- **Officer**: UserAccount ที่มี AppRole = 1 หรือ 2
- **UserContext**: Object ที่ส่งผ่านทุก service method เพื่อระบุ current user และ role
- **GrantedBy**: UserID ของ Admin ที่เป็นผู้ assign visibility
- **OrgUnit**: หน่วยงานใน hierarchy 3 ระดับ: SubCommittee(1) → Division(2) → Department(3)

---

## Requirements

### Requirement 1: Database Schema — UserOrgVisibility Table

**User Story:** As an Admin, I want a dedicated permission table to store per-user OrgUnit visibility grants, so that visibility can be managed independently from role assignments.

#### Acceptance Criteria

1. THE Database SHALL contain a table `security.UserOrgVisibility` with columns: `UserID` (int, FK → `core.UserAccount`), `OrgUnitID` (int, FK → `core.OrgUnit`), `GrantedBy` (int, FK → `core.UserAccount`), `GrantedDate` (datetime)
2. THE Database SHALL enforce a composite primary key on (`UserID`, `OrgUnitID`) in `security.UserOrgVisibility`
3. THE Database SHALL enforce referential integrity via foreign keys on `UserID`, `OrgUnitID`, and `GrantedBy`
4. WHEN a `core.UserAccount` record is deleted, THE Database SHALL cascade-delete or restrict deletion of related `security.UserOrgVisibility` rows per FK constraint
5. THE Database SHALL provide stored procedures: `usp_UserOrgVisibility_GetByUser`, `usp_UserOrgVisibility_Grant`, `usp_UserOrgVisibility_Revoke`, `usp_UserOrgVisibility_GetByOrgUnit`
6. THE Database SHALL be updated via a numbered migration script following the `NNN_Description.sql` convention in `CMS.Data/Scripts/`

---

### Requirement 2: Domain Model — UserOrgVisibility

**User Story:** As a developer, I want a domain model representing a visibility grant, so that the data can be passed between layers in a type-safe way.

#### Acceptance Criteria

1. THE System SHALL provide a class `UserOrgVisibility` in `CMS.Domain/Models/` with properties: `UserID` (int), `OrgUnitID` (int), `GrantedBy` (int), `GrantedDate` (DateTime)
2. THE `UserOrgVisibility` model SHALL have no dependencies on CMS.Data, CMS.Services, or CMS.Presentation
3. WHERE transient display data is needed (e.g. `OrgUnitName`, `GrantedByName`), THE `UserOrgVisibility` model SHALL expose nullable string properties populated by the service layer, not the database

---

### Requirement 3: Data Layer — IUserOrgVisibilityRepository

**User Story:** As a developer, I want a repository interface and ADO.NET implementation for UserOrgVisibility, so that all database access follows the established repository pattern.

#### Acceptance Criteria

1. THE System SHALL provide an interface `IUserOrgVisibilityRepository` in `CMS.Data/Interfaces/` with methods: `GetByUser(int userId)`, `GetByOrgUnit(int orgUnitId)`, `Grant(UserOrgVisibility record)`, `Revoke(int userId, int orgUnitId)`
2. THE System SHALL provide a class `UserOrgVisibilityRepository` in `CMS.Data/Repositories/` implementing `IUserOrgVisibilityRepository` using ADO.NET and calling stored procedures
3. WHEN `Grant` is called with a (`UserID`, `OrgUnitID`) pair that already exists, THE `UserOrgVisibilityRepository` SHALL perform an upsert without throwing a duplicate key exception
4. WHEN `Revoke` is called for a (`UserID`, `OrgUnitID`) pair that does not exist, THE `UserOrgVisibilityRepository` SHALL complete without error
5. THE `UserOrgVisibilityRepository` SHALL NOT use any ORM; all queries SHALL call stored procedures via `SqlCommand`

---

### Requirement 4: Service Layer — IUserOrgVisibilityService

**User Story:** As a developer, I want a service that encapsulates visibility business logic, so that callers do not need to know about role fallback rules or audit requirements.

#### Acceptance Criteria

1. THE System SHALL provide an interface `IUserOrgVisibilityService` in `CMS.Services/Interfaces/` with methods: `GetVisibleOrgUnits(int userId, UserContext caller)`, `GrantVisibility(int userId, int orgUnitId, UserContext caller)`, `RevokeVisibility(int userId, int orgUnitId, UserContext caller)`, `GetGrantsForOrgUnit(int orgUnitId, UserContext caller)`
2. WHEN `GetVisibleOrgUnits` is called for a user who has no rows in `UserOrgVisibility`, THE `Visibility_Service` SHALL fall back to role-based visibility rules (Admin → all, Manager → own subtree, Officer → own OrgUnit only)
3. WHEN `GetVisibleOrgUnits` is called for a user who has at least one row in `UserOrgVisibility`, THE `Visibility_Service` SHALL return only the OrgUnits explicitly granted, ignoring role-based rules
4. WHEN `GrantVisibility` is called by a non-Admin caller, THE `Visibility_Service` SHALL throw an `UnauthorizedAccessException`
5. WHEN `RevokeVisibility` is called by a non-Admin caller, THE `Visibility_Service` SHALL throw an `UnauthorizedAccessException`
6. WHEN `GrantVisibility` or `RevokeVisibility` is called by an Admin, THE `Visibility_Service` SHALL record an audit entry via `IAuditService`
7. THE `Visibility_Service` implementation SHALL depend on `IUserOrgVisibilityRepository`, `IOrgUnitRepository`, and `IAuditService` via constructor injection

---

### Requirement 5: FolderService — GetRootFolders Uses Visibility

**User Story:** As a user, I want the folder tree to show only OrgUnits I have visibility permission for, so that I cannot see folders belonging to OrgUnits outside my access.

#### Acceptance Criteria

1. WHEN `Folder_Service.GetRootFolders` is called, THE `Folder_Service` SHALL resolve the set of visible OrgUnit IDs for the calling user via `IUserOrgVisibilityService.GetVisibleOrgUnits`
2. THE `Folder_Service` SHALL return only root folders whose `OrgUnitID` is in the resolved visible set
3. WHEN the resolved visible set is empty, THE `Folder_Service` SHALL return an empty collection
4. THE `Folder_Service` SHALL NOT apply role-based OrgUnit filtering directly; all visibility resolution SHALL be delegated to `IUserOrgVisibilityService`

---

### Requirement 6: OrgStructureService — GetHierarchy Respects Visibility

**User Story:** As a user, I want the OrgUnit hierarchy to be filtered to only units I can see, so that org pickers and trees do not expose unauthorized units.

#### Acceptance Criteria

1. THE `OrgStructure_Service` SHALL expose a method `GetVisibleHierarchy(UserContext user)` that returns only OrgUnits visible to the calling user
2. WHEN `GetVisibleHierarchy` is called, THE `OrgStructure_Service` SHALL delegate visibility resolution to `IUserOrgVisibilityService.GetVisibleOrgUnits`
3. THE existing `GetHierarchy()` method (no filter) SHALL remain available for Admin-only internal use

---

### Requirement 7: FolderForm — FilterVisibleOrgUnits Uses Service

**User Story:** As a user, I want the folder form to filter the OrgUnit tree using the new permission service, so that the UI is consistent with the data layer.

#### Acceptance Criteria

1. WHEN `FolderForm` loads the OrgUnit tree, THE `FolderForm` SHALL call `IUserOrgVisibilityService.GetVisibleOrgUnits` instead of applying inline role-based logic
2. THE `FolderForm` SHALL display only OrgUnit tree nodes whose IDs are returned by `GetVisibleOrgUnits`
3. WHEN `GetVisibleOrgUnits` returns an empty set, THE `FolderForm` SHALL display an empty tree with a status message indicating no visible units

---

### Requirement 8: UserAccessForm — Assign OrgUnit Visibility UI

**User Story:** As an Admin, I want a UI panel in UserAccessForm to assign and revoke OrgUnit visibility for individual users, so that I can manage fine-grained access without changing roles.

#### Acceptance Criteria

1. THE `UserAccessForm` SHALL contain a tab or panel labelled "สิทธิ์มองเห็นหน่วยงาน" accessible only when the current user is Admin
2. WHEN an Admin selects a user in the user list, THE `UserAccessForm` SHALL display the list of OrgUnits currently granted to that user via `IUserOrgVisibilityService.GetVisibleOrgUnits`
3. THE `UserAccessForm` SHALL provide a control to add an OrgUnit grant by selecting from the full OrgUnit hierarchy and calling `IUserOrgVisibilityService.GrantVisibility`
4. THE `UserAccessForm` SHALL provide a control to remove an OrgUnit grant by selecting a granted entry and calling `IUserOrgVisibilityService.RevokeVisibility`
5. WHEN `GrantVisibility` or `RevokeVisibility` succeeds, THE `UserAccessForm` SHALL refresh the granted list and display a success status message
6. IF `GrantVisibility` or `RevokeVisibility` throws an exception, THE `UserAccessForm` SHALL display an error message without closing the form
7. WHEN the current user is not Admin, THE `UserAccessForm` SHALL hide or disable the "สิทธิ์มองเห็นหน่วยงาน" panel

---

### Requirement 9: Dependency Wiring — Program.cs

**User Story:** As a developer, I want the new service and repository to be wired into the manual DI composition root, so that all forms receive the correct dependencies at startup.

#### Acceptance Criteria

1. THE `Program.cs` composition root SHALL instantiate `UserOrgVisibilityRepository` and pass it to `UserOrgVisibilityService` via constructor injection
2. THE `Program.cs` composition root SHALL pass `IUserOrgVisibilityService` to `FolderService`, `OrgStructureService`, `FolderForm`, and `UserAccessForm`
3. THE System SHALL NOT use a DI container; all wiring SHALL be explicit constructor calls in `Program.cs`

---

### Requirement 10: Backward Compatibility — Role-Based Fallback

**User Story:** As an existing user with no explicit visibility grants, I want the system to behave exactly as before, so that the feature rollout does not break existing access.

#### Acceptance Criteria

1. WHEN a user has zero rows in `security.UserOrgVisibility`, THE `Visibility_Service` SHALL apply the legacy role-based rules: Admin(4) sees all OrgUnits, Manager(3) sees own OrgUnit and all descendants, Officer(1,2) sees own OrgUnit only
2. THE legacy role-based fallback SHALL produce results identical to the previous `GetVisibleOrgUnitIds` logic in `UserAccessForm`
3. WHEN an Admin grants at least one explicit visibility record for a user, THE `Visibility_Service` SHALL switch that user fully to explicit-grant mode, disabling the role-based fallback for that user
