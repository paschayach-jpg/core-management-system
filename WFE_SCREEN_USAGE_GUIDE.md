# WFE Screen Usage Guide

เอกสารนี้กำหนดรูปแบบการใช้งานของแต่ละหน้าจอใน Workflow Engine (WFE) ว่าแต่ละหน้ามีไว้ให้ผู้ใช้ทำงานอะไร ใช้เมื่อไร และควรมี action/state อะไรบ้าง

## รูปแบบมาตรฐานของแต่ละหน้าจอ

ให้บันทึกหน้าจอ WFE ทุกหน้าด้วยหัวข้อชุดเดียวกันนี้ เพื่อให้ทีมออกแบบ UI, developer, tester และผู้ใช้ธุรกิจคุยกันด้วยภาษาเดียวกัน

| หัวข้อ | ความหมาย |
| --- | --- |
| หน้าจอ | ชื่อหน้าหรือฟอร์มในระบบ |
| ผู้ใช้หลัก | กลุ่มผู้ใช้ที่ใช้หน้านี้เป็นประจำ |
| เป้าหมาย | ผู้ใช้เปิดหน้านี้มาเพื่อทำงานอะไรให้สำเร็จ |
| ข้อมูลหลัก | ข้อมูลที่ต้องแสดงหรือให้ผู้ใช้แก้ไข |
| งานหลัก | ลำดับงานที่ผู้ใช้ต้องทำในหน้านี้ |
| Action หลัก | ปุ่มหรือคำสั่งสำคัญ |
| State สำคัญ | Empty, Loading, Error, No permission, Success หรือสถานะงานเฉพาะ |
| หน้าต่อเนื่อง | Dialog หรือหน้าที่เปิดต่อจากหน้านี้ |

## ภาพรวมเมนู WFE

| กลุ่มเมนู | หน้า | ใช้ทำอะไร |
| --- | --- | --- |
| การทำงาน | งานของฉัน | ดูและจัดการงาน workflow ที่มอบหมายให้ผู้ใช้ปัจจุบัน |
| การทำงาน | ตรวจสอบ Workflow | ติดตาม workflow ทั้งระบบสำหรับหัวหน้า/ผู้ดูแล |
| การทำงาน | เริ่ม Workflow | เลือก template และสร้าง workflow instance ใหม่ |
| การทำงาน | ประวัติ Workflow | ค้นหา workflow ที่เคยเกิดขึ้นและเปิดดูรายละเอียด |
| จัดการ | จัดการ Template | ค้นหา สร้าง แก้ไข duplicate import/export และส่งอนุมัติ template |
| จัดการ | Workflow Designer | ออกแบบขั้นตอน workflow, connection, SLA, approver และเงื่อนไข |
| จัดการ | อนุมัติ Template | ตรวจสอบ template ที่รออนุมัติ แล้ว approve/reject |
| ตั้งค่า | Delegation | มอบหมายงานแทน/ตั้งค่าผู้รับมอบหมายเมื่อเจ้าของงานไม่พร้อม |
| ตั้งค่า | รายงาน SLA | ดู compliance, overdue, cycle time และ export รายงาน |
| ตั้งค่า | Visual Flow | ดูเส้นทาง workflow แบบภาพ เพื่อเข้าใจ step ปัจจุบันและ path ที่วิ่ง |
| ตั้งค่า | ตั้งค่า | ตั้งค่าผู้ใช้หรือ profile ที่เกี่ยวกับ workflow |

## หน้าหลัก

### 1. งานของฉัน / WorkflowDashboardFormNew

| หัวข้อ | รายละเอียด |
| --- | --- |
| ผู้ใช้หลัก | ผู้ปฏิบัติงาน, approver, requester |
| เป้าหมาย | เห็นงานที่ต้องทำวันนี้และดำเนินการกับ task ของตัวเอง |
| ข้อมูลหลัก | My Tasks, workflow instances ของผู้ใช้, SLA metrics |
| งานหลัก | เปิดหน้า > ดู task ที่ค้าง > เลือก task > ดูรายละเอียด > approve/reject/forward/acknowledge หรือเปิด detail |
| Action หลัก | Refresh, Open Detail, Perform Step, Delegate/Reassign ถ้ามีสิทธิ์ |
| State สำคัญ | ไม่มีงาน, งานใกล้ครบ SLA, งาน overdue, service error |
| หน้าต่อเนื่อง | WorkflowStepActionForm, WorkflowInstanceDetailForm, WorkflowDetailFormNew |

