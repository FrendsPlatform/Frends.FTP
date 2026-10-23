using System.Collections.Generic;

namespace Frends.FTP.UploadFiles.Definitions
{
    internal class SingleFileTransferResult
    {
        public SingleFileTransferResult()
        {
            ErrorMessages = new List<string>();
        }

        /// <summary>
        /// True if the transfer was successful, otherwise false.
        /// </summary>
        /// <example>true</example>
        public bool Success { get; set; }

        /// <summary>
        /// True if action was skipped, otherwise false.
        /// </summary>
        /// <example>false</example>
        public bool ActionSkipped { get; set; }

        /// <summary>
        /// Error messages of the transfer.
        /// </summary>
        /// <example>[ "Connection timed out" ]</example>
        public IList<string> ErrorMessages { get; set; }

        /// <summary>
        /// Name of the transferred file.
        /// </summary>
        /// <example>file1.txt</example>
        public string TransferredFile { get; set; }

        /// <summary>
        /// Path of the transferred file.
        /// </summary>
        /// <example>/somedir/file1.txt</example>
        public string TransferredFilePath { get; set; }
    }
}
