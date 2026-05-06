---
name: wfe-engine
description: "WFE v2.0 Engine implementation guide — WorkflowOrchestrator, ConditionEvaluator, PluginSystem, OCC, StepGraph, AuditHashChain. Use when implementing or extending Workflow Engine services, repos, or domain models."
---

# WFE v2.0 Engine — Coding Guide

> อ้างอิง: `SDD-WFE-v2.0.md`, `SRS-WFE-v2.0.md`, `WFE-Database.md`, `WFE-Wireframe.md`
> UI pattern แยกต่างหาก → ใช้ `/wfe-form-ui`

---

## 1. ความแตกต่าง v1.0 → v2.0 (GAP ที่ต้องระวัง)

| ส่วน | Current Code (v1.0) | SDD v2.0 Target |
|------|---------------------|-----------------|
| `IWorkflowService` | Synchronous + `UserContext` | Async (`StartAsync`, `CompleteStepAsync`) + `StartWorkflowDto` |
| `WorkflowInstance` | `TemplateVersion` (int), `CurrentStepId` (int FK) | `FrozenDefJSON`, `CurrentStepCode` (varchar), `RefType`+`RefId`, `TenantId`, `RowVer` |
| Condition Evaluation | ไม่มี evaluator จริง | `ConditionEvaluator` (NCalc library) |
| AutoTask | ไม่มี plugin concept | `IWorkflowPlugin` + `PluginRegistry` |
| Concurrency | ไม่มี OCC | rowversion `RowVer` + `ConcurrencyGuard` |
| Cache | `WorkflowTemplateCacheImpl` (มีแล้ว) | `StepGraphCache` (MemoryCache 30 min) |
| AuditLog | ADUserName snapshot (มีแล้ว) | + HashChain SHA256 ทุก row |
| Linked Document | LinkedFileID/TaskID/FolderID | `RefType` (VARCHAR) + `RefId` (INT) |

---

## 2. Namespace Structure (ที่ต้องสร้าง/ย้าย)

```
CMS.Services/
└── Workflow/                         ← namespace ใหม่ (ย้ายออกจาก root)
    ├── IWorkflowService.cs           (update signature)
    ├── IDefinitionService.cs         (ใหม่)
    ├── Engine/
    │   ├── WorkflowOrchestrator.cs   (ใหม่ — core)
    │   ├── StateMachine.cs           (ใหม่)
    │   ├── ConcurrencyGuard.cs       (ใหม่ — OCC)
    │   └── NotificationDispatcher.cs (ใหม่ — bridge MOD-007)
    ├── Definition/
    │   ├── DefinitionService.cs      (ใหม่)
    │   ├── DefinitionParser.cs       (ใหม่)
    │   ├── DefinitionValidator.cs    (ใหม่)
    │   ├── VersionManager.cs         (ใหม่)
    │   └── StepGraphCache.cs         (refactor จาก WorkflowTemplateCacheImpl)
    ├── Rules/
    │   ├── IRuleEngine.cs            (ใหม่)
    │   ├── RuleEngine.cs             (ใหม่)
    │   ├── ConditionEvaluator.cs     (ใหม่ — NCalc)
    │   ├── SlaCalculator.cs          (ใหม่)
    │   └── EscalationChecker.cs      (refactor จาก WFESLABackgroundJob)
    └── Plugins/
        ├── IWorkflowPlugin.cs        (ใหม่)
        ├── PluginRegistry.cs         (ใหม่)
        ├── PluginContext.cs          (ใหม่)
        ├── SendNotificationPlugin.cs (ใหม่)
        ├── CallRiscServerPlugin.cs   (ใหม่)
        ├── CalcPremiumPlugin.cs      (ใหม่)
        └── SendOicReportPlugin.cs    (ใหม่)

CMS.Domain/
└── Models/Workflow/
    ├── WorkflowDefinition.cs   (update — เพิ่ม IsActivated, TenantId)
    ├── WorkflowInstance.cs     (update — เพิ่ม FrozenDefJSON, RefType, RefId, TenantId, RowVer)
    ├── WorkflowStep.cs         (update — เพิ่ม PluginCode, TransitionsJSON, AssigneeRole)
    ├── WorkflowStepInstance.cs (update — เพิ่ม DelegatedBy, TenantId, StepCode)
    ├── StepGraph.cs            (ใหม่ — in-memory parsed model)
    ├── StepNode.cs             (ใหม่)
    ├── TransitionRule.cs       (ใหม่)
    ├── EscalationConfig.cs     (ใหม่)
    └── RuleContext.cs          (ใหม่)

CMS.Services/DTOs/ (update/เพิ่ม)
    ├── StartWorkflowDto.cs     (ใหม่ — แทน CreateWorkflowInstanceRequest)
    ├── CompleteStepDto.cs      (ใหม่ — แทน CompleteWorkflowStepRequest)
    └── CancelInstanceDto.cs    (ใหม่)
```

