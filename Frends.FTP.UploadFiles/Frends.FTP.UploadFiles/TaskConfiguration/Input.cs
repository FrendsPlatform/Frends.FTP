namespace Frends.FTP.UploadFiles.TaskConfiguration;

/// <summary>
/// Essential parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Source file location.
    /// </summary>
    /// <example>{ Directory = "C:\sourcefiles", FileName = "my_files*.txt" }</example>
    public Source Source { get; set; } = new Source();

    /// <summary>
    /// Destination directory location.
    /// </summary>
    /// <example>{ Directory = "/somedir", FileName = "myFile%Year%.txt" }</example>
    public Destination Destination { get; set; } = new Destination();

    /// <summary>
    /// Optional info of the file transfer.
    /// </summary>
    /// <example>{ TransferName = "FTP Upload" }</example>
    public Info Info { get; set; } = new Info();
}
