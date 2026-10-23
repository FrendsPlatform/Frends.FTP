using Frends.FTP.ReadFile.Definitions;
using NUnit.Framework;

namespace Frends.FTP.ReadFile.Tests;

[TestFixture]
internal class ErrorHandlerTest
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    private static Input DefaultInput() => new() { Path = "/upload/test.txt" };

    // Unreachable endpoint: nothing listens on port 1.
    private static Connection DefaultConnection() => new()
    {
        Address = "127.0.0.1",
        Port = 1,
        UserName = "user",
        Password = "pass",
        ConnectionTimeout = 2,
        BufferSize = 4096,
    };

    private static Options DefaultOptions() => new();

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var ex = Assert.Catch<Exception>(() =>
            FTP.ReadFile(DefaultInput(), DefaultConnection(), DefaultOptions(), default));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public void Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = DefaultOptions();
        options.ThrowErrorOnFailure = false;
        var result = FTP.ReadFile(DefaultInput(), DefaultConnection(), options, default);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = DefaultOptions();
        options.ErrorMessageOnFailure = CustomErrorMessage;
        var ex = Assert.Throws<Exception>(() =>
            FTP.ReadFile(DefaultInput(), DefaultConnection(), options, default));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Contains.Substring(CustomErrorMessage));
    }
}
