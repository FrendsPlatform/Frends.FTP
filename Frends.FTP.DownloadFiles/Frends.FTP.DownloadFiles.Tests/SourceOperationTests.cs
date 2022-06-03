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
        var guid = Guid.NewGuid().ToString();
        Helpers.CreateFileOnFTP(guid, "file1.txt");
        Helpers.CreateFileOnFTP(guid, "file2.txt");
        Helpers.CreateFileOnFTP(guid, "file3.txt");
        
        var result = CallDownloadFiles(
            SourceOperation.Delete,
            guid,
            "file*.txt",
            nameof(SourceOperation_Delete));
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.SuccessfulTransferCount);
        
        // Make sure we deleted the files
        Assert.IsFalse(Helpers.FileExistsOnFTP(guid, "file1.txt"));
        Assert.IsFalse(Helpers.FileExistsOnFTP(guid, "file2.txt"));
        Assert.IsFalse(Helpers.FileExistsOnFTP(guid, "file3.txt"));
    }

    [Test]
    public void SourceOperation_Nothing()
    {
        // Setup
        var guid = Guid.NewGuid().ToString();
        Helpers.CreateFileOnFTP(guid, "file1.txt");
        Helpers.CreateFileOnFTP(guid, "file2.txt");
        Helpers.CreateFileOnFTP(guid, "file3.txt");
        
        var result = CallDownloadFiles(
            SourceOperation.Nothing,
            guid,
            "file*.txt",
            nameof(SourceOperation_Nothing));
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.SuccessfulTransferCount);    
        
        // Make sure nothing happened to files
        Assert.IsTrue(Helpers.FileExistsOnFTP(guid, "file1.txt"));
        Assert.IsTrue(Helpers.FileExistsOnFTP(guid, "file2.txt"));
        Assert.IsTrue(Helpers.FileExistsOnFTP(guid, "file3.txt"));
    }

    [Test]
    public void SourceOperation_Move()
    {
        // Setup
        var guid = Guid.NewGuid().ToString();
        Helpers.CreateFileOnFTP(guid, "file1.txt");
        Helpers.CreateFileOnFTP(guid, "file2.txt");
        Helpers.CreateFileOnFTP(guid, "file3.txt");
        var moveToFullPath = Guid.NewGuid().ToString();
        
        var result = CallDownloadFiles(
            SourceOperation.Move,
            guid,
            "file*.txt",
            nameof(SourceOperation_Move),
            moveToFullPath);
        
        Assert.IsTrue(result.Success, result.UserResultMessage);
        Assert.AreEqual(3, result.SuccessfulTransferCount);
        
        // Check that original files are gone
        Assert.IsFalse(Helpers.FileExistsOnFTP(guid, "file1.txt"));
        Assert.IsFalse(Helpers.FileExistsOnFTP(guid, "file2.txt"));
        Assert.IsFalse(Helpers.FileExistsOnFTP(guid, "file3.txt"));
        
        // Check that they are moved to new dir
        Assert.IsFalse(Helpers.FileExistsOnFTP(guid, "file1.txt"));
        Assert.IsFalse(Helpers.FileExistsOnFTP(guid, "file2.txt"));
        Assert.IsFalse(Helpers.FileExistsOnFTP(guid, "file3.txt"));
        Assert.IsTrue(Helpers.FileExistsOnFTP("file1.txt", moveToFullPath));
        Assert.IsTrue(Helpers.FileExistsOnFTP("file2.txt", moveToFullPath));
        Assert.IsTrue(Helpers.FileExistsOnFTP("file3.txt", moveToFullPath));
    }
    

    [Test]
    public void SourceOperation_RenameWithMacro()
    {
        // Setup
        var guid = Guid.NewGuid().ToString();
        Helpers.CreateFileOnFTP(guid, "file1.txt");
        Helpers.CreateFileOnFTP(guid, "file2.txt");
        Helpers.CreateFileOnFTP(guid, "file3.txt");
        
        var result = CallDownloadFiles(
            SourceOperation.Rename,
            guid,
            "file*.txt",
            $"/{nameof(SourceOperation_RenameWithMacro)}",
            renameTo: "%Year%-%SourceFileName%%SourceFileExtension%");
        
        Assert.IsTrue(result.Success, result.UserResultMessage);
        Assert.AreEqual(3, result.SuccessfulTransferCount);

        var year = DateTime.Today.Year;
        Assert.IsFalse(Helpers.FileExistsOnFTP(guid, $"{year}-file1.txt"));
        Assert.IsFalse(Helpers.FileExistsOnFTP(guid, $"{year}-file2.txt"));
        Assert.IsFalse(Helpers.FileExistsOnFTP(guid, $"{year}-file3.txt"));
    }
    
    private Result CallDownloadFiles(
        SourceOperation sourceOperation,
        string sourceDirName,
        string sourceFileName,
        string targetDir,
        string moveToDir = null,
        string renameTo = null)
    {
        var source = new Source
        {
            Directory = sourceDirName, FileName = sourceFileName, Operation = sourceOperation,
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