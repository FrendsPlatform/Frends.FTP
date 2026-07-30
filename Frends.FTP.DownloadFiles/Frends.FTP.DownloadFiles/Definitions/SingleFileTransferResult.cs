namespace Frends.FTP.DownloadFiles.Definitions;

internal class SingleFileTransferResult
{
    /// <summary>
    /// True if the single file transfer succeeded, otherwise false.
    /// </summary>
    /// <example>true</example>
    public bool Success { get; set; }

    /// <summary>
    /// True if the transfer action was skipped (e.g. no source files found), otherwise false.
    /// </summary>
    /// <example>false</example>
    public bool ActionSkipped { get; set; }

    /// <summary>
    /// List of error messages collected during the transfer attempt.
    /// </summary>
    /// <example>[ "Destination file already exists" ]</example>
    public IList<string> ErrorMessages { get; set; }

    /// <summary>
    /// The name of the transferred file.
    /// </summary>
    /// <example>file1.txt</example>
    public string TransferredFile { get; set; }

    /// <summary>
    /// The full path of the transferred file on the local file system.
    /// </summary>
    /// <example>C:\downloads\file1.txt</example>
    public string TransferredFilePath { get; set; }

    public SingleFileTransferResult()
    {
        ErrorMessages = new List<string>();
    }
}