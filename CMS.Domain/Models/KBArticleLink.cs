using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Links KB articles to other entities (Folder/Task/Program/Law/Project)
    /// Maps to KMS.t_KBArticleLink
    /// EntityType: 1=Folder, 2=Task, 3=Program, 4=Law, 5=Project
    /// </summary>
    public class KBArticleLink
    {
        public int      LinkID      { get; set; }  // รหัส Link (PK)
        public int      ArticleID   { get; set; }  // FK → KMS.t_KBArticle
        public byte     EntityType  { get; set; }  // 1=Folder 2=Task 3=Program 4=Law 5=Project
        public string      EntityID    { get; set; }  // ID ของ Entity ที่เชื่อม
        public string   CreatedBy   { get; set; }  // ADUserName ผู้สร้าง Link
        public DateTime CreatedDate { get; set; }  // วันเวลาที่สร้าง
    }
}