---

## 3. Domain Models ที่ต้องเพิ่ม/แก้

### WorkflowInstance — fields ที่ขาด
```csharp
// เพิ่มใน CMS.Domain/Models/WorkflowInstance.cs
public string FrozenDefJSON { get; set; }     // snapshot ตอน StartInstance
public string RefType { get; set; }           // "MOTOR_CLAIM", "LEAVE_REQUEST"
public int? RefId { get; set; }              // PK ของ source document
public string CurrentStepCode { get; set; }  // แทน CurrentStepId
public int TenantId { get; set; }
public byte[] RowVer { get; set; }           // rowversion OCC
public string TriggerByADUser { get; set; }  // แทน InitiatedBy (naming align กับ SDD)
public string ContextDataJSON { get; set; }  // runtime variables
```

### StepGraph — in-memory model (ใหม่ทั้งหมด)
```csharp
public class StepGraph
{
    public string DefinitionCode { get; set; }
    public string RawJson { get; set; }              // เก็บไว้สำหรับ FrozenDefJSON
    public int DefinitionId { get; set; }
    public Dictionary<string, StepNode> Steps { get; init; }

    public StepNode GetStartStep() => Steps.Values.First(s => s.StepType == "START");
    public StepNode GetStep(string code) => Steps[code];

    public static StepGraph Parse(string json)       // parse FrozenDefJSON → StepGraph
    {
        // deserialize JSON → StepGraph + build Steps dictionary
        throw new NotImplementedException();
    }
}

public class StepNode
{
    public string StepCode { get; set; }
    public string StepType { get; set; }             // "HumanTask","AutoTask","DECISION","PARALLEL","MERGE","END"
    public string AssigneeRole { get; set; }
    public int SLAHours { get; set; }
    public string PluginCode { get; set; }
    public EscalationConfig Escalation { get; set; }
    public List<TransitionRule> Transitions { get; set; }
}

public class TransitionRule
{
    public string Condition { get; set; }            // NCalc expression
    public string NextStep { get; set; }
}

public class EscalationConfig
{
    public int AfterHours { get; set; }
    public string EscalateTo { get; set; }           // Role หรือ ADUserName
}

public class RuleContext
{
    public string Action { get; set; }               // "APPROVE","REJECT","FORWARD"
    public Dictionary<string, object> Variables { get; set; }
}
```

---

## 4. IWorkflowService — Signature ใหม่

```csharp
// แทนที่ IWorkflowService เดิม
public interface IWorkflowService
{
    Task<ServiceResult<WorkflowInstanceDto>> StartAsync(StartWorkflowDto dto);
    Task<ServiceResult<bool>> CompleteStepAsync(CompleteStepDto dto);
    Task<ServiceResult<bool>> CancelInstanceAsync(CancelInstanceDto dto);
    Task<WorkflowInstanceDetailDto> GetInstanceStatusAsync(int instanceId, int tenantId);
    Task<ServiceResult<bool>> DelegateStepAsync(DelegateStepDto dto);
    Task<WFDashboardDto> GetMonitorDashboardAsync(int tenantId);
    Task<List<SLAReportRowDto>> GetSLAReportAsync(DateTime from, DateTime to, int tenantId);
}

public class StartWorkflowDto
{
    public string DefinitionCode { get; set; }
    public string RefType { get; set; }
    public int RefId { get; set; }
    public string TriggerByADUser { get; set; }
    public int TriggerType { get; set; }             // 1=Manual,2=Auto,3=Scheduled
    public Dictionary<string, object> InitialContext { get; set; }
    public int TenantId { get; set; }
}

public class CompleteStepDto
{
    public int StepInstanceID { get; set; }
    public string Decision { get; set; }             // "APPROVE","REJECT","FORWARD"
    public string Comment { get; set; }
    public string ADUserName { get; set; }
    public Dictionary<string, object> UpdatedContext { get; set; }
    public int TenantId { get; set; }
}
```

