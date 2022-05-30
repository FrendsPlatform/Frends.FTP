using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.FTP.UploadFiles.Enums;

namespace Frends.FTP.UploadFiles.TaskConfiguration
{
    /// <summary>
    /// Source transfer options
    /// </summary>
    public class Source
    {
        /// <summary>
        /// Directory on the server.
        /// </summary>
        [DefaultValue("/")]
        [DisplayFormat(DataFormatString = "Text")]
        public string Directory { get; set; } = "/";

        /// <summary>
        /// File name or file mask of the files to be fetched.
        /// </summary>
        [DefaultValue("\"\"")]
        public string FileName { get; set; }

        /// <summary>
        /// What to do if source file is not found. Error = alarm and fail,
        /// Info = alarm info and quit with success status, Ignore = quit
        /// with success status.
        /// </summary>
        /// <example>SourceNotFoundAction.Error</example>
        [DefaultValue(SourceNotFoundAction.Error)]
        public SourceNotFoundAction NotFoundAction { get; set; }

        /// <summary>
        /// What to do with the source file after transfer.
        /// </summary>
        /// <example>SourceOperation.Delete</example>
        [DefaultValue(SourceOperation.Delete)]
        public SourceOperation Operation { get; set; }

        /// <summary>
        /// Parameter for Rename operation. Set the file name for the source file.
        /// </summary>
        /// <example>%SourceFileName%%SourceFileExtension%</example>
        [UIHint(nameof(Operation), "", SourceOperation.Rename)]
        public string FileNameAfterTransfer { get; set; }

        /// <summary>
        /// Parameter for Move operation. Set the full file path for source file.
        /// </summary>
        /// <example>c:\movedFiles</example>
        [UIHint(nameof(Operation), "", SourceOperation.Move)]
        public string DirectoryToMoveAfterTransfer { get; set; }

        /// <summary>
        /// The paths to the files to transfer, mainly meant to be used with the file trigger with the syntax: #trigger.data.filePaths
        /// </summary>
        /// <example>#trigger.data.filePaths</example>
        [DefaultValue("")]
        public object FilePaths { get; set; }
    }
}
