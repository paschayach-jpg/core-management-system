using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
using CMS.Services.Workflow.DTOs;

namespace CMS.Services.Workflow.Interfaces
{
    // SRS §15.1 — IWorkflowService v2.0 async interface
    // Replaces the synchronous IWorkflowService (v1.0) for new callers
    public interface IWorkflowServiceV2
    {
        // FR-008-08: เริ่ม Workflow Instance ใหม่
        Task<ServiceResult<WorkflowInstanceDto>> StartAsync(StartWorkflowDto dto, CancellationToken ct = default(CancellationToken));

        // FR-008-11: Complete / Approve / Reject step
        Task<ServiceResult<bool>> CompleteStepAsync(CompleteStepDto dto, CancellationToken ct = default(CancellationToken));

        // FR-008-13: Cancel instance (Manager/Admin)
        Task<ServiceResult<bool>> CancelInstanceAsync(CancelInstanceDto dto, CancellationToken ct = default(CancellationToken));

        // FR-008-14: ดู Status + Step history
        Task<WorkflowInstanceDetailDto> GetInstanceStatusAsync(int instanceId, int tenantId);

        // FR-008-29: Delegation
        Task<ServiceResult<bool>> DelegateStepAsync(DelegateStepDto dto, CancellationToken ct = default(CancellationToken));

        // FR-008-30: Monitor Dashboard
        Task<WFDashboardDto> GetMonitorDashboardAsync(int tenantId);

        // FR-008-31: SLA Report
        Task<List<SLAReportRowDto>> GetSLAReportAsync(DateTime dateFrom, DateTime dateTo, int tenantId);
    }
}
