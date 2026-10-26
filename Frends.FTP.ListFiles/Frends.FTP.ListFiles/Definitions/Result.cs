using System.Collections.Generic;

namespace Frends.FTP.ListFiles.Definitions;

/// <summary>
/// List of files.
/// </summary>
public class Result
{
    internal Result(bool success, List<FileItem> files, Error error = null)
    {
        Success = success;
        Files = files;
        Error = error;
    }

    /// <summary>
    /// Indicates whether the operation completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// List of files.
    /// </summary>
    /// <example>[ { "test.txt", "/ListFiles/test.txt", "2022-05-20T13:21:42Z", 1048576 }, { "test2.txt", "/ListFiles/test2.txt", "2022-05-20T13:21:42Z", 1048576 } ]</example>
    public List<FileItem> Files { get; private set; }

    /// <summary>
    /// Error details. Null when Success is true.
    /// </summary>
    /// <example>null</example>
    public Error Error { get; private set; }
}
