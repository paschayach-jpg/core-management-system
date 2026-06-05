using System;
using System.Collections.Generic;

namespace CMS.Services.Workflow.DTOs
{
    // SRS §15.4 — WFDashboardDto
    public class WFDashboardDto
    {
        public int RunningCount       { get; set; }
        public int OverdueCount       { get; set; }
        public int EscalatedCount     { get; set; }
        public int CompletedTodayCount { get; set; }
        public List<WorkflowInstanceSummaryDto> RecentInstances { get; set; }

        public WFDashboardDto()
        {
            RecentInstances = new List<WorkflowInstanceSummaryDto>();
        }
    }

    public class WorkflowInstanceSummaryDto
    {
        public int      InstanceId      { get; set; }
        public string   InstanceCode    { get; set; }
        public string   DefinitionName  { get; set; }
        public string   RefType         { get; set; }
        public int      RefId           { get; set; }
        public string   Status          { get; set; }
        public string   InitiatedBy     { get; set; }
        public DateTime StartedDate     { get; set; }
        public string   CurrentStepCode { get; set; }
        public bool     IsOverdue       { get; set; }
        public bool     IsEscalated     { get; set; }
    }

    // SRS §15.4 — SLAReportRowDto
    public class SLAReportRowDto
    {
        public string   InstanceCode    { get; set; }
        public string   DefinitionName  { get; set; }
        public string   StepCode        { get; set; }
        public string   AssignedTo      { get; set; }
        public DateTime StartedDate     { get; set; }
        public DateTime DueDate         { get; set; }
        public DateTime? CompletedDate  { get; set; }
        public bool     IsOnTime        { get; set; }
        public bool     IsEscalated     { get; set; }
        public int      HoursOverdue    { get; set; }
    }

    // SRS §15.2 — SaveDefinitionDto
    public class SaveDefinitionDto
    {
        public int    DefinitionId   { get; set; }  // 0 = create new
        public string DefinitionCode { get; set; }
        public string Name           { get; set; }
        public string Description    { get; set; }
        public string DefinitionJson { get; set; }
        public string SavedByADUser  { get; set; }
        public int    TenantId       { get; set; }
    }

    public class WorkflowDefinitionDto
    {
        public int    DefinitionId   { get; set; }
        public string DefinitionCode { get; set; }
        public string Name           { get; set; }
        public string Description    { get; set; }
        public int    Version        { get; set; }
        public bool   IsActivated    { get; set; }
        public string DefinitionJson { get; set; }
        public DateTime CreatedDate  { get; set; }
        public string CreatedBy      { get; set; }
    }
}
