using Frends.FTP.UploadFiles.Enums;
using Frends.FTP.UploadFiles.TaskConfiguration;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Authentication;
using System.Threading;

namespace Frends.FTP.UploadFiles.Tests;

[TestClass]
public class UploadFilesTests
{
    private string _dir;
    private readonly string _file = "file1.txt";

    private Source _source = new();
    private Connection _connection = new();
    private Destination _destination = new();
    private Options _options = new();
    private Info _info = new();

    [TestInitialize]
    public void SetUp()
    {
        _dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(_dir);
        var targetDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "DockerVolumes", "data", "TestFiles");
        var fullPath = Path.GetFullPath(targetDir);
        Directory.CreateDirectory(fullPath);


        File.WriteAllText(Path.Combine(_dir, _file), "test");

        _source = new()
        {
            Directory = _dir,
            FileName = _file,
            DirectoryToMoveAfterTransfer = _dir,
            FileNameAfterTransfer = _file + Guid.NewGuid().ToString(),
            NotFoundAction = default,
            FilePaths = _dir,
            Operation = default,
        };

        _connection = new()
        {
            Address = Helpers.FtpHost,
            UserName = Helpers.FtpUsername,
            Password = Helpers.FtpPassword,
            Port = Helpers.FtpPort,
            TransportType = default,
            Mode = default,
            KeepConnectionAliveInterval = default,
            ConnectionTimeout = 60,
            Encoding = default,
            BufferSize = default,
            UseFTPS = false,
            SslMode = default,
            CertificateHashStringSHA1 = "D911262984DE9CC32A3518A1094CD24249EA5C49",
            EnableClientAuth = default,
            ValidateAnyCertificate = default,
            ClientCertificatePath = default,
            SecureDataChannel = false,
        };

        _destination = new()
        {
            Directory = "/TestFiles",
            Action = default,
            FileName = $"{_file}-{Guid.NewGuid()}",
        };

        _options = new()
        {
            CreateDestinationDirectories = default,
            OperationLog = default,
            PreserveLastModified = default,
            RenameDestinationFileDuringTransfer = default,
            RenameSourceFileBeforeTransfer = default,
            ThrowErrorOnFail = default,
        };

