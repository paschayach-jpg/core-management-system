using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.DMS.Document
{
    /// <summary>
    /// View contract for the ProjectPresenter.
    /// Implemented by ProjectForm.
    /// </summary>
    public interface IProjectView
    {
        /// <summary>แสดงรายการ Project ทั้งหมด</summary>
        void ShowProjects(IEnumerable<Folder> projects);

        /// <summary>แสดงข้อความ Error</summary>
        void ShowError(string message);

        /// <summary>แสดงข้อความแจ้งเตือนทั่วไป</summary>
        void ShowInfo(string message);

        /// <summary>Project ที่ถูกเลือกอยู่ หรือ null ถ้าไม่ได้เลือก</summary>
        Folder SelectedProject { get; }
    }
}
