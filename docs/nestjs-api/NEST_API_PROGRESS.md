# สถานะการทำ NestJS API (repo `core-management-system-nest-api`)

> สรุป ณ จุดที่หยุดงาน · 2026-10-14 · เขียนเพื่อให้ทีม/ผู้ทำต่อรู้ว่า **อะไรเสร็จ อะไรค้าง อะไรต้องทำต่อ และอะไรแนะนำ**
> repo ของ API: `https://github.com/paschayach-jpg/core-management-system-nest-api` (branch `main`) — เอกสารนี้อยู่ที่ repo เดิม (`popnahaff`) ตามที่สั่ง
> ข้อจำกัดสำคัญ: **ไม่มี SQL Server ในสภาพแวดล้อมที่พัฒนา** — ทุกอย่างที่ "ผ่าน" ด้านล่างคือผ่านด้วย typecheck / lint / unit test / e2e แบบ DataSource ปลอม **ยังไม่มีสิ่งใดถูกรันกับ DB จริง**

## 0. สรุปสั้น

| ส่วน | สถานะ | อยู่ที่ไหน |
|---|---|---|
| โครงหลัก (config, SQL Server หลาย DataSource, SpExecutor, TransactionRunner, CRUD-log, error/guard/Swagger/health) | ✅ เสร็จ | `origin/main` |
| Auth (AD proxy → JWT, UserContext, PDPA consent) | ✅ เสร็จ | `origin/main` |
| Shared (ผู้ใช้, หน่วยงาน, HR master, เมนู, config, แจ้งเตือน, audit) | ✅ เสร็จ | `origin/main` |
| DMS core (โฟลเดอร์/ไฟล์/ACL/ถังขยะ/เทมเพลต/ทางลัด/visibility) | ✅ เสร็จ | `origin/main` |
| RGP (โครงการ) | ✅ เสร็จ | `origin/main` |
| WFE (workflow) | ✅ เสร็จ | `origin/main` |
| DMS approvals / แชร์ / โควตา / authority | 🟡 agent ทำเสร็จ **merge ในเครื่องแล้วแต่ยังไม่ push** และมี **บั๊กต่อสาย 1 จุดที่ยังแก้ไม่จบ** (ข้อ 1) | `main` ในเครื่อง (commit `acbfeda`) |
| KMS (หมวด/บทความ/import Excel ฯลฯ) | 🟠 agent ทำไปแล้ว 4 commit แต่ **ยังไม่ merge, ไม่มีรายงานสรุป, ไม่มีเอกสาร module, ยังไม่ได้ตรวจ** | branch `feat/kms` ใน `/home/user/nest-kms` (ในเครื่องเท่านั้น) |
| เอกสาร/Docker/CI/คู่มือ | ✅ เสร็จ (README, ARCHITECTURE, CONVENTIONS, DECISIONS, CRUD_LOG, RECOMMENDATIONS, Dockerfile, compose, GitHub Actions) | `origin/main` |
| ทดสอบกับ SQL Server จริง | ❌ ยังไม่ได้ทำเลย | — |

## 1. ⚠ สถานะ git ที่ต้องรู้ก่อนทำต่อ (สำคัญ — งานบางส่วนอยู่ในเครื่องที่ไม่ถาวร)

เครื่องที่พัฒนาเป็น container ชั่วคราว งานต่อไปนี้ **ยังไม่อยู่บน GitHub** และจะหายถ้า container ถูกเก็บคืน:

| รายการ | รายละเอียด |
|---|---|
| `origin/main` ล่าสุด | `e52b503` (รวม DMS core, RGP, WFE และจำกัดสิทธิ์ audit ตาม workflow instance) |
| `main` ในเครื่องล้ำหน้า | commit `acbfeda` "รวม DMS approvals" (merge branch `feat/dms-approvals`) — **ยังไม่ push** |
| ไฟล์แก้ค้างที่ยังไม่ commit | (1) `src/modules/dms/core/dms-core.module.ts` — เพิ่ม `@Global()` + คอมเมนต์ **แต่ยังไม่ได้เพิ่ม `Global` ใน `import { … } from '@nestjs/common'`** → ตอนนี้ **typecheck จะไม่ผ่านจนกว่าจะเพิ่ม import** (2) `test/unit/dms-approvals/module-wiring.spec.ts` — เขียนใหม่ให้ทดสอบกับ DMS core จริง (ต้องคงไว้ — เทสต์นี้เจอบั๊กจริง) |
| branch `feat/dms-core`, `feat/dms-approvals`, `feat/rgp`, `feat/wfe` | merge เข้า `main` แล้ว (worktree `/home/user/nest-*` ลบได้) |
| branch `feat/kms` | **ยังไม่ merge** · worktree `/home/user/nest-kms` · commits: `46cdb78` → `62d8c3c` → `0373eda` → `890b16d` · ไม่เคยถูก push |

