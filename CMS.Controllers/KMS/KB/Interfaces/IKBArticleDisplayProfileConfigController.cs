using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.KMS.KB.Interfaces
{
    /// <summary>
    /// View contract สำหรับหน้า config Article Display Profile (implement โดย KBArticleDisplayProfileConfigForm)
    /// </summary>
    public interface IKBArticleDisplayProfileConfigController
    {
        int?   EditingProfileId    { get; set; }
        string ProfileName         { get; set; }
        string ProfileDescription  { get; set; }
        bool   DeleteEnabled       { set; }

        void BindProfileList(IEnumerable<KBArticleDisplayProfile> profiles);
        void BindColumnGrid(IEnumerable<KBArticleDisplayProfileColumnRow> rows);

        /// <summary>รายการ category (active เท่านั้น) ให้เลือกผูก Profile นี้ พร้อมรายชื่อที่ผูกอยู่แล้วก่อน edit</summary>
        void BindCategoryAssignment(IEnumerable<KBCategory> allCategories, IEnumerable<int> assignedCategoryIds);

        /// <summary>อ่านค่าที่ admin กรอกในช่อง Label/Visible ของ grid กลับออกมา</summary>
        IList<KBArticleDisplayProfileColumnRow> ReadColumnGridEdits();

        /// <summary>อ่านรายชื่อ CategoryID ที่ admin ติ๊กเลือกไว้ในตอนที่จะ save</summary>
        IList<int> ReadAssignedCategoryIds();

        void ClearForm();
        void ShowError(string message);
        void ShowSuccess(string message);
    }
}
