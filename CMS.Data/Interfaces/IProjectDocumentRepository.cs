using CMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace CMS.Data.Interfaces
{
    public interface IProjectDocumentRepository
    {
        DataTable GetDocsByState(int i_proj_state_id);
        DataTable GetDocsByProject(int i_project_id);
        int InsertDocWorkflow(ProjectDocumentWorkflowModel model);
        bool InsertDocWorkflowFromTemplate(int i_project_id, string i_user);
        bool UpdateDocWorkflow(ProjectDocumentWorkflowModel model);
        bool DeleteDocWorkflow(int i_doc_wf_id);
        bool UpdateDocStatus(int i_doc_wf_id, string c_doc_status, string i_user);
        int GetNextSeqNo(int i_proj_state_id);
    }
}
