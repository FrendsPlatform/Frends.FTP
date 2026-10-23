using System;
using System.IO;
using System.Linq;
using System.Threading;
using FluentFTP;
using Frends.FTP.UploadFiles.Enums;
using Frends.FTP.UploadFiles.TaskConfiguration;
using NUnit.Framework;

namespace Frends.FTP.UploadFiles.Tests;

[TestFixture]
public class UploadFilesTests
{
    private readonly string file = "file1.txt";
    private string tempDir;

    private Source source = new();
    private Connection connection = new();
    private Destination destination = new();
    private Options options = new();
    private Info info = new();

    [SetUp]
    public void SetUp()
    {
        tempDir = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "../../../tempFiles");
        Directory.CreateDirectory(tempDir);
        File.WriteAllText(Path.Combine(tempDir, file), "test");
        Directory.CreateDirectory(Path.Combine(tempDir, "Done"));

        source = new()
        {
            Directory = tempDir,
            FileName = file,
            DirectoryToMoveAfterTransfer = Path.Combine(tempDir, "Done"),
            FileNameAfterTransfer = file + Guid.NewGuid(),
            NotFoundAction = default,
            FilePaths = tempDir,
            Operation = default,
        };

        connection = new()
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
            BufferSize = 4096,
            UseFTPS = false,
            SslMode = default,
            CertificateHashStringSHA1 = "D911262984DE9CC32A3518A1094CD24249EA5C49",
            EnableClientAuth = default,
            ValidateAnyCertificate = default,
            ClientCertificatePath = default,
            SecureDataChannel = false,
            VerifyOption = default,
            RetryAttempts = 0,
        };

        destination = new()
        {
            Directory = "/",
            Action = default,
            FileName = $"{file}-{Guid.NewGuid()}",
        };

        options = new()
        {
            CreateDestinationDirectories = true,
            OperationLog = true,
            PreserveLastModified = true,
            RenameDestinationFileDuringTransfer = true,
            RenameSourceFileBeforeTransfer = true,
            ThrowErrorOnFailure = true,
        };

        info = new()
        {
            ProcessUri = default,
            TaskExecutionID = default,
            TransferName = default,
            WorkDir = default,
        };
    }

    [TearDown]
    public void CleanUp()
    {
        if (Directory.Exists(tempDir))
            Directory.Delete(tempDir, true);

        using (var client = new FtpClient(Helpers.FtpHost, Helpers.FtpPort, Helpers.FtpUsername, Helpers.FtpPassword))
        {
            client.ConnectTimeout = 10;
            client.Connect();
            if (client.DirectoryExists("/"))
                client.DeleteDirectory("/");
            client.Disconnect();
            client.Dispose();
        }
    }

    [Test]
    public void UploadFTPS_IncorrectFingerprint()
    {
        var connection = this.connection;
        connection.SslMode = FtpsSslMode.Explicit;
        connection.UseFTPS = true;
        connection.CertificateHashStringSHA1 = "incorrect";

        // Test and assert
        var ex = Assert.Throws<AggregateException>(() => FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None));
        Assert.IsTrue(ex.Message.Contains("The remote certificate was rejected by the provided RemoteCertificateValidationCallback."));
    }

    [Test]
    public void UploadFTPS_IncorrectAuth()
    {
        var connectionA = connection;
        connectionA.Address = null;

        var connectionB = connection;
        connectionB.Port = 0;

        var connectionC = connection;
        connectionC.Password = null;

        var connectionD = connection;
        connectionD.UserName = null;

        var ex1 = Assert.Throws<NullReferenceException>(() => FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connectionA, options, CancellationToken.None));
        Assert.AreEqual(typeof(NullReferenceException), ex1.GetType());

        var ex2 = Assert.Throws<NullReferenceException>(() => FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connectionB, options, CancellationToken.None));
        Assert.AreEqual(typeof(NullReferenceException), ex2.GetType());

        var ex3 = Assert.Throws<NullReferenceException>(() => FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connectionC, options, CancellationToken.None));
        Assert.AreEqual(typeof(NullReferenceException), ex3.GetType());

        var ex4 = Assert.Throws<NullReferenceException>(() => FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connectionD, options, CancellationToken.None));
        Assert.AreEqual(typeof(NullReferenceException), ex4.GetType());
    }

    [Test]
    public void UploadFTP_UploadFile()
    {
        var result = FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None);
        Assert.IsTrue(result.Success);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
    }

    [Test]
    public void UploadFTPS_DestinationActions()
    {
        var destinationActions = new[] { DestinationAction.Error, DestinationAction.Overwrite, DestinationAction.Append };

        foreach (var destinationAction in destinationActions)
        {
            var destination = this.destination;
            destination.Action = destinationAction;

            var result = FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None);
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

    [Test]
    public void UploadFTPS_FtpMode()
    {
        var ftpModes = new[] { FtpMode.Active, FtpMode.Passive };

        options.ThrowErrorOnFailure = false;

        foreach (var ftpMode in ftpModes)
        {
            var connection = this.connection;
            connection.Mode = ftpMode;

            var result = FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None);
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

    [Test]
    public void UploadFTPS_FtpsSslModes()
    {
        var ftpsSslModes = new[] { FtpsSslMode.None, FtpsSslMode.Explicit, FtpsSslMode.Auto };

        foreach (var ftpsSslMode in ftpsSslModes)
        {
            var connection = this.connection;
            connection.SslMode = ftpsSslMode;
            connection.UseFTPS = true;

            var result = FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None);
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

    [Test]
    public void UploadFTPS_SecureDataChannel()
    {
        var boo = new[] { true, false };

        foreach (var bo in boo)
        {
            var connection = this.connection;
            connection.SecureDataChannel = bo;

            var result = FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None);
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

    [Test]
    public void UploadFTPS_FtpTransportTypes()
    {
        var ftpTransportTypes = new[] { FtpTransportType.Binary, FtpTransportType.Ascii };

        foreach (var ftpTransportType in ftpTransportTypes)
        {
            var connection = this.connection;
            connection.TransportType = ftpTransportType;

            var result = FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None);
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

    [Test]
    public void UploadFTPS_NotFoundActions()
    {
        var notFoundActions = new[] { SourceNotFoundAction.Error, SourceNotFoundAction.Ignore, SourceNotFoundAction.Info };

        foreach (var notFoundAction in notFoundActions)
        {
            options.ThrowErrorOnFailure = false;
            var source = this.source;
            source.NotFoundAction = notFoundAction;
            source.FileName = string.Empty;

            var result = FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None);

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

    [Test]
    public void UploadFTPS_SourceOperations()
    {
        var sourceOperations = new[] { SourceOperation.Move, SourceOperation.Delete, SourceOperation.Rename, SourceOperation.Nothing };

        foreach (var sourceOperation in sourceOperations)
        {
            var source = this.source;
            source.Operation = sourceOperation;

            var result = FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None);
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

    [Test]
    public void UploadFTPS_FileEncodings()
    {
        var fileEncodings = new[] { "UTF-8", "ASCII", "foobar123", string.Empty };

        foreach (var fileEncoding in fileEncodings)
        {
            var connection = this.connection;
            connection.Encoding = fileEncoding;

            if (connection.Encoding.Equals("foobar123"))
            {
                var ex = Assert.Throws<ArgumentException>(() => FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None));
                Assert.IsTrue(ex.Message.Contains("is not a supported encoding name"), $"FileEncoding: {fileEncoding}");
            }
            else
            {
                var result = FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None);
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

    [Test]
    public void UploadFTPS_Port()
    {
        var ports = new[] { 0, 21, 210 };

        foreach (var port in ports)
        {
            options.ThrowErrorOnFailure = false;
            var connection = this.connection;
            connection.Port = port;

            var result = FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None);
            if (port == 210)
            {
                Assert.IsFalse(result.Success, $"Port: {port}");
                Assert.IsTrue(result.ActionSkipped, $"Port: {port}");
                Assert.AreEqual(0, result.SuccessfulTransferCount, $"Port: {port}");
                Assert.AreEqual(0, result.TransferErrors.Count, $"Port: {port}");
                Assert.AreEqual(0, result.TransferredFileNames.Count(), $"Port: {port}");
                Assert.AreEqual(0, result.TransferredFilePaths.Count(), $"Port: {port}");
                Assert.IsTrue(result.UserResultMessage.Contains("Unable to establish the socket: No such host is known."), $"Port: {port}");
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
            }

            CleanUp();
            SetUp();
        }
    }

    [Test]
    public void UploadFTPS_KeepConnectionAliveInterval()
    {
        var intervals = new[] { 1, 100, 0 };

        foreach (var interval in intervals)
        {
            options.ThrowErrorOnFailure = true;
            var connection = this.connection;
            connection.KeepConnectionAliveInterval = interval;

            var result = FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None);
            Assert.IsTrue(result.Success, $"Interval: {interval}");
            Assert.IsFalse(result.ActionSkipped, $"Interval: {interval}");
            Assert.AreEqual(1, result.SuccessfulTransferCount, $"Interval: {interval}");
            Assert.AreEqual(0, result.TransferErrors.Count, $"Interval: {interval}");
            Assert.AreEqual(1, result.TransferredFileNames.Count(), $"Interval: {interval}");
            Assert.AreEqual(1, result.TransferredFilePaths.Count(), $"Interval: {interval}");
            Assert.IsTrue(result.UserResultMessage.Contains("files transferred"), $"Interval: {interval}");

            CleanUp();
            SetUp();
        }
    }

    [Test]
    public void UploadFTPS_ConnectionTimeout()
    {
        var timeouts = new[] { 10, 15, 50, 100 };

        foreach (var timeout in timeouts)
        {
            var connection = this.connection;
            connection.ConnectionTimeout = timeout;

            var result = FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None);
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

    [Test]
    public void UploadFTPS_BufferSize()
    {
        var sizes = new[] { 1, 100, 1000, 10000 };

        foreach (var size in sizes)
        {
            var connection = this.connection;
            connection.BufferSize = size;

            var result = FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None);
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

    [Test]
    public void UploadFTPS_ValidateAnyCertificate()
    {
        var boo = new[] { true, false };

        foreach (var bo in boo)
        {
            var connection = this.connection;
            connection.ValidateAnyCertificate = bo;

            var result = FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None);
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

    [Test]
    public void UploadFTPS_FileNameAfterTransfer_Missing()
    {
        var source = this.source;
        source.FileNameAfterTransfer = string.Empty;

        var result = FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None);

        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.UserResultMessage.Contains("files transferred"));
        Assert.IsFalse(result.ActionSkipped);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
        Assert.AreEqual(0, result.TransferErrors.Count);
        Assert.AreEqual(1, result.TransferredFileNames.Count());
        Assert.AreEqual(1, result.TransferredFilePaths.Count());
        Assert.IsTrue(result.OperationsLog.Count < 4);
    }

    [Test]
    public void UploadFTPS_DirectoryToMoveAfterTransfer_Missing()
    {
        var sourceOperations = new[] { SourceOperation.Move, SourceOperation.Delete, SourceOperation.Rename, SourceOperation.Nothing };

        options.ThrowErrorOnFailure = false;

        foreach (var sourceOperation in sourceOperations)
        {
            var source = this.source;
            source.DirectoryToMoveAfterTransfer = "DoesntExists";
            source.Operation = sourceOperation;

            var result = FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None);

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

    [Test]
    public void UploadFTPS_ThrowErrorOnFailure()
    {
        var source = this.source;
        source.FileName = "DoesntExists";

        var options = this.options;
        options.ThrowErrorOnFailure = true;

        var ex = Assert.Throws<Exception>(() => FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None));
        Assert.AreEqual(typeof(Exception), ex.GetType());
    }

    [Test]
    public void UploadFTPS_PreserveLastModified()
    {
        var boo = new[] { true, false };

        foreach (var bo in boo)
        {
            var options = this.options;
            options.PreserveLastModified = bo;

            var result = FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None);
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

    [Test]
    public void UploadFTPS_OperationLog()
    {
        var boo = new[] { true, false };

        foreach (var bo in boo)
        {
            var options = this.options;
            options.OperationLog = bo;

            var result = FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, CancellationToken.None);
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

    [Test]
    public void UploadFTPS_CurrentUserHasNoCertificates()
    {
        var connection = new Connection
        {
            Address = Helpers.FtpHost,
            UserName = Helpers.FtpUsername,
            Password = Helpers.FtpPassword,
            Port = Helpers.FtpsPort,
            SslMode = FtpsSslMode.Explicit,
            EnableClientAuth = true,
            UseFTPS = true,
            ValidateAnyCertificate = false,
            CertificateHashStringSHA1 = string.Empty,
            ClientCertificatePath = string.Empty,
            ClientCertificateName = string.Empty,
            ClientCertificateThumbprint = string.Empty,
        };

        var ex = Assert.Throws<AggregateException>(() =>
        {
            FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = new Info() }, connection, new Options(), default);
        });

        Assert.IsTrue(ex.Message.Contains("The remote certificate was rejected by the provided RemoteCertificateValidationCallback."));
    }

    [Test]
    public void UploadFTP_TestVerifyOptionsRetry()
    {
        var path = Helpers.CreateLargeDummyZipFiles(tempDir, 10);

        var source = new Source
        {
            FilePaths = new string[] { path },
            Operation = SourceOperation.Nothing,
            NotFoundAction = SourceNotFoundAction.Error,
        };

        var destination = new Destination
        {
            Directory = tempDir,
            FileName = "test.zip",
            Action = DestinationAction.Overwrite,
        };

        connection.VerifyOption = VerifyOptions.Retry;
        connection.RetryAttempts = 5;

        var errors = 0;
        var success = 0;

        for (var i = 0; i < 10; i++)
        {
            try
            {
                var result = FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, default);
                var sourcePath = Path.Combine(destination.Directory, destination.FileName);
                var testfile = Helpers.GetFileFromFtp(Path.GetDirectoryName(sourcePath), Path.GetFileName(sourcePath));
                Assert.IsTrue(Helpers.ExtractLargeZipFile(testfile, tempDir));
                success++;
            }
            catch (Exception ex)
            {
                errors++;
                Console.WriteLine(ex.Message);
            }
        }

        Console.WriteLine($"Errors: {errors}\nSuccessful: {success}");

        Assert.AreEqual(10, success);
        Assert.AreEqual(0, errors);
    }

    [Test]
    public void UploadFTP_TestVerifyOptionsThrow()
    {
        var path = Helpers.CreateLargeDummyZipFiles(tempDir, 10);

        var source = new Source
        {
            FilePaths = new string[] { path },
            Operation = SourceOperation.Nothing,
            NotFoundAction = SourceNotFoundAction.Error,
        };

        var destination = new Destination
        {
            Directory = tempDir,
            FileName = "test.zip",
            Action = DestinationAction.Overwrite,
        };

        connection.VerifyOption = VerifyOptions.Throw;

        for (int i = 0; i < 10; i++)
        {
            try
            {
                var result = FTP.UploadFiles(new Input { Source = source, Destination = destination, Info = info }, connection, options, default);
                Assert.IsTrue(result.Success);
                var sourcePath = Path.Combine(destination.Directory, destination.FileName);
                var testfile = Helpers.GetFileFromFtp(Path.GetDirectoryName(sourcePath), Path.GetFileName(sourcePath));
                Assert.IsTrue(Helpers.ExtractLargeZipFile(testfile, tempDir));
            }
            catch (Exception ex)
            {
                Assert.IsTrue(ex.Message.Contains("Transferred file size or Checksum was different from the source file."));
                break;
            }
        }
    }
}
