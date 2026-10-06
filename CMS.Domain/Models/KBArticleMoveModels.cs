using System.Collections.Generic;

namespace CMS.Domain.Models
{
    /// <summary>ผลตรวจก่อนย้ายบทความ (หน้า "ย้ายบทความ") — ยังไม่เขียนฐานข้อมูล</summary>
    public class KBArticleMovePreview
    {
        /// <summary>จำนวนบทความที่จะถูกย้ายจริง (ไม่รวมที่อยู่หมวดปลายทางอยู่แล้ว/ถูกลบ)</summary>
        public int ToMoveCount { get; set; }

        /// <summary>จำนวนที่ข้ามเพราะอยู่หมวดปลายทางอยู่แล้ว</summary>
        public int AlreadyInDestinationCount { get; set; }

        /// <summary>ข้อความเตือน (ไม่บล็อกการย้าย — ผู้ใช้ต้องยืนยัน)</summary>
        public IList<string> Warnings { get; set; } = new List<string>();
    }

    /// <summary>ผลการย้ายบทความ</summary>
    public class KBArticleMoveResult
    {
        public int RequestedCount { get; set; }
        public int MovedCount     { get; set; }
        public int SkippedCount   { get { return RequestedCount - MovedCount; } }
    }
}
