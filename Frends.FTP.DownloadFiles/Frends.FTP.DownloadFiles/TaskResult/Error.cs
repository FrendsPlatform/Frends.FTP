namespace Frends.FTP.DownloadFiles.TaskResult;

/// <summary>
/// Error information returned when the task encounters a failure and ThrowErrorOnFailure is false.
/// </summary>
public class Error
{
    /// <summary>
    /// Error message describing the failure.
    /// </summary>
    /// <example>FTP transfer failed: Connection refused.</example>
    public string Message { get; set; }

    /// <summary>
    /// The exception that caused the failure, if available.
    /// </summary>
    /// <example>System.Exception: Connection refused</example>
    public Exception AdditionalInfo { get; set; }
}
