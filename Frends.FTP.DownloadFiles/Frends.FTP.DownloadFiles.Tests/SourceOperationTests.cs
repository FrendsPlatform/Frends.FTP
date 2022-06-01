using System;
using System.IO;
using System.Threading;
using Frends.FTP.DownloadFiles.Enums;
using Frends.FTP.DownloadFiles.TaskConfiguration;
using Frends.FTP.DownloadFiles.TaskResult;
using Frends.FTP.DownloadFiles.Tests.Lib;
using NUnit.Framework;

namespace Frends.FTP.DownloadFiles.Tests;

[TestFixture]
public class SourceOperationTests : DownloadFilesTestBase
{
    [Test]
    public void SourceOperation_Delete()
    {
        // Setup
        CreateDummyFileInFtpDir("file1.txt");
        CreateDummyFileInFtpDir("file2.txt");
        CreateDummyFileInFtpDir("file3.txt");
        
        var result = CallDownloadFiles(
            SourceOperation.Delete,
            "file*.txt",
            nameof(SourceOperation_Delete));
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.SuccessfulTransferCount);    
        Assert.IsFalse(FtpFileExists("file1.txt"));
        Assert.IsFalse(FtpFileExists("file2.txt"));
        Assert.IsFalse(FtpFileExists("file3.txt"));
    }

    [Test]
    public void SourceOperation_Nothing()
    {
        // Setup
        CreateDummyFileInFtpDir("file1.txt");
        CreateDummyFileInFtpDir("file2.txt");
        CreateDummyFileInFtpDir("file3.txt");
        
        var result = CallDownloadFiles(
            SourceOperation.Nothing,
            "file*.txt",
            $"/{nameof(SourceOperation_Nothing)}");
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.SuccessfulTransferCount);    
        Assert.IsTrue(FtpFileExists("file1.txt"));
        Assert.IsTrue(FtpFileExists("file2.txt"));
        Assert.IsTrue(FtpFileExists("file3.txt"));
    }

    [Test]
    public void SourceOperation_Move()
    {
        // Setup
        CreateDummyFileInFtpDir("file1.txt");
        CreateDummyFileInFtpDir("file2.txt");
        CreateDummyFileInFtpDir("file3.txt");
        var moveToFullPath = CreateDummyFtpDir();
        var moveToSubdirName = Path.GetFileName(moveToFullPath);
        
        var result = CallDownloadFiles(
            SourceOperation.Move,
            "file*.txt",
            nameof(SourceOperation_Move),
            "/"+moveToSubdirName);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.SuccessfulTransferCount);
        
        // Check that original files are gone
        Assert.IsFalse(FtpFileExists("file1.txt"));
        Assert.IsFalse(FtpFileExists("file2.txt"));
        Assert.IsFalse(FtpFileExists("file3.txt"));
        
        // Check that they are moved to new dir
        Assert.IsTrue(FtpFileExists("file1.txt", moveToSubdirName));
        Assert.IsTrue(FtpFileExists("file2.txt", moveToSubdirName));
        Assert.IsTrue(FtpFileExists("file3.txt", moveToSubdirName));
    }
    

    [Test]
    public void SourceOperation_Rename()
    {
        // Setup
        CreateDummyFileInFtpDir("file1.txt");
        CreateDummyFileInFtpDir("file2.txt");
        CreateDummyFileInFtpDir("file3.txt");
        
        var result = CallDownloadFiles(
            SourceOperation.Rename,
            "file*.txt",
            $"/{nameof(SourceOperation_Rename)}",
            renameTo: "%Year%-%SourceFileName%%SourceFileExtension%");
        
        Assert.IsTrue(result.Success, result.UserResultMessage);
        Assert.AreEqual(3, result.SuccessfulTransferCount);

        var year = DateTime.Today.Year;
        Assert.IsTrue(FtpFileExists($"{year}-file1.txt"));
        Assert.IsTrue(FtpFileExists($"{year}-file2.txt"));
        Assert.IsTrue(FtpFileExists($"{year}-file3.txt"));
    }
    
    private Result CallDownloadFiles(
        SourceOperation sourceOperation, string sourceFileName, string targetDir,
        string moveToDir = null,
        string renameTo = null)
    {
        var source = new Source
        {
            Directory = FtpSubDirName, FileName = sourceFileName, Operation = sourceOperation,
            DirectoryToMoveAfterTransfer = moveToDir,
            FileNameAfterTransfer = renameTo
        };
        var destination = new Destination
            { Directory = targetDir, Action = DestinationAction.Overwrite };
        var options = new Options { CreateDestinationDirectories = true };
        var connection = Helpers.GetFtpsConnection();

        var result = FTP.DownloadFiles(source, destination, connection, options, new Info(), new CancellationToken());
        return result;
    }
}