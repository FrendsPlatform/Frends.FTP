using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.FTP.ListFiles.Definitions;

/// <summary>
/// Additional parameters.
/// </summary>
public class Options
{
    /// <summary>
    /// Whether to throw an error on failure.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(true)]
    public bool ThrowErrorOnFailure { get; set; } = true;

    /// <summary>
    /// Overrides the error message on failure. If `ThrowErrorOnFailure` is set to `true`, then the original exception will be wrapped in a new Exception with this error message.
    /// </summary>
    /// <example>Listing files from the FTP server failed</example>
    [DisplayFormat(DataFormatString = "Text")]
    [DefaultValue("")]
    public string ErrorMessageOnFailure { get; set; } = string.Empty;
}
