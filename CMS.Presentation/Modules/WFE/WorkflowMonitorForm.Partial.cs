using System;
using System.Collections.Generic;
using System.Linq;
using CMS.Domain.Enums;
using CMS.Services.DTOs;

namespace CMS.Presentation.Modules.Workflow
{
    public partial class WorkflowMonitorForm
    {
        public void DisplayWorkflowInstances(List<WorkflowInstanceResult> instances)
        {
            try
            {
                _displayRows = (instances ?? new List<WorkflowInstanceResult>())
                    .Select(i => new MonitorRowItem
                    {
                        WorkflowInstanceId = i.WorkflowInstanceId,
                        InstanceId = $"WFI-{i.WorkflowInstanceId:D5}",
                        WorkflowName = i.WorkflowName ?? "-",
                        AssigneeName = i.InitiatedBy ?? "-",
                        StartDate = i.InitiatedDate.ToString("dd/MM/yy HH:mm"),
                        StepProgress = string.Format(
                            "{0}/{1}",
                            (i.StepHistory ?? new List<WorkflowStepInstanceResult>()).Count(s => s.Status == WorkflowStepInstanceStatus.Completed),
                            (i.StepHistory ?? new List<WorkflowStepInstanceResult>()).Count),
                        CurrentStepName = (i.StepHistory ?? new List<WorkflowStepInstanceResult>())
                            .FirstOrDefault(s => s.Status == WorkflowStepInstanceStatus.Pending || s.Status == WorkflowStepInstanceStatus.InProgress)
                            ?.StepName ?? "-",
                        StatusText = i.Status.ToString()
                    })
                    .ToList();

                _currentPage = 1;
                ApplyLocalFilter();
            }
            catch (Exception ex)
            {
                ShowError($"DisplayWorkflowInstances error: {ex.Message}");
            }
        }
    }
}
