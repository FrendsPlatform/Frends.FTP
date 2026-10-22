using Frends.FTP.UploadFiles.Enums;
using System;

namespace Frends.FTP.UploadFiles.Definitions
{
    internal class FileTransferInfo
    {
        /// <summary>
        /// Gets or sets the transfer name.
        /// </summary>
        /// <example>file.txt</example>
        public string TransferName { get; set; } // 0
        /// <summary>
        /// Gets or sets the batch id.
        /// </summary>
        /// <example>00000000-0000-0000-0000-000000000000</example>
        public Guid BatchId { get; set; } // 1
        /// <summary>
        /// Gets or sets the source file.
        /// </summary>
        /// <example>file.txt</example>
        public string SourceFile { get; set; } // 2
        /// <summary>
        /// Gets or sets the transfer started.
        /// </summary>
        /// <example>2024-01-01T12:00:00</example>
        public DateTime TransferStarted { get; set; } // 3
        /// <summary>
        /// Gets or sets the transfer ended.
        /// </summary>
        /// <example>2024-01-01T12:00:00</example>
        public DateTime TransferEnded { get; set; } // 4
        /// <summary>
        /// Gets or sets the destination file.
        /// </summary>
        /// <example>file.txt</example>
        public string DestinationFile { get; set; } // 5
        /// <summary>
        /// Gets or sets the file size.
        /// </summary>
        /// <example>1</example>
        public long FileSize { get; set; } // 6
        /// <summary>
        /// Gets or sets the error info.
        /// </summary>
        /// <example>file.txt</example>
        public string ErrorInfo { get; set; } // 7
        /// <summary>
        /// Gets or sets the result.
        /// </summary>
        /// <example>Result</example>
        public TransferResult Result { get; set; } // 10

        /// <summary>
        /// Returns a textual representation of the file transfer.
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
}