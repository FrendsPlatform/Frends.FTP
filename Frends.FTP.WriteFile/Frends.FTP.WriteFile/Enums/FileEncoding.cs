namespace Frends.FTP.WriteFile.Enums;

// Pragma for self-explanatory enum attributes.
#pragma warning disable 1591
#pragma warning disable SA1602
/// <summary>
/// Enumeration of file encoding options.
/// </summary>
public enum FileEncoding
{
    UTF8,
    ANSI,
    ASCII,
    WINDOWS1252,

    /// <summary>
    /// Other enables users to add other encoding options as string.
    /// </summary>
    Other,
}