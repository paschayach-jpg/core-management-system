using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// บทความ Knowledge Base รองรับหลายประเภท (SOP/FAQ/Guideline/Template/Policy)
    /// พร้อม Lifecycle Management (Draft → Review → Published → Archived)
    /// Maps to KMS.t_KBArticle
    /// </summary>
    public class KBArticle
    {
        public int      ArticleID        { get; set; }
        public string   ArticleCode      { get; set; }   // KB-YYYY-NNN
        public string   Title            { get; set; }
        public string   ContentHTML      { get; set; }
        public string   Summary          { get; set; }
        public int      CategoryID       { get; set; }
        public byte     ArticleType      { get; set; }   // 1=SOP 2=FAQ 3=Guideline 4=Template 5=Policy
        public byte     Status           { get; set; }   // 1=Draft 2=Review 3=Published 4=Archived
        public int      CurrentVersion   { get; set; }
        public DateTime? PublishedDate   { get; set; }
        public DateTime? EffectiveDate   { get; set; }   // dtpEffective
        public DateTime? ExpiryDate      { get; set; }
        public DateTime? ReviewDueDate   { get; set; }
        public int      ViewCount        { get; set; }
        public int      LikeCount        { get; set; }
        public bool     IsOfficialPolicy { get; set; }
        public string   Tags             { get; set; }   // comma-separated
        public int      ConfidentialityLevel  { get; set; }   // FK → ConfidentialityLevel enum (1=HC,2=SC,3=NC,4=PC)
        public string   CreatedBy        { get; set; }
        public DateTime CreatedDate      { get; set; }
        public string   ReviewedBy       { get; set; }
        public bool     IsDelete         { get; set; }

        // Display helpers (set by repository / service)
        public string CategoryName       { get; set; }
        public string LinkedEntities     { get; set; }   // pipe-separated EntityIDs from KMS.t_KBArticleLink (ordered by CreatedDate DESC)
        public string ArticleTypeName
        {
            get
            {
                switch (ArticleType)
                {
                    case 1: return "SOP";
                    case 2: return "FAQ";
                    case 3: return "Guideline";
                    case 4: return "Template";
                    case 5: return "Policy";
                    default: return "Unknown";
                }
            }
        }
        public string StatusName
        {
            get
            {
                switch (Status)
                {
                    case 1: return "Draft";
                    case 2: return "Review";
                    case 3: return "Published";
                    case 4: return "Archived";
                    default: return "Unknown";
                }
            }
        }
    }
}
