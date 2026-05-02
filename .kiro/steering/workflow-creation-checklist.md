# Workflow Template Creation - Complete Checklist ✅

## 1️⃣ DATABASE LAYER
- [x] SQL Script created: `010_CreateWorkflowTables.sql`
  - [x] `dbo.WorkflowDefinition` table
  - [x] `dbo.WorkflowStep` table  
  - [x] `dbo.WorkflowApprover` table
- [ ] SQL Script **MUST RUN** in SQL Server

## 2️⃣ DATA ACCESS LAYER (CMS.Data)
- [x] `WorkflowTemplateRepository.cs` - CreateTemplate method
  - Inserts into `dbo.WorkflowDefinition`
  - Parameters: Name, Description, Version, IsActive, CreatedBy
  - OUTPUT: WorkflowDefinitionId

## 3️⃣ BUSINESS LOGIC LAYER (CMS.Services)
- [x] `WorkflowTemplateService.cs` - CreateTemplate method
  - Validates template name
  - Creates template record
  - Logs audit entry
  - Returns WorkflowTemplateResult

## 4️⃣ UI LAYER - WIZARD FORMS (CMS.Presentation)

### Step 1: Basic Info ✅
- File: `WorkflowCreateTemplateForm.cs/Designer.cs`
- Controls:
  - TextBox: Template Name (Required)
  - TextBox: Description (Optional)
- Functions:
  - ✅ Validates template name
  - ✅ Saves to WizardState
  - ✅ Navigates to Step 2

### Step 2: Workflow Flow Design ✅
- File: `WorkflowCreateTemplateStep2Form.cs/Designer.cs`
- Controls:
  - **Editable DataGridView**: 
    - Column 1: Step Name (Required)
    - Column 2: Action
    - Column 3: Next Step
- Functions:
  - ✅ AllowUserToAddRows = true
  - ✅ AllowUserToDeleteRows = true
  - ✅ Validates at least 1 step exists
  - ✅ Saves to WizardState.Steps

### Step 3: Approval Configuration ✅
- File: `WorkflowCreateTemplateStep3Form.cs/Designer.cs`
- Controls:
  - **DataGridView** (populated from Step 2 steps):
    - Column 1: Step Name (auto-filled)
    - Column 2: Approver Group (editable)
    - Column 3: Approval Level (editable)
- Functions:
  - ✅ Loads steps from WizardState
  - ✅ Allows editing approver groups
  - ✅ Saves to WizardState.Approvers

### Step 4: Review & Publish ✅
- File: `WorkflowCreateTemplateStep4Form.cs/Designer.cs`
- Controls:
  - **Summary TextBox** (read-only):
    - Template Name
    - Description
    - Step count & list
    - Approvers count & list
  - **RadioButton Group**:
    - Draft (บันทึกเป็น Draft)
    - Publish (เผยแพร่) - Default selected
- Functions:
  - ✅ Shows actual summary from all steps
  - ✅ Allows Draft/Publish selection
  - ✅ Calls CreateTemplate service on completion
  - ✅ Saves to database

## 5️⃣ WIZARD CONTROLLER ✅
- File: `WorkflowCreateTemplateWizard.cs`
- Features:
  - ✅ Maintains `WorkflowWizardState` throughout wizard
  - ✅ Navigation: Step 1 → 2 → 3 → 4
  - ✅ Back button: Retry result navigates to previous step
  - ✅ OnWizardComplete: Calls CreateTemplate service
  - ✅ Serializes steps & approvers to TemplateDefinition field

## 6️⃣ DATA MODEL ✅
- File: `WorkflowWizardState.cs`
- Classes:
  - ✅ `WorkflowWizardState` - holds all wizard data
  - ✅ `WorkflowStepData` - step information
  - ✅ `WorkflowApproverData` - approver information

## 7️⃣ INTEGRATION WITH LIST VIEW ✅
- File: `WorkflowTemplateListForm.cs`
- Flow:
  - ✅ "Create New Template" button calls wizard
  - ✅ Wizard completes and saves to database
  - ✅ ListForm calls `_presenter.LoadTemplates()` to refresh
  - ✅ New template appears in grid

## 🔧 END-TO-END FLOW
```
1. User clicks "+ สร้าง Template"
2. Step 1: Enter Template Name → Next
3. Step 2: Add 2+ Workflow Steps → Next
4. Step 3: Set Approvers for each step → Next
5. Step 4: Review Summary → Select Publish/Draft → Complete
6. OnWizardComplete():
   - Calls CreateTemplate service with WizardState data
   - Service inserts into dbo.WorkflowDefinition
   - Returns to list with success message
   - New template visible in grid ✨
```

---

## ✅ REQUIRED BEFORE TESTING:
1. **MUST RUN SQL Script**: `CMS.Data/Scripts/010_CreateWorkflowTables.sql`
   - Creates dbo.WorkflowDefinition table
   - Creates dbo.WorkflowStep table
   - Creates dbo.WorkflowApprover table

2. **Rebuild Solution**: Build → Rebuild Solution (All components compiled)

3. **Launch App & Test**:
   - Open CMS App
   - Go to Workflow Engine → Template Workflow
   - Click "+ สร้าง Template"
   - Complete all 4 steps
   - Verify template appears in list

---

## 🐛 POTENTIAL ISSUES & FIXES:

| Issue | Cause | Solution |
|-------|-------|----------|
| "Invalid object name 'dbo.WorkflowDefinition'" | SQL Script not run | Run 010_CreateWorkflowTables.sql in SSMS |
| DataGridView not showing steps from Step 2 | WizardState not passed | Check WizardState initialization in wizard controller |
| "Template name is required" validation error | No name entered | Enter template name in Step 1 |
| "Please add at least 1 step" error | No steps in grid | Add rows to DataGridView in Step 2 |
| Steps list not visible in Step 3 | Form not loading data | Check LoadFormData() method |
| Summary shows empty/blank | Data not serialized | Check SerializeWizardState() method |
| Template not appearing in list after save | LoadTemplates() not called | Verify WorkflowTemplateListForm refresh logic |

---

**Last Updated**: 2026-04-30  
**Status**: ✅ Ready for Testing
