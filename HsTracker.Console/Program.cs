using HsTracker.Core.Parsers;
using HsTracker.Core.Watchers;

internal class Program
{
    private static void Main(string[] args)
    {
        using LogDirectoryWatcher watcher = new(
            "/home/toki/Data/Games/BattleNet/Hearthstone",
            "Logs",
            "Power.log"
        );
        watcher.Init();
        // using FileStream filestream = new(
        //     "/home/toki/Work/HsTracker/Power.log",
        //     FileMode.Open,
        //     FileAccess.Read,
        //     FileShare.ReadWrite
        // );
        // using StreamReader streamReader = new(filestream);

        // new PowerLogParser().ReadBlock(streamReader);
        Console.ReadLine();
    }
}
