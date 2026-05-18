using System.Text.Json;
using HsTracker.Models;
using HsTracker.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HsTracker.Cache;

public class HsCardsCache : ICardsCacheHandler
{
    private readonly ILogger<HsCardsCache> _logger;

    private readonly IMemoryCache _memoryCache;

    private ICardsCacheHandler? _next;

    public HsCardsCache(ILogger<HsCardsCache> logger, IMemoryCache memoryCache)
    {
        _logger = logger;
        _memoryCache = memoryCache;
    }

    public void Handle()
    {
        // TODO: Remove magic string
        var locale = _memoryCache.Get<string>(HsTrackerConsts.Locale) ?? "en_US";
        var sourceDir = Path.Combine(AppContext.BaseDirectory, "data", "hearthstone_data", locale);

        foreach (var file in Directory.EnumerateFiles(sourceDir, "*.json"))
        {
            var json = File.ReadAllText(file);

            var page = JsonSerializer.Deserialize<HsCardsPage>(
                json,
                HsTrackerJsonSerializerContext.Default.HsCardsPage
            );

            if (page == null || page.Cards == null)
            {
                _logger.LogError($"Failed to deserialize card data from file {file}");
                continue;
            }

            var cardData = _memoryCache.Get<List<CardData>>(HsTrackerConsts.CardData);
            cardData?.AddRange(
                page.Cards.Select(c => new CardData
                {
                    Id = c.Id,
                    CardId = null,
                    Name = c.Name,
                    ManaCost = c.ManaCost,
                    FullImage = GenerateFullImagePathById(c.Id, locale),
                    CropImage = GenerateCropImagePathById(c.Id, locale),
                })
            );
        }

        _next?.Handle();
    }

    public void SetNext(ICardsCacheHandler handler)
    {
        _next = handler;
    }

    private string? GenerateCropImagePathById(int id, string locale)
    {
        return GenerateImagePath(id, locale, "crop_");
    }

    private string? GenerateFullImagePathById(int id, string locale)
    {
        return GenerateImagePath(id, locale, "full_");
    }

    private string? GenerateImagePath(int id, string locale, string prefix)
    {
        var directoryPath = Path.Combine(
            AppContext.BaseDirectory,
            "resources",
            "card_images",
            locale,
            $"{id}"
        );

        if (!Directory.Exists(directoryPath))
        {
            _logger.LogError($"Directory for card id {id} not found in {directoryPath}");
            return null;
        }

        var filePath = Directory.EnumerateFiles(directoryPath, $"{prefix}*.png").FirstOrDefault();

        if (!File.Exists(filePath))
        {
            _logger.LogError($"Image for card id {id} not found in {directoryPath}");
            return null;
        }

        return filePath;
    }
}
