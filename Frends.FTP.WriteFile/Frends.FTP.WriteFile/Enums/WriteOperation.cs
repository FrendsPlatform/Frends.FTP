namespace Frends.FTP.WriteFile.Enums;

// Pragma for self-explanatory enum attributes.
#pragma warning disable 1591
#pragma warning disable SA1602

/// <summary>
/// Enumeration for operation if destination file exists.
/// </summary>
public enum WriteOperation
{
    Append,
    Overwrite,
    Error,
}