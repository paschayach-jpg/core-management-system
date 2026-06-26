# ข้อกำหนดการเชื่อมต่อฐานข้อมูลและ Stored Procedures ของ WFE (Workflow Engine)

เอกสารนี้รวบรวมรายละเอียดการเชื่อมต่อฐานข้อมูล (Database Connection) และ Stored Procedures ทั้งหมดที่ใช้ในการทำงานของระบบ **Workflow Engine (WFE)** ในโปรเจกต์นี้ เพื่อใช้อ้างอิงในการพัฒนา ย้ายระบบ หรือบำรุงรักษา

---

## 1. การเชื่อมต่อฐานข้อมูล (Database Connection)

ระบบ WFE ใช้การเชื่อมต่อฐานข้อมูลร่วมกับระบบ CMS โดยมีรายละเอียดการกำหนดค่าดังนี้:

* **ไฟล์ที่กำหนดค่า**: [CMS.Presentation/app.config](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Presentation/app.config#L8-L13)
* **คีย์การกำหนดค่า**:
  * `<add name="CmsDb" connectionString="..." />` (ใช้ใน Data Access Layer)
  * `<add key="ConnectionString" value="..." />` (ใช้ใน Legacy / Repositories Direct Call)
* **ค่า Connection String (ระบบจริง/ทดสอบ)**:
  ```text
  Data Source=192.168.250.101;Initial Catalog=CMS_DB;User ID=K2pack;Password=P@ssw0rd;
  ```
* **ฐานข้อมูล**: SQL Server (2019 ขึ้นไป)
* **การจัดเรียงภาษา (Collation)**: `Thai_CI_AS`

---

## 2. รายชื่อ Stored Procedures ทั้งหมดของ WFE

Stored Procedures ของระบบ WFE ถูกแบ่งกลุ่มตามขอบเขตการทำงานออกเป็น 4 กลุ่มหลัก ดังนี้:

### กลุ่มที่ 1: การจัดการเวิร์กโฟลว์และขั้นตอน (Workflow Instance & Step Management)
ใช้สำหรับสร้างและควบคุมสถานะการวิ่งของกระบวนการเวิร์กโฟลว์

#### 1. `WFE.sp_CreateWFInstance`
* **วัตถุประสงค์**: สร้างข้อมูลอินสแตนซ์เวิร์กโฟลว์ใหม่เมื่อเริ่มกระบวนการลงในตาราง `WFE.t_WFInstance`
* **พารามิเตอร์**:
  * `@WFDefinitionId` (`INT`): ไอดีของเทมเพลตเวิร์กโฟลว์หลัก
  * `@TemplateVersion` (`INT`): เวอร์ชันของเทมเพลตที่ใช้เริ่มระบบ
  * `@InitiatedBy` (`NVARCHAR(100)`): รหัสพนักงาน/ชื่อ AD User ผู้เริ่มกระบวนการ
  * `@InputParameters` (`NVARCHAR(MAX)`): ข้อมูลอินพุตในรูปแบบ JSON String
  * `@NewWFInstanceId` (`INT OUTPUT`): ไอดีของเวิร์กโฟลว์อินสแตนซ์ที่เพิ่งสร้างขึ้นใหม่
* **เรียกใช้งานจาก**: [WorkflowRepository.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/Repositories/WorkflowRepository.cs#L30-L52) (`CreateWorkflowInstance`)

#### 2. `WFE.sp_CreateWFStepInstance`
* **วัตถุประสงค์**: สร้างข้อมูลขั้นตอนย่อย (Step Instance) ลงในตาราง `WFE.t_WFStepInstance` เมื่อกระบวนการไหลไปถึงขั้นตอนนี้
* **พารามิเตอร์**:
  * `@WorkflowInstanceId` (`INT`): ไอดีของอินสแตนซ์เวิร์กโฟลว์ที่เกี่ยวข้อง
  * `@WorkflowStepId` (`INT`): ไอดีขั้นตอนจาก `WFE.s_WFStep`
  * `@AssignedTo` (`NVARCHAR(256)`): ชื่อ AD User ของผู้รับผิดชอบในการอนุมัติ/ดำเนินการขั้นตอนนีั
  * `@SLADeadline` (`DATETIME`): วันและเวลาเป้าหมายสิ้นสุดตาม SLA
  * `@NewWorkflowStepInstanceId` (`INT OUTPUT`): ไอดีของขั้นตอนย่อยที่สร้างสำเร็จ
* **เรียกใช้งานจาก**: [WorkflowStepInstanceRepository.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/Repositories/WorkflowStepInstanceRepository.cs#L30-L52) (`CreateStepInstance`)

#### 3. `dbo.sp_WF_GetPendingSteps`
* **วัตถุประสงค์**: ดึงข้อมูลขั้นตอนที่ค้างการทำงานและต้องดำเนินการทั้งหมดในระบบ (Status = Pending หรือ InProgress)
* **พารามิเตอร์**:
  * `@LookAheadHours` (`INT`, รองรับ `NULL`): ใช้สำหรับกรองเฉพาะขั้นตอนที่จะครบกำหนด SLA ภายในกี่ชั่วโมงข้างหน้า (ถ้าผ่านค่าเป็น `NULL` จะดึงข้อมูลค้างทั้งหมด)
* **เรียกใช้งานจาก**: [WorkflowStepInstanceRepository.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/Repositories/WorkflowStepInstanceRepository.cs#L109-L143) (`GetPendingStepInstances`)

---

### กลุ่มที่ 2: การมอบหมายงานแทน (Workflow Delegation)
ใช้สำหรับส่งงานต่อไปยังเพื่อนร่วมงาน หรือการตั้งกฎแทนที่ผู้รับผิดชอบชั่วคราว

#### 4. `dbo.sp_WF_CreateDelegation`
* **วัตถุประสงค์**: บันทึกสิทธิ์การทำงานแทนลงในตาราง `dbo.WFDelegation` พร้อมกำหนดสถานะยกเลิกกฎอันเดิม (Deactivate) และตรวจสอบว่าการมอบสิทธิ์ต่อกันไม่ลึกเกินค่าที่จำกัดไว้ (Max Delegation Depth)
* **พารามิเตอร์**:
  * `@WorkflowStepInstanceId` (`INT`): ไอดีขั้นตอนงานที่ต้องการมอบหมาย
  * `@DelegatedFrom` (`NVARCHAR(100)`): ชื่อผู้มอบงาน (เจ้าของสิทธิ์เดิม)
  * `@DelegatedTo` (`NVARCHAR(100)`): ชื่อผู้รับมอบหมายงานแทน
  * `@DelegationReason` (`NVARCHAR(500)`, `NULL`): เหตุผลในการมอบหมายงานแทน
  * `@MaxDelegationDepth` (`INT`): ค่าความลึกสูงสุดในการมอบหมายทอดต่อกัน (ค่าเริ่มต้น = 3)
  * `@StartDate` (`DATETIME`, `NULL`): วันที่เริ่มต้นมีผล
  * `@EndDate` (`DATETIME`, `NULL`): วันที่สิ้นสุดสิทธิ์
  * `@Remarks` (`NVARCHAR(500)`, `NULL`): หมายเหตุประกอบเพิ่มเติม
  * `@NewWFDelegationId` (`INT OUTPUT`): รหัสประวัติการมอบงานที่ถูกสร้างใหม่
* **ไฟล์สคริปต์ SQL**: [098_Create_WFDelegation.sql](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/Scripts/098_Create_WFDelegation.sql#L58-L127)
* **เรียกใช้งานจาก**: [DelegationRepository.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/Repositories/DelegationRepository.cs#L29-L55) (`CreateDelegation`)

#### 5. `dbo.sp_WF_GetDelegationHistory`
* **วัตถุประสงค์**: ดึงรายการประวัติการส่งมอบงานทั้งหมดตามไอดีขั้นตอนงาน เพื่อดูลำดับขั้นตอนการเปลี่ยนมือผู้รับผิดชอบ
* **พารามิเตอร์**:
  * `@WorkflowStepInstanceId` (`INT`): ไอดีขั้นตอนงานที่ตรวจประวัติ
* **ไฟล์สคริปต์ SQL**: [098_Create_WFDelegation.sql](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/Scripts/098_Create_WFDelegation.sql#L132-L160)
* **เรียกใช้งานจาก**: [DelegationRepository.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/Repositories/DelegationRepository.cs#L84-L103) (`GetDelegationsByStepInstance`)

---

### กลุ่มที่ 3: แดชบอร์ดและรายงานวิเคราะห์ประสิทธิภาพ SLA (SLA Reporting & Analytics)
ใช้ประมวลผลข้อมูลสถิติเพื่อแสดงผลภาพกราฟและตารางประสิทธิภาพในแดชบอร์ด

#### 6. `dbo.sp_WF_GetSLAComplianceMetrics`
* **วัตถุประสงค์**: คำนวณสรุปประสิทธิภาพภาพรวมของ SLA (จำนวนรวมของขั้นตอน, งานเสร็จตรงเวลา, งานกลุ่มเสี่ยง, งานล่าช้า, เปอร์เซ็นต์ On-time, และค่าเฉลี่ยชั่วโมงการอนุมัติ)
* **พารามิเตอร์**:
  * `@WorkflowDefinitionId` (`INT`, `NULL`): ตัวกรองเฉพาะเทมเพลตเวิร์กโฟลว์ที่เจาะจง
  * `@FromDate` (`DATETIME`, `NULL`): วันที่เริ่มต้นช่วงข้อมูลรายงาน
  * `@ToDate` (`DATETIME`, `NULL`): วันที่สิ้นสุดช่วงข้อมูลรายงาน
* **ไฟล์สคริปต์ SQL**: [080_WFE_ApprovalLifecycle_And_SLAReport.sql](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/Scripts/080_WFE_ApprovalLifecycle_And_SLAReport.sql#L108-L142)
* **เรียกใช้งานจาก**: [WFReportRepository.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/Repositories/WFReportRepository.cs#L28-L56) (`GetSLAComplianceMetrics`)

#### 7. `dbo.sp_WF_GetTaskDistribution`
* **วัตถุประสงค์**: สรุปข้อมูลจำนวนและสถานะงานของรายบุคคล (เสร็จแล้ว, ค้างอยู่, ปฏิเสธ, ทำงานล่าช้ากว่า SLA) เพื่อแสดงผลในตารางผู้รับผิดชอบงาน
* **พารามิเตอร์**:
  * `@WorkflowDefinitionId` (`INT`, `NULL`): ตัวกรองเฉพาะเทมเพลตเวิร์กโฟลว์
  * `@FromDate` (`DATETIME`, `NULL`): วันที่เริ่มต้นรายงาน
  * `@ToDate` (`DATETIME`, `NULL`): วันที่สิ้นสุดรายงาน
* **ไฟล์สคริปต์ SQL**: [080_WFE_ApprovalLifecycle_And_SLAReport.sql](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/Scripts/080_WFE_ApprovalLifecycle_And_SLAReport.sql#L144-L177)
* **เรียกใช้งานจาก**: [WFReportRepository.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/Repositories/WFReportRepository.cs#L62-L106) (`GetTaskDistribution`)

#### 8. `dbo.sp_WF_GetBottleneckAnalysis`
* **วัตถุประสงค์**: ตรวจวิเคราะห์ขั้นตอนที่เป็นคอขวดของกระบวนการทำงาน โดยประเมินจำนวนครั้งที่ทำจริง จำนวนที่ล่าช้า และเวลาการตอบสนองเฉลี่ย/สูงสุด
* **พารามิเตอร์**:
  * `@WorkflowDefinitionId` (`INT`, `NULL`): ตัวกรองเฉพาะเทมเพลต
  * `@FromDate` (`DATETIME`, `NULL`): วันที่เริ่มต้นวิเคราะห์
  * `@ToDate` (`DATETIME`, `NULL`): วันที่สิ้นสุดวิเคราะห์
* **ไฟล์สคริปต์ SQL**: [080_WFE_ApprovalLifecycle_And_SLAReport.sql](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/Scripts/080_WFE_ApprovalLifecycle_And_SLAReport.sql#L179-L222)
* **เรียกใช้งานจาก**: [WFReportRepository.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/Repositories/WFReportRepository.cs#L112-L159) (`GetBottleneckAnalysis`)

---

### กลุ่มที่ 4: เวิร์กโฟลว์คำขออนุมัติเอกสารและสิทธิ์ (Approval Lifecycle Requests)
ใช้สำหรับจัดเก็บและตรวจสอบสิทธิ์และสถานะของการร้องขอ (Request) และโหนดผู้อนุมัติ (Step) ของระบบงานเอกสาร

#### 9. `workflow.sp_GetApprovalRequestByID`
* **วัตถุประสงค์**: ดึงรายละเอียดข้อมูลคำขออนุมัติจากตาราง `workflow.ApprovalRequest` ตามรหัส ID ของคำขอ
* **พารามิเตอร์**:
  * `@RequestID` (`INT`): รหัสคำขออนุมัติ
* **เรียกใช้งานจาก**: [ApprovalRepository.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/Repositories/ApprovalRepository.cs#L67-L83) (`GetRequest`)

#### 10. `workflow.sp_GetAllApprovalSteps`
* **วัตถุประสงค์**: ดึงรายการขั้นตอนทั้งหมดสำหรับการขออนุมัติรายการนั้นๆ จากตาราง `workflow.ApprovalStep`
* **พารามิเตอร์**:
  * `@RequestID` (`INT`): รหัสคำขออนุมัติ
* **เรียกใช้งานจาก**: [ApprovalRepository.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/Repositories/ApprovalRepository.cs#L108-L125) (`GetAllSteps`)

#### 11. `workflow.sp_GetExpiredPendingRequests`
* **วัตถุประสงค์**: ดึงรายการข้อมูลคำขออนุมัติที่เลยกำหนดเวลาหมดอายุแล้ว แต่ผู้ใช้ยังไม่ได้เข้าอนุมัติหรือปฏิเสธ
* **พารามิเตอร์**: ไม่มีพารามิเตอร์
* **เรียกใช้งานจาก**: [ApprovalRepository.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/Repositories/ApprovalRepository.cs#L176-L192) (`GetExpiredPendingRequests`)

#### 12. `workflow.sp_UpdateApprovalStepStatus`
* **วัตถุประสงค์**: ปรับปรุงสถานะรายขั้นตอนการอนุมัติ (เช่น อนุมัติแล้ว, ปฏิเสธ, มอบหมายงานแทน, รอดำเนินการ) พร้อมแนบความคิดเห็น
* **พารามิเตอร์**:
  * `@StepID` (`INT`): ไอดีโหนดขั้นตอนที่ต้องการปรับสถานะ
  * `@StepStatus` (`TINYINT`): สถานะใหม่ที่เปลี่ยน (เช่น 1=Pending, 2=Approved, 3=Rejected, 4=Delegated)
  * `@ActionRemark` (`NVARCHAR(MAX)`): เหตุผลหรือข้อคิดเห็นประกอบ
* **เรียกใช้งานจาก**: [ApprovalRepository.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/Repositories/ApprovalRepository.cs#L194-L208) (`UpdateStepStatus`)

#### 13. `workflow.sp_UpdateApprovalRequestStatus`
* **วัตถุประสงค์**: ปรับปรุงสถานะภาพรวมของใบคำขออนุมัติหลัก (เช่น ได้รับการอนุมัติ, ถูกปฏิเสธ, รอการอนุมัติ)
* **พารามิเตอร์**:
  * `@RequestID` (`INT`): รหัสของคำขออนุมัติหลัก
  * `@RequestStatus` (`TINYINT`): สถานะภาพรวมใหม่
  * `@ResolvedBy` (`INT`): ไอดีผู้ใช้ที่เป็นคนลงมติขั้นสุดท้าย
  * `@ResolveRemark` (`NVARCHAR(MAX)`): หมายเหตุประกอบการสรุปผลคำขอ
* **เรียกใช้งานจาก**: [ApprovalRepository.cs](file:///c:/Users/pitakpong/Documents/cms_import/CMS.Data/Repositories/ApprovalRepository.cs#L210-L225) (`UpdateRequestStatus`)
