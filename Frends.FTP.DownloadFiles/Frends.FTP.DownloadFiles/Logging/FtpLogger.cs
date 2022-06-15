using System;
using System.Collections.Concurrent;
using System.IO;
using Frends.FTP.DownloadFiles.Definitions;
using Frends.FTP.DownloadFiles.Enums;
using Serilog;

namespace Frends.FTP.DownloadFiles.Logging;

internal interface IFtpLogger : IDisposable
{
    void NotifyError(BatchContext context, string msg, Exception e);
        
    void NotifyInformation(BatchContext context, string msg);

    void NotifyTrace(string message);

    void LogTransferSuccess(SingleFileTransfer transfer, BatchContext context);

    void LogTransferFailed(SingleFileTransfer transfer, BatchContext context, string errorMessage, Exception exception);
}

internal class FtpLogger : IFtpLogger
{
    private ConcurrentBag<FileTransferInfo> _fileTransfers;
    private ILogger _log;

    private bool _disposed;

    public FtpLogger(ILogger log)
    {
        _fileTransfers = new ConcurrentBag<FileTransferInfo>();
        _log = log;
    }

    ~FtpLogger()
    {
        Dispose(false);
    }

    public void NotifyError(BatchContext context, string msg, Exception e)
    {
        try
        {
            if (context == null)
            {
                context = new BatchContext();
            }

            var sourceEndPointName = GetSourceEndPointName(context);
            var destinationEndPointName = GetDestinationEndPointName(context);
            var transferName = context.Info == null ? "unknown" : context.Info.TransferName;
            var transferNameForLog = transferName ?? string.Empty;

            var errorMessage = $"\r\n\r\nFRENDS FTP file transfer '{transferNameForLog}' from '{sourceEndPointName}' to '{destinationEndPointName}': \r\n{msg}\r\n";
            _log.Error(errorMessage, e);
        }
        catch (Exception ex)
        {
            _log.Error("Error when logging error message: " + ex.Message, ex);
        }
    }

    public void NotifyInformation(BatchContext context, string msg)
    {
        try
        {
            _log.Information(msg);
        }
        catch (Exception ex)
        {
            _log.Error("Error when logging information message: " + ex.Message, ex);
        }
    }

    public void LogTransferSuccess(SingleFileTransfer transfer, BatchContext context)
    {
        try
        {
            var fileTransferInfoForSuccess = CreateFileTransferInfo(TransferResult.Success, transfer, context);
            _fileTransfers.Add(fileTransferInfoForSuccess);
            _log.Information("File transfer succeeded: " + transfer.SourceFile);
        }
        catch (Exception ex)
        {
            _log.Error("Error when logging success message: " + ex.Message, ex);
        }
    }
        
    public void LogTransferFailed(SingleFileTransfer transfer, BatchContext context, string errorMessage, Exception exception)
    {
        try
        {
            var fileTransferInfoForFailure = CreateFileTransferInfo(TransferResult.Fail, transfer, context, errorMessage);
            _fileTransfers.Add(fileTransferInfoForFailure);

            _log.Error("File transfer failed: " + fileTransferInfoForFailure.ErrorInfo, exception);
        }
        catch (Exception ex)
        {
            _log.Error("Error when logging failure: " + ex.Message, ex);
        }
    }

    public void NotifyTrace(string message)
    {
        // only log to debug trace
        _log.Debug(message);
    }

    private string GetSourceEndPointName(BatchContext context)
    {
        return "File: " + context.Source.Directory + context.Source.FileName;
    }

    private string GetDestinationEndPointName(BatchContext context)
    {
        return $"FTP://{context.Connection.Address}/{context.Destination.Directory}/{context.Destination.FileName}";
    }

    public static FileTransferInfo CreateFileTransferInfo(TransferResult result, SingleFileTransfer transfer, BatchContext context, string errorMessage = null)
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
            localFileName = context.Info.WorkDir;
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
            ErrorInfo = errorMessage ?? string.Empty
        };
    }

    private static long GetFileSize(string filepath)
    {
        return File.Exists(filepath) ? new FileInfo(filepath).Length : 0;
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }

    private void Dispose(bool disposing)
    {
        if (_disposed)
        {
            return;
        }

        _fileTransfers = null;
        _log = null;

        _disposed = true;
    }
}

