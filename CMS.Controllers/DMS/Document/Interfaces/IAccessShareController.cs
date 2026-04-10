using System;
using CMS.Domain.Models;
using CMS.Services.DTOs;

namespace CMS.Controllers.DMS.Document.Interfaces
{
    public interface IAccessShareController
    {
        ShareAccessResult ValidateAndGrantAccess(Guid token, string password, UserContext user);
        void IncrementDownloadCount(int externalShareId);
    }
}
