using System;
using Frends.FTP.UploadFiles.Enums;

namespace Frends.FTP.UploadFiles.Definitions
{
    internal class FileTransferInfo
    {
        /// <summary>
        /// Descriptive name of the transfer.
        /// </summary>
        /// <example>FTP Upload</example>
        public string TransferName { get; set; }

        /// <summary>
        /// Identifier of the transfer batch.
        /// </summary>
        /// <example>3f2504e0-4f89-11d3-9a0c-0305e82c3301</example>
        public Guid BatchId { get; set; }

        /// <summary>
        /// Path of the source file.
        /// </summary>
        /// <example>C:\sourcefiles\file1.txt</example>
        public string SourceFile { get; set; }

        /// <summary>
        /// Time when the file transfer started.
        /// </summary>
        /// <example>2024-12-11 10:30:00</example>
        public DateTime TransferStarted { get; set; }

        /// <summary>
        /// Time when the file transfer ended.
        /// </summary>
        /// <example>2024-12-11 10:30:05</example>
        public DateTime TransferEnded { get; set; }

        /// <summary>
        /// Path of the destination file.
        /// </summary>
        /// <example>/somedir/file1.txt</example>
        public string DestinationFile { get; set; }

        /// <summary>
        /// Size of the file in bytes.
        /// </summary>
        /// <example>1024</example>
        public long FileSize { get; set; }

        /// <summary>
        /// Information about the error that occurred during the transfer.
        /// </summary>
        /// <example>Connection timed out</example>
        public string ErrorInfo { get; set; }

        /// <summary>
        /// Result of the file transfer.
        /// </summary>
        /// <example>TransferResult.Success</example>
        public TransferResult Result { get; set; }

        /// <summary>
        /// Returns a string describing the file transfer.
        /// </summary>
        /// <returns>Description of the file transfer.</returns>
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
}
