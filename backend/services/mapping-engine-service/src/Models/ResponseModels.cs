namespace MappingEngineService.Models;

public class ResponseModels
{
}

public class SuccessResponse<T>
{
    public bool Success { get; set; } = true;
    public T? Data { get; set; }
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public ErrorDetails Error { get; set; } = new();
    public string CorrelationId { get; set; } = string.Empty;
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;
}

public class ErrorDetails
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public Dictionary<string, string[]>? ValidationErrors { get; set; }
}
