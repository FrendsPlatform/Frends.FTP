using System;
using System.IO;
using NUnit.Framework;

namespace Frends.FTP.DownloadFiles.Tests.Lib;

public class DownloadFilesTestBase
{
    protected string LocalDirFullPath;
    protected static FtpHelper FtpHelper;
    protected string FtpDir;

    [OneTimeSetUp]
    public void OneTimeSetUp()
    {
        FtpHelper = new FtpHelper();
    }

    [OneTimeTearDown]
    public void OneTimeTearDown()
    {
        FtpHelper.Dispose();
    }

    protected bool LocalFileExists(string fileName, string subDir = null)
    {
        var path = subDir == null ? LocalDirFullPath : Path.Combine(Path.GetTempPath(), subDir);
        return File.Exists(Path.Combine(path, fileName));
    }

    protected static string CreateLocalDir(string name)
    {
        var dir = Path.Combine(Path.GetTempPath(), name);
        Directory.CreateDirectory(dir);
        return dir;
    }

    [SetUp]
    public void SetUp()
    {
        LocalDirFullPath = CreateLocalDir(Guid.NewGuid().ToString());
        FtpDir = Guid.NewGuid().ToString();
        FtpHelper.CreateDirectoryOnFTP(FtpDir);
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(LocalDirFullPath, true);
        FtpHelper.DeleteDirectoryOnFTP(FtpDir);
    }
}