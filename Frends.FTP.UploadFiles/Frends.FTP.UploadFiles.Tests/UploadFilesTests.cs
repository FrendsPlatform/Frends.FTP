using Frends.FTP.UploadFiles.Enums;
using Frends.FTP.UploadFiles.TaskConfiguration;
using NUnit.Framework;
using System;
using System.IO;
using System.Security.Authentication;
using System.Threading;

namespace Frends.FTP.UploadFiles.Tests
{
    [TestFixture]
    public class UploadFilesTests
    {
        private string _dir;

        private Source _source = new()
        {
            Directory = default,
            FileName = default,
            DirectoryToMoveAfterTransfer = default,
            FileNameAfterTransfer = default,
            NotFoundAction = default,
            FilePaths = default,
            Operation = default
        };

        private Connection _connection = new()
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
            CertificateHashStringSHA1 = default,
            EnableClientAuth = default,
            ValidateAnyCertificate = default,
            ClientCertificatePath = default,
            SecureDataChannel = false,
        };

        private Destination _destination = new()
        {
            Directory = default,
            Action = default,
            FileName = default,
        };

        private Options _options = new()
        {
            CreateDestinationDirectories = default,
            OperationLog = default,
            PreserveLastModified = default,
            RenameDestinationFileDuringTransfer = default,
            RenameSourceFileBeforeTransfer = default,
            ThrowErrorOnFail = default
        };

        private Info _info = new()
        {
            ProcessUri = default,
            TaskExecutionID = default,
            TransferName = default,
            WorkDir = default,
        };

        private void CreateDummyFileInDummyDir(string fileName)
        {
            File.WriteAllText(Path.Combine(_dir, fileName), "test");
            File.WriteAllText(Path.Combine(_dir, fileName + "empty"), null);
        }

        [SetUp]
        public void SetUp()
        {
            _dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
            Directory.CreateDirectory(_dir);
        }

        [TearDown]
        public void TearDown()
        {
            if (Directory.Exists(_dir))
                Directory.Delete(_dir, true);
        }

        [Test]
        public void UploadFTP()
        {
            var destinationActions = new[] { DestinationAction.Error, DestinationAction.Overwrite, DestinationAction.Append };

            foreach (var destinationAction in destinationActions)
            {
                SetUp();
                var fileName = @$"file{Guid.NewGuid()}.txt";
                CreateDummyFileInDummyDir(fileName);

                var source = _source;
                source.Directory = _dir;
                source.FileName = fileName;

                var destination = _destination;
                destination.Directory = "/";
                destination.Action = destinationAction;

                // Test and assert
                var result = FTP.UploadFiles(source, destination, _connection, _options, _info, new CancellationToken());
                Assert.IsTrue(result.Success);
                Assert.AreEqual(1, result.SuccessfulTransferCount);
                TearDown();
            }
        }