**บั๊กต่อสายที่เจอตอน merge (ยังไม่จบ):** `PublicShareAccessService` (ลิงก์แชร์สาธารณะ ของ approvals) ฉีด `DMS_FILE_UNCHECKED_PORT` แบบ `@Optional()` แต่ `DmsCoreModule` ไม่ได้เป็น `@Global()` และ approvals ตั้งใจไม่ import core (กัน module วน) → ในแอปจริงค่าเป็น `undefined` ⇒ **ดาวน์โหลดผ่านลิงก์แชร์ภายนอกจะตอบ 503 เสมอ** · เทสต์เดิมของ approvals ไม่เจอเพราะจำลอง core ด้วย module ปลอม · เทสต์ที่เขียนใหม่ใช้ AppModule จริง จึง **ล้ม 1 ข้อ (1209 ผ่าน / 1 ล้ม จาก 1210)** — การแก้ที่เสนอ: เติม `Global` ใน import แล้วคง `@Global()` บน `DmsCoreModule` (ปลอดภัย เพราะ module นี้ export เฉพาะ token ของ port 2 ตัว)

## 2. สิ่งที่เสร็จแล้ว

### 2.1 ตามข้อกำหนด 4 ข้อของผู้ว่าจ้าง
| ข้อกำหนด | ทำแล้วอย่างไร |
|---|---|
| 1. แนวทางที่นิยมปัจจุบัน | NestJS 11 + TypeORM 0.3 + TypeScript strict · REST `/api/v1/…` + OpenAPI · DTO validate แบบ whitelist · JWT · pino structured log · zod ตรวจ env · Docker/CI · (เหตุผลที่ไม่ใช้รุ่นใหม่สุดอยู่ใน `DECISIONS.md` D-01) |
| 2. แยก service ตาม module | `auth`, `shared`, `dms` (permissions/core/approvals), `kms`, `rgp`, `wfe` — แต่ละ module มี service/controller/DTO ของตัวเอง คุยกันผ่าน provider ที่ export หรือ "port" (token + interface) |
| 3. เชื่อม SQL Server | `mssql` ผ่าน TypeORM · 2 DataSource (CMS_DB และ BUI_RG) · entity 63 ตาราง + Stored Procedure 124 ตัวแบบมีชนิดพารามิเตอร์ สร้างจาก DDL/SP ที่ export (`npm run codegen`) · `SpExecutor` (หลาย recordset/OUTPUT/transaction/deadlock retry) · `useUTC:false` + `TZ=Asia/Bangkok` เพราะ DB เก็บเวลาท้องถิ่น |
| 4. ทุก service รองรับ CRUD-log | `CrudLoggedService` เป็นคลาสแม่ของทุก service (เทสต์สถาปัตยกรรมบังคับ) · event ผ่าน `CrudLogService` ไปยัง sink ที่เสียบเพิ่มได้ (console/pino เปิดอยู่, `audit.AuditLog` ปิดอยู่) · ระดับ `off/write/all` · redact ความลับ · ไม่ block/ไม่ทำคำขอล้ม · ระบบเก็บ log ใหม่ในอนาคต = implement `CrudLogSink` แล้ว register ไม่ต้องแก้ service |

