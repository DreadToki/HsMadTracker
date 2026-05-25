using HsTracker.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HsTracker.Cache
{
    public class HsCardDataCache : IDataCacheHandler
    {
        private readonly ILogger<HsCardDataCache> _logger;

        private readonly IMemoryCache _memoryCache;

        private IDataCacheHandler? _next;

        public HsCardDataCache(ILogger<HsCardDataCache> logger, IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public void Handle()
        {
            _memoryCache.Set(HsTrackerConsts.HsCardData, new List<HsCardData>());
            _next?.Handle();
        }

        public void SetNext(IDataCacheHandler handler)
        {
            _next = handler;
        }
    }
}
