# HC Folder Creation Approval Workflow

## Overview

HC (Highly Confidential) folders now require admin approval before creation. This ensures that sensitive folder creation is properly controlled and audited.

## Workflow Process

### 1. User Requests HC Folder Creation
When a user attempts to create a folder with `ConfidentialityLevel = 1 (HC)`:

```csharp
var request = new CreateFolderRequest
{
    Name = "Sensitive Project",
    ParentFolderId = 5,
    OrgUnitId = 2,
    ConfidentialityLevel = 1  // HC level
};

var folder = folderService.CreateFolder(request, userContext);
```

**Result**: Instead of creating the folder immediately, the system:
- Creates an `ApprovalRequest` with `RequestType = "CREATE_HC"`
- Stores folder metadata (name, parent, depth, etc.) in the `Remark` field as JSON
- Creates approval steps for all active Admin users (UserRole = 4)
- Returns a placeholder folder with negative FolderID (RequestID * -1) to indicate pending approval
- Notifies all admins of the pending approval

### 2. Admin Reviews and Approves

Admins see the pending request in the Approval Inbox:
- Request Type: "CREATE_HC"
- Subject: "สร้าง HC Folder: [FolderName]"
- Remark: Contains folder creation details in JSON format

Admin can:
- **Approve**: Folder is created with the specified properties
- **Reject**: Folder creation is cancelled

### 3. Folder Creation After Approval

When the final admin approves:
- `ApprovalService.ApproveStep()` detects `RequestType = "CREATE_HC"`
- Calls `FolderService.CreateHCFolderAfterApproval(approvalRequest, user)`
- Folder is created with:
  - Original creator as `CreatedBy` (from approval request)
  - Current timestamp as `CreatedDate`
  - All specified properties (name, parent, confidentiality level, etc.)
- Audit log records the creation with action "CREATE_HC_APPROVED"
- Requester is notified of approval

## Non-HC Folders

Folders with `ConfidentialityLevel > 1` (SC, NC, PC) are created immediately without approval.

## Database Changes

### New SQL Script: `030_HC_Folder_Approval_Workflow.sql`

- Updates `workflow.ApprovalRequest.RequestType` CHECK constraint to include `'CREATE_HC'`
- Adds index on `RequestType` and `RequestStatus` for performance
- Adds stored procedure `workflow.sp_GetPendingCreateHCRequests` to retrieve pending HC folder requests

## Code Changes

### FolderService
- **Modified**: `CreateFolder()` - Now checks if folder is HC and creates approval request instead
- **New**: `CreateHCFolderAfterApproval()` - Creates folder after approval is granted

### ApprovalService
- **Modified**: `ApproveStep()` - Added handling for `CREATE_HC` request type
- **New Dependency**: `IFolderService` - Injected to call `CreateHCFolderAfterApproval()`

### IFolderService Interface
- **New Method**: `CreateHCFolderAfterApproval(ApprovalRequest, UserContext)`

### Program.cs (DI Composition)
- Updated `FolderService` instantiation to include `IUserAccountRepository`
- Updated `ApprovalService` instantiation to include `IFolderService`

## Approval Request Data Format

The `Remark` field stores folder creation data as JSON:

```json
{
  "ParentFolderId": 5,
  "OrgUnitId": 2,
  "FolderName": "Sensitive Project",
  "FolderDepth": 2,
  "ConfidentialityLevel": 1,
  "InheritPermission": false
}
```

## Error Handling

### During Request Creation
- **No Admins Found**: Throws `InvalidOperationException` - "ไม่พบ Admin ในระบบ ไม่สามารถสร้าง HC Folder ได้"
- **Invalid Folder Name**: Same validation as non-HC folders
- **Duplicate Name**: Checked before creating approval request

### During Approval
- **Parent Folder Deleted**: Throws error - "Cannot create folder under a deleted parent"
- **Parent Folder Archived**: Throws error - "Cannot create folder under an archived parent"
- **Duplicate Name Created**: Throws error - "A folder named '[name]' already exists in the same parent"
- **Invalid JSON in Remark**: Throws error with details

## Audit Trail

All HC folder creation activities are logged:

| Action | Module | Details |
|--------|--------|---------|
| REQUEST_CREATE_HC | Folder | Approval request submitted |
| APPROVE | Workflow | Approval step approved |
| CREATE_HC_APPROVED | Folder | Folder created after approval |

## Testing

### Test Case 1: Create HC Folder
```csharp
var request = new CreateFolderRequest
{
    Name = "Test HC Folder",
    ParentFolderId = 1,
    OrgUnitId = 1,
    ConfidentialityLevel = 1
};

var result = folderService.CreateFolder(request, userContext);
// Result: FolderID < 0 (pending approval)
// Approval request created in database
```

### Test Case 2: Approve HC Folder Creation
```csharp
// Admin approves the request
approvalService.ApproveStep(stepId, "Approved", adminContext);
// Result: Folder is created with positive FolderID
```

### Test Case 3: Create Non-HC Folder
```csharp
var request = new CreateFolderRequest
{
    Name = "Public Folder",
    ParentFolderId = 1,
    OrgUnitId = 1,
    ConfidentialityLevel = 4  // PC (Public)
};

var result = folderService.CreateFolder(request, userContext);
// Result: Folder created immediately with positive FolderID
```

## Migration Steps

1. Apply SQL script: `030_HC_Folder_Approval_Workflow.sql`
2. Rebuild solution
3. Restart application
4. Test HC folder creation workflow

## Future Enhancements

- Add UI to display pending HC folder creation requests
- Add ability to modify folder properties before approval
- Add escalation for expired requests
- Add bulk approval for multiple requests
