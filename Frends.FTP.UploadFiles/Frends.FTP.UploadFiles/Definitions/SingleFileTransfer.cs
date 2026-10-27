using System;
using System.IO;
using FluentFTP;
using Frends.FTP.UploadFiles.Enums;
using Frends.FTP.UploadFiles.Logging;

namespace Frends.FTP.UploadFiles.Definitions
{
    /// <summary>
    /// Class for executing single file transfer.
    /// </summary>
    internal class SingleFileTransfer
    {
        private readonly RenamingPolicy renamingPolicy;
        private readonly IFtpLogger logger;
        private readonly SingleFileTransferResult result;
        private readonly FtpClient client;
        private readonly BatchContext batchContext;
        private readonly string destinationDirectoryWithMacrosExpanded;
        private TransferState state;
        private string sourceFileDuringTransfer;
        private string destinationFileDuringTransfer;

        public SingleFileTransfer(FileItem file, BatchContext context, FtpClient client, RenamingPolicy renamingPolicy, IFtpLogger logger)
        {
            this.renamingPolicy = renamingPolicy;
            this.logger = logger;
            this.client = client;
            SourceFile = file;
            batchContext = context;

            destinationDirectoryWithMacrosExpanded =
                renamingPolicy.ExpandDirectoryForMacros(context.Destination.Directory);
            DestinationFileNameWithMacrosExpanded = renamingPolicy.CreateRemoteFileName(
                    file.Name,
                    context.Destination.FileName);

            result = new SingleFileTransferResult { Success = true };
        }

        /// <summary>
        /// Source file to transfer.
        /// </summary>
        /// <example>C:\sourcefiles\file1.txt</example>
        public FileItem SourceFile { get; set; }

        /// <summary>
        /// Destination file name with macros expanded.
        /// </summary>
        /// <example>file1_2024.txt</example>
        public string DestinationFileNameWithMacrosExpanded { get; set; }

        /// <summary>
        /// Transfers a single file.
        /// </summary>
        /// <returns>The result of the file transfer.</returns>
        public SingleFileTransferResult TransferSingleFile()
        {
            try
            {
                result.TransferredFile = SourceFile.Name;
                result.TransferredFilePath = SourceFile.FullPath;
                if (batchContext.Options.RenameSourceFileBeforeTransfer)
                    RenameSourceFile();
                else
                    sourceFileDuringTransfer = SourceFile.FullPath;

                if (DestinationFileExists(DestinationFileNameWithMacrosExpanded))
                {
                    switch (batchContext.Destination.Action)
                    {
                        case DestinationAction.Append:
                            AppendDestinationFile();
                            break;
                        case DestinationAction.Overwrite:
                            PutDestinationFile(batchContext.Connection.VerifyOption, removeExisting: true);
                            break;
                        case DestinationAction.Error:
                            Trace(TransferState.CheckIfDestinationFileExists, "Checking if destination file {0} exists", SourceFile.Name);
                            throw new DestinationFileExistsException(DestinationFileNameWithMacrosExpanded);
                    }
                }
                else
                {
                    PutDestinationFile(batchContext.Connection.VerifyOption);
                }

                if (batchContext.Options.PreserveLastModified)
                    RestoreModified();

                ExecuteSourceOperation();
                logger.LogTransferSuccess(this, batchContext);
            }
            catch (Exception ex)
            {
                var sourceFileRestoreMessage = RestoreSourceFileAfterErrorIfItWasRenamed();
                HandleTransferError(ex, sourceFileRestoreMessage);
            }
            finally
            {
                CleanUpFiles();
            }

            return result;
        }

        private static bool FileDefinedAndExists(string filename)
        {
            return !string.IsNullOrWhiteSpace(filename) && File.Exists(filename);
        }

