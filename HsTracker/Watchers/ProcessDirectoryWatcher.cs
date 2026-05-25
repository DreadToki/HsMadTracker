using System.Dynamic;
using System.Text.RegularExpressions;
using HsTracker.SystemTools.Xdotool;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HsTracker.Watchers;

public partial class ProcessDirectoryWatcher : BackgroundService
{
    private const string ProcessDirectoryName = "/proc";

    [GeneratedRegex(
        @"(?=Name:\s+(?<name>\S+))(?=[\s\S]*?Pid:\s+(?<pid>\d+))",
        RegexOptions.Multiline
    )]
    private static partial Regex ProcessProperties { get; }

    private string? _gameExecutableFileName;

    private readonly ILogger<ProcessDirectoryWatcher> _logger;

    private readonly IConfiguration _configuration;

    private readonly IMemoryCache _memoryCache;

    public ProcessDirectoryWatcher(
        ILogger<ProcessDirectoryWatcher> logger,
        IConfiguration configuration,
        IMemoryCache memoryCache
    )
    {
        _logger = logger;
        _configuration = configuration;
        _memoryCache = memoryCache;
    }

    private Dictionary<int, string> GetActiveProcesses()
    {
        var processDirectories = Directory.EnumerateDirectories(ProcessDirectoryName);

        var processes = new Dictionary<int, string>();

        foreach (var processDirectory in processDirectories)
        {
            var statusFilePath = Path.Combine(processDirectory, "status");

            if (!File.Exists(statusFilePath))
            {
                _logger.LogWarning(
                    "Status file not found for process directory: {ProcessDirectory}",
                    processDirectory
                );
                continue;
            }

            var text = File.ReadAllText(statusFilePath);

            if (ProcessProperties.Match(text) is Match props && props.Success)
            {
                var processName = props.Groups["name"].Value;
                var processId = int.Parse(props.Groups["pid"].Value);

                processes[processId] = processName;
            }
        }

        return processes;
    }

    protected override async Task ExecuteAsync(CancellationToken c)
    {
        _gameExecutableFileName = _configuration.GetValue<string>("GameExecutableFileName");

        if (string.IsNullOrWhiteSpace(_gameExecutableFileName))
        {
            _logger.LogError("Game executable file name is not configured.");
            return;
        }

        var known = new Dictionary<int, string>();

        using var timer = new PeriodicTimer(TimeSpan.FromMilliseconds(1000));

        while (await timer.WaitForNextTickAsync(c))
        {
            var current = GetActiveProcesses();

            foreach (var (pid, processName) in current.Except(known))
            {
                if (
                    string.Compare(
                        processName,
                        _gameExecutableFileName,
                        StringComparison.InvariantCulture
                    ) != 0
                )
                {
                    continue;
                }

                // _memoryCache.Set(HsTrackerConsts.GameProcessId, pid);

                var windowIds = Xdotool.SearchByPid(pid);

                foreach (var windowId in windowIds)
                {
                    var geometry = Xdotool.GetGeometry(windowId);

                    _logger.LogInformation(
                        "Found game window (ID: {WindowId}) with geometry: {Geometry}",
                        windowId,
                        geometry
                    );
                }

                _logger.LogInformation(
                    "Process created: {ProcessName} (PID: {ProcessId})",
                    processName,
                    pid
                );
            }
            foreach (var (pid, processName) in known.Except(current))
            {
                if (
                    string.Compare(
                        processName,
                        _gameExecutableFileName,
                        StringComparison.InvariantCulture
                    ) != 0
                )
                {
                    continue;
                }

                // _memoryCache.Remove(HsTrackerConsts.GameProcessId);

                _logger.LogInformation(
                    "Process exited: {ProcessName} (PID: {ProcessId})",
                    processName,
                    pid
                );
            }

            known = current;
        }
    }
}