### 2.2 รายละเอียดต่อ module (จำนวน endpoint = จำนวน operation ที่ agent/ผมรายงาน)
| Module | Endpoint | เอกสารใน repo API | หมายเหตุ |
|---|--:|---|---|
| Auth | 5 | `docs/modules/shared.md` | login/exchange/me/consent/logout |
| Shared | 59 | `docs/modules/shared.md` | users 7, org-units 11, hr 24, menus 8, system-config 3, notifications 4, audit 2 |
| DMS core | 56 | `docs/modules/dms-core.md` | โฟลเดอร์ 11, ไฟล์ 11, ACL 8, ถังขยะ 5, เทมเพลต 10, ทางลัด 3, visibility 8 |
| DMS approvals | 49 | `docs/modules/dms-approvals.md` | approvals 14, shares 14, public 2, quota 10, authority 9 (merge ในเครื่อง) |
| RGP | 77 | `docs/modules/rgp.md` | |
| WFE | 49 | `docs/modules/wfe.md` | template 25, instance/step 14, delegation 6, SLA 4 |
| KMS | ≈65 handler | (ยังไม่มีเอกสาร) | อยู่ใน `feat/kms` ยังไม่ได้ตรวจ |
| **รวมที่ merge แล้ว** | **≈295** | | ไม่รวม KMS |

### 2.3 โครงสร้างพื้นฐาน/เอกสารที่ส่งมอบ
Dockerfile (multi-stage, non-root), `docker-compose.yml` (API + SQL Server 2022), GitHub Actions (typecheck · lint · format · test · e2e · build · docker), Dependabot, `npm run openapi:export` (ไม่ต้องมี DB), `docs/db/001_recommended_indexes.sql` (ข้อเสนอ index ยังไม่เคยรัน), เอกสาร `README`, `ARCHITECTURE`, `CONVENTIONS`, `DECISIONS`, `CRUD_LOG`, `RECOMMENDATIONS`, เอกสารอ้างอิงระบบเดิมที่ `docs/reference/` (สำเนาจาก repo นี้ + DDL/SP export)

### 2.4 ผลตรวจล่าสุด
| จุด | typecheck | lint | format | unit test | e2e (DI+guard) | build |
|---|:-:|:-:|:-:|---|:-:|:-:|
| `origin/main` (`e52b503`, ยืนยันบน GitHub แล้ว) | ✅ | ✅ | — (รันก่อน merge WFE: ผ่าน) | ✅ 56 suites / 1,023 tests | ✅ 9 | — (agent รายงานผ่านบน branch ของตน) |
| `main` ในเครื่อง หลัง merge approvals | ✅ (ก่อนมีไฟล์แก้ค้าง) | ✅ | — | ❌ 70 suites / 1,210 tests: **1 ล้ม** (บั๊กต่อสายในข้อ 1) | ✅ 9 | — |

## 3. สิ่งที่ต้องทำ (เรียงตามลำดับ)

### 3.1 ทำทันทีเพื่อให้ `main` กลับมาเขียว (งานเล็ก)
1. เติม `Global` ใน import ของ `dms-core.module.ts` (ค้างจากตอนถูกขัดจังหวะ) → รัน `npm run typecheck && npm run lint && npm test && npm run test:e2e` → ต้องเขียวทั้งหมดรวมเทสต์ `module-wiring`
2. commit (ไฟล์แก้ค้าง 2 ไฟล์) แล้ว **push `main`** — กันงานหาย
3. เพิ่ม path public ของลิงก์แชร์ (`POST /api/v1/dms/public/shares/{token}/open` และ `/download`) ให้เทสต์ e2e (`test/e2e/app-wiring.e2e-spec.ts` รายการ `publicPaths`) ถ้ายังไม่ได้ระบุ — เพื่อให้เทสต์ "ทุก endpoint ต้องมี security ยกเว้น public" ยังมีความหมาย

4. บน GitHub มี branch ที่ Dependabot เปิดให้อัตโนมัติ (เช่น `dependabot/docker/node-26-alpine`, `…/actions/checkout-7`, `…/setup-node-7`, `…/build-push-action-7`) — ทบทวนก่อน merge; **อย่าเพิ่ง merge node-26** (เป้าหมายคือ Node 22 LTS ตาม `.nvmrc`/Dockerfile)

