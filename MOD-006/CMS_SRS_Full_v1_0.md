# Software Requirements Specification
## Core Management System (CMS)
### SRS-CMS-2511AL01-v1.0 | IEEE 830 / ISO/IEC 29148

SRS
Software Requirements Specification

Core Management System (CMS)
ระบบการจัดการแกนกลาง — บริษัทประกันภัย
รหัสโครงการ
2511AL01
Version
1.0
วันที่จัดทำ
มีนาคม 2569
Modules
MOD-001~023
ขอบเขตเอกสาร
เอกสารนี้กำหนดข้อกำหนดซอฟต์แวร์ตามมาตรฐาน IEEE 830 / ISO/IEC 29148 สำหรับระบบ CMS
ครอบคลุม Functional Requirements, Non-Functional Requirements, PDPA, Compliance, และ Architecture Constraints

Confidential — For Internal Use Only | .NET 4.5 WinForms | SQL Server 2019 | On-Premise



## 1. Document Control

Field
Details
Document Title
Software Requirements Specification — Core Management System (CMS)
Document ID
SRS-CMS-2511AL01-v1.0
Project Code
2511AL01
Project Name
Core Management System (CMS)
Version
1.0
Status
Final — Retrospective (จัดทำย้อนหลังจาก SDD/SFS)
Date
March 2026 (มีนาคม 2569)
Author
Senior System Analyst & Domain Expert (Insurance)
Reviewer
IT Manager, Compliance Officer, Business Owner
Approver
Project Sponsor
Standard
IEEE 830-1998 / ISO/IEC 29148:2018
Classification
Internal Confidential — CRAF 2SC


### 1.1 Revision History
Version
Date
Author
Change Description
1.0
มีนาคม 2569
Senior SA
Initial retrospective SRS — จัดทำจาก BRD v1.0, SDD v1.0, SFS v2.0


### 1.2 Referenced Documents
Document
Version
Date
Location
Business Requirements Document (BRD)
1.0
Nov 2568
Project Files / BRD.md
System Design Document (SDD)
1.0
Dec 2568
Project Files / CMS_System_Features.docx
System Features Specification (SFS)
2.0
Feb 2569
Project Files / MOD-*.md
CMS Data Dictionary
1.0
Jan 2569
Project Files / CMS_DataDictionary.docx
Database Addendum
1.0
Mar 2569
Project Files / INDEX_DatabaseAddendum.md
OIC Circular 2566
—
2566
OIC Thailand
CRAF Data Classification Standard
—
—
CRAF Official
Personal Data Protection Act B.E. 2562 (PDPA)
—
2562
Thai Parliament
ISO/IEC 27001:2022 — Information Security
—
2022
ISO
OWASP Top 10:2021
—
2021
OWASP Foundation



## 2. Introduction

### 2.1 Purpose
เอกสาร Software Requirements Specification (SRS) ฉบับนี้กำหนดข้อกำหนดซอฟต์แวร์อย่างละเอียดและครบถ้วนสำหรับ Core Management System (CMS) ตามมาตรฐาน IEEE 830-1998 และ ISO/IEC 29148:2018 โดยจัดทำย้อนหลัง (Retrospective SRS) จาก BRD, SDD, และ SFS ที่ได้ออกแบบและตกลงแล้ว เพื่อให้มีเอกสาร Requirements ที่ Traceable ได้ถึง Implementation จริงสำหรับกระบวนการ UAT, Audit, และ OIC Compliance


### 2.2 Scope
Core Management System (CMS) เป็น Enterprise Document & Knowledge Management System สำหรับบริษัทประกันภัย พัฒนาบนสภาพแวดล้อม On-Premise ด้วย .NET 4.5 WinForms และ SQL Server 2019 ครอบคลุม 23 Modules ตั้งแต่การจัดการเอกสาร, Workflow Automation, Knowledge Management, Project Management ไปจนถึง System Administration


### 2.3 Intended Audience
Audience
Purpose
Sections of Interest
Development Team
Implement ระบบตาม Specification
All
QA / Testing Team
สร้าง Test Cases จาก Requirements
§4, §5, §6
Project Manager
Track Requirements Coverage
§3, §8, §9
Business Analyst
Validate Requirements ความครบถ้วน
§4, §5
IT Manager / Architect
Review Technical Constraints
§3, §6
Compliance Officer
Verify PDPA & OIC Compliance
§7
OIC Auditor
ตรวจสอบ Regulatory Compliance
§7, §9
Project Sponsor
Executive Overview
§2, §3


### 2.4 Product Overview
ระบบ CMS ออกแบบให้เป็น Central Repository สำหรับ Documents, Knowledge, และ Business Processes ขององค์กร โดยมี Workflow Engine เป็น Core ที่เชื่อมทุก Module เข้าด้วยกัน ดังแผนภาพด้านล่าง

Module Group
Modules
วัตถุประสงค์
Foundation Layer
MOD-001 ถึง MOD-004
Folder Organization, File Management, Security
Collaboration Layer
MOD-005 ถึง MOD-007
File Sharing, Knowledge Base, Announcements
Process Engine
MOD-008 ถึง MOD-012
Workflow, Task, Todo, Notes, Work Instructions
Operations Layer
MOD-013 ถึง MOD-015
Project, Resource, Timeline Management
Reference Data Layer
MOD-016 ถึง MOD-019
Employee, Programs, IT Landscape, Data Dictionary
Administration Layer
MOD-020 ถึง MOD-023
User/Role, Audit, Config, Integration


### 2.5 Definitions, Acronyms, and Abbreviations
Term / Acronym
Definition
CMS
Core Management System — ระบบหลักที่พัฒนาในโครงการนี้
SRS
Software Requirements Specification
BRD
Business Requirements Document
SDD
System Design Document
SFS
System Features Specification
OIC
Office of Insurance Commission (สำนักงานคณะกรรมการกำกับและส่งเสริมการประกอบธุรกิจประกันภัย)
PDPA
Personal Data Protection Act B.E. 2562 (พระราชบัญญัติคุ้มครองข้อมูลส่วนบุคคล พ.ศ. 2562)
CRAF
Classification and Risk Assessment Framework — มาตรฐานการจัดหมวดหมู่ข้อมูลขององค์กร
AD / LDAP
Active Directory / Lightweight Directory Access Protocol — ระบบจัดการ Identity ขององค์กร
ADUserName
ชื่อผู้ใช้จาก Active Directory รูปแบบ domain\username — ใช้สำหรับ Audit Stamp ทุก Transaction
RBAC
Role-Based Access Control — การควบคุมสิทธิ์ตาม Role
1HC
High Confidential — ระดับความลับสูงสุด (สีแดง)
2SC
Semi Confidential — ระดับความลับรอง (สีส้ม)
3NC
Normal Confidential — ระดับความลับปกติ (สีน้ำเงิน)
4PC
Public Confidential — ระดับสาธารณะ (สีเขียว)
SLA
Service Level Agreement — ข้อตกลงระดับการให้บริการ คำนวณ Business Days
WBS
Work Breakdown Structure — โครงสร้างการแบ่งงานโครงการ
PII
Personally Identifiable Information — ข้อมูลส่วนบุคคล
DSR
Data Subject Request — คำร้องขอสิทธิ์ตาม PDPA
JWT
JSON Web Token — Token-based Authentication สำหรับ API
Thai_CI_AS
Thai Case Insensitive Accent Sensitive — SQL Server Collation สำหรับภาษาไทย
MOD
Module — หน่วยฟังก์ชันการทำงานหลักของระบบ (MOD-001 ถึง MOD-023)
FR
Functional Requirement
NFR
Non-Functional Requirement
UC
Use Case



## 3. System Overview & Architecture

### 3.1 System Context
ระบบ CMS ทำงานเป็น Central Hub ขององค์กร โดยเชื่อมต่อกับ External Systems ดังนี้:

External System
Direction
Protocol
Purpose
Active Directory (AD)
Bidirectional
LDAP/LDAPS
Authentication, User Sync, Group Membership
Email Server (SMTP)
Outbound
SMTP + TLS 1.2
Notifications, Alerts, Reminders
Core Insurance System
Bidirectional
REST API / File
Policy/Claims Data Reference
Network Drive
Bidirectional
SMB/CIFS
Physical File Storage — \\bui-filecenter\Core Management
Outlook Calendar
Read-only
Exchange API
Calendar Sync, Meeting Scheduling (Optional)


### 3.2 Technical Architecture Constraints

> 💡 หมายเหตุ: ข้อกำหนด Tech Stack เหล่านี้เป็น Hard Constraints ที่ไม่สามารถเปลี่ยนแปลงได้ — ทุก Design Decision ต้องสอดคล้องกับ Constraints นี้


Constraint
Specification
Rationale
Frontend Framework
.NET 4.5 WinForms (Visual Designer + Code-behind)
Legacy compatibility — บริษัทยังใช้ Windows Desktop Environment
Architecture Pattern
Modular MVC — Visual Designer แยกจาก Code-behind
Maintainability + Team skillset alignment
Backend / API
ASP.NET Web API (.NET 8) สำหรับ REST Integration
API layer สำหรับ Third-party Integration
Database
Microsoft SQL Server 2019
Existing infrastructure, Thai_CI_AS Collation
Data Access
Raw ADO.NET — ห้ามใช้ ORM (EF/Dapper)
Performance control, Explicit SQL for Audit
File Storage
Network Drive: \\bui-filecenter\Core Management
On-premise requirement, No cloud
Deployment
On-Premise เท่านั้น — ห้ามใช้ Cloud
Regulatory + Data sovereignty
Source Control
GitLab CE (On-Premise)
Existing infrastructure
Collation
Thai_CI_AS ทุก Text Column
รองรับภาษาไทยทุก Query


