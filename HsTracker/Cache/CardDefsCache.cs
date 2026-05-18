using System.Xml;
using System.Xml.Serialization;
using HsTracker.Models;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HsTracker.Cache;

public class CardDefsCache : ICardsCacheHandler
{
    private readonly ILogger<CardDefsCache> _logger;

    private readonly IMemoryCache _memoryCache;

    private ICardsCacheHandler? _next;

    public CardDefsCache(ILogger<CardDefsCache> logger, IMemoryCache memoryCache)
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

        var cardData = _memoryCache.Get<List<CardData>>(HsTrackerConsts.CardData);

        cardData?.ForEach(cd => cd.CardId = cardDefs?.Entities?.Single(c => c.Id == cd.Id).CardId);

        _next?.Handle();
    }

    public void SetNext(ICardsCacheHandler handler)
    {
        _next = handler;
    }
}
