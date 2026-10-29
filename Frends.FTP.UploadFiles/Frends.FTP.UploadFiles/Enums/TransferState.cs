namespace Frends.FTP.UploadFiles.Enums
{
    internal enum TransferState
    {
        /// <summary>
        /// State: rename source file before transfer.
        /// </summary>
        RenameSourceFileBeforeTransfer,

        /// <summary>
        /// State: put file.
        /// </summary>
        PutFile,

        /// <summary>
        /// State: source operation move.
        /// </summary>
        SourceOperationMove,

        /// <summary>
        /// State: source operation rename.
        /// </summary>
        SourceOperationRename,

        /// <summary>
        /// State: source operation delete.
        /// </summary>
        SourceOperationDelete,

        /// <summary>
        /// State: restore source file.
        /// </summary>
        RestoreSourceFile,

        /// <summary>
        /// State: remove temporary destination file.
        /// </summary>
        RemoveTemporaryDestinationFile,

        /// <summary>
        /// State: append to destination file.
        /// </summary>
        AppendToDestinationFile,

        /// <summary>
        /// State: delete destination file.
        /// </summary>
        DeleteDestinationFile,

        /// <summary>
        /// State: rename destination file.
        /// </summary>
        RenameDestinationFile,

        /// <summary>
        /// State: clean up files.
        /// </summary>
        CleanUpFiles,

        /// <summary>
        /// State: check if destination file exists.
        /// </summary>
        CheckIfDestinationFileExists,

        /// <summary>
        /// State: destination file exists.
        /// </summary>
        DestinationFileExists,

        /// <summary>
        /// State: checksum.
        /// </summary>
        Checksum,

        /// <summary>
        /// State: restore modified.
        /// </summary>
        RestoreModified,
    }
}
