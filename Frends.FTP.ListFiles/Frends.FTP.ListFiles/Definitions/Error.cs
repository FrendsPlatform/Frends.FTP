using System;

namespace Frends.FTP.ListFiles.Definitions;

/// <summary>
/// Error details.
/// </summary>
public class Error
{
    /// <summary>
    /// Summary of the error.
    /// </summary>
    /// <example>FTP directory '/NoFilesHere' doesn't exist.</example>
    public string Message { get; set; }

    /// <summary>
    /// The exception that caused the error.
    /// </summary>
    /// <example>System.ArgumentException: FTP directory '/NoFilesHere' doesn't exist.</example>
    public Exception AdditionalInfo { get; set; }
}
