﻿using Frends.FTP.ReadFile.Definitions;
using Frends.FTP.ReadFile.Enums;
using System;
using System.ComponentModel;
using System.Net.Security;
using System.Security.Cryptography.X509Certificates;
using System.Text;
using System.Threading;
using FluentFTP;

namespace Frends.FTP.ReadFile;

///<summary>
/// Main Task class.
/// </summary>
public class FTP
{
    /// <summary>
    /// Reads file from the FTP(S) server.
    /// [Documentation](https://tasks.frends.com/tasks/frends-tasks/Frends.FTP.ReadFile)
    /// </summary>
    /// <param name="connection">Connection parameters</param>
    /// <param name="input">Input parameters</param>
    /// <param name="cancellationToken">Token to stop task. This is generated by Frends.</param>
    /// <returns>Object { string Content, string Path, double SizeBytes, DateTime LastWriteTime }</returns>
    public static Result ReadFile([PropertyTab] Connection connection, [PropertyTab] Input input, CancellationToken cancellationToken)
    {
        using var client = CreateFtpClient(connection);
        client.Connect();

        if (!client.IsConnected) throw new ArgumentException($"Error while connecting to destination: {connection.Address}");

        if (!client.Download(out byte[] bytes, input.Path)) throw new ArgumentException($"Could not find file '{input.Path}'.");

        Encoding encoding = GetEncoding(input.FileEncoding, input.EnableBom, input.EncodingInString);
        var content = encoding.GetString(bytes);
        var file = client.GetObjectInfo(input.Path);

        client.Disconnect();
        client.Dispose();

        return new Result(content, file);
    }

    internal static FtpClient CreateFtpClient(Connection connect)
    {
        var client = new FtpClient(connect.Address, connect.Port, connect.UserName, connect.Password);
        switch (connect.SslMode)
        {
            case FtpsSslMode.None:
                client.EncryptionMode = FtpEncryptionMode.None;
                break;
            case FtpsSslMode.Implicit:
                client.EncryptionMode = FtpEncryptionMode.Implicit;
                break;
            case FtpsSslMode.Explicit:
                client.EncryptionMode = FtpEncryptionMode.Explicit;
                break;
            case FtpsSslMode.Auto:
                client.EncryptionMode = FtpEncryptionMode.Auto;
                break;
            default:
                throw new ArgumentOutOfRangeException();
        }

        if (connect.UseFTPS)
        {
            if (connect.EnableClientAuth)
                client.ClientCertificates.Add(new X509Certificate2(connect.ClientCertificatePath));

            client.ValidateCertificate += (control, e) =>
            {
                // If cert is valid and such - go on and accept.
                if (e.PolicyErrors == SslPolicyErrors.None)
                {
                    e.Accept = true;
                    return;
                }

                // Accept if we want to accept a certain hash.
                e.Accept = e.Certificate.GetCertHashString() == connect.CertificateHashStringSHA1;
            };

            client.ValidateAnyCertificate = connect.ValidateAnyCertificate;
            client.DataConnectionEncryption = connect.SecureDataChannel;
        }

        client.NoopInterval = connect.KeepConnectionAliveInterval;

        if (!string.IsNullOrWhiteSpace(connect.Encoding)) client.Encoding = Encoding.GetEncoding(connect.Encoding);

        // Client lib timeout is in milliseconds, ours is in seconds, thus *1000 conversion
        client.ConnectTimeout = connect.ConnectionTimeout * 1000;
        client.LocalFileBufferSize = connect.BufferSize;

        // Transport type Binary / ASCII
        switch (connect.TransportType)
        {
            case FtpTransportType.Binary:
                client.UploadDataType = FtpDataType.Binary;
                client.DownloadDataType = FtpDataType.Binary;
                break;
            case FtpTransportType.Ascii:
                client.UploadDataType = FtpDataType.ASCII;
                client.DownloadDataType = FtpDataType.ASCII;
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unknown FTP transport type {connect.TransportType}");
        }

        // Active/passive
        switch (connect.Mode)
        {
            case FtpMode.Active:
                client.DataConnectionType = FtpDataConnectionType.AutoActive;
                break;
            case FtpMode.Passive:
                client.DataConnectionType = FtpDataConnectionType.AutoPassive;
                break;
            default:
                throw new ArgumentOutOfRangeException($"Unknown FTP mode {connect.Mode}");
        }

        return client;
    }

    private static Encoding GetEncoding(FileEncoding encoding, bool enableBom, string encodingString = null)
    {
        switch (encoding)
        {
            case FileEncoding.UTF8:
                return enableBom ? new UTF8Encoding(true) : new UTF8Encoding(false);
            case FileEncoding.ASCII:
                return new ASCIIEncoding();
            case FileEncoding.ANSI:
                return Encoding.Default;
            case FileEncoding.WINDOWS1252:
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                return Encoding.GetEncoding("windows-1252");
            case FileEncoding.Other:
                Encoding.RegisterProvider(CodePagesEncodingProvider.Instance);
                var e = Encoding.GetEncoding(encodingString);
                if (e == null) throw new ArgumentException($"Encoding string {encodingString} is not a valid code page name.");
                return e;
            default:
                throw new ArgumentOutOfRangeException($"Unknown Encoding type: '{encoding}'.");
        }
    }
}