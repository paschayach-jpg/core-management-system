using CMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace CMS.Data.Interfaces
{
    public interface ITemplateDocRepository
    {
        DataTable GetDocumentTypes();
        DataTable GetMasterDocuments();
        DataTable GetDocsByState(int i_state_id);
        DataTable GetDocsByTemplate(int i_template_id);
        int InsertTemplateDoc(TemplateDocModel model);
        bool UpdateTemplateDoc(TemplateDocModel model);
        bool DeleteTemplateDoc(int i_template_doc_id);
        bool IsDuplicate(int i_state_id, int i_doc_id, int excludeId = -1);
        int GetNextSeqNo(int i_state_id);
    }
}
