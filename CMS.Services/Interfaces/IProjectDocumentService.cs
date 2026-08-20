using System;
using System.Data;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    public interface IProjectDocumentService
    {
        DataTable GetDocWorkflow(int i_proj_state_id);
        DataTable GetDocWorkflowByProject(int i_project_id);
        int AddDocWorkflow(ProjectDocumentWorkflowModel model);
        bool UpdateDocWorkflow(ProjectDocumentWorkflowModel model);
        bool DeleteDocWorkflow(int i_doc_wf_id, out string errorMessage);
        bool UpdateDocStatus(int i_doc_wf_id, string c_doc_status, string i_user);
        DataTable SearchEFiling(string c_efiling_ref_no, string n_doc_name);
        int CreateEFilingRequest(EFilingRequestModel model);
        bool UpdateEFilingRequestStatus(int i_efiling_req_id, string c_request_status, DateTime? d_receive_date, string n_remark, string i_user);
        DataTable GetEFilingRequests(int i_project_id);
    }
}
