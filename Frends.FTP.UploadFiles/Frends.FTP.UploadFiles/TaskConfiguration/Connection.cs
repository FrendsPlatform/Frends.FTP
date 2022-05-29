using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.FTP.UploadFiles.TaskConfiguration
{
    /// <summary>FTP transport types.</summary>
    public enum FtpTransportType
    {
        /// <summary>Binary transport.</summary>
        Binary,
        
        /// <summary>ASCII transport.</summary>
        Ascii,
    }

    /// <summary>FTP connection modes.</summary>
    public enum FtpMode
    {
        /// <summary>Passive mode</summary>
        Passive,
        /// <summary>Active mode.</summary>
        Active,
    }

    /// <summary>FTPS encryption modes.</summary>
    public enum FtpsSslMode
    {
        /// <summary>No encryption (plain text).</summary>
        None,
        
        /// <summary>Use explicit encryption.</summary>
        Explicit,
        
        /// <summary>Use implicit encryption.</summary>
        Implicit,
        
        /// <summary>Tries to use FTPS encryption and falls back to plain text FTP.</summary>
        Auto,
    }

    /// <summary>Parameters class usually contains parameters that are required.</summary>
    public class Connection
    {
        /// <summary>
        /// FTP(S) host address
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string Address { get; set; }

        /// <summary>
        /// Port number to use in the connection to the server.
        /// </summary>
        [DefaultValue(21)]
        public int Port { get; set; } = 21;

        /// <summary>
        /// Username to use for authentication to the server. Note that the file endpoint only supports
        /// username for remote shares and the username must be in the format DOMAIN\Username.
        /// </summary>
        [DisplayFormat(DataFormatString = "Text")]
        public string UserName { get; set; }

        /// <summary>
        /// Password to use in the authentication to the server.
        /// </summary>
        [PasswordPropertyText]
        public string Password { get; set; }

        /// <summary>
        /// Type of transfer: 'Ascii' sends files as text and must only be used for sending ASCII text files. 'Binary' (default) sends files as raw data and should be used for sending e.g. UTF-encoded text files
        /// </summary>
        [DefaultValue(FtpTransportType.Binary)]
        public FtpTransportType TransportType { get; set; }

        /// <summary>
        /// Connection mode to use to connect to the FTP server
        /// </summary>
        public FtpMode Mode { get; set; }

        /// <summary>
        /// Sends NOOP command to keep connection alive at specified time-interval in seconds. If set to 0 the connection is not kept alive. Default value is 0
        /// </summary>
        [DefaultValue(0)]
        public int KeepConnectionAliveInterval { get; set; }

        /// <summary>
        /// The length of time, in seconds, until the connection times out. You can use value 0 to indicate that the connection does not time out. Default value is 60 seconds
        /// </summary>
        [DefaultValue(60)]
        public int ConnectionTimeout { get; set; } = 60;

        /// <summary>
        /// If set, this encoding will be used to encode and decode command parameters and server responses, such as file names. Example values: utf-8, utf-16, windows-1252
        /// </summary>
        [DefaultValue("")]
        public string Encoding { get; set; }

        /// <summary>
        /// Integer value of used buffer size as KB.
        /// Default value is 32 KB.
        /// </summary>
        [DefaultValue(32)]
        public int BufferSize { get; set; }

        #region FTPS settings

        [DefaultValue("false")]
        public bool UseFTPS { get; set; } = false;
        
        /// <summary>
        /// Whether the data channel is secured or not
        /// </summary>
        [DefaultValue("true")]
        [UIHint(nameof(UseFTPS),"", true)]
        public bool SecureDataChannel { get; set; }

        /// <summary>
        /// Specifies whether to use Explicit or Implicit SSL
        /// </summary>
        [UIHint(nameof(UseFTPS),"", true)]
        public FtpsSslMode SslMode { get; set; }

        /// <summary>
        /// If enabled the client certificate is searched from user's certificate store
        /// </summary>
        [DefaultValue("false")]
        [UIHint(nameof(UseFTPS),"", true)]
        public bool EnableClientAuth { get; set; }
        
        /// <summary>
        /// If enabled the any certificate will be considered valid.
        /// </summary>
        [DefaultValue("false")]
        [UIHint(nameof(UseFTPS),"", true)]
        public bool ValidateAnyCertificate { get; set; }
        
        /// <summary>
        /// Path to client certificate (X509).
        /// </summary>
        [DefaultValue("c:\\example.cer")]
        [UIHint(nameof(UseFTPS),"", true)]
        public string ClientCertificatePath { get; set; }

        #endregion
    }
}
