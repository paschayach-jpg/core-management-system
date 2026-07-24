# Business Requirements Document
## Core Management System (CMS)
### ระบบบริหารจัดการข้อมูลองค์กรกลาง


BUSINESS REQUIREMENTS DOCUMENT
Core Management System (CMS)
ระบบบริหารจัดการข้อมูลองค์กรกลาง
รหัสโครงการ
2511AL01
ประเภทโครงการ
Enterprise Document & Knowledge Management
วันที่เริ่มต้น
12 พฤศจิกายน 2568
ระยะเวลา
90 วันทำการ (ถึง 28 มีนาคม 2569)
Version
BRD v2.0 (Retroactive — Based on SRS/SDD)
สถานะ
Approved (Design Complete)

บริษัทประกันภัย — ฝ่ายเทคโนโลยีสารสนเทศ
มีนาคม 2569



## 1. Document Control

### 1.1 ประวัติการแก้ไข (Revision History)
Version
วันที่
ผู้แก้ไข
รายละเอียด
1.0
12 พ.ย. 2568
Business Analyst
สร้าง BRD เบื้องต้น
2.0
31 มี.ค. 2569
Senior SA
BRD ย้อนหลัง (Retroactive) จาก SRS v1.0 & SDD — ครบ 23 Modules


### 1.2 ผู้รับผิดชอบเอกสาร
บทบาท
ชื่อ
อนุมัติ/รับทราบ
Project Sponsor
TBD
☐ อนุมัติ
Business Owner
TBD
☐ อนุมัติ
IT Manager
TBD
☐ รับทราบ
Compliance Officer
TBD
☐ รับทราบ


> 💡 Design Rationale: BRD ฉบับนี้เป็น "Retroactive BRD" — สร้างภายหลังจาก SRS/SDD เพื่อให้ครบถ้วนตามหลัก SDLC ของ OIC Circular 2566 ที่กำหนดให้มีเอกสาร Business Requirements ก่อนการออกแบบระบบ เนื้อหาสอดคล้องกับสิ่งที่ออกแบบไว้จริงใน SRS v1.0 ทุกประการ




## 2. Executive Summary

### 2.1 ภาพรวมโครงการ
Core Management System (CMS) เป็นระบบ Enterprise-grade สำหรับบริหารจัดการเอกสาร ความรู้ และกระบวนการทางธุรกิจขององค์กรประกันภัย ออกแบบเพื่อรองรับการทำงานแบบ On-premise บน .NET 4.5 WinForms พร้อม SQL Server 2019 โดยมีโครงสร้างแบบ Modular MVC Architecture

รายการ
รายละเอียด
ชื่อโครงการ
Core Management System (CMS)
รหัสโครงการ
2511AL01
ประเภท
Enterprise Document & Knowledge Management System
Platform
.NET 4.5 WinForms + SQL Server 2019 (Thai_CI_AS)
Storage
Network Drive Share (On-premise) — \\bui-filecenter\Core Management
จำนวน Concurrent Users
50 Users (Design Capacity: 100 Users)
จำนวน Modules
23 Modules (MOD-001 ถึง MOD-023)
Compliance
OIC Circular 2566, CRAF Data Classification, PDPA Thailand 2562


### 2.2 บริบทและความจำเป็นทางธุรกิจ
ในฐานะบริษัทประกันภัย ความถูกต้องของข้อมูลและความเร็วในการให้บริการเป็นปัจจัยหลักในการสร้างความเชื่อมั่นให้กับลูกค้าและผู้ถือกรมธรรม์ ปัจจุบันองค์กรประสบปัญหา:
- เอกสารกระจายอยู่หลายที่ ขาดการจัดการแบบรวมศูนย์
- ไม่มีระบบ Workflow อัตโนมัติ ทำให้การอนุมัติล่าช้า
- ความรู้องค์กรสูญหายเมื่อพนักงานลาออก
- ขาดการติดตามงานและโครงการอย่างมีประสิทธิภาพ
- ไม่สามารถตรวจสอบย้อนหลัง (Audit Trail) ได้ครบถ้วนตามที่ OIC กำหนด


### 2.3 วัตถุประสงค์ทางธุรกิจ
วัตถุประสงค์
รายละเอียด
ผลลัพธ์ที่คาดหวัง
ลดความผิดพลาดของข้อมูล
จัดเก็บเอกสารแบบรวมศูนย์พร้อม Version Control
ลดลง 50%
เพิ่มความเร็วในการค้นหา
Full-text Search + Folder Classification
ลดลง 50%
Workflow อัตโนมัติ
23 Modules เชื่อมต่อกัน พร้อม SLA Timer
ลดลง 30%
รักษาความรู้องค์กร
Knowledge Base + Version History
Retention 90%
Compliance ตาม OIC
Audit Trail ครบทุก Transaction + PDPA Consent
Zero Violation



## 3. Module Overview — ภาพรวม 23 Modules

### 3.1 สถาปัตยกรรม Module (Architecture Overview)
ระบบ CMS แบ่งออกเป็น 23 Modules จัดกลุ่มตาม Functional Area ทุก Module เชื่อมต่อผ่าน Workflow Core และ Audit Service ร่วมกัน

