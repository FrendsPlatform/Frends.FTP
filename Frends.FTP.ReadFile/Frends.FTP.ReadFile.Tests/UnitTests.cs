using Frends.FTP.ReadFile.Definitions;
using Frends.FTP.ReadFile.Enums;
using Frends.FTP.ReadFile.Tests.Lib;
using NUnit.Framework;
using System.Security.Authentication;
using System.Text;

namespace Frends.FTP.ReadFile.Tests;

[TestFixture]
public class UnitTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Input _input;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private string _dir = "/upload";
    private readonly string _content = "This is a test file qwertyuiopåasdfghjklöäzxcvbnm";

    [SetUp]
    public void SetUp()
    {
        _input = new Input
        {
            Path = Path.Combine(_dir, $"{Guid.NewGuid()}.txt").Replace("\\", "/"),
            FileEncoding = FileEncoding.UTF8,
            EnableBom = true
        };

        TestHelpers.UploadDummyFile(_input.Path, _content);
    }

    [TearDown]
    public void Teardown()
    {
        var connection = TestHelpers.GetFtpConnection();
        using var client = FTP.CreateFtpClient(connection);
        client.Connect();

        if (client.DirectoryExists(_dir))
            client.DeleteDirectory(_dir);

        client.Disconnect();
        client.Dispose();
    }

    [Test]
    public void ReadFile_ReadTestFTP()
    {
        var connection = TestHelpers.GetFtpConnection();
        var result = FTP.ReadFile(_input, connection, new Options(), default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(TestHelpers.CheckThatFileExistsInServer(_input.Path));
        Assert.AreEqual(_content, result.Content);
    }

    [Test]
    public void ReadFile_ReadTestFTPS()
    {
        var connection = TestHelpers.GetFtpsConnection();
        var result = FTP.ReadFile(_input, connection, new Options(), default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(TestHelpers.CheckThatFileExistsInServer(_input.Path));
        Assert.AreEqual(_content, result.Content);
    }

    [Test]
    public void ReadFile_TestThrowsWhenFileNotExistFTP()
    {
        var connection = TestHelpers.GetFtpsConnection();
        _input.Path = "file/that/dont/exists.txt";
        var ex = Assert.Throws<ArgumentException>(() => FTP.ReadFile(_input, connection, new Options(), default));
        Assert.AreEqual($"Could not find file '{_input.Path}'.", ex.Message);
    }

    [Test]
    public void ReadFile_TestWithASCIIFTP()
    {
        var connection = TestHelpers.GetFtpsConnection();
        _input.FileEncoding = FileEncoding.ASCII;
        var result = FTP.ReadFile(_input, connection, new Options(), default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(TestHelpers.CheckThatFileExistsInServer(_input.Path));
        Assert.AreEqual(Encoding.ASCII.GetString(Encoding.UTF8.GetBytes(_content)), result.Content);
    }

    [Test]
    public void ReadFile_TestWithANSIFTP()
    {
        var connection = TestHelpers.GetFtpsConnection();
        _input.FileEncoding = FileEncoding.ANSI;
        var result = FTP.ReadFile(_input, connection, new Options(), default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(TestHelpers.CheckThatFileExistsInServer(_input.Path));
        Assert.AreEqual(_content, result.Content);
        Assert.AreEqual(Encoding.Default.GetString(Encoding.UTF8.GetBytes(_content)), result.Content);
    }

    [Test]
    public void ReadFile_TestWithWINDOWS1252FTP()
    {
        var connection = TestHelpers.GetFtpsConnection();
        _input.FileEncoding = FileEncoding.WINDOWS1252;
        var result = FTP.ReadFile(_input, connection, new Options(), default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(TestHelpers.CheckThatFileExistsInServer(_input.Path));
        Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
        var win = Encoding.GetEncoding("windows-1252");
        Assert.AreEqual(win.GetString(Encoding.UTF8.GetBytes(_content)), result.Content);
    }

    [Test]
    public void ReadFile_TestWithEncodingOtherFTP()
    {
        var connection = TestHelpers.GetFtpsConnection();
        _input.FileEncoding = FileEncoding.Other;
        _input.EncodingInString = "iso-8859-1";
        var result = FTP.ReadFile(_input, connection, new Options(), default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(TestHelpers.CheckThatFileExistsInServer(_input.Path));
        Assert.AreEqual(Encoding.Latin1.GetString(Encoding.UTF8.GetBytes(_content)), result.Content);
    }

    [Test]
    public void ReadFile_TestThrowsWithIncorrectFingerprint()
    {
        var connection = new Connection
        {
            Address = TestHelpers.FtpHost,
            UserName = TestHelpers.FtpUsername,
            Password = TestHelpers.FtpPassword,
            Port = TestHelpers.FtpsPort,
            SslMode = FtpsSslMode.Explicit,
            UseFTPS = true,
            CertificateHashStringSHA1 = "nope"
        };

        // Test and assert
        var ex = Assert.Throws<AggregateException>(() =>
        {
            var result = FTP.ReadFile(_input, connection, new Options(), default);
        });

        Assert.AreEqual(1, ex.InnerExceptions.Count);
        Assert.AreEqual(typeof(AuthenticationException), ex.InnerExceptions[0].GetType());
    }

    [Test]
    public void UploadFTPS_CorrectFingerprint()
    {
        var connection = new Connection
        {
            Address = TestHelpers.FtpHost,
            UserName = TestHelpers.FtpUsername,
            Password = TestHelpers.FtpPassword,
            Port = TestHelpers.FtpPort,
            SslMode = FtpsSslMode.Explicit,
            UseFTPS = true,
            CertificateHashStringSHA1 = "D911262984DE9CC32A3518A1094CD24249EA5C49"
        };
        var result = FTP.ReadFile(_input, connection, new Options(), default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(TestHelpers.CheckThatFileExistsInServer(_input.Path));
        Assert.AreEqual(_content, result.Content);
    }
}