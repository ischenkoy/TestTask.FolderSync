namespace TestTask.FolderSyncService
{
  /// <summary>
  /// The <see cref="SyncWorker"/> options.
  /// </summary>
  public class SyncWorkerOptions
  {
    /// <summary>
    /// Gets or sets the source path.
    /// </summary>
    public required string SourcePath { get; set; }

    /// <summary>
    /// Gets or sets the target path.
    /// </summary>
    public required string TargetPath { get; set; }

    /// <summary>
    /// Gets or sets the sync interval.
    /// </summary>
    public TimeSpan SyncInterval { get; set; } = TimeSpan.FromMinutes(10);

    /// <summary>
    /// Gets or sets the log file path.
    /// </summary>
    public string LogFilePath { get; set; } = $"{Environment.CurrentDirectory}";
  }
}