### 3.3 Solution Structure (7 Projects)
Project Name
Layer
Responsibility
Key Technologies
CMS.Presentation
UI Layer
WinForms + Visual Designer, Event Handlers
Windows Forms, GDI+
CMS.Controllers
Controller Layer
Business Flow, Request/Response Orchestration
C# .NET 4.5
CMS.Services
Business Logic
Rules, Validation, SLA Calc, Workflow Engine
C# Business Objects
CMS.DataAccess
Repository Layer
DB Access, Stored Procedures, File I/O
ADO.NET, SqlClient
CMS.Models
Domain Models
POCO Classes — No Business Logic
C# POCOs
CMS.Common
Shared Utilities
Constants, SessionContext, AuditHelper, Logger
C# Library
CMS.Tests
Unit Tests
70% Coverage — Critical Paths
MSTest / NUnit


### 3.4 Database Design Standards

#### 3.4.1 Naming Convention
Prefix
Table Type
Examples
m_
Master Tables
m_UserAccount, m_OrgUnit, m_Employee, m_Role
s_
Setup/Config Tables
s_WorkflowDefinition, s_RolePermission, s_FolderTemplate
t_
Transaction Tables
t_FileDocument, t_TaskItem, t_WorkflowInstance, t_Project
l_
Log Tables (Append-Only)
l_AuditLog, l_ShareAccessLog, l_ADSyncLog
r_
Reference/Lookup Tables
r_FileType, r_SystemCalendar, r_SkillMaster
c_
Configuration Tables
c_SystemConfig, c_EmailTemplate, c_NotificationTemplate


#### 3.4.2 Audit Stamp Standard

> ⚠ ข้อควรระวัง: กฎบังคับ: ทุก Table ที่มี CreatedBy/UpdatedBy ต้องใช้ ADUserName (Snapshot) ไม่ใช่ FK → UserID เพื่อให้ Audit Log ยังอ่านได้แม้ User ถูกลบ


Field
Data Type
Constraint
Description
CreatedBy
NVARCHAR(100)
NOT NULL
ADUserName Snapshot — เช่น corp\john.smith
CreatedDate
DATETIME
NOT NULL DEFAULT GETDATE()
บันทึกอัตโนมัติ
UpdatedBy
NVARCHAR(100)
NULL
ADUserName ผู้แก้ไขล่าสุด
UpdatedDate
DATETIME
NULL
วันที่แก้ไขล่าสุด


#### 3.4.3 Audit Log Design (l_AuditLog)
Design Decision
Specification
Reason
Append-Only
INSERT + SELECT เท่านั้น ไม่มี UPDATE/DELETE Permission
OIC Circular 2566: Immutable Audit Trail
No Foreign Keys
ไม่มี FK ไปยัง Table อื่น
Log ยังคงอยู่แม้ Master Record ถูกลบ
Retention Period
7 ปี (OIC กำหนด ≥ 5 ปี)
Safety Margin + Regulatory Buffer
ADUserName Snapshot
เก็บ ADUserName ณ เวลาทำรายการ
ไม่ขึ้นอยู่กับ Live FK
Old/New Value
JSON Format ใน OldValue/NewValue
Data Change Forensics



### 3.5 User Roles & Access Overview
Role
Level
Description
Key Capabilities
Officer
1
พนักงานทั่วไป
View 3NC/4PC, Upload, Create Task/Todo, Comment
Sr. Officer
2
พนักงานอาวุโส
ทุกอย่างของ Officer + Delete Own, ส่ง Workflow, View 2SC
Manager
3
ผู้จัดการ / หัวหน้าแผนก
ทุกอย่างของ Sr.Officer + Approve Workflow, External Share, View 1HC (Assigned)
Admin
4
ผู้ดูแลระบบ
Full Control ทุก Module + จัดการ User/Role/Config



## 4. Functional Requirements — Foundation Layer
Foundation Layer (MOD-001 ถึง MOD-004) เป็นแกนหลักของระบบ CMS ทุก Module อื่นอ้างอิง Folder Structure และ Permission Model ที่กำหนดใน Layer นี้ การออกแบบต้องมั่นคงและเปลี่ยนแปลงได้น้อยที่สุดหลัง Go-Live


---

### MOD-001
BR-001
Folder Organization Structure
Form: OrgStructureForm, OrgTreeView
Priority
Critical
Status
✅ Defined


#### 4.1.1 Overview
จัดการโครงสร้างองค์กร 3 ระดับ (Sub-Committee → Division → Department) พร้อม 2 มุมมอง: Organization View และ Confidentiality View ตาม CRAF Standards กำหนดให้ทุก Department มี 4 Conf-Root Folder (1HC/2SC/3NC/4PC) โดยอัตโนมัติ


#### 4.1.2 Functional Requirements
FR ID
Requirement
Priority
Acceptance Criteria
FR-001-01
รองรับ 3 ระดับ: Sub-Committee → Division → Department พร้อม Parent-Child Relationship
Must
สร้าง Hierarchy ครบ 3 ระดับและแสดงได้ถูกต้อง
FR-001-02
Admin สร้าง/แก้ไข/ลบ (Soft-Delete) Folder Unit ได้ทุกระดับ
Must
Unit ที่ลบยังคงอยู่ใน DB พร้อม IsDeleted=1
FR-001-03
Organization View: โครงสร้างแนวดิ่งตามลำดับชั้น
Must
TreeView แสดง Hierarchy ครบถ้วน
FR-001-04
Confidentiality View: จัดกลุ่มตาม 1HC/2SC/3NC/4PC
Must
Switch View ไม่ต้อง Reload หน้าจอ
FR-001-05
ทุก Organizarion Auto-สร้าง 4 Conf-Root Folder ทันทีที่สร้าง
Must
4 Folder ถูกสร้างอัตโนมัติ Lock ไม่ให้แก้ไข/ลบ
FR-001-06
ค้นหาภายใน TreeView ด้วย Keyword
Should
ผลลัพธ์แสดงภายใน 2 วินาที
FR-001-07
รองรับชื่อภาษาไทยและอังกฤษ (Collation Thai_CI_AS)
Must
ค้นหาภาษาไทยได้ถูกต้อง


#### 4.1.3 Business Rules
- BR-001-01: Department ต้องอยู่ใน Division เดียวเท่านั้น
- BR-001-02: Division ต้องอยู่ใต้ Sub-Committee อย่างน้อย 1
- BR-001-03: ชื่อ Folder Unit ต้อง Unique ภายใน Parent เดียวกัน
- BR-001-04: Deleted Units = Soft-Delete (IsDeleted=1) — ห้ามลบ Physical
- BR-001-05: เฉพาะ Admin เท่านั้นที่แก้ไขโครงสร้องได้
- BR-001-06: Conf-Root Folders (1HC/2SC/3NC/4PC) ถูก Lock — ห้ามแก้ไขชื่อ/ลบ/ตั้ง Level
- BR-001-07: โหนด Sub-Committee/Division/Department ไม่มี User-settable Confidentiality Level


---

### MOD-002
BR-002
Folder Structure Management
Form: FolderManagerForm, FolderTreePanel
Priority
Critical
Status
✅ Defined


#### 4.2.1 Overview
จัดการ Sub-Folder ภายใน Conf-Root Folder ที่สร้างโดย MOD-001 โดย Sub-Folder ต้อง Inherit Confidentiality Level จาก Parent เสมอ ไม่สามารถลด Level ได้ รองรับ Depth สูงสุด 10 ระดับ และ Recycle Bin 30 วัน


#### 4.2.2 Functional Requirements
FR ID
Requirement
Priority
Acceptance Criteria
FR-002-01
สร้าง Sub-Folder ภายใน Conf-Root หรือ Sub-Folder อื่น (Max Depth: 10)
Must
สร้าง Folder Level 10 ได้ — Level 11 บล็อก
FR-002-02
Folder Operations: Create, Rename, Move, Delete, Archive
Must
ทุก Operation ทำงานภายใน 2 วินาที
FR-002-03
Recycle Bin: เก็บ Deleted Folders 30 วัน — กู้คืนได้
Must
กู้คืน Folder + Files ทั้งหมดสำเร็จ
FR-002-04
Sub-Folder Inherit Confidentiality Level จาก Parent
Must
System Block หากพยายาม Set Level ต่ำกว่า Parent
FR-002-05
Folder Templates — สร้างโครงสร้างมาตรฐาน
Should
Apply Template สร้าง Folder Structure ถูกต้อง
FR-002-06
แสดง Folder Statistics (จำนวนไฟล์, ขนาด, วันแก้ไขล่าสุด)
Should
Statistics ถูกต้องและ Update Real-time
FR-002-07
Color-coded Icons ตาม Confidentiality Level (Red/Orange/Blue/Green)
Must
Icon สีถูกต้องตาม Level ที่กำหนด


> 📋 Business Rule: Folder ที่มีไฟล์ต้องได้รับ Admin Approval ก่อนลบ | Empty Folder ลบได้โดย Owner หรือ Admin | ชื่อ Folder ห้ามมี: \ / : * ? " < > |



---

### MOD-003
BR-003
File Upload and Storage
Form: FileUploadForm, FileListView, FilePreviewPanel
Priority
Critical
Status
✅ Defined


#### 4.3.1 Overview
จัดการ File Upload, Storage, และ Retrieval โดยแยก Physical File (Network Drive) กับ Metadata (SQL Server) เพื่อ Scalability ทุกไฟล์ต้องผ่าน Virus Scan ก่อน Storage


