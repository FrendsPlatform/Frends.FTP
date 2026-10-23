using System;
using System.ComponentModel.DataAnnotations;
using FluentFTP;

namespace Frends.FTP.WriteFile.Definitions;

/// <summary>
/// Return object with private setters.
/// </summary>
public class Result
{
    internal Result(bool success, Error error = null, FtpListItem file = null)
    {
        Success = success;
        Error = error;
        if (file == null) return;
        Path = file.FullName;
        SizeInMegaBytes = Math.Round(file.Size / 1024d / 1024d, 3);
    }

    /// <summary>
    /// Indicates whether the operation completed successfully.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; private set; }

    /// <summary>
    /// Full path to the written file. Null when Success is false.
    /// </summary>
    /// <example>/destination/newfile.txt</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string Path { get; private set; }

    /// <summary>
    /// Size of the new file in destination. Zero when Success is false.
    /// </summary>
    /// <example>3.2</example>
    public double SizeInMegaBytes { get; private set; }

    /// <summary>
    /// Error details. Null when Success is true.
    /// </summary>
    /// <example>null</example>
    public Error Error { get; private set; }
}
