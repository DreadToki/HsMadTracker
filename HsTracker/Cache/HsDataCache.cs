using System.Xml;
using System.Xml.Serialization;
using HsTracker.Models;
using HsTracker.Models.HsData;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HsTracker.Cache;

public class HsDataCache : IDataCacheHandler
{
    private readonly ILogger<HsDataCache> _logger;

    private readonly IMemoryCache _memoryCache;

    private IDataCacheHandler? _next;

    public HsDataCache(ILogger<HsDataCache> logger, IMemoryCache memoryCache)
    {
        _logger = logger;
        _memoryCache = memoryCache;
    }

    public void Handle()
    {
        var file = Path.Combine(AppContext.BaseDirectory, "data", "hsdata", "CardDefs.xml");

        using var xml = new FileStream(file, FileMode.Open, FileAccess.Read);

        var serializer = new XmlSerializer(typeof(CardDefs));

        var reader = XmlReader.Create(xml);

        var cardDefs = serializer.Deserialize(reader) as CardDefs;

        var cardData = _memoryCache.Get<List<HsCardData>>(HsTrackerConsts.HsCardData);

        cardData?.ForEach(cd => cd.CardId = cardDefs?.Entities?.Single(c => c.Id == cd.Id).CardId);

        _next?.Handle();
    }

    public void SetNext(IDataCacheHandler handler)
    {
        _next = handler;
    }
}
