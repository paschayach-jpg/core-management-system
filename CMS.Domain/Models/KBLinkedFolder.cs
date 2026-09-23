using System.Collections.Generic;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Folder อ้างอิงที่สังเคราะห์ขึ้นใน treeCategories จาก KMS.t_KBArticleLink (EntityType=Law)
    /// เช่น "คำสั่ง (นายทะเบียน/กระทรวง)", "หมวดภายใน" ใต้ node มาตรา หรือ "กฎหมาย" ใต้ node คำสั่ง/ประกาศ
    /// </summary>
    public class KBLinkedFolder
    {
        public string Label { get; set; }
        public IList<KBArticle> Articles { get; set; }
    }
}
