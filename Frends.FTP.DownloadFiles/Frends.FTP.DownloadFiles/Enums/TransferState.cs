namespace Frends.FTP.DownloadFiles.Enums
{
    internal enum TransferState
    {
        RenameSourceFileBeforeTransfer,
        AppendToDestinationFile,
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