#### 4.3.2 File Storage Path Convention
Component
Pattern/Value
Network Root
\\bui-filecenter\Core Management (อ่านจาก c_SystemConfig.NetworkDriveRoot)
Credentials
pmc1/Bui@12345 (Read-Write-Modify) | pmc2/Bui@12345 (Read Only)
Path Structure
{OrgUnitCode}\{ConfCode}\{FolderID_8digits}\{FileID_10digits}_{OriginalName}
Example
DEPT-001\3NC\00000042\0000000123_รายงานประจำปี.pdf
Rationale
ใช้ FolderID/FileID แทนชื่อเพื่อป้องกัน Special Characters และเพิ่ม Security


#### 4.3.3 Functional Requirements
FR ID
Requirement
Priority
Acceptance Criteria
FR-003-01
Upload หลายไฟล์พร้อมกัน (สูงสุด 10 ไฟล์/ครั้ง) + Progress Bar
Must
Progress แสดง % ถูกต้อง Upload 10 ไฟล์พร้อมกันสำเร็จ
FR-003-02
Virus Scan ทุกไฟล์ก่อน Storage — บล็อก Infected File
Must
Infected File ถูกบล็อก + Log VIRUS_DETECTED
FR-003-03
File Type Whitelist: PDF/DOC/DOCX/XLS/XLSX/PPT/PPTX/JPG/PNG/GIF/ZIP
Must
ไฟล์นอก Whitelist ถูกปฏิเสธพร้อม Error Message
FR-003-04
Max File Size: 100 MB ต่อไฟล์
Must
ไฟล์ > 100MB ถูกบล็อก
FR-003-05
Checksum (SHA-256) สำหรับ Integrity Verification + Duplicate Detection
Should
Checksum ตรวจจับ Duplicate ได้ถูกต้อง
FR-003-06
ListView แสดง Icon, ชื่อ, ขนาด, วันที่, ผู้อัปโหลด, Actions
Must
Sort/Filter ทุก Column ทำงานได้
FR-003-07
Preview: PDF, JPG, PNG (Inline Preview Panel)
Should
Preview แสดงใน Panel ไม่ต้องเปิด App อื่น
FR-003-08
Department Quota: Default 40 GB (Admin ตั้งค่าได้)
Should
Exceed Quota แจ้ง Manager + Block Upload
FR-003-09
Auto-Archive ไฟล์อายุ > 7 ปี (Configurable)
Should
Background Job Archive ไฟล์ตามกำหนด


#### 4.3.4 Error Handling
Error Code
Condition
System Response
FILE-001
ไฟล์ > 100 MB
บล็อก Upload + แสดง Error พร้อมขนาดไฟล์ที่เกิน
FILE-002
ประเภทไฟล์ไม่อนุญาต
แสดง 'ประเภทไฟล์ .xxx ไม่ได้รับอนุญาต' พร้อม Whitelist
FILE-003
Virus Detected
บล็อก + แจ้ง Admin ทันที + Log VIRUS_DETECTED
FILE-004
Network Drive Unavailable
Retry 3 ครั้ง (30 วินาที) → Error 'ไม่สามารถเชื่อมต่อ Storage'
FILE-005
Department Quota เต็ม
แจ้ง Manager + Block Upload จนกว่าจะได้ Approval


---

### MOD-004
BR-004
Folder Settings and Security
Form: FolderPermissionForm, FolderSettingsForm
Priority
Critical
Status
✅ Defined


#### 4.4.1 Overview
ระบบ Permission Management สำหรับ Folder ใช้หลัก RBAC + Folder-level Permission โดย Explicit Deny Override Allow และ Permission Inheritance จาก Parent → Child เปลี่ยนแปลงได้โดย Admin


#### 4.4.2 Confidentiality Level Specifications
Level
Color Code
Default Access
External Share
Audit Requirement
1HC — High Confidential
แดง #FF0000
Executive Level Only
Block — ไม่อนุญาตในทุกกรณี
Log ทุก View/Download/Access
2SC — Semi Confidential
ส้ม #FF8C00
Dept Head + Above
ต้องได้ Manager Approval
Log ทุก Action
3NC — Normal Confidential
น้ำเงิน #0070C0
Staff ใน Department
Standard Approval Process
Log Write/Delete Actions
4PC — Public Confidential
เขียว #00B050
พนักงานทุกคน
Allowed (Read-only default)
Log Delete Actions


#### 4.4.3 Permission Types
Permission
Bit Value
Capability
Read (R)
0b00001
ดูไฟล์และโครงสร้าง Folder — ไม่สามารถ Download ได้
Write (W)
0b00010
Upload ไฟล์ใหม่ + แก้ไข Metadata
Delete (D)
0b00100
ย้ายไฟล์ไป Recycle Bin
Modify (M)
0b01000
เปลี่ยน Folder Settings และ Properties
Full Control (F)
0b11111
ทุกอย่าง + Grant Access ให้คนอื่น


> 📋 Business Rule: Explicit Deny Override Allow | Permission Inheritance: Child Inherits Parent | Break Inheritance ได้โดย Admin เท่านั้น | ทุก Permission Change ต้อง Log ใน l_AuditLog | 1HC ห้าม External Share ในทุกกรณี — Block ที่ System Level




## 5. Functional Requirements — Collaboration & Process Engine

---

### MOD-005
BR-005
File Sharing System
Form: ShareFileDialog, ShareLinkManagerForm, SharedWithMePanel
Priority
High
Status
✅ Defined


#### 5.1.1 Functional Requirements
FR ID
Requirement
Priority
Acceptance Criteria
FR-005-01
Internal Share: แชร์ให้ User/Dept/Role/Group ภายในองค์กร
Must
ผู้รับได้รับ Notification และเข้าถึงไฟล์ได้
FR-005-02
ตั้ง Permission: View-Only หรือ Download
Must
View-Only ไม่แสดงปุ่ม Download
FR-005-03
ตั้ง Expiry Date (สูงสุด 90 วัน)
Must
Share หมดอายุ Auto-Disable ทันที
FR-005-04
Share Link: Token-based (GUID) พร้อม Password Optional
Must
Link ทำงานได้ก่อน Expiry / ไม่ทำงานหลัง Expiry
FR-005-05
Max Downloads Limit ต่อ Link
Should
หลังครบ Limit Link Auto-Disable
FR-005-06
Revoke Share/Link ทันที
Must
Access ถูก Block ทันทีหลัง Revoke
FR-005-07
Share Access Log: ใคร, เมื่อไร, Download ไหม
Should
Log ครบถ้วน Export ได้
FR-005-08
Notification: แจ้งผู้รับเมื่อแชร์ + แจ้งก่อน Expire 3 วัน
Should
Email ส่งถึงผู้รับภายใน 5 นาที


> 📋 Business Rule: 1HC Files ห้าม External Share — Block ที่ System Level ไม่ใช่แค่ UI | External Share ต้องได้ Manager Approval | Shared Content Inherit Source Folder Permissions — ไม่สามารถ Share สิทธิ์สูงกว่าที่ตัวเองมี



---

### MOD-006
BR-006
Knowledge Base System
Form: KnowledgeBaseForm, ArticleEditorForm, ArticleViewer
Priority
Critical
Status
✅ Defined


#### 5.2.1 Content Types
Type
Code
Description
Review Required
Recertification
Wiki Page
WIKI
Rich Text Knowledge Page — Images, Links, Tables
Yes
Annual Recommended
SOP
SOP
Standard Operating Procedure — Numbered Steps + Expected Results
Yes
Annual — Mandatory
Template
TMPL
Fill-in-the-blank Document Template
Yes
Annual
Best Practice
BP
Lessons Learned, Tips & Tricks, Case Studies
Yes
Annual Recommended
FAQ
FAQ
Q&A Format — Helpful/Not Helpful Voting
Yes
Annual Recommended


#### 5.2.2 Functional Requirements
FR ID
Requirement
Priority
Acceptance Criteria
FR-006-01
Full-text Search ใน Article Content + Title + Tags
Must
ผลลัพธ์ภายใน 2 วินาที — Relevance Ranked
FR-006-02
Hierarchical Category + Tag-based Organization
Must
Multi-category Assignment + Filter ได้
FR-006-03
Rich Text Editor (Bold/Italic/Table/Image/Link/Code Block)
Must
Content แสดงผลถูกต้องทุก Format
FR-006-04
Version Control + Change History + Diff View
Must
ดู Version เก่าได้ทุก Version
FR-006-05
Review Workflow ก่อน Publish (Draft → Review → Published)
Must
Draft มองเห็นเฉพาะ Author + Reviewer
FR-006-06
Content Expiration + Auto-Unpublish + Author Notification
Must
Expired Article Auto-Draft + Email แจ้ง Author
FR-006-07
Link Article กับ OrgUnit, Workflow, Task, Program
Should
Link Reference ปรากฏใน Article
FR-006-08
Annual Review Reminder (12 เดือน Auto-Reminder)
Must
Email แจ้ง Author 30 วันก่อนครบ 1 ปี
FR-006-09
Import จาก Excel Template (Bulk Import)
Should
Import 100 Articles — Validation Error Report
FR-006-10
Export เป็น PDF / Word
Should
Format ถูกต้อง Header/Footer ครบ


> 📋 Business Rule: Published Article ต้องผ่าน Reviewer Approval | Obsolete → Archive เท่านั้น ห้ามลบ | SOP ต้อง Annual Recertification | Draft Articles มองเห็นเฉพาะ Author + Reviewer



