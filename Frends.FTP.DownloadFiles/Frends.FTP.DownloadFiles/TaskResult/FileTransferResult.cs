namespace Frends.FTP.DownloadFiles.TaskResult;

internal class FileTransferResult
{
    /// <summary>
    /// Action Skipped.
    /// </summary>
    /// <example>false</example>
    public bool ActionSkipped { get; set; }
    /// <summary>
    /// Success.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }
    /// <summary>
    /// User Result Message.
    /// </summary>
    /// <example>Transfer completed</example>
    public string UserResultMessage { get; set; }
    /// <summary>
    /// Successful Transfer Count.
    /// </summary>
    /// <example>2</example>
    public int SuccessfulTransferCount { get; set; }
    /// <summary>
    /// Failed Transfer Count.
    /// </summary>
    /// <example>0</example>
    public int FailedTransferCount { get; set; }
    /// <summary>
    /// Transferred File Names.
    /// </summary>
    /// <example>file1.txt</example>
    public IEnumerable<string> TransferredFileNames { get; set; }
    /// <summary>
    /// Transfer Errors.
    /// </summary>
    /// <example>file2.txt</example>
    public Dictionary<string, IList<string>> TransferErrors { get; set; }
    /// <summary>
    /// Transferred File Paths.
    /// </summary>
    /// <example>/out/file1.txt</example>
    public IEnumerable<string> TransferredFilePaths { get; set; }
    /// <summary>
    /// Operations Log.
    /// </summary>
    /// <example>Connected</example>
    public IDictionary<string, string> OperationsLog { get; set; }
    /// <summary>
    /// Error.
    /// </summary>
    /// <example>null</example>
    public Error Error { get; set; }
}