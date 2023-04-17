namespace Frends.FTP.UploadFiles.Enums
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
        CheckIfDestinationFileExists,
        DestinationFileExists
    }
}