Module ID
BR Ref
ชื่อ Module
รายละเอียด
Priority
กลุ่ม
MOD-001
BR-001
Folder Organization Structure
จัดการโครงสร้างแฟ้มตามลำดับชั้น Sub-Committee → Division → Department
Critical
Document
MOD-002
BR-002
Folder Structure Management
สร้าง จัดการ และลบโฟลเดอร์ รองรับ Hierarchy 10 ระดับ
Critical
Document
MOD-003
BR-003
File Upload and Storage
อัปโหลดไฟล์ Network Drive พร้อม Virus Scan และ Metadata
Critical
Document
MOD-004
BR-004
Folder Settings and Security
RBAC + ACL 4 ระดับ (1HC/2SC/3NC/4PC) + Permission Inheritance
Critical
Security
MOD-005
BR-005
File Sharing System
แชร์ภายในและ Share Link ภายนอก พร้อม Expiry และ Tracking
High
Collaboration
MOD-006
BR-006
Knowledge Base System
SOP, FAQ, Wiki, Template พร้อม Full-text Search
Critical
Knowledge
MOD-007
BR-007
Announcement System
ประกาศแบบ Targeted พร้อม Mandatory Read Tracking
Medium
Communication
MOD-008
BR-008
Workflow Management
Visual Workflow Designer + SLA Timer + Escalation
Critical
Process
MOD-009
BR-009
Task Management
Kanban Board + SLA + Sub-task 3 ระดับ
Critical
Process
MOD-010
BR-010
Todo List Management
Personal Todo + Priority + Due Date
Medium
Productivity
MOD-011
BR-011
Notes and Comments System
Rich Text Notes + Contextual Comments
Medium
Collaboration
MOD-012
BR-012
Work Instructions
SOP Step-by-step + ลิงก์กับ Workflow
Medium
Knowledge
MOD-013
BR-013
Project Management
Gantt Chart + WBS + Milestone Tracking
High
Project
MOD-014
BR-014
Resource Management
Resource Allocation + Capacity Planning
High
Project
MOD-015
BR-015
Timeline and Deadline
Timeline View รวม Project/Task/Workflow
Medium
Project
MOD-016
BR-016
Employee Management
ข้อมูลพนักงาน PDPA-Compliant + Skills Matrix
High
HR
MOD-017
BR-017
Programs List Management
คลัง Software/Application ขององค์กร
Medium
IT
MOD-018
BR-018
IT Landscape Mapping
System Integration Map + Dependency Visualization
Medium
IT
MOD-019
BR-019
Data Dictionary System
Enterprise Data Dictionary + Business Glossary
Medium
IT
MOD-020
BR-020
User and Role Management
AD Sync + RBAC + PDPA Consent Management
Critical
Admin
MOD-021
BR-021
Audit and Compliance
Audit Log 5+ ปี + OIC Compliance Reports
Critical
Compliance
MOD-022
BR-022
Configuration Management
System Config + Notification Template
Medium
Admin
MOD-023
BR-023
Integration Layer
REST API + AD Authentication + Legacy Integration
High
Integration


> 💡 Design Rationale: การแบ่ง 23 Modules สอดคล้องกับ MindMap.drawio ที่วาง Workflow ไว้เป็นศูนย์กลาง (Process Hub) โดย Document/Assign/Knowledge Base/Work Instruction เชื่อมต่อเข้าหา Workflow โดยตรง และทุก Module รายงานผลผ่าน Database → Data Dictionary




## 4. Business Requirements รายละเอียด

### 4.1 กลุ่ม Document & Folder Management (MOD-001 ถึง MOD-005)

> 💡 Design Rationale: กลุ่มนี้เป็นฐานราก (Foundation) ของระบบ เพราะทุก Module อื่นอ้างอิงไฟล์และโฟลเดอร์ การออกแบบ 4 ระดับความลับ (1HC/2SC/3NC/4PC) ตอบสนอง CRAF Data Classification และ OIC Circular 2566 ที่กำหนดให้จำแนกข้อมูลตามความอ่อนไหว


##### BR-001: Folder Organization Structure
รายการ
รายละเอียด
Priority
Critical
ความต้องการทางธุรกิจ
ต้องการระบบบริหารโครงสร้างแฟ้มที่ยืดหยุ่น รองรับหลายมุมมองตามระดับความลับ
Functional Requirements
Hierarchy 3 ระดับ: Sub-Committee → Division → Department
Multi-view: มองตามความลับ (1HC/2SC/3NC/4PC) หรือตามองค์กร
TreeView Expand/Collapse + Search
Soft Delete สำหรับ Audit Trail
Business Rules
แต่ละ Department ต้องอยู่ใต้ Division เดียวเท่านั้น
Admin เท่านั้นที่แก้ไขโครงสร้างได้
ชื่อต้องไม่ซ้ำกันภายใต้ Parent เดียวกัน
Acceptance Criteria
- ✓ ผู้ใช้สร้าง/แก้ไข/ลบหน่วยโครงสร้างได้
- ✓ TreeView แสดง Hierarchy ครบถ้วน
- ✓ เปลี่ยน View ระหว่างความลับและองค์กรได้
- ✓ การเปลี่ยนแปลงถูก Log พร้อม ADUserName และ Timestamp


##### BR-002: Folder Structure Management
รายการ
รายละเอียด
Priority
Critical
ความต้องการทางธุรกิจ
จัดระเบียบเอกสารใน Hierarchy ที่สะท้อนโครงสร้างองค์กร รองรับ Template เพื่อความสม่ำเสมอ
Functional Requirements
สร้างโฟลเดอร์จาก Template หรือ Manual
รองรับ Sub-folder ลึกสุด 10 ระดับ
Color-coded: 1HC=แดง, 2SC=ส้ม, 3NC=น้ำเงิน, 4PC=เขียว
Recycle Bin 30 วัน
Business Rules
ชื่อโฟลเดอร์ห้ามมี \ / : * ? " < > |
โฟลเดอร์ที่มีไฟล์ต้องได้รับ Admin Approval ก่อนลบ
Archive ซ่อนแต่ยังเก็บข้อมูล
Acceptance Criteria
- ✓ สร้างโฟลเดอร์จาก Template ไม่เกิน 3 Clicks
- ✓ Tree แสดง Color-coded ครบถ้วน
- ✓ ลบแล้ว Recover ได้ภายใน 30 วัน


