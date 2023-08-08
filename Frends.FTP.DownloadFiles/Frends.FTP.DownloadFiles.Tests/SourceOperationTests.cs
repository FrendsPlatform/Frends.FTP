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
        var guid = Guid.NewGuid().ToString();
        FtpHelper.CreateFileOnFTP(guid, "file1.txt");
        FtpHelper.CreateFileOnFTP(guid, "file2.txt");
        FtpHelper.CreateFileOnFTP(guid, "file3.txt");

        var result = CallDownloadFiles(
            SourceOperation.Delete,
            guid,
            "file*.txt",
            LocalDirFullPath);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.SuccessfulTransferCount);

        // Make sure we deleted the files
        Assert.IsFalse(FtpHelper.FileExistsOnFTP(guid, "file1.txt"));
        Assert.IsFalse(FtpHelper.FileExistsOnFTP(guid, "file2.txt"));
        Assert.IsFalse(FtpHelper.FileExistsOnFTP(guid, "file3.txt"));
    }

    [Test]
    public void SourceOperation_Nothing()
    {
        // Setup
        FtpHelper.CreateFileOnFTP(FtpDir, "file1.txt");
        FtpHelper.CreateFileOnFTP(FtpDir, "file2.txt");
        FtpHelper.CreateFileOnFTP(FtpDir, "file3.txt");

        var result = CallDownloadFiles(
            SourceOperation.Nothing,
            FtpDir,
            "file*.txt",
            LocalDirFullPath);

        Assert.IsTrue(result.Success);
        Assert.AreEqual(3, result.SuccessfulTransferCount);

        // Make sure nothing happened to files
        Assert.IsTrue(FtpHelper.FileExistsOnFTP(FtpDir, "file1.txt"));
        Assert.IsTrue(FtpHelper.FileExistsOnFTP(FtpDir, "file2.txt"));
        Assert.IsTrue(FtpHelper.FileExistsOnFTP(FtpDir, "file3.txt"));
    }

    [Test]
    public void SourceOperation_Move()
    {
        // Setup
        FtpHelper.CreateFileOnFTP(FtpDir, "file1.txt");
        FtpHelper.CreateFileOnFTP(FtpDir, "file2.txt");
        FtpHelper.CreateFileOnFTP(FtpDir, "file3.txt");
        var moveToSubDir = Guid.NewGuid().ToString();
        FtpHelper.CreateDirectoryOnFTP(moveToSubDir);

        var result = CallDownloadFiles(
            SourceOperation.Move,
            FtpDir,
            "file*.txt",
            LocalDirFullPath,
            $"/{moveToSubDir}");

        Assert.IsTrue(result.Success, result.UserResultMessage);
        Assert.AreEqual(3, result.SuccessfulTransferCount);

        // Check that original files are gone
        Assert.IsFalse(FtpHelper.FileExistsOnFTP(FtpDir, "file1.txt"));
        Assert.IsFalse(FtpHelper.FileExistsOnFTP(FtpDir, "file2.txt"));
        Assert.IsFalse(FtpHelper.FileExistsOnFTP(FtpDir, "file3.txt"));

        // Check that they are moved to new dir
        Assert.IsFalse(FtpHelper.FileExistsOnFTP(FtpDir, "file1.txt"));
        Assert.IsFalse(FtpHelper.FileExistsOnFTP(FtpDir, "file2.txt"));
        Assert.IsFalse(FtpHelper.FileExistsOnFTP(FtpDir, "file3.txt"));
        Assert.IsTrue(FtpHelper.FileExistsOnFTP(moveToSubDir, "file1.txt"));
        Assert.IsTrue(FtpHelper.FileExistsOnFTP(moveToSubDir, "file2.txt"));
        Assert.IsTrue(FtpHelper.FileExistsOnFTP(moveToSubDir, "file3.txt"));
    }

    [Test]
    public void SourceOperation_MoveToFolderDoesNotExist_ProducesError()
    {
        // Setup
        FtpHelper.CreateFileOnFTP(FtpDir, "file1.txt");
        var moveToSubDir = Guid.NewGuid().ToString();
        FtpHelper.CreateDirectoryOnFTP(moveToSubDir);

        var result = CallDownloadFiles(
            SourceOperation.Move,
            FtpDir,
            "file*.txt",
            LocalDirFullPath,
            $"/does-not-exist{Guid.NewGuid()}");

        Assert.IsFalse(result.Success, result.UserResultMessage);
        Assert.AreEqual(0, result.SuccessfulTransferCount);
        Assert.AreEqual(1, result.FailedTransferCount);

        // Check that original file is still there
        Assert.IsTrue(FtpHelper.FileExistsOnFTP(FtpDir, "file1.txt"), result.UserResultMessage);
    }

    [Test]
    public void SourceOperation_FileAlreadyExistsInMoveToFolder()
    {
        // Setup
        FtpHelper.CreateFileOnFTP(FtpDir, "file1.txt");
        var moveToSubDir = Guid.NewGuid().ToString();
        FtpHelper.CreateDirectoryOnFTP(moveToSubDir);
        FtpHelper.CreateFileOnFTP(moveToSubDir, "file1.txt");

        var result = CallDownloadFiles(
            SourceOperation.Move,
            FtpDir,
            "file*.txt",
            LocalDirFullPath,
            $"/does-not-exist{Guid.NewGuid()}",
            throwError: false);

        Assert.IsFalse(result.Success, result.UserResultMessage);
        Assert.AreEqual(0, result.SuccessfulTransferCount);
        Assert.AreEqual(1, result.FailedTransferCount);

        // Check that original file is still there
        Assert.IsTrue(FtpHelper.FileExistsOnFTP(FtpDir, "file1.txt"), result.UserResultMessage);
    }

    [Test]
    public void SourceOperation_RenameWithMacro()
    {
        // Setup
        FtpHelper.CreateFileOnFTP(FtpDir, "file1.txt");
        FtpHelper.CreateFileOnFTP(FtpDir, "file2.txt");
        FtpHelper.CreateFileOnFTP(FtpDir, "file3.txt");

        var result = CallDownloadFiles(
            SourceOperation.Rename,
            FtpDir,
            "file*.txt",
            LocalDirFullPath,
            renameTo: "%Year%-%SourceFileName%%SourceFileExtension%",
            operationslog: false);

        Assert.IsTrue(result.Success, result.UserResultMessage);
        Assert.AreEqual(3, result.SuccessfulTransferCount);

        var year = DateTime.Today.Year;
        Assert.IsFalse(FtpHelper.FileExistsOnFTP(FtpDir, $"file1.txt"));
        Assert.IsFalse(FtpHelper.FileExistsOnFTP(FtpDir, $"file2.txt"));
        Assert.IsFalse(FtpHelper.FileExistsOnFTP(FtpDir, $"file3.txt"));
        Assert.IsTrue(FtpHelper.FileExistsOnFTP(FtpDir, $"{year}-file1.txt"));
        Assert.IsTrue(FtpHelper.FileExistsOnFTP(FtpDir, $"{year}-file2.txt"));
        Assert.IsTrue(FtpHelper.FileExistsOnFTP(FtpDir, $"{year}-file3.txt"));
    }

    private Result CallDownloadFiles(
        SourceOperation sourceOperation,
        string sourceDirName,
        string sourceFileName,
        string targetDir,
        string moveToDir = null,
        string renameTo = null,
        bool operationslog = true,
        bool throwError = true)
    {
        var source = new Source
        {
            Directory = sourceDirName,
            FileName = sourceFileName,
            Operation = sourceOperation,
            DirectoryToMoveAfterTransfer = moveToDir,
            FileNameAfterTransfer = renameTo
        };
        var destination = new Destination
        { Directory = targetDir, Action = DestinationAction.Overwrite };
        var options = new Options { ThrowErrorOnFail = throwError, CreateDestinationDirectories = true, RenameSourceFileBeforeTransfer = true, OperationLog = operationslog };
        var connection = FtpHelper.GetFtpsConnection();

        var result = FTP.DownloadFiles(source, destination, connection, options, new Info(), new CancellationToken());
        return result;
    }
}