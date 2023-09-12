using FluentFTP;
using Frends.FTP.UploadFiles.Enums;
using Frends.FTP.UploadFiles.Logging;
using Frends.FTP.UploadFiles.TaskConfiguration;
using Frends.FTP.UploadFiles.TaskResult;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;

namespace Frends.FTP.UploadFiles.Definitions
{
    /// <summary>
    /// Main class for FTP file transfers
    /// </summary>
    internal class FileTransporter
    {
        private readonly IFtpLogger _logger;
        private readonly BatchContext _batchContext;
        private readonly string[] _filePaths;
        private readonly RenamingPolicy _renamingPolicy;
        private readonly string _sourceDirectoryWithMacrosExpanded;
        private readonly string _destinationDirectoryWithMacrosExpanded;

        public FileTransporter(IFtpLogger logger, BatchContext context, Guid instanceId)
        {
            _logger = logger;
            _batchContext = context;
            _renamingPolicy = new RenamingPolicy(_batchContext.Info.TransferName, instanceId);

            _sourceDirectoryWithMacrosExpanded =
                _renamingPolicy.ExpandDirectoryForMacros(_batchContext.Source.Directory);
            _destinationDirectoryWithMacrosExpanded =
                _renamingPolicy.ExpandDirectoryForMacros(_batchContext.Destination.Directory);

            Result = new List<SingleFileTransferResult>();
            _filePaths = ConvertObjectToStringArray(context.Source.FilePaths);
        }

        private List<SingleFileTransferResult> Result { get; }

        public FileTransferResult Run(CancellationToken cancellationToken)
        {
            var userResultMessage = "";
            try
            {
                // Fetch source file info and check if files were returned.
                var (files, success) = GetSourceFiles();

                // If source directory doesn't exist, modify userResultMessage accordingly.
                if (!success)
                {
                    userResultMessage = $"Directory '{_sourceDirectoryWithMacrosExpanded}' doesn't exist.";
                    _logger.NotifyError(_batchContext, userResultMessage, new Exception(userResultMessage));
                    return FormFailedFileTransferResult(userResultMessage);
                }

                if (files == null || !files.Any())
                {
                    if (files == null)
                        _logger.NotifyInformation(_batchContext,
                            "Source end point returned null list for file list. If there are no files to transfer, the result should be an empty list.");

                    var noSourceResult = NoSourceOperation(_batchContext, _batchContext.Source);
                    Result.Add(noSourceResult);
                }
                else
                {
                    using (var client = CreateFtpClient(_batchContext.Connection))
                    {
                        client.Connect();

                        if (!client.IsConnected)
                        {
                            _logger.NotifyError(_batchContext, "Error while connecting to destination: ", new Exception(userResultMessage));
                            return FormFailedFileTransferResult(userResultMessage);
                        }

                        // Check does the destination directory exists.
                        if (!client.DirectoryExists(_destinationDirectoryWithMacrosExpanded))
                        {
                            if (_batchContext.Options.CreateDestinationDirectories)
                            {
                                try
                                {
                                    CreateAllDirectories(client, _destinationDirectoryWithMacrosExpanded);
                                }
                                catch (Exception ex)
                                {
                                    userResultMessage = $"Error while creating destination directory '{_destinationDirectoryWithMacrosExpanded}': {ex.Message}";
                                    _logger.NotifyError(_batchContext, userResultMessage, new Exception(userResultMessage));
                                    return FormFailedFileTransferResult(userResultMessage);
                                }
                            }
                            else
                            {
                                userResultMessage = $"Destination directory '{_destinationDirectoryWithMacrosExpanded}' was not found.";
                                _logger.NotifyError(_batchContext, userResultMessage, new Exception(userResultMessage));
                                return FormFailedFileTransferResult(userResultMessage);
                            }
                        }

                        client.SetWorkingDirectory(_destinationDirectoryWithMacrosExpanded);

                        //_batchContext.DestinationFiles = client.GetListing(".");

                        foreach (var file in files)
                        {
                            // Check that the connection is alive and if not try to connect again
                            if (!client.IsConnected)
                            {
                                client.Connect();
                                _logger.NotifyInformation(_batchContext, "Reconnected.");
                            }

                            cancellationToken.ThrowIfCancellationRequested();
                            var singleTransfer = new SingleFileTransfer(file, _batchContext, client, _renamingPolicy, _logger);
                            var result = singleTransfer.TransferSingleFile();
                            Result.Add(result);
                        }
                        client.Disconnect();
                    }
                }
            }
            catch (SocketException)
            {
                userResultMessage = $"Unable to establish the socket: No such host is known.";
                _logger.NotifyError(_batchContext, userResultMessage, new Exception(userResultMessage));
                return FormFailedFileTransferResult(userResultMessage);
            }

            return FileTransporter.FormResultFromSingleTransferResults(Result);
        }

