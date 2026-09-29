namespace Frends.FTP.DeleteFiles.Tests;

using System;
using System.Threading;
using System.Threading.Tasks;
using Frends.FTP.DeleteFiles.Definitions;
using NUnit.Framework;

[TestFixture]
internal class ErrorHandlerTest
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var ex = Assert.CatchAsync<Exception>(async () =>
            await FTP.DeleteFiles(DefaultInput(), FtpHelper.GetFtpConnection(), DefaultOptions(), CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public async Task Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;

        var result = await FTP.DeleteFiles(DefaultInput(), FtpHelper.GetFtpConnection(), options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = DefaultOptions();
        options.ErrorMessageOnFailure = CustomErrorMessage;

        var ex = Assert.CatchAsync<Exception>(async () =>
            await FTP.DeleteFiles(DefaultInput(), FtpHelper.GetFtpConnection(), options, CancellationToken.None));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring(CustomErrorMessage));
    }

    private static Input DefaultInput() => new()
    {
        FileMask = "*",
        Directory = "/NoSuchDirectory",
    };

    private static Options DefaultOptions() => new();
}
