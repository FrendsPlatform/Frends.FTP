using Frends.FTP.DownloadFiles.TaskConfiguration;

namespace Frends.FTP.DownloadFiles.Definitions;

internal class BatchContext
{
    /// <summary>
    /// Transfer info containing name, working directory, and process context.
    /// </summary>
    /// <example>new Info { TransferName = "MyTransfer" }</example>
    public Info Info { get; set; }

    /// <summary>
    /// Transfer options controlling behaviour such as error handling and logging.
    /// </summary>
    /// <example>new Options { ThrowErrorOnFailure = true }</example>
    public Options Options { get; set; }

    /// <summary>
    /// Unique identifier for this batch transfer instance.
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    public Guid InstanceId { get; set; }

    /// <summary>
    /// The UTC date and time when the batch transfer started.
    /// </summary>
    /// <example>2024-01-15T10:30:00</example>
    public DateTime BatchTransferStartTime { get; set; }

    /// <summary>
    /// Source file location and transfer options.
    /// </summary>
    /// <example>new Source { Directory = "/files", FileName = "*.txt" }</example>
    public Source Source { get; set; }

    /// <summary>
    /// Destination directory location and overwrite options.
    /// </summary>
    /// <example>new Destination { Directory = "C:\\downloads" }</example>
    public Destination Destination { get; set; }

    /// <summary>
    /// FTP connection parameters.
    /// </summary>
    /// <example>new Connection { Address = "ftp.example.com", Port = 21 }</example>
    public Connection Connection { get; set; }
}