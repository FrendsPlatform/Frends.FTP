namespace Frends.FTP.DeleteFiles.Tests.Lib;

using System;
using NUnit.Framework;
using Frends.FTP.DeleteFiles.Definitions;

public class DeleteFilesTestBase
{
    protected static FtpHelper FtpHelper = new();
    protected Input input = new();
    protected string FtpDir = string.Empty;

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        if (FtpHelper != null)
            FtpHelper.Dispose();
    }

    [SetUp]
    public void SetUp()
    {
        FtpDir = Guid.NewGuid().ToString();
        var files = new string[]
        {
            $"Test1.txt",
            $"Test1.xlsx",
            $"Test1.xml",
            $"testfile.txt",
            $"DemoTest.txt",
        };

        foreach (var file in files)
        {
            FtpHelper.CreateFileOnFTP(FtpDir, file);
        }

        input = new Input
        {
            FileMask = "*",
            Directory = FtpDir,
        };
    }

    [TearDown]
    public void TearDown()
    {
        FtpHelper.DeleteDirectoryOnFTP(FtpDir);
    }
}