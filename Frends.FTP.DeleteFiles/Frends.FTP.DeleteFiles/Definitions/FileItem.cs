namespace Frends.FTP.DeleteFiles.Definitions;

using FluentFTP;
using System.IO;

/// <summary>
/// Single file data.
/// </summary>
public class FileItem
{
    internal FileItem(FtpListItem file)
    {
        Name = file.Name;
        FullPath = file.FullName;
    }

    internal FileItem(string file)
    {
        Name = Path.GetFileName(file);
        FullPath = file;
    }

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
}