namespace Frends.FTP.DownloadFiles.Enums;

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