---

### MOD-007
BR-007
Announcement System
Form: AnnouncementForm, AnnouncementListPanel, AnnouncementDashboard
Priority
Medium
Status
✅ Defined


#### 5.3.1 Functional Requirements
FR ID
Requirement
Priority
Acceptance Criteria
FR-007-01
Target Audience: All / Dept / Division / SubCommittee / Role / Individual
Must
ผู้ใช้เห็นเฉพาะประกาศที่ Target ถึงตัวเอง
FR-007-02
Priority Levels: Critical(Red) / High(Orange) / Medium(Blue) / Low(Green)
Must
สีแสดงถูกต้องตาม Priority
FR-007-03
Schedule Publication Date + Expiry Date (Auto-Archive)
Must
ประกาศ Publish/Archive ตรงเวลา
FR-007-04
Critical Priority → Pop-up Alert บน Login (สูงสุด 3 Pop-ups/Session)
Must
Pop-up แสดงครั้งแรกที่ Login
FR-007-05
Read Receipt: Track View + Acknowledge per User
Must
Admin ดู Read Rate % ได้
FR-007-06
RequireAck=true → บังคับกด 'รับทราบ' ก่อนปิด — Reminder ทุก Login
Must
Block ปิดได้จนกว่าจะ Acknowledge
FR-007-07
Email Notification ทันทีหรือ Daily Digest (User เลือกได้)
Should
Email ส่งภายใน 5 นาที


> 📋 Business Rule: Announcements ที่ไม่ตั้ง Expiry → Auto-Expire หลัง 30 วัน | History เก็บ 1 ปี | Popup สูงสุด 3 ครั้ง/Login Session



---

### MOD-008
BR-008
Workflow Management
Form: WorkflowDesignerForm, WorkflowInstanceForm, WorkflowMonitorForm
Priority
Critical
Status
✅ Defined


#### 5.4.1 Overview
Workflow Engine คือ Core ของระบบ CMS ตามการวิเคราะห์ MindMap Architecture ทุก Module เชื่อมผ่าน Workflow Engine โดย Workflow Instance จะ Freeze Definition ณ เวลาที่เริ่ม เพื่อป้องกัน Inconsistency เมื่อ Admin แก้ไข Definition ระหว่าง Instance ที่กำลังทำงาน


#### 5.4.2 Step Types
Step Type
Code
Description
Completion Rule
Start Node
START
Trigger Point — Manual หรือ Event-based
Auto-proceed
Task Node
TASK
Action ที่ Assignee ต้องดำเนินการ
Completed / Rejected
Decision Node
DECISION
Conditional Branch (If-Then-Else ตาม Field Value)
Route ตาม Condition
Parallel Gateway
PARALLEL
หลายคน/หลาย Path ทำงานพร้อมกัน
AllMustComplete หรือ MajorityComplete
Merge Gateway
MERGE
รวม Parallel Paths กลับเป็น Path เดียว
Auto-proceed เมื่อ Condition ครบ
End Node
END
สิ้นสุด Workflow
Auto-complete Instance


#### 5.4.3 Functional Requirements
FR ID
Requirement
Priority
Acceptance Criteria
FR-008-01
Visual Workflow Designer: Drag-Drop Steps + Routing + Undo/Redo
Must
Non-technical User สร้าง Workflow ง่ายได้
FR-008-02
Pre-built Insurance Templates: Policy Approval, Claims Processing, Underwriting
Must
Templates Apply ได้และ Customizable
FR-008-03
SLA Timer per Step (Business Days — ไม่รวม Weekend + Holidays)
Must
SLA คำนวณถูกต้อง ตรวจสอบ r_SystemCalendar
FR-008-04
Escalation เมื่อเกิน SLA → แจ้ง Level ถัดไปอัตโนมัติ
Must
Escalation เกิดภายใน 5 นาทีหลัง SLA Breach
FR-008-05
Workflow Instance Freeze Definition at Start Time
Must
แก้ไข Definition ไม่กระทบ Running Instance
FR-008-06
Delegation Support (Approve แทน) พร้อม Log 'Delegated by X'
Should
Delegation ถูก Log ครบถ้วน
FR-008-07
Workflow Dashboard: Pending / In Progress / Completed / Overdue
Must
Dashboard Load ภายใน 3 วินาที
FR-008-08
Workflow ทุก Step Action ต้อง Audit Log
Must
ทุก Approve/Reject/Forward Log ใน l_AuditLog


> 📋 Business Rule: SLA คำนวณจาก Business Days เท่านั้น ผ่าน r_SystemCalendar | Reject ต้อง Require Comment | Changes to Active Workflow Apply ต่อ New Instance เท่านั้น | Completed Workflow = Read-Only



---

### MOD-009
BR-009
Task Management
Form: TaskBoardForm(Kanban), TaskListForm(Email-style), TaskDetailForm
Priority
Critical
Status
✅ Defined


#### 5.5.1 Functional Requirements
FR ID
Requirement
Priority
Acceptance Criteria
FR-009-01
Email-style View: ListView + Sort/Filter + Quick Actions
Must
Column Sort/Filter ทำงานถูกต้อง
FR-009-02
Kanban Board: Configurable Columns per User (JSON in s_UserPreference)
Must
Drag-Drop ระหว่าง Columns สำเร็จ
FR-009-03
Switch View: Email ↔ Kanban — Remember User Preference
Must
View Preference บันทึกข้าม Session
FR-009-04
Subtasks: สูงสุด 3 ระดับ (Task→Subtask→Sub-subtask)
Must
Level 4 ถูกบล็อก
FR-009-05
Task Relationships: Link to File, Workflow, Document, Knowledge, Program
Must
Link Reference Access ได้จาก Task
FR-009-06
SLA Warning: Yellow ที่ 80% เวลา / Red เมื่อ Overdue
Must
Status Auto-เปลี่ยนเป็น OVERDUE + แจ้ง Assignee + Manager
FR-009-07
Notifications: Email + In-App Bell + Popup Reminder (1 Day / 1 Hour)
Must
Notification ส่งตามเวลาที่กำหนด
FR-009-08
High/Critical Task: Require Reason ก่อน Extend Due Date
Must
Extension บันทึก Reason + Log
FR-009-09
Task Activity Log: ทุก Change พร้อม Timestamp + User
Must
Activity Log ครบถ้วนทุก Change
FR-009-10
Deleted Tasks: Recoverable ภายใน 30 วัน
Must
Restore Task + ทุก Subtask สำเร็จ


---

### MOD-010
BR-010
Todo List Management
Form: TodoListPanel (Embedded in Dashboard)
Priority
Medium
Status
✅ Defined


#### 5.6.1 Functional Requirements
FR ID
Requirement
Priority
FR-010-01
Personal Todo List (Private) + Team Todo + Project Todo
Must
FR-010-02
Quick Add: สร้าง Todo Item ด้วย Title เดียว
Must
FR-010-03
Mark Complete/Incomplete + Reorder (Drag-Drop)
Must
FR-010-04
Recurring Todo (Daily/Weekly/Monthly)
Should
FR-010-05
Convert Todo → Formal Task (MOD-009)
Should
FR-010-06
Progress Statistics: Completion Rate, Streak Days, Weekly Chart
Should
FR-010-07
Max 100 Active Todos ต่อ User
Must


---

### MOD-011
BR-011
Notes and Comments System
Form: NotePanel, CommentThreadPanel (Embedded)
Priority
Medium
Status
✅ Defined


#### 5.7.1 Functional Requirements
FR ID
Requirement
Priority
FR-011-01
Comment บน Entity ต่างๆ: File, Task, Workflow, Project, Article
Must
FR-011-02
Thread Reply สูงสุด 3 ระดับ + Collapse/Expand Threads
Must
FR-011-03
@Mention Users → Auto-complete + Notification (จำกัด 10 Users/Comment)
Should
FR-011-04
แนบไฟล์ในความคิดเห็น (สูงสุด 10MB ต่อ Comment)
Should
FR-011-05
Edit Comment ได้ภายใน 24 ชั่วโมง — แสดง 'edited' Indicator
Must
FR-011-06
Soft-Delete: แสดง '[ลบแล้ว]' เพื่อรักษา Thread Continuity
Must
FR-011-07
Export Notes เป็น PDF/Word พร้อม Metadata
Should


---

### MOD-012
BR-012
Work Instructions
Form: WorkInstructionForm, WIListView, WIExecutionPanel
Priority
High
Status
✅ Defined


#### 5.8.1 Functional Requirements
FR ID
Requirement
Priority
FR-012-01
สร้าง WI พร้อม Numbered Steps + Sub-steps + Conditional Steps
Must
FR-012-02
Visual Aids ต่อ Step: Screenshots, Diagrams, Annotated Images
Should
FR-012-03
Version Control (Major/Minor) — Minor = ไม่ต้อง Re-Approve
Must
FR-012-04
Approval Workflow ก่อน Activate
Must
FR-012-05
Check-off Steps ขณะปฏิบัติงาน + Log ทุก Check-off (ADUserName + Timestamp)
Must
FR-012-06
IsMandatory=true → ต้อง Complete WI ก่อน Close Task ได้
Must
FR-012-07
Export เป็น PDF + Printable Format
Should



## 6. Functional Requirements — Operations, Reference & Administration

---

### MOD-013
BR-013
Project Management
Form: ProjectForm, GanttChartView, ProjectDashboard
Priority
High
Status
✅ Defined


