using System;
using System.IO;
using NUnit.Framework;

namespace Frends.FTP.DownloadFiles.Tests.Lib;

/// <summary>
/// Provides simple dir and helper methods for local file creation
/// </summary>
public class DownloadFilesTestBase
{
    protected string Dir;
    
    protected void CreateDummyFileInDummyDir(string fileName)
    {
        File.WriteAllText(Path.Combine(Dir, fileName), "test");
    }

    protected bool DummyFileExists(string fileName, string dir = null)
    {
        return File.Exists(Path.Combine(dir ?? Dir, fileName));
    }

    protected string CreateDummyDir()
    {
        var dir = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(dir);
        return dir;
    }
    
    [SetUp]
    public void SetUp()
    {
        Dir = CreateDummyDir();
    }

    [TearDown]
    public void TearDown()
    {
        Directory.Delete(Dir, true);
    }
}