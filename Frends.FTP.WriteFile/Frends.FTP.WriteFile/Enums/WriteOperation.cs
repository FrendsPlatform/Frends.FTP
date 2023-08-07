namespace Frends.FTP.WriteFile.Enums;

// Pragma for self-explanatory enum attributes.
#pragma warning disable 1591

/// <summary>
/// Enumeration for operation if destination file exists.
/// </summary>
public enum WriteOperation
{
    Append,
    Overwrite,
    Error
}

