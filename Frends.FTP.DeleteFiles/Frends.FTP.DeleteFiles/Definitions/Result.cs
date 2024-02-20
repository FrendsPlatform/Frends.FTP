namespace Frends.FTP.DeleteFiles.Definitions;

using System.Collections.Generic;

/// <summary>
/// Result class usually contains properties of the return object.
/// </summary>
public class Result
{
    internal Result(List<string> files)
    {
        Files = files;
    }

    /// <summary>
    /// Contains the input repeated the specified number of times.
    /// </summary>
    /// <example>Example of the output</example>
    public List<string> Files { get; private set; }
}
