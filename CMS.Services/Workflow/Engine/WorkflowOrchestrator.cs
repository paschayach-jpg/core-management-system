using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;
using CMS.Domain.Models.Workflow;
using CMS.Services.Workflow.Definition;
using CMS.Services.Workflow.DTOs;
using CMS.Services.Workflow.Plugins;
using CMS.Services.Workflow.Rules;

namespace CMS.Services.Workflow.Engine
{
    // SRS §11.1 StartWorkflow, §11.2 CompleteStep — main orchestration engine
    public class WorkflowOrchestrator
    {
        private readonly IStepGraphCache    _cache;
        private readonly IDefinitionParser  _parser;
        private readonly IPluginRegistry    _pluginRegistry;
        private readonly IConditionEvaluator _conditionEvaluator;
        private readonly IConcurrencyGuard  _concurrencyGuard;
        private readonly IWfeRepository     _repository;

        public WorkflowOrchestrator(
            IStepGraphCache     cache,
            IDefinitionParser   parser,
            IPluginRegistry     pluginRegistry,
            IConditionEvaluator conditionEvaluator,
            IConcurrencyGuard   concurrencyGuard,
            IWfeRepository      repository)
        {
            _cache              = cache;
            _parser             = parser;
            _pluginRegistry     = pluginRegistry;
            _conditionEvaluator = conditionEvaluator;
            _concurrencyGuard   = concurrencyGuard;
            _repository         = repository;
        }

        // ─── SRS §11.1: StartAsync ──────────────────────────────────────────
        public async Task<ServiceResult<WorkflowInstanceDto>> StartAsync(
            StartWorkflowDto dto, CancellationToken ct = default(CancellationToken))
        {
            if (dto == null)
                return ServiceResult<WorkflowInstanceDto>.Fail("WF-E001", "StartWorkflowDto is null");

            // 1. Load Definition & get StepGraph (from Cache or DB)
            var graph = await GetOrLoadGraphAsync(dto.DefinitionCode, dto.TenantId);
            if (graph == null)
                return ServiceResult<WorkflowInstanceDto>.Fail("WF-E005",
                    string.Format("Workflow Definition '{0}' ไม่พบหรือยังไม่ได้ Activate", dto.DefinitionCode));

            // 2. Load raw DefinitionJSON for FrozenDefJSON snapshot (BR-008-01)
            var frozenJson = await _repository.GetDefinitionJsonAsync(dto.DefinitionCode, dto.TenantId);

            // 3. Create WorkflowInstance (Status=Running, FrozenDefJSON)
            var instance = new WfeInstanceRecord
            {
                DefinitionCode   = dto.DefinitionCode,
                FrozenDefJson    = frozenJson,
                RefType          = dto.RefType,
                RefId            = dto.RefId,
                TriggerByADUser  = dto.TriggerByADUser,
                TriggerType      = dto.TriggerType,
                Status           = WfeInstanceStatus.Running,
                StartedDate      = DateTime.Now,
                TenantId         = dto.TenantId,
                ContextJson      = SerializeContext(dto.InitialContext)
            };

            instance.InstanceId = await _repository.CreateInstanceAsync(instance);
            instance.InstanceCode = GenerateInstanceCode(instance.InstanceId);
            await _repository.UpdateInstanceCodeAsync(instance.InstanceId, instance.InstanceCode);

            // 4. Audit log — INSTANCE_STARTED
            await _repository.AppendAuditLogAsync(new WfeAuditEntry
            {
                InstanceId  = instance.InstanceId,
                Action      = "INSTANCE_STARTED",
                ADUserName  = dto.TriggerByADUser,
                Detail      = string.Format("DefinitionCode={0}, RefType={1}, RefId={2}",
                                            dto.DefinitionCode, dto.RefType, dto.RefId),
                TenantId    = dto.TenantId
            });

            // 5. Process first step (START node → auto-proceed to first real step)
            await ProcessNextStepAsync(instance, graph, graph.StartStepCode, dto.InitialContext, dto.TriggerByADUser, ct);

            return ServiceResult<WorkflowInstanceDto>.Ok(new WorkflowInstanceDto
            {
                InstanceId      = instance.InstanceId,
                InstanceCode    = instance.InstanceCode,
                DefinitionCode  = dto.DefinitionCode,
                Status          = instance.Status.ToString(),
                CurrentStepCode = instance.CurrentStepCode,
                StartedDate     = instance.StartedDate,
                RefType         = dto.RefType,
                RefId           = dto.RefId
            });
        }

