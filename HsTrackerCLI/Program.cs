using HsTracker.Cache;
using HsTracker.Parsers;
using HsTracker.Readers;
using HsTracker.Watchers;
using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;

namespace HsTrackerCLI;

internal class Program
{
    private static void Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddLogging();

        builder.Services.AddMemoryCache();

        builder.Services.AddSingleton<PowerLogParser>();

        builder.Services.AddSingleton<PowerLogReader>();

        builder.Services.AddSingleton<SessionLogsDirectoryWatcher>(provider =>
        {
            var configuration = provider.GetRequiredService<IConfiguration>();
            var memoryCache = provider.GetRequiredService<IMemoryCache>();

            var watcher = new SessionLogsDirectoryWatcher(
                provider.GetRequiredService<ILogger<SessionLogsDirectoryWatcher>>(),
                configuration,
                memoryCache
            );

            watcher.SetNext(
                new PowerLogFileWatcher(
                    provider.GetRequiredService<ILogger<PowerLogFileWatcher>>(),
                    configuration,
                    memoryCache,
                    provider.GetRequiredService<PowerLogReader>()
                )
            );

            return watcher;
        });

        builder.Services.AddSingleton<CardDataCache>(provider =>
        {
            var memoryCache = provider.GetRequiredService<IMemoryCache>();

            var cardDataCache = new CardDataCache(
                provider.GetRequiredService<ILogger<CardDataCache>>(),
                memoryCache
            );

            var hsCardsCache = new HsCardsCache(
                provider.GetRequiredService<ILogger<HsCardsCache>>(),
                memoryCache
            );

            var cardDefsCache = new CardDefsCache(
                provider.GetRequiredService<ILogger<CardDefsCache>>(),
                memoryCache
            );

            hsCardsCache.SetNext(cardDefsCache);
            cardDataCache.SetNext(hsCardsCache);

            return cardDataCache;
        });

        var host = builder.Build();

        host.Services.GetRequiredService<SessionLogsDirectoryWatcher>().Handle();

        host.Services.GetRequiredService<CardDataCache>().Handle();

        host.Run();
    }
}
