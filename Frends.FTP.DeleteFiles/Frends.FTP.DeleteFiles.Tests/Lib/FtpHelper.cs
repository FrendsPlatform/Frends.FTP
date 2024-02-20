namespace Frends.FTP.DeleteFiles.Tests;

using System;
using System.Text;
using FluentFTP;
using Frends.FTP.DeleteFiles.Definitions;
using Frends.FTP.DeleteFiles.Enums;

public class FtpHelper : IDisposable
{
    // Those values are taken directly from docker compose (see docker-compose.yml)
    internal static readonly string FtpHost = "localhost";
    internal static readonly int FtpPort = 21;
    internal static readonly int FtpsPort = 21;
    internal static readonly string FtpUsername = "frendsuser";
    internal static readonly string FtpPassword = "frendspass";
    internal static readonly string Sha1Hash = "D911262984DE9CC32A3518A1094CD24249EA5C49";
    readonly FtpClient client = new();

    public FtpHelper()
    {
        client = new FtpClient(FtpHost, FtpPort, FtpUsername, FtpPassword);
        client.Connect();
    }

    public void Dispose()
    {
        GC.SuppressFinalize(this);
        client.Disconnect();
        if (!client.IsDisposed) client.Dispose();
    }

    public void DeleteDirectoryOnFTP(string ftpDir)
    {
        client.DeleteDirectory(ftpDir, FtpListOption.Recursive);
    }

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
        };

        return connection;
    }

    internal void CreateFileOnFTP(string subDir, string fileName, string content = "hello")
    {
        client.CreateDirectory(subDir);
        client.SetWorkingDirectory(subDir);
        client.Upload(Encoding.UTF8.GetBytes(content), fileName);
        client.SetWorkingDirectory("/");
    }

    internal void CreateDirectoryOnFTP(string subDir)
    {
        client.CreateDirectory(subDir);
    }
}