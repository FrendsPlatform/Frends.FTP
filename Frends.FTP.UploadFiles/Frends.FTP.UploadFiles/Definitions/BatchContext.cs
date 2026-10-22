using Frends.FTP.UploadFiles.TaskConfiguration;
using System;

namespace Frends.FTP.UploadFiles.Definitions
{
    internal class BatchContext
    {
        /// <summary>
        /// Gets or sets the info.
        /// </summary>
        /// <example>Info</example>
        public Info Info { get; set; }
        /// <summary>
        /// Gets or sets the options.
        /// </summary>
        /// <example>Options</example>
        public Options Options { get; set; }
        /// <summary>
        /// Gets or sets the instance id.
        /// </summary>
        /// <example>00000000-0000-0000-0000-000000000000</example>
        public Guid InstanceId { get; set; }
        /// <summary>
        /// Gets or sets the batch transfer start time.
        /// </summary>
        /// <example>2024-01-01T12:00:00</example>
        public DateTime BatchTransferStartTime { get; set; }
        /// <summary>
        /// Gets or sets the source.
        /// </summary>
        /// <example>Source</example>
        public Source Source { get; set; }
        /// <summary>
        /// Gets or sets the destination.
        /// </summary>
        /// <example>Destination</example>
        public Destination Destination { get; set; }
        /// <summary>
        /// Gets or sets the connection.
        /// </summary>
        /// <example>Connection</example>
        public Connection Connection { get; set; }
    }
}