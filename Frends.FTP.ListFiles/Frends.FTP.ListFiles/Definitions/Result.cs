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
        public List<ListObject> ListObject { get; set; }

        internal Result(List<ListObject> listObject)
        {
            ListObject = listObject;
        }
    }
}

