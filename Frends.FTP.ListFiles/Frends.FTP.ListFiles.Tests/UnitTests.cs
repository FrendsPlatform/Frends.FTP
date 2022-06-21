
using Frends.FTP.ListFiles.Definitions;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace Frends.FTP.ListFiles.Tests;

[TestClass]
public class UnitTests
{
    /// <summary>
    /// Test files and directories created into Azure container. "Top dir" for this task's testing is /ListFiles.
    /// </summary>
    private readonly string? _host = Environment.GetEnvironmentVariable("HiQ_FTP_Host");
    private readonly string? _user = Environment.GetEnvironmentVariable("HiQ_FTP_User");
    private readonly string? _pw = Environment.GetEnvironmentVariable("HiQ_FTP_Password");
    private readonly string _path = "/ListFiles";

    Input? input;

    /// <summary>
    /// List all files from top dir using null as filename. Returns 5 files from top dir and skip sub dir files.
    /// </summary>
    [TestMethod]
    public void ListFiles_FilenameIsNUll_ListsAllFiles_Test()
    {
        input = new Input()
        {
            Filename = null,
            Host = _host,
            Port = 21,
            Path = _path,
            Username = _user,
            Password = _pw
        };


        var result = FTP.ListFiles(input, default);

        Assert.IsTrue(result.Result.Files.Count == 5);

        Assert.IsTrue(result.Result.Files.Any(x => 
            x.Name.Contains("Test1.txt") ||
            x.Name.Contains("Test1.xlsx") ||
            x.Name.Contains("Test1.xml") ||
            x.Name.Contains("testfile.txt") ||
            x.Name.Contains("DemoTest.txt")
        ));

        Assert.IsFalse(result.Result.Files.Any(x => 
            x.Name.Contains("_test.txt") ||
            x.Name.Contains("pref_test.txt") ||
            x.Name.Contains("pro_test.txt") ||
            x.Name.Contains("pro_tet.txt") ||
            x.Name.Contains("prof_test.txt")
            ));
    }

    /// <summary>
    /// List all files from top dir using wildcard * as filename. Returns 5 files from top dir and skip sub dir files.
    /// Filename is *.
    /// </summary>
    [TestMethod]
    public void ListFiles_FilenameWithWildcard_ListsAllFiles_Test()
    {
        input = new Input()
        {
            Filename = "*",
            Host = _host,
            Port = 21,
            Path = _path,
            Username = _user,
            Password = _pw
        };


        var result = FTP.ListFiles(input, default);

        Assert.IsTrue(result.Result.Files.Count == 5);

        Assert.IsTrue(result.Result.Files.Any(x =>
            x.Name.Contains("Test1.txt") ||
            x.Name.Contains("Test1.xlsx") ||
            x.Name.Contains("Test1.xml") ||
            x.Name.Contains("testfile.txt") ||
            x.Name.Contains("DemoTest.txt")
        ));

        Assert.IsFalse(result.Result.Files.Any(x =>
            x.Name.Contains("_test.txt") ||
            x.Name.Contains("pref_test.txt") ||
            x.Name.Contains("pro_test.txt") ||
            x.Name.Contains("pro_tet.txt") ||
            x.Name.Contains("prof_test.txt")
            ));
    }

    /// <summary>
    /// List file from /Subfolder. Returns 5 files and skips top dir files.
    /// </summary>
    [TestMethod]
    public void ListFiles_ListsAllFilesFromSubdir_Test()
    {
        input = new Input()
        {
            Filename = null,
            Host = _host,
            Port = 21,
            Path = _path,
            Username = _user,
            Password = _pw
        };

        var result = FTP.ListFiles(input, default);

        Assert.IsTrue(result.Result.Files.Count == 5);

        Assert.IsTrue(result.Result.Files.Any(x =>
            x.Name.Contains("Test1.txt") ||
            x.Name.Contains("Test1.xlsx") ||
            x.Name.Contains("Test1.xml") ||
            x.Name.Contains("testfile.txt") ||
            x.Name.Contains("DemoTest.txt")
        ));

        Assert.IsFalse(result.Result.Files.Any(x =>
            x.Name.Contains("_test.txt") ||
            x.Name.Contains("pref_test.txt") ||
            x.Name.Contains("pro_test.txt") ||
            x.Name.Contains("pro_tet.txt") ||
            x.Name.Contains("prof_test.txt")
            ));
    }

