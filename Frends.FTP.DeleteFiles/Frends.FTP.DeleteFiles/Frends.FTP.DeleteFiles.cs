namespace Frends.FTP.DeleteFiles;

using System;
using System.Text.RegularExpressions;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using Frends.FTP.DeleteFiles.Definitions;
using Frends.FTP.DeleteFiles.Enums;

/// <summary>
/// Main class of the Task.
/// </summary>
public static class FTP
{
    /// <summary>
    /// Frends Task for deleting files from FTP(S) server.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.FTP.DeleteFiles).
    /// </summary>
    /// <param name="input">Input parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="cancellationToken">Cancellation token given by Frends.</param>
    /// <returns>Object { List&lt;string&gt; Files }.</returns>
    public static async Task<Result> DeleteFiles([PropertyTab] Input input, [PropertyTab] Connection connection, CancellationToken cancellationToken)
    {
        var deleted = new List<string>();

        try
        {
            using var client = CreateFtpClient(connection);

            client.Connect();

            if (!client.IsConnected) throw new ArgumentException($"Error while connecting to destination: {connection.Address}");

            if (!await client.DirectoryExistsAsync(input.Directory, cancellationToken))
                throw new ArgumentException($"FTP directory '{input.Directory}' doesn't exist.");

            var files = await GetFiles(client, input, cancellationToken);

            foreach (var file in files)
            {
                cancellationToken.ThrowIfCancellationRequested();
                await client.DeleteFileAsync(file.FullPath, cancellationToken);
                deleted.Add(file.FullPath);
            }

            return new Result(deleted);
        }
        catch (Exception ex)
        {
            throw new ArgumentException($"Error occured while deleting files: {ex.Message}\nDeleted files: {string.Join("\n", deleted)}");
        }
    }

    private static FtpClient CreateFtpClient(Connection connect)
    {
        var client = new FtpClient(connect.Address, connect.Port, connect.UserName, connect.Password)
        {
            EncryptionMode = connect.SslMode switch
            {
                FtpsSslMode.None => FtpEncryptionMode.None,
                FtpsSslMode.Implicit => FtpEncryptionMode.Implicit,
                FtpsSslMode.Explicit => FtpEncryptionMode.Explicit,
                FtpsSslMode.Auto => FtpEncryptionMode.Auto,
                _ => FtpEncryptionMode.None,
            },
        };

        if (connect.UseFTPS)
        {
            if (connect.EnableClientAuth)
            {
                if (!string.IsNullOrEmpty(connect.ClientCertificatePath))
                {
                    client.ClientCertificates.Add(new X509Certificate2(connect.ClientCertificatePath));
                }
                else
                {
                    using var store = new X509Store(StoreName.My, StoreLocation.CurrentUser);
                    try
                    {
                        store.Open(OpenFlags.ReadOnly);
                        if (!string.IsNullOrEmpty(connect.ClientCertificateName))
                            client.ClientCertificates.Add(store.Certificates.Find(X509FindType.FindBySubjectName, connect.ClientCertificateName, false)[0]);
                        else if (!string.IsNullOrEmpty(connect.ClientCertificateThumbprint))
                            client.ClientCertificates.Add(store.Certificates.Find(X509FindType.FindByThumbprint, connect.ClientCertificateThumbprint, false)[0]);
                        else
                            client.ClientCertificates.AddRange(store.Certificates);
                    }
                    finally
                    {
                        store.Close();
                    }
                }
            }

            client.ValidateCertificate += (control, e) =>
            {
                // If cert is valid and such - go on and accept
                if (e.PolicyErrors == SslPolicyErrors.None)
                {
                    e.Accept = true;
                    return;
                }

                // Accept if we want to accept a certain hash
                e.Accept = e.Certificate.GetCertHashString() == connect.CertificateHashStringSHA1;
            };

            client.ValidateAnyCertificate = connect.ValidateAnyCertificate;
            client.DataConnectionEncryption = connect.SecureDataChannel;
        }

        client.NoopInterval = connect.KeepConnectionAliveInterval;

        if (!string.IsNullOrWhiteSpace(connect.Encoding)) client.Encoding = Encoding.GetEncoding(connect.Encoding);

        // Client lib timeout is in milliseconds, ours is in seconds, thus *1000 conversion
        client.ConnectTimeout = connect.ConnectionTimeout * 1000;
        client.ReadTimeout = connect.ConnectionTimeout * 1000;
        client.DataConnectionConnectTimeout = connect.ConnectionTimeout * 1000;
        client.DataConnectionReadTimeout = connect.ConnectionTimeout * 1000;

        client.LocalFileBufferSize = connect.BufferSize;

        // Active/passive
        client.DataConnectionType = connect.Mode switch
        {
            FtpMode.Active => FtpDataConnectionType.AutoActive,
            FtpMode.Passive => FtpDataConnectionType.AutoPassive,
            _ => throw new ArgumentOutOfRangeException($"Unknown FTP mode {connect.Mode}"),
        };

        return client;
    }

    private static async Task<List<FileItem>> GetFiles(FtpClient ftp, Input input, CancellationToken cancellationToken)
    {
        var directoryList = new List<FileItem>();
        var filePaths = ConvertObjectToStringArray(input.FilePaths);

        if (filePaths != null)
        {
            foreach (var file in filePaths.ToList())
            {
                cancellationToken.ThrowIfCancellationRequested();
                directoryList.Add(new FileItem(file));
            }

            return directoryList;
        }

        var regex = "^" + Regex.Escape(input.FileMask).Replace("\\?", ".").Replace("\\*", ".*") + "$";
        var regexStr = string.IsNullOrEmpty(input.FileMask) ? string.Empty : regex;

        var ftpFiles = await ftp.GetListingAsync(input.Directory, cancellationToken);

        var files = new List<FileItem>();
        foreach (var file in ftpFiles)
        {
            cancellationToken.ThrowIfCancellationRequested();

            if (file.Name != "." && file.Name != ".." && file.Type != FtpFileSystemObjectType.File)
                continue; // skip directories and links

            if (Regex.IsMatch(file.Name, regexStr, RegexOptions.IgnoreCase) || FileMatchesMask(file.Name, input.FileMask))
                files.Add(new FileItem(file));
        }

        return files;
    }

    private static bool FileMatchesMask(string filename, string mask)
    {
        const string regexEscape = "<regex>";
        string pattern = string.Empty;

        // Check is pure regex wished to be used for matching
        if (mask != null && mask.StartsWith(regexEscape))
        {
            // Use substring instead of string.replace just in case some has regex like '<regex>//File<regex>' or something else like that
            pattern = mask[regexEscape.Length..];
        }
        else if (mask != null)
        {
            pattern = mask.Replace(".", "\\.");
            pattern = pattern.Replace("*", ".*");
            pattern = pattern.Replace("?", ".+");
            pattern = string.Concat("^", pattern, "$");
        }

        return Regex.IsMatch(filename, pattern, RegexOptions.IgnoreCase);
    }

    private static string[] ConvertObjectToStringArray(object objectArray)
    {
        var res = objectArray as object[];
        return res?.OfType<string>().ToArray();
    }
}
