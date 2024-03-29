﻿using Serilog;
using System.ComponentModel;
using Frends.FTP.DownloadFiles.Definitions;
using Frends.FTP.DownloadFiles.Logging;
using Frends.FTP.DownloadFiles.TaskConfiguration;
using Frends.FTP.DownloadFiles.TaskResult;

namespace Frends.FTP.DownloadFiles;

/// <summary>
/// Class containing the FTP.DownloadFiles task
/// </summary>
public static class FTP
{
    /// <summary>
    /// Download files from an FTP server.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.FTP.DownloadFiles)
    /// </summary>
    /// <frendsdocs>
    /// # Transfer overview
    /// The file transfer progress has the following steps:
    ///
    /// 1. Initialize
    ///     
    ///     Initializes the transfer and opens the source connection.
    ///
    /// 1. ListFiles
    ///     
    ///     Get a list of files from the source endpoint according to the filename/mask. If there are no files to transfer, the source connections are closed, and the transfer finishes. The result of the task will then depend on what the option `NoSourceAction` was set to. If it was set to Error, either the #result.Success property will be set to false, or if the `ThrowErrorOnFail` option was also set, an exception is thrown. If the `NoSourceAction` was set to `Info` or `Ignore`, the `#result.Success` will be set to true and `#result.ActionSkipped` also set to true.
    ///
    /// 1. Transfer files
    ///
    ///     If there are files to transfer, they are then transferred individually. For every file in the list returned from the source endpoint, the following process is repeated:
    ///
    ///     1. GetFile
    ///     
    ///         Get a file from the source endpoint to the local work directory. If the parameter `RenameSourceFileBeforeTransfer` is set to `true`, the file is first renamed with a temporary filename before transfer.
    ///
    ///     1. Rename or move the source file.
    ///         
    ///         This is done before transferring the file to the destination, this means that possible errors in the renaming or moving that would cause the transfer to fail will happen as early as possible - before we actually try to transfer files onward.
    ///
    ///     1. Download the file.
    ///
    ///         If destination file already exists, depending on the parameter `DestinationFileExistsAction` either an exception is thrown, the destination file is overwritten or the source file is appended to the destination file.
    ///
    ///         If the parameter `RenameDestinationFileDuringTransfer` is `true`, the file is first transferred with a temporary file name and afterwards renamed to intended filename, otherwise the file is transferred with the intended filename. The intended filename has its possible file masks expanded.
    ///
    ///     1. Do the source file operation.
    ///         
    ///         Perform the operation defined by the `SourceOperation`.
    ///
    /// 1. Finish
    ///
    ///     Close the source and destination endpoint connections.
    ///
    ///     If the transfer is cancelled (e.g. by calling Terminate on the process instance), the files that are currently being transferred will be processed until finished, but no new files will be transferred. The cancelled transfer end result will be Failed.
    ///
    /// # Macro reference
    ///
    /// Macros can be used to dynamically configure source directory, destination directory or destination file name for a file transfer.
    ///
    /// Generally the following rules apply for macros:
    ///
    /// - Macros are case insensitive.
    /// - You can use any number of macros in all of the cases.
    /// - Dates and times are formatted with leading zeros.
    ///
    /// The following macros can be used with all of dynamically configurable locations for file transfer:
    ///
    /// - %Ticks% = will be replace with the current time as Ticks.
    /// - %DateTime% = will be replaced with date and time in format: "yyyy-MM-dd-HH-mm-ss".
    /// - %DateTimeMs% = will be replace with date and time in format: "yyyy-MM-dd-HH-mm-ss-fff".
    /// - %Date% = will be replaced with date in format: "yyyy-MM-dd".
    /// - %Time% = will be replaced with time in format: "HH-mm-ss".
    /// - %Year% = will be replaced with current year.
    /// - %Month% = will be replaced with current month.
    /// - %Day% = will be replaced with current day.
    /// - %Hour% = will be replaced with current hour.
    /// - %Minute% = will be replaced with current minute.
    /// - %Second% = will be replaced with current second.
    /// - %Millisecond% = will be replaced with current millisecond.
    /// - %WeekDay% = will be replaced with a number of weekday, ranging from 1 (monday) to 7 (sunday).
    /// - %Guid% = will be replaced with a new unique identifier.
    /// - %TransferId% = will be replaced with the transfer id.
    /// - %TransferName% = will be replaced with TransferName parameter specified in Connection point schema.
    /// - %TransferGroupName% = will be replaced with TransferGroupName parameter specified in routine's task arguments.
    ///
    /// Destination file name has two additional macros that can be used for dynamically creating destination file name.
    ///
    /// - %SourceFileName% = will be replaced with source file name without extension.
    /// - %SourceFileExtension% = will be replaced with source file's extension, with the dot '.' included, i.e. if the source file is named 'foo.txt', the %SourceFileExtension% will be expanded as '.txt'. If the source file name does not have an extension, the macro result will be empty, i.e. for original file name "foo", "bar%SourceFileExtension%" will result in "bar"
    /// </frendsdocs>
    /// <param name="info">Transfer info parameters</param>
    /// <param name="connection">Transfer connection parameters</param>
    /// <param name="source">Source file location</param>
    /// <param name="destination">Destination directory location</param>
    /// <param name="options">Transfer options</param>
    /// <param name="cancellationToken">CancellationToken is given by Frends</param>
    /// <returns>Result object {bool ActionSkipped, bool Success, string UserResultMessage, int SuccessfulTransferCount, int FailedTransferCount, string FileName, string SourcePath, string DestinationPath, bool Success} </returns>
    public static Result DownloadFiles(
        [PropertyTab] Source source,
        [PropertyTab] Destination destination,
        [PropertyTab] Connection connection,
        [PropertyTab] Options options,
        [PropertyTab] Info info,
        CancellationToken cancellationToken)
    {
        var maxLogEntries = options.OperationLog ? (int?)null : 100;
        var transferSink = new TransferLogSink(maxLogEntries);
        var operationsLogger = new LoggerConfiguration()
            .MinimumLevel.Debug()
            .WriteTo.Sink(transferSink)
            .CreateLogger();
        var fileTransferLog = Log.Logger;

        using (var logger = InitializeFtpLogger(operationsLogger))
        {
            if (string.IsNullOrEmpty(info.ProcessUri))
                fileTransferLog.Warning("ProcessUri is empty. This means the transfer view cannot link to the correct page");

            if (!Guid.TryParse(info.TaskExecutionID, out var executionId))
            {
                fileTransferLog.Warning("'{0}' is not a valid task execution ID, will default to random Guid", info.TaskExecutionID);
                executionId = Guid.NewGuid();
            }

            var batchContext = new BatchContext
            {
                Info = info,
                Options = options,
                InstanceId = executionId,
                BatchTransferStartTime = DateTime.Now,
                Source = source,
                Destination = destination,
                Connection = connection
            };

            var fileTransporter = new FileTransporter(logger, batchContext, executionId);
            var result = fileTransporter.Run(cancellationToken);

            if (options.ThrowErrorOnFail && !result.Success)
                throw new Exception($"FTP transfer failed: {result.UserResultMessage}. " +
                                    $"Latest operations: \n{GetLogLines(transferSink.GetBufferedLogMessages())}");

            if (options.OperationLog)
                result.OperationsLog = GetLogDictionary(transferSink.GetBufferedLogMessages());

            return new Result(result);
        }
    }

