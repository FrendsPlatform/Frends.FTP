using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.FTP.DownloadFiles.Enums;

namespace Frends.FTP.DownloadFiles.TaskConfiguration;

/// <summary>
/// Source transfer options
/// </summary>
public class Source
{
    /// <summary>
    /// Directory on the server.
    /// </summary>
    /// <example>/directory</example>
    [DefaultValue("/")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Directory { get; set; } = "/";

    /// <summary>
    /// File name or file mask of the files to be fetched.
    /// </summary>
    /// <example>example*.csv</example>
    [DefaultValue("\"\"")]
    public string FileName { get; set; }

    /// <summary>
    /// What to do if source file is not found. Error = alarm and fail,
    /// Info = alarm info and quit with success status, Ignore = quit
    /// with success status.
    /// </summary>
    /// <example>SourceNotFoundAction.Error</example>
    [DefaultValue(SourceNotFoundAction.Error)]
    public SourceNotFoundAction NotFoundAction { get; set; }

    /// <summary>
    /// What to do with the source file after transfer.
    /// </summary>
    /// <example>SourceOperation.Delete</example>
    [DefaultValue(SourceOperation.Delete)]
    public SourceOperation Operation { get; set; }

    /// <summary>
    /// Parameter for Rename operation. Set the file name for the source file.
    /// </summary>
    /// <example>%SourceFileName%%SourceFileExtension%</example>
    [UIHint(nameof(Operation), "", SourceOperation.Rename)]
    public string FileNameAfterTransfer { get; set; }

    /// <summary>
    /// Parameter for Move operation. Sets the directory to which source files will be moved after transfer.
    /// </summary>
    /// <example>/movedFiles</example>
    [UIHint(nameof(Operation), "", SourceOperation.Move)]
    public string DirectoryToMoveAfterTransfer { get; set; }
}

