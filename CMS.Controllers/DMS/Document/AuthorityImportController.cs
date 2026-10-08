using System;
using System.Collections.Generic;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Models;
using CMS.Services.DTOs;
using CMS.Services.Interfaces;

namespace CMS.Controllers.DMS.Document
{
    public sealed class AuthorityImportController : IAuthorityImportController
    {
        private readonly IAuthorityImportService _service;

        public AuthorityImportController(IAuthorityImportService service)
        {
            if (service == null) throw new ArgumentNullException("service");
            _service = service;
        }

        public byte[] GenerateTemplate(UserContext user)
        {
            return _service.GenerateTemplate(user);
        }

        public IList<string> GetSheetNames(string filePath, UserContext user)
        {
            return _service.GetSheetNames(filePath, user);
        }

        public AuthorityImportPreview BuildPreview(string filePath, string sheetName, bool updateUserLevel, bool importPositions, UserContext user)
        {
            return _service.BuildPreview(filePath, sheetName, updateUserLevel, importPositions, user);
        }

        public AuthorityImportResult Commit(AuthorityImportPreview preview, UserContext user)
        {
            return _service.Commit(preview, user);
        }
    }
}
