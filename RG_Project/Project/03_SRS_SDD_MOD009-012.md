# 03 — SRS + SDD: MOD-009 ถึง MOD-012
## กลุ่ม Task Management, Todo, Notes & Comments, Work Instructions
**CMS โครงการ 2511AL01 | Version 1.0 | เมษายน 2569**

---

> **หมายเหตุ:** MOD-008 (Workflow Engine) มีเอกสารละเอียดแยกต่างหากใน `/WFE/SRS+SDD/` ซึ่งสมบูรณ์แล้ว

---

## MOD-009: Task Management

**BR Reference:** BR-009 | **Priority:** High | **Form:** TaskBoardForm.cs, TaskDetailForm.cs

### วัตถุประสงค์

MOD-009 จัดการงาน (Task) ที่มาจากทั้ง Manual Create และจาก Workflow Engine (MOD-008) อัตโนมัติ รองรับ 2 View: Kanban Board (คล้าย Trello) และ Email Style Board (คล้าย Outlook) โดยสลับ View ได้โดยไม่ Reload

### Functional Requirements

| FR ID | ข้อกำหนด | Priority |
|-------|---------|---------|
| FR-009-01 | สร้าง/แก้ไข/ลบ Task พร้อม Title, Description, Priority, Due Date | Must |
| FR-009-02 | Kanban Board View: แสดง Task เป็น Card ใน Column ตาม Status | Must |
| FR-009-03 | Email Style Board View: แสดง Task เป็นรายการอ่านคล้าย Email | Must |
| FR-009-04 | สลับ View (Kanban ↔ Email) โดยไม่ Reload | Must |
| FR-009-05 | Drag-and-Drop Card ระหว่าง Column (Kanban View) | Must |
| FR-009-06 | Task Hierarchy: Task → Sub-Task (Depth ≤ 5) | Must |
| FR-009-07 | Assign Task ให้ User/Role/OrgUnit | Must |
| FR-009-08 | Progress Tracking: % Complete, Status, Due Date | Must |
| FR-009-09 | Filter/Search Task ตาม Status, Assignee, Priority, Due Date | Must |
| FR-009-10 | แสดงสัญลักษณ์ SLA Warning เมื่อใกล้ครบกำหนด | Must |
| FR-009-11 | Link Task กับ Folder/Document/Knowledge Base/Program | Must |
| FR-009-12 | Link Task กับ Workflow Instance (Auto-Created Tasks) | Must |
| FR-009-13 | บันทึก Comment + Mention User ใน Task | Must |
| FR-009-14 | แนบไฟล์กับ Task | Should |
| FR-009-15 | Recycle Bin สำหรับ Deleted Task (30 วัน) | Must |
| FR-009-16 | Export Task List เป็น Excel | Should |

### Task Status Flow

```
[Draft] → [Open] → [InProgress] → [Review/WaitApprove] → [Done]
                        ↓                                    ↑
                    [OnHold]  ────────────────────────────────┘
                        ↓
                  [Cancelled]
```

### Business Rules

```
BR-009-01: Task จาก Workflow Step สร้างอัตโนมัติ — Assignee = Step Assignee
BR-009-02: Completion % ของ Parent Task คำนวณจาก Sub-Tasks อัตโนมัติ
BR-009-03: ถ้า Task เกิน Due Date → แสดงสีแดง + สัญลักษณ์ Overdue
BR-009-04: Soft-Delete Task → เก็บใน Recycle Bin 30 วัน
BR-009-05: Task ที่ Link กับ Workflow Instance: Cancel ได้เฉพาะผ่าน Workflow เท่านั้น
BR-009-06: Task ที่มี Sub-Task: ปิดไม่ได้จนกว่า Sub-Tasks จะ Done ทั้งหมด
           (ยกเว้น Admin Override)
```

### Kanban Board Column Design

```
┌─────────────┬─────────────┬──────────────┬──────────────┬──────────┐
│   Draft     │    Open     │  In Progress │    Review    │   Done   │
│  (สีเทา)   │  (สีน้ำเงิน)│   (สีเหลือง) │   (สีม่วง)  │ (สีเขียว)|
├─────────────┼─────────────┼──────────────┼──────────────┼──────────┤
│ [Card]      │ [Card]      │ [Card]       │ [Card]       │ [Card]   │
│ Task Title  │ Task Title  │ Task Title   │ Task Title   │ Done!    │
│ @Assignee   │ @Assignee   │ @Assignee    │ @Assignee    │          │
│ Due: DD/MM  │ Due: DD/MM  │ ⚠️ Overdue  │ Due: DD/MM  │          │
│ Priority: H │ Priority: M │ Priority: H  │ Priority: L  │          │
└─────────────┴─────────────┴──────────────┴──────────────┴──────────┘
```

---

## MOD-010: Todo List Management

**BR Reference:** BR-010 | **Priority:** Medium | **Form:** TodoListForm.cs

### Functional Requirements

| FR ID | ข้อกำหนด | Priority |
|-------|---------|---------|
| FR-010-01 | สร้าง/แก้ไข/ลบ Todo Item | Must |
| FR-010-02 | แบ่งประเภท: Personal Todo (ส่วนตัว) หรือ Shared Todo (ทีม) | Must |
| FR-010-03 | ตั้ง Due Date, Priority, Category | Must |
| FR-010-04 | Mark as Complete / Incomplete | Must |
| FR-010-05 | แสดง % Progress ของ Todo List (Complete / Total) | Must |
| FR-010-06 | Recurring Todo: Daily/Weekly/Monthly | Should |
| FR-010-07 | Link Todo กับ Task หรือ Project | Should |
| FR-010-08 | Filter ตาม Status, Priority, Category, Due Date | Must |
| FR-010-09 | Sort ตาม Due Date, Priority, Status | Must |
| FR-010-10 | แสดง Today's Todo Dashboard | Should |

