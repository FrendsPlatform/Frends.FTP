using System;
using System.IO;
using FluentFTP;
using Frends.FTP.DownloadFiles.Enums;
using Frends.FTP.DownloadFiles.Logging;

namespace Frends.FTP.DownloadFiles.Definitions
{
    internal class SingleFileTransfer
    {
        private readonly RenamingPolicy _renamingPolicy;
        private readonly IFtpLogger _logger;
        private readonly SingleFileTransferResult _result;
        private readonly FtpClient _client;
        private readonly BatchContext _batchContext;
        private TransferState _state;
        private string _sourceFileDuringTransfer;
        private string _destinationFileDuringTransfer;

        public SingleFileTransfer(FileItem file, BatchContext context, FtpClient client, RenamingPolicy renamingPolicy, IFtpLogger logger)
        {
            _renamingPolicy = renamingPolicy;
            _logger = logger;
            _client = client;
            SourceFile = file;
            _batchContext = context;

            DestinationFileNameWithMacrosExpanded = renamingPolicy.CreateRemoteFileName(
                    file.Name,
                    context.Destination.FileName);

            _result = new SingleFileTransferResult { Success = true };
        }

        public FileItem SourceFile { get; set; }

        public string DestinationFileNameWithMacrosExpanded { get; set; }
        
        public SingleFileTransferResult TransferSingleFile()
        {
            try
            {
                _result.TransferredFile = SourceFile.Name;
                _result.TransferredFilePath = SourceFile.FullPath;
                if (_batchContext.Options.RenameSourceFileBeforeTransfer)
                    RenameSourceFile();
                else 
                    _sourceFileDuringTransfer = SourceFile.FullPath;

                if (DestinationFileExists(DestinationFileNameWithMacrosExpanded))
                {
                    switch (_batchContext.Destination.Action)
                    {
                        case DestinationAction.Overwrite:
                            PutDestinationFile(removeExisting: true);
                            break;
                        case DestinationAction.Error:
                            throw new DestinationFileExistsException(DestinationFileNameWithMacrosExpanded);
                    }
                }
                else
                {
                    PutDestinationFile();
                }

                if (_batchContext.Options.PreserveLastModified)
                    RestoreModified();

                ExecuteSourceOperation();
                _logger.LogTransferSuccess(this, _batchContext);
                CleanUpFiles();
            }
            catch (Exception ex)
            {
                var sourceFileRestoreMessage = RestoreSourceFileAfterErrorIfItWasRenamed();
                HandleTransferError(ex, sourceFileRestoreMessage);
            }
            CleanUpFiles();
            return _result;
        }

        private bool DestinationFileExists(string fileName)
        {
            Trace(
                TransferState.CheckIfDestinationFileExists,
                "Checking if destination file {0} exists",
                SourceFile.Name);
            var fullDestinationPath = GetDestinationFilePath(fileName);
            return File.Exists(fullDestinationPath);
        }

        private string GetDestinationFilePath(string fileName)
        {
            var destinationDir = _renamingPolicy.ExpandDirectoryForMacros(_batchContext.Destination.Directory);
            return Path.Combine(destinationDir, fileName);
        }

        /// <summary>
        /// Rename file with unique file name. 
        /// </summary>
        private void RenameSourceFile()
        {
            var uniqueFileName = Util.CreateUniqueFileName();
            var workdir = !string.IsNullOrEmpty(_batchContext.Info.WorkDir)
                ? _batchContext.Info.WorkDir
                : Path.GetDirectoryName(SourceFile.FullPath);
            var renamedFile = Path.Combine(workdir, uniqueFileName);

            Trace(TransferState.RenameSourceFileBeforeTransfer, "Renaming source file {0} to temporary file name {1} before transfer", SourceFile.Name, uniqueFileName);
            File.Move(SourceFile.FullPath, renamedFile);

            _sourceFileDuringTransfer = renamedFile;
        }

