using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Domain model สำหรับ Quick Access / Favorite Folder
    /// Requirements: 7.5
    /// </summary>
    public class QuickAccessFolder
    {
        public int QuickAccessID { get; set; }
        public int UserID        { get; set; }
        public int FolderID      { get; set; }
        public DateTime AddedDate { get; set; }

        /// <summary>Transient — populated by service layer, not stored in DB.</summary>
        public Folder Folder { get; set; }
    }
}
