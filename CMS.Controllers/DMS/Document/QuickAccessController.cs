using System.Collections.Generic;
using System.Linq;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class QuickAccessController : IQuickAccessController
    {
        private readonly IQuickAccessService _quickAccessService;

        public QuickAccessController(IQuickAccessService quickAccessService = null)
        {
            _quickAccessService = quickAccessService;
        }

        public IList<QuickAccessFolder> GetByUser(int userId)
        {
            if (_quickAccessService == null)
                return new List<QuickAccessFolder>();

            return _quickAccessService.GetByUser(userId).ToList();
        }

        public void Add(int userId, int folderId)
        {
            if (_quickAccessService == null)
                return;

            _quickAccessService.Add(userId, folderId);
        }

        public void Remove(int userId, int folderId)
        {
            if (_quickAccessService == null)
                return;

            _quickAccessService.Remove(userId, folderId);
        }
    }
}
