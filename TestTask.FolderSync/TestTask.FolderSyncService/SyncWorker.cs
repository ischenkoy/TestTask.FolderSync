using Microsoft.Extensions.Options;
using TestTask.FolderSyncService.SynchronizationUtils;

namespace TestTask.FolderSyncService
{
  /// <summary>
  /// The background service which performs two directories one-way synchronization (source-to-target).
  /// </summary>
  public class SyncWorker : BackgroundService
	{
    private readonly SyncWorkerOptions _options;
		private readonly string _sourceDir;
		private readonly string _targetDir;
    private readonly TimeSpan _syncInterval;
    private readonly ILogger<SyncWorker> _logger;

    /// <summary>
    /// Initializes a new instance of the <see cref="SyncWorker"/> class.
    /// </summary>
    /// <param name="logger">The logger.</param>
    /// <param name="options">The options.</param>
    public SyncWorker(ILogger<SyncWorker> logger, IOptions<SyncWorkerOptions> options)
    {
      _options = options.Value;
      _sourceDir = _options.SourcePath;
      _targetDir = _options.TargetPath;
      _syncInterval = _options.SyncInterval;
      _logger = logger;
    }

    /// <summary>
    /// Method is called when <see cref="IHostedService"/> starts.
    /// </summary>
    /// <param name="stoppingToken">The cancellation token.</param>
    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
		{
      if (!Directory.Exists(_sourceDir))
      {
        throw new DirectoryNotFoundException($"Source directory doesn't exist: {_sourceDir}");
      }

      if (!Directory.Exists(_targetDir))
      {
        throw new DirectoryNotFoundException($"Source directory doesn't exist: {_sourceDir}");
      }

      _logger.LogInformation("Starting SyncWorker with following parameters: " +
        "\nSourcePath: {SourcePath}" +
        "\nTargetPath: {TargetPath}" +
        "\nSyncInterval: {SyncInterval}" +
        "\nLogFilePath: {LogFilePath}",
        _sourceDir, _targetDir, _syncInterval, _options.LogFilePath);

      while (!stoppingToken.IsCancellationRequested)
			{
        try
        {
          await Task.Run(() => SyncContents(_sourceDir, _targetDir), stoppingToken);

          _logger.LogInformation(message: 
            "Synchronization of {SourceDir} to {TargetDir} is successfully completed", _sourceDir, _targetDir);
        }
        catch (Exception ex)
        {
          _logger.LogError(message: "Failed to sync: {Message},{NewLine}{StackTrace}", 
            ex.Message, Environment.NewLine, ex.StackTrace);

          throw;
        }
				
				await Task.Delay(_syncInterval, stoppingToken);
			}

      _logger.LogInformation("Stopping SyncWorker...");
    }

