using System.Collections.Generic;

namespace Frends.FTP.UploadFiles.TaskResult
{
    internal class FileTransferResult
    {
        /// <summary>
        /// Gets or sets the action skipped.
        /// </summary>
        /// <example>true</example>
        public bool ActionSkipped { get; set; }
        /// <summary>
        /// Gets or sets the success.
        /// </summary>
        /// <example>true</example>
        public bool Success { get; set; }
        /// <summary>
        /// Gets or sets the user result message.
        /// </summary>
        /// <example>file.txt</example>
        public string UserResultMessage { get; set; }
        /// <summary>
        /// Gets or sets the successful transfer count.
        /// </summary>
        /// <example>1</example>
        public int SuccessfulTransferCount { get; set; }
        /// <summary>
        /// Gets or sets the failed transfer count.
        /// </summary>
        /// <example>1</example>
        public int FailedTransferCount { get; set; }
        /// <summary>
        /// Gets or sets the transferred file names.
        /// </summary>
        /// <example>TransferredFileNames</example>
        public IEnumerable<string> TransferredFileNames { get; set; }
        /// <summary>
        /// Gets or sets the transfer errors.
        /// </summary>
        /// <example>TransferErrors</example>
        public Dictionary<string, IList<string>> TransferErrors { get; set; }
        /// <summary>
        /// Gets or sets the transferred file paths.
        /// </summary>
        /// <example>TransferredFilePaths</example>
        public IEnumerable<string> TransferredFilePaths { get; set; }
        /// <summary>
        /// Gets or sets the operations log.
        /// </summary>
        /// <example>OperationsLog</example>
        public IDictionary<string, string> OperationsLog { get; set; }
    }
}