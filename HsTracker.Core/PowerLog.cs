namespace HsTracker.Core;

public class PowerLog
{
    // TODO: Review the usage of FileSystemEventHandler
    public event EventHandler<string>? NewLogFileChanged;
    public event EventHandler<string>? NewPowerTaskListLog;
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

        _monitoringPath.PathChange += (sender, path) =>
        {
            _currentLogFilePath = path.FullName;
            SetupFileStream(_currentLogFilePath!);
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
        // TODO: Manage logic of the flow of parsing different entities. Moreover, subscribe different classes to different outputs
        /*
            1. What if to separate the log reading: two different flows:
                1) First reads GameState Logs => logs as soon as possible. (Review these entities:
                    GameState.DebugPrintGame, GameState.DebugPrintEntityChoices, GameState.DebugPrintEntitiesChosen and others)
                2) Second reads PowerTaskList Logs => logs with the delay of the animation what is happening on the screen. (Main use case for this project)
                3) Third reads everything else => just to be sure that I am not missing anything important.
        */
        string? logContent;

        while ((logContent = _streamReader?.ReadLine()) != null)
        {
            // Idea: Here should be the implementation of only extracting PowerTaskList for example
            // Consider rewriting sending parts of logs with Channels and not with events.
            // Moreover, think of implementing some kind of buffer for the logs.
            RouteLogLine(logContent);
        }
    }

    private void RouteLogLine(string logContent)
    {
        if (logContent.Contains("PowerTaskList"))
        {
            NewLogFileChanged?.Invoke(this, logContent);
            NewPowerTaskListLog?.Invoke(this, logContent);
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
        System.Console.WriteLine($"Stopped watching {_currentLogFilePath}");
    }
}
