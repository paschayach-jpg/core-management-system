using System.Collections.Generic;
using CMS.Data.Interfaces;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    public class ProjectAttachmentService : IProjectAttachmentService
    {
        private readonly IProjectAttachmentRepository _repo;

        public ProjectAttachmentService(IProjectAttachmentRepository repo)
        {
            _repo = repo;
        }

        public IList<ProjectAttachmentModel> GetAttachments(int projectId)
        {
            return _repo.GetByProject(projectId);
        }

        public void LinkAttachments(int projectId, IEnumerable<int> fileIds, string user)
        {
            if (fileIds == null) return;
            foreach (var fileId in fileIds)
            {
                if (fileId > 0)
                    _repo.Insert(projectId, fileId, user);
            }
        }

        public void RemoveAttachment(int projectId, int fileId, string user)
        {
            _repo.DeleteByProjectAndFile(projectId, fileId);
        }
    }
}
