using System.ComponentModel;

namespace Frends.FTP.DownloadFiles.TaskConfiguration;

/// <summary>
/// Input parameters for the FTP download task, combining source, destination, and transfer info.
/// </summary>
public class Input
{
    /// <summary>
    /// Source file location and transfer options.
    /// </summary>
    /// <example>new Source { Directory = "/files", FileName = "*.txt" }</example>
    [DefaultValue(null)]
    public Source Source { get; set; }

    /// <summary>
    /// Destination directory location and overwrite options.
    /// </summary>
    /// <example>new Destination { Directory = "C:\\downloads", Action = DestinationAction.Overwrite }</example>
    [DefaultValue(null)]
    public Destination Destination { get; set; }

    /// <summary>
    /// Optional transfer info such as transfer name, working directory, and process context.
    /// </summary>
    /// <example>new Info { TransferName = "MyTransfer" }</example>
    [DefaultValue(null)]
    public Info Info { get; set; }
}
