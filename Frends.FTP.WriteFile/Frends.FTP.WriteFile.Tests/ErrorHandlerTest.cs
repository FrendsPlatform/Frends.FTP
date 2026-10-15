using Frends.FTP.WriteFile.Definitions;
using NUnit.Framework;

namespace Frends.FTP.WriteFile.Tests;

using Helpers = Frends.FTP.WriteFile.Tests.Lib.Helpers;

[TestFixture]
public class ErrorHandlerTest
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    private static Input InvalidInput() => new()
    {
        Content = "This is a test file",
        Path = "/upload/test.txt",
        WriteBehaviour = Enums.WriteOperation.Overwrite,
    };

    private static Connection InvalidConnection()
    {
        var connection = Helpers.GetFtpConnection();
        connection.Address = "invalid.host.invalid";
        connection.ConnectionTimeout = 2;
        return connection;
    }

    [Test]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var ex = Assert.Catch(() =>
            FTP.WriteFile(InvalidInput(), InvalidConnection(), new Options(), default));
        Assert.That(ex, Is.Not.Null);
    }

    [Test]
    public void Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = new Options { ThrowErrorOnFailure = false };
        var result = FTP.WriteFile(InvalidInput(), InvalidConnection(), options, default);
        Assert.That(result.Success, Is.False);
        Assert.That(result.Error, Is.Not.Null);
    }

    [Test]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = new Options { ErrorMessageOnFailure = CustomErrorMessage };
        var ex = Assert.Throws<Exception>(() =>
            FTP.WriteFile(InvalidInput(), InvalidConnection(), options, default));
        Assert.That(ex, Is.Not.Null);
        Assert.That(ex!.Message, Contains.Substring(CustomErrorMessage));
    }
}
