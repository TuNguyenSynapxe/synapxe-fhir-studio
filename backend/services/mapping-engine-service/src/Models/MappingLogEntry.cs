namespace MappingEngineService.Models;

public class MappingLogEntry
{
    public string Path { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public LogSeverity Severity { get; set; } = LogSeverity.Info;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}
