using System.IO;
using System.Threading;
using Frends.FTP.DownloadFiles.Enums;
using Frends.FTP.DownloadFiles.TaskConfiguration;
using Frends.FTP.DownloadFiles.TaskResult;
using Frends.FTP.DownloadFiles.Tests.Lib;
using NUnit.Framework;

namespace Frends.FTP.DownloadFiles.Tests;

[TestFixture]
public class DestinationActionTests : DownloadFilesTestBase
{
    [Test]
    public void DestinationAction_Append_NoRenameInTransfer()
    {
        FtpHelper.CreateFileOnFTP(FtpDir, "file1.txt", "mycontent");

        var result1 = CallDownloadFiles(
            DestinationAction.Overwrite,
            "file1.txt",
            LocalDirFullPath,
            false);
        var result2 = CallDownloadFiles(
            DestinationAction.Append,
            "file1.txt",
            LocalDirFullPath,
            false);

        Assert.IsTrue(result1.Success, result1.UserResultMessage);
        Assert.IsTrue(result2.Success, result2.UserResultMessage);
        Assert.AreEqual(1, result2.SuccessfulTransferCount);
        Assert.AreEqual("mycontentmycontent", File.ReadAllText($"{LocalDirFullPath}/file1.txt"));
    }

    [Test]
    public void DestinationAction_Append_WithRenameInTransfer()
    {
        FtpHelper.CreateFileOnFTP(FtpDir, "file1.txt", "mycontent");

        var result1 = CallDownloadFiles(
            DestinationAction.Overwrite,
            "file1.txt",
            LocalDirFullPath,
            true);
        var result2 = CallDownloadFiles(
            DestinationAction.Append,
            "file1.txt",
            LocalDirFullPath,
            true);

        Assert.IsTrue(result1.Success, result1.UserResultMessage);
        Assert.IsTrue(result2.Success, result2.UserResultMessage);
        Assert.AreEqual(1, result2.SuccessfulTransferCount);
        Assert.AreEqual("mycontentmycontent", File.ReadAllText($"{LocalDirFullPath}/file1.txt"));
    }

    private Result CallDownloadFiles(
        DestinationAction destinationAction, string sourceFileName, string targetDir,
        bool renameDestinationFilesDuringTransfer)
    {
        var source = new Source
        {
            Directory = FtpDir,
            FileName = sourceFileName,
            Operation = SourceOperation.Nothing
        };

        var destination = new Destination { Directory = targetDir, Action = destinationAction };
        var options = new Options { CreateDestinationDirectories = true, RenameDestinationFileDuringTransfer = renameDestinationFilesDuringTransfer };
        var connection = FtpHelper.GetFtpsConnection();

        var result = FTP.DownloadFiles(source, destination, connection, options, new Info(), new CancellationToken());
        return result;
    }
}