        private void PutDestinationFile(bool removeExisting = false)
        {
            var doRename = _batchContext.Options.RenameDestinationFileDuringTransfer;

            _destinationFileDuringTransfer = doRename ? Util.CreateUniqueFileName() : DestinationFileNameWithMacrosExpanded;
            
            Trace(
                TransferState.PutFile,
                "Downloading {0}destination file {1}",
                doRename ? "temporary " : string.Empty,
                _destinationFileDuringTransfer);

            _client.DownloadFile(
                GetDestinationFilePath(_destinationFileDuringTransfer), _sourceFileDuringTransfer);

            if (!doRename) return;
            
            if (removeExisting)
            {
                Trace(
                    TransferState.DeleteDestinationFile,
                    "Deleting destination file {0} that is to be overwritten",
                    DestinationFileNameWithMacrosExpanded);

                File.Delete(GetDestinationFilePath(DestinationFileNameWithMacrosExpanded));
            }

            Trace(
                TransferState.RenameDestinationFile,
                "Renaming temporary destination file {0} to target file {1}",
                _destinationFileDuringTransfer,
                DestinationFileNameWithMacrosExpanded);

            File.Move(_destinationFileDuringTransfer, GetDestinationFilePath(DestinationFileNameWithMacrosExpanded));
        }

        /// <summary>
        /// Restores the LastWriteTime stamp to the destination file.
        /// </summary>
        private void RestoreModified()
        {
            File.SetLastWriteTime(DestinationFileNameWithMacrosExpanded, SourceFile.Modified);
        }

        private void ExecuteSourceOperation()
        {
            var filePath = (string.IsNullOrEmpty(_sourceFileDuringTransfer) ? SourceFile.FullPath : _sourceFileDuringTransfer);
            switch (_batchContext.Source.Operation)
            {
                case SourceOperation.Move:
                    var moveToPath = _renamingPolicy.CreateRemoteFileNameForMove(_batchContext.Source.DirectoryToMoveAfterTransfer, SourceFile.FullPath);
                    Trace(TransferState.SourceOperationMove, "Moving source file {0} to {1}", SourceFile.FullPath, moveToPath);
                    _client.MoveFile(filePath, moveToPath);

                    if (SourceFile.FullPath == null)
                    {
                        _logger.NotifyInformation(_batchContext, "Source end point returned null as the moved file. It should return the name of the moved file.");
                    }
                    break;

                case SourceOperation.Rename:
                    var renameToPath = _renamingPolicy.CreateFilePathForRename(SourceFile.FullPath, _batchContext.Source.FileNameAfterTransfer);
                    Trace(TransferState.SourceOperationRename, "Renaming source file {0} to {1}", SourceFile.FullPath, renameToPath);
                    
                    _client.MoveFile(filePath, renameToPath);

                    if (SourceFile.FullPath == null)
                    {
                        _logger.NotifyInformation(_batchContext, "Source end point returned null as the renamed file. It should return the name of the renamed file.");
                    }
                    break;

                case SourceOperation.Delete:
                    Trace(TransferState.SourceOperationDelete, "Deleting source file {0} after transfer", Path.GetFileName(SourceFile.FullPath));
                    _client.DeleteFile(filePath);
                    break;

                case SourceOperation.Nothing:
                    if (_batchContext.Options.RenameSourceFileBeforeTransfer)
                    {
                        Trace(
                            TransferState.RestoreSourceFile,
                            "Restoring source file from {0} to the original name {1}",
                            Path.GetFileName(_sourceFileDuringTransfer),
                            Path.GetFileName(SourceFile.FullPath));

                        _client.MoveFile(filePath, SourceFile.FullPath);
                    }
                    break;
            }
        }

        private void CleanUpFiles()
        {
            if (_batchContext.Source.Operation == SourceOperation.Nothing)
            {
                Trace(
                    TransferState.CleanUpFiles,
                    "Skip removing file {0} because source operation set to Nothing", _sourceFileDuringTransfer);
            }
            else
            {
                Trace(TransferState.CleanUpFiles, "Removing temporary file {0}", _sourceFileDuringTransfer);
                TryToRemoveLocalTempFile(_sourceFileDuringTransfer);
            }

            TryToRemoveDestinationTempFile();
        }

