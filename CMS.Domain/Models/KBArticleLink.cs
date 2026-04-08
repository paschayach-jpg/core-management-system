using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// เชื่อม Article กับ Entity อื่น (Folder/Task/Program/Law/Project) แบบ Many-to-Many
    /// Maps to KMS.t_KBArticleLink
    /// EntityType: 1=Folder 2=Task 3=Program 4=Law 5=Project
    /// </summary>
    public class KBArticleLink
    {
        public int      LinkID      { get; set; }
        public int      ArticleID   { get; set; }
        public byte     EntityType  { get; set; }
        public int      EntityID    { get; set; }
        public string   CreatedBy   { get; set; }
        public DateTime CreatedDate { get; set; }

        public string EntityTypeName
        {
            get
            {
                switch (EntityType)
                {
                    case 1: return "Folder";
                    case 2: return "Task";
                    case 3: return "Program";
                    case 4: return "Law";
                    case 5: return "Project";
                    default: return "Unknown";
                }
            }
        }
    }
}
