using Frends.FTP.UploadFiles.Definitions;
using Frends.FTP.UploadFiles.TaskConfiguration;
using NUnit.Framework;
using System;
using System.IO;
using System.Security.Authentication;
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
        public void UploadFTPS_CorrectFingerprint()
        {
            // Setup
            CreateDummyFileInDummyDir("file1.txt");
            var source = new Source { Directory = _dir, FileName = "file1.txt" };
            var destination = new Destination { Directory = "/", Action = DestinationAction.Overwrite };
            
            // Our test certificate hashes:
            // SHA-256: 90:bc:7f:71:14:f5:c2:ad:03:46:d6:ff:75:d5:fe:12:ba:74:23:73:54:31:70:60:b4:8b:bd:8e:87:21:9c:16
            // SHA-1: d9:11:26:29:84:de:9c:c3:2a:35:18:a1:09:4c:d2:42:49:ea:5c:49
            var connection = new Connection
            {
                Address = _ftpHost,
                UserName = _ftpUsername,
                Password = _ftpPassword,
                Port = _ftpsPort,
                SslMode = FtpsSslMode.Explicit,
                UseFTPS = true,
                CertificateHashStringSHA1 = "D911262984DE9CC32A3518A1094CD24249EA5C49"
            };
            

            // Test and assert
            var result = FTP.UploadFiles(source, destination, connection, new Options(), new Info(), new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
        }

        [Test]
        public void UploadFTPS_IncorrectFingerprint()
        {
            // Setup
            CreateDummyFileInDummyDir("file1.txt");
            var source = new Source { Directory = _dir, FileName = "file1.txt" };
            var destination = new Destination { Directory = "/", Action = DestinationAction.Overwrite };
            
            // Our test certificate hashes:
            // SHA-256: 90:bc:7f:71:14:f5:c2:ad:03:46:d6:ff:75:d5:fe:12:ba:74:23:73:54:31:70:60:b4:8b:bd:8e:87:21:9c:16
            // SHA-1: d9:11:26:29:84:de:9c:c3:2a:35:18:a1:09:4c:d2:42:49:ea:5c:49
            var connection = new Connection
            {
                Address = _ftpHost,
                UserName = _ftpUsername,
                Password = _ftpPassword,
                Port = _ftpsPort,
                SslMode = FtpsSslMode.Explicit,
                UseFTPS = true,
                CertificateHashStringSHA1 = "nope"
            };
            

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