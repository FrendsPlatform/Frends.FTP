using System.Collections.Generic;

namespace Frends.FTP.UploadFiles.Definitions
{
    internal class SingleFileTransferResult
    {
        /// <summary>
        /// Gets or sets the success.
        /// </summary>
        /// <example>true</example>
        public bool Success { get; set; }
        /// <summary>
        /// Gets or sets the action skipped.
        /// </summary>
        /// <example>true</example>
        public bool ActionSkipped { get; set; }
        /// <summary>
        /// Gets or sets the error messages.
        /// </summary>
        /// <example>ErrorMessages</example>
        public IList<string> ErrorMessages { get; set; }
        /// <summary>
        /// Gets or sets the transferred file.
        /// </summary>
        /// <example>file.txt</example>
        public string TransferredFile { get; set; }
        /// <summary>
        /// Gets or sets the transferred file path.
        /// </summary>
        /// <example>file.txt</example>
        public string TransferredFilePath { get; set; }

        public SingleFileTransferResult()
        {
            ErrorMessages = new List<string>();
        }
    }
}