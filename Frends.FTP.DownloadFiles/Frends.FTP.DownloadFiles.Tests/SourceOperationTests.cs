using System;
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
        CreateDummyFileInDummyDir("file1.txt");
        CreateDummyFileInDummyDir("file2.txt");
        CreateDummyFileInDummyDir("file3.txt");
        
        var result = CallUploadFiles(
            SourceOperation.Delete,
            "file*.txt",
            nameof(SourceOperation_Delete));
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.SuccessfulTransferCount);    
        Assert.IsFalse(DummyFileExists("file1.txt"));
        Assert.IsFalse(DummyFileExists("file2.txt"));
        Assert.IsFalse(DummyFileExists("file3.txt"));
    }

    [Test]
    public void SourceOperation_Nothing()
    {
        // Setup
        CreateDummyFileInDummyDir("file1.txt");
        CreateDummyFileInDummyDir("file2.txt");
        CreateDummyFileInDummyDir("file3.txt");
        
        var result = CallUploadFiles(
            SourceOperation.Nothing,
            "file*.txt",
            $"/{nameof(SourceOperation_Nothing)}");
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.SuccessfulTransferCount);    
        Assert.IsTrue(DummyFileExists("file1.txt"));
        Assert.IsTrue(DummyFileExists("file2.txt"));
        Assert.IsTrue(DummyFileExists("file3.txt"));
    }

    [Test]
    public void SourceOperation_Move()
    {
        // Setup
        CreateDummyFileInDummyDir("file1.txt");
        CreateDummyFileInDummyDir("file2.txt");
        CreateDummyFileInDummyDir("file3.txt");
        var moveTo = CreateDummyDir();
        
        var result = CallUploadFiles(
            SourceOperation.Move,
            "file*.txt",
            $"/{nameof(SourceOperation_Move)}",
            moveTo);
        
        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.SuccessfulTransferCount);
        
        // Check that original files are gone
        Assert.IsFalse(DummyFileExists("file1.txt"));
        Assert.IsFalse(DummyFileExists("file2.txt"));
        Assert.IsFalse(DummyFileExists("file3.txt"));
        
        // Check that they are moved to new dir
        Assert.IsTrue(DummyFileExists("file1.txt", moveTo));
        Assert.IsTrue(DummyFileExists("file2.txt", moveTo));
        Assert.IsTrue(DummyFileExists("file3.txt", moveTo));
    }
    

    [Test]
    public void SourceOperation_Rename()
    {
        // Setup
        CreateDummyFileInDummyDir("file1.txt");
        CreateDummyFileInDummyDir("file2.txt");
        CreateDummyFileInDummyDir("file3.txt");
        
        var result = CallUploadFiles(
            SourceOperation.Rename,
            "file*.txt",
            $"/{nameof(SourceOperation_Rename)}",
            renameTo: "%Year%-%SourceFileName%%SourceFileExtension%");
        
        Assert.IsTrue(result.Success, result.UserResultMessage);
        Assert.AreEqual(3, result.SuccessfulTransferCount);

        var year = DateTime.Today.Year;
        Assert.IsTrue(DummyFileExists($"{year}-file1.txt"));
        Assert.IsTrue(DummyFileExists($"{year}-file2.txt"));
        Assert.IsTrue(DummyFileExists($"{year}-file3.txt"));
    }
    
    private Result CallUploadFiles(
        SourceOperation sourceOperation, string sourceFileName, string targetDir,
        string moveToDir = null,
        string renameTo = null)
    {
        var source = new Source
        {
            Directory = Dir, FileName = sourceFileName, Operation = sourceOperation,
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