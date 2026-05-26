# Workflow Monitor Dashboard — SQL Schema Problem Fix Guide

## 📋 Problem Summary
UI Cannot Load: **Invalid column name 'AssignedDate', 'CompletedBy', 'OutputData', 'SLADeadline', 'SLAStatus'**

The application code expects these column names but the database may use different naming conventions (e.g., `AssignedOn`, `DueDate`, `SLAState`, etc.).

---

## 🔍 Step 1: Diagnose the Schema (Required First)

### Run this diagnostic script first (in SSMS):
```sql
:r C:\path\to\workspace\CMS.Data\Scripts\000_Diagnose_Schema.sql
```

**This will show:**
- ✓ All actual column names in `WFE.t_WFStepInstance`
- ✓ Which expected columns are missing
- ✓ Stored procedures that reference the table
- ✓ Related table schema

**Save the output** — you'll need this to choose your fix strategy.

---

## 🛠️ Step 2: Choose Your Fix Strategy

Based on the diagnostic output, choose ONE of the following:

### Option A: Columns Use Different Names (MOST LIKELY)
**Example:** DB has `WFStepInstanceId` but code expects `WorkflowStepInstanceId`

**Solution:** Run the migration script to create aliases/physical columns

```bash
# In PowerShell terminal, navigate to project root
cd c:\Users\pitakpong\Documents\cms_import

# Then in SSMS, run:
# :r CMS.Data\Scripts\090_WFE_Add_Column_Aliases.sql
```

**Before Running Aliases Script:**
- Make sure you've run the diagnostic first
- Backup your database
- Verify table exists: `SELECT TOP 0 * FROM WFE.t_WFStepInstance;`

---

### Option B: Columns Are Completely Missing
**Symptom:** Diagnostic shows "MISSING" for multiple columns

**Solution:** 
1. Check if data exists in candidate columns (e.g., `AssignedOn`, `DueDate`)
2. If yes → run aliases script (Option A)
3. If no → you may need a full schema restoration or custom migration

---

### Option C: Columns Are Computed (Read-Only)
**Symptom:** Error says "computed column" cannot be modified

**Solution:** 
1. Drop the computed columns first
2. Create physical columns
3. Copy data if needed

**SQL to drop computed columns (if needed):**
```sql
ALTER TABLE WFE.t_WFStepInstance
DROP COLUMN WorkflowStepInstanceId,
            WorkflowInstanceId,
            WorkflowStepId,
            AssignedDate,
            CompletedBy,
            OutputData,
            SLADeadline,
            SLAStatus;
```

Then run the aliases script again.

---

## 📝 Step 3: Implementation Workflow

### Phase 1: Discovery (5 minutes)
1. Run diagnostic script: `000_Diagnose_Schema.sql`
2. **Record** the output — copy/paste into a text file
3. Identify which columns actually exist in your DB

### Phase 2: Backup (2 minutes)
```sql
-- In SSMS, backup before migration
BACKUP DATABASE [YourDatabase] 
TO DISK = 'C:\Backups\pre_alias_migration.bak';
```

### Phase 3: Execute Fix (5-10 minutes)
Based on diagnostic findings, run the appropriate script:
- If columns have different names → run `090_WFE_Add_Column_Aliases.sql`
- If columns are computed → drop them first, then run aliases script
- If columns are missing → consult your DBA for schema restoration

### Phase 4: Verify (5 minutes)
After running migration, verify columns exist:
```sql
SELECT TOP 10 
    WorkflowStepInstanceId, 
    AssignedDate, 
    CompletedBy, 
    OutputData, 
    SLADeadline, 
    SLAStatus
FROM WFE.t_WFStepInstance;
```

### Phase 5: Rebuild & Test (10 minutes)
```powershell
# In PowerShell, rebuild application
dotnet build CMS.sln -c Debug

# Run the app and test Workflow Monitor UI
# Navigate to: Workflow Engine > Monitor
```

---

## 🎯 Expected Result After Fix

- ✅ UI loads without "Invalid column name" errors
- ✅ Workflow Monitor shows dashboard with real data
- ✅ Counts display: In Progress, SLA Exceeded, Completed
- ✅ Filters work: Status, Template, User, Date Range
- ✅ Workflow list populates with tasks

---

## ❌ If Problems Persist

1. **Still seeing "Invalid column name"?**
   - Run diagnostic again
   - Verify migration script ran successfully
   - Check if there are still computed columns (`is_computed = 1`)

2. **UI still blank?**
   - Check application logs: `bin/Debug/` folder
   - Run query: `SELECT COUNT(*) FROM WFE.t_WFStepInstance;`
   - If count = 0, you may have no test data yet

3. **Performance issues?**
   - Add indexes on `AssignedTo`, `AssignedDate`, `SLADeadline`
   - See: `CMS.Data/Scripts/091_Add_Indexes.sql` (will create if needed)

---

## 📌 Key Files
- **Diagnostic:** `CMS.Data/Scripts/000_Diagnose_Schema.sql`
- **Migration:** `CMS.Data/Scripts/090_WFE_Add_Column_Aliases.sql`
- **Rollback:** `CMS.Data/Scripts/099_Rollback_Aliases.sql` (to be created if needed)

---

## 🚀 Next Steps
1. **Now:** Run the diagnostic script and provide the output
2. **Then:** We'll choose the right fix and apply it
3. **Finally:** Rebuild and test the UI

**Ready?** Let's run the diagnostic! 👇
