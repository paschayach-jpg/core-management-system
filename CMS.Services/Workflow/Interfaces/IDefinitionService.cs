using System.Threading.Tasks;
using CMS.Domain.Models.Workflow;
using CMS.Services.Workflow.Definition;
using CMS.Services.Workflow.DTOs;

namespace CMS.Services.Workflow.Interfaces
{
    // SRS §15.3 — IDefinitionService
    public interface IDefinitionService
    {
        // FR-008-15: Get StepGraph (ผ่าน Cache)
        Task<StepGraph> GetStepGraphAsync(string definitionCode, int tenantId);

        // FR-008-04: บันทึก Definition (Draft)
        Task<ServiceResult<WorkflowDefinitionDto>> SaveAsync(SaveDefinitionDto dto);

        // FR-008-04: Activate version (immutable หลังจากนี้) (BR-008-08)
        Task<ServiceResult<bool>> ActivateVersionAsync(int definitionId, string approvedByADUser, int tenantId);

        // FR-008-03: Validate Definition ก่อน Save/Activate
        Task<ValidationResult> ValidateAsync(string definitionJson, int tenantId);
    }
}
