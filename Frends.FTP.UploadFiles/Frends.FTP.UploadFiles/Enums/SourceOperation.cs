namespace Frends.FTP.UploadFiles.Enums
{
    /// <summary>
    /// Enumeration to specify operation for the source file after transfer.
    /// </summary>
    public enum SourceOperation
    {
        /// <summary>
        /// Will delete files after uploading.
        /// </summary>
        Delete,
        /// <summary>
        /// Will rename the files after uploading.
        /// </summary>
        Rename,
        
        /// <summary>
        /// Will move the files after uploading.
        /// </summary>
        Move,
        
        /// <summary>
        /// Will do nothing to source files after uploading.
        /// </summary>
        Nothing
    }
}
