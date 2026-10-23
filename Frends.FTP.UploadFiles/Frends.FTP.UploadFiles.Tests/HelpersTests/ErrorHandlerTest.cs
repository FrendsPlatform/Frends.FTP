using System;
using System.Threading;
using Frends.FTP.UploadFiles.TaskConfiguration;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Frends.FTP.UploadFiles.Tests.HelpersTests;

[TestClass]
public class ErrorHandlerTest
{
    private const string CustomErrorMessage = "CustomErrorMessage";

    [TestMethod]
    public void Should_Throw_Error_When_ThrowErrorOnFailure_Is_True()
    {
        var options = new Options { ThrowErrorOnFailure = true };
        Assert.ThrowsException<NullReferenceException>(() =>
            FTP.UploadFiles(new Input(), InvalidConnection(), options, CancellationToken.None));
    }

    [TestMethod]
    public void Should_Return_Failed_Result_When_ThrowErrorOnFailure_Is_False()
    {
        var options = new Options { ThrowErrorOnFailure = false };
        var result = FTP.UploadFiles(new Input(), InvalidConnection(), options, CancellationToken.None);
        Assert.IsFalse(result.Success);
        Assert.IsNotNull(result.Error);
    }

    [TestMethod]
    public void Should_Use_Custom_ErrorMessageOnFailure()
    {
        var options = new Options { ThrowErrorOnFailure = true, ErrorMessageOnFailure = CustomErrorMessage };
        var ex = Assert.ThrowsException<Exception>(() =>
            FTP.UploadFiles(new Input(), InvalidConnection(), options, CancellationToken.None));
        StringAssert.Contains(ex.Message, CustomErrorMessage);
    }

    private static Connection InvalidConnection() => new Connection { Address = string.Empty };
}
