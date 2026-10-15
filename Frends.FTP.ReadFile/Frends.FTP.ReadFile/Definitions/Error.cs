using System;

namespace Frends.FTP.ReadFile.Definitions;

/// <summary>
/// Error details.
/// </summary>
public class Error
{
    /// <summary>
    /// Summary of the error.
    /// </summary>
    /// <example>Could not find file '/upload/test.txt'.</example>
    public string Message { get; set; }

    /// <summary>
    /// The exception that caused the error.
    /// </summary>
    /// <example>System.ArgumentException: Could not find file '/upload/test.txt'.</example>
    public Exception AdditionalInfo { get; set; }
}
