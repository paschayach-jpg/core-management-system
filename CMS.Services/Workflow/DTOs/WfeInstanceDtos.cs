using System;
using System.Collections.Generic;

namespace CMS.Services.Workflow.DTOs
{
    // SRS §15.2 — StartWorkflowDto
    public class StartWorkflowDto
    {
        public string DefinitionCode { get; set; }
        public string RefType        { get; set; }  // e.g. "MOTOR_CLAIM"
        public int    RefId          { get; set; }
        public string TriggerByADUser { get; set; }
        public int    TriggerType    { get; set; }  // 1=Manual 2=Auto 3=Scheduled
        public Dictionary<string, object> InitialContext { get; set; }
        public int    TenantId       { get; set; }

        public StartWorkflowDto()
        {
            InitialContext = new Dictionary<string, object>();
        }
    }

    // SRS §15.2 — CompleteStepDto
    public class CompleteStepDto
    {
        public int    StepInstanceId  { get; set; }
        public string Decision        { get; set; }  // APPROVE | REJECT | FORWARD
        public string Comment         { get; set; }  // required when Decision=REJECT (BR-008-04)
        public string ADUserName      { get; set; }
        public Dictionary<string, object> UpdatedContext { get; set; }
        public int    TenantId        { get; set; }

        public CompleteStepDto()
        {
            UpdatedContext = new Dictionary<string, object>();
        }
    }

    // SRS §15.2 — CancelInstanceDto
    public class CancelInstanceDto
    {
        public int    InstanceId          { get; set; }
        public string Reason              { get; set; }  // required (BR-008-05)
        public string CancelledByADUser   { get; set; }
        public int    TenantId            { get; set; }
    }

    // SRS §15.4 — DelegateStepDto
    public class DelegateStepDto
    {
        public int    StepInstanceId      { get; set; }
        public string DelegatorADUser     { get; set; }
        public string DelegateeADUser     { get; set; }
        public DateTime StartDate         { get; set; }
        public DateTime EndDate           { get; set; }
        public int    TenantId            { get; set; }
    }

    // Return DTO for StartAsync
    public class WorkflowInstanceDto
    {
        public int      InstanceId      { get; set; }
        public string   InstanceCode    { get; set; }
        public string   DefinitionCode  { get; set; }
        public string   Status          { get; set; }
        public string   CurrentStepCode { get; set; }
        public DateTime StartedDate     { get; set; }
        public string   RefType         { get; set; }
        public int      RefId           { get; set; }
    }

    // Full detail DTO for GetInstanceStatusAsync
    public class WorkflowInstanceDetailDto
    {
        public int      InstanceId        { get; set; }
        public string   InstanceCode      { get; set; }
        public string   DefinitionName    { get; set; }
        public string   RefType           { get; set; }
        public int      RefId             { get; set; }
        public string   Status            { get; set; }
        public string   InitiatedBy       { get; set; }
        public DateTime StartedDate       { get; set; }
        public DateTime? CompletedDate    { get; set; }
        public int      CompletionPercent { get; set; }
        public string   CurrentStepCode   { get; set; }
        public List<StepHistoryDto> StepHistory { get; set; }

        public WorkflowInstanceDetailDto()
        {
            StepHistory = new List<StepHistoryDto>();
        }
    }

    public class StepHistoryDto
    {
        public string   StepCode       { get; set; }
        public string   StepName       { get; set; }
        public string   Decision       { get; set; }
        public string   CompletedBy    { get; set; }
        public DateTime? StartedDate   { get; set; }
        public DateTime? CompletedDate { get; set; }
        public bool     IsEscalated    { get; set; }
        public string   Comment        { get; set; }
    }
}
