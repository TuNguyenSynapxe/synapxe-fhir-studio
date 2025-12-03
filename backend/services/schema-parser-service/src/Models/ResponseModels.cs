namespace SchemaParserService.Models;

public class SuccessResponse<T>
{
    public bool Success { get; set; } = true;
    public T? Data { get; set; }
    public List<string> Warnings { get; set; } = new();
    public string CorrelationId { get; set; } = string.Empty;
}

public class ErrorResponse
{
    public bool Success { get; set; } = false;
    public ErrorDetails Error { get; set; } = new();
    public string CorrelationId { get; set; } = string.Empty;
}

public class ErrorDetails
{
    public string Code { get; set; } = string.Empty;
    public string Message { get; set; } = string.Empty;
    public List<string> Details { get; set; } = new();
    public string Target { get; set; } = string.Empty;
    public string TraceId { get; set; } = string.Empty;
}
