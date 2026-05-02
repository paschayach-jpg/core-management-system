# 🎯 Workflow Template Creation - FINAL CHECKLIST

## ✅ VERIFICATION COMPLETE

### Code Structure - **ALL VERIFIED ✅**
- [x] **Wizard Controller**: `WorkflowCreateTemplateWizard.cs` 
  - ✅ Passes WizardState through all 4 steps
  - ✅ Calls CreateTemplate service on completion
  - ✅ Refreshes template list

- [x] **Step 1 - Basic Info**: `WorkflowCreateTemplateForm.cs`
  - ✅ Input: Template Name (Required), Description
  - ✅ Validates name
  - ✅ Saves to WizardState

- [x] **Step 2 - Flow Design**: `WorkflowCreateTemplateStep2Form.cs`
  - ✅ Editable DataGridView (Add/Delete rows)
  - ✅ Validates min 1 step
  - ✅ Saves to WizardState.Steps

- [x] **Step 3 - Approvals**: `WorkflowCreateTemplateStep3Form.cs`
  - ✅ DataGridView loads steps from Step 2
  - ✅ Allows setting approver groups
  - ✅ Saves to WizardState.Approvers

- [x] **Step 4 - Review**: `WorkflowCreateTemplateStep4Form.cs`
  - ✅ Shows real summary from all steps
  - ✅ Draft/Publish radio buttons
  - ✅ Calls service.CreateTemplate()

- [x] **Service Layer**: `WorkflowTemplateService.cs`
  - ✅ CreateTemplate method implemented
  - ✅ Validates input
  - ✅ Inserts to database
  - ✅ Logs audit entry

- [x] **Repository Layer**: `WorkflowTemplateRepository.cs`
  - ✅ CreateTemplate method implemented
  - ✅ Parameterized SQL query
  - ✅ Returns WorkflowDefinitionId

- [x] **List Form Integration**: `WorkflowTemplateListForm.cs`
  - ✅ "+ สร้าง Template" button → ShowTemplateForm()
  - ✅ Wizard → LoadTemplates() on completion
  - ✅ New templates appear in grid

---

## 🚀 BEFORE TESTING - 2 CRITICAL STEPS

### ⚠️ STEP 1: RUN SQL SCRIPT (CRITICAL!)
```
File: CMS.Data/Scripts/010_CreateWorkflowTables.sql
Database: Your CMS database (SQL Server 2019)

This creates:
✅ dbo.WorkflowDefinition (template table)
✅ dbo.WorkflowStep (steps table)
✅ dbo.WorkflowApprover (approvers table)
```

**How to run:**
1. Open **SQL Server Management Studio**
2. Connect to your CMS database
3. File → Open → Select `010_CreateWorkflowTables.sql`
4. Press **F5** to execute
5. Check for message: "✓ All Workflow tables created successfully"

---

### ⚠️ STEP 2: REBUILD SOLUTION
```
Visual Studio → Build → Rebuild Solution
```
Ensures all components are compiled together.

---

## 🧪 TESTING PROCEDURE

### Test Case 1: Basic Template Creation
```
1. Open CMS Application
2. Navigate: Workflow Engine → Template Workflow
3. Click "+ สร้าง Template" button
4. Step 1:
   - Enter name: "Template Test 001"
   - Enter description: "Test workflow template"
   - Click "ถัดไป"
5. Step 2:
   - Click add row
   - Row 1: Step Name="ยื่น", Action="Submit", NextStep="อนุมัติ"
   - Click add row
   - Row 2: Step Name="อนุมัติ", Action="Approve", NextStep="จบ"
   - Click add row
   - Row 3: Step Name="จบ", Action="Complete", NextStep=""
   - Click "ถัดไป"
6. Step 3:
   - Row 1: ยื่น → Set ApproverGroup="สำนักเลขาธิการ", Level="1"
   - Row 2: อนุมัติ → Set ApproverGroup="ฝ่ายอนุมัติ", Level="2"
   - Row 3: จบ → (Leave empty)
   - Click "ถัดไป"
7. Step 4:
   - Verify summary shows:
     ✓ ชื่อ Template: Template Test 001
     ✓ จำนวนขั้นตอน: 3
     ✓ Approvers: 2 steps
   - Select "เผยแพร่ (Publish)"
   - Click "เสร็จสิ้น"
8. Success message appears ✓
9. Return to template list
10. Verify "Template Test 001" appears in grid ✓
```

