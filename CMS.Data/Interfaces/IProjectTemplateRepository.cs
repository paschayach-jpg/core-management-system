using CMS.Domain.Models;
using System;
using System.Collections.Generic;
using System.Data;

namespace CMS.Data.Interfaces
{
    public interface IProjectTemplateRepository
    {
        DataTable GetActiveTemplates();
        DataTable GetAll();
        int Insert(string code, string name, string desc, string user, bool isLegalDict = false);
        bool Update(int id, string code, string name, string desc, string active, string user, bool isLegalDict = false);
        bool IsLegalDict(int templateId);
        int CopyStatesFrom(int sourceTemplateId, int targetTemplateId, string user);
        DataTable GetAllActive();
        bool Delete(int id, out string error);
        DataTable GetStates(int templateId);
        int InsertState(int templateId, string name, string desc, bool required, int seqNo, int? estimatedDays, string user);
        bool UpdateState(int stateId, string name, string desc, bool required, int seqNo, int? estimatedDays, string user);
        bool DeleteState(int stateId, out string error);
        int GetNextStateSeqNo(int templateId);
        bool IsCodeDuplicate(string code, int excludeId = -1);
    }
}