        #region Helper methods
        private static FtpClient CreateFtpClient(Connection connect)
        {
            var client = new FtpClient(connect.Address, connect.Port, connect.UserName, connect.Password);
            switch (connect.SslMode)
            {
                case FtpsSslMode.None:
                    client.EncryptionMode = FtpEncryptionMode.None;
                    break;
                case FtpsSslMode.Implicit:
                    client.EncryptionMode = FtpEncryptionMode.Implicit;
                    break;
                case FtpsSslMode.Explicit:
                    client.EncryptionMode = FtpEncryptionMode.Explicit;
                    break;
                case FtpsSslMode.Auto:
                    client.EncryptionMode = FtpEncryptionMode.Auto;
                    break;
                default:
                    throw new ArgumentOutOfRangeException();
            }

            if (connect.UseFTPS)
            {
                if (connect.EnableClientAuth)
                {
                    if (!string.IsNullOrEmpty(connect.ClientCertificatePath))
                    {
                        client.ClientCertificates.Add(new X509Certificate2(connect.ClientCertificatePath));
                    }
                    else
                    {
                        X509Store store = new X509Store(StoreName.My, StoreLocation.LocalMachine);
                        try
                        {
                            store.Open(OpenFlags.ReadOnly);
                            client.ClientCertificates.AddRange(store.Certificates);
                        }
                        finally
                        {
                            store.Close();
                        }
                    }
                }

                client.ValidateCertificate += (control, e) =>
                {
                    // If cert is valid and such - go on and accept
                    if (e.PolicyErrors == SslPolicyErrors.None)
                    {
                        e.Accept = true;
                        return;
                    }

                    // Accept if we want to accept a certain hash
                    e.Accept = e.Certificate.GetCertHashString() == connect.CertificateHashStringSHA1;
                };

                client.ValidateAnyCertificate = connect.ValidateAnyCertificate;
                client.DataConnectionEncryption = connect.SecureDataChannel;
            }

            client.NoopInterval = connect.KeepConnectionAliveInterval;

            if (!string.IsNullOrWhiteSpace(connect.Encoding)) client.Encoding = Encoding.GetEncoding(connect.Encoding);

            // Client lib timeout is in milliseconds, ours is in seconds, thus *1000 conversion
            client.ConnectTimeout = connect.ConnectionTimeout * 1000;
            client.LocalFileBufferSize = connect.BufferSize;

            // Transport type Binary / ASCII
            switch (connect.TransportType)
            {
                case FtpTransportType.Binary:
                    client.UploadDataType = FtpDataType.Binary;
                    client.DownloadDataType = FtpDataType.Binary;
                    break;
                case FtpTransportType.Ascii:
                    client.UploadDataType = FtpDataType.ASCII;
                    client.DownloadDataType = FtpDataType.ASCII;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown FTP transport type {connect.TransportType}");
            }

            // Active/passive
            switch (connect.Mode)
            {
                case FtpMode.Active:
                    client.DataConnectionType = FtpDataConnectionType.AutoActive;
                    break;
                case FtpMode.Passive:
                    client.DataConnectionType = FtpDataConnectionType.AutoPassive;
                    break;
                default:
                    throw new ArgumentOutOfRangeException($"Unknown FTP mode {connect.Mode}");
            }

            return client;
        }

        private Tuple<List<FileItem>, bool> GetSourceFiles()
        {
            var fileItems = new List<FileItem>();

            if (_filePaths != null)
            {
                fileItems = _filePaths.Select(p => new FileItem(p) { Name = p }).ToList();
                if (fileItems.Any())
                    return new Tuple<List<FileItem>, bool>(fileItems, true);
                return new Tuple<List<FileItem>, bool>(fileItems, false);
            }

            // Return empty list if source directory doesn't exists.
            if (!Directory.Exists(_sourceDirectoryWithMacrosExpanded))
                return new Tuple<List<FileItem>, bool>(fileItems, false);

            // fetch all file names in given directory
            var files = Directory.GetFiles(_sourceDirectoryWithMacrosExpanded);

            // return Tuple with empty list and success.true if files are not found.
            if (!files.Any())
                return new Tuple<List<FileItem>, bool>(fileItems, true);

            // create List of FileItems from found files.
            foreach (var file in files)
            {
                if (Util.FileMatchesMask(Path.GetFileName(file), _batchContext.Source.FileName))
                {
                    FileItem item = new FileItem(Path.GetFullPath(file));
                    _logger.NotifyInformation(_batchContext, $"FILE LIST {item.FullPath}");
                    fileItems.Add(item);
                }
            }

            return new Tuple<List<FileItem>, bool>(fileItems, true);
        }

