using System;

namespace Frends.FTP.ListFiles.Definitions;

/// <summary>
/// Single file data.
/// </summary>
public class ListObject
{
    /// <summary>
    /// Last modified timestamp of the file. 
    /// </summary>
    /// <example>2022-05-20T13:21:42Z</example>
    public DateTime LastModified { get; set; }

    /// <summary>
    /// Filename.
    /// </summary>
    /// <example>testfile.txt</example>
	    public string Name { get; set; }

    /// <summary>
    /// Full path of the file.
    /// </summary>
    /// <example>Top directory: /testfile.txt , subdirectory: /Subdirectory/testfile.txt</example>
    public string FullPath { get; set; }

    /// <summary>
    /// File size in bytes.
    /// </summary>
    /// <example>1048576</example>
	    public long SizeBytes { get; set; }
}




