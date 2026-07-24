# CMS Data Dictionary
## พจนานุกรมข้อมูล — ฉบับสมบูรณ์
### โปรเจ็ค 2511AL01 | SQL Server 2019 (Thai_CI_AS)
> Compliance: OIC Circular 2566 | CRAF Data Classification | PDPA 2562


Core Management System
Data Dictionary
พจนานุกรมข้อมูล — ฉบับสมบูรณ์
โปรเจ็ค: 2511AL01  |  SQL Server 2019 (Thai_CI_AS)
Compliance: OIC Circular 2566  |  CRAF Data Classification  |  PDPA 2562
จัดทำโดย: Senior System Analyst  |  7 เมษายน 2569

9 Schemas  |  60+ Tables  |  12 Stored Procedures


สรุป Database Schema ทั้งหมด
Schema
Full Name
Tables
Modules
คำอธิบาย
EMS
Employee Management System
9
MOD-016, MOD-020
จัดการพนักงาน, บัญชีผู้ใช้, โครงสร้างองค์กร, ทักษะ, ใบอนุญาต และ Role Permission Matrix
DMS
Document Management System
11
MOD-001 ถึง MOD-005
จัดการ Folder Hierarchy, ไฟล์เอกสาร, ACL Permission, Version Control, Share ทั้งภายในและภายนอก
KMS
Knowledge Management System
7
MOD-006, MOD-007
Knowledge Base (SOP/FAQ/Guideline), Announcement ประกาศแจ้งเวียน, Version Control บทความ
WFE
Workflow Engine
5
MOD-008
Workflow Process Engine — Version Freeze, Multi-step Approval, SLA Timer (ไม่นับวันหยุด), Escalation, Delegation
TMS
Task Management System
6
MOD-009 ถึง MOD-012
จัดการ Task และ Sub-task (≤3 ระดับ), Todo List, Notes/Comments, Work Instructions
PMS
Project Management System
7
MOD-013, MOD-014, MOD-015
จัดการโปรเจ็ค, WBS, Milestone, Resource Allocation (ตรวจ Overlap 100%), Timeline & Deadline
ITL
IT Landscape System
4
MOD-017, MOD-018, MOD-019
บริหารจัดการ Programs List, IT Landscape Mapping, Integration Relationships, Data Dictionary ของแต่ละระบบ
ATS
Audit Trail System
4
MOD-021
ระบบ Audit Trail ตาม OIC Circular 2566 — Append-Only, เก็บ ≥ 5 ปี, ไม่มี FK (Log Independence), ADUserName เป็น Snapshot
COM
Common & Configuration
6
MOD-022, MOD-023
ตาราง Common ใช้ร่วมกันทุก Module — System Config (Key-Value), Notification, API Token, User Group
> * ทุก Table รองรับภาษาไทย (NVARCHAR, Thai_CI_AS Collation)  |  ข้อมูลสีแดง = PII ต้องปฏิบัติตาม PDPA 2562


คำอธิบายสัญลักษณ์ (Key Legend)
สัญลักษณ์
ความหมาย
🔑 PK
Primary Key — รหัสหลักของตาราง ค่าต้องไม่ซ้ำและไม่เป็น NULL
🔗 FK
Foreign Key — อ้างอิงไปยัง Primary Key ของตารางอื่น
◈ UQ
Unique Constraint — ค่าต้องไม่ซ้ำกันในทุก Row
⚡ IDX
Index — คอลัมน์ที่สร้าง Non-clustered Index เพิ่มเติมเพื่อ Performance
🔒 PII
Personal Identifiable Information — ข้อมูลส่วนบุคคลตาม PDPA 2562 ต้องขอ Consent ก่อน Access



---

## EMS — Employee Management System
Module: MOD-016, MOD-020  |  จัดการพนักงาน, บัญชีผู้ใช้, โครงสร้างองค์กร, ทักษะ, ใบอนุญาต และ Role Permission Matrix


### EMS.m_OrgUnit

> **[Master Data]** โครงสร้างองค์กร 3 ระดับ: Sub-Committee → Division → Department รองรับ Self-referencing Hierarchy และ Soft Delete

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
OrgUnitID
INT IDENTITY
รหัส OrgUnit (Primary Key, Auto Increment)
1
🔑 PK
OrgUnitCode
NVARCHAR(20)
รหัสหน่วยงาน เช่น SC001, DV001, DP001
DP001
◈ UQ
OrgUnitNameTH
NVARCHAR(200)
ชื่อหน่วยงานภาษาไทย
แผนกพัฒนาระบบ

OrgUnitNameEN
NVARCHAR(200)
ชื่อหน่วยงานภาษาอังกฤษ
Software Dev

OrgUnitType
TINYINT
1=Sub-Committee  2=Division  3=Department
3

ParentOrgUnitID
INT
FK ชี้ตัวเอง (Self-ref) — NULL = ระดับสูงสุด
2
🔗 FK
CostCenter
VARCHAR(20)
รหัส Cost Center สำหรับระบบการเงิน
CC-IT-001

SortOrder
INT
ลำดับการแสดงผลใน Tree View
1

IsActive
BIT
1=ใช้งาน  0=ปิดการใช้งาน
1

IsDeleted
BIT
Soft Delete Flag (1=ถูกลบแล้ว)
0

DeletedBy
NVARCHAR(100)
ADUserName ผู้ลบ (Snapshot)
DOMAIN\admin

DeletedDate
DATETIME
วันเวลาที่ทำ Soft Delete
2024-01-15

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง Record
DOMAIN\admin

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-01-01

UpdatedBy
NVARCHAR(100)
ADUserName ผู้แก้ไขล่าสุด
DOMAIN\admin

UpdatedDate
DATETIME
วันเวลาที่แก้ไขล่าสุด
2024-06-01



### EMS.m_UserAccount

> **[Master Data]** บัญชีผู้ใช้งาน Sync จาก Active Directory — ไม่เก็บ Password — PDPA: ADUserName, DisplayName, Email เป็น PII

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
UserID
INT IDENTITY
รหัส User (PK)
1
🔑 PK
ADUserName
NVARCHAR(100)

> **[PII]** Windows Domain\Username

DOMAIN\somchai
◈🔒
DisplayName
NVARCHAR(200)

> **[PII]** ชื่อ-นามสกุล สำหรับแสดงผลในระบบ

สมชาย รักษ์ไทย
🔒 PII
Email
NVARCHAR(200)

> **[PII-Consent]** อีเมลบริษัท ต้องขอ Consent ก่อน

som@company.th
🔒 PII
OrgUnitID
INT
FK → EMS.m_OrgUnit (สังกัดแผนก)
5
🔗 FK
AppRole
TINYINT
1=Officer  2=Sr.Officer  3=Manager  4=Admin
2

IsActive
BIT
1=บัญชีใช้งานได้  0=ปิดบัญชี
1

LastLoginDate
DATETIME
วันเวลา Login ล่าสุด
2024-06-15

ConsentGiven
BIT
PDPA: ยืนยัน Consent แล้ว (1=ใช่)
1

ConsentDate
DATETIME
วันที่ให้ Consent
2024-01-10

ConsentVersion
VARCHAR(10)
เวอร์ชัน Privacy Policy ที่ยืนยัน
1.0

FailedLoginCount
TINYINT
จำนวนครั้ง Login ผิดติดต่อกัน (Reset เมื่อสำเร็จ)
0

IsLocked
BIT
ล็อกบัญชีชั่วคราวเพราะ Login ผิดเกินกำหนด
0

LockedUntil
DATETIME
บัญชีล็อกถึงวันเวลานี้ (NULL=ไม่ล็อก)
NULL

CreatedDate
DATETIME
วันเวลาที่สร้างบัญชี
2024-01-01

UpdatedDate
DATETIME
วันเวลาที่แก้ไขล่าสุด
2024-06-01



### EMS.m_Employee

> **[Master Data]** ข้อมูลพนักงานโดยละเอียด 1:1 กับ UserAccount — PDPA: PII หลายคอลัมน์ — DATA_COMPRESSION=ROW

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
EmployeeID
INT IDENTITY
รหัสพนักงาน (PK)
1
🔑 PK
EmployeeCode
VARCHAR(20)
รหัสพนักงาน รูปแบบ EMP-YYYY-NNN
EMP-2566-001
◈ UQ
UserAccountID
INT
FK → EMS.m_UserAccount (1:1)
3
🔗◈ FK+UQ
FullNameTH
NVARCHAR(200)

> **[PII]** ชื่อ-นามสกุล ภาษาไทย

สมชาย รักษ์ไทย
🔒 PII
FullNameEN
NVARCHAR(200)

> **[PII]** ชื่อ-นามสกุล ภาษาอังกฤษ

Somchai R.
🔒 PII
NickName
NVARCHAR(100)
ชื่อเล่น
ชาย

Gender
TINYINT
1=Male  2=Female  3=Other  NULL=ไม่ระบุ
1

DateOfBirth
DATE

> **[PII]** วันเดือนปีเกิด

1990-05-20
🔒 PII
NationalID
VARCHAR(13)

> **[PII-Sensitive]** เลขบัตรประชาชน เก็บแบบ Encrypted

1XXXXXXXXXXX
🔒 PII
Email
NVARCHAR(200)

> **[PII-Consent]** อีเมลบริษัท

som@company.th
🔒 PII
MobilePhone
VARCHAR(20)

> **[PII-Consent]** เบอร์มือถือ

081-234-5678
🔒 PII
PersonalEmail
NVARCHAR(200)

> **[PII-Consent]** อีเมลส่วนตัว

som@gmail.com
🔒 PII
Address
NVARCHAR(500)

> **[PII-Consent]** ที่อยู่ปัจจุบัน

กรุงเทพฯ
🔒 PII
EmergencyContact
NVARCHAR(200)

> **[PII-3rdParty]** ชื่อผู้ติดต่อฉุกเฉิน

นิดา รักษ์ไทย
🔒 PII
EmergencyPhone
VARCHAR(20)

> **[PII-3rdParty]** เบอร์ผู้ติดต่อฉุกเฉิน

082-XXX-XXXX
🔒 PII
OrgUnitID
INT
FK → EMS.m_OrgUnit
5
🔗 FK
PositionID
INT
FK → EMS.s_PositionCatalog
2
🔗 FK
ReportingManagerID
INT
FK → EMS.m_Employee (Self) ผู้บังคับบัญชา
1
🔗 FK
EmploymentStatus
TINYINT
1=Active 2=Probation 3=Leave 4=Resigned 5=Retired
1