##### BR-003: File Upload and Storage
รายการ
รายละเอียด
Priority
Critical
ความต้องการทางธุรกิจ
อัปโหลดและจัดเก็บไฟล์อย่างปลอดภัยพร้อม Access Control, Virus Scan และ Audit Trail
Functional Requirements
อัปโหลดหลายไฟล์พร้อมกัน (สูงสุด 10 ไฟล์)
Drag-and-drop + Progress Indicator
จัดเก็บบน Network Drive: \\bui-filecenter\Core Management
Metadata ใน Database (FileID, ชื่อ, ขนาด, Type, Checksum)
Virus Scan อัตโนมัติก่อนจัดเก็บ
Business Rules
ขนาดไฟล์สูงสุด 100 MB
ประเภทไฟล์ที่รองรับ: PDF, DOC, DOCX, XLS, XLSX, PPT, PPTX, JPG, PNG, GIF, ZIP
Quota 40 GB ต่อ Department (ปรับได้ใน Config)
ไฟล์อายุ 7 ปีย้ายไป Archive (ปรับได้)
Acceptance Criteria
- ✓ อัปโหลดหลายไฟล์พร้อมกันได้
- ✓ ไฟล์ที่ติดไวรัสถูก Reject พร้อมแจ้งเตือน
- ✓ Metadata ถูกต้องใน Database
- ✓ Progress แสดง Real-time


##### BR-004: Folder Settings and Security
รายการ
รายละเอียด
Priority
Critical
ความต้องการทางธุรกิจ
ควบคุมการเข้าถึงเอกสารผ่าน Folder-level Security ตาม Hierarchy และ Role
Functional Requirements
กำหนด Permission 5 ระดับ: Read/Write/Delete/Modify/Full Control
Permission by User, Role, OrgUnit, Group
Inheritance จาก Parent Folder (Override ได้โดย Admin)
Approval Workflow สำหรับ Permission Change
Visual Indicator (สี, Lock Icon, Share Icon)
Business Rules
1HC ต้องได้ Executive Approval ก่อนสร้าง
External Sharing ต้องได้ Department Head Approval
Permission เปลี่ยนแปลงต้อง Log ทุกครั้ง
Explicit Deny Override Allow เสมอ
Acceptance Criteria
- ✓ สีโฟลเดอร์ตรงกับระดับความลับ
- ✓ ผู้ใช้ที่ไม่มีสิทธิ์เข้าถึงไม่ได้
- ✓ Permission เปลี่ยนแปลงมีผลทันที


##### BR-005: File Sharing System
รายการ
รายละเอียด
Priority
High
ความต้องการทางธุรกิจ
แชร์ไฟล์ภายในองค์กรและภายนอกอย่างปลอดภัย พร้อม Expiry และ Tracking
Functional Requirements
Internal Share: User/Department/Role/Group
Share Link: GUID-based Token + Password + Max Downloads + Expiry
Access Log: ผู้เข้าถึง, วันเวลา, จำนวน Download
Email Notification เมื่อมีการแชร์หรือเข้าถึง
Business Rules
แชร์สูงสุด 90 วัน
1HC ไม่อนุญาต External Share
Manager ต้อง Approve External Share
Link หมดอายุถูก Disable อัตโนมัติ
Acceptance Criteria
- ✓ Share Link ทำงานเฉพาะในช่วง Validity
- ✓ Password-protected Link ต้องใส่ Password ถูก
- ✓ การเข้าถึงทุกครั้งถูก Log


### 4.2 กลุ่ม Knowledge Management & Communication (MOD-006 ถึง MOD-007)

> 💡 Design Rationale: Knowledge Management เป็น "สมองขององค์กร" — การจัดเก็บ SOP/FAQ/Template ช่วยลด Training Cost และ Error Rate โดยเฉพาะในธุรกิจประกันภัยที่ต้องอ้างอิง Regulation อย่างต่อเนื่อง


##### BR-006: Knowledge Base System
รายการ
รายละเอียด
Priority
Critical
ความต้องการทางธุรกิจ
รวบรวมความรู้องค์กร (SOP, FAQ, Template, Best Practice) ไว้ที่เดียวพร้อม Search ที่มีประสิทธิภาพ
Functional Requirements
Content Types: Wiki, SOP, Template, Best Practice, FAQ
Rich Text Editor + Version Control + Review Workflow
Full-text Search + Category/Tag Filter
Link Knowledge กับ Folder/Task/Program/Workflow
Import Excel + Export PDF/Word
Business Rules
เผยแพร่ต้องผ่าน Reviewer Approval
บทความหมดอายุ Auto-Unpublish
Review บังคับทุก 12 เดือน
Draft มองเห็นเฉพาะ Author และ Reviewer
Acceptance Criteria
- ✓ ค้นหาผลลัพธ์ภายใน 2 วินาที
- ✓ Version History บันทึกและดูย้อนหลังได้
- ✓ Excel Import สร้าง Article ที่ถูกต้อง


