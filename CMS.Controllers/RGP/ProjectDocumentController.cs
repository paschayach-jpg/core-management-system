using System;
using System.Data;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.RGP
{
    /// <summary>Thin facade เหนือ IProjectDocumentService สำหรับ Presentation ของโมดูล Project (RGP)</summary>
    public sealed class ProjectDocumentController
    {
        private readonly IProjectDocumentService _docService;

        public ProjectDocumentController() : this(RgpComposition.DocService) { }

        public ProjectDocumentController(IProjectDocumentService docService)
        {
            _docService = docService;
        }

        public DataTable GetDocWorkflow(int i_proj_state_id) => _docService.GetDocWorkflow(i_proj_state_id);

        public DataTable GetDocWorkflowByProject(int i_project_id) => _docService.GetDocWorkflowByProject(i_project_id);

        public int AddDocWorkflow(ProjectDocumentWorkflowModel model) => _docService.AddDocWorkflow(model);

        public bool UpdateDocWorkflow(ProjectDocumentWorkflowModel model) => _docService.UpdateDocWorkflow(model);

        public bool DeleteDocWorkflow(int i_doc_wf_id, out string errorMessage) =>
            _docService.DeleteDocWorkflow(i_doc_wf_id, out errorMessage);

        public bool UpdateDocStatus(int i_doc_wf_id, string c_doc_status, string i_user) =>
            _docService.UpdateDocStatus(i_doc_wf_id, c_doc_status, i_user);

        public DataTable SearchEFiling(string c_efiling_ref_no, string n_doc_name) =>
            _docService.SearchEFiling(c_efiling_ref_no, n_doc_name);

        public int CreateEFilingRequest(EFilingRequestModel model) => _docService.CreateEFilingRequest(model);

        public bool UpdateEFilingRequestStatus(int i_efiling_req_id, string c_request_status,
            DateTime? d_receive_date, string n_remark, string i_user) =>
            _docService.UpdateEFilingRequestStatus(i_efiling_req_id, c_request_status, d_receive_date, n_remark, i_user);

        public DataTable GetEFilingRequests(int i_project_id) => _docService.GetEFilingRequests(i_project_id);
    }
}