        [Test]
        public void UploadFTPS_CorrectFingerprint_FileExists()
        {
            var destinationActions = new[] { DestinationAction.Error, DestinationAction.Overwrite, DestinationAction.Append };
            var ftpsSslModes = new[] { FtpsSslMode.None, FtpsSslMode.Explicit, FtpsSslMode.Auto };
            var ftpTransportTypes = new[] { FtpTransportType.Binary, FtpTransportType.Ascii };
            var notFoundActions = new[] { SourceNotFoundAction.Error, SourceNotFoundAction.Ignore, SourceNotFoundAction.Info };
            var bools = new[] { false, true };
            var sourceOperations = new[] { SourceOperation.Move, SourceOperation.Delete, SourceOperation.Rename, SourceOperation.Nothing };
            var fileEncodings = new[] { "UTF-8", "ASCII", "foobar123", string.Empty };

            foreach (var bo in bools)
                foreach (var ftpTransportType in ftpTransportTypes)
                    foreach (var destinationAction in destinationActions)
                        foreach (var notFoundAction in notFoundActions)
                            foreach (var ftpsSslMode in ftpsSslModes)
                                foreach (var sourceOperation in sourceOperations)
                                    foreach (var fileEncoding in fileEncodings)
                                    {
                                        Random random = new();
                                        var randomNumber = random.Next(1, 100);
                                        SetUp();
                                        var fileName = @$"file{Guid.NewGuid()}.txt";
                                        CreateDummyFileInDummyDir(fileName);

                                        var source = _source;
                                        source.Directory = _dir;
                                        source.FileName = fileName;
                                        source.NotFoundAction = notFoundAction;
                                        source.Operation = sourceOperation;
                                        source.FileNameAfterTransfer = fileName + "AfterTransfer";
                                        source.DirectoryToMoveAfterTransfer = _dir;
                                        source.FilePaths = _dir;

                                        var destination = _destination;
                                        destination.Directory = "/";
                                        destination.Action = destinationAction;
                                        destination.FileName = fileName + "EDIT";

                                        var connection = _connection;
                                        connection.SslMode = ftpsSslMode;
                                        connection.UseFTPS = true;
                                        connection.CertificateHashStringSHA1 = "D911262984DE9CC32A3518A1094CD24249EA5C49";
                                        connection.TransportType = ftpTransportType;
                                        connection.ConnectionTimeout = randomNumber;
                                        connection.KeepConnectionAliveInterval = randomNumber;
                                        connection.Encoding = fileEncoding;
                                        connection.BufferSize = randomNumber + 1000;
                                         
                                        var options = _options;
                                        options.ThrowErrorOnFail = bo;
                                        options.PreserveLastModified = bo;
                                        options.RenameSourceFileBeforeTransfer = bo;
                                        options.RenameDestinationFileDuringTransfer = bo;
                                        options.CreateDestinationDirectories = bo;

                                        var info = _info;
                                        info.TransferName = fileName + "transfername";
                                        info.WorkDir = _dir;
                                        info.TaskExecutionID = "123";

                                        if (connection.Encoding.Equals("foobar123"))
                                        {
                                            var ex = Assert.Throws<ArgumentException>(() => FTP.UploadFiles(source, destination, connection, options, info, new CancellationToken()));
                                            Assert.IsTrue(ex.Message.Contains("is not a supported encoding name"));
                                        }
                                        else
                                        {
                                            var result = FTP.UploadFiles(source, destination, connection, options, info, new CancellationToken());
                                            Assert.IsTrue(result.Success);
                                            Assert.AreEqual(1, result.SuccessfulTransferCount);
                                        }
                                        TearDown();
                                    }
        }

        [Test]
        public void UploadFTPS_CorrectFingerprint_FileDoesntExists()
        {
            var destinationActions = new[] { DestinationAction.Error, DestinationAction.Overwrite, DestinationAction.Append };
            var ftpsSslModes = new[] { FtpsSslMode.None, FtpsSslMode.Explicit, FtpsSslMode.Auto };
            var ftpTransportTypes = new[] { FtpTransportType.Binary, FtpTransportType.Ascii };
            var notFoundActions = new[] { SourceNotFoundAction.Error, SourceNotFoundAction.Ignore, SourceNotFoundAction.Info };
            var bools = new[] { true, false };
            var sourceOperations = new[] { SourceOperation.Move, SourceOperation.Delete, SourceOperation.Rename, SourceOperation.Nothing };
            var fileEncodings = new[] { "UTF-8", "ASCII", "foobar123", string.Empty };

            foreach (var bo in bools)
                foreach (var notFoundAction in notFoundActions)
                    foreach (var ftpTransportType in ftpTransportTypes)
                        foreach (var destinationAction in destinationActions)
                            foreach (var ftpsSslMode in ftpsSslModes)
                                foreach (var sourceOperation in sourceOperations)
                                    foreach (var fileEncoding in fileEncodings)
                                    {
                                        Random random = new();
                                        var randomNumber = random.Next(1, 100);
                                        SetUp();
                                        var fileName = "no.file";

                                        var source = _source;
                                        source.Directory = _dir;
                                        source.FileName = fileName;
                                        source.NotFoundAction = notFoundAction;
                                        source.Operation = sourceOperation;
                                        source.FileNameAfterTransfer = fileName + "AfterTransfer";
                                        source.DirectoryToMoveAfterTransfer = _dir;

                                        var destination = _destination;
                                        destination.Directory = "/";
                                        destination.Action = destinationAction;
                                        destination.FileName = fileName + "EDIT";

                                        var connection = _connection;
                                        connection.SslMode = ftpsSslMode;
                                        connection.UseFTPS = true;
                                        connection.CertificateHashStringSHA1 = "D911262984DE9CC32A3518A1094CD24249EA5C49";
                                        connection.TransportType = ftpTransportType;
                                        connection.ConnectionTimeout = randomNumber;
                                        connection.KeepConnectionAliveInterval = randomNumber;
                                        connection.Encoding = fileEncoding;
                                        connection.BufferSize = randomNumber + 1000;

                                        var options = _options;
                                        options.ThrowErrorOnFail = bo;
                                        options.PreserveLastModified = bo;
                                        options.RenameSourceFileBeforeTransfer = bo;
                                        options.RenameDestinationFileDuringTransfer = bo;
                                        options.CreateDestinationDirectories = bo;

                                        var info = _info;
                                        info.TransferName = fileName + "transfername";
                                        info.WorkDir = _dir;
                                        info.TaskExecutionID = "123";

                                        if (notFoundAction is SourceNotFoundAction.Error)
                                        {
                                            if (options.ThrowErrorOnFail)
                                            {
                                                var ex = Assert.Throws<Exception>(() => FTP.UploadFiles(source, destination, connection, options, info, new CancellationToken()));
                                                Assert.IsTrue(ex.Message.Contains("No source files found from directory"));
                                            }
                                            else
                                            {
                                                var result = FTP.UploadFiles(source, destination, connection, options, info, new CancellationToken());
                                                Assert.IsFalse(result.Success);
                                                Assert.AreEqual(0, result.SuccessfulTransferCount);
                                                Assert.IsTrue(result.UserResultMessage.Contains("No source files found from"));
                                            }
                                        }
                                        else if (notFoundAction is SourceNotFoundAction.Ignore)
                                        {
                                            var result = FTP.UploadFiles(source, destination, connection, options, info, new CancellationToken());
                                            Assert.IsTrue(result.Success);
                                            Assert.AreEqual(0, result.SuccessfulTransferCount);
                                            Assert.IsTrue(result.UserResultMessage.Contains("No source files found from"));
                                        }
                                        else if (notFoundAction is SourceNotFoundAction.Info)
                                        {
                                            var result = FTP.UploadFiles(source, destination, connection, options, info, new CancellationToken());
                                            Assert.IsTrue(result.Success);
                                            Assert.AreEqual(0, result.SuccessfulTransferCount);
                                            Assert.IsTrue(result.UserResultMessage.Contains("No source files found from"));
                                        }
                                        TearDown();
                                    }
        }