        _info = new()
        {
            ProcessUri = default,
            TaskExecutionID = default,
            TransferName = default,
            WorkDir = default,
        };
    }

    [TestCleanup]
    public void CleanUp()
    {
        if (Directory.Exists(_dir))
        {
            try
            {
                Directory.Delete(_dir, true);
                Console.WriteLine("Directory deleted successfully.");
            }
            catch (Exception ex)
            {
                Console.WriteLine($"An error occurred while deleting the directory: {ex.Message}");
            }
        }

        var dirs = new List<string>() { "TestFiles", "DestinationAction_Append_NoRenameInTransfer", "DestinationAction_Append_WithRenameInTransfer", "MacrosWorkInDestinationFileName", "MacrosWorkInSourceDirectory", "SourceOperation_Delete", "SourceOperation_Move", "SourceOperation_Nothing", "SourceOperation_Rename", "SourceOperation_Delete" };

        foreach (var dir in dirs)
        {
            var targetDir = Path.Combine(Directory.GetCurrentDirectory(), "..", "..", "..", "DockerVolumes", "data", dir);
            var fullPath = Path.GetFullPath(targetDir);
            if (Directory.Exists(fullPath))
            {
                try
                {
                    Directory.Delete(fullPath, true);
                    Console.WriteLine($"Directory '{fullPath}' deleted successfully.");
                }
                catch (Exception ex)
                {
                    Console.WriteLine($"An error occurred while deleting directory '{fullPath}': {ex.Message}");
                }
            }
        }
    }

    [TestMethod]
    public void UploadFTPS_IncorrectFingerprint()
    {
        var connection = _connection;
        connection.SslMode = FtpsSslMode.Explicit;
        connection.UseFTPS = true;
        connection.CertificateHashStringSHA1 = "incorrect";

        // Test and assert
        var ex = Assert.ThrowsException<AggregateException>(() => FTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
        Assert.AreEqual(1, ex.InnerExceptions.Count);
        Assert.AreEqual(typeof(AuthenticationException), ex.InnerExceptions[0].GetType());
    }

    [TestMethod]
    public void UploadFTPS_IncorrectAuth()
    {
        var connectionA = _connection;
        connectionA.Address = null;

        var connectionB = _connection;
        connectionB.Port = 0;

        var connectionC = _connection;
        connectionC.Password = null;

        var connectionD = _connection;
        connectionD.UserName = null;

        var ex1 = Assert.ThrowsException<NullReferenceException>(() => FTP.UploadFiles(_source, _destination, connectionA, _options, _info, new CancellationToken()));
        Assert.AreEqual(typeof(NullReferenceException), ex1.GetType());

        var ex2 = Assert.ThrowsException<NullReferenceException>(() => FTP.UploadFiles(_source, _destination, connectionB, _options, _info, new CancellationToken()));
        Assert.AreEqual(typeof(NullReferenceException), ex2.GetType());

        var ex3 = Assert.ThrowsException<NullReferenceException>(() => FTP.UploadFiles(_source, _destination, connectionC, _options, _info, new CancellationToken()));
        Assert.AreEqual(typeof(NullReferenceException), ex3.GetType());

        var ex4 = Assert.ThrowsException<NullReferenceException>(() => FTP.UploadFiles(_source, _destination, connectionD, _options, _info, new CancellationToken()));
        Assert.AreEqual(typeof(NullReferenceException), ex4.GetType());
    }

    [TestMethod]
    public void UploadFTP_UploadFile()
    {
        var result = FTP.UploadFiles(_source, _destination, _connection, _options, _info, new CancellationToken());
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [TestMethod]
    public void UploadFTPS_DestinationActions()
    {
        var destinationActions = new[] { DestinationAction.Error, DestinationAction.Overwrite, DestinationAction.Append };

        foreach (var destinationAction in destinationActions)
        {
            var destination = _destination;
            destination.Action = destinationAction;

            var result = FTP.UploadFiles(_source, destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success, $"DestinationAction: {destinationAction}");
            Assert.IsFalse(result.ActionSkipped, $"DestinationAction: {destinationAction}");
            Assert.AreEqual(1, result.SuccessfulTransferCount, $"DestinationAction: {destinationAction}");
            Assert.AreEqual(0, result.TransferErrors.Count, $"DestinationAction: {destinationAction}");
            Assert.AreEqual(1, result.TransferredFileNames.Count(), $"DestinationAction: {destinationAction}");
            Assert.AreEqual(1, result.TransferredFilePaths.Count(), $"DestinationAction: {destinationAction}");
            Assert.IsTrue(result.UserResultMessage.Contains("files transferred"), $"DestinationAction: {destinationAction}");
            Assert.IsTrue(result.OperationsLog.Count < 4, $"DestinationAction: {destinationAction}");

            CleanUp();
            SetUp();
        }
    }

    [TestMethod]
    public void UploadFTPS_FtpMode()
    {
        var ftpModes = new[] { FtpMode.Active, FtpMode.Passive };

        foreach (var ftpMode in ftpModes)
        {
            var connection = _connection;
            connection.Mode = ftpMode;

            var result = FTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            if (ftpMode is FtpMode.Active)
            {
                Assert.IsFalse(result.Success);
                Assert.IsTrue(result.UserResultMessage.Contains("Error: Error while uploading the file to the server. See InnerException for more info."), $"FtpMode: {ftpMode}");
                Assert.IsFalse(result.ActionSkipped, $"FtpMode: {ftpMode}");
                Assert.AreEqual(0, result.SuccessfulTransferCount, $"FtpMode: {ftpMode}");
                Assert.AreEqual(1, result.TransferErrors.Count, $"FtpMode: {ftpMode}");
                Assert.AreEqual(0, result.TransferredFileNames.Count(), $"FtpMode: {ftpMode}");
                Assert.AreEqual(0, result.TransferredFilePaths.Count(), $"FtpMode: {ftpMode}");
            }
            else
            {
                Assert.IsTrue(result.Success, $"FtpMode: {ftpMode}");
                Assert.IsFalse(result.ActionSkipped, $"FtpMode: {ftpMode}");
                Assert.AreEqual(1, result.SuccessfulTransferCount, $"FtpMode: {ftpMode}");
                Assert.AreEqual(0, result.TransferErrors.Count, $"FtpMode: {ftpMode}");
                Assert.AreEqual(1, result.TransferredFileNames.Count(), $"FtpMode: {ftpMode}");
                Assert.AreEqual(1, result.TransferredFilePaths.Count(), $"FtpMode: {ftpMode}");
                Assert.IsTrue(result.UserResultMessage.Contains("files transferred"), $"FtpMode: {ftpMode}");
            }
            Assert.IsTrue(result.OperationsLog.Count < 4, $"FtpMode: {ftpMode}");

            CleanUp();
            SetUp();
        }
    }

    [TestMethod]
    public void UploadFTPS_FtpsSslModes()
    {
        var ftpsSslModes = new[] { FtpsSslMode.None, FtpsSslMode.Explicit, FtpsSslMode.Auto };

        foreach (var ftpsSslMode in ftpsSslModes)
        {
            var connection = _connection;
            connection.SslMode = ftpsSslMode;
            connection.UseFTPS = true;

            var result = FTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success, $"FtpsSslMode: {ftpsSslMode}");
            Assert.IsFalse(result.ActionSkipped, $"FtpsSslMode: {ftpsSslMode}");
            Assert.AreEqual(1, result.SuccessfulTransferCount, $"FtpsSslMode: {ftpsSslMode}");
            Assert.AreEqual(0, result.TransferErrors.Count, $"FtpsSslMode: {ftpsSslMode}");
            Assert.AreEqual(1, result.TransferredFileNames.Count(), $"FtpsSslMode: {ftpsSslMode}");
            Assert.AreEqual(1, result.TransferredFilePaths.Count(), $"FtpsSslMode: {ftpsSslMode}");
            Assert.IsTrue(result.UserResultMessage.Contains("files transferred"), $"FtpsSslMode: {ftpsSslMode}");
            Assert.IsTrue(result.OperationsLog.Count < 4, $"FtpsSslMode: {ftpsSslMode}");

            CleanUp();
            SetUp();
        }
    }

    [TestMethod]
    public void UploadFTPS_SecureDataChannel()
    {
        var boo = new[] { true, false };

        foreach (var bo in boo)
        {
            var connection = _connection;
            connection.SecureDataChannel = bo;

            var result = FTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success, $"Bo: {bo}");
            Assert.IsFalse(result.ActionSkipped, $"Bo: {bo}");
            Assert.AreEqual(1, result.SuccessfulTransferCount, $"Bo: {bo}");
            Assert.AreEqual(0, result.TransferErrors.Count, $"Bo: {bo}");
            Assert.AreEqual(1, result.TransferredFileNames.Count(), $"Bo: {bo}");
            Assert.AreEqual(1, result.TransferredFilePaths.Count(), $"Bo: {bo}");
            Assert.IsTrue(result.UserResultMessage.Contains("files transferred"), $"Bo: {bo}");
            Assert.IsTrue(result.OperationsLog.Count < 4, $"Bo: {bo}");

            CleanUp();
            SetUp();
        }
    }

    [TestMethod]
    public void UploadFTPS_FtpTransportTypes()
    {
        var ftpTransportTypes = new[] { FtpTransportType.Binary, FtpTransportType.Ascii };

        foreach (var ftpTransportType in ftpTransportTypes)
        {
            var connection = _connection;
            connection.TransportType = ftpTransportType;

            var result = FTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success, $"FtpTransportType: {ftpTransportType}");
            Assert.IsFalse(result.ActionSkipped, $"FtpTransportType: {ftpTransportType}");
            Assert.AreEqual(1, result.SuccessfulTransferCount, $"FtpTransportType: {ftpTransportType}");
            Assert.AreEqual(0, result.TransferErrors.Count, $"FtpTransportType: {ftpTransportType}");
            Assert.AreEqual(1, result.TransferredFileNames.Count(), $"FtpTransportType: {ftpTransportType}");
            Assert.AreEqual(1, result.TransferredFilePaths.Count(), $"FtpTransportType: {ftpTransportType}");
            Assert.IsTrue(result.UserResultMessage.Contains("files transferred"), $"FtpTransportType: {ftpTransportType}");
            Assert.IsTrue(result.OperationsLog.Count < 4, $"FtpTransportType: {ftpTransportType}");

            CleanUp();
            SetUp();
        }
    }

    [TestMethod]
    public void UploadFTPS_NotFoundActions()
    {
        var notFoundActions = new[] { SourceNotFoundAction.Error, SourceNotFoundAction.Ignore, SourceNotFoundAction.Info };

        foreach (var notFoundAction in notFoundActions)
        {
            var source = _source;
            source.NotFoundAction = notFoundAction;
            source.FileName = "";

            var result = FTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());

            if (notFoundAction is SourceNotFoundAction.Error)
            {
                Assert.IsFalse(result.Success, $"action: {notFoundAction}");
                Assert.IsFalse(result.ActionSkipped, $"action: {notFoundAction}");
                Assert.AreEqual(0, result.SuccessfulTransferCount, $"action: {notFoundAction}");
                Assert.AreEqual(1, result.TransferErrors.Count, $"action: {notFoundAction}");
                Assert.AreEqual(0, result.TransferredFileNames.Count(), $"action: {notFoundAction}");
                Assert.AreEqual(0, result.TransferredFilePaths.Count(), $"action: {notFoundAction}");

            }
            else
            {
                Assert.IsTrue(result.Success, $"action: {notFoundAction}");
                Assert.IsTrue(result.ActionSkipped, $"action: {notFoundAction}");
                Assert.AreEqual(0, result.SuccessfulTransferCount, $"action: {notFoundAction}");
                Assert.AreEqual(0, result.TransferErrors.Count, $"action: {notFoundAction}");
                Assert.AreEqual(0, result.TransferredFileNames.Count(), $"action: {notFoundAction}");
                Assert.AreEqual(0, result.TransferredFilePaths.Count(), $"action: {notFoundAction}");

            }
            Assert.IsTrue(result.UserResultMessage.Contains("No source files found"), $"action: {notFoundAction}");
            Assert.IsTrue(result.OperationsLog.Count < 4, $"action: {notFoundAction}");

            CleanUp();
            SetUp();
        }
    }

    [TestMethod]
    public void UploadFTPS_SourceOperations()
    {
        var sourceOperations = new[] { SourceOperation.Move, SourceOperation.Delete, SourceOperation.Rename, SourceOperation.Nothing };

        foreach (var sourceOperation in sourceOperations)
        {
            var source = _source;
            source.Operation = sourceOperation;

            var result = FTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success, $"SourceOperation: {sourceOperation}");
            Assert.IsFalse(result.ActionSkipped, $"SourceOperation: {sourceOperation}");
            Assert.AreEqual(1, result.SuccessfulTransferCount, $"SourceOperation: {sourceOperation}");
            Assert.AreEqual(0, result.TransferErrors.Count, $"SourceOperation: {sourceOperation}");
            Assert.AreEqual(1, result.TransferredFileNames.Count(), $"SourceOperation: {sourceOperation}");
            Assert.AreEqual(1, result.TransferredFilePaths.Count(), $"SourceOperation: {sourceOperation}");
            Assert.IsTrue(result.UserResultMessage.Contains("files transferred"), $"SourceOperation: {sourceOperation}");
            Assert.IsTrue(result.OperationsLog.Count < 4, $"SourceOperation: {sourceOperation}");

            CleanUp();
            SetUp();
        }
    }

    [TestMethod]
    public void UploadFTPS_FileEncodings()
    {
        var fileEncodings = new[] { "UTF-8", "ASCII", "foobar123", string.Empty };

        foreach (var fileEncoding in fileEncodings)
        {
            var connection = _connection;
            connection.Encoding = fileEncoding;

            if (connection.Encoding.Equals("foobar123"))
            {
                var ex = Assert.ThrowsException<ArgumentException>(() => FTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken()));
                Assert.IsTrue(ex.Message.Contains("is not a supported encoding name"), $"FileEncoding: {fileEncoding}");
            }
            else
            {
                var result = FTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
                Assert.IsTrue(result.Success, $"FileEncoding: {fileEncoding}");
                Assert.IsFalse(result.ActionSkipped, $"FileEncoding: {fileEncoding}");
                Assert.AreEqual(1, result.SuccessfulTransferCount, $"FileEncoding: {fileEncoding}");
                Assert.AreEqual(0, result.TransferErrors.Count, $"FileEncoding: {fileEncoding}");
                Assert.AreEqual(1, result.TransferredFileNames.Count(), $"FileEncoding: {fileEncoding}");
                Assert.AreEqual(1, result.TransferredFilePaths.Count(), $"FileEncoding: {fileEncoding}");
                Assert.IsTrue(result.UserResultMessage.Contains("files transferred"), $"FileEncoding: {fileEncoding}");
                Assert.IsTrue(result.OperationsLog.Count < 4, $"FileEncoding: {fileEncoding}");
            }

            CleanUp();
            SetUp();
        }
    }

    [TestMethod]
    public void UploadFTPS_Port()
    {
        var ports = new[] { 0, 21, 210 };

        foreach (var port in ports)
        {
            var connection = _connection;
            connection.Port = port;

            var result = FTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            if (port == 210)
            {
                Assert.IsFalse(result.Success, $"Port: {port}");
                Assert.IsTrue(result.ActionSkipped, $"Port: {port}");
                Assert.AreEqual(0, result.SuccessfulTransferCount, $"Port: {port}");
                Assert.AreEqual(0, result.TransferErrors.Count, $"Port: {port}");
                Assert.AreEqual(0, result.TransferredFileNames.Count(), $"Port: {port}");
                Assert.AreEqual(0, result.TransferredFilePaths.Count(), $"Port: {port}");
                Assert.IsTrue(result.UserResultMessage.Contains("Unable to establish the socket: No such host is known."), $"Port: {port}");
                Assert.IsTrue(result.OperationsLog.Count < 4, $"Port: {port}");
            }
            else
            {
                Assert.IsTrue(result.Success, $"Port: {port}");
                Assert.IsFalse(result.ActionSkipped, $"Port: {port}");
                Assert.AreEqual(1, result.SuccessfulTransferCount, $"Port: {port}");
                Assert.AreEqual(0, result.TransferErrors.Count, $"Port: {port}");
                Assert.AreEqual(1, result.TransferredFileNames.Count(), $"Port: {port}");
                Assert.AreEqual(1, result.TransferredFilePaths.Count(), $"Port: {port}");
                Assert.IsTrue(result.UserResultMessage.Contains("files transferred"), $"Port: {port}");
                Assert.IsTrue(result.OperationsLog.Count < 4, $"Port: {port}");
            }

            CleanUp();
            SetUp();
        }
    }

    [TestMethod]
    public void UploadFTPS_KeepConnectionAliveInterval()
    {
        var intervals = new[] { 0, 1, 100 };

        foreach (var interval in intervals)
        {
            var connection = _connection;
            connection.KeepConnectionAliveInterval = interval;

            var result = FTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success, $"Interval: {interval}");
            Assert.IsFalse(result.ActionSkipped, $"Interval: {interval}");
            Assert.AreEqual(1, result.SuccessfulTransferCount, $"Interval: {interval}");
            Assert.AreEqual(0, result.TransferErrors.Count, $"Interval: {interval}");
            Assert.AreEqual(1, result.TransferredFileNames.Count(), $"Interval: {interval}");
            Assert.AreEqual(1, result.TransferredFilePaths.Count(), $"Interval: {interval}");
            Assert.IsTrue(result.UserResultMessage.Contains("files transferred"), $"Interval: {interval}");
            Assert.IsTrue(result.OperationsLog.Count < 4, $"Interval: {interval}");

            CleanUp();
            SetUp();
        }
    }

    [TestMethod]
    public void UploadFTPS_ConnectionTimeout()
    {
        var timeouts = new[] { 1, 100 };

        foreach (var timeout in timeouts)
        {
            var connection = _connection;
            connection.ConnectionTimeout = timeout;

            var result = FTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success, $"Timeout: {timeout}");
            Assert.IsFalse(result.ActionSkipped, $"Timeout: {timeout}");
            Assert.AreEqual(1, result.SuccessfulTransferCount, $"Timeout: {timeout}");
            Assert.AreEqual(0, result.TransferErrors.Count, $"Timeout: {timeout}");
            Assert.AreEqual(1, result.TransferredFileNames.Count(), $"Timeout: {timeout}");
            Assert.AreEqual(1, result.TransferredFilePaths.Count(), $"Timeout: {timeout}");
            Assert.IsTrue(result.UserResultMessage.Contains("files transferred"), $"Timeout: {timeout}");
            Assert.IsTrue(result.OperationsLog.Count < 4, $"Timeout: {timeout}");

            CleanUp();
            SetUp();
        }
    }

    [TestMethod]
    public void UploadFTPS_BufferSize()
    {
        var sizes = new[] { 1, 100, 1000, 10000 };

        foreach (var size in sizes)
        {
            var connection = _connection;
            connection.BufferSize = size;

            var result = FTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success, $"Size: {size}");
            Assert.IsFalse(result.ActionSkipped, $"Size: {size}");
            Assert.AreEqual(1, result.SuccessfulTransferCount, $"Size: {size}");
            Assert.AreEqual(0, result.TransferErrors.Count, $"Size: {size}");
            Assert.AreEqual(1, result.TransferredFileNames.Count(), $"Size: {size}");
            Assert.AreEqual(1, result.TransferredFilePaths.Count(), $"Size: {size}");
            Assert.IsTrue(result.UserResultMessage.Contains("files transferred"), $"Size: {size}");
            Assert.IsTrue(result.OperationsLog.Count < 4, $"Size: {size}");

            CleanUp();
            SetUp();
        }
    }

    [TestMethod]
    public void UploadFTPS_ValidateAnyCertificate()
    {
        var boo = new[] { true, false };

        foreach (var bo in boo)
        {
            var connection = _connection;
            connection.ValidateAnyCertificate = bo;

            var result = FTP.UploadFiles(_source, _destination, connection, _options, _info, new CancellationToken());
            Assert.IsTrue(result.Success, $"bo: {bo}");
            Assert.IsFalse(result.ActionSkipped, $"bo: {bo}");
            Assert.AreEqual(1, result.SuccessfulTransferCount, $"bo: {bo}");
            Assert.AreEqual(0, result.TransferErrors.Count, $"bo: {bo}");
            Assert.AreEqual(1, result.TransferredFileNames.Count(), $"bo: {bo}");
            Assert.AreEqual(1, result.TransferredFilePaths.Count(), $"bo: {bo}");
            Assert.IsTrue(result.UserResultMessage.Contains("files transferred"), $"bo: {bo}");
            Assert.IsTrue(result.OperationsLog.Count < 4, $"bo: {bo}");

            CleanUp();
            SetUp();
        }
    }

    [TestMethod]
    public void UploadFTPS_FileNameAfterTransfer_Missing()
    {
        var source = _source;
        source.FileNameAfterTransfer = "";

        var result = FTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());

        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.UserResultMessage.Contains("files transferred"));
        Assert.IsFalse(result.ActionSkipped);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
        Assert.AreEqual(0, result.TransferErrors.Count);
        Assert.AreEqual(1, result.TransferredFileNames.Count());
        Assert.AreEqual(1, result.TransferredFilePaths.Count());
        Assert.IsTrue(result.OperationsLog.Count < 4);
    }

    [TestMethod]
    public void UploadFTPS_DirectoryToMoveAfterTransfer_Missing()
    {
        var sourceOperations = new[] { SourceOperation.Move, SourceOperation.Delete, SourceOperation.Rename, SourceOperation.Nothing };

        foreach (var sourceOperation in sourceOperations)
        {
            var source = _source;
            source.DirectoryToMoveAfterTransfer = "DoesntExists";
            source.Operation = sourceOperation;

            var result = FTP.UploadFiles(source, _destination, _connection, _options, _info, new CancellationToken());

            if (sourceOperation is SourceOperation.Move)
            {
                Assert.IsFalse(result.Success, $"operation: {sourceOperation}");
                Assert.IsTrue(result.UserResultMessage.Contains("not find"), $"operation: {sourceOperation}");
                Assert.IsFalse(result.ActionSkipped, $"operation: {sourceOperation}");
                Assert.AreEqual(0, result.SuccessfulTransferCount, $"operation: {sourceOperation}");
                Assert.AreEqual(1, result.TransferErrors.Count, $"operation: {sourceOperation}");
                Assert.AreEqual(0, result.TransferredFileNames.Count(), $"operation: {sourceOperation}");
                Assert.AreEqual(0, result.TransferredFilePaths.Count(), $"operation: {sourceOperation}");
            }
            else
            {
                Assert.IsTrue(result.Success, $"operation: {sourceOperation}");
                Assert.IsTrue(result.UserResultMessage.Contains("files transferred"), $"operation: {sourceOperation}");
                Assert.IsFalse(result.ActionSkipped, $"operation: {sourceOperation}");
                Assert.AreEqual(1, result.SuccessfulTransferCount, $"operation: {sourceOperation}");
                Assert.AreEqual(0, result.TransferErrors.Count, $"operation: {sourceOperation}");
                Assert.AreEqual(1, result.TransferredFileNames.Count(), $"operation: {sourceOperation}");
                Assert.AreEqual(1, result.TransferredFilePaths.Count(), $"operation: {sourceOperation}");
            }
            Assert.IsTrue(result.OperationsLog.Count < 4, $"operation: {sourceOperation}");

            CleanUp();
            SetUp();
        }
    }

    [TestMethod]
    public void UploadFTPS_ThrowErrorOnFail()
    {
        var source = _source;
        source.FileName = "DoesntExists";

        var options = _options;
        options.ThrowErrorOnFail = true;

        var ex = Assert.ThrowsException<Exception>(() => FTP.UploadFiles(source, _destination, _connection, options, _info, new CancellationToken()));
        Assert.AreEqual(typeof(Exception), ex.GetType());
    }

    [TestMethod]
    public void UploadFTPS_PreserveLastModified()
    {
        var boo = new[] { true, false };

        foreach (var bo in boo)
        {
            var options = _options;
            options.PreserveLastModified = bo;

            var result = FTP.UploadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success, $"bo: {bo}");
            Assert.IsFalse(result.ActionSkipped, $"bo: {bo}");
            Assert.AreEqual(1, result.SuccessfulTransferCount, $"bo: {bo}");
            Assert.AreEqual(0, result.TransferErrors.Count, $"bo: {bo}");
            Assert.AreEqual(1, result.TransferredFileNames.Count(), $"bo: {bo}");
            Assert.AreEqual(1, result.TransferredFilePaths.Count(), $"bo: {bo}");
            Assert.IsTrue(result.UserResultMessage.Contains("files transferred"), $"bo: {bo}");
            Assert.IsTrue(result.OperationsLog.Count < 4, $"bo: {bo}");

            CleanUp();
            SetUp();
        }
    }

    [TestMethod]
    public void UploadFTPS_OperationLog()
    {
        var boo = new[] { true, false };

        foreach (var bo in boo)
        {
            var options = _options;
            options.OperationLog = bo;

            var result = FTP.UploadFiles(_source, _destination, _connection, options, _info, new CancellationToken());
            Assert.IsTrue(result.Success, $"bo: {bo}");
            Assert.IsFalse(result.ActionSkipped, $"bo: {bo}");
            Assert.AreEqual(1, result.SuccessfulTransferCount, $"bo: {bo}");
            Assert.AreEqual(0, result.TransferErrors.Count, $"bo: {bo}");
            Assert.AreEqual(1, result.TransferredFileNames.Count(), $"bo: {bo}");
            Assert.AreEqual(1, result.TransferredFilePaths.Count(), $"bo: {bo}");
            Assert.IsTrue(result.UserResultMessage.Contains("files transferred"), $"bo: {bo}");
            Assert.IsTrue(result.OperationsLog.Count < 4, $"bo: {bo}");

            CleanUp();
            SetUp();
        }
    }
}