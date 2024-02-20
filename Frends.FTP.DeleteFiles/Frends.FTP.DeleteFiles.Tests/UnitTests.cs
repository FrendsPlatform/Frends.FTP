namespace Frends.FTP.DeleteFiles.Tests;

using System;
using System.Linq;
using System.Threading.Tasks;
using FluentFTP;
using Frends.FTP.DeleteFiles.Definitions;
using Frends.FTP.DeleteFiles.Enums;
using Frends.FTP.DeleteFiles.Tests.Lib;
using NUnit.Framework;

[TestFixture]
internal class UnitTests : DeleteFilesTestBase
{
    [Test]
    public async Task DeleteFilesFTP_FilenameIsNUll_ListsAllFiles_Test()
    {
        input.FileMask = string.Empty;
        var result = await FTP.DeleteFiles(input, FtpHelper.GetFtpConnection(), default);

        Assert.AreEqual(5, result.Files.Count);

        Assert.IsTrue(result.Files.Any(x =>
            x.Contains("Test1.txt") ||
            x.Contains("Test1.xlsx") ||
            x.Contains("Test1.xml") ||
            x.Contains("testfile.txt") ||
            x.Contains("DemoTest.txt")));

        Assert.IsFalse(result.Files.Any(x =>
            x.Contains("_test.txt") ||
            x.Contains("pref_test.txt") ||
            x.Contains("pro_test.txt") ||
            x.Contains("pro_tet.txt") ||
            x.Contains("prof_test.txt")));
    }

    [Test]
    public async Task DeleteFilesFTP_FilenameWithWildcard_ListsAllFiles_Test()
    {
        var result = await FTP.DeleteFiles(input, FtpHelper.GetFtpConnection(), default);

        Assert.AreEqual(5, result.Files.Count);

        Assert.IsTrue(result.Files.Any(x =>
            x.Contains("Test1.txt") ||
            x.Contains("Test1.xlsx") ||
            x.Contains("Test1.xml") ||
            x.Contains("testfile.txt") ||
            x.Contains("DemoTest.txt")));

        Assert.IsFalse(result.Files.Any(x =>
            x.Contains("_test.txt") ||
            x.Contains("pref_test.txt") ||
            x.Contains("pro_test.txt") ||
            x.Contains("pro_tet.txt") ||
            x.Contains("prof_test.txt")));
    }

    [Test]
    public void DeleteFilesFTP_DirectoryDoNotExists_Test()
    {
        input = new Input()
        {
            FileMask = string.Empty,
            Directory = "/NoFilesHere",
        };

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await FTP.DeleteFiles(input, FtpHelper.GetFtpConnection(), default));

