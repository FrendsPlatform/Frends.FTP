namespace Frends.FTP.DeleteFiles.Definitions;

using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

/// <summary>
/// Input class usually contains parameters that are required.
/// </summary>
public class Input
{
    /// <summary>
    /// Full path of the target file to be deleted.
    /// </summary>
    /// <example>/destination</example>
    [DefaultValue("/")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Directory { get; set; }

    /// <summary>
    /// Pattern to match (Optional).
    /// If left empty, every file is deleted from given directory.
    /// </summary>
    /// <example>*.txt</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string FileMask { get; set; }

    /// <summary>
    /// The paths to the files to be deleted, mainly meant to be used with the file trigger with the syntax: #trigger.data.filePaths
    /// </summary>
    /// <example>#trigger.data.filePaths</example>
    [DefaultValue("")]
    public object FilePaths { get; set; }
}