### 2. ตรวจสอบ Workflow / WorkflowMonitorForm

| หัวข้อ | รายละเอียด |
| --- | --- |
| ผู้ใช้หลัก | หัวหน้า, admin, process owner |
| เป้าหมาย | เห็นสุขภาพ workflow ทั้งระบบและจุดติดขัด |
| ข้อมูลหลัก | จำนวน instance ตามสถานะ, SLA chart, task distribution, bottleneck, escalation stats, instance list |
| งานหลัก | เลือกช่วง/มุมมอง > ตรวจ KPI > เจาะดู bottleneck หรือ instance > สั่งแก้ไขเมื่อจำเป็น |
| Action หลัก | Refresh, Filter, Open Instance Detail, Reassign, Suspend/Resume/Cancel ถ้ามีสิทธิ์ |
| State สำคัญ | ไม่มีข้อมูลในช่วงที่เลือก, ข้อมูลโหลดไม่สำเร็จ, มี overdue/escalation สูง |
| หน้าต่อเนื่อง | WorkflowInstanceDetailForm, WorkflowVisualFlowForm, ReassignDialog |

### 3. เริ่ม Workflow / WorkflowStartDialog

| หัวข้อ | รายละเอียด |
| --- | --- |
| ผู้ใช้หลัก | requester หรือผู้เริ่มกระบวนการ |
| เป้าหมาย | สร้าง workflow instance ใหม่จาก template ที่เผยแพร่แล้ว |
| ข้อมูลหลัก | รายการ template, preview flow, title/description/context, priority |
| งานหลัก | ค้นหา template > เลือก template > ตรวจ preview > กรอกคำอธิบาย/priority > เริ่ม workflow |
| Action หลัก | Search Template, Select Template, Start Workflow, Cancel |
| State สำคัญ | ไม่มี template ที่ active, ยังไม่เลือก template, ข้อมูลเริ่ม workflow ไม่ครบ, create สำเร็จ |
| หน้าต่อเนื่อง | WorkflowDashboardFormNew หลังสร้างสำเร็จ |

### 4. ประวัติ Workflow / HistorySearchForm

| หัวข้อ | รายละเอียด |
| --- | --- |
| ผู้ใช้หลัก | ผู้ใช้ทั่วไป, admin, auditor |
| เป้าหมาย | ค้นหา workflow เก่าตาม template, status, วันที่, requester หรือ assignee |
| ข้อมูลหลัก | เงื่อนไขค้นหา, result grid, pagination/export |
| งานหลัก | กรอก filter > Search > เปิด detail หรือ export ผลลัพธ์ |
| Action หลัก | Search, Clear, Export, Open Detail |
| State สำคัญ | ไม่พบข้อมูล, filter ไม่ถูกต้อง, export สำเร็จ/ล้มเหลว |
| หน้าต่อเนื่อง | WorkflowDetailFormNew |

### 5. จัดการ Template / WorkflowTemplateListForm

| หัวข้อ | รายละเอียด |
| --- | --- |
| ผู้ใช้หลัก | workflow admin, process designer |
| เป้าหมาย | จัดการ template lifecycle ตั้งแต่ draft ถึง publish/deactivate |
| ข้อมูลหลัก | template grid, status, category, version, selection summary |
| งานหลัก | ค้นหา/filter template > เลือก template > สร้าง/แก้ไข/duplicate/delete > submit approval > import/export > ดู version |
| Action หลัก | Create, Edit, Duplicate, Delete, Submit Approval, Deactivate, Version History, Import, Export |
| State สำคัญ | ไม่มี template, ยังไม่เลือก row, action ถูกปิดตามสิทธิ์/สถานะ, import/export error |
| หน้าต่อเนื่อง | WorkflowDesignerForm, WorkflowCreateTemplateWizard, Version History Dialog |

### 6. Workflow Designer / WorkflowDesignerForm

| หัวข้อ | รายละเอียด |
| --- | --- |
| ผู้ใช้หลัก | process designer, workflow admin |
| เป้าหมาย | ออกแบบ workflow template แบบ visual canvas และตั้งค่ารายละเอียดของ step |
| ข้อมูลหลัก | canvas, step nodes, connections, properties panel, validation errors |
| งานหลัก | วาง step > เชื่อม step > ตั้ง approver/SLA/action/condition > validate > save draft หรือ submit |
| Action หลัก | Add Step, Configure Step, Connect, Configure Connection, Validate, Save, Submit |
| State สำคัญ | canvas ว่าง, step ยังตั้งค่าไม่ครบ, connection ไม่ถูกต้อง, validation errors, save สำเร็จ |
| หน้าต่อเนื่อง | Step Config Dialog, Connection Config Dialog, TemplateNameDialog |