        /// <summary>
        /// Checks if destination file exists.
        /// </summary>
        /// <param name="filename">The destination file name.</param>
        /// <returns>True if the destination file exists, otherwise false.</returns>
        private bool DestinationFileExists(string filename)
        {
            Trace(
                TransferState.CheckIfDestinationFileExists,
                "Checking if destination file {0} exists",
                SourceFile.Name);
            return client.FileExists(filename);
        }

        /// <summary>
        /// Rename file with unique file name.
        /// </summary>
        private void RenameSourceFile()
        {
            var uniqueFileName = Util.CreateUniqueFileName();
            var workdir = !string.IsNullOrEmpty(batchContext.Info.WorkDir)
                ? batchContext.Info.WorkDir
                : Path.GetDirectoryName(SourceFile.FullPath);
            var renamedFile = Path.Combine(workdir, uniqueFileName);

            Trace(TransferState.RenameSourceFileBeforeTransfer, "Renaming source file {0} to temporary file name {1} before transfer", SourceFile.Name, uniqueFileName);
            File.Move(SourceFile.FullPath, renamedFile);

            sourceFileDuringTransfer = renamedFile;
        }

        /// <summary>
        /// Appends source file content to existing destination file.
        /// </summary>
        private void AppendDestinationFile()
        {
            var doRename = batchContext.Options.RenameDestinationFileDuringTransfer;

            Trace(
                TransferState.AppendToDestinationFile,
                "Appending file {0} to existing file {1}",
                SourceFile.Name,
                DestinationFileNameWithMacrosExpanded);

            // No renaming needed - just do the native call with client
            if (!doRename)
            {
                client.UploadFile(sourceFileDuringTransfer, DestinationFileNameWithMacrosExpanded, FtpRemoteExists.AddToEnd);
            }
            else
            {
                var destinationFileDuringTransfer = Util.CreateUniqueFileName();

                // Move file to temp location if file already exists
                if (client.FileExists(DestinationFileNameWithMacrosExpanded))
                    client.MoveFile(DestinationFileNameWithMacrosExpanded, destinationFileDuringTransfer);

                // Do the upload to temp location
                client.UploadFile(sourceFileDuringTransfer, destinationFileDuringTransfer, FtpRemoteExists.AddToEnd);

                // Move file back to original location
                client.MoveFile(destinationFileDuringTransfer, DestinationFileNameWithMacrosExpanded);
            }
        }

        private void PutDestinationFile(VerifyOptions verifyOptions, bool removeExisting = false)
        {
            var doRename = batchContext.Options.RenameDestinationFileDuringTransfer;

            destinationFileDuringTransfer = doRename ? Util.CreateUniqueFileName() : DestinationFileNameWithMacrosExpanded;

            Trace(
                TransferState.PutFile,
                "Uploading {0}destination file {1}",
                doRename ? "temporary " : string.Empty,
                destinationFileDuringTransfer);

            if (!Enum.TryParse<FtpVerify>(verifyOptions.ToString(), out var verifyOption))
            {
                // Handle the parsing failure, perhaps set a default value or log an error
                verifyOption = FtpVerify.None; // or choose an appropriate default
            }

            client.UploadFile(
                sourceFileDuringTransfer,
                destinationFileDuringTransfer,
                existsMode: FtpRemoteExists.Overwrite,
                verifyOptions: verifyOption);

            if ((verifyOptions == VerifyOptions.Retry || verifyOptions == VerifyOptions.Throw) && client.CompareFile(sourceFileDuringTransfer, destinationFileDuringTransfer, FtpCompareOption.Auto) == FtpCompareResult.NotEqual)
            {
                try
                {
                    client.DeleteFile(destinationFileDuringTransfer);
                    throw new ArgumentException($"Transferred file size or Checksum was different from the source file. File {Path.GetFileName(sourceFileDuringTransfer)} was most likely corrupted during transfer.");
                }
                catch (FtpCommandException ex)
                {
                    // Log the exception and decide whether to proceed or rethrow
                    logger.NotifyError(batchContext, $"Failed to delete corrupted file '{destinationFileDuringTransfer}': {ex.Message}", ex);
                    throw;
                }
            }

            if (!doRename) return;

            if (removeExisting)
            {
                Trace(
                    TransferState.DeleteDestinationFile,
                    "Deleting destination file {0} that is to be overwritten",
                    DestinationFileNameWithMacrosExpanded);

                client.DeleteFile(DestinationFileNameWithMacrosExpanded);
            }

            Trace(
                TransferState.RenameDestinationFile,
                "Renaming temporary destination file {0} to target file {1}",
                destinationFileDuringTransfer,
                DestinationFileNameWithMacrosExpanded);

            client.Rename(destinationFileDuringTransfer, DestinationFileNameWithMacrosExpanded);
        }

