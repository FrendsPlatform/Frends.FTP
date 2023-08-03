using Microsoft.VisualStudio.TestTools.UnitTesting;
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

    [TestInitialize]
    public void SetUp()
    {
        Dir = CreateDummyDir();
    }

    [TestCleanup]
    public void TearDown()
    {
        if (Directory.Exists(Dir)) Directory.Delete(Dir, true);

        var currentDirectoryInfo = new DirectoryInfo(Directory.GetCurrentDirectory());
        var parentDir = currentDirectoryInfo.Parent?.Parent.Parent;

        if (parentDir != null)
        {
            var dataFolder = Path.Combine(parentDir.FullName, "DockerVolumes", "data");

            if (Directory.Exists(dataFolder))
            {
                var subDirectories = Directory.GetDirectories(dataFolder);

                foreach (var subDirectory in subDirectories)
                    Directory.Delete(subDirectory, true);

                var files = Directory.GetFiles(dataFolder);
                foreach (var file in files)
                    File.Delete(file);

                Console.WriteLine("data-folder cleaned.");
            }
        }
    }
}