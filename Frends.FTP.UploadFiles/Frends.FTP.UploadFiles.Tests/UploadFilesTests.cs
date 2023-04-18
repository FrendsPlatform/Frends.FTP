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
        public void UploadFTPS_CorrectFingerprint()
        {
            var destinationActions = new[] { DestinationAction.Error, DestinationAction.Overwrite, DestinationAction.Append };
            var ftpsSslModes = new[] { FtpsSslMode.None, FtpsSslMode.Explicit, FtpsSslMode.Auto };
            foreach (var destinationAction in destinationActions)
            {

                foreach (var ftpsSslMode in ftpsSslModes)
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

                    // Our test certificate hashes:
                    // SHA-256: 90:bc:7f:71:14:f5:c2:ad:03:46:d6:ff:75:d5:fe:12:ba:74:23:73:54:31:70:60:b4:8b:bd:8e:87:21:9c:16
                    // SHA-1: d9:11:26:29:84:de:9c:c3:2a:35:18:a1:09:4c:d2:42:49:ea:5c:49
                    var connection = _connection;
                    connection.SslMode = ftpsSslMode;
                    connection.UseFTPS = true;
                    connection.CertificateHashStringSHA1 = "D911262984DE9CC32A3518A1094CD24249EA5C49";

                    // Test and assert
                    var result = FTP.UploadFiles(source, destination, connection, _options, _info, new CancellationToken());
                    Assert.IsTrue(result.Success);
                    Assert.AreEqual(1, result.SuccessfulTransferCount);
                    TearDown();
                }
            }
        }

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

            // Our test certificate hashes:
            // SHA-256: 90:bc:7f:71:14:f5:c2:ad:03:46:d6:ff:75:d5:fe:12:ba:74:23:73:54:31:70:60:b4:8b:bd:8e:87:21:9c:16
            // SHA-1: d9:11:26:29:84:de:9c:c3:2a:35:18:a1:09:4c:d2:42:49:ea:5c:49
            var connection = _connection;
            connection.SslMode = FtpsSslMode.Explicit;
            connection.UseFTPS = true;
            connection.CertificateHashStringSHA1 = "incorrect";

            // Test and assert
            var ex = Assert.Throws<AggregateException>(() =>
            {
                var result = FTP.UploadFiles(source, destination, connection, new Options(), new Info(),
                    new CancellationToken());
            });

            Assert.AreEqual(1, ex.InnerExceptions.Count);
            Assert.AreEqual(typeof(AuthenticationException), ex.InnerExceptions[0].GetType());
        }
    }
}