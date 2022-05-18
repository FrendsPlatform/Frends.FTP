using System;

namespace Frends.FTP.ListFiles.Definitions
{
    /// <summary>
    /// Single file data.
    /// </summary>
    public class ListObject
    {
        /// <summary>
        /// Last modified timestamp of the file. 
        /// </summary>
        /// <example>3.5.2022 0.00.00</example>
        public DateTime Modified { get; set; }

        /// <summary>
        /// Filename.
        /// </summary>
        /// <example>testfile.txt</example>
	    public string Name { get; set; }

        /// <summary>
        /// Full path of the file.
        /// </summary>
        /// <example>/testfile.txt</example>
        public string FullPath { get; set; }

        /// <summary>
        /// File size.
        /// </summary>
        /// <example>17</example>
	    public long Size { get; set; }

        /// <summary>
        /// File was found and added into result. Success = false if no files found with given parameters. 
        /// </summary>
        /// <example>true</example>
        public bool Success { get; set; }
    }
}




