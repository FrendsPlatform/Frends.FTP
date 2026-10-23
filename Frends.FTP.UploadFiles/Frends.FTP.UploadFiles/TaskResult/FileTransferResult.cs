using System.Collections.Generic;

namespace Frends.FTP.UploadFiles.TaskResult
{
    internal class FileTransferResult
    {
        /// <summary>
        /// True if action was skipped, otherwise false.
        /// </summary>
        /// <example>false</example>
        public bool ActionSkipped { get; set; }

        /// <summary>
        /// True if the transfer was successful, otherwise false.
        /// </summary>
        /// <example>true</example>
        public bool Success { get; set; }

        /// <summary>
        /// Message of the transfer operations containing a simple representation of what happened in the task.
        /// </summary>
        /// <example>1 files transferred: file1.txt</example>
        public string UserResultMessage { get; set; }

        /// <summary>
        /// Count of files that have been successfully transferred.
        /// </summary>
        /// <example>16</example>
        public int SuccessfulTransferCount { get; set; }

        /// <summary>
        /// Count of files that have not been transferred.
        /// </summary>
        /// <example>2</example>
        public int FailedTransferCount { get; set; }

        /// <summary>
        /// List of transferred file names.
        /// </summary>
        /// <example>[ "file1.txt", "file2.txt" ]</example>
        public IEnumerable<string> TransferredFileNames { get; set; }

        /// <summary>
        /// Dictionary of file names and error messages of the failed transfers.
        /// </summary>
        /// <example>{ "file1.txt": [ "error1" ] }</example>
        public Dictionary<string, IList<string>> TransferErrors { get; set; }

        /// <summary>
        /// List of transferred file paths.
        /// </summary>
        /// <example>[ "/somedir/file1.txt" ]</example>
        public IEnumerable<string> TransferredFilePaths { get; set; }

        /// <summary>
        /// Operations logs for the transfer.
        /// </summary>
        /// <example>{ "2022-05-31 16.21.39.00Z": "operation1" }</example>
        public IDictionary<string, string> OperationsLog { get; set; }
    }
}
