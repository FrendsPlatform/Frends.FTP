using System;
using Frends.FTP.UploadFiles.TaskConfiguration;

namespace Frends.FTP.UploadFiles.Definitions
{
    internal class BatchContext
    {
        /// <summary>
        /// Optional info of the file transfer.
        /// </summary>
        /// <example>FTP Upload</example>
        public Info Info { get; set; }

        /// <summary>
        /// Options of the file transfer.
        /// </summary>
        /// <example>ThrowErrorOnFailure = true</example>
        public Options Options { get; set; }

        /// <summary>
        /// Unique identifier of the task execution.
        /// </summary>
        /// <example>3f2504e0-4f89-11d3-9a0c-0305e82c3301</example>
        public Guid InstanceId { get; set; }

        /// <summary>
        /// Time when the batch transfer started.
        /// </summary>
        /// <example>2024-12-11 10:30:00</example>
        public DateTime BatchTransferStartTime { get; set; }

        /// <summary>
        /// Source file location.
        /// </summary>
        /// <example>C:\sourcefiles</example>
        public Source Source { get; set; }

        /// <summary>
        /// Destination directory location.
        /// </summary>
        /// <example>/somedir</example>
        public Destination Destination { get; set; }

        /// <summary>
        /// Connection parameters of the FTP server.
        /// </summary>
        /// <example>ftp.example.com</example>
        public Connection Connection { get; set; }
    }
}