        /// <summary>
        /// Restores the LastWriteTime stamp to the destination file.
        /// </summary>
        private void RestoreModified()
        {
            Trace(
                TransferState.RestoreModified,
                "Restoring the modified timestamp on transferred file {0}",
                destinationFileDuringTransfer);

            client.SetModifiedTime(DestinationFileNameWithMacrosExpanded, SourceFile.Modified);
        }

        /// <summary>
        /// Executes operations set to the source file.
        /// </summary>
        private void ExecuteSourceOperation()
        {
            var filePath = string.IsNullOrWhiteSpace(sourceFileDuringTransfer) ? SourceFile.FullPath : sourceFileDuringTransfer;
            switch (batchContext.Source.Operation)
            {
                case SourceOperation.Move:
                    var moveToPath = RenamingPolicy.CreateRemoteFileNameForMove(batchContext.Source.DirectoryToMoveAfterTransfer, SourceFile.FullPath);
                    Trace(TransferState.SourceOperationMove, "Moving source file {0} to {1}", SourceFile.FullPath, moveToPath);
                    File.Move(filePath, moveToPath);

                    if (string.IsNullOrWhiteSpace(SourceFile.FullPath))
                        logger.NotifyInformation(batchContext, "Source end point returned null as the moved file. It should return the name of the moved file.");
                    break;

                case SourceOperation.Rename:
                    var renameToPath = renamingPolicy.CreateFilePathForRename(SourceFile.FullPath, batchContext.Source.FileNameAfterTransfer);
                    Trace(TransferState.SourceOperationRename, "Renaming source file {0} to {1}", SourceFile.FullPath, renameToPath);

                    File.Move(filePath, renameToPath);

                    if (string.IsNullOrWhiteSpace(SourceFile.FullPath))
                        logger.NotifyInformation(batchContext, "Source end point returned null as the renamed file. It should return the name of the renamed file.");

                    break;

                case SourceOperation.Delete:
                    Trace(TransferState.SourceOperationDelete, "Deleting source file {0} after transfer", Path.GetFileName(SourceFile.FullPath));
                    File.Delete(filePath);
                    break;

                case SourceOperation.Nothing:
                    if (batchContext.Options.RenameSourceFileBeforeTransfer)
                    {
                        Trace(
                            TransferState.RestoreSourceFile,
                            "Restoring source file from {0} to the original name {1}",
                            Path.GetFileName(sourceFileDuringTransfer),
                            Path.GetFileName(SourceFile.FullPath));

                        File.Move(filePath, SourceFile.FullPath);
                    }

                    break;
            }
        }

        private void CleanUpFiles()
        {
            if (batchContext.Source.Operation == SourceOperation.Nothing)
            {
                Trace(
                    TransferState.CleanUpFiles,
                    "Skip removing file {0} because source operation set to Nothing",
                    sourceFileDuringTransfer);
            }
            else
            {
                Trace(TransferState.CleanUpFiles, "Removing temporary file {0}", sourceFileDuringTransfer);
                TryToRemoveLocalTempFile(sourceFileDuringTransfer);
            }

            TryToRemoveDestinationTempFile();
        }

