namespace Frends.FTP.DownloadFiles.Enums
{
    /// <summary>
    /// Enumeration to specify operation for the source file after transfer.
    /// </summary>
    public enum SourceOperation
    {
        /// <summary>
        /// Will delete files after downloading.
        /// </summary>
        Delete,
        /// <summary>
        /// Will rename the files after downloading.
        /// </summary>
        Rename,
        
        /// <summary>
        /// Will move the files after downloading.
        /// </summary>
        Move,
        
        /// <summary>
        /// Will do nothing to source files after downloading.
        /// </summary>
        Nothing
    }
}
