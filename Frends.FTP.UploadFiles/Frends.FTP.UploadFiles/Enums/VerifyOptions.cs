namespace Frends.FTP.UploadFiles.Enums
{
    /// <summary>
    /// Options for verifying the file after transfer.
    /// </summary>
    public enum VerifyOptions
    {
        /// <summary>
        /// No verification of the file is performed
        /// </summary>
        None,
        /// <summary>
        /// The checksum of the file is verified, if supported by the server. If the checksum
        /// comparison fails then the Task will retry the download/upload a specified amount of times
        /// before giving up. If the checksum is invalid on the last attempt the Task will delete the file and throw an exception.
        /// (See Frends.FTP.UploadFiles.Connection.RetryAttempts)
        /// </summary>
        Retry,
        /// <summary>
        /// The checksum of the file is verified, if supported by the server. If the checksum
        /// comparison fails then an exception will be thrown.
        /// </summary>
        Throw
    }
}