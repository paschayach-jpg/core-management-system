using System.Collections.Generic;
using CMS.Domain.Models;
using CMS.Services.Interfaces;

namespace CMS.Controllers.RGP
{
    /// <summary>Thin facade เหนือ IProjectAttachmentService สำหรับ Presentation ของโมดูล Project (RGP)</summary>
    public sealed class ProjectAttachmentController
    {
        private readonly IProjectAttachmentService _service;

        public ProjectAttachmentController() : this(RgpComposition.AttachmentService) { }

        public ProjectAttachmentController(IProjectAttachmentService service)
        {
            _service = service;
        }

        public IList<ProjectAttachmentModel> GetAttachments(int projectId) =>
            _service.GetAttachments(projectId);

        public void LinkAttachments(int projectId, IEnumerable<int> fileIds, string user) =>
            _service.LinkAttachments(projectId, fileIds, user);

        public void RemoveAttachment(int projectId, int fileId, string user) =>
            _service.RemoveAttachment(projectId, fileId, user);
    }
}
