# RG_Claim — Project Module Skill Reference

> **อัปเดตล่าสุด:** 2026-06-22
> **Branch:** `Project_edit_by_chanvit`

## ภาพรวมระบบ

โมดูล **Project** เป็นระบบบริหารโปรเจ็คแบบ Windows Forms ที่ใช้ Template-based workflow พร้อม Progress tracking, Task management, Document workflow และ E-Filing integration โครงสร้างเป็น **3-tier: Models → Services → Controllers → Forms**

---

## Architecture Summary

```
Forms (UI)
  └─ Services (Business Logic / Orchestration)
       └─ Controllers (DAL / SQL CRUD)
            └─ SQL Server (BIS_Claim DB + BUI_RG DB)
```

---

## Database Tables

### Master Tables
| Table | คำอธิบาย |
|-------|----------|
| `m_project_template` | เทมเพลตโปรเจ็ค (TMPL001, TMPL002 ...) |
| `m_template_state` | ขั้นตอน (State) ภายในแต่ละเทมเพลต |
| `m_template_doc` | เอกสารหลัก (master doc) ที่ map กับ template state |
| `m_project_member_role` | นิยาม Role ของสมาชิก |

### Transactional Tables
| Table | คำอธิบาย |
|-------|----------|
| `t_project` | ข้อมูลหัวโปรเจ็ค (project_no, name, template, status, dates) |
| `t_project_state` | State ของโปรเจ็ค (copy จาก template + estimated_days + i_ref_project_id) |
| `t_project_progress` | บันทึก progress แบบ append-only (ห้าม UPDATE/DELETE) |
| `t_project_doc_workflow` | เอกสาร workflow ต่อ state (required/optional, status) |
| `t_project_efiling_request` | คำขอ E-Filing (Requested/Received) |
| `t_project_task` | Task management — MOD-009 |
| `t_project_task_comment` | Comment threading บน task (depth ≤ 3) |
| `t_project_state_member` | สมาชิกที่ assign ต่อ state |
| `t_project_member` | สมาชิกระดับโปรเจ็ค |

---

## Models

| Model | Key Fields |
|-------|-----------|
| `ProjectModel` | i_project_id, c_project_no, n_project_name, i_template_id, c_status (Open/InProgress/Completed), i_pct_overall |
| `ProjectStateModel` | i_proj_state_id, n_state_name, i_pct_complete, i_estimated_days, i_ref_project_id, n_ref_project_name |
| `ProjectProgressModel` | i_progress_id, i_pct_complete, c_status (Pending/InProgress/Completed), n_remark, i_action_user, d_action_date |
| `ProjectTaskModel` | i_task_id, c_task_code, n_title, i_priority (1=Low/2=Medium/3=High/4=Critical), c_status (Todo/InProgress/InReview/Done/Cancelled), n_assignee, d_due_date, i_pct_complete, b_sla_warning, b_overdue, i_subtask_count |
| `ProjectTaskCommentModel` | i_comment_id, n_content, i_thread_depth (≤3), f_is_edited, f_is_deleted |
| `ProjectDocumentWorkflowModel` | i_doc_wf_id, n_doc_name, c_doc_status (Pending/Complete), i_doc_id |
| `EFilingRequestModel` | i_efiling_req_id, c_efiling_ref_no, c_request_status, d_receive_date |
| `ProjectStateMemberModel` | i_state_member_id, n_member_name, c_role |

---

## Services

### `ProjectService`
| Method | คำอธิบาย |
|--------|----------|
| `GenerateProjectNo()` | สร้างเลขโปรเจ็คแบบ atomic — รูปแบบ `PRJ-YY-######` |
| `CreateProject(model)` | INSERT โปรเจ็ค + auto-copy states จาก template + auto-populate doc workflow |
| `UpdateProject(model)` | อัปเดต header |
| `DeleteProject()` | บล็อคถ้ามี progress อยู่ |
| `CloseProject()` | บล็อคถ้า required states ยังไม่ครบ |
| `SearchProjects()` | Filter: no/name/status/template/date range |
| `GetTemplates()` | ดึง template ที่ active สำหรับ ComboBox |

