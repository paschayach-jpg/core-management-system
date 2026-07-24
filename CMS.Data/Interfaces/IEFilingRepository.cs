using CMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace CMS.Data.Interfaces
{
    public interface IEFilingRepository
    {
        DataTable SearchEFilingDoc(string c_efiling_ref_no, string n_doc_name);
        bool CheckEFilingRefExists(string c_efiling_ref_no);
        int InsertEFilingRequest(EFilingRequestModel model);
        bool UpdateEFilingRequestStatus(int i_efiling_req_id, string c_request_status, DateTime? d_receive_date, string n_remark, string i_user);
        DataTable GetEFilingRequestsByProject(int i_project_id);
        DataTable GetEFilingRequestsByState(int i_proj_state_id);
    }
}
