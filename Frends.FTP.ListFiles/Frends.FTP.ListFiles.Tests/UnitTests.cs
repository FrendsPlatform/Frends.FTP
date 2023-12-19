using FluentFTP;
using Frends.FTP.ListFiles.Definitions;
using Frends.FTP.ListFiles.Tests.Lib;
using NUnit.Framework;

namespace Frends.FTP.ListFiles.Tests;

/// <summary>
/// To run these tests you need to run `docker-compose -f Frends.FTP.ListFiles.Tests/docker-compose.yml up -d`
/// </summary>
[TestFixture]
public class UnitTests : ListFilesTestBase
{
    /// <summary>
    /// List all files from top dir using null as filename. Returns 5 files from top dir and skip sub dir files.
    /// </summary>
    [Test]
    public async Task ListFilesFTP_FilenameIsNUll_ListsAllFiles_Test()
    {
        input.FileMask = "";
        var result = await FTP.ListFiles(input, FtpHelper.GetFtpConnection(), default);

        Assert.AreEqual(5, result.Files.Count);

        Assert.IsTrue(result.Files.Any(x =>
            x.Name.Contains("Test1.txt") ||
            x.Name.Contains("Test1.xlsx") ||
            x.Name.Contains("Test1.xml") ||
            x.Name.Contains("testfile.txt") ||
            x.Name.Contains("DemoTest.txt")
        ));

        Assert.IsFalse(result.Files.Any(x =>
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
    [Test]
    public async Task ListFilesFTP_FilenameWithWildcard_ListsAllFiles_Test()
    {
        var result = await FTP.ListFiles(input, FtpHelper.GetFtpConnection(), default);

        Assert.AreEqual(5, result.Files.Count);

        Assert.IsTrue(result.Files.Any(x =>
            x.Name.Contains("Test1.txt") ||
            x.Name.Contains("Test1.xlsx") ||
            x.Name.Contains("Test1.xml") ||
            x.Name.Contains("testfile.txt") ||
            x.Name.Contains("DemoTest.txt")
        ));

        Assert.IsFalse(result.Files.Any(x =>
            x.Name.Contains("_test.txt") ||
            x.Name.Contains("pref_test.txt") ||
            x.Name.Contains("pro_test.txt") ||
            x.Name.Contains("pro_tet.txt") ||
            x.Name.Contains("prof_test.txt")
            ));
    }

    /// <summary>
    /// Given path doesn't exist. Making sure that this throws error.
    /// </summary>
    [Test]
    public void ListFilesFTP_DirectoryDoNotExists_Test()
    {
        input = new Input()
        {
            FileMask = "",
            Directory = "/NoFilesHere"
        };

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await FTP.ListFiles(input, FtpHelper.GetFtpConnection(), default));

        Assert.AreEqual("FTP directory '/NoFilesHere' doesn't exist.", ex.Message);
    }

    /// <summary>
    /// Given Directory doesn't contain any files. Making sure that this doesn't cause any errors.
    /// </summary>
    [Test]
    public async Task ListFilesFTP_NoFilesInDirectory_Test()
    {
        input = new Input()
        {
            FileMask = "",
            Directory = "/NoFilesHere"
        };

        FtpHelper.CreateDirectoryOnFTP(input.Directory);

        var result = await FTP.ListFiles(input, FtpHelper.GetFtpConnection(), default);

        Assert.AreEqual(0, result.Files.Count);

        FtpHelper.DeleteDirectoryOnFTP(input.Directory);
    }

    /// <summary>
    /// Test with complete filename. Returns Test1.txt.
    /// </summary>
    [Test]
    public async Task ListFilesFTPS_CompleteFileName_Test()
    {
        input = new Input()
        {
            FileMask = "Test1.txt",
            Directory = FtpDir
        };

        var result = await FTP.ListFiles(input, FtpHelper.GetFtpsConnection(), default);

        Assert.AreEqual(1, result.Files.Count);
        Assert.IsTrue(result.Files.Any(x =>
            x.Name.Contains("Test1.txt")
        ));

        Assert.IsFalse(result.Files.Any(x =>
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
    [Test]
    public async Task ListFilesFTPS_FilenameWithWildcard_Test()
    {
        input = new Input()
        {
            FileMask = "test*",
            Directory = FtpDir,
        };

        var result = await FTP.ListFiles(input, FtpHelper.GetFtpsConnection(), default);

        Assert.AreEqual(4, result.Files.Count);

        Assert.IsTrue(result.Files.Any(x =>
            x.Name.Contains("Test1.txt") ||
            x.Name.Contains("Test1.xlsx") ||
            x.Name.Contains("Test1.xml") ||
            x.Name.Contains("testfile.txt")
        ));

        Assert.IsFalse(result.Files.Any(x =>
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
    [Test]
    public async Task ListFiles_FilenameWithRegex_Test()
    {
        input.FileMask = "Test1.(txt|xlsx)";

        var result = await FTP.ListFiles(input, FtpHelper.GetFtpConnection(), default);

        Assert.AreEqual(2, result.Files.Count);

        Assert.IsTrue(result.Files.Any(x =>
            x.Name.Contains("Test1.txt") ||
            x.Name.Contains("Test1.xlsx")
        ));

        Assert.IsFalse(result.Files.Any(x =>
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
    [Test]
    public async Task ListFilesFTPS_FilenameWithRegex2_Test()
    {
        input.FileMask = "Test1.[^t][^x][^t]";

        var result = await FTP.ListFiles(input, FtpHelper.GetFtpsConnection(), default);

        Assert.AreEqual(1, result.Files.Count);
        Assert.IsTrue(result.Files.Any(x =>
            x.Name.Contains("Test1.xml")
        ));

        Assert.IsFalse(result.Files.Any(x =>
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
    [Test]
    public async Task ListFilesFTPS_FilenameWithRegex3_Test()
    {
        input.FileMask = "<regex>^(?!prof).*_test.txt";
        input.Directory = $"{input.Directory}/Subfolder";

        FtpHelper.CreateDirectoryOnFTP(input.Directory);

        var files = new string[]
        {
            "_test.txt",
            "pref_test.txt",
            "pro_test.txt"
        };

        foreach (var file in files)
        {
            FtpHelper.CreateFileOnFTP(input.Directory, file);
        }

        var result = await FTP.ListFiles(input, FtpHelper.GetFtpsConnection(), default);

        Assert.AreEqual(3, result.Files.Count);

        Assert.IsTrue(result.Files.Any(x =>
            x.Name.Contains("_test.txt") ||
            x.Name.Contains("pref_test.txt") ||
            x.Name.Contains("pro_test.txt")
        ));

        Assert.IsFalse(result.Files.Any(x =>
            x.Name.Contains("Test1.xml") ||
            x.Name.Contains("Test1.txt") ||
            x.Name.Contains("Test1.xlsx") ||
            x.Name.Contains("testfile.txt") ||
            x.Name.Contains("DemoTest.txt") ||
            x.Name.Contains("pro_tet.txt") ||
            x.Name.Contains("prof_test.txt")
            ));

        FtpHelper.DeleteDirectoryOnFTP(input.Directory);
    }

    /// <summary>
    /// Test without username when password is not null. Returns an error because password is not null.
    /// </summary>
    [Test]
    public void ListFilesFTP_UserIsNULLAndPasswordIsNotNull_Test()
    {
        var connection = FtpHelper.GetFtpConnection();
        connection.UserName = string.Empty;

        var ex = Assert.ThrowsAsync<FtpAuthenticationException>(async () => await FTP.ListFiles(input, connection, default));
        Assert.AreEqual("This is a private system - No anonymous login", ex.Message);
    }

    /// <summary>
    /// Test without password when username is not null. Returns an error because username is not null.
    /// </summary>
    [Test]
    public void ListFiles_PasswordIsNULLAndUserIsNotNull_Test()
    {
        var connection = FtpHelper.GetFtpConnection();
        connection.Password = string.Empty;

        var ex = Assert.ThrowsAsync<FtpAuthenticationException>(async () => await FTP.ListFiles(input, connection, default));
        Assert.AreEqual("Login authentication failed", ex.Message);
    }

    /// <summary>
    /// Test without host. Returns an error.
    /// </summary>
    [Test]
    public async Task ListFiles_HostIsNULL_Test()
    {
        var connection = FtpHelper.GetFtpConnection();
        connection.Address = string.Empty;

        var result = await FTP.ListFiles(input, connection, default);
        Assert.AreEqual(0, result.Files.Count);

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await FTP.ListFiles(input, connection, default));
        Assert.AreEqual("Unable to establish the socket: No such host is known.", ex.Message);
    }

    /// <summary>
    /// Test without required (FTP server) credentials.
    /// </summary>
    [Test]
    public void WithoutUserAndPasswordTest()
    {
        var connection = FtpHelper.GetFtpConnection();
        connection.UserName = string.Empty;
        connection.Password = string.Empty;

        var ex = Assert.ThrowsAsync<FtpAuthenticationException>(async () => await FTP.ListFiles(input, connection, default));
        Assert.AreEqual("This is a private system - No anonymous login", ex.Message);
    }
}