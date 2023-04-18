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

        private readonly Source _source = new()
        {
            Directory = default,
            FileName = default,
            DirectoryToMoveAfterTransfer = default,
            FileNameAfterTransfer = default,
            NotFoundAction = default,
            FilePaths = default,
            Operation = default
        };

        private readonly Connection _connection = new()
        {
            Address = default,
            UserName = default,
            Password = default,
            Port = default,
            EnableClientAuth = default,
            KeepConnectionAliveInterval = default,
            ValidateAnyCertificate = default,
            BufferSize = default,
            CertificateHashStringSHA1 = default,
            ClientCertificatePath = default,
            ConnectionTimeout = default,
            Encoding = default,
            Mode = default,
            SecureDataChannel = default,
            SslMode = default,
            TransportType = default,
            UseFTPS = default,
        };

        private readonly Destination _destination = new()
        {
            Directory = default,
            Action = default,
            FileName = default,
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
            Directory.Delete(_dir, true);
        }

        [Test]
        public void UploadFTP()
        {
            var destinationActions = new[] { DestinationAction.Error, DestinationAction.Overwrite, DestinationAction.Append };

            foreach (var destinationAction in destinationActions)
            {
                // Setup
                SetUp();
                CreateDummyFileInDummyDir("file1.txt");
                var source = _source;
                source.Directory = _dir;
                source.FileName = "file1.txt";

                var destination = _destination;
                destination.Directory = "/";
                destination.Action = destinationAction;
                destination.FileName = "fileName";

                var connection = _connection;
                connection.Address = Helpers.FtpHost;
                connection.UserName = Helpers.FtpUsername;
                connection.Password = Helpers.FtpPassword;
                connection.Port = Helpers.FtpPort;

                // Test and assert
                var result = FTP.UploadFiles(source, destination, connection, new Options(), new Info(), new CancellationToken());
                Assert.IsTrue(result.Success);
                Assert.AreEqual(1, result.SuccessfulTransferCount);
                TearDown();
            }
        }

        [Test]
        public void UploadFTPS_CorrectFingerprint()
        {
            var destinationActions = new[] { DestinationAction.Error, DestinationAction.Overwrite, DestinationAction.Append };
            var ftpsSslModes = new[] { FtpsSslMode.None, FtpsSslMode.Explicit, FtpsSslMode.Explicit, FtpsSslMode.None, FtpsSslMode.Auto };
            foreach (var destinationAction in destinationActions)
            {
                SetUp();
                // Setup
                CreateDummyFileInDummyDir("file1.txt");
                var source = _source;
                source.Directory = _dir;
                source.FileName = "file1.txt";


                var destination = _destination;
                destination.Directory = "/";
                destination.Action = DestinationAction.Overwrite;

                foreach (var ftpsSslMode in ftpsSslModes)
                {
                    // Our test certificate hashes:
                    // SHA-256: 90:bc:7f:71:14:f5:c2:ad:03:46:d6:ff:75:d5:fe:12:ba:74:23:73:54:31:70:60:b4:8b:bd:8e:87:21:9c:16
                    // SHA-1: d9:11:26:29:84:de:9c:c3:2a:35:18:a1:09:4c:d2:42:49:ea:5c:49
                    var connection = _connection;
                    connection.Address = Helpers.FtpHost;
                    connection.UserName = Helpers.FtpUsername;
                    connection.Password = Helpers.FtpPassword;
                    connection.Port = Helpers.FtpPort;
                    connection.SslMode = ftpsSslMode;
                    connection.UseFTPS = true;
                    connection.CertificateHashStringSHA1 = "D911262984DE9CC32A3518A1094CD24249EA5C49";

                    // Test and assert
                    var result = FTP.UploadFiles(source, destination, connection, new Options(), new Info(), new CancellationToken());
                    Assert.IsTrue(result.Success);
                    Assert.AreEqual(1, result.SuccessfulTransferCount);
                    TearDown();
                }
            }
        }

        [Test]
        public void UploadFTPS_IncorrectFingerprint()
        {
            var destinationActions = new[] { DestinationAction.Error, DestinationAction.Overwrite, DestinationAction.Append };
            var ftpsSslModes = new[] { FtpsSslMode.None, FtpsSslMode.Explicit, FtpsSslMode.Explicit, FtpsSslMode.None, FtpsSslMode.Auto };
            foreach (var destinationAction in destinationActions)
            {
                SetUp();
                // Setup
                CreateDummyFileInDummyDir("file1.txt");
                var source = _source;
                source.Directory = _dir;
                source.FileName = "file1.txt";


                var destination = _destination;
                destination.Directory = "/";
                destination.Action = DestinationAction.Overwrite;

                foreach (var ftpsSslMode in ftpsSslModes)
                {
                    // Our test certificate hashes:
                    // SHA-256: 90:bc:7f:71:14:f5:c2:ad:03:46:d6:ff:75:d5:fe:12:ba:74:23:73:54:31:70:60:b4:8b:bd:8e:87:21:9c:16
                    // SHA-1: d9:11:26:29:84:de:9c:c3:2a:35:18:a1:09:4c:d2:42:49:ea:5c:49
                    var connection = _connection;
                    connection.Address = Helpers.FtpHost;
                    connection.UserName = Helpers.FtpUsername;
                    connection.Password = Helpers.FtpPassword;
                    connection.Port = Helpers.FtpPort;
                    connection.SslMode = ftpsSslMode;
                    connection.UseFTPS = true;
                    connection.CertificateHashStringSHA1 = "nope";

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
    }
}