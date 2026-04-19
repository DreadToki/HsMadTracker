namespace HsTracker.Core;

public class PowerLogReader(string powerLogFile) : IDisposable
{
    private FileSystemWatcher? _watcher;

    private FileStream? _fileStream;

    private StreamReader? _streamReader;

    private PowerLogParser _parser = new();

    public void Dispose()
    {
        _watcher?.Changed -= OnLogFileChanged;
        _watcher?.Dispose();
        _streamReader?.Dispose();
        _fileStream?.Dispose();
    }

    public void Init()
    {
        FileInfo fileInfo = new(powerLogFile);
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
        while (_streamReader?.ReadLine() is string line)
        {
            System.Console.WriteLine($"{line}");
            _parser.ParseLine(line);
        }
    }
}