---

## 5. Plugin System

```csharp
// CMS.Services/Workflow/Plugins/IWorkflowPlugin.cs
public interface IWorkflowPlugin
{
    string PluginCode { get; }
    Task<PluginResult> ExecuteAsync(PluginContext ctx, CancellationToken ct);
}

public class PluginContext
{
    public int InstanceId { get; set; }
    public string InstanceCode { get; set; }
    public string CurrentStepCode { get; set; }
    public string RefType { get; set; }
    public int? RefId { get; set; }
    public string TriggerByADUser { get; set; }
    public Dictionary<string, object> Variables { get; set; }
    public int TenantId { get; set; }
}

public class PluginResult
{
    public bool Success { get; set; }
    public string ErrorMessage { get; set; }
    public Dictionary<string, object> OutputContext { get; set; }
}

// PluginRegistry — ลงทะเบียนตอน Application Start
public class PluginRegistry
{
    private readonly Dictionary<string, IWorkflowPlugin> _plugins = new();

    public PluginRegistry Register(IWorkflowPlugin plugin)
    {
        _plugins[plugin.PluginCode] = plugin;
        return this;
    }

    public IWorkflowPlugin Get(string code)
    {
        if (!_plugins.TryGetValue(code, out var p))
            throw new WorkflowPluginNotFoundException($"Plugin '{code}' ไม่ได้ลงทะเบียน");
        return p;
    }
}
```

**Built-in Plugin Codes:** `SEND_NOTIFICATION`, `CALL_RISC_SERVER`, `CALC_PREMIUM`, `SEND_OIC_REPORT`

---

## 6. ConditionEvaluator (NCalc)

```csharp
// NuGet: NCalc2 (สำหรับ .NET 4.5)
// Install-Package NCalc2

public class ConditionEvaluator
{
    public bool Evaluate(string condition, RuleContext ctx)
    {
        if (string.IsNullOrWhiteSpace(condition) || condition.Trim() == "true") return true;
        if (condition.Trim() == "false") return false;

        var expr = new Expression(condition);
        foreach (var kv in ctx.Variables ?? new Dictionary<string, object>())
            expr.Parameters["ctx." + kv.Key] = kv.Value;
        expr.Parameters["action"] = ctx.Action ?? string.Empty;

        try {
            return Convert.ToBoolean(expr.Evaluate());
        } catch (Exception ex) {
            throw new WorkflowConditionException($"Condition error: '{condition}' — {ex.Message}");
        }
    }
}
```

**ตัวอย่าง condition strings:**
- `"true"` — pass เสมอ
- `"ctx.claimAmount <= 50000"` — routing ตาม variable
- `"action == 'APPROVE'"` — routing ตาม decision
- `"ctx.claimAmount > 50000 && ctx.policyType == 'MOTOR'"` — compound

---

## 7. OCC (Optimistic Concurrency Control)

```csharp
public class ConcurrencyGuard
{
    private const int MAX_RETRIES = 3;

    public async Task<T> ExecuteWithRetryAsync<T>(Func<Task<T>> op)
    {
        int attempts = 0;
        while (true) {
            try {
                return await op();
            } catch (ConcurrencyException) {
                if (++attempts >= MAX_RETRIES) throw;
                await Task.Delay(200 * attempts);
                // Reload latest RowVer ก่อน retry
            }
        }
    }
}
```

**SP pattern:** `UPDATE t_WorkflowInstance SET ... WHERE InstanceID=@id AND RowVer=@expectedRowVer AND TenantId=@tenant`
ถ้า `@@ROWCOUNT = 0` → return `@RowsAffected = 0` → Application throw `ConcurrencyException`

---

## 8. StepGraphCache

