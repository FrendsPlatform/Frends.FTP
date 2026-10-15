using FluentFTP;
using System;

namespace Frends.FTP.ReadFile.Definitions;

/// <summary>
/// Return object with private setters.
/// </summary>
public class Result
{
    /// <summary>
    /// Indicates whether the operation completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// Content of the file in string format.
    /// </summary>
    /// <example>This is a test file</example>
    public string Content { get; private set; }

    /// <summary>
    /// Full name of the file.
    /// </summary>
    /// <example>c:\source\Test.txt</example>
	public string Path { get; private set; }

    /// <summary>
    /// Size of the read file.
    /// </summary>
    /// <example>0.001</example>
    public double SizeInMegaBytes { get; private set; }

    /// <summary>
    /// Timestamp of when the file was last modified.
    /// </summary>
    /// <example>2022-06-14T12:45:28.6058477+03:00</example>
    public DateTime LastWriteTime { get; private set; }

    /// <summary>
    /// Error details. Null when Success is true.
    /// </summary>
    /// <example>null</example>
    public Error Error { get; private set; }

    internal Result(bool success, string content = null, FtpListItem file = null, Error error = null)
    {
        Success = success;
        Content = content;
        Path = file?.FullName;
        SizeInMegaBytes = file == null ? 0 : Math.Round(file.Size / 1024d / 1024d, 3);
        LastWriteTime = file?.Modified ?? default;
        Error = error;
    }
}
