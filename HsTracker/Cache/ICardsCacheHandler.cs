namespace HsTracker.Cache;

public interface ICardsCacheHandler
{
    void SetNext(ICardsCacheHandler handler);
    void Handle();
}