```csharp
public class StepGraphCache
{
    private static readonly ObjectCache _cache = MemoryCache.Default;
    private const int TTL_MINUTES = 30;

    public StepGraph Get(string code, int tenantId) =>
        _cache[$"StepGraph_{tenantId}_{code}"] as StepGraph;

    public void Set(string code, int tenantId, StepGraph graph)
    {
        var policy = new CacheItemPolicy {
            AbsoluteExpiration = DateTimeOffset.Now.AddMinutes(TTL_MINUTES)
        };
        _cache.Set($"StepGraph_{tenantId}_{code}", graph, policy);
    }

    public void Invalidate(string code, int tenantId) =>
        _cache.Remove($"StepGraph_{tenantId}_{code}");
}
```

**Invalidate เมื่อ:** ActivateVersion() — ให้ call `StepGraphCache.Invalidate()` หลัง activate เสมอ

---

## 9. AuditLog HashChain

```csharp
// ทุก WFE audit log ต้องมี HashChain
private string ComputeHash(AuditLogEntry entry)
{
    var content = $"{entry.EntityType}|{entry.EntityID}|{entry.Action}|{entry.ADUserName}|{entry.LogDate:O}|{entry.PrevHashChain}";
    using (var sha = SHA256.Create())
    {
        var bytes = sha.ComputeHash(Encoding.UTF8.GetBytes(content));
        return BitConverter.ToString(bytes).Replace("-", "").ToLower();
    }
}

// Action Codes ที่ใช้:
// INSTANCE_STARTED, STEP_COMPLETED, STEP_ESCALATED
// INSTANCE_COMPLETED, INSTANCE_CANCELLED, INSTANCE_FAULTED
// STEP_DELEGATED, DEFINITION_ACTIVATED
```

---

## 10. Business Rules Checklist (ต้อง verify ก่อน implement)

| Rule | รายละเอียด |
|------|-----------|
| BR-008-01 | FrozenDefJSON snapshot ทันที StartInstance — ห้ามใช้ live definition |
| BR-008-02 | SLA คำนวณเฉพาะ Working Hours (08:30-17:30, ไม่รวม weekend+holiday จาก `s_SystemCalendar`) |
| BR-008-03 | Escalation: Windows Service poll ทุก 5 นาที → `IsEscalated=1` |
| BR-008-04 | REJECT ต้อง require Comment (validation บน UI และ Service) |
| BR-008-05 | Cancel ต้อง require Reason, Manager/Admin เท่านั้น, ห้าม cancel Status=Completed |
| BR-008-08 | Definition Immutable หลัง `IsActivated=1` — แก้ไขต้องสร้าง Version ใหม่ |
| BR-008-09 | ConditionEvaluator = pure function ห้าม I/O |
| BR-008-10 | OCC conflict → retry สูงสุด 3 ครั้ง |
| BR-008-11 | Plugin fail → Instance = Faulted, แจ้ง Admin ภายใน 5 นาที |
| BR-008-12 | AuditLog = append-only, HashChain SHA256 ทุก row, เก็บ ≥ 5 ปี |
| BR-008-13 | TenantId ต้อง filter ทุก query ห้ามข้าม Tenant |
| BR-008-14 | Validator ก่อน Activate: ต้องมี START node, ≥1 END node, ไม่มี circular, plugin ต้องลงทะเบียนใน s_WorkflowPlugin |

---

## 11. Database — Tables ที่ต้องเพิ่ม/แก้ใน Schema

### Tables ใหม่ทั้งหมด
- `s_WorkflowPlugin` — Plugin Registry
- `s_WFDelegation` — การ delegate สิทธิ์

### Columns ที่ต้องเพิ่มใน t_WorkflowInstance
```sql
FrozenDefJSON NVARCHAR(MAX) NOT NULL,
RefType       VARCHAR(50)   NULL,
RefId         INT           NULL,
TenantId      INT           NOT NULL DEFAULT(1),
RowVer        ROWVERSION    NOT NULL,
CurrentStepCode VARCHAR(50) NULL,   -- แทน CurrentStepId
TriggerByADUser NVARCHAR(100) NOT NULL,
ContextDataJSON NVARCHAR(MAX) NULL,
```

### Columns ที่ต้องเพิ่มใน m_WorkflowDefinition
```sql
IsActivated BIT NOT NULL DEFAULT(0),
TenantId    INT NOT NULL DEFAULT(1),
```

