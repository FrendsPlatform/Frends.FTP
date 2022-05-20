
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
        Assert.IsTrue(result.Result.Files.Any(x => x.Name.Contains("testfile.txt")));
        Assert.IsTrue(result.Result.Files.Any(x => x.Name.Contains("usersdemo.txt")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("TestData")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("SFTPUploadTestFile.txt")));
    }

    /// <summary>
    /// List all files. Checking result count.
    /// </summary>
    [TestMethod]
    public void ListAll2Test()
    {
        input = new Input()
        {
            Filename = "*",
            Host = _host,
            Port = 21,
            Path = "/",
            User = _user,
            Password = _pw
        };


        var result = FTP.ListFiles(input, default);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result.Files.Any(x => x.Name.Contains("testfile.txt")));
        Assert.IsTrue(result.Result.Files.Any(x => x.Name.Contains("usersdemo.txt")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("TestData")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("SFTPUploadTestFile.txt")));
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
        Assert.IsTrue(result.Result.Files.Any(x => x.Name.Contains("InSubDir.txt")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("testfile.txt")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("usersdemo.txt")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("TestData")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("SFTPUploadTestFile.txt")));
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
        Assert.IsTrue(result.Result.Files.Any(x => x.Success.Equals(false)));
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
        Assert.IsTrue(result.Result.Files.Any(x => x.Name.Contains("testfile.txt")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("usersdemo.txt")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("TestData")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("SFTPUploadTestFile.txt")));
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
        Assert.IsTrue(result.Result.Files.Any(x => x.Name.Contains("testfile.txt")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("usersdemo.txt")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("TestData")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("SFTPUploadTestFile.txt")));
    }


    /// <summary>
    /// Test with filename containing regex. Should only return Test1.txt and Test1.xlsx.
    /// </summary>
    [TestMethod]
    public void ListWitPrefixTest()
    {
        input = new Input()
        {
            Filename = "Test1.(txt|xlsx)",
            Host = _host,
            Port = 21,
            Path = "/",
            User = _user,
            Password = _pw
        };


        var result = FTP.ListFiles(input, default);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result.Files.Any(x => x.Name.Contains("Test1.txt")));
        Assert.IsTrue(result.Result.Files.Any(x => x.Name.Contains("Test1.xlsx")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("usersdemo.txt")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("TestData")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("SFTPUploadTestFile.txt")));
    }

    /// <summary>
    /// List only Test1.xml.
    /// </summary>
    [TestMethod]
    public void ListWitPrefix2Test()
    {
        input = new Input()
        {
            Filename = "Test1.[^t][^x][^t]",
            Host = _host,
            Port = 21,
            Path = "/",
            User = _user,
            Password = _pw
        };


        var result = FTP.ListFiles(input, default);
        Assert.IsNotNull(result);
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("Test1.txt")));
        Assert.IsTrue(result.Result.Files.Any(x => x.Name.Contains("Test1.xml")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("usersdemo.txt")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("TestData")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("SFTPUploadTestFile.txt")));
    }

    /// <summary>
    /// List pro_test.txt, pref_test.txt, _test.txt and skip prof_test.txt, pro_tet.txt. 
    /// </summary>
    [TestMethod]
    public void ListWitPrefix3Test()
    {
        input = new Input()
        {
            Filename = "<regex>^(?!prof).*_test.txt",
            Host = _host,
            Port = 21,
            Path = "/Pro",
            User = _user,
            Password = _pw
        };


        var result = FTP.ListFiles(input, default);
        Assert.IsNotNull(result);
        Assert.IsTrue(result.Result.Files.Any(x => x.Name.Contains("pro_test.txt")));
        Assert.IsTrue(result.Result.Files.Any(x => x.Name.Contains("pref_test.txt")));
        Assert.IsTrue(result.Result.Files.Any(x => x.Name.Contains("_test.txt")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("prof_test.txt")));
        Assert.IsTrue(!result.Result.Files.Any(x => x.Name.Contains("pro_tet.txt")));
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