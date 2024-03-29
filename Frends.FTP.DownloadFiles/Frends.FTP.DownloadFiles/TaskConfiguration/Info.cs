﻿using System.ComponentModel;

namespace Frends.FTP.DownloadFiles.TaskConfiguration;

/// <summary>
/// Info class including optional info of the file transfer.
/// </summary>
public class Info
{
    /// <summary>
    /// Optional descriptive name for the transfer. 
    /// Will be included in the file transfer log 
    /// entries and also in all error messages in the event log.
    /// </summary>
    /// <example>FTP Download</example>
    [DefaultValue("\"FTPDownload\"")]
    public string TransferName { get; set; }

    /// <summary>
    /// Directory where temporary files are stored during transfer.
    /// </summary>
    /// <example>c:\workDir</example>
    [DefaultValue("")]
    public string WorkDir { get; set; }

    /// <summary>
    /// The process URI, use #process.uri
    /// </summary>
    /// <example>#process.uri</example>
    [DefaultValue("#process.uri")]
    public string ProcessUri { get; set; }

    /// <summary>
    /// Reference to the Task execution id, use #process.executionid
    /// </summary>
    /// <example>#process.executionid</example>
    [DefaultValue("#process.executionid")]
    public string TaskExecutionID { get; set; }
}