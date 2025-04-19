namespace TestTask.FolderSyncService.SynchronizationUtils
{
  /// <summary>
  /// The directory utilities class.
  /// </summary>
  internal class DirectoryUtilities
  {
    /// <summary>
    /// Copies the directory.
    /// </summary>
    /// <param name="sourceDirectory">The source directory.</param>
    /// <param name="destinationDirectory">The destination directory.</param>
    /// <param name="recursive">If true, copies all sub-directories and their contents.</param>
    public static void CopyDirectory(string sourceDirectory, string destinationDirectory, bool recursive)
    {
      DirectoryInfo directory = new(sourceDirectory);

      DirectoryInfo[] subDirectories = directory.GetDirectories();

      Directory.CreateDirectory(destinationDirectory);

      foreach (FileInfo file in directory.GetFiles())
      {
        string targetFilePath = Path.Combine(destinationDirectory, file.Name);
        file.CopyTo(targetFilePath);
      }

      if (recursive)
      {
        foreach (DirectoryInfo subDirectory in subDirectories)
        {
          string newDestinationDir = Path.Combine(destinationDirectory, subDirectory.Name);
          CopyDirectory(subDirectory.FullName, newDestinationDir, true);
        }
      }
    }

    /// <summary>
    /// Creates lookups for mapping subdirectories in source directory and target directory.
    /// </summary>
    /// <param name="sourceDirectory">The source directory path.</param>
    /// <param name="targetDirectory">The target directory path.</param>
    /// <returns>A list of MappedContents of type DirectoryInfo.</returns>
    public static List<MappedContents<DirectoryInfo>> GetMappedDirectories(string sourceDirectory, string targetDirectory)
    {
      Dictionary<string, DirectoryInfo> sourceLookup = Directory.GetDirectories(sourceDirectory)
        .Select(d => new DirectoryInfo(d)).ToDictionary(d => d.Name, d => d);

      Dictionary<string, DirectoryInfo> targetLookup = Directory.GetDirectories(targetDirectory)
         .Select(d => new DirectoryInfo(d)).ToDictionary(d => d.Name, d => d);

      return MapContents(sourceLookup, targetLookup);
    }

    /// <summary>
    /// Creates lookups for mapping files in source directory and target directory.
    /// </summary>
    /// <param name="sourceDirectory">The source directory path.</param>
    /// <param name="targetDirectory">The target directory path.</param>
    /// <returns>A list of MappedContents of type FileInfo.</returns>
    public static List<MappedContents<FileInfo>> GetMappedFiles(string sourceDirectory, string targetDirectory)
    {
      Dictionary<string, FileInfo> sourceLookup = Directory.GetFiles(sourceDirectory)
        .Select(f => new FileInfo(f)).ToDictionary(f => f.Name, f => f);

      Dictionary<string, FileInfo> targetLookup = Directory.GetFiles(targetDirectory)
        .Select(f => new FileInfo(f)).ToDictionary(f => f.Name, f => f);

      return MapContents(sourceLookup, targetLookup);
    }

    /// <summary>
    /// Maps the contents info between source and target lookups.
    /// </summary>
    /// <param name="sourceLookup">The source lookup.</param>
    /// <param name="targetLookup">The target lookup.</param>
    /// <returns>A list of MappedContents.</returns>
    private static List<MappedContents<TContentsInfo>> MapContents<TContentsInfo>(Dictionary<string, TContentsInfo> sourceLookup,
      Dictionary<string, TContentsInfo> targetLookup)
    {
      HashSet<string> fileNames = [.. sourceLookup.Keys];
      fileNames.UnionWith(targetLookup.Keys);

      List<MappedContents<TContentsInfo>> mappedContents = [];

      foreach (string fileName in fileNames)
      {
        sourceLookup.TryGetValue(fileName, out var sourceInfo);
        targetLookup.TryGetValue(fileName, out var targetInfo);

        mappedContents.Add(new MappedContents<TContentsInfo>
        {
          ContentsName = fileName,
          SourceInfo = sourceInfo,
          TargetInfo = targetInfo
        });
      }

      return mappedContents;
    }
  }
}