        private static void CreateAllDirectories(FtpClient client, string path)
        {
            // Consistent forward slashes
            path = path.Replace(@"\", "/");
            foreach (string dir in path.Split('/'))
            {
                // Ignoring leading/ending/multiple slashes
                if (!string.IsNullOrWhiteSpace(dir))
                {
                    if (!client.DirectoryExists(dir))
                        client.CreateDirectory(dir);

                    client.SetWorkingDirectory(dir);
                }
            }
            // Going back to default directory
            client.SetWorkingDirectory("/");
        }

        private static string[] ConvertObjectToStringArray(object objectArray)
        {
            var res = objectArray as object[];
            return res?.OfType<string>().ToArray();
        }

        private static FileTransferResult FormFailedFileTransferResult(string userResultMessage)
        {
            return new FileTransferResult
            {
                ActionSkipped = true,
                Success = false,
                UserResultMessage = userResultMessage,
                SuccessfulTransferCount = 0,
                FailedTransferCount = 0,
                TransferredFileNames = new List<string>(),
                TransferErrors = new Dictionary<string, IList<string>>(),
                TransferredFilePaths = new List<string>(),
                OperationsLog = new Dictionary<string, string>()
            };
        }

        private static FileTransferResult FormResultFromSingleTransferResults(List<SingleFileTransferResult> singleResults)
        {
            var success = singleResults.All(x => x.Success);
            var actionSkipped = success && singleResults.All(x => x.ActionSkipped);
            var userResultMessage = FileTransporter.GetUserResultMessage(singleResults.ToList());

            var transferErrors = singleResults.Where(r => !r.Success).GroupBy(r => r.TransferredFile ?? "--unknown--")
                    .ToDictionary(rg => rg.Key, rg => (IList<string>)rg.SelectMany(r => r.ErrorMessages).ToList());

            var transferredFileResults = singleResults.Where(r => r.Success && !r.ActionSkipped).ToList();

            return new FileTransferResult
            {
                ActionSkipped = actionSkipped,
                Success = success,
                UserResultMessage = userResultMessage,
                SuccessfulTransferCount = singleResults.Count(s => s.Success && !s.ActionSkipped),
                FailedTransferCount = singleResults.Count(s => !s.Success && !s.ActionSkipped),
                TransferredFileNames = transferredFileResults.Select(r => r.TransferredFile ?? "--unknown--").ToList(),
                TransferErrors = transferErrors,
                TransferredFilePaths = transferredFileResults.Select(r => r.TransferredFilePath ?? "--unknown--").ToList(),
                OperationsLog = new Dictionary<string, string>()
            };
        }

        private static string GetUserResultMessage(IList<SingleFileTransferResult> results)
        {
            var userResultMessage = string.Empty;

            var errorMessages = results.SelectMany(x => x.ErrorMessages).ToList();
            if (errorMessages.Any())
                userResultMessage = MessageJoin(userResultMessage,
                    $"{errorMessages.Count} Errors: {string.Join(", ", errorMessages)}");

            var transferredFiles = results.Select(x => x.TransferredFile).Where(x => x != null).ToList();
            if (transferredFiles.Any())
                userResultMessage = MessageJoin(userResultMessage,
                    string.Format("{0} files transferred: {1}", transferredFiles.Count,
                        string.Join(", ", transferredFiles)));
            else
                userResultMessage = MessageJoin(userResultMessage, "No files transferred.");

            return userResultMessage;
        }

        private static string MessageJoin(params string[] args)
        {
            return string.Join(" ", args.Where(s => !string.IsNullOrWhiteSpace(s)));
        }

        private SingleFileTransferResult NoSourceOperation(BatchContext context, Source source)
        {
            var transferName = context.Info.TransferName ?? string.Empty;

            var msg = context.Source.FilePaths == null
                ? $"No source files found from directory '{_sourceDirectoryWithMacrosExpanded}' with file mask '{source.FileName}' for transfer '{transferName}'"
                : $"No source files found from FilePaths '{string.Join(", ", context.Source.FilePaths)}' for transfer '{transferName}'";

            switch (_batchContext.Source.NotFoundAction)
            {
                case SourceNotFoundAction.Error:
                    _logger.NotifyError(context, msg, new FileNotFoundException());
                    return new SingleFileTransferResult { Success = false, ErrorMessages = { msg } };
                case SourceNotFoundAction.Info:
                    _logger.NotifyInformation(context, msg);
                    return new SingleFileTransferResult { Success = true, ActionSkipped = true, ErrorMessages = { msg } };
                case SourceNotFoundAction.Ignore:
                    return new SingleFileTransferResult { Success = true, ActionSkipped = true, ErrorMessages = { msg } };
                default:
                    throw new Exception("Unknown operation in NoSourceOperation");
            }
        }
        #endregion
    }
}