        // ─── SRS §11.2: CompleteStepAsync ───────────────────────────────────
        public async Task<ServiceResult<bool>> CompleteStepAsync(
            CompleteStepDto dto, CancellationToken ct = default(CancellationToken))
        {
            // BR-008-04: Reject ต้อง Comment
            if (dto.Decision == "REJECT" && string.IsNullOrWhiteSpace(dto.Comment))
                return ServiceResult<bool>.Fail("WF-E010", "กรุณาระบุเหตุผลในการปฏิเสธ (WF-E010)");

            return await _concurrencyGuard.ExecuteWithRetryAsync(async () =>
            {
                // 1. Load StepInstance + InstanceRecord
                var stepInst = await _repository.GetStepInstanceAsync(dto.StepInstanceId, dto.TenantId);
                if (stepInst == null)
                    return ServiceResult<bool>.Fail("WF-E004", "ไม่พบ Step Instance ที่ระบุ");

                var instance = await _repository.GetInstanceAsync(stepInst.InstanceId, dto.TenantId);
                if (instance == null)
                    return ServiceResult<bool>.Fail("WF-E004", "ไม่พบ Workflow Instance");

                // 2. OCC check via rowversion (BR-008-10)
                bool updated = await _repository.TryCompleteStepAsync(
                    dto.StepInstanceId, dto.Decision, dto.Comment, dto.ADUserName, stepInst.RowVer);

                if (!updated)
                    throw new ConcurrencyException("OCC conflict on StepInstance (WF-E007)");

                // 3. Audit log — STEP_COMPLETED
                await _repository.AppendAuditLogAsync(new WfeAuditEntry
                {
                    InstanceId = instance.InstanceId,
                    StepCode   = stepInst.StepCode,
                    Action     = "STEP_COMPLETED",
                    ADUserName = dto.ADUserName,
                    Detail     = string.Format("Decision={0}, Comment={1}", dto.Decision, dto.Comment),
                    TenantId   = dto.TenantId
                });

                // 4. Load graph and resolve next step
                var context = MergeContext(
                    DeserializeContext(instance.ContextJson),
                    dto.UpdatedContext);

                var graph = await GetOrLoadGraphAsync(instance.DefinitionCode, dto.TenantId);
                var currentNode = graph.GetNode(stepInst.StepCode);

                var nextStepCode = ResolveNextStep(currentNode, context, dto.Decision);

                // 5. Process next step
                await ProcessNextStepAsync(instance, graph, nextStepCode, context, dto.ADUserName, ct);

                return ServiceResult<bool>.Ok(true);
            });
        }

        // ─── CancelInstanceAsync ────────────────────────────────────────────
        public async Task<ServiceResult<bool>> CancelInstanceAsync(
            CancelInstanceDto dto, CancellationToken ct = default(CancellationToken))
        {
            // BR-008-05: Reason บังคับ, ห้าม Cancel ที่ Completed
            if (string.IsNullOrWhiteSpace(dto.Reason))
                return ServiceResult<bool>.Fail("WF-E011", "กรุณาระบุเหตุผลการยกเลิก");

            var instance = await _repository.GetInstanceAsync(dto.InstanceId, dto.TenantId);
            if (instance == null)
                return ServiceResult<bool>.Fail("WF-E004", "ไม่พบ Workflow Instance");

            if (instance.Status == WfeInstanceStatus.Completed)
                return ServiceResult<bool>.Fail("WF-E011", "ไม่สามารถยกเลิก Workflow ที่เสร็จสิ้นแล้วได้ (WF-E011)");

            await _repository.CancelInstanceAsync(dto.InstanceId, dto.Reason, dto.CancelledByADUser, dto.TenantId);

            await _repository.AppendAuditLogAsync(new WfeAuditEntry
            {
                InstanceId = dto.InstanceId,
                Action     = "INSTANCE_CANCELLED",
                ADUserName = dto.CancelledByADUser,
                Detail     = "Reason=" + dto.Reason,
                TenantId   = dto.TenantId
            });

            return ServiceResult<bool>.Ok(true);
        }