### 3.2 KMS (ใหญ่ที่สุดที่ยังเหลือ)
1. เปิด worktree `feat/kms` ตรวจโค้ด: ทำคำสั่งตรวจครบ (typecheck/lint/test/e2e/build) และ **อ่านทบทวน** ส่วน import (กฎ Code รวม 29 ตัว, Law/Decree/NonLaw, หมวดรายปี, re-upload = update, text fidelity — ดู `AGENTS.md` หัวข้อ "KMS Import (MOD-006)")
2. เขียน `docs/modules/kms.md` ตามแม่แบบ `_TEMPLATE.md` (endpoint, ส่วนที่ต่างจากเดิม, deferred, รายการที่ต้องทดสอบกับ DB จริง) — agent ยังไม่ได้ทำ
3. ตรวจว่า `DMS_FILE_PORT` ถูกฉีดจริงเมื่อรวมกับ DMS core (เทสต์ของ KMS จำลองด้วย mock) และ DMS folder ปลายทางของไฟล์แนบ import ไม่ถูกฝัง 437
4. merge `feat/kms` เข้า `main` → รันตรวจทั้งชุด → push
5. ผู้ใช้ส่ง Excel ผ่าน HTTP: ยืนยันการออกแบบ "งานเบื้องหลัง + in-memory job" (สถานะหายเมื่อ restart/ไม่รองรับหลาย instance — ดู RECOMMENDATIONS O-03)

### 3.3 งานที่ agent ระบุว่า "ยังไม่ได้ทำ" (deferred) — ต้องตัดสินใจว่าจะทำหรือไม่
| Module | รายการ | เหตุผลที่ยังไม่ทำ |
|---|---|---|
| Shared | นำเข้า Excel ข้อมูลหลัก HR (`OrgMasterService.ExecuteImport`) | ไม่ใช่ CRUD หลัก; ต้องยืนยันว่ายังใช้ |
| Shared | ค้นผู้ใช้ LDAP / `UpsertFromLdap`; sync จาก Employee API; endpoint ของ `EMS.m_Employee`; Menu API ภายนอก | ต้องมี credential/สัญญา API ที่ยังไม่รู้ และไม่พบ use case ที่เรียกในโค้ดที่ compile |
| DMS core | **ซ่อน/แสดงไฟล์** (`HIDE_FILE/UNHIDE_FILE`) | `storage.FileDocument` ไม่มีคอลัมน์ `IsHidden/HiddenBy/HiddenDate` และระบบเดิมไม่เคยบันทึกถาวร — ต้องให้ DBA เพิ่มคอลัมน์ก่อน |
| DMS core | สแกนไวรัส (ระบบเดิมก็ข้าม) | ต้องตัดสินใจเปิด ClamAV (RECOMMENDATIONS S-04) |
| DMS approvals | import สิทธิ์จาก Excel เรียก `sp_UpsertUserAuthority` | ตาราง `EMS.UserAuthority` ไม่มี DDL — ตอนนี้อัปเดตแถวเดิมใน transaction |
| DMS approvals | แชร์โฟลเดอร์ภายนอกรวมโฟลเดอร์ย่อย; ลบ config authority รายแถว | ตาราง `FolderAuthorityConfig` ไม่มี PK → ลบทั้งกลุ่ม ConfigLabel |
| RGP | E-Filing, `GetSubtasks`, `EditComment`, `Restore`, `HasActiveTasks`, ปุ่ม "ส่ง API" ของพจนานุกรมกฎหมาย | ระบบเดิมไม่มีหน้าจอเรียก / ไม่ทราบปลายทางภายนอก |
| RGP | job ลบ Task ในถังขยะหลัง 30 วัน | API ตั้ง `recycle_expiry` ให้แล้วแต่ยังไม่มี job ลบจริง |
| WFE | archive/restore instance; `CompareTemplateVersions` + ประวัติเวอร์ชันจริง | ใน DB ไม่มีคอลัมน์/ตาราง snapshot; condition evaluator ยังเป็น stub (เหมือนระบบเดิม) |

### 3.4 ข้อเสนอแก้ core ที่ agent เสนอและยังไม่ได้ทำ
| ข้อเสนอ | สถานะ |
|---|---|
| `ApprovalRequestStatus.Consumed = 5` ใน enum กลาง (ตอนนี้เป็นค่าคงที่ใน module approvals; ความหมายของ 5 ยังเป็นคำถามเปิด A5) | ยังไม่ทำ — รอทีมยืนยันความหมาย |
| `UsersService.findByAdUserNames` (WFE อ่าน `EMS.UserAccount` เองชั่วคราว) | ยังไม่ทำ |
| deadlock retry เป็นตัวเลือกกลางของ `TransactionRunner` (WFE มีของตัวเอง) | ยังไม่ทำ — เสี่ยงทำผลข้างเคียงซ้ำ ต้องออกแบบก่อน |
| ชื่อ `Module` ใน audit เก่า (`Workflow`) กับ `WFE` | รอตัดสินใจ (ถ้ารายงานเดิมกรองตามชื่อ) |
| ค่า `dbo.SystemConfig` ใหม่ที่โค้ดอ่าน (เช่น `Dms.RecycleBin.AutoPurge`, `WorkflowEscalationEnabled`, key เกณฑ์โควตา) ไม่มีตัวสร้างค่าเริ่มต้น | โค้ดใช้ค่า default เมื่อไม่มี key |

