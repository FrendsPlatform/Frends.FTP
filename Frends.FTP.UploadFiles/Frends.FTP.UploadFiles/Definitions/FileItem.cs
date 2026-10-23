using System;
using System.IO;

namespace Frends.FTP.UploadFiles.Definitions
{
    internal class FileItem
    {
        public FileItem(string fullPath)
        {
            if (!File.Exists(fullPath))
                throw new ArgumentException($"File does not exist: '{fullPath}");

            var fi = new FileInfo(fullPath);
            Modified = fi.LastWriteTime;
            Name = Path.GetFileName(fullPath);
            FullPath = fullPath;
        }

        /// <summary>
        /// The last modified timestamp of the file, if available. If not available, set to the minimum date time value.
        /// </summary>
        /// <example>2024-12-11 10:30:00</example>
        public DateTime Modified { get; }

        /// <summary>
        /// Name of the file.
        /// </summary>
        /// <example>file1.txt</example>
        public string Name { get; set; }

        /// <summary>
        /// Full path of the file.
        /// </summary>
        /// <example>C:\sourcefiles\file1.txt</example>
        public string FullPath { get; }
    }
}