#### 6.1.1 Functional Requirements
FR ID
Requirement
Priority
Acceptance Criteria
FR-013-01
สร้างโครงการ: Code, ชื่อ TH/EN, Type, Start/End, Budget, Priority
Must
Project Code Unique — Validation ทันที
FR-013-02
Milestone Management: ชื่อ, Target Date, Completion Criteria, Dependencies
Must
Milestone Overdue แสดง Red Alert บน Gantt
FR-013-03
Task Breakdown (WBS): Phase → Task → Sub-task + Dependencies
Must
Task Dependency Enforce — Block Start ถ้า Predecessor ไม่ Done
FR-013-04
Gantt Chart: Timeline, Dependencies (Arrows), Critical Path, Milestones, Zoom
Must
Critical Path Highlight ถูกต้อง
FR-013-05
Progress % คำนวณอัตโนมัติจาก Task Done/Total (ไม่ Input เอง)
Must
Progress Update Real-time เมื่อ Task Status เปลี่ยน
FR-013-06
Project Dashboard: Health Status, Budget Utilization, Open Issues
Must
Dashboard Load ภายใน 3 วินาที
FR-013-07
Risk Register ต่อโครงการ (Probability, Impact, Mitigation)
Should
Risk Register Export เป็น PDF
FR-013-08
Change Request Management
Should
Change Log Traceable


> 📋 Business Rule: Projects ต้องมี PM กำหนดอย่างน้อย 1 คน | ปิดโครงการได้เมื่อ Task ทั้งหมด Done/Cancelled | OnHold ต้อง Require Reason | Weekly Status Update สำหรับ Active Project



---

### MOD-014
BR-014
Resource Management
Form: ResourceAllocationForm, UtilizationDashboard, ResourceCalendar
Priority
High
Status
✅ Defined


#### 6.2.1 Functional Requirements
FR ID
Requirement
Priority
FR-014-01
Resource Definition: Human/Equipment/Facility + Availability + Skills
Must
FR-014-02
Allocation to Project/Task + % of Time + Start/End Date
Must
FR-014-03
Utilization Tracking: Current Workload %, Available Capacity
Must
FR-014-04
Over-allocation Alert (>100%) + Under-utilization Alert (<50%)
Must
FR-014-05
Team Workload Heatmap: Red=Over / Green=Under
Should
FR-014-06
Skills-based Resource Matching + Suggestion
Should
FR-014-07
Resource Calendar: Allocations Over Time + Leave/Holidays
Should


> 📋 Business Rule: Resource Capacity = 8 ชม./วัน (Default, ตั้งค่าได้) | Over-allocation ต้อง Manager Approval | Leave ลด Available Capacity อัตโนมัติ



---

### MOD-015
BR-015
Timeline and Deadline Tracking
Form: TimelineView, KPIDashboard, CalendarView
Priority
Medium
Status
✅ Defined


#### 6.3.1 Functional Requirements
FR ID
Requirement
Priority
FR-015-01
Calendar Views: Day / Week / Month / Year + Outlook Sync (Optional)
Must
FR-015-02
SLA & Deadline Countdown Timer + Visual Alerts (Yellow/Orange/Red)
Must
FR-015-03
KPI Dashboard: Task Metrics, SLA Metrics, Resource Metrics, Project Metrics
Must
FR-015-04
Escalation Hierarchy: Responsible → Manager → Higher Level
Must
FR-015-05
Dashboard Customizable Widgets + Export PDF/Excel + Scheduled Reports
Should
FR-015-06
Dashboard Auto-refresh ทุก 15 นาที
Must


---

### MOD-016
BR-016
Employee Management System
Form: EmployeeForm, SkillsMatrixView, OrgChartView
Priority
Medium
Status
✅ Defined


#### 6.4.1 PDPA-Sensitive Fields
Field
CRAF Level
Access Restriction
Consent Required
NationalID
1HC — High Confidential
HR Manager + Admin เท่านั้น + Log ทุก View
Explicit Written Consent
Photo (Biometric-like)
2SC — Semi Confidential
HR + Manager ของ Employee
Required
BirthDate
2SC — Semi Confidential
HR + Manager
Required
FullName TH/EN
3NC — Normal Confidential
ทุก Role (ที่มีสิทธิ์)
Required
PersonalEmail, Phone
3NC — Normal Confidential
HR + Manager
Required
PositionName, Dept
4PC — Public Confidential
ทุก Role
Implied (สัญญาจ้าง)


#### 6.4.2 Functional Requirements
FR ID
Requirement
Priority
FR-016-01
CRUD Employee Profile พร้อม PDPA Consent Check ก่อนเข้าถึง Sensitive Fields
Must
FR-016-02
Skills Matrix: Skill + Level (1=Basic/2=Intermediate/3=Advanced/4=Expert)
Must
FR-016-03
Certification Management: ชื่อ, Expiry, Certificate File Attachment
Should
FR-016-04
Training History per Employee
Should
FR-016-05
Job Description: ตำแหน่ง, ความรับผิดชอบ, Skills Required
Should
FR-016-06
Attendance Status: Present/Remote/OnLeave/BusinessTrip/Absent
Should
FR-016-07
Skills Search: ค้นหา Employee ตาม Skill + Proficiency Level
Should
FR-016-08
Org Chart View ตาม OrgUnit Hierarchy
Should
FR-016-09
Certification Expiration Alert 30 วันก่อน Expire
Should


> 📋 Business Rule: Employee Photo เก็บบน Network Drive — ไม่เก็บ BLOB ใน Database | NationalID เก็บแบบ Encrypted (AES-256) ที่ Application Layer | Resigned Employee = Soft-Inactive ไม่ลบ | Skill Change ต้อง Manager Validation



---

### MOD-017
BR-017
Programs List Management
Form: ProgramListForm, ProgramDetailForm
Priority
Medium
Status
✅ Defined

Catalog ของ Applications/Systems ที่บริษัทใช้งาน เป็น Master Reference สำหรับ MOD-018 (IT Landscape) และ MOD-019 (Data Dictionary) รองรับ Status Lifecycle: Active → Maintenance → Deprecated → Retired พร้อม Relationship Mapping กับ Folders, Tasks, Documents

FR ID
Requirement
Priority
FR-017-01
Program Registry: ชื่อ, Code, Version, Vendor, Platform, Tech Stack, Status
Must
FR-017-02
Ownership: Business Owner, Technical Owner, Support Team, Vendor Contact
Must
FR-017-03
Relationship Mapping: Link กับ Folders, Tasks, Documents, Users
Must
FR-017-04
License Information + Cost + Maintenance Windows
Should


---

### MOD-018
BR-018
IT Landscape Mapping
Form: ITLandscapeForm, SystemMapView, ComponentForm
Priority
Medium
Status
✅ Defined

แผนผัง IT Architecture สำหรับ OIC Audit ตาม OIC Circular 2566 ที่กำหนดให้บริษัทประกันมีแผนผัง IT ที่อัปเดตและ Traceable รองรับ Component Types: Server, Database, Application, Network, External Service

FR ID
Requirement
Priority
FR-018-01
IT Component Registry: ชื่อ, Type, Version, Status, Location
Must
FR-018-02
Integration/Connection Mapping ระหว่าง Components + Data Flow Direction
Must
FR-018-03
Visual Map View (Diagram) พร้อม Zoom/Pan
Must
FR-018-04
Link กับ Programs List (MOD-017) — ไม่ Duplicate ข้อมูล
Must
FR-018-05
Change History Log ทุกการเปลี่ยนแปลง
Must
FR-018-06
Export เป็น PDF/Image สำหรับ OIC Report
Must


> 📋 Business Rule: IT Landscape เป็น Classified Information → Access เฉพาะ IT Manager + Admin | Decommissioned Component = Archive ไม่ลบ (Historical Reference สำหรับ OIC Audit)



---

### MOD-019
BR-019
Data Dictionary System
Form: DataDictionaryForm, EntityBrowser, FieldDetailForm
Priority
Medium
Status
✅ Defined

Data Dictionary สำหรับ CRAF Compliance ตามข้อกำหนด CRAF ที่กำหนดให้ Document ทุก Data Element และ Classification Level OIC ใช้ Data Dictionary ประเมิน Data Governance

FR ID
Requirement
Priority
Acceptance Criteria
FR-019-01
Data Entity: Code, ชื่อ TH/EN, Type (Table/View/API/File), LinkedProgram
Must
Entity Code Unique — Search ได้ภายใน 2 วินาที
FR-019-02
Field Definition: DataType, MaxLength, Nullable, PK, FK
Must
Field เพิ่ม/แก้ไขได้ทุก Attribute
FR-019-03
CRAF Classification ต่อ Field — บังคับทุก Field (ห้าม Null)
Must
Save ไม่ได้หากไม่ได้ Set CRAF Level
FR-019-04
PII Flag + Consent Requirement ต่อ Field
Must
PII Fields แสดง Badge ใน UI
FR-019-05
Version Control + Change History
Should
ดู Version เก่าได้
FR-019-06
Export Data Dictionary เป็น Excel/PDF สำหรับ OIC Reporting
Must
Export ครบถ้วนทุก Field
FR-019-07
Import จาก Excel Template + Validation Report
Should
Import 500 Fields — Error Report ชัดเจน


---

### MOD-020
BR-020
User and Role Management
Form: UserListForm, UserDetailForm, RoleManagerForm, PermissionMatrixForm
Priority
Critical
Status
✅ Defined


