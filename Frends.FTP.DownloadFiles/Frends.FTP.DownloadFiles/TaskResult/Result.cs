namespace Frends.FTP.DownloadFiles.TaskResult;

/// <summary>
/// Frends.FTP.DownloadFiles result.
/// </summary>
public class Result
{
    /// <summary>
    /// True if action was skipped, otherwise false.
    /// </summary>
    /// <example>false</example>
    public bool ActionSkipped { get; }

    /// <summary>
    /// True if the transfer was successful, otherwise false.
    /// </summary>
    /// <example>false</example>
    public bool Success { get; }

    /// <summary>
    /// Message of the transfer operations containing a simple representation of what happened in the task.
    /// </summary>
    /// <example>1 files transferred: file1.txt</example>
    public string UserResultMessage { get; }

    /// <summary>
    /// Count of files that has been successfully transferred.
    /// </summary>
    /// <example>16</example>
    public int SuccessfulTransferCount { get; }

    /// <summary>
    /// Count of files that have not been transferred.
    /// </summary>
    /// <example>2</example>
    public int FailedTransferCount { get; }

    /// <summary>
    /// List of transferred file names.
    /// </summary>
    /// <example>[ "file1.txt", "file2.txt" ]</example>
    public IEnumerable<string> TransferredFileNames { get; }

    /// <summary>
    /// Dictionary of file names and errors messages of the failed transfers.
    /// </summary>
    /// <example>
    /// {
    ///     { "file1.txt", [ "error1", "error2" ] },
    ///     { "file2.txt", [ "error1", "error2" ] }
    /// }</example>
    public Dictionary<string, IList<string>> TransferErrors { get; }

    /// <summary>
    /// List of transferred file paths.
    /// </summary>
    /// <example>[ "C:\dir\file1.txt", "C:\dir\file2.txt" ]</example>
    public IEnumerable<string> TransferredFilePaths { get; }

    /// <summary>
    /// Operations logs for the transfer.
    /// </summary>
    /// <example>
    /// {
    ///     { "2022-05-31 16.21.39.00Z", "operation1" },
    ///     { "2022-05-31 16.22.39.00Z", "operation2" },
    ///     { "2022-05-31 16.23.39.00Z", "operation3" }
    /// }</example>
    public IDictionary<string, string> OperationsLog { get; }

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
}