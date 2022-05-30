using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.FTP.UploadFiles.Enums;

namespace Frends.FTP.UploadFiles.TaskConfiguration
{
    /// <summary>
    /// Destination transfer options
    /// </summary>
    public class Destination
    {
        /// <summary>
        /// Directory on the server.
        /// </summary>
        /// <example>/somedir</example>
        [DefaultValue("/")]
        [DisplayFormat(DataFormatString = "Text")]
        public string Directory { get; set; }

        /// <summary>
        /// File name of the destination file with possible macros.
        /// </summary>
        /// <example>myFile%Year%.txt</example>
        [DefaultValue("")]
        [DisplayFormat(DataFormatString = "Text")]
        public string FileName { get; set; }

        /// <summary>
        /// Operation to determine what to do if destination file exists.
        /// </summary>
        /// <example>DestinationAction.Error</example>
        [DefaultValue(DestinationAction.Error)]
        public DestinationAction Action { get; set; }
    }
}