##### BR-007: Announcement System
รายการ
รายละเอียด
Priority
Medium
ความต้องการทางธุรกิจ
สื่อสารข่าวสารและประกาศสำคัญถึงกลุ่มเป้าหมายที่กำหนดพร้อม Tracking การอ่าน
Functional Requirements
ตั้งค่า Target Audience: OrgUnit/Role/User/All
Priority: Critical/High/Medium/Low
Mandatory Read: ผู้ใช้ต้อง Confirm ก่อนใช้งาน
Pop-up เมื่อ Login หรือตาม Schedule
Email Notification (Optional)
Business Rules
Admin/Manager เท่านั้นที่สร้างประกาศได้
Critical Priority แสดง Pop-up บังคับ
บันทึกสถิติการอ่านต่อรายบุคคล
Acceptance Criteria
- ✓ ส่งประกาศถึงกลุ่มเป้าหมายที่กำหนดเท่านั้น
- ✓ Mandatory Read บังคับก่อน Dismiss
- ✓ Read Rate Report สร้างได้


### 4.3 กลุ่ม Workflow & Task Automation (MOD-008 ถึง MOD-012)

> 💡 Design Rationale: Workflow เป็น "หัวใจ" ของระบบ (ตามที่ MindMap.drawio วาง Process ไว้ตรงกลาง) — การ Freeze Workflow Instance ไว้กับ Version Definition ณ เวลาเริ่มงาน ป้องกันการรบกวน Process ที่กำลังทำงานอยู่เมื่อมีการแก้ไข Template


##### BR-008: Workflow Management
รายการ
รายละเอียด
Priority
Critical
ความต้องการทางธุรกิจ
ออกแบบ กำหนด และติดตาม Business Process แบบ Visual พร้อม SLA Timer
Functional Requirements
Visual Workflow Designer (Drag-and-drop Nodes)
Node Types: Start, Task, Decision, Parallel, Merge, End
SLA Timer + Escalation อัตโนมัติ (ไม่นับ Weekend/Holiday)
Workflow Versioning — Instance Frozen ณ Version ที่เริ่ม
Monitor Dashboard: Real-time Status + SLA Bar
Business Rules
Workflow ที่ Active ไม่สามารถลบได้ (Deactivate เท่านั้น)
Instance Running ใช้ Version Definition ที่ Freeze ไว้
SLA คำนวณจาก Working Hours ผ่าน fn_AddWorkingHours()
Manager/Admin เท่านั้นที่ Approve Template ได้
Acceptance Criteria
- ✓ Designer สร้าง Workflow ได้โดยไม่ต้องเขียน Code
- ✓ SLA Warning แสดงก่อนหมดเวลา
- ✓ Dashboard แสดง Instance Status Real-time


##### BR-009: Task Management
รายการ
รายละเอียด
Priority
Critical
ความต้องการทางธุรกิจ
บริหารงานแบบ Kanban Board พร้อม SLA, Sub-task และ Dependency
Functional Requirements
Kanban Board: Configurable Column ต่อ User (JSON ใน UserPreference)
Sub-task 3 ระดับ (ลึกกว่า 3 ระดับไม่รองรับ)
Task Dependency (Predecessor/Successor)
SLA Timer + Overdue Alert
Assign ให้ User/Role/Group
Business Rules
Sub-task สูงสุด 3 ระดับ
Parent Task ปิดไม่ได้ถ้า Sub-task ยังค้างอยู่
Kanban Column Config เก็บต่อ User ใน UserPreference Table
Acceptance Criteria
- ✓ Kanban ลาก Task ข้าม Column ได้
- ✓ Sub-task แสดง Progress ใน Parent
- ✓ SLA หมดเวลาแสดง Alert อัตโนมัติ


##### BR-010: Todo List Management
รายการ
รายละเอียด
Priority
Medium
ความต้องการทางธุรกิจ
Personal Todo List สำหรับผู้ใช้แต่ละคน
Functional Requirements
สร้าง/แก้ไข/ลบ Todo Item
Priority: High/Medium/Low
Due Date + Reminder
กรองตาม Status และ Priority
Business Rules
Todo เป็น Personal Data ของผู้ใช้แต่ละคน
Admin มองเห็น Todo ของ User อื่นได้เฉพาะ Audit
Acceptance Criteria
- ✓ สร้าง Todo ได้ไม่จำกัดจำนวน
- ✓ Reminder แจ้งเตือนตามที่กำหนด


##### BR-011: Notes and Comments System
รายการ
รายละเอียด
Priority
Medium
ความต้องการทางธุรกิจ
บันทึกหมายเหตุและ Comment แบบ Contextual กับ Entity ต่างๆ
Functional Requirements
Rich Text Notes ผูกกับ File/Task/Workflow/Project
Comment Thread พร้อม @Mention
Note Version History
Private/Public Notes
Business Rules
Author แก้ไข Note ของตัวเองได้ภายใน 24 ชั่วโมง
Deleted Comment แสดง [Deleted] ไม่ Hard Delete
Acceptance Criteria
- ✓ Comment ผูกกับ Entity ที่ถูกต้อง
- ✓ @Mention ส่ง Notification ให้ผู้ถูกกล่าวถึง


##### BR-012: Work Instructions
รายการ
รายละเอียด
Priority
Medium
ความต้องการทางธุรกิจ
คำแนะนำขั้นตอนการทำงานแบบ Step-by-step เชื่อมต่อกับ Workflow
Functional Requirements
Step-by-step Instructions + Screenshot
ลิงก์กับ Workflow Step โดยตรง
Checklist สำหรับแต่ละ Step
Print-friendly Format
Business Rules
Work Instruction ต้อง Review ทุก 6 เดือน
เชื่อมกับ Workflow Step ได้หลาย Instruction ต่อ 1 Step
Acceptance Criteria
- ✓ พิมพ์ Work Instruction เป็น PDF ได้
- ✓ ลิงก์กับ Workflow Step แสดงถูกต้อง



