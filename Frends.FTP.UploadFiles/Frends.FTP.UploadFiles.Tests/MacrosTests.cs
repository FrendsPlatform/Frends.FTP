﻿using Frends.FTP.UploadFiles.Enums;
using Frends.FTP.UploadFiles.TaskConfiguration;
using Frends.FTP.UploadFiles.TaskResult;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using System;
using System.IO;
using System.Threading;

namespace Frends.FTP.UploadFiles.Tests;

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
[TestClass]
public class MacrosTests
{
    private readonly string _dockerDataVolumePath = Path.GetFullPath("../../../DockerVolumes/data");

    private static string CreateLocalDir(string name)
    {
        var path = Path.Combine(Path.GetTempPath(), name);
        Directory.CreateDirectory(path);
        return path;
    }

    private static string CreateLocalFile(string dir, string file)
    {
        var path = Path.Combine(Path.GetTempPath(), dir, file);
        File.WriteAllText(path, "hello");
        return path;
    }

    protected bool FtpFileExists(string fileName, string subDir)
    {
        return File.Exists(Path.Combine(_dockerDataVolumePath, subDir, fileName));
    }

    [TestMethod]
    public void MacrosWorkInSourceDirectory()
    {
        // Setup
        var year = DateTime.Now.Year;
        var guid = Guid.NewGuid().ToString();

        var sourceWithMacros = $"dir-%Year%-{guid}";
        var sourceWithMacrosExpanded = $"dir-{year}-{guid}";

        CreateLocalDir(sourceWithMacrosExpanded);
        CreateLocalFile(sourceWithMacrosExpanded, "file1.txt");

        var result = CallUploadFiles(
            Path.Combine(Path.GetTempPath(), sourceWithMacros),
            "file*.txt",
            nameof(MacrosWorkInSourceDirectory));

        Assert.IsTrue(result.Success, result.UserResultMessage);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
        Assert.IsTrue(FtpFileExists("file1.txt", nameof(MacrosWorkInSourceDirectory)),
            string.Join(",", Directory.EnumerateDirectories(_dockerDataVolumePath)) + "| " +
            string.Join(",", Directory.EnumerateFiles(Path.Combine(_dockerDataVolumePath, nameof(MacrosWorkInSourceDirectory)))));
    }

    [TestMethod]
    public void MacrosWorkInDestinationDirectory()
    {
        // Setup
        var year = DateTime.Now.Year;
        var guid = Guid.NewGuid().ToString();

        var destinationWithMacros = $"dir-%Year%-{guid}";
        var destinationWithMacrosExpanded = $"dir-{year}-{guid}";

        var source = CreateLocalDir(guid);
        CreateLocalFile(guid, "file1.txt");

        var result = CallUploadFiles(
            source,
            "file*.txt",
            destinationWithMacros);

        Assert.IsTrue(result.Success, result.UserResultMessage);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
        Assert.IsTrue(FtpFileExists("file1.txt", destinationWithMacrosExpanded), result.UserResultMessage);
    }

    [TestMethod]
    public void MacrosWorkInDestinationFileName()
    {
        // Setup
        var year = DateTime.Now.Year;
        var guid = Guid.NewGuid().ToString();

        var destinationFileNameWithMacros = $"f-%Year%-%SourceFileName%-{guid}";
        var destinationFileNameWithMacrosExpanded = $"f-{year}-file1-{guid}";

        var source = CreateLocalDir(guid);
        CreateLocalFile(guid, "file1.txt");

        var result = CallUploadFiles(
            source,
            "file1.txt",
            nameof(MacrosWorkInDestinationFileName),
            destinationFileNameWithMacros);

        Assert.IsTrue(result.Success, result.UserResultMessage);
        Assert.AreEqual(1, result.SuccessfulTransferCount);
        Assert.IsTrue(FtpFileExists(destinationFileNameWithMacrosExpanded, nameof(MacrosWorkInDestinationFileName)), result.UserResultMessage);
    }

    private static Result CallUploadFiles(
        string sourceDirectory,
        string sourceFileName,
        string targetDirectory,
        string targetFileName = null)
    {
        var source = new Source
        {
            Directory = sourceDirectory,
            FileName = sourceFileName,
            Operation = SourceOperation.Delete
        };
        var destination = new Destination
        {
            Directory = targetDirectory,
            Action = DestinationAction.Overwrite,
            FileName = targetFileName
        };
        var options = new Options { CreateDestinationDirectories = true };
        var connection = Helpers.GetFtpsConnection();

        var result = FTP.UploadFiles(source, destination, connection, options, new Info(), new CancellationToken());
        return result;
    }
}