using Frends.FTP.UploadFiles.Enums;
using Frends.FTP.UploadFiles.TaskConfiguration;
using Frends.FTP.UploadFiles.TaskResult;
using NUnit.Framework;
using System.Linq;
using System.Threading;

namespace Frends.FTP.UploadFiles.Tests;

[TestFixture]
public class SourceFilesNotFoundActionTests : UploadFilesTestBase
{
    [Test]
    public void SourceFilesNotFoundAction_Ignore()
    {
        var result = CallUploadFiles(
            SourceNotFoundAction.Ignore, "file*.txt",
            $"/{nameof(SourceFilesNotFoundAction_Ignore)}");

        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.ActionSkipped);
        // User result should contain message
        Assert.IsTrue(result.UserResultMessage.Contains("No source files"));
        // Operations logs should NOT contain message
        Assert.IsFalse(result.OperationsLog.Any(o => o.Value.Contains("No source files")));
    }

    [Test]
    public void SourceFilesNotFoundAction_Info()
    {
        var result = CallUploadFiles(
            SourceNotFoundAction.Info, "file*.txt",
            nameof(SourceFilesNotFoundAction_Info));

        Assert.IsTrue(result.Success);
        Assert.IsTrue(result.ActionSkipped);

        // User result should contain message
        Assert.IsTrue(result.UserResultMessage.Contains("No source files"));
        // Operations logs should contain message
        Assert.IsTrue(result.OperationsLog.Any(o => o.Value.Contains("No source files")));
    }

    [Test]
    public void SourceFilesNotFoundAction_Error()
    {
        var result = CallUploadFiles(
            SourceNotFoundAction.Error, "file*.txt",
            nameof(SourceFilesNotFoundAction_Error));

        Assert.IsFalse(result.Success);
        Assert.IsFalse(result.ActionSkipped);

        // User result should contain message
        Assert.IsTrue(result.UserResultMessage.Contains("No source files"));
        // Operations logs should contain message
        Assert.IsTrue(result.OperationsLog.Any(o => o.Value.Contains("No source files")));
    }

    private Result CallUploadFiles(SourceNotFoundAction sourceNotFoundAction, string sourceFileName, string targetDir)
    {
        var source = new Source
        {
            Directory = Dir,
            FileName = sourceFileName,
            Operation = SourceOperation.Delete,
            NotFoundAction = sourceNotFoundAction,
            DirectoryToMoveAfterTransfer = targetDir,
            FileNameAfterTransfer = sourceFileName,
            FilePaths = null
        };
        var destination = new Destination
        { Directory = targetDir, Action = DestinationAction.Overwrite };
        var connection = Helpers.GetFtpConnection();

        var result = FTP.UploadFiles(
            source, destination, connection,
            new Options { OperationLog = true }, new Info(), new CancellationToken());
        return result;
    }
}