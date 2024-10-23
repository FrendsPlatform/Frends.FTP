using FluentFTP;
using Frends.FTP.UploadFiles.TaskConfiguration;
using Frends.FTP.UploadFiles.Enums;
using Ionic.Zip;
using System;
using System.Collections.Generic;
using System.IO;
using System.Text;

namespace Frends.FTP.UploadFiles.Tests;

internal static class Helpers
{
    // Those values are taken directly from docker compose (see docker-compose.yml)
    internal static readonly string FtpHost = "localhost";
    internal static readonly int FtpPort = 21;
    internal static readonly int FtpsPort = 21;
    internal static readonly string FtpUsername = "frendsuser";
    internal static readonly string FtpPassword = "frendspass";
    internal static readonly string Sha1Hash = "D911262984DE9CC32A3518A1094CD24249EA5C49";

    internal static Connection GetFtpsConnection()
    {
        var connection = new Connection
        {
            UseFTPS = true,
            Address = FtpHost,
            UserName = FtpUsername,
            Password = FtpPassword,
            Port = FtpsPort,
            SslMode = FtpsSslMode.Explicit,
            CertificateHashStringSHA1 = Sha1Hash,
            BufferSize = 4096,
            VerifyOption = VerifyOptions.None
        };

        return connection;
    }

    internal static Connection GetFtpConnection()
    {
        var connection = new Connection
        {
            Address = FtpHost,
            UserName = FtpUsername,
            Password = FtpPassword,
            Port = FtpPort,
            SslMode = FtpsSslMode.None,
            BufferSize = 4096,
            VerifyOption = VerifyOptions.None
        };

        return connection;
    }

    internal static string GetFileFromFtp(string subDir, string file)
    {
        try
        {
            var tmpFile = Path.Combine(Path.GetDirectoryName(Path.GetTempFileName()), "test.zip");
            using (var client = new FtpClient(FtpHost, FtpPort, FtpUsername, FtpPassword))
            {
                client.Connect();
                client.SetWorkingDirectory(subDir);
                client.DownloadFile(tmpFile, file);
            }
            return tmpFile;
        }
        catch (Exception ex)
        {
            throw new Exception(ex.Message);
        }
    }

    internal static string CreateLargeDummyZipFiles(string dir, int count)
    {
        var name = "LargeTestFile";
        var extension = ".bin";

        var subDir = Path.Combine(dir, "zips");
        Directory.CreateDirectory(subDir);

        for (int i = 0; i < count; i++)
        {
            var path = Path.Combine(subDir, name + i + extension);
            var fs = new FileStream(path, FileMode.CreateNew);
            fs.Seek(2048L * 1024 * 512, SeekOrigin.Begin);
            fs.WriteByte(0);
            fs.Close();
        }

        using var zipFile = new Ionic.Zip.ZipFile(new UTF8Encoding(false));
        zipFile.UseZip64WhenSaving = Zip64Option.Never;

        foreach (var file in Directory.GetFiles(subDir))
        {
            zipFile.AddFile(file, "");
        }

        zipFile.Save(Path.Combine(dir, "test.zip"));

        return Path.Combine(dir, "test.zip");
    }

    internal static bool ExtractLargeZipFile(string source, string destination)
    {
        try
        {
            var output = new UnzipOutput();
            using var zip = ZipFile.Read(source);
            string path = null;
            zip.ExtractProgress += (sender, e) => Zip_ExtractProgress(e, output, path);
            zip.ExtractExistingFile = ExtractExistingFileAction.OverwriteSilently;
            zip.ExtractAll(destination);
            return true;
        }
        catch
        {
            return false;
        }
    }

    private static void Zip_ExtractProgress(ExtractProgressEventArgs e, UnzipOutput output, string fullPath)
    {
        if (e.EventType == ZipProgressEventType.Extracting_AfterExtractEntry && !e.CurrentEntry.IsDirectory)
        {
            // Path.GetFullPath changes directory separator to "\".
            if (e.ExtractLocation == null) output.ExtractedFiles.Add(Path.GetFullPath(fullPath));
            else output.ExtractedFiles.Add(Path.GetFullPath(Path.Combine(e.ExtractLocation, e.CurrentEntry.FileName)));
        }
    }

    public class UnzipOutput
    {
        /// <summary>
        /// a List-object of extracted files.
        /// </summary>
        /// <example>"ExtractedFiles": ["C:\\temp\\sample.txt",	"C:\\temp\\sample2.txt"]</example>
        public List<string> ExtractedFiles { get; set; }

        internal UnzipOutput()
        {
            ExtractedFiles = new List<string>();
        }
    }
}