using Frends.FTP.ListFiles.Definitions;
using System;
using System.Collections.Generic;
using System.ComponentModel;
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
    /// <param name="input">Input parameters</param>
    /// <param name="cancellationToken">Token to stop task. This is generated by Frends.</param>
    /// <returns>List { DateTime Modified, string Name, string FullPath, long SizeBytes }</returns>
    public static async Task<Result> ListFiles([PropertyTab] Input input, CancellationToken cancellationToken)
    {
        if (string.IsNullOrWhiteSpace(input.Host)) throw new Exception("Host required.");
        if (string.IsNullOrWhiteSpace(input.Username) && !string.IsNullOrWhiteSpace(input.Password)) throw new Exception("Username required.");
        if (!string.IsNullOrWhiteSpace(input.Username) && string.IsNullOrWhiteSpace(input.Password)) throw new Exception("Password required.");

        return new Result { Files = await GetListAsync(input, cancellationToken) };
    }

    private static async Task<List<ListObject>> GetListAsync(Input input, CancellationToken cancellationToken)
    {
        var result = new List<ListObject>();

        try
        {
            var client = new FtpClient(input.Host, input.Port, input.Username, input.Password);
            client.AutoConnect();

            foreach (var item in await client.GetListingAsync(input.Path, FtpListOption.Auto, cancellationToken))
            {
                cancellationToken.ThrowIfCancellationRequested();
                if (item.Type == FtpFileSystemObjectType.Directory)
                    continue;

                if (item.Type == FtpFileSystemObjectType.File && !string.IsNullOrWhiteSpace(input.Filename) && FileMatchesMask(item.Name, input.Filename))
                {
                    result.Add(new ListObject
                    {
                        LastModified = item.Modified,
                        Name = item.Name,
                        FullPath = item.FullName,
                        SizeBytes = item.Size
                    });
                    continue;
                }

                if (item.Type == FtpFileSystemObjectType.File && string.IsNullOrWhiteSpace(input.Filename))
                {
                    result.Add(new ListObject
                    {
                        LastModified = item.Modified,
                        Name = item.Name,
                        FullPath = item.FullName,
                        SizeBytes = item.Size
                    });
                }
            }
        }
        catch (Exception ex)
        {
            throw new Exception($"Error when creating a list of files. {ex}");
        }
        return result;
    }

    private static bool FileMatchesMask(string filename, string mask)
    {
        const string regexEscape = "<regex>";
        string pattern;

        //check is pure regex wished to be used for matching
        if (mask.StartsWith(regexEscape))
            //use substring instead of string.replace just in case some has regex like '<regex>//File<regex>' or something else like that
            pattern = mask.Substring(regexEscape.Length);
        else
        {
            pattern = mask.Replace(".", "\\.");
            pattern = pattern.Replace("*", ".*");
            pattern = pattern.Replace("?", ".+");
            pattern = String.Concat("^", pattern, "$");
        }

        return Regex.IsMatch(filename, pattern, RegexOptions.IgnoreCase);
    }
}