        /*
        [Test]
        public void UploadFTPS_IncorrectFingerprint()
        {
            var fileName = @$"file{Guid.NewGuid()}.txt";
            CreateDummyFileInDummyDir(fileName);

            var source = _source;
            source.Directory = _dir;
            source.FileName = fileName;

            var destination = _destination;
            destination.Directory = "/";

            var connection = _connection;
            connection.SslMode = FtpsSslMode.Explicit;
            connection.UseFTPS = true;
            connection.CertificateHashStringSHA1 = "incorrect";

            // Test and assert
            var ex = Assert.Throws<AggregateException>(() => FTP.UploadFiles(source, destination, connection, new Options(), new Info(), new CancellationToken()));
            Assert.AreEqual(1, ex.InnerExceptions.Count);
            Assert.AreEqual(typeof(AuthenticationException), ex.InnerExceptions[0].GetType());
        }

        [Test]
        public void UploadFTPS_IncorrectAuth()
        {
            var fileName = @$"file{Guid.NewGuid()}.txt";
            CreateDummyFileInDummyDir(fileName);

            var source = _source;
            source.Directory = _dir;
            source.FileName = fileName;

            var destination = _destination;
            destination.Directory = "/";

            var connectionA = _connection;
            connectionA.Address = null;

            var connectionB = _connection;
            connectionB.Port = 0;

            var connectionC = _connection;
            connectionC.Password = null;

            var connectionD = _connection;
            connectionD.UserName = null;

            var ex1 = Assert.Throws<NullReferenceException>(() => FTP.UploadFiles(source, destination, connectionA, new Options(), new Info(), new CancellationToken()));
            Assert.AreEqual(typeof(NullReferenceException), ex1.GetType());

            var ex2 = Assert.Throws<NullReferenceException>(() => FTP.UploadFiles(source, destination, connectionB, new Options(), new Info(), new CancellationToken()));
            Assert.AreEqual(typeof(NullReferenceException), ex2.GetType());

            var ex3 = Assert.Throws<NullReferenceException>(() => FTP.UploadFiles(source, destination, connectionC, new Options(), new Info(), new CancellationToken()));
            Assert.AreEqual(typeof(NullReferenceException), ex3.GetType());

            var ex4 = Assert.Throws<NullReferenceException>(() => FTP.UploadFiles(source, destination, connectionD, new Options(), new Info(), new CancellationToken()));
            Assert.AreEqual(typeof(NullReferenceException), ex4.GetType());
        }
        */
    }
}