        // ─── Internal: Process next step ────────────────────────────────────
        private async Task ProcessNextStepAsync(
            WfeInstanceRecord instance,
            StepGraph graph,
            string stepCode,
            Dictionary<string, object> context,
            string triggerUser,
            CancellationToken ct)
        {
            if (string.IsNullOrEmpty(stepCode)) return;

            var node = graph.GetNode(stepCode);
            if (node == null) return;

            // START → auto-proceed ไปยัง step ถัดไปทันที (SRS §7)
            if (node.StepType == WfeStepTypes.Start)
            {
                var nextCode = ResolveNextStep(node, context, null);
                await ProcessNextStepAsync(instance, graph, nextCode, context, triggerUser, ct);
                return;
            }

            // END → Complete instance
            if (node.StepType == WfeStepTypes.End)
            {
                await _repository.CompleteInstanceAsync(instance.InstanceId, instance.TenantId);
                await _repository.AppendAuditLogAsync(new WfeAuditEntry
                {
                    InstanceId = instance.InstanceId,
                    StepCode   = stepCode,
                    Action     = "INSTANCE_COMPLETED",
                    ADUserName = triggerUser,
                    TenantId   = instance.TenantId
                });
                instance.Status = WfeInstanceStatus.Completed;
                return;
            }

            // AutoTask → execute plugin
            if (node.StepType == WfeStepTypes.AutoTask)
            {
                await ExecuteAutoTaskAsync(instance, node, context, triggerUser, graph, ct);
                return;
            }

            // HumanTask / DECISION / PARALLEL → สร้าง StepInstance
            var dueDate = node.SlaHours.HasValue
                ? await _repository.CalcDueDateAsync(DateTime.Now, node.SlaHours.Value, instance.TenantId)
                : (DateTime?)null;

            var stepInst = new WfeStepInstanceRecord
            {
                InstanceId  = instance.InstanceId,
                StepCode    = stepCode,
                StepType    = node.StepType,
                AssignedTo  = ResolveAssignee(node, context, instance.TriggerByADUser),
                StartedDate = DateTime.Now,
                DueDate     = dueDate,
                Status      = WfeStepStatus.Active,
                TenantId    = instance.TenantId,
                EscalateToUser   = node.Escalation?.EscalateTo,
                EscalationHours  = node.Escalation?.AfterHours
            };

            await _repository.CreateStepInstanceAsync(stepInst);
            await _repository.UpdateInstanceCurrentStepAsync(instance.InstanceId, stepCode, instance.TenantId);
            instance.CurrentStepCode = stepCode;

            await _repository.AppendAuditLogAsync(new WfeAuditEntry
            {
                InstanceId = instance.InstanceId,
                StepCode   = stepCode,
                Action     = "STEP_STARTED",
                ADUserName = triggerUser,
                Detail     = "AssignedTo=" + stepInst.AssignedTo,
                TenantId   = instance.TenantId
            });
        }

        // ─── AutoTask execution (FR-008-22) ─────────────────────────────────
        private async Task ExecuteAutoTaskAsync(
            WfeInstanceRecord instance,
            StepNode node,
            Dictionary<string, object> context,
            string triggerUser,
            StepGraph graph,
            CancellationToken ct)
        {
            IWorkflowPlugin plugin;
            try
            {
                plugin = _pluginRegistry.Resolve(node.PluginCode);
            }
            catch (KeyNotFoundException)
            {
                await FaultInstanceAsync(instance, node.StepCode,
                    string.Format("Plugin '{0}' not registered (WF-E008)", node.PluginCode), triggerUser);
                return;
            }

            var pluginCtx = new PluginContext
            {
                InstanceId  = instance.InstanceId,
                InstanceCode = instance.InstanceCode,
                StepCode    = node.StepCode,
                RefType     = instance.RefType,
                RefId       = instance.RefId,
                TenantId    = instance.TenantId,
                Variables   = context,
                PluginParams = node.PluginParams
            };

            PluginResult result;
            try
            {
                result = await plugin.ExecuteAsync(pluginCtx, ct);
            }
            catch (Exception ex)
            {
                await FaultInstanceAsync(instance, node.StepCode, ex.Message, triggerUser);
                return;
            }

            if (!result.Success)
            {
                await FaultInstanceAsync(instance, node.StepCode, result.ErrorMessage, triggerUser);
                return;
            }

            // Merge output variables back to context
            if (result.OutputVariables != null)
                foreach (var kv in result.OutputVariables)
                    context[kv.Key] = kv.Value;

            await _repository.AppendAuditLogAsync(new WfeAuditEntry
            {
                InstanceId = instance.InstanceId,
                StepCode   = node.StepCode,
                Action     = "AUTOTASK_COMPLETED",
                ADUserName = "SYSTEM",
                Detail     = "Plugin=" + node.PluginCode,
                TenantId   = instance.TenantId
            });

            // Proceed to next step
            var nextCode = ResolveNextStep(node, context, null);
            await ProcessNextStepAsync(instance, graph, nextCode, context, triggerUser, ct);
        }