EmploymentType
TINYINT
1=Permanent  2=Contract  3=Part-time
1

JoinDate
DATE
วันที่เริ่มงาน
2020-06-15

ConfirmedDate
DATE
วันที่ผ่านทดลองงาน
2020-09-15

ResignDate
DATE
วันที่ลาออกหรือพ้นสภาพ (NULL=ยังทำงานอยู่)
NULL

CertificateNumber
VARCHAR(50)
เลขที่ใบอนุญาต เช่น ประกันชีวิต/วินาศภัย
INS-001-2566

CertificateExpiry
DATE
วันหมดอายุใบอนุญาต
2025-12-31

IsActive
BIT
สถานะพนักงาน (1=ทำงานอยู่)
1

IsAnonymized
BIT
PDPA Right to Erasure: ถูก Anonymize แล้ว
0

AnonymizedDate
DATETIME
วันที่ทำ Anonymize
NULL

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\admin

CreatedDate
DATETIME
วันเวลาที่สร้าง
2020-06-15



### EMS.s_PositionCatalog

> **[Static/Template]** Catalog ตำแหน่งงานทั้งหมดในองค์กร (Static Reference Data)

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
PositionID
INT IDENTITY
รหัสตำแหน่ง (PK)
1
🔑 PK
PositionCode
VARCHAR(20)
รหัสตำแหน่ง เช่น POS-001
POS-001
◈ UQ
PositionNameTH
NVARCHAR(200)
ชื่อตำแหน่งภาษาไทย
นักวิเคราะห์

PositionNameEN
NVARCHAR(200)
ชื่อตำแหน่งภาษาอังกฤษ
Analyst

JobGrade
VARCHAR(10)
Grade ของตำแหน่ง เช่น JG3
JG3

AppRoleMapping
TINYINT
Mapping กับ AppRole 1-4
2

IsActive
BIT
สถานะ Active
1

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
SYSTEM

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-01-01



### EMS.m_SkillCatalog

> **[Master Data]** Catalog ทักษะที่ใช้ในองค์กร เช่น SQL, Python, ประกันชีวิต, Project Management

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
SkillID
INT IDENTITY
รหัสทักษะ (PK)
1
🔑 PK
SkillCode
VARCHAR(20)
รหัสทักษะ เช่น SK-001
SK-001
◈ UQ
SkillName
NVARCHAR(200)
ชื่อทักษะ
SQL Server

SkillCategory
TINYINT
1=Tech 2=Insurance 3=Mgmt 4=Soft 5=Language
1

Description
NVARCHAR(500)
คำอธิบายทักษะ
MS SQL DB

IsActive
BIT
สถานะ Active
1

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
SYSTEM

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-01-01



### EMS.t_EmployeeSkill

> **[Transaction]** บันทึกทักษะของพนักงานแต่ละคน (Many-to-Many) พร้อมระดับความเชี่ยวชาญและประสบการณ์

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
SkillRecordID
INT IDENTITY
รหัส Record (PK)
1
?? PK
EmployeeID
INT
FK → EMS.m_Employee
3
🔗 FK
SkillID
INT
FK → EMS.m_SkillCatalog
1
🔗 FK
ProficiencyLevel
TINYINT
1=Beginner 2=Intermediate 3=Advanced 4=Expert
3

YearsExperience
INT
ปีประสบการณ์ด้านทักษะนี้
5

LastUsedDate
DATE
วันที่ใช้ทักษะนี้ล่าสุด
2024-05-01

AssessmentType
TINYINT
1=Self  2=Manager  3=External
2

AssessedBy
NVARCHAR(100)
ADUserName ผู้ประเมิน
DOMAIN\mgr

AssessedDate
DATETIME
วันที่ประเมิน
2024-04-01

ValidUntil
DATE
วันหมดอายุ Certificate
2025-12-31

IsValidated
BIT
ผ่านการยืนยันอย่างเป็นทางการ
1

Remarks
NVARCHAR(300)
หมายเหตุเพิ่มเติม
-



### EMS.t_EmployeeCertificate

> **[Transaction]** ใบอนุญาต/ใบรับรองของพนักงาน เช่น ใบอนุญาตประกันชีวิต, วินาศภัย (คปภ.)

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
CertID
INT IDENTITY
รหัส Certificate (PK)
1
🔑 PK
EmployeeID
INT
FK → EMS.m_Employee
3
🔗 FK
CertType
TINYINT
1=ประกันชีวิต  2=วินาศภัย  3=อื่นๆ
1

CertNumber
VARCHAR(50)
เลขที่ใบอนุญาต
INS-001

IssuedBy
NVARCHAR(200)
ออกโดย เช่น คปภ.
คปภ.

IssuedDate
DATE
วันที่ออกใบอนุญาต
2022-01-15

ExpiryDate
DATE
วันหมดอายุใบอนุญาต
2025-12-31

Status
TINYINT
1=Active  2=Expired  3=Revoked
1

DocumentPath
NVARCHAR(500)
Path ไฟล์ใบอนุญาต บน Network
\\SRV\..

Remarks
NVARCHAR(300)
หมายเหตุ
-

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\hr

CreatedDate
DATETIME
วันเวลาที่สร้าง
2022-01-15



### EMS.s_UserPreference

> **[Static/Template]** การตั้งค่าส่วนตัวของ User: ภาษา, รูปแบบวันที่, Kanban Config, Dashboard Widget (1:1 กับ UserAccount)

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
PrefID
INT IDENTITY
รหัส Preference (PK)
1
🔑 PK
UserID
INT
FK → EMS.m_UserAccount (1:1)
3
🔗◈ FK+UQ
Language
VARCHAR(5)
ภาษาที่ใช้: th หรือ en
th

DateFormat
VARCHAR(20)
รูปแบบวันที่แสดงผล
DD/MM/YYYY

TimeZone
VARCHAR(50)
Timezone ที่ใช้
Asia/Bangkok

EmailNotification
BIT
รับแจ้งเตือนทาง Email
1

InAppNotification
BIT
รับแจ้งเตือนใน Application
1

DefaultHomePage
VARCHAR(100)
หน้าแรกเมื่อ Login สำเร็จ
/dashboard

KanbanColumnConfig
NVARCHAR(MAX)
JSON ตั้งค่าคอลัมน์ Kanban Board
{"cols":[]}