    /// <summary>
    /// Given path doesn't contain any files. Making sure that this doesn't cause any errors.
    /// </summary>
    [TestMethod]
    public void ListFiles_NoFilesInPath_Test()
    {
        input = new Input()
        {
            Filename = null,
            Host = _host,
            Port = 21,
            Path = "/NoFilesHere",
            Username = _user,
            Password = _pw
        };

        var result = FTP.ListFiles(input, default);

        Assert.IsTrue(result.Result.Files.Count == 0);
    }

    /// <summary>
    /// Test with complete filename. Returns Test1.txt.
    /// </summary>
    [TestMethod]
    public void ListFiles_CompleteFileName_Test()
    {
        input = new Input()
        {
            Filename = "Test1.txt",
            Host = _host,
            Port = 21,
            Path = _path,
            Username = _user,
            Password = _pw
        };

        var result = FTP.ListFiles(input, default);

        Assert.IsTrue(result.Result.Files.Count == 1 && result.Result.Files.Any(x =>
            x.Name.Contains("Test1.txt")
        ));

        Assert.IsFalse(result.Result.Files.Any(x =>
            x.Name.Contains("Test1.xlsx") ||
            x.Name.Contains("Test1.xml") ||
            x.Name.Contains("testfile.txt") ||
            x.Name.Contains("DemoTest.txt") ||
            x.Name.Contains("_test.txt") ||
            x.Name.Contains("pref_test.txt") ||
            x.Name.Contains("pro_test.txt") ||
            x.Name.Contains("pro_tet.txt") ||
            x.Name.Contains("prof_test.txt")
            ));
    }

    /// <summary>
    /// Test with filename containing wildcard. Result contains 4 files.
    /// </summary>
    [TestMethod]
    public void ListFiles_FilenameWithWildcard_Test()
    {
        input = new Input()
        {
            Filename = "test*",
            Host = _host,
            Port = 21,
            Path = _path,
            Username = _user,
            Password = _pw
        };

        var result = FTP.ListFiles(input, default);

        Assert.IsTrue(result.Result.Files.Count == 4);

        Assert.IsTrue(result.Result.Files.Any(x =>
            x.Name.Contains("Test1.txt") ||
            x.Name.Contains("Test1.xlsx") ||
            x.Name.Contains("Test1.xml") ||
            x.Name.Contains("testfile.txt")
        ));

        Assert.IsFalse(result.Result.Files.Any(x =>
            x.Name.Contains("DemoTest.txt") ||
            x.Name.Contains("_test.txt") ||
            x.Name.Contains("pref_test.txt") ||
            x.Name.Contains("pro_test.txt") ||
            x.Name.Contains("pro_tet.txt") ||
            x.Name.Contains("prof_test.txt")
            ));
    }

    /// <summary>
    /// Test with filename containing regex. Returns 2 files Test1.txt and Test1.xlsx.
    /// </summary>
    [TestMethod]
    public void ListFiles_FilenameWithRegex_Test()
    {
        input = new Input()
        {
            Filename = "Test1.(txt|xlsx)",
            Host = _host,
            Port = 21,
            Path = _path,
            Username = _user,
            Password = _pw
        };

        var result = FTP.ListFiles(input, default);

        Assert.IsTrue(result.Result.Files.Count == 2);

        Assert.IsTrue(result.Result.Files.Any(x =>
            x.Name.Contains("Test1.txt") ||
            x.Name.Contains("Test1.xlsx")
        ));

        Assert.IsFalse(result.Result.Files.Any(x =>
            x.Name.Contains("Test1.xml") ||
            x.Name.Contains("testfile.txt") ||
            x.Name.Contains("DemoTest.txt") ||
            x.Name.Contains("_test.txt") ||
            x.Name.Contains("pref_test.txt") ||
            x.Name.Contains("pro_test.txt") ||
            x.Name.Contains("pro_tet.txt") ||
            x.Name.Contains("prof_test.txt")
            ));
    }