### 7. อนุมัติ Template / TemplateApprovalForm

| หัวข้อ | รายละเอียด |
| --- | --- |
| ผู้ใช้หลัก | template approver, governance/admin |
| เป้าหมาย | ตรวจ template ที่ถูกส่งมาและตัดสินใจ approve/reject |
| ข้อมูลหลัก | pending template list, pending count, search/category filter, preview/detail |
| งานหลัก | เปิดรายการ pending > เลือก template > preview/ตรวจ version > approve หรือ reject พร้อมเหตุผล |
| Action หลัก | Preview, Approve, Reject, Refresh |
| State สำคัญ | ไม่มี pending template, ยังไม่เลือก template, reject ต้องมีเหตุผล, approve สำเร็จ |
| หน้าต่อเนื่อง | Preview/Designer แบบ read-only, Approval Review/Dialog |

### 8. Delegation / WorkflowDelegationForm

| หัวข้อ | รายละเอียด |
| --- | --- |
| ผู้ใช้หลัก | workflow admin, manager, owner งาน |
| เป้าหมาย | มอบหมายงานแทนหรือจัดการ delegation เมื่อผู้รับผิดชอบไม่สามารถทำงานได้ |
| ข้อมูลหลัก | task ที่เกี่ยวข้อง, ผู้รับมอบหมายเดิม/ใหม่, เหตุผล, ช่วงเวลา |
| งานหลัก | ค้นหา task หรือ user > เลือกผู้รับมอบหมายใหม่ > ระบุเหตุผล > ยืนยัน |
| Action หลัก | Search, Delegate, Reassign, Cancel |
| State สำคัญ | ไม่พบผู้ใช้, service delegation ไม่พร้อม, ไม่มีสิทธิ์, delegation สำเร็จ |
| หน้าต่อเนื่อง | DelegationDialog, ReassignDialog |

### 9. รายงาน SLA / SLAReportForm

| หัวข้อ | รายละเอียด |
| --- | --- |
| ผู้ใช้หลัก | manager, process owner, auditor |
| เป้าหมาย | วิเคราะห์ผลการทำงานตาม SLA และหา overdue/cycle time issue |
| ข้อมูลหลัก | KPI summary, compliance trend, cycle time distribution, overdue by person, status distribution, SLA metric rows |
| งานหลัก | เลือกช่วงวันที่/template/user > ดู KPI และ chart > เจาะรายการ overdue > export รายงาน |
| Action หลัก | Apply Filter, Refresh, Export Excel |
| State สำคัญ | ไม่มีข้อมูลในช่วงที่เลือก, filter ไม่ถูกต้อง, export สำเร็จ/ล้มเหลว |
| หน้าต่อเนื่อง | Excel file/export target |

### 10. Visual Flow / WorkflowVisualFlowForm

| หัวข้อ | รายละเอียด |
| --- | --- |
| ผู้ใช้หลัก | ผู้ปฏิบัติงาน, admin, process owner |
| เป้าหมาย | เห็นเส้นทาง workflow instance ว่าถึง step ไหน วิ่งผ่าน path ใด และติดตรงไหน |
| ข้อมูลหลัก | visual canvas, current step panel, SLA progress, path history, actions |
| งานหลัก | เปิด instance > ดู step ปัจจุบัน > ตรวจ path/skip reason > zoom/pan > ดำเนินการหรือ reassign |
| Action หลัก | Visual, Timeline, Audit, Zoom In/Out, Perform Step, Reassign, Comment |
| State สำคัญ | ยังไม่เลือก instance, ไม่มี flow data, step overdue, action ยังไม่ได้ implement |
| หน้าต่อเนื่อง | WorkflowStepActionForm, ReassignDialog |

### 11. ตั้งค่า / UserProfileForm

| หัวข้อ | รายละเอียด |
| --- | --- |
| ผู้ใช้หลัก | ผู้ใช้ WFE ทุกกลุ่ม |
| เป้าหมาย | ดูหรือปรับข้อมูล profile ที่ใช้กับ workflow เช่น role, department, notification/preference |
| ข้อมูลหลัก | user context, role, department, contact/preference |
| งานหลัก | ตรวจข้อมูลผู้ใช้ > แก้ไขค่าที่อนุญาต > บันทึก |
| Action หลัก | Save, Cancel/Close |
| State สำคัญ | ไม่มี user context, ไม่มีสิทธิ์แก้ไข, save สำเร็จ/ล้มเหลว |
| หน้าต่อเนื่อง | ไม่มี |

