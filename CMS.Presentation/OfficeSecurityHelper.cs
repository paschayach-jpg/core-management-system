using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.IO.Packaging;
using System.Linq;
using System.Security.Cryptography;
using System.Threading.Tasks;
using System.Windows.Forms;
using System.Xml;
using CMS.Controllers.DMS.Document.Interfaces;
using CMS.Domain.Enums;
using CMS.Domain.Models;

namespace CMS.Presentation
{
    /// <summary>
    /// Opens non-PDF files (Word/Excel/PowerPoint and others) according to the user's open mode.
    ///   View → temp copy with an embedded Office editing lock (docx/xlsx/pptx), ReadOnly attribute,
    ///          opened via the "OpenAsReadOnly" shell verb when available.
    ///   Edit → writable temp copy plus an <see cref="OfficeEditSessionDialog"/> to save it back
    ///          into CMS as a new version.
    /// The file always comes from the service (permission check + audit + checksum); the original on
    /// the network share is never opened directly.
    /// Note: this is a soft lock. A View user can still "Save As" a separate copy or copy text out.
    /// </summary>
    internal static class OfficeSecurityHelper
    {
        private const string ReadOnlyVerb = "OpenAsReadOnly";

        private const string RelOfficeDocument = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/officeDocument";
        private const string RelWordSettings   = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/settings";
        private const string RelWorksheet      = "http://schemas.openxmlformats.org/officeDocument/2006/relationships/worksheet";

        private const string NsWord        = "http://schemas.openxmlformats.org/wordprocessingml/2006/main";
        private const string NsSpreadsheet = "http://schemas.openxmlformats.org/spreadsheetml/2006/main";
        private const string NsPresentation = "http://schemas.openxmlformats.org/presentationml/2006/main";

        private const int SpinCount = 100000;

        // CT_Settings children that must precede w:documentProtection (ECMA-376 schema order).
        private static readonly string[] WordSettingsBeforeProtection =
        {
            "writeProtection", "view", "zoom", "removePersonalInformation", "removeDateAndTime",
            "doNotDisplayPageBoundaries", "displayBackgroundShape", "printPostScriptOverText",
            "printFractionalCharacterWidth", "printFormsData", "embedTrueTypeFonts", "embedSystemFonts",
            "saveSubsetFonts", "saveFormsData", "mirrorMargins", "alignBordersAndEdges",
            "bordersDoNotSurroundHeader", "bordersDoNotSurroundFooter", "gutterAtTop",
            "hideSpellingErrors", "hideGrammaticalErrors", "activeWritingStyle", "proofState",
            "formsDesign", "attachedTemplate", "linkStyles", "stylePaneFormatFilter",
            "stylePaneSortMethod", "documentType", "mailMerge", "revisionView", "trackRevisions",
            "doNotTrackMoves", "doNotTrackFormatting"
        };

        // CT_Workbook children that must follow workbookProtection. Anchoring on these (rather than on
        // workbookPr) keeps it after Excel's mc:AlternateContent / xr:revisionPtr extension elements,
        // which Excel otherwise rejects as a corrupt workbook.
        private static readonly string[] WorkbookAfterProtection =
        {
            "bookViews", "sheets", "functionGroups", "externalReferences", "definedNames",
            "calcPr", "oleSize", "customWorkbookViews", "pivotCaches", "smartTagPr",
            "smartTagTypes", "webPublishing", "fileRecoveryPr", "webPublishObjects", "extLst"
        };

        // CT_Worksheet children that must precede sheetProtection.
        private static readonly string[] WorksheetBeforeProtection =
        {
            "sheetPr", "dimension", "sheetViews", "sheetFormatPr", "cols", "sheetData", "sheetCalcPr"
        };

        // CT_Presentation children that must follow modifyVerifier.
        private static readonly string[] PresentationAfterVerifier = { "extLst" };

        private static string OpenFilesRoot
        {
            get { return Path.Combine(Path.GetTempPath(), "CMS", "OpenFiles"); }
        }

        // ── Public API ─────────────────────────────────────────────────────────

