using Frends.FTP.UploadFiles.Enums;
using Frends.FTP.UploadFiles.TaskConfiguration;
using Frends.FTP.UploadFiles.TaskResult;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System.IO;
using System.Threading;

namespace Frends.FTP.UploadFiles.Tests;

[TestClass]
public class DestinationActionTests : UploadFilesTestBase
{
    [TestMethod]
    public void DestinationAction_Append_NoRenameInTransfer()
    {
        CreateDummyFileInDummyDir("file1.txt", "mycontent");

        var result1 = CallUploadFiles(
            DestinationAction.Overwrite, "file1.txt", nameof(DestinationAction_Append_NoRenameInTransfer), false);
        var result2 = CallUploadFiles(
            DestinationAction.Append, "file1.txt", nameof(DestinationAction_Append_NoRenameInTransfer), false);

        Assert.IsTrue(result1.Success, result1.UserResultMessage);
        Assert.IsTrue(result2.Success, result2.UserResultMessage);
        Assert.AreEqual(1, result2.SuccessfulTransferCount);
        Assert.AreEqual("mycontentmycontent", File.ReadAllText(Helpers.GetFileFromFtp(nameof(DestinationAction_Append_NoRenameInTransfer), "file1.txt")));
    }

    [TestMethod]
    public void DestinationAction_Append_WithRenameInTransfer()
    {
        CreateDummyFileInDummyDir("file1.txt", "mycontent");

        var result1 = CallUploadFiles(
            DestinationAction.Overwrite, "file1.txt", nameof(DestinationAction_Append_WithRenameInTransfer), true);
        var result2 = CallUploadFiles(
            DestinationAction.Append, "file1.txt", nameof(DestinationAction_Append_WithRenameInTransfer), true);

        Assert.IsTrue(result1.Success, result1.UserResultMessage);
        Assert.IsTrue(result2.Success, result2.UserResultMessage);
        Assert.AreEqual(1, result2.SuccessfulTransferCount);
        Assert.AreEqual("mycontentmycontent", File.ReadAllText(Helpers.GetFileFromFtp(nameof(DestinationAction_Append_WithRenameInTransfer), "file1.txt")));
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