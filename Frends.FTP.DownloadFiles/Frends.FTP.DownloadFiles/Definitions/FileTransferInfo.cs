using System;
using Frends.FTP.DownloadFiles.Enums;

namespace Frends.FTP.DownloadFiles.Definitions
{
    internal class FileTransferInfo
    {
        public string TransferName { get; set; } // 0

        public Guid BatchId { get; set; } // 1

        public string SourceFile { get; set; } // 2

        public DateTime TransferStarted { get; set; } // 3

        public DateTime TransferEnded { get; set; } // 4

        public string DestinationFile { get; set; } // 5

        public long FileSize { get; set; } // 6

        public string ErrorInfo { get; set; } // 7

        public TransferResult Result { get; set; } // 10

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