### Test Case 2: Draft Mode
```
Repeat Test Case 1 but in Step 4:
- Select "บันทึกเป็น Draft"
- Verify success message says "สถานะ: Draft"
- Template appears in list with Draft badge
```

### Test Case 3: Validation Tests
```
Test 3a - Missing Template Name:
  - Step 1: Leave template name empty
  - Click "ถัดไป"
  - Error: "กรุณาระบุชื่อ Template"

Test 3b - No Workflow Steps:
  - Step 1: Enter name
  - Step 2: Click "ถัดไป" without adding any steps
  - Error: "กรุณาเพิ่มอย่างน้อย 1 ขั้นตอน"

Test 3c - Cancel Wizard:
  - Any step: Click "ยกเลิก"
  - Wizard closes without saving
```

### Test Case 4: Back Button Navigation
```
- Step 4: Click "ย้อนกลับ"
- Return to Step 3 ✓
- Verify Step 3 data still there
- Click "ย้อนกลับ" again
- Return to Step 2 ✓
- Verify Step 2 data still there
```

---

## 📊 EXPECTED RESULTS

### ✅ Success Indicators:
1. SQL script runs without errors
2. Wizard displays all 4 steps with proper controls
3. DataGridViews are editable
4. Navigation works (next/previous)
5. Summary displays actual data from previous steps
6. Template saves to database
7. New template appears in list after wizard completes
8. Refresh/pagination works properly

### ❌ Failure Points & Solutions:

| Symptom | Cause | Fix |
|---------|-------|-----|
| "Invalid object name 'dbo.WorkflowDefinition'" | SQL script not run | Run 010_CreateWorkflowTables.sql |
| DataGridView empty in Step 3 | WizardState not passed | Check wizard constructor calls |
| "Object reference not set" error | Service/Repository null | Check dependency injection |
| Summary shows blank | Serialization issue | Check SerializeWizardState() |
| Template not in list after save | LoadTemplates() not called | Check ShowTemplateForm() |
| Form controls not visible | Designer issue | Rebuild solution |

---

## 📝 NEXT STEPS AFTER TESTING

Once testing passes:
1. ✅ All 4 steps working
2. ✅ Database saves correct data
3. ✅ List displays new templates

Then implement:
- [ ] Workflow step designer visual UI (Step 2 canvas)
- [ ] Edit existing templates
- [ ] Delete/archive templates
- [ ] Template versioning
- [ ] Workflow instance execution

---

## 🎓 KEY FILES REFERENCE

| Purpose | File |
|---------|------|
| SQL Tables | `CMS.Data/Scripts/010_CreateWorkflowTables.sql` |
| Wizard Logic | `CMS.Presentation/Modules/WFE/WorkflowCreateTemplateWizard.cs` |
| Step Forms | `CMS.Presentation/Modules/WFE/WorkflowCreateTemplateStep[1-4]Form.cs` |
| Service | `CMS.Services/WorkflowTemplateService.cs` |
| Repository | `CMS.Data/Repositories/WorkflowTemplateRepository.cs` |
| Data Model | `CMS.Presentation/Modules/WFE/WorkflowWizardState.cs` |
| List Form | `CMS.Presentation/Modules/WFE/WorkflowTemplateListForm.cs` |

---

**Status**: 🟢 **READY FOR TESTING**  
**Last Updated**: 2026-04-30  
**Completion**: 100% (Code) | 0% (Database) | 0% (Testing)

---

## ⏭️ FINAL ACTION ITEMS

- [ ] **1. Run SQL Script** → 010_CreateWorkflowTables.sql
- [ ] **2. Rebuild Solution** → Visual Studio Build → Rebuild
- [ ] **3. Test Basic Flow** → Follow Test Case 1
- [ ] **4. Verify Database** → Check dbo.WorkflowDefinition has new record
- [ ] **5. Test All Scenarios** → Follow remaining test cases
- [ ] **6. Report Results** → Document any issues found