### 3.5 ต้องทดสอบกับ SQL Server จริง (ยังไม่ได้ทำสิ่งใด)
รายการเต็มอยู่ท้ายเอกสาร module แต่ละไฟล์ (หัวข้อ "ต้องทดสอบกับ SQL Server จริง") — ที่เสี่ยงสุด:
- SQL ที่เขียนเองทั้งหมด (recursive CTE, `UPDLOCK/HOLDLOCK`, `OUTPUT INTO`, `OFFSET/FETCH`) และผลของ SP (`sp_InsertFolder`, `sp_SoftDeleteFolder`, `sp_CreateWFInstance`, `sp_CreateWFStepInstance` ที่ใช้ `TABLOCKX`)
- ชื่อคอลัมน์จริงของตารางที่ **ไม่มี DDL**: `storage.QuickAccessFolder`, `dbo.SystemConfig`, `EMS.UserAuthority`, `EMS.UserOrgUnitHiddenView`, `storage.ExternalShare`/`ShareToken` (บางส่วน)
- bigint ที่ mssql คืนเป็นสตริง · เขตเวลา (คอลัมน์ UTC ปนกับท้องถิ่น) · เลขรัน RGP ต้อง seed แถว `PRJ` และ `TK` ใน `RunningNumber`
- การอัปโหลดไฟล์ไทย/ไฟล์ใหญ่/`PhysicalPath` แบบ UNC ของระบบเดิม · ความเข้ากันของ hash รหัสผ่านลิงก์แชร์กับ .NET · Remark แบบเก่า (`ExternalShareID:<n>`) ของคำขอเดิม 40 รายการ
- ประสิทธิภาพ: `EMS.Notification` (525,748 แถว) ไม่มี index

### 3.6 คำถามเปิดที่ต้องให้ทีม/เจ้าของระบบตอบ (กระทบพฤติกรรมจริง)
1. **ต้องเปิดลิงก์แชร์สาธารณะ (ผู้ใช้ภายนอก) หรือไม่?** หน้าจอเดิมเป็น dead code (OPEN_QUESTIONS B1) — API ทำ endpoint ให้แล้ว 2 ตัว (`/dms/public/shares/{token}/open|download`) ถ้าไม่ต้องการให้ปิด/ลบ
2. WinForms จะเปลี่ยนมาเรียก API หรือใช้ SP ตรงควบคู่? (B3) — มีผลกับรูปแบบ `Remark` ของ approval ที่ API เก็บเป็น JSON (`CMS-PAYLOAD/1:`) แต่ยังอ่านรูปแบบเก่าได้
3. `BisDbConnectionString` คือฐานเดียวกับ CMS_DB หรือไม่? (B8) — RGP ใช้ตาราง `EMS.m_*` ของฐานหลักแทน
4. ความหมาย `RequestStatus = 5` และสถานะ "ถอนคำขอ" (A5, C5)
5. สิทธิ์สมาชิกโครงการ RGP: ใครที่ login ก็แก้ได้ (ไม่ตรวจว่าเป็นสมาชิก) — ต้องการเข้มกว่านี้หรือไม่
6. โครงการ RGP "Completed" เมื่อไม่มี state ที่จำเป็นเลย — คงพฤติกรรมเดิมหรือแก้
7. อายุคำขอดาวน์โหลดที่อนุมัติแล้ว, นิยาม "การใช้พื้นที่" โควตา 2 แบบ, key เกณฑ์เตือนโควตา, ความหมายคำขอหลาย step
8. DDL ที่ขาด A1–A3 (`QuickAccessFolder`, `SystemConfig`, key ใน SystemConfig) และ A4 (ความหมาย `EmploymentType/Status`)

## 4. สิ่งที่แนะนำเพิ่ม (สรุป — ฉบับเต็มพร้อมหลักฐานอยู่ที่ `docs/RECOMMENDATIONS.md` ใน repo API)

