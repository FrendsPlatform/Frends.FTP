namespace Frends.FTP.DeleteFiles.Definitions;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.FTP.DeleteFiles.Enums;

/// <summary>Parameters class usually contains parameters that are required.</summary>
public class Connection
{
    /// <summary>
    /// FTP(S) host address.
    /// </summary>
    /// <example>my.ftp.server.com</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string Address { get; set; }

    /// <summary>
    /// Port number to use in the connection to the server.
    /// </summary>
    /// <example>21</example>
    [DefaultValue(21)]
    public int Port { get; set; } = 21;

    /// <summary>
    /// Username to use for authentication to the server. Note that the file endpoint only supports
    /// username for remote shares and the username must be in the format DOMAIN\Username.
    /// </summary>
    /// <example>myUsername</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string UserName { get; set; }

    /// <summary>
    /// Password to use in the authentication to the server.
    /// </summary>
    /// <example>myPassword</example>
    [PasswordPropertyText]
    public string Password { get; set; }

    /// <summary>
    /// Connection mode to use to connect to the FTP server.
    /// </summary>
    /// <example>FtpMode.Passive</example>
    [DefaultValue(FtpMode.Passive)]
    public FtpMode Mode { get; set; }

    /// <summary>
    /// Sends NOOP command to keep connection alive at specified time-interval in seconds. If set to 0 the connection is not kept alive. Default value is 0
    /// </summary>
    /// <example>60</example>
    [DefaultValue(0)]
    public int KeepConnectionAliveInterval { get; set; }

    /// <summary>
    /// The length of time, in seconds, until the connection times out. You can use value 0 to indicate that the connection does not time out. Default value is 60 seconds
    /// </summary>
    /// <example>60</example>
    [DefaultValue(60)]
    public int ConnectionTimeout { get; set; } = 60;

    /// <summary>
    /// If set, this encoding will be used to encode and decode command parameters and server responses, such as file names. Example values: utf-8, utf-16, windows-1252
    /// </summary>
    /// <example>utf-8</example>
    [DefaultValue("")]
    public string Encoding { get; set; }

    /// <summary>
    /// Integer value of used buffer size as bytes.
    /// Default value is 4 KB.
    /// </summary>
    /// <example>4096</example>
    [DefaultValue(4096)]
    public int BufferSize { get; set; }

    #region FTPS settings

    /// <summary>
    /// Whether to use FTPS or not.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    public bool UseFTPS { get; set; } = false;

    /// <summary>
    /// Whether the data channel is secured or not.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    [UIHint(nameof(UseFTPS), "", true)]
    public bool SecureDataChannel { get; set; }

    /// <summary>
    /// Specifies whether to use Explicit or Implicit SSL
    /// </summary>
    /// <example>FtpsSslMode.None</example>
    [DefaultValue(FtpsSslMode.None)]
    [UIHint(nameof(UseFTPS), "", true)]
    public FtpsSslMode SslMode { get; set; }

    /// <summary>
    /// If enabled the client certificate is searched from user's certificate store
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    [UIHint(nameof(UseFTPS), "", true)]
    public bool EnableClientAuth { get; set; }

    /// <summary>
    /// Optional. Enables certification search by name from the certification store of current user.
    /// </summary>
    /// <example>mycert.crt</example>
    [DefaultValue("")]
    [UIHint(nameof(EnableClientAuth), "", true)]
    public string ClientCertificateName { get; set; }

    /// <summary>
    /// Optional. Enables certification search by thumbprint from the certification store of current user.
    /// </summary>
    /// <example>a909502dd82ae41433e6f83886b00d4277a32a7b</example>
    [DefaultValue("")]
    [UIHint(nameof(EnableClientAuth), "", true)]
    public string ClientCertificateThumbprint { get; set; }

    /// <summary>
    /// If enabled the any certificate will be considered valid.
    /// </summary>
    /// <example>false</example>
    [DefaultValue(false)]
    [UIHint(nameof(UseFTPS), "", true)]
    public bool ValidateAnyCertificate { get; set; }

    /// <summary>
    /// Path to client certificate (X509).
    /// </summary>
    /// <example>c:\example.cer</example>
    [DisplayFormat(DataFormatString = "Text")]
    [UIHint(nameof(UseFTPS), "", true)]
    public string ClientCertificatePath { get; set; }

    /// <summary>
    /// Certificate SHA1 hash string to validate against.
    /// </summary>
    /// <example>D911262984DE9CC32A3518A1094CD24249EA5C49</example>
    [DefaultValue("")]
    [UIHint(nameof(UseFTPS), "", true)]
    public string CertificateHashStringSHA1 { get; set; }

    #endregion
}
