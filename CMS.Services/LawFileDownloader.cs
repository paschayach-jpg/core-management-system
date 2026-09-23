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
    /// [Fix ชั่วคราว] แทนที่จะดาวน์โหลดไฟล์จาก URL ใน hyperlink ของ column K (ลิงก์ปลายทางใช้งานไม่ได้ในตอนนี้)
    /// ให้ mapping ด้วยชื่อไฟล์กับไฟล์ที่มีอยู่แล้วใน LocalFolder แทน
    /// TODO: กลับไปดาวน์โหลดจาก URL จริง (ผ่าน HttpClient/WebClient) เมื่อระบบต้นทางพร้อมใช้งาน
    /// </summary>
    public class LawFileDownloader
    {
        private const string LocalFolder = @"C:\Users\paschaya\Downloads\pdf\คำสั่ง\";

        public LawFileDownloader(int timeoutSeconds = 60)
        {
            // timeoutSeconds ไม่ได้ใช้แล้ว (อ่านไฟล์ในเครื่องแทนการดาวน์โหลด) — คง parameter ไว้เพื่อไม่ต้องแก้จุดที่ประกอบ DI
        }

        public LawFileDownloadResult Download(string url, string preferredFileName)
        {
            if (string.IsNullOrWhiteSpace(preferredFileName))
                return Fail("ไม่พบชื่อไฟล์ในเซลล์ (column K)");

            string name = preferredFileName.Trim();

            try
            {
                if (!Directory.Exists(LocalFolder))
                    return Fail(string.Format("ไม่พบโฟลเดอร์ '{0}'", LocalFolder));

                string path = Path.Combine(LocalFolder, name);

                if (!File.Exists(path))
                {
                    // ไม่เจอชื่อตรงเป๊ะ — ลอง match แบบไม่สนตัวพิมพ์เล็ก-ใหญ่ เผื่อชื่อไฟล์จริงเขียนต่างจากในเซลล์เล็กน้อย
                    path = Directory.GetFiles(LocalFolder)
                        .FirstOrDefault(f => string.Equals(Path.GetFileName(f), name, StringComparison.OrdinalIgnoreCase));

                    if (path == null)
                        return Fail(string.Format("ไม่พบไฟล์ '{0}' ในโฟลเดอร์ '{1}'", name, LocalFolder));
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