        private void HandleTransferError(Exception exception, string sourceFileRestoreMessage)
        {
            _result.Success = false; // the routine instance should be marked as failed if even one transfer fails
            var errorMessage =
                $"Failure in {_state}: File '{SourceFile.Name}' could not be transferred to '{_batchContext.Destination.Directory}'. Error: {exception.Message}";
            if (!string.IsNullOrEmpty(sourceFileRestoreMessage))
            {
                errorMessage += " " + sourceFileRestoreMessage;
            }

            _result.ErrorMessages.Add(errorMessage);

            _logger.LogTransferFailed(this, _batchContext, errorMessage, exception);
        }

        private void TryToRemoveDestinationTempFile()
        {
            // If DestinationFileNameDuringTransfer is not set,
            // the destination file already exists and DestinationFileExistAction=Error
            if (string.IsNullOrEmpty(_destinationFileDuringTransfer))
            {
                return;
            }

            // If RenameDestinationFileDuringTransfer=False, there is no temporary file that could be deleted
            if (!_batchContext.Options.RenameDestinationFileDuringTransfer)
            {
                return;
            }

            try
            {
                if (DestinationFileExists(Path.GetFileName(_destinationFileDuringTransfer)))
                {
                    Trace(TransferState.RemoveTemporaryDestinationFile, "Removing temporary destination file {0}", _destinationFileDuringTransfer);
                    File.Delete(_destinationFileDuringTransfer);
                }
            }
            catch (Exception ex)
            {
                _logger.NotifyError(_batchContext,
                    $"Could not clean up temporary destination file '{_destinationFileDuringTransfer}': {ex.Message}", ex);
            }
        }

        private void TryToRemoveLocalTempFile(string fileName)
        {
            try
            {
                if (FileDefinedAndExists(fileName))
                {
                    if ((File.GetAttributes(fileName) & FileAttributes.ReadOnly) == FileAttributes.ReadOnly)
                    {
                        File.SetAttributes(fileName, FileAttributes.Normal); // Clear flags so readonly doesn't cause any problems CO-469
                    }
                    File.Delete(fileName);
                }
            }
            catch (Exception e)
            {
                _logger.NotifyError(_batchContext, $"Could not clean up local file '{fileName}'", e);
            }
        }

        private bool FileDefinedAndExists(string filename)
        {
            return !string.IsNullOrEmpty(filename) && File.Exists(filename);
        }

        private string RestoreSourceFileAfterErrorIfItWasRenamed()
        {
            // restore the source file so we can retry the operations
            // - but only if the source file has been renamed in the first place
            if (string.IsNullOrEmpty(_sourceFileDuringTransfer)) return string.Empty;
            
            try
            {
                if (ShouldSourceFileBeRestoredOnError())
                {
                    File.Move(_sourceFileDuringTransfer, SourceFile.FullPath);
                    return "[Source file restored.]";
                }
            }
            catch (Exception ex)
            {
                var message = string.Format(
                    "Could not restore original source file '{0}' from temporary file '{1}'. Error: {2}.",
                    Path.GetFileName(SourceFile.FullPath),
                    Path.GetFileName(_sourceFileDuringTransfer),
                    ex.Message);

                _logger.NotifyError(_batchContext, message, ex);
                return $"[{message}]";
            }

            return string.Empty;
        }

        private bool ShouldSourceFileBeRestoredOnError()
        {
            if (_batchContext.Options.RenameSourceFileBeforeTransfer)
            {
                return true;
            }

            if (_batchContext.Source.Operation == SourceOperation.Move)
            {
                return true;
            }

            if (_batchContext.Source.Operation == SourceOperation.Rename)
            {
                return true;
            }

            return false;
        }

        private void Trace(TransferState state, string format, params object[] args)
        {
            _state = state;
            _logger.NotifyTrace($"{state}: {string.Format(format, args)}");
        }

        private class DestinationFileExistsException : Exception
        {
            public DestinationFileExistsException(string fileName) 
                : base($"Unable to transfer file. Destination file already exists: {fileName}") { }
        }
    }
}
