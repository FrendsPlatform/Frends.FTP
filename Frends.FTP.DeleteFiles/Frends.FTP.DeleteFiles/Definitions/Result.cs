namespace Frends.FTP.DeleteFiles.Definitions;

using System.Collections.Generic;

/// <summary>
/// Result class usually contains properties of the return object.
/// </summary>
public class Result
{
    internal Result(bool success, List<string> files = null, Error error = null)
    {
        Success = success;
        Files = files ?? new List<string>();
        Error = error;
    }

    /// <summary>
    /// Indicates whether the operation completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// List of full paths of the deleted files.
    /// </summary>
    /// <example>["/destination/Test1.txt", "/destination/Test2.txt"]</example>
    public List<string> Files { get; private set; }

    /// <summary>
    /// Error details. Null when Success is true.
    /// </summary>
    /// <example>null</example>
    public Error Error { get; private set; }
}
