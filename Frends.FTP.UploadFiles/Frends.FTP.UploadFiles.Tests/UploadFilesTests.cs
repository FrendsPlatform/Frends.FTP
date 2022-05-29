using Frends.FTP.UploadFiles.Definitions;
using Frends.FTP.UploadFiles.TaskConfiguration;
using NUnit.Framework;
using System;
using System.IO;
using System.Threading;
using System.Threading.Tasks;
using Frends.FTP.UploadFiles.Enums;

namespace Frends.FTP.UploadFiles.Tests
{
    [TestFixture]
    public class UploadFilesTests
    {
        private readonly string _ftpHost = Environment.GetEnvironmentVariable("FTP_HOST");
        private readonly int _ftpPort = int.Parse(Environment.GetEnvironmentVariable("FTP_PORT"));
        private readonly int _ftpsPort = int.Parse(Environment.GetEnvironmentVariable("FTPS_PORT"));
        private readonly string _ftpUsername = Environment.GetEnvironmentVariable("FTP_USERNAME");
        private readonly string _ftpPassword = Environment.GetEnvironmentVariable("FTP_PASSWORD");

        private string _dir;

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
            // Setup
            CreateDummyFileInDummyDir("file1.txt");
            var source = new Source { Directory = _dir, FileName = "file1.txt" };
            var destination = new Destination { Directory = "/", Action = DestinationAction.Overwrite };
            var connection = new Connection {  Address = _ftpHost, UserName = _ftpUsername, Password = _ftpPassword, Port = _ftpPort };

            // Test and assert
            var result = FTP.UploadFiles(source, destination, connection, new Options(), new Info(), new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFTPS()
        {
            // Setup
            CreateDummyFileInDummyDir("file1.txt");
            var source = new Source { Directory = _dir, FileName = "file1.txt" };
            var destination = new Destination { Directory = "/", Action = DestinationAction.Overwrite };
            var connection = new Connection
            {
                Address = _ftpHost,
                UserName = _ftpUsername,
                Password = _ftpPassword,
                Port = _ftpsPort,
                SslMode = FtpsSslMode.Implicit,
                UseFTPS = true
            };

            // Test and assert
            var result = FTP.UploadFiles(source, destination, connection, new Options(), new Info(), new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }
    }
}