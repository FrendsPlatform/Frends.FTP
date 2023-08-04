using Frends.FTP.ReadFile.Definitions;
using Frends.FTP.ReadFile.Tests.Lib;
using NUnit.Framework;
using System.Security.Authentication;

namespace Frends.FTP.ReadFile.Tests;

[TestFixture]
public class UnitTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Input _input;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private string _dir = "/upload";
    private readonly string _content = "This is a test file qwertyuiop�asdfghjkl��zxcvbnm";

    [SetUp]
    public void SetUp()
    {
        _input = new Input
        {
            Path = Path.Combine(_dir, $"{Guid.NewGuid()}.txt").Replace("\\", "/"),
            FileEncoding = Enums.FileEncoding.UTF8,
            EnableBom = true
        };

        Helpers.UploadDummyFile(_input.Path, _content);
    }

    [TearDown]
    public void Teardown()
    {
        var connection = Helpers.GetFtpConnection();
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
        var connection = Helpers.GetFtpConnection();
        var result = FTP.ReadFile(connection, _input, default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(Helpers.CheckThatFileExistsInServer(_input.Path));
        Assert.AreEqual(_content, result.Content);
    }

    [Test]
    public void ReadFile_ReadTestFTPS()
    {
        var connection = Helpers.GetFtpsConnection();
        var result = FTP.ReadFile(connection, _input, default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(Helpers.CheckThatFileExistsInServer(_input.Path));
        Assert.AreEqual(_content, result.Content);
    }

    [Test]
    public void ReadFile_TestThrowsWhenFileNotExistFTP()
    {
        var connection = Helpers.GetFtpsConnection();
        _input.Path = "file/that/dont/exists.txt";
        var ex = Assert.Throws<ArgumentException>(() => FTP.ReadFile(connection, _input, default));
        Assert.AreEqual($"Could not find file '{_input.Path}'.", ex.Message);
    }

    [Test]
    public void ReadFile_TestWithASCIIFTP()
    {
        var connection = Helpers.GetFtpsConnection();
        _input.FileEncoding = Enums.FileEncoding.ASCII;
        var result = FTP.ReadFile(connection, _input, default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(Helpers.CheckThatFileExistsInServer(_input.Path));
        Assert.AreEqual("This is a test file qwertyuiop??asdfghjkl????zxcvbnm", result.Content);
    }

    [Test]
    public void ReadFile_TestWithANSIFTP()
    {
        var connection = Helpers.GetFtpsConnection();
        _input.FileEncoding = Enums.FileEncoding.ANSI;
        var result = FTP.ReadFile(connection, _input, default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(Helpers.CheckThatFileExistsInServer(_input.Path));
        Assert.AreEqual(_content, result.Content);
    }

    [Test]
    public void ReadFile_TestWithWINDOWS1252FTP()
    {
        var connection = Helpers.GetFtpsConnection();
        _input.FileEncoding = Enums.FileEncoding.WINDOWS1252;
        var result = FTP.ReadFile(connection, _input, default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(Helpers.CheckThatFileExistsInServer(_input.Path));
        Assert.AreEqual("This is a test file qwertyuiopåasdfghjklöäzxcvbnm", result.Content);
    }

    [Test]
    public void ReadFile_TestWithEncodingOtherFTP()
    {
        var connection = Helpers.GetFtpsConnection();
        _input.FileEncoding = Enums.FileEncoding.Other;
        _input.EncodingInString = "iso-8859-1";
        var result = FTP.ReadFile(connection, _input, default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(Helpers.CheckThatFileExistsInServer(_input.Path));
        Assert.AreEqual("This is a test file qwertyuiopåasdfghjklöäzxcvbnm", result.Content);
    }

    [Test]
    public void ReadFile_TestThrowsWithIncorrectFingerprint()
    {
        var connection = new Connection
        {
            Address = Helpers.FtpHost,
            UserName = Helpers.FtpUsername,
            Password = Helpers.FtpPassword,
            Port = Helpers.FtpsPort,
            SslMode = FtpsSslMode.Explicit,
            UseFTPS = true,
            CertificateHashStringSHA1 = "nope"
        };

        // Test and assert
        var ex = Assert.Throws<AggregateException>(() =>
        {
            var result = FTP.ReadFile(connection, _input, default);
        });

        Assert.AreEqual(1, ex.InnerExceptions.Count);
        Assert.AreEqual(typeof(AuthenticationException), ex.InnerExceptions[0].GetType());
    }

    [Test]
    public void UploadFTPS_CorrectFingerprint()
    {
        var connection = new Connection
        {
            Address = Helpers.FtpHost,
            UserName = Helpers.FtpUsername,
            Password = Helpers.FtpPassword,
            Port = Helpers.FtpPort,
            SslMode = FtpsSslMode.Explicit,
            UseFTPS = true,
            CertificateHashStringSHA1 = "D911262984DE9CC32A3518A1094CD24249EA5C49"
        };
        var result = FTP.ReadFile(connection, _input, default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(Helpers.CheckThatFileExistsInServer(_input.Path));
        Assert.AreEqual(_content, result.Content);
    }
}