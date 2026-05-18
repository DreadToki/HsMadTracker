using HsTracker.Readers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HsTracker.Watchers;

public class SessionLogsDirectoryWatcher : IDisposable, IFileSystemWatcherHandler
{
    private const string LogDirectoryName = "Logs";

    private readonly ILogger<SessionLogsDirectoryWatcher> _logger;

    private readonly IConfiguration _configuration;

    private readonly IMemoryCache _memoryCache;

    private FileSystemWatcher? _directoryWatcher;

    private IFileSystemWatcherHandler[]? _handlers;

    public SessionLogsDirectoryWatcher(
        ILogger<SessionLogsDirectoryWatcher> logger,
        IConfiguration configuration,
        IMemoryCache memoryCache
    )
    {
        _logger = logger;
        _configuration = configuration;
        _memoryCache = memoryCache;
    }

    public void Dispose()
    {
        _directoryWatcher?.Created -= OnDirectoryCreated;
        _directoryWatcher?.Dispose();
    }

    public void Handle()
    {
        var gameRootDirectoryPath = _configuration.GetValue<string>("GameRootDirectoryPath");

        if (!Directory.Exists(gameRootDirectoryPath))
        {
            _logger.LogError($"The specified directory '{gameRootDirectoryPath}' does not exist.");
            return;
        }

        string logDirectoryPath = Path.Combine(gameRootDirectoryPath, LogDirectoryName);

        if (!Directory.Exists(logDirectoryPath))
        {
            _logger.LogError($"The specified directory '{logDirectoryPath}' does not exist.");
            return;
        }

        _directoryWatcher = new(logDirectoryPath) { EnableRaisingEvents = true };

        _directoryWatcher.Created += OnDirectoryCreated;
    }

    public void SetNext(params IFileSystemWatcherHandler[] handlers)
    {
        _handlers = handlers;
    }

    private void OnDirectoryCreated(object sender, FileSystemEventArgs e)
    {
        _memoryCache.Set("SessionLogsDirectoryPath", e.FullPath);

        foreach (var handler in _handlers ?? [])
        {
            handler.Handle();
        }
    }
}
