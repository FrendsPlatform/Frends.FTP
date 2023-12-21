﻿using Frends.FTP.ListFiles.Definitions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Cryptography.X509Certificates;
using System.Threading;
using System.Threading.Tasks;
using FluentFTP;
using System.Text.RegularExpressions;

namespace Frends.FTP.ListFiles;

///<summary>
/// Files task.
/// </summary>
public class FTP
{
    /// <summary>
    /// Gets the list of files from the FTP(S) source.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.FTP.ListFiles)
    /// </summary>
    /// <param name="input">Input parameters.</param>
    /// <param name="connection">Connection parameters.</param>
    /// <param name="cancellationToken">Token to stop task. This is generated by Frends.</param>
    /// <returns>Object { List Files [ FileItem { string Name, string FullPath, DateTime LastModified, long SizeBytes } ] }</returns>
    public static async Task<Result> ListFiles([PropertyTab] Input input, [PropertyTab] Connection connection, CancellationToken cancellationToken)
    {
        try
        {
            using FtpClient client = CreateFtpClient(connection);
            client.Connect();

            if (!client.DirectoryExists(input.Directory))
                throw new ArgumentException($"FTP directory '{input.Directory}' doesn't exist.");

            var regex = "^" + Regex.Escape(input.FileMask).Replace("\\?", ".").Replace("\\*", ".*") + "$";
            var regexStr = string.IsNullOrEmpty(input.FileMask) ? string.Empty : regex;

            var files = await GetSourceFilesAsync(client, regexStr, input.Directory, input, cancellationToken);

            if (files == null)
                throw new ArgumentException(
                    "Source end point returned null list for file list. If there are no files to transfer, the result should be an empty list.");

            client.Disconnect();
            client.Dispose();
            return new Result(files);
        }
        catch (SocketException)
        {
            throw new ArgumentException("Unable to establish the socket: No such host is known.");
        }
    }

    private static FtpClient CreateFtpClient(Connection connect)
    {
        var client = new FtpClient(connect.Address, connect.Port, connect.UserName, connect.Password);

        if (connect.UseFTPS)
        {
            client.EncryptionMode = connect.SslMode switch
            {
                FtpsSslMode.None => FtpEncryptionMode.None,
                FtpsSslMode.Implicit => FtpEncryptionMode.Implicit,
                FtpsSslMode.Explicit => FtpEncryptionMode.Explicit,
                FtpsSslMode.Auto => FtpEncryptionMode.Auto,
                _ => FtpEncryptionMode.Auto,
            };

            if (connect.EnableClientAuth)
            {
                if (!string.IsNullOrEmpty(connect.ClientCertificatePath))
                    client.ClientCertificates.Add(new X509Certificate2(connect.ClientCertificatePath));
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

        // Client lib timeout is in milliseconds, ours is in seconds, thus *1000 conversion
        client.ConnectTimeout = connect.ConnectionTimeout * 1000;

        // Active/passive
        client.DataConnectionType = connect.Mode switch
        {
            FtpMode.Active => FtpDataConnectionType.AutoActive,
            FtpMode.Passive => FtpDataConnectionType.AutoPassive,
            _ => throw new ArgumentOutOfRangeException($"Unknown FTP mode {connect.Mode}."),
        };

        return client;
    }

    private static async Task<List<FileItem>> GetSourceFilesAsync(FtpClient client, string regexStr, string path, Input input, CancellationToken cancellationToken)
    {
        var ftpFiles = await client.GetListingAsync(path, cancellationToken);

        var files = new List<FileItem>();
        foreach (var file in ftpFiles)
        {
            if (file.Type == FtpFileSystemObjectType.Link)
                continue; // skip directories and links

            if (file.Name != "." && file.Name != "..")
            {
                if (input.IncludeType == IncludeType.Both
                        || (file.Type == FtpFileSystemObjectType.Directory && input.IncludeType == IncludeType.Directory)
                        || (file.Type == FtpFileSystemObjectType.File && input.IncludeType == IncludeType.File))
                {
                    if (Regex.IsMatch(file.Name, regexStr, RegexOptions.IgnoreCase) || FileMatchesMask(file.Name, input.FileMask))
                        files.Add(new FileItem(file));
                }

                if (file.Type == FtpFileSystemObjectType.Directory && input.IncludeSubdirectories)
                    files.AddRange(await GetSourceFilesAsync(client, regexStr, file.FullName, input, cancellationToken));
            }
        }

        return files;
    }

    private static bool FileMatchesMask(string filename, string mask)
    {
        const string regexEscape = "<regex>";
        string pattern;

        //check is pure regex wished to be used for matching
        if (mask != null && mask.StartsWith(regexEscape))
            //use substring instead of string.replace just in case some has regex like '<regex>//File<regex>' or something else like that
            pattern = mask.Substring(regexEscape.Length);
        else
        {
            pattern = mask.Replace(".", "\\.");
            pattern = pattern.Replace("*", ".*");
            pattern = pattern.Replace("?", ".+");
            pattern = string.Concat("^", pattern, "$");
        }

        return Regex.IsMatch(filename, pattern, RegexOptions.IgnoreCase);
    }
}