# Design Document — Org Unit Visibility Permission

## Overview

Feature นี้เพิ่มกลไก **explicit per-user OrgUnit visibility** ให้กับ CMS โดยแทนที่ inline role-based filtering ที่กระจายอยู่ใน `UserAccessForm` ด้วย service layer ที่มี single source of truth คือ table `security.UserOrgVisibility`

แนวทางที่เลือก (Approach B) ทำงานดังนี้:
- ถ้า user ยังไม่มี row ใน `UserOrgVisibility` → ใช้ role-based fallback เหมือนเดิม (backward compatible)
- ถ้า Admin grant อย่างน้อย 1 row → user นั้นเปลี่ยนเป็น explicit-grant mode ทันที

การเปลี่ยนแปลงครอบคลุม 5 layers ตามลำดับ dependency: Database → Domain → Data → Services → Presentation

---

## Architecture

```
┌─────────────────────────────────────────────────────────────┐
│  CMS.Presentation                                           │
│  UserAccessForm  ──────────────────────────────────────┐   │
│  FolderForm  ──────────────────────────────────────┐   │   │
└────────────────────────────────────────────────────┼───┼───┘
                                                     │   │
┌────────────────────────────────────────────────────▼───▼───┐
│  CMS.Services                                               │
│  IUserOrgVisibilityService  (new)                          │
│  ├── GetVisibleOrgUnits(userId, caller)                    │
│  ├── GrantVisibility(userId, orgUnitId, caller)            │
│  ├── RevokeVisibility(userId, orgUnitId, caller)           │
│  └── GetGrantsForOrgUnit(orgUnitId, caller)                │
│                                                             │
│  FolderService.GetRootFolders  ──► IUserOrgVisibilityService│
│  OrgStructureService.GetVisibleHierarchy  ──► same         │
└──────────────────────────────────────┬──────────────────────┘
                                       │
┌──────────────────────────────────────▼──────────────────────┐
│  CMS.Data                                                    │
│  IUserOrgVisibilityRepository  (new)                        │
│  ├── GetByUser(userId)                                      │
│  ├── GetByOrgUnit(orgUnitId)                                │
│  ├── Grant(record)                                          │
│  └── Revoke(userId, orgUnitId)                              │
└──────────────────────────────────────┬──────────────────────┘
                                       │
┌──────────────────────────────────────▼──────────────────────┐
│  SQL Server                                                  │
│  security.UserOrgVisibility  (new table)                    │
│  usp_UserOrgVisibility_*  (new stored procedures)           │
└─────────────────────────────────────────────────────────────┘
```

### Dependency Wiring (Program.cs)

```
userOrgVisibilityRepo  = new UserOrgVisibilityRepository(configManager)
userOrgVisibilityService = new UserOrgVisibilityService(
    userOrgVisibilityRepo, orgUnitRepository, auditService)

folderService = new FolderService(..., userOrgVisibilityService)   // เพิ่ม optional param
orgStructureService = new OrgStructureService(
    orgUnitRepository, auditService, userOrgVisibilityService)     // เพิ่ม optional param

UserAccessForm(..., userOrgVisibilityService)
```

---

## Components and Interfaces

### 1. IUserOrgVisibilityRepository (CMS.Data/Interfaces/)

```csharp
public interface IUserOrgVisibilityRepository
{
    IEnumerable<UserOrgVisibility> GetByUser(int userId);
    IEnumerable<UserOrgVisibility> GetByOrgUnit(int orgUnitId);
    void Grant(UserOrgVisibility record);   // upsert
    void Revoke(int userId, int orgUnitId); // no-op if not found
}
```

### 2. UserOrgVisibilityRepository (CMS.Data/Repositories/)

- ใช้ ADO.NET + SqlCommand เรียก stored procedures เท่านั้น
- `Grant` → `usp_UserOrgVisibility_Grant` (MERGE / upsert)
- `Revoke` → `usp_UserOrgVisibility_Revoke` (DELETE WHERE; ไม่ throw ถ้าไม่มี row)
- `GetByUser` → `usp_UserOrgVisibility_GetByUser`
- `GetByOrgUnit` → `usp_UserOrgVisibility_GetByOrgUnit`
- Map result set → `UserOrgVisibility` domain model

### 3. IUserOrgVisibilityService (CMS.Services/Interfaces/)

```csharp
public interface IUserOrgVisibilityService
{
    IEnumerable<OrgUnit> GetVisibleOrgUnits(int userId, UserContext caller);
    void GrantVisibility(int userId, int orgUnitId, UserContext caller);
    void RevokeVisibility(int userId, int orgUnitId, UserContext caller);
    IEnumerable<UserOrgVisibility> GetGrantsForOrgUnit(int orgUnitId, UserContext caller);
}
```

