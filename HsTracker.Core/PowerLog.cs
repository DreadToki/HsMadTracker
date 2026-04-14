namespace HsTracker.Core;

public class PowerLog
{
    // TODO: Review the usage of FileSystemEventHandler
    public event EventHandler<string>? NewLogFileChanged;
    public required string CurrentPath { get; set; }
    private const string _currentLogFile = "Power.log";
    string? _currentLogFilePath;
    private FileSystemWatcher? _watcher;
    private FileStream? _fileStream;
    private StreamReader? _streamReader;
    private LogDirectory? _monitoringPath;

    public void StartWatching()
    {
        _monitoringPath = new() { ParentPath = CurrentPath };

        _monitoringPath.PathChange += (path) =>
        {
            _currentLogFilePath = path;
            SetupFileStream(path);
        };
        _monitoringPath.StartWatching();

        if (_currentLogFilePath != null)
        {
            System.Console.WriteLine($"Monitoring path: {_currentLogFilePath}");
        }
    }

    private void SetupFileStream(string directoryPath)
    {
        _streamReader?.Dispose();
        _fileStream?.Dispose();
        _watcher?.Dispose();

        string fullFilePath = Path.Combine(directoryPath, _currentLogFile);

        _watcher = new FileSystemWatcher(directoryPath, _currentLogFile)
        {
            NotifyFilter = NotifyFilters.LastWrite | NotifyFilters.Size | NotifyFilters.FileName,
            EnableRaisingEvents = true,
        };

        if (File.Exists(fullFilePath))
        {
            InitializeReader(fullFilePath);
        }
        else
        {
            _watcher.Created += (s, e) =>
            {
                InitializeReader(e.FullPath);
            };
        }
        _watcher.Changed += OnLogFileChanged;
    }

    private void InitializeReader(string fullFilePath)
    {
        if (_streamReader != null)
            return;

        _fileStream = new FileStream(
            fullFilePath,
            FileMode.Open,
            FileAccess.Read,
            FileShare.ReadWrite
        );
        _streamReader = new StreamReader(_fileStream);
        // Manual first call to read all the previous lines
        ReadLines();
    }

    private void ReadLines()
    {
        string? logContent;

        while ((logContent = _streamReader?.ReadLine()) != null)
        {
            NewLogFileChanged?.Invoke(this, logContent);
        }
    }

    private void OnLogFileChanged(object sender, FileSystemEventArgs e)
    {
        ReadLines();
    }

    public void StopWatching()
    {
        _watcher?.Dispose();
        _fileStream?.Dispose();
        _streamReader?.Dispose();
        _monitoringPath?.StopWatching();
        System.Console.WriteLine($"Stopped watching {_currentLogFile}");
    }
}
