using Serilog;
using TestTask.FolderSyncService;

var builder = Host.CreateApplicationBuilder(args);

var config = builder.Configuration;

if (config["sourcePath"] == null || config["targetPath"] == null)
{
  throw new Exception("sourcePath and targetPath must be provided");
}

string? logFilePath = config["logFilePath"] ?? $"{Environment.CurrentDirectory}/SyncLogs/default.log";

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .WriteTo.Async(a => a.Console())
    .WriteTo.Async(a => a.File(logFilePath, rollingInterval: RollingInterval.Infinite))
    .CreateLogger();

builder.Logging.ClearProviders();
builder.Logging.AddSerilog(Log.Logger);

builder.Services.AddHostedService<SyncWorker>()
  .Configure<SyncWorkerOptions>(config);

var host = builder.Build();
host.Run();
