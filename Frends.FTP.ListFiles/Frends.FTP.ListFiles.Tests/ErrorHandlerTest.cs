using Frends.FTP.ListFiles.Definitions;
using Frends.FTP.ListFiles.Tests.Lib;
using NUnit.Framework;

namespace Frends.FTP.ListFiles.Tests;

[TestFixture]
internal class ErrorHandlerTest : ListFilesTestBase
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    [SetUp]
    public void InvalidInput()
    {
        Input.Directory = "/DirectoryThatDoesNotExist";
    }

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var ex = Assert.ThrowsAsync<ArgumentException>(async () =>
            await FTP.ListFiles(Input, FtpHelper.GetFtpConnection(), new Options(), CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = new Options { ThrowErrorOnFailure = false };
        var result = await FTP.ListFiles(Input, FtpHelper.GetFtpConnection(), options, CancellationToken.None);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = new Options { ErrorMessageOnFailure = CustomErrorMessage };
        var ex = Assert.ThrowsAsync<Exception>(async () =>
            await FTP.ListFiles(Input, FtpHelper.GetFtpConnection(), options, CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring(CustomErrorMessage));
    }
}