### 4. UserOrgVisibilityService (CMS.Services/)

Constructor:
```csharp
public UserOrgVisibilityService(
    IUserOrgVisibilityRepository visibilityRepo,
    IOrgUnitRepository           orgUnitRepo,
    IAuditService                auditService = null)
```

Logic ใน `GetVisibleOrgUnits`:
1. ดึง grants จาก `visibilityRepo.GetByUser(userId)`
2. ถ้า grants ว่าง → ใช้ role-based fallback (ดู Data Models)
3. ถ้า grants มีข้อมูล → return เฉพาะ OrgUnit ที่ตรงกับ granted IDs

### 5. FolderService — การแก้ไข

เพิ่ม optional dependency:
```csharp
public FolderService(
    ...,
    IUserOrgVisibilityService visibilityService = null)
```

แก้ `GetRootFolders`:
```csharp
public IEnumerable<Folder> GetRootFolders(int? orgUnitId, UserContext user)
{
    var visibleIds = _visibilityService != null
        ? _visibilityService.GetVisibleOrgUnits(user.UserID, user)
                            .Select(o => o.OrgUnitID)
                            .ToHashSet()
        : null;

    IEnumerable<Folder> roots = orgUnitId.HasValue
        ? _folderRepository.GetRootFoldersByOrgUnit(orgUnitId.Value)
        : _folderRepository.GetAllRootFolders();

    roots = roots.Where(f => !f.IsDeleted);

    if (visibleIds != null)
        roots = roots.Where(f => f.OrgUnitID.HasValue && visibleIds.Contains(f.OrgUnitID.Value));

    return roots.Where(f => _permissionService.CheckPermission(user.UserID, f.FolderID, "Read"));
}
```

### 6. OrgStructureService — การแก้ไข

เพิ่ม optional dependency และ method ใหม่:
```csharp
public OrgStructureService(
    IOrgUnitRepository        orgUnitRepository,
    IAuditService             auditService             = null,
    IUserOrgVisibilityService visibilityService        = null)

public IEnumerable<OrgUnit> GetVisibleHierarchy(UserContext user)
{
    var visible = _visibilityService != null
        ? _visibilityService.GetVisibleOrgUnits(user.UserID, user)
        : _orgUnitRepository.GetHierarchy(); // fallback: Admin sees all
    return visible;
}
```

`IOrgStructureService` เพิ่ม method:
```csharp
IEnumerable<OrgUnit> GetVisibleHierarchy(UserContext user);
```

### 7. UserAccessForm — การแก้ไข

เพิ่ม dependency:
```csharp
public UserAccessForm(
    IUserAccountRepository    userRepo,
    IOrgStructureService      orgService,
    UserContext               currentUser,
    IEmployeeApiService       employeeApi          = null,
    IUserOrgVisibilityService visibilityService    = null)
```

เพิ่ม Tab/Panel ใหม่ "สิทธิ์มองเห็นหน่วยงาน":
- แสดงเฉพาะเมื่อ `_currentUser.AppRole == 4` (Admin)
- `ListView` แสดง OrgUnits ที่ granted ให้ selected user
- ปุ่ม "เพิ่มสิทธิ์" → เปิด OrgUnit picker → เรียก `GrantVisibility`
- ปุ่ม "ลบสิทธิ์" → เรียก `RevokeVisibility`
- แทนที่ `GetVisibleOrgUnitIds` inline logic ด้วย `visibilityService.GetVisibleOrgUnits`

---

## Data Models

### UserOrgVisibility (CMS.Domain/Models/)

```csharp
public class UserOrgVisibility
{
    public int      UserID      { get; set; }
    public int      OrgUnitID   { get; set; }
    public int      GrantedBy   { get; set; }
    public DateTime GrantedDate { get; set; }

    // Transient — populated by service layer, not DB
    public string OrgUnitName   { get; set; }
    public string GrantedByName { get; set; }
}
```

### Database Schema — 047_UserOrgVisibility.sql

```sql
-- Table
CREATE TABLE security.UserOrgVisibility (
    UserID      INT      NOT NULL,
    OrgUnitID   INT      NOT NULL,
    GrantedBy   INT      NOT NULL,
    GrantedDate DATETIME NOT NULL DEFAULT GETDATE(),
    CONSTRAINT PK_UserOrgVisibility PRIMARY KEY (UserID, OrgUnitID),
    CONSTRAINT FK_UOV_User      FOREIGN KEY (UserID)    REFERENCES core.UserAccount(UserID),
    CONSTRAINT FK_UOV_OrgUnit   FOREIGN KEY (OrgUnitID) REFERENCES core.OrgUnit(OrgUnitID),
    CONSTRAINT FK_UOV_GrantedBy FOREIGN KEY (GrantedBy) REFERENCES core.UserAccount(UserID)
);
```

