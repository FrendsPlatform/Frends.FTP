using System.Text.RegularExpressions;

namespace Frends.FTP.DownloadFiles.Definitions;

///<summary>
/// Policies for creating names for remote files: expands macros etc.
///</summary>
internal class RenamingPolicy
{
    private readonly IDictionary<string, Func<string, string>> _macroHandlers;
    private readonly IDictionary<string, Func<string, string>> _sourceFileNameMacroHandlers;

    public RenamingPolicy(string transferName, Guid transferId)
    {
        _macroHandlers = InitializeMacroHandlers(transferName, transferId);
        _sourceFileNameMacroHandlers = InitializeSourceFileNameMacroHandlers();
    }

    /// <summary>
    /// Creates a remote file name based on the settings and remote file definition
    /// </summary>
    /// <param name="originalFileName">The original file.</param>
    /// <param name="remoteFileDefinition">The remote file path with macros.</param>
    /// <returns>Remote file name with expanded macros.</returns>
    public string CreateRemoteFileName(string originalFileName, string remoteFileDefinition)
    {
        if (!string.IsNullOrEmpty(remoteFileDefinition) && remoteFileDefinition.Contains('?'))
            throw new ArgumentException("Character '?' not allowed in remote filename.", nameof(remoteFileDefinition));

        if (string.IsNullOrEmpty(originalFileName))
            throw new ArgumentException("Original filename must be set.", nameof(originalFileName));

        var originalFileNameWithoutPath = Path.GetFileName(originalFileName);

        if (string.IsNullOrEmpty(remoteFileDefinition))
            return originalFileNameWithoutPath;

        if (!IsFileMask(remoteFileDefinition) &&
            !IsFileMacro(remoteFileDefinition, _macroHandlers) &&
            !IsFileMacro(remoteFileDefinition, _sourceFileNameMacroHandlers))
        {
            // remoteFileDefinition does not have macros
            var remoteFileName = Path.GetFileName(remoteFileDefinition);

            if (string.IsNullOrEmpty(remoteFileName))
                remoteFileDefinition = Path.Combine(remoteFileDefinition, originalFileNameWithoutPath);

            return remoteFileDefinition;
        }

        var result = ExpandMacrosAndMasks(originalFileName, remoteFileDefinition);

        if (result.EndsWith("\\"))
            result = Path.Combine(result, originalFileNameWithoutPath);

        return result;
    }

    /// <summary>
    /// Method for expanding source/destination endpoint directory name for macros when opening the endpoint connection
    /// </summary>
    /// <param name="directory">Directory path including unexpanded macros</param>
    /// <returns>Directory with macros expanded.</returns>
    public string ExpandDirectoryForMacros(string directory)
    {
        if (directory.Contains("%SourceFileName%") || directory.Contains("%SourceFileExtension%"))
            throw new Exception("'%SourceFileName%' and '%SourceFileExtension%' are not supported macros for source and destination directories.");

        return ExpandFileMacros(directory);
    }

    /// <summary>
    /// Creates the file path to use for source operation: Move
    /// The SourceOperationTo should always be a directory. If it is empty, an error should be thrown
    /// The current file name should always be appended to the directory name
    /// The directory name cannot use file macros or file masks        
    /// </summary>        
    public static string CreateRemoteFileNameForMove(string sourceOperationTo, string sourceFilePath)
    {
        var directoryName = sourceOperationTo;
        if (string.IsNullOrEmpty(directoryName))
        {
            throw new ArgumentException(
                "When using move as a source operation, you should always define a directory",
                nameof(sourceOperationTo));
        }

        directoryName = CanonizeAndCheckPath(directoryName);

        // this should always be a directory
        if (!directoryName.EndsWith("/")) directoryName += "/";

        var sourceFileName = Path.GetFileName(sourceFilePath);
        return Path.Combine(directoryName, sourceFileName);
    }

    private static string CanonizeAndCheckPath(string path)
    {
        // make all the paths use forward slashes - this should be supported on File, FTP, and SFTP
        path = path.Replace(Path.DirectorySeparatorChar, '/');

        if (path.IndexOfAny(GetInvalidChars()) != -1)
            throw new ArgumentException("Illegal characters in path: " + path);
        return path;
    }

    private static char[] GetInvalidChars()
    {
        List<char> invalidCharacters = new(Path.GetInvalidFileNameChars());
        invalidCharacters.Remove('/'); // remove the forward slash, as it is supported
        invalidCharacters.Remove(':'); // also the colon is supported
        return invalidCharacters.ToArray();
    }


    public string CreateFilePathForRename(string originalFilePath, string sourceOperationTo)
    {
        if (string.IsNullOrEmpty(sourceOperationTo))
            throw new ArgumentException("When using rename as a source operation, you need to define the new name");

        var filePath = ExpandMacrosAndMasks(originalFilePath, sourceOperationTo);

        var result = CanonizeAndCheckPath(filePath);
        var originalFileDirectory = Path.GetDirectoryName(originalFilePath);

        // Path Combine will ignore originalFileDirectory is result already
        // contains absolute path. Thus we either get the whole path in result or, if
        // it is not an absolute path - then we get a path with original file dir as base.
        return Path.Combine(originalFileDirectory, result);
    }