#### 6.8.1 Functional Requirements
FR ID
Requirement
Priority
Acceptance Criteria
FR-020-01
AD Sync: Auto-provision User จาก AD + Daily Scheduled Sync (00:00)
Must
Sync Log ครบถ้วน — New/Updated/Deactivated Count
FR-020-02
Assign Role: Officer / Sr. Officer / Manager / Admin
Must
Role Change มีผลทันที + Log ROLE_CHANGE
FR-020-03
Permission Matrix: Role × Module × Action (View/Create/Edit/Delete/Approve/Export)
Must
Permission เปลี่ยนมีผลภายใน Login ถัดไป
FR-020-04
Temporary Role Assignment: Effective From/To Date
Should
Role Auto-Expire ตาม EffectiveTo
FR-020-05
User Group Management (Custom Groups)
Should
Group Permission Propagate ถูกต้อง
FR-020-06
Session Management: View Active Sessions + Force Logout
Must
Force Logout มีผลภายใน 1 นาที
FR-020-07
Account Lockout Management: Unlock + View Lockout History
Must
Unlock ทำงานทันที
FR-020-08
PDPA Consent Management: View/Reset ต่อ User (Emergency Only + Log Reason)
Must
Reset Log บันทึก Reason บังคับ


> 📋 Business Rule: ทุก User ต้องมี AD Account — ห้าม Local Account | Admin ต้องมีอย่างน้อย 2 คน | Session Timeout 30 นาที | Lockout ผิด 5 ครั้ง → Lock 30 นาที | Concurrent Sessions สูงสุด 3



---

### MOD-021
BR-021
Audit and Compliance
Form: AuditLogViewerForm, ComplianceReportForm, DSRManagementForm
Priority
Critical
Status
✅ Defined


#### 6.9.1 Auditable Operations Matrix
Module Group
Operations Logged
Authentication
LOGIN_SUCCESS, LOGIN_FAILED (+ Count), LOGOUT, SESSION_TIMEOUT, ACCOUNT_LOCKED
Document/File
FILE_UPLOAD, FILE_DOWNLOAD, FILE_DELETE, FILE_RESTORE, FILE_MOVE, FILE_SHARE
Folder/Security
FOLDER_CREATE, FOLDER_RENAME, FOLDER_DELETE, PERMISSION_CHANGE, INHERITANCE_BREAK
Sharing
SHARE_CREATE, SHARE_REVOKE, SHARE_ACCESS, LINK_GENERATE, LINK_REVOKE
Workflow
WORKFLOW_START, WORKFLOW_APPROVE, WORKFLOW_REJECT, WORKFLOW_ESCALATE, WORKFLOW_CANCEL
Task
TASK_CREATE, TASK_ASSIGN, TASK_STATUS_CHANGE, TASK_OVERDUE, TASK_DELETE
User/Role
USER_CREATE, USER_DEACTIVATE, ROLE_CHANGE, PERMISSION_GRANT, PERMISSION_REVOKE
PDPA
PDPA_CONSENT, CONSENT_RESET, DSR_RECEIVED, DSR_ACCESS, DSR_ERASURE, DATA_EXPORT
Configuration
CONFIG_CHANGE (with Old/New Value), SYSTEM_SETTING_UPDATE, AD_SYNC_COMPLETE


#### 6.9.2 Functional Requirements
FR ID
Requirement
Priority
FR-021-01
Search Audit Log: User, Action, Module, Date Range, IP Address, Record ID
Must
FR-021-02
แสดง Before/After Value (JSON Format) สำหรับ Data Changes
Must
FR-021-03
Export Audit Report เป็น Excel + PDF พร้อม Filter ที่เลือก
Must
FR-021-04
Security Event Monitor: Failed Login Trends + Anomaly Detection
Should
FR-021-05
PDPA DSR Workflow: รับ Request → Process → Confirm ภายใน 30 วัน
Must
FR-021-06
Anomaly Alert: 1HC Access นอกเวลางาน (หลัง 20:00 / ก่อน 06:00)
Should
FR-021-07
Compliance Dashboard: OIC Required Reports + PDPA Summary
Must
FR-021-08
Audit Log Retention Policy Management (Default 7 ปี)
Must


> ⚠ ข้อควรระวัง: MOD-021 เป็น READ-ONLY Interface ทั้งหมด — ไม่มีฟีเจอร์แก้ไข/ลบ Audit Log ในทุกกรณี ถือเป็น Immutable Record



---

### MOD-022
BR-022
Configuration Management
Form: SystemConfigForm, EmailConfigForm, TemplateManagerForm, CalendarForm
Priority
High
Status
✅ Defined


#### 6.10.1 Configuration Categories
Category
Key Settings
Critical?
Approval Required
System Settings
Session Timeout, Upload Quota, Retention Period, Lockout Policy, Archive Policy
Yes
Admin Only
Email Server
SMTP Host/Port/Credentials (Encrypted), TLS, From Address
Yes
2-Admin Approval
Network Drive
Path, Credentials (Encrypted), Retry Policy, Timeout
Yes
2-Admin Approval
Holiday Calendar
r_SystemCalendar — วันหยุดสำหรับ SLA Business Days
Yes
Admin Only
Notification Templates
Email + In-App Templates สำหรับแต่ละ Event Type
No
Admin Only
Theme / Branding
Logo, System Name, Color Scheme
No
Admin Only


> 📋 Business Rule: Email/Network Credentials เก็บ Encrypted (AES-256) ไม่แสดงใน UI เป็น Plain Text | 2-Admin Approval (4-Eyes Principle) สำหรับ Critical Config | ทุก Config Change Log ใน l_AuditLog พร้อม Old/New Value



---

### MOD-023
BR-023
Integration Layer
Form: APIManagementForm, IntegrationLogViewer, WebhookConfigForm
Priority
High
Status
✅ Defined


#### 6.11.1 REST API Endpoints
Group
Method
Endpoint
Description
Auth
POST
/api/v1/auth/login
AD Credential Authentication → JWT Token (1 hour)
Auth
POST
/api/v1/auth/logout
Invalidate Token + Session
Auth
POST
/api/v1/auth/refresh
Refresh JWT Token
Auth
GET
/api/v1/auth/validate
Validate Token + Return Claims
Documents
GET
/api/v1/documents
List (with Permission Filter)
Documents
GET
/api/v1/documents/{id}
Get Detail + Download URL
Documents
POST
/api/v1/documents
Upload (multipart/form-data)
Documents
PUT
/api/v1/documents/{id}
Update Metadata
Documents
DELETE
/api/v1/documents/{id}
Soft-Delete
Folders
GET
/api/v1/folders
List Folders (Permission-filtered)
Folders
GET
/api/v1/folders/{id}/contents
List Folder Contents
Tasks
GET
/api/v1/tasks
List Tasks (filtered by assignee)
Tasks
POST
/api/v1/tasks
Create Task
Tasks
PUT
/api/v1/tasks/{id}
Update Status/Fields
Tasks
POST
/api/v1/tasks/{id}/comments
Add Comment
Users
GET
/api/v1/users
List Users (Admin only)
Users
GET
/api/v1/users/{id}
Get User Profile


#### 6.11.2 API Security & Constraints
Constraint
Specification
Authentication
JWT Token — Bearer Auth ทุก Request
Rate Limiting
1,000 Requests/Hour/User | System-wide: 10,000/Hour
Protocol
HTTPS Only (TLS 1.2+) — HTTP ถูก Redirect หรือ Reject
API Versioning
v1, v2... — Backward Compatible รองรับ 2 Versions พร้อมกัน
IP Whitelist
External Integration ต้อง Register IP
Export Limit
Export File Size สูงสุด 50 MB
Network Drive Retry
Auto-Retry 3 ครั้ง ทุก 30 วินาที → Alert + Queue
Integration Logging
ทุก API Call Log ใน l_AuditLog (API_ACCESS)



## 7. PDPA & Compliance Requirements

> 💡 หมายเหตุ: ส่วนนี้กำหนดข้อกำหนดด้านการปฏิบัติตาม PDPA Thailand B.E. 2562, OIC Circular 2566, และ CRAF Data Classification Standards ซึ่งเป็น Mandatory Requirements ที่ไม่สามารถ Override ได้



### 7.1 PDPA Data Elements & Consent Matrix
Table
Field
PII Type
CRAF Level
Basis
Consent Type
m_UserAccount
ADUserName
Personal Identifier
3NC
สัญญาจ้าง
Implied
m_UserAccount
DisplayName
Personal Data
3NC
สัญญาจ้าง
Implied
m_UserAccount
Email
Contact Data
3NC
สัญญาจ้าง
Required — Login
m_Employee
FullNameTH/EN
Personal Data
3NC
สัญญาจ้าง
Required
m_Employee
NationalID
Sensitive Personal Data
1HC
ไม่จำเป็น
Explicit Written
m_Employee
BirthDate
Personal Data
2SC
สัญญาจ้าง
Required
m_Employee
PhoneNumber
Contact Data
3NC
สัญญาจ้าง
Required
m_Employee
PersonalEmail
Contact Data
3NC
Consent
Required
m_Employee
Photo
Biometric-like Data
2SC
Consent
Required
m_Employee
Address
Personal Data
3NC
สัญญาจ้าง
Required
t_FileDocument
File Content (ถ้ามี PII)
Variable
ตาม Folder Level
Variable
Inherit Folder
l_AuditLog
ActionBy (ADUserName)
Personal Data
3NC
Legal Obligation
Implied


