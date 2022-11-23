using Frends.FTP.DownloadFiles.Enums;

namespace Frends.FTP.DownloadFiles.Definitions;

internal class FileTransferInfo
{
    public string TransferName { get; set; }
    public Guid BatchId { get; set; }
    public string SourceFile { get; set; }
    public DateTime TransferStarted { get; set; }
    public DateTime TransferEnded { get; set; }
    public string DestinationFile { get; set; }
    public long FileSize { get; set; }
    public string ErrorInfo { get; set; }
    public TransferResult Result { get; set; }

    public override string ToString()
    {
        return string.Format(
        $@"{ErrorInfo}

        TransferName: {TransferName}
        BatchId: {BatchId}
        SourceFile: {SourceFile}
        DestinationFile: {DestinationFile}
        TransferStarted: {TransferStarted}
        TransferEnded: {TransferEnded}
        TransferResult: {Result}
        FileSize: {FileSize} bytes
        ServiceId: {string.Empty}");
    }
}