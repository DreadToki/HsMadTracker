namespace HsTracker.Core;

public class LogDirectoryWatcher(
    string gameRootDirectory,
    string logDirectoryName,
    string powerLogFileName
) : IDisposable
{
    private FileSystemWatcher? _directoryWatcher;

    private FileSystemWatcher? _powerLogWatcher;

    private PowerLogReader? _powerLogReader;

    public void Dispose()
    {
        _powerLogReader?.Dispose();
        _powerLogWatcher?.Created -= OnPowerLogCreated;
        _powerLogWatcher?.Dispose();
        _directoryWatcher?.Created -= OnDirectoryCreated;
        _directoryWatcher?.Dispose();
    }

    public void Init()
    {
        string logDirectory = Path.Combine(gameRootDirectory, logDirectoryName);

        if (!Directory.Exists(logDirectory))
        {
            throw new DirectoryNotFoundException(
                $"The specified directory '{logDirectory}' does not exist."
            );
        }

        _directoryWatcher = new(logDirectory) { EnableRaisingEvents = true };

        _directoryWatcher.Created += OnDirectoryCreated;
    }

    private void OnDirectoryCreated(object sender, FileSystemEventArgs e)
    {
        _powerLogWatcher = new(e.FullPath, powerLogFileName) { EnableRaisingEvents = true };

        _powerLogWatcher.Created += OnPowerLogCreated;
    }

    private void OnPowerLogCreated(object sender, FileSystemEventArgs e)
    {
        _powerLogReader = new(e.FullPath);
        _powerLogReader.Init();
    }
}
