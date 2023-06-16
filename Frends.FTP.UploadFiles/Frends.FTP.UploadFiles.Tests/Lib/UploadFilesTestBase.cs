using NUnit.Framework;
using System;
using System.IO;

namespace Frends.FTP.UploadFiles.Tests;

/// <summary>
/// Provides simple dir and helper methods for local file creation
/// </summary>
public class UploadFilesTestBase
{
    protected string Dir;

    protected void CreateDummyFileInDummyDir(string fileName, string contents = "test")
    {
        File.WriteAllText(Path.Combine(Dir, fileName), contents);
    }

    protected bool DummyFileExists(string fileName, string dir = null)
    {
        return File.Exists(Path.Combine(dir ?? Dir, fileName));
    }

    protected static string CreateDummyDir()
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