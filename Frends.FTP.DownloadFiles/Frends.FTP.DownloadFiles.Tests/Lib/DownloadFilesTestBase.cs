using System;
using System.IO;
using NUnit.Framework;

namespace Frends.FTP.DownloadFiles.Tests.Lib;

public class DownloadFilesTestBase
{
    private string dockerDataVolumePath = "..\\..\\..\\DockerVolumes\\data";
    protected string FtpSubDirName;
    protected string FtpSubDirFullPath;
    protected string LocalDirFullPath;
    
    protected void CreateDummyFileInFtpDir(string fileName)
    {
        File.WriteAllText(Path.Combine(FtpSubDirFullPath, fileName), "test");
    }

    protected bool LocalFileExists(string fileName)
    {
        return File.Exists(Path.Combine(LocalDirFullPath, fileName));
    }
    
    protected bool FtpFileExists(string fileName)
    {
        return File.Exists(Path.Combine(FtpSubDirFullPath, fileName));
    }
    
    protected bool FtpFileExists(string fileName, string subdir)
    {
        // We need ../subdir because our FtpDir is already a dummy ftp subdir anyway
        return File.Exists(Path.Combine(FtpSubDirFullPath, "../"+subdir, fileName));
    }

    protected string CreateDummyFtpDir()
    {
        var dir = Path.Combine(dockerDataVolumePath, Guid.NewGuid().ToString());
        var di = Directory.CreateDirectory(dir);
        return di.FullName;
    }

    protected string CreateDummyLocalDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        return dir;
    }
    
    [SetUp]
    public void SetUp()
    {
        FtpSubDirFullPath = CreateDummyFtpDir();
        FtpSubDirName = Path.GetFileName(FtpSubDirFullPath);
        LocalDirFullPath = CreateDummyLocalDir();
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(FtpSubDirFullPath, true);
        Directory.Delete(LocalDirFullPath, true);
    }
}