        private void HandleTransferError(Exception exception, string sourceFileRestoreMessage)
        {
            result.Success = false; // the routine instance should be marked as failed if even one transfer fails
            var errorMessage =
                $"Failure in {state}: File '{SourceFile.Name}' could not be transferred to '{destinationDirectoryWithMacrosExpanded}'. Error: {exception.Message}. InnerException: {exception.InnerException}";
            if (!string.IsNullOrWhiteSpace(sourceFileRestoreMessage))
                errorMessage += " " + sourceFileRestoreMessage;

            result.ErrorMessages.Add(errorMessage);

            logger.LogTransferFailed(this, batchContext, errorMessage, exception);
        }

        private void TryToRemoveDestinationTempFile()
        {
            // If DestinationFileNameDuringTransfer is not set,
            // the destination file already exists and DestinationFileExistAction=Error
            if (string.IsNullOrWhiteSpace(destinationFileDuringTransfer))
                return;

            // If RenameDestinationFileDuringTransfer=False, there is no temporary file that could be deleted
            if (!batchContext.Options.RenameDestinationFileDuringTransfer)
                return;

            try
            {
                if (DestinationFileExists(Path.GetFileName(destinationFileDuringTransfer)))
                {
                    Trace(TransferState.RemoveTemporaryDestinationFile, "Removing temporary destination file {0}", destinationFileDuringTransfer);
                    client.DeleteFile(destinationFileDuringTransfer);
                }
            }
            catch (Exception ex)
            {
                logger.NotifyError(
                    batchContext,
                    $"Could not clean up temporary destination file '{destinationFileDuringTransfer}': {ex.Message}",
                    ex);
            }
        }

        private void TryToRemoveLocalTempFile(string fileName)
        {
            try
            {
                if (!FileDefinedAndExists(fileName)) return;

                if ((File.GetAttributes(fileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    File.SetAttributes(fileName, FileAttributes.Normal); // Clear flags so readonly doesn't cause any problems CO-469

                File.Delete(fileName);
            }
            catch (Exception e)
            {
                logger.NotifyError(batchContext, $"Could not clean up local file '{fileName}'", e);
            }
        }

        private string RestoreSourceFileAfterErrorIfItWasRenamed()
        {
            // restore the source file so we can retry the operations
            // - but only if the source file has been renamed in the first place
            if (string.IsNullOrWhiteSpace(sourceFileDuringTransfer)) return string.Empty;

            try
            {
                if (ShouldSourceFileBeRestoredOnError())
                {
                    File.Move(sourceFileDuringTransfer, SourceFile.FullPath);
                    return "[Source file restored.]";
                }
            }
            catch (Exception ex)
            {
                var message = string.Format(
                    "Could not restore original source file '{0}' from temporary file '{1}'. Error: {2}.",
                    Path.GetFileName(SourceFile.FullPath),
                    Path.GetFileName(sourceFileDuringTransfer),
                    ex.Message);

                logger.NotifyError(batchContext, message, ex);
                return $"[{message}]";
            }

            return string.Empty;
        }

        private bool ShouldSourceFileBeRestoredOnError()
        {
            if (batchContext.Options.RenameSourceFileBeforeTransfer)
            {
                return true;
            }

            return batchContext.Source.Operation switch
            {
                SourceOperation.Move or SourceOperation.Rename => true,
                _ => false,
            };
        }

        private void Trace(TransferState state, string format, params object[] args)
        {
            this.state = state;
            logger.NotifyTrace($"{state}: {string.Format(format, args)}");
        }

        /// <summary>
        /// Exception class for more specific Exception name.
        /// </summary>
        public class DestinationFileExistsException : Exception
        {
            /// <summary>
            /// Initializes a new instance of the <see cref="DestinationFileExistsException"/> class.
            /// Exception message.
            /// </summary>
            /// <param name="fileName">Name of the destination file that already exists.</param>
            public DestinationFileExistsException(string fileName)
                : base($"Unable to transfer file. Destination file already exists: {fileName}")
            {
            }
        }
    }
}
