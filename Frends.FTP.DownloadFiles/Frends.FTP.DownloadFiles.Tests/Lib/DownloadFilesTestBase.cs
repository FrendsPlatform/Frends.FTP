using System;
using System.IO;
using NUnit.Framework;

namespace Frends.FTP.DownloadFiles.Tests.Lib;

public class DownloadFilesTestBase
{
    private string _dockerDataVolumePath = "..\\..\\..\\DockerVolumes\\data";
    private string _ftpSubDirFullPath;
    protected string DummyFtpSubDirName;
    protected string LocalDirFullPath;
    
    protected void CreateDummyFileInFtpDir(string fileName, string subDir = null)
    {
        subDir ??= DummyFtpSubDirName;
        File.WriteAllText(Path.Combine(_dockerDataVolumePath, subDir, fileName), "test");
    }

    protected bool LocalFileExists(string fileName, string subDir = null)
    {
        var path = subDir == null ? LocalDirFullPath : Path.Combine(Path.GetTempPath(), subDir);
        return File.Exists(Path.Combine(path, fileName));
    }
    
    protected bool FtpFileExistsInDummyDir(string fileName)
    {
        return File.Exists(Path.Combine(_ftpSubDirFullPath, fileName));
    }
    
    protected bool FtpFileExists(string fileName, string subDir)
    {
        return File.Exists(Path.Combine(_dockerDataVolumePath, subDir, fileName));
    }

    protected string CreateFtpDir(string name)
    {
        var dir = Path.Combine(_dockerDataVolumePath, name);
        var di = Directory.CreateDirectory(dir);
        return di.FullName;
    }

    protected string CreateLocalDir(string name)
    {
        var dir = Path.Combine(Path.GetTempPath(), name);
        Directory.CreateDirectory(dir);
        return dir;
    }
    
    [SetUp]
    public void SetUp()
    {
        _ftpSubDirFullPath = CreateFtpDir(Guid.NewGuid().ToString());
        DummyFtpSubDirName = Path.GetFileName(_ftpSubDirFullPath);
        LocalDirFullPath = CreateLocalDir(Guid.NewGuid().ToString());
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(_ftpSubDirFullPath, true);
        Directory.Delete(LocalDirFullPath, true);
    }
}