        Assert.AreEqual("Error occured while deleting files: FTP directory '/NoFilesHere' doesn't exist.\nDeleted files: ", ex.Message);
    }

    [Test]
    public async Task DeleteFilesFTP_NoFilesInDirectory_Test()
    {
        input = new Input()
        {
            FileMask = string.Empty,
            Directory = "/NoFilesHere",
        };

        FtpHelper.CreateDirectoryOnFTP(input.Directory);

        var result = await FTP.DeleteFiles(input, FtpHelper.GetFtpConnection(), default);

        Assert.AreEqual(0, result.Files.Count);

        FtpHelper.DeleteDirectoryOnFTP(input.Directory);
    }

    [Test]
    public async Task DeleteFilesFTPS_CompleteFileName_Test()
    {
        input = new Input()
        {
            FileMask = "Test1.txt",
            Directory = FtpDir,
        };

        var result = await FTP.DeleteFiles(input, FtpHelper.GetFtpsConnection(), default);

        Assert.AreEqual(1, result.Files.Count);
        Assert.IsTrue(result.Files.Any(x =>
            x.Contains("Test1.txt")));

        Assert.IsFalse(result.Files.Any(x =>
            x.Contains("Test1.xlsx") ||
            x.Contains("Test1.xml") ||
            x.Contains("testfile.txt") ||
            x.Contains("DemoTest.txt") ||
            x.Contains("_test.txt") ||
            x.Contains("pref_test.txt") ||
            x.Contains("pro_test.txt") ||
            x.Contains("pro_tet.txt") ||
            x.Contains("prof_test.txt")));
    }

    [Test]
    public async Task DeleteFilesFTPS_FilenameWithWildcard_Test()
    {
        input = new Input()
        {
            FileMask = "test*",
            Directory = FtpDir,
        };

        var result = await FTP.DeleteFiles(input, FtpHelper.GetFtpsConnection(), default);

        Assert.AreEqual(4, result.Files.Count);

        Assert.IsTrue(result.Files.Any(x =>
            x.Contains("Test1.txt") ||
            x.Contains("Test1.xlsx") ||
            x.Contains("Test1.xml") ||
            x.Contains("testfile.txt")));

        Assert.IsFalse(result.Files.Any(x =>
            x.Contains("DemoTest.txt") ||
            x.Contains("_test.txt") ||
            x.Contains("pref_test.txt") ||
            x.Contains("pro_test.txt") ||
            x.Contains("pro_tet.txt") ||
            x.Contains("prof_test.txt")));
    }

    [Test]
    public async Task DeleteFiles_FilenameWithRegex_Test()
    {
        input.FileMask = "Test1.(txt|xlsx)";

        var result = await FTP.DeleteFiles(input, FtpHelper.GetFtpConnection(), default);

        Assert.AreEqual(2, result.Files.Count);

        Assert.IsTrue(result.Files.Any(x =>
            x.Contains("Test1.txt") ||
            x.Contains("Test1.xlsx")));

        Assert.IsFalse(result.Files.Any(x =>
            x.Contains("Test1.xml") ||
            x.Contains("testfile.txt") ||
            x.Contains("DemoTest.txt") ||
            x.Contains("_test.txt") ||
            x.Contains("pref_test.txt") ||
            x.Contains("pro_test.txt") ||
            x.Contains("pro_tet.txt") ||
            x.Contains("prof_test.txt")));
    }

    [Test]
    public async Task DeleteFilesFTPS_FilenameWithRegex2_Test()
    {
        input.FileMask = "Test1.[^t][^x][^t]";

        var result = await FTP.DeleteFiles(input, FtpHelper.GetFtpsConnection(), default);

        Assert.AreEqual(1, result.Files.Count);
        Assert.IsTrue(result.Files.Any(x =>
            x.Contains("Test1.xml")));

        Assert.IsFalse(result.Files.Any(x =>
            x.Contains("Test1.txt") ||
            x.Contains("Test1.xlsx") ||
            x.Contains("testfile.txt") ||
            x.Contains("DemoTest.txt") ||
            x.Contains("_test.txt") ||
            x.Contains("pref_test.txt") ||
            x.Contains("pro_test.txt") ||
            x.Contains("pro_tet.txt") ||
            x.Contains("prof_test.txt")));
    }

    [Test]
    public async Task DeleteFilesFTPS_FilenameWithRegex3_Test()
    {
        input.FileMask = "<regex>^(?!prof).*_test.txt";
        input.Directory = $"{input.Directory}/Subfolder";

        FtpHelper.CreateDirectoryOnFTP(input.Directory);

        var files = new string[]
        {
            "_test.txt",
            "pref_test.txt",
            "pro_test.txt",
        };

        foreach (var file in files)
        {
            FtpHelper.CreateFileOnFTP(input.Directory, file);
        }

        var result = await FTP.DeleteFiles(input, FtpHelper.GetFtpsConnection(), default);

        Assert.AreEqual(3, result.Files.Count);

        Assert.IsTrue(result.Files.Any(x =>
            x.Contains("_test.txt") ||
            x.Contains("pref_test.txt") ||
            x.Contains("pro_test.txt")));

        Assert.IsFalse(result.Files.Any(x =>
            x.Contains("Test1.xml") ||
            x.Contains("Test1.txt") ||
            x.Contains("Test1.xlsx") ||
            x.Contains("testfile.txt") ||
            x.Contains("DemoTest.txt") ||
            x.Contains("pro_tet.txt") ||
            x.Contains("prof_test.txt")));

        FtpHelper.DeleteDirectoryOnFTP(input.Directory);
    }

    [Test]
    public void DeleteFilesFTP_UserIsNULLAndPasswordIsNotNull_Test()
    {
        var connection = FtpHelper.GetFtpConnection();
        connection.UserName = string.Empty;

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await FTP.DeleteFiles(input, connection, default));
        Assert.AreEqual("Error occured while deleting files: This is a private system - No anonymous login\nDeleted files: ", ex.Message);
    }

    [Test]
    public void DeleteFiles_PasswordIsNULLAndUserIsNotNull_Test()
    {
        var connection = FtpHelper.GetFtpConnection();
        connection.Password = string.Empty;

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await FTP.DeleteFiles(input, connection, default));
        Assert.AreEqual("Error occured while deleting files: Login authentication failed\nDeleted files: ", ex.Message);
    }

    [Test]
    public async Task DeleteFiles_HostIsEmpty_Test()
    {
        var connection = FtpHelper.GetFtpConnection();
        connection.Address = string.Empty;

        var result = await FTP.DeleteFiles(input, connection, default);
        Assert.AreEqual(5, result.Files.Count);
    }

    [Test]
    public void WithoutUserAndPasswordTest()
    {
        var connection = FtpHelper.GetFtpConnection();
        connection.UserName = string.Empty;
        connection.Password = string.Empty;

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await FTP.DeleteFiles(input, connection, default));
        Assert.AreEqual("Error occured while deleting files: This is a private system - No anonymous login\nDeleted files: ", ex.Message);
    }

    [Test]
    public async Task TestWithNoneSslModes()
    {
        var connection = FtpHelper.GetFtpsConnection();

        connection.SslMode = FtpsSslMode.None;
        var result = await FTP.DeleteFiles(input, connection, default);
        Assert.AreEqual(5, result.Files.Count);
    }

    [Test]
    public async Task TestWithExplicitSslModes()
    {
        var connection = FtpHelper.GetFtpsConnection();

        connection.SslMode = FtpsSslMode.Explicit;
        var result = await FTP.DeleteFiles(input, connection, default);
        Assert.AreEqual(5, result.Files.Count);
    }

    [Test]
    public async Task TestWithAutoSslModes()
    {
        var connection = FtpHelper.GetFtpsConnection();

        connection.SslMode = FtpsSslMode.Auto;
        var result = await FTP.DeleteFiles(input, connection, default);
        Assert.AreEqual(5, result.Files.Count);
    }

    [Test]
    public void TestWithCertificatePath()
    {
        // Our test certificate hashes:
        // SHA-256: 90:bc:7f:71:14:f5:c2:ad:03:46:d6:ff:75:d5:fe:12:ba:74:23:73:54:31:70:60:b4:8b:bd:8e:87:21:9c:16
        // SHA-1: d9:11:26:29:84:de:9c:c3:2a:35:18:a1:09:4c:d2:42:49:ea:5c:49
        var connection = new Connection
        {
            Address = FtpHelper.FtpHost,
            UserName = FtpHelper.FtpUsername,
            Password = FtpHelper.FtpPassword,
            Port = FtpHelper.FtpsPort,
            SslMode = FtpsSslMode.Explicit,
            EnableClientAuth = true,
            UseFTPS = true,
            ValidateAnyCertificate = false,
            CertificateHashStringSHA1 = string.Empty,
            ClientCertificatePath = string.Empty,
        };

        var ex = Assert.ThrowsAsync<ArgumentException>(async () => await FTP.DeleteFiles(input, connection, default));
        Assert.IsTrue(ex.Message.Contains("The remote certificate was rejected by the provided RemoteCertificateValidationCallback."));
    }
}
