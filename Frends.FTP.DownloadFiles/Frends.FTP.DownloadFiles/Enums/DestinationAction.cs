namespace Frends.FTP.DownloadFiles.Enums
{
    /// <summary>
    /// Enumeration to specify actions if destination file exists.
    /// </summary>
    public enum DestinationAction
    {
        /// <summary>
        /// Will overwrite existing file(s).
        /// </summary>
        Overwrite,
        
        /// <summary>
        /// Will throw an error if destination file(s) already exist.
        /// </summary>
        Error
    }
}
