﻿using Frends.FTP.UploadFiles.Enums;
using Frends.FTP.UploadFiles.TaskConfiguration;
using Frends.FTP.UploadFiles.TaskResult;
using NUnit.Framework;
using System.Threading;

namespace Frends.FTP.UploadFiles.Tests;

[TestFixture]
public class DestinationActionTests : UploadFilesTestBase
{
    [Test]
    public void DestinationAction_Append_NoRenameInTransfer()
    {
        CreateDummyFileInDummyDir("file2.txt", "mycontent");

        var result1 = CallUploadFiles(
            DestinationAction.Overwrite, "file2.txt", nameof(DestinationAction_Append_NoRenameInTransfer), false);
        var result2 = CallUploadFiles(
            DestinationAction.Append, "file2.txt", nameof(DestinationAction_Append_NoRenameInTransfer), false);

        Assert.IsTrue(result1.Success, result1.UserResultMessage);
        Assert.IsTrue(result2.Success, result2.UserResultMessage);
        Assert.AreEqual(1, result2.SuccessfulTransferCount);
        Assert.AreEqual("mycontentmycontent", Helpers.GetFileFromFtp(nameof(DestinationAction_Append_NoRenameInTransfer), "file2.txt"));
    }

    [Test]
    public void DestinationAction_Append_WithRenameInTransfer()
    {
        CreateDummyFileInDummyDir("file2.txt", "mycontent");

        var result1 = CallUploadFiles(
            DestinationAction.Overwrite, "file2.txt", nameof(DestinationAction_Append_WithRenameInTransfer), true);
        var result2 = CallUploadFiles(
            DestinationAction.Append, "file2.txt", nameof(DestinationAction_Append_WithRenameInTransfer), true);

        Assert.IsTrue(result1.Success, result1.UserResultMessage);
        Assert.IsTrue(result2.Success, result2.UserResultMessage);
        Assert.AreEqual(1, result2.SuccessfulTransferCount);
        Assert.AreEqual("mycontentmycontent", Helpers.GetFileFromFtp(nameof(DestinationAction_Append_WithRenameInTransfer), "file2.txt"));
    }

    private Result CallUploadFiles(
        DestinationAction destinationAction, string sourceFileName, string targetDir,
        bool renameDestinationFilesDuringTransfer)
    {
        var source = new Source
        {
            Directory = Dir,
            FileName = sourceFileName,
            Operation = SourceOperation.Nothing
        };
        var destination = new Destination
        { Directory = targetDir, Action = destinationAction };
        var options = new Options { CreateDestinationDirectories = true, RenameDestinationFileDuringTransfer = renameDestinationFilesDuringTransfer };
        var connection = Helpers.GetFtpsConnection();

        var result = FTP.UploadFiles(source, destination, connection, options, new Info(), new CancellationToken());
        return result;
    }
}