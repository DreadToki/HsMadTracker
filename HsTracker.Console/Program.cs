using System.Text;
using HsTracker.Core;

internal class Program
{
    private static void Main(string[] args)
    {
        // string debugPath =
        //     "/home/toki/Work/HsMadTracker/HsTracker.Console/Tests/PowerTaskList_Debug.txt";
        // PowerLog monitor = new()
        // {
        //     CurrentPath = "/home/toki/Work/HsMadTracker/HsTracker.Console/Tests/",
        // };

        // // TODO: Review the logic and rewrite a method to write logs to separate files.

        // using (FileStream fs = File.Create(debugPath))
        // {
        //     monitor.NewPowerTaskListLog += (sender, logContent) =>
        //     {
        //         byte[] info = new UTF8Encoding(true).GetBytes(logContent);
        //         fs.Write(info, 0, info.Length);
        //         // System.Console.WriteLine($"{logContent}");
        //     };
        // }

        // monitor.StartWatching();
        // Console.ReadLine();
        // monitor.StopWatching();

        using LogDirectoryWatcher watcher = new(
            "/home/toki/Data/Games/BattleNet/Hearthstone",
            "Logs",
            "Power.log"
        );
        watcher.Init();
        Console.ReadLine();
    }
}
