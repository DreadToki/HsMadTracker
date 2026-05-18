using HsTracker.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HsTracker.Cache
{
    public class CardDataCache : ICardsCacheHandler
    {
        private readonly ILogger<CardDataCache> _logger;

        private readonly IMemoryCache _memoryCache;

        private ICardsCacheHandler? _next;

        public CardDataCache(ILogger<CardDataCache> logger, IMemoryCache memoryCache)
        {
            _logger = logger;
            _memoryCache = memoryCache;
        }

        public void Handle()
        {
            _memoryCache.Set(HsTrackerConsts.CardData, new List<CardData>());
            _next?.Handle();
        }

        public void SetNext(ICardsCacheHandler handler)
        {
            _next = handler;
        }
    }
}
