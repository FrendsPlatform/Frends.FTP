namespace Frends.FTP.DownloadFiles.Enums
{
    internal enum TransferState
    {
        RenameSourceFileBeforeTransfer,
        PutFile,
        SourceOperationMove,
        SourceOperationRename,
        SourceOperationDelete,
        RestoreSourceFile,
        RemoveTemporaryDestinationFile,
        DeleteDestinationFile,
        RenameDestinationFile,
        CleanUpFiles,
        CheckIfDestinationFileExists
    }
}
