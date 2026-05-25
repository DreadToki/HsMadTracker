using HsTracker.Cache;
using HsTracker.Models;
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
    private static async Task Main(string[] args)
    {
        var builder = Host.CreateApplicationBuilder(args);

        builder.Services.AddLogging();

        builder.Services.AddMemoryCache();

        builder.Services.AddSingleton<HearthstoneDataCache>(provider =>
        {
            var hearthstoneDataCache = new HearthstoneDataCache(
                provider.GetRequiredService<ILogger<HearthstoneDataCache>>(),
                provider.GetRequiredService<IMemoryCache>()
            );

            hearthstoneDataCache.SetNext(provider.GetRequiredService<HsDataCache>());

            return hearthstoneDataCache;
        });

        builder.Services.AddSingleton<HsCardDataCache>(provider =>
        {
            var hsCardDataCache = new HsCardDataCache(
                provider.GetRequiredService<ILogger<HsCardDataCache>>(),
                provider.GetRequiredService<IMemoryCache>()
            );

            hsCardDataCache.SetNext(provider.GetRequiredService<HearthstoneDataCache>());

            return hsCardDataCache;
        });

        builder.Services.AddSingleton<HsDataCache>();

        builder.Services.AddSingleton<PowerLogParser>();

        builder.Services.AddSingleton<PowerLogReader>();

        builder.Services.AddSingleton<PowerLogFileWatcher>();

        builder.Services.AddHostedService<ProcessDirectoryWatcher>();

        builder.Services.AddSingleton<SessionLogsDirectoryWatcher>(provider =>
        {
            var watcher = new SessionLogsDirectoryWatcher(
                provider.GetRequiredService<ILogger<SessionLogsDirectoryWatcher>>(),
                provider.GetRequiredService<IConfiguration>(),
                provider.GetRequiredService<IMemoryCache>()
            );

            watcher.SetNext(provider.GetRequiredService<PowerLogFileWatcher>());

            return watcher;
        });

        var host = builder.Build();

        host.Services.GetRequiredService<SessionLogsDirectoryWatcher>().Handle();

        host.Services.GetRequiredService<HsCardDataCache>().Handle();

        await host.RunAsync();
    }
}
