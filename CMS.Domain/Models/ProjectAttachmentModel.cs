using System;

namespace CMS.Domain.Models
{
    /// <summary>
    /// Link row: RGP project  &lt;-&gt;  DMS file (storage.FileDocument).
    /// Maps to storage.t_project_attachment.
    /// The display fields (n_file_name .. i_confidentiality_level) are populated
    /// by the repository JOIN and are not stored in the link table itself.
    /// </summary>
    public class ProjectAttachmentModel
    {
        // ── link table columns ───────────────────────────────────────────────
        public int      i_attach_id   { get; set; }
        public int      i_project_id  { get; set; }
        public int      i_file_id     { get; set; }
        public string   i_create_user { get; set; }
        public DateTime d_create_date { get; set; }

        // ── joined from storage.FileDocument / storage.Folder (display only) ──
        public string   n_file_name          { get; set; }
        public string   c_file_ext           { get; set; }
        public long     l_file_size_bytes    { get; set; }
        public int      i_uploaded_by        { get; set; }
        public DateTime d_uploaded_date      { get; set; }
        public int      i_confidentiality_level { get; set; }
        public string   n_folder_name        { get; set; }
    }
}
