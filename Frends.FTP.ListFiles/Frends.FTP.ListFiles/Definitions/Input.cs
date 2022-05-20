using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.FTP.ListFiles.Definitions
{
    /// <summary>
    /// Input parameters.
    /// </summary>
    public class Input
    {
        /// <summary>
        /// FTP/FTPS host.
        /// </summary>
        /// <example>ftp-source.com</example>
        [DisplayFormat(DataFormatString = "Text")]
        public string Host { get; set; }

        /// <summary>
        /// User.
        /// </summary>
        /// <example>FtpUser</example>
        [DisplayFormat(DataFormatString = "Text")]
        public string User { get; set; }

        /// <summary>
        /// Password.
        /// </summary>
        /// <example>r2d2</example>
        [PasswordPropertyText]
        [DisplayFormat(DataFormatString = "Text")]
        public string Password { get; set; }

        /// <summary>
        /// Port.
        /// </summary>
        /// <example>21</example>
        public int Port { get; set; }

        /// <summary>
        /// Path to file(s).
        /// </summary>
        /// <example>/ , /SubDir</example>
        [DefaultValue("/")]
        [DisplayFormat(DataFormatString = "Text")]
        public string Path { get; set; }

        /// <summary>
        /// Filename. The file mask uses regular expressions, but for convenience, it has special handling for * and ? wildcards. Use * or leave empty to list all files.
        /// </summary>
        /// <example>test.txt, test*.txt, test?.txt, test.(txt|xml), test.[^t][^x][^t],  &lt;regex&gt;^(?!prof).*_test.txt</example>
        [DisplayFormat(DataFormatString = "Text")]
        public string Filename { get; set; }
    }
}


