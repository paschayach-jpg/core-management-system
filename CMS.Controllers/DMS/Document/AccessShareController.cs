using System;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class AccessShareController : IAccessShareController
    {
        private readonly IShareAccessService _shareAccessService;

        public AccessShareController(IShareAccessService shareAccessService = null)
        {
            _shareAccessService = shareAccessService;
        }

        public ShareAccessResult ValidateAndGrantAccess(Guid token, string password, UserContext user)
        {
            if (_shareAccessService == null)
            {
                return new ShareAccessResult
                {
                    IsGranted = false,
                    DenyReason = "Share access service is not available."
                };
            }

            return _shareAccessService.ValidateAndGrantAccess(token, password, user);
        }

        public void IncrementDownloadCount(int externalShareId)
        {
            if (_shareAccessService == null)
                return;

            _shareAccessService.IncrementDownloadCount(externalShareId);
        }
    }
}
