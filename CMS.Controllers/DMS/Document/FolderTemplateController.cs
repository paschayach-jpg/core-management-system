using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Data.Interfaces;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document
{
    public sealed class FolderTemplateController : IFolderTemplateController
    {
        private readonly IFolderTemplateRepository _templateRepository;

        public bool IsAvailable
        {
            get { return _templateRepository != null; }
        }

        public FolderTemplateController(IFolderTemplateRepository templateRepository = null)
        {
            _templateRepository = templateRepository;
        }

        public int Insert(FolderTemplate template)
        {
            if (_templateRepository == null)
                return 0;

            return _templateRepository.Insert(template);
        }

        public void Update(FolderTemplate template)
        {
            if (_templateRepository == null)
                return;

            _templateRepository.Update(template);
        }

        public void Delete(int templateId)
        {
            if (_templateRepository == null)
                return;

            _templateRepository.Delete(templateId);
        }

        public IList<FolderTemplate> GetAll()
        {
            if (_templateRepository == null)
                return new List<FolderTemplate>();

            return _templateRepository.GetAll().ToList();
        }

        public FolderTemplate GetById(int templateId)
        {
            if (_templateRepository == null)
                return null;

            return _templateRepository.GetById(templateId);
        }

        public IList<FolderTemplateItem> GetItems(int templateId)
        {
            if (_templateRepository == null)
                return new List<FolderTemplateItem>();

            return _templateRepository.GetItems(templateId).ToList();
        }

        public int InsertItem(FolderTemplateItem item)
        {
            if (_templateRepository == null)
                return 0;

            return _templateRepository.InsertItem(item);
        }

        public void UpdateItem(FolderTemplateItem item)
        {
            if (_templateRepository == null)
                return;

            _templateRepository.UpdateItem(item);
        }

        public void DeleteItem(int itemId)
        {
            if (_templateRepository == null)
                return;

            _templateRepository.DeleteItem(itemId);
        }

        public void DeleteItemsByTemplate(int templateId)
        {
            if (_templateRepository == null)
                return;

            _templateRepository.DeleteItemsByTemplate(templateId);
        }
    }
}