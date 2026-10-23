using System;
using System.Collections.Concurrent;
using System.IO;
using Frends.FTP.UploadFiles.Definitions;
using Frends.FTP.UploadFiles.Enums;
using Serilog;

namespace Frends.FTP.UploadFiles.Logging
{
    /// <summary>
    /// FTP internal logger interface
    /// </summary>
    internal interface IFtpLogger : IDisposable
    {
        /// <summary>
        /// Notifies of errors
        /// </summary>
        /// <param name="context">The batch context.</param>
        /// <param name="msg">Error message.</param>
        /// <param name="e">Exception that caused the error.</param>
        void NotifyError(BatchContext context, string msg, Exception e);

        /// <summary>
        /// Notifies of info-level messages
        /// </summary>
        /// <param name="context">The batch context.</param>
        /// <param name="msg">Information message.</param>
        void NotifyInformation(BatchContext context, string msg);

        /// <summary>
        /// Notifies of debug trace messages
        /// </summary>
        /// <param name="message">Trace message.</param>
        void NotifyTrace(string message);

        /// <summary>
        /// Logs a single successful file transfer
        /// </summary>
        /// <param name="transfer">The successful transfer.</param>
        /// <param name="context">The batch context.</param>
        void LogTransferSuccess(SingleFileTransfer transfer, BatchContext context);

        /// <summary>
        /// Logs a single failed file transfer
        /// </summary>
        /// <param name="transfer">The failed transfer.</param>
        /// <param name="context">The batch context.</param>
        /// <param name="errorMessage">Error message of the failure.</param>
        /// <param name="exception">Exception that caused the failure.</param>
        void LogTransferFailed(SingleFileTransfer transfer, BatchContext context, string errorMessage, Exception exception);
    }

    /// <summary>
    /// FTP internal logger implementation
    /// </summary>
    internal class FtpLogger : IFtpLogger
    {
        private ConcurrentBag<FileTransferInfo> fileTransfers;
        private ILogger log;

        private bool disposed;

        public FtpLogger(ILogger log)
        {
            fileTransfers = new ConcurrentBag<FileTransferInfo>();
            this.log = log;
        }

        ~FtpLogger() => Dispose(false);

        /// <summary>
        /// Notifies about an error.
        /// </summary>
        /// <param name="context">The batch context.</param>
        /// <param name="msg">Error message.</param>
        /// <param name="e">Exception that caused the error.</param>
        public void NotifyError(BatchContext context, string msg, Exception e)
        {
            try
            {
                if (context == null) context = new BatchContext();

                var sourceEndPointName = GetSourceEndPointName(context);
                var destinationEndPointName = GetDestinationEndPointName(context);
                var transferName = context.Info == null ? "unknown" : context.Info.TransferName;
                var transferNameForLog = transferName ?? string.Empty;

                var errorMessage = $"\r\n\r\nFrends FTP file transfer '{transferNameForLog}' from '{sourceEndPointName}' to '{destinationEndPointName}': \r\n{msg}\r\n";
                log.Error(errorMessage, e);
            }
            catch (Exception ex)
            {
                log.Error("Error when logging error message: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Notifies about information.
        /// </summary>
        /// <param name="context">The batch context.</param>
        /// <param name="msg">Information message.</param>
        public void NotifyInformation(BatchContext context, string msg)
        {
            try
            {
                log.Information(msg);
            }
            catch (Exception ex)
            {
                log.Error("Error when logging information message: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Logs a successful transfer.
        /// </summary>
        /// <param name="transfer">The successful transfer.</param>
        /// <param name="context">The batch context.</param>
        public void LogTransferSuccess(SingleFileTransfer transfer, BatchContext context)
        {
            try
            {
                var fileTransferInfoForSuccess = CreateFileTransferInfo(TransferResult.Success, transfer, context);
                fileTransfers.Add(fileTransferInfoForSuccess);
                log.Information("File transfer succeeded: " + transfer.SourceFile.Name);
            }
            catch (Exception ex)
            {
                log.Error("Error when logging success message: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Logs a failed transfer.
        /// </summary>
        /// <param name="transfer">The failed transfer.</param>
        /// <param name="context">The batch context.</param>
        /// <param name="errorMessage">Error message of the failure.</param>
        /// <param name="exception">Exception that caused the failure.</param>
        public void LogTransferFailed(SingleFileTransfer transfer, BatchContext context, string errorMessage, Exception exception)
        {
            try
            {
                var fileTransferInfoForFailure = CreateFileTransferInfo(TransferResult.Fail, transfer, context, errorMessage);
                fileTransfers.Add(fileTransferInfoForFailure);

                log.Error("File transfer failed: " + fileTransferInfoForFailure.ErrorInfo, exception);
            }
            catch (Exception ex)
            {
                log.Error("Error when logging failure: " + ex.Message, ex);
            }
        }

        /// <summary>
        /// Notifies a trace message.
        /// </summary>
        /// <param name="message">Trace message.</param>
        public void NotifyTrace(string message)
        {
            // only log to debug trace
            log.Debug(message);
        }

        /// <summary>
        /// Method starts dispose.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        /// Virtual method dispose for SFTPLogger
        /// </summary>
        /// <param name="disposing">True when called from Dispose, false when called from the finalizer.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposed) return;

            fileTransfers = null;
            log = null;

            disposed = true;
        }

        private static string GetSourceEndPointName(BatchContext context)
        {
            if (context.Source.FilePaths != null)
                return "Files:" + string.Join(", ", context.Source.FilePaths);

            return "File: " + context.Source.Directory + context.Source.FileName;
        }

        private static string GetDestinationEndPointName(BatchContext context)
        {
            return $"FTP://{context.Connection.Address}/{context.Destination.Directory}/{context.Destination.FileName}";
        }

        private static FileTransferInfo CreateFileTransferInfo(TransferResult result, SingleFileTransfer transfer, BatchContext context, string errorMessage = null)
        {
            // Create 2 dummy endpoints and initialize some local variables which are needed in case if cobalt config is not
            // successfully initialized, i.e. when there has been a failure creating the config (invalid xml etc..) and config elements are left null
            var sourceFile = string.Empty;
            var destinationFile = string.Empty;
            var localFileName = string.Empty;

            if (transfer != null)
            {
                sourceFile = transfer.SourceFile.Name;
                destinationFile = transfer.DestinationFileNameWithMacrosExpanded;
                if (context != null)
                    localFileName = context.Info != null ? context.Info.WorkDir : string.Empty;
            }

            var transferStarted = DateTime.UtcNow;
            var batchId = Guid.Empty;

            var transferName = string.Empty;

            if (context != null)
            {
                transferStarted = context.BatchTransferStartTime;
                batchId = context.InstanceId;
                transferName = context.Info != null ? context.Info.TransferName : string.Empty;
            }

            return new FileTransferInfo
            {
                Result = result,
                SourceFile = sourceFile ?? string.Empty,
                DestinationFile = destinationFile ?? string.Empty,
                FileSize = GetFileSize(localFileName),
                TransferStarted = transferStarted,
                TransferEnded = DateTime.UtcNow,
                BatchId = batchId,
                TransferName = transferName ?? string.Empty,
                ErrorInfo = errorMessage ?? string.Empty,
            };
        }

        private static long GetFileSize(string filepath)
        {
            return File.Exists(filepath) ? new FileInfo(filepath).Length : 0;
        }
    }
}
