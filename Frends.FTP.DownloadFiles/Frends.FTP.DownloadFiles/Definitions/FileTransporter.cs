using FluentFTP;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using Frends.FTP.DownloadFiles.Enums;
using Frends.FTP.DownloadFiles.Logging;
using Frends.FTP.DownloadFiles.TaskConfiguration;
using Frends.FTP.DownloadFiles.TaskResult;

namespace Frends.FTP.DownloadFiles.Definitions;

internal class FileTransporter
{
    private readonly IFtpLogger _logger;
    private readonly BatchContext _batchContext;
    private readonly RenamingPolicy _renamingPolicy;
    private readonly string _sourceDirectoryWithMacrosExpanded;
    private readonly string _destinationDirectoryWithMacrosExpanded;

    public FileTransporter(IFtpLogger logger, BatchContext context, Guid instanceId)
    {
        _logger = logger;
        _batchContext = context;
        _renamingPolicy = new RenamingPolicy(_batchContext.Info.TransferName, instanceId);

        _destinationDirectoryWithMacrosExpanded = _renamingPolicy.ExpandDirectoryForMacros(context.Destination.Directory);
        _sourceDirectoryWithMacrosExpanded = _renamingPolicy.ExpandDirectoryForMacros(context.Source.Directory);

        Result = new List<SingleFileTransferResult>();
    }

    private List<SingleFileTransferResult> Result { get; }

    public FileTransferResult Run(CancellationToken cancellationToken)
    {
        var userResultMessage = "";
        try
        {
            using (var client = CreateFtpClient(_batchContext.Connection))
            {
                client.Connect();

                if (!client.IsConnected)
                {
                    _logger.NotifyError(null, "Error while connecting to FTP: ", new Exception(userResultMessage));
                    return FormFailedFileTransferResult(userResultMessage);
                }

                var (files, success) = GetSourceFiles(client);
                if (!success)
                {
                    // If source directory doesn't exist, modify userResultMessage accordingly.
                    userResultMessage = $"FTP directory '{_sourceDirectoryWithMacrosExpanded}' doesn't exist.";
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
                    if (!CreateDestinationDirIfNeeded(out var failedResult)) return failedResult;

                    client.SetWorkingDirectory(_sourceDirectoryWithMacrosExpanded);

                    foreach (var file in files)
                    {
                        cancellationToken.ThrowIfCancellationRequested();
                        var singleTransfer =
                            new SingleFileTransfer(file, _batchContext, client, _renamingPolicy, _logger);
                        var result = singleTransfer.TransferSingleFile();
                        Result.Add(result);
                    }
                }

                client.Disconnect();
            }
        }
        catch (SocketException)
        {
            userResultMessage = $"Unable to establish the socket: No such host is known.";
            return FormFailedFileTransferResult(userResultMessage);
        }

        return FormResultFromSingleTransferResults(Result);
    }

    private bool CreateDestinationDirIfNeeded(out FileTransferResult fileTransferResult)
    {
        string userResultMessage;
        if (!Directory.Exists(_destinationDirectoryWithMacrosExpanded))
        {
            if (_batchContext.Options.CreateDestinationDirectories)
            {
                try
                {
                    Directory.CreateDirectory(_destinationDirectoryWithMacrosExpanded);
                }
                catch (Exception ex)
                {
                    userResultMessage =
                        $"Error while creating destination directory '{_destinationDirectoryWithMacrosExpanded}': {ex.Message}";
                    {
                        fileTransferResult = FormFailedFileTransferResult(userResultMessage);
                        return false;
                    }
                }
            }
            else
            {
                userResultMessage =
                    $"Destination directory '{_destinationDirectoryWithMacrosExpanded}' was not found.";
                {
                    fileTransferResult = FormFailedFileTransferResult(userResultMessage);
                    return false;
                }
            }
        }

        fileTransferResult = null;
        return true;
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
                throw new ArgumentOutOfRangeException($"Unknown Encoding type: '{connect.SslMode}'.");
        }

        if (connect.UseFTPS)
        {
            if (connect.EnableClientAuth)
                client.ClientCertificates.Add(new X509Certificate2(connect.ClientCertificatePath));

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

    private Tuple<List<FileItem>, bool> GetSourceFiles(FtpClient client)
    {
        if (!client.DirectoryExists(_sourceDirectoryWithMacrosExpanded)) return new Tuple<List<FileItem>, bool>(null, false);

        var ftpFiles = client.GetListing(_sourceDirectoryWithMacrosExpanded);

        var list = new List<FileItem>();
        foreach (var ftpFile in ftpFiles)
        {
            if (ftpFile.Type == FtpFileSystemObjectType.Directory ||
                ftpFile.Type == FtpFileSystemObjectType.Link)
                continue; // skip directories and links

            if (!Util.FileMatchesMask(ftpFile.Name, _batchContext.Source.FileName)) continue;

            var fItm = new FileItem(ftpFile);
            list.Add(fItm);
        }
        return new Tuple<List<FileItem>, bool>(list, true);
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

    private FileTransferResult FormResultFromSingleTransferResults(List<SingleFileTransferResult> singleResults)
    {
        var success = singleResults.All(x => x.Success);
        var actionSkipped = success && singleResults.All(x => x.ActionSkipped);
        var userResultMessage = GetUserResultMessage(singleResults.ToList());

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

        var msg =
            $"No source files found from directory '{_sourceDirectoryWithMacrosExpanded}' with file mask '{source.FileName}' for transfer '{transferName}'";

        switch (_batchContext.Source.NotFoundAction)
        {
            case SourceNotFoundAction.Error:
                _logger.NotifyError(context, msg, new FileNotFoundException());
                return new SingleFileTransferResult { Success = false, ActionSkipped = false, ErrorMessages = { msg } };
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