### 12. Workflow Instance Detail / WorkflowInstanceDetailForm

| หัวข้อ | รายละเอียด |
| --- | --- |
| ผู้ใช้หลัก | requester, assignee, admin, auditor |
| เป้าหมาย | ดูรายละเอียด workflow instance และประวัติ step ทั้งหมด |
| ข้อมูลหลัก | instance details, current step, step history, allowed actions |
| งานหลัก | เปิด instance > ตรวจสถานะ/current step > อ่านประวัติ > suspend/resume/cancel ถ้ามีสิทธิ์ |
| Action หลัก | Suspend, Resume, Cancel, Refresh/Close |
| State สำคัญ | instance ไม่พบ, action ถูกปิดตามสถานะ, service error |
| หน้าต่อเนื่อง | WorkflowVisualFlowForm หรือ action dialog ตามบริบท |

### 13. Plugin Registry / PluginRegistryForm

| หัวข้อ | รายละเอียด |
| --- | --- |
| ผู้ใช้หลัก | system admin |
| เป้าหมาย | จัดการ plugin สำหรับ AutoTask ใน workflow |
| ข้อมูลหลัก | plugin list, active/inactive count, search/status filter |
| งานหลัก | ค้นหา plugin > ดูรายละเอียด > enable/disable/delete ตามสิทธิ์ |
| Action หลัก | Enable, Disable, Delete, Detail |
| State สำคัญ | ไม่มี plugin, ยังไม่เลือก plugin, action ถูกปิดตาม status, service error |
| หน้าต่อเนื่อง | Plugin detail dialog ถ้ามี |

## Dialog และหน้าประกอบ

| Dialog/ฟอร์ม | ใช้ทำอะไร | งานหลักของผู้ใช้ |
| --- | --- | --- |
| WorkflowStepActionForm | ดำเนินการกับ step ที่ได้รับมอบหมาย | อ่านคำแนะนำ/ข้อมูล step > ใส่ comment/ผลลัพธ์ > เลือก approve/reject/forward/acknowledge |
| ApprovalReviewForm | ตรวจและตัดสินใจ approval ราย step | ดู step, assignee, SLA > approve หรือ reject พร้อมเหตุผล |
| DelegationDialog | มอบหมาย task ให้คนอื่น | ตรวจ task > เลือก delegate to > ใส่เหตุผล > ยืนยัน |
| ReassignDialog | เปลี่ยนผู้รับผิดชอบ task | เลือกผู้รับผิดชอบใหม่จาก candidate/search > ใส่เหตุผล > ยืนยัน |
| TemplateNameDialog | ตั้งชื่อและรายละเอียด template ก่อนบันทึก | กรอกชื่อ/รายละเอียด > บันทึกหรือยกเลิก |
| WorkflowCreateTemplateWizard | สร้าง template แบบ wizard | กรอกข้อมูลพื้นฐาน > กำหนด step/approver/form > review > create |
| WorkflowDetailFormNew | ดูรายละเอียด workflow แบบ dialog | อ่านข้อมูล instance และสถานะล่าสุด |

## Checklist สำหรับออกแบบหรือทดสอบแต่ละหน้า

- ผู้ใช้เปิดหน้านี้มาเพื่อทำงานเดียวที่ชัดเจนหรือไม่
- ข้อมูลสำคัญที่สุดอยู่ใน viewport แรกหรือไม่
- ปุ่มหลักอยู่ตำแหน่งที่คาดเดาได้หรือไม่
- action ที่อันตรายมี confirmation และแสดง object/count ที่จะกระทบหรือไม่
- empty/loading/error/no permission แสดงข้อความที่ช่วยให้ผู้ใช้ไปต่อได้หรือไม่
- filter/search มีค่า default ที่สมเหตุผลหรือไม่
- หน้าที่เป็น table/grid แสดงสถานะ, SLA, owner และ action ที่ scan ง่ายหรือไม่
- หน้าที่เป็น form มี validation ใกล้ field และไม่รอแจ้งตอนท้ายอย่างเดียวหรือไม่
- หน้าที่เกี่ยวกับ SLA แยก on-time, at-risk, overdue ด้วยสีและข้อความ ไม่ใช้สีอย่างเดียวหรือไม่
- ทุกหน้าที่เปิด dialog ต่อมีทางกลับ/close ที่ชัดเจนและไม่ทำข้อมูลหายโดยไม่เตือน