### `ProjectStateService`
| Method | คำอธิบาย |
|--------|----------|
| `GetStatesWithProgress()` | States + latest progress + ref project info |
| `AddState()` | เพิ่ม state + auto-increment seq_no |
| `DeleteState()` | บล็อคถ้ามี progress records |
| `GetTotalEstimatedDays()` | รวม estimated days ทุก state |
| `GetActiveProjectsForRef()` | โปรเจ็ค active สำหรับ ref project selector |

### `ProjectProgressService`
| Method | คำอธิบาย |
|--------|----------|
| `UpdateStateProgress()` | INSERT progress → update state status → recalculate project status |
| `GetProgressHistory()` | ดึง log append-only ของ 1 state |
| `GetLatestProgressByProject()` | Latest 1 record ต่อ state + ref project data |
| `GetOverallCompletionPct()` | AVG ของทุก state |

### `ProjectTaskService` (MOD-009)
| Method | คำอธิบาย |
|--------|----------|
| `CreateTask()` | auto-generate task code (`TK-YY-######`) + INSERT |
| `UpdateTask()` / `UpdateStatus()` | อัปเดต task + auto-sync state progress |
| `GetByState()` / `GetByProject()` | Query พร้อม SLA flags (root tasks) |
| `GetAllTasksForProject()` | **[ใหม่]** Recursive CTE — โหลด task ทุก depth พร้อม `tree_depth` |
| `GetSubtasksForDashboard()` | ดึง direct children ของ parent task (legacy) |
| `AddComment()` / `EditComment()` / `DeleteComment()` | Comment threading |
| `SoftDelete()` / `Restore()` | Recycle bin 30 วัน |
| `SyncStatePct()` | คำนวณ % state จาก avg task % |

### `EmployeeController` (ใหม่)
| Method | คำอธิบาย |
|--------|----------|
| `GetActiveEmployees()` | ดึงชื่อพนักงานจาก Common DB (`Connect('m')`) → `dbo.Employees` |

### `ProjectDocumentService`
| Method | คำอธิบาย |
|--------|----------|
| `GetDocWorkflow()` | ดึงเอกสารต่อ state/project |
| `UpdateDocStatus()` | Pending → Complete |
| `SearchEFiling()` | REST API call ไปยัง E-Filing system |
| `CreateEFilingRequest()` / `UpdateEFilingRequestStatus()` | ติดตาม request |

---

## Controllers

### `ProjectController`
- `SearchProjects()` — LEFT JOIN + คำนวณ i_total_states, i_completed_states, i_pct_overall, i_last_worker
- `InsertProject()` / `UpdateProject()` / `DeleteProject()` / `CloseProject()`
- `AllRequiredStatesCompleted()` — check business rule
- `HasActiveProgress()` — validation

### `ProjectStateController`
- `GetStatesByProject()` — JOIN latest progress + ref project overall %
- `InsertStatesFromTemplate()` — bulk INSERT copy states จาก template
- `GetActiveProjectsForRef()` — Combobox (status != 'Completed')

### `ProjectTaskController`
- `GenerateTaskCode()` — Thread-safe `WITH UPDLOCK, ROWLOCK`; auto-INSERT RunningNumber row ถ้าไม่มี; ตัด product ตาม `sys.columns.max_length`
- `GetByState()` / `GetByProject()` — root tasks + LEFT JOIN subtask count/done; SLA_warning (≥80% of due-create span), overdue flag
- `GetAllTasksForProject()` — **[ใหม่]** Recursive CTE: root → children ทุก depth, ผล `tree_depth` + `sort_path` + `i_subtask_count` + `i_subtask_done`; `OPTION (MAXRECURSION 50)`
- `GetSubtasksForDashboard()` — direct children สำหรับ legacy (schema เดียวกับ GetByProject)
- `CalcStatePctFromTasks()` — `AVG(task %)`
- `SoftDelete()` / `Restore()` — sets deleted_date + recycle_expiry (30 วัน)

### `ProjectProgressController`
- `InsertProgress()` — append-only INSERT
- `CalculateOverallProgress()` — AVG ทุก state
- `HasAnyProgress()` — validation

### `EFilingController`
- `SearchEFilingDoc()` — REST API POST ไปยัง `/search`
- `InsertEFilingRequest()` / `UpdateEFilingRequestStatus()`

---

## Forms