### 7.2 PDPA Rights Implementation
PDPA Right
Article
Implementation
SLA
Right to Access
Art. 30
DSR Workflow ใน MOD-021 → Compile PII Report → PDF Export
30 วัน
Right to Erasure
Art. 33
Anonymize PII Fields + Audit Log ADUserName → [ANONYMIZED]
30 วัน
Right to Portability
Art. 31
Export PII Data เป็น JSON/Excel สำหรับ Data Subject
30 วัน
Right to Restriction
Art. 34
Admin Lock Profile = No PII Access จนกว่าจะ Resolve
Immediate
Right to Object
Art. 32
Admin Mark Field = Excluded From Processing
30 วัน
Consent Withdrawal
Art. 14
Admin Reset Consent → User ต้อง Re-consent บน Login ถัดไป
Immediate


### 7.3 Data Retention Policy (CRAF-aligned)
CRAF Level
Description
Retention
Action at Expiry
Auto/Manual
1HC — High Confidential
ข้อมูลลับสูงสุด
10 ปี
Archive + Admin Approval ก่อน Permanent Delete
Manual
2SC — Semi Confidential
ข้อมูลลับรอง
7 ปี
Archive → Review → Delete
Semi-Auto
3NC — Normal Confidential
ข้อมูลลับปกติ
5 ปี
Archive → Auto-Delete หลัง 1 ปีใน Archive
Auto
4PC — Public Confidential
ข้อมูลสาธารณะ
3 ปี
Auto-Archive → Auto-Delete
Auto
l_AuditLog
Audit Records
7 ปี (OIC ≥ 5 ปี)
Archive ไม่ลบ — Compliance Requirement
Never Delete


### 7.4 OIC Circular 2566 Compliance Requirements
OIC Requirement
System Implementation
Module
Audit Trail ทุก Critical Operation
l_AuditLog Append-Only — ทุก Operation ที่กำหนดใน §6.9.1

---

### MOD-021
Audit Log Retention ≥ 5 ปี
System Default 7 ปี (Safety Margin)

---

### MOD-022
IT System Documentation
IT Landscape Map + Change History

---

### MOD-018
Data Classification
CRAF Level บังคับทุก Data Element

---

### MOD-019
Access Control
RBAC + Folder-level Permission + Audit
MOD-004, MOD-020
Incident Notification
Anomaly Alert + Security Event Monitor

---

### MOD-021


### 7.5 Security Implementation
Security Control
Implementation
Standard
Authentication
AD/LDAP Only — ห้าม Local Account | JWT สำหรับ API
OIC, ISO 27001
Password Policy
Enforce AD Policy | No Bypass | Force Reset ตาม AD Rule
ISO 27001
Encryption at Rest
AES-256 สำหรับ Sensitive Fields (NationalID, Credentials)
PDPA, ISO 27001
Encryption in Transit
TLS 1.2+ (HTTPS) ทุก Connection | LDAPS สำหรับ AD
OWASP, ISO 27001
SQL Injection Prevention
Parameterized Queries ทุก Query — ห้าม String Concatenation
OWASP Top 10 A03
Input Validation
Server-side Validation + Whitelist Approach
OWASP Top 10 A03
Session Security
30 min Timeout | 3 Concurrent Max | Secure Session Token
OWASP Top 10 A07
Account Lockout
5 Failed Attempts → Lock 30 min → Log + Alert
ISO 27001
Data Masking
PII Fields Masked ใน Non-authorized Views
PDPA Art. 26
Virus Scanning
ทุก File Upload ผ่าน Anti-Virus Scan ก่อน Storage
Best Practice



## 8. Non-Functional Requirements (NFR)

> 💡 หมายเหตุ: NFR เหล่านี้เป็น Quality Attributes ที่ต้องวัดและ Verify ได้ในกระบวนการ Testing ทุกรายการมีวิธีการวัดที่กำหนดไว้ชัดเจน



### 8.1 Performance Requirements (NFR-P)
NFR ID
Requirement
Target
Measurement Method
Priority
NFR-P01
Page Load Time
< 3 วินาที (P95)
APM Tool — Production Monitoring
High
NFR-P02
Search Response Time
< 2 วินาที (Average)
Automated Load Test
High
NFR-P03
File Upload 50MB
< 30 วินาที
Performance Test — Measured E2E
High
NFR-P04
Report Generation (Standard)
< 10 วินาที
Automated Test
Medium
NFR-P05
Report Generation (Complex)
< 60 วินาที
Manual Test
Medium
NFR-P06
API Response Time
< 1 วินาที (P95)
API Test Suite
High
NFR-P07
Database Query (Average)
< 500 ms
SQL Profiler + Query Analyzer
High
NFR-P08
Concurrent Users (Target)
50 Users — No Degradation
Load Test (JMeter/Locust)
Critical
NFR-P09
Concurrent Users (Design Capacity)
100 Users
Stress Test
High
NFR-P10
Concurrent File Downloads
50 Simultaneous
Stress Test
High


### 8.2 Scalability Requirements (NFR-S)
NFR ID
Requirement
Target
Timeline
NFR-S01
User Scalability
500 Users ภายใน 2 ปี
Year 2 Planning
NFR-S02
Storage Capacity
รองรับ 10 TB Documents
Initial + Expansion Plan
NFR-S03
Document Count
1 ล้าน Documents ใน System
Database Index Strategy
NFR-S04
Database Records
100 ล้าน Records ทั้งระบบ
Partitioning Strategy


### 8.3 Availability & Reliability Requirements (NFR-A)
NFR ID
Requirement
Target
Calculation
NFR-A01
System Uptime
99.5% (ไม่รวม Planned Maintenance)
Max Downtime: 43.8 ชม./ปี
NFR-A02
Planned Maintenance Window
อาทิตย์ 02:00–06:00 น.
Max 4 ชม./เดือน = 48 ชม./ปี
NFR-A03
Max Downtime Per Incident
4 ชั่วโมง
RTO Requirement
NFR-A04
Advance Notice (Planned Maintenance)
3 วัน
Email Notification to All Users
NFR-A05
Incremental Backup Frequency
Daily (Automated)
Off-peak Hours
NFR-A06
Full Backup Frequency
Weekly (Automated)
Sunday Off-peak
NFR-A07
Backup Retention (Online)
30 วัน
Rolling Window
NFR-A08
Backup Retention (Archive)
7 ปี
OIC Compliance
NFR-A09
Recovery Time Objective (RTO)
4 ชั่วโมง
Business Continuity Plan
NFR-A10
Recovery Point Objective (RPO)
24 ชั่วโมง
Last Full Backup
NFR-A11
DR RTO
8 ชั่วโมง
DR Scenario


### 8.4 Security Requirements (NFR-SEC)
NFR ID
Requirement
Specification
Standard
NFR-SEC01
Authentication Method
AD/LDAP Only — ห้าม Local Account ทุกกรณี
OIC, ISO 27001 A.9
NFR-SEC02
Session Timeout
30 นาที (Inactivity)
OWASP A07
NFR-SEC03
Account Lockout
ผิด 5 ครั้ง → Lock 30 นาที
ISO 27001 A.9.4
NFR-SEC04
Concurrent Sessions
สูงสุด 3 Sessions/User
Best Practice
NFR-SEC05
Encryption at Rest
AES-256 — Sensitive Data Only
PDPA, ISO 27001 A.10
NFR-SEC06
Encryption in Transit
TLS 1.2+ (Mandatory) / TLS 1.3 (Preferred)
OWASP, ISO 27001
NFR-SEC07
SQL Injection
Parameterized Queries — 100% Coverage
OWASP A03:2021
NFR-SEC08
Vulnerability Scan
Weekly Automated + Quarterly Assessment
ISO 27001 A.12
NFR-SEC09
Penetration Testing
Annually
ISO 27001 A.18


### 8.5 Usability Requirements (NFR-U)
NFR ID
Requirement
Target
NFR-U01
Navigation Depth
≤ 3 Clicks ถึงทุก Function หลัก
NFR-U02
Primary Language
ภาษาไทย (Primary) + English (Technical Terms)
NFR-U03
Screen Resolution
1920×1080 (Primary) + 1366×768 (Minimum)
NFR-U04
Accessibility
WCAG 2.1 Level A (Minimum)
NFR-U05
Keyboard Navigation
รองรับ Tab Navigation ทุก Form
NFR-U06
Error Messages
ภาษาไทยที่เข้าใจง่าย + Error Code + Suggested Action


### 8.6 Maintainability Requirements (NFR-M)
NFR ID
Requirement
Target
NFR-M01
Unit Test Coverage
70% — Critical Paths (Business Logic, Service Layer)
NFR-M02
Code Documentation
XML Comments บน Public Methods ทุก Method
NFR-M03
Code Review
Pull Request Required ก่อน Merge ทุก Branch
NFR-M04
Technical Debt
Track และ Address ทุก Sprint
NFR-M05
Deployment Documentation
Step-by-step Deployment Guide พร้อม Rollback Plan


### 8.7 Compliance Requirements (NFR-C)
NFR ID
Requirement
Standard
Priority
NFR-C01
Personal Data Protection
PDPA Thailand B.E. 2562
Critical
NFR-C02
Insurance Regulation
OIC Circular 2566
Critical
NFR-C03
Data Classification
CRAF Standards
Critical
NFR-C04
Audit Trail Completeness
100% Critical Operations Logged
Critical
NFR-C05
Security Standards
OWASP Top 10:2021
High
NFR-C06
Security Best Practice
ISO 27001:2022 (Best Effort)
Medium



## 9. System Constraints & Dependencies