### Columns ที่ต้องเพิ่มใน t_WorkflowStepInstance
```sql
DelegatedBy  NVARCHAR(100) NULL,
TenantId     INT NOT NULL DEFAULT(1),
StepCode     VARCHAR(50) NOT NULL,
StepType     VARCHAR(20) NOT NULL,
AssignedADUser NVARCHAR(100) NULL,
```

### l_AuditLog — เพิ่ม HashChain
```sql
HashChain     CHAR(64) NULL,
PrevHashChain CHAR(64) NULL,
```

**Script migration:** ดู `WFE-Database.md` section 10 สำหรับ DDL ครบถ้วน

---

## 12. Stored Procedures หลัก

| SP | หน้าที่ |
|----|---------|
| `sp_WF_StartInstance` | INSERT t_WorkflowInstance + generate InstanceCode |
| `sp_WF_CompleteStep` | UPDATE StepInstance + OCC check RowVer |
| `sp_WF_EscalateOverdue` | SET IsEscalated=1 สำหรับ steps ที่เกิน DueDate |
| `sp_WF_CancelInstance` | SET Status=Cancelled + Reason |
| `sp_WF_ActivateDefinition` | SET IsActivated=1, deactivate เวอร์ชั่นเก่า |
| `sp_WF_GetMonitorDashboard` | Summary + Running instances |
| `sp_WF_GetSLAReport` | SLA Compliance report |
| `fn_AddWorkingHours` | คำนวณ DueDate ตาม Business Hours + Calendar |

---

## 13. Wireframe ↔ Form Mapping

| Wireframe | Form ที่มีอยู่ | สถานะ |
|-----------|---------------|-------|
| WFE-01: Workflow Designer | `WorkflowDesignerForm.cs` | มี — ต้องเพิ่ม Plugin support |
| WFE-02: Instance Detail | `WorkflowInstanceDetailForm.cs` | มี |
| WFE-03: My Tasks | `WorkflowDashboardFormNew.cs` | มี — master reference |
| WFE-04: Complete Step | `WorkflowStepActionForm.cs` (Dialogs) | มี |
| WFE-05: Monitor Dashboard | `WorkflowMonitorForm.cs` | มี |
| WFE-06: SLA Report | `SLAReportForm.cs` | มี |
| WFE-07: Plugin Registry | **ยังไม่มี** — ต้องสร้าง `PluginRegistryForm.cs` |
| WFE-08: Start Workflow | `WorkflowStartDialog.cs` (Dialogs) | มี |
| WFE-09: Definition List | `WorkflowTemplateListForm.cs` | มี |
| WFE-10: Delegate | `DelegationDialog.cs` (Dialogs) | มี |

---

## 14. Integration Points

| Module | Bridge Class | หน้าที่ |
|--------|-------------|---------|
| MOD-009 Task | `TaskAssigner.cs` | สร้าง `t_TaskItem` เมื่อ HumanTask step เริ่ม |
| MOD-007 Notification | `NotificationDispatcher.cs` | แจ้งเตือน Assignee + Escalation + Fault |
| MOD-021 AuditLog | `AuditHelper.cs` | HashChain audit ทุก action |
| MOD-023 Integration | Phase 2 REST API | ปัจจุบัน Direct Reference ก่อน |

---

## 15. ลำดับการ Implement (แนะนำ)

1. **Domain Models** — เพิ่ม fields ใน existing models + สร้าง StepGraph, StepNode, TransitionRule
2. **DB Migration** — run SQL script เพิ่ม columns + สร้าง s_WorkflowPlugin, s_WFDelegation
3. **ConditionEvaluator** — NCalc (test ก่อนด้วย unit test)
4. **StepGraphCache** — MemoryCache wrapper
5. **PluginSystem** — IWorkflowPlugin, PluginRegistry, built-in plugins
6. **WorkflowOrchestrator** — core engine (StartAsync, CompleteStepAsync, ProcessStepAsync)
7. **ConcurrencyGuard** — OCC retry
8. **EscalationChecker** — refactor WFESLABackgroundJob
9. **AuditHelper** — เพิ่ม HashChain
10. **UI forms** — PluginRegistryForm (form เดียวที่ยังขาด)
