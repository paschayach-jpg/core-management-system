using System.Data;
using CMS.Data.Interfaces;

namespace CMS.Controllers.RGP
{
    /// <summary>Thin facade เหนือ IProjectTemplateRepository</summary>
    public sealed class ProjectTemplateController
    {
        private readonly IProjectTemplateRepository _repo;

        public ProjectTemplateController() : this(RgpComposition.TemplateRepository) { }

        public ProjectTemplateController(IProjectTemplateRepository repo)
        {
            _repo = repo;
        }

        public DataTable GetActiveTemplates() => _repo.GetActiveTemplates();

        public DataTable GetAll() => _repo.GetAll();

        public int Insert(string code, string name, string desc, string user, bool isLegalDict = false) =>
            _repo.Insert(code, name, desc, user, isLegalDict);

        public bool Update(int id, string code, string name, string desc, string active, string user, bool isLegalDict = false) =>
            _repo.Update(id, code, name, desc, active, user, isLegalDict);

        public bool IsLegalDict(int templateId) => _repo.IsLegalDict(templateId);

        public int CopyStatesFrom(int sourceTemplateId, int targetTemplateId, string user) =>
            _repo.CopyStatesFrom(sourceTemplateId, targetTemplateId, user);

        public DataTable GetAllActive() => _repo.GetAllActive();

        public bool Delete(int id, out string error) => _repo.Delete(id, out error);

        public DataTable GetStates(int templateId) => _repo.GetStates(templateId);

        public int InsertState(int templateId, string name, string desc, bool required, int seqNo, int? estimatedDays, string user) =>
            _repo.InsertState(templateId, name, desc, required, seqNo, estimatedDays, user);

        public bool UpdateState(int stateId, string name, string desc, bool required, int seqNo, int? estimatedDays, string user) =>
            _repo.UpdateState(stateId, name, desc, required, seqNo, estimatedDays, user);

        public bool DeleteState(int stateId, out string error) => _repo.DeleteState(stateId, out error);

        public int GetNextStateSeqNo(int templateId) => _repo.GetNextStateSeqNo(templateId);

        public bool IsCodeDuplicate(string code, int excludeId = -1) => _repo.IsCodeDuplicate(code, excludeId);
    }
}
