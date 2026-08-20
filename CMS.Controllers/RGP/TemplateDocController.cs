using System.Data;
using CMS.Data.Interfaces;
using CMS.Domain.Models;

namespace CMS.Controllers.RGP
{
    /// <summary>Thin facade เหนือ ITemplateDocRepository</summary>
    public sealed class TemplateDocController
    {
        private readonly ITemplateDocRepository _repo;

        public TemplateDocController() : this(RgpComposition.TemplateDocRepository) { }

        public TemplateDocController(ITemplateDocRepository repo)
        {
            _repo = repo;
        }

        public DataTable GetDocumentTypes() => _repo.GetDocumentTypes();

        public DataTable GetMasterDocuments() => _repo.GetMasterDocuments();

        public DataTable GetDocsByState(int i_state_id) => _repo.GetDocsByState(i_state_id);

        public DataTable GetDocsByTemplate(int i_template_id) => _repo.GetDocsByTemplate(i_template_id);

        public int InsertTemplateDoc(TemplateDocModel model) => _repo.InsertTemplateDoc(model);

        public bool UpdateTemplateDoc(TemplateDocModel model) => _repo.UpdateTemplateDoc(model);

        public bool DeleteTemplateDoc(int i_template_doc_id) => _repo.DeleteTemplateDoc(i_template_doc_id);

        public bool IsDuplicate(int i_state_id, int i_doc_id, int excludeId = -1) =>
            _repo.IsDuplicate(i_state_id, i_doc_id, excludeId);

        public int GetNextSeqNo(int i_state_id) => _repo.GetNextSeqNo(i_state_id);
    }
}