        /// <summary>
        /// Opens <paramref name="file"/> in its default application, locked or editable per the
        /// user's <see cref="FileOpenMode"/>. <paramref name="onNewVersionSaved"/> runs after an
        /// edited copy is saved back (e.g. to refresh a file list).
        /// </summary>
        public static void OpenFile(Form owner, FileDocument file, UserContext user,
            IFileBrowseController fileBrowseController, Action onNewVersionSaved = null)
        {
            if (file == null) return;
            if (fileBrowseController == null)
            {
                MessageBox.Show("File browse controller is not available.", "เปิดไฟล์ไม่ได้",
                    MessageBoxButtons.OK, MessageBoxIcon.Warning);
                return;
            }

            string tempDir = null;
            try
            {
                FileOpenMode mode = fileBrowseController.GetFileOpenMode(file.FileID, user);
                if (mode == FileOpenMode.None)
                {
                    MessageBox.Show("คุณไม่มีสิทธิ์เปิดไฟล์นี้", "เปิดไฟล์ไม่ได้",
                        MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                var result = fileBrowseController.DownloadFile(file.FileID, user);
                if (result == null || !result.Success || result.FileStream == null)
                {
                    MessageBox.Show(
                        string.Format("เปิดไฟล์ไม่สำเร็จ: {0}\n{1}", file.OriginalFileName,
                            result != null ? result.ErrorMessage : "ไม่พบข้อมูลไฟล์จาก service"),
                        "เปิดไฟล์ไม่ได้", MessageBoxButtons.OK, MessageBoxIcon.Warning);
                    return;
                }

                tempDir = Path.Combine(OpenFilesRoot, string.Format("{0}_{1:N}", file.FileID, Guid.NewGuid()));
                Directory.CreateDirectory(tempDir);
                string tempPath = Path.Combine(tempDir, SafeFileName(result.FileName ?? file.OriginalFileName, file.FileID));

                using (result.FileStream)
                using (var fs = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    result.FileStream.CopyTo(fs);
                }

                if (mode == FileOpenMode.Edit)
                {
                    StartProcess(tempPath, null);
                    var session = new OfficeEditSessionDialog(file, tempPath, user, fileBrowseController, onNewVersionSaved);
                    session.Show(owner);
                    return;
                }

                // View mode
                ApplyReadOnlyProtection(tempPath);
                File.SetAttributes(tempPath, File.GetAttributes(tempPath) | FileAttributes.ReadOnly);
                StartProcess(tempPath, ReadOnlyVerb);
                ScheduleCleanup(tempDir);
            }
            catch (Exception ex)
            {
                LogError(file.OriginalFileName, ex);
                if (tempDir != null) TryDeleteDirectory(tempDir);
                MessageBox.Show(
                    string.Format("ไม่สามารถเปิดไฟล์ได้: {0}\nข้อผิดพลาด: {1}", file.OriginalFileName, ex.Message),
                    "ข้อผิดพลาด", MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// Deletes temp copies left over from earlier sessions (older than one day).
        /// Safe to call at startup; never throws.
        /// </summary>
        public static void PurgeStaleTempFiles()
        {
            try
            {
                if (!Directory.Exists(OpenFilesRoot)) return;

                DateTime cutoff = DateTime.Now.AddDays(-1);
                foreach (string dir in Directory.GetDirectories(OpenFilesRoot))
                {
                    if (Directory.GetLastWriteTime(dir) < cutoff)
                        TryDeleteDirectory(dir);
                }

                // Loose files written by the old open flow (pre directory-per-open).
                foreach (string path in Directory.GetFiles(OpenFilesRoot))
                {
                    if (File.GetLastWriteTime(path) < cutoff)
                        TryDeleteFile(path);
                }
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("[PurgeFail] {0}", ex.Message));
            }
        }

        /// <summary>
        /// Removes a temp open directory once the application releases the file.
        /// </summary>
        internal static async void ScheduleCleanup(string tempDir)
        {
            // Office keeps a handle on open documents, so deletion fails until the user closes it.
            // Poll every minute for up to 12 hours; PurgeStaleTempFiles catches anything left.
            for (int i = 0; i < 720; i++)
            {
                await Task.Delay(60000);
                if (TryDeleteDirectory(tempDir)) return;
            }
        }

        // ── Process start ──────────────────────────────────────────────────────

        private static void StartProcess(string path, string preferredVerb)
        {
            var psi = new ProcessStartInfo
            {
                FileName = path,
                UseShellExecute = true
            };

            if (!string.IsNullOrEmpty(preferredVerb))
            {
                try
                {
                    string verb = psi.Verbs.FirstOrDefault(v => string.Equals(v, preferredVerb, StringComparison.OrdinalIgnoreCase));
                    if (verb != null) psi.Verb = verb;
                }
                catch (Exception ex)
                {
                    Debug.WriteLine(string.Format("[VerbLookupFail] {0}", ex.Message));
                }
            }

            Process.Start(psi);
        }

        // ── OpenXML editing lock ───────────────────────────────────────────────

        /// <summary>
        /// Embeds an editing restriction into a docx/xlsx/pptx copy. The password is random and
        /// never stored, so nobody can lift the restriction from inside Office. Other formats
        /// (including legacy .doc/.xls/.ppt) rely on the ReadOnly attribute only.
        /// </summary>
        private static void ApplyReadOnlyProtection(string path)
        {
            string ext = (Path.GetExtension(path) ?? string.Empty).ToLowerInvariant();
            if (ext != ".docx" && ext != ".xlsx" && ext != ".pptx")
                return;

            try
            {
                using (Package package = Package.Open(path, FileMode.Open, FileAccess.ReadWrite))
                {
                    PackagePart mainPart = GetMainPart(package);
                    if (mainPart == null)
                        throw new InvalidDataException("ไม่พบ main document part");

                    switch (ext)
                    {
                        case ".docx": ProtectWord(package, mainPart); break;
                        case ".xlsx": ProtectExcel(package, mainPart); break;
                        case ".pptx": ProtectPowerPoint(mainPart); break;
                    }
                }
            }
            catch (Exception ex)
            {
                // Corrupt, encrypted or Strict-OOXML files: fall back to the ReadOnly attribute.
                LogError(path, ex);
            }
        }

        private static PackagePart GetMainPart(Package package)
        {
            var rel = package.GetRelationshipsByType(RelOfficeDocument).FirstOrDefault();
            if (rel == null) return null;

            Uri partUri = PackUriHelper.ResolvePartUri(new Uri("/", UriKind.Relative), rel.TargetUri);
            return package.PartExists(partUri) ? package.GetPart(partUri) : null;
        }

        private static IEnumerable<PackagePart> GetRelatedParts(Package package, PackagePart source, string relType)
        {
            foreach (var rel in source.GetRelationshipsByType(relType))
            {
                if (rel.TargetMode != TargetMode.Internal) continue;
                Uri partUri = PackUriHelper.ResolvePartUri(source.Uri, rel.TargetUri);
                if (package.PartExists(partUri))
                    yield return package.GetPart(partUri);
            }
        }

        private static void ProtectWord(Package package, PackagePart documentPart)
        {
            PackagePart settingsPart = GetRelatedParts(package, documentPart, RelWordSettings).FirstOrDefault();
            if (settingsPart == null)
                throw new InvalidDataException("ไม่พบ word/settings.xml");

            XmlDocument xml = LoadPart(settingsPart);
            XmlElement root = xml.DocumentElement;

            RemoveChildren(root, NsWord, "documentProtection");

            XmlElement protection = xml.CreateElement("w", "documentProtection", NsWord);
            SetWordAttr(protection, "edit", "readOnly");
            SetWordAttr(protection, "enforcement", "1");
            SetWordAttr(protection, "cryptProviderType", "rsaAES");
            SetWordAttr(protection, "cryptAlgorithmClass", "hash");
            SetWordAttr(protection, "cryptAlgorithmType", "typeAny");
            SetWordAttr(protection, "cryptAlgorithmSid", "14");          // 14 = SHA-512
            SetWordAttr(protection, "cryptSpinCount", SpinCount.ToString());
            SetWordAttr(protection, "hash", RandomBase64(64));
            SetWordAttr(protection, "salt", RandomBase64(16));

            InsertAfterLast(root, protection, NsWord, WordSettingsBeforeProtection);
            SavePart(settingsPart, xml);
        }

        private static void ProtectExcel(Package package, PackagePart workbookPart)
        {
            XmlDocument workbook = LoadPart(workbookPart);
            XmlElement wbRoot = workbook.DocumentElement;

            RemoveChildren(wbRoot, NsSpreadsheet, "workbookProtection");
            XmlElement wbProtection = workbook.CreateElement("workbookProtection", NsSpreadsheet);
            wbProtection.SetAttribute("workbookAlgorithmName", "SHA-512");
            wbProtection.SetAttribute("workbookHashValue", RandomBase64(64));
            wbProtection.SetAttribute("workbookSaltValue", RandomBase64(16));
            wbProtection.SetAttribute("workbookSpinCount", SpinCount.ToString());
            wbProtection.SetAttribute("lockStructure", "1");
            InsertBeforeFirst(wbRoot, wbProtection, NsSpreadsheet, WorkbookAfterProtection);
            SavePart(workbookPart, workbook);

            foreach (PackagePart sheetPart in GetRelatedParts(package, workbookPart, RelWorksheet))
            {
                XmlDocument sheet = LoadPart(sheetPart);
                XmlElement sheetRoot = sheet.DocumentElement;

                RemoveChildren(sheetRoot, NsSpreadsheet, "sheetProtection");
                XmlElement sheetProtection = sheet.CreateElement("sheetProtection", NsSpreadsheet);
                sheetProtection.SetAttribute("algorithmName", "SHA-512");
                sheetProtection.SetAttribute("hashValue", RandomBase64(64));
                sheetProtection.SetAttribute("saltValue", RandomBase64(16));
                sheetProtection.SetAttribute("spinCount", SpinCount.ToString());
                sheetProtection.SetAttribute("sheet", "1");
                sheetProtection.SetAttribute("objects", "1");
                sheetProtection.SetAttribute("scenarios", "1");
                InsertAfterLast(sheetRoot, sheetProtection, NsSpreadsheet, WorksheetBeforeProtection);
                SavePart(sheetPart, sheet);
            }
        }

        private static void ProtectPowerPoint(PackagePart presentationPart)
        {
            // PowerPoint has no enforced edit lock; modifyVerifier makes it ask for a password to
            // modify, offering "Read Only" otherwise. Combined with the ReadOnly attribute.
            XmlDocument xml = LoadPart(presentationPart);
            XmlElement root = xml.DocumentElement;

            RemoveChildren(root, NsPresentation, "modifyVerifier");
            XmlElement verifier = xml.CreateElement("p", "modifyVerifier", NsPresentation);
            verifier.SetAttribute("algorithmName", "SHA-512");
            verifier.SetAttribute("hashValue", RandomBase64(64));
            verifier.SetAttribute("saltValue", RandomBase64(16));
            verifier.SetAttribute("spinValue", SpinCount.ToString());

            InsertBeforeFirst(root, verifier, NsPresentation, PresentationAfterVerifier);
            SavePart(presentationPart, xml);
        }

        // ── XML helpers ────────────────────────────────────────────────────────

        private static XmlDocument LoadPart(PackagePart part)
        {
            var xml = new XmlDocument { PreserveWhitespace = true, XmlResolver = null };
            using (Stream s = part.GetStream(FileMode.Open, FileAccess.Read))
            {
                xml.Load(s);
            }
            return xml;
        }

        private static void SavePart(PackagePart part, XmlDocument xml)
        {
            using (Stream s = part.GetStream(FileMode.Create, FileAccess.Write))
            {
                xml.Save(s);
            }
        }

        private static void SetWordAttr(XmlElement element, string name, string value)
        {
            XmlAttribute attr = element.OwnerDocument.CreateAttribute("w", name, NsWord);
            attr.Value = value;
            element.Attributes.Append(attr);
        }

        private static void RemoveChildren(XmlElement parent, string ns, string localName)
        {
            var matches = parent.ChildNodes.Cast<XmlNode>()
                .Where(n => n.NodeType == XmlNodeType.Element && n.NamespaceURI == ns && n.LocalName == localName)
                .ToList();
            foreach (var n in matches)
                parent.RemoveChild(n);
        }

        /// <summary>
        /// Inserts <paramref name="newChild"/> right after the last child whose name is in
        /// <paramref name="predecessors"/>, or as the first element child when none exist.
        /// </summary>
        private static void InsertAfterLast(XmlElement parent, XmlElement newChild, string ns, string[] predecessors)
        {
            XmlNode anchor = parent.ChildNodes.Cast<XmlNode>().LastOrDefault(n =>
                n.NodeType == XmlNodeType.Element && n.NamespaceURI == ns && predecessors.Contains(n.LocalName));

            if (anchor != null)
            {
                parent.InsertAfter(newChild, anchor);
                return;
            }

            XmlNode firstElement = parent.ChildNodes.Cast<XmlNode>().FirstOrDefault(n => n.NodeType == XmlNodeType.Element);
            if (firstElement != null) parent.InsertBefore(newChild, firstElement);
            else parent.AppendChild(newChild);
        }

        /// <summary>
        /// Inserts <paramref name="newChild"/> right before the first child whose name is in
        /// <paramref name="successors"/>, or appends it when none exist.
        /// </summary>
        private static void InsertBeforeFirst(XmlElement parent, XmlElement newChild, string ns, string[] successors)
        {
            XmlNode anchor = parent.ChildNodes.Cast<XmlNode>().FirstOrDefault(n =>
                n.NodeType == XmlNodeType.Element && n.NamespaceURI == ns && successors.Contains(n.LocalName));

            if (anchor != null) parent.InsertBefore(newChild, anchor);
            else parent.AppendChild(newChild);
        }

        private static string RandomBase64(int byteCount)
        {
            var bytes = new byte[byteCount];
            using (var rng = new RNGCryptoServiceProvider())
            {
                rng.GetBytes(bytes);
            }
            return Convert.ToBase64String(bytes);
        }

        // ── File helpers ───────────────────────────────────────────────────────

        private static string SafeFileName(string name, int fileId)
        {
            if (string.IsNullOrWhiteSpace(name))
                return string.Format("file_{0}", fileId);

            string safe = Path.GetFileName(name.Trim());
            foreach (char c in Path.GetInvalidFileNameChars())
                safe = safe.Replace(c, '_');
            return string.IsNullOrWhiteSpace(safe) ? string.Format("file_{0}", fileId) : safe;
        }

        internal static bool TryDeleteDirectory(string dir)
        {
            try
            {
                if (!Directory.Exists(dir)) return true;

                foreach (string path in Directory.GetFiles(dir, "*", SearchOption.AllDirectories))
                {
                    if (!TryDeleteFile(path))
                        return false;
                }
                Directory.Delete(dir, true);
                return true;
            }
            catch (Exception ex)
            {
                Debug.WriteLine(string.Format("[DeleteDirFail] {0}", ex.Message));
                return false;
            }
        }

        private static bool TryDeleteFile(string path)
        {
            FileAttributes original = 0;
            try
            {
                if (!File.Exists(path)) return true;

                original = File.GetAttributes(path);
                if ((original & FileAttributes.ReadOnly) != 0)
                    File.SetAttributes(path, original & ~FileAttributes.ReadOnly);

                File.Delete(path);
                return true;
            }
            catch (Exception ex)
            {
                // Still open in Office: restore ReadOnly so a View copy stays locked.
                try
                {
                    if ((original & FileAttributes.ReadOnly) != 0 && File.Exists(path))
                        File.SetAttributes(path, original);
                }
                catch { }

                Debug.WriteLine(string.Format("[DeleteFail] {0}", ex.Message));
                return false;
            }
        }

        internal static void LogError(string source, Exception ex)
        {
            try
            {
                string logPath = Path.Combine(Path.GetTempPath(), "cms_office_error.log");
                string entry = string.Format(
                    "[{0:yyyy-MM-dd HH:mm:ss}] source=\"{1}\"{2}{3}{2}{2}",
                    DateTime.Now, source, Environment.NewLine, ex);
                File.AppendAllText(logPath, entry);
            }
            catch (Exception logEx)
            {
                Debug.WriteLine(string.Format("[LogFail] {0}", logEx.Message));
            }
        }
    }
}
