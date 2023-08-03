using Frends.FTP.WriteFile.Definitions;

namespace Frends.FTP.WriteFile.Tests.Lib;
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

    internal static string GetFileContent(string path)
    {
        var connection = GetFtpConnection();
        using var client = FTP.CreateFtpClient(connection);
        client.Connect();
        var localPath = Path.Combine(Path.GetTempPath(), "test.txt");
        client.DownloadFile(localPath, path);
        client.Disconnect();
        client.Dispose();
        var content = File.ReadAllText(localPath);
        File.Delete(localPath);
        return content;
    }
}