### `Frm_Project_List` — รายการโปรเจ็ค
- DataGridView แสดง project_no, name, template, status, % overall, dates
- Filter: status, template, project_no (DefaultView.RowFilter)
- Buttons: Search, Add, Edit, Delete, Excel export
- **[ใหม่]** ปุ่ม **"Tasks"** (สีม่วง) → เปิด `Frm_Task_Dashboard`

### `Frm_Project_Detail` — รายละเอียดโปรเจ็ค (5 แท็บ)
| แท็บ | เนื้อหา |
|-----|---------|
| Tab 1 - Info | หัว project (create/edit) |
| Tab 2 - States | CRUD states + est. days + ref project + members |
| Tab 3 - Progress | History log (append-only) ต่อ state |
| Tab 4 - Documents | Workflow docs + E-Filing requests |
| Tab 5 - Members | ทีม project level |

### `Frm_Project_State_Edit` — แก้ไข State
- State name, seq_no, description, required flag
- Estimated days
- Reference project selector (`Frm_Select_RefProject`)
- State members (`Frm_Select_StateMember`)

### `Frm_Project_Task_List` — MOD-009 Task Management (per-state)
- **List View:** DataGridView + filter assignee/status/priority
- **Kanban Board:** 5 columns (Todo / InProgress / InReview / Done / Cancelled)
- Cards: ระบายสีตาม priority, แสดง assignee, due date, SLA/overdue indicators
- Buttons: New, Edit, Delete, View Tasks

### `Frm_Task_Dashboard` — **[ใหม่]** Dashboard ภาพรวม Task ทุก project
- **สร้าง SplitContainer** แบ่งหน้าจอ: ซ้าย (List/Kanban), ขวา (Detail Panel)
- **List View (Tab 1):** DataGridView tree expand/collapse ไม่จำกัด depth
  - แต่ละ node แสดง expand icon `▸/▾` ถ้ามี sub task
  - Row indentation ตาม `tree_depth × 3 spaces + "└ "`
  - Filter: โครงการ, สถานะ, ผู้รับผิดชอบ, keyword
  - Stats bar 6 chips: ทั้งหมด / รอทำ / กำลังทำ / รอตรวจ / เสร็จ / เกินกำหนด
- **Kanban View (Tab 2):** 5 คอลัมน์, drag-and-drop เปลี่ยน status
  - แสดง root task cards + sub task cards (indented, เส้นเชื่อม)
  - Sub card: แสดง task code, title, assignee, status badge
- **Detail Panel (Email-style, ขวา):** เปิดเมื่อคลิก row
  - Header: task code + ปุ่ม ✕ ปิด
  - แสดงและแก้ไข: สถานะ, priority, % เสร็จ (slider), กำหนดเสร็จ, รายละเอียด
  - Sub task checklist (ติ๊ก = Done, ยกติ๊ก = InProgress)
  - ปุ่ม "+ เพิ่ม Sub Task", "บันทึก", "แก้ไขเต็ม"
- **Data Model:**
  - `_allRows: List<DataRow>` — flat list จาก Recursive CTE เรียงตาม `sort_path`
  - `_childrenMap: Dictionary<int,List<DataRow>>` — parentId → children
  - `_expandedIds: HashSet<int>` — ควบคุม expand state
- **Permission:**
  - `CanEditTask(row)` — root task assignee แก้ไขได้ทุกอย่าง; sub task assignee แก้ไขได้เฉพาะตัวเอง
  - `FindRootAssignee(taskId)` — เดินขึ้น parent chain หา root assignee

### `Frm_Project_Task_Edit` — สร้าง/แก้ไข Task
- Title, description, priority, status, % complete
- **[ใหม่]** Assignee ดึงจาก `EmployeeController.GetActiveEmployees()` (Common DB) แทน state members; แสดงชื่อเป็นตัวเล็กทั้งหมด (`LOWER(...)`)
- **[ใหม่]** Constructor รับ optional `parentTaskId` + `parentTaskCode` สำหรับสร้าง sub task
- **[ใหม่]** สร้าง sub task ของ sub task ได้ (ไม่จำกัด depth)
- Error handling: แสดง `ex.Message` จริงแทน generic "บันทึกไม่สำเร็จ"

### `Frm_Select_RefProject` — เลือก Reference Project
- ค้นหา + select โปรเจ็ค active (status != Completed)

