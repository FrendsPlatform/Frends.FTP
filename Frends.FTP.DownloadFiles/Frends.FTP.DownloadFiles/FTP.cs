using Serilog;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Threading;
using Frends.FTP.DownloadFiles.Definitions;
using Frends.FTP.DownloadFiles.Logging;
using Frends.FTP.DownloadFiles.TaskConfiguration;
using Frends.FTP.DownloadFiles.TaskResult;

namespace Frends.FTP.DownloadFiles
{
    /// <summary>
    /// Class containing the FTP.UploadFiles task
    /// </summary>
    public static class FTP
    {
        /// <summary>
        /// Upload files to an FTP server.
        /// [Documentation](https://tasks.frends.com/tasks#frends-tasks/Frends.FTP.DownloadFiles)
        /// </summary>
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
                {
                    fileTransferLog.Warning("ProcessUri is empty. This means the transfer view cannot link to the correct page");
                }

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
                    throw new Exception($"SFTP transfer failed: {result.UserResultMessage}. " +
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
}