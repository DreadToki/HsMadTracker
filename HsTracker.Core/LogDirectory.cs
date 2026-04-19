namespace HsTracker.Core;

public class LogDirectory
{
    public event EventHandler<PathChangedEventArgs>? PathChange;
    public required string ParentPath { get; set; }
    string? _watchDirectory;
    FileSystemWatcher? _watcher;

    // TODO: Review the whole logic :)
    public void StartWatching()
    {
        _watchDirectory = Path.Combine(ParentPath, "Logs");

        if (!Directory.Exists(_watchDirectory))
        {
            throw new DirectoryNotFoundException(
                $"The specified Path '{_watchDirectory}' does not exist."
            );
        }

        var newestDirectory = Directory
            .GetDirectories(_watchDirectory)
            .Select(d => new DirectoryInfo(d))
            .OrderByDescending(d => d.Name)
            .FirstOrDefault();

        if (newestDirectory != null)
        {
            OnPathChange(newestDirectory.FullName);
        }
        else
        {
            System.Console.WriteLine($"No directories found in {_watchDirectory}");
        }
        _watcher = new FileSystemWatcher(_watchDirectory)
        {
            NotifyFilter = NotifyFilters.DirectoryName,
            EnableRaisingEvents = true,
        };
        _watcher.Created += (s, e) => OnPathChange(e.FullPath);
    }

    private void OnPathChange(string path)
    {
        if (Directory.Exists(path))
        {
            PathChange?.Invoke(null, new() { FullName = path });
        }
    }

    public void StopWatching()
    {
        _watcher?.Dispose();
    }
}
