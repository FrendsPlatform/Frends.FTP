using System;
using System.Text.RegularExpressions;
using System.IO;

namespace Frends.FTP.DownloadFiles.Definitions
{
    internal static class Util
    {
        public static string CreateUniqueFileName()
        {
            return Path.ChangeExtension("frends_" + DateTime.Now.Ticks + Path.GetRandomFileName(), "8CO");
        }

        public static bool FileMatchesMask(string filename, string mask)
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
                pattern = string.Concat("^", pattern, "$");
            }

            return Regex.IsMatch(filename, pattern, RegexOptions.IgnoreCase);
        }
    }
}
