using System.Collections.Generic;

namespace Frends.FTP.UploadFiles.TaskResult
{
    /// <summary>
    /// Return object with private setters.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// Boolean value of the skipped Action.
        /// </summary>
        public bool ActionSkipped { get; }

        /// <summary>
        /// Boolean value of the successful transfer.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Message of the transfer operations.
        /// </summary>
        public string UserResultMessage { get; }

        /// <summary>
        /// Count of files that has been successfully transferred.
        /// </summary>
        public int SuccessfulTransferCount { get; }

        /// <summary>
        /// Count of files that have not been transferred.
        /// </summary>
        public int FailedTransferCount { get; }

        /// <summary>
        /// List of transferred file names.
        /// </summary>
        public IEnumerable<string> TransferredFileNames { get; }

        /// <summary>
        /// Dictionary of file names and errors messages of the failed transfers.
        /// </summary>
        public Dictionary<string, IList<string>> TransferErrors { get; }

        /// <summary>
        /// List of transferred file paths.
        /// </summary>
        public IEnumerable<string> TransferredFilePaths { get; }

        /// <summary>
        /// Operations logs for the transfer.
        /// </summary>
        public IDictionary<string, string> OperationsLog { get; }

        internal Result(Dictionary<string, IList<string>> transferErrors)
        {
            TransferErrors = transferErrors;
        }

        internal Result(FileTransferResult result)
        {
            ActionSkipped = result.ActionSkipped;
            Success = result.Success;
            UserResultMessage = result.UserResultMessage;
            SuccessfulTransferCount = result.SuccessfulTransferCount;
            FailedTransferCount = result.FailedTransferCount;
            TransferredFileNames = result.TransferredFileNames;
            TransferErrors = result.TransferErrors;
            TransferredFilePaths = result.TransferredFilePaths;
            OperationsLog = result.OperationsLog;
        }

        internal Result(
            bool actionSkipped, 
            bool success, 
            string userResultMessage, 
            int successfulTransferCount, 
            int failedTransferCount, 
            IEnumerable<string> transferredFileNames, 
            Dictionary<string, IList<string>> transferErrors, 
            IEnumerable<string> transferredFilePaths, 
            IDictionary<string, string> operationsLog)
        {
            ActionSkipped = actionSkipped;
            Success = success;
            UserResultMessage = userResultMessage;
            SuccessfulTransferCount = successfulTransferCount;
            FailedTransferCount = failedTransferCount;
            TransferredFileNames = transferredFileNames;
            TransferErrors = transferErrors;
            TransferredFilePaths = transferredFilePaths;
            OperationsLog = operationsLog;
        }
    }
}
