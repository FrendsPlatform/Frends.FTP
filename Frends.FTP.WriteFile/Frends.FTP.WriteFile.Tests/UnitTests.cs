using FluentFTP;
using Frends.FTP.WriteFile.Definitions;
using Frends.FTP.WriteFile.Enums;
using Frends.FTP.WriteFile.Tests.Lib;
using NUnit.Framework;
using System.Security.Authentication;

namespace Frends.FTP.WriteFile.Tests;

[TestFixture]
public class UnitTests
{
#pragma warning disable CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private Input _input;
#pragma warning restore CS8618 // Non-nullable field must contain a non-null value when exiting constructor. Consider declaring as nullable.
    private string _dir = "/upload";

    [SetUp]
    public void SetUp()
    {
        _input = new Input
        {
            Content = "This is a test file",
            Path = Path.Combine(_dir, $"{Guid.NewGuid()}.txt").Replace("\\", "/"),
            CreateDestinationDirectories = true,
            FileEncoding = FileEncoding.UTF8,
            EnableBom = true,
            WriteBehaviour = WriteOperation.Error,
            AddNewLine = true
        };
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
    public void WriteFile_WriteTestFTP()
    {
        var connection = Helpers.GetFtpConnection();
        var result = FTP.WriteFile(connection, _input, default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(Helpers.CheckThatFileExistsInServer(_input.Path));
    }

    [Test]
    public void WriteFile_WriteTestFTPS()
    {
        var connection = Helpers.GetFtpsConnection();
        var result = FTP.WriteFile(connection, _input, default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(Helpers.CheckThatFileExistsInServer(_input.Path));
    }

    [Test]
    public void WriteFile_TestAppendFTP()
    {
        var connection = Helpers.GetFtpConnection();
        var result1 = FTP.WriteFile(connection, _input, default);
        Assert.IsTrue(Helpers.CheckThatFileExistsInServer(_input.Path));
        _input.WriteBehaviour = WriteOperation.Append;
        _input.Content = string.Concat(Enumerable.Repeat("This is another line\n", 100));
        var result2 = FTP.WriteFile(connection, _input, default);
        Assert.IsTrue(result1.SizeInMegaBytes < result2.SizeInMegaBytes);
    }

    [Test]
    public void WriteFile_TestAppendWithAddNewLineFTP()
    {
        var connection = Helpers.GetFtpConnection();
        FTP.WriteFile(connection, _input, default);
        _input.WriteBehaviour = WriteOperation.Append;
        _input.Content = "This is another line";
        _input.AddNewLine = true;
        var result = FTP.WriteFile(connection, _input, default);
        Assert.IsTrue(Helpers.GetFileContent(result.Path).Contains(Environment.NewLine));
    }

    [Test]
    public void WriteFile_TestThrowsWhenFileExistsFTP()
    {
        var connection = Helpers.GetFtpsConnection();
        var result = FTP.WriteFile(connection, _input, default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(Helpers.CheckThatFileExistsInServer(_input.Path));

        var ex = Assert.Throws<ArgumentException>(() => FTP.WriteFile(connection, _input, default));
        Assert.AreEqual($"File already exists: {_input.Path}", ex.Message);
    }

    [Test]
    public void WriteFile_TestOverwriteFTP()
    {
        var connection = Helpers.GetFtpsConnection();
        var result = FTP.WriteFile(connection, _input, default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(Helpers.CheckThatFileExistsInServer(_input.Path));

        _input.WriteBehaviour = WriteOperation.Overwrite;
        result = FTP.WriteFile(connection, _input, default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(Helpers.CheckThatFileExistsInServer(_input.Path));
    }

    [Test]
    public void WriteFile_TestThrowsWhenDirectoryNotExistsFTP()
    {
        var connection = Helpers.GetFtpsConnection();
        _input.CreateDestinationDirectories = false;
        var ex = Assert.Throws<FtpException>(() => FTP.WriteFile(connection, _input, default));
        Assert.AreEqual("Error while uploading the file to the server. See InnerException for more info.", ex.Message);
        Assert.AreEqual("Can't open that file: No such file or directory", ex.InnerException?.Message);
    }

    [Test]
    public void WriteFile_TestWithEncodingsFTP()
    {
        var connection = Helpers.GetFtpsConnection();

        foreach (var encoding in Enum.GetValues(typeof(FileEncoding)))
        {
            if ((FileEncoding)encoding == FileEncoding.Other)
                continue;
            _input.WriteBehaviour = WriteOperation.Overwrite;
            _input.FileEncoding = (FileEncoding)encoding;
            var result = FTP.WriteFile(connection, _input, default);
            Assert.AreEqual(_input.Path, result.Path);
            Assert.IsTrue(Helpers.CheckThatFileExistsInServer(_input.Path));
        }
    }

    [Test]
    public void WriteFile_TestWithEncodingOtherFTP()
    {
        var connection = Helpers.GetFtpsConnection();
        _input.FileEncoding = FileEncoding.Other;
        _input.EncodingInString = "iso-8859-1";
        var result = FTP.WriteFile(connection, _input, default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(Helpers.CheckThatFileExistsInServer(_input.Path));
    }

    [Test]
    public void WriteFile_TestThrowsWithIncorrectFingerprint()
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
            var result = FTP.WriteFile(connection, _input, default);
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
        var result = FTP.WriteFile(connection, _input, default);
        Assert.AreEqual(_input.Path, result.Path);
        Assert.IsTrue(Helpers.CheckThatFileExistsInServer(_input.Path));
    }
}