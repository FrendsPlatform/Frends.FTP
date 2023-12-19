using System.ComponentModel;
using System.ComponentModel.DataAnnotations;

namespace Frends.FTP.ListFiles.Definitions;

/// <summary>
/// Input parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Path to file(s).
    /// </summary>
    /// <example>/ , /SubDir</example>
    [DefaultValue("/")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Directory { get; set; }

    /// <summary>
    /// FileMask. 
    /// The file mask uses regular expressions and for convenience it handles * and ? like wildcards (regex .* and .+).
    /// Parameter can begin with &lt;regex&gt;. For example: &lt;regex&gt;^(?!prof).*_test.txt is same as ^(?!prof).*_test.txt).
    /// Use * or leave empty to list all files.
    /// </summary>
    /// <example>test.txt, test*.txt, test?.txt, test.(txt|xml), test.[^t][^x][^t], &lt;regex&gt;^(?!prof).*_test.txt</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string FileMask { get; set; }

    /// <summary>
    /// Types to include in the directory listing.
    /// </summary>
    /// <example>IncludeType.File</example>
    [DefaultValue(IncludeType.File)]
    public IncludeType IncludeType { get; set; } = IncludeType.File;

    /// <summary>
    /// Include subdirectories?
    /// </summary>
    /// <example>true</example>
    public bool IncludeSubdirectories { get; set; }
}


