using System.Collections.Generic;

namespace Frends.FTP.ListFiles.Definitions
{
    /// <summary>
    /// List of files.
    /// </summary>
    public class Result
    {
        /// <summary>
        /// List of files.
        /// </summary>
        public List<ListObject> Files { get; internal set; }
    }
}

