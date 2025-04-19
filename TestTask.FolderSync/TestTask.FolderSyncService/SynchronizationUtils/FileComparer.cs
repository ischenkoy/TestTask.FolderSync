namespace TestTask.FolderSyncService.SynchronizationUtils
{
  /// <summary>
  /// The file comparer class.
  /// </summary>
  internal class FileComparer
  {
    /// <summary>
    /// Fileinfo for source file
    /// </summary>
    protected readonly FileInfo FileInfo1;

    /// <summary>
    /// Fileinfo for target file
    /// </summary>
    protected readonly FileInfo FileInfo2;

    /// <summary>
    /// Initializes a new instance of the <see cref="FileComparer"/> class.
    /// </summary>
    /// <param name="fileInfo01">FileInfo object of first file</param>
    /// <param name="fileInfo02">FileInfo object of second file</param>
    public FileComparer(FileInfo fileInfo1, FileInfo fileInfo2)
    {
      FileInfo1 = fileInfo1;
      FileInfo2 = fileInfo2;
      EnsureFilesExist();
    }

    /// <summary>
    /// Compares the two given files and returns true if the files are the same
    /// </summary>
    /// <returns>true if the files are the same, false otherwise</returns>
    public bool Compare()
    {
      if (IsDifferentLength())
      {
        return false;
      }
      if (IsSameFile())
      {
        return true;
      }
      return OnCompare();
    }

    /// <summary>
    /// Compares the two given files and returns true if the files are the same
    /// </summary>
    /// <returns>true if the files are the same, false otherwise</returns>
    protected bool OnCompare()
    {
      var fileContents01 = File.ReadAllBytes(FileInfo1.FullName);
      var fileContents02 = File.ReadAllBytes(FileInfo2.FullName);

      int lastBlockIndex = fileContents01.Length - fileContents01.Length % sizeof(ulong);

      var totalProcessed = 0;
      while (totalProcessed < lastBlockIndex)
      {
        if (BitConverter.ToUInt64(fileContents01, totalProcessed) != BitConverter.ToUInt64(fileContents02, totalProcessed))
        {
          return false;
        }
        totalProcessed += sizeof(ulong);
      }
      return true;
    }

    /// <summary>
    /// Compares the two given files by full path and name and returns true if it's the same file
    /// </summary>
    /// <returns>true if the same file, false otherwise</returns>
    private bool IsSameFile()
    {
      return string.Equals(FileInfo1.FullName, FileInfo2.FullName, StringComparison.OrdinalIgnoreCase);
    }

    /// <summary>
    /// Does an early comparison by checking files Length, if lengths are not the same, files are definetely different
    /// </summary>
    /// <returns>true if different length</returns>
    private bool IsDifferentLength()
    {
      return FileInfo1.Length != FileInfo2.Length;
    }

    /// <summary>
    /// Makes sure files exist
    /// </summary>
    private void EnsureFilesExist()
    {
      if (FileInfo1.Exists == false)
      {
        throw new ArgumentNullException(nameof(FileInfo1));
      }
      if (FileInfo2.Exists == false)
      {
        throw new ArgumentNullException(nameof(FileInfo2));
      }
    }

  }
}
