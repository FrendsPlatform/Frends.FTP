using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using Frends.FTP.WriteFile.Enums;

namespace Frends.FTP.WriteFile.Definitions;

/// <summary>
/// Input parameters.
/// </summary>
public class Input
{
    /// <summary>
    /// Full path for the file to be written.
    /// </summary>
    /// <example>/ , /SubDir/test.txt</example>
    [DefaultValue("")]
    [DisplayFormat(DataFormatString = "Text")]
    public string Path { get; set; }

    /// <summary>
    /// Text content to be written.
    /// </summary>
    /// <example>This is test file</example>
    [DisplayFormat(DataFormatString = "Text")]
    public string Content { get; set; }

    /// <summary>
    /// Enables Task to create destination directories if they don't exist.
    /// </summary>
    [DefaultValue(false)]
    public bool CreateDestinationDirectories { get; set; }

    /// <summary>
    /// If set, this ecoding will be used to encode and decode command 
    /// parameters and server responses, such as file names. 
    /// By selecting 'Other' you can use any encoding.
    /// </summary>
    /// <example>FileEncoding.ANSI</example>
    [DefaultValue(FileEncoding.ANSI)]
    public FileEncoding FileEncoding { get; set; }

    /// <summary>
    /// Additional option for UTF-8 encoding to enable bom.
    /// </summary>
    /// <example>true</example>
    [UIHint(nameof(FileEncoding), "", FileEncoding.UTF8)]
    [DefaultValue(false)]
    public bool EnableBom { get; set; }

    /// <summary>
    /// File encoding to be used.
    /// Encoding don't support any unicode encoding. It only support the code page encodings. 
    /// A partial list of possible encodings: https://learn.microsoft.com/en-us/dotnet/api/system.text.encoding.getencodings?view=net-6.0#examples.
    /// </summary>
    /// <example>iso-8859-1</example>
    [UIHint(nameof(FileEncoding), "", FileEncoding.Other)]
    public string EncodingInString { get; set; }

    /// <summary>
    /// How the file write should work if a file with the new name already exists.
    /// </summary>
    /// <example>WriteOperation.Append</example>
    public WriteOperation WriteBehaviour { get; set; }

    /// <summary>
    /// If enabled new line is added to the existing file before appending the content.
    /// </summary>
    /// <example>true</example>
    [DefaultValue(false)]
    [UIHint(nameof(WriteBehaviour), "", WriteOperation.Append)]
    public bool AddNewLine { get; set; }
}