### 4.4 กลุ่ม Project & Resource Management (MOD-013 ถึง MOD-015)

> 💡 Design Rationale: Project Management เชื่อมต่อกับ Workflow (Task สร้างจาก Project) และ Resource Management — การใช้ Gantt Chart ใน WinForms ต้องการ Third-party Control (เช่น Bryntum หรือ Custom GDI+) เพราะ .NET 4.5 ไม่มี Gantt built-in


##### BR-013: Project Management
รายการ
รายละเอียด
Priority
High
ความต้องการทางธุรกิจ
วางแผนและติดตามโครงการผ่าน Gantt Chart, WBS และ Milestone
Functional Requirements
Gantt Chart แสดง Timeline และ Dependency
WBS (Work Breakdown Structure) Tree
Milestone Tracking
Health Dashboard: Budget/Timeline/Risk
Export Gantt เป็น PDF/Excel
Business Rules
HealthStatus คำนวณจาก 3 Dimension: Budget/Time/Risk
Project ต้องมี PM ที่ได้รับมอบหมาย
Phase ลบไม่ได้ถ้ามี Task ที่กำลัง Running
Acceptance Criteria
- ✓ Gantt แสดง Dependency ถูกต้อง
- ✓ Milestone ที่เลยกำหนดแสดง Alert
- ✓ Export Gantt เป็น PDF ที่อ่านได้


##### BR-014: Resource Management
รายการ
รายละเอียด
Priority
High
ความต้องการทางธุรกิจ
วางแผนจัดสรรทรัพยากรบุคคลและ Over-allocation Detection
Functional Requirements
Resource Allocation ต่อ Project/Task
Capacity Planning View
Over-allocation Alert (> 100%)
Resource Calendar (วันหยุด/ลา)
Utilization Report
Business Rules
Resource ที่ถูก Allocate > 100% แสดง Warning
วันหยุดและวันลาต้อง Sync จาก SystemCalendar
Manager เท่านั้นที่ Approve Resource Allocation
Acceptance Criteria
- ✓ Over-allocation แสดงเด่นชัด
- ✓ Capacity Chart แสดงถูกต้องตาม Calendar


##### BR-015: Timeline and Deadline
รายการ
รายละเอียด
Priority
Medium
ความต้องการทางธุรกิจ
รวม Timeline ของ Project/Task/Workflow ในมุมมองเดียว
Functional Requirements
Unified Timeline View
กรองตาม Type/Assignee/Department
Color-coded Status
Deadline Alert
Business Rules
Timeline อ้างอิง Working Day จาก SystemCalendar
Acceptance Criteria
- ✓ Timeline รวม 3 ประเภท Event ได้
- ✓ Deadline ที่ใกล้มาแสดง Warning


### 4.5 กลุ่ม HR, IT Landscape & Data (MOD-016 ถึง MOD-019)

> 💡 Design Rationale: กลุ่มนี้เป็น Supporting Infrastructure ที่ให้ข้อมูล Context กับ Modules อื่น เช่น Employee Skills ช่วย Resource Management, IT Landscape ช่วย Integration Planning และ Data Dictionary เป็นแหล่งอ้างอิงสำหรับ Developer


##### BR-016: Employee Management
รายการ
รายละเอียด
Priority
High
ความต้องการทางธุรกิจ
บริหารข้อมูลพนักงานอย่างครบถ้วนตาม PDPA โดยมี Consent Management ระดับ Field
Functional Requirements
ข้อมูลพนักงาน: ชื่อ, ตำแหน่ง, แผนก, ทักษะ
Skills Matrix View
PDPA Consent ต่อ Field (Email/Phone/Address)
Certificate Tracking + Expiry
Emergency Contact (PII Third-party)
Business Rules
Email/Phone/Address ต้องขอ Consent ก่อนแสดง
Right to Erasure: Anonymize แทน Hard Delete
EmergencyContact เป็น PII Third-party ต้องขอ Consent แยก
Row-Level Security สำหรับ HR Data
Acceptance Criteria
- ✓ ผู้ใช้ที่ไม่มีสิทธิ์มองไม่เห็น PII Fields
- ✓ Consent Revoke ซ่อน Field ทันที
- ✓ Certificate ที่หมดอายุแสดง Alert


##### BR-017: Programs List Management
รายการ
รายละเอียด
Priority
Medium
ความต้องการทางธุรกิจ
บัญชี Software/Application ขององค์กรพร้อม Owner และ Access Information
Functional Requirements
ลงทะเบียน Program: ชื่อ, Vendor, Version, Status
Business Owner + Technical Owner
Access URL/Path
Relationship กับ Department/Team
Business Rules
Credential ต้อง Encrypt ก่อนเก็บ
Program ที่ Decommission ต้อง Archive ไม่ใช่ลบ
Acceptance Criteria
- ✓ ค้นหา Program ตาม Keyword ได้
- ✓ Credential แสดงเฉพาะ Admin/Owner


##### BR-018: IT Landscape Mapping
รายการ
รายละเอียด
Priority
Medium
ความต้องการทางธุรกิจ
ภาพรวม Integration ระหว่างระบบและ Dependency Visualization
Functional Requirements
System Integration Map แบบ Visual Graph
Dependency Direction + Type
Change Impact Analysis
Export เป็น PNG/PDF
Business Rules
Integration Change ต้องมี Approval
Version ของ Integration Map เก็บ History
Acceptance Criteria
- ✓ Map แสดง Dependency ถูกต้อง
- ✓ Impact Analysis ระบุ Downstream System