Stored Procedures:
- `usp_UserOrgVisibility_GetByUser @UserID INT`
- `usp_UserOrgVisibility_GetByOrgUnit @OrgUnitID INT`
- `usp_UserOrgVisibility_Grant @UserID INT, @OrgUnitID INT, @GrantedBy INT` — MERGE (upsert)
- `usp_UserOrgVisibility_Revoke @UserID INT, @OrgUnitID INT` — DELETE; no error if missing

### Role-Based Fallback Logic

ใช้เมื่อ user ไม่มี row ใน `UserOrgVisibility`:

| AppRole | Visibility |
|---------|-----------|
| 4 (Admin) | OrgUnit ทั้งหมดที่ IsActive = 1 |
| 3 (Manager) | OrgUnit ของตัวเอง + descendants ทั้งหมด (BFS) |
| 1, 2 (Officer) | เฉพาะ OrgUnit ของตัวเอง (UserAccount.OrgUnitID) |

Logic นี้ย้ายจาก `UserAccessForm.GetVisibleOrgUnitIds` มาอยู่ใน `UserOrgVisibilityService` เพื่อให้เป็น single source of truth

---

## Correctness Properties

*A property is a characteristic or behavior that should hold true across all valid executions of a system — essentially, a formal statement about what the system should do. Properties serve as the bridge between human-readable specifications and machine-verifiable correctness guarantees.*

### Property 1: Grant Idempotence

*For any* valid (UserID, OrgUnitID) pair, calling `Grant` twice with the same pair should result in exactly one row in `UserOrgVisibility` — no duplicate key exception should be thrown.

**Validates: Requirements 3.3**

---

### Property 2: Revoke Idempotence

*For any* (UserID, OrgUnitID) pair regardless of whether a grant exists, calling `Revoke` should complete without throwing an exception.

**Validates: Requirements 3.4**

---

### Property 3: Fallback Matches Legacy Role-Based Logic

*For any* user with zero rows in `UserOrgVisibility` and any OrgUnit hierarchy, `GetVisibleOrgUnits` should return a set of OrgUnits identical to what the legacy `GetVisibleOrgUnitIds` inline logic in `UserAccessForm` would have returned for the same user and role.

**Validates: Requirements 4.2, 10.1, 10.2**

---

### Property 4: Explicit Grants Override Role-Based Fallback

*For any* user who has at least one row in `UserOrgVisibility`, `GetVisibleOrgUnits` should return exactly the set of OrgUnits corresponding to those granted rows — no more, no less — regardless of the user's AppRole.

**Validates: Requirements 4.3, 10.3**

---

### Property 5: Non-Admin Callers Are Rejected for Mutating Operations

*For any* caller with `AppRole < 4`, calling `GrantVisibility` or `RevokeVisibility` should throw `UnauthorizedAccessException`.

**Validates: Requirements 4.4, 4.5**

---

### Property 6: Mutating Operations Produce Audit Entries

*For any* Admin caller and any valid (userId, orgUnitId) pair, calling `GrantVisibility` or `RevokeVisibility` should result in at least one audit entry being recorded via `IAuditService`.

**Validates: Requirements 4.6**

---

### Property 7: GetRootFolders Filters by Visible Set

*For any* user and any set of root folders, `GetRootFolders` should return only folders whose `OrgUnitID` is contained in the set returned by `IUserOrgVisibilityService.GetVisibleOrgUnits` for that user. When the visible set is empty, the result must be an empty collection.

**Validates: Requirements 5.1, 5.2, 5.3**

---

### Property 8: GetVisibleHierarchy Returns Only Visible OrgUnits

*For any* user, every OrgUnit returned by `GetVisibleHierarchy` should have its ID present in the set returned by `IUserOrgVisibilityService.GetVisibleOrgUnits` for that same user.

**Validates: Requirements 6.1, 6.2**

---

## Error Handling

| Scenario | Behavior |
|----------|----------|
| `GrantVisibility` / `RevokeVisibility` called by non-Admin | throw `UnauthorizedAccessException` |
| `Grant` called with non-existent UserID or OrgUnitID | SQL FK violation propagates as `InvalidOperationException` |
| `Revoke` called for non-existent pair | silent no-op (idempotent) |
| `Grant` called with duplicate (UserID, OrgUnitID) | upsert — no exception |
| `GetVisibleOrgUnits` called for user with no OrgUnitID set | role-based fallback returns empty set for Officer |
| `visibilityService` is null (not wired) | FolderService / OrgStructureService fall back to unfiltered behavior (backward compat) |
| DB connection failure | SqlException propagates up; UI catches and displays error message |

