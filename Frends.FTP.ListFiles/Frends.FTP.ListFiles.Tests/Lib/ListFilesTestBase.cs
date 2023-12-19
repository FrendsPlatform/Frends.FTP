using NUnit.Framework;
using Frends.FTP.ListFiles.Definitions;

namespace Frends.FTP.ListFiles.Tests.Lib;

public class ListFilesTestBase
{
    protected string LocalDirFullPath = string.Empty;
    protected static FtpHelper FtpHelper = new();
    protected string FtpDir = string.Empty;

    protected Input input = new();

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        FtpHelper = new FtpHelper();
    }

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
            $"DemoTest.txt"
        };

        foreach (var file in files)
        {
            FtpHelper.CreateFileOnFTP(FtpDir, file);
        }

        input = new Input
        {
            FileMask = "*",
            Directory = FtpDir,
            IncludeSubdirectories = false,
            IncludeType = IncludeType.File
        };
    }

    [TearDown]
    public void TearDown()
    {
        FtpHelper.DeleteDirectoryOnFTP(FtpDir);
    }
}