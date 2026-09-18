using System.Collections.Generic;

namespace CMS.Domain.Models
{
    /// <summary>
    /// มาตราที่พบใน column I 1 ตัว พร้อมรายการบทความที่จับคู่ได้จาก TitleCategory
    /// ถ้าเจอมากกว่า 1 ให้ user เลือกครั้งเดียวแล้วใช้กับทุกแถวในไฟล์
    /// </summary>
    public class KBSectionGroup
    {
        /// <summary>ข้อความตามที่เขียนใน Excel เช่น "มาตรา 108/4"</summary>
        public string SectionRaw { get; set; }

        /// <summary>ข้อความหลังตัดช่องว่างทั้งหมด เช่น "มาตรา108/4" ใช้เทียบกับ TitleCategory</summary>
        public string SectionKey { get; set; }

        public IList<KBSectionCandidate> Candidates { get; set; }

        /// <summary>ArticleID ที่ user เลือก (0 = ยังไม่เลือก)</summary>
        public int SelectedArticleId { get; set; }

        public KBSectionGroup()
        {
            Candidates = new List<KBSectionCandidate>();
        }
    }

    /// <summary>บทความมาตรา 1 record ที่จับคู่กับ SectionKey ได้</summary>
    public class KBSectionCandidate
    {
        public int    ArticleID     { get; set; }
        public string ArticleCode   { get; set; }
        public string TitleCategory { get; set; }

        /// <summary>ปี พ.ศ. ที่ถอดจาก ArticleCode (index 7–9 แล้วเติม '2' ข้างหน้า) เช่น "2535"</summary>
        public string YearDisplay { get; set; }
    }
}
