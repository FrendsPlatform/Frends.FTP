using Frends.FTP.DownloadFiles.TaskConfiguration;

namespace Frends.FTP.DownloadFiles.Definitions;

internal class BatchContext
{
    public Info Info { get; set; }

    public Options Options { get; set; }

    public Guid InstanceId { get; set; }

    public DateTime BatchTransferStartTime { get; set; }

    public Source Source { get; set; }

    public Destination Destination { get; set; }

    public Connection Connection  { get; set; }
}