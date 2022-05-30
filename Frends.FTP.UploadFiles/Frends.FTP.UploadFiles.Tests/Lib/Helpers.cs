using System;
using Frends.FTP.UploadFiles.TaskConfiguration;

namespace Frends.FTP.UploadFiles.Tests;

internal static class Helpers
{
    internal static readonly string FtpHost = Environment.GetEnvironmentVariable("FTP_HOST");
    internal static readonly int FtpPort = int.Parse(Environment.GetEnvironmentVariable("FTP_PORT"));
    internal static readonly int FtpsPort = int.Parse(Environment.GetEnvironmentVariable("FTPS_PORT"));
    internal static readonly string FtpUsername = Environment.GetEnvironmentVariable("FTP_USERNAME");
    internal static readonly string FtpPassword = Environment.GetEnvironmentVariable("FTP_PASSWORD");
    
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
}