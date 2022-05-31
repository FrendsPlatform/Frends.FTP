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
        AppendToDestinationFile,
        DeleteDestinationFile,
        RenameDestinationFile,
        CleanUpFiles,
        CheckIfDestinationFileExists
    }
}