### `Frm_Select_StateMember` / `Frm_Select_StateForTask`
- Add/remove สมาชิก + assign roles ต่อ state

### `Frm_Project_Task_Status` — อัปเดต Status + % ของ Task (popup)

---

## Business Flows

### 1. สร้างโปรเจ็ค
```
User กรอก project_no (หรือ Auto-Generate)
  → GenerateProjectNo() → atomic running number
  → CreateProject() → INSERT t_project (status='Open')
  → InsertStatesFromTemplate() → bulk INSERT t_project_state
  → InsertDocWorkflowFromTemplate() → bulk INSERT t_project_doc_workflow
```

### 2. อัปเดต Progress
```
User เปิด state → คลิก "Update Progress"
  → Frm_Project_Progress_Update
  → ตั้ง % (0=Pending, 1-99=InProgress, 100=Completed)
  → INSERT t_project_progress (append-only)
  → UPDATE t_project_state.c_status
  → Recalculate t_project.c_status
```

### 3. Task Management (MOD-009) — Per-State
```
User เปิด state → "View Tasks" → Frm_Project_Task_List
  → List View หรือ Kanban View (no reload)
  → New Task → Frm_Project_Task_Edit
  → auto-gen task code → INSERT t_project_task
  → อัปเดต status/% → SyncStatePct() → update state %
  → Comments (threading ≤ 3)
  → Delete → SoftDelete() (30 วัน recycle bin)
```

### 4. Task Dashboard (ภาพรวมทุก Project)
```
User คลิก "Tasks" ใน Frm_Project_List → Frm_Task_Dashboard
  → เลือกโครงการ → GetAllTasksForProject() (Recursive CTE)
  → _allRows (flat, tree-ordered) + _childrenMap (parent→children)
  → List View: expand/collapse tree ทุก depth
  → Kanban View: root cards + sub cards
  → คลิก row → Detail Panel (ขวา) แสดงรายละเอียด
  → แก้ไขจาก panel → บันทึก (UpdateStatus + UpdateTask)
  → สร้าง sub task จาก panel → Frm_Project_Task_Edit (parentTaskId)
  → Permission: CanEditTask() ตรวจ assignee chain ก่อน enable ปุ่ม
```

### 4. Reference Project
```
State สามารถ reference 1 โปรเจ็คอื่น (1:1)
→ SearchProjects() ดึง % จาก ref project แทน local state %
→ ใช้สำหรับ dependent projects (Phase 2 รอ Phase 1)
```

---

## Running Number Pattern
- รูปแบบ: `PPP-YY-######` (เช่น `PRJ-26-000001`, `TK-26-000001`)
- Thread-safe ด้วย `WITH UPDLOCK, ROWLOCK`
- Reset ทุกต้นปี (Year_UW column)

---

## Integration Points
| ระบบ | วิธีเชื่อม |
|------|-----------|
| **BUI_RG DB** | Master docs (m_document, m_document_type) |
| **E-Filing API** | REST POST ไปยัง `apiEFiling` (App.config) |
| **LoginManager** | Current user (ADUserName, employee name) |
| **ClaimServiceFactory** | Header mapping + Excel export |

---

## Business Rules
| Rule | เงื่อนไข |
|------|---------|
| ลบโปรเจ็ค | บล็อคถ้ามี progress records อยู่ |
| ปิดโปรเจ็ค | ต้อง complete ทุก required state |
| ลบ state | บล็อคถ้ามี progress |
| Subtask depth | **ไม่จำกัด** (Recursive CTE, MAXRECURSION 50) |
| Comment depth | ≤ 3 ระดับ |
| Soft delete | บันทึก d_deleted_date + d_recycle_expiry = +30 วัน |
| Progress | Append-only — ห้าม UPDATE หรือ DELETE |
| Task permission | Root assignee แก้ไขได้ทุกอย่าง; Sub task assignee แก้ไขได้เฉพาะ task ตัวเอง |
| RunningNumber | Auto-INSERT row ถ้าไม่มี; ตัด product column ตาม sys.columns.max_length |

---

## Key Files Reference

