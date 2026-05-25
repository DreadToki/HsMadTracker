using System.Text.Json;
using HsTracker.Models;
using HsTracker.Models.HearthstoneData;
using HsTracker.Serialization;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Logging;

namespace HsTracker.Cache;

public class HearthstoneDataCache : IDataCacheHandler
{
    private readonly ILogger<HearthstoneDataCache> _logger;

    private readonly IMemoryCache _memoryCache;

    private IDataCacheHandler? _next;

    public HearthstoneDataCache(ILogger<HearthstoneDataCache> logger, IMemoryCache memoryCache)
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

            var page = JsonSerializer.Deserialize<HearthstoneDataCardsPage>(
                json,
                HsTrackerJsonSerializerContext.Default.HearthstoneDataCardsPage
            );

            if (page == null || page.Cards == null)
            {
                _logger.LogError($"Failed to deserialize card data from file {file}");
                continue;
            }

            var cardData = _memoryCache.Get<List<HsCardData>>(HsTrackerConsts.HsCardData);
            cardData?.AddRange(
                page.Cards.Select(c => new HsCardData
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

    public void SetNext(IDataCacheHandler handler)
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

        // TODO: crop_ ends with .jpg || full_ ends with .png
        var filePath = Directory.EnumerateFiles(directoryPath, $"{prefix}*.*").FirstOrDefault();

        if (!File.Exists(filePath))
        {
            _logger.LogError($"Image for card id {id} not found in {directoryPath}");
            return null;
        }

        return filePath;
    }
}
