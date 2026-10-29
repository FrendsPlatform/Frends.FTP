using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using FluentFTP;
using Frends.FTP.UploadFiles.Enums;
using Frends.FTP.UploadFiles.TaskConfiguration;

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
            VerifyOption = VerifyOptions.None,
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
            VerifyOption = VerifyOptions.None,
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

        ZipFile.CreateFromDirectory(subDir, Path.Combine(dir, "test.zip"), CompressionLevel.Optimal, false);

        return Path.Combine(dir, "test.zip");
    }

    internal static bool ExtractLargeZipFile(string source, string destination)
    {
        try
        {
            ZipFile.ExtractToDirectory(source, destination, true);
            return true;
        }
        catch
        {
            return false;
        }
    }
}
