namespace GameHost.Logging;

public sealed class LogEntry
{
    public DateTime Time { get; init; } = DateTime.Now;
    public string Logger { get; init; } = "Hazel";
    public string Level { get; init; } = "Info";
    public string Message { get; init; } = "";
}