**P0 — ก่อนขึ้น production**
- **ทดสอบกับ SQL Server จริง** + job CI ที่ขึ้น SQL Server container พร้อม DDL (O-01)
- **สร้าง index ที่ขาด**: `EMS.Notification` ไม่มี PK/index เลย (inbox ค้นด้วย `RecipientUserId` จะ scan ทั้งตาราง) · `KMS.t_KBArticle` มีแต่ PK+UQ(Code) → ใช้ `docs/db/001_recommended_indexes.sql` (ให้ DBA ตรวจก่อน)
- **ปิด DDL ที่ขาด** (A1–A3) แล้ว `npm run codegen` ใหม่ (D-03)
- **จัดการความลับ**: rotate key ที่เคยฝังในซอร์สเดิม (`NotificationApiKey`), ใช้ secret manager (S-01)
- **ตัดสินใจเรื่องลิงก์แชร์สาธารณะ** + ความแข็งแรงของ hash รหัสผ่าน (S-05)
- **ที่เก็บไฟล์**: mount SMB share ให้ตรงกับ `STORAGE_ROOT` หรือย้ายไป object storage (O-04); **สแกนไวรัส** ถ้ารับอัปโหลดวงกว้าง (S-04)
- **ถ้ารัน > 1 instance**: Redis cache, distributed lock ของ `@Cron` (escalation/SLA/purge), ย้ายสถานะ job import KMS ออกจาก memory (O-03)
- ตั้ง `TRUST_PROXY`, CORS origin จริง, TLS ที่ reverse proxy (S-06)
- ถ้า WinForms ยังเขียน DB ควบคู่ ต้องกำหนดเจ้าของการเขียนต่อตาราง + parity test กับระบบเดิม (F-05)

**P1**: refresh token/เพิกถอน JWT (S-02) · lockout ต่อบัญชี (S-03) · PDPA export/ลบข้อมูล (S-07) · นโยบาย retention ของ Notification/AuditLog (D-06) · สคริปต์ DDL เลขลำดับแบบ MOD-006 (D-04) · Prometheus/OpenTelemetry (O-02) · load test (O-07) · สร้าง client อัตโนมัติ (NSwag/openapi-typescript) + ตรวจ breaking change (F-04) · Husky/CODEOWNERS/PR template (Q-01, Q-02)

**P2**: SSE/WebSocket แจ้งเตือน (F-01) · idempotency key (F-02) · ส่งออก Excel (F-03) · Full-Text Search KMS (D-07) · optimistic concurrency (D-09) · อัปเกรด Nest 12/TypeORM 1.x (Q-04)

## 5. ขั้นตอนต่อไปที่แนะนำ (ลำดับ)
1. แก้ import `Global` → รันตรวจให้เขียว → commit + push `main` (กันงานหาย) — ข้อ 3.1
2. ตรวจ/ทำ KMS ให้จบ แล้ว merge + push — ข้อ 3.2
3. ขอ DDL ที่ขาดจาก DBA แล้วรัน `npm run codegen`; ให้ DBA ตรวจสคริปต์ index — ข้อ 4 (P0)
4. ตั้งฐานทดสอบ SQL Server แล้วเดินรายการ "ต้องทดสอบกับ DB จริง" ของทุก module (เริ่มจาก Auth → Shared → DMS core → approvals) — ข้อ 3.5
5. ประชุมตอบคำถามเปิดข้อ 3.6 (โดยเฉพาะลิงก์แชร์สาธารณะและบทบาทของ WinForms ช่วงเปลี่ยนผ่าน)
6. ทำรายการ P0 ที่เหลือ (ความลับ, ที่เก็บไฟล์, multi-instance) ก่อน staging

## 6. วิธีตรวจซ้ำ (สำหรับผู้ทำต่อ)
```bash
git clone https://github.com/paschayach-jpg/core-management-system-nest-api && cd core-management-system-nest-api
npm ci
npm run typecheck && npm run lint && npm run format:check && npm test && npm run test:e2e && npm run build
npm run openapi:export        # นับ endpoint จริงจาก openapi.json
```
เอกสารที่ควรอ่านก่อนทำต่อ: `README.md` → `docs/CONVENTIONS.md` (สูตรเขียน module) → `docs/DECISIONS.md` (ส่วนที่ต่างจากระบบเดิม) → `docs/modules/<module>.md`
