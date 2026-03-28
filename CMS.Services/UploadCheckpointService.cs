using System;
using System.IO;
using System.IO.IsolatedStorage;
using System.Xml.Serialization;
using CMS.Domain.Models;
using CMS.Infrastructure.Interfaces;
using CMS.Services.Interfaces;

namespace CMS.Services
{
    /// <summary>
    /// Persists upload checkpoints to per-user per-machine IsolatedStorage using XML serialization.
    /// Supports save, load, delete, and expiry cleanup (Requirements 10.1, 10.3, 10.5, 10.6).
    /// </summary>
    public class UploadCheckpointService : IUploadCheckpointService
    {
        private readonly ILoggingService _logger;
        private static readonly XmlSerializer _serializer = new XmlSerializer(typeof(UploadCheckpoint));

        public UploadCheckpointService(ILoggingService logger)
        {
            if (logger == null) throw new ArgumentNullException("logger");
            _logger = logger;
        }

        // File naming: checkpoint_{folderId}_{sanitizedFileName}.xml
        private static string GetFileName(int folderId, string fileName)
        {
            // Sanitize fileName — replace any char that is invalid in a file name with '_'
            var sanitized = fileName ?? string.Empty;
            foreach (char c in Path.GetInvalidFileNameChars())
                sanitized = sanitized.Replace(c, '_');

            return string.Format("checkpoint_{0}_{1}.xml", folderId, sanitized);
        }

        /// <summary>Req 10.1 — Save checkpoint to IsolatedStorage.</summary>
        public void Save(UploadCheckpoint checkpoint)
        {
            if (checkpoint == null) throw new ArgumentNullException("checkpoint");

            string fileName = GetFileName(checkpoint.FolderID, checkpoint.FileName);
            using (var store = IsolatedStorageFile.GetUserStoreForAssembly())
            using (var stream = new IsolatedStorageFileStream(fileName, FileMode.Create, FileAccess.Write, store))
            {
                _serializer.Serialize(stream, checkpoint);
            }
        }

        /// <summary>Req 10.3 — Load checkpoint by folderId + fileName. Returns null if not found.</summary>
        public UploadCheckpoint Load(int folderId, string fileName)
        {
            string file = GetFileName(folderId, fileName);
            using (var store = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                if (!store.FileExists(file))
                    return null;

                try
                {
                    using (var stream = new IsolatedStorageFileStream(file, FileMode.Open, FileAccess.Read, store))
                    {
                        return (UploadCheckpoint)_serializer.Deserialize(stream);
                    }
                }
                catch (Exception ex)
                {
                    _logger.Warn(string.Format(
                        "UploadCheckpointService: corrupt checkpoint file '{0}' — deleting. ({1})", file, ex.Message));
                    TryDeleteFile(store, file);
                    return null;
                }
            }
        }

        /// <summary>Req 10.5 — Delete checkpoint when upload completes.</summary>
        public void Delete(int folderId, string fileName)
        {
            string file = GetFileName(folderId, fileName);
            using (var store = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                TryDeleteFile(store, file);
            }
        }

        /// <summary>
        /// Req 10.6 — Delete all checkpoints older than maxAge.
        /// Corrupt files are deleted and logged — not thrown.
        /// </summary>
        public void DeleteExpired(TimeSpan maxAge)
        {
            using (var store = IsolatedStorageFile.GetUserStoreForAssembly())
            {
                string[] files;
                try
                {
                    files = store.GetFileNames("checkpoint_*.xml");
                }
                catch (Exception ex)
                {
                    _logger.Warn(string.Format(
                        "UploadCheckpointService.DeleteExpired: could not enumerate checkpoint files. ({0})", ex.Message));
                    return;
                }

                DateTime cutoff = DateTime.Now - maxAge;

                foreach (string file in files)
                {
                    UploadCheckpoint cp = null;
                    try
                    {
                        using (var stream = new IsolatedStorageFileStream(file, FileMode.Open, FileAccess.Read, store))
                        {
                            cp = (UploadCheckpoint)_serializer.Deserialize(stream);
                        }
                    }
                    catch (Exception ex)
                    {
                        _logger.Warn(string.Format(
                            "UploadCheckpointService.DeleteExpired: corrupt file '{0}' — deleting. ({1})", file, ex.Message));
                        TryDeleteFile(store, file);
                        continue;
                    }

                    if (cp.CheckpointDate < cutoff)
                    {
                        TryDeleteFile(store, file);
                    }
                }
            }
        }

        private static void TryDeleteFile(IsolatedStorageFile store, string file)
        {
            try
            {
                if (store.FileExists(file))
                    store.DeleteFile(file);
            }
            catch
            {
                // Best-effort delete — ignore errors
            }
        }
    }
}
