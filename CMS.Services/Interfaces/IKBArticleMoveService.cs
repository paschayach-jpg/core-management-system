using System.Collections.Generic;
using CMS.Domain.Models;

namespace CMS.Services.Interfaces
{
    /// <summary>ย้ายบทความ KMS ไปหมวดอื่นที่มีอยู่แล้ว (หน้า "ย้ายบทความ") — เปลี่ยนเฉพาะ CategoryID ไม่สร้าง version ใหม่</summary>
    public interface IKBArticleMoveService
    {
        /// <summary>บทความต้นทางของหมวดที่เลือก; <paramref name="includeSubCategories"/> = รวมหมวดย่อยทุกชั้น</summary>
        IList<KBArticle> GetMoveCandidates(IEnumerable<int> categoryIds, bool includeSubCategories);

        /// <summary>ตรวจก่อนย้าย: นับจำนวน + คำเตือน (ข้ามกฎหมาย/ไม่ใช่กฎหมาย, หมวดปลายทางมีหมวดย่อย, display profile ต่างกัน ฯลฯ) ไม่เขียนฐานข้อมูล</summary>
        KBArticleMovePreview PreviewMove(IEnumerable<int> articleIds, int toCategoryId);

        /// <summary>ย้ายบทความ — โยน exception ถ้าไม่มีบทความ/หมวดปลายทางไม่พบหรือถูกปิดใช้งาน; บันทึก audit ต่อบทความ</summary>
        KBArticleMoveResult Move(IEnumerable<int> articleIds, int toCategoryId, UserContext user);
    }
}
