using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IFolderTemplateController
    {
        bool IsAvailable { get; }

        int Insert(FolderTemplate template);
        void Update(FolderTemplate template);
        void Delete(int templateId);
        IList<FolderTemplate> GetAll();
        FolderTemplate GetById(int templateId);
        IList<FolderTemplateItem> GetItems(int templateId);
        int InsertItem(FolderTemplateItem item);
        void UpdateItem(FolderTemplateItem item);
        void DeleteItem(int itemId);
        void DeleteItemsByTemplate(int templateId);
        void ApplyTemplateToFolder(int parentFolderId, int templateId, int confidentialityLevel, UserContext currentUser);
        void ApplyTemplateToOrgUnit(int orgUnitId, int templateId, string rootFolderName, int confidentialityLevel, UserContext currentUser);
    }
}