    /// <summary>
    /// Performs directories content synchronization.
    /// </summary>
    /// <param name="sourceDirectory">The source directory.</param>
    /// <param name="targetDirectory">The target directory.</param>
    public void SyncContents(string sourceDirectory, string targetDirectory)
    {
      _logger.LogInformation("Starting synchronization: {Source} to {Target}", sourceDirectory, targetDirectory);

      List<MappedContents<FileInfo>> mappedFiles = DirectoryUtilities.GetMappedFiles(sourceDirectory, targetDirectory);

      IEnumerable<MappedContents<FileInfo>> filesToCompare = mappedFiles.Where(f => f.SourceInfo != null && f.TargetInfo != null);
      IEnumerable<MappedContents<FileInfo>> filesToCopy = mappedFiles.Where(f => f.SourceInfo != null && f.TargetInfo == null);
      IEnumerable<MappedContents<FileInfo>> filesToDelete = mappedFiles.Where(f => f.SourceInfo == null && f.TargetInfo != null);

      foreach (MappedContents<FileInfo> mappedFile in filesToCompare)
      {
        FileComparer fileComparer = new(mappedFile.SourceInfo!, mappedFile.TargetInfo!);

        if (!fileComparer.Compare())
        {
          try
          {
            mappedFile.SourceInfo!.CopyTo(mappedFile.TargetInfo!.FullName, true);

            _logger.LogInformation("Replaced {FileName} in target with source file", mappedFile.ContentsName);
          }
          catch (Exception ex) 
          {
            _logger.LogError("Failed copying {FileName} from source to target: {ExceptionMessage}", 
              mappedFile.ContentsName, ex.Message);

            throw;
          }
        }
      }

      foreach (MappedContents<FileInfo> mappedFile in filesToCopy)
      {
        try
        {
          string targetFileName = Path.Combine(targetDirectory, mappedFile.ContentsName);
          mappedFile.SourceInfo!.CopyTo(targetFileName);

          _logger.LogInformation("Copied {FileName} from source to target", mappedFile.ContentsName);
        }
        catch (Exception ex)
        {
          _logger.LogError("Failed copying {FileName} from source to target: {ExceptionMessage}", 
            mappedFile.ContentsName, ex.Message);

          throw;
        }
      }

      foreach (MappedContents<FileInfo> mappedFile in filesToDelete)
      {
        try
        {
          mappedFile.TargetInfo!.Delete();

          _logger.LogInformation("Removed {FileName} from target", mappedFile.ContentsName);
        }
        catch (Exception ex)
        {
          _logger.LogError("Failed removing {FileName} from target: {ExceptionMessage}", mappedFile.ContentsName, ex.Message);

          throw;
        }
      }

      ProcessSubDirectories(sourceDirectory, targetDirectory);
    }

    /// <summary>
    /// Performs sub directories synchronization.
    /// </summary>
    /// <param name="sourceDirectory">The source directory.</param>
    /// <param name="targetDirectory">The target directory.</param>
    private void ProcessSubDirectories(string sourceDirectory, string targetDirectory)
    {
      List<MappedContents<DirectoryInfo>> mappedDirectories = 
        DirectoryUtilities.GetMappedDirectories(sourceDirectory, targetDirectory);

      IEnumerable<MappedContents<DirectoryInfo>> dirsToSync = mappedDirectories
        .Where(f => f.SourceInfo != null && f.TargetInfo != null);

      IEnumerable<MappedContents<DirectoryInfo>> dirsToCopy = mappedDirectories
        .Where(f => f.SourceInfo != null && f.TargetInfo == null);

      IEnumerable<MappedContents<DirectoryInfo>> dirsToDelete = mappedDirectories
        .Where(f => f.SourceInfo == null && f.TargetInfo != null);

      foreach (MappedContents<DirectoryInfo> mappedDirectory in dirsToSync)
      {
        SyncContents(mappedDirectory.SourceInfo!.FullName, mappedDirectory.TargetInfo!.FullName);
      }

      foreach (MappedContents<DirectoryInfo> mappedDirectory in dirsToCopy)
      {
        try
        {
          string destinationDirectory = Path.Combine(targetDirectory, mappedDirectory.ContentsName);
          DirectoryUtilities.CopyDirectory(mappedDirectory.SourceInfo!.FullName, destinationDirectory, true);

          _logger.LogInformation("Copied {DirName} and its contents from source to target", mappedDirectory.ContentsName);
        }
        catch (Exception ex) 
        {
          _logger.LogError("Failed copying {DirName} and its contents from source to target: {ExceptionMessage}",
            mappedDirectory.ContentsName, ex.Message);

          throw;
        }
      }

      foreach (MappedContents<DirectoryInfo> mappedDirectory in dirsToDelete)
      {
        try
        {
          mappedDirectory.TargetInfo!.Delete(true);

          _logger.LogInformation("Removed {DirName} and its contents from target", mappedDirectory.ContentsName);
        }
        catch (Exception ex)
        {
          _logger.LogError("Failed removing {DirName} and its contents from target: {ExceptionMessage}",
            mappedDirectory.ContentsName, ex.Message);

          throw;
        }
      }
    }
  }
}