| ไฟล์ | หน้าที่ |
|------|---------|
| `Models/ProjectModel.cs` | Project header model |
| `Models/ProjectTaskModel.cs` | Task model พร้อม `PriorityLabel()` helper |
| `Services/ProjectService.cs` | Project CRUD + running number |
| `Services/ProjectTaskService.cs` | Task CRUD + sync + recycle bin + `GetAllTasksForProject()` |
| `Controllers/ProjectController.cs` | SQL project queries |
| `Controllers/ProjectTaskController.cs` | SQL task queries + SLA + Recursive CTE |
| `Controllers/EmployeeController.cs` | **[ใหม่]** ดึงพนักงานจาก Common DB (`Connect('m')`) |
| `Forms/Frm_Project_Detail.cs` | หน้าหลักรายละเอียดโปรเจ็ค (5 tabs) |
| `Forms/Frm_Project_List.cs` | รายการโปรเจ็ค + ปุ่ม Tasks → Dashboard |
| `Forms/Frm_Project_Task_List.cs` | List + Kanban view (per-state) |
| `Forms/Frm_Project_Task_Edit.cs` | สร้าง/แก้ไข task + sub task + assignee จาก Employee DB |
| `Forms/Frm_Task_Dashboard.cs` | **[ใหม่]** Dashboard ภาพรวม + Recursive tree + Detail panel + Permission |
| `Forms/Frm_Task_Dashboard.Designer.cs` | **[ใหม่]** Designer layout ของ Dashboard |
| `Forms/Frm_Select_StateForTask.cs` | **[ใหม่]** เลือก State เมื่อสร้าง Task จาก Dashboard |
| `SQL/create_t_project_task.sql` | Schema ตาราง task |
| `SQL/alter_t_project_state_add_ref_project.sql` | เพิ่มคอลัมน์ ref project |

---

## Changelog (Session 2026-06-22)

### ใหม่ทั้งหมด
| ไฟล์ | รายละเอียด |
|------|-----------|
| `Controllers/EmployeeController.cs` | ดึงพนักงานจาก Common DB ผ่าน `Connect('m')` → `dbo.Employees`; LOWER(first+last) |
| `Forms/Frm_Task_Dashboard.cs` | Dashboard ภาพรวม Task (List + Kanban + Detail Panel) |
| `Forms/Frm_Task_Dashboard.Designer.cs` | Layout ของ Dashboard (1200×720, Maximized) |
| `Forms/Frm_Select_StateForTask.cs` | Dialog เลือก State ก่อนสร้าง Task จาก Dashboard |

### แก้ไข
| ไฟล์ | สิ่งที่แก้ไข |
|------|-------------|
| `Controllers/ProjectTaskController.cs` | + `GetAllTasksForProject()` (Recursive CTE); แก้ `GenerateTaskCode()` ให้ auto-INSERT RunningNumber + ตัด product ตาม max_length; `GetByProject()` + LEFT JOIN subtask count/done |
| `Services/ProjectTaskService.cs` | + `GetAllTasksForProject()` wrapper; `CreateTask()` throw exception แทน return -1 |
| `Forms/Frm_Project_List.cs` | + ปุ่ม `btnTasks` (สีม่วง) → `Frm_Task_Dashboard` |
| `Forms/Frm_Project_List.Designer.cs` | เพิ่ม `btnTasks` ใน `pnlFilter` |
| `Forms/Frm_Project_Task_Edit.cs` | Constructor รับ `parentTaskId`/`parentTaskCode`; Assignee จาก `EmployeeController`; error แสดง ex.Message |
| `BIS_Claim.csproj` | เพิ่ม Compile entries สำหรับไฟล์ใหม่ทั้งหมด |

### Bug fixes
| ปัญหา | การแก้ไข |
|-------|---------|
| `Column 'i_subtask_count' does not belong to table` | เพิ่ม LEFT JOIN subtask count ใน `GetByProject()` |
| `GenerateTaskCode failed: Cannot insert NULL into column 'product'` | ตรวจสอบ row exists → INSERT พร้อม product value |
| `String or binary data truncated in column 'product'` | Query `sys.columns.max_length` แล้ว `LEFT(product, max_length)` |
| `Invalid object name 'Common.dbo.m_employees'` | เปลี่ยนเป็น `Connect('m')` + `FROM dbo.Employees` (ไม่ใช้ cross-DB prefix) |
| `error CS0246: Type 'Exception' not found` | เพิ่ม `using System;` ใน `ProjectTaskService.cs` |
