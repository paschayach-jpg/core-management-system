using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Controllers.KMS.KB.Interfaces
{
    /// <summary>
    /// View contract ของหน้า "ย้ายบทความ" (implement โดย KBArticleMoveForm) — ① ต้นทาง ② ปลายทาง ③ สรุป/ยืนยัน
    /// </summary>
    public interface IKBArticleMoveController
    {
        void ShowError(string message);
        void ShowSuccess(string message);

        /// <summary>true = ย้ายทั้งหมวดที่เลือก (ทุกบทความในรายการ) / false = เลือกรายบทความด้วยช่องติ๊กในตาราง</summary>
        bool SourceWholeCategory { get; }

        /// <summary>true = รวมบทความของหมวดย่อยทุกชั้น</summary>
        bool IncludeSubCategories { get; }

        /// <summary>หมวดต้นทางที่เลือก (โหมดทั้งหมวด = หมวดที่ติ๊ก, โหมดรายบทความ = หมวดที่คลิกอยู่)</summary>
        IList<int> SelectedSourceCategoryIds { get; }

        /// <summary>บทความที่ติ๊กในตาราง (ใช้เมื่อ <see cref="SourceWholeCategory"/> = false)</summary>
        IList<int> CheckedArticleIds { get; }

        /// <summary>หมวดปลายทางที่เลือก (null = ยังไม่เลือก)</summary>
        int? DestinationCategoryId { get; }

        /// <summary>แสดงต้นไม้หมวดทั้งหมด (ต้นทางและปลายทางใช้ข้อมูลเดียวกัน; ปลายทางแสดงเฉพาะหมวดที่เปิดใช้งาน)</summary>
        void BindCategories(IList<KBCategory> allCategories);

        /// <summary>แสดงรายการบทความต้นทาง; <paramref name="checkAll"/> = ติ๊กทุกแถว (โหมดทั้งหมวด)</summary>
        void BindSourceArticles(IList<KBArticle> articles, bool checkAll);

        /// <summary>สรุปส่วน ③ — ข้อความสรุป + คำเตือน; <paramref name="canMove"/> = เปิดปุ่มย้าย</summary>
        void ShowSummary(string summary, IList<string> warnings, bool canMove);

        /// <summary>ถามยืนยันก่อนย้าย (true = ยืนยัน)</summary>
        bool ConfirmMove(string message);
    }
}