---

## Testing Strategy

### Unit Tests

เน้น specific examples และ edge cases:

- `UserOrgVisibilityService` — verify fallback logic for each AppRole (1, 2, 3, 4) with empty grants
- `UserOrgVisibilityService` — verify explicit-grant mode overrides role when grants exist
- `UserOrgVisibilityService` — verify `UnauthorizedAccessException` for non-Admin callers
- `FolderService.GetRootFolders` — verify filtering with mock visibility service returning specific IDs
- `FolderService.GetRootFolders` — verify empty result when visibility service returns empty set
- `OrgStructureService.GetVisibleHierarchy` — verify delegation to visibility service

### Property-Based Tests

ใช้ **FsCheck** (NuGet: `FsCheck` สำหรับ .NET Framework 4.5) ซึ่งเป็น property-based testing library สำหรับ .NET

Configuration: ทุก property test ต้องรัน **อย่างน้อย 100 iterations**

Tag format: `// Feature: org-unit-visibility-permission, Property {N}: {property_text}`

**Property 1 — Grant Idempotence**
```csharp
// Feature: org-unit-visibility-permission, Property 1: Grant idempotence
// For any valid (UserID, OrgUnitID), calling Grant twice must not throw
// and must result in exactly one row
[Property(MaxTest = 100)]
Property GrantTwiceDoesNotThrow(int userId, int orgUnitId, int grantedBy) { ... }
```

**Property 2 — Revoke Idempotence**
```csharp
// Feature: org-unit-visibility-permission, Property 2: Revoke idempotence
// For any (UserID, OrgUnitID), Revoke on non-existent pair must not throw
[Property(MaxTest = 100)]
Property RevokeNonExistentDoesNotThrow(int userId, int orgUnitId) { ... }
```

**Property 3 — Fallback Matches Legacy Logic**
```csharp
// Feature: org-unit-visibility-permission, Property 3: Fallback matches legacy role-based logic
// For any user with no grants, GetVisibleOrgUnits == legacy GetVisibleOrgUnitIds
[Property(MaxTest = 100)]
Property FallbackMatchesLegacy(UserAccount user, List<OrgUnit> hierarchy) { ... }
```

**Property 4 — Explicit Grants Override Fallback**
```csharp
// Feature: org-unit-visibility-permission, Property 4: Explicit grants override fallback
// For any user with >= 1 grant, result == granted set regardless of role
[Property(MaxTest = 100)]
Property ExplicitGrantsOverrideRole(UserAccount user, List<UserOrgVisibility> grants) { ... }
```

**Property 5 — Non-Admin Rejected**
```csharp
// Feature: org-unit-visibility-permission, Property 5: Non-Admin callers rejected
// For any caller with AppRole < 4, Grant/Revoke throws UnauthorizedAccessException
[Property(MaxTest = 100)]
Property NonAdminCallerThrows(int appRole, int userId, int orgUnitId) { ... }
```

**Property 6 — Audit Entry on Mutation**
```csharp
// Feature: org-unit-visibility-permission, Property 6: Mutating operations produce audit entries
// For any Admin caller, Grant/Revoke records at least one audit entry
[Property(MaxTest = 100)]
Property MutationProducesAuditEntry(int userId, int orgUnitId) { ... }
```

**Property 7 — GetRootFolders Filters by Visible Set**
```csharp
// Feature: org-unit-visibility-permission, Property 7: GetRootFolders filters by visible set
// For any user and folder list, result contains only folders with OrgUnitID in visible set
[Property(MaxTest = 100)]
Property RootFoldersFilteredByVisibleSet(UserContext user, List<Folder> allRoots) { ... }
```

**Property 8 — GetVisibleHierarchy Returns Only Visible OrgUnits**
```csharp
// Feature: org-unit-visibility-permission, Property 8: GetVisibleHierarchy returns only visible OrgUnits
// For any user, every OrgUnit in GetVisibleHierarchy is in the visible set
[Property(MaxTest = 100)]
Property VisibleHierarchySubsetOfVisibleSet(UserContext user, List<OrgUnit> hierarchy) { ... }
```

### Integration Tests

- ทดสอบ `UserOrgVisibilityRepository` กับ SQL Server จริง (test database)
- ยืนยัน upsert behavior ของ `Grant`
- ยืนยัน no-op behavior ของ `Revoke` เมื่อไม่มี row
- ยืนยัน FK constraint violations ถูก propagate ถูกต้อง
