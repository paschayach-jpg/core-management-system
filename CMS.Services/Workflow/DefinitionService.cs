using System.Threading.Tasks;
using CMS.Domain.Models.Workflow;
using CMS.Services.Workflow.Definition;
using CMS.Services.Workflow.DTOs;
using CMS.Services.Workflow.Engine;
using CMS.Services.Workflow.Interfaces;

namespace CMS.Services.Workflow
{
    // SRS §15.3 — IDefinitionService implementation
    public class DefinitionService : IDefinitionService
    {
        private readonly IStepGraphCache     _cache;
        private readonly IDefinitionParser   _parser;
        private readonly IDefinitionValidator _validator;
        private readonly IWfeRepository      _repository;

        public DefinitionService(
            IStepGraphCache      cache,
            IDefinitionParser    parser,
            IDefinitionValidator validator,
            IWfeRepository       repository)
        {
            _cache      = cache;
            _parser     = parser;
            _validator  = validator;
            _repository = repository;
        }

        public async Task<StepGraph> GetStepGraphAsync(string definitionCode, int tenantId)
        {
            var cached = _cache.Get(definitionCode);
            if (cached != null) return cached;

            var json = await _repository.GetDefinitionJsonAsync(definitionCode, tenantId);
            if (string.IsNullOrEmpty(json)) return null;

            var graph = _parser.Parse(definitionCode, json);
            _cache.Set(definitionCode, graph);
            return graph;
        }

        public async Task<ServiceResult<WorkflowDefinitionDto>> SaveAsync(SaveDefinitionDto dto)
        {
            // Validate ก่อน Save
            var validation = await ValidateAsync(dto.DefinitionJson, dto.TenantId);
            if (!validation.IsValid)
                return ServiceResult<WorkflowDefinitionDto>.Fail(
                    "WF-E001", string.Join("; ", validation.Errors));

            var defId = await _repository.SaveDefinitionAsync(
                dto.DefinitionCode, dto.Name, dto.DefinitionJson, dto.SavedByADUser, dto.TenantId);

            // Invalidate cache เมื่อ Definition เปลี่ยน
            _cache.Invalidate(dto.DefinitionCode);

            return ServiceResult<WorkflowDefinitionDto>.Ok(new WorkflowDefinitionDto
            {
                DefinitionId   = defId,
                DefinitionCode = dto.DefinitionCode,
                Name           = dto.Name,
                Description    = dto.Description,
                DefinitionJson = dto.DefinitionJson
            });
        }

        public async Task<ServiceResult<bool>> ActivateVersionAsync(
            int definitionId, string approvedByADUser, int tenantId)
        {
            await _repository.ActivateDefinitionAsync(definitionId, approvedByADUser, tenantId);

            // BR-008-08: Invalidate cache เมื่อ Activate version ใหม่
            _cache.InvalidateAll();

            return ServiceResult<bool>.Ok(true);
        }

        public Task<ValidationResult> ValidateAsync(string definitionJson, int tenantId)
        {
            try
            {
                var graph = _parser.Parse("VALIDATE_TEMP", definitionJson);
                // TODO: load registered plugin codes from s_WorkflowPlugin table
                var result = _validator.Validate(graph, new string[0]);
                return Task.FromResult(result);
            }
            catch (System.Exception ex)
            {
                var result = new ValidationResult { IsValid = false };
                result.Errors.Add("Parse error: " + ex.Message);
                return Task.FromResult(result);
            }
        }
    }
}
