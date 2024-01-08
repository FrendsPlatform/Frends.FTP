using Frends.FTP.DownloadFiles.TaskConfiguration;
using NUnit.Framework;
using System;
using System.IO;
using System.Security.Authentication;
using System.Threading;
using Frends.FTP.DownloadFiles.Enums;
using Frends.FTP.DownloadFiles.Tests.Lib;

namespace Frends.FTP.DownloadFiles.Tests
{
    [TestFixture]
    public class DownloadFilesTests : DownloadFilesTestBase
    {
        [Test]
        public void DownloadFTP()
        {
            // Setup
            FtpHelper.CreateFileOnFTP(FtpDir, "file1.txt");
            var source = new Source { Directory = FtpDir, FileName = "file1.txt" };
            var destination = new Destination { Directory = LocalDirFullPath, Action = DestinationAction.Overwrite };
            var connection = new Connection
            {
                Address = FtpHelper.FtpHost,
                UserName = FtpHelper.FtpUsername,
                Password = FtpHelper.FtpPassword,
                Port = FtpHelper.FtpPort
            };

            // Test and assert
            var result = FTP.DownloadFiles(source, destination, connection, new Options(), new Info(), new CancellationToken());
            Assert.IsTrue(result.Success, result.UserResultMessage);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.IsTrue(LocalFileExists("file1.txt"), result.UserResultMessage);
        }

        [Test]
        public void DownloadFTP_DestinationFileExists_ExceptionNotThrown()
        {
            // Setup
            FtpHelper.CreateFileOnFTP(FtpDir, "file1.txt");
            var source = new Source { Directory = FtpDir, FileName = "file1.txt" };
            var destination = new Destination { Directory = LocalDirFullPath, Action = DestinationAction.Error };
            var connection = new Connection
            {
                Address = FtpHelper.FtpHost,
                UserName = FtpHelper.FtpUsername,
                Password = FtpHelper.FtpPassword,
                Port = FtpHelper.FtpPort
            };

            var options = new Options
            {
                ThrowErrorOnFail = false
            };

            File.WriteAllText(Path.Combine(destination.Directory, source.FileName), "Hello");

            // Test and assert
            var result = FTP.DownloadFiles(source, destination, connection, options, new Info(), new CancellationToken());
            Assert.IsFalse(result.Success);
            Assert.AreEqual(1, result.FailedTransferCount);
        }

        [Test]
        public void DownloadFTP_DestinationFileExists_ExceptionThrown()
        {
            // Setup
            FtpHelper.CreateFileOnFTP(FtpDir, "file1.txt");
            var source = new Source { Directory = FtpDir, FileName = "file1.txt" };
            var destination = new Destination { Directory = LocalDirFullPath, Action = DestinationAction.Error };
            var connection = new Connection
            {
                Address = FtpHelper.FtpHost,
                UserName = FtpHelper.FtpUsername,
                Password = FtpHelper.FtpPassword,
                Port = FtpHelper.FtpPort
            };

            var options = new Options
            {
                ThrowErrorOnFail = true
            };

            File.WriteAllText(Path.Combine(destination.Directory, source.FileName), "Hello");

            // Test and assert
            var ex = Assert.Throws<Exception>(() => FTP.DownloadFiles(source, destination, connection, options, new Info(), new CancellationToken()));
            Assert.IsTrue(ex.Message.Contains($"Error: Unable to transfer file. Destination file already exists: {destination.FileName}"));
        }

        [Test]
        public void DownloadFTP_RenameSourceFileBeforeTransfer()
        {
            // Setup
            FtpHelper.CreateFileOnFTP(FtpDir, "file1.txt");
            var source = new Source { Directory = FtpDir, FileName = "file1.txt", Operation = SourceOperation.Nothing };
            var destination = new Destination { Directory = LocalDirFullPath, Action = DestinationAction.Overwrite };
            var connection = new Connection
            {
                Address = FtpHelper.FtpHost,
                UserName = FtpHelper.FtpUsername,
                Password = FtpHelper.FtpPassword,
                Port = FtpHelper.FtpPort
            };

            // Test and assert
            var result = FTP.DownloadFiles(
                source, destination, connection, new Options { RenameSourceFileBeforeTransfer = true },
                new Info(), new CancellationToken());
            Assert.IsTrue(result.Success, result.UserResultMessage);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.IsTrue(LocalFileExists("file1.txt"), result.UserResultMessage);

            // Make sure file still exists on FTP
            Assert.IsTrue(FtpHelper.FileExistsOnFTP(FtpDir, "file1.txt"));
        }

