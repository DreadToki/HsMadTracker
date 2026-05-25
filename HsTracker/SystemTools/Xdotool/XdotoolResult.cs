namespace HsTracker.SystemTools.Xdotool;

public record XdotoolResult(string Output, string Error, int ExitCode, bool Success);