        // ─── Fault instance (BR-008-11) ─────────────────────────────────────
        private async Task FaultInstanceAsync(
            WfeInstanceRecord instance, string stepCode, string errorDetail, string triggerUser)
        {
            await _repository.FaultInstanceAsync(instance.InstanceId, instance.TenantId);
            instance.Status = WfeInstanceStatus.Faulted;

            await _repository.AppendAuditLogAsync(new WfeAuditEntry
            {
                InstanceId = instance.InstanceId,
                StepCode   = stepCode,
                Action     = "INSTANCE_FAULTED",
                ADUserName = triggerUser,
                Detail     = errorDetail,
                TenantId   = instance.TenantId
            });
        }

        // ─── Resolve next step from transitions ──────────────────────────────
        private string ResolveNextStep(StepNode node, Dictionary<string, object> context, string action)
        {
            if (node.Transitions == null || node.Transitions.Count == 0)
                return null;

            foreach (var t in node.Transitions)
            {
                try
                {
                    if (_conditionEvaluator.Evaluate(t.Condition, context, action))
                        return t.NextStep;
                }
                catch (Exception ex)
                {
                    throw new InvalidOperationException(
                        string.Format("WF-E013: ประเมินเงื่อนไข '{0}' ล้มเหลว: {1}", t.Condition, ex.Message), ex);
                }
            }

            return null;
        }

        // ─── Helpers ────────────────────────────────────────────────────────
        private async Task<StepGraph> GetOrLoadGraphAsync(string definitionCode, int tenantId)
        {
            var cached = _cache.Get(definitionCode);
            if (cached != null) return cached;

            var defJson = await _repository.GetDefinitionJsonAsync(definitionCode, tenantId);
            if (string.IsNullOrEmpty(defJson)) return null;

            var graph = _parser.Parse(definitionCode, defJson);
            _cache.Set(definitionCode, graph);
            return graph;
        }

        private string ResolveAssignee(StepNode node, Dictionary<string, object> context, string initiatorUser)
        {
            switch (node.AssigneeType)
            {
                case WfeAssigneeTypes.User:
                    return node.AssigneeValue;

                case WfeAssigneeTypes.Initiator:
                    return initiatorUser;

                case WfeAssigneeTypes.Dynamic:
                    object dynVal;
                    if (context.TryGetValue(node.AssigneeValue, out dynVal))
                        return dynVal != null ? dynVal.ToString() : null;
                    return null;

                case WfeAssigneeTypes.Role:
                case WfeAssigneeTypes.OrgUnit:
                default:
                    return node.AssigneeValue;
            }
        }

        private Dictionary<string, object> MergeContext(
            Dictionary<string, object> existing, Dictionary<string, object> updates)
        {
            var result = existing != null
                ? new Dictionary<string, object>(existing)
                : new Dictionary<string, object>();

            if (updates != null)
                foreach (var kv in updates)
                    result[kv.Key] = kv.Value;

            return result;
        }

        private string SerializeContext(Dictionary<string, object> ctx)
        {
            return ctx != null
                ? Newtonsoft.Json.JsonConvert.SerializeObject(ctx)
                : "{}";
        }

        private Dictionary<string, object> DeserializeContext(string json)
        {
            if (string.IsNullOrEmpty(json)) return new Dictionary<string, object>();
            return Newtonsoft.Json.JsonConvert.DeserializeObject<Dictionary<string, object>>(json)
                   ?? new Dictionary<string, object>();
        }

        private static string GenerateInstanceCode(int instanceId)
        {
            return string.Format("WFI-{0}-{1:D5}", DateTime.Now.Year + 543, instanceId);
        }
    }
}
