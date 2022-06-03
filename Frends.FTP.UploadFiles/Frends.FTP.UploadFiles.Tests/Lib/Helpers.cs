using System;
using System.IO;
using FluentFTP;
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
            Address = FtpHost, UserName = FtpUsername, Password = FtpPassword, Port = FtpsPort,
            SslMode = FtpsSslMode.Explicit, CertificateHashStringSHA1 = Sha1Hash
        };

        return connection;
    }
    
    internal static Connection GetFtpConnection()
    {
        var connection = new Connection
        {
            Address = FtpHost, UserName = FtpUsername, Password = FtpPassword, Port = FtpPort,
            SslMode = FtpsSslMode.None
        };

        return connection;
    }

    internal static string GetFileFromFtp(string subDir, string file)
    {
        var tmpFile = Path.GetTempFileName();
        using (var client = new FtpClient(FtpHost, FtpPort, FtpUsername, FtpPassword))
        {
            client.Connect();
            client.SetWorkingDirectory(subDir);
            client.DownloadFile(tmpFile,file);
            return File.ReadAllText(tmpFile);
        }
    }
}