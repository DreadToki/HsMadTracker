using System.Diagnostics;

namespace HsTracker.SystemTools.Xdotool;

public class Xdotool
{
    public static XdotoolResult Run(params string[] args)
    {
        var psi = new ProcessStartInfo("xdotool", string.Join(' ', args))
        {
            RedirectStandardOutput = true,
            RedirectStandardError = true,
            UseShellExecute = false,
        };

        using var p =
            Process.Start(psi) ?? throw new InvalidOperationException("Failed to start xdotool");

        var outputTask = p.StandardOutput.ReadToEnd();
        var errorTask = p.StandardError.ReadToEnd();

        p.WaitForExit();

        return new XdotoolResult(
            Output: outputTask.Trim(),
            Error: errorTask.Trim(),
            ExitCode: p.ExitCode,
            Success: p.ExitCode == 0
        );
    }

    // convenience methods
    public static int[] SearchByPid(int pid)
    {
        var result = Run("search", "--onlyvisible", "--pid", pid.ToString());

        return result
            .Output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(int.Parse)
            .ToArray();
    }

    public static WindowGeometry? GetGeometry(int windowId)
    {
        var result = Run("getwindowgeometry", "--shell", windowId.ToString());
        if (!result.Success)
            return null;

        // --shell flag gives key=value pairs:
        // WINDOW=12345
        // X=1920
        // Y=0
        // WIDTH=2560
        // HEIGHT=1440
        // SCREEN=1
        var vars = result
            .Output.Split('\n', StringSplitOptions.RemoveEmptyEntries)
            .Select(l => l.Split('='))
            .Where(p => p.Length == 2)
            .ToDictionary(p => p[0], p => p[1]);

        return new WindowGeometry(
            WindowId: int.Parse(vars["WINDOW"]),
            X: int.Parse(vars["X"]),
            Y: int.Parse(vars["Y"]),
            Width: int.Parse(vars["WIDTH"]),
            Height: int.Parse(vars["HEIGHT"]),
            Screen: int.Parse(vars["SCREEN"])
        );
    }
}
