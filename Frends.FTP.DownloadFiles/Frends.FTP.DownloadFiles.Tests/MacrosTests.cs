using System;
using System.IO;
using System.Threading;
using Frends.FTP.DownloadFiles.Enums;
using Frends.FTP.DownloadFiles.TaskConfiguration;
using Frends.FTP.DownloadFiles.TaskResult;
using Frends.FTP.DownloadFiles.Tests.Lib;
using NUnit.Framework;

namespace Frends.FTP.DownloadFiles.Tests;

/// <summary>
/// We have several places that support macros:
/// - source dir
/// - destination dir
/// - destination file name
///
/// NOTE: 
/// - source file name does not support macros for some reason in Cobalt, currently not implementing because
///   need to understand reasoning better
/// </summary>
[TestFixture]
public class MacrosTests : DownloadFilesTestBase
{
    [Test]
    public void MacrosWorkInSourceDirectory()
    {
        // Setup
        var year = DateTime.Now.Year;
        Helpers.CreateFileOnFTP($"dir{year}", "file1.txt");
        
        var result = CallDownloadFiles(
            "dir%Year%",
            "file*.txt",
            LocalDirFullPath);
        
        Assert.IsTrue(result.Success, result.UserResultMessage);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
        Assert.IsTrue(LocalFileExists("file1.txt"));
    }
    
    [Test]
    public void MacrosWorkInDestinationDirectory()
    {
        var year = DateTime.Now.Year;
        var guid = Guid.NewGuid().ToString();
        Helpers.CreateFileOnFTP(guid, "file1.txt");
        var destinationDirWithMacros = Path.Combine(Path.GetTempPath(), $"transfer-%Year%-{guid}");
        var destinationDirWithMacrosExpanded = Path.Combine(Path.GetTempPath(), $"transfer-{year}-{guid}");
        
        var result = CallDownloadFiles(
            guid,
            "file1.txt",
            destinationDirWithMacros);
        
        Assert.IsTrue(result.Success, result.UserResultMessage);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
        Assert.IsTrue(File.Exists(Path.Combine(destinationDirWithMacrosExpanded, "file1.txt")), result.UserResultMessage);
    }
    
    [Test]
    public void MacrosWorkInDestinationFilename()
    {
        var year = DateTime.Now.Year;
        var guid = Guid.NewGuid().ToString();
        Helpers.CreateFileOnFTP(guid, "file1.txt");
        var destinationFileNameWithMacros = $"f-%Year%-%SourceFileName%-{guid}";
        var destinationFileNameWithMacrosExpanded = $"f-{year}-file1-{guid}";
        
        var result = CallDownloadFiles(
            guid,
            "file1.txt",
            LocalDirFullPath,
            destinationFileNameWithMacros);
        
        Assert.IsTrue(result.Success, result.UserResultMessage);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
        Assert.IsTrue(File.Exists(Path.Combine(LocalDirFullPath, destinationFileNameWithMacrosExpanded)), result.UserResultMessage);
    }
    
    private Result CallDownloadFiles(
        string sourceDirectory,
        string sourceFileName,
        string targetDirectory,
        string targetFileName = null,
        string moveToDir = null,
        string renameTo = null)
    {
        var source = new Source
        {
            Directory = sourceDirectory, FileName = sourceFileName,
            Operation = SourceOperation.Delete,
            DirectoryToMoveAfterTransfer = moveToDir,
            FileNameAfterTransfer = renameTo
        };
        var destination = new Destination
            { 
                Directory = targetDirectory,
                Action = DestinationAction.Overwrite,
                FileName = targetFileName
            };
        var options = new Options { CreateDestinationDirectories = true };
        var connection = Helpers.GetFtpsConnection();

        var result = FTP.DownloadFiles(source, destination, connection, options, new Info(), new CancellationToken());
        return result;
    }
}