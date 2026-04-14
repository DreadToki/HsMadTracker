using HsTracker.Core;

internal class Program
{
    private static void Main(string[] args)
    {
        PowerLog monitor = new() { CurrentPath = "/home/toki/Data/Games/BattleNet/Hearthstone/" };

        monitor.NewLogFileChanged += (sender, logContent) =>
        {
            System.Console.WriteLine($"{logContent}");
        };

        monitor.StartWatching();
        Console.ReadLine();
        monitor.StopWatching();
    }
}
