using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Data.Interfaces
{
    public interface IFolderTemplateRepository
    {
        int Insert(FolderTemplate template);
        void Update(FolderTemplate template);
        void Delete(int templateId);
        IEnumerable<FolderTemplate> GetAll();
        FolderTemplate GetById(int templateId);
        IEnumerable<FolderTemplateItem> GetItems(int templateId);
        int InsertItem(FolderTemplateItem item);
        void UpdateItem(FolderTemplateItem item);
        void DeleteItem(int itemId);
        void DeleteItemsByTemplate(int templateId);
    }
}
