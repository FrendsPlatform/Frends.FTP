using FluentFTP;

namespace Frends.FTP.DownloadFiles.Definitions;

internal class FileItem
{
    /// <summary>
    /// The last modified timestamp of the file, if available.
    /// If not available, set to the default value, i.e. DateTime.MinValue.
    /// </summary>
    /// <example>2024-01-15T10:30:00</example>
    public DateTime Modified { get; }

    /// <summary>
    /// The name of the file.
    /// </summary>
    /// <example>file1.txt</example>
    public string Name { get; set; }

    /// <summary>
    /// The full path of the file on the FTP server.
    /// </summary>
    /// <example>/files/file1.txt</example>
    public string FullPath { get; }

    public FileItem(FtpListItem ftpListItem)
    {
        Modified = ftpListItem.Modified;
        Name = ftpListItem.Name;
        FullPath = ftpListItem.FullName;
    }
}