        [Test]
        public void DownloadFTPS_CorrectFingerprint()
        {
            // Setup
            FtpHelper.CreateFileOnFTP(FtpDir, "file1.txt");
            var source = new Source { Directory = FtpDir, FileName = "file1.txt" };
            var destination = new Destination { Directory = LocalDirFullPath, Action = DestinationAction.Overwrite };

            // Our test certificate hashes:
            // SHA-256: 90:bc:7f:71:14:f5:c2:ad:03:46:d6:ff:75:d5:fe:12:ba:74:23:73:54:31:70:60:b4:8b:bd:8e:87:21:9c:16
            // SHA-1: d9:11:26:29:84:de:9c:c3:2a:35:18:a1:09:4c:d2:42:49:ea:5c:49
            var connection = new Connection
            {
                Address = FtpHelper.FtpHost,
                UserName = FtpHelper.FtpUsername,
                Password = FtpHelper.FtpPassword,
                Port = FtpHelper.FtpPort,
                SslMode = FtpsSslMode.Explicit,
                UseFTPS = true,
                CertificateHashStringSHA1 = "D911262984DE9CC32A3518A1094CD24249EA5C49"
            };


            // Test and assert
            var result = FTP.DownloadFiles(source, destination, connection, new Options(), new Info(), new CancellationToken());
            Assert.IsTrue(result.Success);
            Assert.AreEqual(1, result.SuccessfulTransferCount);
            Assert.IsTrue(LocalFileExists("file1.txt"));
        }

        [Test]
        public void DownloadFTPS_IncorrectFingerprint()
        {
            // Setup
            FtpHelper.CreateFileOnFTP(FtpDir, "file1.txt");
            var source = new Source { Directory = FtpDir, FileName = "file1.txt" };
            var destination = new Destination { Directory = LocalDirFullPath, Action = DestinationAction.Overwrite };

            // Our test certificate hashes:
            // SHA-256: 90:bc:7f:71:14:f5:c2:ad:03:46:d6:ff:75:d5:fe:12:ba:74:23:73:54:31:70:60:b4:8b:bd:8e:87:21:9c:16
            // SHA-1: d9:11:26:29:84:de:9c:c3:2a:35:18:a1:09:4c:d2:42:49:ea:5c:49
            var connection = new Connection
            {
                Address = FtpHelper.FtpHost,
                UserName = FtpHelper.FtpUsername,
                Password = FtpHelper.FtpPassword,
                Port = FtpHelper.FtpsPort,
                SslMode = FtpsSslMode.Explicit,
                UseFTPS = true,
                CertificateHashStringSHA1 = "nope"
            };


            // Test and assert
            var ex = Assert.Throws<AggregateException>(() =>
            {
                var result = FTP.DownloadFiles(source, destination, connection, new Options(), new Info(),
                    new CancellationToken());

            });

            Assert.AreEqual(1, ex.InnerExceptions.Count);
            Assert.AreEqual(typeof(AuthenticationException), ex.InnerExceptions[0].GetType());
        }

        [Test]
        public void DownloadFTPS_CurrentUserHasNoCertificates()
        {
            // Setup
            FtpHelper.CreateFileOnFTP(FtpDir, "file1.txt");
            var source = new Source { Directory = FtpDir, FileName = "file1.txt" };
            var destination = new Destination { Directory = LocalDirFullPath, Action = DestinationAction.Overwrite };

            // Our test certificate hashes:
            // SHA-256: 90:bc:7f:71:14:f5:c2:ad:03:46:d6:ff:75:d5:fe:12:ba:74:23:73:54:31:70:60:b4:8b:bd:8e:87:21:9c:16
            // SHA-1: d9:11:26:29:84:de:9c:c3:2a:35:18:a1:09:4c:d2:42:49:ea:5c:49
            var connection = new Connection
            {
                Address = FtpHelper.FtpHost,
                UserName = FtpHelper.FtpUsername,
                Password = FtpHelper.FtpPassword,
                Port = FtpHelper.FtpsPort,
                SslMode = FtpsSslMode.Explicit,
                EnableClientAuth = true,
                UseFTPS = true,
                ValidateAnyCertificate = false,
                CertificateHashStringSHA1 = "",
                ClientCertificatePath = ""
            };

            var ex = Assert.Throws<AggregateException>(() =>
            {
                var result = FTP.DownloadFiles(source, destination, connection, new Options(), new Info(),
                    new CancellationToken());

            });

            Assert.AreEqual(1, ex.InnerExceptions.Count);
            Assert.AreEqual(typeof(AuthenticationException), ex.InnerExceptions[0].GetType());

        }

        [Test]
        public void DownloadFTP_LargeFiles()
        {
            FtpHelper.CreateLargeFileOnFTP(FtpDir, 5);
            var source = new Source { Directory = FtpDir, FileName = "*.bin", Operation = SourceOperation.Delete };
            var destination = new Destination { Directory = LocalDirFullPath, Action = DestinationAction.Overwrite };
            var connection = new Connection
            {
                Address = FtpHelper.FtpHost,
                UserName = FtpHelper.FtpUsername,
                Password = FtpHelper.FtpPassword,
                Port = FtpHelper.FtpPort
            };

            // Test and assert
            var result = FTP.DownloadFiles(source, destination, connection, new Options(), new Info(), new CancellationToken());
            Assert.IsTrue(result.Success, result.UserResultMessage);
            Assert.AreEqual(5, result.SuccessfulTransferCount);
        }
    }
}