    private string ExpandMacrosAndMasks(string originalFilePath, string filePath)
    {
        var expandedPath = ExpandFileMacros(filePath);
        expandedPath = ExpandSourceFileNameMacros(expandedPath, originalFilePath);
        expandedPath = ExpandFileMasks(expandedPath, originalFilePath);

        return expandedPath;
    }

    private string ExpandFileMacros(string filePath)
    {
        string filename = filePath;
        if (IsFileMacro(filename, _macroHandlers))
            filename = ReplaceMacro(filename);

        return filename;
    }

    private string ExpandSourceFileNameMacros(string filePath, string originalFile)
    {
        string filename = filePath;
        if (IsFileMacro(filename, _sourceFileNameMacroHandlers))
            filename = ReplaceSourceFileMacro(filename, originalFile);

        return filename;
    }

    private static string ExpandFileMasks(string filePath, string originalFileName)
    {
        string filename = filePath;
        if (IsFileMask(filename))
            filename = NameByMask(originalFileName, filename);

        return filename;
    }

    private static string NameByMask(string filename, string mask)
    {
        //remove extension if it is wanted to be changed, new extension is added later on to new filename
        if (mask.Contains("*.") && Path.HasExtension(filename))
            filename = Path.GetFileNameWithoutExtension(filename);

        int i = mask.IndexOf("*", StringComparison.InvariantCulture);
        if (i >= 0)
        {
            string tmp = mask.Substring(0, i);
            return tmp + filename + mask.Substring(i + 1, (mask.Length - (i + 1)));
        }

        //Not an mask return mask.
        return mask;
    }

    private static bool IsFileMacro(string s, IDictionary<string, Func<string, string>> macroDictionary)
    {
        if (s == null) return false;

        if (macroDictionary.Keys.Where(e => s.ToUpperInvariant().Contains(e.ToUpperInvariant())).Any())
            return true;

        return false;
    }

    private static bool IsFileMask(string input)
    {
        var isFileMask = false;
        if (string.IsNullOrEmpty(input)) return false;
        if (input.Contains('*')) isFileMask = true;
        if (input.Contains('?')) isFileMask = true;
        return isFileMask;
    }

    private static IDictionary<string, Func<string, string>> InitializeSourceFileNameMacroHandlers()
    {
        return new Dictionary<string, Func<string, string>>
            {
                {"%SourceFileName%", Path.GetFileNameWithoutExtension},
                {"%SourceFileExtension%", (originalFile) => Path.HasExtension(originalFile) ? Path.GetExtension(originalFile) : String.Empty},
            };
    }

    private static IDictionary<string, Func<string, string>> InitializeMacroHandlers(string transferName, Guid transferId)
    {
        return new Dictionary<string, Func<string, string>>
            {
                {"%Ticks%", (s) => DateTime.Now.Ticks.ToString()},
                {"%DateTimeMs%", (s) => DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss-fff")},
                {"%DateTime%", (s) => DateTime.Now.ToString("yyyy-MM-dd-HH-mm-ss")},
                {"%Date%", (s) => DateTime.Now.ToString("yyyy-MM-dd")},
                {"%Time%", (s) => DateTime.Now.ToString("HH-mm-ss")},
                {"%Year%", (s) => DateTime.Now.ToString("yyyy")},
                {"%Month%", (s) => DateTime.Now.ToString("MM")},
                {"%Day%", (s) => DateTime.Now.ToString("dd")},
                {"%Hour%", (s) => DateTime.Now.ToString("HH")},
                {"%Minute%", (s) => DateTime.Now.ToString("mm")},
                {"%Second%", (s) => DateTime.Now.ToString("ss")},
                {"%Millisecond%", (s) => DateTime.Now.ToString("fff")},
                {"%Guid%", (s) => Guid.NewGuid().ToString()},
                {"%TransferName%", (s) => !String.IsNullOrEmpty(transferName) ? transferName : String.Empty},
                {"%TransferId%", (s) => transferId.ToString().ToUpper()},
                {"%WeekDay%", (s) => (DateTime.Now.DayOfWeek > 0 ? (int)DateTime.Now.DayOfWeek : 7).ToString()}
            };
    }

    private string ReplaceSourceFileMacro(string fileDefinition, string originalFile)
    {
        return ExpandMacrosFromDictionary(fileDefinition, _sourceFileNameMacroHandlers, originalFile);
    }

    private string ReplaceMacro(string fileDefinition)
    {
        return ExpandMacrosFromDictionary(fileDefinition, _macroHandlers, "");
    }

    private static string ExpandMacrosFromDictionary(string fileDefinition, IDictionary<string, Func<string, string>> macroHandlers, string originalFile)
    {
        foreach (var macroHandler in macroHandlers)
        {
            fileDefinition = Regex.Replace(
                fileDefinition,
                Regex.Escape(macroHandler.Key),
                macroHandler.Value.Invoke(originalFile),
                RegexOptions.IgnoreCase);
        }

        return fileDefinition;
    }
}