##### BR-019: Data Dictionary System
รายการ
รายละเอียด
Priority
Medium
ความต้องการทางธุรกิจ
Enterprise Data Dictionary และ Business Glossary สำหรับ Developer และ Business User
Functional Requirements
Table/Column Definition ระดับ Database
Business Term Glossary
Data Lineage (Source → Target)
Search ตาม Term/Table/Column
Business Rules
Update Data Dictionary ต้อง Approve โดย Data Architect
Import/Export Excel สำหรับ Bulk Update
Acceptance Criteria
- ✓ Search Term ให้ผลลัพธ์ใน 2 วินาที
- ✓ Export Data Dictionary เป็น Excel ได้



### 4.6 กลุ่ม System Administration (MOD-020 ถึง MOD-023)

> 💡 Design Rationale: กลุ่มนี้เป็น "น้ำหล่อเลี้ยง" ทั้งระบบ — MOD-021 (Audit) เรียกใช้จากทุก Module ผ่าน AuditService.Log() แบบ Async ตาม OIC Circular 2566 ที่กำหนดให้เก็บ Log ไม่น้อยกว่า 5 ปี โดยใช้ ADUserName (Snapshot) ไม่ใช่ UserID FK เพื่อให้ Log ยังคงสมบูรณ์แม้ User Account ถูกลบ


##### BR-020: User and Role Management
รายการ
รายละเอียด
Priority
Critical
ความต้องการทางธุรกิจ
บริหารผู้ใช้งานผ่าน AD Sync พร้อม RBAC และ PDPA Consent Management
Functional Requirements
AD Sync อัตโนมัติ (LDAP)
Role: Officer/Sr.Officer/Manager/Admin
PDPA Consent: ConsentGiven + ConsentDate ระดับ Field
User Group Management
Permission Matrix Report
Business Rules
Login ครั้งแรกต้อง Accept PDPA Consent
Account ถูก Disable ใน AD ต้อง Block ใน CMS ทันที
ADUserName ใช้ Stamp ใน Audit ไม่ใช่ UserID
Acceptance Criteria
- ✓ AD Sync ทำงานอัตโนมัติ
- ✓ PDPA Consent บังคับก่อนใช้งาน
- ✓ Permission Matrix Export ได้


##### BR-021: Audit and Compliance
รายการ
รายละเอียด
Priority
Critical
ความต้องการทางธุรกิจ
บันทึก Audit Trail ทุก Transaction ตาม OIC Circular 2566 เก็บ 5+ ปี
Functional Requirements
Append-only AuditLog (INSERT+SELECT เท่านั้น)
ไม่มี Foreign Key บน AuditLog (Log Independence)
Online 1 ปี + Archive 7 ปี
OIC Compliance Report
PDPA Access Log สำหรับ PII Fields
Export Log เป็น CSV/Excel
Business Rules
AuditLog ไม่มี UPDATE/DELETE
Stamp ADUserName Snapshot ไม่ใช่ UserID FK
เก็บ Log ≥ 5 ปี (OIC)
ดู PII Field ต้อง Log ทันที
Acceptance Criteria
- ✓ AuditLog ไม่สามารถแก้ไขหรือลบได้
- ✓ Compliance Report Export ได้ภายใน 2 นาที
- ✓ Log ครบ 100% ของ Critical Operations


##### BR-022: Configuration Management
รายการ
รายละเอียด
Priority
Medium
ความต้องการทางธุรกิจ
จัดการ System Config และ Notification Template แบบรวมศูนย์
Functional Requirements
Key-Value Config Store
Config Groups: Security/Email/SLA/Storage/UI
Notification Template (Email/In-app)
Encrypted Config สำหรับ Sensitive Values
Change Approval สำหรับ Critical Config
Business Rules
Critical Config เปลี่ยนแปลงต้อง Approve
Encrypted Config ไม่แสดง Plain text
Config Change Log เก็บทุกครั้ง
Acceptance Criteria
- ✓ Config เปลี่ยนแปลงมีผลทันทีหรือ Restart ตามประเภท
- ✓ Template ส่ง Email ได้ถูกต้อง


##### BR-023: Integration Layer
รายการ
รายละเอียด
Priority
High
ความต้องการทางธุรกิจ
REST API และ Integration กับ Legacy System ขององค์กร
Functional Requirements
REST API Endpoints สำหรับ External Access
AD Authentication (LDAP/LDAPS)
API Token Management
Webhook สำหรับ Event-driven Integration
Legacy System Connector (File-based/Database)
Business Rules
API Token Expire ตาม Config
HTTPS บังคับสำหรับ API
Rate Limiting ป้องกัน Abuse
API Access Log ทุก Call
Acceptance Criteria
- ✓ API ตอบสนอง < 500ms สำหรับ Simple Query
- ✓ AD Auth ทำงานถูกต้อง
- ✓ Webhook Delivery มี Retry Mechanism



## 5. Non-Functional Requirements

### 5.1 Performance
รายการ
เป้าหมาย
เงื่อนไข
Page Load Time
< 3 วินาที (P95)
50 Concurrent Users
Search Response
< 2 วินาที
Full-text Search
File Upload (10 MB)
< 30 วินาที
Network Drive
API Response
< 500 ms
Simple CRUD
Concurrent Users
50 (รองรับ 100)
ไม่มีการ Degrade


### 5.2 Security & Compliance
- Windows Authentication (AD) — ไม่มี Local Password
- HTTPS บังคับทั้งระบบ + TLS 1.2 ขึ้นไป
- RBAC: Officer / Sr.Officer / Manager / Admin
- Folder-level ACL (4 ระดับ: 1HC/2SC/3NC/4PC)
- Audit Trail: Append-only, ADUserName Snapshot, เก็บ ≥ 5 ปี (OIC)
- PDPA: Consent per Field, Anonymize แทน Delete
- CRAF Data Classification ทุก Folder


