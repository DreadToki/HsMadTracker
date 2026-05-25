namespace HsTracker.Cache;

public interface IDataCacheHandler
{
    void SetNext(IDataCacheHandler handler);
    void Handle();
}
