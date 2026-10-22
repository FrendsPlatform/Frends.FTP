using System;
using System.IO;

namespace Frends.FTP.UploadFiles.Definitions
{
    internal class FileItem
    {
        /// <summary>
        /// The last modified timestamp of the file, if available. If not available, set to the default value, i.e. DateTime.MinValue.
        /// </summary>
        /// <example>2024-01-01T12:00:00</example>
        public DateTime Modified { get; }
        /// <summary>
        /// Gets or sets the name.
        /// </summary>
        /// <example>file.txt</example>
        public string Name { get; set; }
        /// <summary>
        /// Gets the full path.
        /// </summary>
        /// <example>file.txt</example>
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