### 5.3 Availability & Recovery
รายการ
เป้าหมาย
System Uptime
≥ 99.5% (Excluding Planned Maintenance)
Backup Frequency
Database: Daily Full + Hourly Log / Network Drive: Daily
RTO (Recovery Time Objective)
< 4 ชั่วโมง
RPO (Recovery Point Objective)
< 1 ชั่วโมง


### 5.4 Compatibility
- OS: Windows 10 / Windows Server 2019
- .NET Framework 4.5 (Backward Compatible — ห้ามใช้ Feature ที่ต้องการ 4.6+)
- SQL Server 2019 + Thai_CI_AS Collation
- Network Drive: UNC Path + Windows Credentials
- Browser: ไม่ใช้ Web Frontend (WinForms Only)



## 6. PDPA Data Elements & Consent Requirements

> 💡 Design Rationale: PDPA Thailand 2562 กำหนดให้ต้องขอ Consent ก่อนเก็บ/ประมวลผลข้อมูลส่วนบุคคล (Personal Data) ระบบ CMS ออกแบบให้บังคับ Consent ที่ Login ครั้งแรก และ Revoke Consent ได้ตลอดเวลา โดยใช้ ConsentGiven + ConsentDate ระดับ Field ใน m_Employee



### 6.1 ตาราง PII Data Elements
ตาราง
Column
PII Level
Consent Required
Retention
m_UserAccount
ADUserName
PII
สัญญาจ้าง
Employment + 3 ปี
m_UserAccount
DisplayName
PII
สัญญาจ้าง
Employment + 3 ปี
m_UserAccount
Email
PII Sensitive
ต้องขอ Consent
Employment + 3 ปี
m_Employee
FullNameTH/EN
PII
สัญญาจ้าง
Employment + 5 ปี
m_Employee
Email
PII Sensitive
ต้องขอ Consent
Employment + 3 ปี
m_Employee
MobilePhone
PII Sensitive
ต้องขอ Consent
Employment + 3 ปี
m_Employee
Address
PII Sensitive
ต้องขอ Consent
Employment + 3 ปี
m_Employee
EmergencyContact
PII Third-Party
ต้องขอ Consent แยก
Employment + 3 ปี
m_Employee
CertificateNumber
PII
สัญญาจ้าง
Employment + 5 ปี
l_AuditLog
ADUserName
PII
ไม่ต้อง (OIC กฎหมาย)
5 ปี (OIC)
l_AuditLog
IPAddress
PII
ไม่ต้อง (Legitimate Interest)
5 ปี


### 6.2 PDPA Rights ที่รองรับ
Right
การรองรับ
Right to Access
Export ข้อมูล Personal Data ตามคำขอผ่าน MOD-021
Right to Rectification
แก้ไขข้อมูลผ่าน MOD-016 (Employee Management)
Right to Erasure
Anonymize แทน Hard Delete — เก็บ EmployeeID ไว้สำหรับ Audit Trail
Right to Withdraw Consent
ซ่อน Email/Phone แต่ยังคง Record ไว้ — บันทึก RevocationDate
Right to Data Portability
Export เป็น JSON/CSV ผ่าน Admin Function



## 7. Compliance Requirements

### 7.1 OIC Circular 2566
ข้อกำหนด
Module ที่รองรับ
การปฏิบัติ
เก็บ Log ≥ 5 ปี
MOD-021
AuditLog Online 1 ปี + Archive 7 ปี
บันทึก Action ทุก Transaction
MOD-021 (ทุก Module)
Async AuditService.Log()
Access Control ที่เหมาะสม
MOD-004, MOD-020
RBAC + Folder ACL
ระบบสำรองข้อมูล
IT Infrastructure
Daily Backup + Recovery Plan


### 7.2 CRAF Data Classification
ระดับ
สี
สิทธิ์เข้าถึง
Module
1HC — High Confidential
แดง
Executive Level เท่านั้น
MOD-001, 002, 004
2SC — Semi Confidential
ส้ม
Department Head ขึ้นไป
MOD-001, 002, 004
3NC — Normal Confidential
น้ำเงิน
พนักงานที่ได้รับอนุญาตในแผนก
MOD-001, 002, 004
4PC — Public Confidential
เขียว
พนักงานทุกคน (Read-only)
MOD-001, 002, 004



## 8. Technical Architecture Overview

### 8.1 Technical Stack
Layer
Technology
เหตุผล
Presentation Layer
.NET 4.5 WinForms + MDI Shell
Backward Compatibility กับ Environment องค์กร
Business Logic Layer
C# Service Classes + Stored Procedures
แยก Logic ออกจาก UI ตาม MVC Pattern
Data Access Layer
ADO.NET + Stored Procedures
.NET 4.5 ไม่รองรับ EF Core 6+ ได้เต็มที่
Database
SQL Server 2019 (Thai_CI_AS)
รองรับภาษาไทย + FULLTEXT Index
File Storage
Network Drive Share (UNC Path)
On-premise ไม่ใช้ Cloud ตาม Constraint
Authentication
Windows AD (LDAP/LDAPS)
Single Sign-On ผ่าน Domain Account
Architecture Pattern
Modular MVC — Visual Designer แยกจาก Code-behind
Maintainability + รองรับการพัฒนาต่อเนื่อง


