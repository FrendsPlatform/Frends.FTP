using Frends.FTP.ReadFile.Definitions;
using Frends.FTP.ReadFile.Enums;

namespace Frends.FTP.ReadFile.Tests.Lib;
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
            CertificateHashStringSHA1 = Sha1Hash
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
            SslMode = FtpsSslMode.None
        };

        return connection;
    }

    internal static bool CheckThatFileExistsInServer(string path)
    {
        var connection = GetFtpConnection();
        using var client = FTP.CreateFtpClient(connection);
        client.Connect();
        var exists = client.FileExists(path);
        client.Disconnect();
        client.Dispose();

        return exists;
    }

    internal static void UploadDummyFile(string path, string content)
    {
        var connection = GetFtpConnection();
        using var client = FTP.CreateFtpClient(connection);
        var tempFile = Path.Combine(Path.GetTempPath(), $"frends_{Guid.NewGuid()}.8CO");
        File.WriteAllText(tempFile, content);
        client.Connect();
        if (!client.DirectoryExists(Path.GetDirectoryName(path)))
            client.CreateDirectory(Path.GetDirectoryName(path));
        client.UploadFile(tempFile, path, FluentFTP.FtpRemoteExists.Overwrite);
        client.Disconnect();
        client.Dispose();
        File.Delete(tempFile);
    }
}

