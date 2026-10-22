using System.ComponentModel;

namespace Frends.FTP.UploadFiles.TaskConfiguration
{
    /// <summary>
    /// Input parameters of the transfer.
    /// </summary>
    public class Input
    {
        /// <summary>
        /// Source file location.
        /// </summary>
        public Source Source { get; set; } = new Source();

        /// <summary>
        /// Destination directory location.
        /// </summary>
        public Destination Destination { get; set; } = new Destination();

        /// <summary>
        /// Optional info of the file transfer.
        /// </summary>
        public Info Info { get; set; } = new Info();
    }
}
