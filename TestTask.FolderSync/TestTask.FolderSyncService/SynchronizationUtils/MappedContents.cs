using Microsoft.Extensions.FileProviders;

namespace TestTask.FolderSyncService.SynchronizationUtils
{
  /// <summary>
  /// Model of contents mapped between source path and target path.
  /// </summary>
  internal class MappedContents<TContentsInfo>
  {
    /// <summary>
    /// Gets or sets the contents name, e.g. file or folder name.
    /// </summary>
    public required string ContentsName { get; set; }

    /// <summary>
    /// Gets or sets the contents info from source.
    /// </summary>
    public TContentsInfo? SourceInfo { get; set; }

    /// <summary>
    /// Gets or sets the contents info from target.
    /// </summary>
    public TContentsInfo? TargetInfo { get; set; }
  }
}
