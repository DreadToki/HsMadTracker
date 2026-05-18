namespace HsTracker.Watchers;

public interface IFileSystemWatcherHandler
{
    void SetNext(params IFileSystemWatcherHandler[] handlers);
    void Handle();
}
