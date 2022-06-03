using System.Linq;
using System.Threading;
using Frends.FTP.DownloadFiles.Enums;
using Frends.FTP.DownloadFiles.TaskConfiguration;
using Frends.FTP.DownloadFiles.TaskResult;
using Frends.FTP.DownloadFiles.Tests.Lib;
using NUnit.Framework;

namespace Frends.FTP.DownloadFiles.Tests;

[TestFixture]
public class SourceFilesNotFoundActionTests : DownloadFilesTestBase
{
    [Test]
    public void SourceFilesNotFoundAction_Ignore()
    {
        var result = CallDownloadFiles(
            SourceNotFoundAction.Ignore, "file*.txt",
            nameof(SourceFilesNotFoundAction_Ignore));
        
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
        var result = CallDownloadFiles(
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
        var result = CallDownloadFiles(
            SourceNotFoundAction.Error, "file*.txt",
            nameof(SourceFilesNotFoundAction_Error));
        
        Assert.IsFalse(result.Success, result.UserResultMessage);
        Assert.IsFalse(result.ActionSkipped, result.UserResultMessage);
        
        // User result should contain message
        Assert.IsTrue(result.UserResultMessage.Contains("No source files"));
        // Operations logs should contain message
        Assert.IsTrue(result.OperationsLog.Any(o => o.Value.Contains("No source files")));
    }
    
    private Result CallDownloadFiles(SourceNotFoundAction sourceNotFoundAction, string sourceFileName, string targetDir)
    {
        var source = new Source
        {
            Directory = DummyFtpSubDirName, FileName = sourceFileName, Operation = SourceOperation.Delete,
            NotFoundAction = sourceNotFoundAction
        };
        var destination = new Destination
            { Directory = targetDir, Action = DestinationAction.Overwrite };
        var connection = Helpers.GetFtpConnection();

        var result = FTP.DownloadFiles(
            source, destination, connection, 
            new Options { OperationLog = true }, new Info(), new CancellationToken());
        return result;
    }
}