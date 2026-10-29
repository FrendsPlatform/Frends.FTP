using System;
using System.IO;
using System.Text.RegularExpressions;

namespace Frends.FTP.UploadFiles.Definitions
{
    internal static class Util
    {
        /// <summary>
        /// Creates unique file name
        /// </summary>
        /// <returns>return unique file name</returns>
        public static string CreateUniqueFileName()
        {
            return Path.ChangeExtension("frends_" + DateTime.Now.Ticks + Path.GetRandomFileName(), "8CO");
        }

        /// <summary>
        /// Checks if the file name matches the given file mask.
        /// The file mask is also checked with a regular expression.
        /// </summary>
        /// <param name="filename">The file name to check.</param>
        /// <param name="mask">The file mask or a regular expression prefixed with &lt;regex&gt;.</param>
        /// <returns>True if the file name matches the mask, otherwise false.</returns>
        public static bool FileMatchesMask(string filename, string mask)
        {
            const string regexEscape = "<regex>";
            string pattern;

            // check is pure regex wished to be used for matching
            if (mask.StartsWith(regexEscape))
            {
                // use substring instead of string.replace just in case some has regex like '<regex>//File<regex>' or something else like that
                pattern = mask.Substring(regexEscape.Length);
            }
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