DashboardWidgetConfig
NVARCHAR(MAX)
JSON เลือก Widget บน Dashboard
{"widgets":[]

ThemeMode
VARCHAR(10)
light หรือ dark
light

FontSize
TINYINT
1=Small  2=Normal  3=Large
2

UpdatedBy
NVARCHAR(100)
ADUserName ผู้แก้ไขล่าสุด
DOMAIN\u

UpdatedDate
DATETIME
วันเวลาที่แก้ไขล่าสุด
2024-06-15



### EMS.c_RolePermission

> **[Configuration]** Matrix สิทธิ์การใช้งานตาม Role (1-4) ในแต่ละ Module — ตรวจสอบระดับ Module ก่อน Folder-level ACL

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
RolePermID
INT IDENTITY
รหัส (PK)
1
🔑 PK
AppRole
TINYINT
1=Officer 2=Sr.Officer 3=Manager 4=Admin
2

ModuleCode
VARCHAR(20)
รหัส Module เช่น MOD-001, MOD-008
MOD-003

CanView
BIT
ดูข้อมูลได้
1

CanCreate
BIT
สร้างข้อมูลได้
1

CanEdit
BIT
แก้ไขข้อมูลได้
1

CanDelete
BIT
ลบข้อมูลได้
0

CanApprove
BIT
อนุมัติได้
0

CanExport
BIT
Export ข้อมูลออกได้
1

CanAdmin
BIT
จัดการ Admin Settings ได้
0

UpdatedBy
NVARCHAR(100)
ADUserName ผู้แก้ไขล่าสุด
DOMAIN\adm

UpdatedDate
DATETIME
วันเวลาที่แก้ไขล่าสุด
2024-01-01



---

## DMS — Document Management System
Module: MOD-001 ถึง MOD-005  |  จัดการ Folder Hierarchy, ไฟล์เอกสาร, ACL Permission, Version Control, Share ทั้งภายในและภายนอก


### DMS.s_ConfidentialityLevel

> **[Static/Template]** Static Lookup ระดับความลับ 4 ระดับตาม CRAF Data Classification — Seed ข้อมูลตอน Deploy

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
LevelCode
TINYINT
รหัสระดับ (PK): 1=HC 2=SC 3=NC 4=PC
1
🔑 PK
LevelID
VARCHAR(10)
รหัสแสดงผล
1HC

LevelNameTH
NVARCHAR(100)
ชื่อระดับความลับ ภาษาไทย
ลับสูงสุด

LevelNameEN
NVARCHAR(100)
ชื่อระดับความลับ ภาษาอังกฤษ
High Confidential

ColorCode
VARCHAR(10)
สี HEX สำหรับแสดงผล Badge
#FF0000

Description
NVARCHAR(300)
คำอธิบายระดับความลับ
เฉพาะผู้บริหาร

AccessPolicy
NVARCHAR(500)
นโยบายการเข้าถึง
ผู้บริหาร+มอบ



### DMS.s_FolderTemplate

> **[Static/Template]** Template โครงสร้าง Folder สำเร็จรูป เก็บ Sub-folder ใน JSON ใช้เมื่อสร้าง Folder/Project ใหม่

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
TemplateID
INT IDENTITY
รหัส Template (PK)
1
🔑 PK
TemplateName
NVARCHAR(100)
ชื่อ Template
แผนกมาตรฐาน
◈ UQ
TemplateType
TINYINT
1=Department  2=Project  3=Custom
1

StructureJSON
NVARCHAR(MAX)
JSON โครงสร้าง Sub-Folder
{"folders":[]}

Description
NVARCHAR(300)
คำอธิบาย Template
Template ทั่วไป

IsActive
BIT
ใช้งานอยู่หรือไม่
1

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\admin

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-01-01



### DMS.s_AllowedFileType

> **[Static/Template]** Whitelist ประเภทไฟล์ที่อนุญาตให้ Upload (BR-003-BL-01) Admin สามารถแก้ไขได้

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
FileTypeID
INT IDENTITY
รหัสประเภทไฟล์ (PK)
1
🔑 PK
FileExtension
VARCHAR(10)
นามสกุลไฟล์ เช่น .pdf, .docx
.pdf
◈ UQ
MimeType
VARCHAR(100)
MIME Type
application/pdf

FileCategory
TINYINT
1=Document 2=Image 3=Spreadsheet 4=Archive 5=Other
1

MaxFileSizeMB
INT
ขนาดไฟล์สูงสุดที่อนุญาต (MB)
50

Description
NVARCHAR(100)
คำอธิบายประเภทไฟล์
PDF Document

IsAllowed
BIT
อนุญาตให้ Upload หรือไม่
1

UpdatedBy
NVARCHAR(100)
ADUserName ผู้แก้ไขล่าสุด
DOMAIN\admin

UpdatedDate
DATETIME
วันเวลาที่แก้ไขล่าสุด
2024-01-01



### DMS.t_Folder

> **[Transaction]** โครงสร้าง Folder แบบ Hierarchy สูงสุด 10 ระดับ — Conf-Root 4 Folders สร้างอัตโนมัติทุกแผนก (IsSystem=1 ลบไม่ได้)

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
FolderID
INT IDENTITY
รหัส Folder (PK)
1
🔑 PK
FolderGUID
UNIQUEIDENTIFIER
GUID ใช้สร้าง Physical Path
GUID...
◈ UQ
FolderName
NVARCHAR(255)
ชื่อ Folder
เอกสาร IT

ParentFolderID
INT
FK → Self (NULL=Root)
NULL
🔗 FK
OrgUnitID
INT
FK → EMS.m_OrgUnit สังกัดแผนก
5
🔗 FK
ConfidentialityLevel
TINYINT
ระดับความลับ 1=HC 2=SC 3=NC 4=PC
3

IsConfRoot
BIT
Conf-Root สร้างอัตโนมัติ (ลบไม่ได้)
0

FolderDepth
TINYINT
ระดับความลึก 0=Root สูงสุด=10
1

PhysicalPath
NVARCHAR(500)
Relative Path บน Network Drive
DP001\3NC

FolderSizeBytes
BIGINT
ขนาดรวมทุกไฟล์ใน Folder (Bytes)
10485760

FileCount
INT
จำนวนไฟล์ใน Folder
5

TemplateID
INT
FK → DMS.s_FolderTemplate
NULL
🔗 FK
IsSystem
BIT
System Folder ลบไม่ได้ (Conf-Root)
0

IsArchived
BIT
Archive แล้วหรือไม่
0

IsDeleted
BIT
Soft Delete Flag
0

RecycleBinExpiry
DATETIME
วันหมดอายุ Recycle Bin (+30 วัน)
2024-07-15

InheritPermission
BIT
รับ Permission จาก Parent (1=ใช่)
1

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\u

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-01-01



### DMS.t_FileDocument

> **[Transaction]** Metadata ของไฟล์ทุกไฟล์ในระบบ — ตัวไฟล์จริงเก็บบน Network Drive DB เก็บแค่ Metadata + Checksum

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
FileID
INT IDENTITY
รหัสไฟล์ (PK)
1
🔑 PK
FileGUID
UNIQUEIDENTIFIER
GUID ของไฟล์
GUID...
◈ UQ
OriginalFileName
NVARCHAR(500)
ชื่อไฟล์ Original ที่ User Upload
รายงาน.pdf

StoredFileName
VARCHAR(200)
ชื่อไฟล์บน Disk (GUID-based)

### GUID.pdf

FileExtension
VARCHAR(10)
นามสกุลไฟล์
.pdf

FileSizeBytes
BIGINT
ขนาดไฟล์ (Bytes)
2097152

FolderID
INT
FK → DMS.t_Folder
3
🔗 FK
PhysicalPath
NVARCHAR(500)
Full Physical Path บน Network Drive
\\SRV\..

Checksum
VARCHAR(64)
SHA-256 Checksum สำหรับ Duplicate Check
abc...
⚡ IDX
ConfidentialityLevel
TINYINT
ระดับความลับ 1-4
3

VirusScanStatus
TINYINT
0=Pending 1=Clean 2=Infected 3=Error
1

VirusScanDate
DATETIME
วันเวลาที่ Scan Virus
2024-06-01

CurrentVersion
INT
Version ปัจจุบันของไฟล์
1

FileStatus
TINYINT
1=Active  2=Archived  3=Pending
1

IsDeleted
BIT
Soft Delete Flag
0

Tags
NVARCHAR(500)
Tags ค้นหา (Comma-separated)
รายงาน,2566

UploadedBy
NVARCHAR(100)
ADUserName ผู้ Upload
DOMAIN\u

UploadedDate
DATETIME
วันเวลาที่ Upload
2024-06-01

ArchiveDate
DATETIME
วันที่จะ Archive (7 ปีจาก Upload)
NULL



### DMS.t_FileVersion

> **[Transaction]** Version History ของแต่ละไฟล์ เก็บ Physical File ทุก Version บน Network Drive

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
VersionID
INT IDENTITY
รหัส Version (PK)
1
🔑 PK
FileID
INT
FK → DMS.t_FileDocument
5
🔗 FK
VersionNumber
INT
หมายเลข Version เรียงตามลำดับ
1

StoredFileName
VARCHAR(200)
ชื่อไฟล์ Version นี้บน Disk
GUID_v1.pdf

PhysicalPath
NVARCHAR(500)
Physical Path ของ Version นี้
\\SRV\..

FileSizeBytes
BIGINT
ขนาดไฟล์ Version นี้ (Bytes)
2048000

Checksum
VARCHAR(64)
SHA-256 Checksum
abc123...

ChangeNote
NVARCHAR(500)
หมายเหตุการเปลี่ยนแปลง
แก้ไข Section 3

UploadedBy
NVARCHAR(100)
ADUserName ผู้ Upload
DOMAIN\u

UploadedDate
DATETIME
วันเวลาที่ Upload Version นี้
2024-06-10



### DMS.t_FolderPermission

> **[Transaction]** ACL ระดับ Folder รองรับ 4 Principal Types (User/Role/OrgUnit/Group) — Explicit Deny Override All Allows (BR-004)

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
PermissionID
INT IDENTITY
รหัส Permission (PK)
1
🔑 PK
FolderID
INT
FK → DMS.t_Folder
3
🔗 FK
PrincipalType
TINYINT
1=User  2=Role  3=OrgUnit  4=Group
1

PrincipalID
INT
ID ตาม PrincipalType
5

CanRead
BIT
สิทธิ์อ่าน/ดูไฟล์
1

CanWrite
BIT
สิทธิ์เขียน/Upload ไฟล์
1

CanDelete
BIT
สิทธิ์ลบไฟล์/Folder
0

CanModify
BIT
สิทธิ์แก้ไข Folder Settings
0

CanFullControl
BIT
สิทธิ์เต็ม รวมถึงการให้สิทธิ์ผู้อื่น
0

IsDeny
BIT
Explicit Deny — Override ทุก Allow
0

IsActive
BIT
Permission ยังใช้งานอยู่
1

ExpiryDate
DATETIME
วันหมดอายุ (NULL=ไม่มีกำหนด)
NULL

GrantedBy
NVARCHAR(100)
ADUserName ผู้ให้สิทธิ์
DOMAIN\mgr

GrantedDate
DATETIME
วันที่ให้สิทธิ์
2024-01-01

Remarks
NVARCHAR(300)
หมายเหตุประกอบ
-



### DMS.t_ShareRecord

> **[Transaction]** การแชร์ไฟล์ภายในองค์กร (Internal Share) ผ่าน Principal Types

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
ShareID
INT IDENTITY
รหัส Share (PK)
1
🔑 PK
FileID
INT
FK → DMS.t_FileDocument
5
🔗 FK
PrincipalType
TINYINT
1=User 2=Role 3=OrgUnit 4=Group
2

PrincipalID
INT
ID ตาม PrincipalType
3

CanDownload
BIT
อนุญาตให้ Download ไฟล์
1

CanReShare
BIT
อนุญาตให้แชร์ต่อได้
0

ExpiryDate
DATETIME
วันหมดอายุการแชร์
2024-12-31

Message
NVARCHAR(500)
ข้อความแนบพร้อมการแชร์
โปรดตรวจสอบ

IsActive
BIT
ยังใช้งานอยู่
1

SharedDate
DATETIME
วันที่แชร์
2024-06-01

CreatedBy
NVARCHAR(100)
ADUserName ผู้แชร์
DOMAIN\u



### DMS.t_ShareLink

> **[Transaction]** Share Link ภายนอก (Token-based GUID) สำหรับส่งให้บุคคลภายนอกที่ไม่ใช่ User ในระบบ

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
ShareLinkID
INT IDENTITY
รหัส ShareLink (PK)
1
🔑 PK
FileID
INT
FK → DMS.t_FileDocument
5
🔗 FK
Token
VARCHAR(50)
GUID-based Unique Token
abcd1234...
◈ UQ
PasswordHash
VARCHAR(128)
SHA-256 Hash ของ Password (NULL=ไม่ต้องใส่)
NULL

MaxDownloads
INT
จำนวนครั้งที่ Download ได้สูงสุด
5

DownloadCount
INT
จำนวนครั้งที่ Download แล้ว
0

ExpiryDate
DATETIME
วันหมดอายุ Link
2024-07-01

IsViewOnly
BIT
ดูอย่างเดียว (ไม่สามารถ Download)
1

IsActive
BIT
Link ยังใช้งานได้
1

Description
NVARCHAR(200)
คำอธิบาย Link
Share ลูกค้า

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง Link
DOMAIN\u

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-06-01



### DMS.l_FileAccessLog

> **[Logging]** Log การเข้าถึงไฟล์ทุกครั้ง (VIEW/DOWNLOAD/PREVIEW) — แยกจาก AuditLog เพื่อ Performance — BIGINT PK

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
AccessLogID
BIGINT IDENTITY
รหัส Log (PK) — BIGINT Volume สูง
1
🔑 PK
FileID
INT
FileID ที่ถูกเข้าถึง (ไม่มี FK)
5

UserID
INT
UserID (NULL=External ผ่าน ShareLink)
3

ADUserName
NVARCHAR(100)
Snapshot ADUserName ณ เวลา Access
DOMAIN\u

AccessType
VARCHAR(20)
VIEW / DOWNLOAD / PREVIEW
DOWNLOAD

AccessDate
DATETIME
วันเวลาที่เข้าถึง
2024-06-15

IPAddress
VARCHAR(45)

> **[PII]** IPv4 หรือ IPv6 ของ Client

192.168.1.1
🔒 PII
MachineName
NVARCHAR(100)
Hostname ของ Client
PC-001

ShareLinkID
INT
ShareLinkID ถ้า Access ผ่าน ShareLink
NULL

IsSuccess
BIT
เข้าถึงสำเร็จหรือไม่
1



### DMS.l_ShareAccessLog

> **[Logging]** Log การเข้าถึงผ่าน Share Link ภายนอก เก็บ IP, UserAgent สำหรับ Security Audit

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
LogID
BIGINT IDENTITY
รหัส Log (PK)
1
🔑 PK
ShareLinkID
INT
ShareLinkID ที่ถูก Access
3

AccessorUserID
INT
UserID ถ้าเป็น Internal User
NULL

IPAddress
VARCHAR(45)

> **[PII]** IPv4/IPv6 ของผู้เข้าถึง

1.2.3.4
🔒 PII
UserAgent
NVARCHAR(500)
Browser/App Information
Chrome/125

AccessDate
DATETIME
วันเวลาที่เข้าถึง
2024-06-15

WasDownloaded
BIT
มีการ Download จริงหรือไม่
0

IsSuccess
BIT
เข้าถึงสำเร็จหรือไม่
1

FailReason
NVARCHAR(200)
เหตุผลที่ Fail เช่น Expired, WrongPwd
NULL



---

## KMS — Knowledge Management System
Module: MOD-006, MOD-007  |  Knowledge Base (SOP/FAQ/Guideline), Announcement ประกาศแจ้งเวียน, Version Control บทความ


### KMS.m_KBCategory

> **[Master Data]** หมวดหมู่บทความ Knowledge Base รองรับ Self-referencing Hierarchy

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
CategoryID
INT IDENTITY
รหัสหมวดหมู่ (PK)
1
🔑 PK
ParentCategoryID
INT
FK → Self (NULL=Root Category)
NULL
🔗 FK
CategoryCode
VARCHAR(20)
รหัสหมวดหมู่
KB-IT-001
◈ UQ
CategoryName
NVARCHAR(100)
ชื่อหมวดหมู่
เทคโนโลยีฯ

Icon
VARCHAR(50)
Icon Class เช่น fa-book
fa-laptop

Description
NVARCHAR(300)
คำอธิบายหมวดหมู่
บทความ IT

SortOrder
INT
ลำดับการแสดงผล
1

IsActive
BIT
ใช้งานอยู่หรือไม่
1

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\u

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-01-01



### KMS.t_KBArticle

> **[Transaction]** บทความ Knowledge Base รองรับหลายประเภท (SOP/FAQ/Guideline/Template/Policy) พร้อม Lifecycle Management

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
ArticleID
INT IDENTITY
รหัสบทความ (PK)
1
🔑 PK
ArticleCode
VARCHAR(20)
รหัสบทความ รูปแบบ KB-YYYY-NNN
KB-2566-001
◈ UQ
Title
NVARCHAR(500)
ชื่อบทความ
วิธี Upload ไฟล์

ContentHTML
NVARCHAR(MAX)
เนื้อหาบทความ (HTML Format)
<h2>...</h2>

Summary
NVARCHAR(1000)
บทสรุปสั้นสำหรับแสดงใน List
คู่มือการ Upload

CategoryID
INT
FK → KMS.m_KBCategory
2
🔗 FK
ArticleType
TINYINT
1=SOP 2=FAQ 3=Guideline 4=Template 5=Policy
2

Status
TINYINT
1=Draft 2=Review 3=Published 4=Archived
3

CurrentVersion
INT
Version บทความปัจจุบัน
1

PublishedDate
DATETIME
วันที่ Publish บทความ
2024-06-01

ExpiryDate
DATETIME
วันหมดอายุบทความ (NULL=ไม่มีกำหนด)
NULL

ReviewDueDate
DATETIME
วันที่ต้อง Review ใหม่
2025-06-01

ViewCount
INT
จำนวนครั้งที่ดูบทความ
125

LikeCount
INT
จำนวน Like
8

IsOfficialPolicy
BIT
เป็น Official Policy ขององค์กร
0

Tags
NVARCHAR(500)
Tags ค้นหา (Comma-separated)
IT,CMS,คู่มือ

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\u

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-05-15

ReviewedBy
NVARCHAR(100)
ADUserName ผู้ Review
DOMAIN\mgr



### KMS.t_KBVersion

> **[Transaction]** Version History ของบทความ KB เก็บ Snapshot เนื้อหาทุกครั้งที่แก้ไข

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
VersionID
INT IDENTITY
รหัส Version (PK)
1
🔑 PK
ArticleID
INT
FK → KMS.t_KBArticle
3
🔗 FK
VersionNumber
INT
หมายเลข Version
2

ContentSnapshot
NVARCHAR(MAX)
Snapshot เนื้อหาทั้งหมด
<h2>...</h2>

ChangeDescription
NVARCHAR(500)
สรุปการเปลี่ยนแปลง
เพิ่มหัวข้อ 3

CreatedBy
NVARCHAR(100)
ADUserName ผู้แก้ไข
DOMAIN\u

CreatedDate
DATETIME
วันเวลาที่บันทึก Version
2024-06-10



### KMS.t_KBArticleLink

> **[Transaction]** เชื่อม Article กับ Entity อื่นๆ (Folder/Task/Program/Law/Project) แบบ Many-to-Many

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
LinkID
INT IDENTITY
รหัส Link (PK)
1
🔑 PK
ArticleID
INT
FK → KMS.t_KBArticle
3
🔗 FK
EntityType
TINYINT
1=Folder 2=Task 3=Program 4=Law 5=Project
2

EntityID
INT
ID ของ Entity ที่เชื่อม
15

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง Link
DOMAIN\u

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-06-01



### KMS.t_Announcement

> **[Transaction]** ประกาศ/แจ้งเวียนในองค์กร รองรับ Priority, Popup, Mandatory Read, ส่ง Email

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
AnnouncementID
INT IDENTITY
รหัสประกาศ (PK)
1
🔑 PK
AnnouncementCode
VARCHAR(20)
รหัสประกาศ รูปแบบ ANN-YYYY-NNN
ANN-2566-001
◈ UQ
Title
NVARCHAR(500)
หัวข้อประกาศ
แจ้งระบบ CMS

ContentHTML
NVARCHAR(MAX)
เนื้อหาประกาศ (HTML)
<p>...</p>

AttachmentPath
NVARCHAR(500)
Path ไฟล์แนบ (ถ้ามี)
NULL

Priority
TINYINT
1=Critical 2=High 3=Medium 4=Low
3

Status
TINYINT
1=Draft  2=Active  3=Expired
2

StartDate
DATETIME
วันที่เริ่มแสดงประกาศ
2024-06-01

EndDate
DATETIME
วันที่หยุดแสดงประกาศ
2024-06-30

TargetAll
BIT
ส่งให้พนักงานทุกคนในองค์กร
0

ShowPopup
BIT
แสดง Popup เมื่อ Login
1

MandatoryRead
BIT
ต้องกดยืนยันการอ่าน
0

SendEmail
BIT
ส่ง Email แจ้งผู้รับด้วย
0

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\u

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-05-28

PublishedBy
NVARCHAR(100)
ADUserName ผู้ Publish
DOMAIN\mgr



### KMS.t_AnnouncementTarget

> **[Transaction]** กลุ่มเป้าหมายของประกาศ กำหนดได้ทีละ OrgUnit/Role/User/Group

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
TargetID
INT IDENTITY
รหัส Target (PK)
1
🔑 PK
AnnouncementID
INT
FK → KMS.t_Announcement
3
🔗 FK
TargetType
TINYINT
1=OrgUnit 2=Role 3=User 4=Group
1

TargetEntityID
INT
ID ตาม TargetType
5



### KMS.l_AnnouncementReadLog

> **[Logging]** Log การอ่านและยืนยันประกาศของแต่ละ User (ใช้ตรวจสอบ Mandatory Read)

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
LogID
INT IDENTITY
รหัส Log (PK)
1
🔑 PK
AnnouncementID
INT
FK → KMS.t_Announcement
3
🔗 FK
UserID
INT
UserID ที่อ่าน
5

ADUserName
NVARCHAR(100)
Snapshot ADUserName
DOMAIN\u

ViewedDate
DATETIME
วันเวลาที่เปิดอ่าน
2024-06-05

IsAcknowledged
BIT
กดยืนยันรับทราบแล้ว
1

AcknowledgedDate
DATETIME
วันเวลาที่กดยืนยัน
2024-06-05



---

## WFE — Workflow Engine
Module: MOD-008  |  Workflow Process Engine — Version Freeze, Multi-step Approval, SLA Timer (ไม่นับวันหยุด), Escalation, Delegation


### WFE.m_WorkflowDefinition

> **[Master Data]** คำนิยาม Workflow (Template) — Version Freeze: Instance ถูก Snapshot DefinitionJSON ณ เวลาเริ่ม

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
WorkflowDefID
INT IDENTITY
รหัส Workflow Definition (PK)
1
🔑 PK
WorkflowCode
VARCHAR(20)
รหัส Workflow
WF-DOC-001
◈ UQ
WorkflowName
NVARCHAR(200)
ชื่อ Workflow
อนุมัติเอกสาร

Category
TINYINT
1=DocApproval 2=Task 3=HR 4=IT 5=Custom
1

Status
TINYINT
1=Draft  2=Active  3=Inactive
2

DefinitionJSON
NVARCHAR(MAX)
Serialized Node Graph (Steps+Transitions)
JSON...

Version
INT
เวอร์ชัน Definition ปัจจุบัน
1

IsTemplate
BIT
เป็น Template สำหรับสร้าง WF ใหม่
1

TriggerType
TINYINT
1=Manual  2=Auto-File  3=Scheduled
1

Description
NVARCHAR(500)
คำอธิบาย Workflow
2 ขั้นตอน

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\adm

ApprovedBy
NVARCHAR(100)
ADUserName ผู้อนุมัติ Definition
DOMAIN\mgr

ApprovedDate
DATETIME
วันที่อนุมัติ Definition
2024-01-15



### WFE.m_WorkflowStep

> **[Master Data]** ขั้นตอน (Step) ของ Workflow Definition — กำหนด Assignee, SLA, Escalation, Condition

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
StepID
INT IDENTITY
รหัส Step (PK)
1
🔑 PK
WorkflowDefID
INT
FK → WFE.m_WorkflowDefinition
1
🔗 FK
StepCode
VARCHAR(20)
รหัส Step
STEP-001
◈ UQ
StepName
NVARCHAR(200)
ชื่อ Step
Sr.Officer Review

StepType
TINYINT
1=Approval 2=Review 3=Notification 4=Action 5=Decision
1

AssigneeType
TINYINT
1=User 2=Role 3=OrgUnit 4=Manager
2

AssigneeID
INT
ID ตาม AssigneeType
2

SLAHours
INT
SLA เป็นชั่วโมง (0=ไม่กำหนด)
24

EscalationHours
INT
Escalate หลังจากชั่วโมง
48

EscalateToUserID
INT
FK → User ที่ Escalate ไป
5
🔗 FK
Instructions
NVARCHAR(MAX)
คำแนะนำสำหรับผู้ดำเนินการ
กรุณาตรวจสอบ

ConditionExpression
NVARCHAR(500)
Expression Routing เช่น Amount>100000
NULL

AllowReject
BIT
อนุญาตให้ Reject ได้
1

AllowDelegate
BIT
อนุญาตให้ Delegate ได้
0

RequireComment
BIT
บังคับใส่ Comment ก่อน Action
0

SortOrder
INT
ลำดับ Step
1



### WFE.t_WorkflowInstance

> **[Transaction]** Instance ของ Workflow Runtime — Freeze DefinitionJSON เมื่อเริ่ม เพื่อไม่ให้กระทบ In-flight Instance

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
InstanceID
INT IDENTITY
รหัส Instance (PK)
1
🔑 PK
InstanceCode
VARCHAR(30)
รหัส Instance
WFI-2566-00001
◈ UQ
WorkflowDefID
INT
FK → WFE.m_WorkflowDefinition
1
🔗 FK
FrozenDefVersion
INT
Version ที่ Freeze ตอนเริ่ม
1

FrozenDefJSON
NVARCHAR(MAX)
Snapshot JSON ของ Definition
JSON...

Status
TINYINT
1=Running 2=Completed 3=Rejected 4=Cancelled 5=Faulted
1

CurrentStepID
INT
FK → WFE.t_WorkflowStepInstance (Step ปัจจุบัน)
2
🔗 FK
TriggerByUserID
INT
UserID ที่เริ่ม Workflow
3
🔗 FK
LinkedFileID
INT
File ที่เชื่อมกับ Workflow
NULL

LinkedTaskID
INT
Task ที่เชื่อมกับ Workflow
NULL

Subject
NVARCHAR(500)
หัวเรื่อง/คำอธิบาย
ขออนุมัติเอกสาร

ContextDataJSON
NVARCHAR(MAX)
JSON Data ที่ส่งผ่าน Workflow
JSON...

StartDate
DATETIME
วันเวลาที่เริ่ม Workflow
2024-06-01

CompletedDate
DATETIME
วันเวลาที่สิ้นสุด
NULL

DueDate
DATETIME
Deadline รวมของ Workflow
2024-06-05

CompletionPct
DECIMAL(5,2)
เปอร์เซ็นต์ความสำเร็จ
50.00

FaultMessage
NVARCHAR(MAX)
Error ถ้า Status=5 (Faulted)
NULL



### WFE.t_WorkflowStepInstance

> **[Transaction]** Step Instance ของ Workflow Runtime รองรับ Delegate และ Escalate

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
StepInstanceID
INT IDENTITY
รหัส Step Instance (PK)
1
🔑 PK
InstanceID
INT
FK → WFE.t_WorkflowInstance
1
🔗 FK
StepID
INT
FK → WFE.m_WorkflowStep
2
🔗 FK
StepOrder
INT
ลำดับ Step ใน Instance
1

AssignedUserID
INT
User ที่รับผิดชอบ Step นี้
5
🔗 FK
AssignedBy
NVARCHAR(100)
ADUserName ผู้มอบหมาย
DOMAIN\sys

Status
TINYINT
1=Pending 2=InProgress 3=Approved 4=Rejected 5=Delegated 6=Escalated
1

StartedDate
DATETIME
วันเวลาที่เริ่ม Step
2024-06-01

DueDate
DATETIME
กำหนดเสร็จ (คำนวณจาก SLA + ปฏิทินวันทำงาน)
2024-06-03

CompletedDate
DATETIME
วันเวลาที่ดำเนินการเสร็จ
NULL

Decision
VARCHAR(50)
Approved / Rejected / Delegate
Approved

Comments
NVARCHAR(MAX)
ความเห็นประกอบ Decision
เห็นด้วย

IsEscalated
BIT
ถูก Escalate เพราะเกิน SLA
0

EscalatedDate
DATETIME
วันเวลาที่ Escalate
NULL

DelegatedToID
INT
UserID ที่ Delegate ให้
NULL
🔗 FK
CompletedBy
NVARCHAR(100)
ADUserName ผู้ดำเนินการ
DOMAIN\mgr



### WFE.s_SystemCalendar

> **[Static/Template]** ปฏิทินวันทำงาน สำหรับคำนวณ SLA Due Date ที่ไม่นับวันหยุดนักขัตฤกษ์และ Weekend

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
CalendarDate
DATE
วันที่ (PK)
2024-04-13
?? PK
IsWorkingDay
BIT
1=วันทำงาน  0=วันหยุด
0

IsHoliday
BIT
เป็นวันหยุดนักขัตฤกษ์
1

HolidayName
NVARCHAR(100)
ชื่อวันหยุด
วันสงกรานต์

HolidayType
TINYINT
1=National  2=Royal  3=Company
1

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
SYSTEM

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-01-01



---

## TMS — Task Management System
Module: MOD-009 ถึง MOD-012  |  จัดการ Task และ Sub-task (≤3 ระดับ), Todo List, Notes/Comments, Work Instructions


### TMS.t_TaskItem

> **[Transaction]** Task หลัก รองรับ Sub-task สูงสุด 3 ระดับ (BR-009) — เชื่อมกับ Workflow, Project, Kanban Board

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
TaskID
INT IDENTITY
รหัส Task (PK)
1
🔑 PK
TaskCode
VARCHAR(30)
รหัส Task รูปแบบ TASK-YYYY-NNNNN
TASK-2566-00001
◈ UQ
Title
NVARCHAR(500)
ชื่อ Task
พัฒนา Module

DescriptionHTML
NVARCHAR(MAX)
รายละเอียด Task (HTML)
<p>...</p>

Priority
TINYINT
1=Critical 2=High 3=Medium 4=Low
2

Status
TINYINT
1=Backlog 2=Todo 3=InProgress 4=Done 5=Cancelled
3

ParentTaskID
INT
FK → Self (NULL=Root Task)
NULL
🔗 FK
SubtaskDepth
TINYINT
ระดับ Sub-task 0-3 (Max=3)
0

CompletionPct
DECIMAL(5,2)
เปอร์เซ็นต์ความสำเร็จ 0-100
60.00

AssignedToUserID
INT
FK → EMS.m_UserAccount ผู้รับผิดชอบ
3
🔗 FK
CreatedByUserID
INT
FK → EMS.m_UserAccount ผู้สร้าง
1
🔗 FK
DueDate
DATETIME
กำหนดเสร็จ
2024-06-30

StartDate
DATETIME
วันที่เริ่มต้น
2024-06-01

EstimatedHours
DECIMAL(8,2)
ชั่วโมงประมาณ
40.00

ActualHours
DECIMAL(8,2)
ชั่วโมงจริงที่ใช้
35.50

WorkflowStepInstanceID
INT
FK → WFE (ถ้า Task มาจาก WF)
NULL
🔗 FK
ProjectID
INT
FK → PMS.t_Project
NULL
🔗 FK
Tags
NVARCHAR(500)
Tags (Comma-separated)
IT,Dev

Category
NVARCHAR(100)
หมวดหมู่ Task
Development

IsDeleted
BIT
Soft Delete Flag
0

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\u

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-05-01



### TMS.t_TaskComment

> **[Transaction]** Comment ใน Task รองรับ Thread Reply — ไม่จำกัดระดับ

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
CommentID
INT IDENTITY
รหัส Comment (PK)
1
🔑 PK
TaskID
INT
FK → TMS.t_TaskItem
3
🔗 FK
ParentCommentID
INT
FK → Self สำหรับ Thread Reply
NULL
🔗 FK
ContentHTML
NVARCHAR(MAX)
เนื้อหา Comment (HTML)
<p>...</p>

ThreadDepth
TINYINT
ระดับ Thread (0=Root)
0

IsEdited
BIT
ถูกแก้ไขหรือไม่
0

IsDeleted
BIT
Soft Delete Flag
0

CreatedBy
NVARCHAR(100)
ADUserName ผู้ Comment
DOMAIN\u

CreatedDate
DATETIME
วันเวลาที่ Comment
2024-06-10

EditedDate
DATETIME
วันเวลาที่แก้ไขล่าสุด
NULL



### TMS.t_TaskAttachment

> **[Transaction]** ไฟล์แนบใน Task รองรับ 2 รูปแบบ: Link จาก DMS หรือ Upload โดยตรง

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
AttachmentID
INT IDENTITY
รหัส Attachment (PK)
1
🔑 PK
TaskID
INT
FK → TMS.t_TaskItem
3
🔗 FK
AttachType
TINYINT
1=DMSFile (Link)  2=DirectUpload
1

DMSFileID
INT
FK → DMS.t_FileDocument (Type=1)
5
🔗 FK
FileName
NVARCHAR(300)
ชื่อไฟล์ (ถ้า Type=2)
รายงาน.pdf

FilePath
NVARCHAR(500)
Path ไฟล์ (ถ้า Type=2)
\\SRV\..

FileSizeBytes
BIGINT
ขนาดไฟล์ (Bytes)
1048576

CreatedBy
NVARCHAR(100)
ADUserName ผู้แนบไฟล์
DOMAIN\u

CreatedDate
DATETIME
วันเวลาที่แนบ
2024-06-10



### TMS.t_TodoItem

> **[Transaction]** Todo List ส่วนตัวและทีม รองรับ Recurring Task ลิงก์กับ Task และ Project ได้

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
TodoID
INT IDENTITY
รหัส Todo (PK)
1
🔑 PK
Title
NVARCHAR(500)
ชื่อ Todo
ส่งรายงาน

ListType
TINYINT
1=Personal  2=Team  3=Project
1

OwnedByUserID
INT
FK → EMS.m_UserAccount เจ้าของ
3
🔗 FK
SharedOrgUnitID
INT
FK → EMS.m_OrgUnit (ถ้า Team)
NULL
🔗 FK
LinkedProjectID
INT
FK → PMS.t_Project (ถ้ามี)
NULL
🔗 FK
LinkedTaskID
INT
FK → TMS.t_TaskItem (ถ้ามี)
NULL
🔗 FK
Priority
TINYINT
1=Critical 2=High 3=Medium 4=Low
2

IsCompleted
BIT
เสร็จแล้ว
0

DueDate
DATETIME
กำหนดเสร็จ
2024-06-30

Category
NVARCHAR(100)
หมวดหมู่
งานประจำ

SortOrder
INT
ลำดับแสดงผล
1

IsRecurring
BIT
เกิดซ้ำอัตโนมัติ
0

RecurringType
TINYINT
1=Daily  2=Weekly  3=Monthly
NULL

RecurringEndDate
DATETIME
วันสิ้นสุดการเกิดซ้ำ
NULL

IsDeleted
BIT
Soft Delete
0

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\u

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-06-01



### TMS.t_Note

> **[Transaction]** Notes และ Comments ทั่วไป เชื่อมได้กับ Task/Project/File/Workflow รองรับ Private Note

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
NoteID
INT IDENTITY
รหัส Note (PK)
1
🔑 PK
Title
NVARCHAR(300)
ชื่อ Note (Optional)
Meeting Note

ContentHTML
NVARCHAR(MAX)
เนื้อหา Note (HTML)
<p>...</p>

NoteType
TINYINT
1=General 2=Meeting 3=Decision 4=Risk
2

EntityType
TINYINT
1=Task 2=Project 3=File 4=WorkflowInstance
1

EntityID
INT
ID ของ Entity ที่เชื่อม
15

IsPinned
BIT
ปักหมุด Note
0

IsPrivate
BIT
Note ส่วนตัว (เห็นแค่เจ้าของ)
0

OwnedByUserID
INT
FK → EMS.m_UserAccount เจ้าของ
3
🔗 FK
Tags
NVARCHAR(300)
Tags (Comma-separated)
meeting,IT

IsDeleted
BIT
Soft Delete
0

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\u

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-06-01



### TMS.t_WorkInstruction

> **[Transaction]** คู่มือ/วิธีปฏิบัติงาน (Work Instruction) เชื่อมกับ Task และ Knowledge Base

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
WIID
INT IDENTITY
รหัส Work Instruction (PK)
1
🔑 PK
WICode
VARCHAR(20)
รหัส WI รูปแบบ WI-YYYY-NNN
WI-2566-001
◈ UQ
Title
NVARCHAR(500)
ชื่อ Work Instruction
การตรวจสอบ กรมธรรม์

ContentHTML
NVARCHAR(MAX)
เนื้อหา (HTML)
<h2>...</h2>

WIType
TINYINT
1=Process  2=Procedure  3=Work Instruction
3

OrgUnitID
INT
FK → EMS.m_OrgUnit
5
🔗 FK
Status
TINYINT
1=Draft  2=Active  3=Obsolete
2

Version
INT
เวอร์ชัน WI
1

EffectiveDate
DATE
วันที่มีผลบังคับใช้
2024-01-01

ReviewDueDate
DATE
วันที่ต้อง Review ใหม่
2025-01-01

Tags
NVARCHAR(300)
Tags
ประกันชีวิต

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\u

ApprovedBy
NVARCHAR(100)
ADUserName ผู้อนุมัติ
DOMAIN\mgr

ApprovedDate
DATETIME
วันที่อนุมัติ
2024-01-10



---

## PMS — Project Management System
Module: MOD-013, MOD-014, MOD-015  |  จัดการโปรเจ็ค, WBS, Milestone, Resource Allocation (ตรวจ Overlap 100%), Timeline & Deadline


### PMS.m_ResourceCatalog

> **[Master Data]** Catalog ทรัพยากรทั้งหมด (บุคลากร, Hardware, Software License, Budget) ใช้ใน Resource Allocation

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
ResourceID
INT IDENTITY
รหัสทรัพยากร (PK)
1
🔑 PK
ResourceCode
VARCHAR(20)
รหัสทรัพยากร
RES-001
◈ UQ
ResourceName
NVARCHAR(200)
ชื่อทรัพยากร
สมชาย รักษ์ไทย

ResourceType
TINYINT
1=Human 2=Hardware 3=SW License 4=Budget
1

UserAccountID
INT
FK → EMS.m_UserAccount (ถ้า Human)
3
🔗 FK
CostPerHour
DECIMAL(10,2)
ต้นทุนต่อชั่วโมง
500.00

Currency
VARCHAR(5)
สกุลเงิน
THB

IsActive
BIT
ใช้งานอยู่หรือไม่
1

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\u

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-01-01



### PMS.t_Project

> **[Transaction]** โปรเจ็คหลัก รองรับ IT/Business/Compliance/Innovation พร้อม Budget Tracking และ Health Status

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
ProjectID
INT IDENTITY
รหัสโปรเจ็ค (PK)
1
🔑 PK
ProjectCode
VARCHAR(30)
รหัสโปรเจ็ค
PRJ-2566-001
◈ UQ
ProjectName
NVARCHAR(300)
ชื่อโปรเจ็ค
CMS System

Description
NVARCHAR(MAX)
รายละเอียดโปรเจ็ค
-

ProjectType
TINYINT
1=IT 2=Business 3=Compliance 4=Innovation
1

Status
TINYINT
1=Planning 2=Active 3=OnHold 4=Completed 5=Cancelled
2

Priority
TINYINT
1=Critical 2=High 3=Medium 4=Low
1

HealthStatus
TINYINT
1=OnTrack  2=AtRisk  3=OffTrack
1

ProjectManagerUserID
INT
FK → EMS.m_UserAccount PM
5
🔗 FK
SponsorUserID
INT
FK → EMS.m_UserAccount Sponsor
1
🔗 FK
OrgUnitID
INT
FK → EMS.m_OrgUnit
5
🔗 FK
StartDate
DATETIME
วันเริ่มโปรเจ็ค
2023-10-01

EndDate
DATETIME
วันสิ้นสุดที่วางแผน
2024-09-30

BudgetEstimate
DECIMAL(18,2)
งบประมาณประมาณ (THB)
5000000.00

BudgetActual
DECIMAL(18,2)
งบประมาณจริงที่ใช้
2500000.00

CompletionPct
DECIMAL(5,2)
เปอร์เซ็นต์ความสำเร็จ
50.00

LinkedFolderID
INT
FK → DMS.t_Folder โฟลเดอร์เอกสาร
3
🔗 FK
CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\u

CreatedDate
DATETIME
วันเวลาที่สร้าง
2023-10-01



### PMS.t_ProjectMilestone

> **[Transaction]** Milestone ของโปรเจ็ค สำหรับติดตามความคืบหน้าสำคัญ

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
MilestoneID
INT IDENTITY
รหัส Milestone (PK)
1
🔑 PK
ProjectID
INT
FK → PMS.t_Project
1
🔗 FK
MilestoneName
NVARCHAR(200)
ชื่อ Milestone
Design Complete

Description
NVARCHAR(MAX)
รายละเอียด
-

TargetDate
DATETIME
กำหนดบรรลุ Milestone
2023-12-31

CompletedDate
DATETIME
วันที่บรรลุ Milestone จริง
NULL

Status
TINYINT
1=Planned 2=Achieved 3=Missed 4=Cancelled
2

CompletionCriteria
NVARCHAR(500)
เกณฑ์การบรรลุ Milestone
ส่งมอบ SDD

SortOrder
INT
ลำดับ Milestone
1

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\u

CreatedDate
DATETIME
วันเวลาที่สร้าง
2023-10-01



### PMS.t_ProjectTask

> **[Transaction]** WBS (Work Breakdown Structure) ของโปรเจ็ค เชื่อมกับ TMS.t_TaskItem และติดตาม Critical Path

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
ProjectTaskID
INT IDENTITY
รหัส Project Task (PK)
1
🔑 PK
ProjectID
INT
FK → PMS.t_Project
1
🔗 FK
MilestoneID
INT
FK → PMS.t_ProjectMilestone
2
🔗 FK
ParentProjectTaskID
INT
FK → Self (WBS Hierarchy)
NULL
🔗 FK
LinkedTaskID
INT
FK → TMS.t_TaskItem
15
🔗 FK
WBSCode
VARCHAR(20)
รหัส WBS เช่น 1.1.2
1.1.2
◈ UQ
DependencyType
TINYINT
1=FS 2=SS 3=FF 4=SF (Finish-Start ฯลฯ)
1

DependsOnPTID
INT
FK → Self (Predecessor Task)
NULL
🔗 FK
PlannedStart
DATETIME
วันเริ่มที่วางแผน
2024-01-01

PlannedEnd
DATETIME
วันสิ้นสุดที่วางแผน
2024-03-31

ActualStart
DATETIME
วันเริ่มจริง
2024-01-05

ActualEnd
DATETIME
วันสิ้นสุดจริง
NULL

CompletionPct
DECIMAL(5,2)
เปอร์เซ็นต์ความสำเร็จ
75.00

IsCriticalPath
BIT
อยู่ใน Critical Path หรือไม่
0

SortOrder
INT
ลำดับแสดงผลใน Gantt Chart
1



### PMS.t_ResourceAllocation

> **[Transaction]** การจัดสรรทรัพยากร — ตรวจสอบ Overlap ไม่เกิน 100% ผ่าน usp_PMS_CheckResourceOverlap

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
AllocationID
INT IDENTITY
รหัส Allocation (PK)
1
🔑 PK
ResourceID
INT
FK → PMS.m_ResourceCatalog
3
🔗 FK
ProjectID
INT
FK → PMS.t_Project
1
🔗 FK
TaskID
INT
FK → TMS.t_TaskItem
NULL
🔗 FK
AllocationPercent
DECIMAL(5,2)
% Allocation (0-100)
80.00

StartDate
DATETIME
วันเริ่ม Allocation
2024-01-01

EndDate
DATETIME
วันสิ้นสุด Allocation
2024-03-31

HoursAllocated
DECIMAL(8,2)
ชั่วโมงที่จัดสรร
320.00

HoursActual
DECIMAL(8,2)
ชั่วโมงจริงที่ใช้
280.00

IsApproved
BIT
ผ่านการอนุมัติแล้ว
1

ApprovedBy
NVARCHAR(100)
ADUserName ผู้อนุมัติ
DOMAIN\mgr

Notes
NVARCHAR(500)
หมายเหตุ
-

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\u

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-01-01



### PMS.t_ProjectMember

> **[Transaction]** รายชื่อสมาชิกในโปรเจ็ค พร้อมบทบาท

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
MemberID
INT IDENTITY
รหัส Member (PK)
1
🔑 PK
ProjectID
INT
FK → PMS.t_Project
1
🔗 FK
UserID
INT
FK → EMS.m_UserAccount
3
🔗 FK
ProjectRole
TINYINT
1=PM 2=TeamLead 3=Developer 4=Tester 5=Stakeholder
3

JoinedDate
DATE
วันที่เข้าร่วมโปรเจ็ค
2023-10-01

LeftDate
DATE
วันที่ออกจากโปรเจ็ค (NULL=ยังอยู่)
NULL

IsActive
BIT
ยังเป็นสมาชิกอยู่
1

AddedBy
NVARCHAR(100)
ADUserName ผู้เพิ่มสมาชิก
DOMAIN\mgr



### PMS.r_ProjectSummary

> **[Report]** ข้อมูลสรุปโปรเจ็คสำหรับ Dashboard — Refresh ทุกคืน (Scheduled Job)

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
SummaryID
INT IDENTITY
รหัส Summary (PK)
1
🔑 PK
ProjectID
INT
FK → PMS.t_Project (1:1)
1
🔗◈ FK+UQ
TotalTasks
INT
จำนวน Task ทั้งหมด
25

CompletedTasks
INT
จำนวน Task ที่เสร็จแล้ว
15

OverdueTasks
INT
จำนวน Task ที่เกินกำหนด
2

TotalBudget
DECIMAL(18,2)
งบประมาณรวม
5000000.00

UsedBudget
DECIMAL(18,2)
งบประมาณที่ใช้แล้ว
2500000.00

BudgetUtilizePct
DECIMAL(5,2)
% การใช้งบ
50.00

TotalResources
INT
จำนวนทรัพยากรทั้งหมด
5

LastRefreshDate
DATETIME
วันเวลาที่ Refresh ล่าสุด
2024-06-15



---

## ITL — IT Landscape System
Module: MOD-017, MOD-018, MOD-019  |  บริหารจัดการ Programs List, IT Landscape Mapping, Integration Relationships, Data Dictionary ของแต่ละระบบ


### ITL.m_ProgramSystem

> **[Master Data]** รายการระบบ/โปรแกรมทั้งหมดในองค์กร (Programs List + IT Landscape) พร้อม SLA และ Business/Technical Owner

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
ProgramID
INT IDENTITY
รหัสระบบ (PK)
1
🔑 PK
ProgramCode
VARCHAR(20)
รหัสระบบ
SYS-001
◈ UQ
ProgramName
NVARCHAR(300)
ชื่อระบบ
ระบบ CMS

ShortName
VARCHAR(50)
ชื่อย่อ
CMS

Description
NVARCHAR(MAX)
รายละเอียดระบบ
-

SystemType
TINYINT
1=Core 2=Support 3=External 4=Integration
1

Platform
TINYINT
1=Web 2=Desktop 3=Mobile 4=API 5=Batch
1

TechnologyStack
NVARCHAR(300)
เทคโนโลยีที่ใช้
.NET, SQL Server

Version
VARCHAR(50)
เวอร์ชัน
1.0

Vendor
NVARCHAR(200)
ผู้พัฒนา/ผู้จำหน่าย
Internal

Status
TINYINT
1=Active 2=Maintenance 3=Decommission 4=Planning
1

BusinessOwnerUserID
INT
FK → EMS.m_UserAccount Business Owner
1
🔗 FK
TechnicalOwnerUserID
INT
FK → EMS.m_UserAccount Technical Owner
3
🔗 FK
AccessURL
NVARCHAR(500)
URL เข้าใช้งาน
http://cms.co.th

AppPath
NVARCHAR(500)
Path ไฟล์ Application
NULL

ImplementationDate
DATE
วันที่ Implement
2024-01-01

LastUpdateDate
DATE
วันที่ Update ล่าสุด
2024-06-01

MaintenanceWindow
NVARCHAR(200)
ช่วงเวลา Maintenance
เสาร์ 22:00

DataClassification
TINYINT
ระดับความลับข้อมูล 1-4
3

SLAUptime
DECIMAL(5,2)
% Uptime Target เช่น 99.50
99.50

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\u

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-01-01



### ITL.t_ProgramRelationship

> **[Transaction]** ความสัมพันธ์ระหว่างระบบ (IT Landscape Mapping) เช่น DataFeed, API, SharedDB, FileTransfer

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
RelationshipID
INT IDENTITY
รหัส Relationship (PK)
1
🔑 PK
SourceProgramID
INT
FK → ITL.m_ProgramSystem ต้นทาง
1
🔗 FK
TargetProgramID
INT
FK → ITL.m_ProgramSystem ปลายทาง
2
🔗 FK
RelationshipType
TINYINT
1=DataFeed 2=API 3=SharedDB 4=FileTransfer 5=Manual
1

FlowDirection
TINYINT
1=OneWay  2=TwoWay
2

DataExchanged
NVARCHAR(500)
ข้อมูลที่แลกเปลี่ยน
กรมธรรม์, เบี้ย

Frequency
NVARCHAR(100)
ความถี่
Real-time

Protocol
VARCHAR(50)
Protocol เช่น REST, LDAP, SFTP
REST API

IsActive
BIT
ยังใช้งานอยู่
1

Version
INT
เวอร์ชัน Integration
1

Description
NVARCHAR(500)
คำอธิบาย
-

ChangeApprovedBy
NVARCHAR(100)
ADUserName ผู้อนุมัติการเปลี่ยน
DOMAIN\mgr

ChangeApprovedDate
DATETIME
วันที่อนุมัติ
2024-01-15

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\u

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-01-01



### ITL.t_SystemDataDictionary

> **[Transaction]** Data Dictionary ของแต่ละระบบ ระดับ Column — ใช้ใน MOD-019 พร้อม PII Flag สำหรับ PDPA

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
DictID
INT IDENTITY
รหัส Data Dictionary (PK)
1
🔑 PK
ProgramID
INT
FK → ITL.m_ProgramSystem
1
🔗 FK
DatabaseName
NVARCHAR(100)
ชื่อ Database
CMS_DB

SchemaName
NVARCHAR(100)
ชื่อ Schema
DMS

TableName
NVARCHAR(200)
ชื่อตาราง
t_Folder

ColumnName
NVARCHAR(200)
ชื่อ Column
FolderID

DataType
NVARCHAR(50)
Data Type เช่น INT, NVARCHAR
INT

MaxLength
INT
ความยาวสูงสุด (NULL=ไม่กำหนด)
NULL

IsNullable
BIT
อนุญาตให้เป็น NULL
0

IsPrimaryKey
BIT
เป็น Primary Key
1

IsForeignKey
BIT
เป็น Foreign Key
0

DefaultValue
NVARCHAR(200)
ค่า Default
GETDATE()

DescriptionTH
NVARCHAR(500)
คำอธิบายภาษาไทย
รหัส Folder

DescriptionEN
NVARCHAR(500)
คำอธิบายภาษาอังกฤษ
Folder ID

Example
NVARCHAR(200)
ตัวอย่างข้อมูล
1

BusinessTerm
NVARCHAR(200)
ชื่อทางธุรกิจ
รหัสโฟลเดอร์

IsPII
BIT
PDPA: เป็นข้อมูลส่วนบุคคล
0
🔒 PII
IsActive
BIT
ใช้งานอยู่
1

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\u

UpdatedDate
DATETIME
วันเวลาที่แก้ไขล่าสุด
2024-06-01



### ITL.t_ProgramIncident

> **[Transaction]** บันทึก Incident ของระบบ (Downtime, Bug, Security Issue) สำหรับ IT Landscape Monitoring

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
IncidentID
INT IDENTITY
รหัส Incident (PK)
1
🔑 PK
IncidentCode
VARCHAR(20)
รหัส Incident
INC-2566-001
◈ UQ
ProgramID
INT
FK → ITL.m_ProgramSystem
1
🔗 FK
IncidentType
TINYINT
1=Downtime 2=Performance 3=Security 4=DataIssue 5=Other
1

Severity
TINYINT
1=Critical 2=High 3=Medium 4=Low
2

Title
NVARCHAR(300)
หัวข้อ Incident
ระบบล่ม

Description
NVARCHAR(MAX)
รายละเอียด
-

OccurredDate
DATETIME
วันเวลาที่เกิด Incident
2024-06-01

ResolvedDate
DATETIME
วันเวลาที่แก้ไขเสร็จ
2024-06-01

DowntimeMinutes
INT
ระยะเวลา Downtime (นาที)
45

RootCause
NVARCHAR(MAX)
สาเหตุที่แท้จริง
-

Resolution
NVARCHAR(MAX)
วิธีแก้ไข
-

Status
TINYINT
1=Open 2=InProgress 3=Resolved 4=Closed
3

AssignedTo
NVARCHAR(100)
ADUserName ผู้รับผิดชอบ
DOMAIN\it

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\u

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-06-01



---

## ATS — Audit Trail System
Module: MOD-021  |  ระบบ Audit Trail ตาม OIC Circular 2566 — Append-Only, เก็บ ≥ 5 ปี, ไม่มี FK (Log Independence), ADUserName เป็น Snapshot


### ATS.l_AuditLog

> **[Logging]** Audit Log หลัก บันทึกทุก Transaction — BIGINT PK, ไม่มี FK ไป UserAccount เพื่อ Log Independence ตาม OIC

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
AuditID
BIGINT IDENTITY
รหัส Audit (PK) BIGINT เพราะ Volume สูงมาก
1
🔑 PK
AuditDate
DATETIME
วันเวลา Audit
2024-06-15 10:30

UserID
INT
Snapshot UserID ณ เวลา Audit (ไม่ใช่ FK)
3

ADUserName
NVARCHAR(100)

> **[PII]** Snapshot ADUserName

DOMAIN\somchai
🔒 PII
DisplayName
NVARCHAR(200)
Snapshot ชื่อ User ณ เวลา Audit
สมชาย รักษ์ไทย

SchemaName
VARCHAR(10)
Schema ที่เกี่ยวข้อง เช่น DMS, WFE
DMS

Module
VARCHAR(50)
Module ที่เกี่ยวข้อง เช่น MOD-003
MOD-003

Action
VARCHAR(50)
CREATE/UPDATE/DELETE/VIEW/DOWNLOAD/APPROVE/REJECT/LOGIN
UPLOAD

EntityType
VARCHAR(100)
ชื่อ Table/Entity ที่เปลี่ยนแปลง
FileDocument

EntityID
NVARCHAR(50)
PK ของ Record ที่เปลี่ยนแปลง
15

EntityCode
NVARCHAR(100)
Code ของ Record เช่น TASK-2566-001
TASK-2566-001

OldValueJSON
NVARCHAR(MAX)
JSON ข้อมูลก่อนเปลี่ยน (UPDATE/DELETE)
{"status":1}

NewValueJSON
NVARCHAR(MAX)
JSON ข้อมูลหลังเปลี่ยน (CREATE/UPDATE)
{"status":2}

IPAddress
VARCHAR(45)

> **[PII]** IPv4 หรือ IPv6 ของ Client

192.168.1.10
🔒 PII
MachineName
NVARCHAR(100)
Hostname ของ Client
PC-IT-001

SessionID
NVARCHAR(100)
Session ID สำหรับ Session Tracking
sess_abc123

Result
TINYINT
1=Success  2=Failure  3=Denied
1

ErrorMessage
NVARCHAR(500)
ข้อความ Error (ถ้า Result=2 หรือ 3)
NULL

DurationMs
INT
ระยะเวลาดำเนินการ (Milliseconds)
45



### ATS.l_LoginLog

> **[Logging]** Log การ Login/Logout แยกจาก AuditLog เพื่อ Performance และ Security Monitoring

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
LoginLogID
BIGINT IDENTITY
รหัส Log (PK)
1
🔑 PK
ADUserName
NVARCHAR(100)
Snapshot ADUserName
DOMAIN\u

UserID
INT
UserID (NULL ถ้า Login ล้มเหลว)
3

LoginDate
DATETIME
วันเวลาที่ Login
2024-06-15

LogoutDate
DATETIME
วันเวลาที่ Logout
2024-06-15

SessionDuration
INT
ระยะเวลา Session (นาที)
480

IPAddress
VARCHAR(45)

> **[PII]** IPv4/IPv6 ของ Client

192.168.1.1
🔒 PII
MachineName
NVARCHAR(100)
Hostname
PC-001

LoginResult
TINYINT
1=Success 2=Failed 3=Locked 4=Expired
1

FailReason
NVARCHAR(200)
เหตุผลที่ Login ไม่สำเร็จ
NULL



### ATS.l_ADSyncLog

> **[Logging]** Log การ Sync ข้อมูล User จาก Active Directory สำหรับ IT ตรวจสอบ

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
SyncLogID
INT IDENTITY
รหัส Sync Log (PK)
1
🔑 PK
SyncDate
DATETIME
วันเวลาที่ Sync
2024-06-15

UsersCreated
INT
จำนวน User ที่สร้างใหม่
2

UsersUpdated
INT
จำนวน User ที่อัพเดต
5

UsersDeactivated
INT
จำนวน User ที่ปิดการใช้งาน
0

ErrorCount
INT
จำนวน Error ที่เกิดขึ้น
0

ErrorDetails
NVARCHAR(MAX)
รายละเอียด Error (ถ้ามี)
NULL

IsSuccess
BIT
Sync สำเร็จหรือไม่
1

SyncedBy
NVARCHAR(100)
ADUserName ที่ Trigger Sync
SYSTEM

DurationMs
INT
ระยะเวลา Sync (Milliseconds)
3500



### ATS.r_AuditSummary

> **[Report]** สรุป Audit Activity รายวันต่อ Schema+Module — Refresh ทุกคืนสำหรับ Dashboard Compliance

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
SummaryID
INT IDENTITY
รหัส Summary (PK)
1
🔑 PK
SummaryDate
DATE
วันที่สรุป
2024-06-15

SchemaName
VARCHAR(10)
Schema
DMS

Module
VARCHAR(50)
Module
MOD-003

TotalActions
INT
จำนวน Action ทั้งหมด
1250

SuccessCount
INT
จำนวนที่สำเร็จ
1240

FailureCount
INT
จำนวนที่ล้มเหลว
8

DeniedCount
INT
จำนวนที่ถูกปฏิเสธสิทธิ์
2

UniqueUsers
INT
จำนวน User ที่ใช้งาน
35

LastRefreshDate
DATETIME
วันเวลาที่ Refresh ล่าสุด
2024-06-16



---

## COM — Common & Configuration
Module: MOD-022, MOD-023  |  ตาราง Common ใช้ร่วมกันทุก Module — System Config (Key-Value), Notification, API Token, User Group


### COM.c_SystemConfig

> **[Configuration]** ตั้งค่าระบบ Key-Value Admin แก้ไขได้ — รองรับ Encrypted Values และ Approval ก่อน Apply

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
ConfigID
INT IDENTITY
รหัส Config (PK)
1
🔑 PK
ConfigKey
VARCHAR(100)
Key ของ Config
MaxFileSizeMB
◈ UQ
ConfigValue
NVARCHAR(MAX)
ค่า Config (อาจเป็น JSON)
100

ConfigGroup
VARCHAR(50)
กลุ่ม: Storage/Security/Notif/SLA
Storage

DataType
VARCHAR(20)
string/int/decimal/bool/json/path
int

Description
NVARCHAR(500)
คำอธิบาย Config
ขนาดไฟล์สูงสุด

IsEncrypted
BIT
ค่าเป็น Encrypted หรือไม่
0

RequiresApproval
BIT
ต้องผ่าน Approval ก่อน Apply
0

IsActive
BIT
ยังใช้งานอยู่
1

SchemaScope
VARCHAR(10)
NULL=ทุก Schema หรือ DMS/WFE/TMS
NULL

UpdatedBy
NVARCHAR(100)
ADUserName ผู้แก้ไขล่าสุด
DOMAIN\admin

UpdatedDate
DATETIME
วันเวลาที่แก้ไขล่าสุด
2024-01-01



### COM.c_NotificationTemplate

> **[Configuration]** Template Email/In-App Notification สำหรับแต่ละ Event รองรับ Placeholder {{VariableName}}

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
TemplateID
INT IDENTITY
รหัส Template (PK)
1
🔑 PK
TemplateCode
VARCHAR(50)
รหัส Template
TASK_ASSIGNED
◈ UQ
TemplateName
NVARCHAR(200)
ชื่อ Template
Task ถูกมอบหมาย

NotifyType
TINYINT
1=Email  2=InApp  3=Both
3

Subject
NVARCHAR(300)
Subject ของ Email
มี Task ใหม่

BodyHTML
NVARCHAR(MAX)
HTML Template (มี Placeholder)
<p>{{Title}}</p>

TriggerEvent
VARCHAR(100)
Event ที่ Trigger Notification
Task.Assigned

SchemaScope
VARCHAR(10)
Schema ที่ใช้ Template นี้
TMS

IsActive
BIT
ใช้งานอยู่
1

UpdatedBy
NVARCHAR(100)
ADUserName ผู้แก้ไขล่าสุด
DOMAIN\adm

UpdatedDate
DATETIME
วันเวลาที่แก้ไขล่าสุด
2024-01-01



### COM.t_Notification

> **[Transaction]** In-App Notification ที่ส่งให้ User แต่ละคน — BIGINT PK เพราะ Volume สูง

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
NotificationID
BIGINT IDENTITY
รหัส Notification (PK) BIGINT
1
🔑 PK
RecipientUserID
INT
FK → EMS.m_UserAccount ผู้รับ
3
🔗 FK
TemplateID
INT
FK → COM.c_NotificationTemplate
1
🔗 FK
Title
NVARCHAR(300)
หัวข้อ Notification
มี Task ใหม่

Message
NVARCHAR(MAX)
เนื้อหา Notification
Task XXX...

NotifyType
TINYINT
1=Email  2=InApp  3=Both
2

EntityType
VARCHAR(50)
ประเภท Entity ที่เชื่อมโยง
TaskItem

EntityID
INT
ID ของ Entity
15

EntityCode
NVARCHAR(100)
Code ของ Entity
TASK-2566-001

Priority
TINYINT
1=Critical 2=High 3=Medium 4=Low
3

IsRead
BIT
อ่านแล้วหรือไม่
0

ReadDate
DATETIME
วันเวลาที่อ่าน
NULL

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-06-15

ExpiryDate
DATETIME
วันหมดอายุ Notification
NULL



### COM.m_UserGroup

> **[Master Data]** กลุ่ม User แบบ Custom ใช้ใน Permission, Share, Announcement Target

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
GroupID
INT IDENTITY
รหัส Group (PK)
1
🔑 PK
GroupName
NVARCHAR(200)
ชื่อ Group
CMS Project Team
◈ UQ
Description
NVARCHAR(500)
คำอธิบาย Group
ทีมพัฒนา CMS

OrgUnitID
INT
FK → EMS.m_OrgUnit สังกัด
5
🔗 FK
IsActive
BIT
ยังใช้งานอยู่
1

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\u

CreatedDate
DATETIME
วันเวลาที่สร้าง
2024-01-01



### COM.m_UserGroupMember

> **[Master Data]** สมาชิกของ User Group (Many-to-Many) — UQ Constraint ป้องกัน User ซ้ำใน Group เดียวกัน

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
MemberID
INT IDENTITY
รหัส Member (PK)
1
🔑 PK
GroupID
INT
FK → COM.m_UserGroup
1
🔗 FK
UserID
INT
FK → EMS.m_UserAccount
3
🔗 FK
AddedBy
NVARCHAR(100)
ADUserName ผู้เพิ่มสมาชิก
DOMAIN\adm

AddedDate
DATETIME
วันเวลาที่เพิ่มสมาชิก
2024-01-01



### COM.t_APIAccessToken

> **[Transaction]** API Token สำหรับ Integration Layer (MOD-023) — เก็บ Hash เท่านั้น ไม่เก็บ Plain Token

Field (Column)
Data Type
คำอธิบาย
ตัวอย่างข้อมูล
Key
TokenID
INT IDENTITY
รหัส Token (PK)
1
🔑 PK
UserID
INT
FK → EMS.m_UserAccount
1
🔗 FK
ClientName
NVARCHAR(200)
ชื่อ Client Application
POLSYS

TokenHash
VARCHAR(128)
SHA-256 Hash ของ Token
abc123...
◈ UQ
Scope
NVARCHAR(500)
Scopes ที่อนุญาต (Comma-separated)
read,write

IssuedDate
DATETIME
วันที่ออก Token
2024-01-01

ExpiryDate
DATETIME
วันหมดอายุ Token
2024-12-31

LastUsedDate
DATETIME
วันที่ใช้งานล่าสุด
2024-06-15

IsRevoked
BIT
ถูก Revoke แล้วหรือไม่
0

RevokedBy
NVARCHAR(100)
ADUserName ผู้ Revoke
NULL

RevokedDate
DATETIME
วันที่ Revoke
NULL

CreatedBy
NVARCHAR(100)
ADUserName ผู้สร้าง
DOMAIN\admin


CMS Data Dictionary — โปรเจ็ค 2511AL01

หน้า 1  |  Core Management System  |  Confidential Internal Use Only

