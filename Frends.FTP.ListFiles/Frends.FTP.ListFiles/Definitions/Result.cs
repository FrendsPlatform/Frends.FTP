using System.Collections.Generic;

namespace Frends.FTP.ListFiles.Definitions;

/// <summary>
/// List of files.
/// </summary>
public class Result
{
    /// <summary>
    /// List of files.
    /// </summary>
    /// <example>[ { "test.txt", "/ListFiles/test.txt", "2022-05-20T13:21:42Z", 1048576 }, { "test2.txt", "/ListFiles/test2.txt", "2022-05-20T13:21:42Z", 1048576 } ]</example>
    public List<FileItem> Files { get; private set; }

    internal Result(List<FileItem> files)
    {
        Files = files;
    }
}

