namespace Frends.FTP.UploadFiles.Enums
{
    /// <summary>
    /// Enumeration to specify actions if the source file is not found.
    /// </summary>
    public enum SourceNotFoundAction
    {
        /// <summary>
        /// Will log an error in case source files are missing.
        /// </summary>
        Error,
        
        /// <summary>
        /// Will log a notification in case source files are missing.
        /// </summary>
        Info,
        
        /// <summary>
        /// Will not log anything and continue execution without any additional actions.
        /// </summary>
        Ignore
    }
}