    /// <summary>
    /// Test with filename containing regex. Returns 1 file Test1.xml.
    /// </summary>
    [TestMethod]
    public void ListFiles_FilenameWithRegex2_Test()
    {
        input = new Input()
        {
            Filename = "Test1.[^t][^x][^t]",
            Host = _host,
            Port = 21,
            Path = _path,
            Username = _user,
            Password = _pw
        };

        var result = FTP.ListFiles(input, default);

        Assert.IsTrue(result.Result.Files.Count == 1 && result.Result.Files.Any(x =>
            x.Name.Contains("Test1.xml")
        ));

        Assert.IsFalse(result.Result.Files.Any(x =>
            x.Name.Contains("Test1.txt") ||
            x.Name.Contains("Test1.xlsx") ||
            x.Name.Contains("testfile.txt") ||
            x.Name.Contains("DemoTest.txt") ||
            x.Name.Contains("_test.txt") ||
            x.Name.Contains("pref_test.txt") ||
            x.Name.Contains("pro_test.txt") ||
            x.Name.Contains("pro_tet.txt") ||
            x.Name.Contains("prof_test.txt")
            ));
    }

    /// <summary>
    /// List files from subfolder /Subfolder using regex. Result contains 3 files pro_test.txt, pref_test.txt, _test.txt 
    /// and files prof_test.txt, pro_tet.txt. 
    /// </summary>
    [TestMethod]
    public void ListFiles_FilenameWithRegex3_Test()
    {
        input = new Input()
        {
            Filename = "<regex>^(?!prof).*_test.txt",
            Host = _host,
            Port = 21,
            Path = $"{_path}/Subfolder",
            Username = _user,
            Password = _pw
        };

        var result = FTP.ListFiles(input, default);

        Assert.IsTrue(result.Result.Files.Count == 3);

        Assert.IsTrue(result.Result.Files.Any(x =>
            x.Name.Contains("_test.txt") ||
            x.Name.Contains("pref_test.txt") ||
            x.Name.Contains("pro_test.txt")
        ));

        Assert.IsFalse(result.Result.Files.Any(x =>
            x.Name.Contains("Test1.xml") ||
            x.Name.Contains("Test1.txt") ||
            x.Name.Contains("Test1.xlsx") ||
            x.Name.Contains("testfile.txt") ||
            x.Name.Contains("DemoTest.txt") ||
            x.Name.Contains("pro_tet.txt") ||
            x.Name.Contains("prof_test.txt")
            ));
    }

    /// <summary>
    /// Test without username when password is not null. Returns an error because password is not null.
    /// </summary>
    [TestMethod]
    public void ListFiles_UserIsNULLAndPasswordIsNotNull_Test()
    {
        input = new Input()
        {
            Filename = "test*",
            Host = _host,
            Port = 21,
            Path = _path,
            Username = "",
            Password = _pw
        };

        var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await FTP.ListFiles(input, default)).Result;
        Assert.IsTrue(ex.Message.Contains("Username required."));
    }

    /// <summary>
    /// Test without password when username is not null. Returns an error because username is not null.
    /// </summary>
    [TestMethod]
    public void ListFiles_PasswordIsNULLAndUserIsNotNull_Test()
    {
        input = new Input()
        {
            Filename = "test*",
            Host = _host,
            Port = 21,
            Path = _path,
            Username = _user,
            Password = ""
        };

        var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await FTP.ListFiles(input, default)).Result;
        Assert.IsTrue(ex.Message.Contains("Password required."));
    }

    /// <summary>
    /// Test without host. Returns an error.
    /// </summary>
    [TestMethod]
    public void ListFiles_HostIsNULL_Test()
    {
        input = new Input()
        {
            Filename = "test*",
            Host = "",
            Port = 21,
            Path = _path,
            Username = _user,
            Password = _pw
        };

        var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await FTP.ListFiles(input, default)).Result;
        Assert.IsTrue(ex.Message.Contains("Host required."));
    }

    /// <summary>
    /// Test without required (FTP server) credentials.
    /// </summary>
    [TestMethod]
    public void WithoutUserAndPasswordTest()
    {
        input = new Input()
        {
            Filename = null,
            Host = _host,
            Port = 21,
            Path = _path,
            Username = "",
            Password = ""
        };

        var ex = Assert.ThrowsExceptionAsync<Exception>(async () => await FTP.ListFiles(input, default)).Result;
        Assert.IsTrue(ex.Message.Contains("Error when creating a list of files"));
    }
}