using System.Collections.Generic;

namespace CMS.Domain.Models
{
    /// <summary>
    /// ข้อมูลทั้งชุดที่ใช้สร้าง treeCategories ใน KBMainForm
    /// </summary>
    public class KBCategoryTreeData
    {
        public IList<KBCategory> Categories { get; set; }

        /// <summary>CategoryID → จำนวนบทความ</summary>
        public IDictionary<int, int> ArticleCounts { get; set; }

        /// <summary>CategoryID → (Title → จำนวนบทความที่มี Title นี้) ใช้เรนเดอร์ leaf-node "📄 Title"</summary>
        public IDictionary<int, IDictionary<string, int>> TitlesByCategory { get; set; }

        /// <summary>CategoryID → (Title → รายการ folder อ้างอิงที่ต้องแสดงใต้ title-leaf นั้น)</summary>
        public IDictionary<int, IDictionary<string, List<KBLinkedFolder>>> LinkedFoldersByCategoryTitle { get; set; }
    }
}
