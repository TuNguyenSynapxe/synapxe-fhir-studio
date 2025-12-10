using MappingEngineService.Models;
using System.Net;

namespace MappingEngineService.Middleware;

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
            _logger.LogError(ex, "Unhandled exception occurred");
            await HandleExceptionAsync(context, ex);
        }
    }

    private Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        var correlationId = context.Items["X-Correlation-Id"]?.ToString() ?? string.Empty;

        var errorResponse = new ErrorResponse
        {
            Success = false,
            Error = new ErrorDetails
            {
                Code = "INTERNAL_ERROR",
                Message = "An unexpected error occurred. Please contact support."
            },
            CorrelationId = correlationId
        };

        context.Response.ContentType = "application/json";
        context.Response.StatusCode = (int)HttpStatusCode.InternalServerError;

        return context.Response.WriteAsJsonAsync(errorResponse);
    }
}