    private static string GetLogLines(IEnumerable<Tuple<DateTimeOffset, string>> buffer)
    {
        try
        {
            return string.Join("\n", buffer.Select(x => x.Item1 == DateTimeOffset.MinValue ? "..." : $"{x.Item1:HH:mm:ssZ}: {x.Item2}"));
        }
        catch (Exception e)
        {
            return $"Error while creating log: \n{e}";
        }
    }

    private static FtpLogger InitializeFtpLogger(ILogger notificationLogger)
    {
        var logger = new FtpLogger(notificationLogger);
        return logger;
    }

    private static IDictionary<string, string> GetLogDictionary(IList<Tuple<DateTimeOffset, string>> entries)
    {
        const string dateFormat = "yyyy-MM-dd HH:mm:ss.f0Z";

        try
        {
            return entries
                .Where(e => e?.Item2 != null) // Filter out nulls
                .ToLookup(
                    x => x.Item1.ToString(dateFormat))
                .ToDictionary(
                    x => x.Key,
                    x => string.Join("\n", x.Select(k => k.Item2)));
        }
        catch (Exception e)
        {
            return new Dictionary<string, string>
            {
                { DateTimeOffset.Now.ToString(dateFormat), $"Error while creating operation log: \n{e}" }
            };
        }
    }
}