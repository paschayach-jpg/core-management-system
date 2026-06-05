using System;
using System.Collections.Generic;
using CMS.Domain.Enums;

namespace CMS.Presentation.Modules.Workflow
{
    public class WorkflowWizardState
    {
        // Basic Template Info
        public string TemplateCode { get; set; }
        public string TemplateName { get; set; }
        public string Description { get; set; }
        public string Category { get; set; }
        public string Owner { get; set; }
        public int SlaDays { get; set; } = 5;
        public string Priority { get; set; } = "ปกติ";
        public string Initiator { get; set; } = "พนักงานทุกคน";
        public string TagsText { get; set; }

        // Step 1 options
        public bool UseFormBuilder { get; set; } = true;
        public bool RequireOwnerCommitteeApproval { get; set; } = true;
        public bool AllowInitiatorCancel { get; set; }
        public bool AutoStoreStepFilesToDms { get; set; }

        // Starting Mode Selection
        public string StartingMode { get; set; } = "fresh"; // "fresh", "copy", "import"
        public int? CopyFromTemplateId { get; set; }
        public string CopyFromTemplateName { get; set; }
        public string ImportedFilePath { get; set; }

        // Form Builder (Step 3)
        public string FormTitle { get; set; }
        public List<FormSectionDefinition> FormSections { get; set; }
        public List<FormFieldDefinition> FormFields { get; set; }

        // Workflow Structure
        public List<WorkflowStepData> Steps { get; set; }
        public List<WorkflowApproverData> Approvers { get; set; }
        public bool IsPublished { get; set; }

        // Phase 2: Approval Flow
        // True = ส่งเข้า Approval Queue, False = บันทึก Draft เท่านั้น
        public bool SubmitForApproval { get; set; }

        public WorkflowWizardState()
        {
            Steps = new List<WorkflowStepData>();
            Approvers = new List<WorkflowApproverData>();
            FormSections = new List<FormSectionDefinition>();
            FormFields = new List<FormFieldDefinition>();
            IsPublished = true;
        }
    }

    public class WorkflowStepData
    {
        public int StepNumber { get; set; }
        public string StepName { get; set; }
        public string Action { get; set; }
        public string NextStep { get; set; }
    }

    public class WorkflowApproverData
    {
        public string StepName { get; set; }
        public string ApproverGroup { get; set; }
        public string Level { get; set; }
    }

    public class FormSectionDefinition
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public string Title { get; set; }
        public int Order { get; set; }

        public FormSectionDefinition() { }
        public FormSectionDefinition(string title, int order)
        {
            Title = title;
            Order = order;
        }
    }

    public class FormFieldDefinition
    {
        public string Id { get; set; } = Guid.NewGuid().ToString();
        public FormFieldType FieldType { get; set; }
        public string Label { get; set; }
        public string FieldName { get; set; }
        public string HelpText { get; set; }
        public bool Required { get; set; }
        public bool Visible { get; set; } = true;
        public string Source { get; set; }
        public string DefaultValue { get; set; }
        public string ConditionalLogic { get; set; }
        public string MapToContext { get; set; }
        public string SectionId { get; set; }
        public int Order { get; set; }

        public FormFieldDefinition() { }
        public FormFieldDefinition(FormFieldType fieldType, string label, string fieldName, bool required = false)
        {
            FieldType = fieldType;
            Label = label;
            FieldName = fieldName;
            Required = required;
        }
    }
}
