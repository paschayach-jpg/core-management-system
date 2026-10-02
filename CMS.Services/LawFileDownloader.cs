using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace CMS.Services
{
    /// <summary>ผลการดาวน์โหลด/อ่านไฟล์แนบของแถว import</summary>
    public class LawFileDownloadResult
    {
        public bool   Success      { get; set; }
        public string ErrorMessage { get; set; }
        public byte[] Content      { get; set; }
        public string FileName     { get; set; }
        public string MimeType     { get; set; }
    }

    /// <summary>
    /// อ่านไฟล์แนบของแถว import จาก "โฟลเดอร์ในเครื่องที่ผู้ใช้เลือกในหน้า import" (ไม่ตั้งตายตัวในโค้ดแล้ว) โดยจับคู่ด้วยชื่อไฟล์ที่ Template map เป็น
    /// Attachment: ชื่อต้องตรงกับไฟล์ในโฟลเดอร์นั้นครบนามสกุล (ตรงเป๊ะก่อน ไม่เจอลองไม่สนตัวพิมพ์เล็ก-ใหญ่) และไม่ค้นโฟลเดอร์ย่อย
    /// TODO: ดาวน์โหลดจาก URL ใน hyperlink ของเซลล์ (ผ่าน HttpClient/WebClient) เมื่อระบบต้นทางพร้อมใช้งาน — ตอนนี้ URL ถูกอ่านเก็บไว้แต่ไม่ถูกใช้
    /// </summary>
    public class LawFileDownloader
    {
        public LawFileDownloader(int timeoutSeconds = 60)
        {
            // timeoutSeconds ไม่ได้ใช้แล้ว (อ่านไฟล์ในเครื่องแทนการดาวน์โหลด) — คง parameter ไว้เพื่อไม่ต้องแก้จุดที่ประกอบ DI
        }

        /// <param name="sourceFolder">โฟลเดอร์ที่เก็บไฟล์แนบ (ผู้ใช้เลือก) — ว่าง = ล้มเหลวพร้อมข้อความ</param>
        /// <param name="url">URL ใน hyperlink ของเซลล์ (ยังไม่ใช้)</param>
        /// <param name="preferredFileName">ชื่อไฟล์ในเซลล์ที่ Template map เป็น Attachment</param>
        public LawFileDownloadResult Download(string sourceFolder, string url, string preferredFileName)
        {
            if (string.IsNullOrWhiteSpace(sourceFolder))
                return Fail("ยังไม่ได้เลือกโฟลเดอร์ไฟล์แนบ");

            if (string.IsNullOrWhiteSpace(preferredFileName))
                return Fail("ไม่พบชื่อไฟล์ในเซลล์ที่ Template map เป็น Attachment");

            string name = preferredFileName.Trim();

            // ต้องเป็นชื่อไฟล์ล้วน ๆ — กันชื่อในเซลล์ชี้ออกนอกโฟลเดอร์ที่เลือก (path เต็ม, ..\ ฯลฯ)
            if (name.IndexOfAny(Path.GetInvalidFileNameChars()) >= 0
                || !string.Equals(Path.GetFileName(name), name, StringComparison.Ordinal))
                return Fail(string.Format("ชื่อไฟล์ '{0}' ไม่ถูกต้อง (ต้องเป็นชื่อไฟล์ล้วน ไม่มี path)", name));

            try
            {
                if (!Directory.Exists(sourceFolder))
                    return Fail(string.Format("ไม่พบโฟลเดอร์ '{0}'", sourceFolder));

                string path = Path.Combine(sourceFolder, name);

                if (!File.Exists(path))
                {
                    // ไม่เจอชื่อตรงเป๊ะ — ลอง match แบบไม่สนตัวพิมพ์เล็ก-ใหญ่ เผื่อชื่อไฟล์จริงเขียนต่างจากในเซลล์เล็กน้อย
                    path = Directory.GetFiles(sourceFolder)
                        .FirstOrDefault(f => string.Equals(Path.GetFileName(f), name, StringComparison.OrdinalIgnoreCase));

                    if (path == null)
                        return Fail(string.Format("ไม่พบไฟล์ '{0}' ในโฟลเดอร์ '{1}'", name, sourceFolder));
                }

                byte[] bytes = File.ReadAllBytes(path);
                if (bytes.Length == 0)
                    return Fail(string.Format("ไฟล์ '{0}' ว่าง (0 byte)", name));

                string actualName = Path.GetFileName(path);

                return new LawFileDownloadResult
                {
                    Success  = true,
                    Content  = bytes,
                    FileName = actualName,
                    MimeType = GetMimeType(Path.GetExtension(actualName)),
                };
            }
            catch (Exception ex)
            {
                return Fail(string.Format("อ่านไฟล์ '{0}' ไม่สำเร็จ: {1}", name, ex.Message));
            }
        }

        // ── Helpers ──────────────────────────────────────────────────────────

        private static LawFileDownloadResult Fail(string message)
        {
            return new LawFileDownloadResult { Success = false, ErrorMessage = message };
        }

        private static readonly Dictionary<string, string> MimeMap =
            new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
        {
            { ".pdf",  "application/pdf" },
            { ".doc",  "application/msword" },
            { ".docx", "application/vnd.openxmlformats-officedocument.wordprocessingml.document" },
            { ".xls",  "application/vnd.ms-excel" },
            { ".xlsx", "application/vnd.openxmlformats-officedocument.spreadsheetml.sheet" },
            { ".ppt",  "application/vnd.ms-powerpoint" },
            { ".pptx", "application/vnd.openxmlformats-officedocument.presentationml.presentation" },
            { ".jpg",  "image/jpeg" },
            { ".png",  "image/png" },
            { ".gif",  "image/gif" },
            { ".zip",  "application/zip" },
        };

        private static string GetMimeType(string extension)
        {
            string mime;
            if (!string.IsNullOrEmpty(extension) && MimeMap.TryGetValue(extension, out mime))
                return mime;
            return "application/octet-stream";
        }
    }
}
