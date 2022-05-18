
using Frends.FTP.ListFiles.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Frends.FTP.ListFiles.Tests;

[TestClass]
public class UnitTest1
{
    private readonly string? _host = Environment.GetEnvironmentVariable("HiQ_FTP_Host");
    private readonly string? _user = Environment.GetEnvironmentVariable("HiQ_FTP_User");
    private readonly string? _pw = Environment.GetEnvironmentVariable("HiQ_FTP_Password");

    Input? input;

    /// <summary>
    /// List all files
    /// </summary>
    [TestMethod]
    public void ListAllTest()
    {
        input = new Input()
        {
            Filename = null,
            Host = _host,
            Port = 21,
            Path = "/",
            User = _user,
            Password = _pw
        };


        var result = FTP.ListFiles(input, default);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result.ListObject.Any(x => x.Name.Contains("testfile.txt")));
        Assert.IsTrue(result.Result.ListObject.Any(x => x.Name.Contains("usersdemo.txt")));
        Assert.IsTrue(!result.Result.ListObject.Any(x => x.Name.Contains("TestData")));
        Assert.IsTrue(!result.Result.ListObject.Any(x => x.Name.Contains("SFTPUploadTestFile.txt")));
    }

    /// <summary>
    /// List file(s) from subfolder
    /// </summary>
    [TestMethod]
    public void ListAllFromSubDirTest()
    {
        input = new Input()
        {
            Filename = null,
            Host = _host,
            Port = 21,
            Path = "/TestListsubdir",
            User = _user,
            Password = _pw
        };


        var result = FTP.ListFiles(input, default);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result.ListObject.Any(x => x.Name.Contains("InSubDir.txt")));
        Assert.IsTrue(!result.Result.ListObject.Any(x => x.Name.Contains("testfile.txt")));
        Assert.IsTrue(!result.Result.ListObject.Any(x => x.Name.Contains("usersdemo.txt")));
        Assert.IsTrue(!result.Result.ListObject.Any(x => x.Name.Contains("TestData")));
        Assert.IsTrue(!result.Result.ListObject.Any(x => x.Name.Contains("SFTPUploadTestFile.txt")));
    }

    /// <summary>
    /// Files not found. Returns only Success = false.
    /// </summary>
    [TestMethod]
    public void NoFilesTest()
    {
        input = new Input()
        {
            Filename = null,
            Host = _host,
            Port = 21,
            Path = "/NoFilesHere",
            User = _user,
            Password = _pw
        };


        var result = FTP.ListFiles(input, default);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result.ListObject.Any(x => x.Success.Equals(false)));
    }

    /// <summary>
    /// Test with filename. Should only return testfile.txt
    /// </summary>
    [TestMethod]
    public void ListWithFilenameTest()
    {
        input = new Input()
        {
            Filename = "testfile.txt",
            Host = _host,
            Port = 21,
            Path = "/",
            User = _user,
            Password = _pw
        };


        var result = FTP.ListFiles(input, default);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result.ListObject.Any(x => x.Name.Contains("testfile.txt")));
        Assert.IsTrue(!result.Result.ListObject.Any(x => x.Name.Contains("usersdemo.txt")));
        Assert.IsTrue(!result.Result.ListObject.Any(x => x.Name.Contains("TestData")));
        Assert.IsTrue(!result.Result.ListObject.Any(x => x.Name.Contains("SFTPUploadTestFile.txt")));
    }

    /// <summary>
    /// Test with filename containing wildcard. Should only return testfile.txt
    /// </summary>
    [TestMethod]
    public void ListWithFilenameWithWildcardTest()
    {
        input = new Input()
        {
            Filename = "test*",
            Host = _host,
            Port = 21,
            Path = "/",
            User = _user,
            Password = _pw
        };


        var result = FTP.ListFiles(input, default);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result.ListObject.Any(x => x.Name.Contains("testfile.txt")));
        Assert.IsTrue(!result.Result.ListObject.Any(x => x.Name.Contains("usersdemo.txt")));
        Assert.IsTrue(!result.Result.ListObject.Any(x => x.Name.Contains("TestData")));
        Assert.IsTrue(!result.Result.ListObject.Any(x => x.Name.Contains("SFTPUploadTestFile.txt")));
    }

    /// <summary>
    /// Without user cred.
    /// </summary>
    [TestMethod]
    public void MissingUserCredTest()
    {
        input = new Input()
        {
            Filename = "test*",
            Host = _host,
            Port = 21,
            Path = "/",
            User = "",
            Password = _pw
        };


        var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await FTP.ListFiles(input, default)).Result;
        Assert.IsTrue(ex.Message.Contains("Credentials required."));
    }

    /// <summary>
    /// Without password.
    /// </summary>
    [TestMethod]
    public void MissingPasswordTest()
    {
        input = new Input()
        {
            Filename = "test*",
            Host = _host,
            Port = 21,
            Path = "/",
            User = _user,
            Password = ""
        };


        var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await FTP.ListFiles(input, default)).Result;
        Assert.IsTrue(ex.Message.Contains("Credentials required."));
    }

    /// <summary>
    /// Without host.
    /// </summary>
    [TestMethod]
    public void MissingHostTest()
    {
        input = new Input()
        {
            Filename = "test*",
            Host = "",
            Port = 21,
            Path = "/",
            User = _user,
            Password = _pw
        };


        var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await FTP.ListFiles(input, default)).Result;
        Assert.IsTrue(ex.Message.Contains("Host required."));
    }

    /// <summary>
    /// Without required credentials.
    /// </summary>
    [TestMethod]
    public void WithoutUserAndPasswordTest()
    {
        input = new Input()
        {
            Filename = null,
            Host = _host,
            Port = 21,
            Path = "/",
            User = "",
            Password = ""
        };


        var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await FTP.ListFiles(input, default)).Result;
        Assert.IsTrue(ex.Message.Contains("Cannot determine the frame size or a corrupted frame was received"));
    }
}