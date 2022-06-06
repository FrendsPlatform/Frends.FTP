using System.Collections.Generic;

namespace Frends.FTP.DownloadFiles.Definitions
{
    internal class SingleFileTransferResult
    {
        public bool Success { get; set; }

        public bool ActionSkipped { get; set; }

        public IList<string> ErrorMessages { get; set; }

        public string TransferredFile { get; set; }

        public string TransferredFilePath { get; set; }

        public SingleFileTransferResult()
        {
            ErrorMessages = new List<string>();
        }
    }
}
