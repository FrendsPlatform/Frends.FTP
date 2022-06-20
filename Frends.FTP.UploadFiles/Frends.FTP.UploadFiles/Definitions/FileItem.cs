using System;
using System.IO;

namespace Frends.FTP.UploadFiles.Definitions
{
    internal class FileItem
    {
        /// <summary>
        /// The last modified timestamp of the file, if available.
        /// If not available, set to the default value, i.e. <see cref="DateTime.MinValue"/>.
        /// </summary>
        public DateTime Modified { get; }

        public string Name { get; set; }

        public string FullPath { get; }

        public FileItem(string fullPath)
        {
            if (!File.Exists(fullPath))
                throw new ArgumentException($"File does not exist: '{fullPath}");

            var fi = new FileInfo(fullPath);
            Modified = fi.LastWriteTime;
            Name = Path.GetFileName(fullPath);
            FullPath = fullPath;
        }
    }
}
