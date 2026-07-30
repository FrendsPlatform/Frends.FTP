using Frends.FTP.DownloadFiles.Enums;

namespace Frends.FTP.DownloadFiles.Definitions;

internal class FileTransferInfo
{
    /// <summary>
    /// The name of the transfer.
    /// </summary>
    /// <example>MyTransfer</example>
    public string TransferName { get; set; }

    /// <summary>
    /// The unique identifier for the batch transfer.
    /// </summary>
    /// <example>550e8400-e29b-41d4-a716-446655440000</example>
    public Guid BatchId { get; set; }

    /// <summary>
    /// The source file path.
    /// </summary>
    /// <example>/files/file1.txt</example>
    public string SourceFile { get; set; }

    /// <summary>
    /// The UTC date and time when the file transfer started.
    /// </summary>
    /// <example>2024-01-15T10:30:00</example>
    public DateTime TransferStarted { get; set; }

    /// <summary>
    /// The UTC date and time when the file transfer ended.
    /// </summary>
    /// <example>2024-01-15T10:30:05</example>
    public DateTime TransferEnded { get; set; }

    /// <summary>
    /// The destination file path.
    /// </summary>
    /// <example>C:\downloads\file1.txt</example>
    public string DestinationFile { get; set; }

    /// <summary>
    /// The size of the transferred file in bytes.
    /// </summary>
    /// <example>1024</example>
    public long FileSize { get; set; }

    /// <summary>
    /// Error information if the transfer failed, otherwise empty.
    /// </summary>
    /// <example>Connection refused</example>
    public string ErrorInfo { get; set; }

    /// <summary>
    /// The result of the file transfer operation.
    /// </summary>
    /// <example>TransferResult.Success</example>
    public TransferResult Result { get; set; }

    /// <summary>
    /// Returns a string representation of the file transfer info.
    /// </summary>
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