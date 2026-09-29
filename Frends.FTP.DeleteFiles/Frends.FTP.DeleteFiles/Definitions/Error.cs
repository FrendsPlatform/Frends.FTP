namespace Frends.FTP.DeleteFiles.Definitions;

using System;

/// <summary>
/// Error information.
/// </summary>
public class Error
{
    /// <summary>
    /// Error message describing the failure.
    /// </summary>
    /// <example>Error occured while deleting files: FTP directory '/NoFilesHere' doesn't exist.</example>
    public string Message { get; set; }

    /// <summary>
    /// The exception that caused the failure, if available.
    /// </summary>
    /// <example>System.Exception: Connection refused</example>
    public Exception AdditionalInfo { get; set; }
}