### 9.1 Technical Constraints
ID
Constraint
Impact
Mitigation
C-001
.NET 4.5 — No Async/Await (บางส่วน) — Limited Modern Libraries
Performance Ceiling on Some Operations
Design Sync-optimized DAL + Thread Pool Management
C-002
Raw ADO.NET Only — No ORM
More Code for Data Access — Risk of SQL Errors
Strict Code Review + SP-based Queries
C-003
Network Drive Dependency — SPOF
Upload Fail = Full Feature Loss
Retry + Queue + Health Check + Alert
C-004
On-Premise Only — No Cloud Scale
Limited Horizontal Scaling
Capacity Plan + Vertical Scaling Path
C-005
Windows Forms — No Responsive Web
Desktop Only (Phase 1)
Phase 2 Web Migration Plan
C-006
SQL Server 2019 — Licensing Cost
Not latest version
Plan Upgrade Path if needed


### 9.2 Business Constraints
ID
Constraint
Value
B-001
Project Duration
90 Business Days (Fixed, Non-negotiable)
B-002
Go-Live Date
28 มีนาคม 2569 (Target)
B-003
Budget
ตาม Project Sponsor Approval — ±5% Tolerance
B-004
Resources
Internal Team Only (No External Contractors Without Approval)
B-005
Parallel Run
2 สัปดาห์ขั้นต่ำก่อน Full Go-Live
B-006
Training
90% ของ Users ต้องผ่าน Training ก่อน Go-Live
B-007
No Disruption
ห้ามหยุด Current Business Operations ระหว่าง Deployment


### 9.3 External Dependencies
Dependency
Type
Required For
Risk Level
Mitigation
Active Directory
Hard
Authentication, User Sync
High
LDAP Redundancy + Fallback Alert
Network Drive \\bui-filecenter
Hard
All File Operations
High
Monitor + Retry + Queue
SQL Server 2019
Hard
All Data Operations
High
DB Clustering (Future)
SMTP Mail Server
Soft
Notifications
Medium
Queue + Retry + Admin Alert
Core Insurance System
Soft
Reference Data
Low
Graceful Degradation
Outlook Calendar
Optional
Timeline Sync
Low
Feature Disabled if Unavailable


### 9.4 Assumptions
- AS-001: Network Drive มี Available Capacity ≥ 10 TB และ Accessible 24/7
- AS-002: Active Directory ทำงานอยู่และ Accessible ตลอดเวลา
- AS-003: Network Bandwidth ≥ 100 Mbps สำหรับ File Upload/Download
- AS-004: Users มี Modern Browser (Chrome/Edge Latest 2 Versions) สำหรับ Web Components
- AS-005: Database Server มี RAM ≥ 16 GB และ CPU ≥ 8 Cores
- AS-006: Key Stakeholders Available สำหรับ Requirements Validation และ UAT
- AS-007: Timeline 90 Business Days เป็นไปได้จริงด้วย Resource ที่มีอยู่
- AS-008: Existing Backup System รองรับ Network Drive Backup



## 10. Requirements Traceability Matrix (RTM)
RTM แสดงการเชื่อมโยงระหว่าง Business Requirements (BRD) กับ System Requirements (SRS) ในเอกสารนี้ ใช้สำหรับ Verify ว่าทุก Business Requirement ถูก Address ใน System Requirements ครบถ้วน


### 10.1 BRD → SRS Traceability
BR ID
BR Title
SRS Section
Priority
Complexity
Status
BR-001
Folder Organization Structure
§4.1
Critical
High
✅ Defined
BR-002
Folder Structure Management
§4.2
Critical
High
✅ Defined
BR-003
File Upload and Storage
§4.3
Critical
High
✅ Defined
BR-004
Folder Settings and Security
§4.4
Critical
High
✅ Defined
BR-005
File Sharing System
§5.1
High
Medium
✅ Defined
BR-006
Knowledge Base System
§5.2
Critical
High
✅ Defined
BR-007
Announcement System
§5.3
Medium
Medium
✅ Defined
BR-008
Workflow Management
§5.4
Critical
Very High
✅ Defined
BR-009
Task Management
§5.5
Critical
High
✅ Defined
BR-010
Todo List Management
§5.6
Medium
Low
✅ Defined
BR-011
Notes and Comments System
§5.7
Medium
Medium
✅ Defined
BR-012
Work Instructions
§5.8
High
Medium
✅ Defined
BR-013
Project Management
§6.1
High
High
✅ Defined
BR-014
Resource Management
§6.2
High
High
✅ Defined
BR-015
Timeline and Deadline
§6.3
Medium
Medium
✅ Defined
BR-016
Employee Management
§6.4
Medium
Medium
✅ Defined
BR-017
Programs List Management
§6.5
Medium
Low
✅ Defined
BR-018
IT Landscape Mapping
§6.6
Medium
Medium
✅ Defined
BR-019
Data Dictionary System
§6.7
Medium
Medium
✅ Defined
BR-020
User and Role Management
§6.8
Critical
High
✅ Defined
BR-021
Audit and Compliance
§6.9
Critical
High
✅ Defined
BR-022
Configuration Management
§6.10
High
Medium
✅ Defined
BR-023
Integration Layer
§6.11
High
Very High
✅ Defined


### 10.2 NFR Traceability
NFR Group
SRS Section
Standards
Status
Performance (NFR-P01 ถึง P10)
§8.1
IEEE 830 — Performance Attributes
✅ Defined
Scalability (NFR-S01 ถึง S04)
§8.2
Capacity Planning Best Practice
✅ Defined
Availability (NFR-A01 ถึง A11)
§8.3
ISO 22301 Business Continuity
✅ Defined
Security (NFR-SEC01 ถึง SEC09)
§8.4
ISO 27001:2022, OWASP Top 10
✅ Defined
Usability (NFR-U01 ถึง U06)
§8.5
WCAG 2.1, ISO 9241
✅ Defined
Maintainability (NFR-M01 ถึง M05)
§8.6
ISO/IEC 25010 Quality Model
✅ Defined
Compliance (NFR-C01 ถึง C06)
§8.7
PDPA, OIC, CRAF
✅ Defined


### 10.3 Requirements Coverage Summary
Category
Total
Defined
Coverage
Business Requirements (BR)
23
23
100%
Functional Requirements (FR)
100+
100+
100%
Non-Functional Requirements (NFR)
34
34
100%
Compliance Requirements (PDPA/OIC/CRAF)
20+
20+
100%
Security Requirements
10
10
100%


### 10.4 Success Criteria
Metric
Target
Measurement
Timeline
On-time Delivery
90 Business Days
Project Completion Date
28 มีนาคม 2569
User Adoption
80% ภายใน 3 เดือน
Active Login Count
มิถุนายน 2569
System Uptime
99.5%
Production Monitoring
Monthly
Document Search Time
50% Reduction
Before/After User Study
Q3 2569
Approval Cycle Time
30% Reduction
Workflow Report
Q3 2569
Task SLA Compliance
95% On Time
Task Management Report
Monthly
User Satisfaction
4.0/5.0
Post Go-Live Survey
Q3 2569
Audit Trail Completeness
100% Critical Ops
Audit Log Review
Monthly
Zero Critical Compliance Violations
0
OIC Audit
Annual

— End of Document —
Software Requirements Specification  |  Core Management System  |  Version 1.0  |  มีนาคม 2569
Project Code: 2511AL01  |  Standard: IEEE 830 / ISO/IEC 29148  |  Confidential — Internal Use Only
| Software Requirements Specification  |  Core Management System (CMS)  |  Project 2511AL01 | Version 1.0  |  มีนาคม 2569 |
| --- | --- |


| © Core Management System — Internal Confidential | Page   |
| --- | --- |


| Software Requirements Specification  |  Core Management System (CMS)  |  Project 2511AL01 | Version 1.0  |  มีนาคม 2569 |
| --- | --- |


| © Core Management System — Internal Confidential | Page   |
| --- | --- |


| Software Requirements Specification  |  Core Management System (CMS)  |  Project 2511AL01 | Version 1.0  |  มีนาคม 2569 |
| --- | --- |


| © Core Management System — Internal Confidential | Page   |
| --- | --- |


| Software Requirements Specification  |  Core Management System (CMS)  |  Project 2511AL01 | Version 1.0  |  มีนาคม 2569 |
| --- | --- |


| © Core Management System — Internal Confidential | Page   |
| --- | --- |


| Software Requirements Specification  |  Core Management System (CMS)  |  Project 2511AL01 | Version 1.0  |  มีนาคม 2569 |
| --- | --- |


| © Core Management System — Internal Confidential | Page   |
| --- | --- |


| Software Requirements Specification  |  Core Management System (CMS)  |  Project 2511AL01 | Version 1.0  |  มีนาคม 2569 |
| --- | --- |


| © Core Management System — Internal Confidential | Page   |
| --- | --- |


| Software Requirements Specification  |  Core Management System (CMS)  |  Project 2511AL01 | Version 1.0  |  มีนาคม 2569 |
| --- | --- |


| © Core Management System — Internal Confidential | Page   |
| --- | --- |


| Software Requirements Specification  |  Core Management System (CMS)  |  Project 2511AL01 | Version 1.0  |  มีนาคม 2569 |
| --- | --- |


| © Core Management System — Internal Confidential | Page   |
| --- | --- |


| Software Requirements Specification  |  Core Management System (CMS)  |  Project 2511AL01 | Version 1.0  |  มีนาคม 2569 |
| --- | --- |


| © Core Management System — Internal Confidential | Page   |
| --- | --- |


| Software Requirements Specification  |  Core Management System (CMS)  |  Project 2511AL01 | Version 1.0  |  มีนาคม 2569 |
| --- | --- |


| © Core Management System — Internal Confidential | Page   |
| --- | --- |

