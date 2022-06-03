using System;
using System.IO;
using NUnit.Framework;

namespace Frends.FTP.DownloadFiles.Tests.Lib;

public class DownloadFilesTestBase
{
    protected string LocalDirFullPath;
    
    protected bool LocalFileExists(string fileName, string subDir = null)
    {
        var path = subDir == null ? LocalDirFullPath : Path.Combine(Path.GetTempPath(), subDir);
        return File.Exists(Path.Combine(path, fileName));
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
        LocalDirFullPath = CreateLocalDir(Guid.NewGuid().ToString());
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(LocalDirFullPath, true);
    }
}