namespace Frends.FTP.ReadFile.Enums;

/// <summary>
/// Enumeration of file encoding options.
/// </summary>
public enum FileEncoding
{
    /// <summary>
    /// Represents UTF-8 text encoding.
    /// </summary>
    UTF8,

    /// <summary>
    /// Represents ANSI encoding.
    /// </summary>
    ANSI,

    /// <summary>
    /// Represents ASCII encoding.
    /// </summary>
    ASCII,

    /// <summary>
    /// Represents UTF-16 encoding.
    /// </summary>
    UTF16,

    /// <summary>
    /// Represents Windows-1252 encoding.
    /// </summary>
    WINDOWS1252,

    /// <summary>
    /// Other enables users to add other encoding options as string.
    /// </summary>
    Other,
}