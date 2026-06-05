using System;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace CMS.Services.Workflow.Engine
{
    // ─── Status enums ──────────────────────────────────────────────────────
    public enum WfeInstanceStatus
    {
        Running   = 1,
        Completed = 2,
        Cancelled = 3,
        Faulted   = 4
    }

    public enum WfeStepStatus
    {
        Active    = 1,
        Completed = 2,
        Escalated = 3
    }

    // ─── Record DTOs (mapping to DB tables) ────────────────────────────────
    public class WfeInstanceRecord
    {
        public int      InstanceId      { get; set; }
        public string   InstanceCode    { get; set; }
        public string   DefinitionCode  { get; set; }
        public string   FrozenDefJson   { get; set; }  // BR-008-01
        public string   RefType         { get; set; }
        public int      RefId           { get; set; }
        public string   TriggerByADUser { get; set; }
        public int      TriggerType     { get; set; }
        public WfeInstanceStatus Status { get; set; }
        public DateTime StartedDate     { get; set; }
        public DateTime? CompletedDate  { get; set; }
        public string   CurrentStepCode { get; set; }
        public string   ContextJson     { get; set; }
        public int      TenantId        { get; set; }
        public byte[]   RowVer          { get; set; }  // BR-008-10: OCC
    }

    public class WfeStepInstanceRecord
    {
        public int      StepInstanceId  { get; set; }
        public int      InstanceId      { get; set; }
        public string   StepCode        { get; set; }
        public string   StepType        { get; set; }
        public string   AssignedTo      { get; set; }
        public DateTime StartedDate     { get; set; }
        public DateTime? DueDate        { get; set; }
        public DateTime? CompletedDate  { get; set; }
        public string   CompletedBy     { get; set; }
        public string   Decision        { get; set; }
        public string   Comment         { get; set; }
        public WfeStepStatus Status     { get; set; }
        public bool     IsEscalated     { get; set; }
        public string   EscalateToUser  { get; set; }
        public int?     EscalationHours { get; set; }
        public int      TenantId        { get; set; }
        public byte[]   RowVer          { get; set; }
    }

    public class WfeAuditEntry
    {
        public int    InstanceId { get; set; }
        public string StepCode   { get; set; }
        public string Action     { get; set; }  // INSTANCE_STARTED, STEP_COMPLETED, etc.
        public string ADUserName { get; set; }  // snapshot (BR-008-12)
        public string Detail     { get; set; }
        public int    TenantId   { get; set; }
    }

    // ─── Repository interface (implemented in CMS.Data layer) ───────────────
    public interface IWfeRepository
    {
        // Definition
        Task<string> GetDefinitionJsonAsync(string definitionCode, int tenantId);
        Task<int>    SaveDefinitionAsync(string definitionCode, string name, string json, string savedBy, int tenantId);
        Task         ActivateDefinitionAsync(int definitionId, string approvedBy, int tenantId);

        // Instance
        Task<int>    CreateInstanceAsync(WfeInstanceRecord record);
        Task         UpdateInstanceCodeAsync(int instanceId, string instanceCode);
        Task         UpdateInstanceCurrentStepAsync(int instanceId, string stepCode, int tenantId);
        Task         CompleteInstanceAsync(int instanceId, int tenantId);
        Task         CancelInstanceAsync(int instanceId, string reason, string cancelledBy, int tenantId);
        Task         FaultInstanceAsync(int instanceId, int tenantId);
        Task<WfeInstanceRecord> GetInstanceAsync(int instanceId, int tenantId);

        // StepInstance
        Task<int>    CreateStepInstanceAsync(WfeStepInstanceRecord record);
        Task<WfeStepInstanceRecord> GetStepInstanceAsync(int stepInstanceId, int tenantId);
        // BR-008-10: OCC — returns false if rowversion mismatch
        Task<bool>   TryCompleteStepAsync(int stepInstanceId, string decision, string comment, string completedBy, byte[] expectedRowVer);

        // SLA — fn_AddWorkingHours (BR-008-02)
        Task<DateTime> CalcDueDateAsync(DateTime startTime, int slaHours, int tenantId);

        // AuditLog — BR-008-12: append-only + HashChain
        Task AppendAuditLogAsync(WfeAuditEntry entry);
    }
}
