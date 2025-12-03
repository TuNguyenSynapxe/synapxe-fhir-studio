using SchemaParserService.Models;
using System.Net;
using System.Text.Json;

namespace SchemaParserService.Middleware;

public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private static async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["CorrelationId"]?.ToString() ?? Guid.NewGuid().ToString();
        var traceId = context.TraceIdentifier;

        var response = new ErrorResponse
        {
            Success = false,
            CorrelationId = correlationId,
            Error = new ErrorDetails
            {
                Code = exception switch
                {
                    ArgumentException => "VALIDATION_ERROR",
                    InvalidOperationException => "INVALID_OPERATION",
                    NotSupportedException => "NOT_SUPPORTED",
                    _ => "INTERNAL_ERROR"
                },
                Message = exception.Message,
                Details = new List<string> { exception.GetType().Name },
                Target = context.Request.Path,
                TraceId = traceId
            }
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = exception switch
        {
            ArgumentException => (int)HttpStatusCode.BadRequest,
            InvalidOperationException => (int)HttpStatusCode.BadRequest,
            NotSupportedException => (int)HttpStatusCode.BadRequest,
            _ => (int)HttpStatusCode.InternalServerError
        };

        var json = JsonSerializer.Serialize(response, new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase
        });

        await context.Response.WriteAsync(json);
    }
}

public static class ExceptionHandlingMiddlewareExtensions
{
    public static IApplicationBuilder UseExceptionHandling(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<ExceptionHandlingMiddleware>();
    }
}