### 8.2 Database Design Principles
- Table Prefix: m_ (Master), s_ (Setup), t_ (Transaction), l_ (Log), r_ (Reference), c_ (Config)
- Collation: Thai_CI_AS บน NVARCHAR ทุก Column
- Audit Stamp: ADUserName (Snapshot) ไม่ใช่ UserID FK
- AuditLog: Append-only, ไม่มี Foreign Key, เก็บ ≥ 5 ปี
- SLA: คำนวณผ่าน fn_AddWorkingHours() โดยอ้างอิง SystemCalendar
- Workflow Versioning: DefinitionSnapshot ใน t_WorkflowInstance


### 8.3 Integration Architecture
- AD Sync: LDAP Pull ทุก N ชั่วโมง (Config) + Real-time Disable Trigger
- REST API: Token-based, Rate Limiting, HTTPS Only
- Legacy Integration: File-based Connector + DB-link Connector
- Webhook: Event-driven ส่ง JSON Payload พร้อม Retry 3 ครั้ง



## 9. Risk Assessment
Risk ID
รายละเอียด
โอกาส
ผลกระทบ
ความรุนแรง
มาตรการ
R-001
Network Drive Performance ต่ำ
ปานกลาง
สูง
สูง
Performance Test + Capacity Plan + SAN Upgrade
R-002
User ต่อต้านการเปลี่ยนแปลง
สูง
ปานกลาง
สูง
Change Management Plan + Training + Executive Champion
R-003
Integration กับ Legacy System ซับซ้อน
ปานกลาง
สูง
สูง
Early Integration Test + Phased Rollout + Fallback
R-004
Data Migration มีปัญหา
ปานกลาง
สูง
สูง
Pilot Migration 10% + Validation Script + Rollback Plan
R-005
Scope Creep
สูง
สูง
Critical
Strict Change Control + Scope Baseline + CCB
R-006
Security Vulnerability
ต่ำ
สูง
สูง
Penetration Test + Security Code Review + Patch Policy
R-007
OIC Compliance ไม่ผ่าน
ต่ำ
Critical
Critical
Audit Trail Review + Compliance Checklist + Pre-UAT Audit
R-008
PDPA Violation
ต่ำ
Critical
Critical
DPIA + Consent Framework + DPO Review



## 10. Success Criteria

### 10.1 Project Success Metrics
Metric
เป้าหมาย
วิธีวัด
On-time Delivery
90 วันทำการ
Project Completion Date vs Plan
Document Search Time
ลดลง 50%
ก่อน: 10 นาที → เป้า: 5 นาที (Time Study)
Approval Cycle Time
ลดลง 30%
ก่อน: 5 วัน → เป้า: 3.5 วัน (Workflow Report)
Task SLA Compliance
≥ 95%
Task Management Report
User Adoption
≥ 80% ใน 3 เดือน
Active User Count + Login Frequency
System Uptime
≥ 99.5%
Monitoring Tools
Audit Trail Completeness
100% Critical Operations
AuditLog Review (OIC Requirement)
Compliance Violations
Zero
Compliance Audit Results



## 11. Appendices

### 11.1 เอกสารอ้างอิง
เอกสาร
Version
หมายเหตุ
CMS_SRS_Full_v1_0.docx
v1.0
System Requirements Specification (1,305 paragraphs)
CMS_DataDictionary.docx
v1.0
Database Schema — 15 Tables + 14 Stored Procedures
CMS_System_Features.docx
v1.0
System Features Specification (MOD-001 ถึง MOD-004)
CMS_StoredProcedures.sql
v1.0
SP-015 ถึง SP-035 + fn_AddWorkingHours
INDEX_DatabaseAddendum.md
v1.0
Database Tables เพิ่มเติม MOD-005 ถึง MOD-023
MindMap.drawio
v1.0
Module Connection Overview (Workflow เป็น Process Hub)


### 11.2 Glossary
คำศัพท์
ความหมาย
ADUserName
ชื่อผู้ใช้ใน Active Directory — ใช้ Stamp ใน Audit Trail แทน UserID
Audit Trail
บันทึกการกระทำทุกอย่างในระบบ — Append-only, ไม่สามารถแก้ไขหรือลบได้
CRAF
Cyber Risk Assessment Framework — กรอบการจำแนกความเสี่ยงและข้อมูลตาม OIC
OIC Circular 2566
ประกาศสำนักงาน คปภ. ปี 2566 ว่าด้วยการบริหารจัดการความเสี่ยงด้าน IT
PDPA
Personal Data Protection Act 2562 — พระราชบัญญัติคุ้มครองข้อมูลส่วนบุคคล
SLA
Service Level Agreement — กำหนดเวลาในการดำเนินงาน คำนวณเฉพาะ Working Hours
1HC/2SC/3NC/4PC
ระดับความลับของข้อมูล: High Confidential / Semi / Normal / Public Confidential
Retroactive BRD
BRD ที่สร้างย้อนหลังหลังจากที่ออกแบบ SRS/SDD แล้ว เพื่อให้เอกสารครบถ้วนตาม SDLC


> 💡 Design Rationale: BRD ฉบับนี้ผ่านการทบทวนความสอดคล้องกับ SRS v1.0, Data Dictionary, และ Stored Procedures เป็นการ Retroactive Design Documentation ที่ครอบคลุม 23 Modules ครบถ้วน ข้อกำหนดทุกข้อสามารถ Trace ไปยัง Module Design ใน SRS ได้


| Core Management System (CMS) — BRD v2.0 | Project: 2511AL01 | Confidential |
| --- | --- |


| บริษัทประกันภัย | ลับ — ห้ามเผยแพร่ | หน้า  |
| --- | --- |

