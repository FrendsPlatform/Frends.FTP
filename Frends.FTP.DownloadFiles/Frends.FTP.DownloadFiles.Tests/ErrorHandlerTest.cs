using System;
using System.Threading;
using Frends.FTP.DownloadFiles.TaskConfiguration;
using NUnit.Framework;

namespace Frends.FTP.DownloadFiles.Tests;

[TestFixture]
internal class ErrorHandlerTest
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var ex = Assert.Throws<Exception>(() =>
            FTP.DownloadFiles(new Input(), new Connection(), new Options(), CancellationToken.None));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public void Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = new Options
        {
            ThrowErrorOnFailure = false,
        };
        var result = FTP.DownloadFiles(new Input(), new Connection(), options, CancellationToken.None);

        Assert.That(result.Success, Is.False);
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = new Options
        {
            ErrorMessageOnFailure = CustomErrorMessage,
        };
        var ex = Assert.Throws<Exception>(() =>
            FTP.DownloadFiles(new Input(), new Connection(), options, CancellationToken.None));

        Assert.That(ex, Is.Not.Null);
        Assert.That(ex.Message, Contains.Substring(CustomErrorMessage));
    }
}
