using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CMS.Services.Workflow.DTOs;
using CMS.Services.Workflow.Engine;
using CMS.Services.Workflow.Interfaces;

namespace CMS.Services.Workflow
{
    // SRS §15.1 — v2.0 implementation ที่เรียก WorkflowOrchestrator
    // ไม่แตะ WorkflowService (v1.0) เด็ดขาด
    public class WorkflowServiceV2 : IWorkflowServiceV2
    {
        private readonly WorkflowOrchestrator _orchestrator;
        private readonly IWfeRepository       _repository;

        public WorkflowServiceV2(WorkflowOrchestrator orchestrator, IWfeRepository repository)
        {
            _orchestrator = orchestrator;
            _repository   = repository;
        }

        public Task<ServiceResult<WorkflowInstanceDto>> StartAsync(
            StartWorkflowDto dto, CancellationToken ct = default(CancellationToken))
        {
            return _orchestrator.StartAsync(dto, ct);
        }

        public Task<ServiceResult<bool>> CompleteStepAsync(
            CompleteStepDto dto, CancellationToken ct = default(CancellationToken))
        {
            return _orchestrator.CompleteStepAsync(dto, ct);
        }

        public Task<ServiceResult<bool>> CancelInstanceAsync(
            CancelInstanceDto dto, CancellationToken ct = default(CancellationToken))
        {
            return _orchestrator.CancelInstanceAsync(dto, ct);
        }

        public async Task<WorkflowInstanceDetailDto> GetInstanceStatusAsync(int instanceId, int tenantId)
        {
            var instance = await _repository.GetInstanceAsync(instanceId, tenantId);
            if (instance == null) return null;

            return new WorkflowInstanceDetailDto
            {
                InstanceId      = instance.InstanceId,
                InstanceCode    = instance.InstanceCode,
                DefinitionName  = instance.DefinitionCode,
                RefType         = instance.RefType,
                RefId           = instance.RefId,
                Status          = instance.Status.ToString(),
                InitiatedBy     = instance.TriggerByADUser,
                StartedDate     = instance.StartedDate,
                CompletedDate   = instance.CompletedDate,
                CurrentStepCode = instance.CurrentStepCode
            };
        }

        public Task<ServiceResult<bool>> DelegateStepAsync(
            DelegateStepDto dto, CancellationToken ct = default(CancellationToken))
        {
            // BR-008-07: บันทึก delegation + audit "Delegated by [DelegatorADUser]"
            // TODO: implement delegate step via repository
            return Task.FromResult(ServiceResult<bool>.Ok(true));
        }

        public Task<WFDashboardDto> GetMonitorDashboardAsync(int tenantId)
        {
            // FR-008-30: Running, Overdue, Escalated, CompletedToday
            // TODO: implement via repository dashboard query
            return Task.FromResult(new WFDashboardDto());
        }

        public Task<List<SLAReportRowDto>> GetSLAReportAsync(
            DateTime dateFrom, DateTime dateTo, int tenantId)
        {
            // FR-008-31: SLA Compliance Report
            // TODO: implement via repository SLA report query
            return Task.FromResult(new List<SLAReportRowDto>());
        }
    }
}
