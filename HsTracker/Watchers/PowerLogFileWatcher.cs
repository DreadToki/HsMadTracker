using HsTracker.Readers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;

namespace HsTracker.Watchers;

public class PowerLogFileWatcher : IDisposable, IFileSystemWatcherHandler
{
    private readonly ILogger<PowerLogFileWatcher> _logger;

    private readonly IConfiguration _configuration;

    private readonly IMemoryCache _memoryCache;

    private readonly PowerLogReader _powerLogReader;

    private FileSystemWatcher? _powerLogWatcher;

    private IFileSystemWatcherHandler[]? _next;

    public PowerLogFileWatcher(
        ILogger<PowerLogFileWatcher> logger,
        IConfiguration configuration,
        IMemoryCache memoryCache,
        PowerLogReader powerLogReader
    )
    {
        _logger = logger;
        _configuration = configuration;
        _memoryCache = memoryCache;
        _powerLogReader = powerLogReader;
    }

    public void Dispose()
    {
        _powerLogWatcher?.Created -= OnPowerLogCreated;
        _powerLogWatcher?.Dispose();
    }

    public void Handle()
    {
        var powerLogDirectoryPath = _memoryCache.Get<string>("SessionLogsDirectoryPath");

        var powerLogFileName = _configuration.GetValue<string>("PowerLogFileName");

        if (!Directory.Exists(powerLogDirectoryPath) || string.IsNullOrWhiteSpace(powerLogFileName))
        {
            _logger.LogError("Power.log directory path or file name is not configured.");
            return;
        }

        _powerLogWatcher = new(powerLogDirectoryPath!, powerLogFileName)
        {
            EnableRaisingEvents = true,
        };

        _powerLogWatcher.Created += OnPowerLogCreated;

        for (int i = 0; i < _next?.Length; i++)
        {
            _next[i].Handle();
        }
    }

    public void SetNext(params IFileSystemWatcherHandler[] handlers)
    {
        _next = handlers;
    }

    private void OnPowerLogCreated(object sender, FileSystemEventArgs e)
    {
        _powerLogReader.Init(e.FullPath);
    }
}
