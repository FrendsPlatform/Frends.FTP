using System;
using FluentFTP;

namespace Frends.FTP.DownloadFiles.Definitions;

internal class FileItem
{
    /// <summary>
    /// The last modified timestamp of the file, if available.
    /// If not available, set to the default value, i.e. <see cref="DateTime.MinValue"/>.
    /// </summary>
    public DateTime Modified { get; }

    public string Name { get; set; }

    public string FullPath { get; }

    public FileItem(FtpListItem ftpListItem)
    {
        Modified = ftpListItem.Modified;
        Name = ftpListItem.Name;
        FullPath = ftpListItem.FullName;
    }
}

