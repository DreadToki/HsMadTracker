using HsTracker.Parsers;
using Microsoft.Extensions.Logging;

namespace HsTracker.Readers;

public class PowerLogReader : IDisposable
{
    private FileSystemWatcher? _watcher;

    private FileStream? _fileStream;

    private StreamReader? _streamReader;

    private readonly ILogger<PowerLogReader> _logger;

    private readonly PowerLogParser _parser;

    public PowerLogReader(ILogger<PowerLogReader> logger, PowerLogParser parser)
    {
        _logger = logger;
        _parser = parser;
    }

    public void Dispose()
    {
        _watcher?.Changed -= OnLogFileChanged;
        _watcher?.Dispose();
        _streamReader?.Dispose();
        _fileStream?.Dispose();
    }

    public void Init(string powerLogFile)
    {
        var fileInfo = new FileInfo(powerLogFile);
        _fileStream = new(powerLogFile, FileMode.Open, FileAccess.Read, FileShare.ReadWrite);
        _streamReader = new(_fileStream);

        ReadPowerLog();

        _watcher = new FileSystemWatcher(fileInfo.DirectoryName!, fileInfo.Name)
        {
            NotifyFilter = NotifyFilters.LastWrite,
            EnableRaisingEvents = true,
        };

        _watcher.Changed += OnLogFileChanged;
    }

    private void OnLogFileChanged(object sender, FileSystemEventArgs e)
    {
        ReadPowerLog();
    }

    private void ReadPowerLog()
    {
        _parser.ReadBlock(_streamReader);
    }
}