### Business Rules

```
BR-010-01: Personal Todo มองเห็นเฉพาะเจ้าของ
BR-010-02: Shared Todo มองเห็นได้ทุกคนใน OrgUnit ที่ระบุ
BR-010-03: Recurring Todo สร้าง Instance ถัดไปอัตโนมัติเมื่อ Complete
BR-010-04: Todo ที่ Overdue → แสดงสีแดง
BR-010-05: ลบ Todo → Soft-Delete ไม่ต้องผ่าน Recycle Bin (ลบได้เลย)
```

---

## MOD-011: Notes & Comments System

**BR Reference:** BR-011 | **Priority:** Medium | **Form:** FolderNotesForm.cs, TaskCommentsPanel.cs

### Functional Requirements

| FR ID | ข้อกำหนด | Priority |
|-------|---------|---------|
| FR-011-01 | เพิ่ม/แก้ไข/ลบ Notes ต่อ Folder, Task, Project | Must |
| FR-011-02 | Notes มี Priority Level: High/Medium/Low | Must |
| FR-011-03 | Thread Comments (ตอบกลับ Comment) | Must |
| FR-011-04 | @Mention User ใน Comment (แจ้ง Notification) | Must |
| FR-011-05 | แสดง Comment Timeline (เรียงตามเวลา) | Must |
| FR-011-06 | Edit Comment ของตัวเองเท่านั้น | Must |
| FR-011-07 | Admin ลบ Comment ของใครก็ได้ | Must |
| FR-011-08 | แสดง Edited Badge เมื่อ Comment ถูกแก้ไข | Should |
| FR-011-09 | Export Notes เป็น PDF หรือ Word | Should |
| FR-011-10 | ค้นหาใน Notes/Comments ของ Folder/Task | Should |

### Business Rules

```
BR-011-01: User แก้ไข Comment ของตัวเองได้ภายใน 24 ชั่วโมง
BR-011-02: หลัง 24 ชั่วโมง: Comment Lock — ต้องผ่าน Admin Unlock
BR-011-03: @Mention ส่ง In-App Notification + Email ทันที
BR-011-04: Deleted Comment → "[Comment ถูกลบ]" (ไม่ลบจาก DB)
BR-011-05: Thread Depth สูงสุด 3 ระดับ
```

---

## MOD-012: Work Instructions

**BR Reference:** BR-012 | **Priority:** Medium | **Form:** WorkInstructionForm.cs

### Functional Requirements

| FR ID | ข้อกำหนด | Priority |
|-------|---------|---------|
| FR-012-01 | สร้าง/แก้ไข/ลบ Work Instruction ต่อ Task หรือ Folder | Must |
| FR-012-02 | Rich Text Editor พร้อม Heading, Bold, List, Table | Must |
| FR-012-03 | แทรกรูปภาพใน Work Instruction | Should |
| FR-012-04 | Version Control: บันทึก Version ทุกครั้งที่ Save | Must |
| FR-012-05 | ดูประวัติ Version (Version History Panel) | Must |
| FR-012-06 | Compare ระหว่าง Version (Diff View) | Should |
| FR-012-07 | Export Work Instruction เป็น PDF | Should |
| FR-012-08 | แสดง Read Status: ใครอ่านแล้วบ้าง | Could |
| FR-012-09 | Link Work Instruction กับ Knowledge Base | Should |

### Business Rules

```
BR-012-01: Work Instruction Version เพิ่มขึ้น 1 ทุกครั้งที่ Save
BR-012-02: ดู Version ย้อนหลังได้แต่แก้ไขไม่ได้ (Read-only)
BR-012-03: เฉพาะเจ้าของ Task/Folder Owner หรือ Admin เท่านั้นที่แก้ไขได้
```

---

## SDD: Service Design — Task & Collaboration Layer

### TaskService.cs

```csharp
public class TaskService
{
    // สร้าง Task (Manual หรือ Auto จาก WFE)
    public Result CreateTask(TaskRequest req, string userADName, int? workflowStepInstanceID = null)
    
    // อัปเดต Status + Progress%
    public Result UpdateTaskStatus(int taskID, TaskStatus newStatus, string userADName)
    
    // Drag-and-Drop: ย้าย Task ไป Column (Kanban)
    public Result MoveTaskToColumn(int taskID, TaskStatus targetStatus, string userADName)
    
    // ดึง Tasks สำหรับ Kanban Board View
    public KanbanBoardData GetKanbanBoard(KanbanFilter filter, int userID)
    
    // ดึง Tasks สำหรับ Email Style View
    public PagedResult<TaskItem> GetEmailStyleView(TaskFilter filter, int userID)
    
    // คำนวณ Progress% จาก Sub-Tasks อัตโนมัติ
    private decimal CalculateProgressFromSubTasks(int parentTaskID)
    
    // Link Task กับ Entity ต่างๆ
    public Result LinkTask(int taskID, LinkEntityType entityType, int entityID)
}
```

### TodoService.cs

```csharp
public class TodoService
{
    // สร้าง Todo Item (Personal หรือ Shared)
    public Result CreateTodo(TodoRequest req, string userADName)
    
    // Toggle Complete/Incomplete + สร้าง Recurring Instance ถ้า Applicable
    public Result ToggleComplete(int todoID, string userADName)
    
    // ดึง Today's Todo Dashboard
    public TodayDashboard GetTodayDashboard(int userID)
    
    // Background Job: สร้าง Recurring Instance ถัดไป
    public void ProcessRecurringTodos()
}
```

---

*เอกสารนี้ครอบคลุม MOD-009 ถึง MOD-012 | CMS โครงการ 2511AL01 | Version 1.0*
