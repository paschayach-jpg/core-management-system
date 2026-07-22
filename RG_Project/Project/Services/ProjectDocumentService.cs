using RG_Project.Project.Controllers;
using RG_Project.Project.Models;
using System;
using System.Data;

namespace RG_Project.Project.Services
{
    public class ProjectDocumentService
    {
        private readonly ProjectDocumentController _docCtrl;
        private readonly EFilingController         _efilingCtrl;

        public ProjectDocumentService()
        {
            _docCtrl     = new ProjectDocumentController();
            _efilingCtrl = new EFilingController();
        }

        /// <summary>
        /// ดึง Doc Workflow ของ State
        /// </summary>
        public DataTable GetDocWorkflow(int i_proj_state_id)
        {
            return _docCtrl.GetDocsByState(i_proj_state_id);
        }

        /// <summary>
        /// ดึง Doc Workflow ของ Project (ทุก State)
        /// </summary>
        public DataTable GetDocWorkflowByProject(int i_project_id)
        {
            return _docCtrl.GetDocsByProject(i_project_id);
        }

        /// <summary>
        /// เพิ่ม Doc Workflow item (auto-seq_no)
        /// </summary>
        public int AddDocWorkflow(ProjectDocumentWorkflowModel model)
        {
            model.i_seq_no = _docCtrl.GetNextSeqNo(model.i_proj_state_id);
            return _docCtrl.InsertDocWorkflow(model);
        }

        /// <summary>
        /// แก้ไข Doc Workflow item
        /// </summary>
        public bool UpdateDocWorkflow(ProjectDocumentWorkflowModel model)
        {
            return _docCtrl.UpdateDocWorkflow(model);
        }

        /// <summary>
        /// ลบ Doc Workflow item
        /// </summary>
        public bool DeleteDocWorkflow(int i_doc_wf_id, out string errorMessage)
        {
            errorMessage = "";
            bool result = _docCtrl.DeleteDocWorkflow(i_doc_wf_id);
            if (!result)
                errorMessage = "ไม่สามารถลบรายการเอกสารได้";
            return result;
        }

        /// <summary>
        /// อัปเดตสถานะเอกสาร
        /// </summary>
        public bool UpdateDocStatus(int i_doc_wf_id, string c_doc_status, string i_user)
        {
            return _docCtrl.UpdateDocStatus(i_doc_wf_id, c_doc_status, i_user);
        }

        /// <summary>
        /// ค้นหาเอกสารใน E-Filing ผ่าน REST API
        /// </summary>
        public DataTable SearchEFiling(string c_efiling_ref_no, string n_doc_name)
        {
            return _efilingCtrl.SearchEFilingDoc(c_efiling_ref_no, n_doc_name);
        }

        /// <summary>
        /// บันทึก E-Filing Request
        /// </summary>
        public int CreateEFilingRequest(EFilingRequestModel model)
        {
            return _efilingCtrl.InsertEFilingRequest(model);
        }

        /// <summary>
        /// อัปเดตสถานะ E-Filing Request
        /// </summary>
        public bool UpdateEFilingRequestStatus(int i_efiling_req_id, string c_request_status,
            DateTime? d_receive_date, string n_remark, string i_user)
        {
            return _efilingCtrl.UpdateEFilingRequestStatus(i_efiling_req_id,
                c_request_status, d_receive_date, n_remark, i_user);
        }

        /// <summary>
        /// ดึง E-Filing Requests ของ Project
        /// </summary>
        public DataTable GetEFilingRequests(int i_project_id)
        {
            return _efilingCtrl.GetEFilingRequestsByProject(i_project_id);
        }
    }
}
