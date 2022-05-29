using System;
using System.IO;

namespace Frends.FTP.UploadFiles.Definitions
{
    /// <summary>
    /// Transferred file details.
    /// </summary>
    public class FileItem
    {
        /// <summary>
        /// The last modified timestamp of the file, if available.
        /// If not available, set to the default value, i.e. <see cref="DateTime.MinValue"/>.
        /// </summary>
        public DateTime Modified { get; }

        /// <summary>
        /// The name of the file. Does not include the path.
        /// </summary>
        public string Name { get; set; }

        /// <summary>
        /// The full path of the file.
        /// </summary>
        public string FullPath { get; }

        /// <summary>
        /// Constructs a new FileItem from the file in the file path.
        /// </summary>
        /// <param name="fullPath">The full path to the file.</param>
        /// <exception cref="ArgumentException"></exception>
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
        /// Default constructor, use only for testing.
        /// </summary>
        public FileItem() { }
    }
}
